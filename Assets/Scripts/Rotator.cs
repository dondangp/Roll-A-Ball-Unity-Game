using UnityEngine; // Imports Unity engine

public class Rotator : MonoBehaviour // Defines pickup rotator
{ // Opens rotator scope
    public Vector3 rotationSpeed = new Vector3(15f, 30f, 45f); // Exposes spin speed

    void Update() // Updates pickup rotation
    { // Opens update scope
        transform.Rotate(rotationSpeed * Time.deltaTime); // Spins pickup smoothly
    } // Ends update scope
} // Ends rotator scope
