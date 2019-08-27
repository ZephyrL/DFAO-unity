using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using SDFr;

[ExecuteInEditMode]
public class DistanceFieldAtlas : MonoBehaviour
{
    // Global distance field texture to provide rough DF value in global space
    public CustomRenderTexture globalDistanceFieldTexture;

    // a z-stacked texture array (atlas) that stores all local SDF's
    private Texture3D textureAtlas;

    // list of refereces to local SDF's
    private List<Texture3D> texCollection;

    // list of referece to game objects that use SDF baker
    private List<GameObject> gameObjCollection;
    private List<bool> gameObjectStatus;

    // structure represents the transform of objects, along with their SDF bounding box
    private const int VolumeDataStride = 92;
    private struct VolumeData
    {
        public Matrix4x4 WorldToLocal;
        // extent of SDF local bounding box
        public Vector3 Extents;
        // index of texture regarding the texture atlas
        public int TextureIndex;
        // center of SDF local bounding box
        public Vector3 Center;

        // noted that: I guess this is a better structure and not suggest reordering the struct components
        // in case that your platform has a preference to perform like over-32-bits alignment
    }

    // volume transform data on CPU side
    private List<VolumeData> volumeData;

    // a buffer ready to copy the transform data to GPU
    private ComputeBuffer volumeDataBuffer;
    
    // temporary global variables, should be removed in the future and 
    // have the parameters changed dynamically according to global factors
    [Header("Temp var")]
    // width/height/depth of local SDF textures, now we use 64^3 as common
    public int DistanceFieldDepth = 64;
    // bound of global SDF texture, this bounding box should enclose all objects with SDF
    public Vector3 GlobalAABBMin;
    public Vector3 GlobalAABBMax;

    void OnEnable()
    {
        // debug information, check if the stride we set corresponds with actual struct size
        int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(VolumeData));
        Debug.Log("Struct size check: " + (size == VolumeDataStride ? "Pass" : "Fail"));

        // get all objects that have a SDFBaker component: 
        SDFBaker[] bakers = GameObject.FindObjectsOfType<SDFBaker>() as SDFBaker[];

        texCollection = new List<Texture3D>();
        gameObjCollection = new List<GameObject>();
        gameObjectStatus = new List<bool>();
        volumeData = new List<VolumeData>();

        foreach (SDFBaker v in bakers)
        {
            // store reference to game objects that have SDF data
            GameObject obj = v.gameObject;
            // set transformation changed flag as true to trigger texture update
            obj.transform.hasChanged = true;
            gameObjectStatus.Add(obj.activeSelf);
            gameObjCollection.Add(obj);
            
            // prepare transform data of objects for passing to shaders
            VolumeData vd = new VolumeData();
            vd.WorldToLocal = obj.transform.worldToLocalMatrix;
            vd.Extents = v.sdfData.bounds.extents;
            vd.Center = v.sdfData.bounds.center;
            //Debug.Log(obj.name + " " + vd.Center + " " + vd.Extents);
            //Debug.Log(obj.name + "\n" + vd.WorldToLocal);

            // if there are multiple objects sharing the same SDF texture, 
            // only save one copy in the texture atlas, and assign the index to per-object data
            Texture3D tex = v.sdfData.sdfTexture;
            int tempId = texCollection.FindIndex(x => x.name == tex.name);
            if(tempId < 0)
            {
                vd.TextureIndex = texCollection.Count;
                texCollection.Add(tex);
            } else
            {
                vd.TextureIndex = tempId;
            }

            //Debug.Log(obj.name + " texIdx: " + vd.TextureIndex);
            volumeData.Add(vd);
        }

        // copy the listed volume transform data to the computer buffer, 
        // Shader.setBuffer() or _material.setBuffer() would copy the buffer to GPU side
        volumeDataBuffer = new ComputeBuffer(volumeData.Count, VolumeDataStride);
        volumeDataBuffer.SetData(volumeData);

        // the texture atlas is of same with & height with local SDF's,
        // while its depth is (#SDF * depth), hence it's not cubed and doesn't support LOD operations
        textureAtlas = new Texture3D(DistanceFieldDepth, DistanceFieldDepth, DistanceFieldDepth * texCollection.Count, TextureFormat.RHalf, false);
        Debug.Log("Atlas Depth: " + textureAtlas.depth);

        if (SystemInfo.copyTextureSupport == CopyTextureSupport.Copy3D)
            Debug.Log("Don't support Copy3d");
        else
            Debug.Log("Support copy 3d");

        // fuck this
        // should be replaced by a computer shader later
        for (int i = 0; i < texCollection.Count; i++)
        {
            for (int d = 0; d < texCollection[i].depth; d++)
            {
                for (int y = 0; y < texCollection[i].height; y++)
                {
                    for (int x = 0; x < texCollection[i].width; x++)
                    {
                        int depth = i * DistanceFieldDepth + d;
                        textureAtlas.SetPixel(x, y, depth, texCollection[i].GetPixel(x, y, d, 0));
                    }
                }
                // doesnot work
                //Graphics.CopyTexture(texCollection[i], d, 0, textureAtlas, i * DistanceFieldDepth + d, 0);
            }
        }
        // Important: This function does the actual CPU->GPU copy
        textureAtlas.Apply();

        // Global shader properties never change through rendering
        Shader.SetGlobalVector("_BlitScaleBiasRt", new Vector4(1f, 1f, 0f, 0f));
        Shader.SetGlobalVector("_BlitScaleBias", new Vector4(1f, 1f, 0f, 0f));
        // in case there isn't new texture genereation at run time
        Shader.SetGlobalTexture("_TextureAtlas", textureAtlas);

        // initialize global SDF texture
        globalDistanceFieldTexture.Initialize();
        Shader.SetGlobalTexture("_GlobalDFTex", globalDistanceFieldTexture);
    }

    // Update is called once per frame
    void Update()
    {
        // For each frame, Update() should follow steps below:
        // 1: Check if any object moved in the scene, if so,
        //      1.1 renew transform data, 
        //      1.2 update GDF texture
        // 2: render

        Camera cam = Camera.main;
        if(cam == null) return;

        bool update = false;

        List<VolumeData> tempVolumeData = new List<VolumeData>();
        for (int i = 0; i < gameObjCollection.Count; i++)
        {
            GameObject o = gameObjCollection.ElementAt(i);
            if (o.activeSelf != gameObjectStatus[i]){ // an object is changed from active <-> inactive
                update = true;
                gameObjectStatus[i] = o.activeSelf;
                if (o.activeSelf)   // object reaccures, should copy transform data,    
                                    // otherwise the tranform data would be identical to the original position  
                {
                    VolumeData newVolumeData = new VolumeData
                    {
                        WorldToLocal = o.transform.worldToLocalMatrix,
                        Extents = volumeData[i].Extents,
                        TextureIndex = volumeData[i].TextureIndex,
                        Center = volumeData[i].Center
                    };
                    tempVolumeData.Add(newVolumeData);
                    continue;
                }
            }
            if (!o.activeSelf) continue;    // if object is disabled, then it's omitted in buffer before passing to shader
            if (o.transform.hasChanged)     // update transform data of object SDF's 
            {
                update = true;
                o.transform.hasChanged = false;

                VolumeData newVolumeData = new VolumeData
                {
                    WorldToLocal = o.transform.worldToLocalMatrix,
                    Extents = volumeData[i].Extents,
                    TextureIndex = volumeData[i].TextureIndex,
                    Center = volumeData[i].Center
                };
                tempVolumeData.Add(newVolumeData);
            } else
            {
                tempVolumeData.Add(volumeData[i]);
            }
        }

        if (update)
        {

            // 1.1 renew volume data and pass to GPU
            volumeDataBuffer.SetData(tempVolumeData);
            Shader.SetGlobalBuffer("_VolumeData", volumeDataBuffer);
            Shader.SetGlobalInt("bufferLength", tempVolumeData.Count);
            Shader.SetGlobalInt("textureCount", texCollection.Count);

            Shader.SetGlobalVector("_AABBmin", GlobalAABBMin);
            Shader.SetGlobalVector("_AABBmax", GlobalAABBMax);

            // 1.2 re-calculate Global SDF
            globalDistanceFieldTexture.Update();
            Shader.SetGlobalTexture("_GlobalDFTex", globalDistanceFieldTexture);

            // 2. render by OnRenderImage() in GDFCameraAO.cs
        }

    }

    private void OnDisable()
    {
        // mem-ops
        volumeDataBuffer?.Dispose(); 
    }
}
