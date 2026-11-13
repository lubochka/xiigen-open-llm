# API/Dashboard QA HOWTO

Этот документ объясняет, как человек пользуется dashboard/API explorer для XIIGen methods. Он описывает dashboard-to-core documentation and QA coverage only. Dashboard/API pass cannot prove live Claude/Cursor/Codex runtime, token savings, quality improvement, learned model readiness, target promotion, locator update or final readiness.

## Всего / Сделано / Сейчас / Осталось

| Поле | Статус |
|---|---|
| Всего | Нужны понятные API examples, объяснение input/output/debug/raw JSON и границы scope. |
| Сделано | Этот how-to описывает рабочий путь и три primary examples; docs QA review проверяет examples только как documentation coverage. |
| Сейчас | Dashboard/API является только supporting evidence; token pairs 0/9, quality rows 0/9, DPO rows 0/9, lifecycle rows 0/9, final reviews 0/10. |
| Осталось | После docs QA остаются live host runtime/token/quality/learnability evidence packets; dashboard examples должны остаться узким support evidence. |

## Как пользоваться dashboard

1. Открыть dashboard в локальной среде, где WPF UI разрешен.
2. Выбрать API row по человеческому названию.
3. Прочитать описание method, example input и safety note.
4. Вставить или набрать текст в input parameter box.
5. Включить debug/raw JSON только когда это нужно для evidence.
6. Нажать Run.
7. Прочитать result, warnings, errors, debug trace, raw JSON и safe history.
8. Записать evidence scope: только dashboard/API, не live host runtime.

## Примеры API

| Человеческое имя API | Method id | Поле ввода | Пример input | Возвращаемый result | Warning/error/debug/raw JSON | Ограничение |
|---|---|---|---|---|---|---|
| Понять user prompt как structured task | `prompt-to-task` | User prompt | "Составь план улучшения skill selection; не реализуй." | Status, chat display text, structured task refs, check-set results. | Warnings/errors, optional debug trace, raw JSON, correlation id и redaction status. | Доказывает только dashboard/API path для этого method. Не доказывает Claude/Cursor/Codex runtime или semantic quality. |
| Найти orchestration plan для structured task | `orchestration-plan` | Structured task text | "Task: docs QA review; gates: no-overclaim, JSON parse, diff check." | Scenario refs, orchestration family, warnings и check-set results. | Debug может показать redacted task ref, selected scenario fields и reason codes. | Не доказывает token savings, quality improvement или learned model readiness. |
| Выбрать relevant skills для task | `skill-selection` | Task, plan, available/requested skills | "Task: docs QA; available skills: documentation-sync, test-integrity, output-readability-gate." | Selected skills, rejected skills, evidence features, warnings. | Raw/debug panels могут показать selected/rejected ids и scaffold evidence. | Не доказывает DPO training, checkpoint promotion, learnability или final readiness. |

## Возвращаемый result и debug panels

Result panel должен показывать понятный answer/status. Warnings объясняют degraded или missing proof conditions. Errors объясняют validation или controlled failure. Debug trace должен показывать redacted method/input/output/correlation details. Raw JSON должен быть виден только когда он явно запрошен и безопасен. Safe history должен использовать prompt hash или redacted refs, а не raw private prompt text.

## Поля screenshot/transcript/evidence для последующей QA

Будущий dashboard QA packet должен записывать:

- method id and human API name;
- example input or redacted input ref;
- result text and status;
- warnings/errors/debug/raw JSON state;
- correlation id;
- redaction status;
- evidence scope label;
- limitation statement;
- timestamp/freshness.

## Short code anchors

`43.SmartSkillsDomain.Dashboard/ApiDashboardRunner.cs`:

```csharp
public const string DashboardEvidenceScope =
    "dashboard-to-core invocation only; not extension host runtime proof";
```

`43.SmartSkillsDomain.Dashboard/ApiDashboardResultPresenter.cs`:

```csharp
RawJsonText,
DebugTraceText,
DebugTraceSuppressionReason,
```

These anchors explain why dashboard QA can inspect result/debug/raw panels while keeping scope narrow.

## Evidence scope and limitations

Этот пример доказывает только dashboard/API path for the named method. Он не доказывает live Claude/Cursor/Codex runtime, token savings, quality improvement, DPO training, checkpoint promotion, target promotion, locator update or final readiness.

## Команда Phase 13

Фаза 13/14: QA automation, documentation, and no-overclaim evidence.

Что решает этот шаг: дает Лубе одну проверяемую команду для dashboard/API QA, host packet scope checks, docs coverage и no-overclaim scan.

Запуск:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tools\phase13-qa-docs-no-overclaim.ps1 -Json
```

Что это доказывает:

- Existing dashboard/API QA script проходит.
- Existing Claude/Cursor/Codex runtime packets сохраняют точную host/scenario границу.
- Dashboard/API docs объясняют, как запускать API explorer и читать result/debug/raw JSON fields.
- Измененный Phase 13 wording не содержит positive readiness overclaims.

Что это не доказывает:

- Dashboard/API pass не доказывает live Claude/Cursor/Codex runtime.
- Это не доказывает final product readiness, full host UI readiness, full semantic quality, learnability/training success, checkpoint promotion, locator update или Phase 14 final reviews.
