# Cursor Path B panel-route repair run

## Что решал этот шаг

Этот узкий run проверял только маршрут открытия панели в Cursor Extension Development Host: команда `XIIGen: Open Smart Skills Panel` должна открыть видимую панель `XIIGen Smart Skills`. Сценарный prompt в этом run не вводился и не отправлялся.

Итог: `ACCEPTED_ROUTE_PROVEN`. Маршрут панели доказан как route-only evidence, не как полный Path B сценарий.

## Какая UI-control стратегия была проверена

Сначала был повторен безопасный вариант с command palette и точным текстом `Open Smart Skills Panel`, но clipboard paste не заполнил строку поиска. Скриншот `screenshots/02-panel-command-before-activation.png` показывает, что нужная команда не была выбрана, поэтому Enter из этого состояния не нажимался.

Затем использована typed-search стратегия: открыть command palette через F1, набрать `Open Smart Skills Panel`, убедиться по скриншоту, что выделена строка `XIIGen: Open Smart Skills Panel`, и только после этого нажать Enter. Скриншот `screenshots/06-typed-panel-command-before-activation.png` показывает выбранную команду до активации. Скриншот `screenshots/03-immediate-before-enter.png` повторно подтверждает тот же выбранный panel command непосредственно перед Enter.

## Что показывают скриншоты

- `screenshots/02-panel-command-before-activation.png`: первая попытка не доказала выбор panel command; активация из этого состояния не выполнялась.
- `screenshots/06-typed-panel-command-before-activation.png`: command palette показывает выделенную команду `XIIGen: Open Smart Skills Panel`.
- `screenshots/03-immediate-before-enter.png`: перед Enter всё ещё выделена команда `XIIGen: Open Smart Skills Panel`.
- `screenshots/04-after-panel-command-activation.png`: первый снимок после Enter ещё не показывает панель.
- `screenshots/07-late-after-panel-command-activation.png`: видна вкладка и панель `XIIGen Smart Skills`, значит panel route доказан.

## Вердикт маршрута

- Route proven: `true`.
- Status: `ACCEPTED_ROUTE_PROVEN`.
- Prompt typed: `false`.
- Prompt submitted: `false`.
- Secrets captured: `false`; секреты не вводились и не снимались.

## Следующее действие

Следующий bounded run может использовать уже доказанный panel route как вход в отдельную проверку Path B prompt-response. Этот run не доказывает сценарный ответ и не должен считаться полным Path B сценарием.
