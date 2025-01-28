using UnityEngine;
using UnityEngine.Events;

public class DetectCollision : MonoBehaviour
{
    [SerializeField]
    private string tagToDetect = "Agent"; 

    [System.Serializable]
    public class CollisionEvent : UnityEvent<Collision>
    {
    }

    public CollisionEvent boxCollisionEvent = new CollisionEvent();


    private void OnCollisionStay(Collision col)
    {
        if (col.gameObject.CompareTag(tagToDetect))
        {
            boxCollisionEvent.Invoke(col);
        }
    }
}
