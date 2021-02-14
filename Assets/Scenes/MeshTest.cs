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
        var b = this.GetComponentInChildren<SkinnedMeshRenderer>().bones;

        Debug.Log(this.name);
        var tex = this.gameObject.QueryUniqueTextures().ToAtlasOrPassThroughAndParameters();
        var mmts = this.gameObject.QueryMeshMatsTransform_IfHaving().ToArray();
        var qMeshSrc = mmts.QueryMeshDataWithDisposingLast();
        this.mesh = mmts//.Do(x => Debug.Log(x))
            .BuildCombiner<UI32, PositionNormalUvBonedVertex>(this.transform, qMeshSrc, tex.ToTexHashToUvRectFunc(), b)
            .ToTask().Result
            .CreateMesh();

        GetComponent<MeshFilter>().mesh = this.mesh;
        var r = GetComponent<Renderer>();
        var mat = new Material(r.material);
        mat.mainTexture = tex.atlas;
        r.material = mat;
    }

}
