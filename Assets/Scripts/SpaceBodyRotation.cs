using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Rotates the body (this transform) with the mouse to create a "floating in space" feel.
/// Put this on the player/body root; make the camera a child so it rotates with you.
/// For full space feel: turn off pitch clamp and optionally enable roll.
/// </summary>
public class SpaceBodyRotation : MonoBehaviour
{
    [Header("Look")]
    [Tooltip("Degrees of rotation per pixel of mouse movement.")]
    [SerializeField] float mouseSensitivity = 0.12f;

    [Tooltip("Limit up/down pitch (e.g. 85 = earth-like). Turn OFF for true space (360° flip).")]
    [SerializeField] bool clampPitch = false;

    [SerializeField] float maxPitch = 85f;

    [Tooltip("Flip vertical look.")]
    [SerializeField] bool invertY = false;

    [Header("Roll (space feel)")]
    [Tooltip("Allow roll so the body can tilt (e.g. barrel roll). Q/E or secondary axis.")]
    [SerializeField] bool allowRoll = false;

    [Tooltip("Roll speed when using Q/E keys (degrees per second).")]
    [SerializeField] float rollSpeed = 90f;

    [Header("Pivot (rotate from center of body)")]
    [Tooltip("Point to rotate around, in local space. Default (0,1,0) = 1 unit up from transform (typical body center if pivot is at feet). Tweak Y to match your character height.")]
    [SerializeField] Vector3 pivotOffset = new Vector3(0f, 1f, 0f);

    [Tooltip("If set, finds center of first Renderer/Collider in children and uses it as pivot. Overrides Pivot Offset.")]
    [SerializeField] bool autoDetectCenter = true;

    [Header("Cursor")]
    [SerializeField] bool lockCursorOnStart = true;
    [SerializeField] Key unlockKey = Key.Escape;

    float pitch;
    float yaw;
    float roll;
    bool initialized;

    void OnEnable()
    {
        if (lockCursorOnStart)
            LockCursor();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && lockCursorOnStart)
            LockCursor();
    }

    void Start()
    {
        if (autoDetectCenter)
            TryDetectPivotCenter();

        Vector3 e = transform.eulerAngles;
        pitch = NormalizeAngle(e.x);
        yaw = NormalizeAngle(e.y);
        roll = NormalizeAngle(e.z);
        initialized = true;
        if (lockCursorOnStart)
            LockCursor();
    }

    void TryDetectPivotCenter()
    {
        var renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            pivotOffset = transform.InverseTransformPoint(renderer.bounds.center);
            return;
        }
        var col = GetComponentInChildren<Collider>();
        if (col != null)
        {
            pivotOffset = transform.InverseTransformPoint(col.bounds.center);
        }
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

        Cursor.visible = false;

        var mouse = Mouse.current;
        if (mouse == null) return;

        if (!initialized)
        {
            Vector3 e = transform.eulerAngles;
            pitch = NormalizeAngle(e.x);
            yaw = NormalizeAngle(e.y);
            roll = NormalizeAngle(e.z);
            initialized = true;
        }

        Vector2 delta = mouse.delta.ReadValue();
        float mx = delta.x * mouseSensitivity;
        float my = delta.y * mouseSensitivity;
        if (invertY) my = -my;

        yaw += mx;
        pitch -= my;

        if (clampPitch)
            pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);

        if (allowRoll)
        {
            float rollInput = 0f;
            if (keyboard != null)
            {
                if (keyboard[Key.Q].isPressed) rollInput -= 1f;
                if (keyboard[Key.E].isPressed) rollInput += 1f;
            }
            roll += rollInput * rollSpeed * Time.deltaTime;
        }

        // Apply as pitch (X), yaw (Y), roll (Z) — order matches "look then tilt"
        Quaternion newRot = Quaternion.Euler(pitch, yaw, roll);

        if (pivotOffset.sqrMagnitude > 0.0001f)
        {
            // Keep pivot point fixed in world space while rotating
            Vector3 worldPivot = transform.position + transform.rotation * pivotOffset;
            transform.rotation = newRot;
            transform.position = worldPivot - transform.rotation * pivotOffset;
        }
        else
        {
            transform.rotation = newRot;
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
