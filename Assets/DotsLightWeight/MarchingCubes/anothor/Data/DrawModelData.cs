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

namespace DotsLite.MarchingCubes.another.Data
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

            public Resource.GridBitLinesDataBuffer BitLinesPerGrid;
            public Resource.CubeInstancingShaderBuffer CubeInstances;
            
            public Resource.GridCubeIdShaderBufferTexture CubeIds;


            public void Dispose()
            {
                Debug.Log("cube draw model dispose");
                this.BitLinesPerGrid.Dispose();
                this.CubeInstances.Dispose();
                this.CubeIds.Dispose();
            }
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

            public static GridCubeIdShaderBufferTexture Create(int maxGridInstances, int unitOnEdge)
            {
                var n = unitOnEdge;
                var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R8_UInt;
                //var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R32_UInt;
                var buffer = new RenderTexture(n * n, n, 0, format, 0);
                buffer.enableRandomWrite = true;
                buffer.dimension = TextureDimension.Tex2DArray;
                buffer.volumeDepth = maxGridInstances;
                buffer.Create();

                return new GridCubeIdShaderBufferTexture
                {
                    Texture = buffer,
                };
            }
        }
    }
}

