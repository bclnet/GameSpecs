//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.XR;
//#if UNITY_WSA
//using UnityEngine.XR.WSA.Input;
//#endif

//namespace GameEstate.Core.XR
//{
//    public enum XRButton
//    {
//        Menu,
//        Button1,
//        Button2,
//        Button3,
//        Button4,
//        Thumbstick,
//        ThumbstickTouch,
//        SecondaryTouchpad,
//        SecondaryTouchpadTouch,
//        Trigger,
//        Grip,
//        ThumbstickUp,
//        ThumbstickDown,
//        ThumbstickLeft,
//        ThumbstickRight
//    }

//    public enum XRAxis
//    {
//        Trigger,
//        Grip,
//        ThumbstickX,
//        ThumbstickY,
//        SecondaryTouchpadX,
//        SecondaryTouchpadY
//    }

//    public enum XRAxis2D
//    {
//        Thumbstick,
//        SecondaryTouchpad
//    }

//    public enum XRVendor
//    {
//        None = 0, Oculus, OpenVR, WindowsMR
//    }

//    public sealed class XRInput : MonoBehaviour
//    {
//        static XRInput _instance = null;
//        Vector2 _tmpVector = Vector2.zero;
//        List<XRNodeState> _xrNodeStates = new List<XRNodeState>();
//        XRButton[] _buttons = null;
//        bool _running = true;
//        bool[] _axisStates = null;

//        [SerializeField] float _deadZone = 0.1f;

//        public static XRInput Instance
//        {
//            get
//            {
//                if (_instance == null)
//                {
//                    var gameObject = new GameObject("VRInput");
//                    _instance = gameObject.AddComponent<XRInput>();
//                }
//                return _instance;
//            }
//        }

//        public XRVendor Vendor { get; private set; } = XRVendor.None;

//        public bool IsConnected
//        {
//            get
//            {
//                _xrNodeStates.Clear();
//                InputTracking.GetNodeStates(_xrNodeStates);
//                var left = false;
//                var right = false;
//                foreach (var state in _xrNodeStates)
//                {
//                    if (state.nodeType == XRNode.LeftHand) left = state.tracked;
//                    else if (state.nodeType == XRNode.RightHand) right = state.tracked;
//                }
//                return left && right;
//            }
//        }

//        public float DeadZone
//        {
//            get => _deadZone;
//            set
//            {
//                _deadZone = value;
//                if (_deadZone < 0) _deadZone = 0.0f;
//                else if (_deadZone >= 1.0f) _deadZone = 0.9f;
//            }
//        }

//        delegate float GetAxisFunction(string axis);

//        public void Awake()
//        {
//            if (_instance != null && _instance != this)
//            {
//                Destroy(this);
//                return;
//            }
//            var vendor = XRSettings.loadedDeviceName;
//            if (vendor == "Oculus") Vendor = XRVendor.Oculus;
//            else if (vendor == "Openvr") Vendor = XRVendor.OpenVR;
//            else if (vendor == "Windowsmr") Vendor = XRVendor.WindowsMR;
//            _buttons = new XRButton[]
//            {
//                XRButton.Grip, XRButton.Trigger,
//                XRButton.ThumbstickUp, XRButton.ThumbstickDown,
//                XRButton.ThumbstickLeft, XRButton.ThumbstickRight
//            };
//            _axisStates = new bool[_buttons.Length * 2];
//            StartCoroutine(UpdateAxisToButton());
//        }

//        void OnDestroy() => _running = false;

//        IEnumerator UpdateAxisToButton()
//        {
//            var endOfFrame = new WaitForEndOfFrame();
//            var index = 0;
//            while (_running)
//            {
//                index = 0;
//                for (var i = 0; i < _buttons.Length; i++)
//                {
//                    _axisStates[index] = GetButton(_buttons[i], true);
//                    _axisStates[index + 1] = GetButton(_buttons[i], false);
//                    index += 2;
//                }
//                yield return endOfFrame;
//            }
//        }

//        /// <summary>
//        /// Gets the position of a specific node.
//        /// </summary>
//        /// <param name="node"></param>
//        /// <returns></returns>
//        public Vector3 GetLocalPosition(XRNode node) => InputTracking.GetLocalPosition(node);

//        /// <summary>
//        /// Gets the rotation of a specific node.
//        /// </summary>
//        /// <param name="node"></param>
//        /// <returns></returns>
//        public Quaternion GetLocalRotation(XRNode node) => InputTracking.GetLocalRotation(node);

//        // METHODS TO GET BUTTON STATES

//        /// <summary>
//        /// Indicates whether a button is pressed.
//        /// </summary>
//        /// <param name="button">The button.</param>
//        /// <param name="left">Left or Right controller.</param>
//        /// <returns>Returns true if pressed otherwise it returns false.</returns>
//        public bool GetButton(XRButton button, bool left)
//        {
//            if (button == XRButton.Menu)
//            {
//                if (Vendor == XRVendor.OpenVR) return Input.GetButton(left ? "Button 2" : "Button 0");
//                else if (Vendor == XRVendor.WindowsMR) return Input.GetButton(left ? "Button 6" : "Button 7");
//                return Input.GetButton("Button 7");
//            }
//            else if (button == XRButton.Button1) return Input.GetButton("Button 0");
//            else if (button == XRButton.Button2) return Input.GetButton("Button 1");
//            else if (button == XRButton.Button3) return Input.GetButton("Button 2");
//            else if (button == XRButton.Button4) return Input.GetButton("Button 3");
//            else if (button == XRButton.Thumbstick) return Input.GetButton(left ? "Button 8" : "Button 9");
//            else if (button == XRButton.ThumbstickTouch)
//            {
//                if (Vendor == XRVendor.WindowsMR) return Input.GetButton(left ? "Button 18" : "19");
//                else return Input.GetButton(left ? "Button 16" : "17");
//            }
//            else if (button == XRButton.SecondaryTouchpad) return Input.GetButton(left ? "Button 16" : "17");
//            else if (button == XRButton.SecondaryTouchpad) return Input.GetButton(left ? "Button 18" : "19");
//            else if (button == XRButton.Trigger) return GetRawAxis(XRAxis.Trigger, left) > _deadZone;
//            else if (button == XRButton.Grip) return GetRawAxis(XRAxis.Grip, left) > _deadZone;
//            else if (button == XRButton.ThumbstickUp) return GetRawAxis(XRAxis.ThumbstickY, left) > _deadZone;
//            else if (button == XRButton.ThumbstickDown) return GetRawAxis(XRAxis.ThumbstickY, left) < _deadZone * -1.0f;
//            else if (button == XRButton.ThumbstickLeft) return GetRawAxis(XRAxis.ThumbstickX, left) < _deadZone * -1.0f;
//            else if (button == XRButton.ThumbstickRight) return GetRawAxis(XRAxis.ThumbstickX, left) > _deadZone;
//            return false;
//        }

//        /// <summary>
//        /// Indicates whether a button was pressed.
//        /// </summary>
//        /// <param name="button">The button.</param>
//        /// <param name="left">Left or Right controller.</param>
//        /// <returns>Returns true if pressed otherwise it returns false.</returns>
//        public bool GetButtonDown(XRButton button, bool left)
//        {
//            if (button == XRButton.Menu)
//            {
//                if (Vendor == XRVendor.OpenVR) return Input.GetButtonDown(left ? "Button 2" : "Button 0");
//                else if (Vendor == XRVendor.WindowsMR) return Input.GetButtonDown(left ? "Button 6" : "Button 7");
//                return Input.GetButtonDown("Button 7");
//            }
//            else if (button == XRButton.Button1) return Input.GetButtonDown("Button 0");
//            else if (button == XRButton.Button2) return Input.GetButtonDown("Button 1");
//            else if (button == XRButton.Button3) return Input.GetButtonDown("Button 2");
//            else if (button == XRButton.Button4) return Input.GetButtonDown("Button 3");
//            else if (button == XRButton.Thumbstick) return Input.GetButtonDown(left ? "Button 8" : "Button 9");
//            else if (button == XRButton.ThumbstickTouch)
//            {
//                if (Vendor == XRVendor.WindowsMR) return Input.GetButtonDown(left ? "Button 18" : "19");
//                else return Input.GetButtonDown(left ? "Button 16" : "17");
//            }
//            // Simulate other buttons using previous states.
//            var index = 0;
//            for (var i = 0; i < _buttons.Length; i++)
//            {
//                if (_buttons[i] != button)
//                {
//                    index += 2;
//                    continue;
//                }
//                var prev = _axisStates[left ? index : index + 1];
//                var now = GetButton(_buttons[i], left);
//                return now && !prev;
//            }
//            return false;
//        }

//        /// <summary>
//        /// Indicates whether a button was released.
//        /// </summary>
//        /// <param name="button">The button.</param>
//        /// <param name="left">Left or Right controller.</param>
//        /// <returns>Returns true if pressed otherwise it returns false.</returns>
//        public bool GetButtonUp(XRButton button, bool left)
//        {
//            if (button == XRButton.Menu)
//            {
//                if (Vendor == XRVendor.OpenVR) return Input.GetButtonUp(left ? "Button 2" : "Button 0");
//                else if (Vendor == XRVendor.WindowsMR) return Input.GetButtonUp(left ? "Button 6" : "Button 7");
//                return Input.GetButtonUp("Button 7");
//            }
//            else if (button == XRButton.Button1) return Input.GetButtonUp("Button 0");
//            else if (button == XRButton.Button2) return Input.GetButtonUp("Button 1");
//            else if (button == XRButton.Button3) return Input.GetButtonUp("Button 2");
//            else if (button == XRButton.Button4) return Input.GetButtonUp("Button 3");
//            else if (button == XRButton.Thumbstick) return Input.GetButtonUp(left ? "Button 8" : "Button 9");
//            else if (button == XRButton.ThumbstickTouch)
//            {
//                if (Vendor == XRVendor.WindowsMR) return Input.GetButtonUp(left ? "Button 18" : "19");
//                else return Input.GetButtonUp(left ? "Button 16" : "17");
//            }
//            // Simulate other buttons using previous states.
//            var index = 0;
//            for (var i = 0; i < _buttons.Length; i++)
//            {
//                if (_buttons[i] != button)
//                {
//                    index += 2;
//                    continue;
//                }
//                var prev = _axisStates[left ? index : index + 1];
//                var now = GetButton(_buttons[i], left);
//                return !now && prev;
//            }
//            return false;
//        }

//        /// <summary>
//        /// Indicates if the button is pressed on the left or right controller.
//        /// </summary>
//        /// <param name="button">The button.</param>
//        /// <returns>Returns true if the button is pressed on the left or right controller.</returns>
//        public bool GetAnyButton(XRButton button) => GetButton(button, false) || GetButton(button, true);

//        /// <summary>
//        /// Indicates if the button is pressed on both controllers.
//        /// </summary>
//        /// <param name="button">The button.</param>
//        /// <returns>Returns true if the button is pressed on both left and right controllers.</returns>
//        public bool GetBothButtons(XRButton button) => GetButton(button, false) && GetButton(button, true);

//        /// <summary>
//        /// Indicates if the button was pressed on the left or right controller.
//        /// </summary>
//        /// <param name="button">The button.</param>
//        /// <returns>Returns true if the button was pressed on the left or right controller.</returns>
//        public bool GetAnyButtonDown(XRButton button) => GetButtonDown(button, false) || GetButtonDown(button, true);

//        /// <summary>
//        /// Indicates if the button was pressed on both controllers.
//        /// </summary>
//        /// <param name="button">The button.</param>
//        /// <returns>Returns true if the button was pressed on both left and right controllers.</returns>
//        public bool GetBothButtonsDown(XRButton button) => GetButtonDown(button, false) && GetButtonDown(button, true);

//        /// <summary>
//        /// Indicates if the button was released on the left or right controllers.
//        /// </summary>
//        /// <param name="button">The button.</param>
//        /// <returns>Returns true if the button was released on the left or right controller.</returns>
//        public bool GetAnyButtonUp(XRButton button) => GetButtonUp(button, false) || GetButtonUp(button, true);

//        /// <summary>
//        /// Indicates if the button was just released on both controllers.
//        /// </summary>
//        /// <param name="button">The button.</param>
//        /// <returns>Returns true if the button was just released on both controllers.</returns>
//        public bool GetBothButtonsUp(XRButton button) => GetButtonUp(button, false) && GetButtonUp(button, true);

//        // METHODS TO GET AXIS STATE

//        float GetAxis(GetAxisFunction axisFunction, XRAxis axis, bool left)
//        {
//            if (axis == XRAxis.Trigger) return axisFunction(left ? "Axis 9" : "Axis 10");
//            else if (axis == XRAxis.Grip) return axisFunction(left ? "Axis 11" : "Axis 12");
//            else if (axis == XRAxis.ThumbstickX) return axisFunction(left ? "Axis 1" : "Axis 4");
//            else if (axis == XRAxis.ThumbstickY) return axisFunction(left ? "Axis 2" : "Axis 5");
//            else if (axis == XRAxis.SecondaryTouchpadX) return axisFunction(left ? "Axis 17" : "Axis 20");
//            else if (axis == XRAxis.SecondaryTouchpadY) return axisFunction(left ? "Axis 18" : "Axis 21");
//            return 0.0f;
//        }

//        private Vector2 GetAxis2D(GetAxisFunction axisFunction, XRAxis2D axis, bool left)
//        {
//            _tmpVector.x = 0;
//            _tmpVector.y = 0;
//            if (axis == XRAxis2D.Thumbstick)
//            {
//                _tmpVector.x = axisFunction(left ? "Axis 1" : "Axis 4");
//                _tmpVector.y = axisFunction(left ? "Axis 2" : "Axis 5");
//            }
//            else if (axis == XRAxis2D.SecondaryTouchpad)
//            {
//                _tmpVector.x = axisFunction(left ? "Axis 17" : "Axis 20");
//                _tmpVector.y = axisFunction(left ? "Axis 18" : "Axis 21");
//            }
//            return _tmpVector;
//        }

//        /// <summary>
//        /// Gets an axis value.
//        /// </summary>
//        /// <param name="axis">The axis.</param>
//        /// <param name="left">Left or Right controller.</param>
//        /// <returns>Returns the axis value.</returns>
//        public float GetAxis(XRAxis axis, bool left) => GetAxis(Input.GetAxis, axis, left);

//        /// <summary>
//        /// Gets a raw axis value.
//        /// </summary>
//        /// <param name="axis">The axis.</param>
//        /// <param name="left">Left or Right controller.</param>
//        /// <returns>Returns the axis value.</returns>
//        public float GetRawAxis(XRAxis axis, bool left) => GetAxis(Input.GetAxisRaw, axis, left);

//        /// <summary>
//        /// Gets two axis values.
//        /// </summary>
//        /// <param name="axis"></param>
//        /// <param name="left">Left or Right controller.</param>
//        /// <returns>Returns two axis values.</returns>
//        public Vector2 GetAxis2D(XRAxis2D axis, bool left) => GetAxis2D(Input.GetAxis, axis, left);

//        /// <summary>
//        /// Gets two raw axis values.
//        /// </summary>
//        /// <param name="axis"></param>
//        /// <param name="left">Left or Right controller.</param>
//        /// <returns>Returns two axis values.</returns>
//        public Vector2 GetRawAxis2D(XRAxis2D axis, bool left) => GetAxis2D(Input.GetAxis, axis, left);
//    }
//}