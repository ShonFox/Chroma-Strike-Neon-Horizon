# Top-Down Controller — инструкция по сборке

## Что входит в набор

| Файл                | Назначение                                                |
|---------------------|-----------------------------------------------------------|
| BallController.cs   | Обработка ввода: поворот (A/D), буст (W)                  |
| ObjectMovement.cs   | Автодвижение вперёд + физика + буст                       |
| EnergySystem.cs     | Расход/восстановление энергии                            |
| CameraController.cs | Камера сверху с ограничением по границам карты           |
| EnergyUI.cs         | Вывод энергии на экран (Slider + Text)                    |

## Порядок настройки в Unity

### 1. Объект игрока
1. Создайте объект (например, Sphere или вашу модель).
2. Добавьте на него **Rigidbody**.
   - В Rigidbody: Freeze Rotation → отметьте X и Z (Y оставьте).
   - Или это делает скрипт ObjectMovement автоматически (freezeRotation = true).
3. Добавьте компоненты в таком порядке:
   - **BallController**
   - **ObjectMovement**
   - **EnergySystem**
4. В BallController в инспекторе:
   - Object Movement → перетащите этот же объект (или ObjectMovement на нём).
   - Energy System → перетащите EnergySystem с этого же объекта.
   - Turn Speed → настройте (120 — средне, 360 — очень резко).
5. В ObjectMovement настройте:
   - Max Speed — обычная скорость.
   - Acceleration — как быстро разгоняется.
   - Boost Multiplier — во сколько раз быстрее при бусте.
   - Boost Acceleration — как быстро разгоняется при бусте.
6. В EnergySystem настройте:
   - Max Energy, Consume Rate, Regen Rate, Regen Delay.

### 2. Input System (ввод)
Если используете **новую Input System** (InputSystem package):
1. Создайте Input Actions: Assets → Create → Input Actions.
2. Добавьте Action Map (например, "Player").
3. Создайте действия:
   - **Turn** — тип Value → Add Binding → Keyboard → A и D (или Left/Right arrows).
     - Для 2D Vector Composite: Left = A, Right = D → вернёт -1 / +1.
     - Либо просто 1D Axis Composite: Left = A, Right = D.
   - **Boost** — тип Button → Add Binding → Keyboard → W (или Up arrow).
4. На объекте игрока добавьте компонент **Player Input**.
   - Actions → перетащите ваш Input Actions asset.
   - Behavior → Invoke Unity Events.
5. В BallController в инспекторе появятся события:
   - OnTurn → BallController.OnTurn
   - OnBoost → BallController.OnBoost

Если используете **старую Input Manager** — замените обработчики на Input.GetKey в FixedUpdate.

### 3. Камера
1. Выберите Main Camera.
2. Повесьте **CameraController**.
3. В инспекторе:
   - Target → перетащите объект игрока.
   - Camera Height → высота (15 — норм для небольшой карты).
   - Map Bounds → половина размера карты (50,50 = карта 100×100).
   - Follow Speed → 5 (плавно) или 0 (жёстко следит).
4. Камера автоматически смотрит вниз (Euler 90,0,0).

### 4. Энергия на экране (UI)
1. Создайте Canvas: GameObject → UI → Canvas (если ещё нет).
2. Добавьте Slider: GameObject → UI → Slider.
   - Он создаст иерархию: Slider → Background, Fill Area → Fill, Handle.
3. (Опц.) Добавьте Text рядом: GameObject → UI → Text.
4. Создайте пустой объект (или используйте сам Slider) и повесьте **EnergyUI**.
5. В инспекторе EnergyUI:
   - Energy Slider → ваш Slider.
   - Energy Text → ваш Text (если есть).
   - Energy System → объект игрока (где висит EnergySystem).
   - Fill Image → перетащите Fill из иерархии Slider (для смены цвета).
   - Цвета: Full Color (зелёный), Low Color (красный), Low Threshold (0.3 = 30%).

## Логика работы

1. Объект **всегда едет вперёд** (в сторону своего "носа").
2. **A/D** — поворот влево/вправо. Объект разворачивается, и направление движения меняется.
3. **W** — буст: временно едет быстрее. Тратит энергию.
4. **Энергия** восстанавливается сама, когда W не нажата (с задержкой).
5. **Камера** следует сверху и не выходит за границы карты.

## Дополнения, которые стоит рассмотреть

- **TrailRenderer** на игроке — красивый след за объектом.
- **Particle System** при бусте — визуальный эффект ускорения.
- **Звук** — звук ускорения, звук истощения энергии.
- **Препятствия/коллизии** — слой для стен и проверка в OnCollisionEnter.
- **Сбор энергии** — можно добавить пикапы, которые мгновенно восстанавливают энергию.
- **Постепенный поворот** через Rigidbody.MoveRotation вместо transform.rotation —
  если нужна более реалистичная физика поворота.
- **Screen shake** при бусте — лёгкое тряска камеры.
