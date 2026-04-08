using UnityEngine;
using UnityEngine.EventSystems;

public class TouchMoveButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] PlayerInputBridge _playerInputBridge;
    [SerializeField] private float _direction = 1f;
    
    private bool _pressed;

    public void SetDirection(float value) => _direction = value >= 0f ? 1f : -1f;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_pressed) return;

        _pressed = true;
        _playerInputBridge.SetMovePressed(_direction, true);
    }

    public void OnPointerUp(PointerEventData eventData) => Release();

    public void OnPointerExit(PointerEventData eventData) => Release();

    private void OnDisable() => Release();

    private void Release()
    {
        if (!_pressed) return;

        _pressed = false;
        _playerInputBridge.SetMovePressed(_direction, false);
    }
}
