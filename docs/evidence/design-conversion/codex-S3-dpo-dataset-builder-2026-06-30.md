# Codex S3 DPO Dataset Builder Evidence - 2026-06-30

claim_scope: Narrow S3 dataset-builder repair for Mill review findings only. No training runner, no external training, no browser/e2e, no git publishing.
evidence_scope: Local edits to the dataset builder, its focused tests, and this evidence packet in C:\Projects\llm_mvp_core only.
count_claimed: 1 focused dataset-builder test command passed before this evidence rewrite.
independent_evidence_count: 1 command output at this packet revision point.
subagent_id_or_NONE: NONE.
downgrade_needed: No for the listed Mill dataset-builder fixes; yes for any broader S3/training readiness claim.

## Files Changed

- `44.DesignConversionDomain\DesignConversionDpoDatasetBuilder.cs`
- `Tests\DesignConversionDpoDatasetBuilderTests.cs`
- `docs\evidence\design-conversion\codex-S3-dpo-dataset-builder-2026-06-30.md`

## Mill Findings Fixed

1. HIGH source split leakage:
   - `BuildSourceIdentity` no longer includes `SourceKind`.
   - Source identity now canonicalizes by source identity scope/value: `source_design_id`, then `source_example_id`, then source manifest ref, source request id, then record id.
   - `SourceKind` remains row/manifest metadata only.
   - A focused test covers same `source_design_id` with different source kinds and asserts one source hash/group/split for those rows, with no train/held-out source overlap.

2. HIGH JSONL/schema gap:
   - JSONL rows now include guide-required safe fields:
     - `candidate_result`
     - `teacher_labels`
     - `teaching_point`
     - `curriculum_tier`
     - `validation_outcome`
   - `candidate_result` uses safe `dcm-result-*` refs, result hashes, accepted flags, and visual scores.
   - `teacher_labels` derives safe labels from accepted-record evidence: preferred/rejected result refs, `chosen_over_rejected`, score margin, pair family, and defect categories.
   - `validation_outcome` records accepted-row booleans/status only, without raw request, reviewer, license id, candidate id, path, or prompt text.

3. MEDIUM source group id prefix:
   - Source group ids now use `dcm-source-{hash}`.
   - Tests assert the prefix in dataset rows, manifest source groups, and serialized JSONL.

4. MEDIUM tests insufficient:
   - Added/updated focused behavioral assertions for source-kind collision, `dcm-source-*`, guide-required JSONL fields, and raw leakage absence.

5. MEDIUM evidence packet overstatement:
   - This packet does not claim S3 complete, training-runner readiness, real 50+50 availability, promotion, checkpointing, browser/e2e, or external training.
   - The evidence supports only the fixed dataset-builder slice listed above.

## Validation Captured Before This Packet Rewrite

Command:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~DesignConversionDpoDatasetBuilderTests --no-restore
```

Result:

```text
Passed!  - Failed:     0, Passed:     5, Skipped:     0, Total:     5, Duration: 321 ms - LlmFromScratch.Tests.dll (net9.0)
```

Note: the build also printed existing unrelated warnings from other test files. No dataset-builder test failed.

## Remaining Boundary

- Final bounded-worker return packet must report a whitespace/content check run after this evidence packet update and covering all three allowed files without staging.
- Training runner work remains explicitly not advanced in this slice.
