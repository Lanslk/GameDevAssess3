using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationDemoGhost : MonoBehaviour
{
    public Animator animatorController;
    public AudioSource audioSource;
    
    private float timer = 0f;

    private int act = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 3f)
        {
            timer = 0f;
            
            animatorController.SetInteger("Direction", act);
            act = act == 7 ? 1 : act + 1;
        }
    }
}
