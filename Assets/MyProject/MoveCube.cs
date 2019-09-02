using UnityEngine;

public class MoveCube: MonoBehaviour
{
    [SerializeField]
    private ComputeShader m_ComputeShader;

    [SerializeField]
    private Transform m_Cube;

    private ComputeBuffer m_Buffer;

    void Start ()
    {
        m_Buffer = new ComputeBuffer(1, sizeof(float));
        m_ComputeShader.SetBuffer(0, "Result", m_Buffer);
    }

    void Update ()
    {
        m_ComputeShader.SetFloat("positionX", m_Cube.position.x);

        m_ComputeShader.Dispatch(0, 8, 8, 1);

        var data = new float[1];
        m_Buffer.GetData(data);

        float positionX = data[0];

        var boxPosition = m_Cube.position;
        boxPosition.x = positionX;
        m_Cube.position = boxPosition;
    }

    private void OnDestroy()
    {
        m_Buffer.Release();
    }
}