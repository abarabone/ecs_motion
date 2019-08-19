using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

using UniRx;

using Abss.Motion;
using Abss.Geometry;
using Abss.Draw;

namespace Abss.Ecs
{

	public sealed class EcsStarter : MonoBehaviour
	{

		public World	MainWorld		{ get; private set; }
		public World	EntityWorld		{ get; private set; }

		private void Awake()
		{
			createWorlds();
			//for( var i=0; i<5000; i++ )
			//{
			//	World.Active.GetOrCreateManager<EntityManager>().CreateEntity( typeof( MotionInfoData ), typeof( Draw.DrawTargetSphere ) );
			//	World.Active.GetOrCreateManager<EntityManager>().CreateEntity( typeof( MotionInfoData ), typeof( Draw.DrawTargetAabb ) );
			//}
		}
		void createWorlds()
		{
			this.MainWorld		= new World( "main world" );
			this.EntityWorld	= new World( "entity world" );

			World.Active = this.MainWorld;

			new UniRx.CompositeDisposable( new[] { this.MainWorld, this.EntityWorld } )
				.AddTo( this );
		}

		private void Start()
		{
			ScriptBehaviourUpdateOrder.UpdatePlayerLoop( this.MainWorld );
		}
		
		// -------------------------------------

		
		/// <summary>
		/// システムの登録をコンポーネントの enable/disable によって制御させる。
		/// </summary>
		/// <typeparam name="TSystem">登録したい ComponentSystem</typeparam>
		[DefaultExecutionOrder(2)]
		public abstract class _SystemRegistMonoBehaviour<TSystem> : MonoBehaviour
			where TSystem:ComponentSystemBase
		{
			public World MainWorld
			{
				get
				{
					var s = this.GetComponentInParent<EcsStarter>();
					if( s == null ) return default(World);
					
					return s.MainWorld;
				}
			}
			
			protected void Awake()
			{
				var w	= this.MainWorld;
				if( w != null ) w.GetOrCreateSystem<TSystem>().Enabled = false;
			}
			protected void OnEnable()
			{
				var w	= this.MainWorld;
				if( w != null ) w.GetExistingSystem<TSystem>().Enabled = true;
			}
			protected void OnDisable()
			{
				var w	= this.MainWorld;
				if( w != null ) w.GetExistingSystem<TSystem>().Enabled = false;
			}
		}

		/// <summary>
		/// システムのコンストラクタに this を渡したいバージョン
		/// コンストラクタを必要とするシステムは、他のシステムから //[Inject] はできない。
		/// （ CreateManager() のコンストラクタ呼び出しより先に //[Inject] が実行されてしまう場合、依存関係で問題になる）
		/// </summary>
		/// <typeparam name="TSystem">登録したい ComponentSystem</typeparam>
		[DefaultExecutionOrder(1)]
		public abstract class _SystemRegistMonoBehaviourWithParameter<TSystem> : _SystemRegistMonoBehaviour<TSystem>
			where TSystem:ComponentSystemBase
		{
			protected new void Awake()
			{
				var w = this.MainWorld;
				if( w != null ) w.CreateSystem<TSystem>( this ).Enabled = false;
			}
		}
		

		
		// -------------------------------------
		
		void transact( World world )
		{
			
			var em	= world.GetOrCreateSystem<EntityManager>();
			world.AddTo( this );

			em.CreateEntity( typeof(DummyData) );


			var ent	= em.CreateEntity( typeof(DummyData) );

			var commands	= em.BeginExclusiveEntityTransaction();
			


		}
		

		public struct DummyData : IComponentData { }
	}
}

