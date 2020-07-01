using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Entities;
using Unity.Transforms;
//using Unity.Rendering;
using Unity.Properties;
using Unity.Burst;
using Unity.Physics;

using Abarabone.Geometry;
using Abarabone.Utilities;

namespace Abarabone.Character
{
    //廃止予定
    public struct ObjectMainCharacterLinkData : IComponentData
    {
        public Entity PostureEntity;
        //public Entity DrawEntity;
        public Entity MotionEntity;
    }

    public struct ObjectMotionLinkDate : IComponentData
    {
        public Entity MotionEntity;
    }
    public struct ObjectMotion2LinkData : IComponentData
    {
        public Entity MotionEntity0;
        public Entity MotionEntity1;
    }


    // 以下は廃止予定
    public struct CharacterSubMotion1LinkData : IComponentData
    {
        public Entity MotionEntity;
    }
    public struct CharacterSubMotion2LinkData : IComponentData
    {
        public Entity MotionEntity;
    }


}

