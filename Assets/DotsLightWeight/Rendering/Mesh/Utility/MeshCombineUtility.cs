using System;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Linq;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace DotsLite.Geometry
{
    using DotsLite.Common.Extension;
    using DotsLite.Utilities;
    using DotsLite.Geometry.inner;
    using DotsLite.Geometry.inner.unit;
    using DotsLite.Structure.Authoring;


    public class AdditionalParameters
    {
        public Matrix4x4 mtBaseInv;
        public Matrix4x4[] mtPerMesh;

        public Matrix4x4[][] mtInvsPerMesh;//
        public BoneWeight[][] boneWeightsPerMesh;//
        public BoneConversionUtility.BoneConversionDictionary srcBoneIndexToDstBoneIndex;//

        public int[][] texhashPerSubMesh;
        public Func<int, Rect> texHashToUvRect;
        public Func<int, int> texHashToUvIndex;

        public int[] partIdPerMesh;
        public int[][] paletteSubIndexPerSubMesh;    // mesh > submesh . subindex
        public int[][] UvIndexPerSubMesh;           // mesh > submesh . uvindex
    }


    public static partial class MeshCombineUtility
    {

        /// <summary>
        /// 
        /// </summary>
        public static void calculateParameters(
            this AdditionalParameters parameters,
            (Mesh mesh, Material[] mats, Transform tf)[] mmts, Transform tfBase, Func<int, Rect> texHashToUvRectFunc)
        {
            var qMtPerMesh = mmts
                .Select(x => x.tf.localToWorldMatrix);
            var qTexhashPerSubMesh =
                from mmt in mmts
                select
                    from mat in mmt.mats
                    select mat?.mainTexture?.GetHashCode() ?? 0
                ;

            var mtBaseInv = tfBase.worldToLocalMatrix;

            parameters.mtBaseInv = mtBaseInv;
            parameters.mtPerMesh = qMtPerMesh.ToArray();
            parameters.texhashPerSubMesh = qTexhashPerSubMesh.ToArrayRecursive2();
            parameters.texHashToUvRect = texHashToUvRectFunc;
        }


        public static void calculateUvParameters(this AdditionalParameters parameters, Func<int, int> texHashToUvIndexFunc)
        {
            parameters.texHashToUvIndex = texHashToUvIndexFunc;// マテリアルのテクスチャから、uv index を取得するためのラムダ
        }





        //static T findInParent<T>(this GameObject obj)
        //    where T : MonoBehaviour
        //=> obj
        //    .AncestorsAndSelf()
        //    .Select(x => x.GetComponent<T>())
        //    .FirstOrDefault()
        //    ;

        //static T findInParent<T>(this Transform tf) where T : MonoBehaviour => tf.gameObject.findInParent<T>();

        static T getInParent<T>(this GameObject obj)
            where T : IStructurePart
        //=> obj.GetComponentsInParent<T>(true).FirstOrDefault();
        => obj.GetComponentInParent<T>(true);

        static T getInParent<T>(this Transform tf)
            where T : IStructurePart
        => tf.gameObject.getInParent<T>();
    }
}