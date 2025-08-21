using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MachineTrigger : MonoBehaviour
{
    public MiniGameMachine machine;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            other.GetComponent<PlayerInteraction>()?.SetCurrentMachine(machine);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            other.GetComponent<PlayerInteraction>()?.SetCurrentMachine(null);
    }
}
