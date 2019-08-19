using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Abss.Misc
{
	
	public class ChankIterationQuery : IDisposable
	{

		EntityQueryDesc		query;
		NativeList<EntityArchetype> foundArchetypes;

	

		public struct EntityArchetypeQueryCaster
		{
			public ComponentType[] Any;
			public ComponentType[] None;
			public ComponentType[] All;

			public static implicit operator EntityQueryDesc( EntityArchetypeQueryCaster query )
				=>	new EntityQueryDesc
					{
						Any  = query.Any  ?? System.Array.Empty<ComponentType>(),
						None = query.None ?? System.Array.Empty<ComponentType>(),
						All  = query.All  ?? System.Array.Empty<ComponentType>()
					};
		}

		

		public void InitOnCreate( EntityQueryDesc query )
		{
			this.query	= query;
			
			this.foundArchetypes = new NativeList<EntityArchetype>();
		}


		public NativeArray<ArchetypeChunk> CreateChunksOnUpdate<Talloctor>( EntityManager em )
			where Talloctor:IAllocatorLabel, new()
		{
			em.AddMatchingArchetypes( query, foundArchetypes );

			return em.CreateArchetypeChunkArray( query, new Talloctor().Label );
		}


		public NativeArray<T> GetChunkArray<T>( ComponentSystemBase system, in ArchetypeChunk chunk, bool isReadOnly )
			where T:struct, IComponentData
		{
			var chunkComponentType = system.GetArchetypeChunkComponentType<T>( isReadOnly );// ループ外に出したい
			
			return chunk.GetNativeArray<T>( chunkComponentType );
		}
		

		public void Dispose()
		{
			foundArchetypes.Dispose();
		}

	}
}

