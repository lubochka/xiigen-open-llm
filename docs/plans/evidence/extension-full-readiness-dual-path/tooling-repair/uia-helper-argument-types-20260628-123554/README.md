# Финализация пакета UIA helper argument types

Статус: `BLOCKED_FIXED_PATTERN_UNVERIFIED`.

Что найдено:

- В папке были только `logs/` и `run/`; `README.md`, `action-log.md` и `result.json` отсутствовали до этой финализации.
- `logs/runner-exit-code.txt` содержит `1`.
- `logs/runner-stderr.log` показывает падение: `Get-UiaDescendantsFixedPattern : Argument types do not match`.
- `logs/runner-stdout.log` пустой.
- Скрипт `run/uia-helper-argument-types-diagnosis.ps1` содержит проблемный прямой вызов `RootElement.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)`.
- В том же скрипте была произведена заготовка безопаснее: типизированные локальные переменные `TreeScope` и `Condition`, узкий `try/catch` вокруг `FindAll`, переход на ограниченный Win32 `EnumChildWindows` fallback и нормализация массивов через `foreach`/списки.

Что не доказано:

- Успешный `result.json` от исходного прогона не был создан.
- Нет лога, где fallback helper успешно завершился.
- Нет доказательства, что исправленный паттерн проверен.
- Нет доказательства, что Claude runner безопасно повторять.

Следующий минимальный repair:

Выполнить только проверочный прогон fallback helper: отдельный маленький runner должен воспроизвести ветку `Argument types do not match`, обойти прямой UIA descendant enumeration через ограниченный Win32 `EnumChildWindows` fallback, записать stdout/stderr/exit/result и оставить любые Claude prompt/send действия выключенными.
