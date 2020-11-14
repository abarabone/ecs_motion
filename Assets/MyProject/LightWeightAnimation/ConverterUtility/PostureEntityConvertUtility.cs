using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace Abarabone.Model.Authoring
{
    using CharacterMotion;
    using Draw;
    using Character;
    

    using Abarabone.Common.Extension;
    using Abarabone.Misc;
    using Abarabone.Utilities;
    using Unity.Physics;
    using Unity.Physics.Authoring;

    static public class PostureEntitiesConvertUtility
    {


        static public void CreatePostureEntities
            (this GameObjectConversionSystem gcs, GameObject mainGameObject, IEnumerable<Transform> bones)
        {

            var postureEntity = addComponentsPostureEntity_(gcs, mainGameObject);

            setPostureValue(gcs, postureEntity, bones.First());

            return;


            Entity addComponentsPostureEntity_(GameObjectConversionSystem gcs_, GameObject main_)
            {
                var ent = gcs_.GetPrimaryEntity(main_);

                var addtypes = new ComponentTypes
                (
                    //typeof( Posture.NeedTransformTag ),
                    //typeof( Posture.LinkData ),
                    typeof(Translation),
                    typeof(Rotation)
                );
                gcs_.DstEntityManager.AddComponents(ent, addtypes);

                return ent;
            }

            void setPostureValue(GameObjectConversionSystem gcs_, Entity postureEntity_, Transform tfbone_)
            {
                var em = gcs_.DstEntityManager;

                var boneEntity = gcs_.GetPrimaryEntity(tfbone_);

                //em.SetComponentData( postureEntity, new Posture.LinkData { BoneRelationTop = boneTopEntity } );
                em.SetComponentData(postureEntity_, new Rotation { Value = quaternion.identity });
                em.SetComponentData(postureEntity_, new Translation { Value = float3.zero });
            }
        }



    }
}