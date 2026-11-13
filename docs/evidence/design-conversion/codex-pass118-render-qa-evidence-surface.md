# Codex Pass118 Render QA Evidence Surface

## Corrected Required Packet

claim_scope: Pass118 implemented a narrow local DesignConversion render/QA probe surface and ModelServer boundary metadata for browser/render QA.

evidence_scope: Evidence is limited to allowed DesignConversion/ModelServer/test file edits, the Pass118 evidence file, focused DesignConversion/ModelServer test runs, and hygiene/readback scans. No external visual AI provider was called. Real browser screenshots were not claimed unless an injected browser-backed provider reports browser use.

count_claimed: 1

independent_evidence_count: 18

subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT

subagent_identity_self_check: spawned_worker=true; used_NONE=false; parent_must_reconcile=true

downgrade_needed: no for the narrow local render/QA contract/probe; yes for full product, public HTTP, deployment, upload, cross-repo caller wiring, and real visual AI completion.

skill_load_status: all required files found/read in this turn's evidence chain. AGENTS.md lines=6910 sha256=dfc983973a758598; docs/SESSION-START-PROMPT.md lines=620 sha256=450961e74c8e9db4; planning-session-startup lines=2038 sha256=5677e5ad258ae6cd; architect-behavior-classifier lines=1466 sha256=55bdf13c02a06d01; authority-requirement-binding lines=329 sha256=35607efac3f50f3a; reference-inventory-and-source-map lines=336 sha256=ff8388e9c40b3ba1; phase-planning lines=1151 sha256=deb0a3c1277dfc0b; plan-review lines=3744 sha256=5298a2ca4d0a112e; goal-delivery-completeness lines=612 sha256=2012ccff230463c0; claim-verification lines=763 sha256=febbc2894b9bfb47; audit-protocol lines=496 sha256=dadb397c46901a15; specificity-calibration lines=306 sha256=bbcc5ac54a61be46; output-skepticism lines=370 sha256=afe71fa8a2e944d0; reconnaissance-gate lines=467 sha256=1787816227941458; trainable-architecture-decision lines=737 sha256=3a751518e20acbf3; no-product-decisions lines=158 sha256=7009f0b516427a5d; output-readability-gate lines=724 sha256=3f10734fa35c4ad9; implementation-doc-uml lines=114 sha256=cd6bdfb3b2c66efb; implementation-doc-algorithms lines=133 sha256=318047ac164a813a; implementation-doc-code-refs lines=144 sha256=f38bb8be562d4dbd; implementation-doc-status lines=86 sha256=25c8a8dd5cb2d170; implementation-doc-hierarchy lines=102 sha256=a32685db4ff05190.

files_read: 44.DesignConversionDomain/DesignConversionContracts.cs; 44.DesignConversionDomain/DesignConversionThinClientJobRuntime.cs; 44.DesignConversionDomain/DesignConversionInternalService.cs; 44.DesignConversionDomain/DesignConversionQaContracts.cs; 44.DesignConversionDomain/DesignConversionQaProviders.cs; 44.DesignConversionDomain/DesignConversionBrowserQaProviders.cs; 44.DesignConversionDomain/DesignConversionVisualQaBoundaryPolicy.cs; 25.ModelServer/DesignConversion/DesignConversionModelServerApi.cs; Tests/DesignConversionBrowserQaProviderTests.cs; Tests/DesignConversionModelServerApiTests.cs; Tests/DesignConversionLocalFullLoopE2eTests.cs; Tests/DesignConversionThinClientJobRuntimeTests.cs; docs/evidence/design-conversion/codex-pass118-render-qa-evidence-surface.md.

files_changed: 44.DesignConversionDomain/DesignConversionLocalRenderQaProbe.cs; 25.ModelServer/DesignConversion/DesignConversionModelServerApi.cs; Tests/DesignConversionLocalRenderQaProbeTests.cs; Tests/DesignConversionModelServerApiTests.cs; docs/evidence/design-conversion/codex-pass118-render-qa-evidence-surface.md.

commands_run: required skill/file hash read; render/QA/code search; focused dotnet test first run failed at compile due new-test initializer syntax; focused dotnet test second run passed; focused dotnet test final rerun after validation hardening passed; conflict-marker scan; mojibake-marker scan; git diff --check on changed allowed files; identity placeholder scan; target-term readback.

tests_result: final focused command passed: `dotnet test Tests\LlmFromScratch.Tests.csproj -p:TargetFrameworks=net9.0 -p:TargetFramework=net9.0 --filter "FullyQualifiedName~DesignConversion|FullyQualifiedName~ModelServer" --no-restore --logger "console;verbosity=minimal"` reported 163 passed, 0 failed, 0 skipped.

implementation_summary: Added `DesignConversionLocalRenderQaProbe` local contract/probe over injected render and comparison providers; added tests for multi-viewport call-through, refs-only/no raw payload leakage, browser-used boundary metadata, fake/no-browser boundary metadata, invalid viewport failure, invalid expected ref failure, and missing expected ref failure. ModelServer boundary metadata now includes render/QA counts, viewport counts, browser execution flags, visual AI execution flags, refs-only artifact boundaries, and retains `ModelUploadReady=false`.

mental_simulation_result: API or local caller supplies HTML/CSS, expected design export refs, and Viewports; probe validates positive dimensions and expected refs; render provider is invoked; comparison provider compares expected and actual refs; result returns only refs/hashes plus score/defect summaries; fake/deterministic path is marked browser not executed and visual AI not executed; browser-backed providers can mark browser executed without implying visual AI.

boundaries_not_proven: real browser screenshots were not generated by this pass unless a real browser-backed provider is supplied and reports browser use; no external visual AI provider was called; no public HTTP route, deployment, upload, cross-repo caller wiring, or full product completion was proven.

remaining_queue: browser E2E visual fidelity with real expected export files; cross-repo caller wiring; real HTTP/deployment/upload/full product.

packet_self_check: required headings present in this corrected packet; identity value is UNKNOWN_IN_SUBAGENT; identity self-check line present; placeholder identity scan and target-term readback are required after this update.

## Claim Guard

- Claim scope: Pass118 implemented a local DesignConversion render/QA probe surface and ModelServer boundary metadata for browser/render QA.
- Evidence scope: Code edits in allowed DesignConversion/ModelServer/test files; focused DesignConversion/ModelServer tests passed; no external visual AI provider was called.
- Count claimed: 1.
- Independent evidence count: 18.
- Identity value: UNKNOWN_IN_SUBAGENT.
- Identity self-check: spawned_worker=true; used_NONE=false; parent_must_reconcile=true.
- Downgrade needed: no for the narrow local contract/probe surface; yes for full product, public HTTP, deployment, upload, and real visual AI completion.

## Skill Load Status

- AGENTS.md: found_full_file_hash_read, lines=6910, sha256=dfc983973a758598
- docs/SESSION-START-PROMPT.md: found_full_file_hash_read, lines=620, sha256=450961e74c8e9db4
- docs/skills/planning-session-startup-SKILL.md: found_full_file_hash_read, lines=2038, sha256=5677e5ad258ae6cd
- docs/skills/architect-behavior-classifier-SKILL.md: found_full_file_hash_read, lines=1466, sha256=55bdf13c02a06d01
- docs/skills/authority-requirement-binding-SKILL.md: found_full_file_hash_read, lines=329, sha256=35607efac3f50f3a
- docs/skills/reference-inventory-and-source-map-SKILL.md: found_full_file_hash_read, lines=336, sha256=ff8388e9c40b3ba1
- docs/skills/phase-planning-SKILL.md: found_full_file_hash_read, lines=1151, sha256=deb0a3c1277dfc0b
- docs/skills/plan-review-SKILL.md: found_full_file_hash_read, lines=3744, sha256=5298a2ca4d0a112e
- docs/skills/goal-delivery-completeness-SKILL.md: found_full_file_hash_read, lines=612, sha256=2012ccff230463c0
- docs/skills/claim-verification-SKILL.md: found_full_file_hash_read, lines=763, sha256=febbc2894b9bfb47
- docs/skills/audit-protocol-SKILL.md: found_full_file_hash_read, lines=496, sha256=dadb397c46901a15
- docs/skills/specificity-calibration-SKILL.md: found_full_file_hash_read, lines=306, sha256=bbcc5ac54a61be46
- docs/skills/output-skepticism-SKILL.md: found_full_file_hash_read, lines=370, sha256=afe71fa8a2e944d0
- docs/skills/reconnaissance-gate-SKILL.md: found_full_file_hash_read, lines=467, sha256=1787816227941458
- docs/skills/trainable-architecture-decision-SKILL.md: found_full_file_hash_read, lines=737, sha256=3a751518e20acbf3
- docs/skills/no-product-decisions-SKILL.md: found_full_file_hash_read, lines=158, sha256=7009f0b516427a5d
- docs/skills/output-readability-gate-SKILL.md: found_full_file_hash_read, lines=724, sha256=3f10734fa35c4ad9
- docs/skills/implementation-doc-uml-SKILL.md: found_full_file_hash_read, lines=114, sha256=cd6bdfb3b2c66efb
- docs/skills/implementation-doc-algorithms-SKILL.md: found_full_file_hash_read, lines=133, sha256=318047ac164a813a
- docs/skills/implementation-doc-code-refs-SKILL.md: found_full_file_hash_read, lines=144, sha256=f38bb8be562d4dbd
- docs/skills/implementation-doc-status-SKILL.md: found_full_file_hash_read, lines=86, sha256=25c8a8dd5cb2d170
- docs/skills/implementation-doc-hierarchy-SKILL.md: found_full_file_hash_read, lines=102, sha256=a32685db4ff05190

## Files Read

- 44.DesignConversionDomain/DesignConversionContracts.cs
- 44.DesignConversionDomain/DesignConversionThinClientJobRuntime.cs
- 44.DesignConversionDomain/DesignConversionInternalService.cs
- 44.DesignConversionDomain/DesignConversionQaContracts.cs
- 44.DesignConversionDomain/DesignConversionQaProviders.cs
- 44.DesignConversionDomain/DesignConversionBrowserQaProviders.cs
- 44.DesignConversionDomain/DesignConversionVisualQaBoundaryPolicy.cs
- 25.ModelServer/DesignConversion/DesignConversionModelServerApi.cs
- Tests/DesignConversionBrowserQaProviderTests.cs
- Tests/DesignConversionModelServerApiTests.cs
- Tests/DesignConversionLocalFullLoopE2eTests.cs
- Tests/DesignConversionThinClientJobRuntimeTests.cs

## Files Changed

- 44.DesignConversionDomain/DesignConversionLocalRenderQaProbe.cs
- 25.ModelServer/DesignConversion/DesignConversionModelServerApi.cs
- Tests/DesignConversionLocalRenderQaProbeTests.cs
- Tests/DesignConversionModelServerApiTests.cs
- docs/evidence/design-conversion/codex-pass118-render-qa-evidence-surface.md

## Implementation Summary

- Added `DesignConversionLocalRenderQaProbe`, a local Render/QA contract that accepts submitted HTML/CSS, a Viewport list, and expected design export refs.
- The probe invokes `IDesignConversionRenderProvider.RenderAsync` and `IDesignConversionComparisonProvider.CompareAsync` through injected interfaces and returns render refs, comparison refs, score summary, defect summary, and boundary metadata.
- The deterministic/fake path is explicitly marked with `browser_render_not_executed=true` and `visual_ai_not_executed=true`; it does not claim real screenshots.
- If a provider reports `browser_or_playwright_used=true`, the probe marks `browser_render_executed=true` while still keeping `visual_ai_not_executed=true`.
- ModelServer boundary metadata now carries `render_qa_result_count`, `comparison_artifact_count`, Viewport coverage counts, `browser_render_executed`, `browser_render_not_executed`, `visual_ai_executed`, `visual_ai_not_executed`, `render_artifact_refs=refs_or_hashes_only`, and `comparison_artifact_refs=refs_or_hashes_only`.
- `ModelUploadReady` remains false; this pass does not claim upload, deployment, public HTTP, platform integration, or full product readiness.

## Verification Commands Run

- `dotnet test Tests\LlmFromScratch.Tests.csproj -p:TargetFrameworks=net9.0 -p:TargetFramework=net9.0 --filter "FullyQualifiedName~DesignConversion|FullyQualifiedName~ModelServer" --no-restore --logger "console;verbosity=minimal"`: first run failed at compile because the new test file used invalid `with` initializer syntax.
- `dotnet test Tests\LlmFromScratch.Tests.csproj -p:TargetFrameworks=net9.0 -p:TargetFramework=net9.0 --filter "FullyQualifiedName~DesignConversion|FullyQualifiedName~ModelServer" --no-restore --logger "console;verbosity=minimal"`: second run passed, 163 passed, 0 failed, 0 skipped.
- `dotnet test Tests\LlmFromScratch.Tests.csproj -p:TargetFrameworks=net9.0 -p:TargetFramework=net9.0 --filter "FullyQualifiedName~DesignConversion|FullyQualifiedName~ModelServer" --no-restore --logger "console;verbosity=minimal"`: final rerun after validation hardening passed, 163 passed, 0 failed, 0 skipped.
- Conflict marker scan on changed files: no matches.
- Mojibake marker scan on changed files for the configured non-ASCII corruption markers: no matches.
- `git diff --check -- <changed allowed files>`: passed with no output.
- Identity self-check for forbidden placeholder identity forms: no matches.

## Tests Result

Focused DesignConversion/ModelServer verification passed after final code edits: 163 tests passed.

## Mental Simulation Result

1. API or local caller supplies submitted HTML/CSS, expected design export refs, and Viewport list.
2. `DesignConversionLocalRenderQaProbe` validates positive Viewport dimensions and expected render refs.
3. The probe calls the injected render provider. A real browser provider may produce screenshot refs; deterministic/fake providers return refs only and are marked not-browser.
4. The probe calls the injected comparison provider with expected and actual image refs.
5. The result returns only refs/hashes, score summary, defect count/categories, and boundary metadata.
6. visual_ai remains not executed unless a future separate visual judge provider is deliberately introduced.

## Boundaries Not Proven

- Real browser screenshots were not claimed by this pass unless an actual browser-backed provider is supplied and reports browser use.
- No external visual AI provider was called.
- No public HTTP route was added or proven.
- No deployment, upload, platform wiring, cross-repo caller wiring, full product completion, or ModelUploadReady state was proven.

## Remaining Queue

- Browser E2E visual fidelity remains broader than this local contract unless run with a real browser provider and expected export files.
- Cross-repo caller wiring remains open.
- Real HTTP/deployment/upload/full product remains open.

## Packet Self Check

- Identity value: UNKNOWN_IN_SUBAGENT
- Identity self-check: spawned_worker=true; used_NONE=false; parent_must_reconcile=true
- Searched/readback target terms after this write: `Render`, `QA`, `Viewport`, `browser`, `visual_ai`, `ModelUploadReady`, `subagent_id_or_NONE`, and `subagent_identity_self_check`.
