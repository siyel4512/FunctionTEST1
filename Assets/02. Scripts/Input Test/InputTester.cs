// 참고 블로그 : https://daekyoulibrary.tistory.com/entry/Unity-New-Input-System-2-%EC%8A%A4%ED%81%AC%EB%A6%BD%ED%8A%B8%EB%A1%9C-%EC%A0%9C%EC%96%B4%ED%95%98%EA%B8%B0
// 참고 블로그 : https://forum.unity.com/threads/new-input-system-how-to-use-the-hold-interaction.605587/page-3
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputTester : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Behavior "Send Messges"
    private void OnMove(InputValue value)
    {
        Debug.Log($"OnMove value : {value.Get()}");
    }

    private void OnJump(InputValue value)
    {
        Debug.Log("Jump!!!");
    }

    // Behavior "Brodcast Messges"
    // Behavior "Invoke Unity Events"
    // Behavior "Invoke C Sharp Events"
}
