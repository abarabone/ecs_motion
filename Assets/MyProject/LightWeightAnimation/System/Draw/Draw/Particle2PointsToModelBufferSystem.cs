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

using System;

namespace Abarabone.Draw
{

    using Abarabone.Authoring;
    using Abarabone.CharacterMotion;
    using Abarabone.SystemGroup;
    using Abarabone.Geometry;
    using Abarabone.Character;
    using Abarabone.Particle;


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

            var modelBuffers = this.GetComponentDataFromEntity<DrawModel.VectorBufferData>(isReadOnly: true);
            var vectorLengths = this.GetComponentDataFromEntity<DrawModel.VectorLengthData>(isReadOnly: true);

            inputDeps = this.Entities
                //.WithoutBurst()
                .WithBurst()
                .WithReadOnly(modelBuffers)
                .WithReadOnly(vectorLengths)
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
                        if (target.DrawInstanceId == -1) return;


                        var modelBuffer = modelBuffers[linker.DrawModelEntityCurrent];
                        var vcLength = vectorLengths[linker.DrawModelEntityCurrent];

                        const int vectorLengthInBone = 2;
                        var lengthOfInstance = vectorLengthInBone + vcLength.VectorLengthOfInstanceAdditionalData;
                        var i = target.DrawInstanceId * lengthOfInstance + vcLength.VectorLengthOfInstanceAdditionalData;

                        var size = additional.Size;
                        var color = math.asfloat(additional.Color.ToUint());

                        var pModel = modelBuffer.pVectorPerModelInBuffer;
                        pModel[ i + 0 ] = new float4( pos.Start, size );
                        pModel[ i + 1 ] = new float4( pos.End, color );

                    }
                )
                .Schedule( inputDeps );


            this.presentationBarier.AddJobHandleForProducer( inputDeps );
            return inputDeps;
        }
    }
}
