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

using Abarabone.Authoring;
using Abarabone.CharacterMotion;
using Abarabone.SystemGroup;
using Abarabone.Geometry;
using Abarabone.Character;
using Abarabone.Particle;
using System;

namespace Abarabone.Draw
{

    //[DisableAutoCreation]
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.DrawSystemGroup ) )]
    //[UpdateAfter(typeof())]
    [UpdateBefore( typeof( BeginDrawCsBarier ) )]
    public class Particle2PointsToModelBufferSystem : JobComponentSystem
    {

        BeginDrawCsBarier presentationBarier;// 次のフレームまでにジョブが完了することを保証

        protected override void OnStartRunning()
        {
            this.presentationBarier = this.World.GetExistingSystem<BeginDrawCsBarier>();
        }


        protected override unsafe JobHandle OnUpdate( JobHandle inputDeps )
        {

            //var unitSizesOfDrawModel = this.GetComponentDataFromEntity<DrawModel.BoneUnitSizeData>( isReadOnly: true );
            var offsetsOfDrawModel = this.GetComponentDataFromEntity<DrawModel.InstanceOffsetData>( isReadOnly: true );

            inputDeps = this.Entities
                //.WithoutBurst()
                .WithBurst()
                .WithReadOnly(offsetsOfDrawModel)
                .WithAll<DrawInstance.ParticleTag>()
                .WithNone<DrawInstance.MeshTag>()
                .WithNone<Translation, Rotation>()// 物理パーティクルは除外
                .ForEach(
                    (
                        in DrawInstance.TargetWorkData target,
                        in DrawInstance.ModeLinkData linker,
                        in Particle.AdditionalData additional,
                        in Particle.TranslationPtoPData pos
                    ) =>
                    {

                        var i = target.DrawInstanceId * 2;

                        var size = additional.Size;
                        var color = math.asfloat(additional.Color.ToUint());

                        var pInstance = offsetsOfDrawModel[ linker.DrawModelEntity ].pVectorOffsetInBuffer;
                        pInstance[ i + 0 ] = new float4( pos.Start, size );
                        pInstance[ i + 1 ] = new float4( pos.End, color );

                    }
                )
                .Schedule( inputDeps );


            this.presentationBarier.AddJobHandleForProducer( inputDeps );
            return inputDeps;
        }
    }
}
