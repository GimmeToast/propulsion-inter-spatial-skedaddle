using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Applies force to move up/down/left/right (WASD) and back (Space). For use with Rigidbody (e.g. space flight).
/// Bind keys in the Inspector; defaults are W A S D and Space.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class SpaceMovement : MonoBehaviour
{
    [Header("WASD — Up / Down / Left / Right")]
    [SerializeField] Key moveUp = Key.W;
    [SerializeField] Key moveDown = Key.S;
    [SerializeField] Key moveLeft = Key.A;
    [SerializeField] Key moveRight = Key.D;

    [Header("Back")]
    [SerializeField] Key moveBack = Key.Space;

    [Tooltip("When you release Space, a small constant force keeps pushing in that direction (drift).")]
    [SerializeField] float driftForceMagnitude = 8f;

    [Header("Force")]
    [Tooltip("Force applied per axis when holding a key (per second in FixedUpdate).")]
    [SerializeField] float forceMagnitude = 50f;

    [Tooltip("Optional: higher drag so velocity doesn't grow forever (e.g. 1–5).")]
    [SerializeField] float rigidbodyDrag = 2f;

    Rigidbody rb;
    Vector3 driftDirection;
    bool hasDriftDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null && rigidbodyDrag > 0f)
            rb.linearDamping = rigidbodyDrag;
    }

    void FixedUpdate()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null || rb == null) return;

        Vector3 force = Vector3.zero;

        if (keyboard[moveUp].isPressed)    force += Vector3.up;
        if (keyboard[moveDown].isPressed)  force += Vector3.down;
        if (keyboard[moveLeft].isPressed)  force += -transform.right;
        if (keyboard[moveRight].isPressed) force += transform.right;

        if (keyboard[moveBack].isPressed)
        {
            Vector3 back = -transform.forward;
            force += back;
            driftDirection = back.sqrMagnitude > 0.01f ? back.normalized : driftDirection;
            hasDriftDirection = true;
        }
        else if (hasDriftDirection && driftForceMagnitude > 0f)
        {
            // Apply drift separately so it uses its own (smaller) magnitude
            rb.AddForce(driftDirection * (driftForceMagnitude * Time.fixedDeltaTime));
        }

        if (force.sqrMagnitude > 0.01f)
        {
            force = force.normalized * (forceMagnitude * Time.fixedDeltaTime);
            rb.AddForce(force);
        }
    }
}
