//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Unity.Entities;
//using System.Linq;


//namespace DotsLite.Geometry
//{
//    using DotsLite.Common.Extension;
    

//    public class PaletteDictionary
//    {

//        Dictionary<SourcePrefabKeyUnit, Color32[]> dict = new Dictionary<SourcePrefabKeyUnit, Color32[]>();


//        public void AddModel(IMeshModel model)
//        {
//            if (this.dict.ContainsKey(model.SourcePrefabKey)) return;

//            var qColor = model.QueryMmts
//                .SelectMany(x => x.mats)
//                .Select(x => (Color32)x.color)
//                .Distinct()
//                ;
//            this.dict.Add(model.SourcePrefabKey, qColor.ToArray());
//        }

//        public (SourcePrefabKeyUnit key, int ibase, int[] isub)[] BuildPaletteIndexList()
//        {
//            var qBase = this.dict
//                .Select(x => x.Value)
//                .Scan(1, (pre, cur) => pre + cur.Length);
//            var qKeyAndIndex =
//                from obj in (this.dict, qBase).Zip()
//                select (key:obj.src0.Key, ibase:obj.src1, subs:Enumerable.Range(0, obj.src0.Value.Length).ToArray())
//                ;
//            return qKeyAndIndex.ToArray();
//        }

//        //public void BuildPaletteShaderBuffer()
//        //{
//        //    GraphicsBuffer.

//        //    Color32[] buildPalettes_()
//        //    {
//        //        var q = this.dict
//        //            .SelectMany(x => x.Value)
//        //            .Prepend((Color32)Color.white)
//        //            ;
                
//        //    }
//        //}
//    }

//}
