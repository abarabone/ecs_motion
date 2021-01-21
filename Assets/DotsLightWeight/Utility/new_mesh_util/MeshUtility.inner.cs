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

namespace Abarabone.Geometry.inner
{
    using Abarabone.Common.Extension;
    using Abarabone.Utilities;
    using Abarabone.Geometry.inner.unit;



    static class MeshQyeryUtility
    {
        static public IEnumerable<MeshUnit> AsEnumerable(this Mesh.MeshDataArray meshDataArray)
        {
            var baseVertex = 0;
            for (var i = 0; i < meshDataArray.Length; i++)
            {
                yield return new MeshUnit(i, meshDataArray[i], baseVertex);

                baseVertex += meshDataArray[i].vertexCount;
            }
        }
    }


    static partial class ConvertIndicesUtility
    {

        public static IEnumerable<TIdx> QueryConvertIndexData<TIdx>
            (this Mesh.MeshDataArray srcmeshes, IEnumerable<Matrix4x4> mtsPerMesh)
            where TIdx : struct, IIndexUnit<TIdx>
        =>
            from x in srcmeshes.QuerySubMeshForIndexData<TIdx>(mtsPerMesh)
            from xsub in x.submeshes
            let submesh = xsub
            from tri in x.mt.IsMuinusScale()
                ? submesh.Elements().AsTriangle().Reverse()
                : submesh.Elements().AsTriangle()
            from idx in tri.Do(x => Debug.Log(x))//
            select idx.Add(x.mesh.BaseVertex + submesh.Descriptor.baseVertex)
            ;
        
    }

    static class ConvertVertexUtility
    {

        static public IEnumerable<Vector3> QueryConvertPositions
            (this Mesh.MeshDataArray srcmeshes, AdditionalParameters p)
        =>
            from x in srcmeshes.QuerySubMeshForVertices<Vector3>(p, (md, arr) => md.GetVertices(arr))
            from xsub in x.submeshes
            from vtx in xsub.submesh.Elements()
            select (Vector3)math.transform(x.mt, vtx)
            ;


        static public IEnumerable<Vector2> QueryConvertUvs
            (this Mesh.MeshDataArray srcmeshes, AdditionalParameters p, int channel)
        =>
            from x in srcmeshes.QuerySubMeshForVertices<Vector2>(p, (md, arr) => md.GetUVs(channel, arr))
            from xsub in x.submeshes
            from uv in xsub.submesh.Elements()
            select p.texhashToUvRect != null
                ? uv.ScaleUv(p.texhashToUvRect[xsub.texhash])
                : uv
            ;

    }


    static class SubmeshQyeryConvertUtility
    {

        public static IEnumerable<(Matrix4x4 mt, IEnumerable<(SubMeshUnit<T> submesh, int texhash)> submeshes)>
            QuerySubMeshForVertices<T>(
                this Mesh.MeshDataArray srcmeshes,
                AdditionalParameters p,
                Action<Mesh.MeshData, NativeArray<T>> getElementSrc
            ) where T : struct
        =>
            from x in (srcmeshes.AsEnumerable(), p.mtsPerMesh).Zip()
            let submeshes = x.src0.MeshData.QuerySubmeshesForVertices(getElementSrc)
            let mt = p.mtBaseInv * x.src1
            select (
                mt,
                from xsub in (submeshes, p.texhashPerSubMesh).Zip()
                let submesh = xsub.src0
                let texhash = xsub.src1
                select (submesh, texhash)
            );


        public static IEnumerable<(Matrix4x4 mt, MeshUnit mesh, IEnumerable<SubMeshUnit<T>> submeshes)>
            QuerySubMeshForIndexData<T>(this Mesh.MeshDataArray srcmeshes, IEnumerable<Matrix4x4> mtsPerMesh)
            where T : struct
        =>
            from x in (srcmeshes.AsEnumerable(), mtsPerMesh).Zip()
            let mesh = x.src0
            let mt = x.src1
            select (
                mt,
                mesh,
                from xsub in mesh.MeshData.QuerySubmeshesForIndexData<T>()
                select xsub
            );
    }


    static class MeshElementsSourceUtility
    {

        public static IEnumerable<SubMeshUnit<T>> QuerySubmeshesForVertices<T>
            (this Mesh.MeshData meshdata, Action<Mesh.MeshData, NativeArray<T>> getElementSrc) where T : struct
        {
            var array = new NativeArray<T>(meshdata.vertexCount, Allocator.Temp);
            getElementSrc(meshdata, array);

            return
                from i in 0.Inc(meshdata.subMeshCount)
                let desc = meshdata.GetSubMesh(i)
                select new SubMeshUnit<T>(i, desc, () => array.rangeWithUsing(desc.firstVertex, desc.vertexCount))
                ;
        }

        public static IEnumerable<SubMeshUnit<T>> QuerySubmeshesForIndexData<T>
            (this Mesh.MeshData meshdata) where T : struct
        {
            var array = meshdata.GetIndexData<T>();

            return
                from i in 0.Inc(meshdata.subMeshCount)
                let desc = meshdata.GetSubMesh(i)
                select new SubMeshUnit<T>(i, desc, () => array.range(desc.indexStart, desc.indexCount))
                ;
        }


        static IEnumerable<T> rangeWithUsing<T>(this NativeArray<T> array, int first, int length) where T : struct
        {
            using (array) { foreach (var x in array.Range(first, length)) yield return x; }
        }
        static IEnumerable<T> range<T>(this NativeArray<T> array, int first, int length) where T : struct
        {
            foreach (var x in array.Range(first, length)) yield return x;
        }
    }

}
