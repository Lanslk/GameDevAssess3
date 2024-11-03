using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTrack : MonoBehaviour
{
    private GameObject pacStudent;

    private bool zoomIn = false;
    // Start is called before the first frame update
    void Start()
    {
        pacStudent = GameObject.Find("PacStudent");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            zoomIn = !zoomIn;

        }

        if (zoomIn)
        {
            transform.position = new Vector3(pacStudent.transform.position.x,
                pacStudent.transform.position.y,
                transform.position.z);
            Camera.main.orthographicSize = 0.25f;
        }
        else
        {
            transform.position = new Vector3(0.74f, -0.76f, transform.position.z);
            Camera.main.orthographicSize = 0.75f;
        }
    }
}
