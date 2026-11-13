# Look-Like-Run Report — translator-ru-en-general batch-001

## Run Identity

- stream: translator-ru-en-general
- batch_index: 1
- direction: RU_EN
- domain: general
- run_date: 2026-05-11

## What Was Built

- 50 DPO translation pairs (TR-RU-EN-GEN-0101 to TR-RU-EN-GEN-0150)
- batch_focus: daily life contexts — dative constructions, common grammar patterns

## Gate Result

| Metric | Value | Threshold | Status |
|---|---|---|---|
| structural_accuracy | 1.00 | ≥0.99 | ACCEPTED |
| valid / sample | 20 / 20 | — | — |

## Defect Coverage

| Defect Type | Count |
|---|---|
| literal_translation | 8 |
| grammar_error | 8 |
| wrong_register | 7 |
| missing_idiom | 7 |
| false_friend | 6 |
| omission | 7 |
| wrong_word_choice | 7 |

## Inference Test

TranslatorStreamBatchTests.RuEnGeneral_Batch001_StructuralGatePasses — PASS
