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


    public static class MeshCombineUtility
    {

        /// <summary>
        /// 別の関数で
        /// 必要なパラメータだけ計算するようにしたほうがいいかも
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

        public static void calculateBoneParameters(
            this AdditionalParameters parameters, (Mesh mesh, Material[] mats, Transform tf)[] mmts, Transform[] tfBones)
        {
            var qMeshes = mmts
                .Select(x => x.mesh);

            var qBoneWeights =
                from mesh in qMeshes
                select mesh.boneWeights
                ;
            var qMtInvs =
                from mesh in qMeshes
                select mesh.bindposes
                ;
            var qSrcBones = mmts
                .Select(x => x.tf.GetComponentOrNull<SkinnedMeshRenderer>()?.bones ?? x.tf.WrapEnumerable().ToArray());
            ;
            //qSrcBones.SelectMany().ForEach(x => Debug.Log(x.name));
            parameters.boneWeightsPerMesh = qBoneWeights.ToArray();
            parameters.mtInvsPerMesh = qMtInvs.ToArray();
            parameters.srcBoneIndexToDstBoneIndex = (qSrcBones, tfBones).ToBoneIndexConversionDictionary();
        }

        public static void calculatePartParameters(
            this AdditionalParameters parameters, (Mesh mesh, Material[] mats, Transform tf)[] mmts)
        {
            var qPartIdPerMesh =
                from mmt in mmts
                    //.Do(x => Debug.Log($"part id is {x.tf.getInParent<StructurePartAuthoring>()?.PartId ?? -1} from {x.tf.getInParent<StructurePartAuthoring>()?.name ?? "null"}"))
                    //.Do(x => Debug.Log($"part id is {x.tf.findInParent<StructurePartAuthoring>()?.PartId ?? -1} from {x.tf.findInParent<StructurePartAuthoring>()?.name ?? "null"}"))
                select mmt.tf.getInParent<IStructurePart>()?.partId ?? -1
                //select mmt.tf.gameObject.GetComponentInParent<StructurePartAuthoring>()?.PartId ?? -1
                ;
            parameters.partIdPerMesh = qPartIdPerMesh.ToArray();
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