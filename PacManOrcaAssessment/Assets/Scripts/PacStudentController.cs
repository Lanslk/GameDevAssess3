using System;
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

    private int[,] levelMap =
    {
        {1,2,2,2,2,2,2,2,2,2,2,2,2,7},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,4},
        {2,6,4,0,0,4,5,4,0,0,0,4,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,3},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,5},
        {2,5,3,4,4,3,5,3,3,5,3,4,4,4},
        {2,5,3,4,4,3,5,4,4,5,3,4,4,3},
        {2,5,5,5,5,5,5,4,4,5,5,5,5,4},
        {1,2,2,2,2,1,5,4,3,4,4,3,0,4},
        {0,0,0,0,0,2,5,4,3,4,4,3,0,3},
        {0,0,0,0,0,2,5,4,4,0,0,0,0,0},
        {0,0,0,0,0,2,5,4,4,0,3,4,4,0},
        {2,2,2,2,2,1,5,3,3,0,4,0,0,0},
    };
    
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
            //animatorController.SetInteger("Direction", 4);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            lastInput = new Vector2(-0.04f, 0);
            //animatorController.SetInteger("Direction", 3);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            lastInput = new Vector2(0, -0.04f);
            //animatorController.SetInteger("Direction", 2);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            lastInput = new Vector2(0.04f, 0);
            //animatorController.SetInteger("Direction", 1);
        }

        // If PacStudent is not currently moving, check input and move if possible.
        if (!isMoving)
        {
            if (CanMoveToPosition((Vector2)transform.position + lastInput))
            {
                changeDirection(lastInput);
                currentInput = lastInput; // Set current input to last input
                StartCoroutine(MoveToPosition((Vector2)transform.position + lastInput));
            }
            else if (CanMoveToPosition((Vector2)transform.position + currentInput))
            {
                changeDirection(currentInput);
                StartCoroutine(MoveToPosition((Vector2)transform.position + currentInput));
            }
            else
            {
                animatorController.SetBool("isStop", true);
                transform.rotation = Quaternion.Euler(0, 0, 90);
            }
        }
    }
    
    void changeDirection(Vector2 movement) 
    {
        animatorController.SetBool("isStop", false);
        if (movement.x == 0 && movement.y == 0.04f)
        {
            animatorController.SetInteger("Direction", 4);
        }
        else if (movement.x == -0.04f && movement.y == 0f)
        {
            animatorController.SetInteger("Direction", 3);
        }
        else if (movement.x == 0 && movement.y == -0.04f)
        {
            animatorController.SetInteger("Direction", 2);
        }
        else if (movement.x == 0.04f && movement.y == 0)
        {
            animatorController.SetInteger("Direction", 1);
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

        transform.position = targetPosition;
        isMoving = false;
    }

    // Method to check if the next position is walkable based on level map
    bool CanMoveToPosition(Vector2 position)
    {
        int x = Math.Abs((int)Math.Round((position.x - 0.2f) / 0.04f));
        int y = Math.Abs((int)Math.Round((position.y + 0.2f) / 0.04f));
        return levelMap[y, x] == 5 || levelMap[y, x] == 0 || levelMap[y, x] == 6;
    }
}
