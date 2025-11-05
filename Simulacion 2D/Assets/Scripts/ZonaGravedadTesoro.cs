using UnityEngine;
using System.Collections.Generic;

public class ZonaGravedadTesoro : MonoBehaviour
{
    [Header("Propiedades Gravedad")]
    public float fuerzaGravedad = 50f;
    public float radioGravedad = 3f;
    public float constanteG = 100f;
    
    [Header("Tiempo Respawn")]
    public float tiempoRespawn = 30f;
    
    [Header("Visual")]
    public float radioVisual = 0.5f;
    
    private bool activo = true;
    private SpriteRenderer spriteRenderer;
    private List<ManualProjectile> proyectilesEnEscena = new List<ManualProjectile>();

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Método para que los proyectiles se registren automáticamente
    public void RegistrarProyectil(ManualProjectile proyectil)
    {
        if (!proyectilesEnEscena.Contains(proyectil))
        {
            proyectilesEnEscena.Add(proyectil);
        }
    }

    // Método para eliminar proyectiles de la lista
    public void RemoverProyectil(ManualProjectile proyectil)
    {
        proyectilesEnEscena.Remove(proyectil);
    }

    private void Update()
    {
        if (!activo) return;

        // 1. Aplicar gravedad a proyectiles
        AplicarGravedadAProyectiles();
        
        // 2. Detectar colisión con proyectiles (MANUAL)
        DetectarColisionProyectiles();
    }

    private void AplicarGravedadAProyectiles()
    {
        // Usar la lista en lugar de FindObjectsOfType
        for (int i = proyectilesEnEscena.Count - 1; i >= 0; i--)
        {
            ManualProjectile proyectil = proyectilesEnEscena[i];
            if (proyectil == null)
            {
                proyectilesEnEscena.RemoveAt(i);
                continue;
            }

            Vector2 posicionProyectil = proyectil.transform.position;
            Vector2 direccion = (Vector2)transform.position - posicionProyectil;
            float distancia = direccion.magnitude;

            if (distancia <= radioGravedad && distancia > 0.1f)
            {
                float fuerza = (constanteG * fuerzaGravedad) / (distancia * distancia);
                Vector2 fuerzaAplicar = direccion.normalized * fuerza * Time.deltaTime;
                proyectil.AplicarFuerzaExterna(fuerzaAplicar);
            }
        }
    }

    private void DetectarColisionProyectiles()
    {
        for (int i = proyectilesEnEscena.Count - 1; i >= 0; i--)
        {
            ManualProjectile proyectil = proyectilesEnEscena[i];
            if (proyectil == null)
            {
                proyectilesEnEscena.RemoveAt(i);
                continue;
            }

            Vector2 posicionProyectil = proyectil.transform.position;
            float distancia = Vector2.Distance(transform.position, posicionProyectil);

            if (distancia <= (radioVisual + proyectil.collisionRadius))
            {
                RecolectarTesoro();
                break;
            }
        }
    }

    private void RecolectarTesoro()
    {
        Debug.Log("¡TESORO RECOLECTADO!");
        activo = false;
        spriteRenderer.enabled = false;
        Invoke("RespawnTesoro", tiempoRespawn);
    }

    private void RespawnTesoro()
    {
        activo = true;
        spriteRenderer.enabled = true;
        Debug.Log("¡Tesoro reapareció!");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioGravedad);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioVisual);
    }
}