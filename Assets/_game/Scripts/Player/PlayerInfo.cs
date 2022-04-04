using UnityEngine;

[CreateAssetMenu(fileName = "New Player", menuName = "Customisation/New Player")]
public class PlayerInfo : ScriptableObject
{
    public string className;
    
    [Header("Movement")]
    [Tooltip("additive force amount. every physics update that forward is pressed, this force (multiplied by 1/tickrate) will be added to the player.")]
    public float moveSpeed = 2700f;
    [Tooltip("maximum local velocity before input is cancelled")]
    public float maxSpeed = 10f;
    [Tooltip("normal countermovement when not crouching.")]
    public float counterMovement = 0.175f;
    [Tooltip("forward force for when a crouch is started.")]
    public float slideForce = 200f;
    [Tooltip("countermovement when sliding. this doesnt work the same way as normal countermovement.")]
    public float slideCounterMovement = 0.2f;
    [Tooltip("this determines the jump force but is also applied when jumping off of walls, if you decrease it, you may end up being able to walljump and then get back onto the wall leading to infinite height.")]
    public float jumpForce = 550f;

    [Header("Wallrunning")]
    [Tooltip("when a wallrun is started, an upwards force is applied, this describes that force.")]
    public float initialForce = 20f;
    [Tooltip("float to choose how much force is applied outwards when ending a wallrun. this should always be greater than Jump Force")]
    public float escapeForce = 600f;
    [Tooltip("a bool to determine whether or not to actually allow wallrunning.")]
    public bool UseWallrunning = true;

    [Header("Health")] 
    [Tooltip("maximum health that the player has.")]
    public float maxHealth = 100f;
}
