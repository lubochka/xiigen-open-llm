# Codex Pass120 - Browser-Backed Render QA Through Probe

claim_scope:
Pass120 implemented and verified one bounded local DesignConversion product slice: the real browser-backed render provider flowed through `DesignConversionLocalRenderQaProbe` with expected design export refs, multi-viewport PNG artifacts, local score/defect summary, and refs-only boundary metadata.

evidence_scope:
Current-turn required skill/context partial/full reads, Pass119/Pass118/Pass78 evidence reads, targeted source/test reads, one focused test file edit, one focused dotnet test run, generated Pass120 probe JSON readback, and current PNG artifact readback. No external visual AI provider, public HTTP endpoint, cross-repo caller, upload, deployment, or git action was run.

count_claimed:
1 bounded Pass120 implementation/evidence slice.

independent_evidence_count:
8 evidence groups before post-write hygiene: required skill/context reads; Pass119/118/78 evidence reads; targeted source/test reads; browser executable discovery; test implementation edit; focused dotnet test pass; generated probe JSON readback; PNG artifact readback. Final response must add post-write readback/scans/diff if they pass.

subagent_id_or_NONE:
UNKNOWN_IN_SUBAGENT

subagent_identity_self_check:
spawned_worker=true; used_NONE=false; parent_must_reconcile=true

downgrade_needed:
No for the narrow Pass120 browser-backed render-provider-through-probe proof. Yes for full product completion, public HTTP, cross-repo caller wiring, external visual AI, upload/deployment, trained model promotion, and broad suite/CI.

## Skill Load Status

Large governance files were partially read with found/hash/header/targeted gate readback; focused DesignConversion QA guides were read directly. Impact of partial reads: enough to apply claim-honesty, no-permission, skill-load, render-QA, architecture-boundary, and no-overclaim gates for this bounded implementation slice; not claimed as complete semantic review of every large governance file.

| File | Status |
| --- | --- |
| `AGENTS.md` | found/partial_read; first 260 lines plus header/hash/readback; large file. |
| `docs/SESSION-START-PROMPT.md` | found/partial_read; first 220 lines plus header/hash/readback; large file. |
| `docs/skills/planning-session-startup-SKILL.md` | found/partial_read; header/hash/readback plus targeted gate lines from broad readback. |
| `docs/skills/architect-behavior-classifier-SKILL.md` | found/partial_read; header/hash/readback plus targeted gate lines from broad readback. |
| `docs/skills/authority-requirement-binding-SKILL.md` | found/partial_read; header/hash/readback. |
| `docs/skills/reference-inventory-and-source-map-SKILL.md` | found/partial_read; header/hash/readback. |
| `docs/skills/phase-planning-SKILL.md` | found/partial_read; header/hash/readback plus targeted gate lines from broad readback. |
| `docs/skills/codex-planning-quality-GUIDE.md` | found/partial_read; header/hash/readback. |
| `docs/skills/plan-review-SKILL.md` | found/partial_read; header/hash/readback plus targeted gate lines from broad readback; very large file. |
| `docs/skills/goal-delivery-completeness-SKILL.md` | found/partial_read; header/hash/readback plus targeted gate lines from broad readback. |
| `docs/skills/claim-verification-SKILL.md` | found/partial_read; header/hash/readback plus targeted claim-verification lines. |
| `docs/skills/audit-protocol-SKILL.md` | found/partial_read; header/hash/readback. |
| `docs/skills/specificity-calibration-SKILL.md` | found/partial_read; header/hash/readback. |
| `docs/skills/output-skepticism-SKILL.md` | found/partial_read; header/hash/readback. |
| `docs/skills/reconnaissance-gate-SKILL.md` | found/partial_read; header/hash/readback. |
| `docs/skills/trainable-architecture-decision-SKILL.md` | found/partial_read; header/hash/readback. |
| `docs/skills/no-product-decisions-SKILL.md` | found/partial_read; header/hash/readback. |
| `docs/skills/output-readability-gate-SKILL.md` | found/partial_read; header/hash/readback. |
| `docs/skills/qa-session-type-SKILL.md` | found/read; full file read. |
| `docs/skills/generated-code-review-SKILL.md` | found/partial_read; header/hash/readback. |
| `docs/skills/html-css-render-qa-automation-GUIDE.md` | found/read; full file read. |
| `docs/skills/design-conversion-ilearningmachine-architecture-GUIDE.md` | found/read; full file read. |

## Files Read

- `docs/evidence/design-conversion/codex-pass119-remaining-gap-inventory.md`
- `docs/evidence/design-conversion/codex-pass118-render-qa-evidence-surface.md`
- `docs/evidence/design-conversion/codex-pass78-browser-render-qa.md`
- `44.DesignConversionDomain/DesignConversionBrowserQaProviders.cs`
- `44.DesignConversionDomain/DesignConversionLocalRenderQaProbe.cs`
- `44.DesignConversionDomain/DesignConversionThinClientJobRuntime.cs`
- `44.DesignConversionDomain/DesignConversionQaProviders.cs`
- `44.DesignConversionDomain/DesignConversionVisualJudge.cs`
- `25.ModelServer/DesignConversion/DesignConversionModelServerApi.cs`
- `Tests/DesignConversionBrowserQaProviderTests.cs`
- `Tests/DesignConversionLocalRenderQaProbeTests.cs`
- `Tests/DesignConversionModelServerApiTests.cs`

## Files Changed

- `Tests/DesignConversionBrowserRenderQaProbeIntegrationTests.cs`
- `docs/evidence/design-conversion/codex-pass120-browser-backed-render-qa-through-probe.md`

## Implementation Summary

- Added `DesignConversionBrowserRenderQaProbeIntegrationTests.RunAsync_RealBrowserBackedProviderFlowsThroughProbeWithExpectedRefsAndPngArtifacts`.
- The test uses `LocalChromiumBrowserProcessAdapter` and `BrowserBackedDesignConversionRenderProvider`, not a fake render provider.
- The provider is injected into `DesignConversionLocalRenderQaProbe` with three viewports: mobile `390x844`, tablet `768x1024`, and desktop `1440x900`.
- Expected design export refs are passed as refs-only `DesignRenderArtifact` entries: `artifact://pass120/expected/mobile.png`, `tablet.png`, and `desktop.png`.
- The comparison path wraps `DefaultLocalDesignConversionVisualJudge` and records `external_ai_visual_judge_executed=false`; it is local metadata/refs scoring, not external visual AI and not a product visual-fidelity claim.
- The test writes a refs/hash-only probe evidence JSON under the test output `TestResults` tree and asserts the serialized probe result does not include raw HTML or CSS.

## Verification Results

Focused command run:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj -p:TargetFrameworks=net9.0 -p:TargetFramework=net9.0 --filter "FullyQualifiedName~DesignConversionBrowserRenderQaProbeIntegrationTests" --no-restore --logger "console;verbosity=minimal"
```

Result:

- Passed: 1
- Failed: 0
- Skipped: 0
- Duration: 17 s
- Browser executable used: `C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe`

An earlier artifact lookup at `C:\Projects\llm_mvp_core\TestResults\...` failed because the test runner working directory was `Tests\bin\Debug\net9.0`; the actual current artifacts are under `C:\Projects\llm_mvp_core\Tests\bin\Debug\net9.0\TestResults\...`.

## Browser Artifact Evidence

Generated probe evidence JSON:

`C:\Projects\llm_mvp_core\Tests\bin\Debug\net9.0\TestResults\codex-pass120-browser-backed-render-qa-through-probe\ba9973fb71014c36a2c49457d26cd9ab\codex-pass120-probe-evidence.json`

PNG artifacts read back from the generated JSON:

| Viewport | Size | PNG bytes | SHA-256 | Nonwhite pixels | Distinct colors | Path |
| --- | ---: | ---: | --- | ---: | ---: | --- |
| mobile | 390x844 | 58771 | `sha256:2600cd991e15cd668ee583454b22f6a5bed916faa8ebc804a178c5518b09cb71` | 249437 | 1024 | `C:\Projects\llm_mvp_core\Tests\bin\Debug\net9.0\TestResults\codex-pass120-browser-backed-render-qa-through-probe\ba9973fb71014c36a2c49457d26cd9ab\4a4f3d4a72bd41b0a16ab560b53a1677\screenshot.png` |
| tablet | 768x1024 | 209633 | `sha256:0b1ba16271662fa224dc8329d89650ea8c0e204974a454501a2a5aef6f1b5c6e` | 621668 | 1024 | `C:\Projects\llm_mvp_core\Tests\bin\Debug\net9.0\TestResults\codex-pass120-browser-backed-render-qa-through-probe\ba9973fb71014c36a2c49457d26cd9ab\59fcb4c5a19943aab9e64bb95bcbd5a2\screenshot.png` |
| desktop | 1440x900 | 309003 | `sha256:83c8d84cf7e09e44c1e6cffa8e445bb24b44ffe265cf5d391b8af0b4fd9ddb09` | 1023558 | 1024 | `C:\Projects\llm_mvp_core\Tests\bin\Debug\net9.0\TestResults\codex-pass120-browser-backed-render-qa-through-probe\ba9973fb71014c36a2c49457d26cd9ab\9dbc23a5686e4c7c833f3ea766e2c844\screenshot.png` |

Probe boundary metadata readback:

- `browser_render_executed=True`
- `browser_render_not_executed=False`
- `visual_ai_executed=False`
- `visual_ai_not_executed=True`
- `viewport_count=3`
- `expected_design_export_count=3`
- `result_viewport_count=3`
- `average_score=0.97`
- `minimum_score=0.97`
- `total_defect_count=0`
- `raw_html_included=False`
- `raw_css_included=False`
- `raw_expected_design_export_included=False`
- `render_artifact_refs=refs_or_hashes_only`
- `comparison_artifact_refs=refs_or_hashes_only`

## Coverage Update For Original Requirements

| Requirement | Pass120 update |
| --- | --- |
| 3. Render generated HTML/CSS at multiple resolutions through QA automation. | Narrow local proof added: real Edge/Chromium-backed provider flowed through the current local render QA probe and produced three non-empty PNG screenshots for mobile/tablet/desktop. |
| 4. Compare produced screenshots to expected design exports and return score/defects. | Narrow local proof added: expected design export refs were passed through the probe into a local visual-labeler comparison path, returning score `0.97`, defect count `0`, and no external visual AI execution. |

## Boundaries Not Proven

- Full product completion is not proven.
- Public HTTP endpoint is not proven.
- Cross-repo caller wiring is not proven.
- External visual AI execution is not proven.
- The local visual labeler is not an external visual AI or pixel-fidelity provider.
- External upload/deployment is not proven.
- Broad suite/CI is not proven.
- Model training/promotion is not proven by this pass.

## Next Single Pass Recommendation

Run the next single bounded pass on the visual AI/provider boundary or, if Luba wants to keep external services blocked, on an HTTP/local host seam for the already-proven in-process ModelServer surface. Keep it one worker, one route, and preserve `external_visual_ai_executed=false` unless a real configured provider is explicitly authorized.

## Packet Self Check

- `subagent_id_or_NONE` is `UNKNOWN_IN_SUBAGENT`, not `NONE`.
- `subagent_identity_self_check` is present and says spawned_worker=true; used_NONE=false; parent_must_reconcile=true.
- `count_claimed` is 1 bounded Pass120 slice, not full product completion and not broad readiness.
- The evidence does not claim external visual AI, public HTTP, cross-repo caller wiring, upload/deployment, or broad CI.
