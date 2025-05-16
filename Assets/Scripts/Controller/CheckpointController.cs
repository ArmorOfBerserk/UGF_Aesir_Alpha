using UnityEngine;

public class CheckpointController : MonoBehaviour
{
    // #region TEST ONLY
    // public void LoadCheckpoint() {
    //     var data = SaveSystem.LoadGameData();
    //     if (data != null) {
    //         var pos = new Vector3(data.position[0], data.position[1], data.position[2]);
    //         GameObject.FindGameObjectWithTag("Player").transform.position = pos;
    //     }
    // }

    // private void Update() {
    //     if (Input.GetKeyDown(KeyCode.L)) {
    //         LoadCheckpoint();
    //     }
    // }
    // #endregion

    public Material saveCheckpoint;
    public Material midLevelCheckpoint;


    [Tooltip("Se true crea un salvataggio, altrimenti è considerato mid level checkpoint")]
    [SerializeField] private bool shouldSave = true;

    public static Vector3 LastCheckpointPosition { get; private set; }
    

    void OnValidate()
    {
        if(shouldSave) {
            GetComponent<Renderer>().material = saveCheckpoint;
        } else {
            GetComponent<Renderer>().material = midLevelCheckpoint;
        }
    }

    private void Start() {
        LastCheckpointPosition = Vector3.zero;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            if(shouldSave){
                var pos = other.transform.position;
                var data = new GameData(new float[] { pos.x, pos.y, pos.z });
                SaveSystem.SaveGameData(data);
            } 

            LastCheckpointPosition = other.transform.position;
        } 
    }


}
