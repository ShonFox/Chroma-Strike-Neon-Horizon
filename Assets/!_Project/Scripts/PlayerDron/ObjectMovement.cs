using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectMovement : MonoBehaviour
{

    [SerializeField] private Rigidbody _rigidbody;

    [SerializeField, Range(0f, 10f)] private float _groundAcceleration = 4f;
    [SerializeField, Range(0f, 10f)] private float _maxSpeed = 6f;
    [SerializeField, Range(0f, 20f)] private float _boostSpeed = 12f;
    [SerializeField, Range(0f, 20f)] private float _boostAcceleration = 10f;

    [SerializeField, Range(0f, 100f)] private float _maxEnergy = 100f;
    [SerializeField, Range(0f, 100f)] private float _energyPerSecond = 40f;
    [SerializeField, Range(0f, 100f)] private float _energyRegenPerSecond = 5f;

    [SerializeField] private bool _useLocalForward = true;

    [SerializeField] private InputAction _boostAction;
    [SerializeField] private InputAction _steerAction; // ось влево/вправо

    private float _currentEnergy;
    private bool _isBoosting;

    private void Awake()
    {
        _currentEnergy = _maxEnergy;

        if (_boostAction == null)
        {
            _boostAction = new InputAction("Boost", InputActionType.Value);
            _boostAction.AddBinding("<Keyboard>/w");
        }

        if (_steerAction == null)
        {
            _steerAction = new InputAction("Steer", InputActionType.Value);
            _steerAction.AddCompositeBinding("1D Axis").With("Positive", "<Keyboard>/d").With("Negative", "<Keyboard>/a");
        }

        _boostAction.Enable();
        _steerAction.Enable();
    }

    private void Update()
    {
        if (!_isBoosting && _currentEnergy < _maxEnergy)
        {
            _currentEnergy = Mathf.Min(_maxEnergy, _currentEnergy + _energyRegenPerSecond * Time.deltaTime);
        }

        _isBoosting = _boostAction.IsPressed() && _currentEnergy > 0f;

        if (_isBoosting)
        {
            _currentEnergy = Mathf.Max(0f, _currentEnergy - _energyPerSecond * Time.deltaTime);
            if (_currentEnergy <= 0f)
                _isBoosting = false;
        }
    }

    public void Move(float steer)
    {
        Vector3 forward = _useLocalForward ? transform.forward : Vector3.forward;
        Vector3 horizontalDirection = forward + new Vector3(steer, 0f, 0f);

        if (horizontalDirection.sqrMagnitude > 0.001f)
            horizontalDirection.Normalize();

        float targetSpeed = _isBoosting ? _boostSpeed : _maxSpeed;
        float accel = _isBoosting ? _boostAcceleration : _groundAcceleration;

        Vector3 maximalVelocity = horizontalDirection * targetSpeed;
        Vector3 currentVelocity = _rigidbody.linearVelocity;
        float verticalSpeed = currentVelocity.y;
        currentVelocity.y = 0f;

        float deltaAcceleration = accel * Time.fixedDeltaTime;
        currentVelocity = Vector3.MoveTowards(currentVelocity, maximalVelocity, deltaAcceleration);

        currentVelocity.y = verticalSpeed;
        _rigidbody.linearVelocity = currentVelocity;
    }

    public void AddEnergy(float amount)
    {
        _currentEnergy = Mathf.Min(_maxEnergy, _currentEnergy + amount);
    }

    private void FixedUpdate()
    {
        float steer = _steerAction.ReadValue<float>();
        Move(steer);
    }

    private void OnDestroy()
    {
        _boostAction?.Disable();
        _steerAction?.Disable();
    }
}
