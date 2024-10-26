using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacStudentController : MonoBehaviour
{
    [SerializeField]
    public GameObject item;
        
    private Tweener tweener;
        
    // Start is called before the first frame update
    void Start()
    {
        tweener = GetComponent<Tweener>();
    }
    
    void AddTweenForItems(Vector3 position, float duration)
        {
            if (tweener.AddTween(item.transform, item.transform.position, position, duration))
            {
                return;
            }
        }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("a"))
        {
            print("a");
            AddTweenForItems(new Vector3(-2.0f, 0.5f, 0.0f), 1.5f);
        }
                
        if (Input.GetKeyDown("d"))
        {
            print("d");
            AddTweenForItems(new Vector3(2.0f, 0.5f, 0.0f), 1.5f);
        }
        
        if (Input.GetKeyDown("s"))
        {
            print("s");
            AddTweenForItems(new Vector3(0.0f, 0.5f, -2.0f), 0.5f);
        }
        
        if (Input.GetKeyDown("w"))
        {
            print("w");
            AddTweenForItems(new Vector3(0.0f, 0.5f, 2.0f), 0.5f);
        }
    }
}
