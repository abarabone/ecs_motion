using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Abarabone.Common.Extension;
using Abarabone.Motion;

namespace Abarabone.Geometry
{
    
	static public class ChMeshConverter
	{

        
		static public Mesh ConvertToChMesh( Mesh srcmesh, Transform[] tfBones )
		{
            var dstmesh = new Mesh();

            var vtxs        = srcmesh.vertices;
            var boneWeights = srcmesh.boneWeights;
            var bindposes   = srcmesh.bindposes;

            dstmesh.vertices = QueryVertices( vtxs, boneWeights, bindposes ).ToArray();
            dstmesh.normals = srcmesh.normals;
            dstmesh.uv = srcmesh.uv;
            dstmesh.triangles = srcmesh.triangles;
            dstmesh.SetUVs( channel: 1, QueryBoneWeightUvs( boneWeights ).ToList() );
            dstmesh.bounds = srcmesh.bounds;
            dstmesh.colors = QueryBoneIndexColors( boneWeights, tfBones ).ToArray();
            dstmesh.bindposes = QueryEnabledBindPoses( bindposes, tfBones ).ToArray();
            return dstmesh;

            //var newmesh = new Mesh();
            //newmesh.vertices = MeshUtility.ConvertVertices( vtxs, boneWeights, bindposes );
            //newmesh.boneWeights = boneWeights;
            //newmesh.bindposes = bindposes;
            //newmesh.normals = srcmesh.normals;
            //newmesh.uv = srcmesh.uv;
            //newmesh.triangles = srcmesh.triangles;
            //newmesh.AddBoneInfoFrom( uvChannelForWeight: 1, motionClip );
            //return newmesh;
        }


		/// <summary>
		/// 頂点をトランスフォームし、新しい配列として返す。
		/// </summary>
		static public IEnumerable<Vector3> QueryVertices( Vector3[] vertices, BoneWeight[] weights, Matrix4x4[] matrices )
		{
            return
				from x in Enumerable.Zip( vertices, weights, (v, w) => (v, w) )
				select matrices[ x.w.boneIndex0 ].MultiplyPoint( x.v )
				;
		}
		
		static public IEnumerable<Color> QueryBoneIndexColors( BoneWeight[] boneWeights, Transform[] tfBones )
		{
            var im = tfBones
                .Where( tfBone => !tfBone.name.StartsWith( "_" ) )
                .Select( ( tfBone, i ) => i )
                .ToArray();

            var qIndices = boneWeights
                .Select( x => new Color(im[x.boneIndex0], im[x.boneIndex1], im[x.boneIndex2], im[x.boneIndex3]) );

            return qIndices.ToArray();
		}

		static public IEnumerable<Vector4> QueryBoneWeightUvs( BoneWeight[] boneWeights )
		{
            return
                from w in boneWeights
				select new Vector4( w.weight0, w.weight1, w.weight2, w.weight3 );
		}

		static public IEnumerable<Matrix4x4> QueryEnabledBindPoses( Matrix4x4[] bindPoses, Transform[] tfBones )
        {
            return
                from x in (tfBones, bindPoses).Zip()
                let tfBone = x.x
                let bindPose = x.y
                where !tfBone.name.StartsWith( "_" )
                select bindPose
                ;
		}
	}
}