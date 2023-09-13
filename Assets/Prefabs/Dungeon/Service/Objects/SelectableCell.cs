using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectableCell : Selectable
{

    override public void SetSelected(bool _selected)
    {
        selected = _selected;
        if (selected) sprite_controller.SetAlpha(0.5f);
        else sprite_controller.SetAlpha(1f);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}