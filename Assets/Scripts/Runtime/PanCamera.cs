using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WebXRTheater
{
    /// <summary>
    /// A simple camera pan using pointer position (mouse or touch).
    /// </summary>
    public sealed class PanCamera : MonoBehaviour
    {
        [SerializeField]
        Transform target;

        [SerializeField]
        InputActionReference pointerPositionAction;

        [SerializeField]
        [Range(0f, 90f)]
        float maxPanAngle = 15f;

        [SerializeField]
        [Range(0.01f, 1f)]
        float smoothTime = 0.1f;

        [SerializeField]
        Quaternion initialRotation;
        Vector2 currentRotation;
        Vector2 rotationVelocity;

        void OnEnable()
        {
            if (target == null)
            {
                target = transform;
            }
            pointerPositionAction.action.Enable();
        }

        void OnDisable()
        {
            pointerPositionAction.action.Disable();
        }

        void Update()
        {
            if (!Application.isFocused) return;
            if (pointerPositionAction.action == null) return;

            float2 mousePos = pointerPositionAction.action.ReadValue<Vector2>();

            float2 screenSize = new(Screen.width, Screen.height);
            float2 mouseNormalized = math.remap(float2.zero, screenSize, new float2(-1f), new float2(1f), mousePos);
            mouseNormalized = math.clamp(mouseNormalized, -1f, 1f);

            // Smoothing
            var targetRotation = mouseNormalized * maxPanAngle;
            currentRotation = Vector2.SmoothDamp(currentRotation, targetRotation, ref rotationVelocity, smoothTime);

            target.localRotation = initialRotation *
                Quaternion.Euler(-currentRotation.y, currentRotation.x, 0f);
        }
    }
}
