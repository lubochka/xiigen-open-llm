# P21 — SkillSelection compatibility / non-regression

P21 загрузил existing common checkpoint и P17E candidate checkpoint в разные SkillSelectionMachine instances и прогнал одинаковые P16E-derived сценарии. Promotion запрещена, потому что P16E источник является evidence/schema-only, не доказан source-disjoint runtime eval для promotion.

## Что было проверено

P21 взял два файла модели:

- common model: `models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json`
- P17E candidate: `artifacts/smart-skills-semantic-quality-completion/p17e-skillselection-lifecycle-import-export/production-export/skill-selection-checkpoint.json`

Оба checkpoint-а были загружены в разные `SkillSelectionMachine` instances. Потом один и тот же набор P16E-derived сценариев был отправлен в оба экземпляра через `DecideFromTrainedState`.

## Почему promotion всё ещё запрещена

P16E сценарии — лучший уже имеющийся источник для проверки SkillSelection, но сам P16E report говорит, что это schema/evidence-only материал, а не production runtime evaluation. Поэтому даже при наличии строк сравнения P21 не может заявить source-disjoint runtime proof.

## Наблюдения

- Scenario rows: 12
- Одинаковые outputs common/candidate: 3
- Common совпал с expected skill: 0
- Candidate совпал с expected skill: 9

## Итог

Compatibility pass: false.

Promotion allowed: false.

Semantic quality claimed: false.
