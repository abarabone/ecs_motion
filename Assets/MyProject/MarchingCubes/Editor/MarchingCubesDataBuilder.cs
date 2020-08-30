using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace MarchingCubes
{
    static public class MarchingCubesDataBuilder
    {



        static public Vector3[] MakeBaseVtxList()
        {
            return new Vector3[]
            {
                new Vector3(0, 1, 1) * 0.5f,
                new Vector3(-1, 1, 0) * 0.5f,
                new Vector3(1, 1, 0) * 0.5f,
                new Vector3(0, 1, -1) * 0.5f,

                new Vector3(-1, 0, 1) * 0.5f,
                new Vector3(1, 0, 1) * 0.5f,
                new Vector3(-1, 0, -1) * 0.5f,
                new Vector3(1, 0, -1) * 0.5f,

                new Vector3(0, -1, 1) * 0.5f,
                new Vector3(-1, -1, 0) * 0.5f,
                new Vector3(1, -1, 0) * 0.5f,
                new Vector3(0, -1, -1) * 0.5f,
            };
        }



        static public (byte cubeId, int[] indices)[]
        ConvertObjectDataToMachingCubesData(
            (string name, Vector3[] vtxs, int[][] tris)[] objectsData,
            Vector3[] baseVtxList
        )
        {

            var baseVtxIndexBySbvtxDict = makeBaseVtxIndexBySbvtxDict_( baseVtxList );

            var prototypeCubes = makePrototypeCubes_( objectsData );
            var cube254Pattarns = makeCube254Pattarns_( prototypeCubes );
            var triVtxLists = transformSbyteVtxs_( cube254Pattarns, prototypeCubes );

            var triIdxLists = makeVtxIndexListsPerCube_( triVtxLists, baseVtxIndexBySbvtxDict );
            
            return triIdxLists;


            (byte id, IEnumerable<(Vector3 v0, Vector3 v1, Vector3 v2)> trivtxs)[] makePrototypeCubes_
                ( (string name, Vector3[] vtxs, int[][] tris)[] objectsData_ )
            {
                var qExtractedData =
                    from obj in objectsData_
                    where obj.name.Length == 8 + 1
                    where obj.name[ 4 ] == '_'
                    let n = obj.name.Replace( "_", "" )
                    select (cubeId: Convert.ToByte( n, 2 ), obj.vtxs, obj.tris)
                    ;
                var extractedData = qExtractedData.ToArray();


                var qTriVtx =
                    from cube in extractedData
                    select
                        from tri in cube.tris
                        select (v0: cube.vtxs[ tri[ 0 ] ], v1: cube.vtxs[ tri[ 1 ] ], v2: cube.vtxs[ tri[ 2 ] ])
                    ;
                var qId =
                    from cube in extractedData
                    select cube.cubeId
                    ;

                var qVtxAndId =
                    from x in Enumerable.Zip( qId, qTriVtx, ( l, r ) => (id: l, trivtx: r) )
                    select (x.id, trivtxs: x.trivtx)
                    ;
                var vtxsAndIds = qVtxAndId.ToArray();


                //// 確認
                //foreach( var x in qExtractedData )
                //{
                //    Debug.Log( $"{Convert.ToString( x.cubeId, 2 ).PadLeft( 8, '0' )}" );
                //}

                return vtxsAndIds;
            }

            CubePattarn[] makeCube254Pattarns_
                ( IEnumerable<(byte id, IEnumerable<(Vector3 v0, Vector3 v1, Vector3 v2)>)> prototypeCubes_ )
            {
                var qPrototypeId =
                    from cube in prototypeCubes_
                    select new CubePattarn( cube.id )
                    ;
                var prototypeId = qPrototypeId.ToArray();

                var stds = prototypeId;
                var revs = prototypeId.Select( x => x.Reverse() );//.ToArray();
                //var flips = prototypeId.Select( x => x.FlipX() );//.ToArray();//
                var rotstds = rotAllPattarn_( stds );
                var rotrevs = rotAllPattarn_( revs );
                //var rotFlips = rotAllPattarn_( flips );//
                var qId =
                    from x in rotstds.Concat( rotrevs )//.Concat( rotFlips )
                    group x by (x.id, x.primaryId) into g
                    //group x by x.id into g
                    orderby g.Key
                    select g
                    ;
                var idsAndPattarns = qId.Select( x => x.First() ).ToArray();

                //// 確認
                //using( var f = new StreamWriter( @"C:\Users\abarabone\Desktop\mydata\mc.txt" ) )
                //{
                //    var idGroups = qId.ToArray();

                //    f.WriteLine( idGroups.Length );
                //    var ss =
                //        from g in idGroups
                //        //where g.Key.primaryId == 240//
                //        //from p in g
                //        let p = g.First()
                //        let id = Convert.ToString( p.id, 2 ).PadLeft( 8, '0' )
                //        let primaryId = Convert.ToString( p.primaryId, 2 ).PadLeft( 8, '0' )
                //        select (id, primaryId, p.dir, p.up, p.side)
                //        ;
                //    var s = string.Join( "\r\n", ss );
                //    f.WriteLine( s );
                //}

                return idsAndPattarns;


                CubePattarn[] rotAllPattarn_( IEnumerable<CubePattarn> src )
                {
                    var rotx0 = src.Select( x => x.RotX() ).ToArray();
                    var rotx1 = rotx0.Select( x => x.RotX() ).ToArray();
                    var rotx2 = rotx1.Select( x => x.RotX() ).ToArray();
                    var rotx = src.Concat( rotx0 ).Concat( rotx1 ).Concat( rotx2 );//.ToArray();

                    var roty0 = rotx.Select( x => x.RotY() ).ToArray();
                    var roty1 = roty0.Select( x => x.RotY() ).ToArray();
                    var roty2 = roty1.Select( x => x.RotY() ).ToArray();
                    var rotxy = rotx.Concat( roty0 ).Concat( roty1 ).Concat( roty2 );//.ToArray();

                    var rotz0 = rotxy.Select( x => x.RotZ() ).ToArray();
                    var rotz1 = rotz0.Select( x => x.RotZ() ).ToArray();
                    var rotz2 = rotz1.Select( x => x.RotZ() ).ToArray();
                    var rotxyz = rotxy.Concat( rotz0 ).Concat( rotz1 ).Concat( rotz2 );//.ToArray();

                    return rotxyz.ToArray();
                }
            }


            IEnumerable<(byte cubeId, IEnumerable<IEnumerable<(sbyte x, sbyte y, sbyte z)>> triVtxs)> transformSbyteVtxs_(
                IEnumerable<CubePattarn> cubePattarns,
                IEnumerable<(byte id, IEnumerable<(Vector3 v0, Vector3 v1, Vector3 v2)> trivtxs)> prototypeCubes_
            )
            {

                var q =
                    from pat in cubePattarns
                    join proto in prototypeCubes_ on pat.primaryId equals proto.id
                    select
                        from trivtx in proto.trivtxs
                        select new[]
                        {
                            transform_( pat, trivtx.v0 ),
                            transform_( pat, trivtx.v1 ),
                            transform_( pat, trivtx.v2 ),
                        }
                    ;
                return Enumerable.Zip( cubePattarns, q,
                    ( l, r ) => (l.id, l.isReverseTriangle ? r.Select(x=>x.Reverse()) : r) );

                (sbyte x, sbyte y, sbyte z) transform_( CubePattarn cube, Vector3 protoVtx )
                {
                    var vtx = math.sign(protoVtx);
                    var fwd = cube.dir;
                    var up = cube.up;
                    var side = cube.side;
                    //var side = (
                    //    x: -fwd.y * up.z + fwd.z * up.y,
                    //    y: -fwd.z * up.x + fwd.x * up.z,
                    //    z: -fwd.x * up.y + fwd.y * up.x
                    //);
                    var x = vtx.x * side.x + vtx.y * up.x + vtx.z * fwd.x;
                    var y = vtx.x * side.y + vtx.y * up.y + vtx.z * fwd.y;
                    var z = vtx.x * side.z + vtx.y * up.z + vtx.z * fwd.z;
                    return ((sbyte)x, (sbyte)y, (sbyte)z);
                }
            }


            Dictionary<(sbyte x, sbyte y, sbyte z), int>
                makeBaseVtxIndexBySbvtxDict_( IEnumerable<Vector3> baseVtxList_ )
            {
                var dict = baseVtxList_
                    .Select( x => math.sign(x) )
                    .Select( ( x, i ) => (sbvtx: ((sbyte)x.x, (sbyte)x.y, (sbyte)x.z), i) )
                    .ToDictionary( x => x.sbvtx, x => x.i )
                    ;
                return dict;
            }


            (byte cubeId, int[] indices)[] makeVtxIndexListsPerCube_(
                IEnumerable<(byte cubeId, IEnumerable<IEnumerable<(sbyte x, sbyte y, sbyte z)>> triVtxs)> cubeIdsAndVtxLists_,
                Dictionary<(sbyte x, sbyte y, sbyte z), int> baseVtxIndexBySbvtxDict_
            )
            {
                var q =
                    from cube in cubeIdsAndVtxLists_
                    select
                        from triVtx in cube.triVtxs
                        from vtx in triVtx
                        select baseVtxIndexBySbvtxDict_[ vtx ]
                    ;
                return Enumerable.Zip( cubeIdsAndVtxLists_, q, ( l, r ) => (l.cubeId, indices: r.ToArray()) )
                    .ToArray()
                    ;
            }
        }




        struct CubePattarn
        {
            public byte primaryId;
            public byte id;
            public (sbyte x, sbyte y, sbyte z) dir;
            public (sbyte x, sbyte y, sbyte z) up;
            public (sbyte x, sbyte y, sbyte z) side;// reverse 時に必要
            public bool isReverseTriangle;
            public CubePattarn( byte id )
            {
                this.primaryId = id;
                this.id = id;
                this.dir = (0, 0, 1);
                this.up = (0, 1, 0);
                this.side = (1, 0, 0);
                this.isReverseTriangle = false;
            }
            public CubePattarn( CubePattarn src, byte id )
            {
                this = src;
                this.id = id;
            }
            // 左ねじの回転方向とする
            public CubePattarn RotX()
            {
                var x0 = this.id & 0b_0000_0011;
                var x1 = this.id & 0b_0011_0000;
                var x2 = this.id & 0b_1100_0000;
                var x3 = this.id & 0b_0000_1100;
                this.id = (byte)( x0 << 4 | x1 << 2 | x2 >> 4 | x3 >> 2 );

                this.dir = (this.dir.x, (sbyte)-this.dir.z, this.dir.y);
                this.up = (this.up.x, (sbyte)-this.up.z, this.up.y);
                this.side = (this.side.x, (sbyte)-this.side.z, this.side.y);

                return this;
            }
            public CubePattarn RotY()
            {
                var y0 = this.id & 0b_0001_0001;
                var y1 = this.id & 0b_0010_0010;
                var y2 = this.id & 0b_1000_1000;
                var y3 = this.id & 0b_0100_0100;
                this.id = (byte)( y0 << 1 | y1 << 2 | y2 >> 1 | y3 >> 2 );

                this.dir = (this.dir.z, this.dir.y, (sbyte)-this.dir.x);
                this.up = (this.up.z, this.up.y, (sbyte)-this.up.x);
                this.side = (this.side.z, this.side.y, (sbyte)-this.side.x);

                return this;
            }
            public CubePattarn RotZ()
            {
                var z0 = this.id & 0b_0000_0101;
                var z1 = this.id & 0b_0101_0000;
                var z2 = this.id & 0b_1010_0000;
                var z3 = this.id & 0b_0000_1010;
                this.id = (byte)( z0 << 4 | z1 << 1 | z2 >> 4 | z3 >> 1 );
                
                this.dir = ((sbyte)-this.dir.y, this.dir.x, this.dir.z);
                this.up = ((sbyte)-this.up.y, this.up.x, this.up.z);
                this.side = ((sbyte)-this.side.y, this.side.x, this.side.z);

                return this;
            }
            public CubePattarn FlipX()
            {
                var l = this.id & 0b_0101_0101;
                var r = this.id & 0b_1010_1010;
                this.id = (byte)( ( l << 1 ) | ( r >> 1 ) );

                this.dir.x = (sbyte)-this.dir.x;
                this.up.x = (sbyte)-this.up.x;
                this.side.x = (sbyte)-this.side.x;

                this.isReverseTriangle ^= true;
                return this;
            }
            public CubePattarn FlipY()
            {
                var u = this.id & 0b_0000_1111;
                var d = this.id & 0b_1111_0000;
                this.id = (byte)( ( u << 4 ) | ( d >> 4 ) );

                this.dir.y = (sbyte)-this.dir.y;
                this.up.y = (sbyte)-this.up.y;
                this.side.y = (sbyte)-this.side.y;

                this.isReverseTriangle ^= true;
                return this;
            }
            public CubePattarn FlipZ()
            {
                var f = this.id & 0b_0011_0011;
                var b = this.id & 0b_1100_1100;
                this.id = (byte)( ( f << 2 ) | ( b >> 2 ) );

                this.dir.z = (sbyte)-this.dir.z;
                this.up.z = (sbyte)-this.up.z;
                this.side.z = (sbyte)-this.side.z;

                this.isReverseTriangle ^= true;
                return this;
            }
            public CubePattarn Reverse()
            {
                this.id = (byte)( this.id ^ 0b_1111_1111 );

                //this.dir = ((sbyte)-this.dir.x, (sbyte)-this.dir.y, (sbyte)-this.dir.z);
                //this.up = ((sbyte)-this.up.x, (sbyte)-this.up.y, (sbyte)-this.up.z);
                //this.side = ((sbyte)-this.side.x, (sbyte)-this.side.y, (sbyte)-this.side.z);

                this.isReverseTriangle ^= true;
                return this;
            }
        }

    }
}
