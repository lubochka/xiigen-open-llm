# Document Use Log — translator-en-ru-general / batch-018
# Session: 2026-05-11
# Written before training per executor prompt Step 2

| Order | File | Decision derived |
|---|---|---|
| 1 | TRANSLATION-EXECUTOR-HARD-RESET-v1.md | Role locked: learning-set executor only. Mode: PAIRSET_READY_TRAINING_RUNNER_MISSING. No invented metrics. |
| 2 | TRANSLATION-SOURCE-REGISTRY-v3.md | Source TR-S01 Tatoeba: ACCEPT (CC-BY 2.0 FR). synthetic-seed-en-ru-v1: not registered — all prior batch-018 rows must be replaced. |
| 3 | TRANSLATION-TOPIC-ROUTER-ORCHESTRATION-PLAN-v1.md | This is a translator batch, not a router batch. Router R1 and R2 gates are satisfied per STREAM-STATE.json. Topic domain: general (D01). |
| 4 | TRANSLATION-ROUTING-FLOW-DPO-GUIDANCE-v1.md | Skipped — translator batch, not router batch. |
| 5 | TRANSLATION-FOUR-LEVEL-LLM-TRAINING-PLAN-v1.md | Level 1 pair mix: 35 T01, 5 T08, 5 T09, 5 corruption. Adapter: adapter-en-ru-general. |
| 6 | TRANSLATION-50-PAIR-ABLATION-RUNBOOK-v1.md | Output directory confirmed. Gate B requirement confirmed. |
| 7 | TRANSLATION-DPO-REGISTRY-GUIDE-v1.md | Pair families T01, T08, T09 assigned per decision tree. |
| 8 | EXAMINATION-METHOD-TRANSLATION-v1.md | Layer 0-A: TR-S01 ACCEPT. Layer 0-B: families assigned. Layers 1-5: pair construction rules applied. Length gate: max 20 source tokens (Phase 1). |
| 9 | TRANSLATION-DPO-REJECTED-PROMPTS-v1.md | Rejected sides constructed using Prompt 1. Each annotated with [WRONG: error_type - consequence]. |
| 10 | TRANSLATION-TOKENIZER-GUIDE-v1.md | Pre-condition: Cyrillic coverage target >= 95%. Tokenizer check deferred — runner required. TOKENIZER_CHECK_PENDING. |
| 11 | TRANSLATION-LOOK-LIKE-RUN-REPORT-TEMPLATE-v1.md | Template loaded. LOOK-LIKE-RUN-REPORT.md written after evaluation. |
