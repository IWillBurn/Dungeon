using System.Collections.Generic;
using UnityEngine;

public class DungeonController : MonoBehaviour
{
    [SerializeField] GameObject dungeon_exapmle;
    [SerializeField] DungeonObjectsDictionary dictionary;
    Dungeon dungeon;

    public void Start()
    {
        bool generated = false;
        while (!generated) generated = GenerateDunge();
        dungeon.GenerateDungeonObjects();
        dungeon.SetDungeonStatusClosed();
        dungeon.OpenRoom(0);
        dungeon.OpenRoom(1);
        dungeon.OpenRoom(2);
    }

    public bool GenerateDunge()
    {
        if (dungeon != null) Destroy(dungeon.gameObject);
        GameObject dungeon_object = Instantiate(dungeon_exapmle, transform);
        dungeon = dungeon_object.GetComponent<Dungeon>();
        dungeon.dictionary = dictionary;

        RoomGenerateTypes[] rooms = new RoomGenerateTypes[] {
            RoomGenerateTypes.MAIN,
            RoomGenerateTypes.MANAGE,
            RoomGenerateTypes.IN,
            RoomGenerateTypes.CORRIDOR,
            RoomGenerateTypes.CORRIDOR,
            RoomGenerateTypes.CORRIDOR,
            RoomGenerateTypes.CORRIDOR,
            RoomGenerateTypes.ROOM,
            RoomGenerateTypes.ROOM,
            RoomGenerateTypes.ROOM,
            RoomGenerateTypes.BOSS,
        };
        DungeonGenerationHelper helper = new DungeonGenerationHelper(rooms, 10);
        return dungeon.Generate(ref helper);
    }

    public void Update()
    {
        if (dungeon != null) dungeon.Move();
        if (Input.GetKeyDown(KeyCode.Q))
        {
            dungeon.Rotate();
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            for (int i = 0; i < dungeon.rooms.Count; i++)
            {
                if (dungeon.rooms[i].status == RoomStatuses.SHADOWED)
                {
                    dungeon.OpenRoom(i);
                    break;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            bool generated = false;
            while (!generated) generated = GenerateDunge();
            dungeon.GenerateDungeonObjects();
            dungeon.SetDungeonStatusClosed();
            dungeon.OpenRoom(0);
            dungeon.OpenRoom(1);
            dungeon.OpenRoom(2);
        }
    }
}
