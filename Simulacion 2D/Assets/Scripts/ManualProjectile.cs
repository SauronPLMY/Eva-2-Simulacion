using UnityEngine;

public class ManualProjectile : MonoBehaviour
{
    [Header("Física Manual")]
    public Vector2 initialVelocity;
    public float gravity = -9.8f;
    
    private Vector2 currentVelocity;
    private Vector2 currentPosition;
    private float currentTime;

    private void Start()
    {
        currentVelocity = initialVelocity;
        currentPosition = transform.position;
        currentTime = 0f;
    }

    private void Update()
    {
        // Integrar física manualmente (Euler simple)
        float deltaTime = Time.deltaTime;
        currentTime += deltaTime;
        
        // Aplicar gravedad: v = v0 + a*t
        currentVelocity.y += gravity * deltaTime;
        
        // Actualizar posición: p = p0 + v*t
        currentPosition += currentVelocity * deltaTime;
        
        // Aplicar al transform
        transform.position = currentPosition;
        
        // Rotar en dirección del movimiento (opcional)
        if (currentVelocity.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
}