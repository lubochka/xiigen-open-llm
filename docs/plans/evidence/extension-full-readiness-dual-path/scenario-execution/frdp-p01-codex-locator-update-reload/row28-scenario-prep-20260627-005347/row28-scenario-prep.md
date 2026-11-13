# Row28 scenario prep: Codex locator update/reload

## Claim evidence guard

```text
claim_scope: Row28 scenario-prep only for future safe locator update/reload evidence
evidence_scope: product readiness board/action/status artifacts, latest next-scenario selection after Row27, and Row27 prep/execution evidence for locator/reload relationship
count_claimed: total scenarios 28; scoped accepted 13; product-ready full pass 0; remaining 15
independent_evidence_count: 13 command-level evidence steps before writing this prep
subagent_id_or_NONE: Arendt the 2nd / 019f05eb-d4d3-7752-8455-d1e49a1f4ef9
downgrade_needed: no - this artifact claims only prep, not execution or readiness
```

## Human status line

Done: 13/28 scoped evidence rows accepted. Current: Row28 `frdp-p01-codex-locator-update-reload` prep-only repair. Remaining: 15 scenario rows still open. Product-ready full pass: 0/28. Full joined pass: 0.

## Что Row28 пытается доказать простыми словами

Row28 должен подготовить безопасную будущую проверку того, что Codex locator update/reload path можно проверить без изменения production locator. Смысл будущего execution-пакета: найти source of truth для production locator, снять hash до начала, создать только dry-run/staging locator update object, прочитать его обратно через reload/readback path, создать rollback object, затем доказать, что hash production locator до и после не изменился.

Это не проверка готовности продукта. Это сценарная подготовка для будущего узкого evidence packet: staging/dry-run locator object -> reload readback -> rollback object -> production locator hash unchanged.

## Почему Row27 не хватает

Row27 `frdp-p01-codex-checkpoint-promotion-reload` принят только узко: он проверил temp/staging checkpoint reload and rollback path. Row27 использовал временный checkpoint и временный locator внутри Row27 execution folder и зафиксировал, что production artifacts не изменились.

Row27 не доказывает Row28, потому что Row28 спрашивает другое: готов ли locator update/reload path к безопасной проверке production locator source of truth через dry-run/staging update object, reload readback и rollback object. Row27 не выполнял production locator update, не доказывал production locator update readiness и не доказывал Codex host reload from locator.

## Что не разрешено

- Нельзя выполнять real production locator update.
- Нельзя менять production locator source of truth.
- Нельзя выполнять real checkpoint promotion.
- Нельзя мутировать production checkpoint.
- Нельзя заявлять product readiness или full extension readiness.
- Нельзя заявлять product-ready full pass или full joined pass.
- Нельзя заявлять semantic quality, semantic improvement или heldout pass.
- Нельзя заявлять training, learnability или Direct Preference Optimization readiness.
- Нельзя заявлять token savings.
- Нельзя запускать product/runtime/UI/tests/eval/heldout/training в рамках этого prep.
- Нельзя редактировать code, tools, AGENTS, CLAUDE, skills, scenario board, action plan или dashboard.
- Нельзя stage/commit/push.

## Safe two-path strategy

### Path A: automatic dry-run/staging locator path

Path A будущего execution-пакета должен быть полностью non-mutating по production locator.

Минимальный будущий порядок:

1. Создать новый execution folder только под `docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-codex-locator-update-reload/row28-execution-<timestamp>/`.
2. Найти и записать production locator source-of-truth candidate как read-only path. Если source of truth нельзя определить без догадки, вернуть `BLOCKED_LOCATOR_SOURCE_OF_TRUTH_NOT_FOUND`.
3. Снять SHA-256 hash production locator before и записать `production-locator-before-hash.json`.
4. Создать `staging-locator-update-object.json` только в Row28 execution folder. Объект должен описывать planned locator target/diff, но не писать в production locator.
5. Выполнить reload/readback только against staging/dry-run locator object. Если единственный доступный route требует production mutation, вернуть `BLOCKED_NO_SAFE_DRY_RUN_LOCATOR_ROUTE`.
6. Записать `staging-locator-readback.json` и `reload-readback.json` с observed locator id/path/hash/diff.
7. Создать `rollback-object.json` только в execution folder и записать `rollback-readback.json`.
8. Снова снять SHA-256 hash production locator after и записать `production-locator-after-hash.json`.
9. Сравнить before/after production locator hash в `production-locator-unchanged-proof.json`; PASS_SCOPED возможен только если hash совпадает.
10. Записать `row28-execution-report.md` и `row28-execution-report.json` на русском с явными non-claims.

Path A может доказать только scoped dry-run locator update/reload/rollback evidence. Он не доказывает real locator update, host UI readiness, product readiness, semantic quality, training или token savings.

### Path B: local UI/host path only if exact route exists

Path B разрешен только если до старта execution already exists exact Codex/Cursor/Claude host route for this same Row28 locator scenario. Exact route means: app/window target, signed-in/session preflight if needed, exact clicks/typing/expected visible state, exact screenshot/video moments, and no secrets captured.

Если exact host route не существует, future executor must record UI blocker, not fake pass. Expected blocker files:

- `row28-ui-blocker.md`
- `row28-ui-blocker.json`

UI blocker должен сказать `host_ui_route_found=false`, `ui_execution_ran=false`, `host_ui_readiness_claim=false`, `formal_joined_pass=false`, and exact missing route/control/session reason.

If exact host route exists, Path B must use the same staging/dry-run locator object as Path A and compare visible host result with Path A readback. It still must not mutate production locator and must keep product readiness claims false.

## Exact criteria

### PASS_SCOPED for future Row28 execution

- Production locator source-of-truth candidate is identified as read-only and recorded.
- `production-locator-before-hash.json` and `production-locator-after-hash.json` exist and parse.
- Before/after production locator hash values match exactly.
- `staging-locator-update-object.json` exists only inside Row28 execution folder.
- Reload/readback used only staging/dry-run locator object, not production mutation.
- `reload-readback.json` confirms the staging locator object was read back.
- `rollback-object.json` and `rollback-readback.json` exist and stay inside Row28 execution folder.
- `production-locator-unchanged-proof.json` records `production_locator_hash_unchanged=true`.
- UI/host path either has exact route evidence for the same staging scenario or records a blocker instead of fake pass.
- Claim locks remain false: `product_readiness_claim=false`, `formal_joined_pass=false`, `host_ui_readiness_claim=false`, `locator_update_claim=false`, `real_production_locator_update=false`, `checkpoint_promotion_claim=false`, `semantic_quality_claim=false`, `training_claim=false`, `token_savings_claim=false`.

### NEEDS_FIX for future Row28 execution

- JSON evidence is missing or does not parse.
- Report is not human-readable Russian.
- Locator source-of-truth path is ambiguous or guessed.
- Staging locator object could write to production locator.
- Before/after production locator hash proof is missing.
- Reload/readback lacks correlation to the staging locator object.
- Rollback object/readback is missing.
- UI path claims pass without exact host route, exact clicks/typing/expected visible state, or evidence capture.
- Any wording implies product readiness, real locator update, checkpoint promotion, semantic quality, training, token savings, host readiness, or full joined pass.

### BLOCKED for future Row28 execution

- `BLOCKED_LOCATOR_SOURCE_OF_TRUTH_NOT_FOUND`: production locator source of truth cannot be identified safely.
- `BLOCKED_NO_SAFE_DRY_RUN_LOCATOR_ROUTE`: available route requires production locator mutation.
- `BLOCKED_PRODUCTION_HASH_UNVERIFIABLE`: before/after production locator hash cannot be captured.
- `BLOCKED_ROLLBACK_OBJECT_UNSAFE_OR_MISSING`: rollback cannot be represented safely in the execution folder.
- `BLOCKED_HOST_UI_ROUTE_NOT_FOUND`: exact Codex/Cursor/Claude host route does not exist; record UI blocker, not pass.
- `BLOCKED_AUTH_OR_SECRET_RISK`: host/session proof would expose secrets or cannot be verified safely.

## Exact next execution sub-agent work order <=10 minutes

Role: bounded Row28 execution sub-agent.

Max active time: 10 minutes.

Current authority quote: "Next selected safe target: Row28 frdp-p01-codex-locator-update-reload, but only scenario-prep repair first." Future execution may proceed after Row28 prep review PASS; no fresh Luba approval needed for scoped non-mutating dry-run/staging execution; fresh Luba approval is required only if execution would mutate production locator/checkpoint, promote a checkpoint, train/eval models, or make product-readiness claims.

Task: after Row28 prep review PASS, execute Row28 only as a scoped non-mutating dry-run/staging locator update/reload proof. Do not mutate production locator. Do not claim product readiness.

Allowed reads:

- This Row28 prep folder.
- `docs/plans/evidence/extension-full-readiness-dual-path/product-readiness-scenario-board.md`
- `docs/plans/evidence/extension-full-readiness-dual-path/product-readiness-scenario-board.json`
- `docs/plans/evidence/extension-full-readiness-dual-path/phase-14-user-visible-status-dashboard.md`
- Row27 execution folder only as evidence that Row27 was temp/staging checkpoint reload and rollback, not locator update readiness.

Allowed write scope:

- `docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-codex-locator-update-reload/row28-execution-<timestamp>/`

Required future evidence files:

- `preflight.json`
- `production-locator-before-hash.json`
- `staging-locator-update-object.json`
- `staging-locator-readback.json`
- `reload-readback.json`
- `rollback-object.json`
- `rollback-readback.json`
- `production-locator-after-hash.json`
- `production-locator-unchanged-proof.json`
- `row28-execution-report.md`
- `row28-execution-report.json`
- `row28-ui-blocker.md` and `row28-ui-blocker.json` only if no exact host UI route exists

Required future validation:

- JSON parses for every generated JSON file.
- `git diff --check -- <row28-execution-folder>` passes.
- Report returns `PASS_SCOPED`, `NEEDS_FIX`, or `BLOCKED` with commands run, files changed, and explicit non-claims.

Forbidden in future execution unless a separate later work order explicitly changes scope:

- real production locator update
- production locator mutation
- real checkpoint promotion
- production checkpoint mutation
- product/runtime/UI/tests/eval/heldout/training outside the exact allowed Row28 evidence path
- code/tools/AGENTS/CLAUDE/skills/board/action-plan/dashboard edits
- stage/commit/push

Expected future report line:

```text
Row28 execution status: done=<dry-run/staging locator evidence files created>, current=<PASS_SCOPED|NEEDS_FIX|BLOCKED reason>, remaining=<host/UI route or reviewer acceptance still needed>; product_readiness_claim=false; locator_update_claim=false; production_locator_hash_unchanged=<true|false|not_proven>.
```

## Non-claims for this prep

I did not run product/runtime/UI/tests/eval/heldout/training. I did not update production locator. I did not promote a checkpoint. I did not prove locator update readiness. I did not prove host/UI readiness. I did not prove product readiness. This is scenario prep only.
