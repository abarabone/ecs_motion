using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisByAttribute : PropertyAttribute
{
    public string fieldname;

    public VisByAttribute(string fieldname)
    {
        this.fieldname = fieldname;
    }
}
