using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Properties;
using Unity.Burst;

public class FPSCounter : MonoBehaviour
{
    [SerializeField]
    private float m_updateInterval = 0.5f;

	private float m_accum;
	private int m_frames;
	private float m_timeleft;
	[HideInInspector]
	public float m_fps;

	//World	w;

	//void Start()
	//{
	//	w = new World("a");
	//	w.CreateManager<FPSCounterSystem>( m_updateInterval, this );
	//	ScriptBehaviourUpdateOrder.UpdatePlayerLoop( World.Active, w );
	//}
	//private void OnDestroy()
	//{
	//	if( w != null && w.IsCreated ) w.Dispose();
	//}

	private void Update()
	{
		m_timeleft -= Time.deltaTime;
		m_accum += Time.timeScale / Time.deltaTime;
		m_frames++;

		if( 0 < m_timeleft ) return;

		m_fps = m_accum / m_frames;
		m_timeleft = m_updateInterval;
		m_accum = 0;
		m_frames = 0;
	}

	private void OnGUI()
    {
        GUILayout.Label( "FPS: " + m_fps.ToString( "f2" ) );
    }
}

[DisableAutoCreation]
public class FPSCounterSystem : ComponentSystem
{
    [SerializeField]
    private float m_updateInterval = 0.5f;

    private float m_accum;
    private int m_frames;
    private float m_timeleft;
    private float m_fps;

	FPSCounter	script;

	public FPSCounterSystem( float interval, FPSCounter script )
	{
		this.script				= script;
		this.m_updateInterval	= interval;
	}

	protected override void OnUpdate()
    {
        m_timeleft -= Time.deltaTime;
        m_accum += Time.timeScale / Time.deltaTime;
        m_frames++;

        if ( 0 < m_timeleft ) return;

        m_fps = m_accum / m_frames;
        m_timeleft = m_updateInterval;
        m_accum = 0;
        m_frames = 0;

		this.script.m_fps	= this.m_fps;
		//Debug.Log( this.m_fps );
	}

}
