# TOKEN-03 Live Trace Capture Report

Verdict: PASS_BOUNDED_CODEX_APP_SERVER_SUBSET_CAPTURED_ADAPTIVE_MODEL_UNAVAILABLE_NO_TOKEN_CLAIM

Captured a small representative Codex app-server subset for two TOKEN-01 scenarios and two modes. This is real Codex app-server adapter/core evidence for the subset only. Adaptive request fields are present, but adaptive model effect is unavailable/unproven; no token improvement or quality non-regression is claimed.

Trace count by host/mode: Codex baseline=2, Codex adaptive-requested=2; Cursor baseline=0/adaptive=0; Claude baseline=0/adaptive=0.

Artifacts:
- artifacts/extension-runtime-token-quality-learnability/token-03-live-codex-app-server-traces.jsonl
- artifacts/extension-runtime-token-quality-learnability/token-03-live-codex-app-server-pair-results.jsonl
- artifacts/extension-runtime-token-quality-learnability/token-03-live-codex-app-server-quality-scoring-inputs.jsonl
- artifacts/extension-runtime-token-quality-learnability/token-03-live-capture-report.json

No-overclaim: token_savings_claim_allowed=false, quality_non_regression_passed=false, adaptive_model_unavailable=true, no training/eval/checkpoint mutation.

Next work order: TOKEN-03-LIVE-TRACE-CAPTURE-FULL-HOSTS: run the full TOKEN-01 scenario pack through Codex, Cursor, and Claude proven host/runtime paths with a real adaptive model path or explicitly separate adaptive-unavailable rows; then run quality scoring before any token improvement claim.
