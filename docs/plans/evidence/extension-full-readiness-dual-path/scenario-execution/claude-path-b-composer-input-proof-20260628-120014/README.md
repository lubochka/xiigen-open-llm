# Claude Path B: проверка ввода в composer

Финальный статус: `BLOCKED_UIA_HELPER_ARGUMENT_TYPES`.

Этот пакет проверял только один безопасный шаг: можно ли подтвердить, что подготовленный prompt находится в composer Claude, не нажимая Enter и не отправляя prompt.

Что фактически удалось подтвердить:

- Runner прочитал исходный prompt по hash/длине и не записывал raw prompt в evidence.
- Runner прочитал preflight с маршрутом `CLAUDE_ROUTE_READY`.
- Было найдено видимое окно Claude; инвентарь записан в `logs/window-inventory.json`.
- Отправка не выполнялась: `prompt_sent=false`, `enter_sent=false`.

Где выполнение остановилось:

- До выбора маршрута, поиска composer и ввода prompt runner вызвал UIA helper `Get-RedactedUiaSummary`.
- Helper упал при построении redacted summary: `Argument types do not match`.
- В `logs/exception-details.json` тот же сбой привязан к `run/claude-composer-input-proof.ps1`, строка 377, внутри `Get-RedactedUiaSummary`, строка 220.

Что этим пакетом не доказано:

- маршрут был выбран в текущем запуске;
- composer был найден;
- prompt был напечатан или вставлен;
- prompt подтвержден в composer;
- был получен ответ Claude;
- есть core/adapter correlation;
- token savings, semantic quality, learnability/training или checkpoint promotion.

Следующий минимальный repair: исправить только UIA helper/composer detection и повторить этот же composer-input proof без отправки prompt.
