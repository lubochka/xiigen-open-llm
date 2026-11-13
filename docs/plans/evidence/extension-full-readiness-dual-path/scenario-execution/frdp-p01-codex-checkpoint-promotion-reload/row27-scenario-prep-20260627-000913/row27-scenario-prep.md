# Row27 scenario prep: Codex checkpoint promotion/reload

## Человеческий статус

Готово: 12 сценариев приняты только как узкое evidence/status, 0 сценариев доказали product readiness. Сейчас: Row27 prep-only для безопасной проверки checkpoint promotion/reload. Осталось: 16 сценариев открыты; Row27 execution еще не запускался.

## Что Row27 пытается доказать

Row27 `frdp-p01-codex-checkpoint-promotion-reload` должен доказать только узкий безопасный шаг: Codex-side reload может обнаружить намеренно подготовленный staged/temp checkpoint artifact, сверить его hash/readback, пройти compatibility/load check, показать наличие rollback path и не трогать настоящий production checkpoint или production locator.

Текущий board status для Row27: автоматическая часть принята только как guard-only evidence: checkpoint/locator readback и direct-promotion rejection tests passed. Локальный/реальный Codex reload from candidate checkpoint не доказан. Joined pass и product readiness не открыты.

## Что не разрешено

- Нельзя выполнять real checkpoint promotion.
- Нельзя обновлять production locator или настоящий runtime locator source of truth.
- Нельзя мутировать настоящий production checkpoint.
- Нельзя заявлять product-ready, product readiness, full joined pass или Codex readiness.
- Нельзя заявлять semantic quality, heldout pass, token savings, training success, learnability readiness или checkpoint promotion.
- Нельзя запускать training, eval, heldout, UI automation или runtime/product workflow за пределами отдельного будущего work order.
- Нельзя stage/commit/push.

## Безопасная стратегия: два пути

### Path A: automatic staging/temp only

Цель Path A: доказать reload detection на временном staged artifact без production mutation.

Executor должен использовать только temp/staging run folder внутри Row27 evidence root. Если требуется locator, он должен быть temp locator file inside the run folder, not production locator. Если требуется checkpoint, он должен быть temp checkpoint artifact copied/generated inside the run folder, not promoted into production.

Минимальные шаги Path A:

1. Создать новый execution run folder under `docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-codex-checkpoint-promotion-reload/row27-execution-<timestamp>/`.
2. Записать `preflight.json` с абсолютными путями temp checkpoint и temp locator, и с флагами `production_checkpoint_mutated=false`, `production_locator_mutated=false`, `promotion_attempted=false`.
3. Подготовить temp checkpoint artifact только в run folder. Зафиксировать hash/readback в `checkpoint-readback.json`.
4. Подготовить temp locator, который указывает только на temp checkpoint artifact. Зафиксировать diff/readback в `temp-locator-readback.json`.
5. Запустить только non-mutating reload/load check against temp locator/temp checkpoint. Если безопасной команды нет или она требует production locator/checkpoint, остановиться как BLOCKED.
6. Записать `reload-result.json` с observed checkpoint hash, selected artifact path, reload/load status, and no production mutation flags.
7. Записать `rollback-proof.json`: temp locator/checkpoint cleanup or restore step exists and was verified; production locator/checkpoint were not touched.
8. Записать `row27-execution-report.md` на русском: что проверено, что не проверено, какие claims закрыты.

Path A может дать только узкое evidence/status: staged/temp reload detection плюс rollback path. Path A сам по себе не доказывает host UI readiness, product readiness, semantic quality, training или real promotion.

### Path B: local/UI/host only if exact route exists

Цель Path B: если уже существует точный безопасный Codex host/UI route для этого же staged/temp checkpoint scenario, показать host-visible reload behavior без production mutation.

Path B разрешен только если executor до старта может назвать exact host UI route: app/window target, session/auth preflight, exact clicks/typing/expected visible state, screenshots to capture, and no secrets captured. Если такого route нет, executor обязан записать UI blocker, not fake pass.

Минимальные UI/blocker outcomes:

- Если route существует: run only the same temp checkpoint scenario, capture screenshots/video/readback, compare with Path A hash/result, and keep `product_readiness_claim=false`.
- Если route не существует: записать `row27-ui-blocker.json` и `row27-ui-blocker.md` with `host_ui_route_found=false`, `ui_execution_ran=false`, `blocked_reason=<exact missing route/control/session>`.

## PASS / NEEDS_FIX / BLOCKED критерии

PASS для prep-only:

- Этот prep file и JSON существуют в новой Row27 prep folder.
- JSON parses.
- Prep ясно запрещает real promotion, production locator update, product-ready claim, semantic quality claim и training claim.
- Prep задает staging/temp-only Path A и UI-only-if-exact-route Path B.
- Prep содержит exact next execution sub-agent work order до 10 минут.

PASS для будущего Row27 execution:

- Path A использовал только staging/temp locator и temp checkpoint inside Row27 execution folder.
- Reload/load check обнаружил intended staged checkpoint artifact and recorded matching hash/readback.
- Rollback path exists and final verification proves production locator/checkpoint were not touched.
- Claim locks remain false: product readiness, host readiness, semantic quality, training, real promotion, production locator update, checkpoint mutation/promotion.
- If Path B ran, it used the same staged/temp scenario and exact host UI route; if route did not exist, the result is an explicit UI blocker.

NEEDS_FIX для будущего Row27 execution:

- Evidence files are missing, JSON does not parse, or report is not human-readable Russian.
- Temp/staging paths are ambiguous or could point to production.
- Reload result lacks hash/readback correlation.
- Rollback proof is missing or final no-production-mutation verification is missing.
- UI path claims pass without exact host route, exact clicks/typing/expected visible state, or screenshot/video/readback evidence.
- Any report wording implies product readiness, semantic quality, training success, real promotion, production locator update, or checkpoint promotion without explicit authority and evidence.

BLOCKED для будущего Row27 execution:

- No safe non-mutating temp checkpoint reload/load command or callable route exists.
- Available command requires production locator/checkpoint mutation.
- Exact Codex host/UI route is not found; record UI blocker instead of pass.
- Session/auth/preflight cannot be verified without secrets exposure.
- Required artifact path or temp rollback path cannot be created safely.

## Exact next execution sub-agent work order, max 10 minutes

Role: bounded Row27 execution sub-agent.

Authority: execute Row27 prep only as written here; no real promotion, no production locator update, no production checkpoint mutation, no product readiness claim.

Allowed write scope:

`docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-codex-checkpoint-promotion-reload/row27-execution-<timestamp>/`

Allowed reads:

- This prep folder.
- `docs/plans/evidence/extension-full-readiness-dual-path/product-readiness-scenario-board.md`
- `docs/plans/evidence/extension-full-readiness-dual-path/product-readiness-scenario-board.json`
- Existing Row27 scenario folder under `scenario-execution/frdp-p01-codex-checkpoint-promotion-reload/`

Required execution steps:

1. Create execution run folder and write `preflight.json` with temp paths and claim locks.
2. Find or create only a temp/staged checkpoint artifact inside the execution folder. If no safe staged artifact path is available within 10 minutes, return BLOCKED.
3. Find or create only a temp locator inside the execution folder pointing to the temp checkpoint. If this cannot be done without touching production locator, return BLOCKED.
4. Run only a non-mutating reload/load check against the temp locator/temp checkpoint. If the only available route mutates production, return BLOCKED.
5. Write `checkpoint-readback.json`, `temp-locator-readback.json`, `reload-result.json`, `rollback-proof.json`, and `row27-execution-report.md`.
6. For Path B, run UI only if exact Codex host route exists before input. Otherwise write UI blocker files and do not fake a pass.
7. Validate JSON files parse and run `git diff --check -- <execution folder>`.
8. Return PASS/NEEDS_FIX/BLOCKED with commands run, files changed, and explicit non-claims.

Forbidden:

Real checkpoint promotion, production locator update, production checkpoint mutation, training, eval, heldout, product workflow run, broad code edits, board/action-plan edits, AGENTS/CLAUDE/skills edits, stage, commit, push.

Expected report line:

`Row27 execution status: done=<temp evidence files created>, current=<PASS|NEEDS_FIX|BLOCKED reason>, remaining=<host/UI route or reviewer acceptance still needed>; product_readiness_claim=false; checkpoint_promotion_claimed=false.`

## Non-claims for this prep

I did not run product/runtime/UI/tests/eval/heldout/training. I did not verify checkpoint reload. I did not promote a checkpoint. I did not update a locator. This is scenario prep only.
