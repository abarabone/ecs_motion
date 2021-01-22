using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Linq;
using Abarabone.Utilities;
using Abarabone.Geometry;
using Abarabone.Common.Extension;

public class MeshTest : MonoBehaviour
{
    public Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(this.name);
        this.mesh = this.gameObject.ChildrenAndSelf().Do(x => Debug.Log(x))
            .BuildCombiner<UI32, PositionUvVertex>(this.transform)
            .ToTask().Result
            .CreateMesh();

        GetComponent<MeshFilter>().mesh = this.mesh;
    }

}
