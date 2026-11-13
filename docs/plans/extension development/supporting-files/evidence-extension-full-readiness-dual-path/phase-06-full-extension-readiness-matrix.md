# Phase 06.01 - Full extension readiness matrix

CURRENT TASK: Phase 06.01 Full extension readiness matrix for Claude, Cursor, and Codex. Build a human-readable downgrade/intake matrix from existing evidence only. Use Russian human-readable status and counts.

active_task_sentence_source: Luba current unquoted instruction: "CURRENT TASK: Phase 06.01 Full extension readiness matrix for Claude, Cursor, and Codex. Build a human-readable downgrade/intake matrix from existing evidence only."

NOT THE TASK: no new probes/tests/builds, no GUI launch, no prompts/messages, no model/training/checkpoint/locator mutation, no code edit, no product/full-extension/Codex-chat/token/semantic/learnability/checkpoint/locator readiness claim, no `PASS_LOCAL_DESKTOP`, no `PASS_JOINED`, no git stage/commit/push/switch.

EVIDENCE-ONLY: Phase 01 inventory, Phase 03 Automated boundary report/packets/command outputs, Phase 04 local desktop downgrade/marker/blocker evidence, Phase 05 product readiness matrix, execution state/log. These files are evidence inputs only and do not create new readiness.

## Фаза и шаг

Фаза 7 из 17: Full extension readiness matrix. Смысл фазы: отделить узкие доказанные core/boundary факты от реальной готовности расширения в Claude, Cursor и Codex, чтобы ни один host не получил full-readiness claim без joined host evidence.

Шаг 06.01: intake/downgrade. Смысл шага: собрать 30 строк по 10 capability rows для каждого host, привязать каждую строку к существующим evidence links, записать missing evidence, следующий bounded step и запрет overclaim.

Статус: DONE_AS_MATRIX_INTAKE_ONLY. Это матрица downgrade/intake, а не full extension readiness, не product readiness и не joined pass.

## Top summary

| Metric | Count |
|---|---:|
| total rows | 30 |
| PROVEN_NARROW total | 18 |
| DOWNGRADED total | 4 |
| NOT_READY total | 0 |
| BLOCKED_WITH_REASON total | 8 |
| NOT_RUN total | 0 |
| formal `PASS_LOCAL_DESKTOP` rows | 0 |
| formal `PASS_JOINED` rows | 0 |
| full extension readiness claims opened | 0 |

## Per-host counts

| Host | PROVEN_NARROW | DOWNGRADED | NOT_READY | BLOCKED_WITH_REASON | NOT_RUN | Human meaning |
|---|---:|---:|---:|---:|---:|---|
| Claude | 6 | 0 | 0 | 4 | 0 | Есть narrow API/core boundary evidence, но safe local prompt/host route заблокирован. |
| Cursor | 6 | 4 | 0 | 0 | 0 | Есть narrow API/core boundary evidence и marker-only Cursor editor evidence; это не prompt intake и не readiness. |
| Codex | 6 | 0 | 0 | 4 | 0 | Есть narrow API/core boundary evidence, но safe Codex route/chat UI route заблокирован. |

## Why no host can be called fully ready

No host can be called fully ready. The evidence supports only these limited claims:

- Claude: narrow Automated boundary/API-core evidence exists for core routing capability rows, but Claude local route is `BLOCKED_WITH_REASON`; allowed claim is only "boundary-only core slice recorded, host readiness not proven."
- Cursor: narrow Automated boundary/API-core evidence exists and one marker-only local file/editor crop exists for `frdp-p01-cursor-full-extension-entrypoint`; allowed claim is only "Cursor marker-only local display plus boundary-only core slice, not prompt intake and not extension readiness."
- Codex: narrow Automated boundary/API-core evidence exists for core routing capability rows, but Codex route and Codex chat UI route are `BLOCKED_WITH_REASON`; allowed claim is only "boundary-only core slice recorded, Codex host/chat readiness not proven."

Phase 03 accepted 12 Automated boundary rows out of 28, but those are boundary-only. Phase 04 has one Cursor marker-only row, Claude and Codex blockers, formal local desktop pass rows = 0, formal joined pass rows = 0, and readiness claims opened = 0.

## Explicit non-claims

- This matrix does not claim product readiness.
- This matrix does not claim full extension readiness.
- This matrix does not claim full Codex chat UI readiness.
- This matrix does not claim token savings.
- This matrix does not claim semantic quality improvement.
- This matrix does not claim learnability/training success.
- This matrix does not claim checkpoint mutation, checkpoint promotion, or rollback safety.
- This matrix does not claim locator update or host reload from locator.
- This matrix does not claim `PASS_LOCAL_DESKTOP`.
- This matrix does not claim `PASS_JOINED`.

## Evidence base

- [Phase 01 scenario/claim inventory](phase-01-scenario-claim-inventory.md): 28 scenario rows and host/claim ids.
- [Phase 03 Automated boundary report](phase-03-automated-boundary-report.md): 12 accepted boundary-only rows, 16 needs-verification rows, 28 local desktop rows not run.
- [Phase 03 Automated boundary packets JSON](phase-03-automated-boundary-packets.json): machine-readable boundary packet statuses.
- [Phase 04 Cursor marker probe](phase-04-local-desktop-cursor-marker-probe.md): Cursor marker-only local editor proof.
- [Cursor redaction note](phase-04-local-desktop-evidence/cursor/18-cursor-marker-editor-crop-redaction-note.txt): accepted crop, redaction not required.
- [Phase 04 Claude/Codex route establishment](phase-04-claude-codex-route-establishment.md): Claude and Codex `BLOCKED_WITH_REASON` route decisions.
- [Phase 04 local desktop packets JSON](phase-04-local-desktop-packets.json): Cursor marker-only, Claude blocked, Codex blocked rollup.
- [Phase 04 status board](phase-04-local-desktop-status-board.md): final local desktop counts.
- [Phase 05 product readiness matrix](phase-05-product-readiness-matrix.md): product/readiness downgrade context.
- [Execution state](../../EXTENSION-FULL-READINESS-DUAL-PATH-EXECUTION-STATE.md) and [execution log](../../EXTENSION-FULL-READINESS-DUAL-PATH-EXECUTION-LOG.md): rolling status.

## Capability labels

1. Host entrypoint exists and is the intended host surface.
2. User prompt intake path.
3. Prompt -> Task route through llm_mvp_core.
4. Task -> Orchestration plan route through llm_mvp_core.
5. Task/Plan -> Skill selection route through llm_mvp_core.
6. AvailableSkills/RequestedSkills binding.
7. Host UI/local desktop proof.
8. Debug/evidence trace is redacted and readable.
9. Thin-host boundary: logic stays in llm_mvp_core.
10. Extension readiness conclusion for that host.

## Matrix rows

| host | capability_id | capability label | Russian human-readable capability name | what it means | current status | evidence links | missing evidence | next bounded step | overclaim ban sentence |
|---|---|---|---|---|---|---|---|---|---|
| Claude | CLAUDE-HFE-01 | Host entrypoint exists and is the intended host surface. | Точка входа Claude существует и является целевой host surface. | Надо доказать безопасную локальную Claude surface, куда можно показать marker без prompt/message send. | `BLOCKED_WITH_REASON` | [Phase 04 route establishment](phase-04-claude-codex-route-establishment.md); [Phase 04 packets](phase-04-local-desktop-packets.json) | No proof that `claude --name` launch is local-only/no-send; no safe Claude marker route. | Future bounded safe sandbox/manual route proof for Claude before marker probe. | Нельзя заявлять Claude host entrypoint readiness из `--help` metadata или boundary-only evidence. |
| Claude | CLAUDE-HFE-02 | User prompt intake path. | Путь приёма пользовательского prompt в Claude. | Надо доказать, что user prompt can enter Claude extension path without unsafe send or UI ambiguity. | `BLOCKED_WITH_REASON` | [Phase 04 route establishment](phase-04-claude-codex-route-establishment.md); [Phase 04 summary](phase-04-completion-downgrade-summary.md) | No prompt/message was sent; safe local/no-send route not proven. | Establish a safe Claude intake route under a bounded no-send work order. | Нельзя заявлять Claude prompt intake path, потому что prompt intake не запускался и route blocked. |
| Claude | CLAUDE-HFE-03 | Prompt -> Task route through llm_mvp_core. | Prompt -> Task route через llm_mvp_core. | Надо показать, что host-thin request can reach core prompt-to-task logic. | `PROVEN_NARROW` | [Phase 03 report](phase-03-automated-boundary-report.md); [Phase 03 packets](phase-03-automated-boundary-packets.json); [API/core command output](phase-03-command-output/02-dotnet-api-contracts.txt) | No joined Claude host invocation; no local desktop proof. | Join with future Claude local desktop packet only after safe route exists. | Нельзя превращать API/core boundary proof в Claude full extension readiness. |
| Claude | CLAUDE-HFE-04 | Task -> Orchestration plan route through llm_mvp_core. | Task -> Orchestration plan route через llm_mvp_core. | Надо показать, что task can be converted into orchestration plan in core boundary. | `PROVEN_NARROW` | [Phase 03 report](phase-03-automated-boundary-report.md); [Phase 03 packets](phase-03-automated-boundary-packets.json); [API/core command output](phase-03-command-output/02-dotnet-api-contracts.txt) | No Claude host task/plan evidence; no joined report. | Future joined evidence after Claude route proof. | Нельзя заявлять Claude orchestration readiness без real host/local desktop evidence. |
| Claude | CLAUDE-HFE-05 | Task/Plan -> Skill selection route through llm_mvp_core. | Task/Plan -> Skill selection route через llm_mvp_core. | Надо показать, что core can select skills from task/plan boundary. | `PROVEN_NARROW` | [Phase 03 report](phase-03-automated-boundary-report.md); [Phase 03 packets](phase-03-automated-boundary-packets.json); [API/core command output](phase-03-command-output/02-dotnet-api-contracts.txt) | No Claude host skill-selection invocation; no joined report. | Future Claude host packet plus boundary join. | Нельзя заявлять skill selection quality or host readiness from boundary-only contract evidence. |
| Claude | CLAUDE-HFE-06 | AvailableSkills/RequestedSkills binding. | Связка AvailableSkills/RequestedSkills. | Надо показать, что requested skills bind to available skills in core without host logic duplication. | `PROVEN_NARROW` | [Phase 03 report](phase-03-automated-boundary-report.md); [Phase 03 packets](phase-03-automated-boundary-packets.json); [Phase 05 matrix](phase-05-product-readiness-matrix.md) | No Claude host-visible skills binding; no real extension session. | Future host evidence for the same scenario id and marker. | Нельзя заявлять host skill-binding readiness; доказан только narrow boundary context. |
| Claude | CLAUDE-HFE-07 | Host UI/local desktop proof. | Доказательство Claude Host UI/local desktop. | Надо иметь screenshot/video/log/marker or exact blocker for Claude local host. | `BLOCKED_WITH_REASON` | [Phase 04 route establishment](phase-04-claude-codex-route-establishment.md); [Phase 04 status board](phase-04-local-desktop-status-board.md); [Phase 04 packets](phase-04-local-desktop-packets.json) | Safe Claude marker route is not proven; screenshot/video/log not captured. | Future safe Claude local route review, then marker probe if authorized. | Нельзя заявлять `PASS_LOCAL_DESKTOP` for Claude; current evidence is a blocker. |
| Claude | CLAUDE-HFE-08 | Debug/evidence trace is redacted and readable. | Debug/evidence trace читаем и redaction-safe. | Надо иметь readable evidence trace and privacy/redaction status for existing artifacts. | `PROVEN_NARROW` | [Phase 03 report](phase-03-automated-boundary-report.md); [Phase 04 route establishment](phase-04-claude-codex-route-establishment.md); [Phase 05 matrix](phase-05-product-readiness-matrix.md) | No Claude screenshot/video redaction because no safe capture exists. | Future Claude route proof must add redaction-reviewed local artifact. | Нельзя заявлять readiness from readable blocker/debug trace. |
| Claude | CLAUDE-HFE-09 | Thin-host boundary: logic stays in llm_mvp_core. | Thin-host boundary: логика остаётся в llm_mvp_core. | Host wrapper must stay thin; routing logic belongs in core boundary. | `PROVEN_NARROW` | [Phase 03 report](phase-03-automated-boundary-report.md); [Phase 03 packets](phase-03-automated-boundary-packets.json); [Phase 05 matrix](phase-05-product-readiness-matrix.md) | No Claude host runtime proof that wrapper stayed thin during real host invocation. | Future Claude host invocation plus boundary trace join. | Нельзя заявлять full Claude readiness from thin-boundary proof alone. |
| Claude | CLAUDE-HFE-10 | Extension readiness conclusion for that host. | Итог готовности Claude extension. | Final host conclusion must combine entrypoint, prompt intake, core routes, skills, local UI proof, trace, and thin boundary. | `BLOCKED_WITH_REASON` | [Phase 03 report](phase-03-automated-boundary-report.md); [Phase 04 route establishment](phase-04-claude-codex-route-establishment.md); [Phase 05 matrix](phase-05-product-readiness-matrix.md) | Claude local route blocked; no formal local pass; no joined pass. | Repair Claude blocker before any readiness reassessment. | Нельзя заявлять Claude full extension readiness; evidence supports only boundary-only narrow rows plus blocker. |
| Cursor | CURSOR-HFE-01 | Host entrypoint exists and is the intended host surface. | Точка входа Cursor существует и является целевой host surface. | Надо доказать intended Cursor extension surface, not just opening a local marker file. | `DOWNGRADED` | [Cursor marker probe](phase-04-local-desktop-cursor-marker-probe.md); [Cursor command log](phase-04-local-desktop-evidence/cursor/15-cursor-marker-open-command.txt); [Phase 04 packets](phase-04-local-desktop-packets.json) | Only local file/editor marker route is proven; no Cursor extension/chat/AI surface. | Future Cursor extension/intake proof if safe and authorized. | Нельзя заявлять Cursor host entrypoint readiness from marker-only file open. |
| Cursor | CURSOR-HFE-02 | User prompt intake path. | Путь приёма пользовательского prompt в Cursor. | Надо доказать user prompt intake into the intended extension path. | `DOWNGRADED` | [Cursor marker probe](phase-04-local-desktop-cursor-marker-probe.md); [Cursor redaction note](phase-04-local-desktop-evidence/cursor/18-cursor-marker-editor-crop-redaction-note.txt) | No prompt/message was sent; Cursor chat/external service not used; marker-only is not prompt intake. | Future no-send or reviewed Cursor prompt-intake route proof. | Нельзя заявлять Cursor prompt intake; proven evidence is marker-only local display. |
| Cursor | CURSOR-HFE-03 | Prompt -> Task route through llm_mvp_core. | Prompt -> Task route через llm_mvp_core. | Надо показать core prompt-to-task boundary and later join it with Cursor host evidence. | `PROVEN_NARROW` | [Phase 03 report](phase-03-automated-boundary-report.md); [Phase 03 packets](phase-03-automated-boundary-packets.json); [API/core command output](phase-03-command-output/02-dotnet-api-contracts.txt) | No Cursor prompt->task joined host invocation. | Join future Cursor local packet with existing boundary row. | Нельзя заявлять Cursor full extension readiness from core prompt-to-task boundary only. |
| Cursor | CURSOR-HFE-04 | Task -> Orchestration plan route through llm_mvp_core. | Task -> Orchestration plan route через llm_mvp_core. | Надо показать task-to-plan route in core and join it with Cursor host path. | `PROVEN_NARROW` | [Phase 03 report](phase-03-automated-boundary-report.md); [Phase 03 packets](phase-03-automated-boundary-packets.json); [API/core command output](phase-03-command-output/02-dotnet-api-contracts.txt) | No Cursor joined task/plan host evidence. | Future joined Cursor evidence for same scenario id/marker. | Нельзя заявлять Cursor orchestration readiness without joined host proof. |
| Cursor | CURSOR-HFE-05 | Task/Plan -> Skill selection route through llm_mvp_core. | Task/Plan -> Skill selection route через llm_mvp_core. | Надо показать core skill-selection boundary and host-independent logic. | `PROVEN_NARROW` | [Phase 03 report](phase-03-automated-boundary-report.md); [Phase 03 packets](phase-03-automated-boundary-packets.json); [Phase 05 matrix](phase-05-product-readiness-matrix.md) | No Cursor host-visible skill selection; no joined report. | Future Cursor extension route plus joined evidence. | Нельзя заявлять Cursor skill selection quality or readiness from boundary-only evidence. |
| Cursor | CURSOR-HFE-06 | AvailableSkills/RequestedSkills binding. | Связка AvailableSkills/RequestedSkills. | Надо показать binding in core and later prove host uses the same path. | `PROVEN_NARROW` | [Phase 03 report](phase-03-automated-boundary-report.md); [Phase 03 packets](phase-03-automated-boundary-packets.json); [Phase 05 matrix](phase-05-product-readiness-matrix.md) | No Cursor host-visible requested/available skills binding. | Future Cursor host evidence for skills binding. | Нельзя заявлять full Cursor skill binding readiness; доказан только narrow boundary context. |
| Cursor | CURSOR-HFE-07 | Host UI/local desktop proof. | Доказательство Cursor Host UI/local desktop. | Надо иметь local host screenshot/log/marker evidence that is accepted and redaction-safe. | `DOWNGRADED` | [Cursor marker probe](phase-04-local-desktop-cursor-marker-probe.md); [Cursor redaction note](phase-04-local-desktop-evidence/cursor/18-cursor-marker-editor-crop-redaction-note.txt); [Cursor crop](phase-04-local-desktop-evidence/cursor/18-cursor-marker-editor-crop-redaction-not-required.png) | Only one marker-only local row; no formal local desktop pass; no joined pass. | Future reviewed Cursor extension/host scenario beyond marker-only. | Нельзя заявлять formal `PASS_LOCAL_DESKTOP` or Cursor readiness from marker-only crop. |
| Cursor | CURSOR-HFE-08 | Debug/evidence trace is redacted and readable. | Debug/evidence trace читаем и redaction-safe. | Existing trace must be readable and privacy-reviewed. | `PROVEN_NARROW` | [Cursor marker probe](phase-04-local-desktop-cursor-marker-probe.md); [Cursor redaction note](phase-04-local-desktop-evidence/cursor/18-cursor-marker-editor-crop-redaction-note.txt); [Phase 04 status board](phase-04-local-desktop-status-board.md) | No full extension trace; only marker-only trace is redaction-reviewed. | Future Cursor host proof must add redacted/readable trace. | Нельзя заявлять readiness from readable marker-only trace. |
| Cursor | CURSOR-HFE-09 | Thin-host boundary: logic stays in llm_mvp_core. | Thin-host boundary: логика остаётся в llm_mvp_core. | Host should remain thin while core owns prompt/task/plan/skill logic. | `PROVEN_NARROW` | [Phase 03 report](phase-03-automated-boundary-report.md); [Phase 03 packets](phase-03-automated-boundary-packets.json); [Phase 05 matrix](phase-05-product-readiness-matrix.md) | No Cursor real extension invocation proving thin host at runtime. | Join future Cursor host trace with boundary proof. | Нельзя заявлять Cursor full readiness from thin-boundary proof alone. |
| Cursor | CURSOR-HFE-10 | Extension readiness conclusion for that host. | Итог готовности Cursor extension. | Final host conclusion must combine core boundary, prompt intake, local host proof, skills, debug trace, and joined pass. | `DOWNGRADED` | [Phase 03 report](phase-03-automated-boundary-report.md); [Phase 04 Cursor marker probe](phase-04-local-desktop-cursor-marker-probe.md); [Phase 05 matrix](phase-05-product-readiness-matrix.md) | Marker-only local evidence; no prompt intake; no formal local pass; no joined pass. | Continue only after bounded Cursor extension/intake evidence exists. | Нельзя заявлять Cursor full extension readiness; allowed claim is marker-only local evidence plus boundary-only core slices. |
| Codex | CODEX-HFE-01 | Host entrypoint exists and is the intended host surface. | Точка входа Codex существует и является целевой host surface. | Надо доказать clean Codex app/chat or extension surface separate from active conversation/workspace. | `BLOCKED_WITH_REASON` | [Phase 04 route establishment](phase-04-claude-codex-route-establishment.md); [Phase 04 packets](phase-04-local-desktop-packets.json) | Packaged CLI route Access denied; protocol/file associations do not prove marker-only surface. | Future Codex safe route decision before marker probe. | Нельзя заявлять Codex host entrypoint readiness from protocol/file-association metadata. |
| Codex | CODEX-HFE-02 | User prompt intake path. | Путь приёма пользовательского prompt в Codex. | Надо доказать Codex prompt intake into the intended route without using current conversation as proof. | `BLOCKED_WITH_REASON` | [Phase 04 route establishment](phase-04-claude-codex-route-establishment.md); [Phase 04 summary](phase-04-completion-downgrade-summary.md) | No prompt/message was sent; no clean marker-only surface separate from active conversation/workspace. | Future clean Codex route/thread/surface proof, no model prompt send unless authorized. | Нельзя заявлять Codex prompt intake from this active Codex conversation or metadata. |
| Codex | CODEX-HFE-03 | Prompt -> Task route through llm_mvp_core. | Prompt -> Task route через llm_mvp_core. | Надо показать core prompt-to-task boundary and later join it with Codex host evidence. | `PROVEN_NARROW` | [Phase 03 report](phase-03-automated-boundary-report.md); [Phase 03 packets](phase-03-automated-boundary-packets.json); [API/core command output](phase-03-command-output/02-dotnet-api-contracts.txt) | No Codex host invocation; no Codex chat UI evidence. | Future Codex host/chat packet after safe route exists. | Нельзя заявлять Codex readiness from core prompt-to-task boundary only. |
| Codex | CODEX-HFE-04 | Task -> Orchestration plan route through llm_mvp_core. | Task -> Orchestration plan route через llm_mvp_core. | Надо показать task-to-plan core route and joined host evidence later. | `PROVEN_NARROW` | [Phase 03 report](phase-03-automated-boundary-report.md); [Phase 03 packets](phase-03-automated-boundary-packets.json); [API/core command output](phase-03-command-output/02-dotnet-api-contracts.txt) | No Codex host task/plan evidence; no joined pass. | Future Codex host route proof and joined report. | Нельзя заявлять Codex orchestration readiness without Codex host evidence. |
| Codex | CODEX-HFE-05 | Task/Plan -> Skill selection route through llm_mvp_core. | Task/Plan -> Skill selection route через llm_mvp_core. | Надо показать skill selection in core and no host logic substitution. | `PROVEN_NARROW` | [Phase 03 report](phase-03-automated-boundary-report.md); [Phase 03 packets](phase-03-automated-boundary-packets.json); [Phase 05 matrix](phase-05-product-readiness-matrix.md) | No Codex host skill selection invocation. | Future Codex host evidence for same scenario. | Нельзя заявлять Codex skill selection readiness from boundary-only proof. |
| Codex | CODEX-HFE-06 | AvailableSkills/RequestedSkills binding. | Связка AvailableSkills/RequestedSkills. | Надо показать core binding and later Codex host use of that binding. | `PROVEN_NARROW` | [Phase 03 report](phase-03-automated-boundary-report.md); [Phase 03 packets](phase-03-automated-boundary-packets.json); [Phase 05 matrix](phase-05-product-readiness-matrix.md) | No Codex host-visible skills binding; no joined report. | Future Codex host route and joined binding proof. | Нельзя заявлять Codex skill-binding readiness; доказан только narrow boundary context. |
| Codex | CODEX-HFE-07 | Host UI/local desktop proof. | Доказательство Codex Host UI/local desktop. | Надо иметь clean Codex UI/local desktop marker/log/screenshot or exact blocker. | `BLOCKED_WITH_REASON` | [Phase 04 route establishment](phase-04-claude-codex-route-establishment.md); [Phase 04 status board](phase-04-local-desktop-status-board.md); [Phase 04 packets](phase-04-local-desktop-packets.json) | No clean Codex marker-only surface; no screenshot/video/log captured. | Future Codex route review, then marker probe only if safe. | Нельзя заявлять `PASS_LOCAL_DESKTOP` for Codex; current evidence is blocker-only. |
| Codex | CODEX-HFE-08 | Debug/evidence trace is redacted and readable. | Debug/evidence trace читаем и redaction-safe. | Existing Codex blocker/debug trace must be readable and explicit about privacy. | `PROVEN_NARROW` | [Phase 04 route establishment](phase-04-claude-codex-route-establishment.md); [Phase 04 packets](phase-04-local-desktop-packets.json); [Phase 05 matrix](phase-05-product-readiness-matrix.md) | No Codex screenshot/video redaction because clean capture route not proven. | Future Codex route proof must include redaction-reviewed evidence. | Нельзя заявлять Codex readiness from readable blocker/debug trace. |
| Codex | CODEX-HFE-09 | Thin-host boundary: logic stays in llm_mvp_core. | Thin-host boundary: логика остаётся в llm_mvp_core. | Codex host wrapper should stay thin while core owns routing logic. | `PROVEN_NARROW` | [Phase 03 report](phase-03-automated-boundary-report.md); [Phase 03 packets](phase-03-automated-boundary-packets.json); [Phase 05 matrix](phase-05-product-readiness-matrix.md) | No Codex real host invocation proving thin wrapper at runtime. | Future Codex host/chat trace plus boundary join. | Нельзя заявлять Codex full readiness from thin-boundary proof alone. |
| Codex | CODEX-HFE-10 | Extension readiness conclusion for that host. | Итог готовности Codex extension. | Final host conclusion must include Codex entrypoint, prompt intake, core routes, skills, local UI proof, trace, and joined pass. | `BLOCKED_WITH_REASON` | [Phase 03 report](phase-03-automated-boundary-report.md); [Phase 04 route establishment](phase-04-claude-codex-route-establishment.md); [Phase 05 matrix](phase-05-product-readiness-matrix.md) | Codex route/chat UI blocked; no formal local pass; no joined pass. | Repair Codex blocker and Phase 07 Codex chat UI path before reassessment. | Нельзя заявлять Codex full extension readiness or full Codex chat UI readiness; evidence supports only boundary-only narrow rows plus blocker. |

## Counts and downgrade interpretation

| Status | Rows | Meaning |
|---|---:|---|
| `PROVEN_NARROW` | 18 | Boundary/debug/thin-host/core-route evidence exists only in a narrow, non-joined sense. |
| `DOWNGRADED` | 4 | Cursor has marker-only local evidence, which downgrades intended host/prompt/local/conclusion rows. |
| `NOT_READY` | 0 | Rows with hard missing readiness evidence are expressed as downgrade or explicit blocker because each has a known evidence context. |
| `BLOCKED_WITH_REASON` | 8 | Claude and Codex host/prompt/local/conclusion rows are blocked by exact Phase 04 route blockers. |
| `NOT_RUN` | 0 | Phase 06.01 itself is an evidence-only matrix; no new runtime action was scheduled or run. |

## Verification plan for this artifact

- JSON parse `phase-06-full-extension-readiness-matrix.json`.
- Exact search Markdown/JSON for host names Claude/Cursor/Codex and all 10 capability labels.
- Verify counts in Markdown and JSON agree: total 30; Claude 6/0/0/4/0; Cursor 6/4/0/0/0; Codex 6/0/0/4/0.
- Search unsupported readiness claim terms; every hit must be requirement name, non-claim, downgrade, blocker, missing-evidence, zero-count, or overclaim-ban context.
- Run `git diff --check` over changed files.

## Changed files intended by this step

- `docs/plans/evidence/extension-full-readiness-dual-path/phase-06-full-extension-readiness-matrix.md`
- `docs/plans/evidence/extension-full-readiness-dual-path/phase-06-full-extension-readiness-matrix.json`
- `docs/plans/EXTENSION-FULL-READINESS-DUAL-PATH-EXECUTION-STATE.md`
- `docs/plans/EXTENSION-FULL-READINESS-DUAL-PATH-EXECUTION-LOG.md`
