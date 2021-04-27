using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Linq;
using System.Linq;

namespace Abarabone.Model.Authoring
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
