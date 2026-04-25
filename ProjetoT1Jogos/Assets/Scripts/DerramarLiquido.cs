using UnityEngine;

public class DerramarLiquido : MonoBehaviour
{
    [SerializeField] private float anguloMin = 100f;

    private bool inZonaDerramar = false;
    private bool estahDerramando = false;

    void Update()
    {
        if (!inZonaDerramar || estahDerramando) return;

        float angulo = Vector3.Angle(transform.up, Vector3.up);
        if (angulo > anguloMin)
            Pour();
    }

    void Pour()
    {
        estahDerramando = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ZonaDerramar"))
        {
            GetComponent<Outline>().OutlineWidth = 5f;
            inZonaDerramar = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ZonaDerramar"))
        {
            GetComponent<Outline>().OutlineWidth = 0f;
            inZonaDerramar = false;
        }
    }
}