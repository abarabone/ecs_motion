using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using UniRx;
//using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Properties;
using Unity.Burst;
using Abss.Geometry;

namespace Abss.Instance
{

    public struct CharacterLinkData : IComponentData
    {
        public Entity PostureEntity;
        public Entity DrawEntity;
        public Entity MotionEntity;
    }

    public struct PlayerCharacterTag : IComponentData
    { }



}
