# Codex Pass72 Caller Contracts

caller_contracts: true
figma_payload: true
canva_payload: true
sourceKind: true
screenshots: true
originalTree: true
DesignToCode-pass72: true
mergeability: true
product_not_ready: true
remaining_queue: true

claim_scope: Pass72 verified and repaired the FigmaToCode and CanvaToCode caller payload contracts in safe DesignToCode worktrees so their serialized requests include top-level `sourceKind`, `html`, `css`, `sourceTree`, `originalTree`, `screenshots`, `designScreenshots`, `tier`, and user/loop fields when caller config provides them. This is caller contract evidence only, not live e2e, not product readiness, not a commit/push/PR, and not a 50-pass claim.
evidence_scope: Current-pass reads of the required skill stack, Pass70 and Pass71 evidence, FLOW-47 schema/contract files, caller source repo authority/status/remotes, safe worktree creation output, direct payload/test file readback, focused test attempts blocked before Vitest execution, static contract marker checks, `git diff --check`, no-ref synthetic dirty-snapshot merge-tree checks to authority and main, and source-worktree final status checks.
count_claimed: 1 sequential Pass72 caller-contract repair slice, 2 safe worktrees, 2 caller repos repaired, 8 files changed, 2 focused test commands attempted and blocked before test execution, 2 static marker-check passes, 2 `git diff --check` passes, 4 no-ref mergeability checks. 0 passing caller tests, 0 live e2e checks, 0 commits, 0 pushes, 0 PRs, 0 actual merges, 0 source authority worktree writes.
independent_evidence_count: 18 current-pass evidence groups: memory registry lookup, skill-stack line-count/readback, Pass70/71 evidence reads, FLOW-47 schema/contract read, two caller repo fetch/status/ref inspections, two worktree creation outputs, two code/test direct reads, patch output, two focused test blocker outputs, two dependency-state checks, two changed-line readbacks, two static marker checks, two `git diff --check` outputs, two merge-tree groups covering four targets, final source/safe worktree status checks.
subagent_id_or_NONE: Pass72
downgrade_needed: yes. Caller contract tests did not execute because local test dependencies are absent and install commands were outside the explicit allowed command list. Live `/conversion/submit` e2e remains unproven.

## skill_load_status

All required local skill/guidance files in `C:\Projects\llm_mvp_core` were present and read through `Get-Content`; a line-count readback was recorded after the full-content read command.

| group | files | status |
| --- | --- | --- |
| SK-SESSION | `AGENTS.md` 6384 lines; `docs/SESSION-START-PROMPT.md` 578 lines; `docs/skills/planning-session-startup-SKILL.md` 1995 lines | found/read |
| SK-INTENT | `docs/skills/architect-behavior-classifier-SKILL.md` 1441 lines | found/read |
| SK-PLAN | `authority-requirement-binding` 325 lines; `reference-inventory-and-source-map` 336 lines; `phase-planning` 1147 lines | found/read |
| SK-REVIEW | `plan-review` 3618 lines; `goal-delivery-completeness` 612 lines; `claim-verification` 570 lines; `audit-protocol` 471 lines; `specificity-calibration` 306 lines; `output-skepticism` 370 lines; `reconnaissance-gate` 400 lines | found/read |
| SK-ARCH | `trainable-architecture-decision` 735 lines; `no-product-decisions` 158 lines; `output-readability-gate` 724 lines | found/read |

Pass70/71 context files read:

| file | status |
| --- | --- |
| `docs/evidence/design-conversion/codex-pass70-external-host-api-integration.md` 141 lines | found/read |
| `docs/evidence/design-conversion/codex-pass71-flows-sdk-test-verification.md` 259 lines | found/read |

## repo_authority_table

Fresh `git fetch --prune` was run in both source repos. `origin/HEAD` is not configured in either repo, so `origin/main` is reported as main/default.

| repo | source worktree state | main/default | authority_branch | ahead of main | authority decision |
| --- | --- | --- | --- | ---: | --- |
| `C:\Projects\FigmaToCode-design-gen` | `DesignToCode...origin/DesignToCode`, clean; not edited | `origin/main` `a28066597084` from 2026-03-12 | `origin/DesignToCode` `77c381621d2c` from 2026-06-29 | 112 | use `origin/DesignToCode` |
| `C:\Projects\CanvaToCode` | `DesignToCode...origin/DesignToCode`, clean; not edited | `origin/main` `d0a6ebc05540` from 2026-05-12 | `origin/DesignToCode` `2fa6629c9a1e` from 2026-06-28 | 1 | use `origin/DesignToCode` |

## worktree_branch_used

| repo | safe worktree | branch | base commit |
| --- | --- | --- | --- |
| FigmaToCode-design-gen | `C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass72` | `DesignToCode-pass72` tracking `origin/DesignToCode` | `77c381621d2ce1860dfece61113c193f4d4b0fc9` |
| CanvaToCode | `C:\Projects\design-to-code-worktrees\CanvaToCode-DesignToCode-pass72` | `DesignToCode-pass72` tracking `origin/DesignToCode` | `2fa6629c9a1e3a70e5385fa4afa8109a8d264add` |

Source authority worktrees remained clean at final status:

```text
C:\Projects\FigmaToCode-design-gen: ## DesignToCode...origin/DesignToCode
C:\Projects\CanvaToCode: ## DesignToCode...origin/DesignToCode
```

## repair_performed

Figma caller repair:

- `DesignConversionSubmitRequest` now includes `originalTree`, `designScreenshots`, `tier`, optional `userId`, and optional `loopBudget`.
- `buildDesignConversionPayload()` keeps `sourceKind: 'figma'`, top-level generated `html`/`css`, existing `sourceTree`, and existing `screenshots`; it also mirrors sanitized Figma tree data into top-level `originalTree`, mirrors screenshots into `designScreenshots`, and defaults `tier` to the accepted starter tier when not provided.
- `PlatformConversionPanel` now passes `config.designTier`, `config.userId`, and `config.loopBudget` into the payload builder.
- Focused tests were tightened to assert the serialized payload key shape and screenshot alias preservation.

Canva caller repair:

- `SubmitDesignConversionRequest` now includes top-level `html`, `css`, `originalTree`, `designScreenshots`, `tier`, optional `userId`, and optional `loopBudget`.
- `buildCanvaDesignConversionPayload()` keeps `sourceKind: 'canva'`, `sourceTree`, explicit empty `screenshots`, and metadata `screenshotsUnavailableReason`; it now also sends top-level `html`/`css`, top-level `originalTree` equal to the selection summary, explicit empty `designScreenshots`, and `tier`.
- `src/app.tsx` now passes `config.designTier`, `config.userId`, and `config.loopBudget` into the payload builder.
- Focused tests were tightened to assert the full caller contract.

## direct_readback

Figma changed-line readback included:

```text
src/ui/components/platform/DesignConversionTypes.ts:49 sourceKind: 'figma';
src/ui/components/platform/DesignConversionTypes.ts:50 html: string;
src/ui/components/platform/DesignConversionTypes.ts:51 css: string;
src/ui/components/platform/DesignConversionTypes.ts:52 sourceTree: DesignConversionSourceTreeEntry[];
src/ui/components/platform/DesignConversionTypes.ts:53 originalTree: unknown;
src/ui/components/platform/DesignConversionTypes.ts:54 screenshots: DesignConversionScreenshot[];
src/ui/components/platform/DesignConversionTypes.ts:55 designScreenshots: DesignConversionScreenshot[];
src/ui/components/platform/DesignConversionTypes.ts:60 tier: string;
src/ui/components/platform/DesignConversionTypes.ts:61 userId?: string;
src/ui/components/platform/DesignConversionTypes.ts:62 loopBudget?: number;
src/ui/components/platform/DesignConversionTypes.ts:345 originalTree: sanitizedRoot,
src/ui/components/platform/DesignConversionTypes.ts:347 designScreenshots: screenshots,
src/ui/components/platform/PlatformConversionPanel.tsx:78 tier: config.designTier,
src/ui/components/platform/PlatformConversionPanel.tsx:79 userId: config.userId,
src/ui/components/platform/PlatformConversionPanel.tsx:80 loopBudget: config.loopBudget,
tests/design-conversion-client.test.ts:126 Object.keys(submittedBody)
tests/design-conversion-client.test.ts:131 originalTree
tests/design-conversion-client.test.ts:133 designScreenshots
```

Canva changed-line readback included:

```text
src/platform/design-conversion-types.ts:32 sourceKind: DesignConversionSourceKind;
src/platform/design-conversion-types.ts:33 html: string;
src/platform/design-conversion-types.ts:34 css: string;
src/platform/design-conversion-types.ts:35 sourceTree: DesignConversionSourceTreeEntry[];
src/platform/design-conversion-types.ts:36 originalTree: Record<string, unknown>;
src/platform/design-conversion-types.ts:37 screenshots: DesignConversionScreenshot[];
src/platform/design-conversion-types.ts:38 designScreenshots: DesignConversionScreenshot[];
src/platform/design-conversion-types.ts:40 tier: string;
src/platform/design-conversion-types.ts:41 userId?: string;
src/platform/design-conversion-types.ts:42 loopBudget?: number;
src/platform/design-conversion-types.ts:80 sourceKind: 'canva',
src/platform/design-conversion-types.ts:81 html: input.htmlOutput,
src/platform/design-conversion-types.ts:82 css: input.cssOutput,
src/platform/design-conversion-types.ts:103 originalTree: selectionSummary,
src/platform/design-conversion-types.ts:105 designScreenshots: [],
src/app.tsx:122 tier: config.designTier,
src/app.tsx:123 userId: config.userId,
src/app.tsx:124 loopBudget: config.loopBudget,
```

Static contract marker checks passed in both worktrees for source/test markers covering `sourceKind`, `html`, `css`, `sourceTree`, `originalTree`, `screenshots`, `designScreenshots`, `tier`, `userId`, and `loopBudget`.

## verification

Focused test attempts:

```text
C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass72> npm test -- tests/platform-payload.test.ts tests/design-conversion-client.test.ts
result: exit 1
blocker: 'vitest' is not recognized as an internal or external command, operable program or batch file.
dependency state: node_modules=False; node_modules\.bin\vitest.cmd=False; package-lock.json=False; yarn.lock=True; pnpm-lock.yaml=False.
```

```text
C:\Projects\design-to-code-worktrees\CanvaToCode-DesignToCode-pass72> npm test -- tests/platform-payload.test.ts tests/design-conversion-client.test.ts
result: exit 1
blocker: 'vitest' is not recognized as an internal or external command, operable program or batch file.
dependency state: node_modules=False; node_modules\.bin\vitest.cmd=False; package-lock.json=True; yarn.lock=False; pnpm-lock.yaml=False.
```

No local dependency install was run because install commands were not in the explicit allowed command list. Therefore this pass does not claim passing caller tests.

Hygiene:

```text
Figma safe worktree git diff --check: exit 0, no output.
Canva safe worktree git diff --check: exit 0, no output.
```

## mergeability_check

No actual merge, checkout/switch, rebase, reset, clean, commit, push, or PR was performed. Method: temp index seeded from `HEAD`, dirty file blobs indexed into the temp index, `git write-tree`, no-ref `git commit-tree -p HEAD`, and `git merge-tree --write-tree --messages` to each target.

Figma dirty snapshot:

```text
changed_paths=4
synthetic_tree=40be5bcea97dad920e311d8e45b019af62a92809
synthetic_commit=825d4fa6453fdbb8ab20a75b2a492744f894f6b3
target=origin/DesignToCode merge_base=77c381621d2ce1860dfece61113c193f4d4b0fc9 merge_exit=0 conflict_lines=0
target=origin/main merge_base=a28066597084fd57a1ff88fbbf186c17ae511343 merge_exit=0 conflict_lines=0
```

Canva dirty snapshot:

```text
changed_paths=4
synthetic_tree=01f864e548c215f57647a7a7ed98a160f04c84e9
synthetic_commit=9d3a497d229a6eef6fc699cc1cfba17642bc05b6
target=origin/DesignToCode merge_base=2fa6629c9a1e3a70e5385fa4afa8109a8d264add merge_exit=0 conflict_lines=0
target=origin/main merge_base=d0a6ebc055405b67af9ed25c80845c0965f63600 merge_exit=0 conflict_lines=0
```

## mental_simulation

figma_payload:

1. Figma UI generates current `code` and `style`, has a `rootElement`, and may have `frameImageBase64`.
2. `PlatformConversionPanel` builds the payload with config fields `designTier`, `userId`, and `loopBudget`.
3. Serialized request to `/conversion/submit` has exact top-level keys asserted in the focused test: `sourceKind`, `html`, `css`, `sourceTree`, `originalTree`, `screenshots`, `designScreenshots`, `metadata`, `tier`, `userId`, `loopBudget`.
4. If a Figma frame screenshot is available, it is represented both as a `sourceTree` image entry and as matching `screenshots`/`designScreenshots` arrays. If unavailable, both arrays are explicit empty arrays.
5. FLOW-47 Pass70 bridge can accept these fields without guessing source kind or reconstructing the original tree from generated HTML/CSS.

canva_payload:

1. Canva UI generates `htmlOutput` and `cssOutput` from the selected text elements and loads platform config.
2. `buildCanvaDesignConversionPayload()` builds top-level `sourceKind: canva`, `html`, `css`, `sourceTree`, `originalTree`, explicit empty `screenshots`, explicit empty `designScreenshots`, metadata with `screenshotsUnavailableReason`, `tier`, `userId`, and `loopBudget`.
3. `sourceTree` contains `canva-selection.json`, `generated.html`, and `generated.css`; `originalTree` equals the selection summary so FLOW-47 receives an original/source tree without silently reconstructing it.
4. Canva screenshot evidence remains honest: no screenshot API is available in this text adapter, so both screenshot arrays are empty and metadata records the reason.
5. This is serialization/contract evidence only; it does not prove live Canva runtime submission or live server/gateway behavior.

## remaining_queue

remaining_queue:

1. Run focused caller tests after installing/restoring local dependencies through an explicitly authorized local dependency command: Figma uses `yarn.lock` with no `package-lock.json`; Canva uses `package-lock.json`.
2. Run a live `/conversion/submit` host/API probe for Figma and Canva payloads with real authorization/gateway headers after the host is approved/running.
3. Decide whether `designScreenshots` should always duplicate `screenshots` or only be present as an alias for downstream compatibility; Pass72 chose duplication/empty-array explicitness to avoid hidden loss.
4. Continue external provider/browser screenshot proof separately; Pass72 does not prove visual AI, real screenshots from Canva, or product readiness.

## files_changed

Figma safe worktree:

```text
C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass72\src\ui\components\platform\DesignConversionTypes.ts
C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass72\src\ui\components\platform\PlatformConversionPanel.tsx
C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass72\tests\design-conversion-client.test.ts
C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass72\tests\platform-payload.test.ts
```

Canva safe worktree:

```text
C:\Projects\design-to-code-worktrees\CanvaToCode-DesignToCode-pass72\src\app.tsx
C:\Projects\design-to-code-worktrees\CanvaToCode-DesignToCode-pass72\src\platform\design-conversion-types.ts
C:\Projects\design-to-code-worktrees\CanvaToCode-DesignToCode-pass72\tests\design-conversion-client.test.ts
C:\Projects\design-to-code-worktrees\CanvaToCode-DesignToCode-pass72\tests\platform-payload.test.ts
```

Codex evidence:

```text
C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass72-caller-contracts.md
```

## risk_boundary

product_not_ready: true.

No writes were made under `C:\Users\lubao\Documents\xiigen`. No `claude-*` file was edited. No source authority worktree was edited. No global/system install was performed. No local dependency install was performed. No commit, push, PR, checkout/switch in source worktrees, actual merge, rebase, reset, clean, delete, restore, browser check, live host e2e, external AI call, S3 approval, or model upload was performed.

