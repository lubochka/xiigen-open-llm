# Pass226 QSG13/QSG17/QSG19 Repair Evidence

pass226_verdict: BOUNDED_PASS_NON_OOM_QSG13_QSG17_QSG19_REPAIRED
qsg13_result: PASS dotnet test focused class, 1 passed, 0 failed, 0 skipped after LF capture-writer repair.
qsg17_result: PASS dotnet test focused class, 1 passed, 0 failed, 0 skipped after null/missing locator ref repair.
qsg19_result: PASS dotnet test focused class, 1 passed, 0 failed, 0 skipped after locator ref preservation/root materialization repair.
qsg13_19_slice_result: PASS dotnet test QSG13-QSG19 slice, 7 passed, 0 failed, 0 skipped.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT

claim_scope: Pass226 bounded non-OOM repair for QSG13, QSG17, and QSG19 only.
evidence_scope: focused reproduction from QSG13-QSG19 slice, code/artifact readbacks, focused QSG13/QSG17/QSG19 tests, final QSG13-QSG19 slice, git diff hygiene, cached-index check.
count_claimed: 3 repaired non-OOM failures; 3 individual focused passing tests; 1 passing seven-test QSG13-QSG19 slice.
independent_evidence_count: 9 final evidence records: reproduction failure, QSG13 pass, QSG17 pass, QSG19 pass, QSG13-QSG19 slice pass, QSG19 ref readback, git diff check, cached-index check, and evidence artifact marker check.
downgrade_needed: no for bounded Pass226 repair; no full-suite, ProductReady, commit, stage, push, PR, deploy, or external-provider claim.

## Skill Load Status

- AGENTS.md: found/read.
- docs/SESSION-START-PROMPT.md: found/read.
- planning-session-startup-SKILL.md: found/read.
- claim-verification-SKILL.md: found/read.
- audit-protocol-SKILL.md: found/read.
- goal-delivery-completeness-SKILL.md: found/read.
- verification-before-completion-SKILL.md: found/read.
- test-integrity-SKILL.md: found/read.
- generated-code-review-SKILL.md: found/read.
- implementation-integrity-SKILL.md: found/read.
- code-examination-SKILL.md: found/read.
- qa-session-type-SKILL.md: found/read.

## Root Cause By Failure

- QSG13 unsafe display leakage: normal orchestration chat text exposed the raw scenario id in `Orchestration-plan produced scenario ...`; after removing that leak, QSG13 also exposed that adaptive host projection warnings were too verbose to meet the existing 54/54 token-reduction gate.
- QSG17 null locator-ref path handling: QSG16 locator artifact had null proposed manifest and sha fields plus unmaterialized target/rollback roots; the test dereferenced null paths instead of completing refs from real local files.
- QSG19 proposed locator refs not preserved: QSG19 ran against a missing target root, so the positive request failed before the service could copy proposed locator refs into `CheckSetResults`.

## Repair Summary

- Replaced raw orchestration scenario id display with a bounded non-leaking summary.
- Added user-adaptive-only compact warnings for the normal trained-state path while preserving `semantic_quality_claimed=false` and `core_boundary_only_no_host_runtime_claim`.
- Routed QSG13 capture request/response writes through the existing LF-normalizing JSON writer so tracked generated artifacts pass diff hygiene.
- Made QSG17 read nullable locator fields safely, infer missing manifest refs, compute missing sha256 values from materialized local files, and record test-only target/rollback root materialization.
- Made QSG19 independently materialize test-only target and rollback roots before exercising `TargetLocator` plumbing and verifying proposed refs are preserved without `[object Object]`.

## Commands Run

- Reproduction: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ExtensionQualitySemanticGates13AdaptiveDisplayTests|FullyQualifiedName~ExtensionQualitySemanticGates14ClosureTests|FullyQualifiedName~ExtensionQualitySemanticGates15PromotionReadinessTests|FullyQualifiedName~ExtensionQualitySemanticGates16CandidateProbeTests|FullyQualifiedName~ExtensionQualitySemanticGates17ProductRuntimeVerificationTests|FullyQualifiedName~ExtensionQualitySemanticGates18LiveHostTargetRollbackClosureTests|FullyQualifiedName~ExtensionQualitySemanticGates19TargetLocatorPlumbingTests" --logger "console;verbosity=minimal"` -> failed 3/7 before repair.
- QSG13 focused after final edit: passed 1/1.
- QSG17 focused after repair: passed 1/1.
- QSG19 focused after repair: passed 1/1.
- QSG13-QSG19 slice after final edit: passed 7/7.
- `git diff --check` on relevant changed tracked files: PASS, path_count=40.
- `git diff --cached --name-only`: empty output; cached index unchanged.

## Artifact Verification

- QSG17 active locator readback: `safe_to_activate_test_only=true`, `rollback_refs_verified_before_activation=true`, `target_hashes_verified_before_activation=true`.
- QSG19 report readback: `proposed_locator_refs_preserved=true`, `object_object_locator_refs_rejected=true`, `host_locator_ref_rows_passed=true`.
- QSG19 host rows readback: Claude, Cursor, and Codex each reported `plumbing_verified=true`, `proposed_locator_refs_preserved=true`, `locator_refs_not_object_object=true`, and `expected_locator_ref_count=3`.
- Evidence artifact marker readback: required markers present; conflict marker count 0; mojibake marker count 0.

## Remaining NEEDS_FIX Or External Blockers

- No remaining NEEDS_FIX for the bounded Pass226 QSG13/QSG17/QSG19 non-OOM failures.
- Full suite was not run.
- ProductReady was not claimed.
