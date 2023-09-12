using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickable : Selectable
{
    bool picked = false;

    public void SetPicked(bool _picked, Transform _parent)
    {
        picked = _picked;
        SetSelected(false);
        transform.parent = _parent;
    }
}
