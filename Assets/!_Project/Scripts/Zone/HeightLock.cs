using UnityEngine;

/// <summary>
/// HeightLock — жёстко держит объект на заданной высоте.
/// Работает после всех сил физики (LateUpdate), поэтому подняться
/// объект не может даже при странном столкновении.
/// </summary>
public class HeightLock : MonoBehaviour
{
    // Высота, на которой враг обязан находиться.
    [SerializeField] private float _fixedHeight = 0.5f;

    private void LateUpdate()
    {
        Vector3 position = transform.position;
        position.y = _fixedHeight;
        transform.position = position;
    }
}
