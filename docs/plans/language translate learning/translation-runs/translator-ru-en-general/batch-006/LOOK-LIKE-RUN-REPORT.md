# Look-Like-Run Report — translator-ru-en-general batch-006

## Run Identity

- stream: translator-ru-en-general
- batch_index: 6
- direction: RU_EN
- domain: general
- run_date: 2026-05-11

## What Was Built

- 50 DPO translation pairs (TR-RU-EN-GEN-0601 to TR-RU-EN-GEN-0650)
- batch_focus: health and medicine (general register)

## Gate Result

| Metric | Value | Threshold | Status |
|---|---|---|---|
| structural_accuracy | 1.00 | ≥0.99 | ACCEPTED |
| valid / sample | 20 / 20 | — | — |

## Inference Test

TranslatorStreamBatchTests.RuEnGeneral_Batch006_StructuralGatePasses — PASS
