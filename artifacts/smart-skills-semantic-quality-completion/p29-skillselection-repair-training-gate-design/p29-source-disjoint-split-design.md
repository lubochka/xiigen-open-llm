# P29 source-disjoint split design

P29 не обучает модель и не запускает eval. Этот документ только задает дизайн gate-пакета для будущей P30, если Luba и parent architect отдельно разрешат candidate-only training/eval.

## Вердикт по 8 строкам

8 строк P27 достаточно только для gate/design: можно описать source-disjoint split, leakage gates, batch card и authorization matrix. Этого не достаточно для настоящей попытки обучения SkillSelection: всего 4 source family, по 2 строки на family, 4 positive skills и два targetAdapter на один и тот же redacted summary внутри family. Любая метрика на таком объеме будет слишком хрупкой и может легко превратиться в overclaim.

## Как разложены строки

| Split design | Source family | Rows | Target adapters | Selected skill | Почему так |
|---|---|---:|---|---|---|
| candidate_train_design | p24_local_skill_doc_e2e_test_matrix | 1, 2 | IPromptContextTraining, ISkillSelectionTraining | e2e-test-matrix | Только дизайн: family не смешивается с dev/eval. |
| candidate_train_design | p24_local_skill_doc_no_product_decisions | 3, 4 | IPromptContextTraining, ISkillSelectionTraining | no-product-decisions | Только дизайн: family не смешивается с dev/eval. |
| candidate_dev_design | p24_local_skill_doc_output_skepticism | 5, 6 | IPromptContextTraining, ISkillSelectionTraining | output-skepticism | Dev-family отдельно от train/eval. |
| candidate_eval_design | p24_local_skill_doc_self_verification | 7, 8 | IPromptContextTraining, ISkillSelectionTraining | self-verification | Eval-family отдельно от train/dev. |

## Правила split

- source_family не может одновременно попасть в train и dev/eval.
- pair_id, source_hash, source id, path, filename, row ordinal, candidate order, selected-skill position и targetAdapter не являются признаками обучения.
- Все 8 строк остаются local-private candidate rows: commonTrainingAllowed=false.
- Для P30 нужен отдельный work order. P29 не дает права обучать, мутировать checkpoint или продвигать модель.

## Главный риск

Такой split полезен как контрольная схема, но не как доказательство обучаемости. Если P30 будет авторизован, сначала нужно либо добавить больше source-disjoint local-private rows, либо явно назвать запуск маленьким mechanics-smoke без semantic-quality claim.
