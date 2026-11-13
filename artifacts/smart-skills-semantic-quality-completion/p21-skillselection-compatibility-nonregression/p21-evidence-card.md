# P21 evidence card

P21 проверяет не promotion, а возможность честно сравнить два SkillSelection checkpoint-а.

Что доказано:
- existing common checkpoint и P17E candidate checkpoint загружаются в отдельные `SkillSelectionMachine`;
- одинаковые P16E-derived сценарии дают наблюдаемые outputs для обеих моделей;
- production code и `models/skill-selection/skill-selection-v1` не менялись.

Что НЕ доказано:
- source-disjoint runtime eval для promotion;
- semantic quality;
- готовность P17E заменить common model;
- live extension/runtime readiness.

Вывод: promotion запрещена до отдельного source-disjoint runtime gate и совместимости с common skill vocabulary.
