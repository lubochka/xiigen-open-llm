# TOKEN pair 005 - Cursor orchestration-plan

Статус: pair 005 закрыта как учетная baseline/adaptive пара без claims

Всего нужно 9 baseline/adaptive token-пар. Сделано 5/9. Текущая pair 005 Cursor orchestration-plan закрыта как учетная пара. Осталось 4/9.

Что доказано:
- Созданы свежие direct-core refs для Cursor-labelled orchestration-plan baseline_common_no_adaptive и adaptive_user_local_target.
- Adaptive side использует TargetLocator на существующий user-local root и response показывает orchestration_reasoning_user_asset_loaded_first=true.
- Метод, prompt hash, debug=false, evidence class, tokenizer/accounting и redaction policy совпадают внутри пары.

Что не доказано:
- Это не live Cursor UI, не Cursor Extension Host и не chat/runtime proof.
- Нет claims про token savings, quality improvement, semantic quality, learnability, target promotion, locator update или product readiness.
- Quality non-regression не запускался, поэтому token delta не является успехом.

Refs:
- Token pair: artifacts/extension-runtime-token-quality-learnability/token-baseline-adaptive-comparison/token-baseline-adaptive-comparison-20260619-075005/token-pair-005.json
- Baseline trace: artifacts/extension-runtime-token-quality-learnability/token-baseline-adaptive-comparison/token-baseline-adaptive-comparison-20260619-075005/baseline-common-no-adaptive/xiigen-core-trace.json
- Adaptive trace: artifacts/extension-runtime-token-quality-learnability/token-baseline-adaptive-comparison/token-baseline-adaptive-comparison-20260619-075005/adaptive-user-local-target/xiigen-core-trace.json
- Matrix: artifacts/extension-runtime-token-quality-learnability/token-baseline-adaptive-comparison/token-baseline-adaptive-comparison-20260619-075005/token-comparison-matrix.json
- No-overclaim: artifacts/extension-runtime-token-quality-learnability/token-baseline-adaptive-comparison/token-baseline-adaptive-comparison-20260619-075005/token-no-overclaim.json
- Checks: artifacts/extension-runtime-token-quality-learnability/token-baseline-adaptive-comparison/token-baseline-adaptive-comparison-20260619-075005/checks-report.json

Следующий bounded work order: TOKEN-PAIR-CURSOR-SKILL-SELECTION-BASELINE-ADAPTIVE-TRACE-CAPTURE-OR-DEFER-09
