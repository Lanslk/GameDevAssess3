using UnityEngine;

public class PacStudentWalking : MonoBehaviour
{
    //private Animator animator;

    void Start()
    {
        //animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.rotation = Quaternion.Euler(0, 0, 0); // Face Right
            transform.localScale = new Vector3(-0.05f, 0.05f, 0.05f);
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.rotation = Quaternion.Euler(0, 0, -90); // Face Up
            transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.rotation = Quaternion.Euler(0, 0, 0); // Face Left
            transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.rotation = Quaternion.Euler(0, 0, 90); // Face Down
            transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        }
    }
}