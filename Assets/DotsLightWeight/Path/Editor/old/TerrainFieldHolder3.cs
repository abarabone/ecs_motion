using UnityEngine;
using System.Collections;

public class TerrainFieldHolder3 : MonoBehaviour
{

//	public Terrain			terrain		{ get; protected set; }

//	public Material	blankMaterial;


	public float	detailDensity;
	public float	detailDistance;

	
	TerrainField3[,]	splits;
	
	public int	splitLevel;
	
	
	
	public void split()
	{
		
		var terrain = GetComponent<Terrain>();
		
		var tf = transform;
		
		var td = terrain.terrainData;
		
		
		var dstNumNest	= 1 << splitLevel;
		
		splits	= new TerrainField3[ dstNumNest, dstNumNest ];
		
		var dstHmReso	= ( td.heightmapResolution & 0x7ffffffe ) >> splitLevel;
		var dstAmReso	= td.alphamapResolution >> splitLevel;
		var dstDmReso	= td.detailResolution >> splitLevel;
		
		var dstHmSize = Vector3.Scale( td.heightmapScale, new Vector3(dstHmReso,1.0f,dstHmReso) );
		
		
		for( var iy = 0; iy < dstNumNest; iy++ )
		for( var ix = 0; ix < dstNumNest; ix++ )
		{
			
			var igo = new GameObject( "heights " + iy + " : " + ix );
			var irb	= igo.AddComponent<Rigidbody>();
			var ifd	= igo.AddComponent<TerrainField3>();
			var itr = igo.AddComponent<Terrain>();
			var icd	= igo.AddComponent<TerrainCollider>();

			itr.enabled = false;
			irb.isKinematic = true;
			
			itr.editorRenderFlags = TerrainRenderFlags.Heightmap;// | TerrainRenderFlags.details;
			itr.drawTreesAndFoliage = false;

		//	itr.heightmapMaximumLOD		= 3;
			itr.heightmapMaximumLOD		= terrain.heightmapMaximumLOD;
			itr.heightmapPixelError		= 49;//terrain.heightmapPixelError;
			itr.basemapDistance			= terrain.basemapDistance;
			itr.castShadows				= terrain.castShadows;
		//	itr.lightmapIndex			= terrain.lightmapIndex;
	//		itr.treeBillboardDistance	= terrain.treeBillboardDistance;
	//		itr.treeCrossFadeLength		= terrain.treeCrossFadeLength;
	//		itr.treeDistance			= terrain.treeDistance;
	//		itr.treeMaximumFullLODCount	= terrain.treeMaximumFullLODCount;
			itr.detailObjectDensity		= detailDensity;//terrain.detailObjectDensity;
			itr.detailObjectDistance	= detailDistance;//terrain.detailObjectDistance;

			
			var itd = new TerrainData();
			itd.heightmapResolution	= dstHmReso + 1;
			itd.size				= dstHmSize;
			itd.splatPrototypes		= td.splatPrototypes;
			itd.alphamapResolution	= dstAmReso;
			itd.baseMapResolution	= td.baseMapResolution >> splitLevel;
			itd.SetDetailResolution( td.detailResolution >> splitLevel, 8 );//td.detailResolutionPerPatch );
			itd.detailPrototypes	= td.detailPrototypes;
			itd.wavingGrassAmount	= td.wavingGrassAmount;
			itd.wavingGrassSpeed	= td.wavingGrassSpeed;
			itd.wavingGrassStrength	= td.wavingGrassStrength;
			itd.wavingGrassTint		= td.wavingGrassTint;
		//	itd.treePrototypes	= td.treePrototypes;//(TreePrototype[])td.treePrototypes.Clone();
			
			
			var heights = td.GetHeights( ix * dstHmReso, iy * dstHmReso, dstHmReso + 1, dstHmReso + 1 );
			itd.SetHeights( 0, 0, heights );
			
			var alphas = td.GetAlphamaps( ix * dstAmReso, iy * dstAmReso, dstAmReso, dstAmReso );
			itd.SetAlphamaps( 0, 0, alphas );

			foreach( var layer in td.GetSupportedLayers( ix * dstDmReso, iy * dstDmReso, dstDmReso, dstDmReso ) )
			{
				var details = td.GetDetailLayer( ix * dstDmReso, iy * dstDmReso, dstDmReso, dstDmReso, layer );
				itd.SetDetailLayer( 0, 0, layer, details );
			}

			
			itr.terrainData = itd;
			icd.terrainData	= itd;
			igo.layer	= gameObject.layer;
			
			var itf = igo.transform;
			itf.parent = tf;
			itf.localPosition = new Vector3( dstHmSize.x * ix, 0.0f, dstHmSize.z * iy );


			ifd.init( itr, icd, irb );
			
			
			splits[ iy, ix ] = ifd;//itr;
			
		}
		

		// �ߖT�ƂȂ���c�͂������A���ʂ����܂����킩���c�B

		for( var iy = 0; iy < dstNumNest; iy++ )
		for( var ix = 0; ix < dstNumNest; ix++ )
		{
			
			var bottom	= ( iy - 1 > -1 )?			splits[ iy - 1, ix ].terrain: null;
			var top		= ( iy + 1 < dstNumNest )?	splits[ iy + 1, ix ].terrain: null;
			var left	= ( ix - 1 > -1 )?			splits[ iy, ix - 1 ].terrain: null;
			var right	= ( ix + 1 < dstNumNest )?	splits[ iy, ix + 1 ].terrain: null;
			
			splits[ iy, ix ].terrain.SetNeighbors( left, top, right, bottom );
			splits[ iy, ix ].terrain.enabled = true;
			
		}
		
		
		// �ꖇ�n�`�̂����蔻��𖳌��ɂ���B

	//	terrain.enabled = false;
		GetComponent<Collider>().enabled = false;
	//	terrain.terrainData = null;
		
	//	Destroy( collider );
		

		// �Q�[�����̒n�`�ݒ�i�G�f�B�^��ł͕ҏW���Ղ��悤�ɋ@�\���߂ɐݒ肵�Ă���j�B

		terrain.heightmapMaximumLOD		= 512;
		terrain.heightmapPixelError		= 200;
		terrain.basemapDistance			= 0;
		terrain.castShadows				= false;
		terrain.lightmapIndex			= -1;
		
		terrain.editorRenderFlags	= /*TerrainRenderFlags.details | */TerrainRenderFlags.Trees;
		terrain.drawTreesAndFoliage = true;


/*		
		// �e�����n�`�ɖ؂𐶂₷�B

		var scale	= (float)dstNumNest;
		var rscale	= 1.0f / scale;
		
		foreach( var tree in td.treeInstances )
		{
			var ix = Mathf.Floor( tree.position.x * scale );
			var iz = Mathf.Floor( tree.position.z * scale );
			
			addTree( ix, iz, tree, scale, rscale );
		}
		
	//	foreach( var t in splits ) t.Flush();
*/		
		

		// �j�󂪃G�f�B�^�ɔ��f����Ȃ��悤�ɁA�V�����n�`�f�[�^�𐶐�����B
		
		var props	= td.treePrototypes;
		var trees	= td.treeInstances;

		var newtd = new TerrainData();
		newtd.size				= td.size;
		newtd.treePrototypes	= props;
		newtd.treeInstances		= trees;

		terrain.terrainData = newtd;


		// �؂��܂Ƃ߂� GameObject �ƁA�ؒP�̂� GameObject �𐶐�����B
		// �i�؂̔j��̂��߂ɂ͖؂̃R���C�_�𔻕ʂ���K�v�����邪�A�e���C���R���C�_�ɐ��₵���؂���͔��ʕs�\�j

		createTreeController( terrain, newtd, props, trees );

	}


	// �؂��܂Ƃ߂� GameObject �ƁA�ؒP�̂� GameObject �𐶐�����B

	void createTreeController( Terrain tr, TerrainData td, TreePrototype[] props, TreeInstance[] trees )
	{

		var goTrees = new GameObject( "trees" );
		
	//	goTrees.AddComponent<Rigidbody>().isKinematic = true;
		// �Ƃ肠���� Rigidbody ���O���āABullet.getHitter() �� hitter ���擾�ł���悤�ɂ͂����i�V���������ɍ��킹���j�B


		var tfTrees = goTrees.transform;
		
		tfTrees.parent = transform;
		
		tfTrees.position = tr.GetPosition();//Vector3.zero;


		for( var i = 0; i < trees.Length; i++ )
		{

			var tree = trees[ i ];

			var treeCollider = props[ tree.prototypeIndex ].prefab.GetComponent<CapsuleCollider>();

			addTreeObject( tfTrees, i, tree, treeCollider, td.size );	// �ؒP�̂� GameObject �̐���

		}

		Debug.Log( "tree length : " + trees.Length );
		
		TerrainTreeHit3.staticInit( this, td, props, trees, tfTrees.position );
		
	}


	// �ؒP�̂� GameObject �̐���

	void addTreeObject( Transform tfTrees, int id, TreeInstance srcTree, CapsuleCollider srcTreeCollider, Vector3 scale )
	{
		
		if( srcTreeCollider == null ) return;

		
		var go = new GameObject();

		go.layer = UserLayer._bgField;

		var tfgo = go.transform;

		tfgo.position = Vector3.Scale( srcTree.position, scale ) + tfTrees.position;

		tfgo.parent = tfTrees;


		var dst = go.AddComponent<CapsuleCollider>();
		//	var dst = goTrees.AddComponent<CapsuleCollider>();

		dst.center = srcTreeCollider.center * srcTree.heightScale;//Vector3.Scale( srcTree.position, size ) + srcTreeCollider.center;

		dst.height = srcTreeCollider.height * srcTree.heightScale;

		dst.radius = srcTreeCollider.radius * srcTree.widthScale;


		go.AddComponent<TerrainTreeHit3>().init( id );
		
	}



	/*
	void addTree( float ix, float iz, TreeInstance src, float scale, float rscale )
	{
		
		var ti = new TreeInstance();
		
		ti.color			= Color.white;
		ti.heightScale		= src.heightScale;
		ti.widthScale		= src.widthScale;
		ti.lightmapColor	= src.lightmapColor;
		ti.prototypeIndex	= src.prototypeIndex;
		ti.position			= new Vector3( ( src.position.x * scale - ix ), src.position.y, ( src.position.z * scale - iz ) );
		
		splits[ (int)iz, (int)ix ].terrain.AddTreeInstance( ti );
		
	}
	*/


}
