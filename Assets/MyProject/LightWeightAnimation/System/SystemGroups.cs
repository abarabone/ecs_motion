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

using Abarabone.Geometry;
using System.Runtime.InteropServices;

namespace Abarabone.SystemGroup
{

    namespace Simulation
    {

        //[UpdateAfter(typeof(StepPhysicsWorld))]
        [UpdateInGroup(typeof(SimulationSystemGroup))]
        //[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
        //[DisableAutoCreation]
        public class InitializeSystemGroup : ComponentSystemGroup
        { }



        //[UpdateAfter(typeof(StepPhysicsWorld))]
        [UpdateInGroup(typeof(SimulationSystemGroup))]
        //[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
        [UpdateAfter(typeof(InitializeSystemGroup))]
        //[DisableAutoCreation]
        public class HitSystemGroup : ComponentSystemGroup
        { }


        namespace Move
        {
            [UpdateBefore(typeof(EndFramePhysicsSystem))]
            //[UpdateAfter(typeof(StepPhysicsWorld))]
            [UpdateInGroup(typeof(SimulationSystemGroup))]
            //[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
            [UpdateAfter(typeof(HitSystemGroup))]
            //[DisableAutoCreation]
            public class ObjectMoveSystemGroup : ComponentSystemGroup
            { }

        }

    }

    namespace Presentation
    {

        namespace Logic
        {

            [UpdateInGroup( typeof( PresentationSystemGroup ) )]
            //[DisableAutoCreation]
            public class ObjectLogicSystemGroup : ComponentSystemGroup
            { }

        }


        namespace DrawModel
        {

            [UpdateInGroup( typeof( PresentationSystemGroup ) )]
            //[DisableAutoCreation]
            //[UpdateAfter(typeof(ObjectLogicSystemGroup))]
            public class DrawPrevSystemGroup : ComponentSystemGroup
            {
                [UpdateInGroup(typeof(DrawPrevSystemGroup))]
                public class ResetCounter : ComponentSystemGroup { }

                [UpdateInGroup(typeof(DrawPrevSystemGroup))]
                public class Lod : ComponentSystemGroup { }

                [UpdateInGroup(typeof(DrawPrevSystemGroup))] [UpdateAfter(typeof(ResetCounter))] [UpdateAfter(typeof(Lod))]
                public class Culling : ComponentSystemGroup { }

                [UpdateInGroup(typeof(DrawPrevSystemGroup))] [UpdateAfter(typeof(Culling))]
                public class Marking : ComponentSystemGroup { }
                
                [UpdateInGroup(typeof(DrawPrevSystemGroup))] [UpdateAfter(typeof(Culling))]
                public class TempAlloc : ComponentSystemGroup { }
            }

            //[UpdateInGroup( typeof( PresentationSystemGroup ) )]
            ////[DisableAutoCreation]
            //[UpdateAfter( typeof( DrawPrevSystemGroup ) )]
            //[UpdateBefore( typeof( DrawSystemGroup ) )]
            //public class DrawAllocationGroup : ComponentSystemGroup
            //{ }


            namespace MonolithicBoneTransform
            {

                [UpdateInGroup(typeof(PresentationSystemGroup))]
                //[DisableAutoCreation]
                [UpdateAfter(typeof(DrawPrevSystemGroup))]
                public class MonolithicBoneTransformSystemGroup : ComponentSystemGroup
                { }

            }

            namespace MotionBoneTransform
            {

                [UpdateInGroup( typeof( PresentationSystemGroup ) )]
                //[DisableAutoCreation]
                [UpdateAfter( typeof( DrawPrevSystemGroup ) )]
                public class MotionSystemGroup : ComponentSystemGroup
                { }

            }


            [UpdateInGroup( typeof( PresentationSystemGroup ) )]
            //[DisableAutoCreation]
            [UpdateAfter(typeof(MotionBoneTransform.MotionSystemGroup))]
            [UpdateAfter(typeof(MonolithicBoneTransform.MonolithicBoneTransformSystemGroup))]
            public class DrawSystemGroup : ComponentSystemGroup
            { }

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