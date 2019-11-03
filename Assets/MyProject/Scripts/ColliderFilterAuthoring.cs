using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Authoring;
using System;

/// <summary>
/// コライダーの衝突フラグを設定する。
/// コライダー自身と同じ game object から辿って、親を探す。
/// </summary>
public class ColliderFilterAuthoring : MonoBehaviour
{

    public PhysicsCategoryTags BelongsTo;
    public PhysicsCategoryTags CollidesWith;

}
