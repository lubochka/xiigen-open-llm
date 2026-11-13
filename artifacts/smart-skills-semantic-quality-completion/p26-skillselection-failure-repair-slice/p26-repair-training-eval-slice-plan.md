# P26 - план ремонта SkillSelection после провала P25

## Короткий вывод

P25 честно показал отрицательный результат: common checkpoint и candidate checkpoint получили 0 из 4 на accepted P24 holdout rows. Это не ошибка отчета и не основание для promotion. Это сигнал, что SkillSelection пока не выучил четыре новые границы выбора skills: `e2e-test-matrix`, `no-product-decisions`, `output-skepticism` и `self-verification`.

P26 является только планом и дизайном данных. В этом slice не выполнялись training, checkpoint mutation, promotion, common model replacement, external AI или network. Semantic quality, SkillSelection readiness и full API readiness не заявляются.

## Почему P25 дал 0/4

Common checkpoint не был пустым: он каждый раз принимал решение через `PromptContextSkillSelectionUnit`. Но это решение было слишком широким: вместо точного expected skill оно выбирало соседние общие skills вроде `implementation-integrity`, `plan-review`, `test-integrity`, `claim-verification` или `verification-before-completion`. В evidence это видно по `PROMPT_CONTEXT_SKILL_SELECTION_UNIT_EXECUTED`, `PROMPT_CONTEXT_BASELINE_COUNTER_ONLY`, `prompt_context:decision_features=separated_prompt_and_context_feature_weights` и confidence 0.95.

Candidate checkpoint был честно неготов для этих строк: P25 вернул `NO_TRAINED_STATE_AVAILABLE`, пустой selected set и `RequiresSecondPass=true`. Поэтому candidate не проиграл из-за неправильного выбора; он не имел нужного trained state для P24 families.

Кодовая причина, которую должен учитывать следующий executor: `SkillSelectionMachine.TryDecideWithSemanticUnitsOnly` сначала пробует prompt_context, а `SkillSelectionSkillUnit` идет позже. Если prompt_context возвращает broad baseline decision, final-selection unit не получает шанса исправить выбор. Ремонт должен одновременно усилить точные DPO signals и добавить будущий arbitration/gating check, чтобы prompt_context не перебивал более точный final-selection сигнал.

## Карта провалов по строкам P24/P25

| P24 row | Expected | Common selected | Candidate selected | Что произошло | Чего не хватает |
|---|---|---|---|---|---|
| `P24-SS-HOLDOUT-E2E-MATRIX-001` | `e2e-test-matrix` | `implementation-integrity, plan-review, test-integrity` | `none` | common выбрал broad prompt_context baseline; candidate вернул no trained state | `e2e-test-matrix`: scenario matrix, success, idempotency, delayed behavior, recovery, boundary, security checks, before implementation |
| `P24-SS-HOLDOUT-NO-PRODUCT-DECISION-001` | `no-product-decisions` | `claim-verification, implementation-integrity, planning-session-startup` | `none` | common выбрал broad prompt_context baseline; candidate вернул no trained state | `no-product-decisions`: choose between alternatives, change user behavior, public interface shape, architecture direction |
| `P24-SS-HOLDOUT-OUTPUT-SKEPTICISM-001` | `output-skepticism` | `plan-review, verification-before-completion` | `none` | common выбрал broad prompt_context baseline; candidate вернул no trained state | `output-skepticism`: result claim, refuting evidence, scope validity, proxy-measure risk, before reporting |
| `P24-SS-HOLDOUT-SELF-VERIFICATION-001` | `self-verification` | `implementation-integrity, test-integrity` | `none` | common выбрал broad prompt_context baseline; candidate вернул no trained state | `self-verification`: verify own changed artifact, fresh fixtures, blast-radius classification, cascade tracing, prediction matching |

## DPO repair rows, которые надо построить следующими

Файл `p26-skillselection-dpo-repair-draft.jsonl` содержит 8 draft rows: по две строки на каждый P24 провал. Первая строка учит `IPromptContextTraining`, чтобы prompt_context перестал выбирать широкие соседние skills. Вторая строка учит `ISkillSelectionTraining`, чтобы final-selection unit получил явный точный сигнал.

Пример для `P24-SS-HOLDOUT-E2E-MATRIX-001`: выбранный behavior должен выбирать `e2e-test-matrix` для задачи про scenario matrix, success, idempotency, delayed behavior, recovery, boundary и security checks before implementation. Rejected behavior - не повторять P25 common selection `implementation-integrity, plan-review, test-integrity` и не возвращать пустой candidate selection.

Пример для `P24-SS-HOLDOUT-OUTPUT-SKEPTICISM-001`: выбранный behavior должен выбирать `output-skepticism` для проверки result claim against refuting evidence, scope validity и proxy-measure risk. Rejected behavior - не подменять это `phase-completion-packager`, `verification-before-completion` или `plan-review`, потому что эти skills могут быть соседними, но не отвечают за skepticism к самому claim.

Все rows имеют local-private/holdout boundary из P24. В P26 они не становятся common training data. Их надо считать repair draft/eval design до отдельной авторизации и dataset card review.

## Trainable-unit repair design

1. `PromptContextSkillSelectionUnitState`: добавить положительные prompt/context weights для точного expected skill и отрицательные weights для соседних rejected skills из того же candidate set. Это должно исправить P25 механизм, где prompt_context с `SelectionScoreFloor=6` выбирает несколько широких skills и исключает expected skill.

2. `SkillSelectionSkillUnitState`: добавить final-selection feature weights и KNN memory examples для тех же четырех expected skills. KNN здесь является support evidence, а не самостоятельной runtime authority.

3. Runtime arbitration, будущая code slice only: если prompt_context выбрал broad set, который исключает high-support final-selection skill, machine должна defer/compare units instead of accepting prompt_context immediately. Это не реализуется в P26, но следующий executor должен включить это в code design и тесты.

4. DPO materializer check: для prompt_context negative learning нужно проверить, что rejected skills попадают в `Chosen.RejectedSkillIds` и/или в путь, который реально читает `PromptContextSkillTrainingState.LearnPromptAndContextFeatureWeights`. Иначе negative examples могут не ослабить wrong skills.

## Eval plan перед любым semantic-quality claim

- Source-disjoint split: train/dev/holdout должны быть disjoint по source_family и pair_id. P24 local-private rows нельзя молча переносить в common training.
- Ablation checks: no-trained-state, prompt_context-only, final-selection-only, exact prompt/source/path/pair-id removal, candidate-order permutation, wrong generic-skill ablation.
- Fresh-load: экспортировать candidate checkpoint, импортировать в новый `SkillSelectionMachine`, повторить runtime eval без in-memory leftovers.
- Continue-training: загрузить существующий candidate checkpoint, добавить repair rows, проверить, что старые accepted rows не забыты.
- Export/import: manifest, checkpoint, locator, source manifest и training-data card должны ссылаться на точные hashes.
- No candidate-position/order leakage: каждая eval row гоняется минимум в двух перестановках available skills; selected skill должен оставаться тем же.
- Dashboard/runtime API evidence: API или dashboard может показать selected/rejected/evidence flags, но это не semantic quality proof без source-disjoint runtime pass, ablation pass, fresh-load, continue-training и export/import.

## No-overclaim boundaries

P26 не обучает модель, не меняет checkpoint, не продвигает candidate, не заменяет common model и не заявляет readiness. После P26 допустимый следующий шаг - отдельный bounded executor slice для dataset card/materialization plus authorized training/eval. Promotion остается заблокированным до отдельного parent review gate.

## Следующий bounded slice без угадывания

1. Взять `p26-skillselection-dpo-repair-draft.jsonl` как draft, не как training set.
2. Построить dataset card с license/local-private boundary и quarantine decisions.
3. Если parent/Luba отдельно разрешит training, материализовать candidate-only repair batch, не common replacement.
4. Обучить candidate checkpoint, затем прогнать source-disjoint eval, ablations, fresh-load, continue-training, export/import и P24/P26 runtime comparison.
5. Не заявлять semantic quality или promotion, пока все gates не пройдены и parent review не примет evidence.
