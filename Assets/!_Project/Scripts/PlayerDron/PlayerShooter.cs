using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    //Префаб снаряда (тот же, что у врагов).
    [SerializeField] private GameObject _bulletPrefab;

    //МАССИВ точек выстрела: стволы, пушки, дроны-пакеты и т.п.
    //Заполняется в инспекторе: задать Size и перетащить объекты FirePoint.
    [SerializeField] private Transform[] _firePoints;

    //Ссылка на счётчик патронов (компонент AmmoInventory на этом же объекте).
    [SerializeField] private AmmoInventory _ammoInventory;

    //true — стреляет очередью, пока зажата ЛКМ.
    //false — один выстрел на клик.
    [SerializeField] private bool _fireOnHold = false;

    private void Update()
    {
        //Работаем через Input System (как в остальном проекте).
        var mouse = UnityEngine.InputSystem.Mouse.current;
        if (mouse == null) return;

        bool wantShoot = _fireOnHold
            ? mouse.leftButton.isPressed            //Зажата — стреляем каждый кадр.
            : mouse.leftButton.wasPressedThisFrame; //Только что кликнули — один выстрел.

        if (wantShoot)
        {
            Shoot();
        }
    }

    //Один выстрел: тратим ОДИН патрон и стреляем ИЗ ВСЕХ точек одновременно.
    private void Shoot()
    {
        //Защита: массив пуст или не заполнен — просто не стреляем, без ошибки.
        if (_firePoints == null || _firePoints.Length == 0)
        {
            return;
        }

        //Сначала списываем патрон. Нет патронов — нет выстрела.
        //Попадание в каждую точку отдельно не списываем: один клик = один снаряд по счёту.
        if (!_ammoInventory.TryConsume(1))
        {
            return;
        }

        foreach (Transform firePoint in _firePoints)
        {
            if (firePoint == null) continue;

            //Каждая точка списывает свой патрон; нет патронов — эта точка молчит.
            if (!_ammoInventory.TryConsume(1)) break;

            Instantiate(_bulletPrefab, firePoint.position, firePoint.rotation);
        }

    }
}
