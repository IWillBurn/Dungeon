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
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, new Vector2(Mathf.Cos(direction), Mathf.Sin(direction)), 0.5f);
        Debug.DrawRay(transform.position, new Vector2(Mathf.Cos(direction), Mathf.Sin(direction)) * 0.5f, Color.yellow);
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
                selected.SetSelected(true);
            }
        }
    }

    void Update()
    {
        if (finding == FindingObjects.OBJECT) FindObjects();
        else FindCells();
    }
}
