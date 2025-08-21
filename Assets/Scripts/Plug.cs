using UnityEngine;

public class Plug : MonoBehaviour
{
    public int id;
    public bool isLeft;

    public bool IsLocked { get; private set; }
    public RectTransform RT => (RectTransform)transform;

    public void Lock() => IsLocked = true;
    public void Unlock() => IsLocked = false;
}
