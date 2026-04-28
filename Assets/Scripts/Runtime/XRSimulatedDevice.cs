using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using WebXR;

namespace WebXRTheater
{
    /// <summary>
    /// Simulates an XR device input events with pointer.
    /// </summary>
    public sealed class XRSimulatedDevice : MonoBehaviour
    {
        [SerializeField]
        InputAction pointerPositionAction;

        [SerializeField]
        [Range(0f, 90f)]
        float maxPanAngle = 15f;

        [SerializeField]
        Quaternion initialRotation;

        XRHMD simulatedHmd;

        void OnEnable()
        {
            WebXRManager.OnXRChange += OnXRChange;
            // Send initial state event
            var manager = WebXRManager.Instance;
            OnXRChange(manager.XRState, manager.ViewsCount, manager.ViewsLeftRect, manager.ViewsRightRect);
        }

        void OnDisable()
        {
            WebXRManager.OnXRChange -= OnXRChange;
        }

        void Update()
        {
            if (!Application.isFocused) return;
            if (simulatedHmd == null || pointerPositionAction == null) return;

            var position = pointerPositionAction.ReadValue<Vector2>();
            var screenSize = new float2(Screen.width, Screen.height);
            var normalizedPos = math.remap(float2.zero, screenSize, new float2(-1f), new float2(1f), position);
            normalizedPos = math.clamp(normalizedPos, -1f, 1f);

            var targetRotation = normalizedPos * maxPanAngle;
            var rotation = initialRotation * Quaternion.Euler(-targetRotation.y, targetRotation.x, 0f);

            using (StateEvent.From(simulatedHmd, out var eventPtr))
            {
                simulatedHmd.centerEyeRotation.WriteValueIntoEvent(rotation, eventPtr);
                simulatedHmd.deviceRotation.WriteValueIntoEvent(rotation, eventPtr);
                simulatedHmd.isTracked.WriteValueIntoEvent(1f, eventPtr);
                simulatedHmd.trackingState.WriteValueIntoEvent((int)InputTrackingState.Rotation, eventPtr);
                InputSystem.QueueEvent(eventPtr);
            }

        }

        void StartSimulation()
        {
            simulatedHmd ??= InputSystem.AddDevice<XRHMD>("Simulated HMD");
            pointerPositionAction.Enable();
        }

        void StopSimulation()
        {
            pointerPositionAction.Disable();

            if (simulatedHmd != null)
            {
                // Send shutdown events to the device, then remove it.
                using (StateEvent.From(simulatedHmd, out var eventPtr))
                {
                    simulatedHmd.isTracked.WriteValueIntoEvent(0f, eventPtr);
                    simulatedHmd.trackingState.WriteValueIntoEvent(0, eventPtr);
                    InputSystem.QueueEvent(eventPtr);
                }

                InputSystem.RemoveDevice(simulatedHmd);
                simulatedHmd = null;
            }
        }

        void OnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
        {
            switch (state)
            {
                case WebXRState.VR:
                case WebXRState.AR:
                    StopSimulation();
                    break;
                case WebXRState.NORMAL:
                    StartSimulation();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
}
