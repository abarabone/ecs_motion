using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace DotsLite.Draw.Authoring
{
    using DotsLite.Geometry;

    public class ColorPaletteBufferAuthoring : MonoBehaviour
    {


        public ColorPaletteBuilder Palettes { get; } = new ColorPaletteBuilder();

        public string BufferName = "ColorPalettes";

    }
}
