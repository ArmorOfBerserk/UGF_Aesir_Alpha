using UnityEngine;

public class DoNotDestroyOnLoad : MonoBehaviour
{
    public static DoNotDestroyOnLoad Instance { get; private set; }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
            return;
        }
    }
}
