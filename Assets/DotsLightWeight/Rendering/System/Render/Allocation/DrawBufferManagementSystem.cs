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
    public class DrawBufferManagementSystem : SystemBase
    {
        

        protected override void OnCreate()
        {
            base.OnCreate();

            this.Enabled = false;
        }



        protected override void OnUpdate()
        { }



        protected override void OnDestroy()
        {
            if( !this.HasSingleton<DrawSystem.ComputeTransformBufferData>() ) return;

            disposeTransformComputeBuffer_();
            disposeComputeArgumentsBuffersAllModels_();

            disposeTransformNativeBuffer_();

            base.OnDestroy();
            return;


            void disposeTransformComputeBuffer_()
            {
                var cb = this.GetSingleton<DrawSystem.ComputeTransformBufferData>();
                cb.Transforms.Dispose();
            }

            void disposeComputeArgumentsBuffersAllModels_()
            {
                var eq = this.EntityManager.CreateEntityQuery( typeof( DrawModel.ComputeArgumentsBufferData ) );
                using( eq )
                {
                    var args = eq.ToComponentDataArray<DrawModel.ComputeArgumentsBufferData>();
                    foreach( var arg in args )
                    {
                        arg.InstanceArgumentsBuffer.Dispose();
                    }
                }
            }

            void disposeTransformNativeBuffer_()
            {
                var nb = this.GetSingleton<DrawSystem.NativeTransformBufferData>();
                nb.Transforms.Dispose();
            }
        }


    }
}
