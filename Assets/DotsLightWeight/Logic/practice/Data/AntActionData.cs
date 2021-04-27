using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Entities;
using Unity.Transforms;
//using Unity.Rendering;
using Unity.Properties;
using Unity.Burst;
using Unity.Physics;

using Abarabone.Utilities;

namespace Abarabone.Character
{

    public static partial class AntAction
    {


        public struct WalkState : IComponentData
        {
            public int Phase;
        }

        public struct AttackState : IComponentData
        {
            public int Phase;
        }

        // ébíËÅ@äJî≠ÇµÇ‚Ç∑Ç≥ÇÃÇΩÇﬂÇ…Ç¬Ç≠Ç¡ÇΩ
        public struct AttackTimeRange : IComponentData
        {
            public float st;
            public float ed;
        }

        public struct DamageState : IComponentData
        {
            public int Phase;
        }

    }


}
