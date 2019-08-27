using UnityEngine;

namespace SDFr
{
	public class SDFData : AVolumeData
	{
		public Texture3D sdfTexture;
		public float maxDistance;
		
		private static readonly int _SDFVolumeTex = Shader.PropertyToID("_SDFVolumeTex");
		private static readonly int _SDFVolumeExtents = Shader.PropertyToID("_SDFVolumeExtents");

		public void SetMaterialProperties(MaterialPropertyBlock props)
		{
			props.SetTexture(_SDFVolumeTex, sdfTexture);            
			props.SetVector(_SDFVolumeExtents, bounds.extents);
			//TODO apply atlas etc 
		}
	}
}
