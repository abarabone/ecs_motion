using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DotsLite.Structure.Authoring
{
    using DotsLite.Geometry;

    public interface IStructurePart
    {
        int partId { get; set; }
        IEnumerable<IMeshModel> QueryModel { get; }
    }
}
