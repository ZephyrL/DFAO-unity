using UnityEngine;

public class VolumeComputeMethods
{
	// static ComputeShader volumeComputeMethodsShader;	
	// static void LoadResources() { volumeComputeMethodsShader = Resources.Load() }

	public static float[] ExtractVolumeFloatData( Texture3D volume, ComputeShader shader )
	{		
		float[] values  = new float[volume.width * volume.height * volume.depth];
		int[] dim  = new int[]{ volume.width, volume.height, volume.depth };
			
		ComputeBuffer valuesBuffer = new ComputeBuffer(values.Length, 4);
		valuesBuffer.SetData(values);

		int kernel = shader.FindKernel("ExtractVolumeData");
		shader.SetInts("dimensions", dim);
		shader.SetTexture(kernel, "volume", volume);
		shader.SetBuffer(kernel, "values", valuesBuffer);
		shader.Dispatch(kernel, volume.width, volume.height, volume.depth);

		valuesBuffer.GetData(values);

		valuesBuffer.Dispose();

		return values;
	}
	
}
