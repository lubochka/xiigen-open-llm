# Claude UI scenario prep

## Человеческий статус этого slice

Всего действий: 4.
Сделано: 4.
В процессе: 0.
Осталось внутри этого slice: 0.

Что сделано:
1. Создана папка подготовки сценария Claude.
2. Описан один и тот же сценарий для двух путей проверки.
3. Зафиксированы preconditions, точные шаги, evidence и anti-substitution rules.
4. Подготовлены критерии PASS/PARTIAL/BLOCKED для будущего выполнения.

## Рабочая метрика: scenario coverage

Эта папка не заявляет готовность продукта. Рабочая метрика здесь - scenario coverage.

Сценарии всего: 1.
Принято как полностью покрыто: 0.
Покрыто только через API/boundary path: 0.
Покрыто только через UI path: 0.
Заблокировано: 0.
Еще не выполнялось: 1.

Причина: этот slice только готовит сценарий. Claude UI не запускался, prompt не отправлялся.

## Что этот сценарий должен проверить позже

Будущий агент должен доказать один и тот же пользовательский сценарий двумя путями:

Path A: non-UI Claude adapter/API boundary path.
Этот путь доказывает, что Claude adapter или повторяемый boundary runner строит `XiigenExtensionRequest`, передает `RequestedSkills` и `AvailableSkills`, вызывает `llm_mvp_core`, получает core response и не добавляет host-side selector/scorer.

Path B: real Claude UI path.
Этот путь доказывает, что агент реально работает с локальной Claude app/window, выбирает правильный thread/project/extension route, вводит тот же prompt, делает screenshot до submit, submit, screenshot после ответа, и собирает trace/correlation evidence, если оно доступно без секретов.

## Exact prompt

```text
Проверь универсальную оркестрацию для запроса пользователя: нужно принять просьбу, перевести ее в универсальную задачу, построить универсальный план действий и выбрать нужные навыки. Не привязывай ответ к конкретному языку программирования, IDE, стеку или внутренним файлам проекта. Верни коротко: 1) какая задача получилась, 2) какой универсальный план нужен, 3) какие навыки выбраны и почему, 4) какого обучающего сигнала не хватает, если система не уверена.
```

Почему prompt подходит: он проверяет prompt to task, task to orchestration plan, task/plan to skills, и требует универсальное поведение без привязки к стеку.

## Preconditions перед будущим запуском

1. Claude app visible/unlocked.
2. No password/PIN captured.
3. Correct Claude thread/project/extension route identified.
4. Если появляется login/session prompt, агент останавливается до credentials, а пользователь завершает login локально.
5. Если виден model/reasoning tier и он не соответствует ожидаемому, агент записывает warning и не имитирует правильный tier.
6. Если Claude target window не найден, сценарий получает `TARGET_WINDOW_NOT_FOUND`.
7. Если правильный thread/project/extension route не найден, сценарий получает `WRONG_THREAD_OR_SURFACE`.

## Exact future UI steps

1. Найти окно Claude через window inventory.
2. Сделать screenshot окна Claude до действий.
3. Проверить, что окно не закрыто login/password/PIN prompt.
4. Проверить, что выбран правильный thread/project/extension route для XIIGen.
5. Если route не выбран, открыть нужный thread/project/extension route штатными UI-действиями.
6. Ввести exact prompt из этого файла.
7. Сделать screenshot перед submit, где видно введенный prompt и выбранную поверхность.
8. Нажать submit.
9. Дождаться ответа или явной ошибки.
10. Сделать screenshot после ответа.
11. Собрать core request/response/trace или correlation id, если доступно без секретов.
12. Сохранить redaction log.
13. Записать human summary: что было видно в Claude UI, совпадает ли это с Path A, и какой verdict.

## Anti-substitution rules

JSON/log/core trace alone is not Claude UI proof.

Marker-only screenshot is not full UI proof.

Claude adapter proof is not Claude chat UI readiness.

API/boundary evidence не заменяет локальный UI path. UI path должен показывать работу в реальной Claude app/window.

## Acceptance criteria

`PASS`: Path A и Path B выполнены для одного и того же prompt; есть UI screenshots before/after; есть core/adapter correlation или честно записано, почему trace unavailable; нет секретов; ответ виден в правильной Claude surface.

`PARTIAL`: выполнен только один путь или correlation частичный, но evidence честно сужено.

`BLOCKED_LOGIN_REQUIRED`: Claude требует login/session/password/PIN; агент остановился до credentials.

`TARGET_WINDOW_NOT_FOUND`: локальное окно Claude не найдено.

`RESPONSE_NOT_VISIBLE`: prompt отправлен, но ответ не виден или невозможно доказать, что он относится к сценарию.

`CORE_TRACE_MISSING`: UI path выполнен, но core/adapter trace/correlation не найден.

`WRONG_THREAD_OR_SURFACE`: prompt оказался не в том thread/project/extension route.

`MODEL_TIER_WARNING`: visible model/reasoning tier не соответствует ожидаемому; это warning, не подмена evidence.

## Evidence required after execution

1. Window inventory.
2. Screenshot before UI actions.
3. Screenshot before submit.
4. Screenshot after response or error.
5. Core request/response/trace for same prompt.
6. Adapter/plugin trace or correlation note.
7. Redaction log.
8. Human-readable summary.

## What this will not prove

Этот scenario prep не доказывает token savings.

Этот scenario prep не доказывает semantic quality.

Этот scenario prep не доказывает learnability/training.

Этот scenario prep не доказывает DPO correctness.

Этот scenario prep не доказывает checkpoint promotion или locator update.

Этот scenario prep не доказывает Cursor/Codex scenario acceptance.

Этот scenario prep не запускает Claude UI и не отправляет prompt.
