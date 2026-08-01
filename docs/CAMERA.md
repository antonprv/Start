# Camera Component — trait-based rewrite

Переписал `CameraComponent` по тому же принципу, что и `MoverComponent`: вместо одного
монолитного набора настроек — маленькие независимые **трейты** (`ICameraTrait`), которые
складываются в список и каждый кадр прогоняются через `CameraMotor`. Комбинируя трейты,
получаются разные "ощущения" камеры без дублирования кода.

Референсом по скелету взял ваш `Mover` (`PreProcess/Process/PostProcess`, `MovementMotor`,
`MovementTraitResource`/`MovementPreset` для инспектора) — структура 1:1. Из присланного
плагина `GameplayCameras` (Unreal) взял саму идею "нода = маленький кусок поведения,
пайплайн собирает финальную позу камеры", но не стал тащить их полноценный граф нод с
отдельными Definition/Evaluator — это избыточно для того, что вы просили ("по принципу
MoverComponent").

## Структура файлов

```
Framework/Components/Camera/
  Core/
    CameraContext.cs        — снимок инпута/таргета за кадр (аналог MovementContext)
    CameraRigState.cs        — persistent состояние, которое трейты читают/пишут
                                (аналог Velocity, только богаче: Yaw/Pitch/ArmLength/
                                LocalOffset/FOV + "мусорные" транзиентные поля Overshoot*/
                                ExtraDistance)
    CameraPose.cs             — маленькая структура для рантайм-блендинга поз камеры
    CameraMotor.cs            — аналог MovementMotor
    CameraMode.cs             — 5 готовых режимов + Custom
    Interfaces/               — ICameraTrait, ICameraMotor, ICameraPreset
    Resources/                — CameraTraitResource, CameraPreset (инспектор/.tres,
                                 аналог MovementTraitResource/MovementPreset)
  Traits/
    Follow/FollowTargetTrait.cs         — пивот = позиция таргета + высота глаз
    Rotation/MouseLookTrait.cs          — свободный look (FPS/TPS)
    Rotation/FixedAngleTrait.cs         — зафиксированный pitch/yaw (Diablo)
    Rotation/OrbitDragTrait.cs          — вращение только пока зажата кнопка (BG3)
    Pan/EdgeScrollPanTrait.cs           — панорамирование курсором к краю экрана
    Distance/FixedArmLengthTrait.cs     — constant расстояние
    Distance/ScrollZoomTrait.cs         — зум колесом мыши в диапазоне min/max
    Pose/PoseTrait.cs                   — плавное переключение поз камеры через код
    Rotation/AutoRecenterTrait.cs       — Souls-style авто-доворот камеры за спину (см. ниже)
    Feel/CameraLagTrait.cs              — Camera Lag (описано ниже)
    Feel/CameraOvershootTrait.cs        — Camera Overshoot (описано ниже)
    Feel/SmoothingTrait.cs              — финальное сглаживание Yaw/Pitch/ArmLength/FOV
  Presets/
    FirstPersonPreset.cs
    FirstPersonOvershootPreset.cs
    ThirdPersonShoulderPreset.cs
    ThirdPersonSoulslikePreset.cs   (Elden Ring)
    GeneralThirdPersonPreset.cs     (Soulslike + Lag + Overshoot — дефолт игры)
    TopDownFixedPreset.cs      (Diablo)
    TopDownOrbitPreset.cs      (Baldur's Gate 3)
    EdgeScrollTopDownPreset.cs (RTS edge-scroll)

Setup/Engine/Components/Camera/
  CameraComponent.cs  — сам Node3D-компонент: собирает CameraContext, гоняет CameraMotor,
                         применяет итоговый CameraRigState к SpringArm3D/Camera3D
  Examples/            — по одному примеру на каждый пресет + пример хоткей-переключения
                          (см. "Примеры" ниже — почему они лежат в Setup, а не рядом с
                          пресетами во Framework)
```

## Как это работает

Каждый трейт реализует `PreProcess/Process/PostProcess`, как `IMovementTrait`. `Process`
читает `CameraContext` (ввод, позиция/скорость таргета) и правит `CameraRigState` — общий
"рабочий" объект позы камеры. `CameraMotor` хранит этот `State` между кадрами (как
`MovementMotor` хранит `Velocity`) и прогоняет через него список трейтов.

После `Simulate()` `CameraComponent` сам конвертирует финальный `State` в трансформ сцены:
позиция = `PivotPosition + PanOffset + (rotation * LocalOffset)`, поворот применяется на
`SpringArm3D` (как раньше), `SpringArm3D.SpringLength = ArmLength + ExtraDistance`, FOV —
на `Camera3D`. Сам collision-sweep у пружины не трогал — `BepuSpringArm3D` как и раньше
физически укорачивает руку при столкновении, трейты лишь просят "идеальную" длину.

## Camera Lag

`CameraLagTrait`: каждый кадр берёт горизонтальную скорость таргета
(`CameraContext.TargetVelocity`, её сам `CameraComponent` считает через дельту позиции —
специально не завязывался на `IMoverComponent.Velocity`, чтобы камера работала с любым
таргетом, не только с Mover) и добавляет доп. дистанцию к руке:
`desired = Clamp(speed * LagPerSpeed, 0, MaxLagDistance)`. К этой цели `ExtraDistance` идёт
с **разной** скоростью: быстро растёт (`BuildSpeed`), когда персонаж разгоняется, и медленно
возвращается (`RecoverSpeed`), когда останавливается — именно асимметрия и даёт ощущение
"не поспевает / плавно подбирается", как вы описали.

## Camera Overshoot (исправлено)

Изначально я реализовал Overshoot как покачивание угла (Yaw/Pitch перелетают чуть дальше
цели и возвращаются) — но вы имели в виду другое: как в **Metro Gravity** (mrkogamedev),
при быстром вращении камеры она должна **отлетать дальше по дистанции** (вдоль руки), а
потом возвращаться на своё место. Переделал именно так.

`CameraOvershootTrait` теперь смотрит на то же самое (дельту `TargetYaw/TargetPitch` между
кадрами → величину в градусах, насколько резко крутите камеру), но результат идёт не в
поворот, а в новое поле `CameraRigState.OvershootDistance` — отдельное от `ExtraDistance`
(которое занято под Camera Lag), чтобы они не конфликтовали. Оба в финале одинаково
прибавляются к длине пружины в `CameraComponent.ApplyRigState`:
`SpringLength = ArmLength + ExtraDistance + OvershootDistance`.

Логика такая же асимметричная, как у Camera Lag (и это тот же паттерн `MoveTowards` с
разными скоростями туда/обратно):

```
desired = Clamp(angularDelta * OvershootPerDegree, 0, MaxOvershootDistance)
rate    = desired > OvershootDistance ? BuildSpeed : RecoverSpeed
OvershootDistance = MoveTowards(OvershootDistance, desired, rate * delta)
```

Быстро крутите камеру → `OvershootDistance` быстро растёт к пропорциональному значению
(`BuildSpeed`) → камера "отлетает" дальше по руке. Останавливаете вращение → `desired`
падает до нуля → `OvershootDistance` медленно (`RecoverSpeed`) возвращается к нулю, и
камера плавно едет назад на своё нормальное расстояние — ровно эффект из Metro Gravity.

Трейт по-прежнему ничего не знает, что стоит перед ним в списке (MouseLook/OrbitDrag/
FixedAngle/AutoRecenter) — реагирует на любой источник вращения одинаково. В First Person
(`ArmLength = 0`) тот же эффект работает как лёгкий "отъезд" камеры назад при резком
повороте — оставил числа маленькими (`MaxOvershootDistance = 0.15`), чтобы не выглядело как
клиппинг сквозь голову персонажа.

## Third Person Soulslike (Elden Ring) и General Third Person

`ThirdPersonSoulslikePreset` — и ракурс, и движение камеры как в Elden Ring:

- **Ракурс**: камера почти по центру за спиной (небольшой сдвиг вправо `(0.15, 0.35, 0)`
  вместо выраженного over-the-shoulder), сидит выше и дальше (`ArmLength = 4.2`, без
  зума колесом — в Souls-играх нет дальнего/ближнего зума), pitch уже, чем у обычной TPS
  (`-65°..45°` — вниз не так далеко, вверх тоже ограниченно).
- **Движение**: сглаживание вращения/позиции чуть тяжелее, чем в `ThirdPersonShoulder`
  (`RotationSmoothSpeed=10`, `PositionSmoothSpeed=9` вместо 18/10) — камера ощущается не
  дёрганой, а более "весомой". Плюс фирменное поведение Souls-камеры —
  **`AutoRecenterTrait`**: если `IdleDelay` секунд (по умолчанию 1с) не трогать мышь/стик,
  и персонаж при этом движется (`OnlyWhileMoving`+`MinSpeedToRecenter`), yaw сам плавно
  доворачивается за спину персонажа (`RecenterSpeed` град/сек) — ровно то, что происходит
  в Elden Ring на длинном спринте без вмешательства в камеру. Как только тронули
  look-инпут — довороты сразу прекращаются.

`ThirdPersonSoulslikePreset` **специально без** Camera Lag/Overshoot — как вы и просили,
чистый ракурс+движение Elden Ring.

`GeneralThirdPersonPreset` = тот же набор трейтов + `CameraLagTrait` + `CameraOvershootTrait`
поверх. **Это новый режим по умолчанию** — `CameraComponent.InitialMode` теперь
`CameraMode.GeneralThirdPerson` вместо старого `ThirdPersonShoulder`.

## First Person Overshoot

`FirstPersonOvershootPreset` = `FirstPersonPreset` + `CameraOvershootTrait`, но с сильно
меньшими числами, чем в третьем лице (`OvershootPerDegree=0.01`, `MaxOvershootDistance=0.15м`
против `0.035`/`1.2м`) — от первого лица небольшой "отъезд назад" на резкий поворот читается
как вес, а слишком большой быстро выглядит как камера проваливается сквозь голову персонажа
или вызывает укачивание.

## Плавное переключение поз через код

`PoseTrait` держит "домашнюю" позу (`DefaultOffset`/`DefaultArmLength`/`DefaultFOV`) и умеет
блендить к новой по `SetPose(pose, duration)` за явно заданное время (smoothstep, а не
экспоненциальное сглаживание — 0.35с всегда будут 0.35с, а не "почти доехало"). Наружу это
торчит как:

```csharp
// персонаж пригнулся — камера чуть выше и ближе, полсекунды на переход
_cameraComponent.TransitionToPose(new Vector3(0.3f, 0.55f, 0f), armLength: 2.5f, duration: 0.5f);

// встал и пошёл — обратно к обычному "из-за плеча"
_cameraComponent.TransitionToPose(new Vector3(0.55f, 0.25f, 0f), armLength: 3.5f, duration: 0.35f);
```

Метод ищет `PoseTrait` в текущем активном списке трейтов через `_activeTraits.OfType<PoseTrait>()`
— если в режиме его нет (например, в `TopDownFixed`), просто no-op.

## FOV наружу

`CameraComponent` имеет отдельную `[ExportGroup("Field of View")]` с `DefaultFOV`/`MinFOV`/
`MaxFOV` — не зависит ни от одного трейта и применяется как финальный clamp в
`ApplyRigState`, так что независимо от режима итоговый FOV всегда в этих границах. Плюс
FOV можно переопределять точечно на уровне позы (`CameraPose.FOV`, через `PoseTrait`) —
например, чтобы прицеливание сужало FOV, а спринт — расширял, оставаясь в тех же
глобальных Min/Max.

## 9 готовых режимов (CameraMode)

| Mode                     | Состав трейтов                                                                 |
|--------------------------|----------------------------------------------------------------------------------|
| `FirstPerson`             | Follow(eye height) → MouseLook(±85°) → FixedArm(0) → Pose(0) → Smoothing(instant) |
| `FirstPersonOvershoot`    | то же + `Overshoot(маленький)` перед Smoothing                                   |
| `ThirdPersonShoulder`     | Follow → MouseLook → ScrollZoom → Pose(over-the-shoulder) → Lag → Overshoot → Smoothing |
| `ThirdPersonSoulslike` (Elden Ring) | Follow → MouseLook(±65°/45°) → AutoRecenter → FixedArm(4.2) → Pose(near-centered) → Smoothing(heavier) |
| `GeneralThirdPerson` **(дефолт)** | то же, что Soulslike, + `Lag` + `Overshoot` перед Smoothing            |
| `TopDownFixed` (Diablo)   | Follow(ground) → FixedAngle(-55°, yaw закреплён) → FixedArm(14) → Pose(0) → Smoothing(position only) |
| `TopDownOrbit` (BG3)      | Follow → OrbitDrag(вращение по ПКМ) → EdgeScrollPan → ScrollZoom → Pose(0) → Smoothing |
| `EdgeScrollTopDown` (RTS) | Follow(leash) → FixedAngle → EdgeScrollPan → FixedArm → Pose(0) → Smoothing       |

(`Pose(0)` там, где сдвига камеры по умолчанию нет, — это `PoseTrait { DefaultOffset =
Vector3.Zero }`, добавлен во все режимы без выраженного шоулдер-сдвига, чтобы `LocalOffset`
корректно обнулялся при хот-свапе из режима, где он был ненулевым, а не залипал.)

Переключение в рантайме — `cameraComponent.SetCameraMode(CameraMode.FirstPerson)`, состояние
(`Yaw/Pitch/ArmLength/FOV/LocalOffset`) сохраняется, поэтому переход не дёргается — просто
меняется, кто что после этого контролирует (см. пример хоткей-переключения ниже). Свой набор
трейтов — `CameraMode.Custom` + `[Export] CameraPreset CustomPreset` (собирается как `.tres`
в инспекторе, как у Mover'а).

## Примеры

`Setup/Engine/Components/Camera/Examples/` — по одному маленькому скрипту-примеру на каждый
пресет, плюс один пример на плавное переключение по хоткею.

**Почему они лежат в `Setup/`, а не рядом с пресетами в `Framework/Components/Camera/`**:
примеры используют сам `CameraComponent` (Godot `Node3D`, живёт в `Setup`), а зависимость
между проектами однонаправленная — `Setup.csproj` ссылается на `Framework/Components/
Components.csproj`, а не наоборот. Если положить пример, использующий `CameraComponent`,
во `Framework`, он не соберётся (`Framework.Components` физически не видит типы из `Setup`).
Поэтому "рядом" здесь — рядом с самим `CameraComponent.cs`, а не с папкой `Presets`.

Список:

| Файл                                                    | Что показывает |
|-----------------------------------------------------------|----------------|
| `FirstPersonExample.cs`                                    | `SetCameraMode(CameraMode.FirstPerson)` |
| `FirstPersonOvershootExample.cs`                            | `SetCameraMode(CameraMode.FirstPersonOvershoot)` |
| `ThirdPersonShoulderExample.cs`                             | режим + бонус: `TransitionToPose(...)` по зажатию C — камера плавно едет выше/ближе на "присед" и обратно |
| `ThirdPersonSoulslikeExample.cs`                            | `SetCameraMode(CameraMode.ThirdPersonSoulslike)` |
| `GeneralThirdPersonExample.cs`                              | тот самый дефолтный режим (можно и не выставлять — уже `InitialMode`) |
| `TopDownFixedExample.cs`                                    | `SetCameraMode(CameraMode.TopDownFixed)` |
| `TopDownOrbitExample.cs`                                    | `SetCameraMode(CameraMode.TopDownOrbit)` |
| `EdgeScrollTopDownExample.cs`                                | `SetCameraMode(CameraMode.EdgeScrollTopDown)` |
| `GeneralThirdPersonToFirstPersonSwitchExample.cs`            | по **V** плавно переключается `GeneralThirdPerson ↔ FirstPerson` |

Последний — самый содержательный: никакого ручного твина внутри нет. `SetCameraMode` меняет
только список активных трейтов, а `CameraRigState` (Yaw/Pitch/ArmLength/LocalOffset/FOV)
переживает свап без изменений — новый режим просто начинает тянуть *текущие* значения к
своим целям через `SmoothingTrait`/`PoseTrait`, поэтому переход плавный сам по себе, без
дополнительного кода в примере.

## Совместимость с существующим кодом

`MoverComponent` использует только `GetForwardDirection()`, `GetRightDirection()` и
`SetNoclip()` — все три сохранены с той же сигнатурой. Остальной публичный API (`SetFOV`,
`SetSpringArmLength`, `SetCameraPosition`, `SetFollowTarget`, `GetYaw`/`GetPitch`,
`AddRotation`) тоже сохранён по именам, но теперь всё это просто читает/пишет `CameraMotor.State`.

Иерархия сцены не меняется: `CameraComponent (Node3D) → SpringArm3D → Camera3D` — старые
сцены заведутся, просто у `CameraComponent` в инспекторе исчезли старые поля
(`MinPitch`/`MaxPitch`/`PositionLagSpeed`/`RotationLagSpeed` и т.п. — они переехали в
трейты внутри пресетов) и появились новые (`InitialMode`, `CustomPreset`,
`MouseSensitivity`, `DefaultFOV`/`MinFOV`/`MaxFOV`, `EdgeScrollEnabled`/`EdgeScrollMarginPx`,
`OrbitButton`).

## Важное предупреждение

**В этой песочнице нет Godot SDK / dotnet и нет доступа к nuget.org**, поэтому я не смог
реально скомпилировать проект — только вручную сверил каждый вызов (`FMath.*`,
`FastLerp`/`FastNormalized`/`LengthSq`, `Basis.FromEuler(..., EulerOrder.Yxz)`,
`Godot.Collections.Array<T>.Cast<T>()`, DI-паттерн `[Inject]`/`DiContainer.Instance.Inject`,
`GameLogger.LogInfo`) с тем, как это используется в вашем существующем коде (`MoverComponent`,
`FastMathExtensions`, `MovementPreset`), и проверил баланс скобок и совпадение имён полей
между `CameraContext`/`CameraRigState` и всеми трейтами. Но финальную сборку в Godot/Rider
всё равно стоит прогнать самому — я не гарантирую 100% отсутствие опечаток без реального
компилятора под рукой.

Также: `.cs.uid` файлы для новых скриптов Godot сгенерирует сам при первом импорте — вручную
их создавать не нужно.
