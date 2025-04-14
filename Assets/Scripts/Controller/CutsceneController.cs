using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

public class CutsceneController : MonoBehaviour
{
    public VideoPlayer VideoPlayer { get; private set; }

    // TODO: sostituire con sopra
    public UnityEvent OnCutsceneEnd { get; private set; }
    [SerializeField] private bool IsSkippable = true;

    private void OnValidate() {
        GetComponent<VideoPlayer>().targetCamera = Camera.main;
    }

    private void Awake() {
        OnCutsceneEnd = new UnityEvent();
    }

    private void Start() {
        VideoPlayer = GetComponent<VideoPlayer>();
        VideoPlayer.loopPointReached += OnVideoEnd;
        
        if(VideoPlayer.playOnAwake){
            Time.timeScale = 0; 
            StartCoroutine(ClipControls()); 
        } else {
            VideoPlayer.enabled = false; 
        }
    }

    private void OnVideoEnd(VideoPlayer vp = null) {
        StopAllCoroutines(); 
        Debug.Log("Cutscene ended.");
        Time.timeScale = 1;

        OnCutsceneEnd?.Invoke();
        OnCutsceneEnd.RemoveAllListeners();

        VideoPlayer.enabled = false;
    }

    IEnumerator ClipControls(){
        while(true){
            yield return null;

            if (Input.GetKeyDown(KeyCode.Space) && IsSkippable) {
                Debug.Log("Cutscene skipped.");
                VideoPlayer.Stop();
                OnVideoEnd();
            }
        }
    }

    public void PlayCutscene(VideoClip videoClip, bool isSkippable = true) {
        if (VideoPlayer != null) {
            VideoPlayer.enabled = true;
            VideoPlayer.clip = videoClip;
            IsSkippable = isSkippable;
            Time.timeScale = 0; 
            VideoPlayer.Play();
            StartCoroutine(ClipControls()); 
        } else {
            Debug.LogError("VideoPlayer component not found.");
        }
    }

    private void OnDestroy() {
        VideoPlayer.loopPointReached -= OnVideoEnd;
        StopAllCoroutines(); 
        Time.timeScale = 1; 
    }
}
