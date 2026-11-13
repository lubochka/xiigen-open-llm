# P7B: train-only coverage expansion для PromptToTask

## Вердикт

`P7B_FAIL_NEEDS_MORE_TRAIN_ONLY_SOURCE`. Новый checkpoint candidate не создан. Это честный результат: после P7A известно, что P6 checkpoint имеет только две train-only строки, а остальные четыре строки P5L являются eval/holdout резервом. Использовать их для P7B обучения нельзя.

## Какие источники рассмотрены

1. P5L split rows: найдено 6 строк. Две строки уже использованы как P6 train split, две сохранены для P7 eval, две сохранены как holdout.
2. P6 checkpoint: содержит 2 learned patterns и не дает source-disjoint reserved generalization.
3. P7A parent review: прямо запрещает переносить exact P7 reserved/eval/holdout signal combinations в production code; тот же запрет применен к dataset/checkpoint repair.
4. Активный план: содержит универсальные границы, но не является утвержденной train-only taxonomy с DPO rows, split role и parent promotion gate.

## Что принято

Новых accepted train-only rows нет. Две старые training строки остаются допустимыми baseline rows, но они уже вошли в P6 и не расширяют покрытие.

## Что отправлено в quarantine/evidence-only

- P7 eval rows: нельзя тренировать, иначе P7 перестанет быть source-disjoint.
- P7 holdout rows: нельзя тренировать, иначе holdout потеряет смысл.
- P7A rejected mapping: нельзя переносить из кода в checkpoint, потому что parent уже признал его eval-informed fixture fitting.
- Pre-P7 plan taxonomy: полезная evidence, но не самостоятельный train-ready artifact без отдельной материализации rows и parent review.

## Почему checkpoint не создан

Новый checkpoint без новых train-only rows был бы либо копией P6, либо скрытым обучением на eval/holdout/reserved материалах. Оба варианта нарушают границу P7A parent review. Поэтому P7B оставляет точный gap: нужны новые независимые PromptToTask source families для train split или отдельно утвержденная common taxonomy, созданная не из P7 reserved markers.

## Следующее безопасное действие

Нужна следующая bounded phase: материализовать дополнительные train-only PromptToTask rows из независимых source families, проверить source-disjointness относительно P7 eval/holdout, затем создать новый checkpoint candidate и только после этого rerun P7 eval/ablation gate.
