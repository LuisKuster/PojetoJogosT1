using UnityEngine;

/// <summary>
/// Anexe em cada "Glass_Lab_test_tube with liquid".
/// RN01: Volume do tubo diminui proporcionalmente ao que entra no béquer.
/// RN03: Som inicia ao derramar e para ao endireitar/sair da zona.
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
    [Tooltip("Objeto filho 'Glass_Lab_test_tube6' — o Y do scale representa o volume visível.")]
    [SerializeField] private Transform liquidTransform;

    [Tooltip("Referência ao BeakerMixer do copo de béquer na cena.")]
    [SerializeField] private BeakerMixer beakerMixer;

    [Header("Volume — RN01")]
    [Tooltip("Volume inicial do tubo (Y do scale do liquid). " +
             "IMPORTANTE: a soma dos volumeInicial de todos os tubos que serão derramados " +
             "não deve ultrapassar o volumeMaximo configurado no BeakerMixer.")]
    [SerializeField] private float volumeInicial      = 1f;
    [Tooltip("Velocidade com que o líquido sai do tubo (unidades de scale/segundo).")]
    [SerializeField] private float velocidadeDerramar = 0.3f;
    [Tooltip("Ângulo mínimo (graus) entre o eixo up do tubo e Vector3.up para considerar 'virado'.")]
    [SerializeField] private float anguloMinDerramar  = 100f;

    [Header("Som — RN03")]
    [Tooltip("Som de líquido saindo do tubo. Toca em loop enquanto inclinado na ZonaDerramar.")]
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

        // RN03 — AudioSource configurado em loop
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
            PararSom(); // RN03 — para o som ao sair da zona ou terminar
            return;
        }

        float angulo = Vector3.Angle(transform.up, Vector3.up);

        if (angulo > anguloMinDerramar)
        {
            TocarSom(); // RN03 — inicia o som ao derramar
            Derramar();
        }
        else
        {
            PararSom(); // RN03 — para o som ao endireitar o tubo
        }
    }

    void Derramar()
    {
        // RN01 — reduz o volume do tubo proporcionalmente por frame
        float reducao = velocidadeDerramar * Time.deltaTime;

        // Garante que não vai abaixo de zero
        reducao     = Mathf.Min(reducao, volumeAtual);
        volumeAtual -= reducao;

        // Notifica o béquer com o volume exato que saiu
        if (beakerMixer != null)
            beakerMixer.ReceberIngrediente(this, reducao);

        if (volumeAtual <= 0f)
        {
            volumeAtual    = 0f;
            jaDerramouTudo = true;
            PararSom();
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
        if (!audioSource.isPlaying) audioSource.Play();
    }

    void PararSom()
    {
        if (audioSource != null && audioSource.isPlaying) audioSource.Stop();
    }

    // RN01 — atualiza o Y do scale do objeto filho para refletir o volume atual
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
            Outline outline = GetComponent<Outline>();
            if (outline != null) outline.OutlineWidth = 5f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ZonaDerramar"))
        {
            inZonaDerramar = false;
            Outline outline = GetComponent<Outline>();
            if (outline != null) outline.OutlineWidth = 0f;
        }
    }

    public Ingrediente GetIngrediente() => ingrediente;
    public bool TemLiquido()            => volumeAtual > 0f;

    /// <summary>Retorna a cor correspondente ao ingrediente desse tubo.</summary>
    public Color GetCorIngrediente()
    {
        return ingrediente switch
        {
            Ingrediente.Vermelho => Color.red,
            Ingrediente.Rosa     => new Color(1f, 0.41f, 0.71f),
            Ingrediente.Verde    => Color.green,
            Ingrediente.Preto    => Color.black,
            Ingrediente.Azul     => Color.blue,
            Ingrediente.Amarelo  => Color.yellow,
            _                    => Color.white,
        };
    }
}