# QUALITY-03 10I report

## Короткий вывод
10I нашёл локальное основание для строки `QUALITY-03-REVIEW-ROW-001`: она привязана к диагностическому trace `TOKEN-03-CLAUDE-PROMPT-TO-TASK-NARROW-ADAPTIVE-TRACE-001` в `token-traces.jsonl`. Это закрывает только missing `source_scenario_id/source_ref/doc_id/source_hash` для row 001.

Это не даёт source/license/consent approval. Все строки остаются `DEFERRED_NOT_APPROVED`; common training закрыт; raw-private replay не одобрен; proof bundle, producer, runner, training, DPO, checkpoint, model, locator и target mutation не выполнялись.

## Counts
Before: 63 proof refs; PASS 9/63; non-inter-rater 0/54; runner-ready 0/9; quality-pass 0/9; source bindings 8/9; approved row assignments 0/9.

After: 63 proof refs; PASS 9/63; non-inter-rater 0/54; runner-ready 0/9; quality-pass 0/9; source bindings 9/9; approved row assignments 0/9.

## Что supplied
- Row 001 source binding: `artifacts/extension-runtime-token-quality-learnability/quality-scoring/quality-scoring-20260619-183510/quality-03-10i-row-001-source-binding.json`.
- Binding source: `artifacts/extension-runtime-token-quality-learnability/token-03-narrow-trace-capture-or-defer/token-03-narrow-trace-capture-or-defer-20260618-224207/token-traces.jsonl`, hash `sha256:5c102084f2a5be9b48c24de521bc05bc429510034be8269d59200fd0e7684c24`.
- Supporting redacted request: `artifacts/extension-runtime-token-quality-learnability/runtime-claude-full-chat-ui-no-eval-proof/runtime-claude-full-chat-ui-no-eval-proof-20260618-190216/positive-debug-on/xiigen-claude-core-request.json`, hash `sha256:69f1610afe25919ed16d79b2206081ab1a8ca7031d640b5448d460d5f6da9a7b`.
- Supporting proof packet: `artifacts/extension-runtime-token-quality-learnability/runtime-claude-full-chat-ui-no-eval-proof/runtime-claude-full-chat-ui-no-eval-proof-20260618-190216/proof-packet.json`, hash `sha256:d889b9a0c351521715e075a3ecd63c683004aeb19bccd2c8a6813d51e854b9ea`.

## Что всё ещё missing
- `source_license_ref` и `license_hash` для каждой exact source group.
- Explicit current-owner/Luba consent или local provenance route для каждой source group.
- Approved train corpus и held-out split.
- Approved raw-private replay-only corpus ref.
- 9 approved row assignments.

## Next work order
`QUALITY-03-NON-INTER-RATER-USER-APPROVAL-OR-LOCAL-LICENSE-METADATA-SUPPLY-10J` записан в `quality-03-10j-next-work-order.md` и `.json`.
