# P30 source-disjoint coverage summary - SkillSelection candidate expansion

Вердикт: P30 нашел 17 дополнительных candidate rows из 17 source families. Этого достаточно для следующего шага проектирования DPO materialization, но недостаточно для обучения, eval или semantic-quality claim.

## Что именно собрано

P30 не копировал raw private text. Каждый источник записан как hash/path reference, source family и короткое redacted summary: чему он может научить SkillSelection на универсальном уровне.

- Репозиторные skill docs дают candidate signals для выбора скилов: skill injection, trainable architecture, readable audit, plan review, DPO format, training gap audit, conversation intent, skepticism, self-verification, e2e matrix, no-product-decisions.
- Local analysis-state дает private/evidence-only families: AGENTS, CLAUDE, PROMPTS, phase transition protocol.
- Named references дают universal-vs-stack separation: skills inventory и universal orchestrator plan.

## Почему это еще не training set

Эти строки пока не являются DPO парами. В них есть предложенные chosen/rejected skill ids, но нет принятого pair package, нет source-disjoint split, нет leakage ablation и нет разрешения на training/eval. Поэтому статус честный: candidate-only.

## Достаточность

- Для следующего шага materialization design: достаточно.
- Для actual training: недостаточно.
- Для source-disjoint eval: недостаточно, потому что eval rows должны быть отдельными source families и не пересекаться с train/dev.
- Для common training: ноль строк разрешено. Все строки private/project evidence до отдельного license/approval gate.

## Coverage по adapter

- IPromptContextTraining: 17 candidate rows.
- ISkillSelectionTraining: 17 candidate rows.

## Что должно случиться дальше

Следующая фаза должна превратить candidate rows в настоящие DPO materialization rows: context, chosen behavior, rejected behavior, teaching point, quarantine case, source-disjoint family, leakage scan и dashboard/API QA сценарии. Только после этого можно обсуждать training authorization.
