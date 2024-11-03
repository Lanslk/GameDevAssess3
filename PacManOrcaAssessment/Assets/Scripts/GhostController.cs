using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

public class GhostController : MonoBehaviour
{
    public Animator animatorController;
    [SerializeField]
    private float moveSpeed = 3f; // Speed at which PacStudent moves between grid positions.
    private Vector2 startPosition;
    private Vector2 targetPosition;
    private float lerpTime = 0.5f; // Time to move from one grid position to another (frame-rate independent).

    private Vector2 lastInput;
    private GameObject pacStudent;
    private int[] movement;
    bool isMoving = false;
    
    private Dictionary<int, Vector2> directionMap = new Dictionary<int, Vector2>{
        { 1, new Vector2(0.04f, 0)},{ 2, new Vector2(0, -0.04f)},{ 3, new Vector2(-0.04f, 0)},{ 4, new Vector2(0, 0.04f)}};
    
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

    private float startTimer = 0f;

    private Vector2 ghost4Target = Vector2.zero;
    
    private TextMeshProUGUI gameOverText;
    
    // Start is called before the first frame update
    void Start()
    {
        DoExpandMap();
        pacStudent = GameObject.Find("PacStudent");
        gameOverText = GameObject.Find("GameOver").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (startTimer < 5f)
        {
            startTimer += Time.deltaTime;
            return;
        }
        
        if (gameOverText.enabled == true) 
        {
            return;
        }

        moveGhost();
    }

    void moveGhost()
    {
        if (isMoving)
        {
            return;
        }
        
        movement = RandomMovement();
        
        if (doRespawnArea())
        {
            return;
        }
        
        //private Dictionary<int, Vector2> directionMap = new Dictionary<int, Vector2>{
        //{ 1, new Vector2(0.04f, 0)},{ 2, new Vector2(0, -0.04f)},{ 3, new Vector2(-0.04f, 0)},{ 4, new Vector2(0, 0.04f)}};
        
        int directionInt = animatorController.GetInteger("Direction");
        bool isWalkingState = directionInt <= 4;
        bool isScaredRecoverState = directionInt == 5 || directionInt == 6;
        bool isDied = directionInt == 7;
        if (transform.gameObject.name == "Ship-blue")
        {
            if (!isDied)
            {
                MoveGhost1();
            }
            else
            {
                MoveDead();
            }
        } else if (transform.gameObject.name == "Ship-green")
        {
            if (isWalkingState)
            {
                MoveGhost2();
            }
            else if (isScaredRecoverState)
            {
                MoveGhost1();
            } else if (isDied)
            {
                MoveDead();
            }
            
        } else if (transform.gameObject.name == "Ship-red")
        {
            if (isWalkingState)
            {
                MoveGhost3();
            }
            else if (isScaredRecoverState)
            {
                MoveGhost1();
            } else if (isDied)
            {
                MoveDead();
            }
        } else if (transform.gameObject.name == "Ship-yellow")
        {
            if (isWalkingState)
            {
                MoveGhost4();
            }
            else if (isScaredRecoverState)
            {
                MoveGhost1();
            } else if (isDied)
            {
                MoveDead();
            }
        }
        
    }
    
    bool doRespawnArea()
    {
        int nowPosX = (int)Math.Round((transform.position.x - 0.2f) / 0.04f);
        int nowPosY = (int)Math.Round((transform.position.y + 0.2f) / 0.04f);
        
        if (nowPosX >= 11 && nowPosX <= 16 && nowPosY <= -13 && nowPosY >= -15) {
            if (animatorController.GetInteger("Direction") == 7) {
                return true;
            }
            Vector2 target = Vector2.zero;
            if (transform.gameObject.name == "Ship-blue")
            {
                target = new Vector2(0.72f, -0.88f);
            } else if (transform.gameObject.name == "Ship-green")
            {
                target = new Vector2(0.72f, -0.64f);
            } else if (transform.gameObject.name == "Ship-red")
            {
                target = new Vector2(0.76f, -0.64f);
            }  else if (transform.gameObject.name == "Ship-yellow")
            {
                target = new Vector2(0.76f, -0.88f);
            }
            
            
            bool moveForward = false;
            foreach (int move in movement)
            {
                Vector2 direction = directionMap[move];
                Vector2 destination = (Vector2)transform.position + direction;
                float distanceWithPacBefore = Vector2.Distance(target, transform.position);
                float distanceWithPacAfter = Vector2.Distance(target, destination);
                bool isBack = direction == -lastInput;
                bool canMove = CanMoveToPosition(destination);
                bool isCloser = distanceWithPacAfter < distanceWithPacBefore;
                
                if (!isBack && canMove && isCloser)
                {
                    lastInput = direction;
                    StartCoroutine(MoveToPosition(destination, moveSpeed, ""));
                    moveForward = true;
                    break;
                }
            }
            
            if (!moveForward)
            {
                foreach (int move in movement)
                {
                    Vector2 direction = directionMap[move];
                    Vector2 destination = (Vector2)transform.position + direction;
                    bool isBack = direction == -lastInput;
                    bool canMove = CanMoveToPosition(destination);
                    
                    if (!isBack && canMove)
                    {
                        lastInput = direction;
                        StartCoroutine(MoveToPosition(destination, moveSpeed, ""));
                        moveForward = true;
                        break;
                    }
                }
            }
            
            return true;
        }
        return false;
    }

    void TargetGhost4()
    {
        float nowX = transform.position.x;
        float nowY = transform.position.y;
        
        bool isTopLeft = nowX <= 0.72f && nowY >= -0.68f;
        bool isDownLeft = nowX <= 0.72f && nowY < -0.68f;
        bool isTopRight = nowX > 0.72f && nowY >= -0.68f;
        bool isDownRight = nowX > 0.72f && nowY < -0.68f;
        
        Vector2 topLeftPos = new Vector2(0.24f, -0.24f);
        Vector2 downLeftPos = new Vector2(0.24f, -1.24f);
        Vector2 topRightPos = new Vector2(1.2f, -0.24f);
        Vector2 downRightPos = new Vector2(1.2f, -1.24f);
        if (isTopLeft)
        {
            if (ghost4Target == topLeftPos)
            {
               if (Vector2.Distance(topLeftPos, transform.position) < 0.04f)
                {
                    ghost4Target = downLeftPos;
                }
            } else if (ghost4Target == Vector2.zero)
            {
                ghost4Target = topLeftPos;
            }
        } else if (isDownLeft)
        {
            if (ghost4Target == downLeftPos)
            {
                if (Vector2.Distance(downLeftPos, transform.position) < 0.04f)
                {
                    ghost4Target = downRightPos;
                }
            } else if (ghost4Target == Vector2.zero)
            {
                ghost4Target = downLeftPos;
            }
        } else if (isDownRight)
        {
            if (ghost4Target == downRightPos)
            {
                if (Vector2.Distance(downRightPos, transform.position) < 0.04f)
                {
                    ghost4Target = topRightPos;
                }
            } else if (ghost4Target == Vector2.zero)
            {
                ghost4Target = downRightPos;
            }
        } else if (isTopRight)
        {
            
            if (ghost4Target == topRightPos)
            {
                if (Vector2.Distance(topRightPos, transform.position) < 0.04f)
                {
                    ghost4Target = topLeftPos;
                }
            } else if (ghost4Target == Vector2.zero)
            {
                ghost4Target = topRightPos;
            }
        }
        
        // 1 right, 2 down, 3 left, 4 up
        //{ 1, new Vector2(0.04f, 0)},{ 2, new Vector2(0, -0.04f)},{ 3, new Vector2(-0.04f, 0)},{ 4, new Vector2(0, 0.04f)}};
                
        if (ghost4Target == topLeftPos)
        {
            movement = new int[] {4, 3, 1, 2};
        } else if (ghost4Target == downLeftPos) 
        {
            movement = new int[] {3, 2, 4, 1};
        } else if (ghost4Target == topRightPos) 
        {
            movement = new int[] {1, 4, 2, 3};
        } else if (ghost4Target == downRightPos) 
        {
            movement = new int[] {2, 1, 3, 4};
        }
    }

    void MoveGhost4()
    {
        TargetGhost4();
        bool moveForward = false;
        foreach (int move in movement)
        {
            Vector2 direction = directionMap[move];
            Vector2 destination = (Vector2)transform.position + direction;
            
            float distanceWithPacBefore = Vector2.Distance(ghost4Target, transform.position);
            float distanceWithPacAfter = Vector2.Distance(ghost4Target, destination);
            
            if (Vector2.Distance(new Vector2(0.4f, -0.76f), destination) < 0.01f  || 
                Vector2.Distance(new Vector2(1.08f, -0.76f), destination) < 0.01f)
            {
                continue;
            }
            
            bool isBack = direction == -lastInput;
            bool canMove = CanMoveToPosition(destination);
            bool isCloser = distanceWithPacAfter < distanceWithPacBefore;
            
            if (!isBack && canMove && isCloser)
            {
                lastInput = direction;
                StartCoroutine(MoveToPosition(destination, moveSpeed, "ghost4"));
                moveForward = true;
                break;
            }
        }

        if (!moveForward)
        {
            foreach (int move in movement)
            {
                Vector2 direction = directionMap[move];
                Vector2 destination = (Vector2)transform.position + direction;
                
                if (Vector2.Distance(new Vector2(0.4f, -0.76f), destination) < 0.01f  || 
                    Vector2.Distance(new Vector2(1.08f, -0.76f), destination) < 0.01f)
                {
                    continue;
                }
            
                bool isBack = direction == -lastInput;
                bool canMove = CanMoveToPosition(destination);
            
                if (!isBack && canMove)
                {
                    lastInput = direction;
                    StartCoroutine(MoveToPosition(destination, moveSpeed, "ghost4"));
                    moveForward = true;
                    break;
                }
            }
            
        }

        if (!moveForward)
        {
            lastInput = -lastInput;
            StartCoroutine(MoveToPosition((Vector2)transform.position + lastInput, moveSpeed, "ghost4"));
        }

        ChangeDirection(lastInput);
    }

    void MoveDead()
    {
        StartCoroutine(MoveToPosition(new Vector2(0.72f, -0.76f), moveSpeed, "dead"));
    }
    
    void MoveGhost3()
    {
        bool moveForward = false;
        foreach (int move in movement)
        {
            Vector2 direction = directionMap[move];
            Vector2 destination = (Vector2)transform.position + direction;
            bool isBack = direction == -lastInput;
            bool canMove = CanMoveToPosition(destination);
            
            if (!isBack && canMove)
            {
                lastInput = direction;
                StartCoroutine(MoveToPosition(destination, moveSpeed, "ghost3"));
                moveForward = true;
                break;
            }
        }

        if (!moveForward)
        {
            lastInput = -lastInput;
            StartCoroutine(MoveToPosition((Vector2)transform.position + lastInput, moveSpeed, "ghost3"));
        }

        ChangeDirection(lastInput);
    }

    void MoveGhost2()
    {
        bool moveForward = false;
        foreach (int move in movement)
        {
            Vector2 direction = directionMap[move];
            Vector2 destination = (Vector2)transform.position + direction;
            float distanceWithPacBefore = Vector2.Distance(pacStudent.transform.position, transform.position);
            float distanceWithPacAfter = Vector2.Distance(pacStudent.transform.position, destination);
            bool isBack = direction == -lastInput;
            bool canMove = CanMoveToPosition(destination);
            bool isCloser = distanceWithPacAfter < distanceWithPacBefore;
            
            if (!isBack && canMove && isCloser)
            {
                lastInput = direction;
                StartCoroutine(MoveToPosition(destination, moveSpeed, "ghost2"));
                moveForward = true;
                break;
            }
        }
        
        if (!moveForward)
        {
            foreach (int move in movement)
            {
                Vector2 direction = directionMap[move];
                Vector2 destination = (Vector2)transform.position + direction;
                bool isBack = direction == -lastInput;
                bool canMove = CanMoveToPosition(destination);
                
                if (!isBack && canMove)
                {
                    lastInput = direction;
                    StartCoroutine(MoveToPosition(destination, moveSpeed, "ghost2"));
                    moveForward = true;
                    break;
                }
            }
        }

        if (!moveForward)
        {
            lastInput = -lastInput;
            StartCoroutine(MoveToPosition((Vector2)transform.position + lastInput, moveSpeed, "ghost2"));
        }

        ChangeDirection(lastInput);
    }
    
    void MoveGhost1()
    {
        bool moveForward = false;
        foreach (int move in movement)
        {
            Vector2 direction = directionMap[move];
            Vector2 destination = (Vector2)transform.position + direction;
            float distanceWithPacBefore = Vector2.Distance(pacStudent.transform.position, transform.position);
            float distanceWithPacAfter = Vector2.Distance(pacStudent.transform.position, destination);
            bool isBack = direction == -lastInput;
            bool canMove = CanMoveToPosition(destination);
            bool isFarther = distanceWithPacAfter >= distanceWithPacBefore;
            
            if (!isBack && canMove && isFarther)
            {
                lastInput = direction;
                StartCoroutine(MoveToPosition(destination, moveSpeed, "ghost1"));
                moveForward = true;
                break;
            }
        }
        
        if (!moveForward)
        {
            foreach (int move in movement)
            {
                Vector2 direction = directionMap[move];
                Vector2 destination = (Vector2)transform.position + direction;
                bool isBack = direction == -lastInput;
                bool canMove = CanMoveToPosition(destination);
                
                if (!isBack && canMove)
                {
                    lastInput = direction;
                    StartCoroutine(MoveToPosition(destination, moveSpeed, "ghost1"));
                    moveForward = true;
                    break;
                }
            }
        }

        if (!moveForward)
        {
            lastInput = -lastInput;
            StartCoroutine(MoveToPosition((Vector2)transform.position + lastInput, moveSpeed, "ghost1"));
        }

        ChangeDirection(lastInput);
    }
    
    void ChangeDirection(Vector2 movement) 
    {
        if (animatorController.GetInteger("Direction") > 4)
        {
            return;
        }
        
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
    
    IEnumerator MoveToPosition(Vector2 destination, float speed, string source)
    {
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

    int[] RandomMovement()
    {
        int[] sequence = {1, 2, 3, 4};
        System.Random random = new System.Random();
        return sequence.OrderBy(x => random.Next()).ToArray();
    }
    
    bool CanMoveToPosition(Vector2 position)
    {
        int nextX = (int)Math.Round((position.x - 0.2f) / 0.04f);
        int nextY = (int)Math.Round((position.y + 0.2f) / 0.04f);
        
        int nowPosX = (int)Math.Round((transform.position.x - 0.2f) / 0.04f);
        int nowPosY = (int)Math.Round((transform.position.y + 0.2f) / 0.04f);
                
        int arrayX = Math.Abs(nextX);
        int arrayY = Math.Abs(nextY);
        
        if (nextY == -14 && (nextX == -1 || nextX == 28))
        {
            return false;
        }
        
        // check if go into respawning area
        if (nextX >= 13 && nextX <= 14 && nextY == -12) {
            if (nowPosX >= 13 && nowPosX <= 14 && nowPosY == -11) {
                return false;
            }
        }
        
        if (nextX >= 13 && nextX <= 14 && nextY == -16) {
            if (nowPosX >= 13 && nowPosX <= 14 && nowPosY == -17) {
                return false;
            }
        }
        
        int nextGrid = expandLevelMap[arrayY, arrayX];
        
        Boolean canMove = nextGrid == 5 || nextGrid == 0 || nextGrid == 6;
        return canMove;
    }
    
    void DoExpandMap()
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
        }
    }
}
