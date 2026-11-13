# P31 evidence card - DPO materialized candidates

Вердикт: P31 подготовил DPO-like candidate rows для будущего SkillSelection materialization. Это не training dataset, не eval dataset, не checkpoint update и не semantic-quality evidence.

## Что создано

- 12 materialized candidate rows.
- 12 source families без пересечения между train-candidate/dev-candidate/eval-holdout-candidate ролями.
- 6 dashboard QA candidate scenarios, которые должны показывать candidate-only статус, а не готовность модели.
- Leakage ablation design: path-only, skill-id-only, stack-specific-only и candidate-order checks.

## Что есть в каждой materialized строке

Каждая строка содержит:

- redacted task summary;
- context package с available skills и forbidden shortcuts;
- chosen decision;
- rejected decision;
- quarantine example;
- teaching point;
- leakage guards;
- flags: training запрещен, eval запрещен, common training запрещен, строка еще не использовалась для обучения.

## Что осталось на следующую фазу

P32 должен сделать executable validation design или тестовый слой для этих materialized candidates: parse checks, dashboard QA visibility, leakage checks, source-disjoint assertions. Обучение и checkpoint mutation все еще запрещены без отдельного authorization.
