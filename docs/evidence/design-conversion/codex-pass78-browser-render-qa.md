# Codex Pass78 Browser Render QA Evidence

claim_scope: browser_render_qa local implementation and focused test evidence only.
evidence_scope: real local Edge headless rendering of generated HTML+CSS at three viewports, PNG artifact inspection, deterministic pixel evidence, focused test run, diff hygiene, and no-write mergeability checks.
count_claimed: 1 sequential Pass78 worker; 3 rendered viewport PNG artifacts in the latest manifest.
independent_evidence_count: 3 PNG files plus 1 focused dotnet test command result plus 1 manifest/hash readback.

## Runtime Inventory

- Browser runtime detected: `C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe` with file version `149.0.4022.98`.
- `msedge`, `chrome`, `chromium`, and `chromium-browser` were not found on PATH by `Get-Command`.
- Node runtime was present (`node v22.16.0`, `npm 11.4.2`), but Pass78 used the existing C# local Chromium/Edge provider path instead of a Node/global install path.
- Scoped repo search found no `playwright`, `Microsoft.Playwright`, `puppeteer`, package-lock, pnpm-lock, yarn-lock, or package.json automation dependency signal outside `bin`, `obj`, and `node_modules` exclusions.
- No global package install was executed.
- No external visual AI/provider, S3, model upload, or production service was called.

## Implementation

- Added `DesignConversionBrowserRenderManifestProvider` to load HTML+CSS through the browser-backed render provider at required viewports and return a manifest with viewport, local PNG path, dimensions, hash, byte length, and pixel evidence.
- Added `DesignConversionBrowserPngEvidenceInspector` to parse PNG dimensions and deterministic nonblank pixel evidence from the browser screenshots.
- Added a test-only artifact root option to `LocalChromiumBrowserProcessAdapter` so focused tests can write under a Pass78 runtime artifact folder.
- Added focused tests proving three real browser PNG artifacts are produced when a local browser is available, and that a missing required viewport fails.

## Verification Commands

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionBrowserQaProviderTests" --no-restore --logger "console;verbosity=minimal"
```

Result: passed. `Failed: 0, Passed: 6, Skipped: 0, Total: 6, Duration: 23 s`.

```powershell
Get-FileHash -Algorithm SHA256 <rendered screenshot path>
```

Result: hashes matched the manifest `sha256:` values listed below.

## PNG Artifacts

Latest manifest:

`C:\Projects\llm_mvp_core\Tests\bin\Debug\net9.0\TestResults\codex-pass78-browser-render-qa\a51ea7326d6844ebafb057ffec17ff88\codex-pass78-browser-render-manifest.json`

| viewport | requested | png dimensions | bytes | sha256 | nonwhite pixels | distinct colors | path |
| --- | ---: | ---: | ---: | --- | ---: | ---: | --- |
| mobile | 360x740 | 360x740 | 44297 | `45d16534ddd41afd8ab59d9c7066e6d6dce2ac6af7f142274232876d696d28a7` | 211144 | 1024 | `C:\Projects\llm_mvp_core\Tests\bin\Debug\net9.0\TestResults\codex-pass78-browser-render-qa\a51ea7326d6844ebafb057ffec17ff88\dc0ffd9c37bf46ca9f6bbd1dd3ee93dc\screenshot.png` |
| tablet | 768x1024 | 768x1024 | 226730 | `e98c76ec56b84ad81296b10e359d09f6c7cac5246d7dace161b337f39ab52dd6` | 672035 | 1024 | `C:\Projects\llm_mvp_core\Tests\bin\Debug\net9.0\TestResults\codex-pass78-browser-render-qa\a51ea7326d6844ebafb057ffec17ff88\270c4a5dffaa45e9b7d55db2d157503a\screenshot.png` |
| desktop | 1440x900 | 1440x900 | 346252 | `a7f29499b567243f00a3a456a09d9b934d28c09d81f445fd109545b543c7a538` | 1060643 | 1024 | `C:\Projects\llm_mvp_core\Tests\bin\Debug\net9.0\TestResults\codex-pass78-browser-render-qa\a51ea7326d6844ebafb057ffec17ff88\b53342a967924d9ca8ff73180c4d1838\screenshot.png` |

Markers: browser_render_qa, multi_resolution_render, png_artifacts, nonblank_pixels.

## Mental Simulation

A Figma/Canva conversion result supplies generated HTML+CSS to the server-side QA render provider. The provider renders the same HTML+CSS through local Edge at mobile/tablet/desktop viewports, writes PNG artifacts, records each viewport's dimensions, byte length, sha256, and nonblank pixel evidence, then stops at the visual QA boundary. The PNGs are evidence for later expected-vs-rendered visual comparison; deterministic pixel checks are evidence only and are not hidden trainable authority.

## Boundary

- browser_render_proven: true for local Edge headless screenshot generation in the focused test.
- visual_ai_not_executed: true.
- product_not_ready: true; this proves local browser rendering, not external visual AI success, provider authorization, production storage, or end-to-end product readiness.
- mergeability: `git diff --check` passed for changed files; no-write temporary-index `git apply --cached --check --3way` passed for the Pass78 source/test/evidence snapshot against `origin/DesignToCode-clean` and `origin/main`.
- remaining_queue: run/approve the external visual AI tier separately, wire any production artifact storage only under the approved plan, and continue broader DesignConversion readiness gates without upgrading this local browser proof into full product readiness.
