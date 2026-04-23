using UnityEngine;

/// <summary>
/// Anexe em cada "Glass_Lab_test_tube with liquid".
/// </summary>
public class TestTubeIngredient : MonoBehaviour
{
    public enum Ingrediente { Vermelho, Rosa, Verde, Preto, Azul, Amarelo }

    [Header("Ingrediente")]
    [SerializeField] private Ingrediente ingrediente;

    [Header("Referências")]
    [SerializeField] private Transform  liquidTransform;
    [SerializeField] private BeakerMixer beakerMixer;

    [Header("Derramamento")]
    [SerializeField] private float volumeInicial      = 1f;
    [SerializeField] private float velocidadeDerramar = 0.3f;
    [SerializeField] private float anguloMinDerramar  = 100f;

    [Header("Som")]
    [SerializeField] private AudioClip somDerramando;
    [SerializeField] [Range(0f, 1f)] private float volumeSom = 1f;

    private AudioSource audioSource;
    private bool  jaDerramouTudo  = false;
    private float volumeAtual;

    // Conta quantos colliders da ZonaDerramar estão se sobrepondo ao tubo.
    // Usar contador em vez de bool evita que OnTriggerExit de um collider
    // cancele a zona quando ainda há outro collider ativo.
    private int zonaCount = 0;
    private bool InZona => zonaCount > 0;

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
        if (!InZona || jaDerramouTudo)
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

        // Se o volume restante for pequeno, derrama tudo de uma vez
        // evita ficar preso com gotinhas que nunca atingem 0
        if (volumeAtual - reducao < volumeInicial * 0.05f)
            reducao = volumeAtual;

        reducao     = Mathf.Min(reducao, volumeAtual);
        volumeAtual -= reducao;

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

    public void Resetar()
    {
        volumeAtual    = volumeInicial;
        jaDerramouTudo = false;
        zonaCount      = 0;
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

    void AtualizarEscalaLiquido()
    {
        if (liquidTransform == null) return;
        Vector3 e = liquidTransform.localScale;
        e.y = volumeAtual;
        liquidTransform.localScale = e;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ZonaDerramar"))
        {
            zonaCount++;
            Outline o = GetComponent<Outline>();
            if (o != null) o.OutlineWidth = 5f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ZonaDerramar"))
        {
            zonaCount = Mathf.Max(0, zonaCount - 1);
            if (zonaCount == 0)
            {
                Outline o = GetComponent<Outline>();
                if (o != null) o.OutlineWidth = 0f;
            }
        }
    }

    public Ingrediente GetIngrediente() => ingrediente;
    public bool  TemLiquido()           => volumeAtual > 0f;
    public float GetVolumeInicial()     => volumeInicial;

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