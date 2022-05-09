//using Game.Core.XR;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.XR;

//namespace GameEstate.Core
//{
//    public static class InputManager
//    {
//        struct XRButtonMapping
//        {
//            public XRButton Button { get; set; }
//            public bool LeftHand { get; set; }

//            public XRButtonMapping(XRButton button, bool left)
//            {
//                Button = button;
//                LeftHand = left;
//            }
//        }

//        static Dictionary<string, XRButtonMapping> _XRMapping = new Dictionary<string, XRButtonMapping>()
//        {
//            { "Jump", new XRButtonMapping(XRButton.Thumbstick, true) },
//            { "Light", new XRButtonMapping(XRButton.Thumbstick, false) },
//            { "Run", new XRButtonMapping(XRButton.Grip, true) },
//            { "Slow", new XRButtonMapping(XRButton.Grip, false) },
//            { "Attack", new XRButtonMapping(XRButton.Trigger, false) },
//            { "Recenter", new XRButtonMapping(XRButton.Menu, false) },
//            { "Use", new XRButtonMapping(XRButton.Trigger, true) },
//            { "Menu", new XRButtonMapping(XRButton.Menu, true) }
//        };

//        public static float GetAxis(string axis)
//        {
//            var r = Input.GetAxis(axis);
//            if (!XRSettings.enabled)
//                return r;
//            var input = XRInput.Instance;
//            if (axis == "Horizontal") r += input.GetAxis(XRAxis.ThumbstickX, true);
//            else if (axis == "Vertical") r += input.GetAxis(XRAxis.ThumbstickY, true);
//            else if (axis == "Mouse X") r += input.GetAxis(XRAxis.ThumbstickX, false);
//            else if (axis == "Mouse Y") r += input.GetAxis(XRAxis.ThumbstickY, false);
//            // Deadzone
//            if (Mathf.Abs(r) < 0.15f)
//                r = 0.0f;
//            return r;
//        }

//        public static bool GetButton(string button)
//        {
//            var r = Input.GetButtonDown(button);
//            if (!XRSettings.enabled)
//                return r;
//            var input = XRInput.Instance;
//            if (_XRMapping.ContainsKey(button))
//            {
//                var mapping = _XRMapping[button];
//                r |= input.GetButton(mapping.Button, mapping.LeftHand);
//            }
//            return r;
//        }

//        public static bool GetButtonUp(string button)
//        {
//            var r = Input.GetButtonUp(button);
//            if (!XRSettings.enabled)
//                return r;
//            var input = XRInput.Instance;
//            if (_XRMapping.ContainsKey(button))
//            {
//                var mapping = _XRMapping[button];
//                r |= input.GetButtonUp(mapping.Button, mapping.LeftHand);
//            }
//            return r;
//        }

//        public static bool GetButtonDown(string button)
//        {
//            var r = Input.GetButtonDown(button);
//            if (!XRSettings.enabled)
//                return r;
//            var input = XRInput.Instance;
//            if (_XRMapping.ContainsKey(button))
//            {
//                var mapping = _XRMapping[button];
//                r |= input.GetButtonDown(mapping.Button, mapping.LeftHand);
//            }
//            return r;
//        }
//    }
//}