# Plan repair report: distillation/source boundary

## Что исправлено

План `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` был
исправлен как plan-only artifact. Красная зона из attachment была формулировка
"local model learns from AI output/correction". Теперь план разделяет:

- external AI output as private diagnostic/evaluation evidence;
- user-approved corrections, local/project-owned evidence, XIIGen/human-authored
  failure patterns and licensed permissive/open sources as possible training
  material;
- common training only from universal orchestration/skill semantics with
  verified source/license/provenance/privacy;
- user-local adaptive learning under `.xiigen` or approved user root, with
  backup/export/import/share, restore, load-without-retrain, continue-training,
  fresh-load, held-out/ablation and rollback.

## Что добавлено в план

- Раздел "Что меняется в плане" with a human-readable Russian explanation.
- Таблица "Было -> должно быть".
- Mermaid-friendly flow for raw external output -> evidence ref -> DPO authoring
  gate -> common/user-local route.
- Exact checks future sub-agents must run for source/provenance/privacy,
  external AI evidence-only, reviewer/evaluator output, local lifecycle and
  no-memorization.
- Phase impact rows for runtime capture, `QUALITY-02`, `QUALITY-03`,
  `LEARN-01` through `LEARN-07`, and final readiness passes 7 and 8.
- Closed-claim list: full semantic quality, learnability, common readiness,
  user-local adaptive readiness, target promotion/locator update and product
  readiness remain closed until packet-backed evidence exists.

## Важная граница

External AI output, Claude/Cursor/Codex/GPT/Gemini answers, vendor/model traces,
reviewer/evaluator prose and adjudication text are not training targets by
default. They are evidence until explicit current approval plus provider terms,
license/source review, privacy/redaction and DPO-authoring gate allow a
transformed row.

## Changed files

- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`
- `artifacts/extension-runtime-token-quality-learnability/plan-repair-distillation-boundary/plan-repair-report.md`
- `artifacts/extension-runtime-token-quality-learnability/plan-repair-distillation-boundary/plan-repair-report.json`

## Validation

Validation status: PASS for this plan repair.

```text
git diff --check -> PASS
plan-repair-report.json parse -> PASS
target plan term verification -> PASS
source-code files changed by this task -> none observed
```

No product code, runtime, eval, training, model call, git add, commit or push is
part of this plan repair.

Worktree note: before this worker, the repository already had a dirty
`docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`
and an unrelated untracked artifact folder. They were not part of this repair.
