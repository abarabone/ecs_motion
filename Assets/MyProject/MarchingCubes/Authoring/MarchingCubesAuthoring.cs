using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections.LowLevel.Unsafe;

namespace Abarabone.MarchingCubes.Authoring
{
    using Abarabone.Draw;
    using Abarabone.Model;

    public class MarchingCubesAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        public int MaxCubeInstances;
        //public int MaxDrawGridLength;

        public MarchingCubeAsset MarchingCubesAsset;
        public Material SrcMaterial;
        public ComputeShader GridCubeIdSetShader;
        public int maxGridLengthInShader;

        public int BlankFreeStockCapacity;
        public int SolidFreeStockCapacity;


        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {


        }
    }
}