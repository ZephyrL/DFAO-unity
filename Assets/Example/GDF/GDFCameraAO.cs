using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class GDFCameraAO : MonoBehaviour
{
    [Header("Ambient Occlusion")]
    public bool _blend = false; // whether blend the shadowing effectes with source frame
    [Range(0.001f, 0.5f)]
    public float _aoStepSize = 0.2f;
    [Range(1, 5)]
    public int _aoMaxIter = 3;
    [Range(-0.25f, 1.0f)]
    public float _aoIntensity = 0.3f;

    [Range(4, 64)]
    public int _shadowPenumbra = 6;
    [Range(0, 1)]
    public float _shadowIntensity = 0.5f;

    [Header("Ray Marching Ambient Occlusion")]
    public bool _rayMarching = false;
    [Header("Enabled only when RM is checked")]
    [Range(1, 300)]
    public int _maxIteration = 200;
    [Range(0.001f, 0.01f)]
    public float _accuracy = 0.003f;

    void OnEnable()
    {
        if (!_AOShader)
        {
            if (!_rayMarching)
            {
                _AOShader = Shader.Find("GDF/GDFCameraScreenAO");
            }
            else
            {
                _AOShader = Shader.Find("GDF/GDFCameraRMAO");
            }
        }

        _material = new Material(_AOShader);
        _camera = GetComponent<Camera>();
        _camera.depthTextureMode = DepthTextureMode.Depth;
    }

    private void OnDisable()
    {
        // dispose components
    }

    private Material _material;
    private Camera _camera;
    private Shader _AOShader;
    
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!_material)
        {
            Graphics.Blit(source, destination);
            return;
        }

        _material.SetMatrix("_CamFrustum", CameraFrustum(_camera));
        _material.SetMatrix("_CamToWorld", _camera.cameraToWorldMatrix);
        _material.SetInt("_blendWithScene", _blend ? 1 : 0);

        _material.SetFloat("_aoStepSize", _aoStepSize);
        _material.SetInt("_aoMaxIter", _aoMaxIter);
        _material.SetFloat("_aoIntensity", _aoIntensity);

        _material.SetFloat("_shadowPenumbra", _shadowPenumbra);
        _material.SetFloat("_shadowIntensity", _shadowIntensity);

        _material.SetInt("_maxIteration", _maxIteration);
        _material.SetFloat("_accuracy", _accuracy);

        // Set GL screen QUADS
        RenderTexture.active = destination;
        _material.SetTexture("_MainTex", source);
        GL.PushMatrix();
        GL.LoadOrtho();

        _material.SetPass(0);
        GL.Begin(GL.QUADS);
        // BL
        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.0f, 3.0f);
        // BR
        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 2.0f);
        // TR
        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 1.0f);
        // TL
        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f);

        GL.End();
        GL.PopMatrix();
    }

    private Matrix4x4 CameraFrustum(Camera cam)
    {
        Matrix4x4 frustum = Matrix4x4.identity;
        float fov = Mathf.Tan((cam.fieldOfView * 0.5f) * Mathf.Deg2Rad);

        Vector3 _up = Vector3.up * fov;
        Vector3 _right = Vector3.right * fov * cam.aspect;

        Vector3 TL = (-Vector3.forward + _up - _right);
        Vector3 TR = (-Vector3.forward + _up + _right);
        Vector3 BR = (-Vector3.forward - _up + _right);
        Vector3 BL = (-Vector3.forward - _up - _right);

        frustum.SetRow(0, TL);
        frustum.SetRow(1, TR);
        frustum.SetRow(2, BR);
        frustum.SetRow(3, BL);

        return frustum;
    }
}
