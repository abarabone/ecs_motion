using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Physics;

public class RagdollJointAuthoring : MonoBehaviour
{

    public Rigidbody connectedBody;

    public float3 positionAinA;
    public float3 positionBinB;
    public float3 twistAxisInA;
    public float3 twistAxisInB;
    public float3 perpendicularAxisInA;
    public float3 perpendicularAxisInB;

    public System.Single maxConeAngle;
    public System.Single minPerpendicularAngle;
    public System.Single maxPerpendicularAngle;
    public System.Single minTwistAngle;
    public System.Single maxTwistAngle;

}
