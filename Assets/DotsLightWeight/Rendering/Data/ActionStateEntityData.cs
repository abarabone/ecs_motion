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

namespace DotsLite.Character
{
    using DotsLite.Geometry;
    using DotsLite.Utilities;


    static public partial class ActionState
    {

        // インスタンス破棄のときに必要（あったほうが楽）
        public struct BinderLinkData : IComponentData
        {
            public Entity BinderEntity;
        }

        //public struct ObjectMainTag : IComponentData
        //{ }


        public struct PostureLinkData : IComponentData
        {
            public Entity PostureEntity;
        }



        // 現行
        public struct MotionLinkDate : IComponentData
        {
            public Entity MotionEntity;
        }
        public struct Motion2LinkData : IComponentData
        {
            public Entity MotionEntity0;
            public Entity MotionEntity1;
        }
        // ↓このほうがシステムの汎用性でるかも
        //public struct Motion2LinkData : IComponentData
        //{
        //    public Entity MotionEntity;
        //}

    }


    ////廃止予定
    //public struct ObjectMainCharacterLinkData : IComponentData
    //{
    //    public Entity PostureEntity;
    //    //public Entity DrawEntity;
    //    public Entity MotionEntity;
    //}

    //// 以下は廃止予定
    //public struct CharacterSubMotion1LinkData : IComponentData
    //{
    //    public Entity MotionEntity;
    //}
    //public struct CharacterSubMotion2LinkData : IComponentData
    //{
    //    public Entity MotionEntity;
    //}


}

