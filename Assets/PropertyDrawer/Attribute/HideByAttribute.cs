using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideByAttribute : PropertyAttribute
{
    public string fieldname;

    public HideByAttribute(string fieldname)
    {
        this.fieldname = fieldname;
    }
}
