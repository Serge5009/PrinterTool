using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Xml;
using UnityEngine;
using UnityEngine.Rendering;

public static class ThreeMFParser
{
    public static Mesh[] Load3MF(string filePath)
    {
        List<Mesh> parsedMeshes = new List<Mesh>();

        using (FileStream zipToOpen = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
        {
            bool foundAnyModel = false;

            foreach (var entry in archive.Entries)
            {
                if (!entry.FullName.EndsWith(".model", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (entry.FullName.IndexOf("Metadata", StringComparison.OrdinalIgnoreCase) >= 0)
                    continue;

                foundAnyModel = true;

                using (Stream xmlStream = entry.Open())
                using (XmlReader reader = XmlReader.Create(xmlStream))
                {
                    List<Vector3> currentVertices = null;
                    List<int> currentTriangles = null;

                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            string elementName = reader.LocalName.ToLowerInvariant();

                            if (elementName == "object")
                            {
                                currentVertices = new List<Vector3>();
                                currentTriangles = new List<int>();
                            }
                            else if (elementName == "vertex" && currentVertices != null)
                            {
                                float x = ParseFloat(reader.GetAttribute("x") ?? reader.GetAttribute("X"));
                                float y = ParseFloat(reader.GetAttribute("y") ?? reader.GetAttribute("Y"));
                                float z = ParseFloat(reader.GetAttribute("z") ?? reader.GetAttribute("Z"));

                                currentVertices.Add(new Vector3(x, z, y));
                            }
                            else if (elementName == "triangle" && currentTriangles != null)
                            {
                                int v1 = int.Parse(reader.GetAttribute("v1") ?? reader.GetAttribute("V1") ?? "0");
                                int v2 = int.Parse(reader.GetAttribute("v2") ?? reader.GetAttribute("V2") ?? "0");
                                int v3 = int.Parse(reader.GetAttribute("v3") ?? reader.GetAttribute("V3") ?? "0");

                                currentTriangles.Add(v1);
                                currentTriangles.Add(v3);
                                currentTriangles.Add(v2);
                            }
                        }
                        else if (reader.NodeType == XmlNodeType.EndElement)
                        {
                            string elementName = reader.LocalName.ToLowerInvariant();

                            if (elementName == "object" && currentVertices != null && currentVertices.Count > 0)
                            {
                                Mesh newMesh = new Mesh();

                                newMesh.indexFormat = IndexFormat.UInt32;
                                newMesh.vertices = currentVertices.ToArray();
                                newMesh.triangles = currentTriangles.ToArray();

                                newMesh.RecalculateNormals();
                                newMesh.RecalculateBounds();

                                parsedMeshes.Add(newMesh);

                                currentVertices = null;
                                currentTriangles = null;
                            }
                        }
                    }
                }
            }

            if (!foundAnyModel)
                throw new Exception("Could not find any valid .model files inside the 3MF archive.");
        }

        return parsedMeshes.ToArray();
    }

    private static float ParseFloat(string val)
    {
        if (string.IsNullOrEmpty(val)) return 0f;
        return float.Parse(val, NumberStyles.Float, CultureInfo.InvariantCulture);
    }
}