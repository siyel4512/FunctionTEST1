using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        // iStep Starter Assets Character Controller improvements
        [Header("iStep Demo Extension")]
        [SerializeField, Tooltip("Please check out the Awake function in this script to inspect what layer collision-pairs get disabled at runtime when this variable is set to true.")]
        protected bool m_applyCustomRuntimeGlobalPhysicsIgnoreCollisionSettings = true;
        [SerializeField, Tooltip("Active / Deactivate iSteps algorithmic 'grounded behaviour' improvements")]
        protected bool m_useIStepGroundedImprovements = true; public bool useIstepGroundedImprovements { get { return m_useIStepGroundedImprovements; } set { m_useIStepGroundedImprovements = value; } }
        [SerializeField, Range(0.01f, 1), Tooltip("The radius for the spherecast used for abyss detection is the result of the multiplication of this value with the Grounded Radius")]
        protected float m_checkAbyssColliderRadiusMultiplier = 0.05f;
        protected Vector3 m_slopeVelocity = Vector3.zero;
        protected Vector3 m_prevAppliedSlopeVelocity = Vector3.zero;
        protected Vector3 m_currVelDir;
        protected Vector3 m_groundedNormal; public Vector3 groundedNormal { get { return m_groundedNormal; } }

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = -53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }


        private void Awake()
        {
            // iStep Starter Assets Character Controller improvements
            if (m_applyCustomRuntimeGlobalPhysicsIgnoreCollisionSettings)
            {
                // ignore layer collision for UI and some other layers
                Physics.IgnoreLayerCollision(0, 5);
                Physics.IgnoreLayerCollision(1, 5);
                Physics.IgnoreLayerCollision(2, 5);
                Physics.IgnoreLayerCollision(4, 5);
                Physics.IgnoreLayerCollision(5, 5);
            }

            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void OnEnable()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            _cinemachineTargetPitch = CinemachineCameraTarget.transform.rotation.eulerAngles.x;

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            _cinemachineTargetPitch = CinemachineCameraTarget.transform.rotation.eulerAngles.x;

            //_hasAnimator = TryGetComponent(out _animator);
            _animator = this.GetComponentInChildren<Animator>();
            _hasAnimator = _animator != null;
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            //_hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            if (m_useIStepGroundedImprovements) improvedGroundedCheckByiStep(); // imporved version of groundedcheck coming with iStep
            else GroundedCheck(); // default version of groundedcheck
            
            Move();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void improvedGroundedCheckByiStep()
        {
            // Function Copyright © Kreshnik Halili

            Vector3 charVelDir = _controller.velocity;
            charVelDir.y = 0;
            if (charVelDir.magnitude > 0.01f) m_currVelDir = charVelDir.normalized;

            m_groundedNormal = Vector3.up;

            if (_verticalVelocity > 0)
            {
                Grounded = false;

                m_slopeVelocity = Vector3.Lerp(m_slopeVelocity, Vector3.zero, Time.deltaTime * 20.0f);

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDGrounded, Grounded);
                }
                
                return;
            }
            
            GroundedRadius = _controller.radius * Mathf.Max(transform.localScale.x, transform.localScale.z);

            Vector3 targetSlopeVelocity = Vector3.zero;

            RaycastHit hit;
            float positiveGroundedOffset = Mathf.Abs(GroundedOffset);
            Vector3 origin = transform.position + transform.up * (GroundedRadius + positiveGroundedOffset);
            float twoTimesPositiveGroundedOffset = positiveGroundedOffset * 2;
            if (Physics.SphereCast(origin, GroundedRadius, -transform.up, out hit, twoTimesPositiveGroundedOffset, GroundLayers, QueryTriggerInteraction.Ignore))
            {
                // check if we are hitting another object on the opposite side of this hit
                Vector3 scaledCenterOffsetVec = _controller.center;
                scaledCenterOffsetVec.x *= transform.localScale.x;
                scaledCenterOffsetVec.y *= transform.localScale.y;
                scaledCenterOffsetVec.z *= transform.localScale.z;
                float scaledHeight = _controller.height * transform.localScale.y;
                Vector3 p1 = transform.position + scaledCenterOffsetVec + transform.up * (scaledHeight * 0.5f - GroundedRadius);
                Vector3 p2 = transform.position + scaledCenterOffsetVec - transform.up * (scaledHeight * 0.5f - GroundedRadius);

                float penetrationCastRadius = GroundedRadius * 0.99f;
                float additionalRayDist = GroundedRadius - penetrationCastRadius;

                Vector3 rayDirDistVec = transform.position - hit.point;
                Vector3 rayDirVec = rayDirDistVec.normalized;
                Vector3 projRayDistVec = Vector3.ProjectOnPlane(rayDirDistVec, transform.up);
                float lengthX = Mathf.Max(projRayDistVec.magnitude, 0.01f);
                float lengthX2 = GroundedRadius - lengthX;
                Vector3 hypothenuse2 = (lengthX2 / lengthX) * rayDirDistVec + rayDirVec * (additionalRayDist + twoTimesPositiveGroundedOffset);

                if (Physics.CapsuleCast(p1, p2, penetrationCastRadius, rayDirVec, hypothenuse2.magnitude, GroundLayers, QueryTriggerInteraction.Ignore))
                {
                    Grounded = true;
                }
                else
                {
                    // we continue with the normal behaviour
                    m_groundedNormal = hit.normal;
                    float angle = Vector3.Angle(hit.normal, transform.up);
                    RaycastHit hit2;

                    if (angle > _controller.slopeLimit)
                    {
                        Vector3 raycastOrigin = transform.position + transform.up * (positiveGroundedOffset + _controller.stepOffset) + m_currVelDir * GroundedRadius;
                        if (Physics.Raycast(raycastOrigin, -transform.up, out hit2, twoTimesPositiveGroundedOffset + _controller.stepOffset * 2.0f, GroundLayers, QueryTriggerInteraction.Ignore))
                        {
                            if (Vector3.Angle(hit2.normal, transform.up) > _controller.slopeLimit)
                            {
                                Grounded = false;
                                Vector3 rightVec = Vector3.Cross(hit.normal, transform.up).normalized;
                                targetSlopeVelocity = Vector3.Cross(hit.normal, rightVec).normalized;
                            }
                            else
                            {
                                if (m_checkAbyssColliderRadiusMultiplier > 0.999f)
                                {
                                    Grounded = true;
                                }
                                else
                                {
                                    float abyssRadius = m_checkAbyssColliderRadiusMultiplier * GroundedRadius;
                                    raycastOrigin = transform.position + transform.up * (positiveGroundedOffset + abyssRadius);
                                    if (Physics.SphereCast(raycastOrigin, abyssRadius, -transform.up, out hit2, twoTimesPositiveGroundedOffset + _controller.stepOffset, GroundLayers, QueryTriggerInteraction.Ignore))
                                    {
                                        Grounded = true;
                                    }
                                    else
                                    {
                                        Grounded = false;
                                        Vector3 rightVec = Vector3.Cross(hit.normal, transform.up).normalized;
                                        targetSlopeVelocity = Vector3.Cross(hit.normal, rightVec).normalized;
                                    }
                                }
                            }
                        }
                        else
                        {
                            Grounded = false;
                            Vector3 rightVec = Vector3.Cross(hit.normal, transform.up).normalized;
                            targetSlopeVelocity = Vector3.Cross(hit.normal, rightVec).normalized;
                        }
                    }
                    else
                    {
                        if (m_checkAbyssColliderRadiusMultiplier > 0.999f)
                        {
                            Grounded = true;
                        }
                        else
                        {
                            float abyssRadius = m_checkAbyssColliderRadiusMultiplier * GroundedRadius;
                            Vector3 raycastOrigin = transform.position + transform.up * (positiveGroundedOffset + abyssRadius);
                            if (Physics.SphereCast(raycastOrigin, abyssRadius, -transform.up, out hit2, twoTimesPositiveGroundedOffset + _controller.stepOffset, GroundLayers, QueryTriggerInteraction.Ignore))
                            {
                                Grounded = true;
                            }
                            else
                            {
                                Grounded = false;
                                Vector3 rightVec = Vector3.Cross(hit.normal, transform.up).normalized;
                                targetSlopeVelocity = Vector3.Cross(hit.normal, rightVec).normalized;
                            }
                        }
                    }
                }
            }
            else
            {
                Grounded = false;
            }

            if (targetSlopeVelocity.magnitude > 0.001f)
            {
                m_slopeVelocity += targetSlopeVelocity * Time.deltaTime * Mathf.Abs(Gravity);
            }
            else
            {
                m_slopeVelocity = Vector3.Lerp(m_slopeVelocity, Vector3.zero, Time.deltaTime * 20.0f);
            }

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            // + iStep requirement
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x - m_prevAppliedSlopeVelocity.x, 0.0f, _controller.velocity.z - m_prevAppliedSlopeVelocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }


            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            if (m_useIStepGroundedImprovements)
            {
                Vector3 slopeVelocityToUse = calculateSlopeVelocityToUseByiStep(targetDirection, _speed); //iStep improvement

                // move the player
                _controller.Move(targetDirection.normalized * _speed * Time.deltaTime +
                                 new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime +
                                 slopeVelocityToUse * Time.deltaTime /* iStep improvement */);

                m_prevAppliedSlopeVelocity = slopeVelocityToUse;
            }
            else
            {
                // move the player
                _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                                 new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
            }

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private Vector3 calculateSlopeVelocityToUseByiStep(Vector3 targetDirection, float movingVel)
        {
            // Function Copyright © Kreshnik Halili

            if (m_slopeVelocity.magnitude > 0.001f && movingVel > 0.001f)
            {
                Vector3 verticalVel = Vector3.Project(m_slopeVelocity, transform.up);
                Vector3 slopeVelPlanar = m_slopeVelocity - verticalVel;

                Vector3 subtract = Vector3.Project(targetDirection.normalized * movingVel, slopeVelPlanar.normalized);
                Vector3 nextSlopeVelPlanar = slopeVelPlanar - subtract;

                float dot = Vector3.Dot(nextSlopeVelPlanar.normalized, slopeVelPlanar.normalized);

                if (dot > 0) return nextSlopeVelPlanar + verticalVel; // this is true when the player runs in the opposite direction as slopevelocity or in the same direction as slopevelocity but less than planarslopevelocity (we don't have to scale verticalVel by the length of the new planar vel in regard to the old planar vel)
                else return Vector3.zero;//verticalVel; // in this case the player runs faster in the same direction as slopevelocity so we don't add more planare velocity in this case
            }
            //else if (Grounded) return Vector3.zero; // this case makes collision worse so we don't use it

            return m_slopeVelocity;
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    if (m_useIStepGroundedImprovements)
                    {
                        _verticalVelocity = Mathf.Lerp(Gravity, -2.0f, Vector3.Dot(Vector3.up, m_groundedNormal));
                    }
                    else
                    {
                        _verticalVelocity = -2f;
                    }
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // reset the jump timeout timer
                    _jumpTimeoutDelta = JumpTimeout;

                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity > _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }

            if (_verticalVelocity > 0)
            {
                // check if we are colliding with the roof
                Vector3 scaledCenterOffsetVec = _controller.center;
                scaledCenterOffsetVec.x *= transform.localScale.x;
                scaledCenterOffsetVec.y *= transform.localScale.y;
                scaledCenterOffsetVec.z *= transform.localScale.z;
                float scaledHeight = _controller.height * transform.localScale.y;
                float scaledRadius = _controller.radius * Mathf.Max(transform.localScale.x, transform.localScale.z);
                Vector3 origin = transform.position + scaledCenterOffsetVec + transform.up * (scaledHeight * 0.5f - scaledRadius);

                float positiveGroundedOffset = Mathf.Abs(GroundedOffset);
                float penetrationCastRadius = scaledRadius * 0.99f;
                float rayDist = scaledRadius - penetrationCastRadius + positiveGroundedOffset;

                RaycastHit hit;
                if (Physics.SphereCast(origin, penetrationCastRadius, transform.up, out hit, rayDist, GroundLayers, QueryTriggerInteraction.Ignore))
                {
                    _jumpTimeoutDelta = 0.0f;
                    _verticalVelocity = Mathf.Lerp(_verticalVelocity, 0.0f, Time.deltaTime * 10.0f);
                }
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            if (m_useIStepGroundedImprovements) return;

            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (LandingAudioClip != null) // iStep fix
                {
                    AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }
    }
}