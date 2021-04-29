using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DotsLite.Physics.Authoring;

namespace DotsLite.Physics.Authoring
{
    using DotsLite.Model.Authoring;

    [CustomEditor(typeof(HitQueryOverlapSphereAuthoring))]
    public class OverlapSphereHandle : Editor
    {
        public void OnSceneGUI()
        {
            HitQueryOverlapSphereAuthoring t = this.target as HitQueryOverlapSphereAuthoring;

            Handles.matrix = Matrix4x4.Scale(Vector3.one * 0.5f);

            EditorGUI.BeginChangeCheck();
            var center = Handles.PositionHandle(t.Center * 2.0f, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "overlap sphere move center");
                t.Center = center * 0.5f;

                if (t.Useage == HitQueryOverlapSphereAuthoring.UseageType.RayForWalling)
                {
                    var top = t.GetComponentInParent<CharacterModelAuthoring>();
                    if (top != null)
                    {
                        var p = top.transform.position;
                        t.Center.x = p.x;
                        t.Center.z = p.z;
                    }
                }

                return;
            }

            Handles.matrix = Matrix4x4.identity;

            EditorGUI.BeginChangeCheck();
            float radius = Handles.RadiusHandle(Quaternion.identity, t.Center, t.Radius);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "overlap sphere change radius");
                t.Radius = radius;

                return;
            }
        }
    }
}

