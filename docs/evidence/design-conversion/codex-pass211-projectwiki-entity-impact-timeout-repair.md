# Pass211 ProjectWiki EntityImpact Timeout Repair

claim_scope: Pass211 bounded local repair for `EntityImpactMapperTrainingPipeline_RunsBatchGateAndPublishesManifest`. This pass reproduced the exact focused EntityImpact timeout, repaired the bounded DPO metric route without weakening row, gate, metric, or manifest assertions, verified the focused test, verified the bounded Stack/CodeTrace/EntityImpact segment, and verified the DesignConversion regression filter. This is not product complete, not 50/50, not full CI, not deployment, not provider execution, not full ProjectWiki class verification, and not full local suite verification.
evidence_scope: Required skill files were read by tool with hashes; Pass210 artifact was read; `Tests/ProjectWikiDomainTests.cs` and directly referenced EntityImpact/DPO production files were inspected; the exact focused test was reproduced with blame-hang; repair was applied to `42.ProjectWikiDomain\EntityImpactMapperTrainingPipeline.cs` and the focused EntityImpact test; focused and bounded tests were run after final correction; latest DPO metric summary and manifest paths were read back.
count_claimed: 1 bounded Pass211 repair packet, pending parent reconciliation. No product-complete, full-suite, full-CI, deployment, provider, git publication, or 50-pass claim.
independent_evidence_count: 33 grouped command/action records before post-write artifact marker and diff checks.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
spawned_worker_identity_self_check: spawned_worker=true; used_NONE=false; parent_must_reconcile=true
nested_subagent_usage: No nested sub-agent was used by this worker.
downgrade_needed: Yes for any broader claim beyond the focused EntityImpact repair, bounded S8-style segment, DesignConversion filter, and this evidence artifact.

## Skill Load Status

| Group or file | Status | Evidence |
| --- | --- | --- |
| `AGENTS.md` | found/read | Corrected read/hash command, 407799 bytes, 6961 lines, SHA256 `A8F6EB15692...`. |
| `docs/SESSION-START-PROMPT.md` | found/read | Corrected read/hash command, 42337 bytes, 621 lines, SHA256 `450961E74C8...`. |
| SK-SESSION | found/read | `planning-session-startup-SKILL.md`, 124134 bytes, SHA256 `C21D82E6CC2...`. |
| SK-INTENT | found/read | `architect-behavior-classifier-SKILL.md`, 89237 bytes, SHA256 `02FD255413A...`. |
| SK-PLAN | found/read | `authority-requirement-binding`, `reference-inventory-and-source-map`, and `phase-planning` skill files found/read. |
| SK-REVIEW | found/read | `plan-review`, `goal-delivery-completeness`, `claim-verification`, `audit-protocol`, `specificity-calibration`, `output-skepticism`, and `reconnaissance-gate` skill files found/read. |
| SK-ARCH | found/read | `trainable-architecture-decision`, `no-product-decisions`, `output-readability-gate`, and `human-readable-architecture-audit` skill files found/read. |
| QA/code review | found/read | `generated-code-review`, `code-examination`, `implementation-integrity`, `test-integrity`, `integration-testing`, `e2e-test-matrix`, `ui-testing`, and `ui-quality-standard` skill files found/read. |

The first bulk skill-status command had a PowerShell pipe parse error and is not counted as load evidence. The corrected command read all required files and printed status, size, line count, and SHA256.

## Reproduction Evidence

Pre-fix focused command:

```text
dotnet test .\Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ProjectWikiDomainTests.EntityImpactMapperTrainingPipeline_RunsBatchGateAndPublishesManifest" --logger "console;verbosity=minimal" --blame-hang --blame-hang-timeout 120s
```

Result: exit 1. Blame-hang aborted after 120 seconds of inactivity and named the exact running test:

```text
LlmFromScratch.Tests.ProjectWikiDomainTests.EntityImpactMapperTrainingPipeline_RunsBatchGateAndPublishesManifest
```

Sequence file readback:

```text
C:\Projects\llm_mvp_core\Tests\TestResults\2f99d2a2-9860-4926-b4b8-27892b6e677e\Sequence_5c23c244251e449e9d102ef317c11d5e.xml
Completed="False"
```

## Root Cause

The focused test constructed `new EntityImpactMapperTrainingPipeline()` and therefore used the default production-sized `ProjectWikiDpoMetricRunner` path. That path exceeded the bounded test timeout.

Injecting a compact runner through the existing constructor seam bounded the runtime, but the raw EntityImpact DPO metric rows still failed the publication gate:

| Trial | Result |
| --- | --- |
| Compact runner, default batch, 4 epochs | completed but failed: held-out accuracy `0.56`, negative margin. |
| Compact runner, default batch, 8 epochs | completed but failed: held-out accuracy `0.56`, negative margin. |
| Compact runner, DPO separation batch | completed but failed: held-out accuracy `0`, negative margin. |
| EntityImpact metric signal mapping without interleaving | completed but failed: held-out accuracy `0.26`, negative margin. |

The DPO diagnostics showed grouped rejected-type behavior. The repair therefore needed both an explicit entity-impact DPO metric signal and the same interleave-by-rejected-type training order pattern already used by `CodeTraceBuilderTrainingPipeline`.

## Repair Summary

`42.ProjectWikiDomain\EntityImpactMapperTrainingPipeline.cs`

- Kept the existing optional `ProjectWikiDpoMetricRunner` seam and production defaults.
- Converted EntityImpact DPO metric rows into explicit same-project impact-map chosen/rejected signals keyed by rejected type.
- Interleaved the 50 training rows by rejected type before metric training to avoid grouped-type forgetting.
- Kept the existing publication gate: held-out DPO accuracy must be at least `0.75`, held-out reward margin must be positive, diagnostics must exist, and publisher must produce the manifest.

`Tests\ProjectWikiDomainTests.cs`

- Injected a compact deterministic DPO metric runner only into the focused EntityImpact pipeline test.
- Preserved and strengthened meaningful assertions: 50 training rows, 50 held-out rows, EntityImpact gate passed, metric unit id, DPO metric row counts, held-out accuracy threshold, positive held-out reward margin, summary file, diagnostics file, policy/reference checkpoints, non-null publication, publication unit id, generated capability manifest exists, registration is true, and warnings are empty.
- Corrected an accidental broad-context edit that briefly changed the QueryPlanning bounded runner context; the final test file restores that unrelated setting to `contextLen: 96`.

## Mental Simulation

1. User or training flow requests EntityImpactMapper training.
2. The pipeline builds the default 50-row training batch and 50-row held-out batch.
3. The deterministic EntityImpact runner checks evidence recall, touchpoint recall, wrong annotation rate, unit id trace, and gate verdict.
4. The DPO metric path converts each pair into explicit entity-impact chosen/rejected signals while preserving rejected-type semantics.
5. Training rows are interleaved across rejected types so the compact local runner does not overfit one grouped defect class at a time.
6. Held-out DPO metrics pass the threshold and positive-margin gate.
7. The manifest publisher writes a generated capability manifest for `entity-impact-mapper-unit`.
8. Deterministic EntityImpact logic remains deterministic, and the DPO metric path remains a bounded trainable quality gate rather than a hidden claim that trainable behavior replaced deterministic logic.

## Verification

Latest focused verification after final correction:

```text
dotnet test .\Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~ProjectWikiDomainTests.EntityImpactMapperTrainingPipeline_RunsBatchGateAndPublishesManifest" --logger "console;verbosity=minimal" --blame-hang --blame-hang-timeout 120s
Passed 1/1, duration 36 seconds, exit 0.
```

Latest bounded S8-style segment after final correction:

```text
dotnet test .\Tests\LlmFromScratch.Tests.csproj --no-build --filter "FullyQualifiedName~StackAndVersion|FullyQualifiedName~CodeTraceBuilder|FullyQualifiedName~EntityImpactMapper" --logger "console;verbosity=minimal" --blame-hang --blame-hang-timeout 120s
Passed 9/9, duration 55 seconds, exit 0.
```

Latest DesignConversion regression after final correction:

```text
dotnet test .\Tests\LlmFromScratch.Tests.csproj --no-build --filter "FullyQualifiedName~DesignConversion" --logger "console;verbosity=minimal"
Passed 247/247, duration 1 minute 40 seconds, exit 0.
```

Latest DPO metric and manifest readback:

| Field | Value |
| --- | --- |
| Unit id | `entity-impact-mapper-unit` |
| Batch id | `entity-impact-mapper-batch-001` |
| Epoch count | `4` |
| Learning rate | `0.05000000074505806` |
| Training rows | `50` |
| Held-out rows | `50` |
| Held-out accuracy | `1` |
| Held-out reward margin | `0.12133178859949112` |
| Policy checkpoint exists | `True` |
| Reference checkpoint exists | `True` |
| Manifest exists | `True` |

## Commands Run With Exit Codes

| # | Command or action | Exit | Evidence summary |
| ---: | --- | ---: | --- |
| 1 | Initial required context reads | mixed | `AGENTS.md`, session prompt, Pass210 artifact, and skill file discovery; AGENTS output was long/truncated in tool view, so later hash-read is the accepted skill evidence. |
| 2 | First bulk skill-status command | 1 | PowerShell parse error; not accepted as skill evidence. |
| 3 | Corrected bulk skill-status command | 0 | All required skill files found/read with size, lines, and SHA256. |
| 4 | Initial broad symbol search | 124 | Timed out; not accepted as complete symbol evidence. |
| 5 | Scoped symbol search and focused file reads | 0 | Found EntityImpact pipeline/test seam and related DPO metric files. |
| 6 | Pre-fix focused EntityImpact reproduction | 1 | Blame-hang aborted after 120 seconds; exact test incomplete. |
| 7 | Sequence readback | 0 | Exact focused test had `Completed="False"`. |
| 8 | Test-local bounded runner patch | 0 | Added compact runner and strict EntityImpact assertions. |
| 9 | Focused trial, compact runner 4 epochs | 1 | No hang; failed DPO gate with held-out accuracy `0.56`, negative margin. |
| 10 | Focused trial, compact runner 8 epochs | 1 | No hang; failed DPO gate with held-out accuracy `0.56`, negative margin. |
| 11 | Focused trial, separation batch | 1 | No hang; failed DPO gate with held-out accuracy `0`, negative margin. |
| 12 | EntityImpact metric signal mapping patch | 0 | Added explicit chosen/rejected signal conversion. |
| 13 | Focused trial, signal mapping without interleaving | 1 | No hang; failed DPO gate with held-out accuracy `0.26`, negative margin. |
| 14 | Scoped DPO summary/diagnostic readback | 0 | Showed grouped rejected-type behavior and failed held-out DPO metrics. |
| 15 | Interleave-by-rejected-type patch | 0 | Interleaved all 50 training rows by rejected type. |
| 16 | Focused EntityImpact after interleaving | 0 | Passed 1/1 before collateral correction. |
| 17 | Bounded S8-style segment before collateral correction | 0 | Passed 9/9. |
| 18 | DesignConversion before collateral correction | 0 | Passed 247/247. |
| 19 | Diff/metric inspection | 0 | Found one collateral test context edit outside EntityImpact. |
| 20 | Collateral correction patch | 0 | Restored QueryPlanning context to `96`. |
| 21 | Focused EntityImpact after final correction | 0 | Passed 1/1. |
| 22 | Bounded S8-style segment after final correction | 0 | Passed 9/9. |
| 23 | DesignConversion after final correction | 0 | Passed 247/247. |
| 24 | Latest DPO metric/manifest readback | 0 | Held-out accuracy `1`, reward margin `0.12133178859949112`, manifest exists. |
| 25 | Process inspection | mixed | `dotnet.exe` PID 16160 remained; command-line check showed Roslyn compiler server, not a hung testhost/vstest process. No cleanup performed. |

## Stale Process Handling

No stale test process was stopped. A remaining `dotnet.exe` process was inspected and classified as the Roslyn compiler server:

```text
"C:\Program Files\dotnet\dotnet.exe" exec "C:\Program Files\dotnet\sdk\9.0.310\Roslyn\bincore\VBCSCompiler.dll" ...
```

Because it was not a proven stale Pass211 testhost/vstest process, no cleanup action was taken.

## Files Changed

| File | Pass211 change |
| --- | --- |
| `42.ProjectWikiDomain\EntityImpactMapperTrainingPipeline.cs` | Added EntityImpact DPO metric signal conversion and interleaved training-row ordering while preserving the existing runner seam and publication gate. |
| `Tests\ProjectWikiDomainTests.cs` | Injected a compact deterministic runner into the focused EntityImpact pipeline test and strengthened assertions around DPO metrics and publication. Existing prior-pass edits in this file are not claimed as Pass211 work. |
| `docs\evidence\design-conversion\codex-pass211-projectwiki-entity-impact-timeout-repair.md` | This evidence artifact. |

## Remaining Scope

Not verified in Pass211:

- Full `ProjectWikiDomainTests` class.
- Full local test suite.
- CI.
- Deployment.
- Provider execution.
- Public endpoint behavior.
- Production checkpoint readiness.
- Git publication or PR readiness.
- Product completion.

## Next Recommended Pass

Continue with the next bounded ProjectWiki/design-to-code completion segment after S8. No next exact blocker was exposed by the requested focused EntityImpact test, the bounded S8-style segment, or the DesignConversion filter in this pass.

pass211_verdict: PASS_BOUNDED_ENTITY_IMPACT_TIMEOUT_REPAIRED
