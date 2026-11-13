# TOKEN-03 baseline/adaptive trace report

Verdict: BLOCKED_REAL_TRACES_UNAVAILABLE_STATIC_RUNNER_INGEST_WRITTEN_NO_TOKEN_CLAIM

Counts:
- Scenarios: 12
- Traces: 108
- Pairs: 54
- Real pairs: 0
- Static pairs: 54
- Modes: baseline_common_model_only, adaptive_common_plus_user_model

Pairing fields:
- Preserved: true
- Required fields: scenario_id, host, method, debug_enabled, prompt_sha256, tokenizer_name, tokenizer_version, tokenizer_accounting_type
- Same scenario set used: true

Quality/token gate:
- quality_non_regression_passed: false
- token_savings_claim_allowed: false
- token improvement claim: FORBIDDEN

Gap:
Only static TOKEN-02 runner placeholder traces were available for the exact TOKEN-01 scenario pack. No live or ingested real baseline/adaptive host outputs exist for the full current scenario set, so quality non-regression cannot pass and token savings cannot be claimed.

Next work order:
TOKEN-03-LIVE-TRACE-CAPTURE: run the current scenario-pack-token-quality-v1.jsonl through live Claude/Cursor/Codex host/runtime paths or ingest equivalent real host traces, capturing baseline and adaptive outputs for identical scenario_id, host, method, debug_enabled, tokenizer_name, tokenizer_version, tokenizer_accounting_type, and prompt_sha256; then run real quality scoring before any token improvement claim.
