using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abarabone.Structure.Authoring
{
    /// <summary>
    /// すべてのパーツに、マスタープレハブへのリンクをセットする。
    /// 本当は実行時に取得できればいいんだが、プレイヤーでは取得できないし、そもそもプレハブは全部展開される。
    /// </summary>
    [CustomEditor(typeof(StructureModelAuthoring))]
    public class ExampleScriptEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            //元のInspector部分を表示
            base.OnInspectorGUI();

            GUILayout.BeginHorizontal();

            //ボタンを表示
            if (GUILayout.Button("set master prefab link on part"))
            {
                var parts = this.targets
                    .OfType<MonoBehaviour>()
                    .SelectMany(s => s.GetComponentsInChildren<StructurePartAuthoring>())
                    .Select(pt => (pt, PrefabUtility.GetCorrespondingObjectFromOriginalSource(pt.gameObject)));
                    ;
                
                foreach ( var (pt, master) in parts )
                {
                    pt.MasterPrefab = master ?? pt.gameObject;
                    Debug.Log($"{pt.name} <- {pt.MasterPrefab.name}");

                    EditorUtility.SetDirty(pt);
                }

                AssetDatabase.SaveAssets();
            }


            if (GUILayout.Button("disribute part id"))
            {
                var parts = this.targets
                    .OfType<MonoBehaviour>()
                    .SelectMany(s => s.GetComponentsInChildren<StructurePartAuthoring>())
                ;

                foreach (var x in parts.Select( (pt, i) => (pt, i) ))
                {
                    x.pt.PartId = x.i;

                    EditorUtility.SetDirty(x.pt);
                }

                AssetDatabase.SaveAssets();
            }

            GUILayout.EndHorizontal();
        }
    }
}
