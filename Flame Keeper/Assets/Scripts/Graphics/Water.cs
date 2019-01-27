using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is used to update a water shader with realtime
/// updates. So, reflections, ripples, etc.
/// </summary>
public class Water : MonoBehaviour
{
    public bool disablePixelLights = true;
    public int textureSize = 1028;
    public float clipPlaneOffset = 0.07f;
    public LayerMask reflectLayers = -1;

    private bool initalized = false;

    private Dictionary<Camera, Camera> reflCams = new Dictionary<Camera, Camera>();
    private RenderTexture reflTex = null;
    private int oldReflTexSize;
    private Dictionary<Camera, float> camState = new Dictionary<Camera, float>();

    static bool recursiveGuard;

    public void Init()
    {
        initalized = true;
    }

    public void OnWillRenderObject()
    {
        if (!initalized)
        {
            Init();
        }

        if (!enabled || !GetComponent<Renderer>() || !GetComponent<Renderer>().sharedMaterial || !GetComponent<Renderer>().enabled)
        {
            return;
        }

        Camera cam = Camera.current;
        if (!cam) return;

        // Render only once per camera
        float lastRender;
        if (camState.TryGetValue(cam, out lastRender))
        {
            if (Mathf.Approximately(Time.time, lastRender) && Application.isPlaying) return;
            camState[cam] = Time.time;
        }
        else
        {
            camState.Add(cam, Time.time);
        }

        // Safeguard from recursive water reflections.
        if (recursiveGuard) return;
        recursiveGuard = true;

        Camera reflectionCamera;
        CreateWaterObjects(cam, out reflectionCamera);

        // find out the reflection plane: position and normal in world space
        Vector3 pos = transform.position;
        Vector3 normal = transform.up;

        // Optionally disable pixel lights for reflection/refraction
        int oldPixelLightCount = QualitySettings.pixelLightCount;
        if (disablePixelLights)
        {
            QualitySettings.pixelLightCount = 0;
        }

        UpdateCameraModes(cam, reflectionCamera);

        // Reflect camera around reflection plane
        float d = -Vector3.Dot(normal, pos) - clipPlaneOffset;
        Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

        Matrix4x4 reflection = Matrix4x4.zero;
        CalculateReflectionMatrix(ref reflection, reflectionPlane);
        Vector3 oldpos = cam.transform.position;
        Vector3 newpos = reflection.MultiplyPoint(oldpos);
        reflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;

        // Setup oblique projection matrix so that near plane is our reflection
        // plane. This way we clip everything below/above it for free.
        Vector4 clipPlane = CameraSpacePlane(reflectionCamera, pos, normal, 1.0f);
        reflectionCamera.projectionMatrix = cam.CalculateObliqueMatrix(clipPlane);

        reflectionCamera.cullingMask = ~(1 << 4) & reflectLayers.value; // never render water layer
        reflectionCamera.targetTexture = reflTex;
        GL.invertCulling = true;
        reflectionCamera.transform.position = newpos;
        Vector3 euler = cam.transform.eulerAngles;
        reflectionCamera.transform.eulerAngles = new Vector3(-euler.x, euler.y, euler.z);
        reflectionCamera.Render();
        reflectionCamera.transform.position = oldpos;
        GL.invertCulling = false;
        GetComponent<Renderer>().sharedMaterial.SetTexture("_ReflectionTex", reflTex);

        // Restore pixel light count
        if (disablePixelLights)
        {
            QualitySettings.pixelLightCount = oldPixelLightCount;
        }

        recursiveGuard = false;
    }

    void OnDisable()
    {
        if (reflTex != null)
        {
            Destroy_(reflTex);
            reflTex = null;
        }

        foreach (var kvp in reflCams)
        {
            if (kvp.Value != null)
            {
                Destroy_((kvp.Value).gameObject);
            }
        }

        reflCams.Clear();

        camState.Clear();
    }

    public void Destroy_(Object o)
    {
        if (Application.isPlaying) Destroy(o);
        else DestroyImmediate(o);
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


    // On-demand create any objects we need for water
    void CreateWaterObjects(Camera currentCamera, out Camera reflectionCamera)
    {

        reflectionCamera = null;

        // Reflection render texture
        if (!reflTex || oldReflTexSize != textureSize)
        {
            if (reflTex)
            {
                DestroyImmediate(reflTex);
            }
            reflTex = new RenderTexture(textureSize, textureSize, 16);
            reflTex.name = "__WaterReflection" + GetInstanceID();
            reflTex.isPowerOfTwo = true;
            oldReflTexSize = textureSize;
        }

        // Camera for reflection
        reflCams.TryGetValue(currentCamera, out reflectionCamera);
        if (!reflectionCamera) // catch both not-in-dictionary and in-dictionary-but-deleted-GO
        {
            GameObject go = new GameObject("Water Refl Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera));
            reflectionCamera = go.GetComponent<Camera>();
            reflectionCamera.enabled = false;
            reflectionCamera.transform.position = transform.position;
            reflectionCamera.transform.rotation = transform.rotation;
            reflectionCamera.gameObject.AddComponent<FlareLayer>();
            reflCams[currentCamera] = reflectionCamera;
        }
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

