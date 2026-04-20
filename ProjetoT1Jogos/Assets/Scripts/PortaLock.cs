using UnityEngine;


/// <summary>
/// Anexe no objeto "Espelho" da porta.
/// Bloqueia o XRGrabInteractable até o puzzle ser resolvido.
/// </summary>
public class PortaLock : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    void Start()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        // Porta começa travada
        if (grab != null)
            grab.enabled = false;

        Debug.Log("[PortaLock] Porta travada. Resolva o puzzle para abrir.");
    }

    /// <summary>
    /// Chamado pelo GeloDaTranca quando o gelo some.
    /// </summary>
    public void LiberarPorta()
    {
        if (grab != null)
            grab.enabled = true;

        Debug.Log("[PortaLock] Porta liberada! Pode abrir.");
    }
}
