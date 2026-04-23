using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeakerMixer : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform liquidTransform;
    [SerializeField] private Renderer  liquidRenderer;

    [Header("Altura do liquido")]
    [SerializeField] private float alturaMaxima = 1.5f;

    [Header("Som")]
    [SerializeField] private AudioClip somBorbulha;
    [SerializeField] private AudioClip somExplosao;
    [SerializeField] [Range(0f, 1f)] private float volumeSom = 1f;

    [Header("Efeitos")]
    [Tooltip("Prefab de fumaca instanciado quando a mistura da errado.")]
    [SerializeField] private GameObject prefabFumacaErro;
    [Tooltip("Prefab de fumaca instanciado quando o bequer e usado no gelo.")]
    [SerializeField] private GameObject prefabFumacaGelo;
    [SerializeField] private float tempoFumaca = 2f;

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

    private HashSet<TestTubeIngredient.Ingrediente> corretosCompletos
        = new HashSet<TestTubeIngredient.Ingrediente>();

    private HashSet<TestTubeIngredient.Ingrediente> todosCompletos
        = new HashSet<TestTubeIngredient.Ingrediente>();

    private Dictionary<TestTubeIngredient.Ingrediente, float> volumeAcumulado
        = new Dictionary<TestTubeIngredient.Ingrediente, float>();

    private Dictionary<TestTubeIngredient.Ingrediente, float> volumeDoTubo
        = new Dictionary<TestTubeIngredient.Ingrediente, float>();

    private List<TestTubeIngredient> tubosDerramados
        = new List<TestTubeIngredient>();

    private bool temIngredienteErrado = false;
    private bool misturaValida        = false;
    private bool jaAvaliou            = false;

    void Start()
    {
        if (liquidRenderer == null && liquidTransform != null)
            liquidRenderer = liquidTransform.GetComponent<Renderer>();

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

        if (!tubosDerramados.Contains(tubo))
            tubosDerramados.Add(tubo);

        if (todosCompletos.Contains(tipo)) return;

        if (!volumeDoTubo.ContainsKey(tipo))
            volumeDoTubo[tipo] = tubo.GetVolumeInicial();

        if (!volumeAcumulado.ContainsKey(tipo))
            volumeAcumulado[tipo] = 0f;

        volumeAcumulado[tipo] = Mathf.Min(
            volumeAcumulado[tipo] + volumeParcial,
            volumeDoTubo[tipo]
        );

        AtualizarVisual();

        if (volumeAcumulado[tipo] >= volumeDoTubo[tipo] * 0.95f)
        {
            todosCompletos.Add(tipo);

            if (ACEITOS.Contains(tipo))
                corretosCompletos.Add(tipo);
            else
                temIngredienteErrado = true;

            Debug.Log($"[BeakerMixer] {tipo} completo! Totais: {todosCompletos.Count}/3");

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
            Debug.Log("[BeakerMixer] Mistura correta! Leve o bequer ao gelo.");
        }
        else
        {
            TocarSom(somExplosao);

            // Fumaca de erro na posicao do bequer
            InstanciarFumaca(prefabFumacaErro, transform.position);

            Debug.Log($"[BeakerMixer] Mistura errada! Resetando em {tempoAteReset}s...");
            StartCoroutine(ResetarApos(tempoAteReset));
        }
    }

    /// <summary>
    /// Chamado pelo BeakerPourer quando o bequer toca o gelo com mistura correta.
    /// Spawna o efeito de fumaca/vapor no gelo.
    /// </summary>
    public void InstanciarEfeitoGelo(Vector3 posicao)
    {
        InstanciarFumaca(prefabFumacaGelo, posicao);
    }

    void InstanciarFumaca(GameObject prefab, Vector3 posicao)
    {
        if (prefab == null) return;
        GameObject efeito = Instantiate(prefab, posicao, Quaternion.identity);
        Destroy(efeito, tempoFumaca);
    }

    void AtualizarVisual()
    {
        // Altura
        float alturaTotal = 0f;
        foreach (var kv in volumeAcumulado)
        {
            if (!volumeDoTubo.ContainsKey(kv.Key)) continue;
            float progresso = Mathf.Clamp01(kv.Value / volumeDoTubo[kv.Key]);
            alturaTotal += (progresso / 3f) * alturaMaxima;
        }
        SetAltura(Mathf.Min(alturaTotal, alturaMaxima));

        // Cor media dos ingredientes com volume > 0
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

        if (count > 0)
        {
            Color corMisturada = new Color(r / count, g / count, b / count, 1f);
            SetCorLiquido(corMisturada);
        }
    }

    void SetAltura(float y)
    {
        if (liquidTransform == null) return;
        Vector3 e = liquidTransform.localScale;
        e.y = y;
        liquidTransform.localScale = e;
    }

    void SetCorLiquido(Color cor)
    {
        if (liquidRenderer == null) return;
        if (liquidRenderer.material.HasProperty("_BaseColor"))
            liquidRenderer.material.SetColor("_BaseColor", cor);
        else
            liquidRenderer.material.SetColor("_Color", cor);

        if (liquidRenderer.material.HasProperty("_EmissionColor"))
            liquidRenderer.material.SetColor("_EmissionColor", cor * 0.5f);
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
        SetCorLiquido(Color.clear);

        Debug.Log("[BeakerMixer] Resetado. Pode tentar de novo!");
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