using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace DotsLite.Structure.Authoring
{
    using DotsLite.Geometry;


    /// <summary>
    /// すべてのパーツに、マスタープレハブへのリンクをセットする。
    /// 本当は実行時に取得できればいいんだが、プレイヤーでは取得できないし、そもそもプレハブは全部展開される。
    /// </summary>
    [CustomEditor(typeof(StructureBuildingAuthoring))]
    public class StructureModelEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            //元のInspector部分を表示
            base.OnInspectorGUI();

            GUILayout.BeginHorizontal();

            //ボタンを表示
            if (GUILayout.Button("set master prefab link per part"))
            {
                var parts = this.targets
                    .OfType<StructureBuildingAuthoring>()
                    .SelectMany(st => st.GetComponentsInChildren<StructureBuildingPartAuthoring>())
                    .Select(pt => (pt, PrefabUtility.GetCorrespondingObjectFromOriginalSource(pt.gameObject)));
                foreach ( var (pt, masterPrefab) in parts )
                {
                    (pt.PartModelOfMasterPrefab as PartModel<UI32, PositionNormalUvVertex>)
                        .SetObject(masterPrefab ?? pt.gameObject);
                    Debug.Log($"{pt.name} <- {pt.PartModelOfMasterPrefab.Obj.name}");

                    EditorUtility.SetDirty(pt);
                }

                var structures = this.targets
                    .OfType<StructureBuildingAuthoring>()
                    .Select(st => (st, PrefabUtility.GetCorrespondingObjectFromOriginalSource(st.gameObject)))
                    .Select(x => (x.st, x.Item2?.GetComponent<StructureBuildingAuthoring>()));
                foreach (var (st, masterPrefab) in structures)
                {
                    st.MasterPrefab = masterPrefab ?? st;
                    Debug.Log($"{st.name} <- {st.MasterPrefab.name}");

                    EditorUtility.SetDirty(st);
                }

                AssetDatabase.SaveAssets();
            }


            if (GUILayout.Button("disribute part id"))
            {
                var parts = this.targets
                    .OfType<MonoBehaviour>()
                    .SelectMany(s => s.GetComponentsInChildren<StructureBuildingPartAuthoring>())
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
