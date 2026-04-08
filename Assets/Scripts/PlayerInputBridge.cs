using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputBridge : MonoBehaviour
{
    [SerializeField] private PlayerController _controller;   

    private int _leftPressed;
    private int _rightPressed;
    private bool _jumpRequested;

    private float MoveAxis => Mathf.Clamp(_rightPressed - _leftPressed, -1f, 1f);

    private void Awake()
    {
        if (_controller != null) return;
       
        _controller = GetComponent<PlayerController>();
    }
    private void Update()
    {
        if (_controller == null) return;        

        var keyboardMove = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed) keyboardMove -= 1f;

            if (Keyboard.current.dKey.isPressed) keyboardMove += 1f;

            if (Keyboard.current.spaceKey.wasPressedThisFrame) RequestJump();
        }        
        

        var finalMove = Mathf.Clamp(keyboardMove + MoveAxis, -1f, 1f);
        bool finalJump = ConsumeJump();

        _controller.SetMoveInput(finalMove);
        
        if (finalJump) _controller.QueueJump();
    }

    public void SetMovePressed(float direction, bool isPressed)
    {
        if (direction < 0f)
        {
            _leftPressed = Mathf.Max(0, _leftPressed + (isPressed ? 1 : -1));
            return;
        }

        _rightPressed = Mathf.Max(0, _rightPressed + (isPressed ? 1 : -1));
    }

    public void RequestJump() => _jumpRequested = true;

    private bool ConsumeJump()
    {
        if (!_jumpRequested) return false;

        _jumpRequested = false;
        return true;
    }    
}
