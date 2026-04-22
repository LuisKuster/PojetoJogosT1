using UnityEngine;

public class PortaCorrerController : MonoBehaviour
{
    private ConfigurableJoint joint;
    private bool travada = true;

    void Start()
    {
        joint = GetComponent<ConfigurableJoint>();
        Travar(); // começa travada
    }

    public void Travar()
    {
        travada = true;
        // Trava o eixo X também
        joint.xMotion = ConfigurableJointMotion.Locked;
    }

    public void Destravar()
    {
        if (!travada) return;
        travada = false;
        // Libera o eixo X com o limite que você já tinha
        joint.xMotion = ConfigurableJointMotion.Limited;
        print("Porta destravada!");
    }
}