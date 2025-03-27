using UnityEngine;

public class CheckpointController : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            var pos = other.transform.position;
            var data = new GameData(new float[] { pos.x, pos.y, pos.z });
            SaveSystem.SaveGameData(data);
        }
    }


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
}
