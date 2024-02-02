using System.Collections;
using System.Collections.Generic;
using System.Data;
using SDFr;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class VFXSDFSetup : MonoBehaviour
{
    public VisualEffect vfx;
    public SDFData data;
    public Transform sdfTransform;
    public Transform spawnSource;
    [Range(0.0001f,0.1f)]
    public float normalDelta = 0.03f;
    [Range(0.0001f,0.1f)]
    public float epsilon = 0.01f;

    private readonly int _sdfrTexture = Shader.PropertyToID("SDFrTexture");
    private readonly int _sdfrTransform = Shader.PropertyToID("SDFrMatrix");
    private readonly int _spawnSource = Shader.PropertyToID("SpawnSource");

    private Material _material;
    
    void OnValidate()
    {
        if (data == null || data.sdfTexture == null) return;
        //if ( vfx == null ) vfx = this.GetComponent<VisualEffect>();
        if (vfx == null) return;
        
        Debug.Log("sdf: "+data.name +" vfx: "+ vfx.name);
        
        vfx.SetTexture(_sdfrTexture,data.sdfTexture);

        Vector3 scale = data.bounds.size;
        
        Matrix4x4 l2w = Matrix4x4.identity;//sdfTransform == null ? transform.localToWorldMatrix : sdfTransform.localToWorldMatrix;

        if (sdfTransform != null)
        {
            l2w = Matrix4x4.TRS(sdfTransform.position, sdfTransform.rotation, scale);
        }
        else
        {
            l2w = Matrix4x4.TRS(transform.position, transform.rotation, scale);
        }
        vfx.SetMatrix4x4(_sdfrTransform,l2w);
    }

    private Matrix4x4 volMatrixLocalToWorld;
    
    void Update()
    {
        if (vfx == null) return;
        if (data == null || data.sdfTexture == null) return;
        vfx.SetTexture(_sdfrTexture,data.sdfTexture);

        if (sdfTransform == null)
        {
            volMatrixLocalToWorld = transform.localToWorldMatrix;
        }
        else
        {
            volMatrixLocalToWorld = sdfTransform.localToWorldMatrix;
        }
        vfx.SetMatrix4x4(_sdfrTransform,volMatrixLocalToWorld);
        
        if ( spawnSource != null )
            vfx.SetMatrix4x4(_spawnSource,Matrix4x4.TRS(spawnSource.position,Quaternion.identity,spawnSource.localScale));
    }

    private void OnRenderObject()
    {
        if (data == null || data.sdfTexture == null) return;
        
        //render preview - only for testing!
        //try to get active camera...
        Camera cam = Camera.main;

#if UNITY_EDITOR          
		if (UnityEditor.SceneView.lastActiveSceneView != null)
        {
            cam = UnityEditor.SceneView.lastActiveSceneView.camera;
        }
#endif

		if (_material == null)
        {
            Shader shader = Shader.Find("XRA/SDFr");
            if (shader != null)
            {
                _material = new Material(shader);
            }
        }

        if (_material == null) return;
        
        CommandBuffer _cmd = new CommandBuffer();
        
        _material.SetTexture("_SDFVolumeTex",data.sdfTexture);
        _material.SetVector("_SDFVolumeExtents", Vector3.one * 0.5f);
        
        //_material.SetMatrix(_SDFVolumeLocalToWorld, _volume.LocalToWorldNoScale);
        _material.SetMatrix( "_SDFVolumeWorldToLocal", volMatrixLocalToWorld.inverse);
        //_material.SetFloat(_SDFVolumeFlip, flip ? -1f : 1f); 
        _material.SetVector("_BlitScaleBiasRt",new Vector4(1f,1f,0f,0f));
        _material.SetVector("_BlitScaleBias", new Vector4(1f, 1f, 0f, 0f));
        _material.SetFloat("_SDFPreviewEpsilon",epsilon);
        _material.SetFloat("_SDFPreviewNormalDelta",normalDelta);
            
        AVolumeUtils.SetupRaymarchingMatrix(
            cam.fieldOfView,
            cam.worldToCameraMatrix,
            new Vector2(cam.pixelWidth, cam.pixelHeight));
        
        _cmd.DrawProcedural(Matrix4x4.identity, _material, 0, MeshTopology.Quads, 4, 1);
        Graphics.ExecuteCommandBuffer(_cmd);
        
        _cmd.Release();
    }

    private void OnDrawGizmosSelected()
    {
            
    }
}
