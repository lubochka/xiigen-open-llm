# Codex Pass176 - Caller Clean Install Evidence

claim_scope: Pass176 verifies clean local dependency restore plus focused caller tests/builds for safe copies of the Figma and Canva DesignToCode caller projects. It proves only bounded local safe-copy package restore and caller checks. It does not prove ProductReady, cloud/deploy/public endpoint, external provider execution, production browser coverage, CI, source repo publication, commit, push, merge, PR, or protected/original repo materialization.
evidence_scope: Fresh required skill/gate reads and targeted readbacks; Pass175 evidence read; Pass175 safe-copy package/lock/junction inspection; creation of two separate Pass176 clean safe copies without copied node_modules; npm clean install attempts and bounded Figma clean-copy lock repair; focused mocked caller tests; focused live-local caller tests; Figma production build; Canva typecheck and production build; root .git absence and real node_modules readbacks; original caller repo status readbacks; artifact marker and diff checks after write.
count_claimed: One Pass176 bounded evidence artifact and two local clean safe-copy caller verification slices. This is not a claim of two passes, ProductReady, source publication, CI, cloud readiness, or external provider readiness.
independent_evidence_count: 40 pre-artifact command/readback groups are summarized below; post-artifact self-check commands are listed in the verification section. Failed/time-out install attempts are counted only as diagnostic evidence, not as successful install proof.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT

## skill_load_status

| File | Status | Evidence boundary |
|---|---|---|
| C:\Projects\llm_mvp_core\AGENTS.md | found/read | Full-file read command executed; targeted readback inspected no-permission-prompt, no-overclaim, skill-load, no-mojibake, bounded-local/ProductReady separation, and git-publication boundaries. |
| C:\Projects\llm_mvp_core\docs\skills\planning-session-startup-SKILL.md | found/read | Full file read command executed; startup/firewall sections read back. |
| C:\Projects\llm_mvp_core\docs\skills\plan-review-SKILL.md | found/read | Full-file read command executed; targeted readback inspected Pass172, permission, claim, history, no-mojibake, and git boundaries. |
| C:\Projects\llm_mvp_core\docs\skills\claim-verification-SKILL.md | found/read | Full file read command executed; claim-boundary/no-overclaim/readback sections read back. |
| C:\Projects\llm_mvp_core\docs\skills\qa-session-type-SKILL.md | found/read | Full file read back in terminal output. |
| C:\Projects\llm_mvp_core\docs\skills\generated-code-review-SKILL.md | found/read | Full file read back in terminal output. |
| C:\Projects\llm_mvp_core\docs\skills\capability-design-review-SKILL.md | found/read | Full file read back in terminal output. |
| C:\Projects\llm_mvp_core\docs\skills\codex-planning-quality-GUIDE.md | found/read | Full-file read command executed; targeted readback inspected no-overclaim, bounded-local/ProductReady, NEEDS_FIX, and git-publication boundaries. |

## clean_safe_copies

| Caller | Clean proof path | Copy source | Copy/install proof |
|---|---|---|---|
| Figma | C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass176-clean | C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass175 | Target did not exist before copy. Robocopy exit code 1, success under robocopy rules. `node_modules` absent after copy, then installed as a real directory. Root `.git` absent. |
| Canva | C:\Projects\design-to-code-worktrees\CanvaToCode-DesignToCode-pass176-clean | C:\Projects\design-to-code-worktrees\CanvaToCode-DesignToCode-pass175 | Target did not exist before copy. Robocopy exit code 1, success under robocopy rules. `node_modules` absent after copy, then installed as a real directory. Root `.git` absent. |

Final clean-copy readback:

| Caller | Root `.git` | `node_modules` | Link/junction target | Node lstat | Dist output |
|---|---|---|---|---|---|
| Figma | absent | present | none | `isDirectory=true`, `isSymbolicLink=false` | `code.js`, `ui.html`, `ui.js` |
| Canva | absent | present | none | `isDirectory=true`, `isSymbolicLink=false` | `app.js`, `app.js.LICENSE.txt` |

## files_read

- C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass175-caller-project-integration.md.
- Required skill/gate files listed in `skill_load_status`.
- C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass175\package.json.
- C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass175\package-lock.json.
- C:\Projects\design-to-code-worktrees\CanvaToCode-DesignToCode-pass175\package.json.
- C:\Projects\design-to-code-worktrees\CanvaToCode-DesignToCode-pass175\package-lock.json.
- Pass176 clean-copy `package.json` and `package-lock.json` files for Figma and Canva.
- Original caller repo status was read only with `git status --short --branch` in C:\Projects\FigmaToCode-design-gen and C:\Projects\CanvaToCode.

## files_changed

- Created C:\Projects\design-to-code-worktrees\FigmaToCode-design-gen-DesignToCode-pass176-clean from the Pass175 safe copy, excluding `node_modules`, `.git`, `dist`, and common cache/output directories.
- Created C:\Projects\design-to-code-worktrees\CanvaToCode-DesignToCode-pass176-clean from the Pass175 safe copy, excluding `node_modules`, `.git`, `dist`, and common cache/output directories.
- Repaired only the Figma Pass176 clean-copy `package-lock.json` using `npm install --package-lock-only --strict-ssl=false --no-audit --no-fund --progress=false`, because copied Pass175 lock did not contain `jsdom`, `puppeteer`, or `vitest` required by Figma `package.json`.
- Installed clean-copy dependencies under each Pass176 `node_modules` directory.
- Generated Figma clean-copy `dist\code.js`, `dist\ui.html`, and `dist\ui.js` with `npm run build`.
- Generated Canva clean-copy `dist\app.js` and `dist\app.js.LICENSE.txt` with `npm run build`.
- Created this evidence artifact: C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass176-caller-clean-install.md.

No original caller repo file was edited by this pass.

## commands_run_with_exit_codes

| # | Command or command group | Exit code | Evidence/result |
|---:|---|---:|---|
| 1 | `rg --files` for required AGENTS/skill files | 0 | Required files found under C:\Projects\llm_mvp_core and docs\skills. |
| 2 | `Test-Path` for Pass175 evidence and both Pass175 safe copies | 0 | All required context paths existed. |
| 3 | `Get-Item` metadata for required skill/gate files | 0 | File sizes and timestamps read. |
| 4 | `Get-Content -Raw` for planning startup skill | 0 | Startup/no-permission/no-overclaim gates read back; terminal output was large. |
| 5 | `Get-Content -Raw` for claim verification skill | 0 | Claim-boundary and no-overclaim gates read back; terminal output was large. |
| 6 | `Get-Content -Raw` for QA session type skill | 0 | QA evidence-boundary skill read back. |
| 7 | `Get-Content -Raw` for generated-code-review skill | 0 | Generated-code/local-evidence boundary skill read back. |
| 8 | `Get-Content -Raw` for capability-design-review skill | 0 | Capability bounded-evidence boundary skill read back. |
| 9 | `Select-String` targeted readback across AGENTS, plan-review, and planning-quality guide | 0 | No permission prompt, no `sandbox_permissions`, no overclaim, ProductReady separation, skill-load, no-mojibake, and git boundaries inspected. |
| 10 | `Get-Content -Raw` Pass175 evidence artifact | 0 | Pass175 clean-install gap and focused commands read. |
| 11 | Full-file read commands for AGENTS, startup, plan-review, claim-verification, and planning-quality guide | 0 | Large-file read commands executed; targeted excerpts are the inspected content boundary. |
| 12 | `Get-FileHash` for required skill/gate files | 0 | Hash command ran; terminal table truncated hashes, so this is freshness evidence only, not a full hash transcription claim. |
| 13 | Pass175 safe-copy `package.json`, lockfile, and node_modules inspection | 0 | Figma and Canva Pass175 copies used `node_modules` junctions; locks were version 3. |
| 14 | Pass176 target existence precheck | 0 | Both target paths were absent before copy. |
| 15 | Robocopy clean safe-copy creation wrapper | 0 | Robocopy exit code 1 for each copy, meaning files copied successfully; `node_modules` absent and lockfiles present after copy. |
| 16 | Figma initial `npm ci` | 124 | Timed out after 604114 ms; no install success claimed. |
| 17 | Exact stale npm process cleanup for Figma first timeout | 0 | Stopped PID 7332 after command line matched `npm-cli.js ci`. |
| 18 | Figma verbose `npm ci --no-audit --no-fund --progress=false --loglevel=verbose` | 124 | Timed out after 904226 ms; npm log showed repeated `UNABLE_TO_VERIFY_LEAF_SIGNATURE` registry retries. |
| 19 | Figma `npm ci --offline --no-audit --no-fund --progress=false` | 1 | Failed with `ENOTCACHED` for `puppeteer`; cache incomplete. |
| 20 | Figma `npm ci --strict-ssl=false --no-audit --no-fund --progress=false` before lock repair | 1 | Failed with EUSAGE lock/package mismatch; missing `jsdom`, `puppeteer`, `vitest`, and transitive lock entries. |
| 21 | Node lock inspection for Figma clean copy | 0 | Confirmed copied lock lacked `node_modules/jsdom`, `node_modules/puppeteer`, and `node_modules/vitest`. |
| 22 | Figma `npm install --package-lock-only --strict-ssl=false --no-audit --no-fund --progress=false` | 0 | Repaired only clean-copy lockfile; no `node_modules` created. |
| 23 | Figma repaired-lock readback | 0 | Repaired lock contains `jsdom 29.1.1`, `puppeteer 24.43.1`, and `vitest 4.1.9`; `node_modules` absent before install. |
| 24 | Figma `npm ci --strict-ssl=false --no-audit --no-fund --progress=false` after lock repair | 1 | Reached Puppeteer postinstall, then failed chrome-headless-shell download due certificate verification. |
| 25 | Figma partial-install cleanup/readback | 0 | No active node/npm process; `node_modules` absent before final rerun. |
| 26 | Figma `PUPPETEER_SKIP_DOWNLOAD=1; npm ci --strict-ssl=false --no-audit --no-fund --progress=false` | 0 | Added 873 packages in the clean copy. This proves package dependency restore, not bundled browser download. |
| 27 | Figma `node_modules` readback | 0 | Real directory, no symlink/junction target; npm 11.4.2, node v22.16.0. |
| 28 | Figma `npm test -- tests/platform-payload.test.ts tests/design-conversion-client.test.ts` | 0 | 2 files passed, 10 tests passed. |
| 29 | Figma `npm test -- tests/design-conversion-live-local.test.ts` | 0 | 1 file passed, 1 live-local caller test passed. |
| 30 | Figma `npm run build` | 0 | Webpack 4.47.0 production build emitted `code.js`, `ui.html`, `ui.js`. |
| 31 | Canva preinstall lock/toolchain readback | 0 | No `node_modules`; lockfileVersion 3; vitest 1.6.1; webpack 5.106.2. |
| 32 | Canva `npm ci --no-audit --no-fund --progress=false` | 0 | Added 280 packages in the clean copy. |
| 33 | Canva `node_modules` readback | 0 | Real directory, no symlink/junction target. |
| 34 | Canva `npm test -- tests/platform-payload.test.ts tests/design-conversion-client.test.ts` | 0 | 2 files passed, 9 tests passed; Vite CJS deprecation warning only. |
| 35 | Canva `npm test -- tests/design-conversion-live-local.test.ts` | 0 | 1 file passed, 1 live-local caller test passed; Vite CJS deprecation warning only. |
| 36 | Canva `npm run typecheck` | 0 | `tsc --noEmit` passed. |
| 37 | Canva `npm run build` | 0 | Webpack 5.106.2 production build compiled successfully and emitted `app.js`. |
| 38 | `git status --short --branch` in original Figma caller repo | 0 | Read-only status showed the same pre-existing Pass175 dirty files: modified DesignConversion client/type/test and untracked live-local test. |
| 39 | `git status --short --branch` in original Canva caller repo | 0 | Read-only status showed the same pre-existing Pass175 dirty files: modified DesignConversion client/type/test and untracked live-local test. |
| 40 | Final clean-copy root `.git`, `node_modules`, lstat, and dist readback | 0 | Both clean copies are non-git roots with real local `node_modules` directories and generated dist outputs. |

## install_results

| Caller | Install result | Notes |
|---|---|---|
| Figma | PASS after bounded clean-copy lock repair and Puppeteer browser-download skip | The copied lock was stale against `package.json`. The clean-copy lock was repaired with npm. Registry certificate verification required `--strict-ssl=false`. Puppeteer browser download failed on certificate verification, then dependency restore passed with `PUPPETEER_SKIP_DOWNLOAD=1`. This does not prove bundled Chromium/browser installation. |
| Canva | PASS from copied lock | `npm ci --no-audit --no-fund --progress=false` exited 0 and added 280 packages. No lock repair was needed. |

## focused_test_build_results

| Caller | Mocked caller tests | Live-local caller test | Typecheck | Build |
|---|---|---|---|---|
| Figma | PASS, exit 0, 2 files and 10 tests passed | PASS, exit 0, 1 file and 1 test passed | N/A, no package script used in Pass175 | PASS, exit 0, webpack production build emitted `code.js`, `ui.html`, `ui.js` |
| Canva | PASS, exit 0, 2 files and 9 tests passed | PASS, exit 0, 1 file and 1 test passed | PASS, exit 0, `tsc --noEmit` | PASS, exit 0, webpack production build emitted `app.js` |

## original_repos_touched

No original caller repo file was edited by this pass. The original repos were used only for read-only status evidence:

- C:\Projects\FigmaToCode-design-gen current status readback: `## DesignToCode...origin/DesignToCode`; modified `src/ui/components/platform/DesignConversionClient.ts`, `src/ui/components/platform/DesignConversionTypes.ts`, `tests/design-conversion-client.test.ts`; untracked `tests/design-conversion-live-local.test.ts`.
- C:\Projects\CanvaToCode current status readback: `## DesignToCode...origin/DesignToCode`; modified `src/platform/design-conversion-client.ts`, `src/platform/design-conversion-types.ts`, `tests/design-conversion-client.test.ts`; untracked `tests/design-conversion-live-local.test.ts`.

No command in this pass wrote to C:\Projects\FigmaToCode-design-gen, C:\Projects\CanvaToCode, C:\Users\lubao\Documents\xiigen, protected platform/FLOW source repos, or git publication targets.

## artifact_verification

Post-write verification command results:

| Check | Exit code | Result |
|---|---:|---|
| Exact field count for the subagent field | 0 | Count = 1. |
| Exact field count for the Pass176 verdict field | 0 | Count = 1. |
| Line-anchored conflict-marker scan | 0 | Count = 0. |
| Configured mojibake marker scan | 0 | Count = 0 for the configured UTF-8/Windows-1252 corruption markers. |
| `git diff --check -- docs/evidence/design-conversion/codex-pass176-caller-clean-install.md` | 0 | Clean. |
| Direct readback by `Select-String` over verification, remaining gaps, verdict, downgrade, and Figma caveat lines | 0 | Readback showed the bounded claim and caveats in this artifact. |
| `git status --short --branch -- docs/evidence/design-conversion/codex-pass176-caller-clean-install.md` | 0 | Artifact is untracked in `llm_mvp_core`; no staging, commit, push, merge, or PR was run. |
| Post-test process sanity and exact cleanup | 0 | Found seven stale MSBuild node-reuse `dotnet` workers from the live-local test window, stopped exact matching PIDs 4720, 7060, 8628, 11444, 15636, 26744, and 27980, then verified no listed PID remained. |

## remaining_gaps

- This is bounded local safe-copy evidence only.
- Figma default `npm ci` with the copied Pass175 lock did not pass; it required clean-copy lock repair.
- Figma package restore used `--strict-ssl=false` because npm registry access hit certificate chain errors in this environment.
- Figma package restore used `PUPPETEER_SKIP_DOWNLOAD=1` after Puppeteer chrome-headless-shell download failed certificate verification. This pass does not prove Puppeteer browser binary download.
- No cloud deploy, public endpoint, external provider call, production browser-farm proof, CI, source repo materialization, commit, push, merge, or PR was run.

pass176_verdict: PASS_CALLER_CLEAN_INSTALL_BOUNDED_SAFE_COPY
downgrade_needed: No downgrade for bounded local caller clean package restore plus focused tests/builds in Pass176 safe copies. Downgrade remains required for default Figma copied-lock install, Figma bundled browser download, ProductReady, cloud/deploy/public endpoint/provider/production readiness, CI, source repo publication/materialization, commit, push, merge, PR, or broad UI/runtime coverage.
