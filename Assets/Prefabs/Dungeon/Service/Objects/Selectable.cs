using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selectable : MonoBehaviour
{
    [SerializeField] protected ObjectDefaultSpriteController sprite_controller;
    protected bool selected = false;

    public bool IsSelected() { return selected; }

    virtual public void SetSelected(bool _selected) {
        selected = _selected;
        if (selected) sprite_controller.SetBorderColor(Color.white);
        else sprite_controller.SetBorderColor(Color.black);
    }
}
