using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Animations.Rigging;

public class UpdateAmmoDisplay : MonoBehaviour
{
    public GameObject holder;
    public TMP_Text magSize;
    public TMP_Text pocketAmmo;
    public Image weaponIcon;

    [SerializeField] TwoBoneIKConstraint leftArm, rightArm;
    [SerializeField] Transform ref_right_hand_grip, ref_left_hand_grip;

    public void SetHandIK(Transform _ref_right_hand_grip, Transform _ref_left_hand_grip, float rightWeight, float leftWeight)
    {
        ref_right_hand_grip.position = _ref_right_hand_grip.position;
        ref_right_hand_grip.rotation = _ref_right_hand_grip.rotation;

        ref_left_hand_grip.position = _ref_left_hand_grip.position;
        ref_left_hand_grip.rotation = _ref_left_hand_grip.rotation;

        leftArm.weight = leftWeight;
        rightArm.weight = rightWeight;
    }
}
