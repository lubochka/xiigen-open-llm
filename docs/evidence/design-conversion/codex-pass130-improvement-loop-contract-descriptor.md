# Pass130 Improvement Loop Contract Descriptor

claim_scope:
- Added one bounded local contract/schema descriptor surface for the Pass129 in-process DesignConversion improvement-loop route.
- Added tests proving the descriptor is discoverable, route-specific, local-only, refs-only, and separate from the existing evaluate-route descriptor.
- This proves only local contract metadata and focused tests for the optional in-repo route and local caller envelope fields.
- It does not prove public HTTP hosting, socket hosting, ASP.NET/minimal API startup, external caller repo wiring, external visual AI/provider calls, production DB, upload/deployment, trained weights, or full product readiness.

evidence_scope:
- Current workspace: `C:\Projects\llm_mvp_core`.
- Direct evidence: `25.ModelServer/DesignConversion/DesignConversionModelServerCallerContract.cs`, `Tests/DesignConversionModelServerImprovementLoopContractTests.cs`, and this Pass130 evidence artifact.
- Evidence-only source read: `docs/evidence/design-conversion/codex-pass129-modelserver-improvement-loop-surface.md`.
- No external `C:\Users\lubao\Documents\xiigen` write was made.
- No `claude-*` file was read or edited for this pass.

count_claimed:
- 1 bounded local contract descriptor implementation pass: Pass130 only.
- 0 public-host passes.
- 0 external-caller passes.
- 0 real-provider passes.
- 0 product-readiness passes.

independent_evidence_count:
- 1 Pass130 evidence artifact: this file.
- 2 successful focused test commands: new Pass130 contract tests and adjacent ModelServer contract/improvement-loop tests.
- 1 direct grep/readback evidence set for the descriptor symbols.

subagent_id_or_NONE:
- UNKNOWN_IN_SUBAGENT

subagent_identity_self_check:
- spawned_worker=true; used_NONE=false; parent_must_reconcile=true

downgrade_needed:
- No downgrade for the narrow local contract descriptor/test claim after the listed focused tests passed.
- Downgrade remains required for any public HTTP, external caller, external provider, deployment, trained model, or product-readiness claim.

skill_load_status:
- AGENTS.md: found/read.
- CLAUDE.md: found/read.
- docs/SESSION-START-PROMPT.md: found/read.
- docs/skills/planning-session-startup-SKILL.md: found/read.
- docs/skills/architect-behavior-classifier-SKILL.md: found/read.
- docs/skills/authority-requirement-binding-SKILL.md: found/read.
- docs/skills/reference-inventory-and-source-map-SKILL.md: found/read.
- docs/skills/phase-planning-SKILL.md: found/read.
- docs/skills/codex-planning-quality-GUIDE.md: found/read.
- docs/skills/plan-review-SKILL.md: found/read.
- docs/skills/goal-delivery-completeness-SKILL.md: found/read.
- docs/skills/claim-verification-SKILL.md: found/read.
- docs/skills/audit-protocol-SKILL.md: found/read.
- docs/skills/specificity-calibration-SKILL.md: found/read.
- docs/skills/output-skepticism-SKILL.md: found/read.
- docs/skills/reconnaissance-gate-SKILL.md: found/read.
- docs/skills/trainable-architecture-decision-SKILL.md: found/read.
- docs/skills/no-product-decisions-SKILL.md: found/read.
- docs/skills/output-readability-gate-SKILL.md: found/read.
- docs/skills/design-conversion-ilearningmachine-architecture-GUIDE.md: found/read.
- docs/skills/qa-session-type-SKILL.md: found/read.
- docs/skills/html-css-render-qa-automation-GUIDE.md: found/read.
- docs/skills/generated-code-review-SKILL.md: found/read.

files_read:
- `AGENTS.md`
- `CLAUDE.md`
- `docs/SESSION-START-PROMPT.md`
- Required `docs/skills/*` stack listed above.
- `25.ModelServer/DesignConversion/DesignConversionModelServerCallerContract.cs`
- `25.ModelServer/DesignConversion/DesignConversionModelServerImprovementLoopHandler.cs`
- `25.ModelServer/DesignConversion/DesignConversionModelServerEndpointRegistry.cs`
- `25.ModelServer/DesignConversion/DesignConversionModelServerLocalCallerAdapter.cs`
- `Tests/DesignConversionModelServerCallerContractTests.cs`
- `Tests/DesignConversionModelServerImprovementLoopTests.cs`
- `docs/evidence/design-conversion/codex-pass129-modelserver-improvement-loop-surface.md`

files_changed:
- `25.ModelServer/DesignConversion/DesignConversionModelServerCallerContract.cs`
- `Tests/DesignConversionModelServerImprovementLoopContractTests.cs`
- `docs/evidence/design-conversion/codex-pass130-improvement-loop-contract-descriptor.md`

commands_run:
- `rg -n "Pass129|DesignConversion|improvement-loop|local caller|PASS27_ORIGINAL_TASK_COVERAGE_AND_LATEST_BASELINE_RECONCILIATION_GATE|Sub-agent error is recovery queue" C:\Users\lubao\.codex\memories\MEMORY.md`
- Required skill/context `Get-Content -Raw` reads before coding.
- `git branch --show-current; git status --short`
- `Get-Content -Raw` for the requested context files and Pass129 evidence artifact.
- `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "DisplayName~DesignConversionModelServerImprovementLoopContractTests" --logger "console;verbosity=minimal"`
- `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "DisplayName~DesignConversionModelServerCallerContractTests|DisplayName~DesignConversionModelServerImprovementLoopTests|DisplayName~DesignConversionModelServerImprovementLoopContractTests" --logger "console;verbosity=minimal"`
- `rg -n "DescribeImprovementLoopRoute|ImprovementLoopRouteSchemaVersion|ImprovementLoopBoundaryMetadata|DesignConversionModelServerImprovementLoopContractTests" 25.ModelServer\DesignConversion\DesignConversionModelServerCallerContract.cs Tests\DesignConversionModelServerImprovementLoopContractTests.cs`
- Conflict-marker regex scan over changed files, without embedding the raw marker strings in this evidence artifact.
- Configured mojibake-marker scan over changed files.
- `rg -n "[ \t]+$" -- <changed files>`
- `git diff --check -- <changed files>`
- `Get-Content` readback over changed descriptor, test, and evidence text.

tests_run:
- Passed: 4/4 `DesignConversionModelServerImprovementLoopContractTests`.
- Passed: 13/13 adjacent `DesignConversionModelServerCallerContractTests`, `DesignConversionModelServerImprovementLoopTests`, and `DesignConversionModelServerImprovementLoopContractTests`.
- Full test suite was not run.

implementation_summary:
- Added `DesignConversionModelServerImprovementLoopContractDescriptor`.
- Added `DesignConversionModelServerCallerContract.DescribeImprovementLoopRoute()`.
- Added route-specific schema version and boundary constants for the local improvement-loop descriptor.
- Added route request fields: `loopId`, `initialPackage`, `maxAttempts`, and optional `metadata`.
- Added response fields for endpoint/method/seam/status, loop stop/status/attempt metadata, refs-only evidence, warnings, and boundary metadata raw-payload flags.
- Added local caller envelope fields to the improvement-loop descriptor by reusing the existing local success/error envelope field descriptor.
- Added validation failures for coordinator-not-configured, request body required, invalid JSON, max attempts required, initial package required, and package field failures.
- Added boundary metadata flags keeping public host, cross-repo caller, socket, deployment, product-ready, and external-provider claims false.
- Added focused Pass130 tests for local route discoverability, required fields, response/envelope no-raw policy, validation failures, and evaluate descriptor separation.

mental_simulation_results:
- Descriptor points to `POST /api/design-conversion/improvement-loop` and declares the route local-only: covered by `DescribeImprovementLoopRoute_DocumentsLocalOnlyRouteAndRequiredFields`.
- Descriptor required fields match handler expectations: covered by assertions for `loopId`, `initialPackage`, and `maxAttempts`.
- Descriptor response fields include loop stop/status/attempt metadata and refs-only boundary metadata: covered by response-field assertions for `contractStatus`, `stopReason`, `attemptCount`, `attempts`, and boundary metadata fields.
- Descriptor metadata keeps public/cross-repo/socket/deployment/product-ready/external-provider false: covered by boundary metadata assertions.
- Existing evaluate descriptor remains stable and is not silently repurposed as the loop descriptor: covered by `DescribeImprovementLoopRoute_KeepsEvaluateDescriptorStableAndSeparate`.
- No raw design tree, HTML, CSS, or private payload example was added to the descriptor; raw input can exist only inside request fields marked non-echoing.

verification_results:
- Focused Pass130 contract tests passed: 4 passed, 0 failed.
- Adjacent ModelServer caller-contract/improvement-loop tests passed: 13 passed, 0 failed.
- Conflict-marker scan over changed files: no matches.
- Mojibake-marker scan over changed files using the configured marker set from the task: no matches.
- Direct trailing-whitespace scan over changed files: no matches.
- `git diff --check -- <changed files>` exited 0. Note: the touched ModelServer contract file sits under a pre-existing untracked directory in this dirty worktree, so the direct trailing-whitespace scan is the meaningful whitespace check for that file.
- Changed human-readable descriptor, test, and evidence text was read back after edits; no visible mojibake or broken encoding was present in the readback.

remaining_gaps:
- `asp_net_host_verified=false`
- `socket_opened=false`
- `public_http_endpoint_verified=false`
- `cross_repo_caller_verified=false`
- `external_provider_called=false`
- `deployment_verified=false`
- `product_ready=false`
- No public/socket host was started.
- No external caller repo was edited.
- No real external visual AI/network/provider call was made.
- No production DB/upload/deployment was attempted.
- No full product readiness claim is made.

next_single_pass_recommendation:
- If Luba continues this chain, the next bounded pass can inspect whether the local caller adapter should consume `DescribeImprovementLoopRoute()` directly instead of its private descriptor, still without broadening into public HTTP or external callers.
