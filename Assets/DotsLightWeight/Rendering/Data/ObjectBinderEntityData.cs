using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

namespace DotsLite.Model
{


    static public partial class ObjectBinder
    {

        public struct MainEntityLinkData : IComponentData
        {
            public Entity MainEntity;
        }

    }


}
