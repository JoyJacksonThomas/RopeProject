using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class FoliageShaderSupport : MonoBehaviour
{
    [Tooltip("Shape normals will point away from the closest position in this list")]
    [SerializeField] private List<Transform> normalFoci;

    private void Run()
    {
        // Grab the mesh and its data
        var mesh = GetComponent<MeshFilter>().sharedMesh;
        var positions = mesh.vertices;
        // Instantiate final data arrays
        var colors = new Color[positions.Length];

        // Convert to world space. This is OK since this mesh is static
        for (int vertex = 0; vertex < positions.Length; vertex++)
        {
            positions[vertex] = transform.TransformPoint(positions[vertex]);
        }

        for (int vertex = 0; vertex < positions.Length; vertex++)
        {
            // Normalize the vector pointing from the nearest normal focii to this vertex
            float3 normal = math.normalize((float3)positions[vertex] - FindClosestNormalFocus(positions[vertex]));
            // And store that as the shape normal
            colors[vertex] = new Color(normal.x, normal.y, normal.z, 0);
        }

        // Store shape normals in the vertex color data
        mesh.SetColors(colors);
    }

    // This function receives a position and returned the position of the closest
    // normal center transform
    private float3 FindClosestNormalFocus(float3 pos)
    {
        int closestID = 0;
        float closestDistanceSq = float.MaxValue;
        for (int i = 0; i < normalFoci.Count; i++)
        {
            float distanceSq = math.distancesq(pos, normalFoci[i].position);
            if (distanceSq < closestDistanceSq)
            {
                closestID = i;
                closestDistanceSq = distanceSq;
            }
        }
        return normalFoci[closestID].position;
    }

    // Run this script on awake
    private void Awake()
    {
        Run();
    }
}
