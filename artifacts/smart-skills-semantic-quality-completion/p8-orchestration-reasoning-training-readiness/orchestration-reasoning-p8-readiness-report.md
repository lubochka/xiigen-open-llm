# P8: готовность OrchestrationReasoning к обучающему checkpoint

## Вердикт

`FAIL_EVIDENCE_READY_FOR_PARENT_REVIEW`.

Checkpoint candidate не создан. Это намеренное решение: в доступных P5E/P5F артефактах есть пять хороших redacted DPO-кандидатов для OrchestrationReasoning и пять независимых семейств источников, но ни одна строка еще не стала обучающим входом. P5F прямо оставил `training_ready=false` для всех строк и пометил проверки на shortcut-подсказки как не запущенные.

## Что уже есть

Есть пять P5E строк для OrchestrationReasoning:

| Строка | Семья источника | Черновая роль из P5F | Смысл |
|---|---|---|---|
| `P5E-DPO-ORCH-001` | `sf-orchestration-phase-transition-8CD45F23` | `train_candidate_draft_only` | Не смешивать артефакт, самопроверку, parent review и следующий шаг. |
| `P5E-DPO-ORCH-002` | `sf-p5a-both-skills-inventory-905F88B4` | `train_candidate_draft_only` | Отделять переносимое ядро orchestration от локальной adapter policy. |
| `P5E-DPO-ORCH-003` | `sf-p5a-orch-universal-orchestrator-plan-C10AD9A5` | `eval_candidate_draft_only` | Сохранять независимые ветки, review до финального оформления и точечный ремонт. |
| `P5E-DPO-ORCH-004` | `sf-p5a-orch-dependency-relations-7DB3DAB0` | `train_candidate_draft_only` | Строить порядок по главному решению, зависимостям, evidence и gates. |
| `P5E-DPO-ORCH-005` | `sf-p5a-orch-flow-isolation-F85D7BAA` | `holdout_candidate_draft_only` | Проверять изоляцию reusable orchestration asset и отсутствие state bleed. |

Эти строки достаточны для чернового source-disjoint split proposal. Они не достаточны для checkpoint candidate, потому что черновая роль не равна разрешению на обучение.

## Почему checkpoint не создан

1. Нет accepted training rows для OrchestrationReasoning. В P5F у всех строк `training_ready=false`.
2. Нет отдельного promotion artifact, который сказал бы: эти строки являются train input, эти строки остаются eval/holdout, и eval/holdout нельзя тренировать.
3. Ablation controls существуют как план, но не выполнены. Нельзя считать, что модель не учится по имени файла, заголовку источника, class/method словам, test labels или nearest-only совпадению.
4. Создание checkpoint из черновых строк повторило бы старую ошибку overclaim: artifact выглядит полезным, но training gate еще не открыт.

## Что модель могла бы учить после ремонта

После отдельного promotion review OrchestrationReasoning сможет учиться только универсальным orchestration boundaries:

- удерживать phase/review/next-step boundary;
- отделять переносимое ядро от adapter policy;
- строить порядок по зависимостям и evidence gates;
- сохранять независимые ветки и точечный repair;
- требовать isolation/no-cross-contamination evidence перед readiness claim.

Стековые детали, пути, заголовки источников, имена классов, имена методов, тестовые labels, номера узлов и exact source wording не должны стать обучающим сигналом.

## Что нужно сделать дальше

1. `P8A_ORCHESTRATION_TRAINING_ROW_PROMOTION_REVIEW`: принять или отложить каждую P5E Orchestration строку. Только train split rows могут стать `training_ready=true`.
2. `P8B_ORCHESTRATION_CHECKPOINT_CANDIDATE`: если P8A даст train-only rows, создать checkpoint candidate под отдельной artifact directory, без overwrite финальных/common моделей.
3. `P8C_ORCHESTRATION_EVAL_ABLATION`: проверить candidate на eval/holdout и shortcut controls.

## Что это не доказывает

Этот P8 пакет не доказывает semantic quality, full API readiness, extension runtime readiness, SkillSelection readiness, финальную готовность модели или P8 pass. Он доказывает только одно: текущие OrchestrationReasoning DPO материалы еще не имеют честного разрешения на создание checkpoint.
