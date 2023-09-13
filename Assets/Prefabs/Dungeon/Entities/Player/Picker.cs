using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Picker : MonoBehaviour
{
    [SerializeField] Selecter selecter;

    Pickable picked;
    DungeonObjectContainer<DungeonObjectItem> item_data;
    DungeonObjectContainer<DungeonObjectFloor> floor_data;
    DungeonObjectContainer<DungeonObjectWall> wall_data;

    void Start()
    {
        picked = null;
        item_data = null;
        wall_data = null;
    }

    void Pick()
    {
        if (selecter.selected != null)
        {
            picked = selecter.selected.GetComponent<Pickable>();
            picked.SetPicked(true, transform);
            selecter.finding = FindingObjects.CELL;
            selecter.selected = null;
            UpdateStorage();
        }
    }

    void UpdateStorage()
    {
        item_data = null;
        floor_data = null;
        wall_data = null;
        DungeonObject dungeon_object = picked.GetComponent<DungeonObject>();
        if (picked.GetComponent<DungeonObjectItem>())
        {
            DungeonObjectContainer<DungeonObjectItem> new_item_data = selecter.entity.controller.GetDungeonController().GetDugneon().dungeon_map.map[dungeon_object.in_map_coordinate_x][dungeon_object.in_map_coordinate_y].item;
            if (new_item_data != null)
            {
                item_data = new DungeonObjectContainer<DungeonObjectItem>(new_item_data.object_key, new_item_data.component);
            }
            selecter.entity.controller.GetDungeonController().GetDugneon().dungeon_map.map[dungeon_object.in_map_coordinate_x][dungeon_object.in_map_coordinate_y].item.object_key = ObjectsKeys.NONE;
            selecter.entity.controller.GetDungeonController().GetDugneon().dungeon_map.map[dungeon_object.in_map_coordinate_x][dungeon_object.in_map_coordinate_y].item.component = null;
        }
    }

    void Drop()
    {
        if (selecter.selected != null)
        {
            Coordinate storage_position = new Coordinate(selecter.selected.GetComponent<DungeonObjectCell>().in_map_coordinate_x, selecter.selected.GetComponent<DungeonObjectCell>().in_map_coordinate_y);
            if (item_data != null && selecter.entity.controller.GetDungeonController().GetDugneon().dungeon_map.map[storage_position.x][storage_position.y].item.object_key == ObjectsKeys.NONE) {
                selecter.entity.controller.GetDungeonController().GetDugneon().dungeon_map.map[storage_position.x][storage_position.y].item = item_data;
                selecter.selected.SetSelected(false);
                picked.SetPicked(false, selecter.entity.controller.GetDungeonController().GetDugneon().gameObject.transform);
                item_data.component.Redraw(storage_position.x, storage_position.y, ref selecter.entity.controller.GetDungeonController().GetDugneon());
                item_data.component.in_map_coordinate_x = storage_position.x;
                item_data.component.in_map_coordinate_y = storage_position.y;
                picked = null;
                item_data = null;
                selecter.finding = FindingObjects.OBJECT;
                selecter.selected = null;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (picked == null) Pick();
            else Drop();
        }
        if (picked != null) {
            picked.transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, 0);
            picked.GetComponent<ObjectDefaultSpriteController>().SetOrder(selecter.entity.sprite_controller.sprite_renderer.sortingOrder);
        }
    }
}
