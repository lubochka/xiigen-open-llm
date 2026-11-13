# QUALITY-PRODUCT-ORCHESTRATION-EXPANDED-V1-SCENARIO-GENERALIZATION-REPAIR-03

Цель: закрыть обнаруженный архитектурный разрыв без leakage. Нужен non-leaky scenario generalization/output contract: held-out проверка должна оценивать переносимую структуру решения, а не exact scenario label, которого не было в train patterns.

Запрещено: git, production promotion, копирование held-out labels/source ids/row ids в checkpoint, semantic-quality claim без fresh runtime + ablation + lifecycle evidence.

Минимальный gate: train-only checkpoint не содержит held-out labels; held-out target измеряет scenario family/decision structure; source-id/row-id/exact-prompt/candidate-order/project-stack ablations не дают pass.
