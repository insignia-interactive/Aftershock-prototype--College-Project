using UnityEngine;

public class MoveCamera : MonoBehaviour {

    public Transform player;

    void Update() {
        // Sets camera holder position to the head of the player
        transform.position = player.transform.position;
    }
}