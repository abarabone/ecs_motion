using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Entities;


namespace DotsLite.Structure
{

    public static class StructureUtility
    {



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ChangeToNear
            (
                this DynamicBuffer<LinkedEntityGroup> children,
                EntityCommandBuffer.ParallelWriter cmd, int uniqueIndex,
                ComponentDataFromEntity<Part.PartData> partData
            )
        {

            cmd.AddComponent<Disabled>(uniqueIndex, children[2].Value);


            for (var i = 3; i < children.Length; i++)
            {
                var child = children[i].Value;
                if (!partData.HasComponent(child)) continue;

                cmd.RemoveComponent<Disabled>(uniqueIndex, children[i].Value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ChangeToFar
            (
                this DynamicBuffer<LinkedEntityGroup> children,
                EntityCommandBuffer.ParallelWriter cmd, int uniqueIndex,
                ComponentDataFromEntity<Part.PartData> partData
            )
        {

            cmd.RemoveComponent<Disabled>(uniqueIndex, children[2].Value);


            for (var i = 3; i < children.Length; i++)
            {
                var child = children[i].Value;
                if (!partData.HasComponent(child)) continue;

                cmd.AddComponent<Disabled>(uniqueIndex, children[i].Value);
            }
        }





        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ChangeComponentsToSleep(
            this EntityCommandBuffer.ParallelWriter cmd, Entity entity, int uniqueIndex,
            Main.BinderLinkData binder,
            ComponentDataFromEntity<Part.PartData> parts,
            BufferFromEntity<LinkedEntityGroup> linkedGroups)
        {
            cmd.RemoveComponent<Unity.Physics.PhysicsVelocity>(uniqueIndex, entity);

            var children = linkedGroups[binder.BinderEntity];
            children.RemoveComponentsToAllBones<Model.Bone.TransformTargetTag>(cmd, uniqueIndex, parts);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ChangeComponentsToWakeUp(
            this EntityCommandBuffer.ParallelWriter cmd, Entity entity, int uniqueIndex,
            Main.BinderLinkData binder,
            ComponentDataFromEntity<Part.PartData> parts,
            BufferFromEntity<LinkedEntityGroup> linkedGroups)
        {
            cmd.AddComponent<Unity.Physics.PhysicsVelocity>(uniqueIndex, entity);

            var children = linkedGroups[binder.BinderEntity];
            children.AddComponentsToAllBones<Model.Bone.TransformTargetTag>(cmd, uniqueIndex, parts);
        }




        /// <summary>
        /// far および near parts すべてのエンティティに、コンポーネントを付加する。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddComponentsToAllBones<T>
            (
                this DynamicBuffer<LinkedEntityGroup> children,
                EntityCommandBuffer.ParallelWriter cmd, int uniqueIndex,
                ComponentDataFromEntity<Part.PartData> partData
            )
            where T : struct, IComponentData
        {

            cmd.AddComponent<T>(uniqueIndex, children[2].Value);


            for (var i = 3; i < children.Length; i++)
            {
                var child = children[i].Value;
                if (!partData.HasComponent(child)) continue;

                cmd.AddComponent<T>(uniqueIndex, children[i].Value);
            }
        }

        /// <summary>
        /// far および near parts すべてのエンティティから、コンポーネントを削除する。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponentsToAllBones<T>
            (
                this DynamicBuffer<LinkedEntityGroup> children,
                EntityCommandBuffer.ParallelWriter cmd, int uniqueIndex,
                ComponentDataFromEntity<Part.PartData> partData
            )
            where T : struct, IComponentData
        {

            cmd.RemoveComponent<T>(uniqueIndex, children[2].Value);


            for (var i = 3; i < children.Length; i++)
            {
                var child = children[i].Value;
                if (!partData.HasComponent(child)) continue;

                cmd.RemoveComponent<T>(uniqueIndex, children[i].Value);
            }
        }

    }
}
