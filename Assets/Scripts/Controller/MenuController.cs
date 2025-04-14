using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.Video;

public class MenuController : MonoBehaviour
{
    public string sceneToLoad = "TestSaves";
    [SerializeField] private CutsceneController cutsceneController;
    [SerializeField] private VideoClip clip;

    public void Continue(){
        transform.parent.gameObject.SetActive(false);
        if(!File.Exists(Application.persistentDataPath + "/data.aesir")){
            cutsceneController.OnCutsceneEnd.AddListener(Load);
            cutsceneController.PlayCutscene(clip, true);
        } else{
            Load();
        }
    }

    public void NewGame(){
        transform.parent.gameObject.SetActive(false);
        if(File.Exists(Application.persistentDataPath + "/data.aesir")){
            File.Delete(Application.persistentDataPath + "/data.aesir");
        }
        cutsceneController.OnCutsceneEnd.AddListener(Load);
        cutsceneController.PlayCutscene(clip, true);
    }

    void Load(){
        SceneManager.LoadScene(sceneToLoad);
    }

    public void Exit(){
        Application.Quit();
    }
}
