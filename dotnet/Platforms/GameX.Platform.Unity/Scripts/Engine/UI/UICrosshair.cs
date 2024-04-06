using UnityEngine;
using UnityEngine.UIElements;

namespace GameX.Engine.UI
{
    [RequireComponent(typeof(Image))]
    public class UICrosshair : MonoBehaviour
    {
        Image _crosshair = null;

        //public bool Enabled
        //{
        //    get => _crosshair.enabledSelf;
        //    set => _crosshair.enabledSelf = value;
        //}

        void Awake()
            => _crosshair = GetComponent<Image>();

        void Start()
        {
            //var crosshairTexture = (Texture2D)null; // BaseEngine.instance.Asset.LoadTexture("target", true);
            //_crosshair.image = UIUtils.CreateSprite(crosshairTexture);
        }

        public void SetActive(bool active)
            => gameObject.SetActive(active);
    }
}
