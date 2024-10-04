using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationDemoPacStudent : MonoBehaviour
{
    public Animator animatorController;

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
 ;
            changeAnim(act);
            act = act == 5 ? 1 : act + 1;
        }
    }

    private void changeAnim(int i)
    {
        if (i == 3) {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            animatorController.SetInteger("Direction", 3);
        }
        
        if (i == 1) {
            transform.rotation = Quaternion.Euler(0, 0, -0);
            animatorController.SetInteger("Direction", 1);
        }
        
        if (i == 4) {
            transform.rotation = Quaternion.Euler(0, 0, -90);
            animatorController.SetInteger("Direction", 4);
        }
        
        if (i == 2) {
            transform.rotation = Quaternion.Euler(0, 0, 90);
            animatorController.SetInteger("Direction", 2);
        }
        
        if (i == 5) {
            animatorController.SetInteger("Direction", 5);
        }
    }
}
