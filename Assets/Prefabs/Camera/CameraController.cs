using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class CameraTarget
{
    public Vector2 target;
    public int delay;
    public CameraTarget(Vector2 _target, int _delay)
    {
        target = _target;
        delay = _delay;
    }
}

public class CameraController : MonoBehaviour
{
    Queue<CameraTarget> targets = new Queue<CameraTarget>();
    CameraTarget target;
    CameraTarget default_target;
    float x, y;
    int delay_time, delay_end;

    public void Start()
    {
        x = transform.position.x;
        y = transform.position.y;
        delay_time = 0;
        delay_end = 0;
        target = null;
        default_target = new CameraTarget(new Vector2(x, y), 0);
    }

    public void ChangeDefaultTarget(Vector2 _target)
    {
        default_target.target = _target;
    }

    public void AddTarget(Vector2 _target, int _delay)
    {
        targets.Enqueue(new CameraTarget(_target, _delay));
    }

    public void Move(CameraTarget camera_target)
    {
        x += (camera_target.target.x - x) / 8;
        y += (camera_target.target.y - y) / 8;
        if (!NeedMove(camera_target)) {
            x = camera_target.target.x;
            y = camera_target.target.y;
        }
        transform.position = new Vector3(x, y, -10);
    }

    public bool NeedMove(CameraTarget camera_target)
    {
        return Mathf.Sqrt(Mathf.Pow(camera_target.target.x - x, 2) + Mathf.Pow(camera_target.target.y - y, 2)) > 0.01;
    }

    public void NextTarget()
    {
        if (targets.Count != 0) target = targets.Dequeue();
        else target = null;
    }

    void Update()
    {
        if (target == null)
        {
            NextTarget();
        }
        if (target != null)
        {
            if (NeedMove(target)) Move(target);
            else
            {
                delay_time = 0;
                delay_end = target.delay;
                NextTarget();
            }
        }
        else {
            if (NeedMove(default_target)) Move(default_target);
        }
    }
}
