using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace DotsLite.Draw.Authoring
{
    using DotsLite.Geometry;

    public class ColorPalletBufferAuthoring : MonoBehaviour
    {


        public ColorPalletBuilder Pallets { get; } = new ColorPalletBuilder();

        public string BufferName = "ColorPallets";

    }
}
