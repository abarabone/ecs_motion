using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Collections;

namespace Abss.Cs
{
	

	public struct SimpleComputeShaderUnit
	{
		public readonly ComputeShader	shader;
		public readonly int				kernelIndex;
		
		public SimpleComputeShaderUnit( string name, ComputeShader shader )
		{
			this.shader			= shader;
			this.kernelIndex	= shader.FindKernel( name );
		}

		public void SetBuffer<TBuffer>( TBuffer simpleComputeBuffer )
			where TBuffer:ISimpleComputeBuffer
		{
			shader.SetBuffer( kernelIndex, simpleComputeBuffer.id, simpleComputeBuffer.buffer );
		}
		
		public void Dispatch( int freqx, int freqy = 1, int freqz = 1 )
		{
			shader.Dispatch( kernelIndex, freqx, freqy, freqz );
		}
	}
	

	public interface ISimpleComputeBuffer
	{
		int				id		{ get; }
		ComputeBuffer	buffer	{ get; }
	}

	public struct SimpleComputeBuffer<T> : System.IDisposable, ISimpleComputeBuffer
		where T:struct
	{
		public int				id		{ get; }
		public ComputeBuffer	buffer	{ get; }

		public void Dispose() => this.buffer.Release();
		
		public SimpleComputeBuffer( string name, int bufferLength )
		{
			this.id		= UnityEngine.Shader.PropertyToID( name );
			this.buffer	= new ComputeBuffer( bufferLength, Marshal.SizeOf<T>() );
		}
		public SimpleComputeBuffer( string name, NativeArray<T> data )
			:this( name, data.Length )
		{
			this.buffer.SetData( data );
		}
	}
	
	public struct SimpleAppendBuffer<T> : System.IDisposable, ISimpleComputeBuffer
		where T:struct
	{
		public int				id		{ get; }
		public ComputeBuffer	buffer	{ get; }
		
		public void Dispose() => this.buffer.Release();
		
		public SimpleAppendBuffer( string name, NativeArray<T> data ) : this( name, data.Length )
		{
			this.buffer.SetData( data );
		}
		public SimpleAppendBuffer( string name, int bufferLength )
		{
			this.id		= UnityEngine.Shader.PropertyToID( name );
			this.buffer	= new ComputeBuffer( bufferLength, Marshal.SizeOf<T>(), ComputeBufferType.Append );
		}
	}

	public struct SimpleIndirectArgsBuffer : System.IDisposable//, ISimpleComputeBuffer
	{
	//	public int				id		{ get; }
		public ComputeBuffer	buffer	{ get; }
		
		public void Dispose() => this.buffer.Release();
		
		//public SimpleIndirectArgsBuffer( string name, Mesh mesh = null )
		public SimpleIndirectArgsBuffer( Mesh meshSample = null )
		{
			var buf = new SimpleIndirectArgsManualyBuffer( meshSample );
		//	this.id		= buf.id;
			this.buffer	= buf.buffer;
		}

		public void CopyCount<TBuffer>( TBuffer appendBuffer )
			where TBuffer:ISimpleComputeBuffer
		{
			ComputeBuffer.CopyCount( appendBuffer.buffer, this.buffer, 0 );//これちがうんじゃないか？
		}
	}
	
	public unsafe struct SimpleIndirectArgsManualyBuffer : System.IDisposable//, ISimpleComputeBuffer
	{
	//	public int				id		{ get; }
		public ComputeBuffer	buffer	{ get; }
		internal uint[]			args	{ get; }
		
		public uint	InstanceCount { set { args[1] = value; buffer.SetData( this.args ); } }

		public void Dispose() => this.buffer.Release();
		
		//public SimpleIndirectArgsManualyBuffer( string name, Mesh mesh = null )
		public SimpleIndirectArgsManualyBuffer( Mesh meshSample = null )
		{
		//	this.id		= UnityEngine.Shader.PropertyToID( name );
			this.buffer	= new ComputeBuffer( 1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments );
			this.args	= createArgsArray();
			this.buffer.SetData( this.args );
			return;

			uint[] createArgsArray()
			{
				if( meshSample == null ) return new uint [] { 0, 0, 0, 0, 0 };

				return new uint []
				{
					meshSample.GetIndexCount( 0 ),
					0,	// インスタンスカウント
					meshSample.GetIndexStart( 0 ),
					meshSample.GetBaseVertex( 0 ),
					0	// 開始インスタンス位置
				};
			}
		}
	}


	public static class ComputeShaderExtension
	{
		public static void SetBuffer<TBuffer>( this Material mat, TBuffer simpleComputeBuffer )
			where TBuffer:ISimpleComputeBuffer
		{
			mat.SetBuffer( simpleComputeBuffer.id, simpleComputeBuffer.buffer );
		}
	}

}
