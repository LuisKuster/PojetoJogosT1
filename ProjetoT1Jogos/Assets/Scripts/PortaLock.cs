using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PortaLock : MonoBehaviour
{
    [Header("Som")]
    [SerializeField] private AudioClip somVitoria;
    [SerializeField] [Range(0f, 1f)] private float volumeSom = 1f;

    [Header("Outline")]
    [SerializeField] private float outlineWidth = 5f;

    [Header("Deteccao de abertura")]
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

        if (outline != null)
            outline.OutlineWidth = 0f;

        TravarPorta();
    }

    void Update()
    {
        if (portaLiberada && outlineAtivo && hinge != null)
            if (Mathf.Abs(hinge.angle) > anguloAberturaMin)
                DesativarOutline();
    }

    void TravarPorta()
    {
        if (grab != null) grab.enabled = false;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    public void LiberarPorta()
    {
        portaLiberada = true;

        if (grab != null) grab.enabled = true;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.None;
        }

        if (audioSource != null && somVitoria != null)
            audioSource.PlayOneShot(somVitoria, volumeSom);

        AtivarOutline();
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
    }
}