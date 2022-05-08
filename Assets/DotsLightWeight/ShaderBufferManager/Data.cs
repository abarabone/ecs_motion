using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

namespace DotsLite.Draw
{

    public static class ShaderBuffer
    {
        public class ColorPalletData : IComponentData
        {
            public GraphicsBuffer Buffer;
            public int NameId;
        }
    }

    public static class DrawModelShaderBuffer
    {
        public class ColorPalletLinkData : IComponentData
        {
            public Entity BufferEntity;
        }
    }
}
