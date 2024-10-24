using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationStartScenePacStudent : MonoBehaviour
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

    void Start()
    {
        tweener = GetComponent<Tweener>();
        if (tweener == null) print("Tweener not found!");

        itemList.Add(item);
        

        // Start moving the object to the first target position
        MoveToNextPosition();
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

    // 1 right 2 down 3 left 4 up
    private void changeAnim(int direction)
    {
        switch (direction)
        {
            case 1:
                transform.localScale = new Vector3(50, 50, 0);
                transform.rotation = Quaternion.Euler(0, 0, 0);
                animatorController.SetInteger("Direction", 1);
                break;
            case 2:
                transform.localScale = new Vector3(50, 50, 0);
                transform.rotation = Quaternion.Euler(0, 0, 90);
                animatorController.SetInteger("Direction", 2);
                break;
            case 3:
                transform.localScale = new Vector3(-50, 50, 0);
                transform.rotation = Quaternion.Euler(0, 0, 0);
                animatorController.SetInteger("Direction", 3);
                break;
            case 4:
                transform.localScale = new Vector3(50, 50, 0);
                transform.rotation = Quaternion.Euler(0, 0, -90);
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

    // Wait for the current tween to finish, then move to the next position
    IEnumerator WaitAndMoveNext(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        MoveToNextPosition();  // Move to the next target
    }
}
