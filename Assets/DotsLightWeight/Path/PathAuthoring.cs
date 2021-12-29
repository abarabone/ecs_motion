using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DotsLite.Path.Authoring
{
	public class PathAuthoring : MonoBehaviour
	{

		public Transform StartAnchor;
		public Transform EndAnchor;

		public Transform EffectStartSide;
		public Transform EffectEndSide;

		public int Freq;

		public GameObject ModelTopPrefab;


		void Awake()
		{
        
		}

	}
}
