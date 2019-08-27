using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SDFr
{
	public enum Primitive { sphere, box }

	[ExecuteInEditMode] //required for previewing
	public class SDFGenerator : AVolumeBaker<SDFVolume, SDFData>
	{
		// public class SDFPrimitive		{			public Primitive primitive;		}

		[SerializeField] private float previewEpsilon = 0.003f;
		[SerializeField] private float previewNormalDelta = 0.02f;
        [SerializeField] private Visualisation previewMode = Visualisation.Normal;

        [SerializeField] private SDFData sdfData;
		[SerializeField] private Texture3D debugTex3D; //for viewing existing texture3D not baked with SDFr

		//		[SerializeField] private Transform[] primitives; //for viewing existing texture3D not baked with SDFr

			

		public override int MaxDimension => 256;

#if UNITY_EDITOR

		private const string _sdfPreviewShaderName = "XRA/SDFr";
		private static Shader _shader; //TODO better way 

		public override AVolumePreview<SDFData> CreatePreview()
		{
			if ( _shader == null ) _shader = Shader.Find( _sdfPreviewShaderName );

			AVolumePreview<SDFData> sdf = new SDFPreview(sdfData, _shader, transform);

			if ( debugTex3D != null ) ( sdf as SDFPreview ).debugTex3D = debugTex3D;

			return sdf;
		}

		/// <summary>renders a procedural quad TODO fit the bounds of the SDF Volume</summary>
		private void OnRenderObject()
		{
			if ( !IsPreviewing ) return;

			//try to get active camera...
			Camera cam = Camera.main;

			if ( UnityEditor.SceneView.currentDrawingSceneView != null ) // lastActiveSceneView            
				cam = UnityEditor.SceneView.currentDrawingSceneView.camera;

			SDFPreview preview = _aPreview as SDFPreview;

			preview?.Draw( cam, false, previewEpsilon, previewNormalDelta );
		}

		[ContextMenu("Bake")]
		public override void Bake()
		{
			if ( bakedRenderers == null ) bakedRenderers = new List<Renderer>();

			bakedRenderers.Clear();

			AVolumeSettings settings = new AVolumeSettings(bounds, dimensions, useStandardBorder);

			//first check if any objects are parented to this object
			//if anything is found, try to use renderers from those instead of volume overlap
			if ( !SDFBaker.GetMeshRenderersInChildren( ref settings, ref bakedRenderers, transform, fitToVertices ) )
			{
				//otherwise try to get renderers intersecting the volume
				//get mesh renderers within volume
				if ( !SDFBaker.GetMeshRenderersIntersectingVolume( settings, transform, ref bakedRenderers ) )
				{
					// TODO: display error
					return;
				}
			}

			SDFVolume sdfVolume = AVolume<SDFVolume>.CreateVolume(transform, settings);

			// sdfVolume.Bake( raySamples, bakedRenderers, BakeComplete );

			Debug.Log( $"settings: {settings.BoundsLocal} {settings.Dimensions} {settings.CellCount} " );

			sdfVolume.Dispose();
		}

		//TODO improve asset saving 
		private void BakeComplete( SDFVolume sdfVolume, float[] distances, float maxDistance, object passthrough )
		{
			//update the bounds since they may have been adjusted during bake
			bounds = sdfVolume.Settings.BoundsLocal;

			string path = "";
			if ( sdfData != null )
			{
				//use path of existing sdfData
				path = AssetDatabase.GetAssetPath( sdfData );

				//check if asset at path
				Object obj = AssetDatabase.LoadAssetAtPath<SDFData>(path);
				if ( obj != null )
				{
					if ( ( ( SDFData )obj ).sdfTexture != null )
					{
						//destroy old texture
						Object.DestroyImmediate( ( ( SDFData )obj ).sdfTexture, true );
					}
					//destroy old asset
					//TODO this will break references...
					Object.DestroyImmediate( obj, true );
				}
			}

			// If not exisiting asset get path to save sdfData to.
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


			//create new SDFData
			sdfData = ScriptableObject.CreateInstance<SDFData>();
			sdfData.bounds = sdfVolume.Settings.BoundsLocal;
			sdfData.voxelSize = sdfVolume.Settings.VoxelSize;
			sdfData.dimensions = sdfVolume.Settings.Dimensions;

			float minAxis = Mathf.Min( sdfData.bounds.size.x, Mathf.Min( sdfData.bounds.size.y, sdfData.bounds.size.z ) );
			sdfData.nonUniformScale = new Vector3( sdfData.bounds.size.x / minAxis, sdfData.bounds.size.y / minAxis, sdfData.bounds.size.z / minAxis );

			// Create Texture3D and set name to filename of sdfData
			Texture3D newTex    = new Texture3D( sdfData.dimensions.x, sdfData.dimensions.y, sdfData.dimensions.z, TextureFormat.RHalf, false);
			newTex.name = System.IO.Path.GetFileNameWithoutExtension( path );

			//TODO improve
			Color[] colorBuffer = new Color[distances.Length];
			for ( int i = 0; i < distances.Length; i++ )
			{
				//NOTE for compatibility with Visual Effect Graph, 
				//the distance must be negative inside surfaces.
				//normalize the distance for better support of scaling bounds
				//Max Distance is always the Magnitude of the baked bound size
				float normalizedDistance = distances[i] / maxDistance;
				colorBuffer[ i ] = new Color( normalizedDistance, 0f, 0f, 0f );
			}
			newTex.SetPixels( colorBuffer );
			newTex.Apply();


			sdfData.sdfTexture = newTex;
			sdfData.maxDistance = maxDistance;

			EditorUtility.SetDirty( sdfData );

			//create it
			AssetDatabase.CreateAsset( sdfData, path );
			AssetDatabase.AddObjectToAsset( newTex, sdfData );

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			if ( IsPreviewing )
				TogglePreview();
			if ( !IsPreviewing )
				TogglePreview();
		}
#endif
	}
}
