using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.IO;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace DotsLite.MarchingCubes.Data
{

    using DotsLite.Draw;
    using DotsLite.Utilities;


    public static partial class CubeDrawModel
    {
        public struct GridTypeData : IComponentData
        {
            public int UnitOnEdge;
        }
        public class MakeCubesShaderResourceData : IComponentData
        {
            public ComputeShader MakeCubesShader;

            public Resource.GridBitLinesDataBuffer GridBitLines;
            public Resource.CubeInstancingShaderBuffer CubeInstances;
            
            public Resource.GridCubeIdShaderBufferTexture CubeIds;

            public int XLineLengthPerGrid;


            public void Alloc(InitializeData init)
            {
                this.Dispose();
                this.GridBitLines = Resource.GridBitLinesDataBuffer.Create(init.maxGridBufferLength, init.unitOnEdge);
                this.CubeInstances = Resource.CubeInstancingShaderBuffer.Create(init.maxCubeInstance);

                if (!init.useNormal) return;
                this.CubeIds = Resource.GridCubeIdShaderBufferTexture.Create(init.maxGridInstructions, init.unitOnEdge);
            }
            public void Dispose()
            {
                if (GridBitLines.Buffer == null) return;
                Debug.Log("cube draw model dispose");

                this.GridBitLines.Dispose();
                this.CubeInstances.Dispose();
                this.CubeIds.Dispose();
            }
        }
        public class GridIdSeedData : IComponentData
        {
            public int NextId;
        }

        public class InitializeData : IComponentData
        {
            public MarchingCubesAsset asset;
            public ComputeShader cubeMakeShader;
            public Material material;

            public int maxGridBufferLength;
            public int maxCubeInstance;
            public int maxGridInstructions;
            public int unitOnEdge;

            public bool useNormal;
        }
    }




    namespace Resource
    {

        public struct GridBitLinesDataBuffer : IDisposable
        {
            public ComputeBuffer Buffer { get; private set; }

            public static GridBitLinesDataBuffer Create(int maxGrids, int unitOnEdge) => new GridBitLinesDataBuffer
            {
                Buffer = new ComputeBuffer(unitOnEdge * unitOnEdge / (32 / unitOnEdge) * maxGrids, Marshal.SizeOf<uint>()),
            };

            public void Dispose()
            {
                this.Buffer?.Release();
                this.Buffer = null;
            }
        }

        public struct CubeInstancingShaderBuffer : IDisposable
        {
            public ComputeBuffer Buffer { get; private set; }

            public static CubeInstancingShaderBuffer Create(int maxCubeInstances) => new CubeInstancingShaderBuffer
            {
                Buffer = new ComputeBuffer(maxCubeInstances, Marshal.SizeOf<uint>(), ComputeBufferType.Append),
            };

            public void Dispose()
            {
                this.Buffer?.Release();
                this.Buffer = null;
            }
        }



        public struct GridCubeIdShaderBufferTexture : IDisposable
        {
            public RenderTexture Texture { get; private set; }

            public void Dispose()
            {
                this.Texture?.Release();
                this.Texture = null;
            }

            public static GridCubeIdShaderBufferTexture Create(int maxGridInstructions, int unitOnEdge)
            {
                var n = unitOnEdge;
                var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R8_UInt;
                //var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R32_UInt;
                var buffer = new RenderTexture(n * n, n, 0, format, 0);
                buffer.enableRandomWrite = true;
                buffer.dimension = TextureDimension.Tex2DArray;
                buffer.volumeDepth = maxGridInstructions;
                buffer.Create();

                return new GridCubeIdShaderBufferTexture
                {
                    Texture = buffer,
                };
            }
        }
    }
}

