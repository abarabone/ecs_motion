using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace DotsLite.Draw
{
    
    using DotsLite.Misc;
    using DotsLite.SystemGroup;
    using DotsLite.Utilities;
    using DotsLite.Dependency;

    ////[DisableAutoCreation]
    //[UpdateInGroup(typeof( SystemGroup.Presentation.DrawModel.DrawSystemGroup ) )]
    //public class BeginDrawCsBarier : EntityCommandBufferSystem
    //{ }


    /// <summary>
    /// メッシュをインスタンシングバッファを使用してインスタンシング描画する
    /// </summary>
    //[DisableAutoCreation]
    //[UpdateAfter(typeof( BeginDrawCsBarier ) )]
    [UpdateInGroup(typeof( SystemGroup.Presentation.Render.Draw.Call ) )]
    public partial class DrawBufferToShaderDataSystem : SystemBase, BarrierDependency.IRecievable
    {


        public BarrierDependency.Reciever Reciever { get; } = BarrierDependency.Reciever.Create();



        protected override void OnDestroy()
        {
            base.OnDestroy();

            this.Reciever.Dispose();
        }

        protected override unsafe void OnUpdate()
        {
            this.Reciever.CompleteAllDependentJobs(this.Dependency);

            var nativeBuffer = this.GetSingleton<DrawSystem.NativeTransformBufferData>().Transforms;
            var computeBuffer = this.GetSingleton<DrawSystem.ComputeTransformBufferData>().Transforms;
            var bufferInfo = this.GetSingleton<DrawSystem.TransformBufferInfoData>();
            computeBuffer.SetData( nativeBuffer.AsNativeArray(bufferInfo.CurrentVectorLength) );
        }

    }
}
