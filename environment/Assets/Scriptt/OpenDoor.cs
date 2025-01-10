using UnityEngine;

public class OpenDoor : MonoBehaviour
{
    public GameObject door;
    private Animator anim;


    void Start()
    {
        anim = door.GetComponent<Animator>();
    }

    void OnTriggerStay(Collider other)
    {
        if(other.tag == "Agent")
        {
            anim.SetBool("Opening", true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.tag == "Agent")
        {
            anim.SetBool("Opening", false);
        }
    }
}
