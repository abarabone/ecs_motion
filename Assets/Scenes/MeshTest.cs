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
        var tex = this.gameObject.QueryUniqueTextures().ToAtlasOrPassThroughAndParameters();
        this.mesh = this.gameObject//.Do(x => Debug.Log(x))
            .BuildCombiner<UI32, PositionNormalUvBonedVertex>(this.transform, tex.ToTexHashToUvRectFunc())
            .ToTask().Result
            .CreateMesh();

        GetComponent<MeshFilter>().mesh = this.mesh;
        var r = GetComponent<Renderer>();
        var mat = new Material(r.material);
        mat.mainTexture = tex.atlas;
        r.material = mat;
    }

}
