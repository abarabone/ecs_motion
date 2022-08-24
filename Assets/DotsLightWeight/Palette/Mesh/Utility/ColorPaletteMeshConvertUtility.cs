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

    public static class ColorPaletteMeshUtility
    {

        /// <summary>
        /// メッシュ構築用のパラメータとして、
        /// パレットのサブインデックスを、サブメッシュ単位で列挙する。
        /// サブインデックスは、マテリアルの palette sub index から取得する。
        /// マテリアルが null の場合は、0 を返す。
        /// </summary>
        public static void CalculatePaletteSubIndexParameter(
            this AdditionalParameters parameters,
            (Mesh mesh, Material[] mats, Transform tf)[] mmts)
        {
            var q =
                from mmt in mmts
                select
                    from mat in mmt.mats
                    select getPaletteSubIndex_(mat)
                ;
            parameters.paletteSubIndexPerSubMesh = q.ToArrayRecursive2();


            /// <summary>
            /// マテリアルから、パレットインデックス情報を取得する。
            /// 該当するプロパティがない場合のインデックスは、0 とする。
            /// </summary>
            static int getPaletteSubIndex_(Material mat) =>
                (mat?.HasProperty("_PaletteSubIndex") ?? false)
                    ? mat.GetInt("_PaletteSubIndex")
                    : 0
                ;
        }
    }
}
