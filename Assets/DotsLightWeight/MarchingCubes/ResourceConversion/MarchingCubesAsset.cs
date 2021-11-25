using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DotsLite.MarchingCubes
{

    public class MarchingCubesAsset : ScriptableObject
    {
        public Vector3[] BaseVertexList;
        public CubeWrapper[] CubeIdAndVertexIndicesList;

        [System.Serializable]
        public struct CubeWrapper// タプルがシリアライズできないので
        {
            public byte cubeId;
            public byte primaryCubeId;
            public Quaternion rotation;
            public int[] vertexIndices;
            public Vector3[] normalsForTriangle;
            public Vector3[] normalsForVertex;
        }

        //public (byte cubeId, int[] vtxIdxs)[] CubeIdsAndIndexLists =>
        //    this.CubeIndexLists.Select( x => (x.cubeId, x.indices) ).ToArray();


    }

}
