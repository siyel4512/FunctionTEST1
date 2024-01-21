using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputCarControl : MonoBehaviour
{
    // move
    private Vector2 direction;
    private float moveTime = 0;
    public float moveDelayTime = 0.5f;

    // brake
    public bool isBrake;
    public float brakeTime = 0;
    public float brakeDelayTime = 0.5f;

    // drift
    public bool isDrift;
    public float driftTime = 0;
    public float driftDelayTime = 0.5f;

    // change camera view
    public bool isChangeCameraView;

    private float rotation;
    private float torque;
    private float brake;
    private float drift;

    public float Rotation => rotation;
    public float Torque => torque;
    public float Brake => brake;
    public float Drift => drift;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // move
        if (direction != Vector2.zero)
        {
            moveTime += Time.deltaTime;
            float t = moveTime / moveDelayTime;
            rotation = Mathf.Lerp(0f, direction.x, t);
            torque = Mathf.Lerp(0f, direction.y, t);
        }
        else
        {
            moveTime = 0;
        }

        // brake
        if (isBrake)
        {
            brakeTime += Time.deltaTime;
            float t = brakeTime / brakeDelayTime;
            brake = Mathf.Lerp(0f, 1f, t);
        }
        else
        {
            brakeTime = 0;
            brake = 0;
        }

        // drift
        if (isDrift)
        {
            driftTime += Time.deltaTime;
            float t = driftTime / driftDelayTime;
            brake = Mathf.Lerp(0f, 1f, t);
        }
        else
        {
            driftTime = 0;
            drift = 0;
        }
    }

    #region new input system
    public void OnMove(InputValue value)
    {
        direction = value.Get<Vector2>();
    }

    public void OnBrake()
    {
        if (!isBrake)
        {
            isBrake = true;
        }
        else
        {
            isBrake = false;
        }
    }

    public void OnDrift()
    {
        if (!isDrift)
        {
            isDrift = true;
        }
        else
        {
            isDrift = false;
        }
    }

    public void OnChangeCameraView()
    {
        Debug.Log($"OnCamControl");

        isChangeCameraView = true;
    }

    public void OnLook(InputValue value)
    {
        //Debug.Log($"OnLook : {value}");
    }
    #endregion
}
