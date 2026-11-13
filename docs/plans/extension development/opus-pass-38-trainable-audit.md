# Pass P38 — Trainable-architecture real state: TRAINABLE-ARCHITECTURE-AUDIT-v1..v4 vs реальный код

Дата: 2026-07-08. Ассессор: Opus 4.8. Фокус: сравнить сам аудит trainable-архитектуры
(`docs/plans/TRAINABLE-ARCHITECTURE-AUDIT-v1.md` + v2/v3/v4 + EXECUTION-STATE) с ЖИВЫМ кодом
`C:\Projects\llm_mvp_core`. Вопрос: настоящие ILearningMachine/ILogicalUnit или
детерминированные shortcut'ы / скрытый детерминизм — и ЧТО из предписанного аудитом реально
исполнено. Все находки — evidence-bound (file:line / artifact), только НОВЫЕ относительно
мастер-регистра (P05, P21–P28 уже покрыли per-machine код трёх расширений).

## Что проверено (bounded reads)

- `docs/plans/TRAINABLE-ARCHITECTURE-AUDIT-v1.md` (1652) — секции 1–8, включая §4 ProjectWiki
  KNN / PromptFlowGrounding и §1 «Конкретная замена всех словарей SkillSelectionMachine».
- `TRAINABLE-ARCHITECTURE-AUDIT-EXECUTION-STATE.json` целиком — статус исполнения SS-01..SS-12.
- `TRAINABLE-ARCHITECTURE-AUDIT-v2.md` (голова/вердикт), `-v3.md` (§1 вердикт), `-v4.md` (целиком).
- Реальный код §4: `42.ProjectWikiDomain/OrchestrationKnnStore.cs` (67),
  `PromptFlowGroundingModel.cs` (195), `OrchestrationPlanMachine.cs` (572);
  тест `Tests/OrchestrationPlanTests.cs` (799) целиком.
- Грепы: `knn_role` по репо (113 хитов, ни одного в `42.ProjectWikiDomain/`),
  вызыватели `OrchestrationPlanMachine`/`PromptFlowGroundingModel`, `semantic_quality` в `42.`.

## Ключевой факт прохода

Аудит trainable-архитектуры БЫЛ исполнен (SS-01..SS-12, все accepted+pushed,
`goal_reached:true`), но «завершён» он тем, что **перенёс сырые словари/счётчики в
типизированные `ILogicalUnit`-state-записи** внутри `SkillSelectionMachine`/PromptToTask/
Orchestration, — то есть выполнена БУКВА «allowed-level wrapper», а не суть R4 (заменить
скрытый детерминизм обучаемым семантическим выбором). Это подтверждает сам же поздний аудит
v3/v4 своими словами: три поставляемых расширения остаются «честно промаркированными
pattern-match baselines», `SemanticQualityClaimed=false`, «ещё не полноценные trainable
semantic models», с большим списком «Remaining work». При этом EXECUTION-STATE тех же repair'ов
пишет `remaining_known_gap: "None for this active audit scope"` и `actionable_gaps: []`.

Отдельно: САМЫЙ строгий обучаемый юнит репозитория живёт НЕ в трёх расширениях, а в
`42.ProjectWikiDomain` (`OrchestrationPlanMachine`/`PromptFlowGroundingModel`) — там есть
реальный weights-ablation тест, input-only-feature leakage-guard'ы, exact-prompt-leakage gate,
target-label-leakage rejection. Аудит НЕ потребовал довести три поставляемые машины до паритета
с этим уже существующим в репо эталоном; и предписанный им §4-repair (метка `knn_role`) вообще
не исполнен (SS-01..SS-12 не трогали `42.ProjectWikiDomain`).

## НОВЫЕ находки (dedup против P05/P21–P28)

### G38-1 — Аудит записал `goal_reached=true` через relocation словарей, суть R4 осталась открытой
- Real-status: BUILT_NOT_PROVEN / FALSE-COMPLETION-AT-AUDIT-LEVEL.
- Evidence: `TRAINABLE-ARCHITECTURE-AUDIT-EXECUTION-STATE.json` `goal_reached:true`,
  `architecture_review.remaining_known_gap:"None for this active audit scope. All raw
  dictionary/source-pair replacements named by the audit are implemented..."`, `actionable_gaps:[]`,
  `completed_slices` SS-01..SS-12 all `accepted`/`pushed`; SS-12 `implemented_change` = «Removed
  machine-level _taskCompletionSignals/_taskCompletionSourcePairs authority; added
  TaskCompletionSkillTrainingState ... TaskCompletionSkillUnit runtime path». ПРОТИВОРЕЧИТ
  `TRAINABLE-ARCHITECTURE-AUDIT-v4.md` («semantic quality всё ещё не заявлена ... checkpoint
  rewrite и положительные fresh-load/continue-training gates остаются будущей работой» +
  раздел «Remaining work») и `-v3.md §1` («ещё не полноценные trainable semantic models»).
- Gap: «завершение» аудита = переезд сырых `Dictionary` в typed `ILogicalUnit`-state; фактическая
  обучаемость (learned generalization вместо lexical/deterministic authority) НЕ достигнута, но
  state объявляет 0 остаточных gap'ов. Это false-completion на уровне управляющего артефакта.
- Closure: пометить EXECUTION-STATE как «wrapper-relocation done, trainable-capability OPEN»;
  вернуть в `actionable_gaps` пункты v4 «Remaining work»; запретить `goal_reached=true`, пока не
  доказан хотя бы один held-out semantic uplift на загруженном checkpoint.

### G38-2 — Аудит переопределил R4: детерминизм как final authority разрешён «если SemanticQualityClaimed=false»
- Real-status: GOVERNANCE DOWNGRADE / R4-REINTERPRETATION.
- Evidence: `-v1.md:48-56` («это не значит, что они запрещены ... могут быть baseline ... не
  могут быть выданы за semantic unit БЕЗ честных gates»); `-v3.md §1` («В SkillSelectionMachine
  есть много typed dictionary/KNN state ...; сейчас это допустимо только потому, что
  SemanticQualityClaimed=false»). Живой runtime-authority — лексический (P23: `Score=Σ
  per-word learned weights`, argmax).
- Gap: Luba R4 (NO HIDDEN DETERMINISM) требует ОБУЧАЕМОЙ замены (named unit + trained state +
  checkpoint + fresh-load/continue-training + ablation), где нужен trainable unit — не «честной
  метки baseline». Аудит выдал бессрочный escape-hatch: три поставляемые машины могут навсегда
  оставаться детерминированными baseline'ами при `SemanticQualityClaimed=false`. Это ослабление
  требования, а не его выполнение.
- Closure: зафиксировать в governance, что honest-label baseline — временная стадия, а не
  выполнение R4 для поставляемого расширения; ввести срок/gate перехода baseline→trainable;
  не принимать «claimed=false» как основание закрыть R4.

### G38-3 — §4-repair (ProjectWiki KNN role-labeling) не исполнен; SS-slices не касались 42.ProjectWikiDomain
- Real-status: MISSING / prescribed-repair-unexecuted.
- Evidence: `-v1.md:1579-1588` предписывает `knn_role=model_component|fallback_baseline` в report
  + тесты роли/fallback-honesty; `git grep -in "knn_role|fallback_baseline|model_component" --
  42.ProjectWikiDomain/` → 0; `EXECUTION-STATE` `allowed_write_scope` всех слайсов — только
  `43.SmartSkillsDomain/*` + Tests, ни один не в `42.ProjectWikiDomain`.
- Gap: конкретный §4-repair (явная роль KNN в отчёте/манифесте OrchestrationPlan) отсутствует;
  §4 вообще выпал из исполнения. Ни один прежний проход (P21–P28) §4/`OrchestrationPlanMachine`
  не покрывал.
- Closure: добавить в OrchestrationPlan report/manifest поле `knn_role` (model_component для
  Mode1 grounding, fallback_baseline для Mode2) + тест роли; либо зафиксировать §4 как
  отдельный незакрытый пункт бэклога.

### G38-4 — Эталон trainable-integrity уже есть в 42.ProjectWikiDomain, но аудит не потребовал паритета для 3 расширений
- Real-status: ARCH ASYMMETRY / reference-pattern-not-ported.
- Evidence: `Tests/OrchestrationPlanTests.cs` — `Mode1Grounding_Ablation_EmptyWeightsDegrade`
  (:260-272, ablation: пустые weights → теряется LearnedFlowGrounding),
  `PromptFlowGroundingModel_ConfidenceUsesInputOnlyFeatures` (:514-539),
  `_DoesNotGroundFromChosenTitleOnly` (:480-496), `_DoesNotUsePredictedFlowRefAsKnnQueryFeature`
  (:498-512), `ExactPromptOverlap_FailsGate` (:284-309),
  `OrchestrationPlanMachine_Mode1GroundingRejectsTargetLabelLeakageRows` (:541-562). Это ровно те
  guard'ы, которых нет у трёх поставляемых машин (P21 gap-2: prompt-to-task ablation = структурный
  no-op; P23 gap-5: нет ablation-harness skill-selection; P23 gap-6: нет лексического
  leakage-контроля). Аудит §1/§2/§3 не потребовал довести три машины до этого эталона.
- Gap: репозиторий уже содержит рабочую реализацию требуемых ablation/leakage-guard'ов, но аудит
  оставил три расширения без паритета — closure не «изобрести», а ПОРТировать существующий паттерн.
- Closure: перенести из `OrchestrationPlanMachine`/`PromptFlowGroundingModel` в prompt-to-task/
  orchestration-reasoning/skill-selection: (а) weights-ablation тест (обнуление learned state →
  деградация/отказ от claim), (б) input-only-feature guard, (в) exact-prompt + word-overlap
  leakage gate; гейтить промоушен на их прохождении.

### G38-5 — Юниты, ВВЕДЁННЫЕ исполнением аудита (TaskCompletionSkillUnit и др.), сами не прошли независимый ablation/leakage-аудит
- Real-status: PARTIAL / self-reviewed-not-independently-audited.
- Evidence: `EXECUTION-STATE.current_slice.review_cycles` = только parent self-review
  («parent_architecture_review_passed», «parent_code_review_passed», «parent_final_raw_field_audit_passed»);
  `subagent_policy.parent_session_may_directly_edit_production_code:false`, но ревью — parent, не
  независимый sub-agent. Новый `TaskCompletionSkillUnit` несёт KNN (`FindNearestTaskCompletionExamples`,
  `ComputeTaskCompletionSupport`) и `LearnCompletionFeatureWeights` (лексические веса), но для него
  нет ablation-harness'а (тот же класс, что G38-4) и нет независимой проверки leakage.
- Gap: код, добавленный самим repair'ом (typed KNN/feature-weight юниты), закрыт parent-само-ревью
  без независимого ablation/source-disjoint gate — риск, что «wrapper» просто переупаковал ту же
  лексическую детерминированность в новый юнит и объявил закрытым.
- Closure: прогнать независимый (sub-agent) ablation/leakage/source-disjoint аудит именно по
  юнитам, введённым SS-01..SS-12; не засчитывать parent-self-review как независимую проверку.

## Итог P38

Аудит trainable-архитектуры — добросовестный по разметке и honesty-флагам, НО его «завершение»
(`goal_reached=true`) означает relocation словарей в typed units, а не выполнение R4: три
поставляемые машины остаются детерминированными lexical-baseline'ами (что подтверждают v3/v4).
§4 (ProjectWiki KNN) выпал из исполнения. Готовый эталон trainable-integrity уже есть в
`42.ProjectWikiDomain`, но не портирован в расширения. Это НЕ «проект готов» — это «аудит закрыт
на wrapper-уровне, обучаемость не доказана».
