using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Collections;
using System;
using System.IO;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using System.Runtime.CompilerServices;

namespace Abarabone.Utilities
{

    public interface ITypedEntity<T>
        where T : struct, IComponentData
    {
        Entity Entity { get; set; }
    }

    public interface ITypedEntity<T0, T1>
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
    {
        Entity Entity { get; set; }
    }

    public interface ITypedEntity<T0, T1, T2>
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
    {
        Entity Entity { get; set; }
    }

    public interface ITypedEntity<T0, T1, T2, T3>
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            where T3 : struct, IComponentData
    {
        Entity Entity { get; set; }
    }

    public interface ITypedEntity<T0, T1, T2, T3, T4>
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            where T3 : struct, IComponentData
            where T4 : struct, IComponentData
    {
        Entity Entity { get; set; }
    }


    static public class EntityExtension
    {

        static public T Get<T>
            ( this ComponentDataFromEntity<T> componentDataFromEntity, ITypedEntity<T> entity )
            where T : struct, IComponentData
            => componentDataFromEntity[ entity.Entity ];

        static public void Set<T>
            ( this ComponentDataFromEntity<T> componentDataFromEntity, ITypedEntity<T> entity, T componentData )
            where T : struct, IComponentData
            => componentDataFromEntity[ entity.Entity ] = componentData;



        static public T0 Get<T0, T1>
            ( this ComponentDataFromEntity<T0> componentDataFromEntity, ITypedEntity<T0, T1> entity )
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            => componentDataFromEntity[ entity.Entity ];

        static public void Set<T0, T1>
            ( this ComponentDataFromEntity<T0> componentDataFromEntity, ITypedEntity<T0, T1> entity, T0 componentData )
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            => componentDataFromEntity[ entity.Entity ] = componentData;

        static public T1 Get<T0, T1>
            ( this ComponentDataFromEntity<T1> componentDataFromEntity, ITypedEntity<T0, T1> entity )
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            => componentDataFromEntity[ entity.Entity ];

        static public void Set<T0, T1>
            ( this ComponentDataFromEntity<T1> componentDataFromEntity, ITypedEntity<T0, T1> entity, T1 componentData )
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            => componentDataFromEntity[ entity.Entity ] = componentData;


        static public T0 Get<T0, T1, T2>
            ( this ComponentDataFromEntity<T0> componentDataFromEntity, ITypedEntity<T0, T1, T2> entity )
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            => componentDataFromEntity[ entity.Entity ];

        static public void Set<T0, T1, T2>
            ( this ComponentDataFromEntity<T0> componentDataFromEntity, ITypedEntity<T0, T1, T2> entity, T0 componentData )
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            => componentDataFromEntity[ entity.Entity ] = componentData;

        static public T1 Get<T0, T1, T2>
            ( this ComponentDataFromEntity<T1> componentDataFromEntity, ITypedEntity<T0, T1, T2> entity )
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            => componentDataFromEntity[ entity.Entity ];

        static public void Set<T0, T1, T2>
            ( this ComponentDataFromEntity<T1> componentDataFromEntity, ITypedEntity<T0, T1, T2> entity, T1 componentData )
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            => componentDataFromEntity[ entity.Entity ] = componentData;

        static public T2 Get<T0, T1, T2>
            ( this ComponentDataFromEntity<T2> componentDataFromEntity, ITypedEntity<T0, T1, T2> entity )
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            => componentDataFromEntity[ entity.Entity ];

        static public void Set<T0, T1, T2>
            ( this ComponentDataFromEntity<T2> componentDataFromEntity, ITypedEntity<T0, T1, T2> entity, T2 componentData )
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            => componentDataFromEntity[ entity.Entity ] = componentData;


        static public T0 Get<T0, T1, T2, T3>
            ( this ComponentDataFromEntity<T0> componentDataFromEntity, ITypedEntity<T0, T1, T2, T3> entity )
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            where T3 : struct, IComponentData
            => componentDataFromEntity[ entity.Entity ];

        static public void Set<T0, T1, T2, T3>
            ( this ComponentDataFromEntity<T0> componentDataFromEntity, ITypedEntity<T0, T1, T2, T3> entity, T0 componentData )
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            where T3 : struct, IComponentData
            => componentDataFromEntity[ entity.Entity ] = componentData;

        static public T1 Get<T0, T1, T2, T3>
            ( this ComponentDataFromEntity<T1> componentDataFromEntity, ITypedEntity<T0, T1, T2, T3> entity )
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            where T3 : struct, IComponentData
            => componentDataFromEntity[ entity.Entity ];

        static public void Set<T0, T1, T2, T3>
            ( this ComponentDataFromEntity<T1> componentDataFromEntity, ITypedEntity<T0, T1, T2, T3> entity, T1 componentData )
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            where T3 : struct, IComponentData
            => componentDataFromEntity[ entity.Entity ] = componentData;

        static public T2 Get<T0, T1, T2, T3>
            ( this ComponentDataFromEntity<T2> componentDataFromEntity, ITypedEntity<T0, T1, T2, T3> entity )
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            where T3 : struct, IComponentData
            => componentDataFromEntity[ entity.Entity ];

        static public void Set<T0, T1, T2, T3>
            ( this ComponentDataFromEntity<T2> componentDataFromEntity, ITypedEntity<T0, T1, T2, T3> entity, T2 componentData )
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            where T3 : struct, IComponentData
            => componentDataFromEntity[ entity.Entity ] = componentData;

        static public T3 Get<T0, T1, T2, T3>
            ( this ComponentDataFromEntity<T3> componentDataFromEntity, ITypedEntity<T0, T1, T2, T3> entity )
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            where T3 : struct, IComponentData
            => componentDataFromEntity[ entity.Entity ];

        static public void Set<T0, T1, T2, T3>
            ( this ComponentDataFromEntity<T3> componentDataFromEntity, ITypedEntity<T0, T1, T2, T3> entity, T3 componentData )
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            where T3 : struct, IComponentData
            => componentDataFromEntity[ entity.Entity ] = componentData;


        static public T0 Get<T0, T1, T2, T3, T4>
            ( this ComponentDataFromEntity<T0> componentDataFromEntity, ITypedEntity<T0, T1, T2, T3, T4> entity )
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            where T3 : struct, IComponentData
            where T4 : struct, IComponentData
            => componentDataFromEntity[ entity.Entity ];

        static public void Set<T0, T1, T2, T3, T4>
            ( this ComponentDataFromEntity<T0> componentDataFromEntity, ITypedEntity<T0, T1, T2, T3, T4> entity, T0 componentData )
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            where T3 : struct, IComponentData
            where T4 : struct, IComponentData
            => componentDataFromEntity[ entity.Entity ] = componentData;

        static public T1 Get<T0, T1, T2, T3, T4>
            ( this ComponentDataFromEntity<T1> componentDataFromEntity, ITypedEntity<T0, T1, T2, T3, T4> entity )
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            where T3 : struct, IComponentData
            where T4 : struct, IComponentData
            => componentDataFromEntity[ entity.Entity ];

        static public void Set<T0, T1, T2, T3, T4>
            ( this ComponentDataFromEntity<T1> componentDataFromEntity, ITypedEntity<T0, T1, T2, T3, T4> entity, T1 componentData )
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            where T3 : struct, IComponentData
            where T4 : struct, IComponentData
            => componentDataFromEntity[ entity.Entity ] = componentData;

        static public T2 Get<T0, T1, T2, T3, T4>
            ( this ComponentDataFromEntity<T2> componentDataFromEntity, ITypedEntity<T0, T1, T2, T3, T4> entity )
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            where T3 : struct, IComponentData
            where T4 : struct, IComponentData
            => componentDataFromEntity[ entity.Entity ];

        static public void Set<T0, T1, T2, T3, T4>
            ( this ComponentDataFromEntity<T2> componentDataFromEntity, ITypedEntity<T0, T1, T2, T3, T4> entity, T2 componentData )
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            where T3 : struct, IComponentData
            where T4 : struct, IComponentData
            => componentDataFromEntity[ entity.Entity ] = componentData;

        static public T3 Get<T0, T1, T2, T3, T4>
            ( this ComponentDataFromEntity<T3> componentDataFromEntity, ITypedEntity<T0, T1, T2, T3, T4> entity )
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            where T3 : struct, IComponentData
            where T4 : struct, IComponentData
            => componentDataFromEntity[ entity.Entity ];

        static public void Set<T0, T1, T2, T3, T4>
            ( this ComponentDataFromEntity<T3> componentDataFromEntity, ITypedEntity<T0, T1, T2, T3, T4> entity, T3 componentData )
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            where T3 : struct, IComponentData
            where T4 : struct, IComponentData
            => componentDataFromEntity[ entity.Entity ] = componentData;

        static public T4 Get<T0, T1, T2, T3, T4>
            ( this ComponentDataFromEntity<T4> componentDataFromEntity, ITypedEntity<T0, T1, T2, T3, T4> entity )
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            where T3 : struct, IComponentData
            where T4 : struct, IComponentData
            => componentDataFromEntity[ entity.Entity ];

        static public void Set<T0, T1, T2, T3, T4>
            ( this ComponentDataFromEntity<T4> componentDataFromEntity, ITypedEntity<T0, T1, T2, T3, T4> entity, T4 componentData )
            where T0 : struct, IComponentData
            where T1 : struct, IComponentData
            where T2 : struct, IComponentData
            where T3 : struct, IComponentData
            where T4 : struct, IComponentData
            => componentDataFromEntity[ entity.Entity ] = componentData;

    }

}
