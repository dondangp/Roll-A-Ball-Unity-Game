using UnityEngine; // Imports Unity engine

public class CameraController : MonoBehaviour // Defines camera follower
{ // Opens camera scope
    public GameObject player; // References followed player
    Vector3 offset; // Stores camera offset

    void Start() // Initializes camera offset
    { // Opens start scope
        offset = transform.position - player.transform.position; // Calculates follow offset
    } // Ends start scope

    void LateUpdate() // Runs after movement
    { // Opens lateupdate scope
        transform.position = player.transform.position + offset; // Follows player position
    } // Ends lateupdate scope
} // Ends camera scope
