using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Linq;
using System.Linq;
using System;
using Unity.Mathematics;

namespace DotsLite.Authoring
{
    [Serializable]
    public struct BinaryLength2
    {
        public binary_length u;
        public binary_length v;
        public static implicit operator int2(BinaryLength2 src) => new int2((int)src.u, (int)src.v);
    }
    public enum binary_length
    {
        length_1 = 1,
        length_2 = 2,
        length_4 = 4,
        length_8 = 8,
        length_16 = 16,
        //length_32 = 32,
        //length_64 = 64,
        //length_128 = 128,
        //length_256 = 256,
    }
}
namespace DotsLite.Model.Authoring
{
    public static class FindObjectExtension
    {
        public static T FindParent<T>(this MonoBehaviour x)
            where T : MonoBehaviour
        =>
            x.gameObject
                .AncestorsAndSelf()
                .Where(x => x.GetComponent<T>() != null)
                .First()
                .GetComponent<T>();
    }

}
