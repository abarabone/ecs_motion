using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;

namespace DotsLite.Draw.disable
{
    using DotsLite.CharacterMotion;
    using DotsLite.SystemGroup;
    using DotsLite.Geometry;
    using DotsLite.Character;
    using DotsLite.ParticleSystem;
    using DotsLite.Dependency;


    /// <summary>
    /// 物理をつけたパーティクル用　暫定でのこしてある　長さ固定なのでいまいち
    /// </summary>
    [DisableAutoCreation]
    [UpdateInGroup( typeof( SystemGroup.Presentation.Render.Draw ) )]
    //[UpdateAfter(typeof())]
    //[UpdateBefore( typeof( BeginDrawCsBarier ) )]
    [UpdateBefore(typeof(DrawBufferToShaderDataSystem))]
    public partial class PsylliumToDrawModelBufferSystem : DependencyAccessableSystemBase
    {


        BarrierDependency.Sender bardep;

        protected override void OnCreate()
        {
            base.OnCreate();

            this.bardep = BarrierDependency.Sender.Create<DrawBufferToShaderDataSystem>(this);
        }

        protected unsafe override void OnUpdate()
        {
            using var barScope = bardep.WithDependencyScope();


            var nativeBuffers = this.GetComponentDataFromEntity<DrawSystem.NativeTransformBufferData>(isReadOnly: true);
            var drawSysEnt = this.GetSingletonEntity<DrawSystem.NativeTransformBufferData>();

            //var unitSizesOfDrawModel = this.GetComponentDataFromEntity<DrawModel.BoneUnitSizeData>( isReadOnly: true );
            var offsetsOfDrawModel = this.GetComponentDataFromEntity<DrawModel.VectorIndexData>( isReadOnly: true );

            this.Entities
                .WithBurst()
                .WithReadOnly(nativeBuffers)
                .WithReadOnly(offsetsOfDrawModel)
                .WithAll<DrawInstance.PsylliumTag>()
                .WithNone<DrawInstance.MeshTag>()
                .ForEach(
                    (
                        in DrawInstance.TargetWorkData target,
                        in DrawInstance.ModelLinkData linker,
                        in Particle.OptionalData additional,
                        in Translation pos,
                        in Rotation rot
                    ) =>
                    {
                        if (target.DrawInstanceId == -1) return;


                        var i = target.DrawInstanceId * 2;

                        var size = additional.Radius;
                        var color = math.asfloat(additional.BlendColor.ToUint());

                        var pInstance = nativeBuffers[drawSysEnt].Transforms.pBuffer + offsetsOfDrawModel[linker.DrawModelEntityCurrent].ModelStartIndex;
                        //var pInstance = offsetsOfDrawModel[ linker.DrawModelEntityCurrent ].pVectorOffsetPerModelInBuffer;
                        pInstance[ i + 0 ] = new float4( pos.Value, size );
                        pInstance[ i + 1 ] = new float4( pos.Value + math.forward( rot.Value ), color );
                    }
                )
                .ScheduleParallel();
        }
    }
}
