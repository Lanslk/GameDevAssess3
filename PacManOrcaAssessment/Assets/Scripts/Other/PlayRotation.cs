using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayRotation : MonoBehaviour
{
    public Animator animatorController;
    public AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            //transform.rotation = Quaternion.Euler(0, 0, 0);
            animatorController.SetInteger("Direction", 3);
            
            if (audioSource) {
                audioSource.Play();
            }
        }
        
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            //transform.rotation = Quaternion.Euler(0, 0, -0);
            animatorController.SetInteger("Direction", 1);
            
            if (audioSource) {
                audioSource.Play();
            }
        }
        
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            //transform.rotation = Quaternion.Euler(0, 0, -90);
            animatorController.SetInteger("Direction", 4);
            
            if (audioSource) {
                audioSource.Play();
            }
        }
        
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            //transform.rotation = Quaternion.Euler(0, 0, 90);
            animatorController.SetInteger("Direction", 2);
            
            if (audioSource) {
                audioSource.Play();
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Space)) {
            animatorController.SetInteger("Direction", 5);
            
            if (audioSource) {
                audioSource.Play();
            }
        }
        
        if (Input.GetKeyDown(KeyCode.R)) {
            animatorController.SetInteger("Direction", 6);
            
            if (audioSource) {
                audioSource.Play();
            }
        }
        
        if (Input.GetKeyDown(KeyCode.D)) {
            animatorController.SetInteger("Direction", 7);
            
            if (audioSource) {
                audioSource.Play();
            }
        }
    }
}
