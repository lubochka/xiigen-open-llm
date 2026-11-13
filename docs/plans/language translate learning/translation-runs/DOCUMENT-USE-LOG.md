# Document Use Log — Translation Learning Cycle
# Written: 2026-05-11
# Cycle: router-r1-direction/batch-001
# Prior state: no prior runs

## Reading Order

All 11 required documents were read in order before any pair was constructed.
No pair was written before this log was completed.

| # | Document | Version | Purpose | Read |
|---|---|---|---|---|
| 1 | TRANSLATION-SMALL-MODEL-RATIONALE-v1.md | 1.0 | Motivation: why small model + routing is sufficient | yes |
| 2 | TRANSLATION-SOURCE-REGISTRY-v3.md | 3.0 | Source authority decisions; v1 and v2 are historical only | yes |
| 3 | TRANSLATION-TOPIC-ROUTER-ORCHESTRATION-PLAN-v1.md | 1.0 | Routing architecture, adapter IDs, topic domains, measurement gates | yes |
| 4 | TRANSLATION-ROUTING-FLOW-DPO-GUIDANCE-v1.md | 1.0 | Router DPO pair shape, R1 batch rules, promotion gates | yes |
| 5 | TRANSLATION-FOUR-LEVEL-LLM-TRAINING-PLAN-v1.md | 1.0 | Difficulty ladder, pair mix per level, stream schedule | yes |
| 6 | TRANSLATION-50-PAIR-ABLATION-RUNBOOK-v1.md | 1.0 | Batch mechanics, source inventory, split rules, decision states | yes |
| 7 | TRANSLATION-DPO-REGISTRY-GUIDE-v1.md | 1.0 | Pair families T01-T09, pair shape requirements, hard gates | yes |
| 8 | EXAMINATION-METHOD-TRANSLATION-v1.md | 1.0 | Row-by-row qualification, Layer 0-A/0-B/1/2/3/4/5, failure modes | yes |
| 9 | TRANSLATION-DPO-REJECTED-PROMPTS-v1.md | 1.0 | Rejected side construction; Prompts 1-4 for create/audit/repair | yes |
| 10 | TRANSLATION-TOKENIZER-GUIDE-v1.md | 1.0 | Cyrillic/Hebrew/Latin coverage; length limits pre-tokenizer-extension | yes |
| 11 | TRANSLATION-LOOK-LIKE-RUN-REPORT-TEMPLATE-v1.md | 1.0 | Report format for LOOK-LIKE-RUN-REPORT.md | yes |

## Authority Decisions Applied

- Source authority: TRANSLATION-SOURCE-REGISTRY-v3.md (v3 is canonical; v1 and v2 are stale)
- First-run sources: TR-S01 Tatoeba (RU-EN, HE-EN) and reverse directions
- TR-S03 Knesset: support only, not chosen-side authority
- TR-S04 UN Corpus: not used in R1 (R1 is direction detection, no legal_diplomatic yet)
- All REVIEW_REQUIRED, QUARANTINE, and BLOCK sources: excluded from train-ready

## Token Length Gates Applied

Tokenizer state: Phase 1 (pre-tokenizer-extension, byte-level fallback for Cyrillic/Hebrew).
- Hebrew sentences: maximum 15 source words per EXAMINATION-METHOD-TRANSLATION-v1.md Layer 0-A
- Russian sentences: maximum 20 source words
- All candidate sentences verified under these limits before pair construction

## Routing Contract

All router R1 pairs target:
  TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>

Internal node responsible for R1: TranslationDirectionRouterUnit
