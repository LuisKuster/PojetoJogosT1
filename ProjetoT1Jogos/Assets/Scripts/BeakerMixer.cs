using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Anexe no "Beaker water".
/// - Derramamento contínuo sem travar no meio
/// - Cor do béquer muda progressivamente conforme os tubos entram
/// - Som de borbulha quando os 3 corretos completam
/// - Som de explosão + reset quando o 3º completa com pelo menos 1 errado
/// </summary>
public class BeakerMixer : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Transform liquidTransform;
    [SerializeField] private Renderer  liquidRenderer;

    [Header("Altura do líquido")]
    [SerializeField] private float alturaMaxima = 1.5f;

    [Header("Som")]
    [Tooltip("Toca quando os 3 elementos corretos são adicionados.")]
    [SerializeField] private AudioClip somBorbulha;
    [Tooltip("Toca quando o 3º elemento é adicionado e pelo menos 1 é errado.")]
    [SerializeField] private AudioClip somExplosao;
    [SerializeField] [Range(0f, 1f)] private float volumeSom = 1f;

    [Header("Reset")]
    [SerializeField] private float tempoAteReset = 5f;

    private static readonly HashSet<TestTubeIngredient.Ingrediente> ACEITOS
        = new HashSet<TestTubeIngredient.Ingrediente>
        {
            TestTubeIngredient.Ingrediente.Azul,
            TestTubeIngredient.Ingrediente.Vermelho,
            TestTubeIngredient.Ingrediente.Amarelo,
        };

    private AudioSource audioSource;

    // Conjunto de tubos completamente derramados (corretos)
    private HashSet<TestTubeIngredient.Ingrediente> corretosCompletos
        = new HashSet<TestTubeIngredient.Ingrediente>();

    // Conjunto de todos os tipos que já foram completados (corretos + errados)
    private HashSet<TestTubeIngredient.Ingrediente> todosCompletos
        = new HashSet<TestTubeIngredient.Ingrediente>();

    // Volume acumulado por tipo de ingrediente
    private Dictionary<TestTubeIngredient.Ingrediente, float> volumeAcumulado
        = new Dictionary<TestTubeIngredient.Ingrediente, float>();

    // Volume total de cada tubo (capturado na primeira vez)
    private Dictionary<TestTubeIngredient.Ingrediente, float> volumeDoTubo
        = new Dictionary<TestTubeIngredient.Ingrediente, float>();

    // Todos os tubos usados (para resetar se errar)
    private List<TestTubeIngredient> tubosDerramados
        = new List<TestTubeIngredient>();

    private bool temIngredienteErrado = false;
    private bool misturaValida        = false;
    private bool jaAvaliou            = false;

    void Start()
    {
        // Fallback — busca o Renderer do liquidTransform se o campo estiver vazio
        if (liquidRenderer == null && liquidTransform != null)
        {
            liquidRenderer = liquidTransform.GetComponent<Renderer>();
            Debug.Log($"[BeakerMixer] Renderer buscado automaticamente: {liquidRenderer != null}");
        }

        if (liquidTransform != null)
        {
            liquidTransform.gameObject.SetActive(true);
            SetAltura(0f);
        }

        if (liquidRenderer != null)
            SetCorLiquido(Color.clear);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake  = false;
        audioSource.spatialBlend = 1f;
        audioSource.volume       = volumeSom;
    }

    public void ReceberIngrediente(TestTubeIngredient tubo, float volumeParcial)
    {
        if (misturaValida || jaAvaliou) return;

        var tipo = tubo.GetIngrediente();

        // Registra tubo usado
        if (!tubosDerramados.Contains(tubo))
            tubosDerramados.Add(tubo);

        // Se esse tipo já foi completado, ignora
        if (todosCompletos.Contains(tipo)) return;

        // Captura volume total do tubo na primeira vez
        if (!volumeDoTubo.ContainsKey(tipo))
            volumeDoTubo[tipo] = tubo.GetVolumeInicial();

        // Acumula volume — sem checar jaEraCompleto antes, deixa fluir sempre
        if (!volumeAcumulado.ContainsKey(tipo))
            volumeAcumulado[tipo] = 0f;

        volumeAcumulado[tipo] = Mathf.Min(
            volumeAcumulado[tipo] + volumeParcial,
            volumeDoTubo[tipo]   // nunca passa do total do tubo
        );

        // Atualiza visual (altura + cor) a cada frame
        AtualizarVisual();

        // Checa se esse tubo foi completamente derramado (>= 95%)
        if (volumeAcumulado[tipo] >= volumeDoTubo[tipo] * 0.95f)
        {
            todosCompletos.Add(tipo);

            if (ACEITOS.Contains(tipo))
                corretosCompletos.Add(tipo);
            else
                temIngredienteErrado = true;

            Debug.Log($"[BeakerMixer] {tipo} completo! Totais: {todosCompletos.Count}/3");

            // Avalia quando 3 tubos foram completados
            if (todosCompletos.Count >= 3)
                AvaliarResultado();
        }
    }

    void AvaliarResultado()
    {
        jaAvaliou = true;

        if (!temIngredienteErrado && corretosCompletos.Count == 3)
        {
            misturaValida = true;
            SetAltura(alturaMaxima);
            TocarSom(somBorbulha);
            Debug.Log("[BeakerMixer] Mistura correta! Leve o béquer ao gelo.");
        }
        else
        {
            TocarSom(somExplosao);
            Debug.Log($"[BeakerMixer] Mistura errada! Resetando em {tempoAteReset}s...");
            StartCoroutine(ResetarApos(tempoAteReset));
        }
    }

    void AtualizarVisual()
    {
        // --- ALTURA ---
        float alturaTotal = 0f;
        foreach (var kv in volumeAcumulado)
        {
            if (!volumeDoTubo.ContainsKey(kv.Key)) continue;
            float progresso = Mathf.Clamp01(kv.Value / volumeDoTubo[kv.Key]);
            alturaTotal += (progresso / 3f) * alturaMaxima;
        }
        SetAltura(Mathf.Min(alturaTotal, alturaMaxima));

        // --- COR ---
        // Média simples das cores dos ingredientes que já têm volume > 0
        int   count = 0;
        float r = 0, g = 0, b = 0;

        foreach (var kv in volumeAcumulado)
        {
            if (kv.Value <= 0f) continue;
            Color cor = GetCorDoIngrediente(kv.Key);
            r += cor.r;
            g += cor.g;
            b += cor.b;
            count++;
        }

        if (count > 0 && liquidRenderer != null)
        {
            Color corMisturada = new Color(r / count, g / count, b / count, 1f);
            // URP usa _BaseColor, fallback para _Color se não encontrar
            if (liquidRenderer.material.HasProperty("_BaseColor"))
                liquidRenderer.material.SetColor("_BaseColor", corMisturada);
            else
                liquidRenderer.material.SetColor("_Color", corMisturada);
        }
    }

    void SetAltura(float y)
    {
        if (liquidTransform == null) return;
        Vector3 e = liquidTransform.localScale;
        e.y = y;
        liquidTransform.localScale = e;
    }

    void TocarSom(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        audioSource.PlayOneShot(clip, volumeSom);
    }

    IEnumerator ResetarApos(float segundos)
    {
        yield return new WaitForSeconds(segundos);
        ResetarTudo();
    }

    void ResetarTudo()
    {
        foreach (var tubo in tubosDerramados)
            if (tubo != null) tubo.Resetar();

        tubosDerramados.Clear();
        corretosCompletos.Clear();
        todosCompletos.Clear();
        volumeAcumulado.Clear();
        volumeDoTubo.Clear();

        temIngredienteErrado = false;
        misturaValida        = false;
        jaAvaliou            = false;

        SetAltura(0f);

        if (liquidRenderer != null)
            SetCorLiquido(Color.clear);
    }

    void SetCorLiquido(Color cor)
    {
        if (liquidRenderer == null) return;

        if (liquidRenderer.material.HasProperty("_BaseColor"))
            liquidRenderer.material.SetColor("_BaseColor", cor);
        else
            liquidRenderer.material.SetColor("_Color", cor);

        // Atualiza também a Emission pra cor combinada ficar correta
        if (liquidRenderer.material.HasProperty("_EmissionColor"))
            liquidRenderer.material.SetColor("_EmissionColor", cor * 0.5f);
    }

    Color GetCorDoIngrediente(TestTubeIngredient.Ingrediente tipo)
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

    public bool MisturaValida() => misturaValida;
}