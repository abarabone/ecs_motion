using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Linq;
using System.Threading.Tasks;
using System;
using Abarabone.Common.Extension;

namespace Abss.Geometry
{
    
	public static class MeshUtility
	{
		
		static public GameObject AddToNewGameObject( this Mesh mesh, Material mat )
		{
			var go = new GameObject();
			go.AddComponent<MeshFilter>().sharedMesh = mesh;
			go.AddComponent<MeshRenderer>().sharedMaterial = mat;

			return go;
		}

		static public Mesh ToWriteOnly( this Mesh mesh )
		{
			mesh.UploadMeshData( markNoLongerReadable: true );
			return mesh;
		}
		static public Mesh ToDynamic( this Mesh mesh )
		{
			mesh.MarkDynamic();
			return mesh;
		}


		//static public IEnumerable<(int i0, int i1, int i2)> AsTriangleTupple( this int[] indexes )
		//{
		//	for( var i=0; i<indexes.Length; i+=3 )
		//	{
		//		yield return ( indexes[i+0], indexes[i+1], indexes[i+2] );
		//	}
		//}

	}

}
