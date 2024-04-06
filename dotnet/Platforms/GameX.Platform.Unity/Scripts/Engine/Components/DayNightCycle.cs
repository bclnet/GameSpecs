using UnityEngine;

namespace GameX.Engine.Components
{
    public class DayNightCycle : MonoBehaviour
    {
        Transform _transform;
        Quaternion _originalOrientation;

        [SerializeField] float _rotationTime = 0.5f;

        void Start()
        {
            _transform = transform;
            _originalOrientation = _transform.rotation;
            RenderSettings.sun = GetComponent<Light>();
        }

        void Update()
            => _transform.Rotate(_rotationTime * Time.deltaTime, 0.0f, 0.0f);
    }
}
