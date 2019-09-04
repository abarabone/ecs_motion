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
    
		Mesh conv()
		{
			var newmesh = new Mesh();
			newmesh.vertices	= MeshUtility.ConvertVertices(this.mesh.vertices,this.mesh.boneWeights,this.mesh.bindposes);//this.mesh.vertices;
			newmesh.boneWeights	= this.mesh.boneWeights;
			newmesh.bindposes	= this.mesh.bindposes;
			newmesh.normals		= this.mesh.normals;
			newmesh.uv			= this.mesh.uv;
			newmesh.triangles	= this.mesh.triangles;
			newmesh.AddBoneInfoFrom( uvChannelForWeight:1, this.clip );
            return newmesh;
		}

    void Start ()
    {
        this.cnvMesh = conv();

        this.boneBuffer = new SimpleComputeBuffer<bone_unit>( "bones", this.boneLength * this.instanceCount );
        this.argsBuffer = new SimpleIndirectArgsBuffer( this.cnvMesh, instanceCount:(uint)this.instanceCount );
        
        this.mat.SetBuffer( this.boneBuffer );
        this.mat.SetInt( "boneLength", this.boneLength );
    }

    void Update ()
    {
        var bones = new NativeArray<bone_unit>( this.boneLength * this.instanceCount, Allocator.Temp );
        for( var i=0; i<this.instanceCount*this.boneLength; i++ )
        {
            bones[i] = new bone_unit
            {
                pos = new float4(i/this.boneLength,0,0,1),
                rot = quaternion.identity
            };
        }
        this.boneBuffer.Buffer.SetData( bones );
        bones.Dispose();

        //Graphics.DrawMesh( this.cnvMesh, Matrix4x4.identity, this.mat, 0 );
        Graphics.DrawMeshInstancedIndirect( this.cnvMesh, 0, this.mat, new Bounds( Vector3.zero, Vector3.one * 100 ), this.argsBuffer );
        //Graphics.DrawMeshInstanced(this.cnvMesh,0,this.mat,new Matrix4x4[ 1 ] { Matrix4x4.identity} );
    }

    private void OnDestroy()
    {
        this.boneBuffer.Dispose();
        this.argsBuffer.Dispose();
    }
}