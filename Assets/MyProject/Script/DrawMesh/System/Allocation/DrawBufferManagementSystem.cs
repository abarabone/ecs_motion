﻿using System.Collections;
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

using Abarabone.Misc;
using Abarabone.Utilities;
using Abarabone.SystemGroup;

namespace Abarabone.Draw
{

    //[DisableAutoCreation]
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup ) )]
    public class DrawBufferManagementSystem : JobComponentSystem
    {
        

        protected override void OnCreate()
        {

            initDrawSystemComponents_( this.EntityManager );

            this.Enabled = false;
            return;


            Entity initDrawSystemComponents_( EntityManager em_ )
            {
                var arch = em_.CreateArchetype(
                    typeof( DrawSystemComputeTransformBufferData ),
                    typeof( DrawSystemNativeTransformBufferData )
                );
                var ent = em_.CreateEntity( arch );
                em_.SetName( ent, "draw system" );


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

            disposeTransformComputeBuffer_();
            disposeComputeArgumentsBuffersAllModels_();
            
            return;


            void disposeTransformComputeBuffer_()
            {
                var cb = this.GetSingleton<DrawSystemComputeTransformBufferData>();
                cb.Transforms.Dispose();
            }

            void disposeComputeArgumentsBuffersAllModels_()
            {
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
}
