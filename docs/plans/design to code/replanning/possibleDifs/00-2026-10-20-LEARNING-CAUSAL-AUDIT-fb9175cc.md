# Добавочный аудит обучения RP01 — исходный fb9175cc и уточнение d962f958

**Текущий NEXT: №10 PARENT-ADOPTION-01 → №11 N63-INGRESS-LENGTH-LEARNED-01 → №12 N63-CORRECTION-COLLISION.** Только документация. [Основной отчёт](2026-10-20-learning-causal-audit-fb9175cc/README.md) содержит source audit, сохранённые полезные изменения и регрессии, результаты соседних веток и две новые гипотезы SiLU/HNSW.

[Актуальный реестр](2026-10-20-learning-causal-audit-fb9175cc/04-registry.md):47coreID+2новых=49; три прежних предложения уже включены в core и повторно не считаются. [Позднее уточнение d962f958](2026-10-20-learning-causal-audit-fb9175cc/10-late-core-delta-d962f958.md) документирует частичный N63C:1DPO+1SFT вA, поздний RNG-save failure, ошибочные0counters, полезные NLL/rank/margin изменения и незавершённую A/B цель. Старые raw/claim сохраняются, повтор не назначен.

[Измерения и кандидаты](2026-10-20-learning-causal-audit-fb9175cc/02-saved-results-and-candidates.md), [исходная проверка163карточек и8prompts](2026-10-20-learning-causal-audit-fb9175cc/03-card-contract-audit.md), [успешная соседняя ветка и доменные ограничения](2026-10-20-learning-causal-audit-fb9175cc/08-knn-history-and-source-audit.md).

Protected core, production и старые эксперименты этим аудитом не менялись; новое обучение не запускалось. Полезная измеренная метрика сохраняется успехом своей оси/домена с ухудшениями рядом.0→0полныхответов не закрывает учебную цель. По live NEXT/STOP предыдущая работа прекращается немедленно, без report/hash/review/Git/retry.

**Конечный срез core —256f603e.** [Последнее уточнение](2026-10-20-learning-causal-audit-fb9175cc/11-final-cutoff-256f603e.md) сохраняет отдельно последующий A/B N63C-PROBE и уже признанный положительный candidate; прежний A-only запуск не переименован в этот probe.
