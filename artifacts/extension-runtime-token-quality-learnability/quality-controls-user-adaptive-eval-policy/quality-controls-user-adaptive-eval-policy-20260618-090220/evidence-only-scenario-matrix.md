# Evidence-only scenario matrix

Run: `quality-controls-user-adaptive-eval-policy-20260618-090220`

Покрытие: **5/12 scenarios** и **33/54 pair rows**. Все строки остаются `EVIDENCE_ONLY_NO_EVAL_OR_TRAINING_NOW`.

Почему eval/training нельзя начинать сейчас:

- эти строки являются control/review evidence, а не approved user-local eval inputs;
- нет current user-local eval-only approval, raw-private input ref и redacted evidence separation;
- runtime/token/no-overclaim/shortcut/final-readiness строки легко превращаются в overclaim, если их исполнить без отдельных gates;
- common route закрыт: accepted permissive/common source packets = 0;
- любые training/model/checkpoint/locator/common mutations запрещены.

## Scenario rows

| Row | Scenario | Current use |
|---:|---|---|
| 4 | `TQ-004-orchestration-runtime-proof` | runtime proof control only; no live-host readiness claim |
| 5 | `TQ-005-orchestration-token-pairing` | token pairing control only; no token savings claim |
| 10 | `TQ-010-mixed-no-overclaim` | no-overclaim control only |
| 11 | `TQ-011-mixed-shortcut-audit` | deterministic-shortcut audit control only |
| 12 | `TQ-012-mixed-final-readiness` | final readiness matrix control only; final readiness closed |

## Pair row count by scenario

| Scenario | Pair rows | Status |
|---|---:|---|
| `TQ-004-orchestration-runtime-proof` | 3 | evidence-only |
| `TQ-005-orchestration-token-pairing` | 3 | evidence-only |
| `TQ-010-mixed-no-overclaim` | 9 | evidence-only |
| `TQ-011-mixed-shortcut-audit` | 9 | evidence-only |
| `TQ-012-mixed-final-readiness` | 9 | evidence-only |

The complete 33 pair rows are canonical in `evidence-only-scenario-matrix.jsonl`.
