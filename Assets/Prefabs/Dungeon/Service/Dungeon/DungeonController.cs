using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class DungeonController : MonoBehaviour
{
    [SerializeField] GameObject dungeon_exapmle;
    [SerializeField] DungeonObjectsDictionary dictionary;
    [SerializeField] EntityController entity_controller;
    [SerializeField] CameraController camera_controller;
    Vector2[] moves = new Vector2[4];
    Dungeon dungeon;

    public ref Dungeon GetDugneon()
    {
        return ref dungeon;
    }
    
    public ref CameraController GetCamera()
    {
        return ref camera_controller;
    }

    public void Start() {
        moves[0] = new Vector2(10, 0);
        moves[1] = new Vector2(0, 10);
        moves[2] = new Vector2(-10, 0);
        moves[3] = new Vector2(0, -10);

        bool generated = false;
        while (!generated) generated = GenerateDunge();
        dungeon.GenerateDungeonObjects();
        dungeon.SetDungeonStatusClosed();
        dungeon.OpenRoom(0);
        camera_controller.AddTarget(dungeon.new_center, 0);
        entity_controller.SummonEntity(EntitiesKeys.PLAYER, dungeon.size_h / 2, dungeon.size_w / 2);
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
        Vector2[] moves = new Vector2[4];
        //if (dungeon != null) dungeon.Move();
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
                    camera_controller.AddTarget(dungeon.new_center, 0);
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
            camera_controller.AddTarget(dungeon.new_center, 0);
            entity_controller.SummonEntity(EntitiesKeys.PLAYER, dungeon.size_h / 2, dungeon.size_w / 2);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            dungeon.FocusNextRoom(); 
        }

        /*
        if (dungeon.NeedMove())
        {
            int ratio = camera_controller.GetComponent<PixelPerfectCamera>().pixelRatio;
            dungeon.MoveCenter(ratio);
            camera_controller.transform.position = new Vector3(dungeon.center.x, dungeon.center.y, -10);
        }
        */

    }

    /*
    public void FixedUpdate()
    {
        int ratio = camera.GetComponent<PixelPerfectCamera>().pixelRatio;
        if (Input.GetKey(KeyCode.D)) dungeon.MoveNewCenter(0.25f * ratio, 0f);
        if (Input.GetKey(KeyCode.S)) dungeon.MoveNewCenter(0f, -0.25f * ratio);
        if (Input.GetKey(KeyCode.A)) dungeon.MoveNewCenter(-0.25f * ratio, 0f);
        if (Input.GetKey(KeyCode.W)) dungeon.MoveNewCenter(0f, 0.25f * ratio);
    }
    */
}
