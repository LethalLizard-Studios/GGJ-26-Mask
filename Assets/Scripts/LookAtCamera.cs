using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private void FixedUpdate()
    {
        transform.LookAt(Camera.main.transform, Vector3.up);
    }
}
