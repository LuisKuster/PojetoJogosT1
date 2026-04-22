using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Anexe no objeto "Beaker water".
/// RN01: Volume do béquer aumenta proporcionalmente ao que sai dos tubos, respeitando volumeMaximo.
/// RN02: Cor do líquido é a média acumulada das cores dos ingredientes recebidos.
///       Mistura errada ainda dispara explosão; mistura certa ainda libera o puzzle.
/// RN03: Som de mistura toca enquanto recebe líquido e para quando para de receber.
/// </summary>
public class BeakerMixer : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Objeto filho 'Beaker liquid' — Y do scale representa o volume visível.")]
    [SerializeField] private Transform liquidTransform;

    [Tooltip("Renderer do 'Beaker liquid', para mudar a cor do material.")]
    [SerializeField] private Renderer liquidRenderer;

    [Header("Volume — RN01")]
    [Tooltip("Volume máximo do béquer. Deve ser >= soma dos volumeInicial dos tubos que serão usados. " +
             "Ex: 3 tubos com volumeInicial=1 → volumeMaximo=3.")]
    [SerializeField] private float volumeMaximo  = 3f;

    [Header("Combinação válida")]
    [Tooltip("Ingredientes que formam a mistura correta. Adicione exatamente os que devem ser usados.")]
    [SerializeField] private List<TestTubeIngredient.Ingrediente> ingredientesValidos
        = new List<TestTubeIngredient.Ingrediente>
        {
            TestTubeIngredient.Ingrediente.Azul,
            TestTubeIngredient.Ingrediente.Vermelho
        };

    [Header("Explosão")]
    [SerializeField] private float tempoAteReset = 3f;
    [SerializeField] private GameObject prefabFumaca;

    [Header("Som")]
    [SerializeField] private AudioClip somMisturando;
    [SerializeField] private AudioClip somExplosao;
    [SerializeField] private AudioClip somSucesso;
    [SerializeField] [Range(0f, 1f)] private float volumeSom = 1f;

    private AudioSource audioSource;

    // Tubos que foram usados nessa tentativa (para resetar se explodir)
    private List<TestTubeIngredient> tubosDerramados = new List<TestTubeIngredient>();

    // Acumula volume por ingrediente
    private Dictionary<TestTubeIngredient.Ingrediente, float> ingredientesRecebidos
        = new Dictionary<TestTubeIngredient.Ingrediente, float>();

    private float volumeAtual                  = 0f;
    private bool  explosaoJaDisparada          = false;
    private bool  misturaConcluidaComSucesso   = false;

    // RN02 — cor acumulada atual do béquer
    private Color corAtualLiquido = Color.clear;

    // RN03 — controle do som de mistura
    private float tempoSemReceber = 0f;
    private const float DELAY_PARAR_SOM = 0.15f; // para o som se ficar esse tempo sem receber líquido

    void Start()
    {
        if (liquidTransform != null)
        {
            liquidTransform.gameObject.SetActive(true);
            var s = liquidTransform.localScale;
            s.y = 0f;
            liquidTransform.localScale = s;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip         = somMisturando;
        audioSource.loop         = true;
        audioSource.playOnAwake  = false;
        audioSource.spatialBlend = 1f;
        audioSource.volume       = volumeSom;
    }

    void Update()
    {
        // RN03 — para o som se nenhum tubo estiver derramando
        if (audioSource != null && audioSource.isPlaying)
        {
            tempoSemReceber += Time.deltaTime;
            if (tempoSemReceber > DELAY_PARAR_SOM)
                audioSource.Stop();
        }
    }

    /// <summary>
    /// Chamado pelo TestTubeIngredient a cada frame enquanto derrama.
    /// </summary>
    public void ReceberIngrediente(TestTubeIngredient tubo, float volume)
    {
        if (explosaoJaDisparada || misturaConcluidaComSucesso) return;

        // RN03 — reinicia o contador pois recebeu líquido agora
        tempoSemReceber = 0f;
        if (audioSource != null && somMisturando != null && !audioSource.isPlaying)
            audioSource.Play();

        // Registra o tubo
        if (!tubosDerramados.Contains(tubo))
            tubosDerramados.Add(tubo);

        var tipo = tubo.GetIngrediente();

        if (ingredientesRecebidos.ContainsKey(tipo))
            ingredientesRecebidos[tipo] += volume;
        else
            ingredientesRecebidos[tipo] = volume;

        // RN01 — aumenta o volume do béquer respeitando o máximo
        float volumeAntes = volumeAtual;
        volumeAtual = Mathf.Min(volumeAtual + volume, volumeMaximo);
        float volumeEfetivo = volumeAtual - volumeAntes; // quanto realmente entrou

        if (volumeEfetivo > 0f)
        {
            AtualizarEscalaLiquido();

            // RN02 — atualiza a cor acumulada com base no volume efetivo que entrou
            AtualizarCorAcumulada(tubo.GetCorIngrediente(), volumeEfetivo);
        }

        AvaliarMistura();
    }

    // RN02 — média ponderada das cores pelo volume de cada ingrediente
    void AtualizarCorAcumulada(Color novaCor, float volumeNovo)
    {
        if (volumeAtual <= 0f) return;

        // Peso da cor nova = volume que acabou de entrar / volume total atual
        float pesoNovo     = volumeNovo / volumeAtual;
        float pesoAnterior = 1f - pesoNovo;

        corAtualLiquido = corAtualLiquido * pesoAnterior + novaCor * pesoNovo;
        AplicarCorLiquido(corAtualLiquido);
    }

    void AvaliarMistura()
    {
        // Só avalia quando tiver mais de um ingrediente
        if (ingredientesRecebidos.Count < 2) return;

        // Verifica se os ingredientes recebidos batem exatamente com os válidos
        bool misturaCorreta = ingredientesRecebidos.Count == ingredientesValidos.Count;
        if (misturaCorreta)
        {
            foreach (var valido in ingredientesValidos)
            {
                if (!ingredientesRecebidos.ContainsKey(valido))
                {
                    misturaCorreta = false;
                    break;
                }
            }
        }

        if (misturaCorreta && !misturaConcluidaComSucesso)
        {
            misturaConcluidaComSucesso = true;

            if (audioSource != null && somSucesso != null)
                audioSource.PlayOneShot(somSucesso, volumeSom);

            Debug.Log("[BeakerMixer] Mistura válida! Leve o béquer até o gelo.");
            return;
        }

        // Se tem ingredientes errados (não estão na lista de válidos) → explosão
        if (!misturaConcluidaComSucesso)
        {
            foreach (var kv in ingredientesRecebidos)
            {
                if (!ingredientesValidos.Contains(kv.Key))
                {
                    if (!explosaoJaDisparada)
                    {
                        explosaoJaDisparada = true;
                        StartCoroutine(SequenciaExplosao());
                    }
                    return;
                }
            }
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
        corAtualLiquido            = Color.clear;

        AtualizarEscalaLiquido();

        // Volta o líquido transparente
        if (liquidRenderer != null)
            liquidRenderer.material.color = Color.clear;

        Debug.Log("[BeakerMixer] Resetado. Pode tentar de novo!");
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

    /// <summary>Consultado pelo BeakerPourer — só derrete o gelo se mistura estiver certa.</summary>
    public bool MisturaValida() => misturaConcluidaComSucesso;
}