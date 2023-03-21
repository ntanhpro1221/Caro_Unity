using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hub : MonoBehaviour
{
    public static HashSet<Vector2> cellx = new HashSet<Vector2>();
    public static HashSet<Vector2> cello = new HashSet<Vector2>();
    public static bool win = false;
    public static bool hostTurn = true;
}
