using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;

namespace DotsLite.Geometry.Palette
{
    using DotsLite.Common.Extension;
    using DotsLite.Utilities;
    using DotsLite.Geometry.inner;
    using DotsLite.Geometry.inner.unit;
    using DotsLite.Structure.Authoring;
    using DotsLite.Utility;
    using DotsLite.Model.Authoring;
    using DotsLite.Draw;
    using DotsLite.Draw.Authoring.Palette;

    /// <summary>
    /// �G���e�B�e�B����
    /// </summary>
    public static class ColorPaletteConversionUtility
    {

        /// <summary>
        /// �C���X�^���X�P�ɑ΂��A�p���b�g�h�c�f�[�^��ǉ�����B
        /// �h�c�́A�p���b�g�̃o�b�t�@���̈ʒu�B�p���b�g�� PaletteAsset ����擾����B
        /// </summary>
        public static void AddColorPaletteComponents(this GameObjectConversionSystem gcs, Entity ent, ColorPaletteAsset palette = null)
        {
            var paletteIdBase = palette == null
                ? gcs.GetColorPaletteBuilder().RegistAndGetId(palette.Colors)
                : 0;

            var em = gcs.DstEntityManager;
            em.AddComponentData(ent, new Palette.ColorPaletteData
            {
                BaseIndex = paletteIdBase,
            });
            em.AddComponentData(ent, new Draw.DrawInstance.TransferSpecialTag { });
        }
    }


}