using UnityEngine;
using UnityEngine.Events;

public class OnTriggerEvent : MonoBehaviour
{
    [SerializeField] private UnityEvent m_OnTriggerEntered;
    [SerializeField] private UnityEvent m_OnTriggerExited;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        m_OnTriggerEntered.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        m_OnTriggerExited.Invoke();
    }
}
