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
        var objs = this.GetComponentsInChildren<Transform>().Select(x => x.gameObject);
        var tex = objs.QueryUniqueTextures().PackTextureAndQueryHashAndUvRect();
        this.mesh = objs//.Do(x => Debug.Log(x))
            .BuildCombiner<UI32, PositionUvVertex>(this.transform, tex)
            .ToTask().Result
            .CreateMesh();

        GetComponent<MeshFilter>().mesh = this.mesh;
        var r = GetComponent<Renderer>();
        var mat = new Material(r.material);
        mat.mainTexture = tex.atlas;
        r.material = mat;
    }

}
