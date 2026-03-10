using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// FPS mouse look on Main Camera. Cursor locks and hides as soon as this view is active.
/// Rotation uses raw mouse delta each frame — no smoothing — so camera tracks mouse 1:1 via sensitivity.
/// </summary>
public class MouseLook : MonoBehaviour
{
    [Header("Look")]
    [Tooltip("Rotation applied per pixel of mouse movement (degrees per pixel). Higher = snappier, more 1:1 with mouse. Try 0.08–0.25.")]
    [SerializeField] float mouseSensitivity = 0.12f;

    [Tooltip("Max up/down angle in degrees.")]
    [SerializeField] float maxPitch = 85f;

    [Tooltip("Flip vertical look.")]
    [SerializeField] bool invertY = false;

    [Header("Cursor")]
    [Tooltip("Lock and hide cursor as soon as this view is active (OnEnable) and when window regains focus.")]
    [SerializeField] bool lockCursorOnStart = true;

    [Tooltip("Press this key to unlock cursor (e.g. for menus).")]
    [SerializeField] Key unlockKey = Key.Escape;

    [Header("Setup (optional)")]
    [Tooltip("If true, only pitch (up/down) is applied here; parent should handle yaw.")]
    [SerializeField] bool pitchOnly = false;

    [Header("First-person (hide player body/head)")]
    [SerializeField] LayerMask hideFromView = 0;

    float pitch;
    float yaw;
    Camera cam;
    bool initialized;

    void OnEnable()
    {
        if (!lockCursorOnStart) return;
        LockCursor();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && lockCursorOnStart)
            LockCursor();
    }

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam != null && hideFromView != 0)
            cam.cullingMask &= ~hideFromView;

        Vector3 e = transform.eulerAngles;
        pitch = NormalizeAngle(e.x);
        yaw = NormalizeAngle(e.y);
        initialized = true;

        if (lockCursorOnStart)
            LockCursor();
    }

    void LateUpdate()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard[unlockKey].wasPressedThisFrame)
        {
            if (Cursor.lockState == CursorLockMode.Locked)
                UnlockCursor();
            else
                LockCursor();
        }

        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        // Keep cursor hidden (some systems show it briefly otherwise)
        Cursor.visible = false;

        var mouse = Mouse.current;
        if (mouse == null) return;

        // Raw pixel delta this frame — no smoothing, direct application
        Vector2 delta = mouse.delta.ReadValue();
        float mx = delta.x * mouseSensitivity;
        float my = delta.y * mouseSensitivity;
        if (invertY)
            my = -my;

        if (!initialized)
        {
            Vector3 e = transform.eulerAngles;
            pitch = NormalizeAngle(e.x);
            yaw = NormalizeAngle(e.y);
            initialized = true;
        }

        if (pitchOnly)
        {
            pitch -= my;
            pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);
            transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
        else
        {
            yaw += mx;
            pitch -= my;
            pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    static float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;
        return angle;
    }
}
