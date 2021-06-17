using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Aaa// : IAaa
{
    public int i;
}
public interface IAaa
{ }

public class testcomponent : MonoBehaviour
{

    [SerializeReference]
    public Aaa aaa
        = new Aaa
        {
            i = 1
        };


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
