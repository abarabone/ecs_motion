using System.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

public class ecs : MonoBehaviour
{
    World myWorld;
    EntityManager myEntityManager;

    private void Awake()
    {
        World.Active = new World("def");
        // WorldとWorld用EntityManagerを作成
        // 初期世界以外は最初から全てのシステムが登録されていない
        myWorld = new World("my world");
		//myEntityManager = myWorld.GetOrCreateManager<EntityManager>();
		myEntityManager = myWorld.EntityManager;
        Debug.Log("a");
	}

    private void OnDestroy()
    {
        //// worldの停止は非常に高い負荷になる
        World.Active.Dispose();
        myWorld.Dispose();
        Debug.Log("a");
    }

    IEnumerator Start ()
    {
        // 事前にEntityを一つ作っておく。コレがないとワールド移行時にすごい負荷になる模様
        var e = myEntityManager.CreateEntity( typeof( DummyData ) );
        World.Active.EntityManager.MoveEntitiesFrom( out var ee, myEntityManager );
        //var ee = World.Active.EntityManager.CreateEntity( typeof( DummyData ) );
        // 2019/8/26でもなるみたい
        //myEntityManager.DestroyEntity( e );             // 消しても問題ないみたい
        World.Active.EntityManager.DestroyEntity( ee ); // 消しても問題ないみたい
        ee.Dispose();


        // マウスクリックでロードを開始
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));  

        // Entityを事前に作成しておく。
        // 排他モードになるとEntityManagerが使えなくなるので、作るものは事前に作成
        var arch    = myEntityManager.CreateArchetype( typeof(DummyData) );
        //var entity = myEntityManager.CreateEntity( arch );
        //myEntityManager.DestroyEntity( entity );

        // 排他モードを開始してジョブを実行する
        var commands = myEntityManager.BeginExclusiveEntityTransaction();
        myEntityManager.ExclusiveEntityTransactionDependency = new CreateEntityJob()
        {
            commands = commands,//.Schedule(,
            //entity = entity,
            arch = arch,
            count = 38000
        }.Schedule( myEntityManager.ExclusiveEntityTransactionDependency );
        //}.Schedule( 38000, 100, myEntityManager.ExclusiveEntityTransactionDependency );
        JobHandle.ScheduleBatchedJobs();

        // 処理が完了するまで待つ
        yield return new WaitUntil(() => myEntityManager.ExclusiveEntityTransactionDependency.IsCompleted);

        // 処理が完了したら排他モードを停止。
        // 現在アクティブなワールドへ、作ったEntityを全て移動する（個別に移動する機能や、コピーする機能はまだない）
        myEntityManager.EndExclusiveEntityTransaction();
        World.Active.EntityManager.MoveEntitiesFrom(myEntityManager);
    }
}

/// <summary>
/// 任意のEntityをCount個作成するジョブ
/// </summary>
//[Unity.Burst.BurstCompile] できないみたい
struct CreateEntityJob : IJob//ParallelFor
{
    public ExclusiveEntityTransaction commands; // コマンドを実行するEntityManagerのExclusiveEntityTransaction
    public Entity entity;                       // 生成するEntity。複数種類作りたいならジョブを繋げる
    public int count;
    public EntityArchetype arch;

    public void Execute()
    {
        for (int i = 0; i < count; i++)
        {
            //var e = commands.Instantiate( entity );
            var e = commands.CreateEntity( arch );
            var c = new DummyData() { value = i };
            commands.SetComponentData(e, c);
        }
    }

    public void Execute( int i )
    {
        var e = commands.Instantiate(entity);
        var c = new DummyData() { value = i };
        commands.SetComponentData(e, c);
    }
}

struct DummyData : IComponentData
{
    public int value;
}
