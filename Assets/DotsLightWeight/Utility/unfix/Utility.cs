using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System;
using System.Linq;
using UnityEngine;
using Unity.Linq;
using Unity.Collections;
using Unity.Mathematics;

using Abarabone.Common.Extension;
using Abarabone.Geometry;

namespace Abarabone.Utilities
{
    static public class Extentions
    {

        /// <summary>
        /// 
        /// </summary>
        public static bool IsSingle<T>(this IEnumerable<T> src) => src.Any() && src.Skip(1).IsEmpty();


		/// <summary>
		/// 値を取得、keyがなければデフォルト値を設定し、デフォルト値を取得
		/// </summary>
		public static TV GetOrDefault<TK, TV>(this Dictionary<TK, TV> dic, TK key,TV defaultValue = default(TV))
		{
			return dic.TryGetValue(key, out var result) ? result : defaultValue;
		}

        static public IEnumerable<string> MakePath( this IEnumerable<GameObject> gameObjects, GameObject root )
        {
            return gameObjects.Select( go => go.MakePath(root) );
        }
        static public IEnumerable<string> MakePath( this IEnumerable<GameObject> gameObjects )
		{
			return gameObjects.Select( go => go.MakePath() );
		}

        static public string MakePath( this GameObject gameObject, GameObject root )
        {
            if (root == gameObject) return "";

            var offset = root.MakePath().Length;
            if (offset == 0) return gameObject.MakePath();
            
            return gameObject.MakePath().Substring( offset + 1 );
        }
        static public string MakePath( this GameObject gameObject )
        {
            var qNames = gameObject.AncestorsAndSelf().Reverse().Skip(1).Select(x => x.name);

            return string.Join( "/", qNames );
        }

        static public string GetParentPath( this string path )
        {
            var len = path.LastIndexOf( "/" );
            
            return len == -1 ? "": path.Substring( 0, len );
        }

        static public HashSet<string> ToEnabledBoneHashSet( this AvatarMask boneMask_ )
        {
            var qEnabledBonePaths =
                from id in Enumerable.Range( 0, boneMask_.transformCount )
                let isEnabled = boneMask_.GetTransformActive( id )
                where isEnabled
                let path = boneMask_.GetTransformPath(id)
                select path
                ;

            return new HashSet<string>( qEnabledBonePaths );
        }

        // Misc.ToNativeArray() だと大丈夫なのに、こちらだとハングするケースがある、なぜ？？ unsafe がらみ？
        //public static NativeArray<T> ToNativeArray<T>( this IEnumerable<T> src, Allocator allocator )
        //    where T : struct
        //{
        //    var arr = new NativeArray<T>( src.Count(), allocator, NativeArrayOptions.UninitializedMemory );
        //    var i = 0;
        //    //foreach( var x in src.Select((item,i)=>(i,item)) )
        //    foreach( var x in src )
        //    {
        //        arr[ i++ ] = x;
        //    }
        //    return arr;
        //}
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
    }
    static public class ComputeShaderUtility
    {
        static public ComputeBuffer CreateIndirectArgumentsBuffer() =>
            new ComputeBuffer( 1, sizeof( uint ) * 5, ComputeBufferType.IndirectArguments, ComputeBufferMode.Immutable );
    }


    /// <summary>
    /// 方向と長さを、4 floats で管理する。
    /// </summary>
    public struct DirectionAndLength
    {
        public float4 value;

        public float3 Ray { get => this.Direction * this.Length; }
        public float3 Direction { get => value.As_float3(); set => this.value = new float4(value, this.value.w); }
        public float Length { get => value.w; set => this.value.w = value; }
    }
}


namespace Abarabone.Utilities
{
    using Unity.Collections.LowLevel.Unsafe;

    public static unsafe class NativeUtility
    {
        public static NativeArray<T> AsNativeArray<T>(this UnsafeList<T> list)
            where T : unmanaged
        {
            var arr = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(list.Ptr, list.length, Allocator.Invalid);

            // これをやらないとNativeArrayのインデクサアクセス時に死ぬ
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref arr, AtomicSafetyHandle.Create());
#endif

            return arr;
        }
    }
}