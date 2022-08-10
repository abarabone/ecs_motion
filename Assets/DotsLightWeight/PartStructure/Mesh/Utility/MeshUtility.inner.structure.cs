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


        static public IEnumerable<Color32> QueryConvertPartId
            (this IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
        =>
            from permesh in (p.partIdPerMesh, srcmeshes).Zip()
            let pid = permesh.src0
            let color = new Color32
            {
                r = (byte)(pid & 0x1f),
                g = (byte)(pid >> 5 & 0xff),// 32bit ‚È‚Ì‚Å >> 5
            }
            from vtx in Enumerable.Range(0, permesh.src1.MeshData.vertexCount)
            select color
            ;


    }


}
