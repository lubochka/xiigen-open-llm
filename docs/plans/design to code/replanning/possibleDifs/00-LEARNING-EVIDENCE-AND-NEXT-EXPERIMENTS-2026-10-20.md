# 20 октября: доказательства обучения и следующий невыполненный опыт

[Добавочный пакет](2026-10-20-learning-evidence-and-next-experiments/README.md).

**Следующий — LF-03, №07.** На окончательном срезе core827 №01–06 уже завершены, включая N62-R2 и E-OPT-STEP. В общей программе38 ID:6 completed,32 pending; [единственная действующая очередь](2026-10-20-learning-evidence-and-next-experiments/04-execution-registry.md). Новый запуск завершённых опытов не назначен.

Добавлены два точных tensor-фактора с карточками/промптами и действительным DPO before/after; условный repeated-Train E07 оставлен вне очереди. [Шесть сохранённых результатов и original RP01](2026-10-20-learning-evidence-and-next-experiments/02-results-and-candidates.md) показывают полезные метрики вместе с ухудшениями. В original RP01 preference margin вырос на обоих доменах при прежних неправильных свободных ответах.

[Позднее согласование669](2026-10-20-learning-evidence-and-next-experiments/07-core669-status-and-results.md) отделяет уже исправленные P54/NG01/R1 от оставшихся gaps и закрепляет новый NG01 host. Новые гипотезы не выдаются за измеренные результаты.

Этот пакет только добавляет документы в possibleDifs. Защищённая папка, production и старые файлы этой работой не менялись; обучения в этом аудите не было.

[Окончательный срез827](2026-10-20-learning-evidence-and-next-experiments/08-final-core827.md): E-OPT raw называет фактор из R1 при другом факторе в отчёте; полезные дельты сохранены, attribution отдельно ограничена.
