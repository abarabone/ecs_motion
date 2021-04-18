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


        static public void InitPostureEntity
            (this GameObjectConversionSystem gcs, GameObject mainGameObject)//, Transform topBone)
        {

            //var postureEntity = createPostureEntity_(gcs, mainGameObject);
            var postureEntity = addComponentsPostureEntity_(gcs, mainGameObject);

            setPostureValue(gcs, postureEntity);//, topBone);

            return;


            //static Entity createPostureEntity_(GameObjectConversionSystem gcs, GameObject main)
            Entity addComponentsPostureEntity_(GameObjectConversionSystem gcs, GameObject main)
            {
                var ent = gcs.GetPrimaryEntity(main);

                //var addtypes = gcs.DstEntityManager.CreateArchetype
                var addtypes = new ComponentTypes
                (
                    //typeof( Posture.NeedTransformTag ),
                    //typeof( Posture.LinkData ),
                    typeof(Translation),
                    typeof(Rotation)
                );
                gcs.DstEntityManager.AddComponents(ent, addtypes);
                //var ent = gcs.CreateAdditionalEntity(main, addtypes);

                return ent;
            }

            static void setPostureValue(GameObjectConversionSystem gcs, Entity postureEntity)//, Transform tfbone_)
            {
                var em = gcs.DstEntityManager;

                //var boneEntity = gcs_.GetPrimaryEntity(tfbone_);

                //em.SetComponentData( postureEntity, new Posture.LinkData { BoneRelationTop = boneTopEntity } );
                em.SetComponentData(postureEntity, new Rotation { Value = quaternion.identity });
                em.SetComponentData(postureEntity, new Translation { Value = float3.zero });
            }
        }



    }
}