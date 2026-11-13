# Cursor real UI scenario after local unlock/login

## Человеческий статус этого slice

Фаза 4 из 10: живой runtime в реальном Cursor UI.

Статус: blocked until local unlock/login, but scenario prep can be completed.

Всего действий в этом prep-slice: 4.
Сделано: 4.
В процессе: 0.
Осталось: 0 внутри prep-slice.

Что этот файл решает: он описывает точный сценарий для будущего запуска Cursor UI, чтобы агент не угадывал, куда нажимать, что вводить, какие доказательства собирать и где остановиться без overclaim.

Product readiness count remains 0.

## Preconditions before execution

1. Windows уже локально разблокирован пользователем.
   - no PIN/password captured.
   - Агент не вводит PIN, пароль, код подтверждения, recovery phrase или другие секреты.
   - Если экран снова заблокирован, сценарий заканчивается как `BLOCKED_LOGIN_REQUIRED`.

2. Целевое окно Cursor видно на рабочем столе:
   - ожидаемый title: `[Extension Development Host] cursor-xiigen-smart-skills - Cursor`.
   - Перед любым действием агент делает window inventory и screenshot.
   - Если целевое окно не найдено, сценарий заканчивается как `TARGET_WINDOW_NOT_FOUND`.

3. Если Cursor показывает, что нужен login:
   - открыть browser login можно только если пользователь физически рядом и сам выполняет credential entry;
   - агент обязан остановиться до ввода credentials;
   - после того как пользователь завершил login, агент возвращается к preflight и делает новый screenshot без секретов.

4. Если виден `Storage Error` modal:
   - сначала сделать screenshot и записать текст modal;
   - безопасные действия: `Close`, `Cancel`, `Dismiss`, `Retry`, если они не обещают удаление данных;
   - нельзя нажимать destructive options вроде `Reset`, `Delete`, `Clear storage`, `Factory reset`, если сценарий явно не разрешил это отдельно;
   - если modal неясен или мешает сценарию, verdict `BLOCKED_STORAGE_MODAL`.

## Dual-path same-scenario proof

Этот сценарий должен быть доказан двумя путями с одним и тем же смысловым запросом.

### Path A: automated/boundary path

Цель Path A: доказать, что тот же redacted prompt проходит через adapter/source boundary к `llm_mvp_core`, без реального Cursor UI.

Что должно быть в evidence Path A:

1. Redacted prompt ровно из раздела "Exact prompt".
2. `XiigenExtensionRequest` создан Cursor adapter path.
3. `RequestedSkills` переданы в core.
4. `AvailableSkills` переданы в core.
5. Core response возвращает structured task / orchestration refs / selected skills или честный warning.
6. Host trace показывает, что Cursor host не добавил свой selector/scorer:
   - ожидаемый факт: `hostSelectorOrScorerAdded=false`.
7. Если Path A уже был доказан старым packet, новый executor должен либо сослаться на конкретный evidence folder, либо rerun тот же boundary runner и сохранить fresh result рядом с Path B.

Path A не доказывает живой Cursor UI. Он нужен как пара к Path B, чтобы UI-сценарий сравнивался с тем же входом, а не с другим тестом.

### Path B: real Cursor UI path

Цель Path B: доказать, что пользовательский запрос реально вводится в Cursor UI, extension принимает его, вызывает XIIGen/core и показывает результат в Cursor UI.

Шаги Path B:

1. Сделать screenshot рабочего стола до действий.
2. Найти окно `[Extension Development Host] cursor-xiigen-smart-skills - Cursor` по title, process id и bounds.
3. Активировать именно это окно.
4. Сделать screenshot активного Cursor target window.
5. Если открыт settings/login/error modal, выполнить правила из Preconditions и не продолжать, пока состояние не стало безопасным.
6. Открыть XIIGen Smart Skills route одним из разрешённых способов, в таком порядке:
   - Cursor command palette: открыть команду extension, если она видна как `XIIGen Smart Skills` или `xiigen.smartSkills.select`;
   - если есть dedicated panel, открыть panel и выбрать prompt input;
   - если есть chat participant route, открыть chat и выбрать XIIGen participant.
7. Ввести exact prompt из раздела "Exact prompt".
8. Сделать screenshot перед submit, где видно, что prompt введён, но секретов нет.
9. Submit prompt.
10. Дождаться ответа или timeout, заранее записав timeout value в evidence.
11. Сделать screenshot после ответа.
12. Сохранить core trace/correlation id, если extension показывает или пишет его.
13. Сохранить adapter trace, если extension пишет локальный trace.
14. Сравнить ответ с ожидаемыми visible markers.

Ожидаемые visible markers после ответа:

- Видно, что ответ относится к XIIGen Smart Skills, а не к обычному Cursor chat.
- Видно хотя бы одно из:
  - structured task / задача;
  - orchestration plan / план оркестрации;
  - selected skills / выбранные скилы;
  - warning/error от core, если core честно отказал.
- Нет UI-only fake result без core trace.

## Exact prompt

Использовать ровно этот prompt, без личных данных:

```text
Проверь короткий запрос: "Нужно составить понятный план проверки API-дешборда без привязки к конкретному стеку". Сформируй универсальную задачу, предложи план оркестрации и выбери подходящие скилы из доступных.
```

Почему этот prompt выбран:

- он короткий и безопасный;
- он не содержит секретов, имён клиентов, токенов или приватных документов;
- он проверяет три нужные смысловые части: prompt -> task, task -> orchestration plan, task/plan -> skills;
- он требует универсальную часть, не привязанную к конкретному стеку.

## Acceptance criteria

`PASS`:
- Path A и Path B выполнены с тем же exact prompt;
- Path A содержит `XiigenExtensionRequest`, `RequestedSkills`, `AvailableSkills`, core response и trace без host selector/scorer;
- Path B показывает prompt в реальном Cursor UI до submit и ответ после submit;
- есть screenshot до submit и после response;
- есть trace или correlation id, связывающий UI run с core call;
- no PIN/password captured;
- no product readiness overclaim beyond this scenario.

`PARTIAL`:
- Path A прошёл, но Path B дал только часть UI evidence;
- или Path B показал UI response, но trace/correlation id отсутствует;
- или UI показывает честный core warning/error, но не подтверждает полный success.

`BLOCKED_LOGIN_REQUIRED`:
- Windows/Cursor/browser просит login, PIN, password, 2FA или другой секрет;
- агент остановился до credential entry;
- screenshot/redaction log сохранены без секретов.

`BLOCKED_STORAGE_MODAL`:
- `Storage Error` modal мешает сценарию;
- безопасное действие неясно;
- screenshot сохранён, destructive option не нажималась.

`TARGET_WINDOW_NOT_FOUND`:
- окно `[Extension Development Host] cursor-xiigen-smart-skills - Cursor` не найдено в window inventory.

`RESPONSE_NOT_VISIBLE`:
- prompt был отправлен, но ответ в Cursor UI не появился или не виден на screenshot.

`CORE_TRACE_MISSING`:
- UI ответ виден, но нет core request/response/trace/correlation id, поэтому нельзя доказать, что ответ пришёл из `llm_mvp_core`.

## Evidence required after execution

После будущего запуска сценария нужно сохранить:

1. Window inventory:
   - title;
   - process id;
   - handle;
   - bounds;
   - выбранный target window.
2. Screenshots:
   - desktop before actions;
   - Cursor target before input;
   - prompt typed before submit;
   - response visible after submit;
   - login/storage/error blocker screenshots if any.
3. Core request/response/trace:
   - redacted `XiigenExtensionRequest`;
   - `RequestedSkills`;
   - `AvailableSkills`;
   - core response;
   - correlation id if available.
4. Adapter trace:
   - route used by Cursor extension;
   - whether host added selector/scorer;
   - error/warning if adapter failed.
5. No-secret/redaction log:
   - confirms no PIN/password/token was captured;
   - lists any screenshots redacted.
6. Human summary:
   - what was tested;
   - what passed;
   - what was blocked;
   - what this proves;
   - what it does not prove.

## What this scenario will not prove

Даже если этот Cursor UI scenario пройдёт, он сам по себе не доказывает:

- token savings;
- semantic quality improvement;
- learnability/training;
- DPO quality;
- checkpoint mutation/promotion;
- locator update;
- full Claude readiness;
- full Codex readiness;
- product readiness всех extensions.

Для этих claims нужны отдельные сценарии, отдельные метрики и отдельные evidence packets.

## Format requirements for future evidence

- All text files UTF-8 no BOM.
- LF line endings.
- No trailing whitespace.
- `scenario.json` parse clean.
- Screenshots must not show secrets.
- Any blocked result must be reported as blocked, not converted into pass.
