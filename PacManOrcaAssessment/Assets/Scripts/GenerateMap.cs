using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateMap : MonoBehaviour
{
    
    public GameObject[] tilePrefabs; // Array of prefabs indexed from 1 to 7

    public GameObject PacStudent;
    
    public float tileSize = 0.00125f; // Size of each tile (in Unity units)

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

    void Start()
    {
        // Vector3 position = new Vector3(0.4f, -0.42f, 0f);
        // Instantiate(PacStudent, position, Quaternion.identity, transform);
        
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
                
                if (tileType == 0) continue;

                if (tileType == 1 || tileType == 3)
                {
                    
                    bool hasWallLeft = (x - 1 >= 0) && (levelMap[y, x - 1] == 2);
                    bool hasWallRight = (x + 1 < levelMap.GetLength(1)) && (levelMap[y, x + 1] == 2);

                    bool hasWallUp = (y - 1 >= 0) && (levelMap[y - 1, x] == 2);
                    bool hasWallDown = (y + 1 < levelMap.GetLength(0)) && (levelMap[y + 1, x] == 2);
                    
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
                
                if (tileType == 2)
                {
                    // Check surroundings to see if the wall should be horizontal or vertical
                    bool hasWallLeft = (x - 1 >= 0) && (levelMap[y, x - 1] == 2);
                    bool hasWallRight = (x + 1 < levelMap.GetLength(1)) && (levelMap[y, x + 1] == 2);
        
                    
                    if (hasWallLeft || hasWallRight) rotation = Quaternion.Euler(0, 0, 90);

                }
                
                //TODO
                if (tileType == 3)
                {

                    bool hasWallLeft = (x - 1 >= 0) && (levelMap[y, x - 1] == 4 || levelMap[y, x - 1] == 3);
                    bool hasWallRight = (x + 1 < levelMap.GetLength(1)) &&
                                        (levelMap[y, x + 1] == 4 || levelMap[y, x + 1] == 3);

                    bool hasWallUp = (y - 1 >= 0) && (levelMap[y - 1, x] == 4 || levelMap[y - 1, x] == 3);
                    bool hasWallDown = (y + 1 < levelMap.GetLength(0)) &&
                                       (levelMap[y + 1, x] == 4 || levelMap[y + 1, x] == 3);

                    //print(x + "," + y + " : " + hasWallLeft + hasWallRight + hasWallUp + hasWallDown);

                    // Count how many are true
                    int trueCount = (hasWallLeft ? 1 : 0) + (hasWallRight ? 1 : 0) + (hasWallUp ? 1 : 0) + (hasWallDown ? 1 : 0);

                    if (trueCount == 1)
                    {
                        if (hasWallLeft) rotation = Quaternion.Euler(0, 0, 90);
                        if (hasWallRight) rotation = Quaternion.Euler(0, 0, 0);
                        if (hasWallUp) rotation = Quaternion.Euler(0, 0, 270);
                        if (hasWallDown) rotation = Quaternion.Euler(0, 0, 180);
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
                        bool isUpV = (x - 1 >= 0) && judgeInnerSide(y - 1, x) && levelMap[y - 1, x] == 4;
                        bool isRightV = (x + 1 < levelMap.GetLength(1)) && judgeInnerSide(y, x + 1) && levelMap[y, x + 1] == 4;
                        
                        if (isUpV && isRightV) rotation = Quaternion.Euler(0, 0, 180);
                        if (!isUpV && isRightV) rotation = Quaternion.Euler(0, 0, 270);
                        if (!isUpV && !isRightV) rotation = Quaternion.Euler(0, 0, 0);
                        if (isUpV && !isRightV) rotation = Quaternion.Euler(0, 0, 90);
                        print("x:" + x + " y:" + y);
                        print(isUpV + "," + isRightV);
                    }
                }
                
                //TODO
                if (tileType == 4)
                {
                    
                    if (judgeInnerSide(x, y)) rotation = Quaternion.Euler(0, 0, 90);

                }

                // Instantiate the corresponding tile prefab
                Vector3 position = new Vector3(x * tileSize, -y * tileSize, 0); // Calculate position
                GameObject tilePrefab = tilePrefabs[tileType - 1]; // Get prefab from array (1-indexed)
                Instantiate(tilePrefab, position, rotation, transform); // Instantiate prefab
            }
        }
    }


    // true = vertical, false = horizontal
    bool judgeInnerSide(int x, int y)
    {
        // Check surroundings to see if the wall should be horizontal or vertical
        bool hasWallLeft = (x - 1 >= 0) && (levelMap[y, x - 1] == 4 || levelMap[y, x - 1] == 3);
        bool hasWallRight = (x + 1 < levelMap.GetLength(1)) &&
                            (levelMap[y, x + 1] == 4 || levelMap[y, x + 1] == 3);

        bool hasWallUp = (y - 1 >= 0) && (levelMap[y - 1, x] == 4 || levelMap[y - 1, x] == 3);
        bool hasWallDown = (y + 1 < levelMap.GetLength(0)) &&
                           (levelMap[y + 1, x] == 4 || levelMap[y + 1, x] == 3);

                    
        int trueCount = (hasWallLeft ? 1 : 0) + (hasWallRight ? 1 : 0) + (hasWallUp ? 1 : 0) + (hasWallDown ? 1 : 0);

        if (trueCount == 2)
        {
            if (hasWallUp || hasWallDown) return true;
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
