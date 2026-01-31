using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public string promptMessage = "'F' Interact";
    public UnityEvent OnInteractedWith;
    public Animation animationOnInteract;

    private bool canInteract = true;

    public void Interact()
    {
        if (animationOnInteract != null)
        {
            animationOnInteract.Stop();
            animationOnInteract.Play();
        }

        OnInteractedWith.Invoke();
    }

    public bool CanBeInteractedWith()
    {
        return canInteract && (animationOnInteract == null || (animationOnInteract != null && !animationOnInteract.isPlaying));
    }

    public void SetCanInteractWith(bool canInteract)
    {
        this.canInteract = canInteract;
    }
}
