using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;
    
    [SerializeField] private Menu[] menus;

    private void Awake()
    {
        Instance = this;
    }

    // Open menu using a string
    public void OpenMenu(string menuName)
    {
        // Loops through the menus 
        for (int i = 0; i < menus.Length; i++)
        {
            // if the current menu has the same name as the string passed open it else if the menu is already open close it
            if (menus[i].menuName == menuName)
            {
                menus[i].Open();
            } else if (menus[i].open)
            {
                CloseMenu(menus[i]);
            }
        }  
    } 

    // Open menu using Menu script
    public void OpenMenu(Menu menu)
    {
        // loops through the menus 
        for (int i = 0; i < menus.Length; i++)
        {
            // if menu is open then close it
            if (menus[i].open)
            {
                CloseMenu(menus[i]);
            }
        }  
        
        // open menu thats Menu script was passed
        menu.Open();
    }
    
    // Close menu using Menu script
    public void CloseMenu(Menu menu)
    {
        menu.Close();
    }
}