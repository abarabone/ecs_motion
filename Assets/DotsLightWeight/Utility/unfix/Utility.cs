using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System;
using System.Linq;
using UnityEngine;
using Unity.Linq;
using Unity.Collections;
using Unity.Mathematics;

namespace DotsLite.Utilities
{
    using DotsLite.Common.Extension;
    using DotsLite.Geometry;


    static class DevUtil
    {

        public struct dispo : IDisposable
        {
            Action act;
            public dispo(Action act) => this.act = act;
            public void Dispose() => this.act();
        }

        public struct dispona : IDisposable
        {
            string name;
            IDisposable d;
            public dispona(IDisposable d, string name = null) { this.d = d; this.name = name ?? Time.time.ToString(); Debug.Log($"{this.name} dispo st"); }
            public void Dispose() { Debug.Log($"{this.name} dispo ed"); this.d.Dispose(); }
        }
    }


    static public class Extentions
    {

        public static IEnumerable<T> Logging<T>(this IEnumerable<T> src) =>
            src.Do(x => Debug.Log(x));

        public static IEnumerable<T> Logging<T>(this IEnumerable<T> src, Func<T, String> toString) =>
            src.Do(x => Debug.Log(toString(x)));




        public static IEnumerable<T> Using<T>(this IEnumerable<T> src)
        {
            using ((IDisposable)src) foreach (var e in src) yield return e;// ボクシングすると思う…
        }

        public static IEnumerable<T> UsingEach<T>(this IEnumerable<T> src) where T : IDisposable
        {
            foreach (var e in src)
            {
                using (e) yield return e;
            }
        }

        public static IEnumerable<T> WrapEnumerable<T>(this T src) =>
            new[] { src };
            //Enumerable.Repeat(src, 1);

        /// <summary>
        /// 
        /// </summary>
        public static bool IsSingle<T>(this IEnumerable<T> src) => src.Any() && src.Skip(1).IsEmpty();




        static public Dictionary<TKey, TValue> ToDictionary<TKey, TValue>
            (this (IEnumerable<TKey> keys, IEnumerable<TValue> values) src)
        =>
            src.Zip().ToDictionary(x => x.src0, x => x.src1);

        static public Dictionary<TKey, TValue> ToDictionaryOrNull<TKey, TValue>
            (this (IEnumerable<TKey> keys, IEnumerable<TValue> values) src)
        {
            if (src.keys == null || src.values == null) return null;

            return src.ToDictionary();
        }



        public static void AddRange<Tkey, Tvalue>
            (this Dictionary<Tkey, Tvalue> dict, IEnumerable<Tkey> keys, IEnumerable<Tvalue> values)
        {
            foreach (var (key, value) in (keys, values).Zip())
            {
                dict.Add(key, value);
            }
        }
        public static void AddRange<Tkey, Tvalue>
            (this Dictionary<Tkey, Tvalue> dict, IEnumerable<Tkey> keys, Tvalue value)
        {
            foreach (var key in keys)
            {
                dict.Add(key, value);
            }
        }



        /// <summary>
        /// 値を取得、keyがなければデフォルト値を設定し、デフォルト値を取得
        /// </summary>
        public static TV GetOrDefault<TK, TV>(this Dictionary<TK, TV> dic, TK key,TV defaultValue = default(TV))
		{
			return dic.TryGetValue(key, out var result) ? result : defaultValue;
		}

        //      static public IEnumerable<string> MakePath( this IEnumerable<GameObject> gameObjects, GameObject root )
        //      {
        //          return gameObjects.Select( go => go.MakePath(root) );
        //      }
        //      static public IEnumerable<string> MakePath( this IEnumerable<GameObject> gameObjects )
        //{
        //	return gameObjects.Select( go => go.MakePath() );
        //}

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

        static public IEnumerable<int> UpTo(this int start, int end) =>
            Enumerable.Range(start, end - start + 1);

        static public IEnumerable<int> Inc(this int start, int times) =>
            Enumerable.Range(start, times);
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

}


namespace DotsLite.Utilities
{
    using Unity.Collections.LowLevel.Unsafe;

    public static unsafe class NativeUtility
    {
        static public NativeArray<T> AsNativeArray<T>(this UnsafeList<T> list)
            where T : unmanaged
        {
            var arr = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(list.Ptr, list.length, Allocator.Invalid);

        #if ENABLE_UNITY_COLLECTIONS_CHECKS
            // これをやらないとNativeArrayのインデクサアクセス時に死ぬ
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref arr, AtomicSafetyHandle.Create());
        #endif

            return arr;
        }

        public static NativeArray<T> PtrToNativeArray<T>(T* ptr, int length)
            where T : unmanaged
        {
            var arr = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(ptr, length, Allocator.Invalid);

        #if ENABLE_UNITY_COLLECTIONS_CHECKS
            // これをやらないとNativeArrayのインデクサアクセス時に死ぬ
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref arr, AtomicSafetyHandle.Create());
        #endif

            return arr;
        }

        static public IEnumerable<T> Range<T>(this NativeArray<T> src, int start, int length) where T : struct =>
            from i in Enumerable.Range(start, length) select src[i];
    }
}