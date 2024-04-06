using GameX.Engine.UI;
using System;
using UnityEngine;

namespace GameX.Engine.Components
{
    public class PlayerComponent : MonoBehaviour
    {
        const RenderingPath RenderPath = RenderingPath.Forward;
        const float CameraFarClip = 500.0f;
        const bool HasRunAndSlow = false;
        const bool HasLight = false;

        Transform _camTransform;
        Transform _transform;
        CapsuleCollider _capsuleCollider;
        Rigidbody _rigidbody;
        UICrosshair _crosshair;
        bool _isGrounded = false;
        bool _isFlying = false;

        [Header("Movement Settings")]
        public float slowSpeed = 3;
        public float normalSpeed = 5;
        public float fastSpeed = 10;
        public float flightSpeedMultiplier = 3 * 5;
        public float airborneForceMultiplier = 5;
        public float mouseSensitivity = 3;
        public float minVerticalAngle = -90;
        public float maxVerticalAngle = 90;

        [Header("Misc")]
        public Light lantern;
        public Transform leftHand;
        public Transform rightHand;

        public bool IsFlying
        {
            get => _isFlying;
            set
            {
                _isFlying = value;
                if (!_isFlying) _rigidbody.useGravity = true;
                else _rigidbody.useGravity = false;
            }
        }

        public bool Paused { get; private set; } = false;

        void Start()
        {
            if (Camera.main == null) throw new InvalidOperationException("Camera.main missing");
            _transform = GetComponent<Transform>();
            _camTransform = Camera.main.GetComponent<Transform>();
            _capsuleCollider = GetComponent<CapsuleCollider>() ?? throw new InvalidOperationException("Player:CapsuleCollider missing");
            _rigidbody = GetComponent<Rigidbody>() ?? throw new InvalidOperationException("Player:Rigidbody missing");
            // Setup the camera
            var camera = Camera.main;
            camera.renderingPath = RenderPath;
            camera.farClipPlane = CameraFarClip;
            _crosshair = FindObjectOfType<UICrosshair>();
        }

        void Update()
        {
            if (Paused) return;
            Rotate();
            if (Input.GetKeyDown(KeyCode.Tab)) IsFlying = !IsFlying;
            if (_isGrounded && !IsFlying && Input.GetButtonDown("Jump"))
            {
                var newVelocity = _rigidbody.velocity;
                newVelocity.y = 5;
                _rigidbody.velocity = newVelocity;
            }
            if (HasLight && Input.GetButtonDown("Light")) lantern.enabled = !lantern.enabled;
            //// clamp
            //var lastPostion = _transform.position;
            //if (lastPostion.y < 0) { lastPostion.y = 0; _transform.position = lastPostion; }
        }

        void FixedUpdate()
        {
            _isGrounded = CalculateIsGrounded();
            if (_isGrounded || IsFlying) SetVelocity();
            else if (!_isGrounded || !IsFlying) ApplyAirborneForce();
        }

        void Rotate()
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                if (Input.GetMouseButtonDown(0)) Cursor.lockState = CursorLockMode.Locked;
                else return;
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.BackQuote))
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
            var eulerAngles = new Vector3(_camTransform.localEulerAngles.x, _transform.localEulerAngles.y, 0);
            // Make eulerAngles.x range from -180 to 180 so we can clamp it between a negative and positive angle.
            if (eulerAngles.x > 180) eulerAngles.x -= 360;
            var deltaMouse = mouseSensitivity * new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            eulerAngles.x = Mathf.Clamp(eulerAngles.x - deltaMouse.y, minVerticalAngle, maxVerticalAngle);
            eulerAngles.y = Mathf.Repeat(eulerAngles.y + deltaMouse.x, 360);
            _camTransform.localEulerAngles = new Vector3(eulerAngles.x, 0, 0);
            _transform.localEulerAngles = new Vector3(0, eulerAngles.y, 0);
        }

        void SetVelocity()
        {
            Vector3 velocity;
            if (!IsFlying)
            {
                velocity = _transform.TransformVector(CalculateLocalVelocity());
                velocity.y = _rigidbody.velocity.y;
            }
            else velocity = _camTransform.TransformVector(CalculateLocalVelocity());
            _rigidbody.velocity = velocity;
        }

        void ApplyAirborneForce()
        {
            var forceDirection = _transform.TransformVector(CalculateLocalMovementDirection());
            forceDirection.y = 0;
            forceDirection.Normalize();
            var force = airborneForceMultiplier * _rigidbody.mass * forceDirection;
            _rigidbody.AddForce(force);
        }

        Vector3 CalculateLocalMovementDirection()
        {
            // Calculate the local movement direction.
            var direction = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
            // A small hack for French Keyboard...
            //if (Application.systemLanguage == SystemLanguage.French)
            //{
            //    // Cancel Qwerty
            //    if (Input.GetKeyDown(KeyCode.W)) direction.z = 0;
            //    else if (Input.GetKeyDown(KeyCode.A)) direction.x = 0;
            //    // Use Azerty
            //    if (Input.GetKey(KeyCode.Z)) direction.z = 1;
            //    else if (Input.GetKey(KeyCode.S)) direction.z = -1;
            //    if (Input.GetKey(KeyCode.Q)) direction.x = -1;
            //    else if (Input.GetKey(KeyCode.D)) direction.x = 1;
            //}
            return direction.normalized;
        }

        float CalculateSpeed()
        {
            var speed = normalSpeed;
            if (HasRunAndSlow && Input.GetButton("Run")) speed = fastSpeed;
            else if (HasRunAndSlow && Input.GetButton("Slow")) speed = slowSpeed;
            if (IsFlying) speed *= flightSpeedMultiplier;
            return speed;
        }

        Vector3 CalculateLocalVelocity() => CalculateSpeed() * CalculateLocalMovementDirection();

        bool CalculateIsGrounded()
        {
            var playerCenter = _transform.position + _capsuleCollider.center;
            var castedSphereRadius = 0.8f * _capsuleCollider.radius;
            var sphereCastDistance = _capsuleCollider.height / 2;
            return Physics.SphereCast(new Ray(playerCenter, -_transform.up), castedSphereRadius, sphereCastDistance);
        }

        public void Pause(bool pause)
        {
            Paused = pause;
            _crosshair.SetActive(!Paused);
            Time.timeScale = pause ? 0.0f : 1.0f;
            Cursor.lockState = pause ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = pause;
            // Used by the VR Component to enable/disable some features.
            SendMessage("OnPlayerPause", pause, SendMessageOptions.DontRequireReceiver);
        }
    }
}