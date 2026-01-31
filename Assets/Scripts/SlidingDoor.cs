using UnityEngine;
using DG.Tweening;

public class SlidingDoor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform m_LeftDoor;
    [SerializeField] private Transform m_RightDoor;

    [Header("X Distances (positive)")]
    [SerializeField] private float m_ClosedX = 0f;
    [SerializeField] private float m_OpenedX = 1f;

    [Header("Timing")]
    [SerializeField] private float m_OpenDuration = 0.35f;
    [SerializeField] private float m_CloseDuration = 0.25f;

    [Header("Close Bounce")]
    [SerializeField] private float m_CloseBounceOpenAmount = 0.12f;
    [SerializeField] private float m_CloseBounceDuration = 0.08f;

    private Sequence m_Sequence;

    private void Awake()
    {
        SetDoorsX(m_ClosedX);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Open();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Close();
    }

    private void Open()
    {
        if (m_LeftDoor == null || m_RightDoor == null) return;

        m_Sequence.Kill(false);
        m_Sequence = DOTween.Sequence();

        m_Sequence.Join(m_LeftDoor.DOLocalMoveX(+m_OpenedX, m_OpenDuration).SetEase(Ease.OutCubic));
        m_Sequence.Join(m_RightDoor.DOLocalMoveX(-m_OpenedX, m_OpenDuration).SetEase(Ease.OutCubic));
    }

    private void Close()
    {
        if (m_LeftDoor == null || m_RightDoor == null) return;

        m_Sequence.Kill(false);
        m_Sequence = DOTween.Sequence();

        m_Sequence.Join(m_LeftDoor.DOLocalMoveX(+m_ClosedX, m_CloseDuration).SetEase(Ease.InCubic));
        m_Sequence.Join(m_RightDoor.DOLocalMoveX(-m_ClosedX, m_CloseDuration).SetEase(Ease.InCubic));

        float leftBounceX = m_ClosedX + m_CloseBounceOpenAmount;
        float rightBounceX = -(m_ClosedX + m_CloseBounceOpenAmount);

        m_Sequence.Append(m_LeftDoor.DOLocalMoveX(leftBounceX, m_CloseBounceDuration).SetEase(Ease.OutQuad));
        m_Sequence.Join(m_RightDoor.DOLocalMoveX(rightBounceX, m_CloseBounceDuration).SetEase(Ease.OutQuad));

        m_Sequence.Append(m_LeftDoor.DOLocalMoveX(+m_ClosedX, m_CloseBounceDuration).SetEase(Ease.InQuad));
        m_Sequence.Join(m_RightDoor.DOLocalMoveX(-m_ClosedX, m_CloseBounceDuration).SetEase(Ease.InQuad));
    }

    private void SetDoorsX(float x)
    {
        Vector3 leftPos = m_LeftDoor.localPosition;
        Vector3 rightPos = m_RightDoor.localPosition;

        leftPos.x = +x;
        rightPos.x = -x;

        m_LeftDoor.localPosition = leftPos;
        m_RightDoor.localPosition = rightPos;
    }
}