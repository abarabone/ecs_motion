using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

namespace Abarabone.Model.Arthuring
{
    [DisallowMultipleComponent]
    class SpawnerAuthoring : MonoBehaviour
    {



        public IModelAuthoring[] ModelPrefabs;

        public Shader Shader = null;

        public int MaxInstanceForComputeBuffer = 1000;
        
        //public BoneType BoneType = BoneType.TR;




    }
}