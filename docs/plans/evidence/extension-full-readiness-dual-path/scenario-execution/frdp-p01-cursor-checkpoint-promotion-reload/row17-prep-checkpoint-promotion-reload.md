# Row17 prep: checkpoint promotion reload

Status: `ROW17_PREP_DONE_REVIEW_REQUIRED`.

Prepared at UTC: `2026-06-26T12:39:23Z`.

Scenario: Row17 `frdp-p01-cursor-checkpoint-promotion-reload`.

## Узкая цель Row17

Row17 должен проверить только core lifecycle proof для staged checkpoint promotion/reload: кандидат checkpoint/manifest читается по хэшу, копируется в временную promotion area, временный active locator/manifest внутри run folder указывает на этот кандидат, core reload path загружает этот кандидат, skill selection выбирает ожидаемый skill, затем rollback возвращает временный active locator к старому временному значению или удаляет его.

Row17 не доказывает product readiness, token savings, full semantic quality, training readiness, common promotion, real production locator update, host UI readiness, full joined pass или full extension readiness.

## Safe staged promotion proof vs real production promotion

Safe staged promotion proof означает: все mutation-like действия происходят только внутри Row17 run folder, например в `row17-temp-promotion-run-<UTC>/temp-promotion-area/`. Разрешенная мутация для execution work order: создать временную promotion area, скопировать туда candidate checkpoint/manifest, записать временный locator/active manifest только внутри этого run folder, выполнить reload через core path против временного locator/root, затем выполнить rollback временного locator/root.

Real production promotion означает: изменить настоящий source-of-truth locator, common/user runtime asset root, installed host extension config, глобальный active manifest или любой production/global pointer вне Row17 run folder. Это в Row17 не разрешено. Real active locator mutation is not allowed in this row. Если execution work order принят, Row17 должен мутировать только temporary locator внутри run folder.

## Source candidate

Primary source checkpoint/manifest:

- Checkpoint: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-learnability-training-continue\row14-3-staged-continue-training-export-20260626T103934Z\staged-export\skill-selection-checkpoint.json`
- Manifest: `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-learnability-training-continue\row14-3-staged-continue-training-export-20260626T103934Z\staged-export\skill-selection-manifest.json`

Direct hashes computed in this prep turn:

- Checkpoint SHA-256: `c0f3047e80610f3852928c04d2ecd8f89857d5e20726b2dc391892ff73ac9dbb`
- Manifest SHA-256: `4ae155818ac6351009626a37f22de56202ec1133c8a2728ab39be2f4dba90475`

Manifest details read in this prep turn:

- `machineId`: `skill-selection-machine`
- `domainTag`: `SmartSkillsDomain.SkillSelection`
- `version`: `1.0.0`
- `checkpointRef`: `skill-selection-checkpoint.json`
- `manifestRef`: `skill-selection-manifest.json`
- `checkpointSchemaVersion`: `1`
- `routeMode`: `candidate_position_scaffold_not_final_semantic_authority`
- `semanticQualityClaimed`: `false`
- `exportedAtUtc`: `2026-06-26T10:45:12.7186568+00:00`

Row16 independently confirmed this same candidate through lifecycle copy/readback/fresh-load evidence. The Row16 fresh-user copy hashes match the Row14 staged-export hashes, and Row16 fresh-load result selected expected skill `client-adaptive-learning-loop` with `trainedPairCount=10`. That Row16 evidence is source identification only for Row17; it is not Row17 execution.

## Execution substeps and PASS/FAIL gates

1. Create temporary promotion run folder.
   - PASS: create only under `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-checkpoint-promotion-reload\row17-temp-promotion-run-<UTC>\`.
   - FAIL: any write outside the Row17 scenario-execution folder, any code edit, any git action, any host UI action.

2. Create temporary promotion area and copy candidate files.
   - PASS: copy the Row14 staged checkpoint and manifest into `temp-promotion-area\candidate\skill-selection-checkpoint.json` and `temp-promotion-area\candidate\skill-selection-manifest.json`.
   - PASS: copied checkpoint hash equals `c0f3047e80610f3852928c04d2ecd8f89857d5e20726b2dc391892ff73ac9dbb`.
   - PASS: copied manifest hash equals `4ae155818ac6351009626a37f22de56202ec1133c8a2728ab39be2f4dba90475`.
   - FAIL: missing file, hash mismatch, source file mutated, or copied file path outside temp run folder.

3. Write temporary locator/active manifest inside run folder only.
   - PASS: create `temp-promotion-area\active\temporary-active-manifest.json` or equivalent temp locator inside the Row17 run folder.
   - PASS: the temp locator points to the copied candidate manifest/checkpoint by relative path and records both expected hashes.
   - PASS: the old active state being rolled back is also a temporary baseline inside this run folder, not a production/global locator.
   - FAIL: write to any production/global locator, extension config, common asset root, user asset root, or file outside Row17 run folder.

4. Reload through core path.
   - PASS: core lifecycle path loads the temporary active manifest/candidate through the same class of core load path evidenced by Row16 (`ImportAsync` and `LoadCheckpointAsync` in the core SkillSelection machine path), using only the temp run folder root/locator.
   - PASS: loaded machine metadata matches `skill-selection-machine`, `SmartSkillsDomain.SkillSelection`, version `1.0.0`.
   - PASS: selected skill equals expected `client-adaptive-learning-loop`.
   - FAIL: load error, fallback to production/global asset, selected skill mismatch, or hidden direct read from the original Row14 source instead of the temp promotion candidate.

5. Verify rollback and old active untouched.
   - PASS: rollback restores the temporary active locator to its previous temporary baseline or removes the temporary active locator, with before/after evidence recorded in the run folder.
   - PASS: candidate checkpoint/manifest hashes stay stable before load, after load, and after rollback.
   - PASS: no product/global locator mutation is claimed. If no explicit production locator path is authorized, the proof remains scoped to "all written files are inside Row17 run folder".
   - FAIL: production/global locator update, unresolved temp active pointer, hash drift, or any claim that real active locator was promoted.

6. Produce execution evidence.
   - PASS: execution packet records source paths, temp paths, hashes, selected skill, rollback evidence, non-claims, and changed files.
   - PASS: `row17_accepted=false`, `product_readiness_full_pass=false`, `full_joined_pass=false`.
   - FAIL: Row17 marked accepted by execution packet, product readiness claimed, or host UI readiness claimed.

## Files and hashes to compare

Required comparisons:

- Source Row14 checkpoint vs copied temp candidate checkpoint.
- Source Row14 manifest vs copied temp candidate manifest.
- Copied temp candidate checkpoint before reload vs after reload vs after rollback.
- Copied temp candidate manifest before reload vs after reload vs after rollback.
- Temporary active locator before staged switch vs after staged switch vs after rollback.
- Row16 fresh-user copy may be compared as supporting readback only:
  - `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-asset-lifecycle-roundtrip\row16-roundtrip-run-20260626T120434Z\fresh-user-asset-root\skill-selection-checkpoint.json`
  - `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-asset-lifecycle-roundtrip\row16-roundtrip-run-20260626T120434Z\fresh-user-asset-root\skill-selection-manifest.json`

Expected hashes:

- Checkpoint SHA-256: `c0f3047e80610f3852928c04d2ecd8f89857d5e20726b2dc391892ff73ac9dbb`
- Manifest SHA-256: `4ae155818ac6351009626a37f22de56202ec1133c8a2728ab39be2f4dba90475`

## Host UI path

This Row17 prep defines a core lifecycle proof only. It does not require a host UI path for this row. Cursor host reload/UI proof would be a separate later joined scenario or separate work order. Row17 local real-program status must remain not run unless a separate host UI work order explicitly authorizes it.

## Non-claims locked false

- Product readiness: false.
- Token savings: false.
- Full semantic quality: false.
- Training readiness: false.
- Common promotion: false.
- Real production locator update: false.
- Real active locator mutation: false.
- Host UI readiness: false.
- Full joined pass: false.
- Full extension readiness: false.
- Row17 accepted: false.

## Next execution work order if prep is accepted

Bounded sub-agent execution work order:

Prepare and execute Row17 `frdp-p01-cursor-checkpoint-promotion-reload` as a core-only staged promotion/reload proof. Allowed write scope: create only under `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-checkpoint-promotion-reload\row17-temp-promotion-run-<UTC>\`; write execution packet files under the Row17 scenario folder. Allowed source reads: Row14 staged export checkpoint/manifest listed in this prep packet; Row16 final/hash/fresh-load evidence listed in this prep packet; Row17 prep files. Forbidden: code edits, git, host UI actions, training/eval/checkpoint mutation outside temp run folder, production/global locator update, real active locator mutation, board/status acceptance edits. Required execution: copy candidate into temp promotion area, write temp active locator only inside run folder, reload through core path, verify selected skill `client-adaptive-learning-loop`, verify rollback of temp active locator, compare all required hashes, and record non-claims. Return `ROW17_EXECUTION_DONE_REVIEW_REQUIRED` or `ROW17_EXECUTION_NEEDS_PARENT_DECISION`. Do not mark Row17 accepted.
