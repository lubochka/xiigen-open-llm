# Parent Review RUNTIME-03 - Cursor Runtime QA

Review sub-agent: REVIEW-RUNTIME-03

Status: **ACCEPTED FOR PHASE COMMIT AS IN_PROGRESS_NARROW_EVIDENCE_ONLY**.

This review accepts the current RUNTIME-03 artifacts only as narrow Cursor Extension Host command-path evidence. It does not close full Cursor readiness, full RUNTIME-03 proof, token quality, learning, training, checkpoint, target promotion, rollback, chat participant UI, panel UI, screenshot, or full plan readiness.

## Evidence Checked

- `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\cursor-runtime-proof.md`
- `C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\cursor-runtime-proof.json`
- `C:\Projects\llm_mvp_core\docs\plans\EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`
- `C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-210605\runtime03-launch-summary.json`
- `C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-210605\runtime03-extension-host-report.json`
- `C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-210605\extension-host-evidence\xiigen-cursor-xiigen.smartSkills.select-request-2026-06-16T18-07-00-867Z.json`
- `C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-210605\extension-host-evidence\xiigen-cursor-xiigen.smartSkills.select-core-response-2026-06-16T18-07-00-867Z.json`
- `C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-210605\extension-host-evidence\xiigen-cursor-xiigen.smartSkills.select-core-trace-2026-06-16T18-07-00-867Z.json`
- `C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-210605\extension-host-evidence\xiigen-cursor-xiigen.smartSkills.select-host-trace-2026-06-16T18-07-00-867Z.json`
- `C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-210605\extension-host-evidence\xiigen-cursor-xiigen.smartSkills.select-entrypoint-response-2026-06-16T18-07-00-867Z.json`
- `C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-210605\extension-host-evidence\xiigen-cursor-xiigen.smartSkills.select-request-2026-06-16T18-07-38-714Z.json`
- `C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-210605\extension-host-evidence\xiigen-cursor-xiigen.smartSkills.select-host-trace-2026-06-16T18-07-38-714Z.json`
- `C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-210605\extension-host-evidence\xiigen-cursor-xiigen.smartSkills.select-entrypoint-response-2026-06-16T18-07-38-714Z.json`
- `C:\Projects\cursor-xiigen-smart-skills\src\extension.ts`

## Findings

1. Positive Cursor evidence is real Extension Host command-path evidence, not helper/dashboard/direct-handler evidence. `Cursor.exe` launched with `--extensionDevelopmentPath C:\Projects\cursor-xiigen-smart-skills` and `--extensionTestsPath ...\runtime03-extensionHostQA.js`; `runtime03-extension-host-report.json` records `extension_host_loaded=true`, `extension_activated=true`, and command execution through `xiigen.smartSkills.select`.

2. The positive request reached the llm_mvp_core core boundary. The positive core trace names `SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest`, and the positive response/trace physical files exist in the `20260616-210605` Extension Host evidence folder.

3. The negative path is incomplete. The controlled missing-core invocation surfaced `CONTRACT_ONLY_DOWNGRADE` through the same command entrypoint, but the expected negative physical core response and core trace files are absent:

```text
C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-210605\extension-host-evidence\xiigen-cursor-xiigen.smartSkills.select-core-response-2026-06-16T18-07-38-714Z.json
C:\Projects\cursor-xiigen-smart-skills\reports\RUNTIME-03-cursor\20260616-210605\extension-host-evidence\xiigen-cursor-xiigen.smartSkills.select-core-trace-2026-06-16T18-07-38-714Z.json
```

4. The next repair should be `HOST-IMPL-02`, not a pure `RUNTIME-03B` rerun. In `src\extension.ts`, the missing-core branch builds an in-memory `buildCoreMissingResponse(...)` and resolves before writing the physical `responseRef` or `coreTraceRef`. The missing physical negative files therefore need a Cursor adapter or harness repair under explicit product-code authorization before the Cursor Extension Host proof is rerun.

5. No full Cursor readiness claim is accepted. The reviewed proof stays `IN_PROGRESS_NARROW_EVIDENCE_ONLY`, with `runtimeProofAcceptedCandidate=false` in the proof JSON and `FAIL_DEFERRED_RUNTIME_PROOF_INCOMPLETE` in the Cursor host report.

6. Architecture review found no hidden model logic moved into the Cursor adapter. The adapter records host facts, delegates to `tools\xiigen-extension-core-api.ps1` with PowerShell, writes host/request/response references, and explicitly records `host_semantic_ranking=false`, `host_training=false`, `semanticQualityClaimed=false`, and no checkpoint/training/eval behavior.

## Verdict

`accepted_for_phase_commit=true` only for the narrow evidence/status repair packet:

```text
accepted_status = IN_PROGRESS_NARROW_EVIDENCE_ONLY
full_runtime_pass = false
full_cursor_readiness = false
exact_next_work_order = HOST-IMPL-02
```

Exact next work order: `HOST-IMPL-02` should repair the Cursor adapter/harness so the controlled missing-core path writes physical negative core response and core trace files, without moving model logic into Cursor and without training/eval/checkpoint mutation. After that repair is reviewed and committed, rerun RUNTIME-03 Cursor Extension Host proof.

## Verification

- JSON parse for proof, review, state, launch summary, and Cursor host report: PASS
- `rg` checks for Extension Host, `CONTRACT_ONLY_DOWNGRADE`, missing physical negative trace, `IN_PROGRESS_NARROW_EVIDENCE_ONLY`, and no full readiness claim: PASS
- `git diff --check`: PASS
