using UnityEngine;
using System.Collections;

public class TerrainTreeHit3 : _Hit3
{

	static TerrainFieldHolder3	holder;
	
	static TerrainData		td;

	static TreePrototype[]	properties;

//	static TreeInstance[]	trees;
	

	static MaterialPropertyBlock	mpb;

	static int	HashIdColor;


	static bool	updatedInFrame;


	public int	id	{ get; protected set; }	// 破壊後は -1 になる


	// テレインコライダに hitprocess をもたせて、res.collider で識別とかしたほうがいいのかもしれないな、ツリー複製しないですんで
	// って思うじゃん？　でも実際やると、テレインコライダしかかえってこないので識別できないんだわこれが


	static public void staticInit( TerrainFieldHolder3 inholder, TerrainData intd, TreePrototype[] props, TreeInstance[] insts, Vector3 ofs )
	{

		holder = inholder;

		td = intd;

		properties = props;

	//	trees = insts;
		
		mpb = new MaterialPropertyBlock();

		HashIdColor	= Shader.PropertyToID( "_Color" );

	}

	public void init( int inid )
	{

		id = inid;

	}
	
	public override void blasted( ref _Bullet3.DamageSourceUnit ds, float pressure, float boringFactor, Vector3 center, float radius, Collider collider, _Action3 attacker )
	//public override void blasted( float damage, float pressure, float boringFactor, Vector3 center, float radius, Collider collider, int teamFlag )
	{
		
		var direction = ( transform.position - center );//.normalized;
		
		fallDownTree( pressure * 0.005f * direction, center );

	}

	public override void shot( ref _Bullet3.DamageSourceUnit ds, Vector3 force, ref RaycastHit hit, _Action3 attacker )
	//public override void shot( float damage, float pressure, Vector3 direction, Vector3 point, int hitIndex, Collider collider, int teamFlag )
	{

		if( ds.damage > 0.0f )
		{

			fallDownTree( force * 0.01f, hit.point );

		}

	}

	void fallDownTree( Vector3 force, Vector3 point )
	{

		if( id != -1 )// 同一フレーム内重複破壊の回避
		{

			var tree = td.GetTreeInstance( id );// unity5 からの機能
			

			var prefab = properties[ tree.prototypeIndex ].prefab;

			var brokenTree = Instantiate<GameObject>
				( prefab, transform.position, Quaternion.AngleAxis( tree.rotation, Vector3.up ) );

			brokenTree.transform.localScale = new Vector3( tree.widthScale, tree.heightScale, tree.widthScale );


			mpb.SetColor( HashIdColor, tree.color );

			brokenTree.GetComponent<Renderer>().SetPropertyBlock( mpb );

			mpb.Clear();//

			
			var rb = brokenTree.GetComponent<Rigidbody>();
			
			if( rb == null ) rb = brokenTree.AddComponent<Rigidbody>();
			
			rb.AddForceAtPosition( force, point, ForceMode.Impulse );

			rb.detectCollisions = false;


			//brokenTree.GetComponent<Collider>().enabled = false;
			
			Destroy( brokenTree, 2.0f );
			
			Destroy( gameObject );



			tree.heightScale	= 0.0f;

			tree.widthScale		= 0.0f;
			
			td.SetTreeInstance( id, tree );// unity5 からの機能
			
			id = -1;

		}

	}
	
}
