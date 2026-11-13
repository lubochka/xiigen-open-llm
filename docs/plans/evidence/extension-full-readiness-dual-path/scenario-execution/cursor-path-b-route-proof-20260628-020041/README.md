# Cursor Path B route-proof run

## Что решает этот шаг

Этот шаг проверял только видимый маршрут XIIGen Smart Skills в Cursor Extension Development Host. Цель была доказать или заблокировать вход в маршрут без ввода сценарного prompt.

Итог: `BLOCKED_UI_CONTROL`. Видимый маршрут XIIGen Smart Skills не доказан.

## Найденные команды и источники

- `xiigenSmartSkills.panel` / `XIIGen: Open Smart Skills Panel`
  - Источник: `C:\Projects\cursor-xiigen-smart-skills\package.json`
  - Источник реализации: `C:\Projects\cursor-xiigen-smart-skills\src\extension.ts:136`
  - Это был основной безопасный кандидат, потому что команда должна открыть панель `XIIGen Smart Skills`.
- `xiigen.smartSkills.select` / `XIIGen: Select Smart Skill`
  - Источник: `C:\Projects\cursor-xiigen-smart-skills\package.json`
  - Источник реализации: `C:\Projects\cursor-xiigen-smart-skills\src\extension.ts:67`
  - Эта команда открывает input `Task for XIIGen smart-skill selection`, поэтому она не доказывает panel route.
- `xiigen-smart-skills` / `XIIGen Smart Skills`
  - Источник: `C:\Projects\cursor-xiigen-smart-skills\package.json`
  - Источник реализации: `C:\Projects\cursor-xiigen-smart-skills\src\extension.ts:103`
  - Это chat participant candidate, но этот run не вводил prompt в chat.

## Что было сделано

1. Прочитан authorized route script: `docs/plans/evidence/extension-full-readiness-dual-path/scenario-plans/cursor-path-b-route-script-20260628-015445/README.md`.
2. Прочитаны manifest/source files только для поиска route candidates:
   - `C:\Projects\cursor-xiigen-smart-skills\package.json`
   - `C:\Projects\cursor-xiigen-smart-skills\src\extension.ts`
3. Найдено окно Cursor Extension Development Host.
4. Начальный title был `[Extension Development Host] Browser Tab - cursor-xiigen-smart-skills - Cursor`.
5. Browser Tab был видим. Повторный runner закрыл его: после close shortcut title стал `[Extension Development Host] cursor-xiigen-smart-skills - Cursor`.
6. Через command palette с clipboard paste был сделан route attempt. Screenshot `03-command-palette-candidate-pasted.png` показывает видимый candidate `XIIGen: Open Smart Skills Panel`, но активным остался `XIIGen: Select Smart Skill`.
7. Enter открыл input `Task for XIIGen smart-skill selection`, а не панель `XIIGen Smart Skills`.
8. Дополнительные попытки через exact panel search и F1 не смогли стабильно вставить exact panel command text. Финальные screenshots показывают command palette/input state, но не панель XIIGen Smart Skills.

## Что было видно

- Screenshot `03-command-palette-candidate-pasted.png`: видимы два candidate command: `XIIGen: Select Smart Skill` и `XIIGen: Open Smart Skills Panel`. Активным выбран `Select Smart Skill`.
- Screenshot `04-route-result.png`: виден input `Task for XIIGen smart-skill selection`; панель `XIIGen Smart Skills` не видна.
- Screenshot `07-panel-retry-route-result.png`: после retry виден Cursor host без панели XIIGen Smart Skills.
- Screenshot `10-f1-attempt-route-result.png`: снова виден input `Task for XIIGen smart-skill selection`; panel route не доказан.

## Route verdict

- Route proven: `false`.
- Status: `BLOCKED_UI_CONTROL`.
- Причина: command palette UI не позволил надежно вставить/выбрать exact panel route. Видимый panel candidate был показан, но выполненным оказался select-command input route.
- Prompt typed: `false`.
- Prompt submitted: `false`.
- Secrets captured: `false`.

## Следующее действие

Если нужен следующий bounded run, сначала нужен repair UI control strategy: например, отдельный сценарий выбора второго command-palette result, route invocation через проверяемый Cursor command UI adapter, либо другой host-control метод, который доказывает exact command title before Enter. Только после принятого route proof можно запускать отдельный prompt-response run.
