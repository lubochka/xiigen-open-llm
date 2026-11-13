# P16E2: rerun SkillSelection eval/ablation против P16D2

## Краткий вывод
P16E2 улучшает P16E baseline: pass 0 -> 10, fail 8 -> 0, gap 4 -> 2. Shortcut leaks остаются 0.

## Что это доказывает
P16D2 checkpoint candidate содержит ожидаемые профили для 10 source-disjoint eval/holdout строк и не материализует eval/holdout строки как training/profile input.

## Что это не доказывает
Это не production runtime, не SkillSelection readiness, не full API readiness, не trained-model readiness и не final model readiness.

## Остаточные gaps
Две candidate-position строки остаются gap, потому что candidate-order guard специально не выбирает навык. P16F lifecycle достижим только если parent принимает этот diagnostics-only scope; иначе нужен еще один repair/role decision.
