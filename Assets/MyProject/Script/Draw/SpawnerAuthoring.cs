using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

namespace Abarabone.Draw.Arthuring
{
    [DisallowMultipleComponent]
    class SpawnerAuthoring : MonoBehaviour
    {


        public interface IModelAuthoring
        {
            GameObject GameObject { get; }
        }


        public IModelAuthoring[] ModelPrefabs;

        public Shader Shader = null;

        public int MaxInstanceForComputeBuffer = 1000;
        
        //public BoneType BoneType = BoneType.TR;




    }
}