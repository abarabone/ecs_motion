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


    [UpdateAfter( typeof( PhysicsBodyConversionSystem ) )]
    [UpdateAfter( typeof( LegacyRigidbodyConversionSystem ) )]
    [UpdateAfter( typeof( LegacyJointConversionSystem ) )]
    public class ModelConversionSystem : GameObjectConversionSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            Debug.Log( GameObjectConversionSettings.FromWorld( this.World, null ). );
        }

        protected override void OnUpdate()
        {
            //Entities.ForEach( ( ModelGroupAuthoring behaviour ) =>
            //    { behaviour.Convert( this.GetPrimaryEntity( behaviour ), this.DstEntityManager, this ); } );

            //Entities.ForEach( ( CharacterModelAuthoring behaviour ) =>
            //    { behaviour.Convert( this.GetPrimaryEntity( behaviour ), this.DstEntityManager, this ); } );

            //Entities.ForEach( ( PsylliumAuthoring behaviour ) =>
            //    { behaviour.Convert( this.GetPrimaryEntity( behaviour ), this.DstEntityManager, this ); } );
        }
    }
    
}