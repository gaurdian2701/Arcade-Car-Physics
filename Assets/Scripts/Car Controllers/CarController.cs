using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Car
{
    [RequireComponent(typeof(Rigidbody))]
    public class CarController : MonoBehaviour
    {
        public bool mshowDebug = false;

        [SerializeField] private Rigidbody mcarRigidBody;

        [Header("Car Torque Properties")] 
        [SerializeField] private AnimationCurve mtorqueCurve;
        [SerializeField] private float mtorquePower = 1.0f;
        
        [Header("Car Steering Properties - Default values are from Ford Mustang 5th gen")]
        [SerializeField] private float mwheelBaseLength = 2.72f;
        [SerializeField] private float mturnRadius = 11.5f;
        [SerializeField] private float mrearTrackLength = 1.6f;
        
        [Header("Car Wheels")]
        [SerializeField] private CarWheel mfrontLeftWheel;
        [SerializeField] private CarWheel mfrontRightWheel;
        [SerializeField] private CarWheel mrearLeftWheel;
        [SerializeField] private CarWheel mrearRightWheel;
        
        private float msteerInput = 0.0f;
        private float mthrottleInput = 0.0f;
        private float mrightWheelSteerAngle = 0.0f;
        private float mleftWheelSteerAngle = 0.0f;
        
        //If we accelerate, we go forward on the torque curve, and vice versa for deceleration. 
        //This value captures that point on the curve.
        private float mtorqueProgressionValue = 0.0f; 
        private float mcurrentTorque = 0.0f;
        private Vector3 debugAntiSlipVelocity = Vector3.zero;

        void Awake()
        {
        }
        private void Start()
        {
        }

        public void ReceiveInput(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            
            if (input.x > 0)
            {
                msteerInput = 1.0f;
            }
            else if (input.x < 0)
            {
                msteerInput = -1.0f;
            }
            else
            {
                msteerInput = 0.0f;
            }

            if (input.y > 0)
            {
                mthrottleInput = 1.0f;
            }
            else if (input.y < 0)
            {
                mthrottleInput = -1.0f;
            }
            else
            {
                mthrottleInput = 0.0f;
            }
        }

        private void FixedUpdate()
        {
            ThrottleCar();
        }

        private void Update()
        {
            SteerCar();
            CalculateTorque();
        }

        private void SteerCar()
        {
            if (msteerInput > 0.0f) //If we are steering right
            {
                mrightWheelSteerAngle = Mathf.Rad2Deg * Mathf.Atan2(mwheelBaseLength, mturnRadius - mrearTrackLength / 2) * msteerInput;
                mleftWheelSteerAngle = Mathf.Rad2Deg * Mathf.Atan2(mwheelBaseLength, mturnRadius + mrearTrackLength / 2) * msteerInput;
            }
            else if(msteerInput < 0.0f) //If we are steering left
            {
                mrightWheelSteerAngle = Mathf.Rad2Deg * Mathf.Atan2(mwheelBaseLength, mturnRadius + mrearTrackLength / 2) * msteerInput;
                mleftWheelSteerAngle = Mathf.Rad2Deg * Mathf.Atan2(mwheelBaseLength, mturnRadius - mrearTrackLength / 2) * msteerInput;
            }
            mfrontLeftWheel.transform.localRotation = Quaternion.AngleAxis(mleftWheelSteerAngle, Vector3.up);
            mfrontRightWheel.transform.localRotation = Quaternion.AngleAxis(mrightWheelSteerAngle, Vector3.up);
        }

        private void CalculateTorque()
        {
            //TODO: Simulate actual torque
            //For now, values from torque curve and apply that to the car
            mcurrentTorque = mtorqueCurve.Evaluate(mtorqueProgressionValue) * mtorquePower;
            
            if (mthrottleInput > 0.0f)
            {
                mtorqueProgressionValue += Time.deltaTime * 0.1f;
            }
            else if(mthrottleInput < 0.0f)
            {
                mtorqueProgressionValue -= Time.deltaTime * 0.1f;
            }
            else
            {
                mtorqueProgressionValue = 0.0f;
            }
        }
        private void ThrottleCar()
        {
            mrearLeftWheel.ApplyThrottleForce(mthrottleInput * mcurrentTorque);
            mrearRightWheel.ApplyThrottleForce(mthrottleInput * mcurrentTorque);
            mfrontLeftWheel.ApplyThrottleForce(mthrottleInput * mcurrentTorque);
            mfrontRightWheel.ApplyThrottleForce(mthrottleInput * mcurrentTorque);
        }
        
        void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Handles.Label(mfrontLeftWheel.transform.position, "Steer angle: " + mleftWheelSteerAngle);
            Handles.Label(mfrontRightWheel.transform.position, "Steer angle: " + mrightWheelSteerAngle);
            Handles.Label(transform.position, "Throttling Force: " + mthrottleInput * mcurrentTorque);
        }
    }
}
