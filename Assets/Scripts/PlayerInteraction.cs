using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    public TextMeshProUGUI promptText;
    Interaction interaction = new Interaction();

    MiniGameMachine current;
    bool lockPrompt; 

    void Update()
    {
        if (current != null && !lockPrompt)
        {
            if (promptText && !promptText.gameObject.activeSelf)
                promptText.gameObject.SetActive(true);

            if (promptText) promptText.text = "Press E to Play";

            if (Input.GetKeyDown(KeyCode.E))
            {
               
                lockPrompt = true;
                if (promptText) promptText.gameObject.SetActive(false);
                interaction.StartInteraction(KeyCode.E, OnResolved);
            }
        }
        else
        {
            if (promptText && promptText.gameObject.activeSelf)
                promptText.gameObject.SetActive(false);
        }

        interaction.HandleInput();
        interaction.Update();
    }

    void OnResolved(bool success)
    {
        if (promptText) promptText.gameObject.SetActive(false);

        if (success && current != null)
        {
            current.SetHighlight(false);
            current.StartMiniGame();
        }
    }

    public void SetCurrentMachine(MiniGameMachine machine)
    {
        if (current != null)
            current.SetHighlight(false);

        current = machine;

       
        if (current == null)
            lockPrompt = false;
        else
            current.SetHighlight(true);
    }
}
