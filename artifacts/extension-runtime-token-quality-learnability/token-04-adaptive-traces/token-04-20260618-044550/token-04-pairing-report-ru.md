# TOKEN-04: pairing report

## Кратко

TOKEN-04 создал adaptive-side handler-level traces для всех 54 TOKEN-01/TOKEN-03 `pair_id` values. Missing adaptive trace rows: 0. Missing baseline rows: 0.

Pairing готов только как scoped handler-level input для QUALITY-03 review. Он не готов для token savings, live-host savings, semantic quality, learnability, production promotion, rollback closure или final readiness.

## Проверка целостности

| Проверка | Количество |
| --- | ---: |
| Baseline rows available | 54 |
| Adaptive rows captured | 54 |
| Same pair id rows | 54 |
| Same prompt hash rows | 54 |
| Same host rows | 54 |
| Same method rows | 54 |
| Same tokenizer/accounting rows | 54 |
| Same redaction policy rows | 54 |
| QUALITY-03 passed rows | 0 |
| Token savings claim allowed rows | 0 |

## Failure / deferred labels

| Label | Количество |
| --- | ---: |
| `DASHBOARD_API_HELPER_HANDLER_ONLY_TOKEN_TRACE` | 54 |
| `MISSING_LIVE_HOST_BASELINE_TRACE_FOR_LIVE_SAVINGS` | 54 |
| `ORCHESTRATION_USER_ADAPTIVE_ASSET_MISSING_COMMON_FALLBACK_USED` | 54 |
| `QUALITY_NON_REGRESSION_NOT_PASSED` | 54 |
| `SCOPED_TEST_ONLY_STAGED_TARGET_NOT_PRODUCTION_LOCATOR` | 54 |

## Почему claim закрыт

Claim остается закрытым, потому что QUALITY-03 same-pair non-regression отсутствует, evidence является только handler-level, а adaptive target - это QSG28 staged/test-only locator, не production locator evidence.
