using UnityEngine;

public class BulletMovement : MonoBehaviour
{
    //физическое тело
    [SerializeField] private Rigidbody _rigidbody;

    //Скорость пули
    [SerializeField] private float _speed = 5;

    //Постоянная скорость пули
    private void Start()
    {
        _rigidbody.linearVelocity = transform.forward * _speed;
    }
}
