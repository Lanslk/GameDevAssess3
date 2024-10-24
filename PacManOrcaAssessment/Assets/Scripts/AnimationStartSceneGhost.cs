using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationStartSceneGhost : MonoBehaviour
{
    public Animator animatorController;

    private float timer = 0f;

    [SerializeField]
    private GameObject item;

    private Tweener tweener;

    private List<GameObject> itemList = new List<GameObject>();

    private Vector3[] positions = new Vector3[]
    {
        new Vector3(7.5f, -3.5f, 0.0f),
        new Vector3(-7.5f, -3.5f, 0.0f),
        new Vector3(-7.5f, 3.5f, 0.0f), 
        new Vector3(7.5f, 3.5f, 0.0f)
    };

    private int currentPositionIndex = 0;
    private float moveDuration = 0f;  // Duration for each movement
    private float moveShortSide = 1.4f;
    private float moveLongSide = 3f;

    private bool isFirstMove = true; // Flag to track the first move

    void Start()
    {
        tweener = GetComponent<Tweener>();
        if (tweener == null) print("Tweener not found!");

        itemList.Add(item);

        float StartWaitTime = getStartWaitTime();
        
        StartCoroutine(WaitBeforeFirstMove(StartWaitTime));
    }

    void Update()
    {
        timer += Time.deltaTime;
    }

    // Method to move to the next position in the sequence
    private void MoveToNextPosition()
    {
        // Ensure the position index loops back to the start
        currentPositionIndex = (currentPositionIndex + 1) % positions.Length;

        switch (currentPositionIndex)
        {
            case 0:
                changeAnim(2);
                moveDuration = moveShortSide;
                break;
            case 1:
                changeAnim(1);
                moveDuration = moveLongSide;
                break;
            case 2:
                changeAnim(4);
                moveDuration = moveShortSide;
                break;
            case 3:
                changeAnim(3);
                moveDuration = moveLongSide;
                break;
        }
        
        // Set the next position target
        Vector3 nextPosition = positions[currentPositionIndex];

        // Call tween to move to the next position
        AddTweenForItems(nextPosition, moveDuration);
    }

    private float getStartWaitTime()
    {
        if (gameObject.name == "Ship-blue")
            return 1.5f;
        else if (gameObject.name == "Ship-green")
            return 3;
        else if (gameObject.name == "Ship-red")
            return 4.5f;
        else
            return 6f;
    }

    // 1 left 2 down 3 right 4 up
    private void changeAnim(int direction)
    {
        switch (direction)
        {
            case 1:
                animatorController.SetInteger("Direction", 3);
                break;
            case 2:
                animatorController.SetInteger("Direction", 2);
                break;
            case 3:
                animatorController.SetInteger("Direction", 1);
                break;
            case 4:
                animatorController.SetInteger("Direction", 4);
                break;
            case 5:
                animatorController.SetInteger("Direction", 5);
                break;
        }
    }

    // Add tween for the object to move to the given position
    void AddTweenForItems(Vector3 position, float duration)
    {
        foreach (GameObject obj in itemList)
        {
            bool isSuccess = tweener.AddTween(obj.transform, obj.transform.position, position, duration);

            if (isSuccess)
            {
                // Automatically trigger the next movement after the duration
                StartCoroutine(WaitAndMoveNext(duration));  
            }
        }
    }

    // Wait before starting the first move
    IEnumerator WaitBeforeFirstMove(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        MoveToNextPosition();
    }
    
    IEnumerator WaitAndMoveNext(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        MoveToNextPosition();
    }
}
