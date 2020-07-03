using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Physics;

using Abarabone.Geometry;

/// <summary>
/// そのうち公式のやつを利用するようにしたい、物理系全般
/// 問題は余計なコンポーネントデータが付くこと
/// </summary>
[RequireComponent(typeof(CharacterJoint))]
public class RagdollJointAuthoring : MonoBehaviour
{

    public Rigidbody connectedBody;

    public float3 positionAinA;
    public float3 positionBinB { get => calcPosBinB( this.positionAinA ); }
    public float3 twistAxisInA;
    public float3 twistAxisInB { get => calcAxisBinB( this.twistAxisInA ); }
    public float3 perpendicularAxisInA;
    public float3 perpendicularAxisInB { get => calcAxisBinB( this.perpendicularAxisInA ); }

    public System.Single maxConeAngle;
    public System.Single minTwistAngle;
    public System.Single maxTwistAngle;
    public System.Single minPerpendicularAngle;
    public System.Single maxPerpendicularAngle;

    private void Awake()
    {
        var chj = this.GetComponent<CharacterJoint>();
        chj.connectedBody = this.connectedBody;
    }

    float3 calcPosBinB( float3 posAinA )
    {
        var worldFromA = new RigidTransform( this.transform.rotation, this.transform.position );
        var worldFromB = ( this.connectedBody == null )
            ? RigidTransform.identity
            : new RigidTransform( this.connectedBody.transform.rotation, this.connectedBody.transform.position );
        
        var bFromA = math.mul( math.inverse( worldFromB ), worldFromA );

        return math.transform( bFromA, posAinA );
    }
    float3 calcAxisBinB( float3 axisAinA )
    {
        var worldFromA = new quaternion( this.transform.rotation.As_float4() );
        var worldFromB = ( this.connectedBody == null )
            ? quaternion.identity
            : new quaternion(this.connectedBody.transform.rotation.As_float4());

        var bFromA = math.mul( math.inverse( worldFromB ), worldFromA );

        return math.mul( bFromA, axisAinA );
    }
}
