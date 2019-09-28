using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using UniRx;
//using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Properties;
using Unity.Burst;
using Abss.Geometry;
using System.Runtime.InteropServices;

namespace Abss.Motion
{

    //static public class ChEntityUtility
    //{
    //    static EntityArchetype CreateArchetype( EntityManager em ) => em.CreateArchetype
    //        ( typeof(MotionInfoData) );
        
    //    static public Entity Create( EntityCommandBuffer.Concurrent ecb, int jobindex, EntityArchetype archetype )
    //    {
    //        return ecb.CreateEntity( jobindex, archetype );
    //    }
    //    static public NativeArray<Entity> Create
    //        ( EntityCommandBuffer.Concurrent ecb, int jobindex, EntityArchetype archetype, int length )
    //    {ecb.CreateEntity()
    //        return ecb.CreateEntity( jobindex, archetype, )
    //    }
    //}

    //static public class MotionStreamEntityUtility
    //{
    //    static EntityArchetype CreateArchetype( EntityManager em ) => em.CreateArchetype
    //        ( typeof(StreamKeyShiftData), typeof(StreamTimeProgressData), typeof(StreamNearKeysCacheData) );

    //}
}
