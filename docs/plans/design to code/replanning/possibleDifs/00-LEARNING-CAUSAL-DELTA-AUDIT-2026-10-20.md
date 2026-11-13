# Аудит причинных различий обучения — 20 октября 2026

Добавочный пакет: **[2026-10-20-learning-causal-delta-audit](2026-10-20-learning-causal-delta-audit/README.md)**.

Проверенные срезы: core **65d03a73a76810a0425a41c33d3c7eeaf4cdbc48**, planning основной срез **a868f46ee3695728406173f8cd945e278ddb0def**, конечная дельта **8aa040fc676973154e009e0eec13689eb9f6bd1d**.

- **137 актуальных карточек / 470 P-подслучаев**, с точными source/blob и ограничениями структурной проверки.
- Подтверждены полезные результаты N-KU, EMB/MASK, P125, phrase ingress, cold restore и исторических KNN/replay, с ухудшениями рядом. Уже внесённые fixes не предлагаются повторно.
- **Новая RMS-COORD-ALLOC-01**: per-element coordinate allocations внутри реального SFT/LoRA update; полный маленький рецепт, manifest36 файлов, executor prompt и независимый review. Статус NOT_RUN.
- DPO content vs markers, KNN labels/geometry/codec, DPO→Unit material, memory/Flow, retention и restore сведены в одну карту с доказательствами и existing coverage.
- Учтён свежий core commit: stop/next уже есть в137/137, добавлены N63 и укорочены пути. Устаревшие исправления не дублируются.
- Корпус поиска новизны:720 planning +303 protected Markdown и late patch aggregate;24 последних planning Markdown учтены отдельной сверкой. Полнотекстовый поиск не выдан за ручную аттестацию каждого опыта.

**Нового обучения, build/tests, изменения продукта и записи в защищённую папку не было.** Новые документы добавляются; старые эксперименты сохраняются. Успешные результаты представлены как кандидаты для решения пользователя, без автоматического переноса в продукт.

