using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityData
{
    public EntitiesKeys key;
    public GameObject component;
    public EntityData(EntitiesKeys _key, ref GameObject _component)
    {
        key = _key;
        component = _component;
    }
}

public class EntityController : MonoBehaviour
{
    [SerializeField] DungeonController dungeon_controller;
    [SerializeField] EntitiesDictionary dictionary;

    Dictionary<int, EntityData> entities;
    int key_number;
    int player_key = -1;
    int player_room_number;

    public void Start()
    {
        entities = new Dictionary<int, EntityData>();
        key_number = 0;
        player_room_number = -1;
    }

    public void SummonEntity(EntitiesKeys key, int position_x, int position_y)
    {
        GameObject entity = Instantiate(dictionary.GetByKey(key), dungeon_controller.getDugneon().gameObject.transform);
        entity.transform.position = dungeon_controller.getDugneon().GetComponent<Dungeon>().FloatCoordinates(position_x, position_y);
        EntityController controller = this;
        entity.GetComponent<EntityBehaviour>().controller = controller;
        entities.Add(key_number, new EntityData(key, ref entity));
        if (key == EntitiesKeys.PLAYER) player_key = key_number;
        key_number++;
    }

    public void MoveToCell(ref DungeonObjectCell cell)
    {
        Dungeon dungeon = dungeon_controller.getDugneon();
        if (dungeon.dungeon_map.map[cell.in_map_coordinate_x][cell.in_map_coordinate_y].is_corridor == false)
        {
            int new_player_room_number = dungeon.dungeon_map.map[cell.in_map_coordinate_x][cell.in_map_coordinate_y].id;
            if (new_player_room_number != player_room_number)
            {
                player_room_number = new_player_room_number;
                dungeon.FocusRoom(player_room_number);
                dungeon_controller.GetCamera().ChangeDefaultTarget(dungeon.new_center);
            }
        }
    }
}
