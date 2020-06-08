using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Burst;
using Unity.Deformations;
using Unity.Physics;
using Unity.Jobs;

public class PracSpawnSystem : ComponentSystem
{

    protected override void OnStartRunning()
    {

    }
    
    protected override void OnUpdate()
    {
        this.Entities.ForEach( ( ref wotim  ref SpawnData spawn ) => this.EntityManager.Instantiate( spawn.ent ) );
    }
    
}
