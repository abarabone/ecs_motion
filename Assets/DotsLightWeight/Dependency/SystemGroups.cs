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
//using Unity.Rendering;
using Unity.Properties;
using Unity.Burst;
using Unity.Physics.Systems;

using DotsLite.Geometry;
using System.Runtime.InteropServices;


// update 順序は、システムの順序
// ジョブ同士はシステムの順序で dependency が組まれると思われる
// ただしジョブと update 間の実行順は制御できないので、そのためのバリアである、と推測する

namespace DotsLite.SystemGroup
{

    namespace Simulation
    {

        //namespace Hit
        //{
        //    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
        //    [UpdateBefore(typeof(BuildPhysicsWorld))]
        //    [UpdateBefore(typeof(InitializeSystemGroup))]
        //    public class HitSystemGroup : ComponentSystemGroup
        //    { }
        //}

        //[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
        //[UpdateBefore(typeof(BuildPhysicsWorld))]
        //public class InitializeSystemGroup : ComponentSystemGroup
        //{ }

        //namespace Move
        //{
        //    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
        //    [UpdateBefore(typeof(BuildPhysicsWorld))]
        //    [UpdateAfter(typeof(Hit.HitSystemGroup))]
        //    public class ObjectMoveSystemGroup : ComponentSystemGroup
        //    { }
        //}

        //// PhysicsWorld


        [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
        [UpdateBefore(typeof(BuildPhysicsWorld))]
        public class Initialize : ComponentSystemGroup
        { }

        namespace Move
        {
            [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
            [UpdateBefore(typeof(BuildPhysicsWorld))]
            [UpdateAfter(typeof(Initialize))]
            public class ObjectMove : ComponentSystemGroup
            { }
        }

        // PhysicsWorld

        namespace Hit
        {
            [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
            [UpdateAfter(typeof(ExportPhysicsWorld))]
            [UpdateBefore(typeof(EndFramePhysicsSystem))]
            public class Hit : ComponentSystemGroup
            { }
        }


    }

    namespace Presentation
    {

        namespace Logic
        {

            [UpdateInGroup( typeof( PresentationSystemGroup ) )]
            public class ObjectLogic : ComponentSystemGroup
            { }

        }


        namespace Render
        {

            [UpdateInGroup( typeof( PresentationSystemGroup ) )]
            [UpdateAfter(typeof(Logic.ObjectLogic))]
            public class DrawPrev : ComponentSystemGroup
            {
                [UpdateInGroup(typeof(DrawPrev))]
                public class ResetCounter : ComponentSystemGroup { }

                [UpdateInGroup(typeof(DrawPrev))]
                public class Lod : ComponentSystemGroup { }

                [UpdateInGroup(typeof(DrawPrev))] [UpdateAfter(typeof(ResetCounter))] [UpdateAfter(typeof(Lod))]
                public class Culling : ComponentSystemGroup { }

                [UpdateInGroup(typeof(DrawPrev))] [UpdateAfter(typeof(Culling))]
                public class Marking : ComponentSystemGroup { }
                
                [UpdateInGroup(typeof(DrawPrev))] [UpdateAfter(typeof(Culling))]
                public class TempAlloc : ComponentSystemGroup { }
            }

            //[UpdateInGroup( typeof( PresentationSystemGroup ) )]
            ////[DisableAutoCreation]
            //[UpdateAfter( typeof( DrawPrevSystemGroup ) )]
            //[UpdateBefore( typeof( DrawSystemGroup ) )]
            //public class DrawAllocationGroup : ComponentSystemGroup
            //{ }


            //namespace MonolithicBoneTransform
            //{

            //    [UpdateInGroup(typeof(PresentationSystemGroup))]
            //    [UpdateAfter(typeof(DrawPrevSystemGroup))]
            //    public class MonolithicBoneTransformSystemGroup : ComponentSystemGroup
            //    { }

            //}

            //namespace MotionBoneTransform
            //{

            //    [UpdateInGroup( typeof( PresentationSystemGroup ) )]
            //    [UpdateAfter( typeof( DrawPrevSystemGroup ) )]
            //    public class MotionSystemGroup : ComponentSystemGroup
            //    { }

            //}


            // グループを小分けにしてしまうと、スケジュールの自由度が制限されてしまうかも…

            [UpdateInGroup( typeof( PresentationSystemGroup ) )]
            [UpdateAfter(typeof(DrawPrev))]
            //[UpdateAfter(typeof(MotionBoneTransform.MotionSystemGroup))]
            //[UpdateAfter(typeof(MonolithicBoneTransform.MonolithicBoneTransformSystemGroup))]
            public class Draw : ComponentSystemGroup
            {

                [UpdateInGroup(typeof(Draw))]
                public class Transform : ComponentSystemGroup
                {
                    [UpdateInGroup(typeof(Transform))]
                    public class MonolithicBone : ComponentSystemGroup { }

                    [UpdateInGroup(typeof(Transform))]
                    public class MotionBone : ComponentSystemGroup { }
                }

                [UpdateInGroup(typeof(Draw))]
                [UpdateAfter(typeof(Transform))]
                public class Transfer : ComponentSystemGroup { }

                [UpdateInGroup(typeof(Draw))]
                [UpdateAfter(typeof(Transfer))]
                public class Sort : ComponentSystemGroup { }

                [UpdateInGroup(typeof(Draw))]
                [UpdateAfter(typeof(Sort))]
                public class Call : ComponentSystemGroup { }
            }

            [UpdateInGroup(typeof(PresentationSystemGroup))]
            [UpdateAfter(typeof(Draw))]
            public class DrawAfter : ComponentSystemGroup
            {
                [UpdateInGroup(typeof(DrawAfter))]
                public class TempFree : ComponentSystemGroup { }
            }

            //[UpdateInGroup( typeof( PresentationSystemGroup ) )]
            ////[DisableAutoCreation]
            //[UpdateAfter( typeof( DrawSystemGroup ) )]
            //public class DrawDeallocationGroup : ComponentSystemGroup
            //{ }

        }


    }





    ////[DisableAutoCreation]
    //[UpdateInGroup( typeof( PresentationSystemGroup ) )]
    //public class ObjectMoveSystemGroup : ComponentSystemGroup
    //{ }

    ////[DisableAutoCreation]
    //[UpdateInGroup( typeof( PresentationSystemGroup ) )]
    //public class ObjectLogicSystemGroup : ComponentSystemGroup
    //{ }


    ////[DisableAutoCreation]
    ////[UpdateAfter(typeof(ObjectLogicSystemGroup))]
    //[UpdateInGroup( typeof( PresentationSystemGroup ) )]
    //public class DrawPrevSystemGroup : ComponentSystemGroup
    //{ }

    ////[DisableAutoCreation]
    //[UpdateAfter( typeof( PresentationSystemGroup ) )]
    //[UpdateInGroup( typeof( SimulationSystemGroup ) )]
    //public class MotionSystemGroup : ComponentSystemGroup
    //{ }

    //[UpdateAfter( typeof( PresentationSystemGroup ) )]
    //[UpdateBefore( typeof( DrawSystemGroup ) )]
    //[UpdateInGroup( typeof( SimulationSystemGroup ) )]
    //public class DrawAllocationGroup : ComponentSystemGroup
    //{ }

    ////[DisableAutoCreation]
    ////[UpdateAfter( typeof( MotionSystemGroup ) )]
    ////[UpdateAfter( typeof( LateSimulationSystemGroup ) )]
    //[UpdateInGroup( typeof( LateSimulationSystemGroup ) )]
    //public class DrawSystemGroup : ComponentSystemGroup
    //{ }

}