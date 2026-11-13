# Cursor row12 token trace measurement

Статус: TOKEN_SAVINGS_MEASURED_CURSOR_ROW12.

Запущенная команда:
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "C:\Projects\llm_mvp_core\tools\frdp-row12-cursor-token-trace-harness.ps1" -CoreRoot "C:\Projects\llm_mvp_core" -SpecPath "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\row12-workload-spec.json" -MeasurementRoot "C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\token-savings-cursor-row12-20260628-153837"

Exit code: 0.

Что измерено: узкий no-send row12 путь Cursor/core API из row12-workload-spec.json через tools/frdp-row12-cursor-token-trace-harness.ps1. Harness создал paired baseline/adaptive token traces и pair-validation.json.

Числа: before_tokens=6209, after_tokens=6209, delta_tokens=0 (after_minus_before), delta_percent=0. Наблюдаемого уменьшения токенов нет: token_reduction_observed=false.

Что это доказывает узко: для одного синтетического row12 workload baseline/adaptive traces сопоставимы по scenario_id, pair_id, prompt_hash и tokenizer/accounting method; внешний AI не отправлялся; quality/readiness/training/checkpoint claims выключены.

Что это НЕ доказывает: экономию токенов как улучшение, semantic quality improvement, learning, training, DPO, checkpoint promotion, product readiness, full Cursor host readiness или host-visible Cursor сценарий.

Основные файлы: measurement.json, result.json, pair-validation.json, baseline-trace.json, adaptive-trace.json, stdout.log, stderr.log.
