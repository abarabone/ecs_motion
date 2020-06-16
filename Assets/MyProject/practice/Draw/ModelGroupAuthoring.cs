using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

namespace Abarabone.Model.Authoring
{
    using Draw;
    using Utilities;


    public class ModelGroupAuthoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {

        public class ModelAuthoringBase : MonoBehaviour
        { }

        public ModelAuthoringBase[] ModelPrefabs;



        void IDeclareReferencedPrefabs.DeclareReferencedPrefabs( List<GameObject> referencedPrefabs )
        {
            referencedPrefabs.AddRange( this.ModelPrefabs.Select(x => x.gameObject) );
        }


        public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
        {

            dstManager.DestroyEntity( entity );


            prefabEntities = this.ModelPrefabs
                .Select( x => conversionSystem.GetPrimaryEntity( x.gameObject ) )
                .ToArray();

        }

        IEnumerable<Entity> prefabEntities;
        void OnDestroy()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            this.prefabEntities
                .Where( x => em.HasComponent<ModelPrefabNoNeedLinkedEntityGroupTag>(x) )
                .ForEach( x => em.RemoveComponent<LinkedEntityGroup>(x) );
        }

    }


    static public class ModelAuthoringExtension
    {

        static public Entity CreateDrawModelEntityComponents
            ( this ModelGroupAuthoring.ModelAuthoringBase author,
                EntityManager em, Mesh mesh, Material mat, BoneType boneType, int boneLength )
        {

            var sys = em.World.GetExistingSystem<DrawBufferManagementSystem>();
            var boneVectorBuffer = sys.GetSingleton<DrawSystemComputeTransformBufferData>().Transforms;
            setShaderProps_( mat, mesh, boneVectorBuffer, boneLength );

            return createEntityAndInitComponents_( boneLength );


            void setShaderProps_( Material mat_, Mesh mesh_, ComputeBuffer boneVectorBuffer_, int boneLength_ )
            {
                mat_.SetBuffer( "BoneVectorBuffer", boneVectorBuffer_ );
                mat_.SetInt( "BoneLengthEveryInstance", boneLength_ );
                //mat_.SetInt( "BoneVectorOffset", 0 );// 毎フレームのセットが必要
            }

            Entity createEntityAndInitComponents_( int boneLength_ )
            {
                var drawModelArchetype = em.CreateArchetype(
                    typeof( DrawModelBoneUnitSizeData ),
                    typeof( DrawModelInstanceCounterData ),
                    typeof( DrawModelInstanceOffsetData ),
                    typeof( DrawModelGeometryData ),
                    typeof( DrawModelComputeArgumentsBufferData )
                );
                var ent = em.CreateEntity( drawModelArchetype );

                em.SetComponentData( ent,
                    new DrawModelBoneUnitSizeData
                    {
                        BoneLength = boneLength_,
                        VectorLengthInBone = (int)boneType,
                    }
                );
                em.SetComponentData( ent,
                    new DrawModelGeometryData
                    {
                        Mesh = mesh,
                        Material = mat,
                    }
                );
                em.SetComponentData( ent,
                    new DrawModelComputeArgumentsBufferData
                    {
                        InstanceArgumentsBuffer = ComputeShaderUtility.CreateIndirectArgumentsBuffer(),
                    }
                );

                return ent;
            }

        }
    }
}
