using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class GenerateMap : MonoBehaviour
{
    
    public GameObject[] tilePrefabs; // Array of prefabs indexed from 1 to 7

    public GameObject PacStudent;
    
    public float tileSize = 0.04f; // Size of each tile (in Unity units)

    // The provided level map array
    int[,] levelMap =
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
    
    // int[,] levelMap =
    // {
    //     {1,2,2,2,2,2,2,2,2,2,2,2,2,2},
    //     {2,5,5,5,5,5,5,5,5,3,4,3,5,3},
    //     {2,5,3,4,4,3,5,3,4,3,0,4,5,3},
    //     {2,5,4,0,0,4,5,4,0,0,0,4,5,5},
    //     {2,6,3,4,4,3,5,3,4,4,4,3,5,3},
    //     {2,5,5,5,5,5,5,5,5,5,5,5,5,4},
    //     {2,5,3,4,4,3,5,3,4,4,4,3,5,4},
    //     {2,5,3,4,4,3,5,3,4,4,4,3,5,4},
    //     {2,5,5,5,5,5,5,5,5,5,5,5,5,3},
    //     {1,2,2,2,2,1,5,5,5,3,4,4,3,0},
    //     {0,0,0,0,0,2,5,3,4,3,0,0,4,0},
    //     {0,0,0,0,0,2,5,4,0,0,0,0,4,0},
    //     {0,0,0,0,0,2,5,4,0,0,0,3,3,0},
    //     {2,2,2,2,2,1,5,3,4,4,4,3,0,0},
    //     {0,0,0,0,0,0,5,0,0,0,0,0,0,0},
    // };

    void Start()
    {
        BuildLevel();
    }

    void BuildLevel()
    {

        // Loop through the level map
        for (int y = 0; y < levelMap.GetLength(0); y++)
        {
            for (int x = 0; x < levelMap.GetLength(1); x++)
            {
                int tileType = levelMap[y, x];
                Quaternion rotation = Quaternion.identity; // Default rotation
                
                
                switch(tileType) 
                {
                    case 0:
                        continue;
                    case 1:
                        rotation = rotateType1(x, y);
                        break;
                    case 2:
                        rotation = rotateType2(x, y);
                        break;
                    case 3:
                        rotation = rotateType3(x, y);
                        break;
                    case 4:
                        rotation = rotateType4(x, y);
                        break;
                    default:
                        break;
                }

                // Instantiate the corresponding tile prefab
                Vector3 position = new Vector3(x * tileSize, -y * tileSize, 0); // Calculate position
                GameObject tilePrefab = tilePrefabs[tileType - 1]; // Get prefab from array (1-indexed)
                Instantiate(tilePrefab, position, rotation, transform); // Instantiate prefab
            }
        }
    }

    private Quaternion rotateType1(int x, int y)
    {
        bool hasWallLeft = (x - 1 >= 0) && (levelMap[y, x - 1] == 2);
        bool hasWallRight = (x + 1 < levelMap.GetLength(1)) && (levelMap[y, x + 1] == 2);

        bool hasWallUp = (y - 1 >= 0) && (levelMap[y - 1, x] == 2);
        bool hasWallDown = (y + 1 < levelMap.GetLength(0)) && (levelMap[y + 1, x] == 2);
                    
        if (hasWallUp && hasWallRight)
        {
            return Quaternion.Euler(0, 0, 90);
        } 
        else if (hasWallUp && hasWallLeft)
        {
            return Quaternion.Euler(0, 0, 180);
        } 
        else if (hasWallDown && hasWallLeft)
        {
            return Quaternion.Euler(0, 0, 270);
        }
        else
        {
            return Quaternion.identity;
        }
    }

    private Quaternion rotateType2(int x, int y)
    {
        bool hasWallLeft = (x - 1 >= 0) && (levelMap[y, x - 1] == 2);
        bool hasWallRight = (x + 1 < levelMap.GetLength(1)) && (levelMap[y, x + 1] == 2);
        if (hasWallLeft || hasWallRight)
        {
            return Quaternion.Euler(0, 0, 90);
        } else
        {
            return Quaternion.identity;
        }
    }
    
    private Quaternion rotateType3(int x, int y)
    {
        Quaternion rotation = quaternion.identity;

        bool hasWallLeft = (x - 1 >= 0) && (levelMap[y, x - 1] == 4 || levelMap[y, x - 1] == 3);
        bool hasWallRight = (x + 1 < levelMap.GetLength(1)) &&
                            (levelMap[y, x + 1] == 4 || levelMap[y, x + 1] == 3);

        bool hasWallUp = (y - 1 >= 0) && (levelMap[y - 1, x] == 4 || levelMap[y - 1, x] == 3);
        bool hasWallDown = (y + 1 < levelMap.GetLength(0)) &&
                           (levelMap[y + 1, x] == 4 || levelMap[y + 1, x] == 3);
        
        // Count how many are true
        int trueCount = (hasWallLeft ? 1 : 0) + (hasWallRight ? 1 : 0) + (hasWallUp ? 1 : 0) + (hasWallDown ? 1 : 0);

        if (trueCount == 1)
        {
            if (hasWallLeft)
            {
                if (y == levelMap.GetLength(2) - 1)
                {
                    rotation = Quaternion.Euler(0, 0, 270);
                }
                else
                {
                    rotation = Quaternion.Euler(0, 0, 180);
                }
                
            }

            if (hasWallRight)
            {
                if (y == levelMap.GetLength(2) - 1)
                {
                    rotation = Quaternion.Euler(0, 0, 0);
                }
                else
                {
                    rotation = Quaternion.Euler(0, 0, 90);
                }
            }

            if (hasWallUp)
            {
                if (x == levelMap.GetLength(1) - 1)
                {
                    rotation = Quaternion.Euler(0, 0, 90);
                }
                else
                {
                    rotation = Quaternion.Euler(0, 0, 180);
                }

            }

            if (hasWallDown)
            {
                if (x == levelMap.GetLength(1) - 1) {
                	rotation = Quaternion.Euler(0, 0, 0);
                }
				else {
					rotation = Quaternion.Euler(0, 0, 270);
            	}
			}
        }
        else if (trueCount == 2)
        {
            if (hasWallUp && hasWallRight)
            {
                rotation = Quaternion.Euler(0, 0, 90);
            }
            else if (hasWallUp && hasWallLeft)
            {
                rotation = Quaternion.Euler(0, 0, 180);
            }
            else if (hasWallDown && hasWallLeft)
            {
                rotation = Quaternion.Euler(0, 0, 270);
            }
        }
        else if (trueCount >= 3)
        {
            bool isUpV = judgeInnerSide(x, y - 1) && levelMap[y - 1, x] == 4;
            bool isLeftV = judgeInnerSide(x - 1, y) && levelMap[y, x - 1] == 4;
            
            if (isUpV && isLeftV) rotation = Quaternion.Euler(0, 0, 90);
            if (!isUpV && isLeftV) rotation = Quaternion.Euler(0, 0, 0);
            if (!isUpV && !isLeftV) rotation = Quaternion.Euler(0, 0, 270);
            if (isUpV && !isLeftV) rotation = Quaternion.Euler(0, 0, 180);
        }
        return rotation;
    }

    private Quaternion rotateType4(int x, int y)
    {
        if (judgeInnerSide(x, y))
        {
            return Quaternion.Euler(0, 0, 90);
        }
        else
        {
            return Quaternion.identity;
        }
    }

    bool jusdgeIsInnerWall(int item)
    {
        if (item == 4 || item == 3 || item == 7)
        {
            return true;
        }

        return false;
    }

    // true = vertical, false = horizontal
    bool judgeInnerSide(int x, int y)
    {
        // Check surroundings to see if the wall should be horizontal or vertical
        bool hasWallLeft = jusdgeIsInnerWall(levelMap[y, x - 1]);
        bool hasWallRight = (x + 1 == levelMap.GetLength(1)) || (jusdgeIsInnerWall(levelMap[y, x + 1]));
        bool hasWallUp = jusdgeIsInnerWall(levelMap[y - 1, x]);
        bool hasWallDown = (y + 1 == levelMap.GetLength(0)) || (jusdgeIsInnerWall(levelMap[y + 1, x]));
        
        int trueCount = (hasWallLeft ? 1 : 0) + (hasWallRight ? 1 : 0) + (hasWallUp ? 1 : 0) + (hasWallDown ? 1 : 0);
        if (trueCount == 1 || trueCount == 2)
        {
            if (hasWallUp || hasWallDown)
            {
                return true;
            }
        }
        else if (trueCount == 3)
        {
            if (hasWallDown && hasWallUp)  return true;
        }

        return false;
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
