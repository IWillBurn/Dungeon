using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public enum RoomTypes
{
    NONE,
    MAIN,
    MANAGE,
    END,
    CORRIDOR,
    GOLD,
    BOSS,
    IN,
}

public enum RoomGenerateTypes
{
    NONE,
    MAIN,
    DEFAULT,
    MANAGE,
    END,
    CORRIDOR,
    GOLD,
    BOSS,
    ROOM,
    IN,
}

public enum RoomStatuses
{
    CLOSED,
    SHADOWED,
    OPENED,
}

public record RoomGenerationParameters
{
    public int size_w_min, size_h_min, size_w_max, size_h_max, corridor_size_min, corridor_size_max, id, min_remoutness, max_remoutness;
    public bool end;
    public RoomTypes room_type;


    public RoomGenerationParameters(int _size_w_min, int _size_h_min, int _size_w_max, int _size_h_max,
                                    int _corridor_size_min, int _corridor_size_max, int _id, int _min_remoutness, int _max_remoutness,
                                    bool _end, RoomTypes _room_type)
    {
        size_w_min = _size_w_min;
        size_h_min = _size_h_min;
        size_w_max = _size_w_max;
        size_h_max = _size_h_max;
        corridor_size_min = _corridor_size_min;
        corridor_size_max = _corridor_size_max;
        id = _id;
        min_remoutness = _min_remoutness;
        max_remoutness = _max_remoutness;
        end = _end;
        room_type = _room_type;
    }
}

public record Coordinate
{
    public int x, y;

    public Coordinate(int _x, int _y)
    {
        x = _x; y = _y;
    }

    public Coordinate(ref Coordinate a, ref Coordinate b)
    {
        x = a.x + b.x;
        y = a.y + b.y;
    }
}

public record GenerateRange
{
    public int start_x, start_y, end_x, end_y, room_id;
    public RotateState rotate;
    
    public GenerateRange(int _start_x, int _start_y, int _end_x, int _end_y, int _room_id, RotateState _rotate)
    {
        start_x = _start_x;
        start_y = _start_y;
        end_x = _end_x;
        end_y = _end_y;
        rotate = _rotate;
        room_id = _room_id;
    }
}

public class RoomGenerationParametersDictionary
{
    public static Dictionary<RoomGenerateTypes, RoomGenerationParameters> dictionary = new Dictionary<RoomGenerateTypes, RoomGenerationParameters>
    {
        { RoomGenerateTypes.CORRIDOR, new RoomGenerationParameters(5, 5, 10, 10, 4, 4, -1, 0, 1000000, false, RoomTypes.CORRIDOR) },
        { RoomGenerateTypes.MAIN, new RoomGenerationParameters(6, 6, 6, 6, 2, 5, -1, 1, 1000000, false, RoomTypes.MAIN) },
        { RoomGenerateTypes.ROOM, new RoomGenerationParameters(6, 6, 6, 6, 2, 5, -1, 1, 1000000, true, RoomTypes.NONE) },
        { RoomGenerateTypes.BOSS, new RoomGenerationParameters(12, 12, 12, 12, 4, 4, -1, 2, 1000000, true, RoomTypes.BOSS) },
        { RoomGenerateTypes.MANAGE, new RoomGenerationParameters(6, 6, 6, 6, 1, 4, -1, 0, 0, true, RoomTypes.MANAGE) },
        { RoomGenerateTypes.IN, new RoomGenerationParameters(6, 6, 6, 6, 1, 4, -1, 0, 0, true, RoomTypes.IN) },
    };
}

public class RotateState {
    public int state;

    public RotateState()
    {
        state = 0;
    }

    public RotateState(int _state)
    {
        state = _state;
    }

    public static Coordinate Delta(int state)
    {
        if (state == 0) return new Coordinate(1, 0);
        if (state == 1) return new Coordinate(0, -1);
        if (state == 2) return new Coordinate(-1, 0);
        return new Coordinate(0, 1);
    }

    public static Coordinate RoomDelta(int state)
    {
        if (state == 0) return new Coordinate(-1, -1);
        if (state == 1) return new Coordinate(-1, 1);
        if (state == 2) return new Coordinate(-1, -1);
        return new Coordinate(-1, -1);
    }

    public void Set(int _state)
    {
        state = _state;
    }

    public void Next() {
        state = (state + 1) % 4;
    }

    public Coordinate Orto()
    {
        if (state == 0 || state == 2) return new Coordinate(1, 3);
        return new Coordinate(0, 2);
    }

    public int Another()
    {
        return (state + 2) % 4;
    }

    public Coordinate Delta()
    {
        if (state == 0) return new Coordinate(1, 0);
        if (state == 1) return new Coordinate(0, 1);
        if (state == 2) return new Coordinate(-1, 0);
        return new Coordinate(0, -1);
    }

    public int X(int x, int y, int w, int h)
    {
        if (state == 0) return x;
        if (state == 1) return h-y-1;
        if (state == 2) return w-x-1;
        return y;
    }

    public int Y(int x, int y, int w, int h)
    {
        if (state == 0) return y;
        if (state == 1) return x;
        if (state == 2) return h-y-1;
        return w-x-1;
    }

    public int W(int w, int h)
    {
        if (state == 0) return w;
        if (state == 1) return h;
        if (state == 2) return w;
        return h;
    }

    public int H(int w, int h)
    {
        if (state == 0) return h;
        if (state == 1) return w;
        if (state == 2) return h;
        return w;
    }
}

public class CorridorData
{
    public Coordinate corridor_start, corridor_end;
    public int corridor_to;
    public CorridorData(int _corridor_to, Coordinate _corridor_start, Coordinate _corridor_end)
    {
        corridor_start = _corridor_start;
        corridor_end = _corridor_end;
        corridor_to = _corridor_to;
    }
}

public class RoomGenerationPoolsData
{
    public static Dictionary<RoomGenerateTypes, RoomGenerateObjectsData> data = new Dictionary<RoomGenerateTypes, RoomGenerateObjectsData> {

        { RoomGenerateTypes.MAIN, new RoomGenerateObjectsData( new RoomObjectsPool
        (
            new List<List<ObjectsKeys>>
            {
                new List<ObjectsKeys> { ObjectsKeys.ITEM_POT },
            },
            new List<int> { 1 },
            1
        ),
        new List<int> { 3 })
        },
        { RoomGenerateTypes.MANAGE, new RoomGenerateObjectsData( new RoomObjectsPool
        (
            new List<List<ObjectsKeys>>
            {
                new List<ObjectsKeys> { ObjectsKeys.ITEM_POT },
            },
            new List<int> { 1 },
            1
        ),
        new List<int> { 3 })
        },
        { RoomGenerateTypes.IN, new RoomGenerateObjectsData( new RoomObjectsPool
        (
            new List<List<ObjectsKeys>>
            {
                new List<ObjectsKeys> { ObjectsKeys.ITEM_POT },
            },
            new List<int> { 1 },
            1
        ),
        new List<int> { 3 })
        },
        { RoomGenerateTypes.CORRIDOR, new RoomGenerateObjectsData( new RoomObjectsPool
        (
            new List<List<ObjectsKeys>>
            {
                new List<ObjectsKeys> { ObjectsKeys.ITEM_POT },
            },
            new List<int> { 1 },
            1
        ),
        new List<int> { 4 })
        },
        { RoomGenerateTypes.ROOM, new RoomGenerateObjectsData( new RoomObjectsPool
        (
            new List<List<ObjectsKeys>>
            {
                new List<ObjectsKeys> { ObjectsKeys.ITEM_POT },
            },
            new List<int> { 1 },
            1
        ),
        new List<int> { 3 })
        },
        { RoomGenerateTypes.BOSS, new RoomGenerateObjectsData( new RoomObjectsPool
        (
            new List<List<ObjectsKeys>>
            {
                new List<ObjectsKeys> { ObjectsKeys.ITEM_POT },
            },
            new List<int> { 1 },
            1
        ),
        new List<int> { 10 })
        },
    };
}

public class DungeonObjectContainer<T>
{
    public T component;
    public ObjectsKeys object_key;
    public DungeonObjectContainer()
    {
        object_key = ObjectsKeys.NONE;
    }
    public DungeonObjectContainer(ObjectsKeys _key)
    {
        object_key = _key;
    }
    public DungeonObjectContainer(ObjectsKeys _key, T _component)
    {
        object_key = _key;
        component = _component;
    }
}

public class DungeonCell
{
    public DungeonObjectContainer<DungeonObjectCell> cell;
    public DungeonObjectContainer<DungeonObjectItem> item;
    public DungeonObjectContainer<DungeonObjectFloor> floor;
    public DungeonObjectContainer<DungeonObjectWall> wall_ld;
    public DungeonObjectContainer<DungeonObjectWall> wall_lu;
    public DungeonObjectContainer<DungeonObjectWall> wall_ru;
    public DungeonObjectContainer<DungeonObjectWall> wall_rd;
    public int id;
    public bool is_corridor;

    public DungeonCell(int _id)
    {
        id = _id;
        cell = new();
        item = new();
        floor = new();
        wall_ld = new();
        wall_lu = new();
        wall_ru = new();
        wall_rd = new();
        is_corridor = false;
    }

    public void SetCorridor(bool _is_corridor) {
        is_corridor = _is_corridor;
    }

    public bool AddObject(ref DungeonObjectContainer<DungeonObjectCell> new_object) {
        if (cell.object_key == ObjectsKeys.NONE)
        {
            cell.object_key = new_object.object_key;
            return true;
        }
        return false;
    }

    public bool AddObject(ref DungeonObjectContainer<DungeonObjectItem> new_object) {
        if (item.object_key == ObjectsKeys.NONE)
        {
            item.object_key = new_object.object_key;
            return true;
        }
        return false;
    }

    public bool AddObject(ref DungeonObjectContainer<DungeonObjectFloor> new_object)
    {
        if (floor.object_key == ObjectsKeys.NONE)
        {
            floor.object_key = new_object.object_key;
            return true;
        }
        return false;
    }

    public bool AddObject(ref DungeonObjectContainer<DungeonObjectWall> new_object, int position)
    {
        if (position == -1)
        {
            position = UnityEngine.Random.Range(0, 4);
            for (int i = 0; i < 4; i++)
            {
                int current_position = (position + i) % 4;
                if (current_position == 0 && wall_ld.object_key == ObjectsKeys.NONE) { wall_ld.object_key = new_object.object_key; return true; }
                if (current_position == 1 && wall_lu.object_key == ObjectsKeys.NONE) { wall_lu.object_key = new_object.object_key; return true; }
                if (current_position == 2 && wall_ru.object_key == ObjectsKeys.NONE) { wall_ru.object_key = new_object.object_key; return true; }
                if (current_position == 3 && wall_rd.object_key == ObjectsKeys.NONE) { wall_rd.object_key = new_object.object_key; return true; }
            }
        }
        else
        {
            if (position == 0 && wall_ld.object_key == ObjectsKeys.NONE) { wall_ld.object_key = new_object.object_key; return true; }
            if (position == 1 && wall_lu.object_key == ObjectsKeys.NONE) { wall_lu.object_key = new_object.object_key; return true; }
            if (position == 2 && wall_ru.object_key == ObjectsKeys.NONE) { wall_ru.object_key = new_object.object_key; return true; }
            if (position == 3 && wall_rd.object_key == ObjectsKeys.NONE) { wall_rd.object_key = new_object.object_key; return true; }
        }
        return false;
    }

    public bool AddObject(ref ObjectGenerateData new_object)
    {
        bool result = false;
        if (new_object.position == PositionTypes.CELL)
        {
            DungeonObjectContainer<DungeonObjectCell> new_object_container = new(new_object.key);
            result = AddObject(ref new_object_container);
        }
        if (new_object.position == PositionTypes.ITEM)
        {
            DungeonObjectContainer<DungeonObjectItem> new_object_container = new(new_object.key);
            result = AddObject(ref new_object_container);
        }
        if (new_object.position == PositionTypes.FLOOR)
        {
            DungeonObjectContainer<DungeonObjectFloor> new_object_container = new(new_object.key);
            result = AddObject(ref new_object_container);
        }
        if (new_object.position == PositionTypes.WALL)
        {
            DungeonObjectContainer<DungeonObjectWall> new_object_container = new(new_object.key);
            result = AddObject(ref new_object_container, -1);
        }
        return result;
    }

    public void SetStatusOpened()
    {
        if (cell.component != null) cell.component.SetStatusOpened();
        if (item.component != null) item.component.SetStatusOpened();
        /*
        if (floor.component != null) floor.component.SetStatusOpened();
        if (wall_ld.component != null) wall_ld.component.SetStatusOpened();
        if (wall_lu.component != null) wall_lu.component.SetStatusOpened();
        if (wall_ru.component != null) wall_ru.component.SetStatusOpened();
        if (wall_rd.component != null) wall_rd.component.SetStatusOpened();
        */
    }

    public void SetStatusShadowed()
    {
        if (cell.component != null) cell.component.SetStatusShadowed();
        if (item.component != null) item.component.SetStatusShadowed();
        /*
        if (floor.component != null) floor.component.SetStatusShadowed();
        if (wall_ld.component != null) wall_ld.component.SetStatusShadowed();
        if (wall_lu.component != null) wall_lu.component.SetStatusShadowed();
        if (wall_ru.component != null) wall_ru.component.SetStatusShadowed();
        if (wall_rd.component != null) wall_rd.component.SetStatusShadowed();
        */
    }

    public void SetStatusClosed()
    {
        if (cell.component != null) cell.component.SetStatusClosed();
        if (item.component != null) item.component.SetStatusClosed();
        /*
        if (floor.component != null) floor.component.SetStatusClosed();
        if (wall_ld.component != null) wall_ld.component.SetStatusClosed();
        if (wall_lu.component != null) wall_lu.component.SetStatusClosed();
        if (wall_ru.component != null) wall_ru.component.SetStatusClosed();
        if (wall_rd.component != null) wall_rd.component.SetStatusClosed();
        */
    }
}

public class RoomInfo
{
    public RoomStatuses status;
    public int id, parent, remoteness;
    public Coordinate room_start, room_end;
    public Coordinate corridore_start, corridore_end;
    public RoomGenerateTypes room_generate_type;
    public RoomTypes room_type;
    public RoomInfo(int _id, int _parent, int _remoteness, Coordinate _room_start, Coordinate _room_end,
                    Coordinate _corridore_start, Coordinate _corridore_end,
                    RoomGenerateTypes _room_generate_type, RoomTypes _room_type)
    {
        status = RoomStatuses.CLOSED;
        id = _id;
        parent = _parent;
        remoteness = _remoteness;
        room_start = _room_start;
        room_end = _room_end;
        room_generate_type = _room_generate_type;
        room_type = _room_type;
        corridore_start = _corridore_start;
        corridore_end = _corridore_end;
    }

    public void SetShadowed()
    {

    }

    public void SetOpened()
    {

    }
}

public class AIData
{
    public bool is_empty;
    public bool is_dangerous;

    public AIData()
    {
        is_empty = false;
        is_dangerous = false;
    }
}

public class DungeonMap
{
    public List<List<DungeonCell>> map;
    public DungeonMap(int size_w, int size_h, int _id)
    {
        map = new List<List<DungeonCell>>();
        for (int i = 0; i < size_w; i++)
        {
            List<DungeonCell> map_row = new List<DungeonCell>();
            for (int j = 0; j < size_h; j++)
            {
                DungeonCell cell = new (_id);
                map_row.Add(cell);
            }
            map.Add(map_row);
        }
    }
}

public class DungeonMapAI
{
    public List<List<AIData>> map;
    public DungeonMapAI(int size_w, int size_h)
    {
        map = new List<List<AIData>>();
        for (int i = 0; i < size_w; i++)
        {
            List<AIData> map_row = new List<AIData>();
            for (int j = 0; j < size_h; j++)
            {
                AIData cell = new AIData();
                map_row.Add(cell);
            }
            map.Add(map_row);
        }
    }
}

public record DungeonParameters
{
    public int id;
    public RotateState rotate;
    public DungeonParameters(int _id)
    {
        id = _id;
        rotate = new RotateState();
    }
}

public class DungeonGenerationHelper
{
    public Queue<RoomGenerateTypes> rooms;
    public List<List<GenerateRange>> ranges;
    public int max_remoutness;

    public DungeonGenerationHelper(RoomGenerateTypes[] _rooms, int _remountness)
    {
        max_remoutness = 0;
        ranges = new List<List<GenerateRange>>();
        for (int i = 0; i < _remountness; i++)
        {
            List<GenerateRange> empty = new List<GenerateRange>();
            ranges.Add(empty);
        }
        rooms = new Queue<RoomGenerateTypes> ();
        for (int i = 0; i < _rooms.Length; i++) rooms.Enqueue(_rooms[i]);
    }

    public int CountOfRanges()
    {
        int result = 0;
        for (int i = 0; i < ranges.Count; i++)
        {
            result += ranges[i].Count;
        }
        return result;
    }

    public int CountOfRanges(int start, int end)
    {
        int result = 0;
        for (int i = start; i < end; i++)
        {
            result += ranges[i].Count;
        }
        return result;
    }

    public void FindMaxRemoutness()
    {
        for (int i = 0; i < ranges.Count; i++)
        {
            if (ranges[i].Count > 0) max_remoutness = i;
        }
    }
} 

public class Dungeon : MonoBehaviour
{
    [SerializeField] GameObject collider_examble;

    PolygonCollider2D dungeon_collider;

    int count_of_rooms = 0;
    int focused_room = 0;

    public Vector2 center;
    public Vector2 new_center;
    Coordinate[] frame;

    public DungeonObjectsDictionary dictionary;
    public Dictionary<RoomGenerateTypes, RoomObjectsPool> pools;
    public int size_w;
    public int size_h; 
    public DungeonMap dungeon_map;
    public List<List<CorridorData>> graph;
    public List<RoomInfo> rooms;
    public DungeonMapAI ai_map;
    public DungeonParameters parameters;

    public Vector2 FloatCoordinates(int position_x, int position_y) {
        float start_x = size_w / 2f;
        float start_y = size_h / 2f;
        return new Vector2(0.5f * (position_x - start_x) - 0.5f * (position_y - start_y) + transform.position.x, -0.25f * (position_x - start_x) - 0.25f * (position_y - start_y) + transform.position.y - 0.5f);
    }

    bool InRange(ref List<List<DungeonCell>> list, int x, int y) {
        int size_x = list.Count;
        int size_y = list[0].Count;
        return (x > 0) && (x < size_x) && (y > 0) && (y < size_y);
    }

    public Dungeon GetDungeon(int _size_w, int _size_h)
    {
        size_w = _size_w;
        size_h = _size_h;
        dungeon_map = new DungeonMap(size_w, size_h, -1);
        ai_map = new DungeonMapAI(size_w, size_h);
        parameters = new DungeonParameters(-1);
        rooms = new List<RoomInfo>();
        graph = new List<List<CorridorData>>();

        frame = new Coordinate[2];
        frame[0] = new Coordinate(size_w / 2, size_h / 2);
        frame[1] = new Coordinate(size_w / 2, size_h / 2);

        center = new Vector2(0,0);
        new_center = new Vector2(0,0);
        FindCenter(new Coordinate(frame[0].x, frame[0].y), new Coordinate(frame[1].x, frame[1].y));
        center = new_center;

        Dungeon dungeon = this;

        GameObject dungeon_collider_object = Instantiate(collider_examble, gameObject.transform);
        dungeon_collider = dungeon_collider_object.GetComponent<PolygonCollider2D>();

        Vector2[] main_collider = new Vector2[4];
        main_collider[0] = new Vector2(10000, -10000);
        main_collider[1] = new Vector2(10000, 10000);
        main_collider[2] = new Vector2(-10000, 10000);
        main_collider[3] = new Vector2(-10000, -10000);
        dungeon_collider.SetPath(0, main_collider);

        return dungeon;
    }

    public bool Generate(ref DungeonGenerationHelper helper)
    {
        GetDungeon(500, 500);
        for (int i = 0; i < helper.rooms.Count; i++)
        {
            List<CorridorData> empty = new List<CorridorData>();
            graph.Add(empty);
        }


        GenerateFirstRoom(ref helper);
       
        bool generated;
        while (helper.rooms.Count != 0)
        {
            generated = false;
            int room_number = count_of_rooms;
            count_of_rooms++;

            RoomGenerateTypes room = helper.rooms.Dequeue();

            RoomGenerationParameters parameters = RoomGenerationParametersDictionary.dictionary[room];
            parameters.id = room_number;
            while (generated == false)
            {
                generated = TryToGenerateRoom(ref helper, ref parameters, ref room);
                if (helper.CountOfRanges(Mathf.Max(0, parameters.min_remoutness), Mathf.Min(helper.max_remoutness, parameters.max_remoutness) + 1) == 0)
                {
                    return false;
                }
            }
        }
        InitDungeonTemplate();
        return true;
    }

    public void GenerateFirstRoom(ref DungeonGenerationHelper helper)
    {
        RoomGenerateTypes room = helper.rooms.Dequeue();
        RoomGenerationParameters parameters = RoomGenerationParametersDictionary.dictionary[room];

        count_of_rooms++;
        int size_w = UnityEngine.Random.Range(parameters.size_w_min, parameters.size_w_max);
        int size_h = UnityEngine.Random.Range(parameters.size_h_min, parameters.size_w_max);
        Coordinate room_start = new(250 - size_w / 2, 250 - size_h / 2);
        Coordinate room_end = new(250 + size_w / 2, 250 + size_h / 2);
        Coordinate corridore_start = new(-1, -1);
        Coordinate corridore_end = new(-1, -1);
        GenerationAddRectangle(ref room_start, ref room_end, 0, false);
        rooms.Add(new RoomInfo(0, -1, 0, room_start, room_end, corridore_start, corridore_end, room, parameters.room_type));
        AddBordersOfRoom(ref helper, ref room_start, ref room_end, new int[0], 0, -1);
        GenerateObjectsInRoom(rooms.Count - 1);
    }

    void AddBordersOfRoom(ref DungeonGenerationHelper helper, ref Coordinate room_start, ref Coordinate room_end, int[] exclusions, int id, int parent_id)
    {
        int min_x = Mathf.Min(room_start.x, room_end.x);
        int min_y = Mathf.Min(room_start.y, room_end.y);
        int max_x = Mathf.Max(room_start.x, room_end.x);
        int max_y = Mathf.Max(room_start.y, room_end.y);
        List<GenerateRange> ranges = new List<GenerateRange> {
            new (min_x, min_y + 1, min_x + 1, max_y - 1, id, new RotateState(2)),
            new (min_x + 1, min_y, max_x - 1, min_y + 1, id, new RotateState(3)),
            new (max_x, min_y + 1, max_x + 1, max_y - 1, id, new RotateState(0)),
            new (min_x + 1, max_y, max_x - 1, max_y + 1, id, new RotateState(1))
        };
        for (int i = 0; i < 4; i++)
        {
            bool in_exclusions = false;
            for (int j = 0; j < exclusions.Length; j++)
            {
                if (i == exclusions[j]) { in_exclusions = true; }
            }
            if (!in_exclusions)
            {
                int remoutness = 0;
                if (parent_id != -1) remoutness = rooms[parent_id].remoteness + 1;
                helper.ranges[remoutness].Add(ranges[i]);
                helper.max_remoutness = Mathf.Max(remoutness, helper.max_remoutness);
            }
        }
    }

    bool TryToGenerateRoom(ref DungeonGenerationHelper helper, ref RoomGenerationParameters parameters, ref RoomGenerateTypes room_generate_type)
    {
        if (helper.CountOfRanges(Mathf.Max(0, parameters.min_remoutness), Mathf.Min(helper.max_remoutness, parameters.max_remoutness) + 1) == 0) return false;
        int remountness = -1;
        while (remountness == -1 || helper.ranges[remountness].Count == 0)
        {
            remountness = UnityEngine.Random.Range(Mathf.Max(0, parameters.min_remoutness), Mathf.Min(helper.max_remoutness, parameters.max_remoutness) + 1);
        }
        int position = UnityEngine.Random.Range(0, helper.ranges[remountness].Count);
        GenerateRange range = helper.ranges[remountness][position];
        helper.ranges[remountness].RemoveAt(position);
        helper.FindMaxRemoutness();

        int cooridor_size = UnityEngine.Random.Range(parameters.corridor_size_min, parameters.corridor_size_max);
        int room_w = UnityEngine.Random.Range(parameters.size_w_min, parameters.size_w_max);
        int room_h = UnityEngine.Random.Range(parameters.size_h_min, parameters.size_h_max);

        int corridor_start_x = UnityEngine.Random.Range(range.start_x, range.end_x - 1);
        int corridor_start_y = UnityEngine.Random.Range(range.start_y, range.end_y - 1);
        int corridor_end_x = corridor_start_x + range.rotate.Delta().x * cooridor_size + ((range.rotate.Delta().x + 1) % 2) * 2;
        int corridor_end_y = corridor_start_y + range.rotate.Delta().y * cooridor_size + ((range.rotate.Delta().y + 1) % 2) * 2;

        Coordinate corridor_start = new (corridor_start_x, corridor_start_y);
        Coordinate corridor_end = new (corridor_end_x, corridor_end_y);
        if (TryToGenerateRectangle(ref corridor_start, ref corridor_end, 0) == false) return false;

        Coordinate orto_delta = RotateState.Delta(range.rotate.Orto().x);
        int variants_x = room_w * Mathf.Abs(orto_delta.x) - 2;
        int variants_y = room_h * Mathf.Abs(orto_delta.y) - 2;
        int variant_x = 0;
        int variant_y = 0;

        if (variants_x > 0) variant_x = UnityEngine.Random.Range(0, variants_y); else variants_x = 1;
        if (variants_y > 0) variant_y = UnityEngine.Random.Range(0, variants_y); else variants_y = 1;

        Coordinate random_start = new (variant_x, variant_y);

        bool can_generate = false;

        Coordinate room_start = new (0, 0);
        Coordinate room_end = new (0, 0);

        for (int i = 0; i < variants_x; i++)
        {
            for (int j = 0; j < variants_y; j++)
            {           
                int delta_x = (i + random_start.x) % variants_x;
                int delta_y = (j + random_start.y) % variants_y;
                if (delta_x < 2 && delta_y < 2) continue;
                
                int room_start_x = corridor_end_x + delta_x + room_w * RotateState.RoomDelta(range.rotate.state).x;
                int room_start_y = corridor_end_y + delta_y + room_h * RotateState.RoomDelta(range.rotate.state).y;
                int room_end_x = corridor_end_x + delta_x;
                int room_end_y = corridor_end_y + delta_y;

                room_start = new(Mathf.Min(room_start_x, room_end_x), Mathf.Min(room_start_y, room_end_y));
                room_end = new(Mathf.Max(room_start_x, room_end_x), Mathf.Max(room_start_y, room_end_y));

                if (TryToGenerateRectangle(ref room_start, ref room_end, 2) != false)
                {
                    can_generate = true;
                    break;
                }
            }
            if (can_generate) break;
        }
        if (!can_generate) return false;

        Coordinate corridor_current_start = new Coordinate(Mathf.Min(corridor_start.x, corridor_end.x), Mathf.Min(corridor_start.y, corridor_end.y));
        Coordinate corridor_current_end = new Coordinate(Mathf.Max(corridor_start.x, corridor_end.x), Mathf.Max(corridor_start.y, corridor_end.y));

        CorridorData corridor_to = new CorridorData(parameters.id, corridor_current_start, corridor_current_end);
        CorridorData corridor_from = new CorridorData(range.room_id, corridor_current_start, corridor_current_end);
        graph[parameters.id].Add(corridor_from);
        graph[range.room_id].Add(corridor_to);

        GenerationAddRectangle(ref corridor_start, ref corridor_end, parameters.id, true);
        GenerationAddRectangle(ref room_start, ref room_end, parameters.id, false);
        int[] exlusion = { range.rotate.state };
        rooms.Add(new RoomInfo(parameters.id, range.room_id, rooms[range.room_id].remoteness + 1, room_start, room_end, corridor_current_start, corridor_current_end, room_generate_type, parameters.room_type));
        if (!parameters.end) AddBordersOfRoom(ref helper, ref room_start, ref room_end, exlusion, parameters.id, range.room_id);
        GenerateObjectsInRoom(rooms.Count - 1);
        return true;
    }

    bool TryToGenerateRectangle(ref Coordinate start, ref Coordinate end, int outline_size)
    {
        for (int i = Mathf.Min(start.x, end.x) - outline_size; i < Mathf.Max(start.x, end.x) + outline_size; i++)
        {
            for (int j = Mathf.Min(start.y, end.y) - outline_size; j < Mathf.Max(start.y, end.y) + outline_size; j++)
            {
                if (dungeon_map.map[i][j].id != -1) return false;
            }
        }
        return true;
    }

    void GenerationAddRectangle(ref Coordinate start, ref Coordinate end, int id, bool is_corridor)
    {
        for (int i = Mathf.Min(start.x, end.x); i < Mathf.Max(start.x, end.x); i++)
        {
            for (int j = Mathf.Min(start.y, end.y); j < Mathf.Max(start.y, end.y); j++)
            {
                dungeon_map.map[i][j].id = id;
                dungeon_map.map[i][j].is_corridor = is_corridor;
            }
        }
    }

    void InitDungeonTemplate()
    {
        for (int i = 0; i < size_w; i++)
        {
            for (int j = 0; j < size_h; j++)
            {
                if (dungeon_map.map[i][j].id != -1) dungeon_map.map[i][j].cell.object_key = ObjectsKeys.CELL_STONE;
            }
        }
    }

    public void GenerateObjectsInRoom(int room_id)
    {
        RoomInfo room = rooms[room_id];
        RoomGenerateObjectsData data = RoomGenerationPoolsData.data[room.room_generate_type];
        for (int i = 0; i < data.counts.Count; i++)
        {
            for (int j = 0; j < data.counts[i]; j++)
            {
                ObjectGenerateData new_object = dictionary.generate_data[data.pool.GetItem(i, i + 1, false)];
                bool generated = false;
                while (!generated)
                {
                    int x = UnityEngine.Random.Range(Mathf.Min(room.room_start.x, room.room_end.x), Mathf.Max(room.room_start.x, room.room_end.x));
                    int y = UnityEngine.Random.Range(Mathf.Min(room.room_start.y, room.room_end.y), Mathf.Max(room.room_start.y, room.room_end.y));
                    generated = dungeon_map.map[x][y].AddObject(ref new_object);
                }
            }
        }
    }

    public void OpenRoom(int room_id)
    {
        SetRoomStatusOpened(room_id);
        SetStatusShadowedNeighboringRooms(room_id);
        RecalculateFrame();
        AddRoomToCollider(room_id);
        FocusRoom(room_id);
    }

    public void SetDungeonStatusClosed()
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            SetRoomStatusClosed(i);
        }
    }

    public void SetStatusShadowedNeighboringRooms(int room_id)
    {
        for (int i = 0; i < graph[room_id].Count; i++)
        {
            int to_room_id = graph[room_id][i].corridor_to;
            if (rooms[to_room_id].status == RoomStatuses.CLOSED) SetRoomStatusShadowed(to_room_id);
        }
    }

    public void SetRoomStatusOpened(int room_id)
    {
        RoomInfo room = rooms[room_id];
        for (int i = room.room_start.x; i < room.room_end.x; i++)
        {
            for (int j = room.room_start.y; j < room.room_end.y; j++)
            {
                dungeon_map.map[i][j].SetStatusOpened();
            }
        }
        for (int z = 0; z < graph[room_id].Count; z++)
        {
            CorridorData corridor = graph[room_id][z];
            for (int i = corridor.corridor_start.x; i < corridor.corridor_end.x; i++)
            {
                for (int j = corridor.corridor_start.y; j < corridor.corridor_end.y; j++)
                {
                    dungeon_map.map[i][j].SetStatusOpened();
                }
            }
        }
        rooms[room_id].status = RoomStatuses.OPENED;
    }

    public void SetRoomStatusShadowed(int room_id)
    {
        RoomInfo room = rooms[room_id];
        for (int i = room.room_start.x; i < room.room_end.x; i++)
        {
            for (int j = room.room_start.y; j < room.room_end.y; j++)
            {
                dungeon_map.map[i][j].SetStatusShadowed();
            }
        }
        for (int z = 0; z < graph[room_id].Count; z++)
        {
            CorridorData corridor = graph[room_id][z];
            for (int i = corridor.corridor_start.x; i < corridor.corridor_end.x; i++)
            {
                for (int j = corridor.corridor_start.y; j < corridor.corridor_end.y; j++)
                {
                    dungeon_map.map[i][j].SetStatusShadowed();
                }
            }
        }
        rooms[room_id].status = RoomStatuses.SHADOWED;
    }

    public void SetRoomStatusClosed(int room_id)
    {
        RoomInfo room = rooms[room_id];
        for (int i = room.room_start.x; i < room.room_end.x; i++)
        {
            for (int j = room.room_start.y; j < room.room_end.y; j++)
            {
                dungeon_map.map[i][j].SetStatusClosed();
            }
        }
        for (int z = 0; z < graph[room_id].Count; z++)
        {
            CorridorData corridor = graph[room_id][z];
            for (int i = corridor.corridor_start.x; i < corridor.corridor_end.x; i++)
            {
                for (int j = corridor.corridor_start.y; j < corridor.corridor_end.y; j++)
                {
                    dungeon_map.map[i][j].SetStatusClosed();
                }
            }
        }
        rooms[room_id].status = RoomStatuses.CLOSED;
    }

    public void GenerateDungeonObjects()
    {
        Dungeon dungeon = this;
        for (int z = 0; z < rooms.Count; z++)
        {
            RoomInfo room = rooms[z];
            Coordinate corridore_start = room.corridore_start;
            Coordinate corridore_end = room.corridore_end;
            if (corridore_start.x != -1)
            {
                for (int i = corridore_start.x; i < corridore_end.x; i++)
                {
                    for (int j = corridore_start.y; j < corridore_end.y; j++)
                    {
                        int position_x = parameters.rotate.X(i, j, size_w, size_h);
                        int position_y = parameters.rotate.Y(i, j, size_w, size_h);
                        DungeonObject.GenerateDungeonObjects(position_x, position_y, ref dungeon);
                    }
                }
            }
            Coordinate room_start = room.room_start;
            Coordinate room_end = room.room_end;
            for (int i = room_start.x; i < room_end.x; i++)
            {
                for (int j = room_start.y; j < room_end.y; j++)
                {
                    int position_x = parameters.rotate.X(i, j, size_w, size_h);
                    int position_y = parameters.rotate.Y(i, j, size_w, size_h);
                    DungeonObject.GenerateDungeonObjects(position_x, position_y, ref dungeon);
                }
            }
        }


    }

    public void CalcualateCollider() {
        dungeon_collider.pathCount = 0;

        dungeon_collider.pathCount++;
        Vector2[] main_collider = new Vector2[4];
        main_collider[0] = new Vector2(-10000, -10000);
        main_collider[1] = new Vector2(10000, -10000);
        main_collider[2] = new Vector2(10000, 10000);
        main_collider[3] = new Vector2(-10000, 10000);
        dungeon_collider.SetPath(0, main_collider);

        for (int z = 0; z < rooms.Count; z++)
        {
            AddRoomToCollider(z);
        }
    }

    public void AddRoomToCollider(int room_id)
    {
        RoomInfo room = rooms[room_id];
        Coordinate corridore_start = room.corridore_start;
        Coordinate corridore_end = room.corridore_end;
        if (corridore_start.x != -1)
        {
            dungeon_collider.pathCount++;
            dungeon_collider.SetPath(dungeon_collider.pathCount - 1, CalculateRectangleCollider(corridore_start, corridore_end));
        }
        Coordinate room_start = room.room_start;
        Coordinate room_end = room.room_end;
        dungeon_collider.pathCount++;
        dungeon_collider.SetPath(dungeon_collider.pathCount - 1, CalculateRectangleCollider(room_start, room_end));
    }

    public Vector2[] CalculateRectangleCollider(Coordinate start, Coordinate end) {
        float start_x = size_w / 2f;
        float start_y = size_h / 2f;

        Vector2[] result = new Vector2[4];

        Vector2 rectangle_u = FloatCoordinates(start.x, start.y);
        rectangle_u.y += 0.5f;
        Vector2 rectangle_r = FloatCoordinates(end.x, start.y);
        rectangle_r.y += 0.5f;
        Vector2 rectangle_d = FloatCoordinates(end.x, end.y);
        rectangle_d.y += 0.5f;
        Vector2 rectangle_l = FloatCoordinates(start.x, end.y);
        rectangle_l.y += 0.5f;

        result[0] = rectangle_u;
        result[1] = rectangle_r;
        result[2] = rectangle_d;
        result[3] = rectangle_l;
        return result;
    }
    
    public void FocusRoom(int room_id)
    {
        FindCenter(rooms[room_id].room_start, rooms[room_id].room_end);
        focused_room = room_id;
    }

    public void FocusNextRoom()
    {
        focused_room = (focused_room + 1) % rooms.Count;
        while (rooms[focused_room].status != RoomStatuses.OPENED)
        {
            focused_room = (focused_room + 1) % rooms.Count;
        }
        FindCenter(rooms[focused_room].room_start, rooms[focused_room].room_end);
    }

    public void RecalculateFrame()
    {
        CalculateOpenFrame();
        FindCenter(new Coordinate(frame[0].x, frame[0].y), new Coordinate(frame[1].x, frame[1].y));
    }

    public void CalculateOpenFrame()
    {
        Coordinate start = new Coordinate(1000000, 1000000);
        Coordinate end = new Coordinate(-1, -1);

        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].status == RoomStatuses.OPENED)
            {
                start.x = Mathf.Min(start.x, rooms[i].room_start.x);
                start.y = Mathf.Min(start.y, rooms[i].room_start.y);
                end.x = Mathf.Max(end.x, rooms[i].room_end.x);
                end.y = Mathf.Max(end.y, rooms[i].room_end.y);
            }
        }
        frame = new Coordinate[2];
        frame[0] = start;
        frame[1] = end;
    }

    public void FindCenter(Coordinate start, Coordinate end)
    {
        float start_x = size_w / 2f;
        float start_y = size_h / 2f;
        int position_x = (start.x + end.x) / 2;
        int position_y = (start.y + end.y) / 2;
        Vector2 center = FloatCoordinates(position_x, position_y);
        new_center.x = center.x;
        new_center.y = center.y;
    }

    public void MoveNewCenter(float move_x, float move_y) {
        new_center.x += move_x;
        new_center.y += move_y;
    }

    public bool NeedMove()
    {
        return Mathf.Sqrt(Mathf.Pow(new_center.x - center.x, 2) + Mathf.Pow(new_center.y - center.y, 2)) > 0.01;
    }

    public void MoveCenter(int ration)
    {
        center.x += (new_center.x - center.x) / (32 / ration);
        center.y += (new_center.y - center.y) / (32 / ration);
        if (!NeedMove())
        {
            center.x = new_center.x;
            center.y = new_center.y;
        }
    }

    public void Rotate() {
        parameters.rotate.Next();
        Redraw();
    }

    public void Redraw()
    {
        Dungeon dungeon = this;
        for (int i = 0; i < size_w; i++)
        {
            for (int j = 0; j < size_h; j++)
            {
                int position_x = parameters.rotate.X(i, j, size_w, size_h);
                int position_y = parameters.rotate.Y(i, j, size_w, size_h);
                RedrawCell(i, j, position_x, position_y, ref dungeon);
            }
        }
    }

    public void RedrawCell(int real_position_x, int real_position_y, int virtual_position_x, int virtual_position_y, ref Dungeon dungeon)
    {
        DungeonObjectCell cell = dungeon_map.map[real_position_x][real_position_y].cell.component;
        if (cell != null) cell.Redraw(virtual_position_x, virtual_position_y, ref dungeon);
    }

    public void Move()
    {
        Vector2 screenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
        transform.position = new Vector3(worldPosition.x, worldPosition.y, 0);
    }

    public bool MergeDungeons(ref Dungeon mearge_dungeon, int position_x, int position_y)
    {
        if (!IsDungeonsMergeable(ref mearge_dungeon, position_x, position_y)) return false;

        return true;
    }

    public bool IsDungeonsMergeable(ref Dungeon mearge_dungeon, int position_x, int position_y)
    {
        return true;
    }
}