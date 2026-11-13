# Сверка публикации с новым HEAD planning

Перед обновлением ветки появился сторонний commit **8aa040fc676973154e009e0eec13689eb9f6bd1d** поверх ранее проверенного a868f46. Первая попытка записи с ожиданием старого HEAD не изменила ветку на наш прежний commit; фактический новый HEAD прочитан перед повторной публикацией. Пакет добавляется поверх нового дерева, без удаления/перезаписи его файлов.

[Точный compare](https://github.com/lubochka/xiigen-planning/compare/a868f46ee3695728406173f8cd945e278ddb0def...8aa040fc676973154e009e0eec13689eb9f6bd1d).

## Содержание дельты

Добавлены **42 файла**, включая **24 Markdown**, в пакет 2026-10-20-learning-audit-additions и его входной указатель. Совпадений путей с нашим пакетом нет.

Для всех24 Markdown получен полный текст:23 — из проверенных полных added-file patches, один дополнительно прочитан целиком, поскольку compare не вернул его patch. Выполнен поиск FlatToIndices, mappedBIndices, RMS-COORD, RMS-INDEX, MultiplyBroadcast и RmsNorm: **0 совпадений**. Содержательно прочитаны описания четырёх новых факторов и их учебных цепочек. Они не совпадают с нашим RMS factor.

| Опубликованный другим агентом ID | Его фактор | Почему это отдельный механизм |
|---|---|---|
| [BPE-WEIGHTED-CHUNKS-01][bpe] | Одна копия одинакового BPE chunk с сохранённой частотой/порядком вместо повторных списков | Изменяет стоимость настоящего обучения tokenizer; не RMS broadcast |
| [DPO-TOKENIZER-SNAPSHOT-01][tokenizer] | Один закрытый Save/Load/hash snapshot tokenizer на подготовку нескольких exact пар | Изменяет подготовку потребляемого DPO материала, сохраняя freeze; не вычисление индекса Tensor |
| [WIKI-FEATURES-ONCE-01][once] | Featurize один раз перед18эпохами того же native Train | Стоимость лексического perceptron, не нормализация Transformer |
| [WIKI-FEATURE-KEYS-01][keys] | Разделение unigram/bigram key codec, включая underscore | Возможная условная потеря различия учебных признаков; semantic gain ещё не измерен |

Эти четыре ID зарегистрированы в сводной сверке как **уже опубликованные чужие гипотезы, DESIGNED_NOT_RUN**. Мы не присваиваем их новизну, не запускаем их, не изменяем карточки и не выдаём их за физически добавленные в protected.

Основной наш ledger охватывал696 planning +303 protected Markdown. С этой конечной дельтой дополнительно проверены24 planning Markdown: **720 planning +303 protected =1023 полнотекстовых документа**, плюс ранее описанный агрегат156 core patches. Это полнота названного поиска, а не новая ручная аттестация каждого эксперимента.

Наш current core остаётся65d03a73; число137 карточек/470P-подслучаев не меняется из-за публикации planning. Правило stop/next уже закрыто тем core commit. Новых primary runtime measurements в новом planning commit или в нашем аудите не заявляется.

**Итог:** RMS-COORD-ALLOC-01 остаётся отдельной source-confirmed NOT_RUN гипотезой в проверенном корпусе. Новые сторонние документы сохраняются в родительском дереве публикации.

Машинная запись этой проверки: [publication-head-reconciliation.json](publication-head-reconciliation.json).

[bpe]: https://github.com/lubochka/xiigen-planning/blob/8aa040fc676973154e009e0eec13689eb9f6bd1d/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-audit-additions/neural-bpe/BPE-WEIGHTED-CHUNKS-01.md
[tokenizer]: https://github.com/lubochka/xiigen-planning/blob/8aa040fc676973154e009e0eec13689eb9f6bd1d/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-audit-additions/neural-bpe/DPO-TOKENIZER-SNAPSHOT-01.md
[once]: https://github.com/lubochka/xiigen-planning/blob/8aa040fc676973154e009e0eec13689eb9f6bd1d/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-audit-additions/wiki/03-features-once-experiment.md
[keys]: https://github.com/lubochka/xiigen-planning/blob/8aa040fc676973154e009e0eec13689eb9f6bd1d/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-audit-additions/wiki/02-feature-keys-experiment.md

