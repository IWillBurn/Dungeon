using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class Selecter : MonoBehaviour
{
    [SerializeField] EntityBehaviour entity;
    [SerializeField] PolygonCollider2D selecter_collider;
    Selectable selected;


    void Start()
    {
        selected = null;
    }

    public void UpdateCollider()
    {
        float h = 0.2f;
        selecter_collider.pathCount = 0;
        selecter_collider.pathCount++;
        Vector2 lu = new Vector2(0, 0);
        Vector2 ru = new Vector2(0, 0);
        Vector2 rd = new Vector2(0, 0);
        Vector2 ld = new Vector2(0, 0);

        float direction = entity.parameters[(int)EntityParametersKeys.VIEW_DIRECTION];

        lu.x = - h * Mathf.Sin(direction);
        lu.y = h * Mathf.Cos(direction);
        ld.x = h * Mathf.Sin(direction);
        ld.y = -h * Mathf.Cos(direction);

        float delta_x = Mathf.Cos(direction);
        float delta_y = Mathf.Sin(direction);

        ru.x = lu.x + delta_x;
        ru.y = lu.y + delta_y;
        rd.x = ld.x + delta_x;
        rd.y = ld.y + delta_y;
        Vector2[] path = new Vector2[4];
        path[0] = lu;
        path[1] = ru;
        path[2] = rd;
        path[3] = ld;

        selecter_collider.SetPath(selecter_collider.pathCount - 1, path);
    }

    float Distance(Vector2 a, Vector2 b) { return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.y - b.y, 2)); }

    void FindObjects()
    {
        float direction = entity.parameters[(int)EntityParametersKeys.VIEW_DIRECTION];
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, new Vector2(Mathf.Cos(direction), Mathf.Sin(direction)), entity.parameters[(int)EntityParametersKeys.HAND_SIZE]);
        int[] info = { 0, -1, 1, -2, 2, -3, 3, -4, 4, -5, 5 };
        Selectable result = null;
        Selectable min_result = null;
        for (int i = 0; i < info.Length; i++)
        {
            result = FindTargetByDirection(direction + info[i] * 0.31f);
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
        if (selected != null && Distance(transform.position, selected.transform.position) > 0.6f) selected.SetSelected(false);
    }

    Selectable FindTargetByDirection(float direction)
    {
        Selectable result = null;
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, new Vector2(Mathf.Cos(direction), Mathf.Sin(direction)), entity.parameters[(int)EntityParametersKeys.HAND_SIZE]);
        Debug.DrawRay(transform.position, new Vector2(Mathf.Cos(direction), Mathf.Sin(direction)) * entity.parameters[(int)EntityParametersKeys.HAND_SIZE], Color.yellow);
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit2D hit = hits[i];
            Selectable selectable = hit.collider.transform.gameObject.GetComponent<Selectable>();
            if (selectable != null)
            {
                result = selectable;
                break;
            }
        }
        return result;
    }

    void Update()
    {
        FindObjects();
    }
}
