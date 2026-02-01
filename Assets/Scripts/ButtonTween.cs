using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonTween : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] private Image m_TargetImage;
    [SerializeField] private RectTransform m_TargetRectTransform;

    [Header("Hover")]
    [SerializeField] private float m_TweenDuration = 0.15f;
    [SerializeField] private Color m_NormalColor = Color.white;
    [SerializeField] private Color m_HoverColor = Color.white;
    [SerializeField] private float m_NormalScaleX = 1f;
    [SerializeField] private float m_HoverScaleX = 1.08f;
    [SerializeField] private Ease m_Ease = Ease.OutSine;

    private Tween m_ColorTween;
    private Tween m_ScaleTween;

    private void Awake()
    {
        if (m_TargetRectTransform == null) m_TargetRectTransform = transform as RectTransform;
        if (m_TargetImage == null) m_TargetImage = GetComponent<Image>();
    }

    private void OnDisable()
    {
        m_ColorTween.Kill(false);
        m_ScaleTween.Kill(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        TweenTo(m_HoverColor, m_HoverScaleX);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TweenTo(m_NormalColor, m_NormalScaleX);
    }

    private void TweenTo(Color targetColor, float targetScaleX)
    {
        if (m_TargetImage != null)
        {
            m_ColorTween.Kill(false);
            m_ColorTween = m_TargetImage.DOColor(targetColor, m_TweenDuration).SetEase(m_Ease);
        }

        if (m_TargetRectTransform != null)
        {
            m_ScaleTween.Kill(false);

            Vector3 scale = m_TargetRectTransform.localScale;
            scale.x = targetScaleX;

            m_ScaleTween = m_TargetRectTransform.DOScale(scale, m_TweenDuration).SetEase(m_Ease);
        }
    }
}
