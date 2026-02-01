using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveToPosition : MonoBehaviour
{
    [System.Serializable]
    private class Path
    {
        [SerializeField] private Transform m_Location;
        [SerializeField] private Transform[] m_Pathway;

        public Transform Location => m_Location;
        public Transform[] Pathway => m_Pathway;
    }

    [SerializeField] private Path[] m_Paths;

    [SerializeField] private AudioSource m_MoveSound;

    [SerializeField] private bool m_KillsPlayerAtEnd = false;

    [SerializeField] private Transform m_Player;

    [SerializeField] private float m_MoveSpeed = 20.0f;
    [SerializeField] private float m_WaypointReachedDistance = 0.15f;

    private Coroutine m_MoveRoutine;

    private void OnDisable()
    {
        if (m_MoveRoutine != null)
        {
            StopCoroutine(m_MoveRoutine);
            m_MoveRoutine = null;
        }
    }

    public void BeginMove()
    {
        if (m_MoveRoutine != null)
        {
            StopCoroutine(m_MoveRoutine);
            m_MoveRoutine = null;
        }

        Path path = GetClosestPath();
        if (path == null) return;

        m_MoveSound.Play();
        m_MoveRoutine = StartCoroutine(FollowPath(path));
    }

    private Path GetClosestPath()
    {
        if (m_Paths == null || m_Paths.Length == 0) return null;

        Path closest = null;
        float bestSqr = float.MaxValue;
        Vector3 p = m_Player != null ? m_Player.position : transform.position;

        for (int i = 0; i < m_Paths.Length; i++)
        {
            Path candidate = m_Paths[i];
            if (candidate == null) continue;

            Transform loc = candidate.Location;
            if (loc == null) continue;

            float sqr = (loc.position - p).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                closest = candidate;
            }
        }

        return closest;
    }

    private IEnumerator FollowPath(Path path)
    {
        Transform[] pathway = path.Pathway;

        if (pathway != null && pathway.Length > 0)
        {
            for (int i = 0; i < pathway.Length; i++)
            {
                Transform waypoint = pathway[i];
                if (waypoint == null) continue;

                yield return MoveTo(waypoint.position);
            }
        }
        else if (path.Location != null)
        {
            yield return MoveTo(path.Location.position);
        }

        if (m_KillsPlayerAtEnd && m_Player != null)
        {
            while (true)
            {
                transform.position = Vector3.MoveTowards(transform.position, m_Player.position, m_MoveSpeed * Time.deltaTime);
                yield return null;

                if ((transform.position - m_Player.position).sqrMagnitude <= 2.0 * 2.0)
                {
                    m_MoveSound.Stop();
                    KilledPlayer();
                    yield break;
                }
            }
        }

        m_MoveSound.Stop();
        m_MoveRoutine = null;
    }

    private IEnumerator MoveTo(Vector3 targetPosition)
    {
        float reachedSqr = m_WaypointReachedDistance * m_WaypointReachedDistance;

        while ((transform.position - targetPosition).sqrMagnitude > reachedSqr)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, m_MoveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private void KilledPlayer()
    {
        Debug.Log("Killed Player");

        SceneManager.LoadScene(1);
    }
}
