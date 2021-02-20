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

namespace Abarabone.Geometry
{
    using Abarabone.Common.Extension;
    using Abarabone.Utilities;
    using Abarabone.Geometry.inner;
    using Abarabone.Geometry.inner.unit;



    public struct srcMeshDataFromModelGroup : IDisposable
    {
        public srcMeshDataFromModelGroup
            (Mesh.MeshDataArray marr, IEnumerable<IEnumerable<SrcMeshesModelCombinePack>> e)
        {
            this.marr = marr;
            this.AsEnumerable = e;
        }
        Mesh.MeshDataArray marr;
        public IEnumerable<IEnumerable<SrcMeshesModelCombinePack>> AsEnumerable { get; private set; }
        public void Dispose() => this.marr.Dispose();
    }

    public struct srcMeshDataFromModel : IDisposable
    {
        public srcMeshDataFromModel
            (Mesh.MeshDataArray marr, IEnumerable<SrcMeshesModelCombinePack> e)
        {
            this.marr = marr;
            this.AsEnumerable = e;
        }
        Mesh.MeshDataArray marr;
        public IEnumerable<SrcMeshesModelCombinePack> AsEnumerable { get; private set; }
        public void Dispose() => this.marr.Dispose();
    }



    public struct SrcMeshesModelCombinePack
    {
        public SrcMeshesModelCombinePack(IEnumerable<SrcMeshUnit> e, ObjectAndMmts ommts)
        {
            this.AsEnumerable = e;
            this.mmts = ommts.mmts;
            this.obj = ommts.obj;
        }
        public IEnumerable<SrcMeshUnit> AsEnumerable { get; private set; }
        public GameObject obj { get; private set; }
        public (Mesh mesh, Material[] mats, Transform tf)[] mmts { get; private set; }
    }

    public struct ObjectAndMmts
    {
        public GameObject obj;
        public (Mesh mesh, Material[] mats, Transform tf)[] mmts;
    }



    public static class MeshQueryUtility
    {

        public static srcMeshDataFromModelGroup QueryMeshDataFromModelGroup
            (this IEnumerable<IEnumerable<ObjectAndMmts>> ommtsss)
        {
            var srcmeshes = ommtsss.SelectMany().Select(x => x.mmts).SelectMany().Select(x => x.mesh).ToArray();
            var mesharr = Mesh.AcquireReadOnlyMeshData(srcmeshes);

            var imesh = 0;
            var q =
                from ommtss in ommtsss
                select
                    from ommts in ommtss
                    let len = ommts.mmts.Count()
                    let meshes = queryMesh_(imesh.PostAdd(len), len)
                    select new SrcMeshesModelCombinePack(meshes, ommts)
                ;
            return new srcMeshDataFromModelGroup(mesharr, q);

            IEnumerable<SrcMeshUnit> queryMesh_(int first, int length)
            {
                var baseVertex = 0;

                for (var i = 0; i < length; i++)
                {
                    yield return new SrcMeshUnit(i, mesharr[i + first], baseVertex);

                    baseVertex += mesharr[i + first].vertexCount;
                }
            }
        }
        
        public static srcMeshDataFromModel QueryMeshDataFromModel
            (this IEnumerable<ObjectAndMmts> ommtss)
        {
            var srcmeshes = ommtss.Select(x => x.mmts).SelectMany().Select(x => x.mesh).ToArray();
            var mesharr = Mesh.AcquireReadOnlyMeshData(srcmeshes);

            var imesh = 0;
            var q =
                from ommts in ommtss
                let len = ommts.mmts.Count()
                let meshes = queryMesh_(imesh.PostAdd(len), len)
                select new SrcMeshesModelCombinePack(meshes, ommts)
                ;
            return new srcMeshDataFromModel(mesharr, q);

            IEnumerable<SrcMeshUnit> queryMesh_(int first, int length)
            {
                var baseVertex = 0;

                for (var i = 0; i < length; i++)
                {
                    yield return new SrcMeshUnit(i, mesharr[i + first], baseVertex);

                    baseVertex += mesharr[i + first].vertexCount;
                }
            }
        }



        public static IEnumerable<GameObject> Objs(this IEnumerable<ObjectAndMmts> ommtss) =>
            ommtss.Select(x => x.obj);


    }

}
