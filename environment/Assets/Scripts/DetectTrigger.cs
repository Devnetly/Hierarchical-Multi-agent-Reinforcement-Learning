using UnityEngine;
using UnityEngine.Events;

public class DetectTrigger : MonoBehaviour
{
    [SerializeField]
    private string tagToDetect = "Agent"; 

    [SerializeField]
    private float reward = 1;

    [SerializeField]
    private bool isCheckpoint = false;

    private Collider m_col;
    [System.Serializable]
    public class TriggerEvent : UnityEvent<Collider, float>
    {
    }
    public TriggerEvent cpTriggerEnterEvent = new TriggerEvent();

    public TriggerEvent firstStageTriggerExitEvent = new TriggerEvent();
    public TriggerEvent firstStageTriggerEnterEvent = new TriggerEvent();

    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag(tagToDetect))
        {
            if (isCheckpoint)
            {
                col.gameObject.GetComponent<PuzzleAgent>().FoundCheckpoint = true;
                cpTriggerEnterEvent.Invoke(m_col, reward);
            }
            firstStageTriggerEnterEvent.Invoke(col, reward);
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.CompareTag(tagToDetect))
        {
            firstStageTriggerExitEvent.Invoke(col, reward);
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        m_col = GetComponent<Collider>();
    }
}
