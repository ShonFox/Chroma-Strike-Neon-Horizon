using UnityEngine;
using UnityEngine.InputSystem;

public class BallController : MonoBehaviour
{
    [SerializeField] private Transform _mainCamera;
    [SerializeField] private ObjectMovement _droneMovement;

    private Vector2 _inputDirection;

    private void FixedUpdate()
    {
        if (_inputDirection != Vector2.zero)
        {
            HandleMovement();
        }
    }

    private void HandleMovement()
    {
        Vector3 cameraForward = _mainCamera.forward;
        Vector3 cameraRight = _mainCamera.right;

        cameraForward.y = 0;
        cameraRight.y = 0;

        cameraForward.Normalize();
        cameraRight.Normalize();

        // Исправленная формула: используем cameraRight для X и cameraForward для Y
        Vector3 movementDirection =
            cameraRight * _inputDirection.x +
            cameraForward * _inputDirection.y;

        if (movementDirection.sqrMagnitude > 0.001f)
        {
            movementDirection.Normalize();
        }

        // Передаём только горизонтальное отклонение (ось X) в Move,
        // так как автодвижение вперёд уже заложено внутри DroneMovement
        _droneMovement.Move(_inputDirection.x);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _inputDirection = context.ReadValue<Vector2>();
        }
        else if (context.canceled)
        {
            _inputDirection = Vector2.zero;
        }
    }
}
