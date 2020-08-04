using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using Unity.Transforms;

namespace Abarabone.Geometry
{
    using Plane = Unity.Physics.Plane;


    public struct ViewFrastum
    {

        float4 up;
        float4 down;
        float4 right;
        float4 left;


        public ViewFrastum(Camera cam)
        {
            var mt = cam.projectionMatrix;
            var m0 = mt.GetRow(0);
            var m1 = mt.GetRow(1);
            var m3 = mt.GetRow(3);
            this.up = m3 - m1;
            this.down = m3 + m1;
            this.right = m3 - m0;
            this.left = m3 + m0;
        }


        public bool IsInside(AABB bbox)
        {
            
            
            var ud = inside_distance_(bbox, this.up);
            var dd = inside_distance_(bbox, this.down);
            var ld = inside_distance_(bbox, this.left);
            var rd = inside_distance_(bbox, this.right);

            var areInside = new float4(ud, dd, ld, rd) >= 0.0f;

            return math.all(areInside);


            float inside_distance_(AABB bbox_, float4 pl)
            {

                var p_or_n = math.sign(pl.xyz);
                var pn = p_or_n * bbox_.Extents + bbox_.Center;

                return math.dot(pl, new float4(pn, 1.0f));
            }
        }

        public void TransformToLocal(Rotation rot, Translation pos)
        {
            var rgtf = new RigidTransform(rot.Value, pos.Value);
            var rgtf_inv = math.inverse(rgtf);

            this.up = math.transform(rgtf_inv, math.transform( this.up);
        }
    }
}
