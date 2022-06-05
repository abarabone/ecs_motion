using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Linq;

namespace DotsLite.LoadPath.Authoring
{
    using DotsLite.Geometry;
    using DotsLite.Structure.Authoring;
    using DotsLite.Model.Authoring;

    /// <summary>
    /// 
    /// </summary>
    [CustomEditor(typeof(StructureAreaAuthoring))]
    public class FitPathMesh : Editor// DistributeModelOriginId
    {
        public override void OnInspectorGUI()
        {
            //元のInspector部分を表示
            base.OnInspectorGUI();

            GUILayout.BeginHorizontal();

            //ボタンを表示
            if (GUILayout.Button("build path meshes"))
            {
                var qPath =
                    from monobe in this.targets.OfType<MonoBehaviour>()
                    from path in monobe.gameObject.GetComponentsInChildren<PathAuthoring>()
                    select path
                    ;
                foreach (var path in qPath)
                {
                    path.BuildPathMeshes();

                    EditorUtility.SetDirty(path);
                }

                AssetDatabase.SaveAssets();
            }

            //ボタンを表示
            if (GUILayout.Button("fit terrain to path"))
            {
                var paths = this.targets
                    .OfType<MonoBehaviour>()
                    .SelectMany(x => x.gameObject.GetComponentsInChildren<PathAuthoring>())
                    ;
                foreach (var path in paths)
                {
                    path.BuildPathMeshes();

                    EditorUtility.SetDirty(path);
                }

                AssetDatabase.SaveAssets();
            }

            //ボタンを表示
            if (GUILayout.Button("fit path to terrain"))
            {
                var paths = this.targets
                    .OfType<MonoBehaviour>()
                    .SelectMany(x => x.gameObject.GetComponentsInChildren<PathAuthoring>())
                    ;
                foreach (var path in paths)
                {
                    path.FitPathToTerrain();

                    EditorUtility.SetDirty(path);
                }

                AssetDatabase.SaveAssets();
            }


            GUILayout.EndHorizontal();
        }
    }
}
