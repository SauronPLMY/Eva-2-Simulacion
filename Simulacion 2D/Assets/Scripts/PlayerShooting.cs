using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerShooting : MonoBehaviour
{
    [Header("Referencias")]
    public LineRenderer trajectoryLine;
    public GameObject projectilePrefab;
    public Transform shootPoint;
    
    [Header("Física Manual")]
    public float maxShootForce = 20f;
    public float minShootForce = 5f;
    public float gravity = -9.8f;
    public int trajectoryPoints = 10;
    public float timeStep = 0.1f;
    
    private bool isAiming = false;
    private Vector2 initialVelocity;
    private Vector2 mousePosition;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        trajectoryLine.positionCount = trajectoryPoints;
        trajectoryLine.enabled = false;
    }

    private void Update()
    {
        if (isAiming)
        {
            CalculateTrajectory();
        }
    }

    // Input System - Click para apuntar/disparar
    public void OnAim(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isAiming = true;
            trajectoryLine.enabled = true;
        }
        else if (context.canceled && isAiming)
        {
            isAiming = false;
            trajectoryLine.enabled = false;
            Shoot();
        }
    }

    // Input System - Posición del mouse (Vector2)
    public void OnMousePosition(InputAction.CallbackContext context)
    {
        mousePosition = context.ReadValue<Vector2>();
    }

    private void CalculateTrajectory()
    {
        // Convertir mouse position a mundo
        Vector3 worldMousePos = mainCamera.ScreenToWorldPoint(
            new Vector3(mousePosition.x, mousePosition.y, 10f) // Usar 10f de distancia
        );
        
        // Calcular dirección
        Vector2 direction = (worldMousePos - shootPoint.position).normalized;
        
        // Fuerza proporcional a la distancia
        float distance = Vector2.Distance(shootPoint.position, worldMousePos);
        float force = Mathf.Clamp(distance * 2f, minShootForce, maxShootForce);
        
        initialVelocity = direction * force;

        // Calcular trayectoria
        Vector3 startPos = shootPoint.position;
        for (int i = 0; i < trajectoryPoints; i++)
        {
            float time = i * timeStep;
            Vector3 point = startPos + new Vector3(
                initialVelocity.x * time,
                initialVelocity.y * time + 0.5f * gravity * time * time,
                0
            );
            trajectoryLine.SetPosition(i, point);
        }
    }

    private void Shoot()
    {
        if (projectilePrefab != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
            ManualProjectile physics = projectile.GetComponent<ManualProjectile>();
            if (physics != null)
            {
                physics.initialVelocity = initialVelocity;
                physics.gravity = gravity;
            }
        }
    }
}