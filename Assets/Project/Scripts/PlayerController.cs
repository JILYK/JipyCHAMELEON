using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    public LineRenderer lineRenderer; // Линия для визуализации притяжения
    public float baseAttractionStrength = 10f; // Базовая сила притяжения
    private float currentAttractionStrength; // Текущая сила притяжения
    public float maxDistanceForAttraction = 20f; // Максимальная дистанция для действия силы притяжения
    public float minDistanceToSphere = 0.5f; // Минимальная дистанция для разрыва связи

    public float maxApproachSpeed = 5f; // Максимальная скорость, при которой связь разрывается
    public float dampingFactor = 0.95f; // Коэффициент демпфирования
    public float elasticity = 5f; // Коэффициент упругости
    public float maxHorizontalSpeed = 20f; // Максимальная горизонтальная скорость
    public float attractionIncreaseRate = 2f; // Скорость увеличения силы притяжения

    private Rigidbody2D rb; // Rigidbody игрока
    private bool isAttracted = false; // Указывает, активировано ли притяжение
    public GameObject currentSphere; // Ссылка на текущую сферу
    public List<Image> heartIcons = new List<Image>(); // Массив UI Image для сердец
    public Sprite heartOnSprite; // Спрайт для полного сердца
    public Sprite heartOffSprite; // Спрайт для пустого сердца
    public bool hasSelectedFirstSphere = false; // Флаг, был ли выбран первый объект
    public float previousYPosition; // Предыдущая позиция игрока по Y
    private int movementIndex = 1; // Индекс движения: 1 (вверх), -1 (вниз)
    [SerializeField] private int health = 3; // Текущее здоровье игрока
    private int maxHealth = 3; // Максимальное здоровье
    private int intermediatePointCount = 5; // Количество промежуточных точек
    public float invulnerabilityDuration = 1f; // Время неуязвимости после получения урона
    private bool isInvulnerable = false; // Уязвимость игрока
    [SerializeField] private float invulnerabilityTime = 3; // Время до конца неуязвимости
    public Animator background;
    public ParticleSystem damageParticle;
    public ParticleSystem addHeartParticle;
    private Image backgroundColor;

    public ParticleSystem[] particleSystems = { };

    private void OnEnable()
    {
        isInvulnerable = true;
    }

    void Start()
    {
        backgroundColor = background.GetComponent<Image>();
        PlayerImage = gameObject.GetComponent<Image>();
        UpdateHealthDisplay();
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
        lineRenderer.enabled = false;

        currentAttractionStrength = baseAttractionStrength;
        previousYPosition = transform.position.y;

        lineRenderer.positionCount = intermediatePointCount + 2;
    }

    public TrainingManager trainingManager;

    void Update()
    {
        print("CERF" + trainingManager.IsTrainingCompleted());
        if (!trainingManager.IsTrainingCompleted())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (!hasSelectedFirstSphere && hit.collider != null && hit.collider.CompareTag("Sphere"))
            {
                StartCoroutine(RemoveInvulnerability());
                verticalGenerator.startProc = true;
                rb.isKinematic = false;
                hasSelectedFirstSphere = true;
            }

            if (hit.collider != null && hit.collider.CompareTag("Sphere"))
            {
                lineRenderer.enabled = false;
                SetCurrentSphere(hit.collider.gameObject);
            }
            else
            {
                DisconnectSphere();
            }
        }

        if (lineRenderer.enabled && currentSphere != null)
        {
            UpdateLineRenderer();
        }
    }

    void FixedUpdate()
    {
        if (isAttracted && currentSphere != null)
        {
            UpdateMovementIndex();
            ApplyPhysicalAttraction();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bomb"))
        {
            ChangeHealth(-1);
        }

        if (other.CompareTag("Heart"))
        {
            ChangeHealth(1);
            other.gameObject.SetActive(false);
        }
    }

    public void UpdateHealthDisplay()
    {
        Debug.Log("Current Health: " + health);
        Debug.Log("Number of hearts: " + heartIcons.Count);

        for (int i = 0; i < heartIcons.Count; i++)
        {
            print("i = " + i);
            if (i < health)
            {
                heartIcons[i].sprite = heartOnSprite;
            }
            else
            {
                heartIcons[i].sprite = heartOffSprite;
            }
        }
    }

    private void SetCurrentSphere(GameObject newSphere)
    {
        GameManager.Instance.AddScore();
        TriggerRandomParticleEffect();
        currentSphere = newSphere;
        isAttracted = true;
    }

    private void UpdateMovementIndex()
    {
        float currentYPosition = transform.position.y;

        if (currentYPosition > previousYPosition)
        {
            movementIndex = 1;
        }
        else if (currentYPosition < previousYPosition)
        {
            movementIndex = -1;
        }

        previousYPosition = currentYPosition;
    }

    private void UpdateLineRenderer()
    {
        Vector3 start = transform.position;
        Vector3 end = currentSphere.transform.position;

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(intermediatePointCount + 1, end);

        for (int i = 1; i <= intermediatePointCount; i++)
        {
            float t = (float)i / (intermediatePointCount + 1);
            Vector3 intermediatePoint = Vector3.Lerp(start, end, t);
            lineRenderer.SetPosition(i, intermediatePoint);
        }
    }

    private void ApplyPhysicalAttraction()
    {
        Vector2 directionToSphere = (currentSphere.transform.position - transform.position);
        float distance = directionToSphere.magnitude;

        if (distance > maxDistanceForAttraction)
        {
            DisconnectSphere();
            return;
        }

        lineRenderer.enabled = true;
        if (currentSphere == null)
        {
            DisconnectSphere();
            return;
        }

        directionToSphere.Normalize();

        if (movementIndex == -1 && distance > minDistanceToSphere)
        {
            currentAttractionStrength += attractionIncreaseRate * Time.fixedDeltaTime;
        }
        else
        {
            currentAttractionStrength =
                Mathf.Lerp(currentAttractionStrength, baseAttractionStrength, Time.fixedDeltaTime);
        }

        rb.AddForce(directionToSphere * currentAttractionStrength, ForceMode2D.Force);

        if (Mathf.Abs(rb.velocity.x) > maxHorizontalSpeed)
        {
            rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxHorizontalSpeed, rb.velocity.y);
        }

        rb.velocity *= dampingFactor;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            ChangeHealth(-1);
        }
    }

    public void ChangeHealth(int amount)
    {
        if (amount < 0)
        {
            if (isInvulnerable)
            {
                return;
            }

            verticalGenerator.HearSpawntValue = +0.3f;
            background.SetTrigger("DamageT");
            CloneAndDetachParticleSystem(damageParticle);
        }
        else if (amount > 0)
        {
            if (health + amount <= maxHealth && verticalGenerator.HearSpawntValue > 0.1f)
            {
                verticalGenerator.HearSpawntValue = -0.3f;
            }

            CloneAndDetachParticleSystem(addHeartParticle);
            background.SetTrigger("AddHeartT");
        }

        health += amount;
        health = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthDisplay();
        if (gameObject.activeSelf)
        {
            StartCoroutine(RemoveInvulnerability());
        }

        if (health <= 0)
        {
            Debug.Log("Game Over!");
            GameManager.Instance.ToggleWindowObjects(2);
            background.SetTrigger("DamageT");
        }
    }

    public void CloneAndDetachParticleSystem(ParticleSystem originalParticleSystem)
    {
        ParticleSystem clonedParticleSystem = Instantiate(originalParticleSystem);
        clonedParticleSystem.transform.localScale = originalParticleSystem.transform.lossyScale;
        clonedParticleSystem.transform.parent = gameObject.transform.parent;
        clonedParticleSystem.gameObject.SetActive(true);
        clonedParticleSystem.gameObject.transform.localScale = new Vector3(218.340607f, 218.340607f, 218.340607f);
        clonedParticleSystem.transform.position =
            new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, 0);
    }

    private void TriggerRandomParticleEffect()
    {
        int randomIndex = Random.Range(0, particleSystems.Length);
        ParticleSystem selectedParticleSystem = particleSystems[randomIndex];
        CloneAndDetachParticleSystem(selectedParticleSystem);
    }

    private Image PlayerImage;

    private IEnumerator RemoveInvulnerability()
    {
        isInvulnerable = true;

        PlayerImage.color = new Color32(0, 255, 230, 255);
        yield return new WaitForSeconds(invulnerabilityTime);

        PlayerImage.color = new Color32(255, 255, 255, 255);
        backgroundColor.color = new Color32(255, 255, 255, 255);
        isInvulnerable = false;
    }

    public VerticalGenerator verticalGenerator;

    public void DisconnectSphere(bool sendToPool = false)
    {
        isAttracted = false;
        currentSphere = null;
        lineRenderer.enabled = false;
    }
}