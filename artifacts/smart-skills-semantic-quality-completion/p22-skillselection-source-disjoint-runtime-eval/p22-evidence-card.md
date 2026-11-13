# P22 evidence card

P22 искал настоящий source-disjoint runtime evaluation set для SkillSelection.

Что найдено:
- P16E и P16E2 полезны как evidence, но оба отчёта говорят schema-only, не runtime evaluation.
- P21 запускал side-by-side probes, но его собственный report говорит source-disjoint proof=false.
- Поэтому доступные P16E ledger rows записаны как gap rows, а не accepted eval rows.

Что не делалось:
- Side-by-side comparison не перезапускался на invalid rows.
- Model, checkpoint, production code, training data и runtime/common path не менялись.
- Semantic quality, SkillSelection readiness, API readiness и promotion не заявлены.

Следующий repair:
Создать accepted row-level runtime holdout ledger с source-family и pair-id disjointness против обоих checkpoints, а потом запускать common-versus-candidate runtime comparison только на этих строках.
