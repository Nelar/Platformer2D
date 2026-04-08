using UnityEngine;
using UnityEngine.EventSystems;

public class TouchJumpButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] PlayerInputBridge _playerInputBridge;
    public void OnPointerDown(PointerEventData eventData) => _playerInputBridge.RequestJump();
}
