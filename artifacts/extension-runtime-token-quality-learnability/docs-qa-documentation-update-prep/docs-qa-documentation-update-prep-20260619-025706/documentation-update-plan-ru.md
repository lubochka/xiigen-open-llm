# DOCS-QA-DOCUMENTATION-UPDATE-PLAN-OR-PREP-01

Дата: 2026-06-19T02:57:06+03:00
Ветка: codex/chat-gpt-5.5
Режим: documentation update prep only. Этот пакет не редактирует product docs и не закрывает readiness.

## Человеческий статус

| Поле | Статус |
|---|---|
| Всего | Нужно довести 6 треков до честного packet-backed PASS: runtime Claude/Cursor/Codex, token before/after для 9 пар, quality scoring для 9 строк, learnability/DPO/lifecycle для 9 строк, docs/QA и 10 fresh final readiness reviews. |
| Сделано | Создан предыдущий DOCS-QA status/evidence-scope prep packet. Этот packet подготовил точный план будущих docs edits, ledger целей, следующий bounded work order и human status board. Product docs в этой фазе не редактировались. |
| Сейчас | Full UI/chat readiness не закрыта. Token complete baseline/adaptive pairs: 0/9. Quality rows ready for actual scoring: 0/9. Trainable DPO rows: 0/9. Actual lifecycle rows: 0/9. Final readiness reviews counted: 0/10. Claim locks остаются false. |
| Осталось | Следующий безопасный work order: DOCS-QA-DOCUMENTATION-UPDATE-APPLY-01. Он может редактировать только перечисленные docs, без runtime/eval/training/DPO/checkpoint/model calls и без readiness claim. |

## Главный вывод

Следующий docs-edit sub-agent должен обновить человеческую документацию так, чтобы Luba и будущие исполнители видели полный контур: как Claude/Cursor/Codex thin adapters должны вызывать `llm_mvp_core`, как dashboard/API используется как лабораторная поверхность, как вводятся параметры и читаются результаты, какие evidence scopes есть, какие ограничения остаются, и почему документация сама по себе не доказывает runtime/token/quality/learnability readiness.

Каждый doc должен писать обычным русским языком: что это за путь, как человек им пользуется, что считается доказательством, что не считается доказательством, что сейчас уже есть, что отсутствует, какой следующий bounded work order закрывает gap.

## Точные docs targets для следующей фазы

### 1. `docs/projects/43-smart-skills-domain.md`

Изменить существующий doc. Добавить раздел `XIIGen extension runtime/API boundary`.

Что добавить:
- простую UML/Mermaid или PlantUML диаграмму: `Claude/Cursor/Codex host -> thin adapter -> XiigenExtensionRequest -> XiigenExtensionApiService -> PromptToTaskMachine -> OrchestrationReasoningMachine -> SkillSelectionMachine -> XiigenExtensionResponse -> host visible answer/debug/error`;
- no-code algorithm: как host получает prompt, строит request, вызывает core, показывает response, пишет debug/error/correlation/redaction evidence;
- короткие code quotes из `43.SmartSkillsDomain/XiigenExtensionApiContracts.cs` и `43.SmartSkillsDomain/XiigenExtensionApiService.cs`, только как anchors, не как длинные вставки;
- статус честности: dashboard/API/core call может быть supporting evidence, но не live Claude/Cursor/Codex runtime readiness.

Acceptance:
- doc ясно говорит, что логика остается в `llm_mvp_core`, host adapters thin;
- doc не заявляет token savings, semantic quality, learnability, common readiness, target promotion или locator update.

### 2. `docs/projects/43-smart-skills-domain-dashboard.md`

Изменить существующий dashboard doc. Он уже содержит UML, no-code algorithms, usage, code refs и QA mapping; следующий sub-agent должен добавить extension-line appendix.

Что добавить/обновить:
- раздел `How this dashboard supports extension runtime/token/quality/learnability docs`;
- API/dashboard usage flow: выбрать API row, прочитать example, ввести параметр/prompt, нажать Run, прочитать result/warnings/errors/debug/raw JSON/history;
- примеры для `prompt-to-task`, `orchestration-plan`, `skill-selection` с input, expected returned result fields, warning/error/debug evidence;
- отдельный блок limitations: dashboard/API evidence proves dashboard-to-core only, not Claude/Cursor/Codex live UI/chat, not token savings, not trained model readiness;
- ссылка на future docs: `docs/API-DASHBOARD-QA-HOWTO-ru.md` и `docs/EVIDENCE-SCOPES-HOWTO-ru.md`.

Acceptance:
- dashboard examples имеют method, input, output/result, error/warning, correlation/debug/redaction and evidence-scope label;
- в doc нет wording, которое делает dashboard pass заменой live host runtime.

### 3. `docs/projects/43-smart-skills-semantic-quality-authorization.md`

Изменить существующий doc. Добавить раздел `Quality and DPO admission for extension line`.

Что добавить:
- no-code algorithm for quality scoring admission: complete pair integrity -> reviewer/adjudication refs -> source-disjoint held-out refs -> ablation/no-leakage/no-memorization controls -> scoring work order;
- DPO candidate boundary: chosen/rejected/provenance refs are candidates only; trainable rows are 0/9 now; raw external AI/vendor outputs stay evidence-only/private/adaptive/quarantine unless approved;
- объяснить, почему `0/9 quality rows ready` и `0/9 trainable DPO rows` держат quality/learnability claims closed;
- acceptance wording for future docs QA.

Acceptance:
- doc separates quality prep, actual scoring, DPO candidate, actual training and learnability;
- doc keeps quality improvement, full semantic quality, full learnability, common model readiness and target promotion false.

### 4. `docs/projects/PROJECT-DOCUMENTATION-INDEX.md`

Изменить существующий index. Добавить ссылки на новые how-to docs из этой линии.

Acceptance:
- new docs discoverable from project docs index;
- index labels them as extension runtime/token/quality/learnability documentation, not readiness evidence.

### 5. `docs/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-HOWTO-ru.md`

Создать новый top-level Russian how-to.

Содержимое:
- one-page human map of the whole line;
- total/done/current/remaining board with the exact current counts;
- Target Outcome Matrix summary: runtime, token, quality, learnability, docs/QA, final reviews;
- UML overview and no-code algorithm for end-to-end extension path;
- exact claim locks and no-overclaim statement;
- links to host-specific, dashboard/API, debug, lifecycle, evidence-scope and status-template docs.

Acceptance:
- читатель понимает, что план движется, но full readiness не закрыта;
- no status label appears without plain human explanation.

### 6. `docs/CLAUDE-XIIGEN-HOWTO-ru.md`

Создать host-specific doc for Claude.

Содержимое:
- intended Claude hook/plugin/command path;
- how Claude should build `XiigenExtensionRequest`, call core, display visible answer/error/debug;
- what evidence must exist for Claude live host runtime proof: visible answer, debug/error path, input/output, correlation id, freshness, same-run artifacts;
- current boundary: current Claude evidence is narrow/no-eval, not full Claude Desktop/chat UI readiness;
- short code quote anchors where real Claude adapter/core refs exist; if exact adapter path is missing, say so and require runtime-map follow-up instead of inventing it.

Acceptance:
- doc helps a human use/verify Claude path, but says missing live proof remains open.

### 7. `docs/CURSOR-XIIGEN-HOWTO-ru.md`

Создать host-specific doc for Cursor.

Содержимое:
- intended Cursor/VS Code Extension Host/UI/CLI path;
- safe launch/no-external-AI/no-backend boundary;
- Extension Host logs, visible command/chat/panel answer/error, core request/response and correlation id;
- current boundary: Cursor full UI proof is blocked/deferred until safe launch contract and live evidence exist;
- no-code algorithm and evidence checklist.

Acceptance:
- no claim that helper/direct adapter/dashboard evidence proves Cursor readiness.

### 8. `docs/CODEX-XIIGEN-HOWTO-ru.md`

Создать host-specific doc for Codex.

Содержимое:
- intended Codex plugin/app/chat/app-server path;
- installed plugin/cache adapter smoke vs real Codex UI/app runtime;
- visible answer/error/debug/correlation evidence required;
- current boundary: plugin-cache adapter smoke only; not full Codex desktop chat UI/app readiness.

Acceptance:
- package smoke/direct adapter proof remains labelled narrow evidence only.

### 9. `docs/API-DASHBOARD-QA-HOWTO-ru.md`

Создать API/dashboard usage and QA doc.

Содержимое:
- dashboard/API explorer workflow step by step;
- API examples for `prompt-to-task`, `orchestration-plan`, `skill-selection`;
- parameter input examples, returned result fields, warning/error/debug/raw JSON panels and safe history;
- screenshot/transcript/evidence fields expected in later dashboard QA report;
- limitations: dashboard/API pass cannot prove live Claude/Cursor/Codex runtime, token savings, quality improvement or learned model readiness.

Acceptance:
- every example includes method, input, returned result, evidence scope and limitation.

### 10. `docs/XIIGEN-DEBUG-TRACE-HOWTO-ru.md`

Создать debug trace guide.

Содержимое:
- what debug trace must show: xiigen method, redacted input/output, error, correlation id, run timestamp/freshness, redaction status, evidence-scope label;
- positive and controlled error examples;
- how debug appears in dashboard, Claude, Cursor and Codex docs;
- raw/private vs redacted refs.

Acceptance:
- debug trace is a verification aid only; it does not claim readiness by itself.

### 11. `docs/LOCAL-MODEL-LIFECYCLE-HOWTO-ru.md`

Создать local adaptive lifecycle doc.

Содержимое:
- common/repo leg vs user-local adaptive leg;
- DPO chosen/rejected/provenance/source/license/privacy/redaction approval chain;
- backup/export/import/restore/load-without-retrain/continue-training/fresh-load/held-out/ablation/rollback checklist;
- current status: trainable DPO rows 0/9 and actual lifecycle rows 0/9.

Acceptance:
- saved checkpoint only is explicitly insufficient;
- no training/continue-training/checkpoint/locator/target mutation is claimed.

### 12. `docs/EVIDENCE-SCOPES-HOWTO-ru.md`

Создать evidence-scope glossary.

Содержимое:
- definitions and examples for dashboard/API, handler-level, CLI helper, package smoke, test-only adapter, live host runtime, token delta report, quality eval report, local DPO candidate, continue-training report, fresh-load report, held-out/ablation report, rollback report, documentation QA report and final full-extension report;
- for each scope: what it proves, what it cannot prove, exact safe wording.

Acceptance:
- safe wording mirrors plan Section 11 and previous evidence-scope prep.

### 13. `docs/RUSSIAN-STATUS-AND-EXECUTION-LOG-TEMPLATE.md`

Создать status/log template.

Содержимое:
- mandatory `Всего / Сделано / Сейчас / Осталось`;
- claim lock table;
- next bounded work order field;
- how to write plain Russian status without opaque labels only;
- examples using current counts: token 0/9, quality 0/9, DPO 0/9, lifecycle 0/9, final reviews 0/10.

Acceptance:
- future reports can copy the template without overclaiming readiness.

## Как писать UML, no-code algorithms и code quotes

UML:
- use Mermaid or PlantUML blocks that can be copied into draw.io;
- every diagram must have a short human explanation below it;
- diagrams must separate host surface, thin adapter, core API, machines, model/lifecycle artifacts and evidence reports.

No-code algorithms:
- write steps as human actions and system actions;
- include success branch, controlled error branch and evidence-recording branch;
- name what the algorithm does not prove.

Short code quotes:
- quote only 1-5 focused lines when needed;
- prefer references to exact files/classes: `XiigenExtensionApiContracts.cs`, `XiigenExtensionApiService.cs`, dashboard runner/request builder files;
- do not paste long code blocks into docs;
- after every quote, explain why it matters and what it does not prove.

## API/dashboard documentation requirements

Every API/dashboard section must show:
- human API name;
- technical method id;
- input parameter/prompt shape;
- example input;
- run action;
- returned result fields;
- warnings/errors/debug/raw JSON fields;
- correlation id and redaction status where available;
- evidence scope label;
- limitation statement.

Safe limitation wording:

```text
Этот пример доказывает только dashboard/API path for the named method. Он не доказывает live Claude/Cursor/Codex runtime, token savings, quality improvement, DPO training, checkpoint promotion or final readiness.
```

## Claude/Cursor/Codex runtime proof boundaries

Docs must use these boundaries:
- Claude: current evidence is narrow/no-eval local hook/core-boundary only; full Claude Desktop/chat UI remains open.
- Cursor: current evidence is blocked/deferred until safe launch/no-external-AI/no-backend contract and live Extension Host/UI proof exist.
- Codex: current evidence is installed plugin/cache adapter smoke only; full Codex desktop chat UI/app proof remains open.

Docs may explain how proof should look. Docs must not claim the proof exists unless a named runtime packet already proves that exact host/surface/scenario.

## Forbidden wording for next docs phase

Do not write:
- "готово", "полностью готово", "ready", "done", "complete", "PASS_WITH_LIMITATIONS" for the overall extension line;
- token savings proven;
- quality improvement proven;
- full semantic quality proven;
- learnability proven;
- runtime readiness proven for all hosts;
- common model readiness;
- target promotion;
- locator update;
- final readiness reviews passed.

Allowed wording:
- "documentation coverage improved";
- "dashboard/API behavior documented as supporting evidence";
- "runtime/token/quality/learnability evidence remains open";
- "следующий bounded work order известен".
