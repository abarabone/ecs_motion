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



    static partial class ConvertVertexUtility
    {


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


        static public IEnumerable<Color32> QueryUvOffsetIndex(
            this IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
        {
            var qVtx = srcmeshes.QuerySubMeshForUnitVertices();
            var qColor =
                from idxs in p.UvIndexPerSubMesh
                from idx in idxs
                select new Color32
                {
                    b = (byte)idx,
                };
            return
                from x in (qVtx, qColor).Zip()
                let vtxs = x.src0
                let color = x.src1
                from _ in vtxs
                select color
                ;
        }
    }

}
