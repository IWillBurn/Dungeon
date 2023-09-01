using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DrawPositions
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
    public DrawPositions draw_positions;
    public Vector2 main_position;
    public PositionTypes position;
    public int in_map_coordinate_x;
    public int in_map_coordinate_y;
    public static Dictionary<DrawPositions, Vector2> draw_delta =
        new Dictionary<DrawPositions, Vector2>
        {
            { DrawPositions.CELL, new Vector2(0,0)},
            { DrawPositions.ITEM, new Vector2(0,0.438f)},
            { DrawPositions.FLOOR, new Vector2(0,0)},
            { DrawPositions.WALL_LD, new Vector2(0,0)},
            { DrawPositions.WALL_LU, new Vector2(0,0)},
            { DrawPositions.WALL_RD, new Vector2(0,0)},
            { DrawPositions.WALL_RU, new Vector2(0,0)},
        };

    static public void GenerateDungeonObjects(int position_x, int position_y, ref Dungeon dungeon)
    {
        DungeonObjectCell.GenerateDungeonObject(position_x, position_y, ref dungeon);
        DungeonObjectItem.GenerateDungeonObject(position_x, position_y, ref dungeon);
    }

    public virtual void Redraw(int position_x, int position_y, int size_w, int size_h, ref Dungeon dungeon) { }

    public virtual void SetStatusOpened() { }
    public virtual void SetStatusShadowed() { }
    public virtual void SetStatusClosed() { }

    public void RedrawAs(DrawPositions position)
    {
        transform.position = new Vector3(main_position.x + draw_delta[position].x, main_position.y + draw_delta[position].y, 0);
    }
}
