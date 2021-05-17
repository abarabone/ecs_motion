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
    using DotsLite.Common.Extension;

    /// <summary>
    /// 他メッシュとのアトラス対応は後回し
    /// </summary>
    public class ParticleModelSourceAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        public Shader DrawShader;
        public Texture2D Texture;

        //public bool UseRoundMesh;

        public ParticleMeshType ParticleType;

        public BinaryLength2 Division;

        [SerializeField]
        public SrcTexture[] SrcTexutres;
        [Serializable]
        public struct SrcTexture
        {
            public Texture2D texuture;
            public int2 indexOfLeftTop;
            public BinaryLength2 cellUsage;
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

            var tex = this.Texture.As() ?? this.PackTexture();

            switch (this.ParticleType)
            {
                case ParticleMeshType.billboadUv:
                    {
                        var mesh = this.createBillboadMesh();
                        createModelEntity_(conversionSystem, entity, this.gameObject, this.DrawShader, mesh, tex, BoneType.P1uv);
                        addParamComponents_(conversionSystem, entity, this.Division);
                    }
                    break;
                case ParticleMeshType.psyllium:
                    {
                        var mesh = this.createPsylliumMesh();
                        createModelEntity_(conversionSystem, entity, this.gameObject, this.DrawShader, mesh, tex, BoneType.PtoP);
                    }
                    break;
                case ParticleMeshType.psylliumUv:
                    {
                        var mesh = this.createPsylliumMesh();
                        createModelEntity_(conversionSystem, entity, this.gameObject, this.DrawShader, mesh, tex, BoneType.PtoPuv);
                        addParamComponents_(conversionSystem, entity, this.Division);
                    }
                    break;
                default:
                    break;
            }

            return;


            static void createModelEntity_(
                GameObjectConversionSystem gcs, Entity entity, GameObject main,
                Shader shader, Mesh mesh, Texture tex, BoneType bonetype)
            {
                var mat = new Material(shader);
                mat.mainTexture = tex;

                const int boneLength = 1;

                gcs.InitDrawModelEntityComponents(main, entity, mesh, mat, bonetype, boneLength);
            }

            static void addParamComponents_(
                GameObjectConversionSystem gcs, Entity ent, BinaryLength2 div)
            {
                var em = gcs.DstEntityManager;

                var types = new ComponentTypes(new ComponentType[]
                {
                    typeof(BillboadModel.UvInformationData),
                    typeof(BillboadModel.IndexToUvData),
                });
                em.AddComponents(ent, types);
                
                em.SetComponentData(ent, new BillboadModel.UvInformationData
                {
                    Division = (uint2)(int2)div,
                });
                em.SetComponentData(ent, new BillboadModel.IndexToUvData
                {
                    CellSpan = new float2(1.0f) / (int2)div,
                });
            }

        }

        Mesh createBillboadMesh()
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


        Mesh createPsylliumMesh()
        {

            float height = 0.5f;// 1.0f;
            float width = 0.5f;// 1.0f;
            float radius = width;

            Mesh mesh = new Mesh();
            mesh.name = "psyllium";

            mesh.vertices = new Vector3[]
            {
            new Vector3 (-width, -height, -radius),     // 0
            new Vector3 (-width, -height, 0),           // 1
            new Vector3 (width , -height, -radius),     // 2
            new Vector3 (width , -height, 0),           // 3

            new Vector3 (-width,  height, 0),           // 4
            new Vector3 ( width,  height, 0),           // 5

            new Vector3 (-width,  height, radius),      // 6 
            new Vector3 (width ,  height, radius),      // 7

                //new Vector3 (-width,  height, -radius),     // 8
                //new Vector3 (width ,  height, -radius),     // 9
                //new Vector3 (-width, -height, radius),      // 10
                //new Vector3 (width , -height, radius),      // 11
            };

            mesh.uv = new Vector2[]
            {
            new Vector2 (0, 0),
            new Vector2 (0, 0.5f),
            new Vector2 (1, 0),
            new Vector2 (1, 0.5f),
            new Vector2 (0, 0.5f),
            new Vector2 (1, 0.5f),
            new Vector2 (0, 1),
            new Vector2 (1, 1),

                //new Vector2 (0, 0),
                //new Vector2 (1, 0),
                //new Vector2 (0, 1),
                //new Vector2 (1, 1),
            };

            //mesh.colors = new Color[]
            //{
            //    new Color (0, 0, 0, 0),
            //    new Color (0, 0, 0, 0),
            //    new Color (0, 0, 0, 0),
            //    new Color (0, 0, 0, 0),
            //    new Color (0, 0, 0, 0),
            //    new Color (0, 0, 0, 0),
            //    new Color (0, 0, 0, 0),
            //    new Color (0, 0, 0, 0),

            //    new Color (0, 0, 0, 0),
            //    new Color (0, 0, 0, 0),
            //    new Color (0, 0, 0, 0),
            //    new Color (0, 0, 0, 0),
            //};

            mesh.triangles = new int[]
            {
            0, 1, 2,
            1, 3, 2,
            1, 4, 3,
            4, 5, 3,
            4, 6, 5,
            6, 7, 5,

                // 8, 4, 9,
                // 4, 5, 9,
                // 1,10, 3,
                //10,11, 3
            };

            return mesh;
        }




        public Texture2D PackTexture()
        {
            var size = this.TextureSize;
            var div = (int2)this.Division;
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
                var idx = new int2(std.x, std.y);// rev.y);
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
