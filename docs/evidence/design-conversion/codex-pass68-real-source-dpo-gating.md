# Codex Pass68 - Real Source DPO Gating

claim_scope: Pass68 implemented and verified a local DesignConversion gating slice for approval_preflight, source_manifest, source_disjoint row construction, negative_pair_quality enforcement, leakage quarantine, and training_candidate_only/product_not_ready boundaries.
evidence_scope: Local code/test reads and edits in `44.DesignConversionDomain` and `Tests`; Claude files and Pass60-67 packets were guidance/evidence only; no real S3, owner approval, production data, checkpoint upload, or product readiness was fabricated.
count_claimed: 1 sequential Pass68 implementation slice, 1 focused test run.
independent_evidence_count: 5 direct evidence groups: guidance inventory, code edits, test edits, focused dotnet test output, mergeability/readback commands.
subagent_id_or_NONE: Pass68.
downgrade_needed: Yes for product readiness, real training readiness, real S3 readiness, and global mergeability; no downgrade for the focused local test result.

## Inventory

- `approval_preflight`: existing real S3 validator checked approval/source bundle shape, but dataset row construction did not require an explicit approval/source preflight object.
- `source_manifest`: existing row records carried source manifest refs and source identity hashes, but dataset construction did not block rows when source manifest/source-disjoint preflight was absent.
- `source_disjoint`: dataset split grouped by source identity, but the preflight state was not surfaced in the manifest/JSONL boundary.
- `negative_pair_quality`: DPO generator already rejected several low-quality/hidden-deterministic negatives; real S3 row validation did not require safe chosen/rejected candidate objects plus `negative_quality_reason_hash` in `result_features`.
- `quarantine`: leakage and hidden deterministic fields were already routed through generator quarantine; Pass68 added a direct leakage regression test and dataset exclusion reasons for missing source/negative quality.
- `training_candidate_only` / `product_not_ready`: local fixtures could be structurally useful, but needed explicit boundary metadata to avoid being mistaken for real approved data.

## Repair

- Added `DesignConversionDpoDatasetPreflight` to route missing approval, missing source manifest, missing source-disjoint declaration, real validation, and local fixture training-candidate status.
- Dataset building now blocks rows unless preflight allows construction; missing approval/source manifest produces an empty dataset with manifest reasons instead of accepted rows.
- Dataset rows now emit safe chosen/rejected candidate objects, target unit/machine part, source structure, result features, preflight metadata, and product-not-ready markers.
- Real S3 DPO row validation now requires:
  - target `unit_id` and `machine_part`;
  - source kind plus source identity/source structure hashes;
  - `result_features` including chosen/rejected refs, pair family, and non-placeholder negative quality reason hash;
  - safe root and candidate-result chosen/rejected objects;
  - teacher label preference, rejected result ref, and pair family.
- Training pipeline batch construction now returns no batches when approval/source preflight is missing and requires source manifest, meaningful negative quality, and non-quarantine taxonomy for train-ready rows.
- Focused tests added or adjusted for missing approval blocks, missing source manifest blocks, placeholder rejected quality blocks, valid local fixture as training_candidate_only/product_not_ready, and leakage quarantine.

## Verification

- Focused tests:
  - command: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionDpoDatasetBuilderTests|FullyQualifiedName~DesignConversionDpoGeneratorTests|FullyQualifiedName~DesignConversionRealS3EvidenceBundleValidatorTests|FullyQualifiedName~DesignConversionTrainingPipelineTests|FullyQualifiedName~DesignConversionMeasurementGateTests|FullyQualifiedName~DesignConversionPromotionTests" --no-restore --logger "console;verbosity=minimal"`
  - result: Passed, 60 total, 0 failed, 0 skipped.
- `git diff --check` for the touched accepted scope printed no whitespace errors.
- Direct whitespace scan over touched accepted files printed no trailing-whitespace findings, including untracked files that `git diff --check` cannot cover.
- Marker readback found `approval_preflight`, `source_manifest`, `source_disjoint`, `negative_pair_quality`, `quarantine`, `training_candidate_only`, and `product_not_ready` in changed code/tests.

## Mergeability

- `git fetch origin --prune` completed; git reported repository maintenance noise about unreachable loose objects, and no prune/clean was run.
- `git merge-base HEAD origin/main` and `git merge-base HEAD origin/codex/chat-gpt-5.5` both returned `642b67c2bb2330b35a2b5a4e3972f7c6657abff3`.
- Branch-level `git merge-tree --name-only --messages HEAD origin/main` and the same command to `origin/codex/chat-gpt-5.5` exited nonzero with conflicts outside the Pass68 accepted scope:
  - `docs/plans/evidence/extension-full-readiness-dual-path/quality-semantic-row12-heldout-ablation-freshload-20260628T160537Z/heldout.json`
  - `docs/plans/evidence/extension-full-readiness-dual-path/quality-semantic-row12-heldout-ablation-freshload-20260628T160537Z/heldout.ru.md`
  - `docs/skills/output-readability-gate-SKILL.md`
- Changed-scope target diff for the accepted Pass68 files against both target refs printed no output.
- Therefore mergeability evidence supports only: Pass68 changed scope showed no target-side file drift by diff and no named merge-tree conflicts, but global branch/all-dirty mergeability is not proven.

## Mental Simulation

- No approval: `approval_preflight_missing` blocks dataset rows; output remains product_not_ready.
- Approval but no source manifest: `source_manifest_preflight_missing` blocks dataset rows; output remains product_not_ready.
- Valid local fixture with high-quality negative: rows can be built only as `training_candidate_only`, with source_disjoint declared and product_not_ready true.
- Leaky row: generator quarantines leakage/shortcut fields; dataset builder excludes non-train-ready records.

## Remaining Queue

- Obtain real Luba/owner approval packet; do not substitute local fixture approval.
- Obtain and validate real source_manifest/source-disjoint grouping from approved source data.
- Prove heldout/source-disjoint measurement on real approved rows.
- Produce real checkpoint/fresh-load/e2e evidence before any product readiness, real training readiness, model upload, or promotion claim.
- Resolve unrelated branch-level merge-tree conflicts before any global mergeability claim.
