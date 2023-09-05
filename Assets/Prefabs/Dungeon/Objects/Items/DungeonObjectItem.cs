using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonObjectItem : DungeonObject
{
    public int size_w, size_h;

    [SerializeField] ObjectDefaultSpriteController sprite_controller;

    private void Start()
    {
        sprite_controller.SetDrawBorder(1);
    }

    public static void GenerateDungeonObject(int position_x, int position_y, ref Dungeon dungeon)
    {
        float start_x = (dungeon.size_w) / 2f;
        float start_y = (dungeon.size_h) / 2f;
        float create_x = 0.5f * (position_x - start_x) - 0.5f * (position_y - start_y) + draw_delta[DrawPositions.ITEM].x;
        float create_y = -0.25f * (position_x - start_x) - 0.25f * (position_y - start_y) - 0.5f + draw_delta[DrawPositions.ITEM].y;
        ObjectsKeys object_key = dungeon.dungeon_map.map[position_x][position_y].item.object_key;
        if (object_key == ObjectsKeys.NONE) return;
        GameObject obj = Instantiate(dungeon.dictionary.GetByKey(object_key), dungeon.gameObject.transform, false);
        obj.transform.position = new Vector3(create_x, create_y, 0);
        obj.GetComponent<SpriteRenderer>().sortingOrder = position_x + position_y;
        dungeon.dungeon_map.map[position_x][position_y].item.component = obj.GetComponent<DungeonObjectItem>();
        dungeon.dungeon_map.map[position_x][position_y].item.component.in_map_coordinate_x = position_x;
        dungeon.dungeon_map.map[position_x][position_y].item.component.in_map_coordinate_y = position_y;
    }

    public override void Redraw(int position_x, int position_y, ref Dungeon dungeon)
    {
        float start_x = dungeon.size_w / 2f;
        float start_y = dungeon.size_h / 2f;
        float create_x = 0.5f * (position_x - start_x) - 0.5f * (position_y - start_y) + dungeon.transform.position.x;
        float create_y = -0.25f * (position_x - start_x) - 0.25f * (position_y - start_y) - 0.5f + dungeon.transform.position.y;
        transform.position = new Vector3(create_x, create_y, 0);
        GetComponent<SpriteRenderer>().sortingOrder = position_x + position_y;
    }

    public override void SetStatusOpened()
    {
        sprite_controller.Enable();
        sprite_controller.SetAlpha(1f);
    }

    public override void SetStatusShadowed()
    {
        sprite_controller.Enable();
        sprite_controller.SetAlpha(0.5f);
    }

    public override void SetStatusClosed()
    {
        sprite_controller.Disable();
    }

}
