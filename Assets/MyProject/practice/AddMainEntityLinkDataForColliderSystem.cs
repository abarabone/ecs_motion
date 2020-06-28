using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using Unity.Physics.Authoring;
using Unity.Transforms;
using Unity.Entities.Conversion;
using Unity.Entities.Hybrid;
using System.Linq;
using Abarabone.Motion;
using Unity.Physics;

namespace Abarabone.Model.Authoring
{

    //[DisableAutoCreation]
    public class AddMainEntityLinkDataForColliderSystem : GameObjectConversionSystem
    {
        protected override void OnDestroy()
        {
            var em = this.DstEntityManager;

            using( var q = em.CreateEntityQuery(
                typeof( BoneRelationLinkData ),
                typeof( PhysicsCollider ),
                typeof( Prefab ) )
            )
            using( var ents = q.ToEntityArray( Allocator.TempJob ) )
            {
                foreach( var ent in ents )
                {

                }
            }

            base.OnDestroy();
        }

        protected override void OnUpdate()
        { }
    }

}