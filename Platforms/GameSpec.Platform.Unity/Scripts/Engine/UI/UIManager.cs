//using UnityEngine;

//namespace GameEstate.Engine.UI
//{
//    [RequireComponent(typeof(Canvas))]
//    public class UIManager : MonoBehaviour
//    {
//        Canvas _canvas;

//        [Header("HUD Elements"), SerializeField] UICrosshair _crosshair = null;
//        public UICrosshair Crosshair => _crosshair;

//        [SerializeField] UIInteractiveText _interactiveText = null;
//        public UIInteractiveText InteractiveText => _interactiveText;

//        public Transform HUD { get; private set; }
//        public Transform UI { get; private set; }

//        public bool Visible
//        {
//            get => _canvas.enabled;
//            set => _canvas.enabled = value;
//        }

//        public bool Active
//        {
//            get => HUD.gameObject.activeSelf;
//            set
//            {
//                HUD.gameObject.SetActive(value);
//                UI.gameObject.SetActive(value);
//            }
//        }

//        void Awake()
//        {
//            _canvas = GetComponent<Canvas>();
//            HUD = transform.Find("HUD");
//            UI = transform.Find("UI");
//        }
//    }
//}
