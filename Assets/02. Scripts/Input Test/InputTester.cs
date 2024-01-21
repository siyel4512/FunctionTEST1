// 참고 : https://daekyoulibrary.tistory.com/entry/Unity-New-Input-System-2-%EC%8A%A4%ED%81%AC%EB%A6%BD%ED%8A%B8%EB%A1%9C-%EC%A0%9C%EC%96%B4%ED%95%98%EA%B8%B0
// 참고 : https://forum.unity.com/threads/new-input-system-how-to-use-the-hold-interaction.605587/page-3
// 참고 : https://lektion-von-erfolglosigkeit.tistory.com/233
// 참고 : https://velog.io/@lunetis/zero-1
// 참고 블로그 : https://github.com/lunetis/OperationZERO?tab=readme-ov-file

using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class InputTester : MonoBehaviour
{
    public bool isSendMessages;
    public PlayerInput input;
    public InputActionMap playerActionMap;
    public InputAction action1;
    public InputAction action2;

    private float moveTime;
    public float delayTime = 1f;

    // Start is called before the first frame update
    void Start()
    {
    
    }

    //// Update is called once per frame
    //void Update()
    //{
    //    //curVec = Vector2.SmoothDamp(curVec, moveVec, ref smoothInputVelocity, smoothRatio, maxSpeed);
    //    //Debug.Log(curVec);

    //    //Debug.Log(moveVec);

    //    if (moveVec != Vector2.zero)
    //    {
    //        moveTime += Time.deltaTime;
    //        float t = moveTime / delayTime;
    //        Debug.Log(Mathf.Lerp(0f, moveVec.y, t));
    //    }
    //    else
    //    {
    //        moveTime = 0;
    //    }
    //}

    public Vector2 smoothInputVelocity;
    public Vector2 moveVec;
    public Vector2 curVec;
    public float smoothRatio;
    public float maxSpeed;

    private void SmoothDamp_Test()
    {
        
    }

    // Behavior "Send Messges"
    private void OnMove(InputValue value)
    {
        if (isSendMessages)
        {
            //Debug.Log($"OnMove value : {value.Get<Vector2>()}");
            moveVec = value.Get<Vector2>();
        }
    }

    private void OnJump()
    {
        if (isSendMessages)
        {
            //Debug.Log($"OnJump value : {value.Get<float>()}");
            Debug.Log("Jump!!!");
        }
    }

    // Behavior "Brodcast Messges"

    // Behavior "Invoke Unity Events"
    private void OnEnable()
    {
        if (!isSendMessages)
        {
            input = GetComponent<PlayerInput>();
            playerActionMap = input.actions.FindActionMap("InputTest");
            action1 = playerActionMap.FindAction("Test");
            action2 = playerActionMap.FindAction("Move");

            action1.performed += Test;
            action2.performed += TestMove;
        }
    }

    private void OnDisable()
    {
        if (!isSendMessages)
        {
            action1.performed -= Test;
            action2.performed -= TestMove;
        }
    }

    private void TestMove(InputAction.CallbackContext context)
    {
        Vector2 value = context.ReadValue<Vector2>();
        Debug.Log($"TestMove : {value}");
    }

    float tempValue = 0f;

    private void Test(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        Debug.Log("Test Input1 : " + value);

        //Vector2 temp = Vector2.SmoothDamp()

        Debug.Log("Test Input2 : " + value);
    }

    // Behavior "Invoke C Sharp Events"
}
