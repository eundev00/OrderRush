using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class SmoothNormalsToVertexColor : MonoBehaviour
{
    static Dictionary<Mesh, Mesh> cache = new Dictionary<Mesh, Mesh>();

    void Awake()
    {
        var meshFilter = GetComponent<MeshFilter>();
        var original = meshFilter.sharedMesh;

        if (original == null) return;

        if (!cache.TryGetValue(original, out Mesh baked))
        {
            baked = Instantiate(original);
            BakeSmoothNormals(baked);
            cache[original] = baked;
        }

        meshFilter.mesh = baked;
    }

    void BakeSmoothNormals(Mesh mesh)
    {
        var vertices = mesh.vertices;
        var normals = mesh.normals;

        var groups = new Dictionary<Vector3, List<int>>();
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 pos = vertices[i];
            if (!groups.ContainsKey(pos)) groups[pos] = new List<int>();
            groups[pos].Add(i);
        }

        var colors = new Color[vertices.Length];

        foreach (var group in groups.Values)
        {
            Vector3 sum = Vector3.zero;
            foreach (int i in group) sum += normals[i];
            Vector3 avg = (sum / group.Count).normalized;

            Color encoded = new Color(avg.x * 0.5f + 0.5f, avg.y * 0.5f + 0.5f, avg.z * 0.5f + 0.5f, 1f);
            foreach (int i in group) colors[i] = encoded;
        }

        mesh.colors = colors;
    }
}