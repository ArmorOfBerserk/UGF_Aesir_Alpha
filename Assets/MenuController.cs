using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class MenuController : MonoBehaviour
{
    public void Continue(){
        SceneManager.LoadScene("TestSaves");
    }

    public void NewGame(){
        if(File.Exists(Application.persistentDataPath + "/data.aesir")){
            File.Delete(Application.persistentDataPath + "/data.aesir");
        }
        SceneManager.LoadScene("TestSaves");
    }

    public void Exit(){
        Application.Quit();
    }
}
