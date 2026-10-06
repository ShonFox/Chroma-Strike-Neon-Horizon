using UnityEngine;

public class BulletLifetime : MonoBehaviour
{
    //Время через которое уничтожится снаряд
    [SerializeField] private float _lifeTime = 3f;

    //Уничтожение пули
    private void Start()
    {
        Destroy(gameObject, _lifeTime);
    }
}
