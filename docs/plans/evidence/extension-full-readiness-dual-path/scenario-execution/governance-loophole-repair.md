# Governance Loophole Repair Evidence Note

Authority quote from Luba:

```text
"и при этом чинил свои скилы позволяющие эти лазейки"
"запомни ты всё еще не исполнитель"
```

Active repair target: product-readiness work must not stop at downgrade,
blocker, not-proven, not-ready, needs-verification, no-claim, or status-only
evidence. Parent architect remains non-executor; execution and governance repair
work proceeds through bounded sub-agent packets.

Rules added:

- Downgrade/blocker/not-proven/status-only evidence is not completion and
  creates the next concrete work item unless all product requirements are proven
  or Luba explicitly stops.
- Product-readiness reports use scenario counters: total scenarios, fully joined
  proven, partial, not moved, blocked with next action, exact scenario moved.
- Every scenario should attempt dual-path proof where possible: automated/core/
  API path plus local real-program or closest honest local/test-only adapter
  path. If direct host UI is impossible, the report names a feasible alternate
  proof path instead of stopping.
- Completion reports separate evidence accounting done from product requirement
  proven.
- Parent architect may orchestrate, wait, review returned packets, request
  bounded fixes, and commit/push accepted sub-agent output only when assigned;
  parent direct-edit/executor work remains forbidden under architect-only or
  sub-agent-only authority.
- Parent Architect Subagent-Only Firewall exact phrase gate requires every
  scoped governance/skill file to contain: `parent local sidecar work ban`,
  `read-only verification after a sub-agent packet returns`,
  `sub-agent waiting is not a stop`, and
  `no local non-overlapping work escape`.

Human-readable Russian status example:

```text
Сценарии: всего 12; полностью доказано 3; частично доказано 5; не сдвинуто 2; заблокировано с понятным следующим действием 2.
Сдвинулся сценарий Codex prompt-to-task: automated boundary доказан, local proof частичный; продуктовая готовность ещё не доказана.
Учёт доказательств завершён, но требование продукта не доказано; следующий шаг — выполнить ближайший честный proof path и сохранить лог/скриншот.
```

## Per-File Firewall Phrase Verification

All 12 originally scoped files are intentionally in scope. Each row is expected
to contain all five exact phrases: `Parent Architect Subagent-Only Firewall`,
`parent local sidecar work ban`, `read-only verification after a sub-agent packet
returns`, `sub-agent waiting is not a stop`, and `no local non-overlapping work
escape`.

| File | Scope | Exact phrase result |
| --- | --- | --- |
| `AGENTS.md` | in scope | all five exact phrases present |
| `CLAUDE.md` | in scope | all five exact phrases present |
| `docs/HOW-TO-USE-SKILLS.md` | in scope | all five exact phrases present |
| `docs/SESSION-START-PROMPT.md` | in scope | all five exact phrases present |
| `docs/skills/phase-planning-SKILL.md` | in scope | all five exact phrases present |
| `docs/skills/plan-review-SKILL.md` | in scope | all five exact phrases present |
| `docs/skills/verification-before-completion-SKILL.md` | in scope | all five exact phrases present |
| `docs/skills/claim-verification-SKILL.md` | in scope | all five exact phrases present |
| `docs/skills/session-output-contract-SKILL.md` | in scope | all five exact phrases present |
| `docs/skills/plugin-runtime-reality-gate-SKILL.md` | in scope | all five exact phrases present |
| `docs/skills/agent-output-format-SKILL.md` | in scope | all five exact phrases present |
| `docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/governance-loophole-repair.md` | in scope | all five exact phrases present |
