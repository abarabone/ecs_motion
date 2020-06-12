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

    
    public class DisableTransformConverion : GameObjectConversionSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            foreach( var x in this.World.Systems )
            {
                
                if( x.ToString() == "TransformConversion")
                {
                    x.Enabled = false;
                    
                    break;
                }
                
            }
        }

        protected override void OnUpdate()
        { }
    }
    
}