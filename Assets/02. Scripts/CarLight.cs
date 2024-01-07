using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarLight : MonoBehaviour
{
    public enum Side
    {
        Front,
        Back
    }

    [Serializable]
    public struct Light
    {
        public GameObject lightObj;
        public Material lightMat;
        public Side side;
    }

    public List<Light> lights;

    public bool isFrontLightOn;
    public bool isBackLightOn;

    // Start is called before the first frame update
    void Start()
    {
        isFrontLightOn = false;
        isBackLightOn = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.H))
        {
            OperateFrontLights();
        }
    }

    public void OperateFrontLights()
    {
        isFrontLightOn = !isFrontLightOn;

        if (isFrontLightOn)
        {
            // Turn On Lights
            foreach(var light in lights)
            {
                if (light.side == Side.Front && light.lightObj.activeInHierarchy == false)
                {
                    light.lightObj.SetActive(true);
                }
            }
        }
        else
        {
            // Turn Off Lights
            foreach (var light in lights)
            {
                if (light.side == Side.Front && light.lightObj.activeInHierarchy == true)
                {
                    light.lightObj.SetActive(false);
                }
            }
        }
    }

    public void OperateBackLights()
    {
        if (isBackLightOn)
        {
            // Turn On Lights
            foreach (var light in lights)
            {
                if (light.side == Side.Back && light.lightObj.activeInHierarchy == false)
                {
                    light.lightObj.SetActive(true);
                }
            }
        }
        else
        {
            // Turn Off Lights
            foreach (var light in lights)
            {
                if (light.side == Side.Back && light.lightObj.activeInHierarchy == true)
                {
                    light.lightObj.SetActive(false);
                }
            }
        }
    }
}
