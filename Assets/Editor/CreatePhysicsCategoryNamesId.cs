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
using Unity.Entities.UniversalDelegates;

public class CreatePhysicsCategoryNamesConstants : MonoBehaviour
{

    [MenuItem("Assets/Create Physics Category Names Constats")]
    static public void Create()
    {
        if (Selection.objects == null) return;

        makeConstListFile_(Selection.objects);

        return;


        void makeConstListFile_(UnityEngine.Object[] selectedObjects)
        {

            var catnameIdLists =
                from asset in selectedObjects.OfType<PhysicsCategoryNames>()
                select new
                {
                    assetname = asset.name,
                    categories =
                        from x in asset.CategoryNames.Select((cat, i) => (cat, i))
                        where x.cat != ""
                        select x
                };
            var catnameFlagLists =
                from asset in catnameIdLists
                select new
                {
                    asset.assetname,
                    categories = asset.categories.Select(x => (x.cat, i:1<<x.i))
                };

            foreach (var x in catnameIdLists)
            {
                ConstCreator.Create($"{x.assetname}Id", x.categories, $"Assets/{x.assetname}Id.cs");
            }
            foreach (var x in catnameFlagLists)
            {
                ConstCreator.Create($"{x.assetname}Flag", x.categories, $"Assets/{x.assetname}Flag.cs");
            }
        }
    }

}
