using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

using Abss.Motion;

public class ecs_boot : MonoBehaviour
{
    
    
    void Start()
    {
        World.Active = new World("main world");
        ScriptBehaviourUpdateOrder.UpdatePlayerLoop( World.Active );
        World.Active.CreateSystem<ChCreationSystem>();

    }

    void OnDestroy()
    {
        World.Active.Dispose();
    }
}
