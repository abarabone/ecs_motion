using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace DotsLite.Structure.Authoring
{
    public class ShowMeshElementEditor : MonoBehaviour
    {
        //[MenuItem("Assets/Show Mesh Attributes")]
        static public void command()
        {
            var mesh = Selection.activeObject as Mesh;
            if (mesh == null) return;
            
            var q =
                from attr in Enumerable.Range(0, 14).Cast<VertexAttribute>()//.OfType<VertexAttribute>()
                where mesh.HasVertexAttribute(attr)
                let format = mesh.GetVertexAttributeFormat(attr)
                let dimension = mesh.GetVertexAttributeDimension(attr)
                select $"{attr} {format} {dimension}"
                ;

            Debug.Log(string.Join("\n", q.Prepend(mesh.name)));
        }
    }
}
