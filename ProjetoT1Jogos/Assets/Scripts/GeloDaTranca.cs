using System.Collections;
using UnityEngine;

/// <summary>
/// Anexe no cubo de gelo que esta em cima da machaneta.
/// Quando receber liquido suficiente do bequer, some e libera a porta.
/// O som de vitoria e gerenciado pelo PortaLock.
/// </summary>
public class GeloDaTranca : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Referencia ao PortaLock do Espelho da porta.")]
    [SerializeField] private PortaLock portaLock;

    [Header("Configuracoes")]
    [SerializeField] private float volumeParaDerretar = 1f;
    [SerializeField] private float tempoSumico        = 0.5f;

    [Header("Som")]
    [Tooltip("Som de derretimento/gelo quebrando.")]
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
        // Som de derretimento
        if (audioSource != null && somDerretendo != null)
            audioSource.PlayOneShot(somDerretendo, volumeSom);

        // Animacao de sumico
        Vector3 escalaInicial = transform.localScale;
        float   tempo         = 0f;

        while (tempo < tempoSumico)
        {
            tempo += Time.deltaTime;
            float t = tempo / tempoSumico;
            transform.localScale = Vector3.Lerp(escalaInicial, Vector3.zero, t);
            yield return null;
        }

        // Libera a porta (som de vitoria toca no PortaLock)
        if (portaLock != null)
            portaLock.LiberarPorta();

        Debug.Log("[GeloDaTranca] Gelo derreteu! Porta liberada.");

        Destroy(gameObject);
    }
}