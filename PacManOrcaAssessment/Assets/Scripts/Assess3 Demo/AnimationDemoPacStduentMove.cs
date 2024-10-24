using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationDemoPacStudentMove: MonoBehaviour
{
    public Animator animatorController;

    private float timer = 0f;

    private int act = 1;
    
    [SerializeField]
    private GameObject item;
    
    private Tweener tweener;
    
    private List<GameObject> itemList = new List<GameObject>();
    
    // Start is called before the first frame update
    void Start()
    {
        tweener = GetComponent<Tweener>();
        if (tweener == null) print("test");
        itemList.Add(item);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > 1f)
        {
            timer = 0f;
            if (item.transform.position.x == 0.267f && item.transform.position.y == -0.21f)
            {
                changeAnim(3);
                AddTweenForItems(new Vector3(0.044f, -0.21f, 0.0f), 2f);
            }

            if (item.transform.position.x == 0.044f && item.transform.position.y == -0.21f)
            {
                changeAnim(4);
                AddTweenForItems(new Vector3(0.046f, -0.053f, 0.0f), 2f);
            }
            
            if (item.transform.position.x == 0.046f && item.transform.position.y == -0.053f)
            {
                changeAnim(1);
                AddTweenForItems(new Vector3(0.267f, -0.044f, 0.0f), 2f);
            }
            if (item.transform.position.x == 0.267f && item.transform.position.y == -0.044f)
            {
                changeAnim(2);
                AddTweenForItems(new Vector3(0.267f, -0.21f, 0.0f), 2f);
            }
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
    
    // Loop through the itemList and attempt to add a new tween
    void AddTweenForItems(Vector3 position, float duration)
    {
        foreach (GameObject obj in itemList)
        {
            if (tweener.AddTween(obj.transform, obj.transform.position, position, duration))
            {
                return;
            }
        }
    }
}
