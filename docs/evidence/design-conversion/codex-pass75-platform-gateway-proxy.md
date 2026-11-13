# Codex Pass75 Platform Gateway Proxy

platform_gateway_proxy: true
authority_branch: origin/main
DesignToCode-pass75: true
conversion_submit: true
figma_payload: true
canva_payload: true
FLOW-47: true
upstream_unavailable: true
mergeability: true
product_not_ready: true
remaining_queue: true

claim_scope: Pass75 inspected the platform authority branch read-only, created the safe platform worktree `C:\Projects\design-to-code-worktrees\xiigen-platform-dot-net-angular-DesignToCode-pass75`, repaired the platform `/conversion/submit` gateway/proxy contract so Figma/Canva caller fields are preserved into the FLOW-47 dispatch boundary, added focused platform API tests, ran the focused API test project, ran `git diff --check`, and ran a no-ref dirty-snapshot mergeability check to `origin/main`. This proves the platform gateway/proxy contract slice only. It is not product readiness, not real end-user Figma/Canva e2e, not visual-provider readiness, not training readiness, and not merge/commit readiness.

evidence_scope: Current-pass skill/guidance read-status commands, Pass70/71/73/74 evidence reads, platform source repo fetch/status/branch inspection, safe worktree creation, platform route/client/model/test reads, code/test edits in the safe worktree, direct readback marker searches, focused `dotnet test` output, `git diff --check`, source-worktree status check, no-ref `git merge-tree` dirty-snapshot mergeability output, and this evidence file.

count_claimed: 1 sequential Pass75 worker slice, 1 safe platform worktree, 1 focused API test-project run with 17/17 tests passed, 1 `git diff --check` pass, 1 dirty-snapshot mergeability probe. 0 commits, 0 pushes, 0 PRs, 0 actual merges, 0 source worktree branch switches, 0 writes under `C:\Users\lubao\Documents\xiigen`, 0 `claude-*` edits, 0 real provider/S3/model-upload calls, 0 live browser checks.

independent_evidence_count: 15 current evidence groups: memory/governance lookup, skill read-status output, four prior-pass evidence reads, platform authority fetch/status table, safe worktree creation output, route/proxy/test source reads, code/test diff, focused API test output, `git diff --check`, direct readback markers, source worktree status, dirty-snapshot mergeability check, process-safety check, final dirty status/stat.

subagent_id_or_NONE: Pass75

downgrade_needed: yes. The evidence supports only platform gateway/proxy contract preservation and upstream-unavailable handling. `product_not_ready` remains true.

## skill_load_status

All required local files under `C:\Projects\llm_mvp_core` were found and read by current-pass `Get-Content -Raw`/read-status commands before platform edits.

| group | files | status |
| --- | --- | --- |
| SK-SESSION | `AGENTS.md`; `docs/SESSION-START-PROMPT.md`; `docs/skills/planning-session-startup-SKILL.md` | found/read |
| SK-INTENT | `docs/skills/architect-behavior-classifier-SKILL.md` | found/read |
| SK-PLAN | `authority-requirement-binding`; `reference-inventory-and-source-map`; `phase-planning` | found/read |
| SK-REVIEW | `plan-review`; `goal-delivery-completeness`; `claim-verification`; `audit-protocol`; `specificity-calibration`; `output-skepticism`; `reconnaissance-gate` | found/read |
| SK-ARCH | `trainable-architecture-decision`; `no-product-decisions`; `output-readability-gate` | found/read |
| Executor review | `implementation-integrity`; `generated-code-review` | found/read |

## repo_authority_table

`git fetch --prune` in the source platform repo exited 0. Authority selection rule was latest updated branch ahead of default if present; otherwise main/default. No remote branch was ahead of default.

| repo | source worktree state | default/main | recent remote branches | authority_branch | action |
| --- | --- | --- | --- | --- | --- |
| `C:\Projects\xiigen-platform-dot-net-angular` | `UpdateUniversalSkills`, existing untracked `evidence/`; not edited or switched | `origin/main` `34936965e2be0e26735469b831826d19bbf31d6a`, 2026-06-29T16:34:28+03:00, `Merge pull request #2 from lubochka/UpdateUniversalSkills` | `origin/main` ahead 0; `origin/UpdateUniversalSkills` ahead 0 behind 1; `origin/DesignToCode` ahead 0 behind 13 | `origin/main` | read/fetch/status only |

## worktree_branch_used

```text
path: C:\Projects\design-to-code-worktrees\xiigen-platform-dot-net-angular-DesignToCode-pass75
branch: DesignToCode-pass75
base: origin/main 34936965e2be0e26735469b831826d19bbf31d6a
base_date: 2026-06-29T16:34:28+03:00
source_branch_mutated: false
```

## repair_performed

The platform already exposed `/conversion/submit` and `/conversion/jobs/{jobId}` on `origin/main`, but it did not preserve the complete Pass74 caller payload shape. The repair keeps the platform route and FLOW-47 dispatch boundary minimal:

- `DesignConversionSubmitRequest` now accepts `originalTree`, `designScreenshots`, flexible metadata values, and `sourceTree` `mediaType`/`encoding`.
- `Flow47DesignConversionJobRequest` now carries `OriginalTree`, `DesignScreenshots`, and flexible metadata through the platform service.
- `HttpFlow47DesignConversionClient` now sends caller-compatible top-level FLOW-47 fields: `sourceKind`, `html`, `css`, `sourceTree`, `originalTree`, `screenshots`, `designScreenshots`, `metadata`, `tier`, `loopBudget`, and `userId`, while retaining the legacy `sourceType/sourceUri/outputTargets/options` shape.
- Missing or unreachable FLOW-47 now returns `upstream_unavailable` through platform submit/status failure handling, mapped to HTTP 503, rather than fake conversion success.
- Focused tests were added for Figma payload preservation, Canva payload preservation, upstream unavailable submit behavior, and HTTP request body preservation toward FLOW-47.

## direct_readback

Readback marker searches after edits:

```text
src/Xiigen.Platform.Api/DesignConversion/DesignConversionEndpointModels.cs:
11 OriginalTree
13 DesignScreenshots
20 Metadata as IReadOnlyDictionary<string, object?>
27 MediaType
28 Encoding
168 ToUpstreamAwareFailureStatus

src/Xiigen.Platform.Api/DesignConversion/Flow47DesignConversion.cs:
14 UpstreamUnavailableCode = "upstream_unavailable"
54 OriginalTree
56 DesignScreenshots
192-198 source validation includes originalTree/designScreenshots
408/455 HttpRequestException handling
499-502 originalTree/designScreenshots copied into options
517-519 originalTree/designScreenshots copied into top-level FLOW-47 submit body
640/658/667 metadata string helper for sourceUri/outputTargets

src/Xiigen.Platform.Api/Program.cs:
443 conversion.MapPost("/submit")
496 ToUpstreamAwareFailureStatus for submit failure
510 conversion.MapGet("/jobs/{jobId}")
528 ToUpstreamAwareFailureStatus for status failure

tests/Xiigen.Platform.Api.Tests/DesignConversionEndpointTests.cs:
118 SubmitFigmaPayloadPreservesCallerContractForFlow47Boundary
153 SubmitCanvaPayloadPreservesCallerContractForFlow47Boundary
186 SubmitWithoutFlow47BaseUrlReturnsUpstreamUnavailable
300 sourceKind top-level FLOW-47 body assertion
303 originalTree top-level FLOW-47 body assertion
305 designScreenshots top-level FLOW-47 body assertion
309-310 originalTree/designScreenshots options assertions
661 CreateFigmaPayload
721 CreateCanvaPayload
855/867 flexible metadata assertions
```

## verification

Focused API test command:

```powershell
$env:DOTNET_ROOT='C:\Users\lubao\.dotnet'
$env:PATH='C:\Users\lubao\.dotnet;' + $env:PATH
dotnet test tests\Xiigen.Platform.Api.Tests\Xiigen.Platform.Api.Tests.csproj --logger "console;verbosity=normal"
```

Result:

```text
selected SDK: 10.0.300 from C:\Users\lubao\.dotnet\sdk\10.0.300
Test Run Successful.
Total tests: 17
Passed: 17
Total time: 8.2844 Seconds
```

Relevant newly proved tests:

```text
Passed SubmitFigmaPayloadPreservesCallerContractForFlow47Boundary
Passed SubmitCanvaPayloadPreservesCallerContractForFlow47Boundary
Passed SubmitWithoutFlow47BaseUrlReturnsUpstreamUnavailable
Passed HttpFlowClientPostsFlow47SubmitContractHeadersAndMapsStatus
```

Hygiene:

```text
git diff --check
result: exit 0, no output
```

Build side effect boundary:

```text
dotnet test changed tracked package artifacts in the safe worktree:
artifacts/packages/Xiigen.Fabrics.InMemory.0.1.1.nupkg
artifacts/packages/Xiigen.Kernel.0.1.1.nupkg
artifacts/packages/Xiigen.Platform.Marketplace.0.1.0.nupkg
No forbidden reset/restore/delete cleanup was performed.
```

Process cleanup:

```text
No Start-Process or external live server was used in Pass75.
WebApplicationFactory/TestServer instances started and shut down inside dotnet test.
No Xiigen.Platform.Api process was present in the process check.
Existing dotnet processes were observed before/around the run and were not claimed as platform server processes.
```

## mergeability_check

No actual merge, checkout/switch, rebase, reset, clean, restore, commit, push, or PR was performed.

Method: `git fetch --prune`, temp `GIT_INDEX_FILE`, `git read-tree HEAD`, hash/index the 7 dirty tracked paths, `git write-tree`, no-ref `git commit-tree -p HEAD`, and `git merge-tree --write-tree --messages origin/main synthetic_commit`.

```text
target: origin/main
authority_branch: origin/main
main/default: origin/main
changed_paths: 7
synthetic_tree: 8797f1376631cdc888fe8f28960c32198afa992b
synthetic_commit: 179ab721e7464c88d335c32b1a94fe77fd13387a
merge_exit: 0
merge_output: 8797f1376631cdc888fe8f28960c32198afa992b
conflict_lines: 0
mergeability: clean into origin/main and main/default
```

## mental_simulation

figma_payload:

1. Figma caller posts platform `/conversion/submit` with headers `X-Roles=tenant_ops`, `X-Tenant-Id`, `X-User-Id`, `X-Design-Tier=starter`, `X-Loop-Budget=2`, and idempotency key.
2. Platform minimal API authorizes the tenant role, validates tier/budget, builds a `DesignConversionSubmitCommand`, and preserves `sourceKind=figma`, `html`, `css`, `sourceTree`, `originalTree`, `screenshots`, `designScreenshots`, metadata, tier, loop budget, user, tenant, and idempotency context.
3. Platform `HttpFlow47DesignConversionClient` posts to configured FLOW-47 base URL using `/flow-47/design-conversion/jobs` after normalizing older route settings. The body includes both legacy fields and caller-compatible top-level fields accepted by FLOW-47 Pass74.
4. If FLOW-47 accepts, platform maps job id/status/current output/verdict/warnings/evidence back to platform response. If FLOW-47 is missing/unreachable/non-success, platform returns `upstream_unavailable` with HTTP 503 and no fake conversion success.

canva_payload:

1. Canva caller posts platform `/conversion/submit` with `sourceKind=canva`, generated HTML/CSS, `sourceTree` entries including `canva-selection.json`, `originalTree`, empty screenshot arrays, metadata including screenshot-unavailable reason, tier, user, and loop budget.
2. Platform accepts and preserves the Canva payload through the fake-client boundary in tests and through the HTTP FLOW-47 submit body shape.
3. Empty Canva screenshot arrays remain honest evidence rather than invented screenshots.
4. Remaining unproven: real Canva adapter submission, real platform process to live FLOW-47 process, real browser screenshot/render comparison, real visual provider, and trained model output.

## remaining_queue

remaining_queue:

1. Decide whether tracked package artifacts generated by `dotnet test` should be accepted in a later authorized integration branch or cleaned by an authorized cleanup pass.
2. Run an optional live two-process probe only when a bounded pass authorizes starting platform and FLOW-47 local hosts together; Pass75 used focused API tests, not live host processes.
3. Run real Figma/Canva app-to-platform submission only after credentials/host route are available; current proof is platform API/test boundary.
4. Keep real visual provider/browser screenshots/training evidence separate; this pass proves gateway/proxy contract preservation only.

## files_changed

Safe platform worktree:

```text
C:\Projects\design-to-code-worktrees\xiigen-platform-dot-net-angular-DesignToCode-pass75\src\Xiigen.Platform.Api\DesignConversion\DesignConversionEndpointModels.cs
C:\Projects\design-to-code-worktrees\xiigen-platform-dot-net-angular-DesignToCode-pass75\src\Xiigen.Platform.Api\DesignConversion\Flow47DesignConversion.cs
C:\Projects\design-to-code-worktrees\xiigen-platform-dot-net-angular-DesignToCode-pass75\src\Xiigen.Platform.Api\Program.cs
C:\Projects\design-to-code-worktrees\xiigen-platform-dot-net-angular-DesignToCode-pass75\tests\Xiigen.Platform.Api.Tests\DesignConversionEndpointTests.cs
C:\Projects\design-to-code-worktrees\xiigen-platform-dot-net-angular-DesignToCode-pass75\artifacts\packages\Xiigen.Fabrics.InMemory.0.1.1.nupkg
C:\Projects\design-to-code-worktrees\xiigen-platform-dot-net-angular-DesignToCode-pass75\artifacts\packages\Xiigen.Kernel.0.1.1.nupkg
C:\Projects\design-to-code-worktrees\xiigen-platform-dot-net-angular-DesignToCode-pass75\artifacts\packages\Xiigen.Platform.Marketplace.0.1.0.nupkg
```

Codex evidence:

```text
C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass75-platform-gateway-proxy.md
```

## risk_boundary

product_not_ready: true.

No writes were made under `C:\Users\lubao\Documents\xiigen`. No `claude-*` file was edited. The source authority worktree `C:\Projects\xiigen-platform-dot-net-angular` was not edited or switched. No commit, push, PR, checkout/switch source worktree, actual merge, rebase, reset, clean, delete, restore, external AI call, real S3 call, model upload, browser check, or real Figma/Canva app e2e was performed.
