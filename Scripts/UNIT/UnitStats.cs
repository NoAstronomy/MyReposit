using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitStats", menuName = "Create UnitStats")]
public class UnitStats : ScriptableObject
{
    [Header("Unit movement speed")]
    [Range(0, 100)]
    public float movementSpeed;

    [Header("Unit jump force")]
    [Range(0, 20)]
    public float jumpForce;

    [Header("Unit jump force of wall")][Range(0, 20)]
    public float jumpForceWall;

    [Header("Unit Jump Curve")]
    public AnimationCurve jumpCurve;

    [Header("Unit Double Jump Curve")]
    public AnimationCurve doubleJumpCurve;

    [Header("Unit Fallen Jump Curve")]
    public AnimationCurve fallenJumpCurve;

    [Header("Unit Jump in side of Wall Curve ")]
    public AnimationCurve jumpOfWallCurveSide;

    [Header("Unit Jump up of Wall Curve ")]
    public AnimationCurve jumpOfWallCurveUp;

    [Header("Speed curve for roll")]
    public AnimationCurve rollSpeedCurve;

   // [Header("Speed curfe for roll")]
   // public AnimationCurve dashSpeedCurve;

}
