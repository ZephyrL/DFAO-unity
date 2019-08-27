using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SDFr
{
	public enum Visualisation {  Normal, IntensitySteps, HeatmapSteps, Distance }

    [ExecuteInEditMode] // required for previewing
    public class SDFBaker : AVolumeBaker<SDFVolume,SDFData>
    {
        [SerializeField] private int raySamples = 256;
        // [SerializeField] private int jitterSeed = 555;
        // [SerializeField] private float jitterScale = 0.75f;
        [SerializeField] private bool sdfFlip = false; // invert sign of preview
        [SerializeField] private float previewEpsilon = 0.003f;
        [SerializeField] private float previewNormalDelta = 0.02f;
        [SerializeField] private Visualisation previewMode = Visualisation.Normal;


        public SDFData sdfData;
        [SerializeField] private Texture3D debugTex3D; // for viewing existing texture3D not baked with SDFr
                                                       // this field would be automatically filled after SDF genereation

		public ComputeShader  volumeComputeMethodsShader;

        public override int MaxDimension => 256;

		public void LogDistances()
		{
			//  No debugging if the volume is > 8^3 as thats more than 512 entries!
			if ( sdfData.sdfTexture.width * sdfData.sdfTexture.height * sdfData.sdfTexture.depth > 512 ) return;
			
			float[] data    = VolumeComputeMethods.ExtractVolumeFloatData( sdfData.sdfTexture, volumeComputeMethodsShader);
			Vector3Int dim  = new Vector3Int( sdfData.sdfTexture.width, sdfData.sdfTexture.height, sdfData.sdfTexture.depth);
			string log      = $"{this.name}  Dimension: {dim}  MaxLength: {sdfData.maxDistance} Mag: {bounds.size.magnitude}\n";

			for ( int z = 0; z < sdfData.sdfTexture.depth; z++ )
			{
				for ( int y = 0; y < sdfData.sdfTexture.height; y++ )
				{
					for ( int x = 0; x < sdfData.sdfTexture.width; x++ )
					{
						log += string.Format( "{0:F6}, ", data[ z * dim.y * dim.x + y * dim.x + x ] * bounds.size.magnitude );
					}
					log += "\n";
				}
				log += "\n";
			}

			Debug.Log(log);
		}

#if UNITY_EDITOR
        
        private const string _sdfPreviewShaderName = "XRA/SDFr";
        private static Shader _shader; // TODO better way 
        
        public override AVolumePreview<SDFData> CreatePreview()
        {
            if (_shader == null)
            {
                _shader = Shader.Find(_sdfPreviewShaderName);
            }
            
            AVolumePreview<SDFData> sdf = new SDFPreview(sdfData, _shader, transform);
            if (debugTex3D != null)
            {
                (sdf as SDFPreview).debugTex3D = debugTex3D;
            }

            return sdf;
        }

        // / <summary>
        // / renders a procedural quad
        // / TODO fit the bounds of the SDF Volume
        // / </summary>
        private void OnRenderObject()
        {
            if (!IsPreviewing) return;
            
            // try to get active camera...
            Camera cam = Camera.main;

            //  lastActiveSceneView
            if (SceneView.currentDrawingSceneView != null) cam = SceneView.currentDrawingSceneView.camera;
            
            SDFPreview preview = _aPreview as SDFPreview;
            
            preview?.Draw(cam,sdfFlip,previewEpsilon,previewNormalDelta);
        }
        
        public override void Bake()
        {
            if (bakedRenderers == null)
            {
                bakedRenderers = new List<Renderer>();
            }
            bakedRenderers.Clear();
                        
            AVolumeSettings settings = new AVolumeSettings(bounds, dimensions, useStandardBorder);
            Debug.Log(settings.BoundsLocal);
            Debug.Log(settings.Dimensions);
            
            // first check if any objects are parented to this object
            // if anything is found, try to use renderers from those instead of volume overlap
            if (!SDFBaker.GetMeshRenderersInChildren( ref settings, ref bakedRenderers, transform, fitToVertices))
            {
                // otherwise try to get renderers intersecting the volume
                // get mesh renderers within volume
                if (!SDFBaker.GetMeshRenderersIntersectingVolume( settings, transform, ref bakedRenderers))
                {
                    // TODO display error?
                    return;
                }
            }
            Debug.Log("Renderer count: " + bakedRenderers.Count);

            SDFVolume sdfVolume = AVolume<SDFVolume>.CreateVolume(transform, settings);
            
            //  BakeComplete is callback after the completion of parallel baking
            sdfVolume.Bake( raySamples, bakedRenderers, BakeComplete );
            
            sdfVolume.Dispose();
        }
        
        // TODO improve asset saving 
        private void BakeComplete( SDFVolume sdfVolume, float[] distances, float maxDistance, object passthrough )
        {
            // update the bounds since they may have been adjusted during bake
            bounds = sdfVolume.Settings.BoundsLocal;
            
            string path = "";
            if (sdfData != null)
            {
                // use path of existing sdfData
                path = AssetDatabase.GetAssetPath(sdfData);
                
                // check if asset at path
                Object obj = AssetDatabase.LoadAssetAtPath<SDFData>(path);
                if (obj != null)
                {
                    if (((SDFData) obj).sdfTexture != null)
                    {
                        // destroy old texture
                        Object.DestroyImmediate(((SDFData) obj).sdfTexture,true);
                    }
                    // destroy old asset
                    // TODO this will break references...
                    Object.DestroyImmediate(obj,true);
                }
            }
            
			//  If not exisiting asset get path to save sdfData to.
			if ( string.IsNullOrEmpty( path ) )
			{
				string suggestedName = "sdfData_" + this.name;

				path = EditorUtility.SaveFilePanelInProject( "Save As...", suggestedName, "asset", "Save the SDF Data" );
				
				if ( string.IsNullOrEmpty( path ) )
				{
					if ( EditorUtility.DisplayDialog( "Error", "Path was invalid, retry?", "ok", "cancel" ) )
					{
						path = EditorUtility.SaveFilePanelInProject( "Save As...", suggestedName, "asset", "Save the SDF Data" );
					}

					if ( string.IsNullOrEmpty( path ) ) return;					
				}
			}


            // create new SDFData
            sdfData = ScriptableObject.CreateInstance<SDFData>();
            sdfData.bounds = sdfVolume.Settings.BoundsLocal;
            sdfData.voxelSize = sdfVolume.Settings.VoxelSize;
            sdfData.dimensions = sdfVolume.Settings.Dimensions;

            float minAxis = Mathf.Min( sdfData.bounds.size.x, Mathf.Min( sdfData.bounds.size.y, sdfData.bounds.size.z ) );
            sdfData.nonUniformScale = new Vector3( sdfData.bounds.size.x/minAxis, sdfData.bounds.size.y/minAxis, sdfData.bounds.size.z/minAxis );

			bool mipmaps = true;

			//  Create Texture3D and set name to filename of sdfData
            Texture3D newTex	= new Texture3D( sdfData.dimensions.x, sdfData.dimensions.y, sdfData.dimensions.z, TextureFormat.RHalf, mipmaps);
			newTex.name			= System.IO.Path.GetFileNameWithoutExtension(path);
            
            // TODO improve
            Color[] colorBuffer = new Color[distances.Length];
            for (int i = 0; i < distances.Length; i++)
            {
                // NOTE for compatibility with Visual Effect Graph, 
                // the distance must be negative inside surfaces.
                // normalize the distance for better support of scaling bounds
                // Max Distance is always the Magnitude of the baked bound size
                float normalizedDistance = distances[i] / maxDistance;
                colorBuffer[i] = new Color(normalizedDistance,0f,0f,0f);
            }
            newTex.SetPixels(colorBuffer);
            newTex.Apply();
			
			 
            sdfData.sdfTexture	= newTex;
            sdfData.maxDistance = maxDistance;
            
            EditorUtility.SetDirty(sdfData);
                        
            // create it
            AssetDatabase.CreateAsset(sdfData, path);
            AssetDatabase.AddObjectToAsset(newTex,sdfData);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            if (IsPreviewing)
                TogglePreview();
            if (!IsPreviewing ) 
                TogglePreview();
        }

		public void BakeHalfSizeTest()
		{
			SDFData sdfmip		= ScriptableObject.CreateInstance<SDFData>();
            sdfmip.bounds		= sdfData.bounds;
            sdfmip.voxelSize	= sdfData.voxelSize * 2;
            sdfmip.dimensions	= new Vector3Int( sdfData.dimensions.x/2, sdfData.dimensions.y/2, sdfData.dimensions.z/2 );

            float minAxis = Mathf.Min( sdfmip.bounds.size.x, Mathf.Min( sdfmip.bounds.size.y, sdfmip.bounds.size.z ) );
            sdfmip.nonUniformScale = new Vector3( sdfmip.bounds.size.x/minAxis, sdfmip.bounds.size.y/minAxis, sdfmip.bounds.size.z/minAxis );
			
			
			//  Save sdf
			string suggestedName = "sdfData_" + this.name + "_HalfBake";
			string path			 = EditorUtility.SaveFilePanelInProject( "Save As...", suggestedName, "asset", "Save the SDF Data" );

			if ( string.IsNullOrEmpty( path ) )
			{
				if ( EditorUtility.DisplayDialog( "Error", "Path was invalid, retry?", "ok", "cancel" ) )				
					path = EditorUtility.SaveFilePanelInProject( "Save As...", suggestedName, "asset", "Save the SDF Data" );
				
				if ( string.IsNullOrEmpty( path ) ) return;
			}

			//  GetDistances from srcVolume
			float[] distances	= VolumeComputeMethods.ExtractVolumeFloatData( sdfData.sdfTexture, volumeComputeMethodsShader);
			Vector3Int dim		= new Vector3Int( sdfData.sdfTexture.width, sdfData.sdfTexture.height, sdfData.sdfTexture.depth);

			//  Get half size data
            Color[] colorBuffer = new Color[distances.Length/2/2/2];

			int index = 0;

			for ( int z = 0; z < sdfData.sdfTexture.depth; z+=2 )			
				for ( int y = 0; y < sdfData.sdfTexture.height; y+=2 )				
					for ( int x = 0; x < sdfData.sdfTexture.width; x+=2 )					
						colorBuffer[index++] = new Color( distances[ z * dim.y * dim.x + y * dim.x + x ] ,0f, 0f, 0f);				
			
			
			//  Create Texture3D and set name to filename of sdfData
            Texture3D newTex	= new Texture3D( sdfmip.dimensions.x, sdfmip.dimensions.y, sdfmip.dimensions.z, TextureFormat.RHalf, false);
			newTex.name			= System.IO.Path.GetFileNameWithoutExtension(path); 			
            newTex.SetPixels(colorBuffer);
            newTex.Apply();
						 
            sdfmip.sdfTexture	= newTex;
            sdfmip.maxDistance = sdfData.maxDistance; //  TODO - IS this still constant?

			EditorUtility.SetDirty(sdfmip);
                        
            // create it
            AssetDatabase.CreateAsset(sdfmip, path);
            AssetDatabase.AddObjectToAsset(newTex,sdfmip);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
		}
#endif
    }
}