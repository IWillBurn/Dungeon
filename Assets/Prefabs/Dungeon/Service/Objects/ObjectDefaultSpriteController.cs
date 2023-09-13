using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class ObjectDefaultSpriteController : MonoBehaviour
{
    public SpriteRenderer sprite_renderer;
    public void Start()
    {
        sprite_renderer = gameObject.GetComponent<SpriteRenderer>();
    }

    public void Enable() { sprite_renderer.enabled = true; }
    public void Disable() { sprite_renderer.enabled = false; }

    public void SetAlpha(float alpha){
        sprite_renderer.material.SetFloat("_MainAlpha", alpha);
    }

    public void SetDrawBorder(float draw_border)
    {
        sprite_renderer.material.SetFloat("_DrawBorder", draw_border);
    }

    public void SetOrder(int order)
    {
        sprite_renderer.sortingOrder = order;
    }

    public void SetBorderColor(Color color)
    {
        sprite_renderer.material.SetColor("_BorderColor", color);
    }
}