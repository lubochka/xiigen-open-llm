# P18 human docs refresh

Вердикт slice: `READY_FOR_PARENT_REVIEW`.

## Что сделано

P18 обновляет человекочитаемую документацию для `43.SmartSkillsDomain` после P17D/P17E/P17F.

Главная поправка: документация теперь говорит не "готово", а честно разделяет три уровня:

- P17D: `SkillSelection` имеет component runtime improvement evidence.
- P17E: `SkillSelection` candidate проходит lifecycle import/export/fresh-load как component evidence.
- P17F: API firewall через `XiigenExtensionApiService.ShouldClaimSemanticQuality` удерживает full semantic-quality claim в `false`.

## Что теперь понятно человеку

Документ `docs/projects/43-smart-skills-domain.md` теперь начинается с русского summary, содержит draw.io-importable Mermaid classDiagram для ролей, flowchart, таблицу симуляций по соединениям, no-code алгоритм evidence flow, короткие code refs и status table.

## Parent review fixes

- Добавлен небольшой Mermaid `classDiagram` с ролями `XiigenExtensionApiService`, `PromptToTaskMachine`, `OrchestrationReasoningMachine`, `SkillSelectionMachine`, P17D/P17E/P17F evidence и blocked claims.
- Добавлены русские notes/legend для ролей и статусов.
- Simulation coverage расширен для связей `PromptToTask -> P17F`, `OrchestrationReasoning -> P17F` и `P17F -> BlockedClaims`.
- Status table переведен на project documentation markers: `⚠️ Частичная` и `Не заявлено / не покрыто P18`.
- Добавлена фраза, что P18 documentation-only и не меняет runtime behavior/model state.

## Что не заявлено

P18 не заявляет:

- full semantic quality;
- `SkillSelection` readiness;
- full API readiness;
- trained/final model readiness;
- dashboard readiness;
- Claude/Cursor/Codex host runtime readiness.

## P17F commit

P17F commit verified as pushed:

`b815b38876ffe7d0ea51346f41f22773c5872322`

## Проверки

Выполненные P18 validation checks:

- JSON parse: passed.
- JSONL parse: passed, 6 rows read.
- Mojibake scan: passed.
- Escaped Cyrillic scan: passed.
- P18 CRLF scan: passed.
- Overclaim true-flag scan: passed.
- P17F commit recorded in state: true.
- P18 state block ready: true.
- `git diff --check` on allowed scope: passed.
- Trailing whitespace scan: passed.
