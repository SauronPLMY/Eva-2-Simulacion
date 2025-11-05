using UnityEngine;

public class AguaFlotabilidad : MonoBehaviour
{
    [Header("Propiedades del Agua")]
    public float densidad = 3f;
    public float nivelAgua = -3f;
    
    [Header("Referencias")]
    public Transform superficieAgua;

    private void Start()
    {
        if (superficieAgua != null)
        {
            nivelAgua = superficieAgua.position.y;
        }
        else
        {
            nivelAgua = -2.49f;
        }
}

    public float CalcularEmpuje(float volumenSumergido)
    {
        // F_E = ρ * g * V (densidad × gravedad × volumen sumergido)
        return densidad * Mathf.Abs(Physics.gravity.y) * volumenSumergido;
    }

    public float GetNivelAgua()
    {
        return nivelAgua;
    }

    // Dibujar gizmo para visualizar el nivel del agua
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Vector3 inicio = new Vector3(-10, nivelAgua, 0);
        Vector3 fin = new Vector3(10, nivelAgua, 0);
        Gizmos.DrawLine(inicio, fin);
    }
}