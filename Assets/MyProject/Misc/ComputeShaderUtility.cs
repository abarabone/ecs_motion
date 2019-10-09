using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Collections;

namespace Abss.Cs
{
	
    /// <summary>
    /// コンピュートシェーダ
    /// ISimpleComputeBuffer を継承したバッファをセット可能。
    /// </summary>
	public struct SimpleComputeShaderUnit
	{
		public readonly ComputeShader	shader;
		public readonly int				kernelIndex;
		
		public SimpleComputeShaderUnit( ComputeShader shader, string kernelName )
		{
			this.shader			= shader;
			this.kernelIndex	= shader.FindKernel( kernelName );
		}

		public void SetBuffer<TBuffer>( TBuffer simpleComputeBuffer )
			where TBuffer:ISimpleComputeBuffer
		{
			shader.SetBuffer( kernelIndex, simpleComputeBuffer.Id, simpleComputeBuffer.Buffer );
		}
		
		public void Dispatch( int freqx, int freqy = 1, int freqz = 1 )
		{
			shader.Dispatch( kernelIndex, freqx, freqy, freqz );
		}
	}
	

    // コンピュートバッファ ----------------------------------------------------------------

	public interface ISimpleComputeBuffer
	{
		int				Id		{ get; }
		ComputeBuffer	Buffer	{ get; }
	}

	public struct SimpleComputeBuffer<T> : System.IDisposable, ISimpleComputeBuffer
		where T:struct
	{
		public int				Id		{ get; }
		public ComputeBuffer	Buffer	{ get; }

		public void Dispose()
        {
            if( this.Buffer != null ) this.Buffer.Release();
        }

		public SimpleComputeBuffer( string name, int bufferLength )
		{
			this.Id		= UnityEngine.Shader.PropertyToID( name );
			this.Buffer	= new ComputeBuffer( bufferLength, Marshal.SizeOf<T>() );
		}
		public SimpleComputeBuffer( string name, NativeArray<T> data )
			:this( name, data.Length )
		{
			this.Buffer.SetData( data );
		}
        
        /// <summary>SetBuffer( ComputeBuffer ) に直接渡せるように暗黙変換</summary>
        public static implicit operator ComputeBuffer( SimpleComputeBuffer<T> scb ) => scb.Buffer;
	}
	
	public struct SimpleAppendBuffer<T> : System.IDisposable, ISimpleComputeBuffer
		where T:struct
	{
		public int				Id		{ get; }
		public ComputeBuffer	Buffer	{ get; }
		
		public void Dispose()
        {
            if( this.Buffer != null ) this.Buffer.Release();
        }

		public SimpleAppendBuffer( string name, NativeArray<T> data ) : this( name, data.Length )
		{
			this.Buffer.SetData( data );
		}
		public SimpleAppendBuffer( string name, int bufferLength )
		{
			this.Id		= UnityEngine.Shader.PropertyToID( name );
			this.Buffer	= new ComputeBuffer( bufferLength, Marshal.SizeOf<T>(), ComputeBufferType.Append );
		}
	}
    
    // ----------------------------------------------------------------

    
    // 引数バッファ ----------------------------------------------------------------

	public struct SimpleIndirectArgsBuffer : System.IDisposable
	{
		public ComputeBuffer	Buffer { get; private set; }
		
		public void Dispose()
        {
            if( this.Buffer != null ) this.Buffer.Release();
        }


		public SimpleIndirectArgsBuffer( Mesh mesh, uint instanceCount = 0, int submeshId = 0 )
        {
            using( var args = new InstancingIndirectArguments( mesh, instanceCount, submeshId ) )
            { 
                this = new SimpleIndirectArgsBuffer( args );
            }
        }
        public SimpleIndirectArgsBuffer( InstancingIndirectArguments arguments )
        {
			this.Buffer	= new ComputeBuffer( 1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments );
            
            this.Buffer.SetData<uint>( arguments );
        }

        public void CreateBuffer() =>
            this.Buffer	= new ComputeBuffer( 1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments );

        
        /// <summary>Draw/Dispatch...Indirect() に直接渡せるように暗黙変換</summary>
        public static implicit operator ComputeBuffer( SimpleIndirectArgsBuffer siab ) => siab.Buffer;
	}

    // 引数バッファに渡すパラメータ - - - - - - - - - - - - - - - - - - -
    public struct InstancingIndirectArguments : System.IDisposable
    {
        NativeArray<uint> indirectArguments;
        public NativeArray<uint> Arguments { get => this.indirectArguments; }

        public uint MeshIndexCount { get => this.indirectArguments[0]; set => this.indirectArguments[0] = value; }
        public uint InstanceCount { get => this.indirectArguments[1]; set => this.indirectArguments[1] = value; }
        public uint MeshBaseIndex { get => this.indirectArguments[2]; set => this.indirectArguments[2] = value; }
        public uint MeshBaseVertex { get => this.indirectArguments[3]; set => this.indirectArguments[3] = value; }
        public uint BaseInstance { get => this.indirectArguments[4]; set => this.indirectArguments[4] = value; }

		public void Dispose()
        {
            if( this.indirectArguments.IsCreated ) this.indirectArguments.Dispose();
        }

        public InstancingIndirectArguments( Mesh mesh, uint instanceCount = 0, int submeshId = 0, Allocator allocator = Allocator.Temp )
        {
            this.indirectArguments = new NativeArray<uint>( 5, allocator, NativeArrayOptions.ClearMemory );

            if( mesh == null ) return;

            this.MeshIndexCount = mesh.GetIndexCount( submeshId );
            this.InstanceCount = instanceCount;
            this.MeshBaseIndex = mesh.GetIndexStart( submeshId );
            this.MeshBaseVertex = mesh.GetBaseVertex( submeshId );
        }
        
        /// <summary>SetData( NativeArray ) に直接渡せるように暗黙変換</summary>
        public static implicit operator NativeArray<uint>( InstancingIndirectArguments iai ) => iai.Arguments;
    }
    // - - - - - - - - - - - - - - - - - - - - - - -
    
    // ----------------------------------------------------------------

    
    public struct IdFromName
    {
        public readonly int Id;

        public IdFromName( string propName ) => this.Id = Shader.PropertyToID( propName );
    }


	public static class ComputeShaderExtension
	{
		public static void SetBuffer<TBuffer>( this Material mat, TBuffer simpleComputeBuffer )
			where TBuffer:ISimpleComputeBuffer
		{
			mat.SetBuffer( simpleComputeBuffer.Id, simpleComputeBuffer.Buffer );
		}
        
	}

}
