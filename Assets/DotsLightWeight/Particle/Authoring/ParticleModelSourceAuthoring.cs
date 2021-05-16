using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace DotsLite.Particle.Aurthoring
{
    using DotsLite.Model;
    using DotsLite.Draw;
    using DotsLite.Model.Authoring;
    using DotsLite.Draw.Authoring;
    using DotsLite.Geometry;

    /// <summary>
    /// 他メッシュとのアトラス対応は後回し
    /// </summary>
    public class ParticleModelSourceAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        public Shader DrawShader;
        public Texture2D Texture;

        public length_define DivisionU;
        public length_define DivisionV;
        public enum length_define
        {
            length_1 = 1,
            length_2 = 2,
            length_4 = 4,
            length_8 = 8,
            length_16 = 16,
            //length_32 = 32,
            //length_64 = 64,
            //length_128 = 128,
            //length_256 = 256,
        }

        [SerializeField]
        public SrcTexture[] SrcTexutres;
        [Serializable]
        public struct SrcTexture
        {
            public Texture2D texuture;
            public int2 indexOfLeftTop;
            public int2 cellUsage;
        }

        public int2 TextureSize;
        public TextureFormat TextureFormat;
        public bool UseMipmap;
        public bool UseLinear;
        public string OutputPath;


        /// <summary>
        /// パーティクル共通で使用するモデルエンティティを作成する。
        /// 最終的に prefab コンポーネントを削除する。（ unity の想定と違って歪みがありそう…）
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            var tex = this.Texture ?? this.PackTexture();
            var mesh = this.createMesh();
            createModelEntity_(conversionSystem, entity, this.gameObject, this.DrawShader, mesh, tex);

            return;


            static void createModelEntity_
                (GameObjectConversionSystem gcs, Entity entity, GameObject main, Shader shader, Mesh mesh, Texture tex)
            {
                var mat = new Material(shader);
                mat.mainTexture = tex;

                const BoneType BoneType = BoneType.P1bb;
                const int boneLength = 1;

                gcs.InitDrawModelEntityComponents(main, entity, mesh, mat, BoneType, boneLength);
            }

            void addParamComponents_(GameObjectConversionSystem gcs, Entity ent)
            {
                var em = gcs.DstEntityManager;

                var div = new uint2((uint)this.DivisionU, (uint)this.DivisionV);

                em.AddComponentData(ent, new BillboadModel.UvInformationData
                {
                    Division = div,
                });
                em.AddComponentData(ent, new BillboadModel.IndexToUvData
                {
                    CellSpan = new float2(1.0f) / div,
                });
            }

        }

        Mesh createMesh()
        {

            float height = 0.5f;// 1.0f;
            float width = 0.5f;// 1.0f;

            Mesh mesh = new Mesh();
            mesh.name = "particle";

            mesh.vertices = new Vector3[]
            {
                new Vector3 (-width, height, 0),     // 0
                new Vector3 (width, height, 0),           // 1
                new Vector3 (-width , -height, 0),     // 2
                new Vector3 (width , -height, 0),           // 3
            };

            mesh.uv = new Vector2[]
            {
                new Vector2 (0, 0),
                new Vector2 (1, 0),
                new Vector2 (0, 1),
                new Vector2 (1, 1),
            };

            mesh.triangles = new int[]
            {
                0, 1, 2,
                1, 3, 2,
            };

            return mesh;
        }

        public Texture2D PackTexture()
        {
            var size = this.TextureSize;
            var div = new int2((int)this.DivisionU, (int)this.DivisionV);
            var span = size / div;

            var fmt = this.TextureFormat;
            var mm = this.UseMipmap;
            var ln = this.UseLinear;

            var texture = new Texture2D(size.x, size.y, fmt, mm, ln);

            foreach (var src in this.SrcTexutres)
            {
                var rdtsize = span * src.cellUsage;
                var rdt = RenderTexture.GetTemporary(rdtsize.x, rdtsize.y);
                RenderTexture.active = rdt;

                Graphics.Blit(src.texuture, rdt);

                var std = src.indexOfLeftTop;
                var rev = div - src.cellUsage - src.indexOfLeftTop;
                var idx = new int2(std.x, rev.y);
                var dstoffset = idx * span;
                texture.ReadPixels(new Rect(0, 0, rdt.width, rdt.height), dstoffset.x, dstoffset.y);

                RenderTexture.active = null;
                RenderTexture.ReleaseTemporary(rdt);
            }

            texture.Apply();

            return texture;

            //System.IO.File.WriteAllBytes(this.OutputPath, texture.EncodeToPNG());

            //AssetDatabase.Refresh();

            //return AssetDatabase.LoadAssetAtPath<Texture2D>(savePath);
        }
    }


    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    public class RemovePrefabComponentsConversion : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        { }
        //{
        //    var em = this.DstEntityManager;

        //    this.Entities
        //        .WithAll<DrawModel.GeometryData, Prefab>()
        //        .ForEach(ent =>
        //        {
        //            em.RemoveComponent<Prefab>(ent);
        //        });
        //}
        protected override void OnDestroy()
        {
            var em = this.DstEntityManager;

            var desc0 = new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(DrawModel.GeometryData),
                    typeof(Prefab)
                }
            };
            using var q = em.CreateEntityQuery(desc0);

            using var ents = q.ToEntityArray(Allocator.Temp);
            foreach (var ent in ents)
            {
                em.RemoveComponent<Prefab>(ent);
            }
        }
    }

}
