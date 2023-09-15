using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public enum FindingObjects
{
    CELL,
    OBJECT,
}

public class Selecter : MonoBehaviour
{
    [SerializeField] PolygonCollider2D selecter_collider;
    public EntityBehaviour entity;
    public Selectable selected;
    public FindingObjects finding;

    public Positions shadow_type;
    public GameObject shadow;
    public SpriteRenderer shadow_sprite_renderer;
    public ObjectDefaultSpriteController shadow_sprite_controller;
    public DungeonObject shadow_dungeon_object;

    void Start()
    {
        selected = null;
        finding = FindingObjects.OBJECT;
    }

    float Distance(Vector2 a, Vector2 b) { return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.y - b.y, 2)); }
    Vector2 Normalize(Vector2 a) {
        float size = Mathf.Sqrt(Mathf.Pow(a.x, 2) + Mathf.Pow(a.y, 2));
        return new Vector2(a.x / size, a.y / size);
    }

    public void StartFindObject()
    {
        finding = FindingObjects.OBJECT;
        selected = null;
        if (shadow != null) Destroy(shadow);
    }

    public void StartFindCell(ObjectsKeys key)
    {
        finding = FindingObjects.CELL;
        selected = null;
        shadow = Instantiate(entity.controller.GetDungeonController().GetDugneon().dictionary.GetByKey(key), new Vector3(1000, 1000, 0), new Quaternion());
        Behaviour[] components = shadow.GetComponents<Behaviour>();
        for (int i = 0; i < components.Length; i++)
        {
            components[i].enabled = false;
        }
        if (shadow.GetComponent<DungeonObjectItem>() != null) shadow_type = Positions.ITEM;
        shadow_dungeon_object = shadow.GetComponent<DungeonObject>();
        shadow_dungeon_object.enabled = true;
        shadow_sprite_renderer = shadow.GetComponent<SpriteRenderer>();
        shadow_sprite_renderer.enabled = true;
        shadow_sprite_controller = shadow.GetComponent<ObjectDefaultSpriteController>();
        shadow_sprite_controller.enabled = true;
        shadow_sprite_controller.SetAlpha(0.55f);
        shadow_sprite_controller.SetMainColor(new Color(0f, 1f, 0.15f));
    }

    void FindObjects()
    {
        float direction = entity.parameters[(int)EntityParametersKeys.VIEW_DIRECTION];
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, new Vector2(Mathf.Cos(direction), Mathf.Sin(direction)), entity.parameters[(int)EntityParametersKeys.HAND_SIZE]);
        int[] info = { 0, -1, 1, -2, 2, -3, 3, -4, 4, -5, 5 };
        Selectable result = null;
        Selectable min_result = null;
        for (int i = 0; i < info.Length; i++)
        {
            result = FindObjectByDirection(direction + info[i] * 0.31f);
            if (result == null) continue;
            if (result != null && i == 0)
            {
                min_result = result;
                break;
            }
            if (min_result == null || Distance(transform.position, min_result.transform.position) > Distance(transform.position, result.transform.position)) min_result = result;
        }

        if (min_result != null && !min_result.IsSelected())
        {
            if (selected != null) selected.SetSelected(false);
            min_result.SetSelected(true);
            selected = min_result;
        }
        if (selected != null && Distance(transform.position, selected.transform.position) > 0.6f) {
            selected.SetSelected(false);
            selected = null;
        }
    }

    Selectable FindObjectByDirection(float direction)
    {
        Selectable result = null;
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, new Vector2(Mathf.Cos(direction), Mathf.Sin(direction)), entity.parameters[(int)EntityParametersKeys.HAND_SIZE]);
        Debug.DrawRay(transform.position, new Vector2(Mathf.Cos(direction), Mathf.Sin(direction)) * entity.parameters[(int)EntityParametersKeys.HAND_SIZE], Color.yellow);
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit2D hit = hits[i];
            Selectable selectable = hit.collider.transform.gameObject.GetComponent<Selectable>();
            if (selectable != null && selectable.gameObject.GetComponent<DungeonObjectItem>() != null)
            {
                result = selectable;
                break;
            }
        }
        return result;
    }

    void FindCells()
    {
        float direction = entity.parameters[(int)EntityParametersKeys.VIEW_DIRECTION];
        RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(transform.position.x, transform.position.y - 0.1f), new Vector2(Mathf.Cos(direction), Mathf.Sin(direction)), 0.8f);
        Debug.DrawRay(new Vector2(transform.position.x, transform.position.y - 0.1f), new Vector2(Mathf.Cos(direction), Mathf.Sin(direction)) * 0.8f, Color.red);
        bool first = true; 
        for (int i = 0; i < hits.Length; i++)
        {
            Selectable result = hits[i].collider.gameObject.GetComponent<Selectable>();
            DungeonObjectCell cell = hits[i].collider.gameObject.GetComponent<DungeonObjectCell>();
            if (result != null && cell != null)
            {
                if (first) {first = false; continue; }
                if (selected != null) selected.SetSelected(false);
                selected = result;
                if (shadow_type == Positions.ITEM && entity.controller.GetDungeonController().GetDugneon().dungeon_map.map[cell.in_map_coordinate_x][cell.in_map_coordinate_y].item.object_key == ObjectsKeys.NONE
                    && entity.controller.GetDungeonController().GetDugneon().dungeon_map.map[cell.in_map_coordinate_x][cell.in_map_coordinate_y].status == RoomStatuses.OPENED)
                {
                    selected.SetSelected(true);
                    shadow_dungeon_object.Redraw(cell.in_map_coordinate_x, cell.in_map_coordinate_y, ref entity.controller.GetDungeonController().GetDugneon());
                    break;
                }
                selected = null;

            }
        }
    }

    void Update()
    {
        if (finding == FindingObjects.OBJECT) FindObjects();
        else FindCells();
    }
}
