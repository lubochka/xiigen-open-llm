# Selected follow-up work order

PASS review selects: DOCS-QA-CURSOR-RUNTIME-PROOF-EXECUTION-PACKET-BUILD-NO-RUNTIME-NO-SCORING-NO-TRAINING-01.

Why: Claude runtime proof review passed and runtime_proof_done_count remains 1. Cursor and Codex runtime proofs are still not done. State shows Cursor support/preflight review exists, but it does not show a ready Cursor runtime execution/final-check packet. The safe next step is therefore a bounded Cursor execution-packet build before any Cursor runtime launch.

Scope locks for the next work order:

- no scoring or quality eval;
- no training, DPO, checkpoint, locator, target, or model mutation;
- no dotnet build/test;
- no external AI, web, model, or raw-private corpus reads;
- no git authority inside the artifact-producing sub-agent;
- parent performs commit/push only after review.
