using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;


public class StartMenu : MonoBehaviour
{

    private GameObject StartMenuInstance;
    private GameObject StartMenuPrefab;
    private GameObject quitConfirmPanel;
    bool startMenuActive = true;

    [Header("Scene Name")]
    public string characterSelectSceneName = "Scene Demo";
    public string StartMenu1 = "StartMenu";

    




    public void LoadSelectionCanvas()

    {

        SceneManager.LoadScene(characterSelectSceneName, LoadSceneMode.Single);


    }

    public void QuitGame() 
    
    { 
        if (quitConfirmPanel != null) 
        {
            quitConfirmPanel.SetActive(true);
            if ( StartMenuInstance != null) 
            
            {

                StartMenuInstance.SetActive(false);
            
            }        
        }
        else
        {
            QuitConfirmed();
        }
    
    
    
    }


    public void QuitConfirmed()

    {



        




    }

    public void Settings() 
    {
    
    
    
    
    }


}