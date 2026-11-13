# Codex Pass178 - Figma Visual Drift Classification

claim_scope: Pass178 classifies the scene-level visual drift reported after Pass177 for the Figma caller safe-copy chain. It verifies the drift remains in the visual fixture report, verifies the visual test still passes in a Pass178 safe copy, and verifies focused DesignToCode caller tests/build still pass. It does not claim visual quality repair, ProductReady, CI, source publication, deploy, public endpoint, external provider execution, commit, push, merge, PR, or production browser-farm readiness.
evidence_scope: Fresh gate/skill readbacks; Pass177 evidence artifact readback; Pass177 safe-copy visual test, generated visual reports, caller payload/client/live-local tests, package, and caller source readbacks; Pass178 safe copy created from Pass177 safe copy; focused caller tests, live-local caller test, production build, Edge-backed visual-layout test, refreshed layout-report readback, process cleanup checks, and artifact verification.
count_claimed: One bounded Pass178 evidence artifact and one visual-drift classification. This is not a sub-agent pass count, not a layout-quality repair cycle, and not ProductReady.
independent_evidence_count: 33 command/readback groups are listed in commands_run_with_exit_codes, including diagnostic failures/timeouts that were not used as passing evidence.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT

## skill_load_status

| File | Status | Evidence |
|---|---|---|
| C:\Projects\llm_mvp_core\AGENTS.md | found/read | Metadata/hash readback exited 0: 5824 lines, 404994 bytes. Marker readbacks covered no-overclaim, no sandbox_permissions under full access, bounded-local-not-ProductReady, visual/runtime proof boundaries, generated artifact rules, and no-mojibake evidence. |
| C:\Projects\llm_mvp_core\docs\skills\planning-session-startup-SKILL.md | found/read | Metadata/hash readback exited 0: 1725 lines, 121764 bytes. Marker readbacks covered startup skill-load, full-access/no-sandbox, evidence_text_only, bounded local scope, screenshot/visual boundary, and visual QA automation gates. |
| C:\Projects\llm_mvp_core\docs\skills\plan-review-SKILL.md | found/read | Metadata/hash readback exited 0: 3250 lines, 216024 bytes. Marker readback returned relevant plan-review gates for skill-load, safe-worktree evidence boundaries, tests/builds/screenshots, visual QA automation, and dual-path visual proof limits; broad marker search timed out after returning relevant context and was treated as diagnostic only. |
| C:\Projects\llm_mvp_core\docs\skills\claim-verification-SKILL.md | found/read | Metadata/hash readback exited 0: 657 lines, 63132 bytes. Marker readbacks covered readback scope fence, required skill-load status, no-mojibake changed-line readback, bounded-local-not-ProductReady, screenshots/visual boundary, and test/build/file-edit claim boundaries. |
| C:\Projects\llm_mvp_core\docs\skills\qa-session-type-SKILL.md | found/read | Full terminal readback exited 0: 193 lines, 10563 bytes. Applied its QA firewall: verify claims, keep local/safe-copy evidence narrow, and do not equate Gate B or tests with ProductReady. |
| C:\Projects\llm_mvp_core\docs\skills\generated-code-review-SKILL.md | found/read | Full terminal readback exited 0: 538 lines, 30983 bytes. Applied its generated-code review firewall: do not accept code/ProductReady claims beyond executed evidence and keep generated/fixture evidence scoped. |
| C:\Projects\llm_mvp_core\docs\skills\capability-design-review-SKILL.md | found/read | Full terminal readback exited 0: 213 lines, 11794 bytes. Applied its capability review firewall: current scope only, bounded local evidence only, and no broad readiness claim. |
| C:\Projects\llm_mvp_core\docs\skills\codex-planning-quality-GUIDE.md | found/read | Full terminal readback exited 0: 1045 lines, 56021 bytes. Applied its planning-quality firewall: separate current live task from evidence text, safe-worktree evidence from ProductReady, and diagnostic drift from caller integration requirements. |

## files_read

- C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass177-figma-browser-download-gap.md.
- C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass177-browser\tests\test-visual-layout.test.ts.
- C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass177-browser\tests\fixtures\generated\cart-layout-report.json.
- C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass177-browser\tests\fixtures\generated\login-layout-report.json.
- C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass177-browser\tests\fixtures\generated\mail-layout-report.json.
- C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass177-browser\tests\platform-payload.test.ts.
- C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass177-browser\tests\design-conversion-client.test.ts.
- C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass177-browser\tests\design-conversion-live-local.test.ts.
- C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass177-browser\src\ui\components\platform\DesignConversionClient.ts.
- C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass177-browser\src\ui\components\platform\DesignConversionTypes.ts.
- C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass177-browser\package.json.
- Required project gate files listed in skill_load_status.

## safe_copy

| Field | Value |
|---|---|
| Source safe copy | C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass177-browser |
| Pass178 safe copy | C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass178-visual-quality |
| Copy method | Robocopy from Pass177 to Pass178 excluding .git directories. The wrapper command timed out before returning a copy exit code, then the exact robocopy process exited and destination shape was verified. |
| Copy verification | Source and destination both had 24515 files, 258282301 bytes, and 606 top-level node_modules entries. |
| Git metadata in safe copy | .git absent in Pass178 safe copy. |
| Generated fixture file count | 9 files under tests\fixtures\generated. |

## files_changed

- Created C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass178-visual-quality.
- Refreshed Pass178 safe-copy dist\code.js, dist\ui.html, and dist\ui.js via npm run build.
- Refreshed Pass178 safe-copy visual reports under tests\fixtures\generated via the Edge-backed visual-layout test.
- Created this evidence artifact: C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass178-figma-visual-drift.md.

No product source, test source, or original repository file was repaired or edited for this pass.

## visual_drift_classification

classification: visual_fixture_diagnostic_not_DesignToCode_caller_blocker

What drift measures:
- The visual-layout test renders static fixture HTML from tests\fixtures\generated for the login, mail, and cart scenes in Puppeteer/Edge.
- It extracts expected node bounds from tests\fixtures\debug-histories\*-round-e.json SCENE_INPUT captures.
- It compares actual browser bounding boxes to expected positions/sizes and records found, missing, sizeCorrect, posCorrect, posOff50, and sorted per-node diffs.
- posOff50 means a found node whose position drift is greater than 50 px.

Why it is outside the DesignToCode caller API/server boundary:
- tests\test-visual-layout.test.ts imports and launches Puppeteer at lines 150 and 171, reads static generated HTML at lines 174 and 187, reads debug history at lines 175 and 190, computes diffs at line 201, computes posOff50 at line 245, logs report counts at lines 247-251, and writes JSON reports at lines 270-280.
- The only assertion in the visual layout body is node presence: expect(nodes.length).toBeGreaterThan(0) at line 191. There is no assertion that posOff50 is zero, that posCorrect equals found, or that missing equals zero.
- The visual test does not instantiate DesignConversionClient, call buildDesignConversionPayload, call fetch, start the local Kestrel route, or send a DesignToCode server request.
- The caller boundary is covered separately by platform-payload.test.ts, design-conversion-client.test.ts, design-conversion-live-local.test.ts, DesignConversionClient.ts, and DesignConversionTypes.ts. Those paths build Figma payloads/sourceTree/screenshots, send fetch requests to /conversion/submit and /api/design-conversion/improvement-loop, and in live-local start the llm_mvp_core DesignConversionLiveLocalServer.

Fresh Pass178 drift counts after rerun:

| Scene | Total | Found | Missing | SizeCorrect | PosCorrect | PosOff50 |
|---|---:|---:|---:|---:|---:|---:|
| login | 25 | 17 | 8 | 16 | 10 | 3 |
| mail | 78 | 57 | 21 | 48 | 41 | 6 |
| cart | 84 | 64 | 20 | 33 | 26 | 30 |

## repair_or_no_fix_reason

No layout repair was made. The drift is real visual-quality drift in pre-existing generated fixture HTML/debug-history comparison, but the current DesignToCode caller integration requirement is the caller API/server boundary. The drift counts are diagnostic output from a passing fixture-level visual test, not a failing caller test and not a server/caller payload contract failure. Repairing the drift would require changing the Figma generator fixture layout output, visual expectations, or fixture renderer behavior, which is outside this Pass178 caller-boundary task unless Luba separately authorizes visual quality repair.

## commands_run_with_exit_codes

| # | Command or group | Exit code | Result |
|---:|---|---:|---|
| 1 | Locate active AGENTS.md and docs\skills files | 0 | Required active files found under C:\Projects\llm_mvp_core. |
| 2 | Required gate file metadata/hash readback | 0 | Paths, line counts, and byte counts recorded. |
| 3 | Required gate file raw/marker readbacks | 0 | Relevant no-overclaim, safe-copy, QA, visual, and ProductReady boundary gates read. |
| 4 | Broad plan-review marker search | 124 | Timed out after returning relevant markers; diagnostic only. |
| 5 | First metadata/hash summary attempt | 1 | PowerShell parser error; diagnostic only and rerun cleanly. |
| 6 | Read Pass177 evidence artifact | 0 | Pass177 scope, browser proof, visual drift caveat, and command evidence read. |
| 7 | Read Pass177 visual-layout test | 0 | Static fixture/debug-history/Puppeteer drift logic read. |
| 8 | Parse Pass177 generated layout reports | 0 | Pre-Pass178 drift counts and top drifts read. |
| 9 | Read Pass177 caller payload/client/live-local/package files | 0 | Caller test/source/package boundary read. |
| 10 | Visual-layout line-reference search | 0 | Exact drift and report-writing lines recorded. |
| 11 | Payload/type line-reference search | 0 | Payload/sourceTree/screenshot lines recorded. |
| 12 | Client/live-local line-reference search | 0 | Fetch and live-local server boundary lines recorded. |
| 13 | Check Pass178 target path absence | 0 | Pass178 target did not already exist. |
| 14 | Check installed Edge executable | 0 | C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe exists. |
| 15 | Robocopy Pass177 safe copy to Pass178 safe copy | 124 | Wrapper timed out before exit code; not accepted as completed evidence by itself. |
| 16 | Check exact robocopy process | 0 | Robocopy PID 26644 was still running after wrapper timeout. |
| 17 | Wait for exact robocopy process | 0 | Robocopy PID 26644 exited. |
| 18 | Check key Pass178 files | 0 | package and focused test files existed. |
| 19 | Verify Pass178 generated fixture listing | 0 | 9 generated fixture/report files present. |
| 20 | Compare source/destination shape | 0 | Source and Pass178 matched on file count, bytes, and top-level node_modules count. |
| 21 | Check .git and generated fixture count in Pass178 | 0 | .git absent; generated fixture count 9. |
| 22 | npm test -- tests\platform-payload.test.ts tests\design-conversion-client.test.ts | 0 | 2 files passed; 10 tests passed. |
| 23 | npm run build | 0 | Webpack production build emitted code.js, ui.html, and ui.js. |
| 24 | npm test -- tests\design-conversion-live-local.test.ts | 0 | 1 file passed; 1 test passed. |
| 25 | Edge-backed npx vitest run tests\test-visual-layout.test.ts --reporter=verbose | 0 | 1 file passed; 3 tests passed; drift counts logged; cleanup-timeout warning emitted. |
| 26 | Post-visual msedge process check | 0 | No matching Pass178/Puppeteer/remote-debugging Edge process returned. |
| 27 | Post-live-local dotnet process check | 0 | No DesignConversionLiveLocalServer dotnet process returned. |
| 28 | Parse refreshed Pass178 layout reports | 0 | login/mail/cart report counts read after rerun. |
| 29 | Read Pass178 dist listing | 0 | code.js, ui.html, and ui.js emitted at 2026-07-03 16:21:44. |
| 30 | Pre-write git status for Pass178 artifact path | 0 | No existing artifact status output before write. |
| 31 | Post-write identity/verdict/conflict/mojibake verification | 0 | Identity field count 1; verdict field count 1; conflict marker count 0; mojibake marker count 0. |
| 32 | git diff --check for this artifact | 0 | Clean. |
| 33 | Artifact content readback | 0 | Full artifact content read back after creation. |

## verification_results

- Caller payload/client tests in Pass178 safe copy: exit 0, 2 files passed, 10 tests passed.
- Production build in Pass178 safe copy: exit 0, emitted dist\code.js, dist\ui.html, dist\ui.js.
- Live-local caller integration in Pass178 safe copy: exit 0, 1 file passed, 1 test passed.
- Edge-backed visual-layout test in Pass178 safe copy: exit 0, 1 file passed, 3 tests passed.
- Visual-layout stderr included cleanup warning: browser cleanup timed out after 5000ms. Post-run process checks found no matching leftover Edge or DesignConversionLiveLocalServer dotnet process.
- Refreshed visual reports still contain drift counts: login 3, mail 6, cart 30 for posOff50.
- Artifact checks after write: identity field count 1, verdict field count 1, conflict marker count 0, mojibake marker count 0, and git diff --check clean.

## original_repos_touched

No edits were made to C:\Projects\FigmaToCode-design-gen, C:\Projects\CanvaToCode, C:\Users\lubao\Documents\xiigen, or protected platform/FLOW source repos. Commands that changed files were confined to the Pass178 safe copy and this llm_mvp_core evidence artifact.

## remaining_gaps

- The scene-level visual drift itself remains unrepaired.
- This pass proves the drift is not a DesignToCode caller API/server integration blocker; it does not prove the generated fixture HTML has high visual fidelity.
- The visual-layout test logged a browser cleanup timeout, but no matching leftover browser process was found afterward.
- No CI, source repo materialization, commit, push, merge, PR, deploy, public endpoint, external provider call, or production browser-farm route was run.

pass178_verdict: PASS_VISUAL_DRIFT_NOT_DESIGNTOCODE_BLOCKER
downgrade_needed: No downgrade for the bounded Pass178 claim that the visual drift is diagnostic fixture-level layout quality and not a blocker for the DesignToCode caller API/server integration boundary. Downgrade remains required for any claim of visual quality repair, ProductReady, CI, source publication/materialization, deployment, public endpoint, external provider execution, production browser coverage, commit, push, merge, or PR.
