# P27 parent review - SkillSelection repair dataset candidate

Вердикт: P27 принят как checked repair dataset candidate. Это не training set, не common training, не checkpoint mutation и не promotion.

Что проверено:

- материализованы все 8 строк из P26 draft rows;
- каждая строка имеет `selectedSkills`, `rejectedSkills`, chosen/rejected behavior, target adapter и intended trained state;
- каждая строка сохраняет `commonTrainingAllowed=false`, `notUsedForTrainingYet=true` и `trainingAllowedInP27=false`;
- каждая строка остается local-private и требует отдельного Luba approval перед common reuse;
- quarantine ledger явно говорит, что future common source-disjointness еще не доказан;
- dataset card и training authorization ledger запрещают training, checkpoint mutation, promotion и common model replacement в P27;
- модельные файлы `models/skill-selection/skill-selection-v1` не изменены.

Почему это принимается:

P27 делает нужную промежуточную работу: превращает P26 failure analysis в проверяемый dataset candidate, но не прячет его под видом уже разрешенного training set. Это правильная граница перед следующим training/eval slice.

Что еще не закрыто:

P27 не обучает candidate checkpoint, не доказывает semantic quality и не разрешает promotion. Следующий slice должен отдельно проверить source-disjoint split, материализовать разрешенный training batch, запустить candidate-only training, затем fresh-load, continue-training, export/import, ablation и leakage gates.

Следующее действие:

P28 должен быть отдельным bounded training/eval slice для candidate-only SkillSelection repair, если active plan/user authorization позволяет training. Если training не разрешен, P28 должен сначала записать training authorization gap и продолжить только безопасные verification/documentation задачи.
