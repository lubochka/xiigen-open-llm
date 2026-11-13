# P17A: матрица selector evidence

| Группа | Роль | Можно считать runtime-positive сейчас | Почему |
|---|---|---:|---|
| P16D2 materialized positive profiles | candidate material | Нет | Не прогнано через fresh `SkillSelectionMachine.DecideFromTrainedState` после native load. |
| candidate_order_leakage_guard | diagnostics-only | Нет | Guard запрещает shortcut и имеет `can_select_skill=false`; это не выбор навыка. |
| P16E2 eval/ablation | component schema eval | Нет | Parent accepted it as component evidence with `runtime_evaluation=false`. |
| P16F lifecycle | artifact lifecycle | Нет | Проверяет bytes/readback identity, не production runtime SkillSelection. |

Положительное runtime evidence требует native-load или approved binder, затем source-disjoint selector run и ablation controls.
