using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Anexe no objeto "Beaker water".
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
    [SerializeField] private Color corMisturaValida   = new Color(0.5f, 0f, 0.8f); // roxo
    [SerializeField] private Color corMisturaInvalida = new Color(0.2f, 0.1f, 0f); // marrom escuro

    [Header("Som")]
    [Tooltip("Som de borbulha/gota que toca cada vez que líquido cai no béquer.")]
    [SerializeField] private AudioClip somMisturando;
    [Tooltip("Som de estouro que toca quando a mistura é inválida.")]
    [SerializeField] private AudioClip somExplosao;
    [SerializeField] [Range(0f, 1f)] private float volumeSom = 1f;

    private AudioSource audioSource;
    private Dictionary<TestTubeIngredient.Ingrediente, float> ingredientesRecebidos
        = new Dictionary<TestTubeIngredient.Ingrediente, float>();
    private float volumeAtual;
    private bool  explosaoJaDisparada = false;

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

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake  = false;
        audioSource.spatialBlend = 1f; // som 3D — sai da posição do béquer
        audioSource.volume       = volumeSom;
    }

    public void ReceberIngrediente(TestTubeIngredient.Ingrediente tipo, float volume)
    {
        if (explosaoJaDisparada) return;

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

        // Mistura válida: apenas Azul + Vermelho
        if (temAzul && temVermelho && total == 2)
        {
            AplicarCorLiquido(corMisturaValida);
            Debug.Log("[BeakerMixer] Mistura válida! Azul + Vermelho = Roxo.");
            return;
        }

        // Só um ingrediente ainda — mostra a cor dele
        if (total == 1)
        {
            foreach (var kv in ingredientesRecebidos)
            {
                AplicarCorLiquido(ObterCorDoIngrediente(kv.Key));
                break;
            }
            return;
        }

        // Qualquer outra combinação = inválida
        AplicarCorLiquido(corMisturaInvalida);
        Debug.Log("[BeakerMixer] Mistura inválida!");

        if (!explosaoJaDisparada)
        {
            explosaoJaDisparada = true;
            DispararExplosao();
        }
    }

    void DispararExplosao()
    {
        // Toca o som de estouro
        if (audioSource != null && somExplosao != null)
            audioSource.PlayOneShot(somExplosao, volumeSom);

        // ================================================================
        // TODO: Implementar animação de explosão aqui.
        // Sugestões:
        //   - Instantiate(explosaoPrefab, transform.position, Quaternion.identity);
        //   - Physics.OverlapSphere para força radial nos objetos próximos
        //   - SceneManager.LoadScene para reset da cena
        // ================================================================
        Debug.Log("[BeakerMixer] EXPLOSÃO! (visual ainda não implementado)");
    }

    void TocarSomMistura()
    {
        if (audioSource == null || somMisturando == null) return;
        // PlayOneShot para não cortar caso chegue vários frames seguidos
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

    public bool MisturaValida() =>
        ingredientesRecebidos.ContainsKey(TestTubeIngredient.Ingrediente.Azul)
        && ingredientesRecebidos.ContainsKey(TestTubeIngredient.Ingrediente.Vermelho)
        && ingredientesRecebidos.Count == 2;
}