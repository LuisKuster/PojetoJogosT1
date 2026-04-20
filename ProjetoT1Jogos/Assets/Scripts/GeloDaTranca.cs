using System.Collections;
using UnityEngine;

/// <summary>
/// Anexe no cubo de gelo que está em cima da maçaneta.
/// Quando receber líquido suficiente do béquer, some e libera a porta.
/// </summary>
public class GeloDaTranca : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Referência ao PortaLock do Espelho da porta.")]
    [SerializeField] private PortaLock portaLock;

    [Header("Configurações")]
    [Tooltip("Quantidade total de líquido necessária para derreter o gelo.")]
    [SerializeField] private float volumeParaDerretar = 1f;

    [Tooltip("Tempo da animação de sumiço do gelo em segundos.")]
    [SerializeField] private float tempoSumico = 0.5f;

    [Header("Som")]
    [SerializeField] private AudioClip somDerretendo;
    [SerializeField] [Range(0f, 1f)] private float volumeSom = 1f;

    private AudioSource audioSource;
    private float volumeRecebido = 0f;
    private bool  jaDerreteu     = false;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake  = false;
        audioSource.spatialBlend = 1f;
        audioSource.volume       = volumeSom;
    }

    /// <summary>
    /// Chamado pelo BeakerPourer a cada frame enquanto derrama.
    /// Retorna true quando o gelo terminou de derreter.
    /// </summary>
    public bool ReceberLiquido(float volume)
    {
        if (jaDerreteu) return true;

        volumeRecebido += volume;

        if (volumeRecebido >= volumeParaDerretar)
        {
            jaDerreteu = true;
            StartCoroutine(Derreter());
            return true;
        }

        return false;
    }

    IEnumerator Derreter()
    {
        // Toca som de derretimento
        if (audioSource != null && somDerretendo != null)
            audioSource.PlayOneShot(somDerretendo, volumeSom);

        // Animação de sumiço: escala vai a zero suavemente
        Vector3 escalaInicial = transform.localScale;
        float   tempo         = 0f;

        while (tempo < tempoSumico)
        {
            tempo += Time.deltaTime;
            float t = tempo / tempoSumico;
            transform.localScale = Vector3.Lerp(escalaInicial, Vector3.zero, t);
            yield return null;
        }

        // Libera a porta
        if (portaLock != null)
            portaLock.LiberarPorta();

        Debug.Log("[GeloDaTranca] Gelo derreteu! Porta liberada.");

        // Destrói o cubo de gelo
        Destroy(gameObject);
    }
}
