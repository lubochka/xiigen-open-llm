# Phase 07.01 - Codex chat UI readiness/downgrade

CURRENT TASK: Phase 07.01 Codex chat UI readiness/downgrade matrix. Build a human-readable artifact that separates Codex plugin/core adapter evidence from real Codex desktop/chat UI readiness. Use existing evidence only; do not run probes.

active_task_sentence_source: Luba current unquoted instruction: "CURRENT TASK: Phase 07.01 Codex chat UI readiness/downgrade matrix. Build a human-readable artifact that separates Codex plugin/core adapter evidence from real Codex desktop/chat UI readiness. Use existing evidence only; do not run probes."

NOT THE TASK: no Codex GUI launch/click/type, no current active Codex conversation as proof, no prompts/messages, no new tests/builds/probes, no model/training/checkpoint/locator mutation, no code edit, no readiness claim, no `PASS_LOCAL_DESKTOP`, no `PASS_JOINED`, no stage/commit/push/switch.

EVIDENCE-ONLY: Phase 00 startup evidence, Phase 03 Automated boundary packets/report/command output, Phase 04 Codex local-route blocker evidence, Phase 05 product readiness matrix, Phase 06 full-extension readiness matrix, execution state/log, and the active plan Phase 07/08 text.

## Фаза и шаг

Фаза 8 из 17: Codex chat UI readiness/downgrade. Смысл фазы: отделить узкие доказательства Codex plugin/core adapter от реальной готовности Codex desktop/chat UI, чтобы helper/plugin/API/core evidence не стало подменой видимого Codex UI proof.

Шаг 07.01: intake/downgrade. Смысл шага: собрать human-readable matrix из существующих evidence files, явно показать, что доказано только narrow boundary/core slices, что не доказано для Codex desktop/chat UI, и какой следующий bounded step можно делать без probe authorization.

Статус: DONE_AS_MATRIX_INTAKE_ONLY. Это не Codex desktop/chat UI readiness, не product readiness, не full extension readiness и не joined pass.

## Короткое различие

Codex plugin/core adapter evidence - это узкое доказательство, что до уровня real Codex desktop/chat UI уже есть часть контрактов: API/core boundary tests, redacted debug trace contracts, thin-client/result-ref behavior, and downgrade wording. Это доказывает только contract/core boundary behavior внутри llm_mvp_core или рядом с ним.

Codex desktop/chat UI evidence - это отдельное доказательство, что реальный Codex app/chat UI был безопасно открыт или изолирован, показал нужный marker, не раскрыл private content, дал screenshot/video/log evidence, и был joined с boundary trace. Такого доказательства сейчас нет.

Текущая active Codex session не является чистым proof of extension chat UI readiness: она уже содержит этот разговор, workspace context, thread state and possible private project content. Она не является fresh isolated marker scenario, не даёт repeatable screenshot/log packet without private content, не доказывает prompt send/display through intended extension path, and cannot become `PASS_LOCAL_DESKTOP` or `PASS_JOINED` by observation alone.

## Evidence base

- [Phase 00/01 startup evidence](phase-00-01-startup-evidence.md): plugin-runtime-reality gate was loaded as governance context; this prevents helper/static/plugin proof from replacing host runtime proof.
- [Phase 03 Automated boundary report](phase-03-automated-boundary-report.md): Codex full-extension row 20 has `PASS_AUTOMATED_BOUNDARY` only; Codex chat UI row 21 has `NO_BOUNDARY_COMMAND_DISCOVERED`.
- [Phase 03 Automated boundary packets JSON](phase-03-automated-boundary-packets.json): machine-readable Codex rows and non-claims.
- [Phase 03 API/core command output](phase-03-command-output/02-dotnet-api-contracts.txt): prior evidence says 24 API/core/boundary contract tests passed, including redacted debug trace and thin-client contract names. This file is evidence only; no test was run in this step.
- [Phase 04 route establishment](phase-04-claude-codex-route-establishment.md): Codex route is `BLOCKED_WITH_REASON`.
- [Phase 04 route packets JSON](phase-04-claude-codex-route-establishment-packets.json): Codex has no clean marker-only surface separate from active conversation/workspace.
- [Phase 04 marker smoke policy](phase-04-local-desktop-evidence/08-marker-smoke-policy.json): Codex safe marker smoke was `NOT_RUN` because the current Codex chat UI may reveal this conversation or workspace details.
- [Phase 05 product readiness matrix](phase-05-product-readiness-matrix.md): full Codex chat UI readiness remains blocked/not proven.
- [Phase 06 full extension readiness matrix](phase-06-full-extension-readiness-matrix.md): Codex has narrow core/boundary rows and four `BLOCKED_WITH_REASON` rows.
- [Active plan Phase 07/08](../../EXTENSION-FULL-READINESS-DUAL-PATH-PLAN-v1.md): Phase 07 separates helper/plugin proof from real Codex chat UI; Phase 08 is token savings before/after.

## Top summary

| Metric | Count |
|---|---:|
| total rows | 9 |
| `PROVEN_NARROW` rows | 2 |
| downgraded rows | 3 |
| `BLOCKED_WITH_REASON` rows | 2 |
| `NOT_RUN` or `NOT_CAPTURED` rows | 2 |
| real Codex desktop/chat UI render rows proven | 0 |
| safe isolated Codex UI session rows proven | 0 |
| prompt/message sends in this step | 0 |
| screenshots captured in this step | 0 |
| formal `PASS_LOCAL_DESKTOP` rows | 0 |
| formal `PASS_JOINED` rows | 0 |
| full Codex chat UI readiness claims opened | 0 |

## Matrix rows

| row label | status | evidence links | missing evidence | next bounded step | overclaim ban |
|---|---|---|---|---|---|
| Installed Codex plugin/skill surface | `DOWNGRADED_TO_GOVERNANCE_SKILL_SURFACE` | [Phase 00 startup evidence](phase-00-01-startup-evidence.md); [Phase 06 matrix](phase-06-full-extension-readiness-matrix.md) | Direct installed Codex plugin runtime invocation proof is not present under this evidence root; startup/governance skill loading is not the same as real plugin runtime in Codex UI. | Keep as context only; a future plugin-runtime proof would need a separate bounded work order and accepted artifact. | Нельзя заявлять installed Codex plugin runtime or Codex UI readiness from startup skill/governance evidence. |
| Core adapter invocation | `PROVEN_NARROW` | [Phase 03 report row 20](phase-03-automated-boundary-report.md); [Phase 03 packets JSON](phase-03-automated-boundary-packets.json); [API/core output](phase-03-command-output/02-dotnet-api-contracts.txt) | No real Codex host invocation, no Codex desktop/chat UI marker, no joined evidence. | Join only after future safe Codex UI/local packet exists. | Нельзя превращать `PASS_AUTOMATED_BOUNDARY` into full extension readiness or Codex desktop/chat UI readiness. |
| Redacted debug trace availability | `PROVEN_NARROW` | [API/core output](phase-03-command-output/02-dotnet-api-contracts.txt); [Phase 06 Codex debug row](phase-06-full-extension-readiness-matrix.md) | No Codex UI screenshot/video redaction; no UI trace correlated to real Codex desktop/chat UI. | Future Codex UI proof must add redaction-reviewed screenshot/video/log and boundary correlation. | Нельзя заявлять readiness from readable/redacted debug trace alone. |
| Real Codex desktop/chat UI render | `BLOCKED_WITH_REASON` | [Phase 04 route establishment](phase-04-claude-codex-route-establishment.md); [Phase 04 route packets JSON](phase-04-claude-codex-route-establishment-packets.json) | No clean Codex desktop/chat UI render with marker; no screenshot/video/log. | Future safe route review, then marker proof only if Luba separately authorizes UI action. | Нельзя заявлять real Codex desktop/chat UI render from protocol/file association or current session. |
| Safe isolated Codex UI session availability | `BLOCKED_WITH_REASON` | [Phase 04 marker smoke policy](phase-04-local-desktop-evidence/08-marker-smoke-policy.json); [Phase 04 route packets JSON](phase-04-claude-codex-route-establishment-packets.json) | No isolated thread/window/file surface proven to hide current conversation, workspace path, account data, prompt text, and project content. | Future bounded route-design packet may propose an isolated route; this artifact does not authorize using it. | Нельзя использовать current active Codex session as clean proof. |
| Prompt send/display through Codex UI | `NOT_RUN` | [Phase 04 route establishment](phase-04-claude-codex-route-establishment.md); [Phase 04 packets JSON](phase-04-claude-codex-route-establishment-packets.json) | No prompt/message sent; no display through intended Codex UI extension path; no no-send route accepted. | Requires separate explicit authorization if ever pursued; current next plan phase does not grant it. | Нельзя заявлять prompt intake/display through Codex UI because it was not run. |
| Screenshot evidence without private content | `NOT_CAPTURED` | [Phase 04 marker smoke policy](phase-04-local-desktop-evidence/08-marker-smoke-policy.json); [Phase 04 route packets JSON](phase-04-claude-codex-route-establishment-packets.json) | No screenshot/video/log; no redaction-safe capture route; no private-content-free marker surface. | Future screenshot proof requires clean isolated UI route and redaction review before capture. | Нельзя заявлять screenshot evidence or `PASS_LOCAL_DESKTOP`; no screenshot was captured. |
| Token/quality/learnability through Codex UI | `DOWNGRADED_CORE_ONLY_NOT_UI` | [Phase 03 token trace output](phase-03-command-output/03-dotnet-token-trace.txt); [Phase 03 quality output](phase-03-command-output/04-dotnet-quality-scoring.txt); [Phase 05 matrix](phase-05-product-readiness-matrix.md) | Token/quality guards are not Codex UI before/after proof; learnability/training was not run; no Codex UI host-visible behavior. | Plan-derived next step is Phase 08 token savings before/after matrix/intake, with no probe authorization from this packet. | Нельзя заявлять token savings, semantic quality, or learnability through Codex UI from core guard tests. |
| Final Codex chat UI readiness conclusion | `NOT_READY_DOWNGRADED` | [Phase 03 row 21](phase-03-automated-boundary-report.md); [Phase 04 completion summary](phase-04-completion-downgrade-summary.md); [Phase 06 matrix](phase-06-full-extension-readiness-matrix.md); [Plan Phase 07](../../EXTENSION-FULL-READINESS-DUAL-PATH-PLAN-v1.md) | No real Codex app/chat UI marker evidence, no safe isolated UI session, no prompt display proof, no screenshot, no joined pass. | Parent review, then next numbered plan step is Phase 08.01 token savings before/after intake/downgrade, unless Luba separately authorizes a Codex UI safe-route proof. | Нельзя заявлять full Codex chat UI readiness, product readiness, full extension readiness, `PASS_LOCAL_DESKTOP`, or `PASS_JOINED`. |

## Counts and downgrade interpretation

| Status family | Rows | Meaning |
|---|---:|---|
| `PROVEN_NARROW` | 2 | Existing evidence supports API/core adapter invocation and redacted debug trace contracts only. |
| downgraded | 3 | Existing evidence is useful but narrower than the row label: governance skill surface, core-only token/quality guards, and final conclusion. |
| `BLOCKED_WITH_REASON` | 2 | Real Codex desktop/chat UI render and isolated session are blocked by the lack of clean, private-content-safe route. |
| `NOT_RUN` or `NOT_CAPTURED` | 2 | Prompt send/display and screenshot capture were not performed and are not authorized by this step. |

## Next bounded plan action

Plan-derived next action after parent review: Phase 08.01 token savings before/after intake/downgrade matrix. Meaning: build the next evidence/downgrade artifact for token savings using only evidence that is already authorized or already exists, and keep token savings closed unless the required before/after workload, tokenizer, quality guard, and host evidence are present.

Codex UI safe route proof is not authorized by this artifact. It remains a possible future bounded work order only if Luba explicitly authorizes UI route proof with privacy constraints.

## Verification plan for this artifact

- JSON parse `phase-07-codex-chat-ui-readiness-downgrade.json`.
- Exact search Markdown/JSON for the 9 required row labels and for `Codex desktop/chat UI`.
- Search unsupported readiness claim terms; every hit must be in title, requirement, distinction, missing-evidence, next-step, non-claim, downgrade, blocker, zero-count, or overclaim-ban context.
- Run `git diff --check` over changed files.

## Changed files intended by this step

- `docs/plans/evidence/extension-full-readiness-dual-path/phase-07-codex-chat-ui-readiness-downgrade.md`
- `docs/plans/evidence/extension-full-readiness-dual-path/phase-07-codex-chat-ui-readiness-downgrade.json`
- `docs/plans/EXTENSION-FULL-READINESS-DUAL-PATH-EXECUTION-STATE.md`
- `docs/plans/EXTENSION-FULL-READINESS-DUAL-PATH-EXECUTION-LOG.md`
