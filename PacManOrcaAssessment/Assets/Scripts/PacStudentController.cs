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
    
    private int nextGrid = 0;

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
        {0,0,0,0,0,0,5,0,0,0,4,0,0,0},
    };
    
    public AudioSource movementAudioSource;
    public AudioClip[] movementClips;
    
    public ParticleSystem dustParticles;
    
    void Start()
    {
        startPosition = transform.position;
        targetPosition = transform.position;
        currentInput = Vector2.zero;
        lastInput = Vector2.zero;
        dustParticles.Stop();
    }

    void Update()
    {
        // Gather player input
        if (Input.GetKeyDown(KeyCode.W))
        {
            lastInput = new Vector2(0, 0.04f);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            lastInput = new Vector2(-0.04f, 0);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            lastInput = new Vector2(0, -0.04f);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            lastInput = new Vector2(0.04f, 0);
        }

        // If PacStudent is not currently moving, check input and move if possible.
        if (!isMoving)
        {
            if (lastInput != Vector2.zero && CanMoveToPosition((Vector2)transform.position + lastInput))
            {
                changeDirection(lastInput);
                currentInput = lastInput; // Set current input to last input
                StartCoroutine(MoveToPosition((Vector2)transform.position + lastInput));
            }
            else if (currentInput != Vector2.zero && CanMoveToPosition((Vector2)transform.position + currentInput))
            {
                changeDirection(currentInput);
                StartCoroutine(MoveToPosition((Vector2)transform.position + currentInput));
            }
            else
            {
                animatorController.SetBool("isStop", true);
                animatorController.SetFloat("StopDirection", (float)animatorController.GetInteger("Direction"));
                
                transform.rotation = Quaternion.Euler(0, 0, 90);
            }
        }
        
        MovementAudio();
    }
    
    void MovementAudio()
    {
        if (animatorController.GetBool("isStop") && movementAudioSource.isPlaying == true)
        {
            movementAudioSource.Stop();
            dustParticles.Stop();
        } else if (!animatorController.GetBool("isStop") && movementAudioSource.isPlaying == false) {       
            if (nextGrid == 0)
            {
                movementAudioSource.clip = movementClips[0];
                //footstepAudioSource.volume = movementSqrMagnitude;               
                movementAudioSource.Play();
                dustParticles.Play();
            } else if (nextGrid == 5) 
            {
                movementAudioSource.clip = movementClips[1];
                movementAudioSource.Play();
                dustParticles.Play();
            } else {
                movementAudioSource.Stop();
                dustParticles.Stop();
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
        
        // Mirror X and Y for positions outside the top-left quadrant
        if (x >= levelMap.GetLength(1))
        {
            x = levelMap.GetLength(1) * 2 - 1 - x;
        }
    
        if (y >= levelMap.GetLength(0))
        {
            y = levelMap.GetLength(0) * 2 - 2 - y;
        }
        nextGrid = levelMap[y, x];
            
        return levelMap[y, x] == 5 || levelMap[y, x] == 0 || levelMap[y, x] == 6;
    }
}
