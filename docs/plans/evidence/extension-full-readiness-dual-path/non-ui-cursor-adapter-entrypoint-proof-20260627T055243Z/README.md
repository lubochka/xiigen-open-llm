# Cursor non-UI adapter boundary proof

Фаза 3 из 10: доказать, что host adapters тонкие и ходят в `llm_mvp_core`.

Текущий шаг: Cursor adapter/source boundary proof, не UI.

## Статус этого slice

Всего проверок: 8.
Сделано: 8.
В процессе: 0.
Осталось: 1 следующий bounded action.

Verdict: `PASS_CURSOR_ADAPTER_TO_CORE_BOUNDARY_ONLY`.

Product readiness count: 0.

## Что было исправлено в этом fix-loop

Предыдущий README не прошёл human-readable evidence gate:

1. В нём остался шаблон verdict вместо настоящего verdict.
2. В строке про product readiness был null/garbled символ.
3. В списке evidence-файлов были PowerShell object interpolation blocks вместо настоящих имён файлов.

Этот README переписан как человекочитаемый evidence-файл. Product code не менялся.

## Что доказано

1. Cursor extension repo найден: `C:\Projects\cursor-xiigen-smart-skills`.
2. `package.json` указывает `main: ./dist/extension.js` и entrypoints: `xiigen.smartSkills.select`, `xiigenSmartSkills.panel`, `xiigen-smart-skills`.
3. `dist/extension.js` можно активировать без правки product code через fake-vscode harness, созданный только в этой evidence-папке.
4. Команда `xiigen.smartSkills.select` зарегистрировалась и была вызвана программно с redacted prompt.
5. Cursor adapter построил `XiigenExtensionRequest` и вызвал `C:\Projects\llm_mvp_core\tools\xiigen-extension-core-api.ps1`.
6. `RequestedSkills` прошли в core: `xiigen-smart-skills`.
7. `AvailableSkills` прошли в core: `xiigen-smart-skills`, `qa-automation`, `planning`.
8. Host trace показывает `hostSelectorOrScorerAdded=false`: host не делал свой selector/scorer и передал выбор в core.

## Что не доказано

1. Не доказан реальный Cursor UI: пользователь не вводил prompt в живую программу Cursor.
2. Не доказано, что ответ отрисован в Cursor chat/panel UI.
3. Не доказаны token savings, semantic quality, learnability/training, checkpoint mutation/promotion, locator update.
4. Не доказана product readiness.

Product readiness count: 0.

## Evidence files

- Runner: `cursor-adapter-proof-runner.js`
- Proof output: `proof-output.json`
- Result summary: `result.json`
- Core request: `xiigen-cursor-xiigen.smartSkills.select-request-2026-06-27T05-52-44-509Z.json`
- Core response: `xiigen-cursor-xiigen.smartSkills.select-core-response-2026-06-27T05-52-44-509Z.json`
- Core trace: `xiigen-cursor-xiigen.smartSkills.select-core-trace-2026-06-27T05-52-44-509Z.json`
- Host trace: `xiigen-cursor-xiigen.smartSkills.select-host-trace-2026-06-27T05-52-44-509Z.json`
- Entrypoint response: `xiigen-cursor-xiigen.smartSkills.select-entrypoint-response-2026-06-27T05-52-44-509Z.json`
- Latest host-runtime copy: `xiigen-cursor-host-runtime-latest.json`

## Следующее одно bounded action

После локального unlock/login нужно отдельно запускать real Cursor UI scenario с тем же смысловым запросом. Этот slice готовит только non-UI adapter proof и не заменяет живой UI path.
