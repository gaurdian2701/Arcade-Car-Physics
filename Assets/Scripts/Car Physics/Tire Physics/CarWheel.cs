using System;
using UnityEngine;

namespace Car
{
    public class CarWheel : MonoBehaviour
    {
        [SerializeField] private Rigidbody mparentRigidbody; 
        [SerializeField] private GameObject mspring;
        [SerializeField] private GameObject mwheelMesh;
        
        public bool mshowSpringDebug = false;
        public bool mshowWheelDebug = false;
        public bool misGrounded = false;

        #region Suspension properties
        
        [Header("Spring Properties")]
        [SerializeField] private float mspringConstant = 1.0f;
        [SerializeField] private float mspringRestLength = 1.0f;
        [SerializeField] private float mspringTravelLength = 0.5f;
        [SerializeField] private float mspringDampingConstant = 1.0f;
        [SerializeField] private float mwheelRadius = 1.0f;

        private Vector3 mfinalWheelRestingPosition = Vector3.zero;
        private RaycastHit mspringCastHitInfo;
        
        #endregion
        
        #region Wheel properties
        
        [Header("Wheel Properties")]
        public bool misLeftWheel = false;
        [SerializeField] [Range(0.0f, 3.0f)] private float mgrip = 1.0f;
        #endregion
        
        #region Physics and Forces variables
        
        private Vector3 mspringRestorationForce = Vector3.zero; //This is also the N value in kinetic friction F = mu * N
        private Vector3 mwheelVelocity = Vector3.zero;
        
        private float mspringLengthCurrentFrame = 0.0f;
        private float mspringLengthPreviousFrame = 0.0f;
        private float mspringVelocity = 0.0f;
        private float mminspringLength = 0.0f;
        private float mmaxspringLength = 0.0f;
        private float mparentMass = 0.0f;
        
        #endregion

        #region Debug
        private Vector3 mdebugWheelProbePoint = Vector3.zero;
        private Vector3 mdebugLocalSlidingVelocity = Vector3.zero;
        private Vector3 mdebugCounterSlideForce = Vector3.zero;
        #endregion
        
        void Start()
        {
            mspringLengthCurrentFrame = mspringRestLength;
            mspringLengthPreviousFrame = mspringLengthCurrentFrame;
            mminspringLength = mspringRestLength - mspringTravelLength;
            mmaxspringLength = mspringRestLength + mspringTravelLength;
            mparentMass = mparentRigidbody.mass;
        }

        void FixedUpdate()
        {
            mwheelVelocity = mparentRigidbody.GetPointVelocity(transform.position);
            
            CalculateWheelRestingPosition();
            CalculateRestorationForce();
            ApplySpringForces();
            ApplySidewaysFriction();
        }

        private void CalculateWheelRestingPosition()
        {
            //Using raycasts to determine where to place the wheel
            if (Physics.Raycast(mspring.transform.position, -mparentRigidbody.transform.up, out mspringCastHitInfo,
                    mspringRestLength + mwheelRadius))
            {
                //If raycast hits the ground, place the wheel on the ground and apply lifting/spring forces
                //to the car since the spring "compresses"
                mdebugWheelProbePoint = mspringCastHitInfo.point;
                mfinalWheelRestingPosition = mspringCastHitInfo.point + mwheelRadius * mparentRigidbody.transform.up;
                mspringLengthCurrentFrame = mspringCastHitInfo.distance;
                misGrounded = true;
            }
            else
            {
                //Else, simply hang the spring in the air at rest length
                mdebugWheelProbePoint = mspring.transform.position -
                                       (mspringRestLength + mwheelRadius)
                                       * mparentRigidbody.transform.up;
                mfinalWheelRestingPosition = mwheelMesh.transform.position;
                mspringLengthCurrentFrame = mspringRestLength;
                misGrounded = false;
            }
            
            mspringLengthCurrentFrame = Mathf.Clamp(mspringLengthCurrentFrame, mminspringLength, mmaxspringLength);
            mwheelMesh.transform.position = mfinalWheelRestingPosition;
        }

        //NOTE: ISOLATE SPRING LOGIC - IT DOES NOT CARE ABOUT WHEEL POSITIONS AND OUTSIDE FORCES. ONLY IT'S OWN LENGTH
        private void CalculateRestorationForce()
        {
            mspringVelocity = (mspringLengthCurrentFrame - mspringLengthPreviousFrame) / Time.fixedDeltaTime;
            mspringLengthPreviousFrame = mspringLengthCurrentFrame;
            
            //Calculate CHANGE in spring's length over time and take that as velocity to multiply with the damping constant
            float displacement = mspringRestLength - mspringLengthCurrentFrame;
            float springForce = mspringConstant * displacement;
            float dampingForce = mspringVelocity * mspringDampingConstant;
            
            mspringRestorationForce = (springForce - dampingForce)
                                * mparentRigidbody.transform.up;
        }
        
        private void ApplySpringForces()
        {
            mparentRigidbody.AddForceAtPosition(mspringRestorationForce, mspring.transform.position);
        }

        private void ApplySidewaysFriction()
        {
            if (misGrounded)
            {
                float slideVelocity = Vector3.Dot(mwheelVelocity, transform.right);
                
                float maxFriction = mgrip * mspringRestorationForce.magnitude;
                float desiredAcceleration = slideVelocity / Time.fixedDeltaTime;
                float desiredFrictionForce = -mparentMass * desiredAcceleration; //F = m * a;
                
                desiredFrictionForce = Mathf.Clamp(desiredFrictionForce, -maxFriction, maxFriction); 
                
                mdebugCounterSlideForce = desiredFrictionForce * transform.right;
                mdebugLocalSlidingVelocity = slideVelocity * transform.right;
                mparentRigidbody.AddForceAtPosition(desiredFrictionForce * transform.right, transform.position);
            }
            else
            {
                mdebugCounterSlideForce = Vector3.zero;
            }
        }

        public void ApplyThrottleForce(float someThrottleForce)
        {
            if (misGrounded)
            {
                mparentRigidbody.AddForceAtPosition(someThrottleForce * transform.forward, transform.position);
            }
        }
        void OnDrawGizmos()
        {
            if (mshowWheelDebug)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(mfinalWheelRestingPosition, mwheelRadius);
                
                Gizmos.color = Color.magenta;
                Gizmos.DrawCube(mdebugWheelProbePoint, new Vector3(0.1f, 0.1f, 0.1f));

                Gizmos.color = Color.orange;
                Gizmos.DrawLine(transform.position, transform.position + mparentRigidbody.GetPointVelocity(transform.position) * 3.0f);
                
                Gizmos.color = Color.darkGreen;
                Gizmos.DrawLine(transform.position, transform.position + mdebugCounterSlideForce);
                
                Gizmos.color = Color.darkViolet;
                Gizmos.DrawLine(transform.position, transform.position + mdebugLocalSlidingVelocity);
            }
            
            if (mshowSpringDebug)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(mspring.transform.position, 0.1f);

                Gizmos.color = Color.green;
                Gizmos.DrawLine(mspring.transform.position, mdebugWheelProbePoint);

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(mspring.transform.position, transform.position + mspringRestorationForce);
            }
        }
    }
}

