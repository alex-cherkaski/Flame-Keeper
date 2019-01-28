using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is used to update a water shader with realtime
/// updates. So, reflections, ripples, etc.
/// </summary>
public class Water : MonoBehaviour
{
    static bool recursiveGuard;

    private Camera mainCamera;
    private float lastRender;
    private Renderer waterRenderer;

    public Camera reflectionCamera;
    public Camera rippleCamera;
    public RenderTexture reflectionTexture;
    public RenderTexture rippleTexture;
    public RippleEmitter ripplePrefab;

    public bool disablePixelLights = true;
    public float clipPlaneOffset = 0.07f;
    public LayerMask reflectLayers = -1;

    private void Start()
    {
        mainCamera = Camera.main; // This operation is real slow so you should only ever do it once
        waterRenderer = GetComponent<Renderer>();
        waterRenderer.sharedMaterial.SetTexture("_RippleTex", rippleTexture);
        waterRenderer.sharedMaterial.SetFloat("_RippleCamSize", rippleCamera.orthographicSize);
    }

    private void Update()
    {
        // Set positions of all ripple objects
        Vector3 p = rippleCamera.transform.position;
        waterRenderer.sharedMaterial.SetVector("_RippleCameraPosition", new Vector4(p.x, p.y, p.z));
    }

   void OnTriggerEnter(Collider other)
    {
        RippleEmitter ripples = RippleEmitter.Instantiate(ripplePrefab, other.transform);
    }

    void OnTriggerExit(Collider other)
    {
        RippleEmitter[] ripples = other.GetComponentsInChildren<RippleEmitter>();
        int length = ripples.Length;
        for (int i = 0; i < length; i++)
        {
            ripples[i].StopEmitting();
            Destroy(ripples[i].gameObject, ripples[i].GetMaxLifetime()); // Destory in a little bit, so ripples have time to fade out
        }
    }

    public void OnWillRenderObject()
    {
        if (!enabled || !GetComponent<Renderer>() || !GetComponent<Renderer>().sharedMaterial || !GetComponent<Renderer>().enabled)
        {
            return;
        }

        if (Camera.current != mainCamera) return;

        // Render only once per camera
        if (Mathf.Approximately(Time.time, lastRender) && Application.isPlaying) return;
        lastRender = Time.time;

        // Safeguard from recursive water reflections.
        if (recursiveGuard) return;
        recursiveGuard = true;

        // find out the reflection plane: position and normal in world space
        Vector3 pos = transform.position;
        Vector3 normal = transform.up;

        // Optionally disable pixel lights for reflection/refraction
        int oldPixelLightCount = QualitySettings.pixelLightCount;
        if (disablePixelLights)
        {
            QualitySettings.pixelLightCount = 0;
        }

        UpdateCameraModes(mainCamera, reflectionCamera);

        // Reflect camera around reflection plane
        float d = -Vector3.Dot(normal, pos) - clipPlaneOffset;
        Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

        Matrix4x4 reflection = Matrix4x4.zero;
        CalculateReflectionMatrix(ref reflection, reflectionPlane);
        Vector3 oldpos = mainCamera.transform.position;
        Vector3 newpos = reflection.MultiplyPoint(oldpos);
        reflectionCamera.worldToCameraMatrix = mainCamera.worldToCameraMatrix * reflection;

        // Setup oblique projection matrix so that near plane is our reflection
        // plane. This way we clip everything below/above it for free.
        Vector4 clipPlane = CameraSpacePlane(reflectionCamera, pos, normal, 1.0f);
        reflectionCamera.projectionMatrix = mainCamera.CalculateObliqueMatrix(clipPlane);

        reflectionCamera.cullingMask = ~(1 << 4) & reflectLayers.value; // never render water layer
        reflectionCamera.targetTexture = reflectionTexture;
        GL.invertCulling = true;
        reflectionCamera.transform.position = newpos;
        Vector3 euler = mainCamera.transform.eulerAngles;
        reflectionCamera.transform.eulerAngles = new Vector3(-euler.x, euler.y, euler.z);
        reflectionCamera.Render();
        reflectionCamera.transform.position = oldpos;
        GL.invertCulling = false;
        GetComponent<Renderer>().sharedMaterial.SetTexture("_ReflectionTex", reflectionTexture);

        // Restore pixel light count
        if (disablePixelLights)
        {
            QualitySettings.pixelLightCount = oldPixelLightCount;
        }

        recursiveGuard = false;
    }

    void UpdateCameraModes(Camera src, Camera dest)
    {
        if (dest == null)
        {
            return;
        }

        dest.allowMSAA = false; // Too expensive, imo
        dest.farClipPlane = src.farClipPlane;
        dest.nearClipPlane = src.nearClipPlane;
        dest.orthographic = src.orthographic;
        dest.fieldOfView = src.fieldOfView;
        dest.aspect = src.aspect;
        dest.orthographicSize = src.orthographicSize;
    }

    // Given position/normal of the plane, calculates plane in camera space.
    Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
    {
        Vector3 offsetPos = pos + normal * clipPlaneOffset;
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(offsetPos);
        Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }

    // Calculates reflection matrix around the given plane
    static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
    {
        reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
        reflectionMat.m01 = (-2F * plane[0] * plane[1]);
        reflectionMat.m02 = (-2F * plane[0] * plane[2]);
        reflectionMat.m03 = (-2F * plane[3] * plane[0]);

        reflectionMat.m10 = (-2F * plane[1] * plane[0]);
        reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
        reflectionMat.m12 = (-2F * plane[1] * plane[2]);
        reflectionMat.m13 = (-2F * plane[3] * plane[1]);

        reflectionMat.m20 = (-2F * plane[2] * plane[0]);
        reflectionMat.m21 = (-2F * plane[2] * plane[1]);
        reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
        reflectionMat.m23 = (-2F * plane[3] * plane[2]);

        reflectionMat.m30 = 0F;
        reflectionMat.m31 = 0F;
        reflectionMat.m32 = 0F;
        reflectionMat.m33 = 1F;
    }

}

