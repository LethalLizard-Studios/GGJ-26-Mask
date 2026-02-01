using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] private int m_SceneBuildIndex;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public void ChangeScene()
    {
        SceneManager.LoadScene(m_SceneBuildIndex);
    }
}
