# Preflight: Claude blank chat safe probe

subagent_id: CLAUDE-BLANK-CHAT-PREFLIGHT-16
timestamp_utc: 2026-06-28T19:44:00Z

## Цель

Проверить, можно ли безопасно открыть или подтвердить пустую авторизованную поверхность Claude chat без ввода секретов и без захвата приватного содержимого существующих разговоров. Если пустой чат подтвержден, разрешена одна попытка отправить тот же anchored prompt из prior blocker.

## Якоря

- marker: `FRDP-P01-ROW12-CLAUDE-ANCHORED-DUAL-PATH-V1`
- scenario_id: `frdp-p01-claude-anchored-dual-path`
- pair_id: `frdp-p01-row12-claude-anchored-v1`
- prompt_sha256: `f1c7299220fbf3c8027dc700a0980e0732761d9af028e9e3e6c8c28dc34feafa`
- prior_blocker: `claude-anchored-dual-path-scenario-20260628T193226Z`

## Prompt, если отправка станет безопасной

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

## Безопасные действия

1. Проверить наличие локального окна Claude без сохранения заголовков разговоров, текста чатов или приватного содержимого.
2. Использовать только безопасные операции, которые могут открыть пустую поверхность: видимый New Chat, безопасный локальный keyboard shortcut, либо безопасный app/menu control, если он доступен без чтения приватного контента.
3. Не вводить учетные данные, токены, пароли, MFA-коды или любые секреты.
4. Не делать скриншоты существующих разговоров, истории, боковой панели с приватными названиями, сообщений пользователя или ответа Claude.
5. Скриншот разрешен только если видна пустая область ввода без приватного контента; кадр должен быть по возможности ограничен blank input/composer area.
6. Если пустой чат не подтвержден, не вставлять prompt и не нажимать submit.
7. Если prompt безопасно отправлен, фиксировать только безопасное доказательство якорей или blocker; не заявлять product readiness, token savings, semantic quality, training или checkpoint promotion.

## Критерий остановки

Одна безопасная попытка или около 10 минут. Если пустой авторизованный Claude chat не подтверждается без риска захвата приватного контента, остановить UI-часть и записать точный blocker.
