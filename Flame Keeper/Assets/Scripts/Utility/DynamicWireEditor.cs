using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DynamicWire)), CanEditMultipleObjects]
public class ObjectBuilderEditor : Editor
{
    private bool needToRebuildMesh = false;
    private bool editPoints = false;

    public override void OnInspectorGUI()
    {
        DynamicWire wireScript = (DynamicWire)target;

        EditorGUILayout.LabelField("Wire Editing", EditorStyles.boldLabel);

        if (GUILayout.Button("Add new point"))
        {
            wireScript.AddNewPoint();
            needToRebuildMesh = true;
        }

        if (GUILayout.Button("Remove last point"))
        {
            wireScript.RemoveLastPoint();
            needToRebuildMesh = true;
        }

        editPoints = EditorGUILayout.Foldout(editPoints, "Edit Points");
        if (editPoints)
        {
            int i = 1;
            List<Vector3> newPoints = new List<Vector3>();
            foreach (Vector3 point in wireScript.wirePoints)
            {
                Vector3 newPoint = EditorGUILayout.Vector3Field("Point " + i.ToString(), point);
                needToRebuildMesh = needToRebuildMesh || newPoint != point;
                newPoints.Add(newPoint);
                i++;
            }
            wireScript.wirePoints = newPoints;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Wire Options", EditorStyles.boldLabel);

        // Radius editor field
        float radius = EditorGUILayout.FloatField("Wire radius", wireScript.radius);
        needToRebuildMesh = needToRebuildMesh || radius != wireScript.radius;
        wireScript.radius = radius;

        // Ring segments editor field
        int ringSegments = EditorGUILayout.IntField("Ring Segments", wireScript.ringSegments);
        needToRebuildMesh = needToRebuildMesh || ringSegments != wireScript.ringSegments;
        wireScript.ringSegments = ringSegments;

        // Radial offset editor field
        float radialOffset = EditorGUILayout.FloatField("Radial Offset", wireScript.radialOffset);
        needToRebuildMesh = needToRebuildMesh || radialOffset != wireScript.radialOffset;
        wireScript.radialOffset = radialOffset;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Gizmo Options", EditorStyles.boldLabel);

        // Gizmo color editor field
        Color gizmoColor = EditorGUILayout.ColorField("Gizmo Color", wireScript.gizmoColor);
        needToRebuildMesh = needToRebuildMesh || gizmoColor != wireScript.gizmoColor;
        wireScript.gizmoColor = gizmoColor;

        // Gizmo size editor field
        float gizmoSize = EditorGUILayout.FloatField("Gizmo Size", wireScript.gizmoSize);
        needToRebuildMesh = needToRebuildMesh || gizmoSize != wireScript.gizmoSize;
        wireScript.gizmoSize = gizmoSize;

        // Rebuild mesh if anything changed
        if (needToRebuildMesh)
        {
            wireScript.CreateMesh();
        }

        // Use this as a quick fix if for some reason the automatic mesh rebuild breaks / doesn't trigger
        /*
        if (GUILayout.Button("Create Mesh"))
        {
            wireScript.CreateMesh();
        }
        */

        needToRebuildMesh = false;
    }
}
