using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace DotsLite.ParticleSystem.Aurthoring
{
    using DotsLite.Model;
    using DotsLite.Draw;
    using DotsLite.Model.Authoring;
    using DotsLite.Draw.Authoring;
    using DotsLite.Geometry;
    using DotsLite.Common.Extension;
    using DotsLite.Authoring;

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
        public int LineParticleSegments;

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
            //if (!this.isActiveAndEnabled) { conversionSystem.DstEntityManager.DestroyEntity(entity); return; }


            var tex = this.Texture.As() ?? this.PackTexture();

            switch (this.ParticleType)
            {
                case ParticleMeshType.billboadUv:
                    {
                        var mesh = this.createBillboadMesh();
                        initModelEntity_(conversionSystem, entity, this.gameObject, this.DrawShader, mesh, tex, BoneType.P1uv);
                        addParamComponents_(conversionSystem, entity, this.Division);
                    }
                    break;
                case ParticleMeshType.psyllium:
                    {
                        var mesh = this.createPsylliumMesh();
                        initModelEntity_(conversionSystem, entity, this.gameObject, this.DrawShader, mesh, tex, BoneType.PtoP);
                    }
                    break;
                case ParticleMeshType.psylliumUv:
                    {
                        var mesh = this.createPsylliumMesh();
                        initModelEntity_(conversionSystem, entity, this.gameObject, this.DrawShader, mesh, tex, BoneType.PtoPuv);
                        addParamComponents_(conversionSystem, entity, this.Division);
                    }
                    break;
                case ParticleMeshType.LinePsyllium:
                    {
                        var mesh = this.createLineParticleMesh_(this.LineParticleSegments + 1, isPsylliumEdge: true);
                        var bonetype = (this.LineParticleSegments + 1 + 1).ToBoneType();
                        initModelEntity_(conversionSystem, entity, this.gameObject, this.DrawShader, mesh, tex, bonetype);
                    }
                    break;
                case ParticleMeshType.LineBillboad:
                    {
                        var mesh = this.createLineParticleMesh_(this.LineParticleSegments + 1, isPsylliumEdge: false);
                        var bonetype = (this.LineParticleSegments + 1 + 1).ToBoneType();
                        initModelEntity_(conversionSystem, entity, this.gameObject, this.DrawShader, mesh, tex, bonetype);
                    }
                    break;
                default:
                    break;
            }

            return;


            static void initModelEntity_(
                GameObjectConversionSystem gcs, Entity entity, GameObject main,
                Shader shader, Mesh mesh, Texture tex, BoneType bonetype)
            {
                var mat = new Material(shader);
                mat.mainTexture = tex;
                //mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");

                const int boneLength = 1;
                const DrawModel.SortOrder order = DrawModel.SortOrder.acs;

                gcs.InitDrawModelEntityComponents(entity, mesh, mat, bonetype, boneLength, order);
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="isPsylliumEdge">真ならＺ型、偽ならエ型のメッシュを生成する。</param>
        Mesh createLineParticleMesh_(int pointNodeLength, bool isPsylliumEdge)
        {

            const float h = 0.5f;
            const float w = 0.5f;
            const float d = 0.5f;

            Mesh mesh = new Mesh();

            mesh.vertices = queryVtx().SelectMany().ToArray();
            mesh.uv = queryUv().SelectMany().ToArray();
            mesh.colors = queryPointNodeIndex().SelectMany().ToArray();
            mesh.triangles = queryTriangleIndex().SelectMany().ToArray();

            return mesh;



            IEnumerable<Vector3[]> queryVtx()
            {
                var startEdgeVtxs = new[] { new Vector3(-w, 0f, -d), new Vector3(+w, 0f, -d) };
                var nodeVtxs = new[] { new Vector3(-w, 0f, 0f), new Vector3(+w, 0f, 0f) };
                var endEdgeVtxs = new[] { new Vector3(-w, 0f, +d), new Vector3(+w, 0f, +d) };

                var qVtx = Enumerable
                    .Repeat(nodeVtxs, pointNodeLength)
                    .Prepend(startEdgeVtxs)
                    .Append(endEdgeVtxs)
                    ;

                if (isPsylliumEdge)
                {
                    return qVtx;
                }
                else
                {
                    return qVtx
                        .Prepend(endEdgeVtxs)
                        .Append(startEdgeVtxs)
                        //.Select( (x,i) => new[] { x.First() + Vector3.up*i, x.Last() + Vector3.up*i } )
                        ;
                }
            }

            IEnumerable<Vector2[]> queryUv()
            {
                var startEdgeUvs = new[] { new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f) };
                var nodeUvs = new[] { new Vector2(0.0f, 0.5f), new Vector2(1.0f, 0.5f) };
                var endEdgeUvs = new[] { new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f) };

                var qUv = Enumerable
                    .Repeat(nodeUvs, pointNodeLength)
                    .Prepend(startEdgeUvs)
                    .Append(endEdgeUvs)
                    ;

                if (isPsylliumEdge)
                {
                    return qUv;
                }
                else
                {
                    return qUv
                        .Prepend(endEdgeUvs)
                        .Append(startEdgeUvs)
                        ;
                }
            }

            IEnumerable<Color[]> queryPointNodeIndex()
            {
                var lastNode = pointNodeLength - 1;
                var qNodeIdxSingle = Enumerable.Range(1, pointNodeLength - 2)
                    .Select(i => new Color(i, i, i - 1, 0))
                    .Prepend(new Color(0, 0, 0, 0))
                    .Prepend(new Color(0, 0, 0, 0))
                    .Append(new Color(lastNode, lastNode - 1, lastNode - 1, 0))
                    .Append(new Color(lastNode, lastNode - 1, lastNode - 1, 0))
                    ;

                if (isPsylliumEdge)
                {
                    return (qNodeIdxSingle, qNodeIdxSingle).Zip((l, r) => new[] { l, r });
                }
                else
                {
                    var q = qNodeIdxSingle
                        .Prepend(new Color(0, 0, 0, 0))
                        .Append(new Color(lastNode, lastNode - 1, lastNode - 1, 0))
                        ;
                    return (q, q).Zip((l, r) => new[] { l, r });
                }
            }

            IEnumerable<IEnumerable<int>> queryTriangleIndex()
            {
                var planeTris = new[]
                {
                    0, 2, 1,
                    2, 3, 1,
                };

                var qTri = Enumerable
                    .Repeat(planeTris, 2 + pointNodeLength - 1)
                    ;

                if (isPsylliumEdge)
                {
                    return qTri
                        .Select((tri, i) => tri.Select(x => x + i * 2))
                        ;
                }
                else
                {
                    // ここはだいぶ難解になってしまった…
                    var firstPlaneTris = new[]
                    {
                        4, 0, 5,
                        0, 1, 5,
                    };
                    var lastPlaneTris = new[]
                    {
                        2, -2, 3,
                        -2, -1, 3,
                    };
                    return qTri
                        .Prepend(firstPlaneTris)
                        .Append(lastPlaneTris)
                        .Select((tri, i) => tri.Select(x => x + i * 2))
                        ;
                    ;
                }
            }

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
