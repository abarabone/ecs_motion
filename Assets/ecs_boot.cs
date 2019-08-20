using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class ecs_boot : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		var w = World.Active;
		ScriptBehaviourUpdateOrder.UpdatePlayerLoop( w );

		var a = w.EntityManager.CreateArchetype( typeof(Abss.Motion.BonePostureData) );
		var e = w.EntityManager.CreateEntity( a );
		w.EntityManager.AddComponent<Abss.Motion.BonePostureData>( e );
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
