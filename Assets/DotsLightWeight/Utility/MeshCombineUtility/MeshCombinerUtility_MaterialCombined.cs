using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Linq;
using System.Threading.Tasks;
using System;
using Abarabone.Common.Extension;

namespace Abarabone.Geometry
{

	/// <summary>
	/// 
	/// </summary>
	public static class MaterialCombined
	{
		/// <summary>
		/// 
		/// </summary>
		public static IEnumerable<Material> QueryCombine(IEnumerable<Material[]> materials_PerObject)
		{
			var qMats = materials_PerObject
				.SelectMany()
				.Distinct()
				;
				//from mat in materials_PerObject.SelectMany().Distinct()
				//	//	orderby mat.name
				//select mat
				//;

			return qMats;
		}
	}

}
