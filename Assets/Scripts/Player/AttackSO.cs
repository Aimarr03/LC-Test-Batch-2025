using UnityEngine;

[CreateAssetMenu(fileName = "AttackSO", menuName = "Scriptable Objects/AttackSO")]
public class AttackSO : ScriptableObject
{
    public int damage;
    public float range;
    public float moveSpeed;
    public AnimatorOverrideController overrideController;
}
