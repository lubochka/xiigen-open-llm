# Governance Human Status Repair

Run id: `governance-human-status-repair-20260618-111642`

## Что исправлено

Эта governance repair закрывает машинно-читаемый обход статуса. Теперь каждый
multi-phase status/update/review/final-or-near-final report должен показывать
человеческую доску: всего задач, сделано с именами, текущая задача и владелец,
осталось с именами, тип текущего результата, закрытые claims с причинами и
следующее bounded действие.

Лог или статус провален, если Luba не может понять из видимого сообщения, что
сделано, что выполняется, что осталось, является ли результат кодом,
артефактом, review, policy или evidence-only, и какие claims остаются закрытыми.
Такой провал запускает governance/skill repair перед product continuation, а
не финальную остановку.

Code review теперь обязан проверять не только тесты или QSG label, а полный
scope: project coding standards, allowed tests/Gate B, границу
`llm_mvp_core`, тонкие Claude/Cursor/Codex host adapters, hidden deterministic
shortcut sweep, маркировку deterministic helper как eval-only/helper/internal
baseline и влияние на docs/QA/dashboard.

Final readiness требует минимум 10 PASS independent execution reviews по
полному исходному target: три полных Claude/Cursor/Codex extensions,
orchestration/skills, local user model export/import/backup/continue-training,
DPO/source quality, runtime/e2e/QA docs и отсутствие hidden deterministic
mechanisms. Non-PASS review не считается; если review находит premature
completion loophole, skills/guides/agent files чинятся и pass пересчитывается
только после recheck.

## Changed files

- `AGENTS.md`
- `CLAUDE.md`
- `docs/HOW-TO-USE-SKILLS.md`
- `docs/skills/agent-output-format-SKILL.md`
- `docs/skills/code-examination-SKILL.md`
- `docs/skills/generated-code-review-SKILL.md`
- `docs/skills/implementation-integrity-SKILL.md`
- `docs/skills/output-readability-gate-SKILL.md`
- `docs/skills/phase-completion-packager-SKILL.md`
- `docs/skills/plan-review-SKILL.md`
- `docs/skills/session-execution-log-SKILL.md`
- `docs/skills/skill-injection-SKILL.md`
- `docs/skills/test-integrity-SKILL.md`
- `docs/skills/trainable-architecture-decision-SKILL.md`
- `docs/skills/verification-before-completion-SKILL.md`
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`
- `artifacts/extension-runtime-token-quality-learnability/governance-human-status-repair/governance-human-status-repair-20260618-111642/audit.json`
- `artifacts/extension-runtime-token-quality-learnability/governance-human-status-repair/governance-human-status-repair-20260618-111642/audit.md`

## Before/After loopholes closed

| Before | After |
|---|---|
| Status could be only QSG/STATE/JSON labels. | Primary status must be a human-readable board with counts, names, result type, closed claims, and next bounded action. |
| Log could be parseable but not understandable to Luba. | Human gate fails such logs and forces governance/skill repair before product continuation. |
| Code review could accept tests/QSG without core-boundary and deterministic-sweep scope. | Execution/code review completeness gate blocks acceptance until standards, tests, core boundary, thin adapters, deterministic labels, and docs/QA/dashboard impact are checked. |
| Final readiness could count summary/non-PASS/narrow helper evidence. | 10 PASS independent execution reviews must cover the full target; findings repair governance before the review counts. |
| Sub-agent work orders could omit human-readable status and exact review scope. | Skill injection and index rules require progress board fields and exact files/sections/hosts/claims scope. |

## Checks run

- Markdown/readability sanity with targeted `rg` for new gate phrases.
- `git diff --check`.
- JSON parse for `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`.
- JSON parse for `audit.json`.
- No source-code changes check.
- Forbidden truthy product claims check.

Result: PASS for all checks.

## Product claims

No product/source-code/runtime/eval/training/model/checkpoint/locator claim was
opened by this repair. The next bounded product action remains
`QUALITY-02-EVAL-ONLY-RUNNER-IMPLEMENTATION-PACKET-NO-EVAL-EXECUTION` after
parent review/load/commit if accepted.
