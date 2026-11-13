# Codex Pass70 External Host API Integration

external_host_api: true

claim_scope: Pass70 created one fresh DesignToCode-pass70 worktree for `xiigen-flows-dot-net-angular`, implemented a bounded FLOW-47 host/API contract bridge for Figma/Canva caller payloads, and wrote this evidence artifact. This is not product readiness, not real external visual AI, not real S3 approval, not model upload, not a browser screenshot claim, and not a 50-pass claim.
evidence_scope: Current-pass file reads, required skill file read-status hashes, read-only Claude guidance hashes, `git fetch --prune` and authority-branch inspection for four repos, code/schema/manifest edits in one fresh flows worktree, JSON parse checks, `git diff --check`, marker/readback searches, failed focused `dotnet test` command with exact SDK blocker, and no-ref synthetic merge-tree checks against the touched repo authority/default target.
count_claimed: 1 sequential Pass70 implementation slice, 1 new safe worktree branch, 1 attempted focused test command blocked before test execution, 1 no-ref dirty-snapshot mergeability check, 1 evidence artifact. 0 commits, 0 pushes, 0 PRs, 0 actual merges, 0 branch switches in source worktrees, 0 product readiness claims.
independent_evidence_count: 11 current-pass evidence groups: skill load/read hashes, Claude guidance read-only hashes, Pass60/64/68/69 reads, fetch/status table, authority branch table, new worktree creation output, changed-file diff/readback, JSON parse output, diff-check output, dotnet SDK blocker output, synthetic merge-tree output.
subagent_id_or_NONE: Pass70
downgrade_needed: yes. product_not_ready remains true; focused tests did not execute because the required .NET SDK is missing.

## skill_load_status

All required local skill/guidance files were present and read with `Get-Content -Raw`; short SHA-256 prefixes were recorded in the terminal output.

| group | files | status |
| --- | --- | --- |
| SK-SESSION | `AGENTS.md`, `docs/SESSION-START-PROMPT.md`, `docs/skills/planning-session-startup-SKILL.md` | found/read |
| SK-INTENT | `docs/skills/architect-behavior-classifier-SKILL.md` | found/read |
| SK-PLAN | `authority-requirement-binding`, `reference-inventory-and-source-map`, `phase-planning` | found/read |
| SK-REVIEW | `plan-review`, `goal-delivery-completeness`, `claim-verification`, `audit-protocol`, `specificity-calibration`, `output-skepticism`, `reconnaissance-gate` | found/read |
| SK-ARCH | `trainable-architecture-decision`, `no-product-decisions`, `output-readability-gate` | found/read |
| Claude guidance | `C:\Users\lubao\Documents\xiigen\llm mvp\design to code session\*.md` | read-only guidance read; no writes |

## repo_authority_table

authority_branch is the latest branch ahead of default when one exists; otherwise default/main.

| repo | source worktree state | default/main | authority_branch | action |
| --- | --- | --- | --- | --- |
| `xiigen-platform-dot-net-angular` | `UpdateUniversalSkills`, 1 untracked `evidence/` entry; not edited | `origin/main` `34936965e2be`, 2026-06-29T16:34:28+03:00 | `origin/main` `34936965e2be`, ahead 0 | read/fetch only |
| `xiigen-flows-dot-net-angular` | `UpdateUniversalSkills`, clean; not edited | `origin/main` `6579ffb12dfb`, 2026-06-29T16:35:46+03:00 | `origin/main` `6579ffb12dfb`, ahead 0 | fresh worktree created |
| `FigmaToCode-design-gen` | current branch `DesignToCode`, clean; not edited | `origin/main` `a28066597084`, 2026-03-12T22:07:52+02:00 | `origin/DesignToCode` `77c381621d2c`, ahead 112 | read/fetch only |
| `CanvaToCode` | current branch `DesignToCode`, clean; not edited | `origin/main` `d0a6ebc05540`, 2026-05-12T10:16:15+03:00 | `origin/DesignToCode` `2fa6629c9a1e`, ahead 1 | read/fetch only |

## worktree_branch_used

DesignToCode worktree used:

```text
path: C:\Projects\design-to-code-worktrees\xiigen-flows-dot-net-angular-DesignToCode-pass70
branch: DesignToCode-pass70
base: origin/main 6579ffb12dfb8cd7822fa7e1561fd03980df2fea
base_date: 2026-06-29T16:35:46+03:00
source_branch_mutated: false
```

Existing dirty `C:\Projects\xiigen-flows-dot-net-angular-DesignToCode-s1b` was inspected but not edited.

## repair_performed

Implemented a bounded FLOW-47 host/API contract bridge:

- `DesignConversionSubmitJobRequest` now exposes top-level `sourceKind`, `html`, `css`, `sourceTree`, `originalTree`, `screenshots`, `designScreenshots`, `metadata`, `tier`, `loopBudget`, and `userId` while retaining legacy `sourceType/sourceUri/options`.
- `DesignConversionIntakeService` computes an effective source kind/type, allows inline payloads without `sourceUri`, mirrors top-level fields into the existing core dispatch options, and still routes through `IDesignConversionCoreClient`.
- `Program.cs` adds caller aliases `/conversion/submit` and `/conversion/jobs/{jobId}`, parses `X-Roles`, and mirrors `X-Design-Tier`, `X-Loop-Budget`, `X-User-Id`, `Idempotency-Key`, and `X-Idempotency-Key` into the request.
- Contract schema, endpoint registry, auth policy registry, and `flow.manifest.json` now expose the caller-compatible payload and `tenant_ops` role without claiming real external provider execution.
- Tests were added for figma_payload and canva_payload mapping into `RealDesignConversionCoreClient.MapToInternalRequest`.

## verification

Commands/results:

```text
git diff --check
result: exit 0, no output

JSON parse: flow.manifest.json, auth-policy-registry.json, flow-endpoints.json, design-conversion-submit-job.schema.json
result: parsed

dotnet test ...Xiigen.Flow.DesignConversionIntake.Tests.csproj --no-restore --logger "console;verbosity=minimal"
result: did not execute tests; SDK blocker
exact blocker: global.json requests .NET SDK 10.0.300, installed SDKs are 7.0.100, 8.0.417, 9.0.310
```

Marker/readback found `sourceKind`, `html`, `css`, `originalTree`, `screenshots`, `designScreenshots`, `tier`, `/conversion/submit`, `tenant_ops`, `X-Roles`, `RealDesignConversionCoreClient`, and `MapToInternalRequest` in the changed code/schema/test files.

## mergeability

mergeability: clean for the dirty Pass70 snapshot against the touched repo authority/default target.

Method: temporary index + `git hash-object`/`git update-index` for the 9 dirty paths, `git write-tree`, no-ref `git commit-tree -p HEAD`, then `git merge-tree --write-tree --messages` to target. No real index, ref, merge, checkout, rebase, reset, commit, push, or PR was performed.

```text
changed_paths: 9
synthetic_tree: 12a9c938a1b2
synthetic_commit: 5d65f913ee3e
authority_branch origin/main: exit 0, conflict_lines 0
default_main origin/main: exit 0, conflict_lines 0
```

## mental_simulation

figma_payload:

1. Figma caller posts `/conversion/submit` with `sourceKind=figma`, top-level `html`, `css`, `sourceTree`, `originalTree`, `screenshots`, and tier/header metadata.
2. API accepts the caller alias, resolves `X-Roles` to `tenant_ops` when a valid authorization context is present, and applies idempotency/tier/loop/user header defaults.
3. Service normalizes `sourceKind` to `figma`, generates `inline://design-conversion/figma/submitted-payload` when `sourceUri` is absent but inline payload exists, persists a job, and forwards the merged options to `IDesignConversionCoreClient`.
4. `RealDesignConversionCoreClient.MapToInternalRequest` receives Figma source kind, html/css, original tree payload, screenshot artifact, tier, and provenance for DesignConversion processing.

canva_payload:

1. Canva caller posts `/conversion/submit` with `sourceKind=canva`, `sourceTree` entries for `canva-selection.json`, generated HTML, generated CSS, empty `screenshots`, metadata, and tier/header metadata.
2. API/service normalize the inline request and forward `sourceTree`, metadata, tier, and user/idempotency context to the core boundary.
3. `RealDesignConversionCoreClient.MapToInternalRequest` extracts generated HTML/CSS and design tree content from `sourceTree`, maps source kind to Canva, and preserves empty screenshot evidence honestly.

Still unproven: live ASP.NET host request, real Authorization/gateway behavior from the plugin callers, real browser screenshots, real visual AI, real training, and product readiness.

## remaining_queue

remaining_queue:

1. Install/provide .NET SDK `10.0.300` or explicitly approve SDK policy update, then run the focused FLOW-47 test project and any affected API tests.
2. Run a live host/API probe for `/conversion/submit` with valid Figma and Canva caller-shaped payloads plus real authorization/gateway headers.
3. Reconcile platform gateway route if `xiigen-platform-dot-net-angular` must proxy `/conversion/submit` to FLOW-47.
4. Run FigmaToCode and CanvaToCode caller contract tests against the server schema/host once the server build can run.
5. Continue real visual provider/browser screenshot/training evidence only after explicit provider and data authority; do not treat deterministic mapping as visual AI or learned output.

## files_changed

External worktree:

- `C:\Projects\design-to-code-worktrees\xiigen-flows-dot-net-angular-DesignToCode-pass70\flows\FLOW-47-design-conversion-intake\contracts\auth\auth-policy-registry.json`
- `C:\Projects\design-to-code-worktrees\xiigen-flows-dot-net-angular-DesignToCode-pass70\flows\FLOW-47-design-conversion-intake\contracts\endpoints\flow-endpoints.json`
- `C:\Projects\design-to-code-worktrees\xiigen-flows-dot-net-angular-DesignToCode-pass70\flows\FLOW-47-design-conversion-intake\contracts\schemas\design-conversion-submit-job.schema.json`
- `C:\Projects\design-to-code-worktrees\xiigen-flows-dot-net-angular-DesignToCode-pass70\flows\FLOW-47-design-conversion-intake\flow.manifest.json`
- `C:\Projects\design-to-code-worktrees\xiigen-flows-dot-net-angular-DesignToCode-pass70\flows\FLOW-47-design-conversion-intake\server\Xiigen.Flow.DesignConversionIntake.Api\Program.cs`
- `C:\Projects\design-to-code-worktrees\xiigen-flows-dot-net-angular-DesignToCode-pass70\flows\FLOW-47-design-conversion-intake\server\Xiigen.Flow.DesignConversionIntake.Application\DesignConversionIntakeConstants.cs`
- `C:\Projects\design-to-code-worktrees\xiigen-flows-dot-net-angular-DesignToCode-pass70\flows\FLOW-47-design-conversion-intake\server\Xiigen.Flow.DesignConversionIntake.Application\DesignConversionIntakeContracts.cs`
- `C:\Projects\design-to-code-worktrees\xiigen-flows-dot-net-angular-DesignToCode-pass70\flows\FLOW-47-design-conversion-intake\server\Xiigen.Flow.DesignConversionIntake.Application\DesignConversionIntakeService.cs`
- `C:\Projects\design-to-code-worktrees\xiigen-flows-dot-net-angular-DesignToCode-pass70\flows\FLOW-47-design-conversion-intake\server\Xiigen.Flow.DesignConversionIntake.Tests\DesignConversionIntakeServiceTests.cs`

Codex evidence:

- `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass70-external-host-api-integration.md`

## risk_boundary

product_not_ready: true.

No writes were made under `C:\Users\lubao\Documents\xiigen`; those files were read-only guidance. No `claude-*` file was edited. No source authority worktree was edited in place. No commit, push, PR, checkout/switch, merge, rebase, reset, clean, delete, restore, browser check, external AI call, real S3 approval, model upload, or real host e2e was performed.
