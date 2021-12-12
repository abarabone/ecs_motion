using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Authoring;

namespace DotsLite.HeightField
{
    public class HeightFieldAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField] PhysicsCategoryTags belongsTo;
        [SerializeField] PhysicsCategoryTags collidesWith;
        //[SerializeField] int groupIndex;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var terrain = GetComponent<Terrain>();

            if (terrain == null)
            {
                Debug.LogError("No terrain found!");
                return;
            }

            CollisionFilter collisionFilter = new CollisionFilter
            {
                BelongsTo = belongsTo.Value,
                CollidesWith = collidesWith.Value,
                //GroupIndex = groupIndex
            };

            dstManager.AddComponentData(entity,
                CreateTerrainCollider(terrain.terrainData, collisionFilter));
        }

        static PhysicsCollider CreateTerrainCollider(TerrainData terrainData, CollisionFilter filter)
        {
            var physicsCollider = new PhysicsCollider();

            //var size = new int2(terrainData.heightmapWidth, terrainData.heightmapHeight);
            var size = new int2(terrainData.heightmapTexture.width, terrainData.heightmapTexture.height);
            var scale = terrainData.heightmapScale;Debug.Log(size);

            var colliderHeights = new NativeArray<float>(size.x * size.y, Allocator.TempJob);

            var terrainHeights = terrainData.GetHeights(0, 0, size.x, size.y);


            for (int j = 0; j < size.y; j++)
                for (int i = 0; i < size.x; i++)
                {
                    var h = terrainHeights[i, j];
                    colliderHeights[j + i * size.x] = h;
                }

            var meshtype = Unity.Physics.TerrainCollider.CollisionMethod.VertexSamples;
            physicsCollider.Value = Unity.Physics.TerrainCollider.Create(colliderHeights, size, scale, meshtype, filter);

            colliderHeights.Dispose();

            return physicsCollider;
        }
    }
}
