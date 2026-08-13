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
    private class ThreeMFObject
    {
        public string id;
        public bool isPrintable = true;
        public List<Vector3> vertices = new List<Vector3>();
        public List<int> triangles = new List<int>();
        public List<ThreeMFComponent> components = new List<ThreeMFComponent>();
    }

    private class ThreeMFComponent
    {
        public string objectId;
        public Matrix4x4 transform = Matrix4x4.identity;
    }

    public static Mesh[] Load3MF(string filePath)
    {
        List<Mesh> finalMeshes = new List<Mesh>();

        float globalScale = 1f;
        Dictionary<string, ThreeMFObject> allObjects = new Dictionary<string, ThreeMFObject>();
        List<ThreeMFComponent> allBuildItems = new List<ThreeMFComponent>();

        using (FileStream zipToOpen = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
        {
            foreach (var entry in archive.Entries)
            {
                if (!entry.FullName.EndsWith(".model", StringComparison.OrdinalIgnoreCase)) continue;
                if (entry.FullName.IndexOf("Metadata", StringComparison.OrdinalIgnoreCase) >= 0) continue;

                using (Stream xmlStream = entry.Open())
                using (XmlReader reader = XmlReader.Create(xmlStream))
                {
                    ThreeMFObject currentObject = null;

                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            string elementName = reader.LocalName.ToLowerInvariant();
                            bool isEmptyElement = reader.IsEmptyElement;

                            if (elementName == "model")
                            {
                                string unit = GetAttributeIgnoreCase(reader, "unit");
                                if (!string.IsNullOrEmpty(unit))
                                {
                                    unit = unit.ToLowerInvariant();
                                    if (unit == "inch") globalScale = 25.4f;
                                    else if (unit == "centimeter") globalScale = 10f;
                                    else if (unit == "meter") globalScale = 1000f;
                                    else if (unit == "foot") globalScale = 304.8f;
                                    else if (unit == "micron") globalScale = 0.001f;
                                }
                            }
                            else if (elementName == "object")
                            {
                                currentObject = new ThreeMFObject();
                                currentObject.id = GetAttributeIgnoreCase(reader, "id");

                                if (reader.HasAttributes)
                                {
                                    for (int i = 0; i < reader.AttributeCount; i++)
                                    {
                                        reader.MoveToAttribute(i);
                                        string aName = reader.Name.ToLowerInvariant();
                                        string aVal = reader.Value.ToLowerInvariant();

                                        if (aName.Contains("type") && (aVal.Contains("other") || aVal.Contains("modifier") || aVal.Contains("support") || aVal.Contains("negative"))) currentObject.isPrintable = false;
                                        if (aVal.Contains("modifier") || aVal.Contains("negative")) currentObject.isPrintable = false;
                                        if ((aName.Contains("text") || aName.Contains("is_modifier") || aName.Contains("is_negative_volume")) && aVal == "1") currentObject.isPrintable = false;
                                    }
                                    reader.MoveToElement();
                                }

                                if (!string.IsNullOrEmpty(currentObject.id))
                                {
                                    string filePathKey = $"/{entry.FullName.Replace("\\", "/")}#{currentObject.id}";

                                    allObjects[currentObject.id] = currentObject;
                                    allObjects[filePathKey.ToLowerInvariant()] = currentObject;
                                }

                                if (isEmptyElement) currentObject = null;
                            }
                            else if (elementName == "metadata" && currentObject != null)
                            {
                                string metaName = (GetAttributeIgnoreCase(reader, "name") ?? "").ToLowerInvariant();
                                string metaVal = (GetAttributeIgnoreCase(reader, "value") ?? "").ToLowerInvariant();

                                if (metaVal.Contains("modifier") || metaVal.Contains("negative_volume") || metaName.Contains("modifier"))
                                {
                                    currentObject.isPrintable = false;
                                }
                            }
                            else if (elementName == "vertex" && currentObject != null)
                            {
                                float x = ParseFloat(GetAttributeIgnoreCase(reader, "x"));
                                float y = ParseFloat(GetAttributeIgnoreCase(reader, "y"));
                                float z = ParseFloat(GetAttributeIgnoreCase(reader, "z"));
                                currentObject.vertices.Add(new Vector3(x, y, z));
                            }
                            else if (elementName == "triangle" && currentObject != null)
                            {
                                int v1 = int.Parse(GetAttributeIgnoreCase(reader, "v1") ?? "0");
                                int v2 = int.Parse(GetAttributeIgnoreCase(reader, "v2") ?? "0");
                                int v3 = int.Parse(GetAttributeIgnoreCase(reader, "v3") ?? "0");
                                currentObject.triangles.Add(v1);
                                currentObject.triangles.Add(v2);
                                currentObject.triangles.Add(v3);
                            }
                            else if (elementName == "component" && currentObject != null)
                            {
                                ThreeMFComponent comp = new ThreeMFComponent();
                                comp.objectId = GetAttributeIgnoreCase(reader, "objectid");
                                comp.transform = ParseTransform(GetAttributeIgnoreCase(reader, "transform"));
                                currentObject.components.Add(comp);
                            }
                            else if (elementName == "item")
                            {
                                ThreeMFComponent item = new ThreeMFComponent();
                                item.objectId = GetAttributeIgnoreCase(reader, "objectid");
                                item.transform = ParseTransform(GetAttributeIgnoreCase(reader, "transform"));
                                allBuildItems.Add(item);
                            }
                        }
                        else if (reader.NodeType == XmlNodeType.EndElement)
                        {
                            if (reader.LocalName.ToLowerInvariant() == "object")
                            {
                                currentObject = null;
                            }
                        }
                    }
                }
            }
        }

        Matrix4x4 globalScaleMatrix = Matrix4x4.Scale(new Vector3(globalScale, globalScale, globalScale));

        if (allBuildItems.Count > 0)
        {
            foreach (var item in allBuildItems)
            {
                Matrix4x4 rootTransform = globalScaleMatrix * item.transform;
                GenerateMeshesRecursive(item.objectId, rootTransform, allObjects, finalMeshes);
            }
        }
        else
        {
            HashSet<string> childIds = new HashSet<string>();
            foreach (var obj in allObjects.Values)
            {
                foreach (var comp in obj.components)
                {
                    childIds.Add(comp.objectId.ToLowerInvariant());
                    childIds.Add(comp.objectId);
                }
            }

            HashSet<ThreeMFObject> rendered = new HashSet<ThreeMFObject>();
            foreach (var obj in allObjects.Values)
            {
                if (!childIds.Contains(obj.id) && !childIds.Contains(obj.id.ToLowerInvariant()) && !rendered.Contains(obj))
                {
                    GenerateMeshesRecursive(obj.id, globalScaleMatrix, allObjects, finalMeshes);
                    rendered.Add(obj);
                }
            }
        }

        return finalMeshes.ToArray();
    }

    private static void GenerateMeshesRecursive(string objectId, Matrix4x4 currentTransform, Dictionary<string, ThreeMFObject> objects, List<Mesh> finalMeshes)
    {
        ThreeMFObject obj = null;

        if (!objects.TryGetValue(objectId, out obj) && !objects.TryGetValue(objectId.ToLowerInvariant(), out obj))
        {
            if (objectId.Contains("#"))
            {
                string[] parts = objectId.Split('#');
                objects.TryGetValue(parts[parts.Length - 1], out obj);
            }
        }

        if (obj == null || !obj.isPrintable) return;

        if (obj.vertices.Count > 0 && obj.triangles.Count > 0)
        {
            Mesh newMesh = new Mesh();
            newMesh.indexFormat = IndexFormat.UInt32;

            Vector3[] transformedVerts = new Vector3[obj.vertices.Count];
            for (int i = 0; i < obj.vertices.Count; i++)
            {
                Vector3 nativeVert = currentTransform.MultiplyPoint3x4(obj.vertices[i]);

                transformedVerts[i] = new Vector3(nativeVert.x, nativeVert.z, nativeVert.y);
            }

            int[] convertedTriangles = new int[obj.triangles.Count];
            for (int i = 0; i < obj.triangles.Count; i += 3)
            {
                convertedTriangles[i] = obj.triangles[i];
                convertedTriangles[i + 1] = obj.triangles[i + 2];
                convertedTriangles[i + 2] = obj.triangles[i + 1];
            }

            newMesh.vertices = transformedVerts;
            newMesh.triangles = convertedTriangles;
            newMesh.RecalculateNormals();
            newMesh.RecalculateBounds();
            finalMeshes.Add(newMesh);
        }

        foreach (var comp in obj.components)
        {
            Matrix4x4 childTransform = currentTransform * comp.transform;
            GenerateMeshesRecursive(comp.objectId, childTransform, objects, finalMeshes);
        }
    }

    private static Matrix4x4 ParseTransform(string transformStr)
    {
        if (string.IsNullOrEmpty(transformStr)) return Matrix4x4.identity;

        string[] parts = transformStr.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 12) return Matrix4x4.identity;

        float[] t = new float[12];
        for (int i = 0; i < 12; i++) t[i] = ParseFloat(parts[i]);

        Matrix4x4 m = new Matrix4x4();
        m.m00 = t[0]; m.m01 = t[3]; m.m02 = t[6]; m.m03 = t[9];
        m.m10 = t[1]; m.m11 = t[4]; m.m12 = t[7]; m.m13 = t[10];
        m.m20 = t[2]; m.m21 = t[5]; m.m22 = t[8]; m.m23 = t[11];
        m.m30 = 0; m.m31 = 0; m.m32 = 0; m.m33 = 1;

        return m;
    }

    private static float ParseFloat(string val)
    {
        if (string.IsNullOrEmpty(val)) return 0f;
        val = val.Replace(',', '.');
        if (float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
            return result;
        return 0f;
    }

    private static string GetAttributeIgnoreCase(XmlReader reader, string attributeName)
    {
        string val = reader.GetAttribute(attributeName);
        if (val != null) return val;

        val = reader.GetAttribute(attributeName.ToLowerInvariant());
        if (val != null) return val;

        if (reader.HasAttributes)
        {
            if (reader.MoveToFirstAttribute())
            {
                do
                {
                    if (reader.Name.Equals(attributeName, StringComparison.OrdinalIgnoreCase) ||
                        reader.LocalName.Equals(attributeName, StringComparison.OrdinalIgnoreCase))
                    {
                        val = reader.Value;
                        break;
                    }
                } while (reader.MoveToNextAttribute());
            }
            reader.MoveToElement();
        }
        return val;
    }
}