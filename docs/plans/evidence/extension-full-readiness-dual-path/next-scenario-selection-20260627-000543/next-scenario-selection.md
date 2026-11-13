# Выбор следующего безопасного сценария

claim_scope: только planning/orchestration selection; не product execution и не readiness proof
evidence_scope: board/action-plan reads and targeted Row24 independence check
count_claimed: total 28; scoped accepted 12; product-ready full pass 0; full joined pass 0; remaining 16
independent_evidence_count: 0 for new product evidence; this packet creates no product evidence
subagent_id_or_NONE: Meitner the 2nd / 019f05bf-ef3a-7230-b34d-2bbf7522a1f1
downgrade_needed: да; вывод ограничен выбором следующего prep/repair шага

## Status

Decision status: `NEEDS_PLAN_REPAIR`.

Это только planning/orchestration selection. Я не запускал product/runtime/UI/tests/eval/heldout, не менял код, не менял board/action plan, не делал commit или push.

## Что прочитано

- `product-readiness-scenario-board.md`
- `product-readiness-scenario-board.json`
- `product-readiness-action-plan.md`
- точечная JSON-проверка Row24 из scenario board, чтобы проверить, написано ли там явно, что Row24 independent

Доказательства поддерживают только узкий вывод: Row23 заблокирован как `BLOCKED_NO_APPROVED_INPUT_PATH`; Row24 не помечен в board как independent; Row27 имеет следующий шаг, который не зависит от Row23, но перед любым исполнением ему нужен scenario-prep repair.

## Done / Current / Remaining

Done: принято 12 scoped evidence/status элементов. Product-ready full pass: 0. Full joined pass: 0. Эти 12 элементов являются узкими status/evidence boundaries, а не доказательством готовности продукта.

Current: выбран следующий безопасный prep-target: Row27, `frdp-p01-codex-checkpoint-promotion-reload`. Человеческий смысл: подготовить, но пока не запускать, проверку того, можно ли для Codex немутирующе load/readback/rollback checkpoint candidate без promotion. Почему это безопасно сейчас: board говорит, что checkpoint/locator readback и direct-promotion rejection уже существуют только как guard-only evidence; Row27 не зависит от Row23; следующий шаг можно ограничить prep-only, где promotion, mutation, runtime execution, UI, tests, eval и heldout явно запрещены.

Remaining: остается 16 scenario rows без product-ready full pass. Из них Row23 заблокирован и ждет ровно один approved safe input/path. Row24 пропущен, потому что board не говорит явно, что он independent. Оставшиеся рабочие категории человеческим языком: Codex checkpoint/locator dry-run planning; Codex DPO/lifecycle planning; Claude route repair и guarded scenarios; Cursor product-workflow или оставшиеся guard scenarios; финальные product workflow rows только после усиления их sub-evidence.

## Почему не Row23 и не Row24

Row23 стоит как `BLOCKED_NO_APPROVED_INPUT_PATH`; approved concrete Row23 input/path candidate count = 0. Board говорит, что следующий Row23 input должен быть ровно одним approved safe Row23 input/path с provenance, approval quote, category, evidence refs, forbidden actions и claim locks.

Row24 не выбран, потому что текущий work order от Luba запрещает использовать Row24, если board явно не говорит, что Row24 independent. В Row24 board написано только: создать read-only learnability evaluation harness или запросить explicit mutating training authority with rollback. Там нет явной фразы, что Row24 independent from Row23.

## Минимальный repair

Минимальный безопасный repair: создать Row27 prep-only scenario packet, который превращает board next action для Row27 в человекочитаемую русскую prep-инструкцию с точными allowed/forbidden actions и expected evidence.

Это не product execution. Этот шаг должен подготовить следующее решение об execution. Если prep не может безопасно сформулировать немутирующий путь, он обязан вернуть точный blocker вместо любого запуска.

## Exact next sub-agent work order

Bounded sub-agent work order, maximum 10 minutes:

You are a bounded sub-agent in `C:\Projects\llm_mvp_core`.

Task type: planning/scenario-prep repair only, not product execution.

Allowed reads:
- `docs/plans/evidence/extension-full-readiness-dual-path/product-readiness-scenario-board.md`
- `docs/plans/evidence/extension-full-readiness-dual-path/product-readiness-scenario-board.json`
- `docs/plans/evidence/extension-full-readiness-dual-path/product-readiness-action-plan.md`
- Row27 referenced evidence links only, if needed for exact prep wording.

Allowed write scope:
- Create one new folder under `docs/plans/evidence/extension-full-readiness-dual-path/scenario-instructions/row27-checkpoint-prep-<timestamp>/`
- Write only `row27-checkpoint-prep.md` and `row27-checkpoint-prep.json`.

Required output:
- Human-readable Russian prep for Row27 `frdp-p01-codex-checkpoint-promotion-reload`.
- Explain phase meaning, step meaning, done/current/remaining.
- Name exact scenario purpose: non-mutating checkpoint candidate load/readback/rollback proof for Codex, with promotion forbidden.
- Include exact allowed future evidence types, exact forbidden actions, exact pass/fail/blocker statuses, and claim locks.
- Include a 10-minute execution split recommendation: prep now; execution only after prep review accepts it.

Forbidden:
- Do not run product/runtime/UI/tests/eval/heldout.
- Do not mutate checkpoint, locator, production config, model files, or training artifacts.
- Do not promote a checkpoint.
- Do not stage, commit, push, edit code, edit AGENTS/CLAUDE/skills, or edit board/action-plan files.
- Do not claim product readiness, checkpoint promotion, host readiness, formal joined pass, or full extension readiness.

Return:
- status `ROW27_PREP_READY` if prep is complete and execution remains locked behind review;
- status `ROW27_PREP_BLOCKED` if a non-mutating candidate-load/readback/rollback prep path cannot be stated safely;
- files changed;
- commands run;
- exact next work order.

## Commands run

- `Get-ChildItem -Path docs/plans/evidence/extension-full-readiness-dual-path -Force | Select-Object Name,LastWriteTime,Length`
- `Get-Content -Path docs/plans/evidence/extension-full-readiness-dual-path/product-readiness-scenario-board.md -Raw`
- `Get-Content -Path docs/plans/evidence/extension-full-readiness-dual-path/product-readiness-scenario-board.json -Raw`
- `Get-Content -Path docs/plans/evidence/extension-full-readiness-dual-path/product-readiness-action-plan.md -Raw`
- PowerShell JSON extraction for rows 1-10 and 24-28 from `product-readiness-scenario-board.json`
- PowerShell JSON extraction for board counts
- PowerShell JSON extraction for Row24
- `Get-Date -Format yyyyMMdd-HHmmss`
- `New-Item -ItemType Directory -Force -Path docs/plans/evidence/extension-full-readiness-dual-path/next-scenario-selection-20260627-000543`

## Files changed

- `docs/plans/evidence/extension-full-readiness-dual-path/next-scenario-selection-20260627-000543/next-scenario-selection.md`
- `docs/plans/evidence/extension-full-readiness-dual-path/next-scenario-selection-20260627-000543/next-scenario-selection.json`
