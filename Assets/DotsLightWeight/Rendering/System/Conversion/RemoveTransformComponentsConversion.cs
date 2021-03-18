using Unity.Entities;
using UnityEngine;
using Unity.Physics.Authoring;
using Unity.Transforms;
using Unity.Entities.Conversion;
using Unity.Entities.Hybrid;
using System.Linq;

namespace Abarabone.Model.Authoring
{

    //[DisableAutoCreation]
    //[UpdateInGroup(typeof( GameObjectDeclareReferencedObjectsGroup ) )]
    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    [UpdateBefore(typeof(TrimBlankEntityFromLinkedEntityGroupSystem))]
    public class RemoveTransformComponentsConversion : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            var em = this.DstEntityManager;

            this.Entities.ForEach
            (
                (Entity e, RemoveTransformComponentsAuthoring c) =>
                {
                    foreach (var tf in c.GetComponentsInChildren<Transform>())
                    {
                        Debug.Log(tf.name);
                        var ent = this.GetPrimaryEntity(tf);

                        em.RemoveComponent<LocalToParent>(ent);
                        em.RemoveComponent<LocalToWorld>(ent);
                        em.RemoveComponent<PreviousParent>(ent);
                        em.RemoveComponent<Parent>(ent);
                        em.RemoveComponent<Child>(ent);

                    }
                }
            );

        }
    }

}
