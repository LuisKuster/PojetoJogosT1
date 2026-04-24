using UnityEngine;

/// <summary>
/// Anexe em um objeto vazio na cena (ex: "GerenciadorAudio").
/// - Toca o audio de instrucao uma vez no inicio
/// - Toca a musica de fundo em loop durante toda a fase
/// </summary>
public class GerenciadorAudio : MonoBehaviour
{
    [Header("Instrucao do instrutor")]
    [Tooltip("Audio com as instrucoes do que o jogador deve fazer.")]
    [SerializeField] private AudioClip instrucao;

    [Tooltip("Atraso em segundos antes de tocar a instrucao.")]
    [SerializeField] private float atrasoInstrucao = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float volumeInstrucao = 1f;

    [Header("Musica de fundo")]
    [Tooltip("Musica de fundo tocada em loop durante toda a fase.")]
    [SerializeField] private AudioClip musicaFundo;

    [Tooltip("Atraso em segundos antes de iniciar a musica de fundo.")]
    [SerializeField] private float atrasoMusica = 0f;

    [Range(0f, 1f)]
    [SerializeField] private float volumeMusica = 0.4f;

    private AudioSource sourceInstrucao;
    private AudioSource sourceMusica;

    void Start()
    {
        // AudioSource separado para instrucao (nao loop)
        sourceInstrucao = gameObject.AddComponent<AudioSource>();
        sourceInstrucao.clip         = instrucao;
        sourceInstrucao.loop         = false;
        sourceInstrucao.playOnAwake  = false;
        sourceInstrucao.spatialBlend = 0f; // 2D — toca no ouvido direto
        sourceInstrucao.volume       = volumeInstrucao;

        // AudioSource separado para musica (loop)
        sourceMusica = gameObject.AddComponent<AudioSource>();
        sourceMusica.clip         = musicaFundo;
        sourceMusica.loop         = true;
        sourceMusica.playOnAwake  = false;
        sourceMusica.spatialBlend = 0f; // 2D — musica ambiente
        sourceMusica.volume       = volumeMusica;

        // Agenda os plays com atraso
        if (instrucao != null)
            Invoke(nameof(TocarInstrucao), atrasoInstrucao);

        if (musicaFundo != null)
            Invoke(nameof(TocarMusica), atrasoMusica);
    }

    void TocarInstrucao()
    {
        sourceInstrucao.Play();
        Debug.Log("[GerenciadorAudio] Instrucao iniciada.");
    }

    void TocarMusica()
    {
        sourceMusica.Play();
        Debug.Log("[GerenciadorAudio] Musica de fundo iniciada.");
    }
}
