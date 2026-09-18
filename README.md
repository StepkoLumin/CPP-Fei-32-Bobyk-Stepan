# CrossApp
Наскрізний проєкт з крос-платформного програмування.

## Предметна область
**Склад**. Сутності: Product (товар), StockBatch (партія), Warehouse (склад), Movement (переміщення).
Призначення: облік залишків товарів по партіях.

*Домовленість про каталоги в Core (на майбутнє):*
- `Core/Dto/` — record-типи формату даних
- `Core/Domain/` — сутності з поведінкою та інваріантами
- `Core/Storage/` — реалізації сховищ

## Структура Solution
- `src/Cli` — точка входу (консольний застосунок).
- `src/Core` — спільна бібліотека (Class Library) з допоміжною логікою.

## Команди для роботи
- **Збірка:** `dotnet build`
- **Запуск:** `dotnet run --project src/Cli`
- **Публікація (Framework-dependent):** `dotnet publish src/Cli -c Release -r win-x64 --self-contained false`
- **Публікація (Self-contained):** `dotnet publish src/Cli -c Release -r win-x64 --self-contained true`

## Порівняння режимів публікації (win-x64)
| Режим | Розмір publish | Потрібен встановлений runtime |
| :--- | :--- | :--- |
| Self-contained | ~70 МБ | Ні |
| Framework-dependent | ~0.2 МБ | Так (.NET 10) |