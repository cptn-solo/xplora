using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.World.HexMap
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class HexMesh : MonoBehaviour
    {

        private Mesh hexMesh;
        [NonSerialized] private List<Vector3> vertices, cellIndices;
        [NonSerialized] List<Color> cellWeights;
        [NonSerialized] private List<int> triangles;
        [NonSerialized] private List<Color> colors;

        private MeshCollider meshCollider;
        private readonly bool useCellData = true;

        static Color weights1 = new Color(1f, 0f, 0f);
        static Color weights2 = new Color(0f, 1f, 0f);
        static Color weights3 = new Color(0f, 0f, 1f);


        void Awake()
        {
            GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
            meshCollider = gameObject.AddComponent<MeshCollider>();
            hexMesh.name = "Hex Mesh";
            vertices = new List<Vector3>();
            colors = new List<Color>();
            triangles = new List<int>();
        }

        /// <summary>
        /// Clear all data.
        /// </summary>
        public void Clear()
        {
            hexMesh.Clear();
            vertices = ListPool<Vector3>.Get();
            if (useCellData)
            {
                cellWeights = ListPool<Color>.Get();
                cellIndices = ListPool<Vector3>.Get();
            }
            triangles = ListPool<int>.Get();
        }

        /// <summary>
        /// Apply all triangulation data to the underlying mesh.
        /// </summary>
        public void Apply()
        {
            hexMesh.SetVertices(vertices);
            ListPool<Vector3>.Add(vertices);
            if (useCellData)
            {
                hexMesh.SetColors(cellWeights);
                ListPool<Color>.Add(cellWeights);
                hexMesh.SetUVs(2, cellIndices);
                ListPool<Vector3>.Add(cellIndices);
            }
            hexMesh.SetTriangles(triangles, 0);
            ListPool<int>.Add(triangles);

            hexMesh.RecalculateNormals();

            meshCollider.sharedMesh = hexMesh;
        }

        public void Triangulate(HexCell[] cells)
        {
            Clear();
            for (int i = 0; i < cells.Length; i++)
            {
                Triangulate(cells[i]);
            }
            Apply();
        }

        private void Triangulate(HexCell cell)
        {
            Vector3 center = cell.transform.localPosition;
            for (int i = 0; i < 6; i++)
            {
                AddTriangle(
                    center,
                    center + HexMetrics.corners[i],
                    center + HexMetrics.corners[i + 1]
                );

                Vector3 indices;
                indices.x = indices.y = indices.z = cell.Index;
                AddTriangleCellData(indices, weights1);
            }
        }

        private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            int vertexIndex = vertices.Count;
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
        }

        /// <summary>
        /// Add cell data for a triangle.
        /// </summary>
        /// <param name="indices">Terrain type indices.</param>
        /// <param name="weights1">First terrain weights.</param>
        /// <param name="weights2">Second terrain weights.</param>
        /// <param name="weights3">Third terrain weights.</param>
        public void AddTriangleCellData(
            Vector3 indices, Color weights1, Color weights2, Color weights3
        )
        {
            cellIndices.Add(indices);
            cellIndices.Add(indices);
            cellIndices.Add(indices);
            cellWeights.Add(weights1);
            cellWeights.Add(weights2);
            cellWeights.Add(weights3);
        }

        /// <summary>
        /// Add cell data for a triangle.
        /// </summary>
        /// <param name="indices">Terrain type indices.</param>
        /// <param name="weights">Terrain weights, uniform for entire triangle.</param>
        public void AddTriangleCellData(Vector3 indices, Color weights) =>
            AddTriangleCellData(indices, weights, weights, weights);
    }
}