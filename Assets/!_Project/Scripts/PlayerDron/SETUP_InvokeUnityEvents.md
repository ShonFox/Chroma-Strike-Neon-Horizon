# Настройка Invoke Unity Events — пошаговое руководство

## 1. Создание Input Actions Asset

1. В Project окне: **Правый клик → Create → Input Actions**
2. Назовите, например, `PlayerControls`
3. Дважды кликните — откроется редактор Input Actions

## 2. Настройка Actions

В редакторе Input Actions создайте Action Map (например, `Gameplay`):

| Action Name | Action Type | Binding      | Клавиша  |
|-------------|-------------|--------------|----------|
| Turn        | Value       | 1D Axis      | A / D    |
| Boost       | Button      | Button       | W        |

### Как настроить Turn (A/D):
1. Нажмите `+` в разделе Action
2. Name: `Turn`, Action Type: `Value`, Control Type: `Axis`
3. В разделе Bindings нажмите `+` → `Add 1D Axis Composite`
4. Настройте:
   - **Negative** (влево): Binding → Key `A`
   - **Positive** (вправо): Binding → Key `D`
   - **Composite** → название `Turn Axis`

### Как настроить Boost (W):
1. Нажмите `+` в разделе Action
2. Name: `Boost`, Action Type: `Button`
3. Binding → `Key` → `W`

## 3. Настройка PlayerInput на объекте

1. Выделите ваш объект игрока в Hierarchy
2. **Add Component → Player Input**
3. В компоненте Player Input:
   - **Actions**: перетащите ваш `PlayerControls` asset
   - **Default Map**: `Gameplay`
   - **Behavior**: `Invoke Unity Events`

## 4. Привязка методов к событиям (Invoke Unity Events)

После выбора `Invoke Unity Events` в PlayerInput появится
раздел **Events → Gameplay** (или имя вашей Action Map).

### Action "Turn":
1. Нажмите `+` в разделе `Turn`
2. Перетащите объект с `BallController` в поле Object
3. В выпадающем списке выберите:
   `BallController → OnTurn (InputAction.CallbackContext)`

### Action "Boost":
1. Нажмите `+` в разделе `Boost`
2. Перетащите объект с `BallController` в поле Object
3. В выпадающем списке выберите:
   `BallController → OnBoost (InputAction.CallbackContext)`

## 5. Итоговая структура объектов в Hierarchy

```
Player (GameObject)
├── BallController.cs          ← ввод игрока
├── ObjectMovement.cs          ← физика движения
├── EnergySystem.cs            ← энергия
├── PlayerInput (component)    ← Input System, Behavior = Invoke Unity Events
└── Rigidbody (component)      ← физика

Main Camera (GameObject)
└── CameraController.cs        ← следит за игроком сверху

Canvas (GameObject)
├── EnergyUI.cs                ← вывод энергии на экран
└── Slider (UI element)         ← полоска энергии
```

## 6. Настройка компонентов в инспекторе

### BallController:
- **Object Movement**: перетащите сам объект Player (на котором висит ObjectMovement)
- **Energy System**: перетащите сам объект Player (на котором висит EnergySystem)
- **Turn Speed**: 180 (скорость поворота, град/сек)

### ObjectMovement:
- **Base Speed**: 5 (базовая скорость)
- **Boost Multiplier**: 2 (во сколько раз ускоряется)
- **Acceleration**: 10 (скорость разгона)
- **Boost Acceleration**: 20 (скорость разгона при бусте)

### EnergySystem:
- **Max Energy**: 100
- **Drain Rate**: 30 (расход в сек)
- **Regen Rate**: 15 (восстановление в сек)
- **Regen Delay**: 1 (задержка перед восстановлением, сек)

### CameraController:
- **Target**: перетащите объект Player
- **Height**: 15 (высота камеры)
- **Follow Speed**: 5
- **Map Min/Max X/Z**: границы карты (-50..50 по умолчанию)

### EnergyUI:
- **Energy Slider**: перетащите UI Slider
- **Energy System**: перетащите объект с EnergySystem

## 7. Настройка Rigidbody

На объекте Player:
- **Use Gravity**: ❌ выключить (объект не должен падать)
- **Is Kinematic**: ❌ выключить (нужна физика скорости)
- **Constraints**: Freeze Position Y (чтобы объект не улетал вверх/вниз)
  - Скрипт ObjectMovement уже замораживает вращение по X и Z
  - Дополнительно заморозите Position Y в инспекторе Rigidbody

## 8. Проверка

После настройки:
1. Нажмите Play
2. Объект сразу начнёт двигаться вперёд
3. A/D поворачивают объект
4. W включает ускорение, тратит энергию
5. Отпустили W — энергия восстанавливается
6. Камера следует сверху, не выходит за границы
7. Полоска энергии на экране обновляется
