using UnityEngine;

public class GerenciadorAudio : MonoBehaviour
{
    [Header("Instrucao do instrutor")]
    [SerializeField] private AudioClip instrucao;
    [SerializeField] private float atrasoInstrucao = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float volumeInstrucao = 1f;

    [Header("Musica de fundo")]
    [SerializeField] private AudioClip musicaFundo;
    [SerializeField] private float atrasoMusica = 0f;
    [Range(0f, 1f)]
    [SerializeField] private float volumeMusica = 0.4f;

    private AudioSource sourceInstrucao;
    private AudioSource sourceMusica;

    void Start()
    {
        sourceInstrucao = gameObject.AddComponent<AudioSource>();
        sourceInstrucao.clip         = instrucao;
        sourceInstrucao.loop         = false;
        sourceInstrucao.playOnAwake  = false;
        sourceInstrucao.spatialBlend = 0f;
        sourceInstrucao.volume       = volumeInstrucao;

        sourceMusica = gameObject.AddComponent<AudioSource>();
        sourceMusica.clip         = musicaFundo;
        sourceMusica.loop         = true;
        sourceMusica.playOnAwake  = false;
        sourceMusica.spatialBlend = 0f;
        sourceMusica.volume       = volumeMusica;

        if (instrucao != null)
            Invoke(nameof(TocarInstrucao), atrasoInstrucao);

        if (musicaFundo != null)
            Invoke(nameof(TocarMusica), atrasoMusica);
    }

    void TocarInstrucao() => sourceInstrucao.Play();
    void TocarMusica()    => sourceMusica.Play();
}