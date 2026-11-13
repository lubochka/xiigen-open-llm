# Pass P23 — Pipeline: task/plan-to-skills (реальный SkillSelectionMachine + source-disjoint runtime eval)

Дата: 2026-07-08. Ассессор: Opus 4.8. Срез: третья стадия конвейера расширений (`task/plan → skills`) —
РЕАЛЬНЫЙ механизм выбора скиллов `SkillSelectionMachine` и вопрос «есть ли настоящая
source-disjoint runtime-оценка выбора скиллов». Оценка ведётся от живого кода/артефактов
`C:\Projects\llm_mvp_core`, не от копий пакета.

## Что прочитано (bounded reads)

- `43.SmartSkillsDomain\SkillSelectionMachine.cs` (2982 строки; прочитаны 1-1075, 1357-1426, 2726-2885 —
  публичный API, `DecideFromTrainedState`, `TryDecideWithSemanticUnitsOnly`, `TrainBatchAsync`,
  `EvaluateAsync`, `LearnPair`, `LearnPromptFeatureWeights`, `ExtractPromptUnderstandingSignals`,
  `ExtractPromptContextSignals`, `ValidateCheckpoint`).
- `43.SmartSkillsDomain\SkillSelectionLogicalHierarchy.cs` (2276; слайс `.Evaluate`/scoring юнитов, строки 441-492, 562-613, 720-747).
- `43.SmartSkillsDomain\SkillSelectionContracts.cs` (1622; профиль/состояния/decision-контракты).
- `43.SmartSkillsDomain\SmartSkillsSemanticUnitCatalog.cs` (129; статический каталог 11 юнитов).
- Промоутнутый ассет `models\skill-selection\skill-selection-v1\{manifest.json, runner-report.json,
  skill-selection-v1-checkpoint.json (1.3 МБ)}`.
- Тесты: `Tests\SmartSkillsSemanticQualityP22SourceDisjointRuntimeEvalTests.cs` (прочитан целиком, 424 строки),
  grep по `Tests\SmartSkillsSemanticQualityP16..P23`, `SkillSelectionMachineTests`.
- Ablation: `ls 43.SmartSkillsDomain\*blation*` → только `PromptToTaskAblationHarness.cs`.

## Как реально работает выбор скиллов (по коду)

1. Вход границы расширения → `SkillSelectionRequest{Prompt, ProjectRoot, AvailableSkillIds, HostFacts,
   UserPromptSourceEntryId}` → `DecideFromTrainedState` (`:107`).
2. `DecideFromTrainedState` СНАЧАЛА зовёт `TryDecideWithSemanticUnitsOnly` (`:173`) — каскад из 11
   «семантических» юнитов, пробуемых по порядку: `skill_selection → prompt_context → result_gap →
   output_validation → task_completion → intermediate_checks → skill_candidates → routing → task_type →
   task_understanding → prompt_understanding`. **Первый юнит с непустым решением ПОБЕЖДАЕТ** (short-circuit).
3. Каждый юнит (`SkillSelectionLogicalHierarchy.cs:441-492` для prompt_understanding, аналогично прочие):
   - `requestSignals = ExtractPromptUnderstandingSignals(prompt)`;
   - для каждого кандидат-скилла: `Score = Σ_{signal ∈ requestSignals} profile.PromptFeatureWeights[signal]`
     (`:454-455`), т.е. сумма выученных весов слов;
   - `argmax` по Score (`:458 OrderByDescending`); если `Score <= 0` → нет решения (`:462`);
   - `SelectedSkillIds = [argmax skill]` (ровно ОДИН скилл), `Confidence = Math.Min(0.95, 0.50 + Score*0.05)` (`:492`).
4. `ExtractPromptUnderstandingSignals` (`SkillSelectionMachine.cs:2726`): **сырые слова промпта** —
   split по whitespace, оставить буквы/цифры, lower, длина ≥4, distinct, sort. НИКАКОГО выученного
   словаря/алиасов/эмбеддинга/стемминга: пространство признаков = буквальные ≥4-символьные слова промпта.
5. Обучение (`LearnPromptFeatureWeights`, `:1370-1382`): для КАЖДОГО chosen-скилла инкрементит
   `skillProfile[word] += 1` по всем словам промпта. **`pair.Rejected` не используется вообще** — нет
   маржи chosen−rejected, нет down-weight отклонённых скиллов. Это положительно-счётная таблица
   частот слов на скилл (Naive-Bayes-подобная), а не preference-optimization.
6. `_candidatePositionWeights` (позиционный счётчик, дефект P02) теперь НЕ авторитет выбора: в
   `DecideFromTrainedState` он используется только когда семантический каскад вернул null, и тогда
   возвращается ПУСТОЙ выбор (`SelectedSkillIds: []`) с leakage-guard evidence (`:128-142`); реализован
   `CandidateOrderLeakageGuardUnit` (`:145-148`), которого P02 не нашёл. **Коррекция P02 gap-4:** guard
   существует, позиционный счётчик низведён до diagnostics/leakage-only. Резидуал — см. G5 ниже.

## Source-disjoint runtime eval — центральный вопрос среза

`models\skill-selection\skill-selection-v1\runner-report.json`: `passed:true`, `held_out_accuracy:1`,
`accuracy_at_export:1`, `exact_prompt_leakage:0`, `eligible_pair_count:75`, `trained_pair_count:900`
(=75×12 адаптеров), `unit_profile_counts` по 65-81 на юнит, `semantic_quality_claimed:false`,
`coverage_type:"s14_contract_catalog_not_runtime_semantic_quality"`. `semantic_domain_matrix[*].held_out_fixture`
(`heldout.prompt_understanding.v1` …) — строки КАТАЛОГА (`SmartSkillsSemanticUnitCatalog`), а не исполненные
per-unit held-out прогоны; evidence прямо: «catalog-backed … metadata; continue report does not claim runtime
semantic quality».

Собственный тест проекта `SmartSkillsSemanticQualityP22SourceDisjointRuntimeEvalTests` — это НЕ оценка, а
**фиксация отсутствия оценки**: `acceptedRuntimeRows = Array.Empty` (`:88`), поэтому
`sourceDisjointRuntimeEvalExists` всегда false; verdict `NO_TRUE_SOURCE_DISJOINT_RUNTIME_EVAL_FOUND` (`:205`);
единственная scenario-строка `not_run_no_true_source_disjoint_runtime_eval` (`:156/:278`); контролирующие
отчёты `p16e/p16e2` → `candidate_schema_eval_only:true` (schema-only, не runtime), `p21` →
`sourceDisjointProven:false`. Тест ассертит, что модели НЕ меняются и никакой side-by-side не запускался.

Вывод: заявленная `held_out_accuracy:1` НЕ является source-disjoint. Для лексического bag-of-words argmax
100% на held-out означает лексическую утечку слов из train-корпуса (память кооккурренса слов), а не
генерализацию; сам проект (P22) признаёт, что настоящего source-disjoint runtime eval для skill-selection
не существует. `exact_prompt_leakage:0` проверяет лишь ТОЧНЫЙ реплей промпта (SHA256 `_trainedPromptKeys`) +
перестановку порядка кандидатов, но не лексическое перекрытие ≥4-символьных слов train↔eval — реальный
источник утечки у bag-of-words модели.

## НОВЫЕ дыры (не дублируют P02/P05/P10/P14/P18/P21/P22)

| Компонент | Реальный статус | Evidence file:line | Дыра | Шаги закрытия |
|---|---|---|---|---|
| Source-disjoint runtime eval выбора скиллов | MISSING (self-recorded gap) | `Tests\SmartSkillsSemanticQualityP22...Tests.cs:88,:145-156,:205,:278`; controlling `p16e/p16e2 candidate_schema_eval_only:true`, `p21 sourceDisjointProven:false`; `runner-report.json:224 held_out_accuracy:1` | Собственный тест проекта фиксирует `NO_TRUE_SOURCE_DISJOINT_RUNTIME_EVAL_FOUND` (0 принятых rows, scenario not_run); source-disjoint гейт для выбора скиллов не построен, при этом промоутнутый ассет заявляет held-out accuracy 1. Прямо мой фокус. | Построить row-level runtime holdout ledger, где source-family+pair-id отсутствуют в обоих checkpoints; прогнать `DecideFromTrainedState` на нём для common+candidate; гейтить промоушен на реальной source-disjoint accuracy; держать readiness=false до прохождения. |
| Промоутнутая `held_out_accuracy:1` — циркулярная/in-sample, не гейтит промоушен | BUILT_NOT_PROVEN / circular held-out | `SkillSelectionMachine.cs:381-383` accuracy=correct/count, `:374` exact `SameSkillSet`, `:370` `DecideFromTrainedState`; eval-rows из того же корпуса (75 eligible→900); каждый юнит отдаёт РОВНО один скилл (`Hierarchy:477 SelectedSkillIds:[scored.SkillId]`) | `EvaluateAsync` меряет точное множество на строках того же корпуса; 100% достижимо лишь на тривиально-однокскилловых строках, совпадающих с argmax; ни один прогон не гейтит промоушен на source-disjoint наборе. Отдельно от P21 gap-3 (prompt-to-task, 5 авторских сценариев) — здесь skill-selection + exact-set + fan-out 900. | Заменить in-sample eval на source-disjoint held-out; ассертить, что accuracy падает на настоящем disjoint-наборе; персистить per-row правильность; не промоутить при circular=100%. |
| Авторитет выбора = выученный лексический bag-of-words argmax; «11 семантических юнитов» — misnomer | BUILT_NOT_PROVEN / lexical-only, no semantic generalization | `SkillSelectionMachine.cs:2726-2736` (сигналы = сырые ≥4-симв. слова); `SkillSelectionLogicalHierarchy.cs:454-455` `Score=Σ per-word learned weights`, `:458` argmax, `:462` Score≤0→no decision, `:492` `Confidence=0.50+Score*0.05`; каждый юнит `runtime_semantic_quality_claimed:false` (runner-report) | Решение — линейный счётный скорер по буквальным словам промпта: синоним/парафраз без общего ≥4-симв. слова даёт Score=0 → нет решения. Веса выучены (не hardcoded alias как prompt-to-task), но модель лексическая, не семантическая; 11 «semantic units» — одинаковый bag-of-words argmax поверх разных весовых таблиц. Уточняет P05 gap-1 («profile counters») точным алгоритмом и потолком генерализации. | Заменить лексические сигналы на выученный/эмбеддинговый экстрактор (словарь из данных, не из слов промпта); доказать инвариантность к парафразу/синониму; ablation, что решение не сводится к общим словам. |
| Тренер skill-selection игнорирует rejected-сторону — нет DPO-маржи / reward scalarization | STUB / positive-count only | `SkillSelectionMachine.cs:1370-1382` `LearnPromptFeatureWeights` инкрементит только `chosenSkillId` профиль; `pair.Rejected` не читается; `:759-768` только `_candidatePositionWeights` от chosen; confidence — константная формула | Хотя подаются `SkillSelectionMachinePair` с Chosen/Rejected, обучение = положительно-счётная таблица частот слов на chosen-скилл; rejected-сторона отброшена, нет `reward(chosen)−reward(rejected)`, нет down-weight. «DPO» номинально. Нарушение reward-scalarization + acceptance-signal-independence для стадии skills. Отдельно от P22 gap-2 (orchestration trainer) и P21 gap-4 (prompt-to-task append). | Реализовать реальный контрастив: использовать rejected-скиллы (down-weight/маржа) → мутация типизированного состояния; убрать confidence-константу; доказать held-out прирост от использования rejected-стороны. |
| Нет ablation-harness для skill-selection (только candidate-order guard) | MISSING (ablation harness) | `ls 43.SmartSkillsDomain\*blation*`→только `PromptToTaskAblationHarness.cs`; в skill-selection только `CandidateOrderLeakageGuardUnit` (`SkillSelectionMachine.cs:145-148`, перестановка порядка кандидатов) | Коррекция P02 gap-4: guard появился, НО полноценного ablation (инвариантность к word-shuffle/удалению сигналов/подмене синонимов, доказывающего, что это не лексический lookup) для skill-selection нет; CLAUDE.md требует ablation-gate для обучаемого юнита. Отдельно от P21 gap-2 (prompt-to-task ablation — структурный no-op). | Написать `SkillSelectionAblationHarness`: прогон на ЗАГРУЖЕННОМ checkpoint с трансформами (leakage-removal, word-permutation, synonym-substitution); ассертить стабильность/деградацию выбора; фиксировать реальные ablation-контрасты. |
| `exact_prompt_leakage:0` переоценивает «нет утечки» для лексической модели | PARTIAL / leakage-guard incomplete | `runner-report.json:225-226` `exact_prompt_leakage:0`/`exact_prompt_hit_count:0`; guard = `_trainedPromptKeys` SHA256 точного промпта (`SkillSelectionMachine.cs:735,:2787`) + candidate-order permutation; НЕТ проверки перекрытия ≥4-симв. слов train↔eval | Leakage-контроль ловит только точный реплей промпта и позиционную утечку, но не главный риск bag-of-words: общие ≥4-символьные слова между train и eval промптами (лексическая утечка признаков). Поэтому «0 leakage» + accuracy 1 не доказывает генерализацию. Новый класс (не в P02/P21). | Добавить лексический leakage-контроль (Jaccard слов train↔eval, per-feature overlap); держать accuracy-claim в паре с этим контролем; при высоком word-overlap считать eval не-disjoint. |

## Границы вывода

- Проверен реальный код и промоутнутый ассет; НЕ запускались тесты/тренинг/runtime (bounded read-only).
- `held-out accuracy 1` — это исполненный, но circular in-sample eval; source-disjoint eval, по признанию
  самого проекта (P22), отсутствует.
- Коррекции к мастеру: P02 gap-4 (guard теперь реализован; позиционный счётчик — diagnostics-only, не
  авторитет). Резидуал вынесен в G5 (нет полного ablation-harness).
- Не заявляю semantic quality / readiness / promotion выбора скиллов — все флаги ассета `*_claimed:false`.
