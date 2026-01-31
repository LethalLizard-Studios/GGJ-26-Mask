using UnityEngine;

public class EventManager : MonoBehaviour
{
    [Range(1, 10)]
    [SerializeField] private int m_Difficulty;
    [SerializeField] private int m_NightLength = 240;

    [Header("Events")]
    [SerializeField] private float m_PowerOutagePerRun = 1;
    [SerializeField] private Vector2 m_SpotlightMoveDelay = new Vector2(30, 78);
    [SerializeField] private Vector2 m_MaskAppreanceDelay = new Vector2(65, 130);
    [SerializeField] private float m_MaskKillDelay = 6;

    public void PowerOutage()
    {

    }

    public void RandomSpotlightMove()
    {

    }

    public void RandomMaskAppreance()
    {

    }
}
