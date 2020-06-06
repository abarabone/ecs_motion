using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine.InputSystem;

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;

using Abss.Misc;
using Abss.Utilities;
using Abss.SystemGroup;

namespace Abss.Draw
{

    //[DisableAutoCreation]
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup ) )]
    public class DrawBufferManagementSystem : JobComponentSystem
    {
        

        protected override void OnCreate()
        {

            initDrawSystemComponents_( this.EntityManager );

            return;


            Entity initDrawSystemComponents_( EntityManager em_ )
            {
                var arch = em_.CreateArchetype(
                    typeof( DrawSystemComputeTransformBufferData ),
                    typeof( DrawSystemNativeTransformBufferData )
                );
                var ent = em_.CreateEntity( arch );


                const int maxBufferLength = 1000 * 16 * 2;//

                var stride = Marshal.SizeOf( typeof( float4 ) );

                em_.SetComponentData( ent,
                    new DrawSystemComputeTransformBufferData
                    {
                        Transforms = new ComputeBuffer
                            ( maxBufferLength, stride, ComputeBufferType.Default, ComputeBufferMode.Immutable ),
                    }
                );

                return ent;
            }

        }



        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            return inputDeps;
        }


        protected override void OnDestroy()
        {
            if( !this.HasSingleton<DrawSystemComputeTransformBufferData>() ) return;

            var cb = this.GetSingleton<DrawSystemComputeTransformBufferData>();
            cb.Transforms.Dispose();

            var eq = this.EntityManager.CreateEntityQuery( typeof( DrawModelComputeArgumentsBufferData ) );
            using( eq )
            {
                var args = eq.ToComponentDataArray<DrawModelComputeArgumentsBufferData>();
                foreach( var arg in args )
                {
                    arg.InstanceArgumentsBuffer.Dispose();
                }
            }
        }

    }
}
