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
    [CustomEditor(typeof(StructureAreaAuthoring))]
    public class StructureAreaEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            //元のInspector部分を表示
            base.OnInspectorGUI();

            GUILayout.BeginHorizontal();

            //ボタンを表示
            if (GUILayout.Button("set master prefab link per models"))
            {
                var parts = this.targets
                    .OfType<MonoBehaviour>()
                    .SelectMany(x => x.GetComponentsInChildren<StructureModelAuthoring>())
                    .Select(st => (st, PrefabUtility.GetCorrespondingObjectFromOriginalSource(st.gameObject)));
                    ;
                
                foreach ( var (st, masterPrefab) in parts )
                {
                    st.MasterPrefab = masterPrefab ?? st.gameObject;
                    Debug.Log($"{st.name} <- {st.MasterPrefab.name}");

                    EditorUtility.SetDirty(st);
                }

                AssetDatabase.SaveAssets();
            }

            GUILayout.EndHorizontal();
        }
    }
}
