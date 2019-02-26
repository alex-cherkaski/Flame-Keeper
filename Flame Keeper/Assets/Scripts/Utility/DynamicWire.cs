using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class DynamicWire : MonoBehaviour
{
    public List<Vector3> wirePoints = new List<Vector3>();
    public float radius = 1.0f;
    public int ringSegments = 6;
    public float radialOffset = 0.0f;

    [SerializeField]
    public Color gizmoColor = Color.red;

    [SerializeField]
    public float gizmoSize = 0.05f;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        foreach (Vector3 point in wirePoints)
        {
            Gizmos.DrawCube(point + this.transform.position, new Vector3(gizmoSize, gizmoSize, gizmoSize));
        }
    }

    public void AddNewPoint()
    {
        if (wirePoints.Count < 1)
        {
            wirePoints.Add(new Vector3(0, 0, 0));
        }
        else if (wirePoints.Count < 2)
        {
            wirePoints.Add(wirePoints[0] + Vector3.up * 0.25f);
        }
        else
        {
            // Append new point in the direction of the last normal
            Vector3 lastNormal = Vector3.Normalize(wirePoints[wirePoints.Count - 1] - wirePoints[wirePoints.Count - 2]);
            wirePoints.Add(wirePoints[wirePoints.Count - 1] + lastNormal * 0.25f);
        }
    }

    public void RemoveLastPoint()
    {
        if (wirePoints.Count != 0)
        {
            wirePoints.RemoveAt(wirePoints.Count - 1);
        }
    }

#if UNITY_EDITOR
    public void CreateMesh()
    {
        EditorUtility.SetDirty(this);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        if (wirePoints.Count < 2)
        {
            Debug.LogError("Must define at least two wire points!");
            return;
        }

        Mesh wireMesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        float radianDelta = Mathf.PI * 2.0f / ringSegments;

        // Create all the verts / normals / uvs
        for (int i = 0; i < wirePoints.Count; i++)
        {
            if (i == 0)
            {
                Vector3 firstPoint = wirePoints[i];
                Vector3 secondPoint = wirePoints[i + 1];
                Vector3 normal = Vector3.Normalize(secondPoint - firstPoint);

                Vector3 altVector = normal == Vector3.up ? Vector3.left : Vector3.up;
                Vector3 axis1 = Vector3.Normalize(Vector3.Cross(normal, altVector));
                Vector3 axis2 = Vector3.Normalize(Vector3.Cross(normal, axis1));

                for (int j = 0; j < ringSegments; j++)
                {
                    Vector3 vertPos = firstPoint + axis1 * Mathf.Sin(radialOffset + j * radianDelta) * radius + axis2 * Mathf.Cos(radialOffset + j * radianDelta) * radius;
                    vertices.Add(vertPos);
                    normals.Add(Vector3.Normalize(vertPos - firstPoint));
                    uvs.Add(new Vector2(j / (ringSegments - 1.0f), i / (wirePoints.Count - 1.0f)));
                }
            }
            else if (i == wirePoints.Count - 1)
            {
                Vector3 firstPoint = wirePoints[i - 1];
                Vector3 secondPoint = wirePoints[i];
                Vector3 normal = Vector3.Normalize(secondPoint - firstPoint);

                Vector3 altVector = normal == Vector3.up ? Vector3.left : Vector3.up;
                Vector3 axis1 = Vector3.Normalize(Vector3.Cross(normal, altVector));
                Vector3 axis2 = Vector3.Normalize(Vector3.Cross(normal, axis1));

                for (int j = 0; j < ringSegments; j++)
                {
                    Vector3 vertPos = secondPoint + axis1 * Mathf.Sin(radialOffset + j * radianDelta) * radius + axis2 * Mathf.Cos(radialOffset + j * radianDelta) * radius;
                    vertices.Add(vertPos);
                    normals.Add(Vector3.Normalize(vertPos - secondPoint));
                    uvs.Add(new Vector2(j / (ringSegments - 1.0f), i / (wirePoints.Count - 1.0f)));
                }
            }
            else
            {
                Vector3 firstPoint = wirePoints[i - 1];
                Vector3 secondPoint = wirePoints[i];
                Vector3 thirdPoint = wirePoints[i + 1];
                Vector3 normal1 = Vector3.Normalize(secondPoint - firstPoint);
                Vector3 normal2 = Vector3.Normalize(thirdPoint - secondPoint);
                Vector3 normal = (normal1 + normal2) / 2.0f;

                Vector3 altVector = normal == Vector3.up ? Vector3.left : Vector3.up;
                Vector3 axis1 = Vector3.Normalize(Vector3.Cross(normal, altVector));
                Vector3 axis2 = Vector3.Normalize(Vector3.Cross(normal, axis1));

                for (int j = 0; j < ringSegments; j++)
                {
                    Vector3 vertPos = secondPoint + axis1 * Mathf.Sin(radialOffset + j * radianDelta) * radius + axis2 * Mathf.Cos(radialOffset + j * radianDelta) * radius;
                    vertices.Add(vertPos);
                    normals.Add(Vector3.Normalize(vertPos - secondPoint));
                    uvs.Add(new Vector2(j / (ringSegments - 1.0f), i / (wirePoints.Count - 1.0f)));
                }
            }
        }


        for (int i = 0; i < wirePoints.Count - 1; i++)
        {
            // Add triangles
            for (int j = 0; j < ringSegments; j++)
            {
                int f1 = i * ringSegments + j;
                int f2 = j != ringSegments - 1 ? i * ringSegments + ((j + 1) % ringSegments) : i * ringSegments;
                int s1 = (i + 1) * ringSegments + j;
                int s2 = j != ringSegments - 1 ? (i + 1) * ringSegments + ((j + 1) % ringSegments) : (i + 1) * ringSegments;
                triangles.AddRange(new int[] { f1, s1, f2 });
                triangles.AddRange(new int[] { s1, s2, f2 });
            }
        }

        wireMesh.SetVertices(vertices);
        wireMesh.SetNormals(normals);
        wireMesh.SetTriangles(triangles, 0);
        wireMesh.SetUVs(0, uvs);

        this.GetComponent<MeshFilter>().mesh = wireMesh;
    }
#endif
}
