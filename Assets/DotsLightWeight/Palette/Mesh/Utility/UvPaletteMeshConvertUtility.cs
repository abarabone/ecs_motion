using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DotsLite.Geometry.inner.palette
{
    using DotsLite.Common.Extension;
    using DotsLite.Utilities;
    using DotsLite.Geometry.inner.unit;
    using DotsLite.Misc;
    using DotsLite.Geometry;

    public static class UvPaletteMeshUtility
    {

        /// <summary>
        /// モデル構成要素のマテリアル配列から、マテリアルの通し番号配列を構築する。
        /// 通し番号は、各サブメッシュに対して必要なので、逆に言うと元のメッシュのサブマテリアルとサブメッシュは同じ数の必要がある。
        /// </summary>
        public static void CalculateUvPaletteSubIndexParameter(
            this AdditionalParameters parameters,
            (Mesh mesh, Material[] mats, Transform tf)[] mmts)
        {
            var qMatLength =
                from mmt in mmts
                select mmt.mats.Length
                ;
            var qStartIndex = new[] { 0 }.Concat(qMatLength);

            var q =
                from x in (mmts, qStartIndex).Zip()
                let mmt = x.src0
                let ist = x.src1
                select
                    from imat in mmt.mats.Select((x, i) => i)
                    select ist + imat
                ;
            parameters.uvPaletteSubIndexPerSubMesh = q.ToArrayRecursive2();
        }
    }
}
