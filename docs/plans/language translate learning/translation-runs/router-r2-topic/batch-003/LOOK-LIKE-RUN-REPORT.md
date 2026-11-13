# Look-Like-Run Report — router-r2-topic batch-003

## Run Identity

- stream: router-r2-topic
- batch_index: 3
- router_level: R2
- run_date: 2026-05-11

## What Was Built

- 50 train DPO pairs (TR-ROUTE-R2-0301 to TR-ROUTE-R2-0350)
- 50 heldout rows (TR-ROUTE-R2-HO-0301 to TR-ROUTE-R2-HO-0350)
- batch_focus: stress_test_edge_cases_and_domain_overlap_phrases

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

- Domain overlap: personal(emotion+wedding=2) beats cultural(ceremony=1) — personal wins
- Multi-keyword Latin legal: contract+clause → legal (score=2, confidence=0.92)
- Hebrew construct form: "לתרופות" contains "תרופ" → medical detected
- Hebrew legal forms: "הנאשם" contains "נאשם", "השופט" contains "שופט"
- General fallback confirmed for plain-language store/weather sentences

## Inference Test

TranslationTopicRoutingFlowTests.R2Batch003_HeldOut50_TopicAccuracyMeetsGate — PASS

## Stream Status

COMPLETE — 150/150 target pairs reached across 3 accepted batches.
