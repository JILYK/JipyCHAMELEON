using System.Collections.Generic;
using UnityEngine;

public class VerticalGenerator : MonoBehaviour
{
    [Header("Prefabs")] public GameObject heartPrefab; // Префаб сердца

    private List<GameObject> heartsPool = new List<GameObject>(); // Пул сердец

    public GameObject spherePrefab; // Префаб липкой сферы
    public GameObject trapPrefab; // Префаб ловушки
    public Transform player;
    private PlayerController playerController; // Ссылка на контроллер игрока
    public Camera mainCamera; // Ссылка на камеру

    [Header("Generation Settings")] public int initialPoolSize = 10; // Размер пула объектов
    public float verticalSpacing = 3f; // Расстояние между объектами по Y

    [Header("Area Settings")] public BoxCollider2D generationArea; // Область генерации объектов (BoxCollider2D)
    public GameObject PAPA; // Область генерации объектов (BoxCollider2D)
    [Header("Generation Settings")] public float HearSpawntValue = 0.05f;
    public float BombSpawntValue = 0.35f;
    public float minYDistance = 1.0f; // Минимальное расстояние между объектами по Y
    public float minXDistance = 1.0f; // Минимальное расстояние между объектами по X
    public int maxAttempts = 10; // Максимальное количество попыток найти допустимый X

    private List<GameObject> spheresPool = new List<GameObject>(); // Пул сфер
    private List<GameObject> trapsPool = new List<GameObject>(); // Пул ловушек

    private Bounds bounds; // Границы BoxCollider2D
    private float playerPreviousYPosition; // Предыдущая позиция игрока по Y

    private float GetValidXPosition()
    {
        float xPosition = Random.Range(bounds.min.x, bounds.max.x); // Генерируем случайную позицию по X
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            bool valid = true;

            foreach (var sphere in spheresPool)
            {
                if (sphere != null && Mathf.Abs(xPosition - sphere.transform.position.x) < minXDistance)
                {
                    valid = false;
                    break;
                }
            }

            if (valid)
            {
                foreach (var trap in trapsPool)
                {
                    if (trap != null && Mathf.Abs(xPosition - trap.transform.position.x) < minXDistance)
                    {
                        valid = false;
                        break;
                    }
                }
            }

            if (valid)
            {
                return xPosition;
            }

            xPosition = Random.Range(bounds.min.x, bounds.max.x);
            attempts++;
        }

        Debug.LogWarning("Не удалось найти допустимую позицию по X после максимального числа попыток.");
        return xPosition;
    }

    private void Start()
    {
        playerController = player.GetComponent<PlayerController>();
        bounds = generationArea.bounds;
        playerPreviousYPosition = player.position.y;
        FillScreenWithObjects();
    }

    private void FillScreenWithObjects()
    {
        float screenHeight = bounds.size.y;
        float currentY = bounds.min.y;

        while (currentY < bounds.max.y)
        {
            CreateObjectForFullScreen(currentY);
            currentY += verticalSpacing;
        }
    }

    private void CreateObjectForFullScreen(float yPosition)
    {
        float xPosition = GetValidXPosition();

        Vector3 spherePosition = new Vector3(xPosition, yPosition, 0f);
        GameObject sphere = Instantiate(spherePrefab, spherePosition, Quaternion.identity, PAPA.transform);
        spheresPool.Add(sphere);

        Vector3 trapPosition = new Vector3(xPosition, yPosition - verticalSpacing / 2, 0f);
        GameObject trap = Instantiate(trapPrefab, trapPosition, Quaternion.identity, PAPA.transform);
        trapsPool.Add(trap);

        Vector3 heartPosition = new Vector3(xPosition, yPosition, 0f);
        GameObject heart = Instantiate(heartPrefab, heartPosition, Quaternion.identity, PAPA.transform);
        heart.SetActive(false);
        heartsPool.Add(heart);
    }

    public bool startProc = false;

    void Update()
    {
        if (startProc)
        {
            CheckPlayerMovement();
            RegenerateOffScreenObjects();
        }
    }

    private void CheckPlayerMovement()
    {
        float playerCurrentYPosition = player.position.y;
        float yDifference = playerCurrentYPosition - playerPreviousYPosition;

        if (yDifference > 0)
        {
            MovePoolDown(yDifference);
        }

        playerPreviousYPosition = playerCurrentYPosition;
    }

    private void MovePoolDown(float yDifference)
    {
        PAPA.transform.position += Vector3.down * yDifference;
    }

    private void RegenerateOffScreenObjects()
    {
        bool heartSpawned = false;
        bool bombSpawned = false;
        float spawnChance = Random.value;

        heartSpawned = CheckAndRegenerateObject(heartsPool, bounds.max.y, HearSpawntValue, spawnChance);
        bombSpawned = CheckAndRegenerateObject(trapsPool, bounds.max.y, BombSpawntValue, spawnChance);

        if (!heartSpawned && !bombSpawned)
        {
            CheckAndRegenerateObject(spheresPool, bounds.max.y, 1f, spawnChance);
        }

        DisableObjectsOutOfBounds(heartsPool);
        DisableObjectsOutOfBounds(trapsPool);
        DisableObjectsOutOfBounds(spheresPool);

        EnsureMinimumSpheres(3);
    }

    private bool CheckAndRegenerateObject(List<GameObject> pool, float maxYPosition, float spawnValue,
        float spawnChance)
    {
        bool objectSpawned = false;

        foreach (var obj in pool)
        {
            if (obj.transform.position.y < bounds.min.y)
            {
                obj.SetActive(true);

                if (spawnChance < spawnValue)
                {
                    float yPosition = maxYPosition;
                    float xPosition = GetValidXPosition();
                    obj.transform.position = new Vector3(xPosition, yPosition, 0f);
                    objectSpawned = true;
                }
                else
                {
                    obj.SetActive(false);
                }
            }
        }

        return objectSpawned;
    }

    private void DisableObjectsOutOfBounds(List<GameObject> pool)
    {
        foreach (var obj in pool)
        {
            bool isOutOfBounds = obj.transform.position.y < bounds.min.y || obj.transform.position.y > bounds.max.y;
            if (isOutOfBounds)
            {
                obj.SetActive(false);
            }
        }
    }

    private void EnsureMinimumSpheres(int minCount)
    {
        int activeSpheres = 0;

        foreach (var sphere in spheresPool)
        {
            if (sphere != null && sphere.activeSelf)
            {
                activeSpheres++;
            }
        }

        foreach (var sphere in spheresPool)
        {
            if (sphere != null && !sphere.activeSelf && activeSpheres < minCount)
            {
                sphere.SetActive(true);
                activeSpheres++;
            }

            if (activeSpheres >= minCount)
            {
                break;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (generationArea != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(generationArea.bounds.center, generationArea.bounds.size);
        }
    }

    public void ReturnSphereToPool(GameObject sphere)
    {
        spheresPool.Add(sphere);
    }

    public void RestartGame()
    {
        playerController.previousYPosition = -4.095461f;
        startProc = false;
        playerController.DisconnectSphere();

        GameManager.Instance.currentScore = 0;

        playerController.ChangeHealth(3);

        player.localPosition = new Vector3(-0.0126f, -893.580017f, 0);
        player.GetComponent<Rigidbody2D>().velocity = UnityEngine.Vector2.zero;
        player.GetComponent<Rigidbody2D>().isKinematic = true;

        playerController.hasSelectedFirstSphere = false;

        RegenerateOffScreenObjects();
    }
}