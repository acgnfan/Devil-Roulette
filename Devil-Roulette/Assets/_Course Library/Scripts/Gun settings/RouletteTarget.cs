using UnityEngine;

public enum TargetType
{
    Self,
    Enemy
}

public class RouletteTarget : MonoBehaviour
{
    public TargetType type;
}

