using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace DotsLite.Model.Authoring
{
    using DotsLite.Geometry;


    [CustomEditor(typeof(ModelGroupAuthoring.ModelAuthoringBase), true)]
    public class DistributeModelOriginId : Editor
    {
        public override void OnInspectorGUI()
        {
            //GUILayout.BeginHorizontal();


            //ボタンを表示
            if (GUILayout.Button("generate or get model origin id"))
            {
                var xs =
                    from model in this.targets
                        .OfType<ModelGroupAuthoring.ModelAuthoringBase>()
                    let prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(model)
                    select (model, prefab);

                foreach (var (model, prefab) in xs)
                {
                    if (prefab == null)
                    {
                        model.QueryModel.ForEach(m => m.GenerateSourcePrefabKey());
                        model.QueryModel.ForEach(m => Debug.Log($"prefab key generated : {model.name} {m.SourcePrefabKey}"));
                    }
                    EditorUtility.SetDirty(model);
                }

                AssetDatabase.SaveAssets();
            }

            //GUILayout.EndHorizontal();

            //元のInspector部分を表示
            base.OnInspectorGUI();
        }
    }
}
