using UnityEngine;
using Abss.Cs;
using Unity.Mathematics;
using Unity.Collections;

using Abss.Geometry;

public class MoveCube: MonoBehaviour
{

    public Mesh mesh;
    public Material mat;
    public Abss.Motion.MotionClip clip;
    public int boneLength;
    public int instanceCount;

    Mesh cnvMesh;
    SimpleComputeBuffer<bone_unit> boneBuffer;
    SimpleIndirectArgsBuffer argsBuffer;


    struct bone_unit
    {
        public float4 pos;
        public quaternion rot;
    }
    
		private void Awake()
		{
			
			var srcmr	= GetComponentInChildren<SkinnedMeshRenderer>();
			var srcmesh	= srcmr.sharedMesh;
			
			var dstmesh = new Mesh();

			dstmesh.vertices	= MeshUtility.ConvertVertices( srcmesh.vertices, srcmesh.boneWeights, srcmesh.bindposes );
			dstmesh.triangles	= srcmesh.triangles;
			dstmesh.normals		= srcmesh.normals;
			dstmesh.uv			= srcmesh.uv;
			dstmesh.bounds		= srcmesh.bounds;

			var dstmr	= srcmr.gameObject.AddComponent<MeshRenderer>();
			var dstmf	= srcmr.gameObject.AddComponent<MeshFilter>();

			dstmr.material	= srcmr.material;
			dstmf.mesh		= dstmesh;
			
			Component.Destroy( srcmr );
		}

    void Start ()
    {
        this.cnvMesh = this.mesh.AddBoneInfoFrom( 1, this.clip );

        this.boneBuffer = new SimpleComputeBuffer<bone_unit>( "bones", this.boneLength * this.instanceCount );
        this.argsBuffer = new SimpleIndirectArgsBuffer( this.cnvMesh, instanceCount:(uint)this.instanceCount );
        
        this.mat.SetBuffer( this.boneBuffer );
    }

    void Update ()
    {
        var bones = new NativeArray<bone_unit>( this.boneLength * this.instanceCount, Allocator.Temp );
        for( var i=0; i<this.instanceCount; i++ )
        {
            bones[i*this.boneLength] = new bone_unit
            {
                pos = new float4(i,0,0,0),
                rot = quaternion.identity
            };
        }

        Graphics.DrawMesh( this.cnvMesh, Matrix4x4.identity, this.mat, 0 );
        //Graphics.DrawMeshInstancedIndirect( this.cnvMesh, 0, mat, new Bounds(Vector3.zero,Vector3.one*100), this.argsBuffer );
        
        bones.Dispose();
    }

    private void OnDestroy()
    {
        this.boneBuffer.Dispose();
        this.argsBuffer.Dispose();
    }
}