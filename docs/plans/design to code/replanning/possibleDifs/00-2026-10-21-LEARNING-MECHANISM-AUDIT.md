# 21 октября: аудит обучения, актуальные результаты и новый POS-фактор

Вход в пакет: **[2026-10-21-learning-mechanism-audit/README.md](2026-10-21-learning-mechanism-audit/README.md)**.

Финальный source cutoff: core `003083feab72157e54184bb66d0d940f48d836fd`, planning `8346dfb697be43041a477886db26c508453fe5ce`. Учебные source/raw закреплены на core256; delta003 уточняет правила ожидания. Исходный d962/fc0dd аудит сохранён исторически и согласован с поздними изменениями.

- [Последняя сверка](2026-10-21-learning-mechanism-audit/08-latest-reconciliation.md): completed N63C-PROBE с полезным частичным результатом, исправленные карточки, оставшиеся source/result и readiness-разрывы.
- [SFT↔DPO carrier](2026-10-21-learning-mechanism-audit/09-n63c-framing.md): конкретный обучающий и измерительный контекст нового probe различаются; causal effect ещё не измерен, семья P07 известна.
- [Карта механизмов](2026-10-21-learning-mechanism-audit/01-learning-paths.md): DPO/LoRA, токены, восстановление, DPO→Units, kNN/AST/POS и доменные границы.
- [Актуальный реестр](2026-10-21-learning-mechanism-audit/04-registry.md): 50 canonical ID = 47 core + уже опубликованные SiLU/HNSW + новый POS. SiLU-DENOM объединён с SILU-SIGMOID, второй опыт не создаётся.
- [Результаты и кандидаты](2026-10-21-learning-mechanism-audit/05-results-and-candidates.md): полезные измерения сохраняются вместе с ухудшениями.
- [Новый POS native Train→Predict рецепт](2026-10-21-learning-mechanism-audit/cards/POS-QUERY-SET-ONCE-01.md): 2 Train, 128 Predict, один фактор query-set reuse; последний приоритет, NOT_RUN.
- [Последнее уточнение ожидания](2026-10-21-learning-mechanism-audit/reviews/latest-wait-reconciliation.md): discovery фактических схем и same-handle wait до исходного deadline.
- [Источники и независимая проверка](2026-10-21-learning-mechanism-audit/07-source-and-review.md): pins, навыки, source/raw receipts и пределы.

Только добавочные документы. Защищённый core, production, старые experiments и raw не менялись. Runtime, сборок, тестов и обучения не было.
