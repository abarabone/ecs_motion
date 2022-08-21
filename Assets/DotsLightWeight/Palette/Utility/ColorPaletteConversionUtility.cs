//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Runtime.CompilerServices;
//using System.Collections.Generic;
//using System.Runtime.InteropServices;
//using UnityEngine;
//using Unity.Entities;
//using UnityEngine.Rendering;
//using Unity.Collections;
//using Unity.Linq;
//using Unity.Mathematics;
//using Unity.Collections.LowLevel.Unsafe;

//namespace DotsLite.Geometry.Palette
//{
//    using DotsLite.Common.Extension;
//    using DotsLite.Utilities;
//    using DotsLite.Geometry.inner;
//    using DotsLite.Geometry.inner.unit;
//    using DotsLite.Structure.Authoring;
//    using DotsLite.Utility;
//    using DotsLite.Model.Authoring;
//    using DotsLite.Draw;
//    using DotsLite.Draw.Authoring.Palette;

//    //public class ColorPaletteBuilder
//    //{

//    //    List<Color32> colors = new List<Color32>();


//    //    public int AddAndGetId(Color32[] values)
//    //    {
//    //        var id = this.colors.Count;

//    //        this.colors.AddRange(values);

//    //        return id;
//    //    }

//    //    public unsafe GraphicsBuffer BuildShaderBuffer()
//    //    {
//    //        var buf = new GraphicsBuffer(GraphicsBuffer.Target.Structured, this.colors.Count, sizeof(Color32));

//    //        buf.SetData(this.colors);

//    //        return buf;
//    //    }
//    //}


//    /// <summary>
//    /// モデルのＵＶパレット情報を構築する
//    /// </summary>
//    public static class UvPaletteMeshUtility
//    {

//        // tex rect は atlas 単位
//        // atlas 単位で buffer 化
//        // buffer には uv surface しか登録しない
//        // uv surfaces の sub mesh ごとに sub index
//        // 頂点に uv sub index を持たせる
//        // 同じ uv sub index は同じ surface 
//        // uv は同じ面構成単位で base index 指定　色違いでも同じ物体ならＯＫ
//        // エディタでは、サブメッシュ単位でテクスチャを指定する scriptable object を持たせる
//        // sub index は palette と同じマテリアルの palette sub index でよい
//        // palette はカラーセット単位で base index 指定

//        /// <summary>
//        /// 
//        /// </summary>
//        public static void CalculateUvSubIndexParameter(
//            this IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts,
//            ref AdditionalParameters p,
//            IEnumerable<int> texhashesForUvSurface)
//        {
//            var q =
//                from mmt in mmts
//                select
//                    from mat in mmt.mats
//                    select mat?.mainTexture?.GetHashCode() ?? 0
//                ;
//            var texhashPerSubMesh = q.ToArrayRecursive2();

//            var usedTexhashes = texhashPerSubMesh
//                .SelectMany(x => x)
//                .Distinct()
//                .ToArray();

//            p.texhashPerSubMesh = texhashPerSubMesh;

//        }
//    }

//}
