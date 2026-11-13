# Legacy `DpoImplementation` Branch Reconciliation

## Current status and blocking gate

```text
artifact_state: ACTIVE_RECONCILIATION_OWNER
gate_id: G-LEGACY-BRANCH-RECONCILIATION
gate_state: BLOCKING_14C_PUBLICATION_AND_RESUME
current_published_branch: refs/heads/codex/dpo-implementation
current_published_tip: e7c6af49f2a18ee96145e431433d07b994d15cba
legacy_evidence_branch: refs/heads/DpoImplementation
legacy_evidence_tip: 233b1f0ee6749fb59e86a719ed61f113f0779c72
slice_14c_state: PAUSED_UNCOMMITTED_NEEDS_FIX
canonical_sequence: CODE_FIRST -> TP1_LOCAL eval-only -> TP2_HOSTED eval-only -> G_DPO_FINAL -> DPO/training
teacher_stage_state: TP1_LOCAL_NOT_STARTED; TP2_HOSTED_NOT_STARTED
dpo_training_state: NOT_STARTED
```

This file is the single owner of the file-level reconciliation between the legacy remote branch `DpoImplementation` and the current publication branch `codex/dpo-implementation`. It does not authorize a merge, cherry-pick, port, DPO-pair creation, teacher call, checkpoint operation or training run.

`G-LEGACY-BRANCH-RECONCILIATION` must be independently accepted before slice 14C may resume or be published. Classification may be completed during `CODE_FIRST`; every item classified `PORT_AT_G_DPO_FINAL` remains implementation-blocked until `G_DPO_FINAL` passes. The accepted current branch remains authoritative for current DynamicGraph work.

## Verified branch topology

| Evidence | Verified result | Meaning |
|---|---|---|
| Direct remote legacy ref | `refs/heads/DpoImplementation` = `233b1f0ee6749fb59e86a719ed61f113f0779c72` | Legacy evidence source; not the current publication target. |
| Direct remote current ref | `refs/heads/codex/dpo-implementation` = `e7c6af49f2a18ee96145e431433d07b994d15cba` | Accepted published current tip through slice 14B. |
| Merge base | `6fe15e7b43a97936e14bddaa1fb19ff467c1815d` | The histories diverged after this commit. |
| Symmetric difference | `2171` legacy-only / `2313` current-only commits | This is not a rename or a fast-forward relationship. |
| Ancestry | Neither tip is an ancestor of the other | A wholesale merge or branch substitution is forbidden. |
| Current code-first baseline | `504bcba93f7bcd4db1c93fc094eea3e8133d310d` is not a descendant of the legacy tip; neither commit is an ancestor of the other | Slices 07-14B were built from a different baseline and did not automatically inherit legacy-only translation DPO code. |

Reproduction commands are read-only:

```powershell
git ls-remote --heads origin DpoImplementation codex/dpo-implementation
git merge-base 233b1f0ee6749fb59e86a719ed61f113f0779c72 e7c6af49f2a18ee96145e431433d07b994d15cba
git rev-list --left-right --count 233b1f0ee6749fb59e86a719ed61f113f0779c72...e7c6af49f2a18ee96145e431433d07b994d15cba
git merge-base --is-ancestor 233b1f0ee6749fb59e86a719ed61f113f0779c72 e7c6af49f2a18ee96145e431433d07b994d15cba
git merge-base --is-ancestor e7c6af49f2a18ee96145e431433d07b994d15cba 233b1f0ee6749fb59e86a719ed61f113f0779c72
```

## File-level classification

### `EQUIVALENT_PRESENT`

These artifacts are already present with identical Git objects and must not be ported:

| Path | Verified object in both tips |
|---|---|
| `11.DPO/DpoDataset.cs` | blob `7071c0e638b6c0a88c2790404af181ea8efd2993` |
| `11.DPO/Models/DpoPair.cs` | blob `a2edcc8843290828db772041096b37623d53cd4d` |
| `25.ModelServer/DesignConversion/DesignConversionDpoTrainingHandoff.cs` | blob `354f488428cb4679ebc650f220a13f0b6f37702d` |
| `18.CapabilityTraining/` | tree `421cf17ff6e4cc07e8b8f4f5881e53d656dfc41e` |

### `PORT_AT_G_DPO_FINAL`

The following legacy artifacts contain translation-DPO or checkpoint lifecycle behavior that is absent from the current tip. They are preserved as later port candidates, not current work:

| Legacy path or member | Required later treatment |
|---|---|
| `27.TranslationDomain/SeedCorpus/TranslationSeedCorpusLoader.cs` | Reconcile loader validation and immutable seed provenance. |
| `27.TranslationDomain/SeedData/language-translate-seed-v1.jsonl` | Reconcile the self-authored seed artifact and legal/source manifest; never copy blindly as train-ready data. |
| `27.TranslationDomain/TranslationDpoTrainingHandoff.cs` | Reconcile the typed handoff into the current unit-level DPO contract. |
| `27.TranslationDomain/TranslationDpoTrainerBridge.cs` | Preserve the later `DpoTrainer.StepMany` and `EvaluateMany` bridge semantics behind final DPO authorization. |
| `27.TranslationDomain/TranslationDpoTrainEvaluateRunner.cs` | Preserve train/held-out/fresh-load/continue-training orchestration requirements without invoking it now. |
| `27.TranslationDomain/TranslationTrainingDataArtifactStore.cs` | Reconcile validated artifact storage, manifest and contamination behavior. |
| `27.TranslationDomain/TranslationModelAssetStore.cs` | Reconcile checkpoint/model-asset evidence and rollback compatibility. |
| `11.DPO/DpoTrainer.cs::LoadPolicyCheckpoint` | Restore or supersede the explicit checkpoint load boundary with fresh-load and continue-training tests. Legacy file blob is `1e90a7e4e4e943f61e233719d18c0614d436b63b`; current file blob is `6d76839ee32f4485d2fcd2351d83492bf2c24b94`. |
| `Tests/TranslationSeedCorpusTests.cs` | Port applicable seed validation/provenance coverage. |
| `Tests/TranslationDpoTrainingHandoffTests.cs` | Port applicable unit-level handoff coverage. |
| `Tests/TranslationDpoTrainerBridgeTests.cs` | Port applicable `StepMany`/`EvaluateMany` boundary coverage. |
| `Tests/TranslationDpoTrainEvaluateRunnerTests.cs` | Port held-out, save/load, fresh-load and continue-training coverage. |
| `Tests/TranslationModelAssetTests.cs` | Merge applicable legacy assertions at test level; do not overwrite the current file wholesale. |

All rows above use immutable source reference `refs/heads/DpoImplementation@233b1f0ee6749fb59e86a719ed61f113f0779c72`. Representative legacy provenance includes `1eab1e4084cc` (seed corpus), `157f8134` (seed loader wiring), `6cae9b8027` (handoff), `87a63ae9ef` (trainer bridge), `15e71a252d` (held-out evaluation), `b8e8fe1802` (train/evaluate runner), `c184fd1ea0` and `7f4cc91cd4` (training artifact store/wiring), `5ffa97817f` (model-asset training evidence), and `f4b026b` (`LoadPolicyCheckpoint`). These short hashes are navigation evidence; the immutable legacy tip is the source authority for later file reads.

Required later acceptance proof includes all of the following: checkpoint save; load into a fresh trainer instance; held-out evaluation after fresh load; continue-training from the loaded checkpoint; a second evaluation; manifest/legal/split evidence; and proof that no closed-proprietary output became a training target.

### `SUPERSEDED_KEEP_CURRENT`

- Keep the current DynamicGraph contracts, deterministic builder/projector/runtime owner, host-neutral composition contracts, L4 `AdaptiveLogicalOrchestrator` integration and current assistant/facade chain.
- Keep current ModelServer hardening, including current admission, artifact, approval, network-boundary, durable-state and activation behavior.
- Do not replace current files with legacy branch versions and do not merge the legacy branch wholesale.
- Current authority for these surfaces is `refs/heads/codex/dpo-implementation@e7c6af49f2a18ee96145e431433d07b994d15cba`; `504bcba93f7bcd4db1c93fc094eea3e8133d310d` remains the pre-slice code-first baseline evidence.

### `NEEDS_LATER_FILE_LEVEL_DECISION`

Every other legacy-only or mutually modified path remains unclassified until a bounded diff reads both versions and records one of: `EQUIVALENT_PRESENT`, `PORT_AT_G_DPO_FINAL`, `SUPERSEDED_KEEP_CURRENT`, or `DROP_WITH_EVIDENCE`. An unclassified path may not be silently merged, cherry-picked or treated as absent functionality. In particular, existing current translation contracts/tests must be merged at member/assertion level when their blobs differ.

## Slice 14C boundary

Slice 14C is `PAUSED_UNCOMMITTED_NEEDS_FIX`. Its current worktree changes are a live-local HTTP/session host candidate only. The inspected boundary uses HTTP JSON for request, readiness and session metadata; it is not dataset JSON/JSONL and does not call `DpoTrainer`, `StepMany`, `EvaluateMany`, `TrainBatchAsync`, a teacher API or checkpoint promotion. Its contract explicitly reports `performs_training=false` and rejects/contains `trained_input` only as a non-training boundary.

The last accepted published tip is 14B at `e7c6af49f2a18ee96145e431433d07b994d15cba`. Slice 14C must not be described as accepted, committed or published.

## Remaining code-first gaps

The current implementation is not product-ready. At minimum these code-first gaps remain open or unproven:

1. finish and independently verify the selected live-local host slice after this reconciliation gate;
2. platform client/facade integration and its production call chain;
3. persistence/restart of the selected profile plus revision/CAS state;
4. an explicit authority schema, canonicalization and resolver because current metadata is insufficient;
5. FigmaToCode and CanvaToCode caller integration/runtime proof, plus the applicable flows seam;
6. composite Gate B and same-scenario host/client evidence.

None of those gaps authorizes TP1, TP2, DPO-pair materialization or training.

## Gate acceptance criteria

`G-LEGACY-BRANCH-RECONCILIATION` passes only when:

1. an independent reviewer reproduces the two remote tips, merge-base, divergence counts and ancestry result;
2. every path named in this file is confirmed against both immutable tips;
3. the remaining changed-path inventory has an explicit classification owner and no silent whole-branch merge route;
4. the reviewer confirms that all `PORT_AT_G_DPO_FINAL` implementation actions remain blocked;
5. the reviewer confirms that dirty slice 14C contains no training invocation and is still uncommitted;
6. the execution board, two-phase overlay and file RAG point to this owner and preserve the exact DPO-last sequence.

After acceptance, work may resume only on the bounded non-training 14C slice. DPO port implementation remains deferred until `G_DPO_FINAL`.

## Addendum 14.08.26 — гейт закрыт merge-ом; канон = `codex/dpo-implementation`

> Пометка 14.08.26: упоминания ветки `DpoImplementation` в этом файле — historical; ветка-канон = `codex/dpo-implementation` (см. 14.08.26WorkLeft-05-branch-reconciliation-codex-dpo-implementation.md).

Переверд (живая инструкция Лубы 14.08.26; `14.08.26WorkLeft-05-branch-reconciliation-codex-dpo-implementation.md` §0/§3): ветка `DpoImplementation` была НЕПРАВИЛЬНОЙ рабочей веткой исполнителя, а не «legacy evidence source» с отдельной судьбой; единственная рабочая ветка и цель публикации — `codex/dpo-implementation`. Всё содержимое `DpoImplementation` (32 коммита сверх merge-base `80d0c642b3`; MERGE_HEAD `d2a385f75d`) слито в `codex/dpo-implementation` одним merge-коммитом (14.08/17.08; hash merge-коммита вписать после его создания: `<merge_commit_hash>`); три конфликтных файла (`14.ContinuousLearning/Fabric/Interfaces.cs`, `14.ContinuousLearning/Training/ContinuousLearningModelServerComposition.cs`, `18.CapabilityTraining/LlmIngredientLogicalUnits.cs`) разрешены «обе стороны сохранены» (WorkLeft-05 §2). После merge ветка `DpoImplementation` — historical/archive: не удалять без отдельной отмашки Лубы; новых коммитов в неё не делать; `origin/DpoImplementation` остаётся как есть.

Следствия для этого файла:
- `gate_state: BLOCKING_14C_PUBLICATION_AND_RESUME` и все формулировки «must be independently accepted before 14C» — historical/pre-merge evidence; текущий статус гейта: **CLOSED_1408 (by merge; W-011 → CLOSED_1408, WorkLeft-05 §3 п.5)**. Пофайловая реконсиляция с портом «только после G_DPO_FINAL» заменена фактическим merge-ом; классификации `PORT_AT_G_DPO_FINAL` ниже — historical: содержимое уже в `codex/dpo-implementation`.
- Топология «2171 legacy-only / 2313 current-only, ни одна не предок другой» (пины `233b1f0ee6` / `e7c6af49f`) — верный исторический факт на дату файла; после merge `d2a385f75d` — предок HEAD `codex/dpo-implementation` (`git merge-base --is-ancestor d2a385f75d HEAD` = true после коммита).
- `current_published_tip: e7c6af49f…` — historical (accepted tip slice 14B); текущий tip `codex/dpo-implementation` = `773afd5750` + merge-коммит.
- Запреты этого файла на training/DPO/teacher/checkpoint-операции не меняются: merge — операция git, не активация обучения; порядок `CODE_FIRST -> TP1_LOCAL -> TP2_HOSTED -> G_DPO_FINAL -> DPO/training` остаётся.
- Владелец переверда — `14.08.26WorkLeft-05-branch-reconciliation-codex-dpo-implementation.md`; этот файл — historical owner прежней классификации.

```text
addendum_1408: gate G-LEGACY-BRANCH-RECONCILIATION = CLOSED_1408 (by merge)
branch_canon: codex/dpo-implementation (ветка-канон исправлена 14.08.26; прежде ошибочно DpoImplementation)
merge_source_branch: DpoImplementation (historical/archive after merge; do not delete without explicit Luba authority)
merge_head: d2a385f75d; merge_base: 80d0c642b3; merge_commit_hash: <вписать после коммита>
w_reclass: W-011 -> CLOSED_1408
```
