// 참고 블로그 : https://www.youtube.com/watch?v=eMLOrApviE4&list=PLyh3AdCGPTSLg0PZuD1ykJJDnC1mThI42&index=8
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CarController2 : MonoBehaviour
{
    public enum Axel
    {
        Front,
        Rear
    }

    [Serializable]
    public struct Wheel
    {
        public GameObject wheelModel;
        public WheelCollider wheelCollider;
        public GameObject wheelEffectObj;
        public ParticleSystem smokeParticle;
        public Axel axel;
    }

    public float maxAcceleration = 30.0f;
    public float brakeAcceleration = 50.0f;

    public float turnSensitivity = 1.0f;
    public float maxSteerAngle = 30.0f;

    public Vector3 _centerOfMass;

    public Transform steerModel;

    public List<Wheel> wheels;

    private float moveInput;
    private float steerInput;

    private Rigidbody carRigid;

    private CarLight carLight;

    private void Start()
    {
        carRigid = GetComponent<Rigidbody>();
        carRigid.centerOfMass = _centerOfMass;

        carLight = GetComponent<CarLight>();
    }

    private void Update()
    {
        GetInputs();
        AnimateSteer();
        AnimateWheels();
        WheelEffect();
    }

    private void LateUpdate()
    {
        Move();
        Steer();
        Brake();
    }

    private void GetInputs()
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
    }

    private void Move()
    {
        foreach(var wheel in wheels)
        {
            wheel.wheelCollider.motorTorque = moveInput * maxAcceleration * Time.deltaTime * 2500f;
        }
    }

    private void Steer()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                var _steerAngle = steerInput * turnSensitivity * maxSteerAngle;
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, _steerAngle,0.6f);
            }
        }
    }

    private void Brake()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            foreach(var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 300 * brakeAcceleration * Time.deltaTime;
            }

            carLight.isBackLightOn = true;
            carLight.OperateBackLights();
        }
        else
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 0;
            }

            carLight.isBackLightOn = false;
            carLight.OperateBackLights();
        }
    }

    private void AnimateSteer()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                var _steerAngle = steerInput * turnSensitivity * maxSteerAngle;
                steerModel.localEulerAngles = new Vector3(steerModel.localEulerAngles.x, steerModel.localEulerAngles.y, -(Mathf.Lerp(wheel.wheelCollider.steerAngle, _steerAngle, 0.6f)));
            }
        }
    }

    private void AnimateWheels()
    {
        foreach(var wheel in wheels)
        {
            Quaternion rot;
            Vector3 pos;
            wheel.wheelCollider.GetWorldPose(out pos, out rot);
            wheel.wheelModel.transform.position = pos;
            wheel.wheelModel.transform.rotation = rot;
        }
    }

    private void WheelEffect()
    {
        foreach(var wheel in wheels)
        {
            //if (Input.GetKey(KeyCode.Space))
            if (Input.GetKey(KeyCode.Space) && wheel.axel == Axel.Rear)
            {
                wheel.wheelEffectObj.GetComponentInChildren<TrailRenderer>().emitting = true;
                wheel.smokeParticle.Emit(1);
            }
            else
            {
                wheel.wheelEffectObj.GetComponentInChildren<TrailRenderer>().emitting = false;
            }
        }
    }
}
