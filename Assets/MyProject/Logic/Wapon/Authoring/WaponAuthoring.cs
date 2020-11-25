using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Linq;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace Abarabone.Arms.Authoring
{
    using Abarabone.Model.Authoring;
    using Abarabone.Character;
    using Abarabone.Draw.Authoring;
    using Abarabone.Common.Extension;
    using Abarabone.Draw;
    using Abarabone.CharacterMotion;
    using Abarabone.Arms;
    using Unity.Physics.Authoring;
    using Abarabone.Model;

    /// <summary>
    /// WaponEntity はインスタンス化しない。
    /// FunctionUnit をインスタンス化するためのリファレンスでしかない。
    /// </summary>
    public partial class WaponAuthoring : MonoBehaviour, IWaponAuthoring//, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {

        public IFunctionUnitAuthoring MainUnit;
        public IFunctionUnitAuthoring SubUnit;

    }

}
