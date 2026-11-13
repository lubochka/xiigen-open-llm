# P23 - runtime holdout ledger для SkillSelection

Вердикт: принятые holdout-строки не созданы.

Причина: Локально не найдено ни одной строки, у которой одновременно есть принятая runtime-ready форма, доказанная раздельность source family и pair id против обоих checkpoint-ов, а также запрет на обучение по этой строке.

Проверено кандидатов в gap ledger: 36.

Почему это честный результат:
- P16E и P16E2 полезны как evidence, но сами отчеты говорят, что это schema-only, не runtime evaluation.
- P21 прямо фиксирует, что source-disjoint proof равен false.
- P22 уже нашел ноль accepted runtime-eval rows.
- P16B/P17C строки нельзя просто переименовать в holdout: часть их семей уже обучала candidate checkpoint, а остальные не имеют принятого runtimeInvocationReady доказательства.

Что не делалось:
- side-by-side comparison не запускался;
- обучение не запускалось;
- common и candidate checkpoint не менялись;
- promotion и runtime/common replacement не выполнялись.

Следующее исправление:
Создать новые redacted local SkillSelection runtime holdout rows из approved source families, которых нет ни в одном из двух checkpoints; записать доказательство исключения source-family и pair-id; затем заново запустить только ledger gate до любого comparison.