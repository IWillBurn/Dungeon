using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDefaultSpriteController : MonoBehaviour
{
    [SerializeField] SpriteRenderer sprite_renderer;
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
}