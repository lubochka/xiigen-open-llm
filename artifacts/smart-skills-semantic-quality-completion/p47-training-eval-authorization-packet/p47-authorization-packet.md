# P47 authorization packet: training/eval not authorized

## Короткий статус

Этот пакет превращает P44 и P45 в точный будущий пакет разрешения. Сейчас он ничего не запускает и ничего не меняет: обучение, evaluation action, checkpoint mutation, host runtime, production code и внешние AI-сервисы остаются запрещены.

Текущий факт полномочий: `exact_luba_approval_quote = "NONE"`. Поэтому P47 может только описать, какие данные, фикстуры, checkpoint refs, метрики и rollback-границы должны быть названы в будущем work order, если Luba отдельно разрешит запуск.

## Что было бы использовано после будущего разрешения

Если Luba позже даст свежую явную фразу approval, будущий work order должен сослаться на следующие уже найденные кандидаты:

| Объект | Текущий путь | Роль | Checksum / счетчик |
|---|---|---|---|
| Candidate dataset | `artifacts/smart-skills-semantic-quality-completion/p31-skillselection-dpo-materialized-candidates/p31-materialized-dpo-candidates.jsonl` | материал-кандидат для будущего training slice | `sha256=3FF9B59AE601E6DEE9F1741528F3C49018EA65FF84ECA76D5F77A125D1CA88A6`, `rows=12` |
| Candidate dataset card | `artifacts/smart-skills-semantic-quality-completion/p27-skillselection-repair-dataset-candidate/p27-dataset-card.json` | карточка кандидата и счетчик исходных repair rows | `sha256=6433774C85110843E7E9BFD14FFEC549D636DE1B43811823402E52AED7F7E8CF`, `rows=8` |
| Dashboard QA scenarios | `artifacts/smart-skills-semantic-quality-completion/p31-skillselection-dpo-materialized-candidates/p31-dashboard-qa-scenarios.jsonl` | QA-сценарии, не training rows | `sha256=B3C8AE7A27ED53430937716AF6D506B0646EC6829F47F4BD959A4F96DC393AD5`, `rows=6` |
| Held-out/eval fixtures | `artifacts/smart-skills-semantic-quality-completion/p36-eval-fixture-rows/p36-eval-fixtures.jsonl` | материал-кандидат для будущей evaluation action | `sha256=FA8419A529781CEDBFABBB9048553D5FD2833F6B71149B333107C42A0B60250E`, `rows=36` |
| Source-disjoint guard | `artifacts/smart-skills-semantic-quality-completion/p41-source-disjoint-pre-eval-guard/p41-implementation-report.json` | preflight guard перед будущей evaluation action | `sha256=5812CD40A7A808BC48539E3741FEC133AF331E2EC0DE315EFB002517B3FB6121`, `can_proceed_to_eval_runner=false` |

## Что остается заблокировано сейчас

- Training authorization: заблокировано, потому что нет свежей явной approval-фразы Luba.
- Eval authorization: заблокировано, потому что P41 guard не разрешает evaluation runner, а P45 dry-run намеренно возвращает `can_start_eval=false`.
- Checkpoint mutation: заблокирована, потому что target checkpoint и rollback checkpoint в P45 имеют статус `NONE` / `missing_for_training`.
- Source/license gate: заблокирован для training, потому что P45 помечает его как `missing_for_training`.
- Metrics plan: нужен до любых quality claims; сейчас он записан как будущий план, не как результат.
- Host runtime: вне этого work order и не запускался.

## Почему нельзя запускать сейчас

P47 имеет `implementation_authorized_now: artifact/state only`. В work order прямо запрещены model training, model eval, checkpoint mutation, host runtime invocation, external AI/network, git write/ref commands, spawning subagents и production code. P44 и P45 тоже не дают разрешения: они только собрали readiness/dry-run форму и отказали запуску из-за отсутствия approval и preflight gates.

Любой запуск сейчас нарушил бы границу: источник authority равен текущему P47 work order, а в нем `exact_luba_approval_quote = "NONE"` для training/eval/checkpoint mutation.

## Какая будущая approval-фраза нужна

Будущий запуск возможен только после новой текущей фразы Luba, которая одновременно:

1. явно принимает P47/P44/P45 как основание;
2. явно разрешает training и/или eval и/или checkpoint mutation;
3. называет exact dataset path + checksum;
4. называет held-out/eval fixture path + checksum;
5. называет target checkpoint ref;
6. называет rollback checkpoint ref;
7. подтверждает source/license gate для common training или ограничивает rows user-local/adaptive-only;
8. подтверждает metrics/ablation gates.

Пример формы, которую будущий parent может использовать как проверяемый шаблон, но которая сейчас не является approval:

```text
P47 принят. Разрешаю только <training|eval|checkpoint mutation> для dataset <path sha256=... rows=...>, heldout <path sha256=... rows=...>, target checkpoint <ref>, rollback checkpoint <ref>. Source/license gate принят: <details>. Metrics/ablation gates: <details>. Начинай этот bounded work order now.
```

## Rollback plan

До любой checkpoint mutation будущий work order должен:

- зафиксировать исходный rollback checkpoint ref и manifest ref;
- запретить overwrite без атомарной записи нового checkpoint в отдельный staging path;
- записать checkpoint manifest до promotion;
- выполнить fresh-load probe нового checkpoint;
- выполнить continue-training probe с загрузкой именно нового checkpoint;
- сохранить old-to-new mapping в manifest;
- при любой ошибке вернуть locator на rollback checkpoint и не публиковать новый ref.

## Metrics plan

Будущий eval/report должен минимум содержать:

- per-machine metrics для PromptToTask, OrchestrationReasoning и SkillSelection, если они затрагиваются;
- held-out pass rate по source family;
- ablation delta для exact prompt lookup;
- ablation delta для source id/path/filename lookup;
- ablation delta для title/static route;
- ablation delta для nearest-only baseline;
- ablation delta для no-trained-state baseline;
- candidate-order permutation check для SkillSelection;
- false-positive claim scan;
- regression check против rollback checkpoint;
- explicit failure examples.

## Итоговая граница

P47 сам по себе является только authorization packet и future work-order skeleton. Он не утверждает, что semantic quality доказана, model readiness достигнута, host runtime проверен, training/eval запускались или checkpoint был изменен.
