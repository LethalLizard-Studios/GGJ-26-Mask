using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody))]
public class PropPickup : MonoBehaviour
{
    public Transform sitPoint = null;

    [Header("Pickup Settings")]
    public float moveDuration = 0.35f; // total pickup travel time
    public float throwForce = 10f;

    [Header("Hold Smoothing")]
    public float followSmooth = 10f; // higher = tighter follow
    public float rotateSmooth = 10f;

    private Rigidbody rb;
    private Collider col;
    private bool isHeld = false;
    private Transform pickupPoint;
    private Quaternion heldRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void Pickup()
    {
        if (isHeld) return;

        PropPickupPoint point = FindObjectOfType<PropPickupPoint>();
        if (point == null) return;
        pickupPoint = point.transform;

        rb.isKinematic = true;
        col.enabled = false; // turn off physics collisions while held

        // Move fast then slow down near pickup point
        transform.DOMove(pickupPoint.position, moveDuration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                isHeld = true;
                heldRotation = transform.rotation;
            });
    }

    void Update()
    {
        if (isHeld && pickupPoint)
        {
            // Smoothly move and rotate toward pickup point
            transform.position = Vector3.Lerp(
                transform.position,
                pickupPoint.position,
                Time.deltaTime * followSmooth
            );

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                heldRotation,
                Time.deltaTime * rotateSmooth
            );

            if (Input.GetMouseButtonDown(0))
                Throw();
        }
    }

    void Throw()
    {
        isHeld = false;
        rb.isKinematic = false;

        // Apply throw velocity
        rb.linearVelocity = pickupPoint.forward * throwForce;

        // Small delay before re-enabling collider (to avoid clipping with player)
        Invoke(nameof(EnableCollider), 0.1f);
    }

    void EnableCollider()
    {
        col.enabled = true;
    }
}
