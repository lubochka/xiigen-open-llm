# P31 source-disjoint split manifest - human summary

Вердикт: P31 сделал только design split. Это не eval run и не training run.

## Как разделены source families

- Train-candidate families: skill-injection, trainable architecture, human-readable audit, plan-review, DPO format, training-data-gap, conversation-intent, output-skepticism.
- Dev-candidate families: self-verification, e2e-test-matrix.
- Eval-holdout-candidate families: no-product-decisions, analysis-state AGENTS rules.

## Почему это source-disjoint

Каждая строка P31 имеет ровно одну `source_family`. Одна и та же source family не используется одновременно в train-candidate и eval-holdout-candidate роли. Это важно, потому что будущая модель не должна “узнавать” источник по пути, имени файла или старой формулировке.

## Что это еще не доказывает

Этот split пока не доказывает качество модели. Он только подготавливает честную структуру для будущего eval: когда обучение будет разрешено, eval rows должны оставаться отдельно от train/dev rows.
