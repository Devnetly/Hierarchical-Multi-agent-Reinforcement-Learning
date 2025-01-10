using UnityEngine;
using UnityEngine.Events;

public class DetectTrigger : MonoBehaviour
{
    public string tagToDetect = "agent"; 

    public float reward = 1;

    public bool isCheckpoint = false;

    private Collider m_col;
    [System.Serializable]
    public class TriggerEvent : UnityEvent<Collider, float>
    {
    }

    public TriggerEvent flagTriggerEnterEvent = new TriggerEvent();
    public TriggerEvent cpTriggerEnterEvent = new TriggerEvent();

    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag(tagToDetect))
        {
            if (isCheckpoint)
            {
                col.gameObject.GetComponent<PuzzleAgent>().FoundCheckpoint = true;
                cpTriggerEnterEvent.Invoke(m_col, reward);
            }
            else
            {
                flagTriggerEnterEvent.Invoke(m_col, reward);
            }
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        m_col = GetComponent<Collider>();
    }
}
