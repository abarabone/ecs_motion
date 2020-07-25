using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Unity.Physics;
using Unity.Physics.Editor;
using Unity.Physics.Systems;
using Unity.Physics.Authoring;

public class CreateDotsCollisionCategoryToEnum : MonoBehaviour
{

    [MenuItem("Assets/Create Dots Collision Filter To Enum")]
    static public void Create()
    {
        if (Selection.objects == null) return;

        makeConstListFile_(Selection.objects);

        return;


        void makeConstListFile_(UnityEngine.Object[] selectedObjects)
        {
            var catnameLists =
                from asset in selectedObjects.OfType<PhysicsCategoryNames>()
                select new
                {
                    assetname = asset.name,
                    categories =
                        from x in asset.CategoryNames.Select((cat, i) => (cat, i))
                        where x.cat != ""
                        select x
                };

            foreach (var x in catnameLists)
            {
                ConstCreator.Create($"{x.assetname}Indices", x.categories, $"Assets/{x.assetname}_indices.cs");
            }
        }
    }

}
