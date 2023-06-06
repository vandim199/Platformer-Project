using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Jump")]
public class JumpCurve : ScriptableObject
{
    public float jumpForce = 7;
    public AnimationCurve riseGravity;
    public AnimationCurve fallGravity;
    public float gravityOnRelease = 2;
}
