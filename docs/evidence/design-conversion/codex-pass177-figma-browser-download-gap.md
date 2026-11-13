# Codex Pass177 - Figma Browser Download Gap Evidence

claim_scope: Pass177 classifies the Figma Puppeteer/browser-download caveat from Pass176 and proves the bounded local caller boundary in a separate safe copy. It also proves the repo's Puppeteer visual-layout test can run in that safe copy with an installed system Edge executable instead of Puppeteer's bundled Chromium download. It does not prove ProductReady, cloud/deploy/public endpoint readiness, CI, source repo publication, production browser farm behavior, external provider execution, commit, push, merge, PR, or a successful Puppeteer bundled Chromium download.
evidence_scope: Fresh required gate reads; Pass176 evidence readback; Pass176 Figma package/test/source inspection; creation of Pass177 safe copy from the Pass176 clean safe copy excluding root node_modules/root git/generated output; offline npm restore in the safe copy with Puppeteer browser download skipped; focused DesignToCode caller tests; live-local caller test; production build; bounded visual-layout Puppeteer test using installed system Edge via PUPPETEER_EXECUTABLE_PATH; cache/process/readback checks. One broad usage search timed out and is diagnostic only.
count_claimed: One Pass177 bounded evidence artifact and one local safe-copy browser-gap classification. This is not a claim of one sub-agent pass, ProductReady, production browser coverage, CI, publication, or bundled Chromium download success.
independent_evidence_count: 59 command/readback groups are listed below, including failures/timeouts as diagnostic evidence only.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT

## skill_load_status

| File | Status | Evidence |
|---|---|---|
| C:\Projects\llm_mvp_core\AGENTS.md | found/read | Full-file read/hash command exited 0; SHA256 C8F43836F268C35BCD40FA3490C3D0B355EB1A98E0144D2F1F870FECAF591F73; 6921 lines. |
| C:\Projects\llm_mvp_core\docs\skills\planning-session-startup-SKILL.md | found/read | Full-file read/hash command exited 0; SHA256 678C6A93E395A369D95EAC10B92624F0362E8E4B1D7C306028804C465FE07CBA; 2073 lines. |
| C:\Projects\llm_mvp_core\docs\skills\plan-review-SKILL.md | found/read | Full-file read/hash command exited 0; SHA256 A10D5B098E2A55A837313C8B5B1AA99DC402F9179C6DA3C0778DD3A657B3A959; 3787 lines. |
| C:\Projects\llm_mvp_core\docs\skills\claim-verification-SKILL.md | found/read | Full-file read/hash command exited 0; SHA256 5160418D8B5DFCB992FE9D19979517FBCF61DBB6256EC174AC19E66A5F1AD905; 805 lines. |
| C:\Projects\llm_mvp_core\docs\skills\qa-session-type-SKILL.md | found/read | Full-file read/hash and full terminal readback exited 0; SHA256 BF5544598D4E358AA9333D2A0A47D4E010862DC7F58EBB2181908BD45B86B6DD; 257 lines. |
| C:\Projects\llm_mvp_core\docs\skills\generated-code-review-SKILL.md | found/read | Full-file read/hash and full terminal readback exited 0; SHA256 CEB6CF6BBFFF4121DB8FA898A21765AFDA0CF70E2F9334CD9FA8F171C21EDCC4; 672 lines. |
| C:\Projects\llm_mvp_core\docs\skills\capability-design-review-SKILL.md | found/read | Full-file read/hash and full terminal readback exited 0; SHA256 326002875A7AB5B438EA9E709CF5FDA17D582BAF4C414DB60CBEBE061AFB13AC; 280 lines. |

## files_read

- C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass176-caller-clean-install.md.
- C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass176-clean\package.json.
- C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass176-clean\vitest.config.ts.
- C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass176-clean\tests\platform-payload.test.ts.
- C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass176-clean\tests\design-conversion-client.test.ts.
- C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass176-clean\tests\design-conversion-live-local.test.ts.
- C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass176-clean\tests\test-visual-layout.test.ts.
- C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass176-clean\src\ui\components\platform\DesignConversionClient.ts.
- C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass176-clean\src\ui\components\platform\DesignConversionTypes.ts.
- Required project gate files listed in skill_load_status.

## safe_copy

| Field | Value |
|---|---|
| Source safe copy | C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass176-clean |
| Pass177 safe copy | C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass177-browser |
| Copy exclusions | root node_modules, .git directories, dist, build, .cache, .next, .vite, coverage, *.log |
| Copy result | Robocopy wrapper exited 0; raw robocopy exit code was 1, meaning files were copied successfully under robocopy semantics. |
| Root node_modules before install | absent |
| Root .git before install | absent |
| Node/npm | Node v22.16.0; npm 11.4.2 |

## files_changed

- Created C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass177-browser.
- Created Pass177 safe-copy node_modules with `PUPPETEER_SKIP_DOWNLOAD=1 npm ci --offline --no-audit --no-fund --progress=false`.
- Generated Pass177 safe-copy dist\code.js, dist\ui.html, and dist\ui.js with `npm run build`.
- Refreshed Pass177 safe-copy visual-layout reports under tests\fixtures\generated during the Edge-backed Puppeteer test.
- Created this evidence artifact: C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass177-figma-browser-download-gap.md.

No original caller repo file was edited.

## puppeteer_usage_classification

classification: not_required_for_caller_boundary

Evidence:
- package.json keeps Puppeteer only as a devDependency: `package.json:35`.
- package scripts use webpack build and Vitest: `package.json:7`, `package.json:10`.
- Vitest config includes `tests/**/*.test.ts`: `vitest.config.ts:4`.
- The only source/test import of Puppeteer found by targeted search is the visual-layout test: `tests/test-visual-layout.test.ts:15`; it launches a browser at `tests/test-visual-layout.test.ts:150`.
- The DesignToCode caller tests and code paths are payload/fetch/local-server paths, not Puppeteer paths:
  - `tests/platform-payload.test.ts:24`, `tests/platform-payload.test.ts:65`, `tests/platform-payload.test.ts:105`.
  - `tests/design-conversion-client.test.ts:65`, `tests/design-conversion-client.test.ts:122`, `tests/design-conversion-client.test.ts:221`.
  - `tests/design-conversion-live-local.test.ts:30`, `tests/design-conversion-live-local.test.ts:52`, `tests/design-conversion-live-local.test.ts:62`, `tests/design-conversion-live-local.test.ts:93`.
  - `src/ui/components/platform/DesignConversionClient.ts:125`, `src/ui/components/platform/DesignConversionClient.ts:138`, `src/ui/components/platform/DesignConversionClient.ts:190`, `src/ui/components/platform/DesignConversionClient.ts:194`.
  - `src/ui/components/platform/DesignConversionTypes.ts:335`, `src/ui/components/platform/DesignConversionTypes.ts:400`, `src/ui/components/platform/DesignConversionTypes.ts:419`, `src/ui/components/platform/DesignConversionTypes.ts:457`.

Conclusion: Puppeteer's bundled Chromium download is not required for the DesignToCode caller integration boundary. The separate visual-layout test does require a browser, and that browser-backed route was proven with installed system Edge in the Pass177 safe copy.

## browser_gap_result

- Pass176 caveat reproduced as context: Figma dependency restore had previously passed only with browser download skipped after Puppeteer chrome-headless-shell download hit certificate verification.
- Pass177 did not force an insecure bundled browser download and did not normalize certificate bypass as production quality.
- Pass177 package restore used offline cache plus `PUPPETEER_SKIP_DOWNLOAD=1`; exit code 0; 873 packages added.
- Pass177 safe copy had no project `.cache` and no `node_modules\.cache\puppeteer` after install.
- Installed system Edge was found at C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe.
- Browser-backed visual-layout proof used `PUPPETEER_EXECUTABLE_PATH` pointed at system Edge; verbose Vitest exit code 0; 1 file and 3 tests passed.
- The visual-layout test emitted a cleanup-timeout warning, then no `msedge.exe` process remained in the post-test process readback.
- This proves a bounded local browser-backed UI/e2e-style test in the Pass177 safe copy without bundled Chromium download. It does not prove that Puppeteer's bundled Chromium download succeeds.

## commands_run_with_exit_codes

| # | Command or group | Exit code | Result |
|---:|---|---:|---|
| 1 | `Get-Location` in C:\Projects\llm_mvp_core | 0 | Confirmed workspace path. |
| 2 | `Get-Item .\AGENTS.md` | 0 | AGENTS.md exists. |
| 3 | `rg --files` for required AGENTS/skill files | 0 | Required gate files found. |
| 4 | `git status --short` in llm_mvp_core | 0 | Existing dirty worktree observed; not changed by this command. |
| 5 | `Get-Item` metadata for required gate files | 0 | Sizes/timestamps read. |
| 6 | `Get-Content -Raw` planning-session-startup skill | 0 | File read; output large. |
| 7 | `Get-Content -Raw` plan-review skill | 0 | File read; output large. |
| 8 | `Get-Content -Raw` claim-verification skill | 0 | File read; output large. |
| 9 | Full-file read/hash summary for all required gate files | 0 | Paths, line counts, hashes recorded. |
| 10 | `Get-Content -Raw` QA session type skill | 0 | File read. |
| 11 | `Get-Content -Raw` generated-code-review skill | 0 | File read. |
| 12 | `Get-Content -Raw` capability-design-review skill | 0 | File read. |
| 13 | `Get-Item` Pass176 artifact | 0 | Artifact exists. |
| 14 | `Get-Content -Raw` Pass176 artifact | 0 | Pass176 caveat and command evidence read. |
| 15 | `Get-Content -Raw` Pass176 Figma package.json | 0 | Scripts and dependencies read. |
| 16 | Broad `rg` usage search over Pass176 safe copy | 124 | Timed out; diagnostic only, not accepted as complete usage evidence. |
| 17 | `rg --files` in Pass176 safe copy excluding heavy dirs | 0 | File inventory read. |
| 18 | Narrow `rg` usage search in Pass176 safe copy | 0 | Puppeteer use isolated to package/locks and visual-layout test. |
| 19 | `Get-ChildItem` Pass176 safe-copy root | 0 | Root contents read. |
| 20 | `Get-ChildItem` Pass176 tests directory | 0 | Test inventory read. |
| 21 | `Get-Content -Raw` vitest config | 0 | Test include config read. |
| 22 | `Get-Content -Raw` visual-layout test | 0 | Puppeteer import/launch read. |
| 23 | `Get-Content -Raw` DesignConversion client test | 0 | Caller test read. |
| 24 | `Get-Content -Raw` live-local test | 0 | Live-local caller test read. |
| 25 | `Get-Content -Raw` platform payload test | 0 | Payload builder test read. |
| 26 | `Get-Content -Raw` DesignConversionClient.ts | 0 | Caller fetch client read. |
| 27 | `Get-Content -Raw` DesignConversionTypes.ts | 0 | Caller payload types/builders read. |
| 28 | Targeted `rg` over tests/src/config/package | 0 | Caller/browser references read. |
| 29 | Pass177 target existence precheck | 0 | Target was absent. |
| 30 | Pass176 source safe-copy existence readback | 0 | Source exists. |
| 31 | Worktree directory listing for pass17* Figma copies | 0 | Pass175 and Pass176 copies existed before Pass177. |
| 32 | Robocopy Pass176 safe copy to Pass177 safe copy, excluding root node_modules and generated dirs | 0 | Wrapper exit 0; raw robocopy exit code 1. |
| 33 | Pass177 root node_modules preinstall check | 0 | Absent. |
| 34 | Pass177 root .git preinstall check | 0 | Absent. |
| 35 | Pass177 root listing | 0 | Root contents read. |
| 36 | `node --version; npm --version` | 0 | Node v22.16.0; npm 11.4.2. |
| 37 | `PUPPETEER_SKIP_DOWNLOAD=1 npm ci --offline --no-audit --no-fund --progress=false` | 0 | Added 873 packages; no bundled browser download attempted. |
| 38 | Local Chrome/Edge executable path checks | 0 | Edge path exists; Chrome paths absent. |
| 39 | Pass177 root `.cache` check | 0 | Absent. |
| 40 | Pass177 `node_modules\.cache\puppeteer` check | 0 | Absent. |
| 41 | Pass177 `node_modules\puppeteer\package.json` readback | 0 | Puppeteer package installed. |
| 42 | `npm test -- tests/platform-payload.test.ts tests/design-conversion-client.test.ts` | 0 | 2 files passed; 10 tests passed. |
| 43 | `npm test -- tests/design-conversion-live-local.test.ts` | 0 | 1 file passed; 1 test passed. |
| 44 | `npm run build` | 0 | Webpack build emitted code.js, ui.html, ui.js. |
| 45 | First Edge-backed `npm test -- tests/test-visual-layout.test.ts` | 0 | Exit 0 but no visible output; treated as insufficient until verbose rerun. |
| 46 | Visual report file readback after first Edge run | 0 | Report files existed. |
| 47 | Dist output readback | 0 | code.js, ui.html, ui.js present. |
| 48 | Post-test `node_modules\.cache\puppeteer` check | 0 | Absent. |
| 49 | Verbose Edge-backed `npx --no-install vitest run tests/test-visual-layout.test.ts --reporter=verbose` | 0 | 1 file passed; 3 tests passed; cleanup warning logged. |
| 50 | `Get-CimInstance` for msedge.exe after browser run | 0 | No msedge.exe process remained. |
| 51 | Initial package line-reference `rg` | 1 | Regex quoting error; diagnostic only. |
| 52 | Vitest config line-reference `rg` | 0 | Include/typecheck lines read. |
| 53 | Visual-layout line-reference `rg` | 0 | Puppeteer import/launch lines read. |
| 54 | Caller test/source line-reference `rg` | 0 | Caller boundary lines read. |
| 55 | Retried package line-reference `rg` | 0 | Build/test/Puppeteer dependency lines read. |
| 56 | `msedge.exe --version` | 0 | Command returned no version text; no version claim made. |
| 57 | Visual report timestamp readback | 0 | Reports updated at 2026-07-03 16:04:51. |
| 58 | Pass177 node_modules entry count | 0 | 606 top-level entries read. |
| 59 | Post-run node/npm/dotnet process filter for Pass177/live-local command lines | 0 | No matching leftover process remained. |

## original_repos_touched

No. This pass did not run commands in, edit files in, or write outputs to:

- C:\Projects\FigmaToCode-design-gen
- C:\Projects\CanvaToCode
- C:\Users\lubao\Documents\xiigen
- protected platform/FLOW source repos

The only copied/installed/built/tested caller tree was the Pass177 safe copy under C:\Projects\design-to-code-worktrees.

## artifact_verification

| Check | Exit code | Result |
|---|---:|---|
| Exact identity field count | 0 | Count = 1. |
| Exact verdict field count | 0 | Count = 1. |
| Line-anchored conflict-marker scan | 0 | Count = 0. |
| Configured mojibake marker scan | 0 | Count = 0. |
| `git diff --check -- docs/evidence/design-conversion/codex-pass177-figma-browser-download-gap.md` | 0 | Clean. |

## remaining_gaps

- Puppeteer's bundled Chromium download remains unproven in this environment and was intentionally not forced through an insecure certificate workaround.
- The system Edge proof is bounded local safe-copy evidence only.
- The browser-backed visual-layout test passes as a test harness but its layout report still contains scene-level visual drift counts; this pass did not repair design layout quality.
- No CI, source publication/materialization, commit, push, merge, PR, deploy, public endpoint, external provider call, or production browser-farm route was run.

pass177_verdict: PASS_BROWSER_PROOF_BOUNDED_SAFE_COPY
downgrade_needed: No downgrade for the bounded Pass177 claim that Puppeteer's bundled Chromium download is not required for the DesignToCode caller integration boundary and that a browser-backed local visual-layout test can run in the safe copy using installed system Edge. Downgrade remains required for bundled Chromium download success, ProductReady, production browser coverage, CI, source repo publication/materialization, commit, push, merge, PR, deploy, public endpoint, external provider execution, or visual quality repair.
