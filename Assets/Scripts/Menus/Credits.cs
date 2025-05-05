using UnityEngine;

public class Credits : MonoBehaviour
{
    [SerializeField] private MainMenu mainMenu;
    
    public void CloseCredits()
    {
        mainMenu.CloseCreditsMenu();
    }
}