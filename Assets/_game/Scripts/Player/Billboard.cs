using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera cam;
    
    private void Update()
    {
        // If no camera find a camera in the scene
        if (cam == null) cam = FindObjectOfType<Camera>();
        
        // If still no camera return
        if(cam == null) return;
        
        // Make nametag look at the camera (Camera positioned on the player meaning the nametag will face the player)
        transform.LookAt(cam.transform);
        transform.Rotate(Vector3.up * 180);
    }
}
