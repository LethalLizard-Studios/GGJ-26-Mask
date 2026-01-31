using System;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PropPickup : MonoBehaviour
{
    public event Action<bool> HeldChanged;

    [Header("Optional")]
    [SerializeField] private Transform sitPoint = null;

    [Header("Pickup Settings")]
    [SerializeField] private float moveDuration = 0.35f;
    [SerializeField] private float throwForce = 10f;

    [Header("Hold Smoothing")]
    [SerializeField] private float followSmooth = 10f;
    [SerializeField] private float rotateSmooth = 10f;

    public Transform SitPoint => sitPoint;
    public bool IsHeld => m_isHeld;

    private Rigidbody m_rb;
    private Collider m_col;
    private bool m_isHeld;
    private Transform m_pickupPoint;
    private Quaternion m_heldRotation;
    private Tween m_pickupTween;

    private void Start()
    {
        m_rb = GetComponent<Rigidbody>();
        m_col = GetComponent<Collider>();
    }

    public void Pickup(Transform point)
    {
        if (m_isHeld) return;
        if (point == null) return;

        m_pickupPoint = point;

        m_rb.isKinematic = true;
        m_col.enabled = false;

        m_pickupTween?.Kill();
        m_pickupTween = transform.DOMove(m_pickupPoint.position, moveDuration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() => {
                SetHeld(true);
                m_heldRotation = transform.rotation;
            });
    }

    private void Update()
    {
        if (!m_isHeld || m_pickupPoint == null) return;

        transform.position = Vector3.Lerp(transform.position, m_pickupPoint.position, Time.deltaTime * followSmooth);
        transform.rotation = Quaternion.Slerp(transform.rotation, m_heldRotation, Time.deltaTime * rotateSmooth);

        if (Input.GetMouseButtonDown(0)) Throw();
    }

    private void Throw()
    {
        SetHeld(false);

        m_rb.isKinematic = false;
        m_rb.linearVelocity = m_pickupPoint.forward * throwForce;

        Invoke(nameof(EnableCollider), 0.1f);
    }

    private void EnableCollider()
    {
        m_col.enabled = true;
    }

    private void SetHeld(bool isHeld)
    {
        if (m_isHeld == isHeld) return;
        m_isHeld = isHeld;
        HeldChanged?.Invoke(m_isHeld);
    }
}