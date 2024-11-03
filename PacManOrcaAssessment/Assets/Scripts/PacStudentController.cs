using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PacStudentController : MonoBehaviour
{
    public Animator animatorController;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI ghostScaredTimerText;
    public TextMeshProUGUI startGameCountDownText;
    public TextMeshProUGUI gameCountDownText;
    public TextMeshProUGUI gameOverText;
    public AudioClip scaredMusic;
    public AudioClip ghostDeadMusic;
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
    private List<ParticleSystem> dustParticlesDie = new List<ParticleSystem>();

    public Boolean isStartScene;

    private Vector2 collidePosition;
    private Vector2 backDestination;
    
    private float CollideTimer = 0f;
    private Boolean StartCooldownCollide = false; //cooldown after collide on the wall
    private Boolean StartCoolDownDie = false;  //cooldown after collide on the ghost
    
    // 0 = no teleport, 1 = teleport to right, 2 = teleport to left
    private int teleport = 0;
    
    private int score = 0;
    
    private List<GameObject> ghosts = new List<GameObject>();
    private float ghostScaredTimer = 0f;
    private Boolean startGhostScaredTimer = false;
    
    private List<float> ghostDieTimer = new List<float> {0f, 0f, 0f, 0f};
    private List<bool> startGhostDieTimer = new List<bool> {false, false, false, false};
    private Dictionary<int, string> ghostMap = new Dictionary<int, string>{
        { 0, "Ship-blue" },{ 1, "Ship-green" },{ 2, "Ship-red" },{ 3, "Ship-yellow" } };
    
    private int life = 3;
    private int pellets = 0;
    
    private float gameTimer = 0f;
    private bool isGameOver = false;
    private float gameOverTimer = 0f;
    
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
        
        startPosition = transform.position;
        targetPosition = transform.position;
        currentInput = Vector2.zero;
        lastInput = Vector2.zero;
        CollideTimer = Time.deltaTime;
        
        
        
        for (int i = 0; i < 4; i++)
        {
           ghosts.Add(GameObject.Find(ghostMap[i]));
        }
        
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
        
        if (isGameOver)
        {
            if (gameOverTimer > 3f) {
                GameObject quitButtonObject = GameObject.Find("QuitButton");
                Button quitButton = quitButtonObject.GetComponent<Button>();
                quitButton.onClick.Invoke();
                gameObject.SetActive(false);
            } else
            {
                gameOverTimer += Time.deltaTime;
                return;
            }
        }
        
        if (gameTimerCountDown()) { return;}
        
        
        
        UpdateScoreUI();

        if (isCollide && Vector2.Distance(transform.position, backDestination) > 0.0005f)
        {
            return;
        }
        
        if (StartCooldownCollide)
        {
            CollideTimer += Time.deltaTime;
            if (CollideTimer > 0.01f)
            {
                StartCooldownCollide = false;
                CollideTimer = 0f;
            } else
            {
                return;
            }
        }
        
        if (coolDownDie()) { return; }
        
        ghostDieCountDown();
        
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
        if (!isMoving && lastInput != Vector2.zero && !isGameOver)
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
                    StartCoroutine(MoveToPosition((Vector2)transform.position + currentInput, moveSpeed, "before Collide"));
                }

            }
        }
        
        MovementAudio();
        
        ghostScaredCountDown();
        
        gameOverCheck();
    }
    
    void gameOverCheck()
    {
        if (life == 0 || pellets == 0) {
            gameOverText.enabled = true;
            CheckAndSaveHighScore();
            isGameOver = true;
        }
    }
    
    void CheckAndSaveHighScore()
    {
        int savedHighScore = PlayerPrefs.GetInt("HighScore", 0);
        float savedHighScoreTime = PlayerPrefs.GetFloat("HighScoreTime", float.MaxValue);
    
        bool isNewHighScore = false;
    
        if (score > savedHighScore)
        {
            isNewHighScore = true;
        }
        else if (score == savedHighScore && gameTimer < savedHighScoreTime)
        {
            isNewHighScore = true;
        }
    
        if (isNewHighScore)
        {
            PlayerPrefs.SetInt("HighScore", score);
            PlayerPrefs.SetFloat("HighScoreTime", gameTimer - 5f);
            PlayerPrefs.Save();
        }
        }
        
    Boolean gameTimerCountDown() {
        gameTimer += Time.deltaTime;
        if (gameTimer < 5f) {
            int timeText = 4 - (int)gameTimer;
            startGameCountDownText.text = "" + timeText;
            startGameCountDownText.enabled = true;
            if (gameTimer > 4f) {
                startGameCountDownText.text = "GO!";
            }
            return true;
        } else {
            if (!backgroundMusicSource.isPlaying) {
                backgroundMusicSource.Play();
                eatPellets(1, 1);
                movementAudioSource.clip = movementClips[1];
                movementAudioSource.Play();
            }
            startGameCountDownText.enabled = false;
            
            float elapsedTime = gameTimer - 5f;
            
            TimeSpan timeSpan = TimeSpan.FromSeconds(elapsedTime);
            string timeFormatted = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        
            gameCountDownText.text = timeFormatted;
            return false;
        }
    }
    
    void ghostScaredCountDown()
    {
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
                    if (ghostAnimator != null && ghostAnimator.GetInteger("Direction") == 6)
                    {
                        ghostAnimator.SetInteger("Direction", 1);
                    }
                }
                
                if (!startGhostDieTimer.Contains(true))
                {
                    backgroundMusicSource.clip = normalMusic;
                    backgroundMusicSource.Play();
                }
            } else if (ghostScaredTimer > 7f)
            {
                for (int i = 0; i < ghosts.Count; i++)
                {
                    Animator ghostAnimator = ghosts[i].GetComponent<Animator>();
                    if (ghostAnimator != null && ghostAnimator.GetInteger("Direction") == 5)
                    {
                        ghostAnimator.SetInteger("Direction", 6);
                    }
                }
            }
        }
    }
    
    Boolean coolDownDie()
    {
        if (StartCoolDownDie)
        {
            CollideTimer += Time.deltaTime;
            if (CollideTimer > 1f)
            {   
                CollideTimer = 0f;
                StartCoolDownDie = false;  
                animatorController.SetBool("isStop", true);
                animatorController.SetInteger("Direction", 1);
                animatorController.SetFloat("StopDirection", 1f);
                currentInput = Vector2.zero;
                lastInput = Vector2.zero;
                transform.position = new Vector3(0.24f, -0.24f, 0f);
                
                GameObject lifeObject = GameObject.Find("lifeIndicator" + life);
                Destroy(lifeObject);
                
                foreach (ParticleSystem ps in dustParticlesDie)
                {
                    if (ps != null)
                    {
                        Destroy(ps.gameObject);
                    }
                }
                dustParticles.Clear();
                    
                life -= 1;
            } else 
            {
                return true;
            }
        }
        return false;
    }

    void doExpandMap()
    {
        for (int y = 0; y < 14; y++)
        {
            for (int x = 0; x < 14; x++)
            {
                if (levelMap[y, x] == 5)
                {
                    pellets += 4;
                }
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
            if (levelMap[14, x] == 5)
            {
                pellets++;
            }
        }
        
        for (int y = 0; y < 29; y++)
        {
            string row = "";
            for (int x = 0; x < 28; x++)
            {
                row += expandLevelMap[y, x] + " ";
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Wall")
        {
            Debug.Log("Triggered by: " + other.gameObject.name);
            isCollide = true;
            StartCooldownCollide = true;  
    
            
            backDestination = (Vector2)other.gameObject.transform.position - currentInput;
            
            StartCoroutine(MoveToPosition(backDestination, moveBackSpeed, "moveBack"));
            
            dustParticlesCollide.transform.position = new Vector3(collidePosition.x, collidePosition.y, 0f);
            dustParticlesCollide.Emit(10);
            movementAudioSource.Stop();
            movementAudioSource.clip = movementClips[2];
            movementAudioSource.Play();
        } else if (other.gameObject.tag == "Cherry") {
            score += 100;
            Destroy(other.gameObject);
        } else if (other.gameObject.tag == "Ghost") {
            if (StartCoolDownDie) {
                return;
            }
            Animator collideGhostAnimator = other.gameObject.GetComponent<Animator>();
            if (collideGhostAnimator != null)
            {
                if (collideGhostAnimator.GetInteger("Direction") <= 4) {
                    animatorController.SetFloat("StopDirection", (float)animatorController.GetInteger("Direction"));
                    animatorController.SetInteger("Direction", 5);
                    StartCoolDownDie = true;
                    movementAudioSource.Stop();
                    movementAudioSource.clip = movementClips[3];
                    movementAudioSource.Play();
                    EmitDieParticles();
                } else if (collideGhostAnimator.GetInteger("Direction") == 5 ||collideGhostAnimator.GetInteger("Direction") == 6) {
                    collideGhostAnimator.SetInteger("Direction", 7);
                    
                    backgroundMusicSource.clip = ghostDeadMusic;
                    backgroundMusicSource.Play();
                    score += 300;
                    
                    foreach (KeyValuePair<int, string> kvp in ghostMap)
                    {
                        int key = kvp.Key;
                        string value = kvp.Value;
                        if (other.gameObject.name == value) 
                        {
                            startGhostDieTimer[key] = true;
                        }
                    }
                }
            }
        }
    }
    
    void ghostDieCountDown()
    {
        for (int i = 0; i <  startGhostDieTimer.Count; i++)
        {
            if (startGhostDieTimer[i]) {
                ghostDieTimer[i] += Time.deltaTime;
                
                if (ghostDieTimer[i] > 5f) {
                    string name = "";
                    name = ghostMap[i];

                    GameObject ghostObj = GameObject.Find(name);
                    if (ghostObj != null) {
                        Animator ghostAnimator = ghostObj.GetComponent<Animator>();
                        ghostAnimator.SetInteger("Direction", 1);
                    }
                    startGhostDieTimer[i] = false;
                    ghostDieTimer[i] = 0f;
                    
                    if (!startGhostDieTimer.Contains(true)) {
                        if (startGhostScaredTimer) {
                            backgroundMusicSource.clip = scaredMusic;
                        } else {
                            backgroundMusicSource.clip = normalMusic;
                        }
                        backgroundMusicSource.Play();
                    }
                }
            }
        }
    }
    
    void EmitDieParticles()
    {
        Vector3[] offsets = new Vector3[]
        {
            new Vector3(0.05f, 0f, 0f),
            new Vector3(0f, -0.05f, 0f),
            new Vector3(-0.05f, 0f, 0f),
            new Vector3(0f, 0.05f, 0f),
            new Vector3(0f, 0f, 0f)
        };
    
        foreach (Vector3 offset in offsets)
        {
            ParticleSystem newDustParticle = Instantiate(dustParticlesCollide, transform.position + offset, Quaternion.identity);
            newDustParticle.Emit(10);
            dustParticlesDie.Add(newDustParticle);
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
        } else 
        {
            teleport = 0;
            arrayX = Math.Abs(nextX);
            arrayY = Math.Abs(nextY);
        }
        nextGrid = expandLevelMap[arrayY, arrayX];
        
        Boolean canMove = nextGrid == 5 || nextGrid == 0 || nextGrid == 6;
        if (canMove)
        {
            isCollide = false;
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
            if (ghostAnimator != null && ghostAnimator.GetInteger("Direction") != 7)
            {
                ghostAnimator.SetInteger("Direction", 5);
            }
        }
        
        backgroundMusicSource.clip = scaredMusic;
        backgroundMusicSource.Play();
        
        startGhostScaredTimer = true;
        ghostScaredTimer = 0f;
    }
    
    void eatPellets(int arrayX, int arrayY)
    {
        float positionX = 0.2f + 0.04f * arrayX;
        float positionY = -0.2f - 0.04f * arrayY;
        GameObject pelletObj = FindGameObjectByTagAndPosition("Pellet", new Vector3(positionX, positionY, 0));
        Destroy(pelletObj);
        score += 10;
        pellets--;
        expandLevelMap[arrayY, arrayX] = 0;
        
    }
    
    void UpdateScoreUI()
    {
        scoreText.text = "" + score;
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
