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
    using Abarabone.Authoring;
    using Abarabone.Misc;
    using Abarabone.SystemGroup;
    using Abarabone.Geometry;
    using Abarabone.Particle;


    [UpdateBefore(typeof(DrawCullingSystem))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup))]
    public class DrawLodSelectorSystem : SystemBase
    {

        protected override void OnUpdate()
        {

            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);

            var campos = Camera.current.transform.position;

            this.Entities
                .WithBurst(FloatMode.Fast, FloatPrecision.Standard)
                .WithReadOnly(poss)
                .ForEach(
                        (
                            ref DrawInstance.ModeLinkData modelLink,
                            in DrawInstance.ModelLod2LinkData lodLink,
                            in DrawInstance.PostureLinkData posturelink
                        ) =>
                        {

                            var pos = poss[posturelink.PostureEntity];

                            var distsq = math.distancesq(pos.Value, campos);


                            if (lodLink.SqrDistance0 <= distsq)
                            {
                                modelLink.DrawModelEntityCurrent = lodLink.DrawModelEntity0;

                                return;
                            }


                            if (lodLink.SqrDistance1 <= distsq)
                            {
                                modelLink.DrawModelEntityCurrent = lodLink.DrawModelEntity1;

                                return;
                            }


                            modelLink.DrawModelEntityCurrent = Entity.Null;

                        }
                )
                .ScheduleParallel();

        }

    }

}
