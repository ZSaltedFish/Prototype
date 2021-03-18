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
		private const string EX_NAME = "obj";
		private string _savePath;
		private string _name;
		private GameObject _srcObj;
		public void OnGUI()
		{
			_srcObj = EditorDataFields.EditorDataField("引用Obj", _srcObj);
			_name = EditorDataFields.EditorDataField("文件名", _name);
			_savePath = EditorDataFields.SaveFilePathSelected("保存路径", _savePath, EX_NAME);

			if (GUILayout.Button("生成"))
			{
				MeshFilter[] filters = _srcObj.GetComponentsInChildren<MeshFilter>();
				Mesh newMesh = new Mesh
				{
					name = _name,
					vertices = new Vector3[0],
					triangles = new int[0],
					uv = new Vector2[0],
					tangents = new Vector4[0],
					normals = new Vector3[0],
					uv2 = new Vector2[0],
				};

				foreach (MeshFilter filter in filters)
				{
					CombineMesh(filter, newMesh);
				}

				using (FileStream file = new FileStream($"{_savePath}\\{_name}.{EX_NAME}", FileMode.Create))
                {
					string data = MeshToString(newMesh, new Vector3(-1f, 1f, 1f));
					StreamWriter writer = new StreamWriter(file);
					writer.Write(data);
					writer.Close();
                }

				DestroyImmediate(newMesh);
				AssetDatabase.Refresh();
			}
		}

		private void CombineMesh(MeshFilter meshFilter, Mesh destMesh)
        {
			Mesh srcMesh = meshFilter.sharedMesh;
			int[] triangles = new int[srcMesh.triangles.Length + destMesh.triangles.Length];

			List<Vector3> vertices = new List<Vector3>();

            for (int i = 0; i < srcMesh.vertices.Length; ++i)
			{
				Vector3 subPoint = meshFilter.transform.TransformPoint(srcMesh.vertices[i]);
				//Vector3 localPoint = _srcObj.transform.TransformPoint(subPoint);
				vertices.Add(subPoint);
            }
			vertices.AddRange(destMesh.vertices);
			List<Vector2> uvs = new List<Vector2>(srcMesh.uv);
			uvs.AddRange(destMesh.uv);
			List<Vector2> uv2s = new List<Vector2>(srcMesh.uv2);
			uv2s.AddRange(destMesh.uv2);
			List<Vector4> tangents = new List<Vector4>(srcMesh.tangents);
			tangents.AddRange(destMesh.tangents);
			List<Vector3> nomrals = new List<Vector3>(srcMesh.normals);
			nomrals.AddRange(destMesh.normals);

            for (int i = 0; i < srcMesh.triangles.Length; ++i)
            {
				triangles[i] = srcMesh.triangles[i];
            }

            for (int i = 0; i < destMesh.triangles.Length; ++i)
            {
				int destTrianglesIndex = i + srcMesh.triangles.Length;
				int dvIndex = destMesh.triangles[i] + srcMesh.vertices.Length;
				triangles[destTrianglesIndex] = dvIndex;
            }

			destMesh.vertices = vertices.ToArray();
			destMesh.triangles = triangles;
			destMesh.uv = uvs.ToArray();
			destMesh.uv2 = uv2s.ToArray();
			destMesh.tangents = tangents.ToArray();
			destMesh.normals = nomrals.ToArray();
			destMesh.RecalculateBounds();
        }

		private Mesh CloneMesh(Mesh mesh)
		{
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

			newMesh.vertices = CloneArray(mesh.vertices);
			newMesh.triangles = CloneArray(mesh.triangles);
			newMesh.uv = CloneArray(mesh.uv);
			newMesh.tangents = CloneArray(mesh.tangents);
			newMesh.uv2 = CloneArray(mesh.uv2);
			newMesh.normals = CloneArray(mesh.normals);
			newMesh.bounds = new Bounds(mesh.bounds.center, mesh.bounds.size);
			return newMesh;
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

		[MenuItem("Tools/合并Mesh")]
		public static void Open()
		{
			GetWindow<ReverseNormal>().Show();
		}

		private static string MeshToString(Mesh mesh, Vector3 scale)
		{
			Vector2 textureOffset = Vector2.zero;
			Vector2 textureScale = Vector2.one;

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
