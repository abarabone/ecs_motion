using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Unity.Mathematics;

using MqoUtility;

namespace MarchingCubes
{
    static public class ConvertMqoForMarchingCubes
    {

        [MenuItem( "Assets/Convert Mqo To Meshes" )]
        static public void CreateMesh()
        {
            if( Selection.objects == null ) return;

            var qMqoPath = Selection.objects
                .Select( o => AssetDatabase.GetAssetPath( o ) )
                .Where( path => Path.GetExtension( path ) == ".mqo" )
                ;
            foreach( var path in qMqoPath )
            {
                using( var f = new StreamReader( path ) )
                {
                    var s = f.ReadToEnd();
                    var data = MqoParser.ConvertToObjectsData( s );
                    var meshes = createMesh( data );

                    foreach( var m in meshes )
                    {
                        var folderPath = Path.GetDirectoryName( path );
                        AssetDatabase.CreateAsset( m, $"{folderPath}/{m.name}.asset" );
                    }

                    AssetDatabase.Refresh();
                }
            }
        }

        [MenuItem( "Assets/Convert Mqo For Marching Cubes" )]
        static public void CreateMachingCubesAsset()
        {
            if( Selection.objects == null ) return;

            var qMqoPath = Selection.objects
                .Select( o => AssetDatabase.GetAssetPath( o ) )
                .Where( path => Path.GetExtension( path ) == ".mqo" )
                ;

            var mqopath = qMqoPath.First();

            using( var f = new StreamReader( mqopath ) )
            {
                var s = f.ReadToEnd();
                var objdata = MqoParser.ConvertToObjectsData( s );
                var baseVtxList = MarchingCubesDataBuilder.MakeBaseVtxList();
                var cubeIdAndIndicesList =
                    MarchingCubesDataBuilder.ConvertObjectDataToMachingCubesData( objdata, baseVtxList );
                var cubeIdAnd4TrianglesNormalsList =
                    ConvertMqoForMarchingCubes.calculateNormals( cubeIdAndIndicesList, baseVtxList );
                var cubeIdAnd12VerticesNormalsList =
                    ConvertMqoForMarchingCubes.calculate12VerticesNormalsPerCubePattern( cubeIdAndIndicesList, cubeIdAnd4TrianglesNormalsList );


                //// 確認
                //using( var wf = new StreamWriter( @"C:\Users\abarabone\Desktop\mydata\mc.txt" ) )
                //{

                //    ////cubeIdAndTriangleNormalsList.SelectMany( x => x.normals ).GroupBy( x => x ).ForEach( n => wf.WriteLine( n.Key ) );
                //    //(from x in cubeIdAnd12VerticesNormalsList group x by x.normals.Select(n=>math.dot(n,n)).ToArray()).ForEach( x => draw12(x.First()) );
                //    //void draw12( (byte cubeId, Vector3[] normals) x )
                //    //{
                //    //    wf.WriteLine( $"{x.cubeId} {string.Join(", ", x.normals)}" );
                //    //}

                //    //var a =
                //    //cubeIdAndIndicesList
                //    //    .Select( x => x.cubeId )
                //    //    .Select( x =>
                //    //        new byte[] {
                //    //            x,
                //    //            //(byte)( ( x & 0x0f ) << 4 | ( x & 0xf0 ) >> 4 ),
                //    //            (byte)( ( x & 0b_0011_0011 ) << 2 | ( x & 0b_1100_1100 ) >> 2 ),
                //    //            //(byte)( ( x & 0b_0101_0101 ) << 1 | ( x & 0b_1010_1010 ) >> 1 ),
                //    //        }
                //    //    )
                //    //    .Select( x => x.OrderBy( b => b ) )
                //    //    //.Select( x => string.Join( ", ", x ) )
                //    //    //.GroupBy( x => x.First() )
                //    //    //.Select( x => x.Key )
                //    //    .OrderBy( x => x.First() )
                //    //    //.Select( x => string.Join( ", ", x ) )
                //    //    .ToArray()
                //    //    ;
                //    //wf.WriteLine( a.GroupBy(x=>x.First()).Count() );
                //    //a.ForEach( x => wf.WriteLine( string.Join( ", ", x ) ) )
                //    //;
                //}



                save_( Selection.objects, cubeIdAndIndicesList, cubeIdAnd4TrianglesNormalsList, cubeIdAnd12VerticesNormalsList, baseVtxList );
            }

            return;


            void save_(
                UnityEngine.Object[] selectedObjects,
                (byte cubeId, int[] indices)[] cubeIdsAndIndexLists,
                (byte cubeId, Vector3[] normals)[] cubeIdsAnd4TrianglesNormals,
                (byte cubeId, Vector3[] normals)[] cubeIdAnd12VerticesNormalsList,
                Vector3[] baseVertexList
            )
            {

                // 渡されたアセットと同じ場所のパス生成

                var srcFilePath = AssetDatabase.GetAssetPath( selectedObjects.First() );

                var folderPath = Path.GetDirectoryName( srcFilePath );

                var fileName = Path.GetFileNameWithoutExtension( srcFilePath );

                var dstFilePath = folderPath + $"/Marching Cubes Resource.asset";


                // アセットとして生成
                var asset = ScriptableObject.CreateInstance<MarchingCubeAsset>();
                var qCubeIndexLists =
                    //from x in Enumerable.Zip( cubeIdsAndIndexLists, cubeIdsAndTriangleNormals, (x,y)=>(x.cubeId, x.indices, y.normals) )
                    from i in cubeIdsAndIndexLists
                    join tn in cubeIdsAnd4TrianglesNormals
                        on i.cubeId equals tn.cubeId
                    join vn in cubeIdAnd12VerticesNormalsList
                        on i.cubeId equals vn.cubeId
                    orderby i.cubeId
                    select new MarchingCubeAsset.CubeWrapper
                    {
                        cubeId = i.cubeId,
                        vertexIndices = i.indices,
                        normalsForTriangle = tn.normals,
                        normalsForVertex = vn.normals,
                    };
                //var qCubeTriangleNormalsList =
                //    from x in cubeIdsAndTriangleNormals
                //    select x.normals
                //    ;
                asset.CubeIdAndVertexIndicesList = qCubeIndexLists.ToArray();
                asset.BaseVertexList = baseVertexList;
                AssetDatabase.CreateAsset( asset, dstFilePath );
                AssetDatabase.Refresh();
            }
        }





        static Mesh[] createMesh( (string name, Vector3[] vtxs, int[][] tris)[] objectsData )
        {

            return objectsData
                .Select( x => createMesh_( x.name, x.vtxs, x.tris ) )
                .ToArray();


            Mesh createMesh_( string name, Vector3[] vtxs, int[][] tris )
            {
                var mesh = new Mesh();
                mesh.name = name;
                mesh.vertices = vtxs;
                mesh.triangles = tris.SelectMany( x => x ).ToArray();

                return mesh;
            }
        }


        // とりあえずここに
        static (byte cubeId, Vector3[] normals)[] calculateNormals
            ( (byte cubeId, int[] indices)[] cubeIdsAndIndexLists, Vector3[] baseVertexList )
        {

            var qNormalPerTriangleInCube =
                from x in cubeIdsAndIndexLists
                select
                    from tri in x.indices.Buffer( 3 )
                    let v0 = baseVertexList[ tri[ 0 ] ]
                    let v1 = baseVertexList[ tri[ 1 ] ]
                    let v2 = baseVertexList[ tri[ 2 ] ]
                    select Vector3.Cross( ( v1 - v0 ), ( v2 - v0 ) ).normalized
                ;
            return Enumerable.Zip( cubeIdsAndIndexLists, qNormalPerTriangleInCube, ( x, y ) => (x.cubeId, normals: y) )
                .Select( x => (x.cubeId, x.normals.ToArray()) )
                .ToArray();
        }


        static (byte cubeId, Vector3[] normals)[] calculate12VerticesNormalsPerCubePattern
            ( (byte cubeId, int[] indices)[] cubeIdsAndIndexLists, (byte cubeId, Vector3[] normals)[] normals )
        {

            var q =
                from x in Enumerable.Zip( cubeIdsAndIndexLists, normals, ( l, r ) => (l.cubeId, l.indices, r.normals) )
                select (x.cubeId, to12Indices(x.indices, x.normals))
                ;
            return q.ToArray();


            Vector3[] to12Indices( int[] idx, Vector3[] nms )
            {
                var res = Enumerable.Repeat( Vector3.zero, 12 ).ToArray();
                var qidxnm = Enumerable.Zip( idx.Buffer( 3 ), nms, ( l, r ) => (i3: l, nm: r) );
                foreach(var x in qidxnm)
                {
                    res[ x.i3[ 0 ] ] += x.nm;
                    res[ x.i3[ 1 ] ] += x.nm;
                    res[ x.i3[ 2 ] ] += x.nm;
                }
                return res.Select( x => x.normalized ).ToArray();
            }
        }
    }

}
