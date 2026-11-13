# Next Bounded Work Order - Human-readable Phase 01 inventory step

Фаза 2 из 17: Phase 01 - Scenario and claim inventory. Смысл фазы: превратить
широкие readiness claims по Claude/Cursor/Codex в конкретные host/scenario/claim
rows for later dual-path verification.

Шаг 01.01 из 01.02: Scenario and claim inventory. Смысл шага: создать первую
матрицу сценариев и claims without running runtime/host/training/checkpoint/
locator proof.

Статус: NOT_STARTED. This work order is queued for a future bounded sub-agent;
it was not executed by the reporting-contract repair.

Сделано: 1 of 17 full phases and 1 of 2 tracked Phase 01 steps. В процессе:
none for Phase 01.01 in this packet. Осталось: 15 full phases plus Phase 01.01.

Review нашёл: previous work-order title was an opaque code if shown alone.
Что исправлено: this file now begins with the human-readable phase/step/count
contract before the technical Phase 01.01 label.
Что осталось: execute this work order only after parent review accepts the
reporting-contract repair.
Следующее действие: bounded sub-agent creates the scenario/claim inventory
matrix and keeps all runtime evidence rows `NOT_RUN` or `NEEDS_VERIFICATION`.
Commit/continuation allowed: no git write/ref actions; continuation is limited
to the bounded inventory artifact.
opaque codes: `Phase 01.01 Scenario And Claim Inventory` is secondary only.

Use this as the next exact bounded sub-agent work order after parent review of
the Phase 00/01 opening packet.

```text
CURRENT TASK:
Execute Phase 01.01 of `docs/plans/EXTENSION-FULL-READINESS-DUAL-PATH-PLAN-v1.md`
as a bounded sub-agent: create the first scenario and claim inventory matrix for
Claude, Cursor, Codex and the required claim families. Keep every runtime,
host UI, token, quality, training, checkpoint, locator, and readiness result as
NOT_RUN/NEEDS_VERIFICATION unless this work order explicitly authorizes a local
verification command.

NOT THE TASK:
- Do not execute Phase 02 schema implementation yet.
- Do not run Claude/Cursor/Codex host UI tests.
- Do not run product code, training, DPO construction, checkpoint mutation,
  checkpoint promotion, locator update, browser checks, external AI, or web tools.
- Do not claim product readiness, full extension readiness, full Codex chat UI
  readiness, token savings, semantic quality improvement, learnability/training,
  DPO quality, checkpoint promotion, locator update, or host runtime pass.
- Do not git add/commit/push/checkout/reset/clean/branch/tag/merge/rebase.
- Do not overwrite unrelated work.

EVIDENCE-ONLY:
Old packets, quoted transcripts, previous plans, review ledger rows, existing
host evidence examples, and Section 7 candidate references are evidence only.
They may seed inventory rows but cannot prove current runtime, create git scope,
or open readiness claims.

requested_artifact_type:
execution evidence/status artifacts

allowed_write_scope:
- `docs/plans/EXTENSION-FULL-READINESS-DUAL-PATH-EXECUTION-STATE.md`
- `docs/plans/EXTENSION-FULL-READINESS-DUAL-PATH-EXECUTION-LOG.md`
- `docs/plans/evidence/extension-full-readiness-dual-path/phase-01-scenario-claim-inventory.md`
- optional companion inventory file under
  `docs/plans/evidence/extension-full-readiness-dual-path/phase-01-scenario-claim-inventory.json`

exact_current_luba_quote:
"Ок, приступай к исполнению через суб агентов."

required_rules_and_skills:
- `AGENTS.md`: Human Override Protocol, No Dishonest Claims, GOLD GATE,
  Parent Architect Subagent-Only Firewall, delegated bounded sub-agent actor
  rule, quoted/state authority quarantine, current-task anchor, git authority.
- `CLAUDE.md`: role-firewall authorship pattern, sub-agent-only firewall,
  claim/output honesty, dual-path host verification, no overclaim.
- `docs/plans/EXTENSION-FULL-READINESS-DUAL-PATH-PLAN-v1.md`: Sections 5, 6,
  7, 8 Phase 01, 9, 10, 11, 13, 15.
- `docs/plans/EXTENSION-FULL-READINESS-DUAL-PATH-PLAN-v1.review-ledger.md`:
  evidence boundary and non-claim wording.
- `docs/skills/planning-session-startup-SKILL.md`
- `docs/skills/phase-planning-SKILL.md`
- `docs/skills/plan-review-SKILL.md`
- `docs/skills/claim-verification-SKILL.md`
- `docs/skills/plugin-runtime-reality-gate-SKILL.md`
- `docs/skills/session-output-contract-SKILL.md`

maximum_active_time:
15 minutes expected. If the inventory cannot be made accurately in that slice,
return a partial packet with exact remaining rows and do not continue silently.

implementation_authorized_now:
true only for this evidence/status inventory artifact. Product/runtime/code
implementation is not authorized.

luba_approval_quote_or_NONE:
"Ок, приступай к исполнению через суб агентов."

quoted_material_is_evidence_only:
true

answer_first_interrupt_instruction:
If Luba asks a direct question or issues an interrupt, answer visibly first.
Then continue only if GOLD GATE classifies it as answer_first_then_continue and
the nearest safe already-authorized action still exists.

git_authority:
forbidden for git write/ref actions.

allowed_git_commands:
Only this verification command after edits:
`git diff --check -- docs/plans/EXTENSION-FULL-READINESS-DUAL-PATH-EXECUTION-STATE.md docs/plans/EXTENSION-FULL-READINESS-DUAL-PATH-EXECUTION-LOG.md docs/plans/evidence/extension-full-readiness-dual-path`

forbidden_git_commands:
`git add`, `git commit`, `git push`, `git checkout`, `git reset`, `git clean`,
`git branch`, `git tag`, `git merge`, `git rebase`, and every staging,
publishing, history, or ref-writing command.

commit_push_authorized:
false

allowed_verification:
- targeted textual search in the created Phase 01 inventory for:
  `scenario_id`, `host`, `claim_id`, `Automated boundary status`,
  `Local desktop status`, `evidence`, `remaining`, `downgraded_claim`,
  `NOT_RUN`, `NEEDS_VERIFICATION`.
- targeted textual search in the returned packet/status board for:
  `Фаза X из Y`, `Смысл фазы`, `Шаг X.Y`, `Смысл шага`, `Сделано`,
  `В процессе`, `Осталось`, `Review нашёл`, `Что исправлено`,
  `Следующее действие`, and `opaque codes`, or exact actual-number equivalents
  such as `Фаза 2 из 17` and `Шаг 01.01 из 01.02`.
- `git diff --check` command listed above.

forbidden_broad_verification:
No broad test suite, no host UI launch, no browser automation, no external AI,
no web search, no training/checkpoint/locator command, no broad folder grep as
completion evidence.

requested_model:
gpt-5.5 if available in the parent/sub-agent runtime; otherwise record
runtime_model_or_unavailable and do not make a model-quality claim.

requested_reasoning_effort:
xhigh if available; otherwise record runtime_reasoning_effort_or_unavailable
and do not hide missing metadata.

host:
cross-host inventory for Claude, Cursor, Codex. No host runtime execution.

scenario_id:
Create stable proposed ids, for example
`frdp-p01-<host>-<claim-family>-<short-workflow>`. Do not mark them executed.

marker:
Create proposed marker prefix `FRDP-P01` for future host runs. Do not claim the
marker was shown in any host.

Automated boundary path required:
Each inventory row must have an Automated boundary placeholder with status
`NOT_RUN` or `NEEDS_VERIFICATION`, candidate references if available from the
plan, and remaining verification work.

Local desktop path required:
Each inventory row must have a Local desktop placeholder with status `NOT_RUN`,
real host activation unknown/blocker field, required screenshot/video/log field,
and downgrade behavior.

exact blocker policy:
Use `NOT_DISCOVERED_YET` for work not attempted. Use `BLOCKED_*` only with an
actual blocker observed in this work order.

security_privacy_redaction_required:
true for any future screenshot/video/log evidence. This inventory slice should
not capture host screenshots/logs.

docs_skills_guides_agent_propagation_required:
not in this slice. Record as future Phase 16 dependency only.

commit_push_boundary_owner:
Parent or separate commit-only work order, only if Luba later authorizes git.

parent_will_perform_commit_push_after_review:
false under the current no-git instruction; parent may do it only after a fresh
separate commit/push work order.

Return packet must include:
- changed/created files;
- inventory row counts by host and claim family;
- proof that every row has both Automated boundary and Local desktop placeholders;
- Russian-first human-readable execution status:
  `Фаза X из Y: <человеческое название>. Смысл фазы: <что эта фаза решает для продукта>.`
  `Шаг X.Y из X.N: <человеческое название>. Смысл шага: <что этот шаг проверяет/создаёт>.`
  `Статус: DONE | IN_PROGRESS | NEEDS_FIX | BLOCKED_WITH_REASON | NOT_STARTED` plus human explanation.
  `Сделано: <n> фаз/шагов. В процессе: <текущая фаза/шаг>. Осталось: <n> фаз/шагов.`
  If review/fix exists, include `Review нашёл`, `Что исправлено`, and `Что осталось`.
  Include `Следующее действие` pointing to the next bounded sub-agent work order
  or `NONE`, and state whether commit/continuation is allowed.
  Opaque codes such as `Phase 01.01 Scenario And Claim Inventory`, JSON paths,
  STATE flags, and ALL_CAPS labels may appear only after the human explanation.
- exact evidence paths;
- verification results;
- explicit non-claims;
- no git write/ref actions used.
```
