using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using Unity.Transforms;
using System.Runtime.CompilerServices;

namespace Abarabone.Geometry
{
    using Plane = Unity.Physics.Plane;


    public struct ViewFrustum
    {

        public float4 up;
        public float4 down;
        public float4 right;
        public float4 left;


        public ViewFrustum(Camera cam)
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


        public bool IsInside(AABB bbox)//, Rotation rot, Translation pos, NonUniformScale sca)
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
    }

    public struct ViewFrustumSoa
    {
        float4_soa4 plane;

        public ViewFrustumSoa(Camera cam)
        {
            var vf = new ViewFrustum(cam);

            var pl = new float4x4(vf.up, vf.down, vf.left, vf.right);
            var tpl = math.transpose(pl);
            this.plane = new float4_soa4
            {
                x = tpl.c0,
                y = tpl.c1,
                z = tpl.c2,
                w = tpl.c3,
            };
        }

        public bool4 IsInside(AABB bbox, Rotation rot, Translation pos, NonUniformScale scl)
        {
            var pl = this.plane;

            var ir = math.inverse(rot.Value).value.ExpandToSoa4();
            var t = pos.Value.ExpandToSoa4();

            var n = rotate(ir, pl.xyz);
            var d = pl.w + dot(pl.xyz, t);


            var c = bbox.Center.ExpandToSoa4();
            var es = (bbox.Extents * scl.Value).ExpandToSoa4();

            var p = new float3_soa4()
            {
                x = c.x + es.x * math.sign(n.x),
                y = c.y + es.y * math.sign(n.y),
                z = c.z + es.z * math.sign(n.z),
            };
            
            return dot(p, n) + d >= 0.0f;
        }

        float3_soa4 rotate(float4_soa4 q, float3_soa4 v)
        {
            var qv = cross(q.xyz, v);
            var t = new float3_soa4
            {
                x = 2.0f * qv.x,
                y = 2.0f * qv.y,
                z = 2.0f * qv.z,
            };
            var qt = cross(q.xyz, t);
            var o = new float3_soa4
            {
                x = q.w * t.x + qt.x,
                y = q.w * t.y + qt.y,
                z = q.w * t.z + qt.z,
            };
            return o;
        }
        float3_soa4 cross(float3_soa4 l, float3_soa4 r) =>
            new float3_soa4
            {
                x = l.y * r.z - l.z * r.y,
                y = l.z * r.x - l.x * r.z,
                z = l.x * r.y - l.y * r.x,
            };
        float4 dot(float3_soa4 l, float3_soa4 r) =>
            l.x * r.x + l.y * r.y + l.z * r.z;
    }

    struct float3_soa4
    {
        public float4 x;
        public float4 y;
        public float4 z;
    }
    struct float4_soa4
    {
        public float4 x;
        public float4 y;
        public float4 z;
        public float4 w;

        public float3_soa4 xyz =>
            new float3_soa4
            {
                x = this.x,
                y = this.y,
                z = this.z,
            };
    }

    static class ViewCullingExtension
    {
        static public float3_soa4 ExpandToSoa4(this float3 src) =>
            new float3_soa4
            {
                x = src.xxxx,
                y = src.yyyy,
                z = src.zzzz,
            };
        static public float4_soa4 ExpandToSoa4(this float4 src) =>
            new float4_soa4
            {
                x = src.xxxx,
                y = src.yyyy,
                z = src.zzzz,
                w = src.wwww,
            };
    }
}
