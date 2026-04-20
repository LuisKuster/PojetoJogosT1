using UnityEngine;

/// <summary>
/// Anexe no "Beaker water".
/// Só derrete o gelo se a mistura estiver correta (BeakerMixer.MisturaValida()).
/// </summary>
public class BeakerPourer : MonoBehaviour
{
    [Header("Som")]
    [Tooltip("Som de líquido saindo do béquer enquanto está inclinado no gelo.")]
    [SerializeField] private AudioClip somDerramando;
    [SerializeField] [Range(0f, 1f)] private float volumeSom = 1f;

    [Header("Derramamento")]
    [SerializeField] private float anguloMinDerramar = 100f;
    [SerializeField] private float velocidadeDerramar = 0.5f;

    private AudioSource audioSource;
    private BeakerMixer beakerMixer;
    private GeloDaTranca geloAtual  = null;
    private bool inZonaGelo         = false;
    private bool jaDerramouTudo     = false;

    void Start()
    {
        beakerMixer = GetComponent<BeakerMixer>();

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip         = somDerramando;
        audioSource.loop         = true;
        audioSource.playOnAwake  = false;
        audioSource.spatialBlend = 1f;
        audioSource.volume       = volumeSom;
    }

    void Update()
    {
        if (!inZonaGelo || jaDerramouTudo || geloAtual == null)
        {
            PararSom();
            return;
        }

        // Só age se a mistura estiver correta
        if (beakerMixer == null || !beakerMixer.MisturaValida())
        {
            PararSom();
            return;
        }

        float angulo = Vector3.Angle(transform.up, Vector3.up);

        if (angulo > anguloMinDerramar)
        {
            TocarSom();
            float reducao  = velocidadeDerramar * Time.deltaTime;
            bool  terminou = geloAtual.ReceberLiquido(reducao);
            if (terminou)
            {
                jaDerramouTudo = true;
                PararSom();
            }
        }
        else
        {
            PararSom();
        }
    }

    void TocarSom()
    {
        if (audioSource == null || somDerramando == null) return;
        if (!audioSource.isPlaying) audioSource.Play();
    }

    void PararSom()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ZonaDerremarGelo"))
        {
            geloAtual  = other.GetComponentInParent<GeloDaTranca>();
            inZonaGelo = true;
            Debug.Log("[BeakerPourer] Béquer entrou na zona do gelo.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ZonaDerremarGelo"))
        {
            inZonaGelo = false;
            geloAtual  = null;
            PararSom();
            Debug.Log("[BeakerPourer] Béquer saiu da zona do gelo.");
        }
    }
}