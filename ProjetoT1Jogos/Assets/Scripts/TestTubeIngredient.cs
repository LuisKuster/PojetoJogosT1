using UnityEngine;

/// <summary>
/// Anexe em cada "Glass_Lab_test_tube with liquid".
/// </summary>
public class TestTubeIngredient : MonoBehaviour
{
    public enum Ingrediente
    {
        Vermelho,
        Rosa,
        Verde,
        Preto,
        Azul,
        Amarelo
    }

    [Header("Ingrediente")]
    [SerializeField] private Ingrediente ingrediente;

    [Header("Referências")]
    [Tooltip("Objeto filho 'Glass_Lab_test_tube6' — diminuir Y do scale = menos líquido.")]
    [SerializeField] private Transform liquidTransform;

    [Tooltip("Referência ao BeakerMixer do copo de béquer na cena.")]
    [SerializeField] private BeakerMixer beakerMixer;

    [Header("Derramamento")]
    [SerializeField] private float anguloMinDerramar  = 100f;
    [SerializeField] private float velocidadeDerramar = 0.3f;
    [SerializeField] private float volumeInicial      = 1f;

    [Header("Som")]
    [Tooltip("Som de líquido saindo do tubo. Toca em loop enquanto o tubo está inclinado na ZonaDerramar.")]
    [SerializeField] private AudioClip somDerramando;
    [SerializeField] [Range(0f, 1f)] private float volumeSom = 1f;

    private AudioSource audioSource;
    private bool  inZonaDerramar = false;
    private bool  jaDerramouTudo = false;
    private float volumeAtual;

    void Start()
    {
        volumeAtual = volumeInicial;
        AtualizarEscalaLiquido();

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip         = somDerramando;
        audioSource.loop         = true;
        audioSource.playOnAwake  = false;
        audioSource.spatialBlend = 1f;
        audioSource.volume       = volumeSom;
    }

    void Update()
    {
        if (!inZonaDerramar || jaDerramouTudo)
        {
            PararSom();
            return;
        }

        float angulo = Vector3.Angle(transform.up, Vector3.up);

        if (angulo > anguloMinDerramar)
        {
            TocarSom();
            Derramar();
        }
        else
        {
            PararSom();
        }
    }

    void Derramar()
    {
        float reducao = velocidadeDerramar * Time.deltaTime;
        volumeAtual -= reducao;

        if (volumeAtual <= 0f)
        {
            volumeAtual    = 0f;
            jaDerramouTudo = true;
            PararSom();

            if (beakerMixer != null)
                beakerMixer.ReceberIngrediente(this, volumeInicial);
        }
        else
        {
            if (beakerMixer != null)
                beakerMixer.ReceberIngrediente(this, reducao);
        }

        AtualizarEscalaLiquido();
    }

    /// <summary>
    /// Chamado pelo BeakerMixer quando a mistura deu errado.
    /// Devolve o tubo ao estado original.
    /// </summary>
    public void Resetar()
    {
        volumeAtual    = volumeInicial;
        jaDerramouTudo = false;
        inZonaDerramar = false;
        PararSom();
        AtualizarEscalaLiquido();
        Debug.Log($"[TestTubeIngredient] {ingrediente} resetado.");
    }

    void TocarSom()
    {
        if (audioSource == null || somDerramando == null) return;
        if (!audioSource.isPlaying)
            audioSource.Play();
    }

    void PararSom()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    void AtualizarEscalaLiquido()
    {
        if (liquidTransform == null) return;
        Vector3 escala = liquidTransform.localScale;
        escala.y = volumeAtual;
        liquidTransform.localScale = escala;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ZonaDerramar"))
        {
            inZonaDerramar = true;
            Outline outline = gameObject.GetComponent<Outline>();
            if (outline != null) outline.OutlineWidth = 5f;
            Debug.Log($"[TestTubeIngredient] {ingrediente} entrou na ZonaDerramar.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ZonaDerramar"))
        {
            inZonaDerramar = false;
            Outline outline = gameObject.GetComponent<Outline>();
            if (outline != null) outline.OutlineWidth = 0f;
            Debug.Log($"[TestTubeIngredient] {ingrediente} saiu da ZonaDerramar.");
        }
    }

    public Ingrediente GetIngrediente() => ingrediente;
    public bool TemLiquido() => volumeAtual > 0f;
}