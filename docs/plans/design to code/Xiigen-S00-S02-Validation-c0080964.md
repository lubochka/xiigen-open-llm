# XIIGEN S00–S02 validation

**Reviewed:** 13 September 2026  
**Repository:** `lubochka/llm_mvp_core`  
**Head:** `c0080964e009172522ac945605c05d9b225c6408`  
**Main:** `e6eb902473e8907fcbfeb535e7cf99d33cba2618`  
**Comparison merge base:** `3de8810b4dea800d0639d368d74d68e03ea27e75`  
**Overall verdict:** **NEEDS_CHANGES for complete S00–S02 acceptance.**

## Scope and verification boundary

This is a read-only review of the current GitHub source, the committed slice briefs, the actual comparison, recorded test results and their provenance. It is not an application test rerun: no .NET SDK is installed in this review environment. No project files, permissions, Git history or remote state were changed. The regression scenarios below are proposed checks derived from source traces, not newly executed failures.

The original S00/S01 work is already in the common history represented by the comparison's merge base. Therefore the current `main...codex/dpo-implementation` diff alone is not a complete history of S00–S02. This review also inspected the current S00 reconciliation, S01 tests and workflow records.

The relevant acceptance authorities are the committed Luna `slices/S02.1.md`, `slices/S02-FINAL.md` and `.kiro/xiigen-slices/work/active.md`. The full S02 gate requires matched weights/state identity, required pre-state preserved before mutation, supported reopen/import, controlled next-update equivalence, no duplicate applied-pair training, fail-closed bad-state handling, accepted-parent retention, fixture cleanup and separate reviews. Optimizer-only acceptance is explicitly distinct from all-applicable-state acceptance.

## Per-slice disposition

| Stage | Supported by inspected material | Disposition |
|---|---|---|
| S00 | Reconciliation exists; source refs, skill routes, test project, workspace scope and the original S01 coverage gap are documented. Historical loading/delegation limitations were disclosed. | Preparation delivered; do not repeat it. Current machine-readable state needs reconciliation with later work. |
| S01 | Coordinator tests cover applied-but-unpublished same-run recovery, no retraining, persisted baseline reuse, missing/corrupt weights, changed lesson identity and retained parent on non-improvement. The formerly weak forced-trainer-failure test now observes its injected failure directly. | Core behavior implemented and supported by recorded runs. Do not rewrite it. Do not treat an earlier run as final-head verification. |
| S02.1 | Portable AdamW moments and counter, stable keys, serialization and all-or-nothing dictionary replacement exist. Six optimizer tests are in the recorded combined gate. | Implemented, but the explicit same-element-count/different-shape condition is not enforced by the portable import. |
| S02.2 | Optimizer-state file write/readback, expected-hash validation and no-overwrite discipline exist; the production callback actually uses them. Six state-file tests are in the recorded gate. | Real implementation, not a placeholder. Its existence alone does not prove complete bundle recovery. |
| S02.3 | Trainer state flows through the logical unit and executor into optimizer/RNG files and the owning attempt manifest. Coordinator restore and same-run evaluation paths are present. | Integration exists, but complete-bundle recovery still has the two high-priority identity/validation gaps below. |
| S02.RNG.1 | Deterministic adapter RNG snapshots, restoration, controlled numerical tests and a later atomic restore/cleanup repair exist. | Substantial implementation. The final post-repair test receipt cited for acceptance is missing from the committed result directory. |
| S02 final | A completion note exists, but requirements and final evidence are not fully closed. | **Not accepted by this review.** |

## Finding 1 — Required RNG bytes are not checked on same-run recovery

**Priority: high. Source-confirmed missing validation; proposed failure scenario not executed here.**

Sources:
- `26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.cs`: `TrainAndPublishAsync`, `ResolveOptimizerAndRandomStateBeforePrepareStart`, `ResumeQualityDecisionFromExistingAttemptAsync`, `BuildRecoveredTrainingEvidence`.
- `14.ContinuousLearning/Training/CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest.cs`: `ValidateManifest`, `ValidateRandomManifestFields`, `MatchesRuntime`.
- `26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedModelCheckpointStartService.cs`: `LoadTrainingAttemptCheckpoint`.

For a distinct source attempt, `ResolveOptimizerAndRandomStateBeforePrepareStart` does reopen RNG bytes with the recorded hash, length, step and topology/configuration identities. That protection should be retained.

However, when `OptimizerStateSourceAttempt` is null, the resolver returns caller state without opening the current attempt's RNG artifact. This is the ordinary same-run recovery situation. The later resume method opens baseline, optimizer manifest, optimizer bytes, trial weights and metrics, but does not call `RepositoryOwnedDpoRandomStateFile.OpenAndValidate` for that attempt. Manifest validation checks the RNG metadata's internal structure, not the referenced file. The trial checkpoint loader does not fill that gap.

Consequently, the inspected path contains no rejection of a missing/corrupt required RNG file before a same-run trial is evaluated and potentially promoted. The issue is bundle completeness and preservation of a usable continuation state; it is not an instruction to retrain the already-applied pair. Recovered training evidence also omits the RNG identity members that the fresh path records.

**Required regression:** create a valid dropout-enabled applied-but-unpublished attempt through the coordinator. Remove or corrupt only its required RNG file while preserving weights, optimizer file, manifest and metrics. Invoke the same TrainingRunId through normal recovery. Assert rejection before trial evaluation/publication, zero trainer invocations and unchanged accepted parent. Include a valid-control case so the negative cannot pass merely because staging failed.

**Repair boundary:** validate the required RNG artifact and runtime compatibility on same-run recovery and include its identity in recovered evidence. Keep evaluation-only semantics and legacy acceptance limited to a runtime-proven no-RNG topology.

## Finding 2 — Post-step weights are not cryptographically bound to the saved optimizer/RNG bundle

**Priority: high. Source-confirmed identity gap; substitution scenario not executed here.**

Sources:
- `14.ContinuousLearning/Training/CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest.cs`: manifest record and `BuildManifest`.
- `26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedModelCheckpointStartService.cs`: `LoadTrainingAttemptCheckpoint`.
- Coordinator: `ReopenTrialCheckpointFromAttempt`, `ValidateOptimizerStateRuntimeIdentity`, `ResolveTrainingAttemptMetricsPath`.

The optimizer manifest stores the **accepted parent's** weights hash/length, plus optimizer and RNG artifact identities. It does not store a trusted **post-step trial weights** hash/length that binds those state files to the numerical weights produced by the same update.

The trial loader hashes whatever file currently occupies the expected path, loads it, serializes the loaded model and compares that serialization with the hash of the same current file. This is useful read/load consistency checking. It does not establish that the file is the one originally saved together with the optimizer and RNG state.

A different, valid same-shape checkpoint can satisfy the filename, format, model-shape and load/reserialize checks. Matching parameter names or model dimensions does not bind optimizer moments to the actual numerical weights. The separate next-lesson source check against the accepted parent's hash is valuable, but it does not supply the missing original post-step identity for same-run recovery. Metrics are checked for nonempty existence rather than serving as a validated digest of the bundle.

**Required regression:** stage a legitimate attempt, then replace only its trial file with a valid, finite, same-shape checkpoint produced by the normal serializer. Leave the old optimizer/RNG/manifest intact. Recovery must reject the mixed bundle before evaluation, import or promotion and retain the parent. Testing only truncated/random bytes does not exercise this case.

**Repair boundary:** persist the trial weights relative path, SHA-256 and length in the same owning immutable bundle contract as the optimizer/RNG identities, and verify them when reopening. Preserve the existing transaction/commit-marker and no-overwrite guarantees.

## Finding 3 — The named next-update equivalence test does not reopen its input state from disk

**Priority: medium; acceptance-evidence gap.**

Source: `Tests/DpoTrainerOptimizerStateHandoffTests.cs`, especially `DropoutContinuationMatchesUninterruptedNextUpdateForPolicyOptimizerAndRng`.

This test is a useful controlled numerical comparison. It creates new model/trainer objects and compares their next update with an uninterrupted control. But its continuation inputs are retained in-memory values: `firstOptimizer`, `firstRandom` and the checkpoint byte array. It calls `TryRestoreOptimizerState(firstOptimizer)` and `RestoreRandomState(firstRandom, ...)` directly. The RNG file-writing helper is used for comparing outputs after the update, not to reopen the continuation input.

A different test exercises optimizer-file round-trip, but does not also load the matching policy state and execute the next update. These separate tests do not by themselves establish the combined saved-bundle -> reopened inputs -> next-update invariant.

**Required regression:** through the existing owning contracts, persist one legitimate matching weights/optimizer/RNG bundle, reconstruct new dependencies, reopen and validate all continuation inputs from those files, then execute the next deterministic update and compare weights, moments, counter and RNG cursor to the uninterrupted control. This can remain an explicitly in-process reconstruction; a separate operating-system process is not required merely to close this specific gap.

## Finding 4 — Portable optimizer import checks element count rather than tensor dimensions

**Priority: medium; explicit S02.1 criterion not closed.**

Sources:
- `04.TrainingLoop/Optimizers/AdamWPortableState.cs`: `AdamWPortableParameterState`.
- `04.TrainingLoop/Optimizers/AdamW.cs`: `TryImportPortableState`.
- `.kiro/xiigen-slices/luna/slices/S02.1.md`: shape requirement and edge cases.

The portable entry stores its key and the two moment arrays, not the original dimensions. Import validates the target parameter's `Data.Length`. A matching key whose tensor changes from `[2,3]` to `[3,2]` therefore has the same accepted element count. The committed brief explicitly says equal flattened length is not proof of equal tensor shape and calls for that negative case.

The owning production manifest also validates ModelConfig and optimizer settings, which provides additional protection for the standard model path. This finding is not a claim that every production model shape change bypasses validation. It is the narrower fact that the portable API cannot enforce the promised actual-shape compatibility by itself.

**Required correction:** enforce a trusted shape identity at the responsible contract without breaking existing format rules silently; add an equal-length/different-dimensions rejection test asserting no partial mutation. If a narrower component contract is deliberately retained, reconcile that choice explicitly with the stronger brief rather than claiming the original criterion passed.

## Finding 5 — Final post-repair verification is asserted, but its cited raw evidence is absent

**Priority: blocking for final acceptance evidence; not evidence that the claimed run failed or never occurred.**

The latest acceptance note at `c0080964...` claims that candidate `05f98eb7...` passed focused 9/9 and impacted 101/101 and cites:

`.kiro/xiigen-slices/luna/work/results/20260911T205216-19de33bd497f/status.json`

That path returned 404 at the reviewed head. The complete non-truncated tree of the Luna results directory contains only `20260911T093316-4e541bdb1d8b` and `20260911T181522-5e9d8f51b65c`. The inspected older work-results tree contains additional earlier logs/TRX but not the cited final receipt. GitHub Actions queries returned zero runs associated with both `05f98eb7...` and `c0080964...`.

The actually committed `20260911T181522-5e9d8f51b65c` receipt names head `3a22886fcee0de0bbdf3a32951df825559dc35b4`, focused 9/9 and impacted 98/98. Its actual impacted TRX confirms 98 executed, 98 passed and zero failed/not-executed/aborted. The seven classes are:

| Class | Recorded passing cases |
|---|---:|
| AdamWOptimizerStateRecoveryTests | 6 |
| RepositoryOwnedDpoOptimizerStateFileTests | 6 |
| DpoTrainerOptimizerStateHandoffTests | 9 |
| LoraRandomSeedTests | 38 |
| CodeDocumentationServerOwnedDpoTrainingExecutorTests | 16 |
| CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests | 17 |
| CodeDocumentationRepositoryOwnedModelCheckpointStartServiceTests | 6 |
| **Total** | **98** |

Do not add the focused nine to the combined 98 as unique coverage: they overlap. The earlier receipt correctly says test gates passed and review is required; its acceptance flags are false. These runner flags are not themselves a later review rejection.

Commit `05f98eb7...` then changes actual RNG restore/cleanup code and adds three regression cases. It is not a documentation-only commit, so the earlier 98-pass evidence does not verify the final source candidate. The acceptance note identifies reviewer Parfit and an agent id; that is a review claim, not the full independently inspectable return packet.

**Required closure:** provide the original final raw receipt, logs, TRX and separate review packet with a check that they match the candidate. If unavailable or mismatched, rerun the final required gates and review the actual results. Preserve older evidence as historical. Do not reconstruct a receipt from prose or mark an earlier run current by changing its SHA.

## Finding 6 — Temporary fixture directories remain unowned by cleanup

**Priority: low, but fixture cleanup is in the acceptance checklist.**

In `Tests/DpoTrainerOptimizerStateHandoffTests.cs`, the round-trip test creates a restart directory inline while its finally block deletes only the first checkpoint directory and attempt root. The incompatible-shape test similarly creates a `xiigen-dpo-ckpt-wide-` directory without retaining its path for cleanup.

**Required correction:** retain and clean each test-created directory in finally using the existing fixture conventions. Do not broaden cleanup to unrelated directories.

## Finding 7 — Current workflow state contradicts the later acceptance note

**Priority: medium for reliable autonomous continuation.**

`.kiro/xiigen-slices/work/state.json` still has old top-level S01 blocked status and an old reference head, contains S02.2 with S02.3 as the next action, and ends with statements denying S01 acceptance and S02 implementation. The newer `active.md` records S02.RNG.1 accepted and a subsequent action. These can coexist as historical records only when a current state is unambiguously identified; the inspected ledger does not do that consistently.

The final brief references `work/ledger.json` and a state-family inventory. The inspected current work locations do not provide the requested final ledger/inventory as distinct reconciled artifacts. The active brief discusses the RNG family and other guarantees, but that is not a full row-by-row closure record for every applicable mutable family.

**Required correction:** preserve historical states while recording one authoritative current per-slice status, real evidence references, applicable-family coverage and reviewed source-backed N/A rows. Do not fabricate missing review or execution history. This is a bounded continuity repair, not a reason to redo S00 or rewrite the skills library.

## Recommended finishing order

1. Repair same-run required-RNG validation and bind the post-step weights identity to the saved bundle, with the two discriminating negative regressions.
2. Add the disk-reopened next-update comparison and close the portable-shape requirement at its correct ownership boundary.
3. Clean the two test-owned directory leaks and reconcile the current inventory/ledger without deleting historical evidence.
4. Run the focused/impacted/final gates on the final candidate, preserve actual raw results, and obtain a separate evidence-backed review. Supply an existing final run only if its original artifacts can be verified against that candidate.

This is targeted completion work on a substantial implementation. It is not a recommendation to restart the slice sequence or rebuild the recovery architecture. No S03/evaluator redesign, real domain training or publication operation was performed by this review.

## Reproducible source index

All following paths are relative to `lubochka/llm_mvp_core` at `c0080964e009172522ac945605c05d9b225c6408`:

- `.kiro/xiigen-slices/work/S00-reconciliation.md`
- `.kiro/xiigen-slices/work/state.json`
- `.kiro/xiigen-slices/work/active.md`
- `.kiro/xiigen-slices/luna/slices/S02.1.md`
- `.kiro/xiigen-slices/luna/slices/S02-FINAL.md`
- `.kiro/xiigen-slices/luna/work/results/20260911T181522-5e9d8f51b65c/status.json`
- `.kiro/xiigen-slices/luna/work/results/20260911T181522-5e9d8f51b65c/impacted.trx`
- `04.TrainingLoop/Optimizers/AdamW.cs`
- `04.TrainingLoop/Optimizers/AdamWPortableState.cs`
- `14.ContinuousLearning/Training/DpoTrainingExecution.cs`
- `14.ContinuousLearning/Training/CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest.cs`
- `26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.cs`
- `26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedModelCheckpointStartService.cs`
- `Tests/DpoTrainerOptimizerStateHandoffTests.cs`
- `Tests/CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests.cs`

Additional historical/source anchors: merge base `3de8810b...`; earlier final-gate candidate `3a22886f...`; RNG atomicity repair `05f98eb7...`; latest acceptance-note commit `c0080964...`.
