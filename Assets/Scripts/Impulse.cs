using UnityEngine;

public class Impulse : MonoBehaviour
{
    [Tooltip("Dirección y fuerza del impulso. Puedes ajustar X, Y, Z libremente.")]
    public Vector3 velocidadInicial = new Vector3(0f, 0f, 10f);

    [Tooltip("Elige si quieres aplicar una fuerza instantánea (Impulse) o cambiar la velocidad directamente (VelocityChange).")]
    public ForceMode modoFuerza = ForceMode.VelocityChange;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.AddForce(velocidadInicial, modoFuerza);
        }
        else
        {
            Debug.LogWarning("El objeto no tiene un componente Rigidbody para aplicar el impulso.", this);
        }
    }
}