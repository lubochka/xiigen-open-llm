# P25 parent review - SkillSelection runtime comparison

Вердикт: P25 принят как честное runtime comparison evidence с отрицательным результатом.

Что проверялось:

- сравнение запускалось только на четырех accepted P24 runtime holdout rows;
- common checkpoint и candidate P17E checkpoint загружались в отдельные `SkillSelectionMachine` instances;
- обе модели вызывались через один и тот же runtime метод `SkillSelectionMachine.DecideFromTrainedState`;
- exact prompt, source id, path, filename, pair id и candidate order не использовались как selection authority;
- common/candidate manifests и checkpoints остались byte-identical после сравнения;
- training, checkpoint mutation, promotion и common model replacement не выполнялись.

Результат:

- common checkpoint совпал с expected skill: 0/4;
- candidate P17E checkpoint совпал с expected skill: 0/4;
- P25 threshold не пройден;
- promotion остается заблокированным.

Что это значит:

P25 полезен именно потому, что он не прячет плохой результат. Текущие SkillSelection checkpoints не умеют выбирать новые holdout skills из P24, поэтому следующий шаг должен быть repair/training/eval slice, а не promotion.

Что это не доказывает:

P25 не доказывает semantic quality, SkillSelection readiness, full API readiness или право заменить common model.

Следующее действие по плану:

P26 должен разобрать failed P25 rows, построить bounded repair/training plan или DPO repair batch для SkillSelection, затем запускать отдельный training/eval lifecycle только с разрешенными gates. Promotion остается запрещен до успешных fresh-load, continue-training, ablation и source-disjoint runtime checks.
