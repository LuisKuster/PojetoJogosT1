using UnityEngine;

public class PortaCorrerController : MonoBehaviour
{
    private ConfigurableJoint joint;
    private bool travada = true;

    void Start()
    {
        joint = GetComponent<ConfigurableJoint>();
        Travar();
    }

    public void Travar()
    {
        travada = true;
        joint.xMotion = ConfigurableJointMotion.Locked;
    }

    public void Destravar()
    {
        if (!travada) return;
        travada = false;
        joint.xMotion = ConfigurableJointMotion.Limited;
    }
}