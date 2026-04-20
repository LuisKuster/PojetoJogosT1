using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Anexe no objeto "Beaker water".
/// O béquer sempre pode ser pego. O BeakerPourer só derrete o gelo se a mistura estiver correta.
/// </summary>
public class BeakerMixer : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Objeto filho 'Beaker liquid' — aumentar Y do scale = mais líquido visível.")]
    [SerializeField] private Transform liquidTransform;

    [Tooltip("Renderer do 'Beaker liquid', para mudar a cor do material.")]
    [SerializeField] private Renderer liquidRenderer;

    [Header("Volume de líquido")]
    [SerializeField] private float volumeMaximo  = 3f;
    [SerializeField] private float volumeInicial = 0f;

    [Header("Cores das misturas")]
    [SerializeField] private Color corMisturaValida   = new Color(0.5f, 0f, 0.8f);
    [SerializeField] private Color corMisturaInvalida = new Color(0.2f, 0.1f, 0f);

    [Header("Explosão")]
    [SerializeField] private float tempoAteReset = 3f;
    [SerializeField] private GameObject prefabFumaca;

    [Header("Som")]
    [SerializeField] private AudioClip somMisturando;
    [SerializeField] private AudioClip somExplosao;
    [Tooltip("Som tocado quando a mistura correta é concluída.")]
    [SerializeField] private AudioClip somSucesso;
    [SerializeField] [Range(0f, 1f)] private float volumeSom = 1f;

    private AudioSource audioSource;
    private List<TestTubeIngredient> tubosDerramados = new List<TestTubeIngredient>();
    private Dictionary<TestTubeIngredient.Ingrediente, float> ingredientesRecebidos
        = new Dictionary<TestTubeIngredient.Ingrediente, float>();

    private float volumeAtual;
    private bool  explosaoJaDisparada          = false;
    private bool  misturaConcluidaComSucesso   = false;
    private Color corOriginalLiquido;

    void Start()
    {
        volumeAtual = volumeInicial;

        if (liquidTransform != null)
        {
            liquidTransform.gameObject.SetActive(true);
            var s = liquidTransform.localScale;
            s.y = 0f;
            liquidTransform.localScale = s;
        }

        if (liquidRenderer != null)
            corOriginalLiquido = liquidRenderer.material.color;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake  = false;
        audioSource.spatialBlend = 1f;
        audioSource.volume       = volumeSom;

        // Grab sempre habilitado — o jogador pode pegar o béquer a qualquer momento
        // A restrição de derreter o gelo fica no BeakerPourer (checa MisturaValida())
    }

    public void ReceberIngrediente(TestTubeIngredient tubo, float volume)
    {
        if (explosaoJaDisparada || misturaConcluidaComSucesso) return;

        if (!tubosDerramados.Contains(tubo))
            tubosDerramados.Add(tubo);

        var tipo = tubo.GetIngrediente();

        if (ingredientesRecebidos.ContainsKey(tipo))
            ingredientesRecebidos[tipo] += volume;
        else
            ingredientesRecebidos[tipo] = volume;

        volumeAtual = Mathf.Min(volumeAtual + volume, volumeMaximo);
        AtualizarEscalaLiquido();
        TocarSomMistura();
        AvaliarMistura();
    }

    void AvaliarMistura()
    {
        bool temAzul     = ingredientesRecebidos.ContainsKey(TestTubeIngredient.Ingrediente.Azul);
        bool temVermelho = ingredientesRecebidos.ContainsKey(TestTubeIngredient.Ingrediente.Vermelho);
        int  total       = ingredientesRecebidos.Count;

        if (temAzul && temVermelho && total == 2)
        {
            AplicarCorLiquido(corMisturaValida);

            if (!misturaConcluidaComSucesso)
            {
                misturaConcluidaComSucesso = true;

                // Toca som de sucesso
                if (audioSource != null && somSucesso != null)
                    audioSource.PlayOneShot(somSucesso, volumeSom);

                Debug.Log("[BeakerMixer] Mistura válida! Leve o béquer até o gelo.");
            }
            return;
        }

        if (total == 1)
        {
            foreach (var kv in ingredientesRecebidos)
            {
                AplicarCorLiquido(ObterCorDoIngrediente(kv.Key));
                break;
            }
            return;
        }

        AplicarCorLiquido(corMisturaInvalida);
        Debug.Log("[BeakerMixer] Mistura inválida!");

        if (!explosaoJaDisparada)
        {
            explosaoJaDisparada = true;
            StartCoroutine(SequenciaExplosao());
        }
    }

    IEnumerator SequenciaExplosao()
    {
        if (audioSource != null && somExplosao != null)
            audioSource.PlayOneShot(somExplosao, volumeSom);

        if (prefabFumaca != null)
        {
            GameObject fumaca = Instantiate(prefabFumaca, transform.position, Quaternion.identity);
            Destroy(fumaca, tempoAteReset + 1f);
        }

        Debug.Log($"[BeakerMixer] Explosão! Resetando em {tempoAteReset}s...");
        yield return new WaitForSeconds(tempoAteReset);
        ResetarTudo();
    }

    void ResetarTudo()
    {
        foreach (var tubo in tubosDerramados)
            if (tubo != null) tubo.Resetar();

        tubosDerramados.Clear();
        ingredientesRecebidos.Clear();
        volumeAtual                = 0f;
        explosaoJaDisparada        = false;
        misturaConcluidaComSucesso = false;

        AtualizarEscalaLiquido();
        AplicarCorLiquido(corOriginalLiquido);

        Debug.Log("[BeakerMixer] Resetado. Pode tentar de novo!");
    }

    void TocarSomMistura()
    {
        if (audioSource == null || somMisturando == null) return;
        if (!audioSource.isPlaying)
            audioSource.PlayOneShot(somMisturando, volumeSom);
    }

    void AtualizarEscalaLiquido()
    {
        if (liquidTransform == null) return;
        Vector3 escala = liquidTransform.localScale;
        escala.y = volumeAtual;
        liquidTransform.localScale = escala;
    }

    void AplicarCorLiquido(Color cor)
    {
        if (liquidRenderer == null) return;
        liquidRenderer.material.color = cor;
    }

    Color ObterCorDoIngrediente(TestTubeIngredient.Ingrediente tipo)
    {
        return tipo switch
        {
            TestTubeIngredient.Ingrediente.Vermelho => Color.red,
            TestTubeIngredient.Ingrediente.Rosa     => new Color(1f, 0.41f, 0.71f),
            TestTubeIngredient.Ingrediente.Verde    => Color.green,
            TestTubeIngredient.Ingrediente.Preto    => Color.black,
            TestTubeIngredient.Ingrediente.Azul     => Color.blue,
            TestTubeIngredient.Ingrediente.Amarelo  => Color.yellow,
            _                                       => Color.white,
        };
    }

    /// <summary>
    /// Consultado pelo BeakerPourer para saber se pode derreter o gelo.
    /// </summary>
    public bool MisturaValida() => misturaConcluidaComSucesso;
}