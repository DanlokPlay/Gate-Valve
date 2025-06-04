using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    public float zoomSpeed = 1f;
    public float minZoom = 5f;
    public float maxZoom = 30f;

    private Vector3 _lastPointerPosition;
    private bool _isDragging;
    private bool _isDraggingFromUI;

    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseMovement();
        HandleMouseZoom();
#endif

#if UNITY_ANDROID || UNITY_IOS
        HandleTouchMovement();
        HandleTouchZoom();
#endif
    }

    private void HandleMouseMovement()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            _isDragging = true;
            _isDraggingFromUI = IsPointerOverUI();
            _lastPointerPosition = mouse.position.ReadValue();
        }
        else if (mouse.leftButton.isPressed && _isDragging && !_isDraggingFromUI)
        {
            Vector2 delta = mouse.position.ReadValue() - (Vector2)_lastPointerPosition;
            _lastPointerPosition = mouse.position.ReadValue();

            Vector3 movement = (Vector3)(delta * (Camera.main.orthographicSize * 2f / Screen.height));
            transform.position -= movement;
        }
        else if (mouse.leftButton.wasReleasedThisFrame)
        {
            _isDragging = false;
        }
    }

    private void HandleMouseZoom()
    {
        if (IsPointerOverUI()) return;

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            AdjustZoom(scroll);
        }
    }

    private void HandleTouchMovement()
    {
        var touchscreen = Touchscreen.current;
        if (touchscreen == null || touchscreen.touches.Count < 1) return;

        var touch = touchscreen.touches[0];
        if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
        {
            _isDragging = true;
            _isDraggingFromUI = IsPointerOverUI();
            _lastPointerPosition = touch.position.ReadValue();
        }
        else if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved && _isDragging && !_isDraggingFromUI)
        {
            Vector2 delta = touch.position.ReadValue() - (Vector2)_lastPointerPosition;
            _lastPointerPosition = touch.position.ReadValue();

            Vector3 movement = (Vector3)(delta * (Camera.main.orthographicSize * 2f / Screen.height));
            transform.position -= movement;
        }
    }

    private void HandleTouchZoom()
    {
        var touchscreen = Touchscreen.current;
        if (touchscreen == null || touchscreen.touches.Count < 2) return;

        var touch0 = touchscreen.touches[0];
        var touch1 = touchscreen.touches[1];

        Vector2 prev0 = touch0.position.ReadValue() - touch0.delta.ReadValue();
        Vector2 prev1 = touch1.position.ReadValue() - touch1.delta.ReadValue();

        float prevDistance = Vector2.Distance(prev0, prev1);
        float currentDistance = Vector2.Distance(touch0.position.ReadValue(), touch1.position.ReadValue());

        float delta = currentDistance - prevDistance;
        AdjustZoom(delta * zoomSpeed);
    }

    private void AdjustZoom(float increment)
    {
        float newZoom = Camera.main.orthographicSize - increment;
        Camera.main.orthographicSize = Mathf.Clamp(newZoom, minZoom, maxZoom);
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        return EventSystem.current.IsPointerOverGameObject(Pointer.current?.deviceId ?? -1);
    }
}