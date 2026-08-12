using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

public static class STLParser
{
    public static Mesh LoadBinarySTL(string filePath)
    {
        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        using (BinaryReader br = new BinaryReader(fs))
        {
            // The first 80 bytes are just a text header (often containing software info). We skip it.
            br.ReadBytes(80);

            // The next 4 bytes represent a 32-bit unsigned integer stating the total number of triangles.
            uint triangleCount = br.ReadUInt32();

            // Initialize our arrays. Every triangle has 3 vertices.
            Vector3[] vertices = new Vector3[triangleCount * 3];
            int[] triangles = new int[triangleCount * 3];

            // Loop through the file, reading 50 bytes per triangle
            for (int i = 0; i < triangleCount; i++)
            {
                // Read Face Normal
                float nx = br.ReadSingle();
                float ny = br.ReadSingle();
                float nz = br.ReadSingle();

                // Read Vertexes
                float v1x = br.ReadSingle();
                float v1y = br.ReadSingle();
                float v1z = br.ReadSingle();

                float v2x = br.ReadSingle();
                float v2y = br.ReadSingle();
                float v2z = br.ReadSingle();

                float v3x = br.ReadSingle();
                float v3y = br.ReadSingle();
                float v3z = br.ReadSingle();

                // Read Attribute Byte Count (2 bytes - usually zero, we skip it)
                br.ReadUInt16();

                // Determine the array indices for this triangle
                int vIndex1 = i * 3;
                int vIndex2 = i * 3 + 1;
                int vIndex3 = i * 3 + 2;

                // Assign vertices while swapping Y and Z axes.
                vertices[vIndex1] = new Vector3(v1x, v1z, v1y);
                vertices[vIndex2] = new Vector3(v3x, v3z, v3y);
                vertices[vIndex3] = new Vector3(v2x, v2z, v2y);

                // Map the indices sequentially to form the triangle
                triangles[vIndex1] = vIndex1;
                triangles[vIndex2] = vIndex2;
                triangles[vIndex3] = vIndex3;
            }

            Mesh mesh = new Mesh();

            // Force 32-bit index buffer so large files don't crash
            mesh.indexFormat = IndexFormat.UInt32;

            mesh.vertices = vertices;
            mesh.triangles = triangles;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }
    }
}