# P21 parent review

Вердикт: `PASS_AS_COMPATIBILITY_GAP_EVIDENCE_PENDING_COMMIT`.

## Что принято

P21 принят как side-by-side compatibility/non-regression evidence. Он загрузил existing common checkpoint и P17E candidate checkpoint в отдельные `SkillSelectionMachine` instances и прогнал один и тот же набор из 12 P16E-derived сценариев.

## Что это доказало

- Оба checkpoint-а можно загрузить отдельно.
- Сценарии можно прогнать через `DecideFromTrainedState`.
- P17E candidate на этом fixture показывает больше совпадений с expected skill, чем existing common checkpoint.
- `models/skill-selection/skill-selection-v1` не менялся.

## Что это не доказало

P21 не доказывает source-disjoint runtime eval для promotion, потому что источник сценариев P16E сам помечен как schema/evidence-only, не production runtime evaluation. Поэтому:

- `compatibility_pass=false`;
- `source_disjoint_proven=false`;
- `promotion_allowed=false`;
- semantic quality/readiness не заявлены.

## Parent review

Первичная тревога была про возможный mojibake в P21 output. Byte/codepoint scan показал, что это был channel-rendering artifact: файлы P21 и focused test не содержат U+00D0/U+00D1/U+FFFD markers и question-mark replacement runs. Focused test также проверяет все файлы P21 artifact directory.

## Следующий правильный шаг

Следующий bounded slice должен закрывать не promotion, а источник блокировки: подготовить или найти настоящий source-disjoint runtime eval set для SkillSelection и rerun side-by-side comparison на нём. До этого runtime/common promotion запрещён.
