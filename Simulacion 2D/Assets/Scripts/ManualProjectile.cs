using UnityEngine;
using TMPro;

public class ManualProjectile : MonoBehaviour
{
    [Header("Física Manual")]
    public Vector2 initialVelocity;
    public float gravity = -9.8f;
    
    [Header("Colisiones y Rebotes")]
    public float collisionRadius = 0.3f;
    public float energyLoss = 0.3f;
    public int maxBounces = 3;
    public TextMeshPro damageText;
    
    [Header("Física en Agua")]
    public float resistenciaAgua = 0.3f;
    public float gravedadEnAgua = -60f;
    
    private Vector2 currentVelocity;
    private Vector2 currentPosition;
    private float currentTime;
    private int currentBounces = 0;
    private bool isActive = true;
    private float nivelAgua = -2.49f;

 private void Start()
{
    currentVelocity = initialVelocity;
    currentPosition = transform.position;
    currentTime = 0f;
    
    // Registrar este proyectil en el tesoro
    ZonaGravedadTesoro tesoro = FindObjectOfType<ZonaGravedadTesoro>();
    if (tesoro != null)
    {
        tesoro.RegistrarProyectil(this);
    }
}

private void OnDestroy()
{
    // Remover este proyectil del tesoro al destruirse
    ZonaGravedadTesoro tesoro = FindObjectOfType<ZonaGravedadTesoro>();
    if (tesoro != null)
    {
        tesoro.RemoverProyectil(this);
    }
}

    private void Update()
    {
        if (!isActive) return;

        float deltaTime = Time.deltaTime;
        currentTime += deltaTime;
        
        // VERIFICAR SI ESTÁ EN AGUA (SIMPLIFICADO)
        bool enAgua = currentPosition.y <= nivelAgua;
        
        // APLICAR FÍSICA SEGÚN ESTADO
        if (enAgua)
        {
            // FÍSICA EN AGUA: Más gravedad + resistencia
            currentVelocity.y += gravedadEnAgua * deltaTime;
            currentVelocity *= Mathf.Pow(resistenciaAgua, deltaTime);
            
            // Frenado EXTRA en componente horizontal
            currentVelocity.x *= 0.95f;
            
            // DEBUG VISUAL
            Debug.DrawLine(currentPosition, currentPosition + Vector2.up * 0.5f, Color.blue);
        }
        else
        {
            // FÍSICA EN AIRE: Gravedad normal
            currentVelocity.y += gravity * deltaTime;
            
            // DEBUG VISUAL
            Debug.DrawLine(currentPosition, currentPosition + Vector2.up * 0.5f, Color.green);
        }
        
        // Actualizar posición
        currentPosition += currentVelocity * deltaTime;
        transform.position = currentPosition;
        
        // Rotar en dirección del movimiento
        if (currentVelocity.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        
        // Verificar colisiones
        CheckCollisions();
        
        // Verificar si está fuera de pantalla
        if (IsOutOfBounds())
        {
            Destroy(gameObject);
        }
    }

    // MÉTODO NUEVO: Para recibir fuerzas de gravedad externas
    public void AplicarFuerzaExterna(Vector2 fuerza)
    {
        currentVelocity += fuerza;
    }

    private void CheckCollisions()
    {
        if (!isActive) return;

        // 1. Detectar colisión con enemigos
        GameObject enemy = GameObject.Find("EnemyCharacter");
        if (enemy != null && CheckCircleCollision(enemy.transform.position, 0.4f))
        {
            HandleEnemyCollision(enemy);
            return;
        }

        // 2. Detectar colisión con bote enemigo
        GameObject enemyBoat = GameObject.Find("EnemyBoat");
        if (enemyBoat != null && CheckRectangleCollision(enemyBoat.transform.position, new Vector2(2f, 0.5f)))
        {
            HandleBoatCollision(enemyBoat);
            return;
        }

        // 3. Detectar colisión con bordes de pantalla
        CheckScreenBoundaries();
    }

    private bool CheckCircleCollision(Vector2 otherPosition, float otherRadius)
    {
        float distance = Vector2.Distance(currentPosition, otherPosition);
        return distance < (collisionRadius + otherRadius);
    }

    private bool CheckRectangleCollision(Vector2 rectCenter, Vector2 rectSize)
    {
        Vector2 rectMin = rectCenter - rectSize / 2f;
        Vector2 rectMax = rectCenter + rectSize / 2f;

        float closestX = Mathf.Clamp(currentPosition.x, rectMin.x, rectMax.x);
        float closestY = Mathf.Clamp(currentPosition.y, rectMin.y, rectMax.y);

        float distance = Vector2.Distance(currentPosition, new Vector2(closestX, closestY));
        return distance < collisionRadius;
    }

    private void HandleEnemyCollision(GameObject enemy)
    {
        Vector2 collisionNormal = (currentPosition - (Vector2)enemy.transform.position).normalized;
        ApplyReflection(collisionNormal);
        ShowDamageText("¡Impacto!", Color.red);
        Debug.Log("¡Golpe al enemigo!");
    }

    private void HandleBoatCollision(GameObject boat)
    {
        Vector2 collisionNormal = (currentPosition - (Vector2)boat.transform.position).normalized;
        ApplyReflection(collisionNormal);
        ShowDamageText("¡Bote!", Color.blue);
        
        FlotabilidadBote flotabilidad = boat.GetComponent<FlotabilidadBote>();
        if (flotabilidad != null)
        {
            Vector2 fuerza = -collisionNormal * currentVelocity.magnitude * 0.5f;
            flotabilidad.AplicarFuerza(fuerza);
        }
        
        Debug.Log("¡Golpe al bote!");
    }

    private void ApplyReflection(Vector2 normal)
    {
        float dotProduct = Vector2.Dot(currentVelocity.normalized, normal);
        Vector2 reflection = currentVelocity.normalized - 2 * dotProduct * normal;
        currentVelocity = reflection * currentVelocity.magnitude * (1f - energyLoss);
        currentBounces++;
        if (currentBounces >= maxBounces) Destroy(gameObject);
    }

    private void CheckScreenBoundaries()
    {
        Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        
        if (Mathf.Abs(currentPosition.x) > screenBounds.x - 1f)
        {
            Vector2 normal = new Vector2(-Mathf.Sign(currentPosition.x), 0f);
            ApplyReflection(normal);
        }
        
        if (currentPosition.y > screenBounds.y - 1f)
        {
            Vector2 normal = new Vector2(0f, -1f);
            ApplyReflection(normal);
        }
        
        if (currentPosition.y < -screenBounds.y) Destroy(gameObject);
    }

    private bool IsOutOfBounds()
    {
        Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        return Mathf.Abs(currentPosition.x) > screenBounds.x + 2f || 
               Mathf.Abs(currentPosition.y) > screenBounds.y + 2f;
    }

    private void ShowDamageText(string text, Color color)
    {
        if (damageText != null)
        {
            TextMeshPro textInstance = Instantiate(damageText, currentPosition, Quaternion.identity);
            textInstance.text = text;
            textInstance.color = color;
            Destroy(textInstance.gameObject, 1f);
        }
    }

    // Dibujar radio de colisión en el editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, collisionRadius);
    }
}