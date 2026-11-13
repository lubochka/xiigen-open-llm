# P16E2: source-disjoint eval rerun для SkillSelection

## Вердикт
PASS_CANDIDATE_WITH_GAPS_READY_FOR_PARENT_REVIEW

Это проверка схемы checkpoint candidate, а не production runtime. P16D2 checkpoint не менялся.

## Счётчики
- Eval/holdout строк: 12
- Pass: 10
- Fail: 0
- Gap: 2

## Что улучшилось
После P16D2 ожидаемые профили появились для prompt understanding, task understanding, prompt context, intermediate checks и runtime chain. Это поднимает положительное покрытие с 0 до 10.

## Что остается gap
Candidate-position строки остаются gap: порядок кандидатов должен быть только диагностическим запретом shortcut, а не основанием выбора навыка.
