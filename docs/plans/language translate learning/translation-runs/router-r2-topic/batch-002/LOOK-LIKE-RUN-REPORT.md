# Look-Like-Run Report — router-r2-topic batch-002

## Run Identity

- stream: router-r2-topic
- batch_index: 2
- router_level: R2
- run_date: 2026-05-11

## What Was Built

- 50 train DPO pairs (TR-ROUTE-R2-0201 to TR-ROUTE-R2-0250)
- 50 heldout rows (TR-ROUTE-R2-HO-0201 to TR-ROUTE-R2-HO-0250)
- batch_focus: harder_boundary_cases_and_single_keyword_signals

## Gate Result

| Metric | Value | Threshold | Status |
|---|---|---|---|
| topic_accuracy | 1.00 | ≥0.99 | ACCEPTED |
| correct / total | 50 / 50 | — | — |

## Direction Coverage

| Direction | Count | Accuracy |
|---|---|---|
| RU_EN | 13 | 1.00 |
| EN_RU | 12 | 1.00 |
| HE_EN | 10 | 1.00 |
| EN_HE | 15 | 1.00 |

## Topic Coverage

| Topic | Count | Accuracy |
|---|---|---|
| medical | 8 | 1.00 |
| legal | 8 | 1.00 |
| technical | 8 | 1.00 |
| cultural | 8 | 1.00 |
| personal | 8 | 1.00 |
| general | 10 | 1.00 |

## Key Validations in This Batch

- Single Cyrillic stem detection: болезн + операци = medical (score=2, confidence=0.92)
- Multi-keyword high-confidence: lawsuit + criminal + liability + clause = legal (score=4, confidence=0.97)
- Hebrew personal with preposition-attached keywords: לאהבה contains אהבה, למשפחה contains משפחה
- Technical single-keyword: code + bug + deploy + server = technical (score=4, confidence=0.97)
- General fallback (no domain keywords): office opens at eight = general (confidence=0.90)

## Inference Test

TranslationTopicRoutingFlowTests.R2Batch002_HeldOut50_TopicAccuracyMeetsGate — PASS
