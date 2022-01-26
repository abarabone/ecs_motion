using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace DotsLite.Model.Authoring
{
    using CharacterMotion;
    using Draw;
    using Character;

    using DotsLite.Common.Extension;
    using DotsLite.Misc;
    using DotsLite.Utilities;
    using Unity.Physics;
    using Unity.Physics.Authoring;
    using DotsLite.EntityTrimmer.Authoring;

    static public class PostureEntitiesConvertUtility
    {


        static public void InitPostureEntity
            (this GameObjectConversionSystem gcs, PostureAuthoring posture)//, Transform topBone)
        {

            //var postureEntity = createPostureEntity_(gcs, mainGameObject);
            var postureEntity = addComponentsPostureEntity_(gcs, posture);

            setPostureValue(gcs, postureEntity, posture);//, topBone);

            return;


            //static Entity createPostureEntity_(GameObjectConversionSystem gcs, GameObject main)
            Entity addComponentsPostureEntity_(GameObjectConversionSystem gcs, PostureAuthoring postureObject)
            {
                var em = gcs.DstEntityManager;
                var ent = gcs.GetPrimaryEntity(postureObject);

                //var addtypes = gcs.DstEntityManager.CreateArchetype
                var addtypes = new ComponentTypes
                (
                    //typeof( Posture.NeedTransformTag ),
                    //typeof( Posture.LinkData ),
                    typeof(AddTransformConversion.Translation),
                    typeof(AddTransformConversion.Rotation)
                );
                em.AddComponents(ent, addtypes);
                //var ent = gcs.CreateAdditionalEntity(main, addtypes);

                em.SetName_(ent, $"{postureObject.transform.parent.name} posture");
                return ent;
            }

            static void setPostureValue
                (GameObjectConversionSystem gcs, Entity postureEntity, PostureAuthoring postureObject)//, Transform tfbone_)
            {
                var em = gcs.DstEntityManager;

                //var boneEntity = gcs_.GetPrimaryEntity(tfbone_);
                //em.SetComponentData( postureEntity, new Posture.LinkData { BoneRelationTop = boneTopEntity } );

                var tf = postureObject.transform;
                em.SetComponentData(postureEntity, new Rotation { Value = tf.rotation });
                em.SetComponentData(postureEntity, new Translation { Value = tf.position });
            }
        }



    }
}