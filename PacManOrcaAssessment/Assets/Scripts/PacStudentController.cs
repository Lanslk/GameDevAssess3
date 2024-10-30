using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class PacStudentController : MonoBehaviour
{
    public Animator animatorController;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI ghostScaredTimerText;
    public AudioClip scaredMusic;
    public AudioClip normalMusic;
    
    private AudioSource backgroundMusicSource;

    
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

    private int[,] expandLevelMap = new int[29, 28];
    
    public AudioSource movementAudioSource;
    public AudioClip[] movementClips;
    
    private ParticleSystem dustParticles;
    private ParticleSystem dustParticlesCollide;

    public Boolean isStartScene;

    private Vector2 collidePosition;
    private Vector2 backDestination;
    
    private float CollideTimer = 0f;
    private Boolean StartCooldown = false; //cooldown aftwer collide on the wall
    
    // 0 = no teleport, 1 = teleport to right, 2 = teleport to left
    private int teleport = 0;
    
    private int score = 0;
    
    private GameObject[] ghosts;
    private float ghostScaredTimer = 0f;
    private Boolean startGhostScaredTimer = false;
    
    void Start()
    {
        if (isStartScene)
        {
            return;
        }
        
        UpdateScoreUI();
        dustParticles = transform.Find("Particle System").GetComponent<ParticleSystem>();
        GameObject particleObject = GameObject.FindWithTag("CollideParticle");
        if (particleObject != null)
        {
            dustParticlesCollide = particleObject.GetComponent<ParticleSystem>();
        }
        dustParticles.Stop();
        dustParticlesCollide.Stop();

        doExpandMap();
        ghosts = GameObject.FindGameObjectsWithTag("Ghost");
        
        startPosition = transform.position;
        targetPosition = transform.position;
        currentInput = Vector2.zero;
        lastInput = Vector2.zero;
        CollideTimer = Time.deltaTime;
        
        //eat first pellet
        eatPellets(1, 1);
        movementAudioSource.clip = movementClips[1];
        movementAudioSource.Play();
        
        ghosts = GameObject.FindGameObjectsWithTag("Ghost");
        backgroundMusicSource = Camera.main.GetComponent<AudioSource>();
        ghostScaredTimerText.enabled = false;
    }

    void Update()
    {
        if (isStartScene)
        {
            animatorController.SetBool("isStop", false);
            return;
        }
        
        UpdateScoreUI();

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
        
        if (startGhostScaredTimer)
        {
            ghostScaredTimer += Time.deltaTime;
            
            ghostScaredTimerText.enabled = true;
        
            int currentCountDown = 10 - (int)ghostScaredTimer;
            ghostScaredTimerText.text = currentCountDown.ToString();
        
            if (ghostScaredTimer > 10f)
            {
                ghostScaredTimer = 0f;
                startGhostScaredTimer = false;
                ghostScaredTimerText.enabled = false;
                
                foreach (GameObject ghost in ghosts)
                {
                    Animator ghostAnimator = ghost.GetComponent<Animator>();
                    if (ghostAnimator != null)
                    {
                        ghostAnimator.SetInteger("Direction", 1);
                    }
                }
                if (backgroundMusicSource != null && scaredMusic != null)
                {
                    backgroundMusicSource.clip = normalMusic;
                    backgroundMusicSource.Play();
                }
            } else if (ghostScaredTimer > 7f)
            {
                foreach (GameObject ghost in ghosts)
                {
                    Animator ghostAnimator = ghost.GetComponent<Animator>();
                    if (ghostAnimator != null)
                    {
                        ghostAnimator.SetInteger("Direction", 6);
                    }
                }
            }
        }
    }

    void doExpandMap()
    {
        for (int y = 0; y < 14; y++)
        {
            for (int x = 0; x < 14; x++)
            {
                expandLevelMap[y, x] = levelMap[y, x];
                
                expandLevelMap[y, 27 - x] = levelMap[y, x];
                
                expandLevelMap[28 - y, x] = levelMap[y, x];
                
                expandLevelMap[28 - y, 27 - x] = levelMap[y, x];
            }
        }
        
        for (int x = 0; x < 14; x++)
        {
            expandLevelMap[14, x] = levelMap[14, x];
            expandLevelMap[14, 27 - x] = levelMap[14, x];
        }
        
        for (int y = 0; y < 29; y++)
        {
            string row = "";
            for (int x = 0; x < 28; x++)
            {
                row += expandLevelMap[y, x] + " ";
            }
            //Debug.Log(row);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Wall")
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
        } else if (other.gameObject.tag == "Cherry") {
            score += 100;
            UpdateScoreUI();
            Destroy(other.gameObject);
        }
        
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
        int nextX = (int)Math.Round((position.x - 0.2f) / 0.04f);
        int nextY = (int)Math.Round((position.y + 0.2f) / 0.04f);
        
        int nowPosX = (int)Math.Round((transform.position.x - 0.2f) / 0.04f);
        int nowPosY = (int)Math.Round((transform.position.y + 0.2f) / 0.04f);
        
        int arrayX = 0;
        int arrayY = 0;
        
        if (nextY == -14 && nowPosY == -14 && nextX == -1 && nowPosX == 0) {
            teleport = 1;
            arrayX = 13;
            arrayY = 14;
        } else if (nextY == -14 && nowPosY == -14 && nextX == 28 && nowPosX == 27)
        {
            teleport = 2;
            arrayX = 0;
            arrayY = 14;
        } else {
            teleport = 0;
            arrayX = Math.Abs(nextX);
            arrayY = Math.Abs(nextY);
        }
        nextGrid = expandLevelMap[arrayY, arrayX];
        
        Boolean canMove = nextGrid == 5 || nextGrid == 0 || nextGrid == 6;
        if (canMove)
        {
            isCollide = false;
            lastInputBeforeCollide = lastInput;
        }
        else
        {
            collidePosition = position;
        }

        if (nextGrid == 5)
        {
            eatPellets(arrayX, arrayY);
        } else if (nextGrid == 6) {
            eatPowerPellets(arrayX, arrayY);
        }
        return canMove;
    }

    void eatPowerPellets(int arrayX, int arrayY)
    {
        expandLevelMap[arrayY, arrayX] = 0;
        float positionX = 0.2f + 0.04f * arrayX;
        float positionY = -0.2f - 0.04f * arrayY;
        GameObject pelletObj = FindGameObjectByTagAndPosition("PowerPellet", new Vector3(positionX, positionY, 0));
        Destroy(pelletObj);
        
        foreach (GameObject ghost in ghosts)
        {
            Animator ghostAnimator = ghost.GetComponent<Animator>();
            if (ghostAnimator != null)
            {
                ghostAnimator.SetInteger("Direction", 5);
            }
        }
        
        if (backgroundMusicSource != null && scaredMusic != null)
        {
            backgroundMusicSource.clip = scaredMusic;
            backgroundMusicSource.Play();
        }
        
        startGhostScaredTimer = true;
        ghostScaredTimer = 0f;
    }
    
    void eatPellets(int arrayX, int arrayY)
    {
        score += 10;
        UpdateScoreUI();
        
        expandLevelMap[arrayY, arrayX] = 0;
        float positionX = 0.2f + 0.04f * arrayX;
        float positionY = -0.2f - 0.04f * arrayY;
        GameObject pelletObj = FindGameObjectByTagAndPosition("Pellet", new Vector3(positionX, positionY, 0));
        Destroy(pelletObj);
    }
    
    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "" + score;
        }
    }
    
    GameObject FindGameObjectByTagAndPosition(string tag, Vector3 targetPosition, float tolerance = 0.01f)
    {
        GameObject[] gameObjectsWithTag = GameObject.FindGameObjectsWithTag(tag);
    
        foreach (GameObject obj in gameObjectsWithTag)
        {
            if (Vector3.Distance(obj.transform.position, targetPosition) <= tolerance)
            {
                return obj;
            }
        }
        return null;
    }
}
