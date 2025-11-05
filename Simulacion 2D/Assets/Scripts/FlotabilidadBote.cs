using UnityEngine;

public class FlotabilidadBote : MonoBehaviour
{
    [Header("Propiedades del Bote")]
    public float masa = 1f;
    public float volumen = 0.5f;
    public float amortiguacionAgua = 0.98f;
    public float amortiguacionAire = 1f;
    
    [Header("Referencias")]
    public AguaFlotabilidad agua;
    
    private Vector2 velocidad;
    private Vector2 posicion;
    private bool enAgua = false;
    private float gravedad = 9.81f;

    private void Start()
    {
        posicion = transform.position;
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        
        // Verificar si está en el agua
        float nivelAgua = agua.GetNivelAgua();
        enAgua = posicion.y <= nivelAgua;
        
        // Aplicar fuerzas
        Vector2 fuerzaTotal = Vector2.zero;
        
        // 1. GRAVEDAD (siempre)
        fuerzaTotal.y -= masa * gravedad;
        
        // 2. FLOTABILIDAD (solo si está en agua)
        if (enAgua)
        {
            float volumenSumergido = CalcularVolumenSumergido(nivelAgua);
            float empuje = agua.CalcularEmpuje(volumenSumergido);
            fuerzaTotal.y += empuje;
        }
        
        // Integrar física (F = m*a → a = F/m)
        Vector2 aceleracion = fuerzaTotal / masa;
        velocidad += aceleracion * deltaTime;
        
        // Aplicar amortiguación
        float amortiguacion = enAgua ? amortiguacionAgua : amortiguacionAire;
        velocidad *= Mathf.Pow(amortiguacion, deltaTime);
        
        // Actualizar posición
        posicion += velocidad * deltaTime;
        transform.position = posicion;
        
        // Rotación basada en movimiento (opcional)
        float rotacionZ = -velocidad.x * 2f;
        transform.rotation = Quaternion.Euler(0, 0, rotacionZ);
    }

    private float CalcularVolumenSumergido(float nivelAgua)
    {
        // Calcular qué porcentaje del bote está bajo el agua
        float alturaBote = transform.localScale.y;
        float centroBote = posicion.y;
        float fondoBote = centroBote - alturaBote / 2f;
        
        if (fondoBote >= nivelAgua) return 0f; // Totalmente fuera
        if (fondoBote + alturaBote <= nivelAgua) return volumen; // Totalmente sumergido
        
        // Parcialmente sumergido
        float alturaSumergida = nivelAgua - fondoBote;
        float porcentajeSumergido = alturaSumergida / alturaBote;
        return volumen * porcentajeSumergido;
    }

    // Método para aplicar fuerza externa (cuando el proyectil golpea)
    public void AplicarFuerza(Vector2 fuerza)
    {
        velocidad += fuerza / masa;
    }
}