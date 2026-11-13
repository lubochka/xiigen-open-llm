# Token pair 004 - Cursor prompt-to-task

Status: TOKEN_PAIR_004_COMPLETE_ACCOUNTING_ONLY_NO_SAVINGS_CLAIM

What this proves:
- Fresh same-contract baseline/adaptive token accounting refs were captured through direct llm_mvp_core core API for Cursor-labelled prompt-to-task.
- Pair 004 counts as accounting-only completion: true.

What this does not prove:
- This does not prove live Cursor UI, Cursor Extension Host, Cursor chat/runtime readiness, token savings, quality improvement, semantic quality, learnability, target promotion, locator update, final readiness, or product readiness.

Evidence refs:
- Baseline trace: artifacts/extension-runtime-token-quality-learnability/token-baseline-adaptive-comparison/token-baseline-adaptive-comparison-20260619-073940/baseline-common-no-adaptive/xiigen-core-trace.json
- Adaptive trace: artifacts/extension-runtime-token-quality-learnability/token-baseline-adaptive-comparison/token-baseline-adaptive-comparison-20260619-073940/adaptive-user-local-target/xiigen-core-trace.json
- Baseline request: artifacts/extension-runtime-token-quality-learnability/token-baseline-adaptive-comparison/token-baseline-adaptive-comparison-20260619-073940/baseline-common-no-adaptive/xiigen-cursor-core-request.json
- Adaptive request: artifacts/extension-runtime-token-quality-learnability/token-baseline-adaptive-comparison/token-baseline-adaptive-comparison-20260619-073940/adaptive-user-local-target/xiigen-cursor-core-request.json

Token counts:
- Baseline total input/output/trace: 568/549/1117
- Adaptive total input/output/trace: 1295/1497/2792

Method:
- Host label: Cursor.
- Method: prompt-to-task.
- Evidence class: direct_core_api_no_host_ui.
- Debug: false.
- Tokenizer/accounting: xiigen-token-accounting.v1, xiigen-heuristic-word-punctuation v1.
- Same prompt/method/debug/evidence/tokenizer/redaction contract: true.
- Quality non-regression: NOT_RUN, therefore token savings claim remains false.

Next independent track:
- TOKEN-PAIR-CURSOR-ORCHESTRATION-PLAN-BASELINE-ADAPTIVE-TRACE-CAPTURE-OR-DEFER-08.
