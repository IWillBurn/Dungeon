using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class PlayerBehaviour : EntityBehaviour
{

    void Start()
    {
        sprite_controller.SetDrawBorder(1f);
    }

    public void FixedUpdate()
    {
        sprite_controller.SetOrder(Mathf.FloorToInt((1000 - transform.position.y) * 100));
        float delta_x = 0;
        float delta_y = 0;
        float view_delta_x = 0;
        float view_delta_y = 0;
        if (Input.GetKey(KeyCode.D)) delta_x += 1;
        if (Input.GetKey(KeyCode.S)) delta_y -= 0.5f;
        if (Input.GetKey(KeyCode.A)) delta_x -= 1;
        if (Input.GetKey(KeyCode.W)) delta_y += 0.5f;
        if (delta_y != 0 || delta_x != 0)
        {
            float angle = Mathf.Atan2(delta_y, delta_x);
            parameters[(int) EntityParametersKeys.MOVE_DIRECTION] = angle;
            transform.position = new Vector2(transform.position.x + parameters[(int)EntityParametersKeys.SPEED] * Mathf.Cos(angle), transform.position.y + parameters[(int)EntityParametersKeys.SPEED] * Mathf.Sin(angle));
        }

        if (Input.GetKey(KeyCode.RightArrow)) view_delta_x += 1;
        if (Input.GetKey(KeyCode.DownArrow)) view_delta_y -= 0.5f;
        if (Input.GetKey(KeyCode.LeftArrow)) view_delta_x -= 1;
        if (Input.GetKey(KeyCode.UpArrow)) view_delta_y += 0.5f;
        if (view_delta_x != 0 || view_delta_y != 0) parameters[(int)EntityParametersKeys.VIEW_DIRECTION] = Mathf.Atan2(view_delta_y, view_delta_x);
        else parameters[(int)EntityParametersKeys.VIEW_DIRECTION] = parameters[(int)EntityParametersKeys.MOVE_DIRECTION];
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<DungeonObjectCell>() != null)
        {
            cell = collision.gameObject.GetComponent<DungeonObjectCell>();
            controller.MoveToCell(ref cell);
        }
    }
}
