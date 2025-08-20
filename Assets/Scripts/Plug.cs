using UnityEngine;
using UnityEngine.EventSystems;

public class Plug : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public int id;      
    public bool isLeft; 

    public bool IsLocked { get; private set; }  
    WireConnectMiniGame owner;
    public RectTransform RT => (RectTransform)transform;

    void Awake() => owner = GetComponentInParent<WireConnectMiniGame>();

    
    public void Lock() { IsLocked = true; }
    public void Unlock() { IsLocked = false; }

    public void OnPointerDown(PointerEventData e)
    {
        if (!isLeft || IsLocked || owner.State != MiniGameState.Running) return;
        owner.BeginDragFrom(this, e);
    }

    public void OnDrag(PointerEventData e)
    {
        if (!isLeft || IsLocked) return;
        owner.DragTo(e);
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (!isLeft || IsLocked) return;
        owner.EndDrag(e);
    }
}
