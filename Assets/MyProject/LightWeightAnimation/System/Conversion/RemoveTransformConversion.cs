using Unity.Entities;
using UnityEngine;
using Unity.Physics.Authoring;
using Unity.Transforms;
using Unity.Entities.Conversion;
using Unity.Entities.Hybrid;
using System.Linq;

namespace Abarabone.Model.Authoring
{
    using Particle.Aurthoring;


    [DisableAutoCreation]
    //[UpdateInGroup(typeof( GameObjectDeclareReferencedObjectsGroup ) )]
    public class RemoveTransformConversion : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {

            this.Entities.ForEach
            (
                (Entity ent, Transform c) =>
                {
                    Debug.Log( this.EntityManager.HasComponent<Transform>( ent ) );
                    Debug.Log(this.EntityManager.GetName(ent));
                    this.EntityManager.RemoveComponent<Transform>( ent );
                }
            );

        }
    }

}