using System.Collections;
using UnityEngine;

public class PacStudentController : MonoBehaviour
{
    public Animator animatorController;
    
    [SerializeField]
    private float moveSpeed = 3f; // Speed at which PacStudent moves between grid positions.
    private Vector2 startPosition;
    private Vector2 targetPosition;
    private bool isMoving = false;
    private float lerpTime = 0.5f; // Time to move from one grid position to another (frame-rate independent).
    
    private Vector2 currentInput;
    private Vector2 lastInput;

    void Start()
    {
        startPosition = transform.position;
        targetPosition = transform.position;
        currentInput = Vector2.zero;
        lastInput = Vector2.zero;
    }

    void Update()
    {
        // Gather player input
        if (Input.GetKeyDown(KeyCode.W))
        {
            lastInput = new Vector2(0, 0.04f);
            animatorController.SetInteger("Direction", 4);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            lastInput = new Vector2(-0.04f, 0);
            animatorController.SetInteger("Direction", 3);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            lastInput = new Vector2(0, -0.04f);
            animatorController.SetInteger("Direction", 2);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            lastInput = new Vector2(0.04f, 0);
            animatorController.SetInteger("Direction", 1);
        }

        // If PacStudent is not currently moving, check input and move if possible.
        if (!isMoving)
        {
            // Check if the direction from last input is walkable.
            if (CanMoveToPosition((Vector2)transform.position + lastInput))
            {
                currentInput = lastInput; // Set current input to last input
                StartCoroutine(MoveToPosition((Vector2)transform.position + lastInput));
            }
            // If the direction from current input is walkable, move there.
            else if (CanMoveToPosition((Vector2)transform.position + currentInput))
            {
                StartCoroutine(MoveToPosition((Vector2)transform.position + currentInput));
            }
        }
    }

    // Coroutine to move PacStudent between grid positions using linear lerping
    IEnumerator MoveToPosition(Vector2 destination)
    {
        isMoving = true;
        float elapsedTime = 0f;

        startPosition = transform.position;
        targetPosition = destination;

        while (elapsedTime < lerpTime)
        {
            transform.position = Vector2.Lerp(startPosition, targetPosition, elapsedTime / lerpTime);
            elapsedTime += Time.deltaTime * moveSpeed;
            yield return null; // Wait for next frame.
        }

        transform.position = targetPosition; // Ensure final position is exact.
        isMoving = false; // Movement finished
    }

    // Method to check if the next position is walkable based on level map
    bool CanMoveToPosition(Vector2 position)
    {
        // Implement your logic here to check if the grid position is walkable (e.g., not a wall).
        return true;
    }
}
