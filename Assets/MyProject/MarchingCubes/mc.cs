using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;

namespace MarchingCubes
{

    using vc = Vector3;
    //using MarchingCubeAsset = MarchingCubes.MarchingCubeAsset;
    //using CubeGridArrayUnsafe = MarchingCubes.CubeGridArrayUnsafe;
    //using CubeInstance = MarchingCubes.CubeInstance;
    //using CubeUtility = MarchingCubes.CubeUtility;

    public class mc : MonoBehaviour
    {
        public MarchingCubeAsset MarchingCubeAsset;
        public Material Material;

        public ComputeShader setGridCubeIdShader;

        public CubeGridArrayUnsafe cubeGrids { get; private set; }
        public MeshCollider[,,] cubeGridColliders { get; private set; }

        //uint[] cubeInstances;
        NativeList<CubeUtility.GridInstanceData> gridData;
        NativeList<CubeInstance> cubeInstances;
        //NativeQueue<CubeInstance> cubeInstances;

        MeshResources meshResources;

        public int maxDrawGridLength;

        
        void setResources()
        {
            var res = this.meshResources;
            
            //uint4 cube_patterns[ 254 ][2];
			// [0] : vertex posision index { x: tri0(i0>>0 | i1>>8 | i2>>16)  y: tri1  z: tri2  w: tri3 }
			// [1] : vertex normal index { x: (i0>>0 | i1>>8 | i2>>16 | i3>>24)  y: i4|5|6|7  z:i8|9|10|11 }
            
            //uint4 cube_vtxs[ 12 ];
            // x: near vertex index (x>>0 | y>>8 | z>>16)
            // y: near vertex index offset prev (left >>0 | up  >>8 | front>>16)
            // z: near vertex index offset next (right>>0 | down>>8 | back >>16)
            // w: pos(x>>0 | y>>8 | z>>16)
            
            //uint3 grids[ 512 ][2];
			// [0] : position as float3
			// [1] : near grid id
			// { x: prev(left>>0 | up>>9 | front>>18)  y: next(right>>0 | down>>9 | back>>18)  z: current }
            
            this.Material.SetConstantBuffer( "normals", res.NormalBuffer );
            this.Material.SetConstantBuffer( "cube_patterns", res.CubePatternBuffer );
            this.Material.SetConstantBuffer( "cube_vtxs", res.CubeVertexBuffer );
            //this.Material.SetConstantBuffer( "grids", res.GridBuffer );
            this.Material.SetVectorArray( "grids", new Vector4[ 512 * 2 ] );// res.GridBuffer );

            this.Material.SetBuffer( "cube_instances", res.CubeInstancesBuffer );
            this.Material.SetTexture( "grid_cubeids", res.GridCubeIdBuffer );
            //this.Material.SetBuffer( "grid_cubeids", res.GridCubeIdBuffer );


            this.setGridCubeIdShader.SetBuffer( 0, "src_instances", res.CubeInstancesBuffer );
            this.setGridCubeIdShader.SetTexture( 0, "dst_grid_cubeids", res.GridCubeIdBuffer );
            //this.setGridCubeIdShader.SetBuffer( 0, "dst_grid_cubeids", res.GridCubeIdBuffer );
        }



        unsafe void Awake()
        {
            this.gridData = new NativeList<CubeUtility.GridInstanceData>( this.maxDrawGridLength, Allocator.Persistent );
            this.cubeInstances = new NativeList<CubeInstance>( 1000000, Allocator.Persistent );
            //this.cubeInstances = new NativeQueue<CubeInstance>( Allocator.Persistent );

            this.meshResources = new MeshResources( this.MarchingCubeAsset, this.maxDrawGridLength );
            setResources();

            var res = this.meshResources;
            var cb = createCommandBuffer( res, this.Material );
            Camera.main.AddCommandBuffer( CameraEvent.BeforeSkybox, cb );

            initCubes();
            createHitMesh();
        }
        unsafe void initCubes()
        {
            var res = this.meshResources;

            this.cubeGrids = new CubeGridArrayUnsafe( 8, 5, 8 );
            this.cubeGrids.FillCubes( new int3( -1, 2, -1 ), new int3( 11, 11, 11 ), isFillAll: true );
            this.cubeGrids.FillCubes( new int3( 2, 1, 3 ), new int3( 1, 2, 1 ), isFillAll: true );

            var c = this.cubeGrids[ 0, 0, 0 ];
            ( *c.p )[ 1, 1, 1 ] = 1;
            c[ 31, 1, 1 ] = 1;
            c[ 31, 31, 31 ] = 1;
            c[ 1, 31, 1 ] = 1;
            for( var iy = 0; iy < 15; iy++ )
                for( var iz = 0; iz < 15; iz++ )
                    for( var ix = 0; ix < 13; ix++ )
                        c[ 5 + ix, 5 + iy, 5 + iz ] = 1;
            this.job = this.cubeGrids.BuildCubeInstanceData( this.gridData, this.cubeInstances );

            this.job.Complete();
            //this.gridData.AsArray().ForEach( x => Debug.Log( x ) );

            res.CubeInstancesBuffer.SetData( this.cubeInstances.AsArray() );
            res.GridBuffer.SetData( this.gridData.AsArray() );
            var remain = ( 64 - ( this.cubeInstances.Length & 0x3f ) ) & 0x3f;
            //for( var i = 0; i < remain; i++ ) this.cubeInstances.AddNoResize( new CubeInstance { instance = 1 } );
            //this.setGridCubeIdShader.Dispatch( 0, this.cubeInstances.Length >> 6, 1, 1 );
            Debug.Log( $"{cubeInstances.Length} / {res.CubeInstancesBuffer.count}" );
        }
        void createHitMesh()
        {
            var glen = this.cubeGrids.GridLength;
            this.cubeGridColliders = new MeshCollider[ glen.x, glen.y, glen.z ];

            this.idxLists = this.MarchingCubeAsset.CubeIdAndVertexIndicesList.Select( x => x.vertexIndices ).ToArray();
            this.vtxList = this.MarchingCubeAsset.BaseVertexList.Select( x => new float3( x.x, x.y, x.z ) ).ToArray();
            //var idxLists = this.MarchingCubeAsset.CubeIdAndVertexIndicesList.Select( x => x.vertexIndices ).ToArray();
            //var vtxList = this.MarchingCubeAsset.BaseVertexList.Select( x => new float3( x.x, x.y, x.z ) ).ToArray();
            var q =
                from x in this.cubeInstances.ToArray()
                let gridId = CubeUtility.FromCubeInstance( x.instance ).gridId
                group x by gridId
                ;
            foreach( var cubeId in q )
            {
                var gridid = (int)cubeId.Key;
                var gridpos = this.gridData[ gridid ].Position;// スケールを１としている
                var igrid = ((int4)gridpos >> 5) * new int4( 1, -1, -1, 0 );

                if( igrid.x < 0 || igrid.y < 0 || igrid.z < 0 ) continue;

                var collider = this.cubeGridColliders[ igrid.x, igrid.y, igrid.z ];
                this.cubeGridColliders[ igrid.x, igrid.y, igrid.z ] = this.BuildMeshCollider( gridpos.xyz, collider, cubeId );
            }
        }


        CommandBuffer createCommandBuffer( MeshResources res, Material mat )
        {
            var cb = new CommandBuffer();
            cb.name = "marching cubes drawer";

            cb.DispatchCompute( this.setGridCubeIdShader, 0, res.ArgsBufferForDispatch, 0 );

            cb.DrawMeshInstancedIndirect( res.mesh, 0, mat, 0, res.ArgsBufferForInstancing );

            return cb;
        }


        int[][] idxLists;
        float3[] vtxList;
        public MeshCollider BuildMeshCollider( float3 gridpos, MeshCollider mc, IEnumerable<CubeInstance> cubeInstances )
        {
            if( !cubeInstances.Any() ) return mc;

            var gridid = CubeUtility.FromCubeInstance( cubeInstances.First().instance ).gridId;

            if( mc == null )
            {
                var igrid = ( (int3)gridpos >> 5 ) * new int3( 1, -1, -1 );
                var go = new GameObject( $"grid {igrid.x} {igrid.y} {igrid.z}" );
                go.transform.position = gridpos;

                mc = go.AddComponent<MeshCollider>();
                mc.sharedMesh = new Mesh();
            }
            mc.enabled = false;

            var (i, v) = CubeUtility.MakeCollisionMeshData( cubeInstances, this.idxLists, this.vtxList );
            using( i )
            using( v )
            {
                var mesh = mc.sharedMesh;
                mesh.Clear();
                mesh.MarkDynamic();
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                mesh.SetVertices( v.AsArray() );
                mesh.SetIndices( i.AsArray(), MeshTopology.Triangles, 0 );

                mc.sharedMesh = mesh;
            }
            mc.enabled = true;
            return mc;
        }



        private void OnDestroy()
        {
            this.job.Complete();

            this.meshResources.Dispose();
            this.cubeGrids.Dispose();
            this.gridData.Dispose();
            this.cubeInstances.Dispose();
        }


        JobHandle job;

        private unsafe void Update()
        {
            //var c = this.cubeGrids[ 5, 1, 3 ];
            //c[ i, 0, 0 ] ^= 1;
            //i = i + 1 & 31;
            this.gridData.Clear();
            this.cubeInstances.Clear();
            this.job = this.cubeGrids.BuildCubeInstanceData( this.gridData, this.cubeInstances );

        //}
        //private unsafe void LateUpdate()
        //{
            this.job.Complete();

            var res = this.meshResources;
            res.CubeInstancesBuffer.SetData( this.cubeInstances.AsArray() );
            
            //res.GridBuffer.SetData( this.gridData.AsArray() );
            var grids = new Vector4[this.gridData.Length * 2];
            fixed( Vector4 *pdst = grids )
            {
                var psrc = (Vector4*)this.gridData.GetUnsafeReadOnlyPtr();
                UnsafeUtility.MemCpy( pdst, psrc, this.gridData.Length * 2 * sizeof( float4 ) );
            }
            this.Material.SetVectorArray( "grids", grids );

            var remain = (64 - (this.cubeInstances.Length & 0x3f) ) & 0x3f;
            for(var i=0; i<remain; i++) this.cubeInstances.AddNoResize( new CubeInstance { instance = 1 } );
            var dargparams = new IndirectArgumentsForDispatch( this.cubeInstances.Length >> 6, 1, 1 );
            var dargs = res.ArgsBufferForDispatch;
            dargs.SetData( ref dargparams );
            //this.setGridCubeIdShader.Dispatch( 0, this.cubeInstances.Length >> 6, 1, 1 );//
            
            var mesh = res.mesh;
            var mat = this.Material;
            var iargs = res.ArgsBufferForInstancing;

            var instanceCount = this.cubeInstances.Length;
            var iargparams = new IndirectArgumentsForInstancing( mesh, instanceCount );
            iargs.SetData( ref iargparams );

            //var bounds = new Bounds() { center = Vector3.zero, size = Vector3.one * 1000.0f };//
            //Graphics.DrawMeshInstancedIndirect( mesh, 0, mat, bounds, iargs );//

        }
        int i;





        struct MeshResources : System.IDisposable
        {
            public ComputeBuffer ArgsBufferForInstancing;
            public ComputeBuffer ArgsBufferForDispatch;

            public ComputeBuffer NormalBuffer;
            public ComputeBuffer CubePatternBuffer;
            public ComputeBuffer CubeVertexBuffer;
            public ComputeBuffer GridBuffer;
        
            public ComputeBuffer CubeInstancesBuffer;
            public RenderTexture GridCubeIdBuffer;
            //public ComputeBuffer GridCubeIdBuffer;

            public Mesh mesh;
            

            public MeshResources( MarchingCubeAsset asset, int maxGridLength ) : this()
            {
                this.ArgsBufferForInstancing = ComputeShaderUtility.CreateIndirectArgumentsBufferForInstancing();
                this.ArgsBufferForDispatch = ComputeShaderUtility.CreateIndirectArgumentsBufferForDispatch();

                this.CubeInstancesBuffer = createCubeIdInstancingShaderBuffer_( 32 * 32 * 32 * maxGridLength );
                this.GridCubeIdBuffer = createGridCubeIdShaderBuffer_( maxGridLength );

                var vertexNormalDict = makeVertexNormalsDict_( asset.CubeIdAndVertexIndicesList );Debug.Log( vertexNormalDict.Count );
                this.NormalBuffer = createNormalList_( vertexNormalDict );
                this.CubePatternBuffer = createCubePatternBuffer_( asset.CubeIdAndVertexIndicesList, vertexNormalDict );
                this.CubeVertexBuffer = createCubeVertexBuffer_( asset.BaseVertexList );
                this.GridBuffer = createGridShaderBuffer_( 512 );

                this.mesh = createMesh_();
            }

            public void Dispose()
            {
                if( this.ArgsBufferForInstancing != null ) this.ArgsBufferForInstancing.Dispose();
                if( this.ArgsBufferForDispatch != null ) this.ArgsBufferForDispatch.Dispose();

                if( this.CubeInstancesBuffer != null ) this.CubeInstancesBuffer.Dispose();
                if( this.GridCubeIdBuffer != null ) this.GridCubeIdBuffer.Release();

                if( this.NormalBuffer != null ) this.NormalBuffer.Dispose();
                if( this.CubePatternBuffer != null ) this.CubePatternBuffer.Dispose();
                if( this.CubeVertexBuffer != null ) this.CubeVertexBuffer.Dispose();
                if( this.GridBuffer != null ) this.GridBuffer.Dispose();
            }

            ComputeBuffer createCubeIdInstancingShaderBuffer_( int maxUnitLength )
            {
                var buffer = new ComputeBuffer( maxUnitLength, Marshal.SizeOf<uint>() );

                return buffer;
            }

            RenderTexture createGridCubeIdShaderBuffer_( int maxGridLength )
            {
                var buffer = new RenderTexture( 32 * 32, 32, 0, UnityEngine.Experimental.Rendering.GraphicsFormat.R8_UInt, 0 );
                buffer.enableRandomWrite = true;
                buffer.dimension = TextureDimension.Tex2DArray;
                buffer.volumeDepth = maxGridLength;
                buffer.Create();

                return buffer;
            }
            //ComputeBuffer createGridCubeIdShaderBuffer_( int maxGridLength )
            //{
            //    var buffer = new ComputeBuffer( 32 * 32 * 32 * maxGridLength, Marshal.SizeOf<uint>() );

            //    return buffer;
            //}


            static float3 round_normal_( float3 x )
            {
                var digits = 5;

                return new float3( (float)Math.Round( x.x, digits ), (float)Math.Round( x.y, digits ), (float)Math.Round( x.z, digits ) );
                //return new float3( new half3( x ) );
            }
            Dictionary<float3, int> makeVertexNormalsDict_( MarchingCubeAsset.CubeWrapper[] cubeIdsAndVtxIndexLists_ )
            {
                return cubeIdsAndVtxIndexLists_
                    .SelectMany( x => x.normalsForVertex )
                    .Select( x => round_normal_(x) )
                    .Distinct( x => x )
                    .Select( ( x, i ) => (x, i) )
                    .ToDictionary( x => x.x, x => x.i );
            }

            ComputeBuffer createNormalList_( Dictionary<float3, int> normalToIdDict )
            {
                var buffer = new ComputeBuffer( normalToIdDict.Count, Marshal.SizeOf<Vector4>(), ComputeBufferType.Constant );

                var q =
                    from n in normalToIdDict
                        //.OrderBy( x => x.Value )
                        //.Do( x => Debug.Log( $"{x.Value} {x.Key}" ) )
                        .Select(x => x.Key)
                    select new Vector4
                    {
                        x = n.x,
                        y = n.y,
                        z = n.z,
                        w = 0.0f,
                    };

                buffer.SetData( q.ToArray() );

                return buffer;
            }

            ComputeBuffer createCubePatternBuffer_( MarchingCubeAsset.CubeWrapper[] cubeIdsAndVtxIndexLists_, Dictionary<float3, int> normalToIdDict )
            {
                //var buffer = new ComputeBuffer( 254, Marshal.SizeOf<uint4>() * 2, ComputeBufferType.Constant );
                var buffer = new ComputeBuffer( 254 * 2, Marshal.SizeOf<uint4>(), ComputeBufferType.Constant );

                var q =
                    from cube in cubeIdsAndVtxIndexLists_
                    orderby cube.cubeId
                    select new[]
                    {
                        toTriPositionIndex_( cube.vertexIndices ),
                        toVtxNormalIndex_( cube.normalsForVertex, normalToIdDict )
                    };
                //q.SelectMany(x=>x).ForEach( x => Debug.Log(x) );
                buffer.SetData( q.SelectMany(x=>x).Select(x=>math.asfloat(x)).ToArray() );

                return buffer;


                uint4 toTriPositionIndex_( int[] indices )
                {
                    var idxs = indices
                        .Concat( Enumerable.Repeat( 0, 12 - indices.Length ) )
                        .ToArray();

                    return new uint4
                    {
                        x = (idxs[ 0], idxs[ 1], idxs[ 2], 0).PackToByte4Uint(),
                        y = (idxs[ 3], idxs[ 4], idxs[ 5], 0).PackToByte4Uint(),
                        z = (idxs[ 6], idxs[ 7], idxs[ 8], 0).PackToByte4Uint(),
                        w = (idxs[ 9], idxs[10], idxs[11], 0).PackToByte4Uint(),
                        //x = (uint)( idxs[ 0]<<0 & 0xff | idxs[ 1]<<8 & 0xff00 | idxs[ 2]<<16 & 0xff0000 ),
                        //y = (uint)( idxs[ 3]<<0 & 0xff | idxs[ 4]<<8 & 0xff00 | idxs[ 5]<<16 & 0xff0000 ),
                        //z = (uint)( idxs[ 6]<<0 & 0xff | idxs[ 7]<<8 & 0xff00 | idxs[ 8]<<16 & 0xff0000 ),
                        //w = (uint)( idxs[ 9]<<0 & 0xff | idxs[10]<<8 & 0xff00 | idxs[11]<<16 & 0xff0000 ),
                    };
                }
                uint4 toVtxNormalIndex_( Vector3[] normals, Dictionary<float3, int> normalToIdDict_ )
                {
                    return new uint4
                    {
                        x = (ntoi( 0), ntoi( 1), ntoi( 2), ntoi( 3)).PackToByte4Uint(),
                        y = (ntoi( 4), ntoi( 5), ntoi( 6), ntoi( 7)).PackToByte4Uint(),
                        z = (ntoi( 8), ntoi( 9), ntoi(10), ntoi(11)).PackToByte4Uint(),
                        //x = (uint)( ntoi(0,0) | ntoi(1,8) | ntoi( 2,16) | ntoi( 3,24) ),
                        //y = (uint)( ntoi(4,0) | ntoi(5,8) | ntoi( 6,16) | ntoi( 7,24) ),
                        //z = (uint)( ntoi(8,0) | ntoi(9,8) | ntoi(10,16) | ntoi(11,24) ),
                        w = 0,
                    };
                    //int ntoi( int i, int shift ) => (normalToIdDict_[ round_normal_(normals[ i ]) ] & 0xff) << shift;
                    int ntoi( int i ) { Debug.Log($"{i} @ {round_normal_( normals[ i ] )} => {normalToIdDict_[ round_normal_( normals[ i ] ) ]}"); return normalToIdDict_[ round_normal_( normals[ i ] ) ]; }
                }
            }

            ComputeBuffer createCubeVertexBuffer_( Vector3[] baseVertices )
            {
                var buffer = new ComputeBuffer( 12, Marshal.SizeOf<uint4>(), ComputeBufferType.Constant );

                ((int x, int y, int z) ortho1, (int x, int y, int z) ortho2)[] near_cube_offsets =
                {
                    (( 0, 0, -1), ( 0, -1, 0)),
                    (( -1, 0, 0), ( 0, -1, 0)),
                    (( +1, 0, 0), ( 0, -1, 0)),
                    (( 0, 0, +1), ( 0, -1, 0)),

                    (( -1, 0, 0), ( 0, 0, -1)),
                    (( +1, 0, 0), ( 0, 0, -1)),
                    (( -1, 0, 0), ( 0, 0, +1)),
                    (( +1, 0, 0), ( 0, 0, +1)),

                    (( 0, 0, -1), ( 0, +1, 0)),
                    (( -1, 0, 0), ( 0, +1, 0)),
                    (( +1, 0, 0), ( 0, +1, 0)),
                    (( 0, 0, +1), ( 0, +1, 0)),
                };
                (int ortho1, int ortho2, int slant)[] near_cube_ivtxs =
                {
                    (3,8,11),
                    (2,9,10),
                    (1,10,9),
                    (0,11,8),

                    (5,6,7),
                    (4,7,6),
                    (7,4,5),
                    (6,5,4),

                    (11,0,3),
                    (10,1,2),
                    (9,2,1),
                    (8,3,0),
                };

                var q =
                    from v in Enumerable
                        .Zip( near_cube_offsets, near_cube_ivtxs, ( x, y ) => (ofs: x, ivtx: y) )
                        .Zip( baseVertices, (x,y) => (x.ofs, x.ivtx, pos: y) )
                        let x = (v.ivtx.ortho1, v.ivtx.ortho2, v.ivtx.slant, 0).PackToByte4Uint()
                        let y = (v.ofs.ortho1.x + 1, v.ofs.ortho1.y + 1, v.ofs.ortho1.z + 1, 0).PackToByte4Uint()
                        let z = (v.ofs.ortho2.x + 1, v.ofs.ortho2.y + 1, v.ofs.ortho2.z + 1, 0).PackToByte4Uint()
                        let w = ((int)(v.pos.x * 2) + 1, (int)(v.pos.y * 2) + 1, (int)(v.pos.z * 2) + 1, 0).PackToByte4Uint()
                        //let x = v.ivtx.x<<0 & 0xff | v.ivtx.y<<8 & 0xff00 | v.ivtx.z<<16 & 0xff0000
                        //let y = v.ofs.ortho1.x+1<<0 & 0xff | v.ofs.ortho1.y+1<<8 & 0xff00 | v.ofs.ortho1.z+1<<16 & 0xff0000
                        //let z = v.ofs.ortho2.x+1<<0 & 0xff | v.ofs.ortho2.y+1<<8 & 0xff00 | v.ofs.ortho2.z+1<<16 & 0xff0000
                        //let w = (int)(v.pos.x*2+1)<<0 & 0xff | (int)(v.pos.y*2+1)<<8 & 0xff00 | (int)(v.pos.z*2+1)<<16 & 0xff0000
                    select new uint4( x, y, z, w )
                    ;

                buffer.SetData( q.Select(x=>math.asfloat(x)).ToArray() );

                return buffer;
            }
            
            ComputeBuffer createGridShaderBuffer_( int maxGridLength )
            {
                var buffer = new ComputeBuffer( maxGridLength * 2, Marshal.SizeOf<uint4>(), ComputeBufferType.Constant );

                return buffer;
            }



            Mesh createMesh_()
            {
                var mesh_ = new Mesh();
                mesh_.name = "marching cube unit";

                var qVtx =
                    from i in Enumerable.Range( 0, 12 )
                    select new Vector3( i % 3, i / 3, 0 )
                    ;
                var qIdx =
                    from i in Enumerable.Range( 0, 3 * 4 )
                    select i
                    ;
                mesh_.vertices = qVtx.ToArray();
                mesh_.triangles = qIdx.ToArray();

                return mesh_;
            }
            
        }


    }





    public struct IndirectArgumentsForInstancing
    {
        public uint MeshIndexCount;
        public uint InstanceCount;
        public uint MeshBaseIndex;
        public uint MeshBaseVertex;
        public uint BaseInstance;

        public IndirectArgumentsForInstancing
            ( Mesh mesh, int instanceCount = 0, int submeshId = 0, int baseInstance = 0 )
        {
            //if( mesh == null ) return;

            this.MeshIndexCount = mesh.GetIndexCount( submeshId );
            this.InstanceCount = (uint)instanceCount;
            this.MeshBaseIndex = mesh.GetIndexStart( submeshId );
            this.MeshBaseVertex = mesh.GetBaseVertex( submeshId );
            this.BaseInstance = (uint)baseInstance;
        }

        public NativeArray<uint> ToNativeArray( Allocator allocator )
        {
            var arr = new NativeArray<uint>( 5, allocator );
            arr[ 0 ] = this.MeshIndexCount;
            arr[ 1 ] = this.InstanceCount;
            arr[ 2 ] = this.MeshBaseIndex;
            arr[ 3 ] = this.MeshBaseVertex;
            arr[ 4 ] = this.BaseInstance;
            return arr;
        }
    }

    static public class IndirectArgumentsExtensions
    {
        static public ComputeBuffer SetData( this ComputeBuffer cbuf, ref IndirectArgumentsForInstancing args )
        {
            using( var nativebuf = args.ToNativeArray( Allocator.Temp ) )
                cbuf.SetData( nativebuf );

            return cbuf;
        }

        static public ComputeBuffer SetData( this ComputeBuffer cbuf, ref IndirectArgumentsForDispatch args )
        {
            using( var nativebuf = args.ToNativeArray( Allocator.Temp ) )
                cbuf.SetData( nativebuf );

            return cbuf;
        }
    }

    static public class ComputeShaderUtility
    {
        static public ComputeBuffer CreateIndirectArgumentsBufferForInstancing() =>
            new ComputeBuffer( 1, sizeof( uint ) * 5, ComputeBufferType.IndirectArguments, ComputeBufferMode.Immutable );

        static public ComputeBuffer CreateIndirectArgumentsBufferForDispatch() =>
            new ComputeBuffer( 1, sizeof( int ) * 3, ComputeBufferType.IndirectArguments, ComputeBufferMode.Immutable );

        //static public void SetConstantBuffer( this Material mat, string name, ComputeBuffer buffer ) =>
        //    mat.SetConstantBuffer( name, buffer, 0, buffer.stride * buffer.count );
        static public void SetConstantBuffer( this Material mat, string name, ComputeBuffer buffer )
        {
            var arr = new Vector4[ buffer.stride / Marshal.SizeOf<Vector4>() * buffer.count ];
            buffer.GetData( arr );
            mat.SetVectorArray( name, arr );
        }
    }



    public struct IndirectArgumentsForDispatch
    {
        public int x, y, z;

        public IndirectArgumentsForDispatch( int numx, int numy, int numz )
        {
            this.x = numx;
            this.y = numy;
            this.z = numz;
        }

        public NativeArray<int> ToNativeArray( Allocator allocator )
        {
            var arr = new NativeArray<int>( 3, allocator );
            arr[ 0 ] = this.x;
            arr[ 1 ] = this.y;
            arr[ 2 ] = this.z;
            return arr;
        }
    }


}

