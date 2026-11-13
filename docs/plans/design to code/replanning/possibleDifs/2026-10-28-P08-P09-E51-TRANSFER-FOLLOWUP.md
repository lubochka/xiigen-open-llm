# 28.10.2026. P08/P09/E51: актуальный остаток переноса

Исследовательская фаза: чтение источников и Markdown, без обучения, тестов, сборок или изменения продукта. Срез последнего изученного HEAD203ba60fc487ce47fe3415b0a4abb7ce95bc13e4.

- [Аудит91](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/91-positive-transfer-followup-audit-2026-10-28.md): P02 writer и native sampler уже исправлены; P08 gold probabilityT1+15,10% сохраняется при2/4ответах, но managed allocations+155,65% ставят общий перенос на HOLD. Старое NOT_MEASURED ресурса снято.
- [План92](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/92-positive-transfer-followup-plan-2026-10-28.md): четыре настоящих restored-S1→native-sampler проверки вместо carrier logits; learned cache7/7/7; независимый CDF oracle; measured/structural receipt; E51-specific квалификация всех writer путей. Это планы, ни один не исполнялся аудитором.
- [Охват93](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/93-positive-followup-card-applicability-2026-10-28.md): адресные условия внесены в56файлов/62ID, без заявления о62кодовых аудитах.
- [Пакет источников worker](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/reviews/2026-10-28-positive-candidate-delta-source-audit.md): P13 A=B, новые algorithm FIX не допущены; локальные соседние ветки не содержат нового незлитого пакета после15.10. Старые E51 overlap/oracle/baseline разобраны ранее в11/15/01a. Новое назначение — ремонт E51report qualification, не повторное открытие старой гипотезы.

Приоритет P12/88 сохранён. Никакие старые raw/эксперименты не удалены; общая маршрутизация остаётся у74. Граница доказательства и concrete code paths находятся в91/92, исходные метрики — по ссылкам на первичные receipt.

