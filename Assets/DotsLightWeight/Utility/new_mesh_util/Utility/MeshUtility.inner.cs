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

namespace DotsLite.Geometry.inner
{
    using DotsLite.Common.Extension;
    using DotsLite.Utilities;
    using DotsLite.Geometry.inner.unit;
    using DotsLite.Misc;


    //static class MeshQyeryUtility
    //{
    //    static public IEnumerable<MeshUnit> AsEnumerable(this Mesh.MeshDataArray meshDataArray)
    //    {
    //        var baseVertex = 0;
    //        for (var i = 0; i < meshDataArray.Length; i++)
    //        {
    //            yield return new MeshUnit(i, meshDataArray[i], baseVertex);

    //            baseVertex += meshDataArray[i].vertexCount;
    //        }
    //    }
    //}


    static partial class ConvertIndicesUtility
    {

        public static IEnumerable<TIdx> QueryConvertIndexData<TIdx>(
            this IEnumerable<SrcMeshUnit> srcmeshes, IEnumerable<Matrix4x4> mtsPerMesh)
            where TIdx : struct, IIndexUnit<TIdx>
        =>
            from x in srcmeshes.QuerySubMeshForIndexData<TIdx>(mtsPerMesh)
            from xsub in x.submeshes
            let submesh = xsub
            from tri in x.mt.IsMuinusScale()
                ? submesh.Elements().AsTriangle().Reverse()
                : submesh.Elements().AsTriangle()
            from idx in tri//.Do(x => Debug.Log(x))//
            select idx.Add((uint)x.mesh.BaseVertex + (uint)submesh.Descriptor.baseVertex)
            ;
        
    }

    static class ConvertVertexUtility
    {

        static public IEnumerable<Vector3> QueryConvertPositions(
            this IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
        =>
            from permesh in (srcmeshes, p.mtPerMesh).Zip()
            let mesh = permesh.src0.MeshData
            let mt = math.mul(p.mtBaseInv, permesh.src1)
            from vtx in mesh.QueryMeshVertices<Vector3>((md, arr) => md.GetVertices(arr), VertexAttribute.Position)
            select(Vector3)math.transform(mt, vtx)
            ;


        static public IEnumerable<Vector3> QueryConvertNormals
            (this IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
        =>
            from permesh in (srcmeshes, p.mtPerMesh).Zip()
            let mesh = permesh.src0.MeshData
            let mt = math.mul(p.mtBaseInv, permesh.src1)
            from nm in mesh.QueryMeshVertices<Vector3>((md, arr) => md.GetNormals(arr), VertexAttribute.Normal)
            //select (Vector3)math.mul(math.quaternion(mt), nm)
            select(Vector3)math.rotate(mt, nm)
            ;


        static public IEnumerable<Vector2> QueryConvertUvs(
            this IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p, int channel)
        {
            return p.texHashToUvRect switch
            {
                null =>
                    from mesh in srcmeshes
                    from uv in mesh.MeshData.QueryMeshVertices<Vector2>((md, arr) => md.GetUVs(channel, arr), getAttr_(channel))
                    select uv,
                _ =>
                    from x in srcmeshes.QuerySubMeshForVertices<Vector2>(p, (md, arr) => md.GetUVs(channel, arr), getAttr_(channel))
                    from xsub in x.submeshes
                    let rect = p.texHashToUvRect(xsub.texhash)
                    from uv in xsub.submesh.Elements()
                    select uv.ScaleUv(rect),
                    //select uv.ScaleUv(p.texHashToUvRect(xsub.texhash)),
            };

            VertexAttribute getAttr_(int channel) =>
                channel switch
                {
                    0 => VertexAttribute.TexCoord0,
                    1 => VertexAttribute.TexCoord1,
                    2 => VertexAttribute.TexCoord2,
                    3 => VertexAttribute.TexCoord3,
                    4 => VertexAttribute.TexCoord4,
                    5 => VertexAttribute.TexCoord5,
                    6 => VertexAttribute.TexCoord6,
                    7 => VertexAttribute.TexCoord7,
                    _ => VertexAttribute.TexCoord0,
                };
        }


        static public IEnumerable<Vector3> QueryConvertPositionsWithBone
            (this IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
        =>
            from permesh in (srcmeshes, p.mtInvsPerMesh, p.boneWeightsPerMesh, p.mtPerMesh).Zip()
            let mesh = permesh.src0.MeshData
            let mtInvs = permesh.src1// ���ꂢ���̂��H
            let weis = permesh.src2
            //let mt = permesh.src3 * p.mtBaseInv//p.mtBaseInv
            let mt = math.mul(p.mtBaseInv, permesh.src3)//p.mtBaseInv
            from x in (mesh.QueryMeshVertices<Vector3>((md, arr) => md.GetVertices(arr), VertexAttribute.Position), weis).Zip()
            let vtx = x.src0
            let wei = x.src1
            select (Vector3)math.transform(math.mul(mtInvs[wei.boneIndex0], mt), vtx)
            ;

        static public IEnumerable<uint> QueryConvertBoneIndices
            (this IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
        =>
            from permesh in p.boneWeightsPerMesh.WithIndex()
            from w in permesh.src
            select (uint)(
                p.srcBoneIndexToDstBoneIndex[permesh.i, w.boneIndex0] <<  0 & 0xff |
                p.srcBoneIndexToDstBoneIndex[permesh.i, w.boneIndex1] <<  8 & 0xff |
                p.srcBoneIndexToDstBoneIndex[permesh.i, w.boneIndex2] << 16 & 0xff |
                p.srcBoneIndexToDstBoneIndex[permesh.i, w.boneIndex3] << 24 & 0xff
            );

        static public IEnumerable<Vector4> QueryConvertBoneWeights
            (this IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
        =>
            from permesh in p.boneWeightsPerMesh
            from w in permesh
            select new Vector4(w.weight0, w.weight1, w.weight2, w.weight3)
            ;


        static public IEnumerable<Color32> QueryConvertPartId
            (this IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
        =>
            from permesh in (p.partIdPerMesh, srcmeshes).Zip()
            let pid = permesh.src0
            let color = new Color32
            {
                r = (byte)(pid & 0x1f),
                g = (byte)(pid >> 5 & 0xff),// 32bit �Ȃ̂� >> 5
            }
            from vtx in Enumerable.Range(0, permesh.src1.MeshData.vertexCount)
            select color
            ;


        static public IEnumerable<Color32> QueryColorPaletteSubIndex(
            this IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
        {
            var qVtx = srcmeshes.QuerySubMeshForUnitVertices();
            var qColor =
                from idxs in p.paletteSubIndexPerSubMesh
                from idx in idxs
                select new Color32
                {
                    a = (byte)idx,
                };
            return
                from x in (qVtx, qColor).Zip()
                let vtxs = x.src0
                let color = x.src1
                from _ in vtxs
                select color
                ;
        }
        //static public IEnumerable<Color32> QueryTextureUvOffsetIndex(
        //    this IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
        //{
        //    var qVtx = srcmeshes.QuerySubMeshForUnitVertices();
        //    var qColor =
        //        from idxs in p.UvIndexPerSubMesh
        //        from idx in idxs
        //        select new Color32
        //        {
        //            b = (byte)idx,
        //        };
        //    return
        //        from x in (qVtx, qColor).Zip()
        //        let vtxs = x.src0
        //        let color = x.src1
        //        from _ in vtxs
        //        select color
        //        ;
        //}
    }


    static class SubmeshQyeryConvertUtility
    {

        /// <summary>
        /// ���b�V���P�ʂōs����A�T�u���b�V���P�ʂŌ`��E�e�N�X�`������񋓂���B
        /// </summary>
        public static IEnumerable<(Matrix4x4 mt, IEnumerable<(SrcSubMeshUnit<T> submesh, int texhash)> submeshes)>
            QuerySubMeshForVertices<T>(
                this IEnumerable<SrcMeshUnit> srcmeshes,
                AdditionalParameters p,
                Action<Mesh.MeshData, NativeArray<T>> getElementSrc,
                VertexAttribute attr)
            where T : struct
        =>
            from x in (srcmeshes, p.mtPerMesh, p.texhashPerSubMesh).Zip()
            let submeshes = x.src0.MeshData.QuerySubmeshesForVertices(getElementSrc, attr)
            let mt = p.mtBaseInv * x.src1
            let texhashes = x.src2
            select (
                mt,
                from xsub in (submeshes, texhashes).Zip()//.Do(x => Debug.Log(x.src1+p.texhashToUvRect[x.src1].ToString()))
                let submesh = xsub.src0
                let texhash = xsub.src1
                select (submesh, texhash)
            );


        /// <summary>
        /// ���b�V���P�ʂōs��E�`��A�T�u���b�V���P�ʂŌ`�����񋓂���B
        /// </summary>
        public static IEnumerable<(Matrix4x4 mt, SrcMeshUnit mesh, IEnumerable<SrcSubMeshUnit<T>> submeshes)>
            QuerySubMeshForIndexData<T>(this IEnumerable<SrcMeshUnit> srcmeshes, IEnumerable<Matrix4x4> mtsPerMesh)
            where T : struct, IIndexUnit<T>
        =>
            from x in (srcmeshes, mtsPerMesh).Zip()
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
        /// <summary>
        /// ���b�V���̏W�������񂵁A�T�u���b�V���̒��̒��_��񋓂���B���_���� Unit �ł���A�Ӗ��������Ȃ��B
        /// </summary>
        public static IEnumerable<IEnumerable<Unit>> QuerySubMeshForUnitVertices(
            this IEnumerable<SrcMeshUnit> srcmeshes)
        =>
            from mesh in srcmeshes
            from isub in Enumerable.Range(0, mesh.MeshData.subMeshCount)
            let submesh = mesh.MeshData.GetSubMesh(isub)
            select Enumerable.Repeat(new Unit { }, submesh.vertexCount)
            ;
        public struct Unit { }


        /// <summary>
        /// 
        /// </summary>
        public static IEnumerable<T> QueryMeshVertices<T>(this Mesh.MeshData meshdata,
            Action<Mesh.MeshData, NativeArray<T>> getElementSrc, VertexAttribute attr)
            where T : struct
        {
            //var array = new NativeArray<T>(meshdata.vertexCount, Allocator.TempJob);
            //if (meshdata.GetVertexAttributeDimension(attr) > 0)
            //{
            //    getElementSrc(meshdata, array);
            //}
            //return array.rangeWithUsing(0, array.Length);
            using var array = new NativeArray<T>(meshdata.vertexCount, Allocator.TempJob);
            //using var d = new disposablelogger<T> { dispo = array, msg = $"{attr}" };
            if (meshdata.GetVertexAttributeDimension(attr) > 0)
            {
                getElementSrc(meshdata, array);
            }
            foreach (var e in array.Slice(0, array.Length)) yield return e;
        }

        /// <summary>
        /// 
        /// </summary>
        public static IEnumerable<SrcSubMeshUnit<T>> QuerySubmeshesForVertices<T>(
            this Mesh.MeshData meshdata, Action<Mesh.MeshData, NativeArray<T>> getElementSrc, VertexAttribute attr)
            where T : struct
        {
            using var array = new NativeArray<T>(meshdata.vertexCount, Allocator.TempJob);
            //using var d = new disposablelogger<T> { dispo = array, msg = $"{attr}" };
            if (meshdata.GetVertexAttributeDimension(attr) > 0)
            {
                getElementSrc(meshdata, array);
            }
            var q =
                from i in 0.Inc(meshdata.subMeshCount)
                let desc = meshdata.GetSubMesh(i)
                select new SrcSubMeshUnit<T>(i, desc, () => array.Slice(desc.firstVertex, desc.vertexCount))
                ;
            foreach (var e in q) yield return e;
        }

        /// <summary>
        /// 
        /// </summary>
        public static IEnumerable<SrcSubMeshUnit<T>> QuerySubmeshesForIndexData<T>(this Mesh.MeshData meshdata)
            where T : struct, IIndexUnit<T>
        {
            return
                from i in 0.Inc(meshdata.subMeshCount)
                let desc = meshdata.GetSubMesh(i)
                select new SrcSubMeshUnit<T>(i, desc, () => getIndexDataNativeArray_(desc))
                ;

            // GetIndexData() �̓o�b�t�@�ւ̎Q�Ƃ�Ԃ��̂ŁA�j���s�v
            IEnumerable<T> getIndexDataNativeArray_(SubMeshDescriptor desc) =>
                meshdata.indexFormat switch
                {
                    IndexFormat.UInt16 when typeof(T) != typeof(ushort) =>
                        meshdata.GetIndexData<ushort>()
                            .Slice(desc.indexStart, desc.indexCount)
                            .Select(x => new T().Add(x)),

                    IndexFormat.UInt32 when typeof(T) != typeof(uint) =>
                        meshdata.GetIndexData<uint>()
                            .Slice(desc.indexStart, desc.indexCount)
                            .Select(x => new T().Add(x)),

                    _ =>
                        meshdata.GetIndexData<T>()
                            .Slice(desc.indexStart, desc.indexCount),
                };
        }

        //static IEnumerable<T> range<T>(this NativeArray<T> src, int first, int length) where T : struct
        //{
        //    foreach (var x in src.Range(first, length)) yield return x;
        //}

        //static IEnumerable<T> rangeWithUsing<T>(this NativeArray<T> src, int first, int length) where T : struct
        //{
        //    using var arr = new disposablelogger { dispo = src, msg = $"-" };
        //    foreach (var x in src.Range(first, length)) yield return x;
        //    //using (array) { foreach (var x in array.Range(first, length)) yield return x; }
        //}
        //class disposablelogger<T> : IDisposable where T: struct
        //{
        //    public string msg;
        //    public NativeArray<T> dispo;
        //    public void Dispose()
        //    {
        //        Debug.Log($"dispose {this.dispo.IsCreated} {this.msg}");
        //        this.dispo.Dispose();
        //    }
        //}
    }

}