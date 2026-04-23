using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Anexe no objeto "Espelho" da porta.
/// Trava a porta travando o Rigidbody e desabilitando o Grab.
/// </summary>
public class PortaLock : MonoBehaviour
{
    private XRGrabInteractable grab;
    private Rigidbody          rb;

    void Start()
    {
        grab = GetComponent<XRGrabInteractable>();
        rb   = GetComponent<Rigidbody>();

        TravarPorta();
    }

    void TravarPorta()
    {
        // Desabilita o Grab — não pode ser pego
        if (grab != null)
            grab.enabled = false;

        // Torna kinematic e congela tudo — não pode ser empurrado nem girar
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        Debug.Log("[PortaLock] Porta travada.");
    }

    public void LiberarPorta()
    {
        // Reabilita o Grab
        if (grab != null)
            grab.enabled = true;

        // Descongela o Rigidbody pra porta funcionar com o HingeJoint
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.None;
        }

        Debug.Log("[PortaLock] Porta liberada! Pode abrir.");
    }
}