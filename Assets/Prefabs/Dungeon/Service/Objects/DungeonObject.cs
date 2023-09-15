using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Positions
{
    CELL,
    ITEM,
    FLOOR,
    WALL_LD,
    WALL_LU,
    WALL_RD ,
    WALL_RU,
}

public enum PositionTypes
{
    NONE,
    CELL,
    ITEM,
    FLOOR,
    WALL,
}

public class DungeonObject : MonoBehaviour
{
    public Positions draw_positions;
    public Vector2 main_position;
    public PositionTypes position;
    public int in_map_coordinate_x;
    public int in_map_coordinate_y;
    public static Dictionary<Positions, Vector2> draw_delta =
        new Dictionary<Positions, Vector2>
        {
            { Positions.CELL, new Vector2(0,0)},
            { Positions.ITEM, new Vector2(0,0.438f)},
            { Positions.FLOOR, new Vector2(0,0)},
            { Positions.WALL_LD, new Vector2(0,0)},
            { Positions.WALL_LU, new Vector2(0,0)},
            { Positions.WALL_RD, new Vector2(0,0)},
            { Positions.WALL_RU, new Vector2(0,0)},
        };

    static public void GenerateDungeonObjects(int position_x, int position_y, ref Dungeon dungeon)
    {
        DungeonObjectCell.GenerateDungeonObject(position_x, position_y, ref dungeon);
        DungeonObjectItem.GenerateDungeonObject(position_x, position_y, ref dungeon);
    }

    public virtual void Redraw(int position_x, int position_y, ref Dungeon dungeon) { }

    public virtual void SetStatusOpened() { }
    public virtual void SetStatusShadowed() { }
    public virtual void SetStatusClosed() { }

    public void RedrawAs(Positions position)
    {
        transform.position = new Vector3(main_position.x + draw_delta[position].x, main_position.y + draw_delta[position].y, 0);
    }
}
