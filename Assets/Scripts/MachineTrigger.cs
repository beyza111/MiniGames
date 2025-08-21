using UnityEngine;

public class MachineTrigge : MonoBehaviour
{
    public MiniGameManager manager;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            manager.SetMachineProximity(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            manager.SetMachineProximity(false);
    }
}
