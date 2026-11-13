# codex Pass49 Canva Screenshot Seam Patch Artifact

claim_scope: Patch/evidence artifact only for the Canva screenshot/design export seam discovered before Pass49.
evidence_scope: Read-only inspection of `C:\Projects\CanvaToCode` source/tests and current `C:\Projects\llm_mvp_core` design-conversion plan/evidence docs. No external repo write, test, build, browser, server, upload, training, git write, or product code edit was performed.
count_claimed: 1 patch/evidence artifact.
independent_evidence_count: 1 artifact file after readback verification.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT.
downgrade_needed: yes. This artifact is not an applied patch, not real Canva screenshot evidence, and not readiness.

write_mode: patch_artifact_only
allowed_write: `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass49-canva-screenshot-seam.patch.md`

## Skill Load Status

| Gate or guide | Status | Read evidence used |
|---|---|---|
| `AGENTS.md` | found/read relevant sections | User-provided full instruction block plus live `Select-String` read of honesty, work-order, readiness, skill-load, and human override gates. |
| `docs\SESSION-START-PROMPT.md` | found/read | Live full read; used evidence-bound claim, work-order completeness, future-gate, no-readiness-overclaim, and no external work boundaries. |
| `docs\skills\claim-verification-SKILL.md` | found/read | Live full read; used claim-as-hypothesis, count-claim, applied-vs-proposed patch, and no readiness overclaim rules. |
| `docs\skills\audit-protocol-SKILL.md` | found/read | Live full read; used exact path/source verification, severity consistency, not-checked declaration, and output packet evidence rules. |
| `docs\skills\plan-review-SKILL.md` | found/read | Live read; used plan-not-ready and future-gate boundaries, especially that execution/test gates cannot hide design/dependency decisions. |
| `docs\skills\goal-delivery-completeness-SKILL.md` | found/read | Live full read; used coverage mapping from current task to artifact sections and the rule that runtime delivery is not proven by report existence. |
| `docs\skills\reference-inventory-and-source-map-SKILL.md` | found/read | Live full read; used explicit source inventory/source map and current-target scope separation. |
| `docs\skills\output-readability-gate-SKILL.md` | found/read | Live full read; used readable evidence boundary and no-shortcode-as-primary-status guard. |
| `docs\skills\no-product-decisions-SKILL.md` | found/read | Live full read; used no self-selection of product alternatives beyond a reversible patch proposal. |

## Source Inventory

| Source id | Path | Line evidence | Claim supported | Scope |
|---|---|---:|---|---|
| S1 | `C:\Projects\CanvaToCode\src\platform\design-conversion-types.ts` | 18-35, 52-98 | Current request type supports `screenshots`, payload input has `htmlOutput`, `cssOutput`, `elementCount`, and current builder returns `screenshots: []` with `screenshotsUnavailableReason`. | current code evidence |
| S2 | `C:\Projects\CanvaToCode\src\app.tsx` | 17-18, 29-34, 107-132, 168-183 | Submit path builds payload from generated HTML/CSS and element count, then POSTs via `submitConversion`; no screenshot/export data is gathered in that path. | current code evidence |
| S3 | `C:\Projects\CanvaToCode\src\types\canva-sdk.d.ts` | 27-54 | Local Canva type shim exposes selection registration and text update only; no design export screenshot API is declared in the inspected shim. | current code evidence |
| S4 | `C:\Projects\CanvaToCode\tests\platform-payload.test.ts` | 4-23, 36-50 | Existing test locks the current behavior: `screenshots` equals empty array, source tree has `canva-selection.json`, `generated.html`, `generated.css`, and metadata records unavailable screenshot reason. | current test evidence |
| S5 | `C:\Projects\CanvaToCode\src\canva-adapter.ts` | 4-10, 28-57, 79-93 | Adapter boundary is Canva-shaped text selection to Figma-shaped fake scene node; inspected lines show text element/run/style data, not full design export image data. | current code evidence |
| S6 | `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-S3-caller-project-real-data-discovery-2026-06-30.md` | 100-110 | Prior discovery classified CanvaToCode as a text adapter/platform client and recorded no screenshots/visual diffs plus empty `screenshots` behavior. | evidence-only prior packet |
| S7 | `C:\Projects\llm_mvp_core\docs\plans\DCM-DESIGN-CONVERSION-SERVER-PLAN-v1-codex.md` | 413-417, 1054-1087, 1931-1934, 1966, 1987 | Current DCM plan expects caller payload visual fields, public endpoint only, render QA artifacts, and caller tests, while remaining plan-only. | current plan evidence |

## Current Gap Evidence

1. CanvaToCode currently has a `DesignConversionScreenshot` type and request field, but `buildCanvaDesignConversionPayload` always sets `screenshots: []`.
2. The payload builder records the reason string `Canva text adapter does not expose a design export screenshot API.`
3. The submit path in `src\app.tsx` passes only `htmlOutput`, `cssOutput`, and `elementCount` into the payload builder.
4. The inspected Canva shim declares selection and text update APIs, not screenshot, thumbnail, or full design export APIs.
5. The current plan expects visual fields such as design screenshots/exports and render evidence. That plan evidence is not the same as applied implementation.

The evidence supports only this narrow claim: CanvaToCode can submit generated HTML/CSS and text-selection metadata, but the inspected code does not provide a real Canva screenshot/export artifact.

## Proposed Seam

When the Canva runtime does not expose a screenshot/export API, the caller should submit an explicit visual-evidence status plus a deterministic preview artifact. This keeps the public contract honest:

- `screenshots` remains empty.
- `metadata.visualEvidenceUnavailable` is true.
- `metadata.visualEvidenceUnavailableReason` carries the existing Canva API limitation.
- `metadata.visualEvidenceFallback` is `deterministic-html-preview`.
- `sourceTree` includes `visual-preview.html`, a deterministic HTML document that embeds the generated HTML and CSS.
- The preview artifact is not real screenshot evidence and must not be treated as an original Canva export, expected designer export, or full visual-diff target.
- Platform/core validation accepts this as an explicit unavailable status for Canva text-only submissions, not as readiness.

This seam gives downstream code a stable render input for generated HTML/CSS while preserving the missing original-design evidence boundary.

## Patch Proposal For `C:\Projects\CanvaToCode`

```diff
diff --git a/src/platform/design-conversion-types.ts b/src/platform/design-conversion-types.ts
--- a/src/platform/design-conversion-types.ts
+++ b/src/platform/design-conversion-types.ts
@@
 export interface DesignConversionMetadata {
     elementCount: number;
     scope: DesignConversionScope;
     screenshotsUnavailableReason?: string;
+    visualEvidenceUnavailable?: boolean;
+    visualEvidenceUnavailableReason?: string;
+    visualEvidenceFallback?: 'deterministic-html-preview';
+    visualEvidenceFallbackPath?: string;
+    visualEvidenceFallbackIsNotRealScreenshot?: true;
     [key: string]: unknown;
 }
@@
 export function buildCanvaDesignConversionPayload(
     input: CanvaDesignConversionPayloadInput,
 ): SubmitDesignConversionRequest {
     const screenshotsUnavailableReason = 'Canva text adapter does not expose a design export screenshot API.';
+    const visualPreviewPath = 'visual-preview.html';
+    const visualPreviewHtml = buildDeterministicVisualPreviewHtml(input.htmlOutput, input.cssOutput);
     const selectionSummary = {
         sourceKind: 'canva' as const,
         scope: 'text-only' as const,
         elementCount: input.elementCount,
         screenshotsUnavailableReason,
+        visualEvidenceUnavailable: true,
+        visualEvidenceUnavailableReason: screenshotsUnavailableReason,
+        visualEvidenceFallback: 'deterministic-html-preview' as const,
+        visualEvidenceFallbackPath: visualPreviewPath,
+        visualEvidenceFallbackIsNotRealScreenshot: true as const,
     };
@@
             {
                 path: 'generated.css',
                 mediaType: 'text/css',
                 content: input.cssOutput,
                 encoding: 'utf-8',
             },
+            {
+                path: visualPreviewPath,
+                mediaType: 'text/html',
+                content: visualPreviewHtml,
+                encoding: 'utf-8',
+            },
         ],
         screenshots: [],
         metadata: {
             elementCount: input.elementCount,
             scope: 'text-only',
             screenshotsUnavailableReason,
+            visualEvidenceUnavailable: true,
+            visualEvidenceUnavailableReason: screenshotsUnavailableReason,
+            visualEvidenceFallback: 'deterministic-html-preview',
+            visualEvidenceFallbackPath: visualPreviewPath,
+            visualEvidenceFallbackIsNotRealScreenshot: true,
         },
     };
 }
+
+function buildDeterministicVisualPreviewHtml(htmlOutput: string, cssOutput: string): string {
+    return [
+        '<!doctype html>',
+        '<html>',
+        '<head>',
+        '<meta charset="utf-8">',
+        '<meta name="x-visual-evidence" content="visualEvidenceUnavailable">',
+        '<meta name="x-visual-evidence-boundary" content="not real screenshot">',
+        '<style>',
+        cssOutput,
+        '</style>',
+        '</head>',
+        '<body>',
+        '<main data-source-kind="canva" data-visual-evidence="not-real-screenshot">',
+        htmlOutput,
+        '</main>',
+        '</body>',
+        '</html>',
+    ].join('\n');
+}
```

```diff
diff --git a/tests/platform-payload.test.ts b/tests/platform-payload.test.ts
--- a/tests/platform-payload.test.ts
+++ b/tests/platform-payload.test.ts
@@
         expect(payload.sourceTree.map((entry) => entry.path)).toEqual([
             'canva-selection.json',
             'generated.html',
             'generated.css',
+            'visual-preview.html',
         ]);
@@
     });
+
+    it('adds deterministic preview fallback while preserving not real screenshot boundary', () => {
+        const payload = buildCanvaDesignConversionPayload({
+            htmlOutput: '<div class="title">Hello</div>',
+            cssOutput: '.title { color: red; }',
+            elementCount: 1,
+        });
+
+        expect(payload.screenshots).toEqual([]);
+        expect(payload.metadata).toMatchObject({
+            visualEvidenceUnavailable: true,
+            visualEvidenceFallback: 'deterministic-html-preview',
+            visualEvidenceFallbackPath: 'visual-preview.html',
+            visualEvidenceFallbackIsNotRealScreenshot: true,
+        });
+
+        const preview = payload.sourceTree.find((entry) => entry.path === 'visual-preview.html');
+        expect(preview).toMatchObject({ mediaType: 'text/html', encoding: 'utf-8' });
+        expect(preview?.content).toContain('visualEvidenceUnavailable');
+        expect(preview?.content).toContain('not real screenshot');
+        expect(preview?.content).toContain('<style>');
+        expect(preview?.content).toContain('.title { color: red; }');
+        expect(preview?.content).toContain('<div class="title">Hello</div>');
+    });
 });
```

## Platform/Core Contract Patch Proposal

No platform/core code was edited or tested in this turn. The receiving platform/core contract should be patched in its actual DTO/validator files after endpoint discovery to distinguish three states:

```ts
type VisualEvidenceStatus =
    | 'providedScreenshot'
    | 'visualEvidenceUnavailable'
    | 'invalidMissingEvidenceStatus';
```

Validation rule to add after endpoint file discovery:

```text
If sourceKind/sourcePlatform is `canva`, screenshots is empty, and metadata.visualEvidenceUnavailable is true:
  accept the request as a Canva text-only submission with visual evidence unavailable;
  require sourceTree path `visual-preview.html`;
  record `visualEvidenceUnavailableReason`;
  do not populate designScreenshots or expectedDesignExports from the preview;
  route visual-diff/original-export evidence as blocked_original_export_unavailable.

If sourceKind/sourcePlatform is `canva`, screenshots is empty, and metadata.visualEvidenceUnavailable is not true:
  reject with a validation error such as CANVA_SCREENSHOT_OR_UNAVAILABLE_STATUS_REQUIRED.
```

Render QA may render `visual-preview.html` into actual candidate screenshots for generated HTML/CSS smoke evidence. That rendered image is not real screenshot evidence for the original Canva design and must not satisfy `designScreenshots[]`, `expectedDesignExports[]`, held-out measurement, DPO visual labels, promotion, or readiness by itself.

## Tests To Run After Applying

tests to run: these commands are proposed for a later authorized implementation pass; they were not run for this artifact.

Run only after an executor is authorized to edit and test the relevant repos.

From `C:\Projects\CanvaToCode`:

```powershell
npm test -- --run tests/platform-payload.test.ts
npm test -- --run tests/design-conversion-client.test.ts
git diff --check -- src/platform/design-conversion-types.ts tests/platform-payload.test.ts
```

After platform endpoint discovery identifies the actual files:

```powershell
# Replace the paths with the discovered platform/core DTO and validator test paths.
dotnet test <platform-or-core-test-project> --filter "Canva|VisualEvidence|DesignConversion"
git diff --check -- <discovered-dto-file> <discovered-validator-test-file>
```

## Remaining Limitations

- Not applied: this is a patch artifact only.
- Not run: no tests, builds, browser, server, external API, training, upload, git write, commit, or push were run.
- Not real screenshot: the proposed `visual-preview.html` fallback is deterministic HTML render input, not a Canva screenshot or Canva design export.
- Not readiness: this does not prove full design conversion, visual diff, held-out quality, DPO usefulness, trained model quality, or promotion readiness.
- The platform/core endpoint file path remains to be discovered before implementation. The artifact names the required validation behavior but does not claim a concrete receiving-file patch was applied.

## Next Single Subagent Work Order

CURRENT TASK:
Apply the Pass49 Canva screenshot seam patch in the correct implementation repo(s) after endpoint discovery, preserving the honest missing-screenshot boundary.

NOT THE TASK:
Do not claim real Canva screenshot evidence; do not train, upload, promote, commit, push, or call external APIs; do not use `visual-preview.html` as original design export evidence.

EVIDENCE-ONLY:
This Pass49 artifact is a proposal. Treat it as source-mapped guidance, not applied code.

required_skills_and_gates:
Load `AGENTS.md`, `docs\SESSION-START-PROMPT.md`, `claim-verification-SKILL.md`, `audit-protocol-SKILL.md`, `plan-review-SKILL.md`, `goal-delivery-completeness-SKILL.md`, `reference-inventory-and-source-map-SKILL.md`, `output-readability-gate-SKILL.md`, and `no-product-decisions-SKILL.md`. Include `skill_load_status`; use `UNKNOWN_IN_SUBAGENT` if the platform id is invisible.

context_files_to_read:
`C:\Projects\CanvaToCode\src\platform\design-conversion-types.ts`, `C:\Projects\CanvaToCode\src\app.tsx`, `C:\Projects\CanvaToCode\tests\platform-payload.test.ts`, `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass49-canva-screenshot-seam.patch.md`, and discovered platform/core DTO/validator files.

implementation_or_artifact_details:
Add explicit `visualEvidenceUnavailable` metadata and deterministic `visual-preview.html` source-tree fallback in CanvaToCode; add receiving platform/core validation that accepts the explicit unavailable state and rejects silent empty screenshots; preserve `screenshots: []` and `not real screenshot` boundary.

allowed_files:
Only the exact CanvaToCode implementation/test files and discovered platform/core DTO/test files named by the work order.

forbidden_files:
Unrelated repos, training data, checkpoints, model uploads, git metadata, and `C:\Users\lubao\Documents\xiigen`.

allowed_commands:
Targeted file reads, targeted tests named above, and path-scoped `git diff --check`.

forbidden_commands:
External repo writes outside the work order, broad tests/builds, browser/server/external API, training/upload/promotion, and git write commands unless separately authorized.

verification_required:
Readback of changed files; tests above; marker search for `visualEvidenceUnavailable`, `not real screenshot`, and empty `screenshots` boundary; path-scoped diff check; changed-files report.

return_packet_schema:
`claim_scope`, `evidence_scope`, `count_claimed`, `independent_evidence_count`, `subagent_id_or_NONE`, `downgrade_needed`, `skill_load_status`, `canva_read_evidence`, `changed_files`, `tests_run`, `readback_verification`, `diff_check`, `remaining_blockers`, `next_single_subagent_work_order`, `validation`.
