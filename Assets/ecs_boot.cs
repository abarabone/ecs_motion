using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

using Abss.Motion;

public class ecs_boot : MonoBehaviour
{

    [SerializeField]
    MotionClip MotionClip;

    void Awake()
    {
        World.Active = new World( "main world" );

        var psg = World.Active.GetOrCreateSystem<PresentationSystemGroup>();

        var mg = World.Active.CreateSystem<MotionSystemGroup>();
        psg.AddSystemToUpdateList( mg );

        var ccs = World.Active.CreateSystem<ChCreationSystem>();
        mg.AddSystemToUpdateList( ccs );

        psg.SortSystemUpdateList();

        ScriptBehaviourUpdateOrder.UpdatePlayerLoop( World.Active );


        ChCreationSystem.md = new MotionDataInNative();
        ChCreationSystem.md.ConvertFrom( this.MotionClip );
        
    }

    void OnDestroy()
    {
        World.DisposeAllWorlds();
        WordStorage.Instance.Dispose();
        WordStorage.Instance = null;
        ScriptBehaviourUpdateOrder.UpdatePlayerLoop( null );
    }
}
