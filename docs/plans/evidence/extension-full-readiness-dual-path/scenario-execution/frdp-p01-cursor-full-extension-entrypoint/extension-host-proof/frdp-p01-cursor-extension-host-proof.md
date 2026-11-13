# FRDP P01 Cursor extension-host proof

CURRENT TASK: prove the safe no-send Cursor Extension Host command path for row 11.
NOT THE TASK: product readiness, full Cursor extension readiness, manual UI screenshot, token savings, semantic quality, training, checkpoint mutation, locator update, stage, commit, or push.
EVIDENCE-ONLY: this proves a bounded Extension Host command handler path for the scenario marker.

## Superseded / quarantined status

This historical proof folder is superseded by the Phase 1B proof at `docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-cursor-full-extension-entrypoint/extension-host-proof-phase1b/run-20260624T1805097700936Z/frdp-p01-cursor-extension-host-proof.json`.

Current row 11 status remains partial because user interface / пользовательский интерфейс answer display is missing. Full joined pass remains `0 -> 0`; partial remains `1 -> 1`.

## Result

Scenario: 11, frdp-p01-cursor-full-extension-entrypoint.
Marker: FRDP-P01-frdp-p01-cursor-full-extension-entrypoint.
Verdict: extension_host_command_path_reached_core_api.

## Checks

- Cursor Extension Host command invoked: true.
- Runtime proof command: xiigenSmartSkills.runtimeProof.invoke.
- Method: orchestration-plan.
- Same marker in correlation: true.
- Same marker in prompt: true.
- Core API path reached: true.
- Same-run correlation matched: true.
- Same-run core correlation proven: true.
- External AI or unrelated tool used by extension result: false.
- Semantic quality claimed: false.
- Full manual Cursor UI claimed: false.

## Evidence

- Request: docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-cursor-full-extension-entrypoint/extension-host-proof/extension-host-evidence/xiigen-cursor-xiigenSmartSkills.runtimeProof.invoke-FRDP-P01-frdp-p01-cursor-full-extension-entrypoint-extension-host-no-send-positive_request-request-2026-06-24T17-54-30-058Z.json.
- Core response: docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-cursor-full-extension-entrypoint/extension-host-proof/extension-host-evidence/xiigen-cursor-xiigenSmartSkills.runtimeProof.invoke-FRDP-P01-frdp-p01-cursor-full-extension-entrypoint-extension-host-no-send-positive_request-core-response-2026-06-24T17-54-30-058Z.json.
- Core trace: docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-cursor-full-extension-entrypoint/extension-host-proof/extension-host-evidence/xiigen-cursor-xiigenSmartSkills.runtimeProof.invoke-FRDP-P01-frdp-p01-cursor-full-extension-entrypoint-extension-host-no-send-positive_request-core-trace-2026-06-24T17-54-30-058Z.json.
- Host trace: docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-cursor-full-extension-entrypoint/extension-host-proof/extension-host-evidence/xiigen-cursor-xiigenSmartSkills.runtimeProof.invoke-FRDP-P01-frdp-p01-cursor-full-extension-entrypoint-extension-host-no-send-positive_request-host-trace-2026-06-24T17-54-30-058Z.json.
- Entrypoint response: docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-cursor-full-extension-entrypoint/extension-host-proof/extension-host-evidence/xiigen-cursor-xiigenSmartSkills.runtimeProof.invoke-FRDP-P01-frdp-p01-cursor-full-extension-entrypoint-extension-host-no-send-positive_request-entrypoint-response-2026-06-24T17-54-30-058Z.json.
- Redacted trace: docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-cursor-full-extension-entrypoint/extension-host-proof/extension-host-evidence/xiigen-cursor-xiigenSmartSkills.runtimeProof.invoke-FRDP-P01-frdp-p01-cursor-full-extension-entrypoint-extension-host-no-send-positive_request-redacted-trace-2026-06-24T17-54-30-058Z.json.
- Harness report JSON: docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-cursor-full-extension-entrypoint/extension-host-proof/frdp-p01-cursor-extension-host-proof.json.

## Join decision

Superseded by the Phase 1B proof. Extension-host command path, same core API path, and existing local Cursor marker-file route are present for the row 11 marker, but user interface / пользовательский интерфейс answer display is missing. This supports partial row evidence only; it does not support a formal joined pass, product readiness, or full Cursor product readiness.

Counts if accepted in the board:

- full joined pass: 0 -> 0
- partial: 1 -> 1
- not moved: 8 -> 8
- blocked with next action: 19 -> 19
- readiness claims opened: 0 -> 0

## Non-claims

- Product readiness is not claimed.
- Full Cursor extension/product readiness is not claimed.
- Manual Cursor UI screenshot proof is not claimed.
- User interface / пользовательский интерфейс answer display is not proven.
- Formal joined pass is not claimed.
- Token savings, semantic quality improvement, learnability, training, checkpoint promotion, and locator update are not claimed.
