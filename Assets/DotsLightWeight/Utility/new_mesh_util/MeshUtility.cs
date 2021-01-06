using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Linq;
using System.Threading.Tasks;
using System;
using Abarabone.Common.Extension;
using UnityEngine.Rendering;
using System.Runtime.CompilerServices;

namespace Abarabone.Geometry
{
    static public class MeshInWorkerThreadUtility
    {

        static public IEnumerable<Mesh.MeshData> AsEnumerable(this Mesh.MeshDataArray meshDataArray)
        {
            for(var i = 0; i < meshDataArray.Length; i++)
            {
                yield return meshDataArray[i];
            }
        }


        public struct SubMesh<T> where T : struct
        {
            public IEnumerable<T> VertexData;
        }
        static public IEnumerable<SubMesh<T>> SubMeshes<T>(this Mesh.MeshData meshdata)
            where T:struct
        {
            var vdata = meshdata.GetVertexData<T>();

            foreach (var range in meshdata.getSubmeshRange_())
            {
                yield return new SubMesh<T>
                {
                    VertexData = getVerticesInSubmesh_()
                };

                IEnumerable<T> getVerticesInSubmesh_()
                {
                    for (var i = range.start; i < range.length; i++)
                    {
                        yield return vdata[i];
                    }
                }
            }
        }

        static IEnumerable<(int start, int length)> getSubmeshRange_(this Mesh.MeshData meshdata) =>
            from i in Enumerable.Range(0, meshdata.subMeshCount)
            let submeshDescriptor = meshdata.GetSubMesh(i)
            select (fst: submeshDescriptor.firstVertex, length: submeshDescriptor.vertexCount)
            ;
    }
}
