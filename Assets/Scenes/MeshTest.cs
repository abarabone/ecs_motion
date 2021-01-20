using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Abarabone.Utilities;
using Abarabone.Geometry;

public class MeshTest : MonoBehaviour
{
    public Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(this.name);
        this.mesh = this.gameObject.AsEnumerable()
            .BuildCombiner<UI16, PositionUvVertex>(this.transform)()
            .CreateMesh();

        GetComponent<MeshFilter>().mesh = this.mesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
