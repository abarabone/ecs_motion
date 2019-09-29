using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System;
using System.Linq;
using UnityEngine;
using Unity.Linq;
using Unity.Collections;

namespace Abss.Utilities
{
    static public class Extentions
    {
        
		/// <summary>
		/// 値を取得、keyがなければデフォルト値を設定し、デフォルト値を取得
		/// </summary>
		public static TV GetOrDefault<TK, TV>(this Dictionary<TK, TV> dic, TK key,TV defaultValue = default(TV))
		{
			return dic.TryGetValue(key, out var result) ? result : defaultValue;
		}

		static public IEnumerable<string> MakePath( this IEnumerable<GameObject> gameObjects )
		{
			return gameObjects
				.Select( go => string.Join( "/", go.AncestorsAndSelf().Reverse().Skip(1).Select(x => x.name) ) )
				;
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
}

