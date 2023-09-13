using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EntityParametersKeys
{
    SPEED = 0,
    VIEW_DIRECTION = 1,
    HAND_SIZE = 2,
}

public class EntityBehaviour : MonoBehaviour
{
    [SerializeField] Sprite[] sprites;
    public EntitiesDefaultSpriteController sprite_controller;

    public EntityController controller;
    public DungeonObjectCell cell;

    public float[] parameters = new float[100];

    private void Start()
    {
        
    }
}
