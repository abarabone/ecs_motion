using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using Unity.Physics.Authoring;
using Unity.Transforms;
using Unity.Entities.Conversion;
using Unity.Entities.Hybrid;
using System.Linq;

namespace Abarabone.Model.Authoring
{

    //[DisableAutoCreation]
    public class BlankEntityCleanUpSystem : GameObjectConversionSystem
    {
        protected override void OnDestroy()
        {
            var em = this.DstEntityManager;//World.DefaultGameObjectInjectionWorld.EntityManager;
            var links = new NativeList<LinkedEntityGroup>( Allocator.Temp );

            using( var q = em.CreateEntityQuery(
                typeof( ModelBinderLinkData ),
                typeof( LinkedEntityGroup ),
                typeof( Prefab ) )
            )
            using( var ents = q.ToEntityArray( Allocator.TempJob ) )
            {
                foreach( var ent in ents )
                {
                    var buf = em.GetBuffer<LinkedEntityGroup>( ent );

                    //foreach( var link in buf )
                    for( var i = 0; i < buf.Length; i++ )
                    {
                        var link = buf[ i ];

                        if( em.GetComponentCount(link.Value) == 1 && em.HasComponent<Prefab>(link.Value) )
                        {
                            //em.DestroyEntity( link.Value );
                        }
                        else
                        {
                            links.Add( link );
                        }
                    }

                    if( links.Length == 0 ) continue;

                    buf.Clear();
                    buf.AddRange( links.AsArray() );
                    links.Clear();
                }
            }

            links.Dispose();
            this.Enabled = false;

            base.OnDestroy();
        }

        protected override void OnUpdate()
        { }
    }

}