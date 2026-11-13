# Codex Pass175 - Caller Project Integration Evidence

claim_scope: Pass175 verifies and repairs only the local caller-project boundary for DesignToCode in safe copies of FigmaToCode-design-gen and CanvaToCode. It proves the two caller projects have local TypeScript clients/payload builders/tests for the DesignToCode platform submit route and the local ModelServer improvement-loop route. It does not prove ProductReady, cloud, deploy, public endpoint, external provider, CI, source repo publication, commit, push, merge, or PR readiness.
evidence_scope: Fresh read of required governance/skill files, Pass170/173/174 evidence, read-only inspection of the original caller repos, direct read of the Pass170 platform and ModelServer contract surfaces, safe-copy creation under C:\Projects\design-to-code-worktrees, safe-copy source/test repairs, focused caller tests, live-local ModelServer caller tests, safe-copy builds/typecheck, diff-style whitespace checks, marker/readback checks, and original caller repo status readback. No original caller repo file, protected platform/FLOW repo file, xiigen document, claude file, commit, push, merge, PR, deploy, cloud endpoint, or external provider was edited or invoked.
count_claimed: 1 Pass175 caller integration evidence artifact plus two safe caller-copy repair slices. This is not a pass-count inflation claim and not a ProductReady claim.
independent_evidence_count: 15 evidence groups: required skill/gate reads; Pass170/173/174 evidence reads; original caller repo statuses; original Figma caller code/test reads; original Canva caller code/test reads; Pass170 platform submit model read; ModelServer improvement-loop contract read; safe-copy creation and junction proof; Figma safe repair; Canva safe repair; Figma mocked caller tests; Figma live-local caller test; Figma production build; Canva mocked caller tests/live-local/typecheck/build; hygiene and original-status recheck.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
downgrade_needed: No downgrade for bounded safe-copy caller integration after the commands below. Downgrade remains required for ProductReady, cloud/deploy/public endpoint/provider/production readiness, source repo materialization, commit, push, merge, PR, CI, clean install from lockfiles, broad UI/runtime coverage, or publication into the protected/original caller repos.

## skill_load_status

| File | Status |
|---|---|
| C:\Projects\llm_mvp_core\AGENTS.md | found/read |
| C:\Projects\llm_mvp_core\docs\skills\planning-session-startup-SKILL.md | found/read |
| C:\Projects\llm_mvp_core\docs\skills\plan-review-SKILL.md | found/read |
| C:\Projects\llm_mvp_core\docs\skills\claim-verification-SKILL.md | found/read |
| C:\Projects\llm_mvp_core\docs\skills\qa-session-type-SKILL.md | found/read |
| C:\Projects\llm_mvp_core\docs\skills\generated-code-review-SKILL.md | found/read |
| C:\Projects\llm_mvp_core\docs\skills\capability-design-review-SKILL.md | found/read |
| C:\Projects\llm_mvp_core\docs\skills\codex-planning-quality-GUIDE.md | found/read |

## caller_repos_found

| Caller repo | Framework/language found | Original repo status before/after |
|---|---|---|
| C:\Projects\FigmaToCode-design-gen | TypeScript React Figma plugin, webpack 4, Vitest 4, package name `figma-to-code`. | Git repo on `DesignToCode`; before and after status matched with pre-existing modified DesignConversion client/type/test files and untracked live-local test. |
| C:\Projects\CanvaToCode | TypeScript React Canva adapter/app, webpack 5, Vitest 1, package name `canva-text-adapter`. | Git repo on `DesignToCode`; before and after status matched with pre-existing modified DesignConversion client/type/test files and untracked live-local test. |

## safe_worktrees_or_copies

- Safe Figma copy: C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass175.
- Safe Canva copy: C:\Projects\design-to-code-worktrees\CanvaToCode-DesignToCode-pass175.
- Both safe roots have no root `.git` directory.
- Both safe roots use a `node_modules` junction to the corresponding original repo's existing local dependencies. No install was run or claimed.
- `package-lock.json` was copied into both safe roots after the initial copy; lockfile no-index checks against the originals exited 0.

## files_read

- C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass174-final-bounded-readiness-review.md.
- C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass173-packaging-consumer-restore.md.
- C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass170-platform-flow-real-modelserver-allup.md.
- Required skill/gate files listed in `skill_load_status`.
- C:\Projects\FigmaToCode-design-gen\package.json.
- C:\Projects\FigmaToCode-design-gen\src\ui\components\platform\DesignConversionTypes.ts.
- C:\Projects\FigmaToCode-design-gen\src\ui\components\platform\DesignConversionClient.ts.
- C:\Projects\FigmaToCode-design-gen\src\ui\components\platform\PlatformConversionPanel.tsx.
- C:\Projects\FigmaToCode-design-gen\tests\platform-payload.test.ts.
- C:\Projects\FigmaToCode-design-gen\tests\design-conversion-client.test.ts.
- C:\Projects\FigmaToCode-design-gen\tests\design-conversion-live-local.test.ts.
- C:\Projects\CanvaToCode\package.json.
- C:\Projects\CanvaToCode\src\platform\design-conversion-types.ts.
- C:\Projects\CanvaToCode\src\platform\design-conversion-client.ts.
- C:\Projects\CanvaToCode\src\platform\platform-config.ts.
- C:\Projects\CanvaToCode\tests\platform-payload.test.ts.
- C:\Projects\CanvaToCode\tests\design-conversion-client.test.ts.
- C:\Projects\CanvaToCode\tests\design-conversion-live-local.test.ts.
- C:\Projects\design-to-code-worktrees\xiigen-platform-dot-net-angular-DesignToCode-pass75\src\Xiigen.Platform.Api\DesignConversion\DesignConversionEndpointModels.cs.
- C:\Projects\llm_mvp_core\25.ModelServer\DesignConversion\DesignConversionModelServerImprovementLoopHandler.cs.
- C:\Projects\llm_mvp_core\25.ModelServer\DesignConversion\DesignConversionModelServerCallerContract.cs.

## files_changed

Manual safe-copy source/test repairs:
- C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass175\src\ui\components\platform\DesignConversionTypes.ts.
- C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass175\tests\platform-payload.test.ts.
- C:\Projects\design-to-code-worktrees\CanvaToCode-DesignToCode-pass175\src\platform\design-conversion-types.ts.
- C:\Projects\design-to-code-worktrees\CanvaToCode-DesignToCode-pass175\tests\platform-payload.test.ts.

Safe-copy generated or copied outputs:
- C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass175\dist\code.js.
- C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass175\dist\ui.html.
- C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass175\dist\ui.js.
- C:\Projects\design-to-code-worktrees\CanvaToCode-DesignToCode-pass175\dist\app.js.
- C:\Projects\design-to-code-worktrees\CanvaToCode-DesignToCode-pass175\dist\app.js.LICENSE.txt.
- C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass175\package-lock.json, copied unchanged from original.
- C:\Projects\design-to-code-worktrees\CanvaToCode-DesignToCode-pass175\package-lock.json, copied unchanged from original.

Evidence artifact:
- C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass175-caller-project-integration.md.

No original caller repo file was edited by this pass.

## commands_run_with_exit_codes

| Command | Exit code | Result |
|---|---:|---|
| `rg --files` and `Test-Path` for required skill/evidence/caller roots | 0 | Required roots and files found. |
| `Get-Content -Raw` on Pass170, Pass173, Pass174 evidence | 0 | Prior evidence read. |
| `rg` marker readbacks over required skill/gate files | 0 | Required gates found/read for no approval prompt, evidence honesty, bounded local/ProductReady separation, skill-load reporting, and verification. |
| `git status --short --branch` in both original caller repos before repair | 0 | Both original repos were pre-existing dirty on `DesignToCode`. |
| `Get-Content -Raw` over caller package/client/type/panel/test files | 0 | Existing caller shapes inspected read-only. |
| `Get-Content`/`rg` over Pass170 platform and ModelServer contract files | 0 | Server fields and validation requirements inspected. |
| Safe copy scripts using `robocopy` plus `node_modules` junction creation | 0 | Safe roots created under C:\Projects\design-to-code-worktrees. |
| `npm test -- tests/platform-payload.test.ts tests/design-conversion-client.test.ts` in Figma safe copy | 0 | 2 files passed, 10 tests passed. |
| `npm test -- tests/design-conversion-live-local.test.ts` in Figma safe copy | 0 | 1 file passed, 1 live-local caller test passed against local Kestrel ModelServer route. |
| `npm run build` in Figma safe copy | 0 | Webpack production build passed. |
| `npm test -- tests/platform-payload.test.ts tests/design-conversion-client.test.ts` in Canva safe copy | 0 | 2 files passed, 9 tests passed; Vite CJS deprecation warning only. |
| `npm test -- tests/design-conversion-live-local.test.ts` in Canva safe copy | 0 | 1 file passed, 1 live-local caller test passed against local Kestrel ModelServer route; Vite CJS deprecation warning only. |
| `npm run typecheck` in Canva safe copy | 0 | `tsc --noEmit` passed. |
| `npm run build` in Canva safe copy | 0 | Webpack production build passed. |
| `git diff --no-index --check` for the four manual source/test deltas | 1 expected | Differences existed; no whitespace-warning output. |
| `git diff --no-index --check` for Figma and Canva lockfiles | 0 | Copied lockfiles matched originals. |
| `git diff --no-index --check` for generated dist outputs | 0 or 1 expected | Differences where outputs changed; no whitespace-warning output. |
| Marker scan over four manual source/test deltas | 1 | No conflict marker or configured mojibake marker matches. |
| Conflict-marker scan over generated dist outputs | 1 | No line-start conflict markers. |
| `git status --short --branch` in both original caller repos after repair | 0 | Status matched the before status; original repos were not edited by this pass. |

## caller_payload_mapping

| Requirement | Figma safe-copy mapping | Canva safe-copy mapping |
|---|---|---|
| Endpoint URL seam | `DEFAULT_DESIGN_CONVERSION_CONFIG.baseUrl` default `http://localhost:8787`; UI panel can persist base URL; client trims trailing slash. | `DEFAULT_PLATFORM_CONFIG.baseUrl` default `http://localhost:8787`; platform config load/save seam exists; client trims trailing slash. |
| Platform submit route | `DesignConversionClient.submitConversion` posts to `/conversion/submit`. | `DesignConversionClient.submitConversion` posts to `/conversion/submit`. |
| Headers/tenant/user/idempotency/tier/budget | `buildDesignConversionHeaders` emits `X-Roles`, `X-Tenant-Id`, `X-User-Id`, `X-Idempotency-Key`, `X-Design-Tier`, `X-Loop-Budget`; defaults normalized to `tenant_ops`, `starter`, budget cap 2. | `buildDesignConversionHeaders` emits the same header set; defaults map to `tenant_ops`, `starter`, budget 1 within cap 2. |
| html/css result | `buildDesignConversionPayload` emits top-level `html` and `css` plus source-tree entries `generated/index.html` and `generated/styles.css`. | Safe repair emits top-level `html` and `css` plus source-tree entries `generated.html` and `generated.css`. |
| original tree | Safe repair emits top-level `originalTree` from sanitized Figma root element and keeps `source/root-element.json`. | Safe repair emits top-level `originalTree` from the Canva selection summary and keeps `canva-selection.json`. |
| screenshots/source refs | Figma emits first-class `screenshots` entries when `frameImageBase64` is present; each has id, data URI, mime type, sha256, width/height, and source-tree metadata. | Canva has no export screenshot API; safe repair emits `designScreenshots` with a memory render ref, sha256, positive viewport fallback, and metadata explaining the screenshot fallback. |
| ModelServer improvement-loop route | Existing Figma client posts to `/api/design-conversion/improvement-loop` and builder emits `initialPackage` with sourceKind `Figma`, designTree, originalTree, html, css, screenshots/designRenders, budget, metadata. | Existing Canva client posts to `/api/design-conversion/improvement-loop` and builder emits `initialPackage` with sourceKind `Canva`, designTree, originalTree, html, css, screenshots/designRenders, budget, metadata. |
| refs-only response parsing | Figma parses boundary metadata so raw html/css/design tree/private payload echoes remain false in tests. | Canva parses boundary metadata so raw html/css/design tree/private payload echoes remain false in tests. |

## focused_verification_results

- Figma safe-copy mocked caller tests passed: 2 files, 10 tests.
- Figma safe-copy live-local JS caller test passed: 1 file, 1 test; it started the local DesignConversionLiveLocalServer, sent the Figma caller payload to the local Kestrel route, and asserted refs-only/no-public/no-deploy/ProductReady=false metadata.
- Figma safe-copy production build passed.
- Canva safe-copy mocked caller tests passed: 2 files, 9 tests.
- Canva safe-copy live-local JS caller test passed: 1 file, 1 test; it started the local DesignConversionLiveLocalServer, sent the Canva caller payload to the local Kestrel route, and asserted refs-only/no-public/no-deploy/ProductReady=false metadata.
- Canva safe-copy `tsc --noEmit` passed.
- Canva safe-copy production build passed.
- Manual changed safe source/test files had no conflict marker or configured mojibake marker matches, and direct changed-line readback showed readable ASCII text.
- Generated Figma `dist/ui.js` contains legitimate third-party Lodash Latin transliteration map characters such as `A` mappings for accented letters; that generated-bundle marker hit was inspected as a false positive and is not used for a no-mojibake claim over generated bundles.

## original_repos_touched

No. The original caller repos were read and copied from, but not edited, tested, built, committed, cleaned, reset, pushed, merged, or used for PR work by this pass. Their before and after `git status --short --branch` outputs matched:

- C:\Projects\FigmaToCode-design-gen: `## DesignToCode...origin/DesignToCode`, pre-existing modified `src/ui/components/platform/DesignConversionClient.ts`, `src/ui/components/platform/DesignConversionTypes.ts`, `tests/design-conversion-client.test.ts`, and untracked `tests/design-conversion-live-local.test.ts`.
- C:\Projects\CanvaToCode: `## DesignToCode...origin/DesignToCode`, pre-existing modified `src/platform/design-conversion-client.ts`, `src/platform/design-conversion-types.ts`, `tests/design-conversion-client.test.ts`, and untracked `tests/design-conversion-live-local.test.ts`.

## remaining_gaps

- This is bounded local/safe-copy proof, not ProductReady or production readiness.
- No cloud deploy, public endpoint, external provider call, CI, commit, push, merge, PR, or protected/source repo materialization was run.
- No clean dependency install was run; safe copies used existing local `node_modules` junctions and copied lockfiles. The evidence supports local dependency availability, not reproducible clean install.
- The original caller repos remain pre-existing dirty. This pass did not normalize or publish those originals.
- Broad UI/browser interaction beyond the focused live-local JS caller tests was not run.

pass175_verdict: PASS_CALLER_PROJECT_INTEGRATION_BOUNDED_SAFE_WORKTREE
