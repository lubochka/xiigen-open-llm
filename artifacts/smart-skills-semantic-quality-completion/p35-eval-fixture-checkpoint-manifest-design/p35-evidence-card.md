# P35 evidence card

Вердикт: P35 подготовил дизайн eval fixture и checkpoint rewrite manifest. Это не training, не eval execution, не checkpoint rewrite, не mutation и не semantic quality.

## Что создано

- `p35-eval-fixture-design.md`
- `p35-eval-fixture-design.json`
- `p35-checkpoint-rewrite-manifest-design.md`
- `p35-checkpoint-rewrite-manifest-design.json`
- `p35-readiness-gap-ledger.jsonl`
- `p35-review-cycles.jsonl`
- `p35-self-check.json`
- `p35-evidence-card.md`

## Что использовано как вход

- P31: 12 SkillSelection candidate rows и 6 dashboard/API scenarios.
- P32: artifact-level validation report для P31.
- P33: dashboard/API validation surface examples.
- P34: source-disjoint eval design, ablation design, readiness gaps.

## Главные gaps после P35

1. Нужны materialized eval fixture rows для PromptToTask.
2. Нужны materialized eval fixture rows для OrchestrationReasoning.
3. Нужны SkillSelection permutation fixtures для candidate-order leakage.
4. Нужна явная authorization/source-license gate перед common training.
5. Нужны executed ablation reports.
6. Нужен checkpoint rewrite manifest, заполненный реальными checksums и probe refs.
7. Нужны fresh-load и continue-training probe results.

## Граница доказательства

P35 доказывает только то, что следующий безопасный non-training шаг описан и проверяем. P35 не доказывает, что модели обучены, что eval прошел, что checkpoint можно переписать, что host runtime работает или что semantic quality готова.
