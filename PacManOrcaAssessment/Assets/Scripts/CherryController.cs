using UnityEngine;

public class CherryController : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject zealPrefab;
    public float spawnInterval = 10;
    public float speed = 0.1f;

    void Start()
    {
        InvokeRepeating("SpawnCherry", spawnInterval, spawnInterval);
    }

    void SpawnCherry()
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();
        
        GameObject zealObj = Instantiate(zealPrefab, spawnPosition, Quaternion.identity);
        
        StartCoroutine(MoveCherry(zealObj, spawnPosition));
    }

    Vector3 GetRandomSpawnPosition()
    {
        // Randomly select one of four sides (top, bottom, left, right)
        int side = Random.Range(0, 4);
        Vector3 spawnPosition = Vector3.zero;
        
        Vector3 camMinBounds = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, 0));
        Vector3 camMaxBounds = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        
        switch (side)
        {
            case 0:
                spawnPosition = new Vector3(Random.Range(camMinBounds.x, camMaxBounds.x), camMaxBounds.y, 0);
                break;
            case 1:
                spawnPosition = new Vector3(Random.Range(camMinBounds.x, camMaxBounds.x), camMinBounds.y, 0);
                break;
            case 2:
                spawnPosition = new Vector3(camMinBounds.x, Random.Range(camMinBounds.y, camMaxBounds.y), 0);
                break;
            case 3:
                spawnPosition = new Vector3(camMaxBounds.x, Random.Range(camMinBounds.y, camMaxBounds.y), 0);
                break;
        }
        return spawnPosition;
    }

    System.Collections.IEnumerator MoveCherry(GameObject cherry, Vector3 startPosition)
    {
        float time = 0;
        Vector3 center = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, 0);
        Vector3 endPosition = startPosition + (center - startPosition) * 2.5f;

        while (cherry != null && cherry.transform.position != endPosition)
        {
            cherry.transform.position = Vector3.Lerp(startPosition, endPosition, time);
            time += Time.deltaTime * speed;
            
            if (IsOutOfBounds(cherry.transform.position))
            {
                Destroy(cherry);
                yield break;
            }

            yield return null;
        }
    }

    bool IsOutOfBounds(Vector3 position)
    {
        Vector3 camMinBounds = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, 0));
        Vector3 camMaxBounds = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        return position.x > camMaxBounds.x + 0.1 || position.x < camMinBounds.x - 0.1 || 
               position.y > camMaxBounds.y + 0.1 || position.y < camMinBounds.y - 0.1;
    }

    // Handle PacStudent collision with cherry (implement when details are provided)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PacStudent"))
        {
            // Handle the collision with PacStudent
            // (e.g., give bonus points, trigger some effect, and destroy the cherry)
            Destroy(gameObject);
        }
    }
}
