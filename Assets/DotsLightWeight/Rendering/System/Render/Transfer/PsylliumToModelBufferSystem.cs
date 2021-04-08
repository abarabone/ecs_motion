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

namespace Abarabone.Draw
{
    using Abarabone.CharacterMotion;
    using Abarabone.SystemGroup;
    using Abarabone.Geometry;
    using Abarabone.Character;
    using Abarabone.Particle;
    using Abarabone.Dependency;


    /// <summary>
    /// 物理をつけたパーティクル用　暫定でのこしてある
    /// </summary>
    //[DisableAutoCreation]
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.DrawSystemGroup ) )]
    //[UpdateAfter(typeof())]
    //[UpdateBefore( typeof( BeginDrawCsBarier ) )]
    [UpdateBefore(typeof(DrawMeshCsSystem))]
    public class PsylliumToDrawModelBufferSystem : DependencyAccessableSystemBase
    {


        RegisterBarrier bardep;

        protected override void OnCreate()
        {
            base.OnCreate();

            this.bardep = RegisterBarrier.Create<DrawMeshCsSystem>(this);
        }

        protected unsafe override void OnUpdate()
        {
            using var barScope = bardep.WithDependencyScope();


            //var unitSizesOfDrawModel = this.GetComponentDataFromEntity<DrawModel.BoneUnitSizeData>( isReadOnly: true );
            var offsetsOfDrawModel = this.GetComponentDataFromEntity<DrawModel.InstanceOffsetData>( isReadOnly: true );

            this.Entities
                .WithBurst()
                .WithReadOnly(offsetsOfDrawModel)
                .WithAll<DrawInstance.ParticleTag>()
                .WithNone<DrawInstance.MeshTag>()
                .ForEach(
                    (
                        in DrawInstance.TargetWorkData target,
                        in DrawInstance.ModeLinkData linker,
                        in Particle.AdditionalData additional,
                        in Translation pos,
                        in Rotation rot
                    ) =>
                    {
                        if (target.DrawInstanceId == -1) return;


                        var i = target.DrawInstanceId * 2;

                        var size = additional.Size;
                        var color = math.asfloat(additional.Color.ToUint());

                        var pInstance = offsetsOfDrawModel[ linker.DrawModelEntityCurrent ].pVectorOffsetPerModelInBuffer;
                        pInstance[ i + 0 ] = new float4( pos.Value, size );
                        pInstance[ i + 1 ] = new float4( pos.Value + math.forward( rot.Value ), color );
                    }
                )
                .ScheduleParallel();
        }
    }
}
