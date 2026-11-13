# Codex Path B local-agent UI scenario coverage

claim_scope: один Codex Path B local-agent UI preflight/execution attempt for scenario `codex-full-extension-runtime-20260627-090817`; prompt was not submitted through the UI.
evidence_scope: committed scenario prep and accepted Path A packet were read; Windows window inventory, screen inventory, UI automation summary, and screenshots were collected in this folder.
count_claimed: 1 scenario coverage packet.
independent_evidence_count: 5 evidence groups: scenario prep read, Path A read, window inventory, UI automation summary, screenshots.
subagent_id_or_NONE: NONE visible; local nickname `codex-path-b-local-ui-subagent-20260627-225806`.
downgrade_needed: yes. The evidence supports `Path B blocked`, not `Path B accepted`.

## Человеческий статус

Всего сценариев в этом пакете: 1.

Сценарий: Codex UI должен выполнить тот же запрос, который уже принят в Path A через adapter/API/core boundary.

Path A: уже принят по ранее сохраненному evidence-пакету `docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/codex-path-a-api-boundary-20260627-223748/`.

Path B: заблокирован и не принят. Реальное окно Codex найдено, но текущий Codex thread находится в активном состоянии выполнения: в UI виден статус Thinking и кнопка Stop, а Windows UI Automation не нашел доступного `Edit`-контрола для чистого ввода нового prompt. Отправлять сценарный prompt в это состояние было бы небезопасно: это могло бы вмешаться в уже выполняющийся агентный запуск и не доказало бы правильный XIIGen route.

Это evidence покрытия сценария; count готовности продукта не использовать.

## Точный prompt сценария

```text
Через XIIGen Smart Skills разберись с задачей: пользователь хочет улучшить документацию продукта так, чтобы она была понятна человеку. Переведи запрос в универсальную задачу, найди универсальный план оркестрации и выбери подходящие скилы. Используй только универсальную часть без привязки к конкретному языку программирования, фреймворку или стеку.
```

## Что было сделано

1. Прочитаны подготовленный Codex scenario prep и Path A README/result/request.
2. Из scenario JSON восстановлен intended UTF-8 Russian prompt.
3. Создана одна новая папка evidence для Path B.
4. Собран список экранов.
5. Собран список видимых окон Windows.
6. Найдено окно Codex: title `Codex`, process `Codex.exe`, monitor `\\.\DISPLAY1`.
7. Сделаны screenshots:
   - `screenshots/desktop-before.png`
   - `screenshots/codex-window-before.png`
   - `screenshots/codex-composer-blocked.png`
8. Собран UI Automation summary по окну Codex.
9. Проверено, что UI Automation summary содержит видимую кнопку `Stop` и не содержит доступного `Edit`-контрола.
10. Prompt не вводился и не отправлялся.

## Почему Path B не принят

Path B требует не marker-only evidence, а реальный локальный UI путь: тот же prompt должен быть введен в Codex UI, отправлен, после чего должен появиться видимый ответ или видимая ошибка. В этом запуске удалось подтвердить только локальное наличие Codex UI и состояние блокировки composer во время активного agent run.

Принять Path B нельзя, потому что:

- prompt не был введен в UI;
- prompt не был отправлен;
- ответ в Codex UI по этому prompt не появился;
- correlation/core trace для UI-run не собирался;
- активный thread показывал `Thinking` и `Stop`, поэтому чистый submit-ready state отсутствовал.

## Оставшееся действие

Когда текущий Codex agent run завершится и composer станет доступным, нужен отдельный bounded local UI run:

1. открыть тот же Codex thread/project/plugin route для XIIGen Smart Skills;
2. сделать screenshot до ввода prompt;
3. ввести exact prompt из этого README;
4. сделать screenshot перед отправкой;
5. отправить prompt;
6. дождаться видимого ответа или ошибки;
7. сделать screenshot после ответа или ошибки;
8. собрать correlation/core/plugin trace, если он доступен;
9. записать новый Path B result без claims шире фактического UI evidence.

## Граница claims

Этот пакет доказывает только local Codex UI preflight and blocker evidence. Он не доказывает accepted Path B, token savings, semantic quality, learnability, training, DPO, checkpoint promotion, locator update, Claude coverage, Cursor coverage, or any project-wide readiness.
