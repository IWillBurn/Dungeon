using System.Collections.Generic;
using UnityEngine;

public enum ObjectsKeys
{
    NONE = -1,
    CELL_STONE = 0,
    ITEM_POT = 1, 
}

public enum ObjectsTypes
{
    CELL,
    FLOOR,
    ITEM,
    WALL,
}

public class RoomGenerateObjectsData
{
    public RoomObjectsPool pool;
    public List<int> counts;

    public RoomGenerateObjectsData(RoomObjectsPool _pool, List<int> _counts)
    {
        pool = _pool;
        counts = _counts;
    }
}

public record ObjectGenerateData
{
    public ObjectsKeys key;
    public PositionTypes position;

    public ObjectGenerateData()
    {
        key = ObjectsKeys.NONE;
        position = PositionTypes.NONE;
    }

    public ObjectGenerateData(ObjectsKeys _key, PositionTypes _position)
    {
        key = _key;
        position = _position;
    }
}

public class RoomObjectsPool
{
    List<List<ObjectsKeys>> objects;
    List<int> tiers_muliplier;
    int count_of_tiers;

    public RoomObjectsPool()
    {
        objects = new List<List<ObjectsKeys>>();
        tiers_muliplier = new List<int>();
        count_of_tiers = 0;
    }

    public RoomObjectsPool(List<List<ObjectsKeys>> _objects, List<int> _tiers_muliplier, int _count_of_tiers)
    {
        objects = _objects;
        tiers_muliplier = _tiers_muliplier;
        count_of_tiers = _count_of_tiers;
    }

    public void AddTier(ref List<ObjectsKeys> tier_objects, int multiplier)
    {
        objects.Add(tier_objects);
        tiers_muliplier.Add(multiplier);
        count_of_tiers++;
    }

    public ObjectsKeys GetItem(int start_tier, int end_tier, bool use_multiplier)
    {
        int count = 0;
        for (int i = start_tier; i < end_tier; i++)
        {
            if (use_multiplier) count += objects[i].Count * tiers_muliplier[i];
            else count += objects[i].Count;
        }
        int value = Random.Range(0, count);
        int position = 0;
        int added, diviner;
        for (int i = start_tier; i < end_tier; i++)
        {
            if (use_multiplier) {
                added = (position + objects[i].Count * tiers_muliplier[i]);
                diviner = tiers_muliplier[i];
            }
            else {
                added = position + objects[i].Count;
                diviner = 1;
            }

            if (added > value && value >= position) return objects[i][(value - position) / diviner];

            if (use_multiplier) position = (position + objects[i].Count * tiers_muliplier[i]);
            else position = position + objects[i].Count;
        }
        return ObjectsKeys.NONE;
    }
}

public class DungeonObjectsDictionary : MonoBehaviour
{
    [SerializeField] GameObject[] dictionary;
    public Dictionary<ObjectsKeys, ObjectGenerateData> generate_data = new Dictionary<ObjectsKeys, ObjectGenerateData>
        {
            { ObjectsKeys.CELL_STONE, new ObjectGenerateData(ObjectsKeys.CELL_STONE, PositionTypes.CELL) },
            { ObjectsKeys.ITEM_POT, new ObjectGenerateData(ObjectsKeys.ITEM_POT, PositionTypes.ITEM) },
        };

    public GameObject GetByKey(ObjectsKeys key)
    {
        return dictionary[(int) key];
    }
}
