using UnityEngine;

/// <summary>
/// Script TEMPORÁRIO — anexe no Beaker liquid, execute o jogo,
/// veja o console e depois delete esse script.
/// </summary>
public class DebugMaterial : MonoBehaviour
{
    void Start()
    {
        Renderer r = GetComponent<Renderer>();
        if (r == null) { Debug.Log("Sem Renderer!"); return; }

        Material mat = r.material;
        Debug.Log($"[DebugMaterial] Shader: {mat.shader.name}");

        int count = mat.shader.GetPropertyCount();
        for (int i = 0; i < count; i++)
        {
            string nome = mat.shader.GetPropertyName(i);
            var    tipo = mat.shader.GetPropertyType(i);
            Debug.Log($"[DebugMaterial] Propriedade {i}: {nome} ({tipo})");
        }
    }
}
