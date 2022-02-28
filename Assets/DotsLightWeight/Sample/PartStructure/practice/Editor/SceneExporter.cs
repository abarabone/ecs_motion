using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

/**
 * Scene Exporter
 * 
 * @author			Michael Grenier
 * @author_url		http://mgrenier.me
 * @copyright		2011 (c) Michael Grenier
 * @license			MIT - http://www.opensource.org/licenses/MIT
 * 
 */

class SceneExporter : EditorWindow
{
	[MenuItem("Tools/Export scene...")]
	static public void ExportScene()
	{
		List<GameObject> root = new List<GameObject>();
		foreach (GameObject go in (GameObject[])UnityEngine.Object.FindObjectsOfType(typeof(GameObject)))
			if (go.transform.parent == null)
				root.Add(go);

		ExportGameObject(
			EditorUtility.SaveFilePanel("Export", "", System.IO.Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + ".dae", "dae"),
			root.ToArray()
		);
	}

	[MenuItem("Tools/Export selected...")]
	static public void ExportSelected()
	{
		List<GameObject> selected = new List<GameObject>();

		foreach (UnityEngine.Object go in Selection.objects)
			if (go is GameObject)
				selected.Add((GameObject)go);

		List<GameObject> root = new List<GameObject>();

		for (int i = selected.Count - 1; i >= 0; --i)
		{
			GameObject go = selected[i];

			bool inroot = false;

			Transform parent = go.transform.parent;
			for (; parent != null; parent = parent.parent)
			{
				if (root.Contains(parent.gameObject))
				{
					inroot = true;
					break;
				}
			}

			if (!inroot)
				root.Add(go);
		}

		ExportGameObject(
			EditorUtility.SaveFilePanel("Export", "", System.IO.Path.GetFileNameWithoutExtension(EditorApplication.currentScene) + ".dae", "dae"),
			root.ToArray()
		);
	}


	static public void ExportGameObject(string path, GameObject[] gos)
	{
		Utils.ColladaExporter exporter;
		try
		{
			exporter = new Utils.ColladaExporter(path);
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message);
			return;
		}

		Matrix4x4 root = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1));
		int n = 0;
		for (int a = 0, b = gos.Length; a < b; ++a)
			RecursiveExportGameObject(exporter, gos[a], root, null, ref n);

		exporter.Save();
	}

	static private void RecursiveExportGameObject(Utils.ColladaExporter exporter, GameObject go, Matrix4x4 root, XmlNode node, ref int n)
	{
		Debug.Log("Exporting " + go.name + "...");

		Matrix4x4 local = Matrix4x4.TRS(
			new Vector3(-go.transform.localPosition.x, go.transform.localPosition.y, go.transform.localPosition.z),
			new Quaternion(go.transform.localRotation.x, -go.transform.localRotation.y, go.transform.localRotation.z, go.transform.localRotation.w),
			go.transform.localScale
		);
		local *= root;
		MeshFilter filter = go.GetComponent<MeshFilter>();

		XmlNode parent = node;

		if (filter != null)
		{
			n++;
			exporter.AddGeometry("geometry_" + n, filter.sharedMesh);
			parent = exporter.AddGeometryToScene("geometry_" + n, go.name, local, node);
		}
		else
		{
			n++;
			parent = exporter.AddEmptyToScene("empty_" + n, go.name, local, node);
		}

		foreach (Transform child in go.transform)
			RecursiveExportGameObject(exporter, child.gameObject, root, parent, ref n);

		//Debug.Log("Exported " + go.name);
	}

}