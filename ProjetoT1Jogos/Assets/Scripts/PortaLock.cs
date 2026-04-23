using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Anexe no objeto "Espelho" da porta.
/// - Trava a porta ate o puzzle ser resolvido
/// - Ativa Outline quando liberada
/// - Remove Outline quando a porta comecar a abrir
/// </summary>
public class PortaLock : MonoBehaviour
{
    [Header("Som")]
    [Tooltip("Som de vitoria tocado quando a porta e liberada.")]
    [SerializeField] private AudioClip somVitoria;
    [SerializeField] [Range(0f, 1f)] private float volumeSom = 1f;

    [Header("Outline")]
    [Tooltip("Largura do outline quando a porta esta liberada mas ainda fechada.")]
    [SerializeField] private float outlineWidth = 5f;

    [Header("Deteccao de abertura")]
    [Tooltip("Angulo minimo do HingeJoint para considerar que a porta comecou a abrir.")]
    [SerializeField] private float anguloAberturaMin = 5f;

    private XRGrabInteractable grab;
    private Rigidbody          rb;
    private HingeJoint         hinge;
    private Outline            outline;
    private AudioSource        audioSource;

    private bool portaLiberada = false;
    private bool outlineAtivo  = false;

    void Start()
    {
        grab    = GetComponent<XRGrabInteractable>();
        rb      = GetComponent<Rigidbody>();
        hinge   = GetComponent<HingeJoint>();
        outline = GetComponent<Outline>();

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake  = false;
        audioSource.spatialBlend = 1f;
        audioSource.volume       = volumeSom;

        // Garante outline desativado no inicio
        if (outline != null)
            outline.OutlineWidth = 0f;

        TravarPorta();
    }

    void Update()
    {
        // Quando liberada, monitora se a porta comeou a abrir
        if (portaLiberada && outlineAtivo && hinge != null)
        {
            // HingeJoint.angle retorna o angulo atual da dobradia
            if (Mathf.Abs(hinge.angle) > anguloAberturaMin)
                DesativarOutline();
        }
    }

    void TravarPorta()
    {
        if (grab != null) grab.enabled = false;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        Debug.Log("[PortaLock] Porta travada.");
    }

    /// <summary>
    /// Chamado pelo GeloDaTranca quando o gelo some.
    /// </summary>
    public void LiberarPorta()
    {
        portaLiberada = true;

        if (grab != null) grab.enabled = true;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.None;
        }

        // Toca som de vitoria
        if (audioSource != null && somVitoria != null)
            audioSource.PlayOneShot(somVitoria, volumeSom);

        // Ativa outline na porta
        AtivarOutline();

        Debug.Log("[PortaLock] Porta liberada!");
    }

    void AtivarOutline()
    {
        if (outline == null) return;
        outline.OutlineWidth = outlineWidth;
        outlineAtivo = true;
    }

    void DesativarOutline()
    {
        if (outline == null) return;
        outline.OutlineWidth = 0f;
        outlineAtivo = false;
        Debug.Log("[PortaLock] Porta aberta — outline removido.");
    }
}