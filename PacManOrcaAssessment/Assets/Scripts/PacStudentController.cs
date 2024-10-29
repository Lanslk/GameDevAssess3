using System;
using System.Collections;
using UnityEngine;

public class PacStudentController : MonoBehaviour
{
    public Animator animatorController;
    
    [SerializeField]
    private float moveSpeed = 3f; // Speed at which PacStudent moves between grid positions.
    [SerializeField]
    private float moveBackSpeed = 10f;
    private Vector2 startPosition;
    private Vector2 targetPosition;
    private bool isMoving = false;
    private float lerpTime = 0.5f; // Time to move from one grid position to another (frame-rate independent).
    
    private Vector2 currentInput;
    private Vector2 lastInput;
    private Vector2 lastInputBeforeCollide;
    
    private int nextGrid = 0;

    private Boolean isCollide = false;

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
    
    private ParticleSystem dustParticles;
    private ParticleSystem dustParticlesCollide;

    public Boolean isStartScene;

    private Vector2 collidePosition;
    private Vector2 backDestination;
    
    private float CollideTimer = 0f;
    private Boolean StartCooldown = false;
    
    // 0 = no teleport, 1 = teleport to right, 2 = teleport to left
    private int teleport = 0;
    
    void Start()
    {
        if (!isStartScene)
        {
            dustParticles = transform.Find("Particle System").GetComponent<ParticleSystem>();
            GameObject particleObject = GameObject.FindWithTag("CollideParticle");
            if (particleObject != null)
            {
                dustParticlesCollide = particleObject.GetComponent<ParticleSystem>();
            }
            dustParticles.Stop();
            dustParticlesCollide.Stop();
        }

        startPosition = transform.position;
        targetPosition = transform.position;
        currentInput = Vector2.zero;
        lastInput = Vector2.zero;
        CollideTimer = Time.deltaTime;
    }

    void Update()
    {
        if (isStartScene)
        {
            animatorController.SetBool("isStop", false);
            return;
        }

        if (isCollide && Vector2.Distance(transform.position, backDestination) > 0.0005f)
        {
            return;
        }
        
        if (StartCooldown)
        {
            CollideTimer += Time.deltaTime;
            if (CollideTimer > 0.01f)
            {
                StartCooldown = false;
                CollideTimer = 0f;
            } else
            {
                return;
            }
        }
        
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
        if (!isMoving && lastInput != Vector2.zero)
        {
            if (lastInput != Vector2.zero && CanMoveToPosition((Vector2)transform.position + lastInput))
            {
                ChangeDirection(lastInput);
                currentInput = lastInput; // Set current input to last input
                StartCoroutine(MoveToPosition((Vector2)transform.position + lastInput, moveSpeed, "lastInput"));
            }
            else if (currentInput != Vector2.zero && CanMoveToPosition((Vector2)transform.position + currentInput))
            {
                ChangeDirection(currentInput);
                StartCoroutine(MoveToPosition((Vector2)transform.position + currentInput, moveSpeed, "currentInput"));
            }
            else
            {
                animatorController.SetBool("isStop", true);
                animatorController.SetFloat("StopDirection", (float)animatorController.GetInteger("Direction"));
                
                transform.rotation = Quaternion.Euler(0, 0, 90);
                if (!isCollide) {
                    StartCoroutine(MoveToPosition((Vector2)transform.position + lastInput, moveSpeed, "before Collide"));
                }

            }
        }
        
        MovementAudio();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Triggered by: " + other.gameObject.name);
        isCollide = true;
        StartCooldown = true;  

        backDestination = (Vector2)other.gameObject.transform.position - lastInputBeforeCollide;

        StartCoroutine(MoveToPosition(backDestination, moveBackSpeed, "moveBack"));
        
        dustParticlesCollide.transform.position = new Vector3(collidePosition.x, collidePosition.y, 0f);
        dustParticlesCollide.Emit(10);
        movementAudioSource.Stop();
        movementAudioSource.clip = movementClips[2];
        movementAudioSource.Play();
        
    }
    
    void MovementAudio()
    {
        if (!animatorController.GetBool("isStop") && movementAudioSource.isPlaying == false)
        {
            if (nextGrid == 0)
            {
                movementAudioSource.clip = movementClips[0];
                movementAudioSource.Play();
                dustParticles.Play();
            }
            else if (nextGrid == 5)
            {
                movementAudioSource.clip = movementClips[1];
                movementAudioSource.Play();
                dustParticles.Play();
            }
        }
    }
    
    void ChangeDirection(Vector2 movement) 
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
    IEnumerator MoveToPosition(Vector2 destination, float speed, string source)
    {
        // Teleporting
        if (teleport == 1)
        {
            transform.position = new Vector3(1.28f, -0.76f, 0f);
        } else if (teleport == 2)
        {
            transform.position = new Vector3(0.2f, -0.76f, 0f);
        } else {
            isMoving = true;
            float elapsedTime = 0f;

            startPosition = transform.position;
            targetPosition = destination;

            while (elapsedTime < lerpTime)
            {
                transform.position = Vector2.Lerp(startPosition, targetPosition, elapsedTime / lerpTime);
                elapsedTime += Time.deltaTime * speed;
                yield return null; // Wait for next frame.
            }

            isMoving = false;
        }
    }

    // Method to check if the next position is walkable based on level map
    bool CanMoveToPosition(Vector2 position)
    {
        int x = (int)Math.Round((position.x - 0.2f) / 0.04f);
        int y = (int)Math.Round((position.y + 0.2f) / 0.04f);
        
        int nowPosX = (int)Math.Round((transform.position.x - 0.2f) / 0.04f);
        int nowPosY = (int)Math.Round((transform.position.y + 0.2f) / 0.04f);
        
        if (y == -14 && nowPosY == -14 && x == -1 && nowPosX == 0) {
            teleport = 1;
            x = 13;
            y = 14;
        } else if (y == -14 && nowPosY == -14 && x == 28 && nowPosX == 27)
        {
            teleport = 2;
            x = 0;
            y = 14;
        } else {
            teleport = 0;
            // Mirror X and Y for positions outside the top-left quadrant
            x = Math.Abs(x);
            y = Math.Abs(y);
            if (x >= levelMap.GetLength(1))
            {
                x = levelMap.GetLength(1) * 2 - 1 - x;
            }
                            
            if (y >= levelMap.GetLength(0))
            {
                y = levelMap.GetLength(0) * 2 - 2 - y;
            }
        }
        
        nextGrid = levelMap[y, x];
        
        Boolean canMove = levelMap[y, x] == 5 || levelMap[y, x] == 0 || levelMap[y, x] == 6;
        if (canMove)
        {
            isCollide = false;
            lastInputBeforeCollide = lastInput;
        }
        else
        {
            collidePosition = position;
        }
        return canMove;
    }
}
