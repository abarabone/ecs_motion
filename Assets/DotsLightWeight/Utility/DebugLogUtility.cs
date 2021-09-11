using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace DotsLite.Utility.Log.Show
{
    public static class _
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void _log(string msg) => Debug.Log(msg);
    }
}

namespace DotsLite.Utility.Log.NoShow
{
    public static class _
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void _log(string msg) { }
    }
}