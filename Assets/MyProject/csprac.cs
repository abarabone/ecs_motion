﻿using UnityEngine;
using System.Collections;
using Unity.Collections;

public class csprac : MonoBehaviour {
    public int instanceCount = 100000;
    public Mesh instanceMesh;
    public Material instanceMaterial;
    public int subMeshIndex = 0;

    private int cachedInstanceCount = -1;
    private int cachedSubMeshIndex = -1;
    private ComputeBuffer positionBuffer;
    private ComputeBuffer argsBuffer;
    //private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    //private NativeArray<uint> args;

    public int freq = 1;

    void Start() {
        argsBuffer = new ComputeBuffer( freq, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        UpdateBuffers();
    }

    void Update() {
        // Update starting position buffer
        if (cachedInstanceCount != instanceCount || cachedSubMeshIndex != subMeshIndex)
            UpdateBuffers();

        // Pad input
        if (Input.GetAxisRaw("Horizontal") != 0.0f)
            instanceCount = (int)Mathf.Clamp(instanceCount + Input.GetAxis("Horizontal") * 40000, 1.0f, 5000000.0f);

        // Render
        Graphics.DrawMeshInstancedIndirect(instanceMesh, subMeshIndex, instanceMaterial, new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)), argsBuffer);
    }

    void OnGUI() {
        GUI.Label(new Rect(265, 25, 200, 30), "Instance Count: " + instanceCount.ToString());
        instanceCount = (int)GUI.HorizontalSlider(new Rect(25, 20, 200, 30), (float)instanceCount, 1.0f, 5000000.0f);
    }

    void UpdateBuffers() {
        // Ensure submesh index is in range
        if (instanceMesh != null)
            subMeshIndex = Mathf.Clamp(subMeshIndex, 0, instanceMesh.subMeshCount - 1);

        // Positions
        if (positionBuffer != null)
            positionBuffer.Release();
        positionBuffer = new ComputeBuffer(instanceCount * freq, 16);
        Vector4[] positions = new Vector4[instanceCount * freq];
        for (int i = 0; i < instanceCount * freq; i++) {
            float angle = Random.Range(0.0f, Mathf.PI * 2.0f);
            float distance = Random.Range(20.0f, 100.0f);
            float height = Random.Range(-2.0f, 2.0f);
            float size = Random.Range(0.05f, 0.25f);
            positions[i] = new Vector4(Mathf.Sin(angle) * distance, height, Mathf.Cos(angle) * distance, size);
            //positions[i] = new Vector4(1,0,0) * i;
            //positions[i].w = 1;
        }
        positionBuffer.SetData(positions);
        instanceMaterial.SetBuffer("positionBuffer", positionBuffer);

        // Indirect args
        var args = new NativeArray<uint>( 5 * freq, Allocator.Temp, NativeArrayOptions.ClearMemory );
        if (instanceMesh != null) {
            for( var ii=0; ii<freq; ii++ )
            {
                args[ii*5+0] = (uint)instanceMesh.GetIndexCount(subMeshIndex);// / 4;
                args[ii*5+1] = (uint)instanceCount;
                args[ii*5+2] = (uint)instanceMesh.GetIndexStart(subMeshIndex);// / 4 + (uint)ii * instanceMesh.GetIndexCount(subMeshIndex) / 4;
                args[ii*5+3] = (uint)instanceMesh.GetBaseVertex(subMeshIndex);
            }
        }
        else
        {
            args[0] = args[1] = args[2] = args[3] = 0;
        }
        argsBuffer.SetData(args);
        args.Dispose();

        cachedInstanceCount = instanceCount;
        cachedSubMeshIndex = subMeshIndex;
    }

    void OnDisable() {
        if (positionBuffer != null)
            positionBuffer.Release();
        positionBuffer = null;

        if (argsBuffer != null)
            argsBuffer.Release();
        argsBuffer = null;
    }
}