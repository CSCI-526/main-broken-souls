using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject instructionsPanel;
    public GameObject mainMenuPanel; // the panel that contains your buttons

    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene"); // replace with your actual scene name
    }

    public void ShowInstructions()
    {
        mainMenuPanel.SetActive(false);       // hide main menu
        instructionsPanel.SetActive(true);    // show instructions
    }

    public void HideInstructions()
    {
        instructionsPanel.SetActive(false);   // hide instructions
        mainMenuPanel.SetActive(true);        // show main menu again
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
