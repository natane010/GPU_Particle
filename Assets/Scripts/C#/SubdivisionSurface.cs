using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

public class Model
{
    List<Vertex> vertices;
    List<Edge> edges;
    public List<Triangle> triangles;

    public Model()
    {
        this.vertices = new List<Vertex>();
        this.edges = new List<Edge>();
        this.triangles = new List<Triangle>();
    }

    public Model(Mesh source) : this()
    {
        var points = source.vertices;
        for (int i = 0, n = points.Length; i < n; i++)
        {
            var v = new Vertex(points[i], i);
            vertices.Add(v);
        }

        var triangles = source.triangles;
        for (int i = 0, n = triangles.Length; i < n; i += 3)
        {
            int i0 = triangles[i], i1 = triangles[i + 1], i2 = triangles[i + 2];
            Vertex v0 = vertices[i0], v1 = vertices[i1], v2 = vertices[i2];

            var e0 = GetEdge(edges, v0, v1);
            var e1 = GetEdge(edges, v1, v2);
            var e2 = GetEdge(edges, v2, v0);
            var f = new Triangle(v0, v1, v2, e0, e1, e2);

            this.triangles.Add(f);
            v0.AddTriangle(f); v1.AddTriangle(f); v2.AddTriangle(f);
            e0.AddTriangle(f); e1.AddTriangle(f); e2.AddTriangle(f);
        }
    }

    Edge GetEdge(List<Edge> edges, Vertex v0, Vertex v1)
    {
        var match = v0.edges.Find(e => {
            return e.Has(v1);
        });
        if (match != null) return match;

        var ne = new Edge(v0, v1);
        v0.AddEdge(ne);
        v1.AddEdge(ne);
        edges.Add(ne);
        return ne;
    }

    public void AddTriangle(Vertex v0, Vertex v1, Vertex v2)
    {
        if (!vertices.Contains(v0)) vertices.Add(v0);
        if (!vertices.Contains(v1)) vertices.Add(v1);
        if (!vertices.Contains(v2)) vertices.Add(v2);

        var e0 = GetEdge(v0, v1);
        var e1 = GetEdge(v1, v2);
        var e2 = GetEdge(v2, v0);
        var f = new Triangle(v0, v1, v2, e0, e1, e2);

        this.triangles.Add(f);
        v0.AddTriangle(f); v1.AddTriangle(f); v2.AddTriangle(f);
        e0.AddTriangle(f); e1.AddTriangle(f); e2.AddTriangle(f);
    }

    Edge GetEdge(Vertex v0, Vertex v1)
    {
        var match = v0.edges.Find(e =>
        {
            return (e.a == v1 || e.b == v1);
        });
        if (match != null) return match;

        var ne = new Edge(v0, v1);
        edges.Add(ne);
        v0.AddEdge(ne);
        v1.AddEdge(ne);
        return ne;
    }

    public Mesh Build(bool weld = false)
    {
        var mesh = new Mesh();
        var triangles = new int[this.triangles.Count * 3];

        if (weld)
        {
            for (int i = 0, n = this.triangles.Count; i < n; i++)
            {
                var f = this.triangles[i];
                triangles[i * 3] = vertices.IndexOf(f.v0);
                triangles[i * 3 + 1] = vertices.IndexOf(f.v1);
                triangles[i * 3 + 2] = vertices.IndexOf(f.v2);
            }
            mesh.vertices = vertices.Select(v => v.p).ToArray();
        }
        else
        {
            var vertices = new Vector3[this.triangles.Count * 3];
            for (int i = 0, n = this.triangles.Count; i < n; i++)
            {
                var f = this.triangles[i];
                int i0 = i * 3, i1 = i * 3 + 1, i2 = i * 3 + 2;
                vertices[i0] = f.v0.p;
                vertices[i1] = f.v1.p;
                vertices[i2] = f.v2.p;
                triangles[i0] = i0;
                triangles[i1] = i1;
                triangles[i2] = i2;
            }
            mesh.vertices = vertices;
        }

        mesh.indexFormat = mesh.vertexCount < 65535 ? IndexFormat.UInt16 : IndexFormat.UInt32;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

}

public class Triangle
{
    public Vertex v0, v1, v2;
    public Edge e0, e1, e2;

    public Triangle(
        Vertex v0, Vertex v1, Vertex v2,
        Edge e0, Edge e1, Edge e2
    )
    {
        this.v0 = v0;
        this.v1 = v1;
        this.v2 = v2;

        this.e0 = e0;
        this.e1 = e1;
        this.e2 = e2;
    }

    public Vertex GetOtherVertex(Edge e)
    {
        if (!e.Has(v0)) return v0;
        else if (!e.Has(v1)) return v1;
        return v2;
    }

}

public class Edge
{
    public Vertex a, b;
    public List<Triangle> faces;
    public Vertex ept;

    public Edge(Vertex a, Vertex b)
    {
        this.a = a;
        this.b = b;
        this.faces = new List<Triangle>();
    }

    public void AddTriangle(Triangle f)
    {
        faces.Add(f);
    }

    public bool Has(Vertex v)
    {
        return v == a || v == b;
    }

    public Vertex GetOtherVertex(Vertex v)
    {
        if (a != v) return a;
        return b;
    }
}

public class Vertex
{
    public Vector3 p;
    public List<Edge> edges;
    public List<Triangle> triangles;
    public Vertex updated;

    // reference index to original vertex
    public int index;

    public Vertex(Vector3 p) : this(p, -1)
    {
    }

    public Vertex(Vector3 p, int index)
    {
        this.p = p;
        this.index = index;
        this.edges = new List<Edge>();
        this.triangles = new List<Triangle>();
    }

    public void AddEdge(Edge e)
    {
        edges.Add(e);
    }

    public void AddTriangle(Triangle f)
    {
        triangles.Add(f);
    }

}

public class SubdivisionSurface
{
    public static int count = 0;
    public static Mesh Subdivide(Mesh source, int details = 1, bool weld = false)
    {
        var model = Subdivide(source, details);
        var mesh = model.Build(weld);
        mesh.OptimizeReorderVertexBuffer();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateUVDistributionMetrics();
        mesh.RecalculateBounds();
        
        //AssetDatabase.CreateAsset(mesh, "Assets/prefabs/mekemesh" + count + ".mesh");
        return mesh;
    }

    public static Model Subdivide(Mesh source, int details = 1)
    {
        var model = new Model(source);
        var divider = new SubdivisionSurface();

        for (int i = 0; i < details; i++)
        {
            model = divider.Divide(model);
        }
        
        return model;
    }

    public static Mesh Weld(Mesh mesh, float threshold, float bucketStep)
    {
        Vector3[] oldVertices = mesh.vertices;
        Vector3[] newVertices = new Vector3[oldVertices.Length];
        int[] old2new = new int[oldVertices.Length];
        int newSize = 0;

        // Find AABB
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        for (int i = 0; i < oldVertices.Length; i++)
        {
            if (oldVertices[i].x < min.x) min.x = oldVertices[i].x;
            if (oldVertices[i].y < min.y) min.y = oldVertices[i].y;
            if (oldVertices[i].z < min.z) min.z = oldVertices[i].z;
            if (oldVertices[i].x > max.x) max.x = oldVertices[i].x;
            if (oldVertices[i].y > max.y) max.y = oldVertices[i].y;
            if (oldVertices[i].z > max.z) max.z = oldVertices[i].z;
        }

        // Make cubic buckets, each with dimensions "bucketStep"
        int bucketSizeX = Mathf.FloorToInt((max.x - min.x) / bucketStep) + 1;
        int bucketSizeY = Mathf.FloorToInt((max.y - min.y) / bucketStep) + 1;
        int bucketSizeZ = Mathf.FloorToInt((max.z - min.z) / bucketStep) + 1;
        List<int>[,,] buckets = new List<int>[bucketSizeX, bucketSizeY, bucketSizeZ];

        // Make new vertices
        for (int i = 0; i < oldVertices.Length; i++)
        {
            // Determine which bucket it belongs to
            int x = Mathf.FloorToInt((oldVertices[i].x - min.x) / bucketStep);
            int y = Mathf.FloorToInt((oldVertices[i].y - min.y) / bucketStep);
            int z = Mathf.FloorToInt((oldVertices[i].z - min.z) / bucketStep);

            // Check to see if it's already been added
            if (buckets[x, y, z] == null) buckets[x, y, z] = new List<int>(); // Make buckets lazily

            for (int j = 0; j < buckets[x, y, z].Count; j++)
            {
                Vector3 to = newVertices[buckets[x, y, z][j]] - oldVertices[i];
                if (Vector3.SqrMagnitude(to) < threshold)
                {
                    old2new[i] = buckets[x, y, z][j];
                    goto skip; // Skip to next old vertex if this one is already there
                }
            }

            // Add new vertex
            newVertices[newSize] = oldVertices[i];
            buckets[x, y, z].Add(newSize);
            old2new[i] = newSize;
            newSize++;

        skip:;
        }

        // Make new triangles
        int[] oldTris = mesh.triangles;
        int[] newTris = new int[oldTris.Length];
        for (int i = 0; i < oldTris.Length; i++)
        {
            newTris[i] = old2new[oldTris[i]];
        }

        Vector3[] finalVertices = new Vector3[newSize];
        for (int i = 0; i < newSize; i++)
        {
            finalVertices[i] = newVertices[i];
        }

        mesh.Clear();
        mesh.vertices = finalVertices;
        mesh.triangles = newTris;
        mesh.RecalculateNormals();

        return mesh;
    }

    public Edge GetEdge(List<Edge> edges, Vertex v0, Vertex v1)
    {
        var match = v0.edges.Find(e => {
            return e.Has(v1);
        });
        if (match != null) return match;

        var ne = new Edge(v0, v1);
        v0.AddEdge(ne);
        v1.AddEdge(ne);
        edges.Add(ne);
        return ne;
    }

    Model Divide(Model model)
    {
        var nmodel = new Model();
        for (int i = 0, n = model.triangles.Count; i < n; i++)
        {
            var f = model.triangles[i];

            var ne0 = GetEdgePoint(f.e0);
            var ne1 = GetEdgePoint(f.e1);
            var ne2 = GetEdgePoint(f.e2);

            var nv0 = GetVertexPoint(f.v0);
            var nv1 = GetVertexPoint(f.v1);
            var nv2 = GetVertexPoint(f.v2);

            nmodel.AddTriangle(nv0, ne0, ne2);
            nmodel.AddTriangle(ne0, nv1, ne1);
            nmodel.AddTriangle(ne0, ne1, ne2);
            nmodel.AddTriangle(ne2, ne1, nv2);
        }
        return nmodel;
    }

    public Vertex GetEdgePoint(Edge e)
    {
        if (e.ept != null) return e.ept;

        if (e.faces.Count != 2)
        {
            // boundary case for edge
            var m = (e.a.p + e.b.p) * 0.5f;
            e.ept = new Vertex(m, e.a.index);
        }
        else
        {
            const float alpha = 3f / 8f;
            const float beta = 1f / 8f;
            var left = e.faces[0].GetOtherVertex(e);
            var right = e.faces[1].GetOtherVertex(e);
            e.ept = new Vertex((e.a.p + e.b.p) * alpha + (left.p + right.p) * beta, e.a.index);
        }

        return e.ept;
    }

    public Vertex[] GetAdjancies(Vertex v)
    {
        var adjancies = new Vertex[v.edges.Count];
        for (int i = 0, n = v.edges.Count; i < n; i++)
        {
            adjancies[i] = v.edges[i].GetOtherVertex(v);
        }
        return adjancies;
    }

    public Vertex GetVertexPoint(Vertex v)
    {
        if (v.updated != null) return v.updated;

        var adjancies = GetAdjancies(v);
        var n = adjancies.Length;
        if (n < 3)
        {
            // boundary case for vertex
            var e0 = v.edges[0].GetOtherVertex(v);
            var e1 = v.edges[1].GetOtherVertex(v);
            const float k0 = (3f / 4f);
            const float k1 = (1f / 8f);
            v.updated = new Vertex(k0 * v.p + k1 * (e0.p + e1.p), v.index);
        }
        else
        {
            const float pi2 = Mathf.PI * 2f;
            const float k0 = (5f / 8f);
            const float k1 = (3f / 8f);
            const float k2 = (1f / 4f);
            var alpha = (n == 3) ? (3f / 16f) : ((1f / n) * (k0 - Mathf.Pow(k1 + k2 * Mathf.Cos(pi2 / n), 2f)));

            var np = (1f - n * alpha) * v.p;

            for (int i = 0; i < n; i++)
            {
                var adj = adjancies[i];
                np += alpha * adj.p;
            }

            v.updated = new Vertex(np, v.index);
        }

        return v.updated;
    }

}