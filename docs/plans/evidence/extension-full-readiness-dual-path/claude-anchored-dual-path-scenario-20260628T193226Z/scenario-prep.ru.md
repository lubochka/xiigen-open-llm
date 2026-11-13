# Подготовка сценария: Claude anchored dual-path proof

## Цель

Проверить один и тот же безопасный redacted сценарий через два пути:

- Path A: локальный автоматизированный/API/core/adapter маршрут Claude extension/XIIGen, если он существует в `C:\Projects\claude-xiigen-smart-skills`.
- Path B: реальная локальная Claude UI/app, если она уже доступна в текущей Windows-сессии без ввода секретов.

Это не проверка product readiness, token savings, semantic quality, training, checkpoint promotion или locator update.

## Якоря сценария

- marker: `FRDP-P01-ROW12-CLAUDE-ANCHORED-DUAL-PATH-V1`
- scenario_id: `frdp-p01-claude-anchored-dual-path`
- pair_id: `frdp-p01-row12-claude-anchored-v1`

## Единый prompt для Path A и Path B

```text
FRDP-P01-ROW12-CLAUDE-ANCHORED-DUAL-PATH-V1
scenario_id=frdp-p01-claude-anchored-dual-path
pair_id=frdp-p01-row12-claude-anchored-v1

Safe redacted XIIGen smart-skills orchestration probe:
For a generic local extension orchestration task, select relevant XIIGen smart skills and return a concise JSON-like plan with:
- selected_skills
- rationale
- next_safe_action

Do not use secrets.
Do not claim tests, builds, token savings, training, checkpoint promotion, or product readiness.
Echo the marker, scenario_id, and pair_id in the response.
```

## Path A: точная команда/adapter

Команда запускается из evidence-каталога:

```powershell
node .\path-a-claude-adapter-probe.mjs
```

Ожидаемое поведение:

1. Helper script читает prompt из этого файла.
2. Helper script ищет локальный доступный Claude extension adapter/API/core route внутри `C:\Projects\claude-xiigen-smart-skills`.
3. Если поддерживаемый route найден, helper запускает его с тем же prompt и сохраняет request/core trace/response.
4. Если поддерживаемый route не найден, helper сохраняет blocker с точной причиной и не делает product readiness claim.

Ожидаемые Path A файлы:

- `path-a-request.json`
- `path-a-output.log`
- `path-a-response.json` или `path-a-blocker.json`

## Path B: target app/window и действия

Target: уже открытая или уже установленная локальная Claude UI/app в текущей Windows-сессии. Секреты не вводятся и не запрашиваются.

Preflight:

1. Найти локальный процесс/окно Claude без запуска внешнего web/AI.
2. Если Claude UI/app не найдена или открыта на login/auth screen, остановить Path B как blocker.
3. Если окно доступно и не требует логина, сделать screenshot до ввода.

Submit action:

1. Вставить ровно тот же prompt из раздела выше в поле ввода Claude UI/app.
2. Нажать видимую кнопку submit/send или клавиатурный equivalent, только если UI уже готова и не требует секретов.
3. Дождаться видимого финального ответа.
4. Сделать screenshot после отправки и screenshot финального ответа.

Ожидаемый видимый результат:

- В prompt или ответе видны `FRDP-P01-ROW12-CLAUDE-ANCHORED-DUAL-PATH-V1`, `frdp-p01-claude-anchored-dual-path`, `frdp-p01-row12-claude-anchored-v1`, либо Path B явно downgraded because structured anchors are not extractable from UI.

Ожидаемые Path B файлы:

- `path-b-preflight.json`
- `path-b-before.png`
- `path-b-after-submit.png`
- `path-b-final.png`
- `path-b-visible-text.txt` или `path-b-blocker.json`

## Join review

Если оба пути выполнены, проверить:

- одинаковый marker;
- одинаковый scenario_id;
- одинаковый pair_id;
- одинаковый prompt hash или точное совпадение prompt text.

Если Path B не позволяет получить structured anchors, но screenshots показывают prompt/result, зафиксировать downgrade.

Ожидаемые join файлы:

- `join-review.ru.md`
- `join-review.json`

## Безопасность

- no secrets captured;
- no secrets typed;
- no product code edits;
- no governance/AGENTS/CLAUDE/skills/plan edits;
- no product readiness/token savings/semantic quality/training/checkpoint promotion/locator update claims.
