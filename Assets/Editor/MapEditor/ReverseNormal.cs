using EditorHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Generator
{
	public class ReverseNormal : EditorWindow
	{
		private GameObject SrcObj;
		public void OnGUI()
		{
			SrcObj = EditorDataFields.EditorDataField("引用Obj", SrcObj);
			if (GUILayout.Button("生成"))
			{
				MeshFilter meshfilter = SrcObj.GetComponent<MeshFilter>();
				Mesh mesh = meshfilter.sharedMesh;
				Mesh newMesh = new Mesh
				{
					name = $"ReverseNormal{mesh.name}",
					vertices = new Vector3[mesh.vertices.Length],
					triangles = new int[mesh.triangles.Length],
					uv = new Vector2[mesh.uv.Length],
					tangents = new Vector4[mesh.tangents.Length],
					normals = new Vector3[mesh.normals.Length],
					uv2 = new Vector2[mesh.uv2.Length],
				};
				//            Array.Copy(mesh.vertices, newMesh.vertices, mesh.vertices.Length);
				//Array.Copy(mesh.triangles, newMesh.triangles, mesh.triangles.Length);
				//Array.Copy(mesh.uv, newMesh.uv, mesh.uv.Length);
				//Array.Copy(mesh.tangents, newMesh.tangents, mesh.tangents.Length);
				//Array.Copy(mesh.uv2, newMesh.uv2, mesh.uv2.Length);

				newMesh.vertices = CloneArray(mesh.vertices);
				newMesh.triangles = CloneArray(mesh.triangles);
				newMesh.uv = CloneArray(mesh.uv);
				newMesh.tangents = CloneArray(mesh.tangents);
				newMesh.uv2 = CloneArray(mesh.uv2);

				newMesh.bounds = new Bounds(mesh.bounds.center, mesh.bounds.size);
				Vector3[] revNor = new Vector3[mesh.normals.Length];
                for (int i = 0; i < mesh.normals.Length; ++i)
                {
                    Vector3 normal = -mesh.normals[i];
					revNor[i] = normal;
                }
				newMesh.normals = revNor;
                meshfilter.sharedMesh = newMesh;
				using (FileStream file = new FileStream($"Assets/Model/ReverseNormal_{SrcObj.name}.obj", FileMode.Create))
                {
					string data = MeshToString(meshfilter, new Vector3(-1f, 1f, 1f));
					StreamWriter writer = new StreamWriter(file);
					writer.Write(data);
					writer.Close();
                }

				DestroyImmediate(newMesh);
				meshfilter.sharedMesh = mesh;
				AssetDatabase.Refresh();
			}
		}

		private static T[] CloneArray<T>(T[] arr)
        {
			Array nArr = Array.CreateInstance(typeof(T), arr.Length);
            for (int i = 0; i < arr.Length; i++)
            {
				nArr.SetValue(arr[i], i);
            }
			return nArr as T[];
        }

		[MenuItem("Tools/ReverseNormal")]
		public static void Open()
		{
			GetWindow<ReverseNormal>().Show();
		}

		private static string MeshToString(MeshFilter mf, Vector3 scale)
		{
			Mesh mesh = mf.sharedMesh;
			Material[] sharedMaterials = mf.GetComponent<Renderer>().sharedMaterials;
			Vector2 textureOffset = mf.GetComponent<Renderer>().sharedMaterial.GetTextureOffset("_MainTex");
			Vector2 textureScale = mf.GetComponent<Renderer>().sharedMaterial.GetTextureScale("_MainTex");

			StringBuilder stringBuilder = new StringBuilder().Append("mtllib design.mtl")
				.Append("\n")
				.Append("g ")
				.Append(mesh.name)
				.Append("\n");

			Vector3[] vertices = mesh.vertices;
			for (int i = 0; i < vertices.Length; i++)
			{
				Vector3 vector = vertices[i];
				stringBuilder.Append(string.Format("v {0} {1} {2}\n", vector.x * scale.x, vector.y * scale.y, vector.z * scale.z));
			}

			stringBuilder.Append("\n");

			Dictionary<int, int> dictionary = new Dictionary<int, int>();

			if (mesh.subMeshCount > 1)
			{
				int[] triangles = mesh.GetTriangles(1);

				for (int j = 0; j < triangles.Length; j += 3)
				{
					if (!dictionary.ContainsKey(triangles[j]))
					{
						dictionary.Add(triangles[j], 1);
					}

					if (!dictionary.ContainsKey(triangles[j + 1]))
					{
						dictionary.Add(triangles[j + 1], 1);
					}

					if (!dictionary.ContainsKey(triangles[j + 2]))
					{
						dictionary.Add(triangles[j + 2], 1);
					}
				}
			}

			for (int num = 0; num != mesh.uv.Length; num++)
			{
				Vector2 vector2 = Vector2.Scale(mesh.uv[num], textureScale) + textureOffset;

				if (dictionary.ContainsKey(num))
				{
					stringBuilder.Append(string.Format("vt {0} {1}\n", mesh.uv[num].x, mesh.uv[num].y));
				}
				else
				{
					stringBuilder.Append(string.Format("vt {0} {1}\n", vector2.x, vector2.y));
				}
			}

			for (int k = 0; k < mesh.subMeshCount; k++)
			{
				stringBuilder.Append("\n");

				if (k == 0)
				{
					stringBuilder.Append("usemtl ").Append("Material_design").Append("\n");
				}

				if (k == 1)
				{
					stringBuilder.Append("usemtl ").Append("Material_logo").Append("\n");
				}

				int[] triangles2 = mesh.GetTriangles(k);

				for (int l = 0; l < triangles2.Length; l += 3)
				{
					stringBuilder.Append(string.Format("f {0}/{0} {1}/{1} {2}/{2}\n", triangles2[l] + 1, triangles2[l + 2] + 1, triangles2[l + 1] + 1));
				}
			}

			return stringBuilder.ToString();

		}
	}
}
