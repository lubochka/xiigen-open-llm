# Codex Pass173 - Packaging Consumer Restore Evidence

claim_scope: Pass173 proves only the bounded local/safe-worktree packaging and consumer restore boundary for DesignToCode. The proven boundary is ProjectReference-based local restore/build/test for the safe platform and FLOW consumers that resolve local llm_mvp_core ModelServer/DesignConversion projects. This is not ProductReady, cloud readiness, deployment, provider execution, public endpoint readiness, CI, source publication, commit, PR, merge, or production readiness.
evidence_scope: Fresh-read AGENTS.md, required current skills, Pass170, Pass171, Pass172, current safe platform/FLOW project files and lock files, focused locked restore/build/test command output, safe-worktree status, and optional read-only FigmaToCode/CanvaToCode client search. No source repo file, xiigen document, cloud resource, deploy route, external visual AI provider, commit, push, merge, PR, checkout, switch, reset, clean, or source branch mutation was used.
count_claimed: 1 packaging/consumer restore pass only. This is not 50 passes and not a product-completion count.
independent_evidence_count: 13 evidence groups: required skill reads; Pass170/Pass171/Pass172 evidence reads; project/reference file reads; package lock reads; platform locked restore; FLOW locked restore; platform build; FLOW build; platform focused test; FLOW focused test; lock-file hygiene repair; FLOW post-repair rerun; artifact and hygiene verification.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
downgrade_needed: No downgrade for bounded local ProjectReference consumer restore/build/test after the current commands. Downgrade remains for ProductReady, cloud/deploy/provider/public endpoint/production readiness, source repo materialization, source publication, CI, commit, PR, merge, and clean NuGet package publication because those were not run or authorized.

## skill_load_status

| File | Status |
|---|---|
| C:\Projects\llm_mvp_core\AGENTS.md | found/read |
| C:\Projects\llm_mvp_core\docs\skills\planning-session-startup-SKILL.md | found/read |
| C:\Projects\llm_mvp_core\docs\skills\claim-verification-SKILL.md | found/read |
| C:\Projects\llm_mvp_core\docs\skills\plan-review-SKILL.md | found/read |
| C:\Projects\llm_mvp_core\docs\skills\qa-session-type-SKILL.md | found/read |
| C:\Projects\llm_mvp_core\docs\skills\generated-code-review-SKILL.md | found/read |
| C:\Projects\llm_mvp_core\docs\skills\capability-design-review-SKILL.md | found/read |
| C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass171-post-pass170-final-audit.md | found/read |
| C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass172-governance-skill-repair.md | found/read |

## files_read

- C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass170-platform-flow-real-modelserver-allup.md.
- C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass171-post-pass170-final-audit.md.
- C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass172-governance-skill-repair.md.
- C:\Projects\design-to-code-worktrees\xiigen-platform-dot-net-angular-DesignToCode-pass75\tests\Xiigen.Platform.Api.Tests\Xiigen.Platform.Api.Tests.csproj.
- C:\Projects\design-to-code-worktrees\xiigen-platform-dot-net-angular-DesignToCode-pass75\tests\Xiigen.Platform.Api.Tests\packages.lock.json.
- C:\Projects\design-to-code-worktrees\xiigen-flows-dot-net-angular-DesignToCode-pass70\flows\FLOW-47-design-conversion-intake\server\Xiigen.Flow.DesignConversionIntake.Tests\Xiigen.Flow.DesignConversionIntake.Tests.csproj.
- C:\Projects\design-to-code-worktrees\xiigen-flows-dot-net-angular-DesignToCode-pass70\flows\FLOW-47-design-conversion-intake\server\Xiigen.Flow.DesignConversionIntake.Infrastructure\Xiigen.Flow.DesignConversionIntake.Infrastructure.csproj.
- C:\Projects\design-to-code-worktrees\xiigen-flows-dot-net-angular-DesignToCode-pass70\flows\FLOW-47-design-conversion-intake\server\Xiigen.Flow.DesignConversionIntake.Tests\packages.lock.json.
- Read-only optional search roots: C:\Projects\FigmaToCode-design-gen and C:\Projects\CanvaToCode.

## files_changed

- C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass173-packaging-consumer-restore.md.
- C:\Projects\design-to-code-worktrees\xiigen-flows-dot-net-angular-DesignToCode-pass70\flows\FLOW-47-design-conversion-intake\server\Xiigen.Flow.DesignConversionIntake.Tests\packages.lock.json.

No product code, source-repo, xiigen document, or Claude file edit was needed. One safe-worktree lock file was mechanically normalized from CRLF to LF after `git diff --check` reported trailing-whitespace noise on added lock-file lines. The focused build can regenerate existing package output artifacts because FLOW Infrastructure has GeneratePackageOnBuild=true; those generated package artifacts were already dirty in the safe worktree status before this pass and were not normalized.

## dependency_mechanism_found

- Platform safe consumer mechanism: ProjectReference, not a published llm_mvp_core package. The platform test project references:
  - ..\..\..\xiigen-flows-dot-net-angular-DesignToCode-pass70\flows\FLOW-47-design-conversion-intake\server\Xiigen.Flow.DesignConversionIntake.Application\Xiigen.Flow.DesignConversionIntake.Application.csproj
  - ..\..\..\xiigen-flows-dot-net-angular-DesignToCode-pass70\flows\FLOW-47-design-conversion-intake\server\Xiigen.Flow.DesignConversionIntake.Infrastructure\Xiigen.Flow.DesignConversionIntake.Infrastructure.csproj
  - ..\..\..\..\llm_mvp_core\25.ModelServer\ModelServer.csproj
  - ..\..\src\Xiigen.Platform.Api\Xiigen.Platform.Api.csproj
- FLOW safe consumer mechanism: ProjectReference for local llm_mvp_core and FLOW projects. The FLOW test project references:
  - ..\..\..\..\..\..\llm_mvp_core\25.ModelServer\ModelServer.csproj
  - ..\Xiigen.Flow.DesignConversionIntake.Application\Xiigen.Flow.DesignConversionIntake.Application.csproj
  - ..\Xiigen.Flow.DesignConversionIntake.Infrastructure\Xiigen.Flow.DesignConversionIntake.Infrastructure.csproj
- FLOW Infrastructure mechanism: ProjectReference to ..\..\..\..\..\..\llm_mvp_core\44.DesignConversionDomain\44.DesignConversionDomain.csproj plus package reference to Xiigen.Fabrics.InMemory 0.1.1.
- Lock files classify llm_mvp_core ModelServer, 44.DesignConversionDomain, and transitive llm_mvp_core projects as type Project. This is local source/reference restore evidence, not NuGet publication evidence.

## commands_run_with_exit_codes

| Command | Exit code | Result |
|---|---:|---|
| rg over memory registry for Pass171/Pass172/Pass173/DesignToCode packaging terms | 1 | No direct relevant memory registry line returned; no memory-derived product evidence used. |
| Get-Content -Raw on AGENTS.md and required docs/skills files | 0 | Required files found/read. |
| Get-Content -Raw on Pass170, Pass171, Pass172 evidence | 0 | Prior evidence found/read. |
| rg/Get-Content over platform and FLOW csproj/lock files | 0 | ProjectReference mechanism and lock Project entries identified. |
| C:\Projects\dotnet-sdk-10\dotnet.exe restore C:\Projects\design-to-code-worktrees\xiigen-platform-dot-net-angular-DesignToCode-pass75\tests\Xiigen.Platform.Api.Tests\Xiigen.Platform.Api.Tests.csproj --locked-mode -p:NuGetAudit=false | 0 | Locked restore succeeded and restored platform, FLOW, ModelServer, DesignConversionDomain, and transitive local projects. |
| C:\Projects\dotnet-sdk-10\dotnet.exe restore C:\Projects\design-to-code-worktrees\xiigen-flows-dot-net-angular-DesignToCode-pass70\flows\FLOW-47-design-conversion-intake\server\Xiigen.Flow.DesignConversionIntake.Tests\Xiigen.Flow.DesignConversionIntake.Tests.csproj --locked-mode -p:NuGetAudit=false | 0 | Locked restore succeeded; 21 of 22 projects were already up-to-date. |
| C:\Projects\dotnet-sdk-10\dotnet.exe build C:\Projects\design-to-code-worktrees\xiigen-platform-dot-net-angular-DesignToCode-pass75\tests\Xiigen.Platform.Api.Tests\Xiigen.Platform.Api.Tests.csproj --no-restore -p:NuGetAudit=false | 0 | Build succeeded with 10 pre-existing analyzer warnings from llm_mvp_core SemanticCompiler and a package readme warning; 0 errors. |
| C:\Projects\dotnet-sdk-10\dotnet.exe build C:\Projects\design-to-code-worktrees\xiigen-flows-dot-net-angular-DesignToCode-pass70\flows\FLOW-47-design-conversion-intake\server\Xiigen.Flow.DesignConversionIntake.Tests\Xiigen.Flow.DesignConversionIntake.Tests.csproj --no-restore -p:NuGetAudit=false | 0 | Build succeeded with 0 warnings and 0 errors. |
| C:\Projects\dotnet-sdk-10\dotnet.exe test C:\Projects\design-to-code-worktrees\xiigen-platform-dot-net-angular-DesignToCode-pass75\tests\Xiigen.Platform.Api.Tests\Xiigen.Platform.Api.Tests.csproj --no-build --no-restore --filter FullyQualifiedName~SubmitExplicitBrowserBackedPromotedLocalCompositionInvokesRealFlowAndRealModelServerWithPromotedCheckpoint --logger "console;verbosity=minimal" -p:NuGetAudit=false | 0 | 1 passed, 0 failed, 0 skipped. |
| C:\Projects\dotnet-sdk-10\dotnet.exe test C:\Projects\design-to-code-worktrees\xiigen-flows-dot-net-angular-DesignToCode-pass70\flows\FLOW-47-design-conversion-intake\server\Xiigen.Flow.DesignConversionIntake.Tests\Xiigen.Flow.DesignConversionIntake.Tests.csproj --no-build --no-restore --filter FullyQualifiedName~ExplicitBrowserBackedPromotedLocalCompositionInvokesRealModelServerViaFlow --logger "console;verbosity=minimal" -p:NuGetAudit=false | 0 | 1 passed, 0 failed, 0 skipped. |
| git diff --check over focused FLOW changed project/test/lock files | 1 before repair | Reported trailing-whitespace warnings only in FLOW test packages.lock.json added lines. |
| Mechanical LF normalization of C:\Projects\design-to-code-worktrees\xiigen-flows-dot-net-angular-DesignToCode-pass70\flows\FLOW-47-design-conversion-intake\server\Xiigen.Flow.DesignConversionIntake.Tests\packages.lock.json | 0 | Rewrote only that safe-worktree lock file from CRLF to LF. |
| C:\Projects\dotnet-sdk-10\dotnet.exe restore C:\Projects\design-to-code-worktrees\xiigen-flows-dot-net-angular-DesignToCode-pass70\flows\FLOW-47-design-conversion-intake\server\Xiigen.Flow.DesignConversionIntake.Tests\Xiigen.Flow.DesignConversionIntake.Tests.csproj --locked-mode -p:NuGetAudit=false | 0 after repair | All projects up-to-date for restore. |
| C:\Projects\dotnet-sdk-10\dotnet.exe build C:\Projects\design-to-code-worktrees\xiigen-flows-dot-net-angular-DesignToCode-pass70\flows\FLOW-47-design-conversion-intake\server\Xiigen.Flow.DesignConversionIntake.Tests\Xiigen.Flow.DesignConversionIntake.Tests.csproj --no-restore -p:NuGetAudit=false | 0 after repair | Build succeeded with 0 warnings and 0 errors. |
| C:\Projects\dotnet-sdk-10\dotnet.exe test C:\Projects\design-to-code-worktrees\xiigen-flows-dot-net-angular-DesignToCode-pass70\flows\FLOW-47-design-conversion-intake\server\Xiigen.Flow.DesignConversionIntake.Tests\Xiigen.Flow.DesignConversionIntake.Tests.csproj --no-build --no-restore --filter FullyQualifiedName~ExplicitBrowserBackedPromotedLocalCompositionInvokesRealModelServerViaFlow --logger "console;verbosity=minimal" -p:NuGetAudit=false | 0 after repair | 1 passed, 0 failed, 0 skipped. |
| rg for DesignConversion/FLOW/ModelServer terms in C:\Projects\FigmaToCode-design-gen | 0 | FigmaToCode uses /conversion/submit and local ModelServer improvement-loop client/test paths. |
| rg for DesignConversion/FLOW/ModelServer terms in C:\Projects\CanvaToCode | 0 | CanvaToCode uses /conversion/submit and local ModelServer improvement-loop client/test paths. |

## restore_build_test_results

- Platform locked restore: PASS.
- FLOW locked restore: PASS.
- Platform focused build: PASS with warnings, 0 errors. The warnings are analyzer warnings in llm_mvp_core SemanticCompiler and a package-readme warning, not restore/reference failures.
- FLOW focused build: PASS, 0 warnings, 0 errors before and after the lock-file LF normalization.
- Platform focused test: PASS, 1/1.
- FLOW focused test: PASS, 1/1 before and after the lock-file LF normalization.

The commands prove the safe platform and FLOW consumers can currently resolve the local llm_mvp_core ModelServer/DesignConversion dependency graph through ProjectReference, build against it, and execute the focused real ModelServer harness tests.

## optional_client_classification

- FigmaToCode-design-gen contains a DesignConversionClient that calls /conversion/submit and tests that launch C:\Projects\llm_mvp_core\tools\DesignConversionLiveLocalServer\DesignConversionLiveLocalServer.csproj for live-local ModelServer integration.
- CanvaToCode contains a DesignConversionClient that calls /conversion/submit and tests that launch the same llm_mvp_core live-local server project for local ModelServer integration.
- These optional clients are TypeScript/JS consumers of the platform/ModelServer HTTP API. They do not consume the .NET ProjectReference graph directly. Therefore this Pass173 local .NET packaging restore proof affects them indirectly by proving the server/platform/FLOW side can restore/build/test; it does not prove their npm install, UI runtime, browser flow, or live-local JS tests.

## mental_simulation_result

A future agent could falsely claim product done if the platform/FLOW tests passed earlier but the consumer project graph no longer restored. Pass173 closes that narrow local risk by running locked restore before build/test on both focused consumers. Because both locked restores, both builds, and both exact focused tests pass, package/reference restore no longer blocks bounded local/safe-worktree use of the Pass170 route.

The proof is still intentionally narrow. It does not prove clean NuGet publication, source repo materialization, CI, cloud deployment, public endpoints, external provider behavior, production browser farm readiness, or ProductReady. It proves the current safe-worktree local ProjectReference consumer boundary is usable.

## remaining_gaps

- ProductReady/cloud/deploy/public endpoint/provider/production readiness: not run, not authorized, not proven.
- Source repo materialization into C:\Projects\xiigen-platform-dot-net-angular or C:\Projects\xiigen-flows-dot-net-angular: not run, forbidden, not proven.
- Git commit/push/merge/PR/checkout/switch/reset/clean: not run.
- Clean NuGet package publication of llm_mvp_core ModelServer/DesignConversion: not proven; current mechanism is ProjectReference.
- Broad solution-wide test sweeps and CI: not run.
- Optional FigmaToCode/CanvaToCode npm install/UI/live-local tests: not run; only read-only API consumer classification was performed.
- Safe worktrees remain broadly dirty from earlier authorized passes; Pass173 did not normalize dirty package artifacts or tmp directories. Pass173 only normalized the focused FLOW test packages.lock.json line endings because `git diff --check` failed on that file.

## verification_results

- Evidence artifact schema/marker scan command exited 0 and returned: identity-field count 1, verdict-field count 1, conflict-marker count 0, mojibake-marker count 0.
- Evidence artifact readback command exited 0 and displayed the current English sections without visible mojibake.
- `git diff --check -- docs/evidence/design-conversion/codex-pass173-packaging-consumer-restore.md` exited 0.
- `git diff --no-index --check -- NUL C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass173-packaging-consumer-restore.md` exited 1 as expected for an untracked-vs-NUL no-index diff and produced no whitespace-warning output.
- Platform focused changed-file `git diff --check` exited 0.
- FLOW focused changed-file `git diff --check` exited 0 after the lock-file LF normalization.
- Marker scan over this evidence artifact plus focused platform/FLOW project and lock files exited 1 with no matches, meaning no conflict marker or configured mojibake marker was found.

pass173_verdict: PASS_PACKAGING_CONSUMER_RESTORE_BOUNDED_LOCAL
