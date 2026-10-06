using System.Collections;
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    //точки выстрела
    [SerializeField] private Transform[] _firePoints;
    //указывает на префаб снаряда
    [SerializeField] private GameObject _bulletPrefab;
    //диапазон времени (в секундах)
    [SerializeField] private float _minShootInterval = 3f;
    [SerializeField] private float _maxShootInterval = 7f;

    //разрешение или запрет стрельбы
    private bool _canShoot = false;


    private void OnEnable()
    {
        StartCoroutine(ShootCoroutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    //позволяет извне включить или выключить стрельбу
    public void Shoot(bool canShoot)
    {
        _canShoot = canShoot;
    }

    private IEnumerator ShootCoroutine()
    {
        while (true) 
        {
            if (_canShoot)
            {
                ShootOnce();
            }
            float delay = Random.Range(_minShootInterval, _maxShootInterval);
            yield return new WaitForSeconds(delay);
        }
    }

    //позволяя снаряду лететь в нужном направлении
    private void ShootOnce()
    {
        foreach (Transform firePoint in _firePoints)
        {
            Vector3 spawnPosition = firePoint.position;
            Quaternion bulletDirection = firePoint.rotation;
            Instantiate(_bulletPrefab, spawnPosition, bulletDirection);
        }
    }
}
