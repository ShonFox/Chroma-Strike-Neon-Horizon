using System.Collections;
using UnityEngine;

public class WallBounce : MonoBehaviour
{
    // Сила отброса (скорость, с которой дрон отлетает от стены).
    [SerializeField] private float _knockbackSpeed = 5f;

    // Сколько секунд дрон «откатывается» и не может ехать вперёд.
    // Расстояние отброса ≈ _knockbackSpeed * _knockbackDuration.
    [SerializeField] private float _knockbackDuration = 0.3f;

    // Защита от повторных срабатываний: после удара дрон не может
    // получить новый отброс, пока не вернул управление.
    private bool _isKnockbackActive;

    private void OnCollisionEnter(Collision collision)
    {
        if (_isKnockbackActive || collision.contactCount == 0) return;

        // Нормаль стены в точке касания — направление «от стены».
        Vector3 normal = collision.GetContact(0).normal;

        // Top-down: работаем только в горизонтальной плоскости.
        normal.y = 0f;
        if (normal.sqrMagnitude < 0.001f) return;
        normal.Normalize();

        _isKnockbackActive = true;
        StartCoroutine(Knockback(normal));
    }

    private IEnumerator Knockback(Vector3 direction)
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        // Блокируем автодвижение — иначе оно перезапишет отброс.
        if (TryGetComponent<ObjectMovement>(out var movement))
        {
            movement.SetMovementEnabled(false);
        }

        // Гасим текущую скорость и задаём скорость отброса от стены.
        rb.linearVelocity = Vector3.zero;
        rb.linearVelocity = direction * _knockbackSpeed;

        yield return new WaitForSeconds(_knockbackDuration);

        // Возвращаем управление: ObjectMovement снова начнёт ехать вперёд.
        if (movement != null)
        {
            movement.SetMovementEnabled(true);
        }

        _isKnockbackActive = false;

        // Опционально: разворачиваем нос от стены, чтобы не ехать в неё снова.
        //transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }
}
