# P22 - SkillSelection source-disjoint runtime eval

Вердикт: настоящий source-disjoint runtime eval set не найден.

Причина: Локально не найден принятый набор строк SkillSelection, который одновременно доказывает раздельность источников и реальный runtime-вызов. P16E и P16E2 являются schema-only evidence, а P21 прямо пишет, что source-disjoint proof=false.

Поэтому P22 не перезапускал side-by-side comparison. Если прогнать common и candidate checkpoints на строках, которые не приняты как source-disjoint runtime eval, получится ложный pass.

Граница evidence:
- Это evidence и repair gate.
- Он точно показывает, какой gate отсутствует.
- Он не доказывает semantic quality, SkillSelection readiness, API readiness, model promotion или runtime/common replacement.

Следующий обязательный repair:
1. Создать accepted runtime holdout ledger для SkillSelection.
2. Доказать, что каждая строка отсутствует в training pair ids и source families обоих checkpoints.
3. Добавить redacted prompt evidence, expected skill, available skills и leakage controls.
4. Запустить `SkillSelectionMachine.DecideFromTrainedState` на accepted rows для обоих checkpoints.
5. Держать все readiness и promotion claims false, пока этот gate не пройдет.
