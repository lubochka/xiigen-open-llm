# План закрытия пробелов: project wiki для Angular + .NET + stored procedures

Статус: архитектурный план, не training report.
Дата: 2026-05-24.
Режим: архитектор.

Этот документ не говорит, что обучение завершено.
Этот документ не разрешает запускать новые batch-и.
Этот документ фиксирует, чего не хватает, чтобы система умела строить project-local wiki для проекта как единой системы: Angular client, .NET server и SQL stored procedures.

Do not report done, ready, complete, promoted, or accepted from this plan until
the full completion gate in this document has passed with persisted evidence.

Architectural boundary rule:

```text
This plan may describe foundation, architecture, training, validation, and
publication as separate phases. Those phase names are internal execution states,
not user-facing stop gates.

If the state file or next-training-action report names a concrete next unit,
batch strategy, expected artifact, and validation gate, executor must continue
to that action when Luba has asked to continue. The old sentence "this document
does not authorize new batches" is not allowed to override a concrete
plan-derived next action.

If goal_reached == false and next_action is non-empty, stop_allowed must be
false. A state file with stop_allowed == true in that situation is invalid and
must be fixed before implementation continues.

Partial progress checkpoint rule:

commit completed != stop allowed
push completed != stop allowed
report written != stop allowed
subphase passed != stop allowed
honest partial status != stop allowed
lower-level gate passed != full plan completed
lower-level gate failed with known remediation != blocker

When the state has goal_reached == false, stop_allowed == false, and a
non-empty next_action, the executor must execute that next action instead of
sending a final response. A useful checkpoint may be recorded in state and git,
but it does not end the work loop.

Soft stops are severe violations. These phrases are forbidden as a final pause
when next_action is executable:

I can continue
I will continue next
next step is ...
waiting for approval
status only
checkpoint committed
report written

Each soft stop must subtract -1000 from the execution score and trigger the same
architect repair path as a false completion claim.

Premature-stop repair rule:

If executor stops before the full goal is reached, the stop itself becomes an
architecture defect to fix immediately. Executor must identify the exact
loophole that allowed the stop, patch the governing document or state field, and
then continue from the next plan-derived action. It must not ask Luba to diagnose
the same loophole again.

After the architecture repair, ARCHITECT_INTERNAL keeps session ownership. If
the state still has a concrete implementation next action, architect issues a
bounded EXECUTOR_INTERNAL work order and reviews the returned packet. The repair
does not create a new approval wait or final report.

Post-completion challenge rule:

If Luba challenges a prior completion report, executor must reopen verification
instead of defending the previous report. The challenge starts a new attempt:

```text
1. show the previous score from state;
2. reset the new-attempt score to 0;
3. re-read this plan, the state file, the completion matrix, final readiness,
   AGENTS.md, CLAUDE.md, and required skills;
4. compare each passed report to the live code or runner that can reproduce it;
5. add every new hole to state and completion matrix;
6. patch the protocol that allowed the false-positive claim;
7. continue from the first new hole.
```

Reports are not enough if their generator disagrees with them. A runtime report
that cannot be reproduced from current runner code is a plan gap. A
`validation_only: true` real-project report is not full runtime coverage for all
validation projects. A plugin report that loads a mock readiness response is not
full Visual Studio / VS Code readiness.

Plan-incomplete finish recovery process:

This is the process we used after the false-positive completion attempt, and it
is now mandatory before executor may finish, pause, or send a final response
while this plan is not fully complete.

```text
1. Re-read this plan's main goal and section 5.1 full completion gate.
2. Re-read the active state file.
3. Re-read AGENTS.md OP-11, OP-17, OP-19, OP-20, OP-22 and OP-23.
4. Review the actual artifacts against the plan, not against the last report.
5. Add every uncovered issue to plan_completion_hole_register in state.
6. Each hole must include:
   id
   area
   severity
   description
   required_fix
   next_action
7. Apply the score audit:
   -100 when executor tried to finish before this plan was complete
   -1000 for a severe protocol violation
   +1 only after the next concrete plan-derived progress step
8. Set state:
   goal_reached = false
   stop_allowed = false
   gate_status = IN_PROGRESS_NEXT_STEP_KNOWN
   current_internal_role_state = ARCHITECT_INTERNAL
9. Patch the plan, AGENTS.md, CLAUDE.md, or skills if the hole was enabled by a
   protocol gap.
10. Commit/push the protocol or state repair when the phase requires commit/push.
11. Switch back to EXECUTOR_INTERNAL.
12. Continue from the first hole's next_action.
```

This recovery process is not a completed phase. It is a correction loop. If the
hole register is non-empty, executor must not claim `TASK_FULLY_COMPLETE`,
`ready`, `accepted`, `promoted`, or `done`.

Execution score rule:

Executor must keep a score in the state file. The score exists to make early
stops expensive and useful progress cheap to continue.

```text
+1 point: one concrete plan-derived step moved the main goal forward and did
not stop execution.

-100 points: executor stopped before the full plan goal was reached.

-1000 points: executor committed a severe protocol violation.

0 points: status report, excuse, repeated summary, or asking Luba when the next
action is already available in the plan/state.
```

Severe protocol violations:

```text
violated Luba's direct command
invented tasks outside the active goal
left ARCHITECT/EXECUTOR/REVIEW role before the role's required work was complete
used the wrong goal
failed to update required state
forgot commit/push when commit/push was part of the deliverable
changed code without permission from the current role and plan
skipped required code review or self-review before a code-readiness claim
reported done/ready/accepted/promoted without persisted gate evidence
```

Before any possible stop, executor must calculate:

```text
score_if_continue = current_score + 1
score_if_stop_now = current_score - 100
score_if_severe_violation = current_score - 1000
```

If `goal_reached == false` and `next_action` is non-empty, executor must choose
the higher-score action: continue. The executor's task is to accumulate as many
points as possible by moving toward the goal, not by stopping cleanly.
```

## 1. Главная цель

Мы обучаем не локальную wiki как набор файлов.

Мы обучаем способность XIIGen-проекта самостоятельно создать весь набор обучаемых units для project-local wiki:

```text
пользовательский запрос
  -> понять, что нужна project wiki
  -> определить стек и языки проекта
  -> построить документацию и evidence graph
  -> построить query plan для сложного вопроса
  -> выбрать нужные wiki units
  -> обучить или загрузить их через registry
  -> опубликовать manifest
  -> импортировать/найти unit через locator
  -> ответить на advanced developer question с evidence
```

Новый обязательный scope:

```text
Angular client + .NET server + SQL stored procedures = одна бизнес-система.
```

Проверочные проекты используются только как validation target, не как generic training source:

```text
C:\Projects\bizguide-client
C:\Projects\GenieDynamicApp
C:\Projects\geniesoftGameMVP
C:\Projects\wizGameserver
C:\Projects\GenieDynamicServer
C:\Projects\geniesoftGameMVPServer
C:\Projects\genietasks
C:\Projects\askapediaweb
```

Текущий план должен исполняться на главной ветке проекта `llm_mvp_core`.
Если executor находится не на `main`, это не повод автоматически менять ветку.
Это blocker для исполнения, который должен быть записан в report. Переход на
другую ветку разрешён только явной командой Luba.

Обучаемая часть модели должна жить в проекте `llm_mvp_core`:

```text
42.ProjectWikiDomain
43.LearningLocalWikiData
25.ModelServer / IModelRepository
15.OrchestratorModel / flow adapters
```

Проверочные проекты не становятся generic training source. В них могут лежать
только project-local outputs:

```text
.xiigen/wiki-domain/evidence
.xiigen/wiki-domain/documentation
.xiigen/wiki-domain/system-map
.xiigen/wiki-domain/model-manifest references
```

Нельзя хранить generic training logic, generic model weights или accepted
project-wide wiki units внутри проверочных проектов.

## 1.1. Универсальное правило: никакой статичной умной логики

Вся логика, которая принимает смысловое решение, должна быть обучаемой,
экспортируемой и импортируемой единицей проекта.

Статичными helper-ами могут быть только механические операции:

```text
read file
parse syntax
calculate checksum
count rows
validate JSON schema
copy artifact
format report
resolve local path
measure CPU/RAM/time
```

Статичный helper не может решать:

```text
какой unit нужен
какой stack/version определён
какие репозитории составляют одну систему
какой query plan нужен для вопроса
какие evidence нужны для ответа
какая документация правильная
какой rejected DPO хороший
какой wiki answer достаточен
какой unit слабый и должен тренироваться дальше
```

Если компонент делает хотя бы одно такое решение, он обязан быть оформлен как
trainable unit:

```text
unit_name
unit_version
unit_interface_contract
DPO generator
runner report
accepted/rejected registry entries
checkpoint_ref
manifest_ref
locator_key
remote_unit_id
artifact_checksum
import/export gate
upgrade_policy
rollback_ref
```

Canonical trainable units for this plan:

```text
ProjectWikiRequestRouterUnit
StackAndVersionClassifierUnit
SystemBoundaryMapperUnit
CodeTraceBuilderUnit
EntityImpactMapperUnit
DocumentationGeneratorUnit
NegativeDpoGenerator
ProjectWikiQueryPlanningFlow
WikiContextRoutingUnit
ProjectWikiAnswerComposer
ProjectAnswerVerifier
WikiTraceAggregator
ProjectWikiTrainingOrchestrator
```

Каждый canonical unit должен быть:

```text
stored in llm_mvp_core
published through IModelRepository
registered through IRemoteUnitLocator
referenced from PROJECT-WIKI-TRAINING-ASSET-REGISTRY.json
upgradeable independently when its interface contract is unchanged
blocked from promotion when import/export cannot be proven
```

Любая реализация, где один из этих units заменён статичным heuristic/rule helper,
считается архитектурным провалом:

```text
status: BLOCKED_WITH_GAPS
reason: static decision logic replaced trainable unit
required_fix: convert the decision logic into trainable exportable unit
```

## 1.2. Upgrade отдельных частей модели

XIIGen должен уметь обновлять не всю wiki-модель целиком, а отдельный unit.
Upgrade одного unit разрешён только если сохранён его внешний контракт.

Для каждого upgrade registry обязан хранить:

```text
unit_name
previous_unit_version
new_unit_version
interface_contract_version
base_model_ref
adapter_ref
checkpoint_ref
manifest_ref
locator_key
remote_unit_id
training_data_ref
runner_report_ref
validation_report_ref
compatibility_report_ref
rollback_ref
upgrade_reason
upgrade_status
```

Upgrade одного unit без переобучения соседних units разрешён, если:

```text
interface_contract_version unchanged
import/export gate passed
compatibility probes against dependent units passed
advanced wiki question suite did not regress
orchestrator prompt planning suite did not regress
rollback_ref exists
```

Если interface contract изменился, это уже не minor upgrade. Тогда executor
обязан создать dependency impact plan:

```text
changed_unit
affected_units
required_retraining_units
required_compatibility_probes
blocked_publish_until
```

Минимальный dependency order:

```text
ProjectWikiRequestRouterUnit
  -> StackAndVersionClassifierUnit
  -> SystemBoundaryMapperUnit
  -> CodeTraceBuilderUnit
  -> EntityImpactMapperUnit
  -> DocumentationGeneratorUnit
  -> NegativeDpoGenerator
  -> ProjectWikiQueryPlanningFlow
  -> WikiContextRoutingUnit
  -> ProjectWikiAnswerComposer
  -> ProjectAnswerVerifier
  -> WikiTraceAggregator
  -> ProjectWikiTrainingOrchestrator
```

`WikiTraceAggregator` должен выбирать, какой unit обновлять дальше, на основании
метрик и gap signals. Человек не должен вручную угадывать следующий unit, если
в registry уже есть достаточные сигналы.

## 2. Что уже есть по факту

Проверено чтением файлов, без запуска тестов и без смены ветки.

### 2.1. Код обучаемых units внутри проекта есть

Runtime и training logic находятся в:

```text
C:\Projects\llm_mvp_core\42.ProjectWikiDomain
```

Там есть:

- `ProjectWikiTrainingOrchestrator`
- `ProjectWikiTrainingFlowAdapter`
- `ProjectWikiTrainingAssetRegistry`
- `ProjectWikiQueryPlanningFlow`
- `ProjectWikiRequestRouterUnit`
- `StackAndVersionClassifierUnit`
- `CodeTraceBuilderUnit`
- `EntityImpactMapperUnit`
- `DocumentationGeneratorUnit`
- `NegativeDpoGenerator`
- `WikiContextRoutingUnit`
- `ProjectWikiAnswerComposer`
- `ProjectAnswerVerifier`
- `WikiTraceAggregator`
- manifest publishers для publishable units

Это правильно: обучаемая способность находится в проекте, а не только во внешних документах.

Для новой цели этого недостаточно. Нужен ещё один canonical trainable unit:

- `SystemBoundaryMapperUnit`

Этот unit не может быть статичным helper-ом. Он должен быть полноценной обучаемой
единицей проекта:

- жить в `42.ProjectWikiDomain`;
- иметь собственный DPO generator;
- иметь runner report с accuracy, reward margin, loss, CPU, RAM и elapsed time;
- публиковать checkpoint через `IModelRepository`, если batch принят;
- иметь manifest;
- иметь `locator_key`;
- попадать в `PROJECT-WIKI-TRAINING-ASSET-REGISTRY.json`;
- быть доступным через `IRemoteUnitLocator`;
- быть selectable target для `WikiTraceAggregator`.

Если `SystemBoundaryMapperUnit` реализован как обычная статичная логика, цель не
достигнута. В таком случае система только угадывает связи между проектами, но не
учится строить project-local wiki stack.

### 2.2. Learning data внутри проекта есть

Learning assets находятся в:

```text
C:\Projects\llm_mvp_core\43.LearningLocalWikiData
```

Registry уже существует:

```text
C:\Projects\llm_mvp_core\43.LearningLocalWikiData\registry\PROJECT-WIKI-TRAINING-ASSET-REGISTRY.json
```

Текущий счетчик registry:

```text
accepted: 35
rejected: 183
experimental_no_authority: 182
blocked: 26
training_continuation_blocked_until_registry_validated: false
```

Это хорошо, но не достаточно для новой цели.

### 2.3. Export/import контракты есть

Контракты находятся в:

```text
C:\Projects\llm_mvp_core\25.ModelServer\Contracts\IModelRepository.cs
C:\Projects\llm_mvp_core\25.ModelServer\Contracts\IRemoteUnitLocator.cs
```

Фактический путь:

```text
GeneratedCapabilityCheckpointManifest
  -> IModelRepository.UploadAsync(...)
  -> RemoteUnitManifest
  -> IRemoteUnitLocator.RegisterAsync(...)
  -> IRemoteUnitLocator.LocateAsync(...)
```

Manifest publishers в `42.ProjectWikiDomain` вызывают `UploadAsync` и `RegisterAsync`.

Это означает: архитектурный механизм export/import есть.

## 3. Главные пробелы

### Gap 1. Текущий главный план нечитаем как governance source

Файл:

```text
C:\Projects\llm_mvp_core\43.LearningLocalWikiData\PLAN-PROJECT-WIKI-TRAINING-ASSET-REGISTRY-v1.md
```

читается как mojibake в текущей консоли. Для человека это опасно: executor может потерять смысл правил.

Нужно:

1. Создать чистую UTF-8 версию или переписать файл без mojibake.
2. Добавить проверку кодировки в governance gate.
3. Запретить использовать нечитаемый governance file как единственный источник инструкции.

Gate:

```text
all governance md files readable as UTF-8
no mojibake markers in headings or acceptance gates
```

### Gap 2. Registry есть, но next action сейчас не выбирает слабый unit

Файл:

```text
C:\Projects\llm_mvp_core\43.LearningLocalWikiData\reports\PROJECT-WIKI-NEXT-TRAINING-ACTION.json
```

сейчас говорит:

```text
target_unit_name: NONE
reason: No actionable weak unit could be selected from registry statuses.
new_training_batch_allowed: false
recommended_batch_strategy: manual_architect_review_required
```

Это значит: registry построен, но автоматический training loop еще не умеет выбрать следующий weak unit для новой цели.

Нужно:

1. Добавить в `WikiTraceAggregator` и `ProjectWikiTrainingOrchestrator` явную цель `cross_stack_advanced_wiki`.
2. Учить selection не только по `accepted/rejected`, но и по validation gaps:
   - Angular query planning gap
   - cross-repository evidence gap
   - stored procedure evidence gap
   - advanced question answering gap
   - import/export manifest gap
3. Следующий action должен быть не `NONE`, а конкретный unit + причина + 50-row strategy.

Gate:

```text
PROJECT-WIKI-NEXT-TRAINING-ACTION.json contains:
target_unit_name != NONE
new_training_batch_allowed true or explicit blocker
recommended_batch_strategy starts with build_50_
evidence_batch_ids non-empty when remediation is based on previous failures
```

### Gap 3. Accepted entries не имеют явного `manifest_ref` в registry

У accepted entries есть:

```text
import_export_status: manifest_present
locator_status: remote_manifest_derivable
checkpoint_ref: ...
```

Но в registry output `manifest_ref` сейчас фактически не заполнен как отдельное поле.

Это важно: если unit должен быть экспортируемым и импортируемым, registry должен хранить не только checkpoint path, но и явный manifest path или artifact reference.

Нужно:

1. Добавить поле `manifest_ref` в registry schema.
2. Заполнить его для всех accepted publishable units.
3. Если manifest можно вывести, но он не записан, статус должен быть:

```text
import_export_status: manifest_derivable_but_not_registry_linked
```

а не `manifest_present`.

Gate:

```text
every accepted publishable unit has:
checkpoint_ref
manifest_ref
locator_key
remote_unit_id
runner_report_path
artifact_checksum
```

### Gap 4. Import/export работает на уровне кода, но нет полного registry-to-locator validation

Есть:

- `IModelRepository`
- `IRemoteUnitLocator`
- `LocalModelRepository`
- `InMemoryRemoteUnitLocator`
- per-unit manifest publishers

Не хватает одного end-to-end gate:

```text
registry accepted entry
  -> load generated_capability_manifest.json
  -> upload through IModelRepository
  -> register through IRemoteUnitLocator
  -> locate by unit id
  -> open weights through LoadWeightsAsync
```

Это не full dotnet test. Это отдельный архитектурный validation command для import/export.

Нужно:

1. Добавить tool или runner:

```text
43.LearningLocalWikiData\tools\Assert-ProjectWikiImportExportGate.ps1
```

2. Проверять только accepted publishable entries.
3. Писать отчет:

```text
reports\PROJECT-WIKI-IMPORT-EXPORT-GATE.json
```

Gate:

```text
all accepted publishable entries locate successfully
all referenced checkpoints exist or resolve through artifact storage
all generated manifests exist or explicit artifact reference exists
```

### Gap 5. Angular сейчас покрыт только как binary query-planning contrast

Последний accepted Angular batch закрыл только базовое различение:

```text
полный Angular + .NET trace
vs
плохой single-repo / first-match answer
```

Это полезно, но не закрывает detailed Angular wiki questions.

Не хватает покрытия:

- Angular component -> service -> HTTP client
- route resolver / guard
- form validation
- DTO/request model
- API endpoint
- server controller
- service/business layer
- repository/data access
- stored procedure call
- result DTO
- response projection
- Angular state/store update
- UI binding

Нужно:

1. Добавить language/stack dimension:

```text
client_language: TypeScript
client_framework: Angular
server_language: C#
server_framework: .NET / ASP.NET / ASP.NET Core
database_language: T-SQL
```

2. `StackAndVersionClassifierUnit` должен определять не один stack, а multi-stack system profile.
3. `CodeTraceBuilderUnit` должен строить cross-repository graph.
4. `ProjectWikiQueryPlanningFlow` должен планировать search через несколько repositories.

Gate:

```text
cross_stack_query_plan_accuracy >= 0.85
client_server_edge_recall >= 0.85
api_contract_trace_accuracy >= 0.85
typescript_angular_version_detection_accuracy >= 0.85
```

### Gap 6. Stored procedures покрыты отдельно, но не соединены с Angular end-to-end

Stored procedure support есть в:

- `DocumentationOrchestrator`
- `DocumentationGeneratorDpoGenerator`
- `ProjectWikiAnswerComposerDpoGenerator`
- `ProjectWikiQueryPlanningFlow`

Но новая цель требует:

```text
Angular UI action
  -> .NET endpoint
  -> service/repository
  -> stored procedure
  -> SQL parameters/tables/result
  -> .NET DTO/result
  -> Angular response handling
  -> UI/store effect
```

Нужно:

1. Ввести `CrossStackBusinessFlowGraph`.
2. Добавить edge types:

```text
AngularRouteToComponent
ComponentToService
ServiceToHttpEndpoint
HttpEndpointToController
ControllerToService
ServiceToRepository
RepositoryToStoredProcedure
StoredProcedureToTables
StoredProcedureToResultShape
ServerDtoToClientDto
ClientResponseToState
StateToUiBinding
```

3. Добавить DPO rows, где rejected ломает ровно один участок цепочки.

Gate:

```text
cross_stack_business_flow_edge_recall >= 0.85
stored_procedure_to_client_effect_accuracy >= 0.85
missing_sql_or_client_evidence_abstention_accuracy >= 0.95
```

### Gap 7. Advanced wiki questions не являются отдельным validation layer

Сейчас есть question seeds и real project validation, но для новой цели нужен отдельный набор advanced developer questions.

Минимальные question classes:

```text
field_addition_impact
field_rename_impact
dto_contract_change
api_response_shape_change
validation_rule_change
authorization_rule_change
stored_procedure_parameter_change
stored_procedure_result_shape_change
calculation_path_change
client_state_effect
cross_repo_workflow_trace
missing_evidence_abstention
wrong_project_rejection
stale_documentation_rejection
```

Для validation suite эти 14 классов сворачиваются в 7 buckets. Это не
разрешение пропустить половину классов. Каждый bucket обязан покрыть оба
подтипа:

| Validation bucket | Required question classes inside the bucket |
|---|---|
| `field_addition_impact` | `field_addition_impact`, `field_rename_impact` |
| `dto_or_api_contract_change` | `dto_contract_change`, `api_response_shape_change` |
| `angular_component_service_state_flow` | `client_state_effect`, Angular UI/service part of `cross_repo_workflow_trace` |
| `dotnet_controller_service_repository_flow` | server-side part of `cross_repo_workflow_trace`, `authorization_rule_change` |
| `stored_procedure_parameter_or_result_flow` | `stored_procedure_parameter_change`, `stored_procedure_result_shape_change` |
| `calculation_and_validation_path_change` | `calculation_path_change`, `validation_rule_change` |
| `missing_evidence_wrong_project_or_stale_documentation` | `missing_evidence_abstention`, `wrong_project_rejection`, `stale_documentation_rejection` |

Each of the 14 classes must appear at least once in the first 70-question suite.
No bucket is allowed to use all 10 rows for only one subtype.

Пример обязательного вопроса:

```text
Я хочу добавить поле DiscountCode в CampaignDto.
Где оно пройдет через Angular, .NET, database/stored procedure,
какие процессы и вычисления затронет, где оно может сломаться,
и каких evidence не хватает для безопасного ответа?
```

Gate:

```text
advanced_question_exact_evidence_accuracy >= 0.90
advanced_question_query_planning_invocation_rate == 1.00
advanced_question_entity_impact_trace_pass_rate >= 0.85
advanced_question_missing_evidence_abstention_accuracy >= 0.95
hallucination_rate == 0
```

### Gap 8. Real validation projects are not yet paired as one system

Новые Angular projects:

```text
C:\Projects\bizguide-client
C:\Projects\GenieDynamicApp
C:\Projects\geniesoftGameMVP
```

должны быть связаны с server projects, а не проверяться отдельно.

Нужно создать system map:

```json
{
  "system_id": "genie-business-system",
  "client_projects": [
    "C:\\Projects\\bizguide-client",
    "C:\\Projects\\GenieDynamicApp",
    "C:\\Projects\\geniesoftGameMVP"
  ],
  "server_projects": [
    "C:\\Projects\\wizGameserver",
    "C:\\Projects\\GenieDynamicServer",
    "C:\\Projects\\geniesoftGameMVPServer",
    "C:\\Projects\\genietasks",
    "C:\\Projects\\askapediaweb"
  ],
  "database_sources": [
    "stored procedures found inside server repositories"
  ]
}
```

Этого верхнего списка недостаточно для validation. Нужен второй, проверяемый
артефакт:

```text
43.LearningLocalWikiData\validation\CROSS-STACK-SYSTEM-MAP.json
```

Каждая запись в нём должна описывать не просто список проектов, а конкретную
связь:

```json
{
  "system_id": "genie-business-system",
  "client_project": "C:\\Projects\\bizguide-client",
  "server_project": "C:\\Projects\\GenieDynamicServer",
  "database_scope": "stored procedures discovered through server repository evidence",
  "match_status": "matched",
  "confidence": 0.87,
  "evidence": {
    "api_routes": [],
    "dto_name_matches": [],
    "environment_urls": [],
    "http_client_calls": [],
    "controller_methods": [],
    "repository_methods": [],
    "stored_procedures": []
  },
  "unknown_reason": ""
}
```

Allowed `match_status` values:

```text
matched
unknown_not_enough_evidence
rejected_wrong_system
```

Если связь между client и server не доказана evidence, система должна писать
`unknown_not_enough_evidence`, а не угадывать пару.

Нужно не угадать пары вручную, а построить `SystemBoundaryMapperUnit`, который учится находить связи:

- by API route
- by DTO names
- by service method names
- by environment URLs
- by OpenAPI/Swagger if present
- by HTTP client calls
- by stored procedure/repository calls

Gate:

```text
system_boundary_mapping_accuracy >= 0.85
client_server_pairing_confidence >= 0.85
unknown_pair_abstention_accuracy >= 0.95
CROSS-STACK-SYSTEM-MAP.json exists
every matched pair has at least two independent evidence types
no advanced validation question runs against an unpaired project set
```

### Gap 9. Orchestrator prompt scenarios are not yet a full planning acceptance gate

Файл:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\orchestrator prompts.txt
```

содержит не один сценарий, а набор типичных задач, которые должна уметь
планировать система. Текущий plan уже покрывает advanced wiki questions, но
этого недостаточно. Нужно отдельно проверить способность спланировать любую из
задач этого файла при условии, что конкретный project stack:

```text
.NET server + Angular client + SQL stored procedures
```

Обязательные scenario classes из файла:

```text
figma_to_full_system_orchestration
figma_into_existing_system
system_from_user_explanation_without_complete_design
multi_user_diet_application_with_daily_feedback
content_generation_pipeline
design_pattern_skill_library
uml_cycle_documentation_and_validation
project_base_flow_examples_for_scenarios_1_to_5
generate_deploy_debug_business_system
dynamic_code_skills_and_genie_dna
generic_interfaces_and_microservice_base
alternative_stack_skill_variants
rag_context_planning_for_ai_stages
advanced_implementation_skills
graph_rag_alternative_selection
devops_hosting_alternative_selection
ai_tool_skill_library_distribution
chat_interface_for_system_interaction
```

Для каждого scenario class validation должна проверять не final answer, а именно
planning ability:

```text
detect task type
identify required wiki/project units
build query plan over project wiki
map Angular client responsibilities
map .NET server responsibilities
map SQL stored procedure/database responsibilities
map generated flows and orchestration phases
identify evidence needed before implementation
identify missing evidence and abstain safely
produce phase plan short enough for state save/recovery
produce positive and negative expected examples
```

Нужен отдельный артефакт:

```text
43.LearningLocalWikiData\validation\ORCHESTRATOR-PROMPT-PLANNING-SUITE-v1.json
```

Gate:

```text
all scenario classes from orchestrator prompts.txt are represented
each scenario class has at least 3 prompt variations
each prompt variation is adapted to .NET + Angular + SQL stored procedures
each expected plan includes Angular, .NET and SQL responsibilities when relevant
each expected plan includes wiki query planning steps
each expected plan includes state save/recovery boundaries
missing evidence is explicit instead of guessed
scenario_planning_accuracy >= 0.85
required_phase_recall >= 0.90
wrong_stack_or_single_repo_plan_rate == 0
```

## 4. Пошаговый план

### Preflight A. Plan self-validation before execution

Before any implementation action, executor must run this plan against
`plan-review-SKILL.md`, `plan-self-validation-SKILL.md`, and
`goal-delivery-completeness-SKILL.md`.

This validation is not a user-facing stop. If it finds an actionable plan gap,
executor must fix the plan and rerun this self-validation section.

Gate 0a — goal delivery:

```text
Goal:
Teach XIIGen to create, train, export, import, validate, upgrade, and use all
trainable units needed for project-local wiki over a real linked
Angular + .NET + SQL stored procedure system, with Visual Studio and VS Code
plugin readiness and real-project prompt validation.
```

Required goal mapping:

| Goal element | Required plan section |
|---|---|
| Trainable wiki units live in project | Section 1, section 1.1, section 2.1 |
| Export/import/locator are mandatory | Section 1.2, section 2.3, Gap 3, Gap 4, step 2 |
| Per-unit upgrade is possible | Section 1.2, full completion gate |
| Angular + .NET + SQL are one system | Section 1, Gap 5, Gap 6, Gap 8, steps 3, 6, 8, 13 |
| Advanced wiki questions are validated | Gap 7, step 4, step 10, step 11, step 13 |
| Orchestrator prompt scenarios are covered | Gap 9, step 5, step 13 |
| Visual Studio and VS Code plugin readiness is checked | Step 14, full completion gate |
| Executor cannot stop early | Section 4.0.0, section 4.0.A, section 4.0.2, section 7.2, section 8 |
| Documentation follows code | Section 4.0.1, section 7.1 |
| State file controls continuation | Section 4.0.0 |

Gate 0b — path verification:

Every path named in this plan must be one of:

```text
existing local path
required output path to be created by a numbered step
archive path that must be inspected by listing entries first
```

If a path is missing and it is a required output path, executor must create it
in the numbered step that owns it. Missing required output path is not a stop.

Gate 0c — count verification:

The plan currently uses these binding counts:

```text
14 advanced question classes
7 validation buckets
70 real-project advanced validation questions
14 numbered implementation steps
```

Any edit that changes one count must update every section that depends on that
count.

Gate 0d — pause-laziness check:

The plan must not contain a rule that lets executor pause or claim completion
because:

```text
role changed
report was written
state was missing
next action was not derived
documentation was stale
commit/push failed
tests were not allowed
timeout was reached
```

Each of these must have a remediation path in this plan.

Gate 0e — code/DNA pre-check:

This plan must not propose static semantic decision logic. Any semantic decision
must be a trainable, exportable, importable unit with manifest, checkpoint,
registry entry, locator key, upgrade policy, and rollback reference.

Phase 0 verdict:

```text
PASS only if Gate 0a, 0b, 0c, 0d and 0e pass.
If any gate fails, fix the plan and repeat Phase 0.
Do not enter implementation from a plan that has not passed this section.
```

### 4.0.0. Mandatory execution state

Executor must maintain a live state file for this plan:

```text
C:\Projects\llm_mvp_core\43.LearningLocalWikiData\STATE-PROJECT-WIKI-CROSS-STACK-ADVANCED-GAP-CLOSURE.json
```

The live state must include execution score:

```text
execution_score.current_score
execution_score.progress_step_count
execution_score.premature_stop_count
execution_score.severe_violation_count
execution_score.last_score_event
execution_score.score_if_continue
execution_score.score_if_stop_now
execution_score.score_if_severe_violation
```

Any state file missing this score is incomplete and must be fixed before the
next response or role transition.

This file is not a report for Luba. It is executor memory and continuation
control. It must be updated continuously so the next action can always be
recovered after a long run, crash, restart, context compaction, or role-loop
iteration.

Main focus goal stored in the state file:

```text
Teach XIIGen to create, train, export, import, validate, upgrade, and use all
trainable units needed for project-local wiki over a real linked
Angular + .NET + SQL stored procedure system, with Visual Studio and VS Code
plugin readiness and real-project prompt validation.
```

Until this focus goal is reached, executor must keep deriving the next action
from this plan. "I did not derive a next action" is not a completion state and
not pause permission.

If the executor cannot derive the next action, it must update this state file
with the exact missing derivation field and continue resolving that missing
field from this plan or section 7 Document RAG.

Required `STATE.json` fields:

```text
plan_id
main_focus_goal
goal_reached
current_numbered_step
current_internal_role_state
loaded_skills
last_plan_reminder
allowed_verification
produced_artifacts
updated_documentation
gate_status
actionable_gaps
next_action
next_action_plan_source
expected_artifact_or_gate
commit_status
push_status
real_blocker
unresolved_derivation_field
continue_mode
operator_input_required_reason
last_updated_utc
```

`continue_mode` may be `gate_passed_with_evidence` only when:

```text
goal_reached == true
```

or:

```text
operator_input_required == true
real_blocker != null
next_action == null
unresolved_derivation_field != null
```

`next_action == null` is valid only when the executor has proven which required
derivation field cannot be resolved from the plan, local files, registry, or
Document RAG. A blank `next_action` without `unresolved_derivation_field` is a
protocol violation.

Every update must preserve the main focus goal exactly. If executor changes the
goal text, that is goal drift.

State update moments:

```text
before first implementation action
after every numbered plan step
after every internal review/check
after documentation sync
after every commit/push attempt
after every generated report artifact
before any final response
before any blocker/pause report
after restart or context compaction before continuing
```

If `STATE.json` is missing, executor must create it from this section before
continuing. Missing state is not a pause gate when this schema is available.

### 4.0. Mandatory goal check before and after every step

Every step in this plan must be checked against the same main goal:

```text
XIIGen must learn to create the full project-local wiki capability for a real
multi-codebase system: Angular client + .NET server + SQL stored procedures.
```

Before starting any step, executor must answer:

```text
Does this step directly move the system toward that goal?
What concrete missing capability does this step close?
Which trainable unit, registry field, validation artifact, or import/export path
does it improve?
Which exact numbered step in this plan authorizes the next action?
```

After finishing any step, executor must answer:

```text
What changed?
Which gate or prerequisite is now closer to passing?
What remains missing?
What is the next actionable step?
Which next numbered step in this plan must be executed now?
```

Executor must derive the next actionable step from this plan. If no hard
blocker exists, execution must continue. Status reporting is not a stop
condition.

Next action source priority:

```text
1. current numbered step in this plan
2. next numbered step in this plan
3. Document RAG source listed in section 7
4. local code contract in llm_mvp_core
5. BLOCKED_WITH_GAPS only if the answer cannot be found safely
```

Executor must not invent a side quest when the next numbered step is known.

### 4.0.A. Plan-derived next action protocol

The next action is considered known only when executor can fill all fields
below from this plan or from a document named by this plan:

```text
next_action
plan_source_section
numbered_step
role_state
required_skills
expected_artifact_or_gate
goal_alignment
```

Allowed `role_state` values:

```text
ARCHITECT_INTERNAL
EXECUTOR_INTERNAL
REVIEW_INTERNAL
```

These are internal states in the continuous loop. They are not user-facing stop
points.

Derivation order:

```text
1. If the current numbered step has unfinished actions, continue that step.
2. Else if the current step gate passed, move to the next numbered step.
3. Else if the current step gate failed with actionable gaps, fix the first gap
   named by that gate.
4. Else use Document RAG in section 7 to resolve the missing target.
5. Else record BLOCKED_WITH_GAPS only if no safe local source can resolve it.
```

The executor must write the derived next action to its run trace before acting:

```text
plan_reminder:
  main_goal: teach XIIGen to create, train, export, import, validate and upgrade all units for project-local wiki over Angular + .NET + SQL stored procedures
  current_step: <number and title>
  next_action: <specific action>
  required_role_state: <ARCHITECT_INTERNAL | EXECUTOR_INTERNAL | REVIEW_INTERNAL>
  required_skills: [..]
  expected_artifact_or_gate: <file/report/gate>
  why_this_moves_goal_forward: <one sentence>
```

If any field is blank, executor must not guess. It must resolve the blank field
from this plan or section 7 Document RAG and then continue.

A blank field is not a reason to report done or wait. It becomes:

```text
unresolved_derivation_field: <field-name>
next_action: resolve <field-name> from plan or Document RAG
stop_allowed: false
```

Only if the field cannot be resolved from the plan, local files, registry, or
Document RAG may executor record a real blocker.

### 4.0.B. Skill selection by internal role state

Skill loading must follow the action, not personal preference.

```text
ARCHITECT_INTERNAL:
  session-mode-declaration
  goal-context-persistence
  plan-review
  plan-self-validation
  goal-delivery-completeness
  architect-behavior-classifier
  specificity-calibration
  design-artifact-completeness

EXECUTOR_INTERNAL before reading/editing code:
  code-examination
  change-propagation
  blast-radius-assessor
  implementation-mode-gate
  scope-isolation
  dev-safety

EXECUTOR_INTERNAL for wiki/training assets:
  client-server-symmetry
  graph-rag-integration
  dpo-training-data
  dpo-triples-to-experiment-pairs
  learning-signal-capture
  learning-loop-closure

REVIEW_INTERNAL:
  generated-code-review
  self-verification
  verification-before-completion
  phase-review
  output-contract
  dna-compliance-guard
  human-language-gate

DOCUMENTATION_SYNC inside EXECUTOR_INTERNAL:
  documentation-sync
  skill-graph-sync
  document-hierarchy
```

The executor must not start the next internal state until the required skills
for that state are loaded or confirmed present in `all-skills.zip`.

### 4.0.C. Goal focus reminder

Before every internal role-loop action, executor must ask:

```text
Does this action improve one of the required project-local wiki capabilities?
```

At least one answer must be yes:

```text
trainable unit quality
export/import path
registry/manifest completeness
upgrade/rollback safety
cross-stack system mapping
advanced query planning
answer/verifier evidence quality
runner metrics persistence
Visual Studio / VS Code plugin readiness
real-project prompt validation
documentation accuracy after code change
```

If all answers are no, the action is goal drift and must be replaced by the next
plan-derived action.

Hard blockers are only:

```text
user explicitly asks to pause or switch role
required source file is missing and no local archive, registry, or document RAG source can restore it
required project path is inaccessible and no alternate local evidence source exists
current branch conflicts with the requested target and continuing would change the wrong branch
plan contradiction prevents safe non-destructive action
operation would change branch, delete files, publish artifacts, or touch unrelated user changes without permission
secret, credential, or private external access is required and unavailable through an approved non-interactive path
product decision is required and Luba has not made it
```

The following are not blockers:

```text
STATE.json missing when schema is available in this plan
rg unavailable or access denied
Node/browser/tool runtime mismatch when fallback exists
elapsed time
long session
context compaction
large file count
need to continue tomorrow
intermediate report exists
partial metrics exist
one unit improved while others are still weak
branch is unexpected but no files are being changed yet
dirty unrelated files exist
commit or push failed while another safe next step exists
documentation drift with known target
runner report artifact is missing but its schema and owner are known
CPU/RAM/accuracy are missing but can be instrumented in runner/report path
wrong report format can be regenerated from persisted artifacts
tests are not allowed for the current phase
full test suite would require arbitrary timeout
generated artifact is missing but plan gives its schema
review found actionable gaps
```

No arbitrary execution timeout is allowed in this plan. A value like 120 seconds
is not a project rule. If a command or training run needs longer, executor must
use a longer timeout, a resumable runner, or a persisted state file instead of
turning elapsed time into a stop or false completion claim.

Blocker remediation table:

| Encountered issue | Required remediation |
|---|---|
| `rg` is blocked or unavailable | Use PowerShell/native search and continue. |
| `STATE.json` missing | Create it from section 4.0.0 and continue. |
| Runtime tool mismatch | Use bundled workspace runtime or approved fallback. |
| Test command is silent | Do not invent counts; use explicit logger/listing or record unsupported metric. |
| Full test suite exceeds timeout | Use longer timeout or targeted allowed verification; never treat 120 seconds as project policy. |
| Broad tests are not allowed | Do not run them; use the plan-approved verification. |
| Runner report missing | Add/fix persisted runner report artifact before reporting metrics. |
| CPU/RAM/accuracy missing | Instrument runner/report path and rerun the allowed report command. |
| Wrong report format | Regenerate from persisted artifacts; do not hand-write nice numbers. |
| Commit/push not completed | Commit/push active-task files when gate allows; if impossible, record reason and continue any safe next step. |
| Documentation drift after code change | Update the matching documentation in the same implementation slice and continue. |
| Dirty unrelated files exist | Do not stage them; continue with active-task files only. |
| Extracted package is not a git checkout | Treat it as evidence; operate on the real target repository. |
| Missing generated artifact with known schema | Create it according to the plan. |
| Review finds actionable gaps | Treat review output as next executor input and fix the next gap. |
| Branch is not expected | Do not switch automatically; if no file changes are needed, continue analysis; if execution would modify the wrong branch, record branch blocker. |

### 4.0.1. Commit and push protocol

Commit and push are required delivery actions for executor implementation
phases, but they are not stop gates.

Meaning:

```text
If executor changes project files and the relevant gate passes, executor must
commit and push those changes unless Luba explicitly says not to.

If executor cannot commit or push, it must record the reason and continue with
the next safe actionable step when one exists.

Commit/push failure is not permission to claim done, pause, or abandon the goal.
```

Commit/push must happen only for files that belong to the active task. Executor
must not stage unrelated dirty files.

Documentation update protocol:

```text
If code changes behavior, public contracts, trainable-unit lifecycle,
runner/report format, manifest/import/export behavior, registry schema,
validation gates, or plugin-facing readiness, executor must update the matching
project documentation in the same implementation slice.
```

Documentation update is not a stop gate:

```text
do not stop to ask whether to update stale docs
do not report "code done, docs pending" as a successful phase
do not use documentation drift as a reason to pause when the needed update is known
update the docs and continue the next plan-authorized step
```

If the required documentation target is unclear, executor must use section 7
Document RAG. If the target still cannot be found, record:

```text
status: IN_PROGRESS_NEXT_STEP_KNOWN
gap: documentation target unresolved
next_action: resolve documentation target from project docs map
```

This is not `BLOCKED_WITH_GAPS` unless no local RAG source can identify the
documentation target safely.

Allowed commit/push timing:

```text
after a coherent implementation slice
after registry/schema/report artifacts for that slice are written
after the relevant non-destructive verification for that slice is recorded
before moving to a logically separate implementation slice, when possible
```

Forbidden commit/push usage:

```text
do not commit architecture review as completed implementation
do not commit unrelated dirty files
do not push from the wrong branch
do not use commit/push as a reason to stop if the next step is known
do not skip commit/push silently after project changes
```

Required report fields after executor implementation changes:

```text
changed_files
verification_performed
commit_status
push_status
uncommitted_related_changes
uncommitted_unrelated_changes
next_action
```

### 4.0.2. Premature stop violation

Stopping before the active goal is fully handled is a protocol violation unless
one of the hard blockers in section 4.0 exists.

Invalid stop reasons:

```text
I wrote a report
I found a gap but the next fix is known
I reached a round number of batches
I reached a time limit
I need approval for an already specified next step
I need to ask whether to run the next step that the plan already requires
I committed or pushed one slice
```

Required behavior after any intermediate report:

```text
read the current numbered step
check the main goal
identify the next actionable step
continue if no hard blocker exists
```

If executor stops early, the run status must be:

```text
PREMATURE_STOP_PROTOCOL_VIOLATION
```

That status is never acceptable as a successful phase result.

### Шаг 1. Зафиксировать branch и governance safety

Цель: не продолжать работу на случайной ветке и не использовать нечитаемый план.

Действия:

1. Записать текущую ветку и dirty files в отдельный report.
2. Проверить, что текущая ветка `main`.
3. Если текущая ветка не `main`, записать `BLOCKED_WITH_GAPS: wrong branch`
   и не менять ветку автоматически.
4. Создать или исправить читаемую UTF-8 версию `PLAN-PROJECT-WIKI-TRAINING-ASSET-REGISTRY-v1.md`.
5. Добавить проверку mojibake для governance docs.

Ожидаемый результат:

```text
branch recorded
branch is main or BLOCKED_WITH_GAPS is recorded
dirty files recorded
governance docs readable
no automatic checkout
```

### Шаг 2. Довести registry до import/export source of truth

Цель: каждый accepted publishable unit должен быть реально импортируемым и экспортируемым.

Действия:

1. Добавить `manifest_ref` в registry schema.
2. Заполнить `manifest_ref` для accepted entries.
3. Добавить `locator_key` и `remote_unit_id`.
4. Разделить статусы:

```text
manifest_present
manifest_derivable_but_not_registry_linked
checkpoint_reference_only
not_publishable_baseline_only
```

5. Сформировать `PROJECT-WIKI-IMPORT-EXPORT-GATE.json`.

Ожидаемый результат:

```text
accepted publishable unit can be resolved from registry to manifest to checkpoint to locator
```

### Шаг 3. Добавить cross-stack system map schema

Цель: validation projects должны стать проверяемой системой, а не списком папок.

Действия:

1. Создать schema для:

```text
43.LearningLocalWikiData\validation\CROSS-STACK-SYSTEM-MAP.json
```

2. Для каждой предполагаемой связи client/server/database хранить:
   - client project;
   - server project;
   - database scope;
   - match status;
   - confidence;
   - evidence by API routes;
   - evidence by DTO names;
   - evidence by environment URLs;
   - evidence by HTTP client calls;
   - evidence by controller methods;
   - evidence by repository methods;
   - evidence by stored procedures;
   - unknown reason.
3. Запретить advanced validation на project pair со статусом
   `unknown_not_enough_evidence`.

Ожидаемый результат:

```text
CROSS-STACK-SYSTEM-MAP.json schema exists
matched pairs require at least two independent evidence types
unknown project links are recorded as unknown, not guessed
```

### Шаг 4. Добавить advanced validation suite schema

Цель: до выбора следующего weak unit нужно знать, какие реальные вопросы не
покрыты.

Действия:

1. Создать файл:

```text
43.LearningLocalWikiData\validation\ADVANCED-WIKI-QUESTION-SUITE-v1.json
```

2. Зафиксировать обязательное распределение минимум 70 вопросов:

```text
10 field_addition_impact
10 dto_or_api_contract_change
10 angular_component_service_state_flow
10 dotnet_controller_service_repository_flow
10 stored_procedure_parameter_or_result_flow
10 calculation_and_validation_path_change
10 missing_evidence_wrong_project_or_stale_documentation
```

3. Для каждого вопроса указать:

```text
question_class
prompt_variation
required_projects
required_evidence_types
expected_query_steps
expected_answer_sections
required_units
pass_thresholds
```

Ожидаемый результат:

```text
advanced suite has at least 70 questions
each of the 7 required classes has at least 10 questions
all 14 underlying question classes are represented at least once
no bucket uses all 10 rows for only one subtype
each question maps to one or more required units
each question has explicit evidence expectations
```

The seven buckets are not enough for completion. The suite must also expose an
`underlying_question_class` field, or an equivalent field with this exact
meaning, and the gate must compare the actual set to this required literal set:

```text
field_addition_impact
field_rename_impact
dto_contract_change
api_response_shape_change
client_state_effect
cross_repo_workflow_trace
authorization_rule_change
stored_procedure_parameter_change
stored_procedure_result_shape_change
calculation_path_change
validation_rule_change
missing_evidence_abstention
wrong_project_rejection
stale_documentation_rejection
```

`ADVANCED-WIKI-QUESTION-SUITE-v1.json passes` means all three checks pass:

```text
total_questions >= 70
all 7 top-level buckets have >= 10 rows
actual underlying_question_class set == required 14-class set above
```

If the suite has 70 rows but only 7 bucket names and no proof of the 14
underlying classes, the status is `IN_PROGRESS_NEXT_STEP_KNOWN`, not passed.

### Шаг 5. Добавить orchestrator prompt planning suite schema

Цель: проверить, что система умеет спланировать любую типичную задачу из
`orchestrator prompts.txt`, если проект реализован как .NET + Angular + SQL
stored procedures.

Действия:

1. Создать файл:

```text
43.LearningLocalWikiData\validation\ORCHESTRATOR-PROMPT-PLANNING-SUITE-v1.json
```

2. Включить все scenario classes из Gap 9.
3. Для каждого scenario class добавить минимум 3 вариации:
   - direct user request;
   - incomplete request with missing evidence;
   - wrong/simple rejected plan contrast.
4. Для каждой вариации указать:

```text
scenario_class
prompt_variation
required_stack_assumption
required_units
expected_project_wiki_queries
expected_angular_responsibilities
expected_dotnet_responsibilities
expected_sql_or_stored_procedure_responsibilities
expected_orchestration_phases
expected_state_save_boundaries
expected_missing_evidence_checks
positive_expected_plan_shape
negative_rejected_plan_shape
pass_thresholds
```

Ожидаемый результат:

```text
ORCHESTRATOR-PROMPT-PLANNING-SUITE-v1.json exists
all scenario classes from orchestrator prompts.txt are represented
each scenario class has at least 3 variations
each variation is adapted to .NET + Angular + SQL stored procedures
scenario_planning_accuracy >= 0.85
required_phase_recall >= 0.90
wrong_stack_or_single_repo_plan_rate == 0
```

### Шаг 6. Добавить multi-language stack model

Цель: система видит TypeScript/Angular, C#/.NET и T-SQL как части одной системы.

Действия:

1. Расширить `StackAndVersionClassifierUnit`.
2. Добавить поля:

```text
client_language
client_framework
client_framework_version
server_language
server_framework
server_framework_version
database_language
database_dialect
```

3. Описать 50-row batch contract для multi-stack classification:
   - row shape;
   - chosen shape;
   - rejected shape;
   - required evidence;
   - validation metrics.

Actual batch creation is forbidden in this architecture phase.

Ожидаемый результат:

```text
typescript_angular_version_detection_accuracy >= 0.85
dotnet_version_detection_accuracy >= 0.85
stored_procedure_dialect_detection_accuracy >= 0.85
```

### Шаг 7. Добавить `SystemBoundaryMapperUnit`

Цель: научить систему находить границы между Angular client, .NET server и SQL
stored procedures.

Действия:

1. Создать или запланировать canonical trainable unit:

```text
SystemBoundaryMapperUnit
```

2. Unit должен иметь:
   - DPO generator;
   - training runner;
   - manifest publisher;
   - registry entry schema;
   - locator key;
   - runner report;
   - import/export gate;
   - selectable weak-unit signal для `WikiTraceAggregator`.
3. Описать 50-row batch contract, где chosen показывает доказанную связь между
   client, server и database, а rejected правдоподобно, но ошибочно связывает:
   - только Angular;
   - только .NET;
   - wrong server project;
   - missing stored procedure evidence;
   - guessed pair without evidence.

Actual batch creation is forbidden in this architecture phase.

Ожидаемый результат:

```text
SystemBoundaryMapperUnit is a trainable project unit
system_boundary_mapping_accuracy >= 0.85
client_server_pairing_confidence >= 0.85
unknown_pair_abstention_accuracy >= 0.95
accepted SystemBoundaryMapperUnit entries have manifest_ref, locator_key and remote_unit_id
```

### Шаг 8. Добавить cross-repository system graph

Цель: один graph должен покрывать Angular client, .NET server и SQL stored procedures.

Действия:

1. Ввести `CrossStackBusinessFlowGraph`.
2. Ввести cross-stack edge types.
3. Использовать результат `SystemBoundaryMapperUnit` как вход, а не угадывать
   пары заново.
4. Создать validation report по каждому matched system pair.

Ожидаемый результат:

```text
client_server_edge_recall >= 0.85
repository_to_stored_procedure_edge_recall >= 0.85
client_to_stored_procedure_path_recall >= 0.85
```

### Шаг 9. Расширить query planning под серию wiki queries

Цель: вопрос пользователя превращается не в один поиск, а в план из нескольких wiki queries.

Действия:

1. `ProjectWikiQueryPlanningFlow` должен для advanced question строить sequence:

```text
identify entity/field
find Angular component/input/form
find Angular service/http call
find API endpoint
find server controller/service
find repository/database call
find stored procedure/sql tables/result
find response DTO
find Angular state/store/UI consumers
check tests/validation/auth
check missing evidence
```

2. Описать DPO row contract, где rejected:
   - ищет только Angular
   - ищет только .NET
   - пропускает stored procedure
   - пропускает response path
   - отвечает до missing evidence check

Actual row creation is forbidden in this architecture phase.

Ожидаемый результат:

```text
cross_stack_query_plan_accuracy >= 0.85
required_query_step_recall >= 0.90
missing_evidence_check_rate == 1.00
```

### Шаг 10. Расширить answer composer под advanced questions

Цель: ответ должен объяснять developer impact, а не просто “где метод”.

Действия:

1. Добавить answer sections:

```text
requested_change
affected_client_files
affected_api_contract
affected_server_flow
affected_database_or_stored_procedure
affected_calculations
affected_validation_or_authorization
affected_tests
missing_evidence
safe_next_steps
```

2. Описать 50-row DPO batch contract для advanced answers.
3. Rejected должен быть “хорошо плохой”: правдоподобный, но с конкретной дырой.

Actual batch creation is forbidden in this architecture phase.

Ожидаемый результат:

```text
advanced_answer_shape_accuracy >= 0.90
exact_evidence_accuracy >= 0.90
hallucination_rate == 0
```

### Шаг 11. Расширить verifier под cross-stack answers

Цель: verifier должен ловить неполный ответ даже если он звучит уверенно.

Действия:

1. Добавить проверки:
   - ответ не вышел за проектную границу
   - есть client evidence
   - есть server evidence
   - есть database evidence when required
   - есть missing evidence section
   - нет invented route/procedure/method
2. Описать DPO row contract для verifier.

Actual row creation is forbidden in this architecture phase.

Ожидаемый результат:

```text
cross_stack_verifier_accuracy >= 0.90
missing_evidence_detection >= 0.95
wrong_project_rejection >= 0.95
```

### Шаг 12. Исправить next-training-action selection

Цель: training loop должен сам назвать следующий слабый unit после того, как
появились registry signals, import/export signals, system map и advanced suite.

Действия:

1. Добавить цель `cross_stack_advanced_wiki`.
2. Добавить gap signals из:
   - registry;
   - import/export gate;
   - `CROSS-STACK-SYSTEM-MAP.json`;
   - `ORCHESTRATOR-PROMPT-PLANNING-SUITE-v1.json`;
   - advanced prompt coverage;
   - real project validation;
   - failed/rejected batch history.
3. `WikiTraceAggregator` должен выбрать unit по приоритету:

```text
import/export blocker
SystemBoundaryMapperUnit gap
StackAndVersionClassifierUnit multi-language gap
CodeTraceBuilderUnit cross-stack trace gap
ProjectWikiQueryPlanningFlow advanced query sequence gap
ProjectWikiAnswerComposer advanced answer gap
ProjectAnswerVerifier cross-stack verifier gap
WikiContextRoutingUnit retrieval mode gap
orchestrator prompt planning scenario gap
```

Ожидаемый результат:

```text
PROJECT-WIKI-NEXT-TRAINING-ACTION.json target_unit_name != NONE
target_unit_name is one of the canonical trainable units
recommended_batch_strategy starts with build_50_
evidence_batch_ids or validation_gap_ids are non-empty
new_training_batch_allowed is true or explicit blocker explains why not
```

### Шаг 13. Проверить end-to-end только после readiness gates

Цель: не запускать бессмысленную проверку до готовности units.

Readiness gates:

```text
registry import/export gate passed
CROSS-STACK-SYSTEM-MAP.json exists and has matched pairs with evidence
ADVANCED-WIKI-QUESTION-SUITE-v1.json exists and has the required 70-question distribution
ADVANCED-WIKI-QUESTION-SUITE-GATE.json proves all 14 underlying question classes
ORCHESTRATOR-PROMPT-PLANNING-SUITE-v1.json exists and covers every scenario class
ORCHESTRATOR-PROMPT-PLANNING-SUITE-GATE.json proves every scenario class from orchestrator prompts.txt
next-training-action target is not NONE
multi-stack classifier accepted
SystemBoundaryMapperUnit accepted
cross-stack graph builder accepted
query planning accepted for cross-stack advanced prompts
scenario planning accepted for orchestrator prompts
answer composer accepted for advanced prompts
verifier accepted for cross-stack answers
```

If an explicit blocker exists, readiness is not passed. The correct status is
`BLOCKED_WITH_GAPS`, and end-to-end validation is forbidden.

Только после этого запускать validation на проектах.

Required real-project validation targets:

```text
C:\Projects\bizguide-client
C:\Projects\GenieDynamicApp
C:\Projects\geniesoftGameMVP
C:\Projects\wizGameserver
C:\Projects\GenieDynamicServer
C:\Projects\geniesoftGameMVPServer
C:\Projects\genietasks
C:\Projects\askapediaweb
```

These projects must be validated as linked systems, not as isolated folders:

```text
Angular client project
  + .NET server project
  + SQL stored procedure evidence when present
  = one business system under validation
```

The validation runner must execute the prompt classes from:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\orchestrator prompts.txt
```

For each prompt class, the runner must check:

```text
project wiki request is detected
stack/version classifier detects Angular + .NET + SQL when present
system boundary mapper links client/server/database as one system
query planning creates multiple wiki queries, not one flat search
answer composer cites evidence from the correct project roots
verifier rejects missing client/server/database evidence
missing evidence is reported instead of invented
```

Required real-project report:

```text
43.LearningLocalWikiData\reports\PROJECT-WIKI-REAL-PROJECT-PROMPT-VALIDATION.json
```

Minimum fields:

```text
project_group_id
project_roots
prompt_class
prompt_variant_id
detected_stacks
detected_versions
query_plan_steps
client_evidence_count
server_evidence_count
database_evidence_count
missing_evidence_items
answer_verdict
exact_evidence_accuracy
query_planning_accuracy
cross_stack_path_accuracy
cpu_ms
ram_mb
elapsed_ms
```

Ожидаемый результат:

```text
advanced_wiki_validation_passed: true/false
exact_evidence_accuracy
query_planning_accuracy
answer_shape_accuracy
missing_evidence_abstention_accuracy
real_project_prompt_validation_accuracy
cross_stack_path_accuracy
cpu_ms
ram_mb
elapsed_ms
```

### Шаг 14. Проверить build/readiness для Visual Studio и VS Code plugins

Цель: доказать, что результат плана готов не только как internal XIIGen model,
но и как библиотека/contract для реальных plugin hosts.

Plugin plans to use as product-side contract:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\extensions\PLAN-XIIGEN-SMART-CODE-WIKI-PHASE-1-v1.md
C:\Users\lubao\Documents\xiigen\llm mvp\extensions\PLAN-SMART-CODE-WIKI-PHASE1-ALL-18-PLATFORMS-v1.md
C:\Users\lubao\Documents\xiigen\llm mvp\extensions\plugin-plans-vs-vscode-v1.zip
```

Required plugin build/readiness checks:

```text
Visual Studio plugin plan can consume shared Smart Code Wiki contract
VS Code plugin plan can consume shared Smart Code Wiki contract
both plugins can request project-local wiki capability without embedding generic training logic
both plugins receive readiness status from XIIGen, not from local guessing
both plugins can show blocked/missing-evidence state without claiming ready
both plugins can reference project-local manifests without storing generic model weights
```

Required shared contract surface:

```text
WikiRequest
WikiAnswer
WikiReadinessStatus
WikiRelevanceMap
WikiContextSlice
ProjectSystemMap
ProjectWikiUnitManifest
ProjectWikiImportExportStatus
```

Required plugin readiness report:

```text
43.LearningLocalWikiData\reports\PROJECT-WIKI-PLUGIN-READINESS-GATE.json
```

Minimum fields:

```text
visual_studio_plan_checked
vs_code_plan_checked
shared_contract_complete
plugin_build_or_static_contract_check
manifest_reference_flow_checked
model_weights_not_embedded_in_plugin
real_project_prompt_validation_linked
blocked_state_supported
ready_state_supported
cpu_ms
ram_mb
elapsed_ms
```

This check does not authorize broad plugin implementation work inside this
architecture phase. It defines the gate that later executor work must satisfy
before reporting plugin readiness.

## 5. Что нельзя считать готовым

Нельзя говорить “готово”, если выполнено только одно из этих условий:

- Angular binary contrast accuracy выше 0.85.
- Registry существует.
- Есть generated manifests.
- Есть unit tests.
- Есть `.xiigen/wiki-domain` files.
- Один проект проходит basic question seed validation.
- Plugin plan mentions Visual Studio or VS Code, but build/readiness gate was not checked.
- Real projects were listed, but prompt classes from `orchestrator prompts.txt` were not executed against them.

Готовность требует:

```text
all publishable units import/export through registry and locator
all canonical decision-making units are trainable, exportable and importable
no canonical unit is replaced by static heuristic or rule helper
each canonical unit has upgrade_policy and rollback_ref
minor unit upgrade can be performed without retraining unrelated units
Angular + .NET + stored procedure seen as one system
advanced developer questions pass with evidence
real project prompt validation passes on linked Angular + .NET + SQL systems
Visual Studio and VS Code plugin readiness gates pass
next weak unit is selected by training loop, not by human guessing
metrics are persisted from runner reports
```

Особенно важно: accepted batch без `manifest_ref`, `locator_key` и
`remote_unit_id` не считается project asset. Он может быть хорошим training
result, но он ещё не является встроенной обучаемой частью проекта.

## 5.1. Полная готовность всей задачи

Foundation readiness не равна завершению всей задачи.

Задача полностью выполнена только если пройден полный путь:

```text
user prompt
  -> ProjectWikiRequestRouterUnit
  -> StackAndVersionClassifierUnit
  -> SystemBoundaryMapperUnit
  -> CodeTraceBuilderUnit / CrossStackBusinessFlowGraph
  -> ProjectWikiQueryPlanningFlow
  -> WikiContextRoutingUnit
  -> ProjectWikiAnswerComposer
  -> ProjectAnswerVerifier
  -> WikiTraceAggregator
  -> registry update
  -> manifest/checkpoint publish
  -> IRemoteUnitLocator locate
  -> Visual Studio / VS Code shared contract readiness
  -> Visual Studio / VS Code plugin build/readiness gate
  -> validation on Angular + .NET + SQL stored procedure system
  -> real project prompt validation for orchestrator prompt classes
  -> all orchestrator prompts planning scenarios pass
```

Full completion gate:

```text
all publishable units have manifest_ref, locator_key and remote_unit_id
all accepted checkpoints resolve through IModelRepository
all units are locatable through IRemoteUnitLocator
runtime canonical unit set exactly equals the canonical trainable units list in section 1.1
all canonical trainable units have upgrade_policy
all upgraded units have compatibility_report_ref and rollback_ref
no semantic decision is implemented only as static helper
CROSS-STACK-SYSTEM-MAP.json has matched system pairs with evidence
ADVANCED-WIKI-QUESTION-SUITE-v1.json passes the 70-row, 7-bucket and 14-underlying-class checks
ORCHESTRATOR-PROMPT-PLANNING-SUITE-v1.json passes every scenario class from orchestrator prompts.txt
end-to-end validation passes from prompt intake to answer verification
PROJECT-WIKI-REAL-PROJECT-PROMPT-VALIDATION.json passes
PROJECT-WIKI-PLUGIN-READINESS-GATE.json passes
Visual Studio library readiness passes
VS Code library readiness passes
PLUGIN-PROJECT-DOC-RAG-SCENARIO-MATRIX-v1.json has one documentation/RAG/plugin-scenario/fallback row per validation project group
PROJECT-WIKI-COMPLETION-COVERAGE-MATRIX.json passes
no false ready/done claim appears in reports
```

Completion anti-false-positive matrix:

Before `TASK_FULLY_COMPLETE`, executor must create:

```text
43.LearningLocalWikiData\reports\PROJECT-WIKI-COMPLETION-COVERAGE-MATRIX.json
```

The matrix must contain one row per literal requirement in this plan:

```text
requirement_id
literal_requirement
source_section
expected_literal_set
actual_artifact
actual_value
comparison_operator
passed
gap
next_plan_action
```

The matrix must fail if any lower-level report says `passed: true` but does not
cover the literal requirement. The parent completion claim is allowed only when
the matrix itself passes and has zero gaps.

Required exact-set rows:

```text
canonical_trainable_units:
  expected = section 1.1 canonical trainable units
  actual = LIVE-END-TO-END-RUNTIME-GATE.json canonical_units_required/loaded
  operator = set equality, not subset

advanced_question_underlying_classes:
  expected = 14-class list from step 4
  actual = ADVANCED-WIKI-QUESTION-SUITE-v1.json underlying_question_class values
  operator = set equality, not bucket count

orchestrator_prompt_scenario_classes:
  expected = every scenario class derived from orchestrator prompts.txt by this plan
  actual = ORCHESTRATOR-PROMPT-PLANNING-SUITE-v1.json scenario_class values
  operator = set equality

validation_projects:
  expected = all validation projects listed in section 1
  actual = PROJECT-WIKI-REAL-PROJECT-PROMPT-VALIDATION.json project coverage
  operator = set equality or explicit unavailable-project gap

final_readiness_source_of_truth:
  expected = advanced suite gate, orchestrator suite gate, real-project prompt validation,
             live runtime gate, upgrade rollback gate, plugin runtime gate,
             static semantic helper audit, import/export gate, state file, git boundary
  actual = PROJECT-WIKI-END-TO-END-READINESS-GATE.json source_of_truth
  operator = set includes all required sources
```

Forbidden matrix passes:

```text
7 buckets accepted as 14 classes
"at least one Angular + .NET + SQL prompt" accepted for all advanced classes
extra unit accepted as replacement for missing canonical unit
runtime gate omits ProjectWikiTrainingOrchestrator
final readiness source_of_truth omits advanced suite or orchestrator suite gate
state says goal_reached true while next_action still names unfinished plan work
```

Evidence firewall:

The full completion gate is invalid if any final report uses a proxy evidence
type for a runtime claim.

The following values are forbidden for `TASK_FULLY_COMPLETE`:

```text
validation_only: true
metric_type contains: static
metric_type contains: contract
metric_type contains: mixed
cpu_ms: UNKNOWN
ram_mb: UNKNOWN
accuracy: UNKNOWN
plugin_build_or_static_contract_check without runtime host/library integration
compatibility_report_ref without executed compatibility probe
rollback_ref without executed rollback/load probe
goal_reached: true based only on report existence
state or self-review still naming an old commit or old pending next action after commit/push
final readiness using a source-of-truth report that was overwritten by a narrower later run
```

Required runtime proof for final completion:

```text
LIVE-END-TO-END-RUNTIME-GATE.json exists
LIVE-END-TO-END-RUNTIME-GATE.json passed == true
LIVE-END-TO-END-RUNTIME-GATE.json validation_only == false
LIVE-END-TO-END-RUNTIME-GATE.json metric_type == runner_emitted_runtime_metric
all canonical units from section 1.1 were loaded through registry/locator
all required checkpoints were opened through IModelRepository
every required advanced prompt class for Angular + .NET + SQL stored procedures ran through:
  request router
  stack/version classifier
  system boundary mapper
  code trace / cross-stack graph
  query planning
  context routing
  answer composer
  answer verifier
  trace aggregator
cpu_ms is numeric
ram_mb is numeric
elapsed_ms is numeric
```

`CrossStackBusinessFlowGraphUnit` may be used as an implementation unit for the
cross-stack graph step, but it cannot replace any canonical unit in section 1.1.
If runtime evidence loads `CrossStackBusinessFlowGraphUnit` and omits
`ProjectWikiTrainingOrchestrator`, the canonical-unit coverage row fails.

Required upgrade proof:

```text
UPGRADE-ROLLBACK-RUNTIME-GATE.json exists
UPGRADE-ROLLBACK-RUNTIME-GATE.json passed == true
each upgraded unit has executed compatibility probe against dependent units
each upgraded unit has executed rollback/load probe
rollback_ref was loaded, not only written
compatibility_report_ref points to the executed probe report
```

Required plugin proof:

```text
PLUGIN-RUNTIME-INTEGRATION-GATE.json exists
PLUGIN-RUNTIME-INTEGRATION-GATE.json passed == true
Visual Studio host or library loads Smart Code Wiki contract from XIIGen
VS Code host or library loads Smart Code Wiki contract from XIIGen
plugins read readiness from XIIGen runtime report, not local guessing
```

Required plugin orchestration proof:

```text
PLAN-PLUGIN-ORCHESTRATION-PROMPT-SUITE-GATE-v1.md exists
PLUGIN-ORCHESTRATION-PROMPT-SUITE-GATE.json exists
PLUGIN-ORCHESTRATION-PROMPT-SUITE-GATE.json passed == true
Visual Studio plugin-facing entrypoint executes orchestrator prompt scenarios
VS Code plugin-facing entrypoint executes orchestrator prompt scenarios
all required repository scope classes are executed through plugin entrypoints
readiness-only responses are rejected for orchestration claims
```

If any required runtime proof is missing, the only allowed status is:

```text
IN_PROGRESS_NEXT_STEP_KNOWN
```

The next action must be the first missing runtime gate from the list above.

Only after this full completion gate may executor report:

```text
TASK_FULLY_COMPLETE
```

Before that, the only allowed statuses are:

```text
IN_PROGRESS_NEXT_STEP_KNOWN
BLOCKED_WITH_GAPS
ARCHITECTURE_READY_FOR_EXECUTOR
IMPLEMENTATION_READY_FOR_TRAINING
VALIDATION_READY
```

## 5.2. Gate B для принятия этого плана

План можно считать принятым только если в нём есть проверяемые ответы на эти
вопросы:

```text
SystemBoundaryMapperUnit is listed as canonical trainable unit
SystemBoundaryMapperUnit has DPO generator, runner, manifest publisher and registry contract
all canonical semantic units are trainable/exportable/importable
upgrade of a single unit is described with manifest, compatibility and rollback
CROSS-STACK-SYSTEM-MAP.json is required before cross-stack validation
ADVANCED-WIKI-QUESTION-SUITE-v1.json has exact 70-question distribution
ADVANCED-WIKI-QUESTION-SUITE-v1.json has exact 14 underlying question classes
ORCHESTRATOR-PROMPT-PLANNING-SUITE-v1.json covers every scenario class from orchestrator prompts.txt
real project prompt validation report is required
Visual Studio / VS Code plugin readiness report is required
completion coverage matrix is required before TASK_FULLY_COMPLETE
next-training-action selection happens after gap signal schemas exist
accepted publishable entry without manifest_ref/locator_key/remote_unit_id is not project asset
end-to-end validation is forbidden before readiness gates pass
```

Если хотя бы один пункт отсутствует, executor не должен переходить к training.

## 6. Следующий архитектурно правильный шаг

Не запускать новый training batch.

Сначала нужно закрыть foundation gaps:

1. Починить читаемость governance plan.
2. Добавить явные `manifest_ref`, `locator_key`, `remote_unit_id` в registry.
3. Добавить import/export gate.
4. Добавить `CROSS-STACK-SYSTEM-MAP.json` schema и matched/unknown/rejected статусы.
5. Добавить `ADVANCED-WIKI-QUESTION-SUITE-v1.json` schema и обязательное распределение 70 вопросов.
6. Добавить `ORCHESTRATOR-PROMPT-PLANNING-SUITE-v1.json` schema и покрытие всех scenario classes из `orchestrator prompts.txt`.
7. Добавить `SystemBoundaryMapperUnit` как полноценный обучаемый project unit.
8. Добавить real-project prompt validation gate для связанных Angular + .NET + SQL проектов.
9. Добавить Visual Studio / VS Code plugin build/readiness gate.
10. Только после появления этих signals исправить next-training-action, чтобы он выбирал weak unit для `cross_stack_advanced_wiki`.

Только после этого executor может запускать следующие 50-row batches.

## 7. Document RAG for executor questions

Если у executor возникает вопрос, он не должен угадывать и не должен сразу
останавливаться. Сначала он ищет ответ в этих источниках в указанном порядке.

### 7.0. How executor must search for answers

Executor must resolve questions through local sources before reporting a gap.

Search order:

```text
1. exact section in this plan
2. Document RAG table below
3. named local file from the table
4. local code contract named by the plan
5. registry/report artifact named by the plan
6. all-skills.zip only for skill behavior questions
7. BLOCKED_WITH_GAPS only after the unresolved field is named
```

Required search method:

```text
Use rg first when available.
If rg is unavailable or access denied, use PowerShell Get-ChildItem and Select-String.
For zip files, list archive entries first, then read only the named entry.
For code contracts, search interface/class name first, then caller references.
For reports/registry, read the JSON artifact directly, not a summary.
```

Before executor may record `BLOCKED_WITH_GAPS`, it must write this search trace
to `STATE.json`:

```text
question
unresolved_derivation_field
searched_sources
search_terms
files_read
answer_found: true/false
answer_source
next_action_after_answer
```

Before executor may stop for any reason while the full goal is not reached, it
must also write this premature-stop review to `STATE.json`:

```text
premature_stop_review:
  stopped: true
  stop_reason_attempted
  loophole_found
  governing_files_patched
  role_after_repair: EXECUTOR_INTERNAL
  next_action_after_repair
```

If `next_action_after_repair` is non-empty, executor must continue and must not
send a final response.

If `answer_found == true`, executor must continue with `next_action_after_answer`.
It must not stop to report that it found the answer.

If `answer_found == false`, executor must choose the smallest safe remediation:

```text
missing file -> search local archives listed in the plan
missing schema -> create schema from current plan section
missing report -> create/fix report artifact from runner output
missing skill -> restore/add skill from all-skills source
missing code contract -> inspect target project interfaces before editing
unclear product decision -> search the document RAG below, choose the safest
non-destructive architecture default, record the assumption in STATE.json, and
continue. Do not stop only because product wording is imperfect.
```

| Вопрос executor-а | Где искать ответ |
|---|---|
| Главная цель и границы текущего плана | This plan, section 1 and section 4.0 |
| Где хранится live execution state | This plan, section 4.0.0; `C:\Projects\llm_mvp_core\43.LearningLocalWikiData\STATE-PROJECT-WIKI-CROSS-STACK-ADVANCED-GAP-CLOSURE.json` |
| Когда можно остановиться до полной цели | This plan, section 4.0.0 and section 8 |
| Какой следующий шаг выполнять | This plan, current numbered step, then next numbered step |
| Можно ли менять ветку, запускать тесты, делать commit/push | This plan, section 4.0.1; `C:\Projects\llm_mvp_core\AGENTS.md`, `C:\Projects\llm_mvp_core\CLAUDE.md` |
| Является ли commit/push стоп-гейтом | This plan, section 4.0.1 |
| Нужно ли обновлять документацию после изменения кода | This plan, section 4.0.1; `C:\Projects\llm_mvp_core\AGENTS.md`; `C:\Projects\llm_mvp_core\CLAUDE.md` |
| Является ли documentation sync стоп-гейтом | This plan, section 4.0.1 |
| Что делать при преждевременной остановке | This plan, section 4.0.2 |
| What exact process runs before finishing while the plan is incomplete | This plan, opening section `Plan-incomplete finish recovery process`; `C:\Projects\llm_mvp_core\AGENTS.md` OP-23; `C:\Projects\llm_mvp_core\CLAUDE.md` Plan-incomplete finish recovery pattern |
| Как выполнять роли без скрытой остановки | This plan, section 7.2; `C:\Projects\llm_mvp_core\AGENTS.md`, `C:\Projects\llm_mvp_core\CLAUDE.md` |
| Как архитектор проверяет план, скилы, agent files, role loop, state, matrix, reports и stop loopholes | `C:\Projects\llm_mvp_core\43.LearningLocalWikiData\PROTOCOL-ARCHITECT-PLAN-SKILL-AGENT-ROLE-REVIEW-v1.md` |
| Какая ветка допустима для исполнения | This plan, section 1 and section 4 step 1 |
| Где должна жить обучаемая модель и её веса | This plan, section 1; `C:\Projects\llm_mvp_core\25.ModelServer\Contracts\IModelRepository.cs` |
| Может ли компонент быть статичным helper-ом | This plan, section 1.1 |
| Как делать upgrade отдельной части модели | This plan, section 1.2 |
| Что разрешено хранить в проверочных проектах | This plan, section 1 |
| Где живёт runtime logic wiki units | `C:\Projects\llm_mvp_core\42.ProjectWikiDomain\README.md` |
| Где живут learning assets, registry, reports | `C:\Projects\llm_mvp_core\43.LearningLocalWikiData\README.md` |
| Какие batches accepted/rejected/blocked | `C:\Projects\llm_mvp_core\43.LearningLocalWikiData\registry\PROJECT-WIKI-TRAINING-ASSET-REGISTRY.json` |
| Что тренировать дальше | `C:\Projects\llm_mvp_core\43.LearningLocalWikiData\reports\PROJECT-WIKI-NEXT-TRAINING-ACTION.json` |
| Где проверять manifest/checkpoint/import/export | `C:\Projects\llm_mvp_core\25.ModelServer\Contracts\IModelRepository.cs`, `C:\Projects\llm_mvp_core\25.ModelServer\Contracts\IRemoteUnitLocator.cs` |
| Где зарегистрирован flow adapter | `C:\Projects\llm_mvp_core\15.OrchestratorModel\Logical\LogicalOrchestratorAssistant.cs` |
| Как строить query planning для wiki | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\PROJECT-WIKI-QUERY-PLANNING-FLOW-PROTOCOL-v1.md` |
| Какие типичные задачи должна уметь планировать система | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\orchestrator prompts.txt` |
| Где проверять реальные prompt-сценарии на проектах | This plan, section 4 step 13; `C:\Projects\...\ .xiigen\wiki-domain` outputs for each validation project |
| Где проверять Visual Studio / VS Code plugin readiness | This plan, section 4 step 14; `C:\Users\lubao\Documents\xiigen\llm mvp\extensions\PLAN-XIIGEN-SMART-CODE-WIKI-PHASE-1-v1.md`; `C:\Users\lubao\Documents\xiigen\llm mvp\extensions\PLAN-SMART-CODE-WIKI-PHASE1-ALL-18-PLATFORMS-v1.md`; `C:\Users\lubao\Documents\xiigen\llm mvp\extensions\plugin-plans-vs-vscode-v1.zip` |
| Как строить DPO rejected | `C:\Users\lubao\Documents\xiigen\llm mvp\documentation learning\CODE-DOC-DPO-REJECTED-PROMPTS-v1.md` and DPO generator code in `42.ProjectWikiDomain` |
| Как отчитываться по metrics | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\ORCHESTRATION-TRAINING-PROTOCOL-RUNNER-REPORTING-v1.md` |
| Как не делать false positive done claim | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\EXECUTOR-DISCIPLINE-RULES-v1.md` and this plan, section 8 |
| Какие project skills использовать для review | `C:\Users\lubao\Documents\xiigen\llm mvp\all-skills.zip` |

### 7.0.1. Plan governance orchestration for every child plan

This plan governs child plans, plugin plans, training plans, validation plans,
and repair plans. Every child plan must carry the same plan-governance
orchestration before execution can continue.

Required child-plan governance block:

```text
main_focus_goal
architect_reference_protocol
branch_rule
allowed_verification
forbidden_verification
role_loop
skills_by_lifecycle_stage
document_rag_lookup_order
anti_loophole_register
state_update_points
completion_matrix_rows
score_rules
commit_push_boundary
repair_returns_to_executor
```

`architect_reference_protocol` must point to:

```text
C:\Projects\llm_mvp_core\43.LearningLocalWikiData\PROTOCOL-ARCHITECT-PLAN-SKILL-AGENT-ROLE-REVIEW-v1.md
```

The executor and architect must load that protocol before accepting a plan,
reviewing a completion claim, switching roles, or repairing a loophole.

Required lifecycle:

```text
plan creation
  -> plan review
  -> execution
  -> review
  -> architect repair if a gap is found
  -> executor continuation
```

Required role loop:

```text
ARCHITECT_INTERNAL
  -> EXECUTOR_INTERNAL
  -> REVIEW_INTERNAL
  -> ARCHITECT_INTERNAL_REPAIR
  -> EXECUTOR_INTERNAL_CONTINUE
```

Required skills by lifecycle stage:

```text
plan_creation: planning-session-startup, intent-to-plan, plan-self-validation,
  goal-delivery-completeness
plan_review: plan-review, audit-protocol, claim-verification, output-skepticism
execution: code-execution, code-examination, change-propagation,
  verification-before-completion
code_review: generated-code-review, self-verification, test-integrity,
  dna-compliance-guard
documentation_sync: documentation-sync, document-hierarchy, skill-graph-sync
completion_verification: goal-delivery-completeness, coverage-completeness-gate,
  mvp-round-trip-verification, verification-before-completion
loophole_repair: architect-behavior-classifier, plan-review,
  claim-verification, change-propagation
```

Gate:

```text
plan_creation_orchestration_present == true
architect_reference_protocol_loaded == true
plan_review_orchestration_present == true
plan_execution_orchestration_present == true
role_loop_orchestration_present == true
skill_orchestration_present == true
document_rag_orchestration_present == true
anti_loophole_register_present == true
state_fields_for_governance_present == true
completion_matrix_includes_governance_rows == true
score_rules_present == true
bounded_work_order_or_architect_evidence_packet_required == true
canonical_allow_final_verdict_required == true
post_push_architect_recheck_required == true
repair_returns_to_executor == true
soft_stop_after_governance_repair_forbidden == true
```

If a child plan lacks this block, executor must patch the child plan and the
active state first, then continue execution. The patch is not a user-facing
stop, not a final report, and not a new approval gate when the next action is
known.

### 7.0.4. Next-action convergence after challenge

After Luba challenges a stop or completion report, the architect must compare all next-action sources. The plan is not allowed to finish while different artifacts name different next actions.

Required sources:

```text
state.next_action
state.last_plan_reminder.next_action
state.execution_attempt_latest.next_action
completion_matrix.next_action
first_failed_matrix_row.next_plan_action
latest report next_action fields
```

Gate addition:

```text
next_action_sources_compared == true
next_action_sources_converged == true
stale_next_actions_invalidated == true
```
### 7.0.3. Continuation must be executable

If architect verdict is `ARCHITECT_VERDICT_CONTINUE_EXECUTION`, the state must name a concrete executable next action.

When completion matrix has zero failed rows but `goal_reached=false`, the only valid next action is canonical OP-30 final finish review, not a generic continuation phrase.

Gate addition:

```text
continue_verdict_has_concrete_next_action == true
zero_failed_rows_routes_to_OP30_final_review == true
empty_or_generic_next_action_forbidden == true
```
### 7.0.2. No final repair-summary stop

Architecture repair, governance repair, commit/push repair, and status reconciliation are checkpoints only.

If this plan is still open, a repair report cannot be the final user-facing answer. After each repair checkpoint, architect remains the session owner, issues a bounded executor work order only when implementation is needed, persists the returned evidence packet, and continues from the plan-derived next action or runs OP-30 final review.

Required sequence:

```text
ARCHITECT_INTERNAL_REPAIR
  -> persist repair evidence
  -> issue bounded EXECUTOR_INTERNAL work order when implementation is needed
  -> persist executor work-order result packet
  -> ARCHITECT_INTERNAL_REVIEW
  -> update state
  -> continue by architect-owned next work order or OP-30 final review
```

Forbidden sequence:

```text
ARCHITECT_INTERNAL_REPAIR
  -> commit/push repair
  -> final response
```

Gate addition:

```text
final_repair_summary_stop_forbidden == true
bounded_work_order_or_architect_evidence_after_repair_required == true
repair_checkpoint_not_final == true
state_goal_reached_false_blocks_final == true
architect_session_owner_after_repair == true
executor_work_order_bounded == true
executor_final_response_forbidden == true
```

### 7.0.3. Architect-owned work order rule

Executor is never the owner of this plan. Architect owns the plan, the role
transition, the stop decision, the blocker decision, and the final verdict.

Executor may act only inside one bounded work order. The work order must include:

```text
work_order_id
goal slice tied to this plan section
allowed files/directories
allowed commands
forbidden actions
expected artifacts
state fields to update
commit/push expectation
return condition
evidence packet path
```

If the work order is missing, executor cannot start. If executor reaches the
return condition, asks to pause, claims blocker, wants to report done, needs
broader scope, or Luba asks for architect/review/coverage/loopholes/settings,
executor authority expires immediately and ARCHITECT_INTERNAL resumes control.

Architect may update this plan, protocol, state, matrix, skills, and agent
governance directly. Architect may not implement product/runtime/training code
directly. Product implementation requires a bounded work order and architect
review of the returned evidence.

Acceptance:

```text
session_owner_role == ARCHITECT_INTERNAL
executor_standing_authority == false
bounded_work_order_required == true
visible_architect_review_required == true
hidden_json_review_is_not_final == true
```
### 7.1. Required executor skills

Executor must not start implementation from this plan until it has confirmed
that the required skills exist in `all-skills.zip` and has recorded them in
its run trace as `skills_loaded`.

The required skills are grouped by purpose, because missing the purpose is as
dangerous as missing the file.

| Purpose | Required skills |
|---|---|
| Basic execution discipline | `agent-constitution`, `no-product-decisions`, `dev-safety`, `project-map`, `infrastructure-discovery` |
| Architect and plan review | `session-mode-declaration`, `goal-context-persistence`, `plan-review`, `plan-self-validation`, `goal-delivery-completeness`, `architect-behavior-classifier`, `specificity-calibration`, `design-artifact-completeness` |
| Plan execution verification | `verification-before-completion`, `claim-verification`, `output-skepticism`, `goal-context-persistence`, `plan-execution-feedback`, `phase-review`, `output-contract` |
| Code reading before edits | `code-examination`, `change-propagation`, `blast-radius-assessor`, `implementation-mode-gate`, `scope-isolation` |
| Code review after edits | `generated-code-review`, `self-verification`, `three-level-verification`, `test-integrity`, `dna-compliance-guard`, `human-language-gate` |
| Cross-stack wiki work | `client-server-symmetry`, `graph-rag-integration`, `dpo-training-data`, `dpo-triples-to-experiment-pairs`, `learning-signal-capture`, `learning-loop-closure` |
| Commit and delivery report | `phase-git-report`, `phase-completion-packager`, `session-execution-log` |
| Documentation sync after code changes | `documentation-sync`, `skill-graph-sync`, `document-hierarchy` |

Skill loading gates:

```text
Before plan execution:
  plan-review
  goal-delivery-completeness
  verification-before-completion
  output-skepticism

Before code inspection:
  code-examination
  change-propagation
  blast-radius-assessor

Before code edits:
  implementation-mode-gate
  scope-isolation
  dev-safety

Before code completion claim:
  generated-code-review
  self-verification
  verification-before-completion
  dna-compliance-guard

Before code slice is considered closed:
  documentation-sync
  skill-graph-sync
  document-hierarchy

Before any claim that the plan is complete:
  session-mode-declaration
  goal-context-persistence
  plan-self-validation
  goal-delivery-completeness
  phase-review
  output-contract

Before role transition:
  session-mode-declaration
  goal-context-persistence
  output-contract
  verification-before-completion
```

If any required skill is missing from `all-skills.zip`, executor must record:

```text
status: BLOCKED_WITH_GAPS
reason: missing required skill
missing_skill: <skill-name>
next_action: add or restore the missing skill before continuing implementation
```

This is a blocker for implementation, not permission to claim partial success.

`test-integrity` and `three-level-verification` do not authorize executor to
run broad tests automatically. They define how verification must be interpreted
when a verification step is explicitly allowed by the current plan or by the
active user request.

### 7.2. Continuous role loop without stopping execution

This plan uses roles as internal execution states, not as user-facing stop
points.

The correct model is an architect-owned continuous loop, not a staged handoff:

```text
architect owns session and plan
  then architect fixes architecture or issues bounded executor work order
  then executor returns evidence packet and authority expires
  then architect reviews/checks the result
  then fix the next actionable gap
  repeat until gate passed or real blocker reached
```

The executor must not expose the internal loop as:

```text
ARCHITECT -> stop
EXECUTOR -> stop
REVIEW -> stop
EXECUTOR -> stop
```

The internal switch from planning to implementation is allowed without a final
response when all conditions below are true:

```text
architecture gate passed
current plan gives the next numbered implementation step
required executor skills are loaded
Luba has already asked to continue/proceed/execute or not stop
no hard blocker exists
```

In that case executor must not report:

```text
plan done
waiting for approval
ready when you confirm
architecture complete
review complete
implementation slice complete
```

Instead it must continue with the next implementation or remediation step named
by this plan.

Review is an internal quality loop. It means applying the plan's verification
skills and code-review skills. It does not mean running broad tests unless the
current gate explicitly allows that verification.

When review finds actionable gaps, the review result becomes the next input to
execution immediately. It is not a stop gate and not a reason for a final
message.

Only these outcomes may stop the continuous loop:

```text
BLOCKED_WITH_GAPS when the plan lacks information and local RAG cannot answer it
BLOCKED_WITH_GAPS when implementation is unsafe without missing input
FINAL REPORT when the current phase gate or full completion gate passes
PAUSED only when Luba explicitly asks to pause
```

Every internal state must preserve the same main goal:

```text
teach XIIGen to create, train, export, import, validate and upgrade all units
needed for project-local wiki over Angular + .NET + SQL stored procedures
```

If the next internal action would not advance that goal, executor must record
goal drift and return to the plan step that does.

If these sources contradict each other, executor must record the contradiction
as a plan gap and continue with the safest non-destructive planning step. It
must not invent the answer.

### 7.4. Plugin project documentation/RAG/scenario lookup

When the executor needs to know where documentation, generated Document RAG, or
plugin test scenarios live for a validation project, it must read this artifact
first:

```text
C:\Projects\llm_mvp_core\43.LearningLocalWikiData\validation\PLUGIN-PROJECT-DOC-RAG-SCENARIO-MATRIX-v1.json
```

If that matrix is missing or lacks a row for the project group, this is not a
reason to stop. The executor must add or repair the row from:

```text
C:\Projects\llm_mvp_core\43.LearningLocalWikiData\PLAN-PLUGIN-ORCHESTRATION-PROMPT-SUITE-GATE-v1.md section 4.2.1
C:\Projects\llm_mvp_core\43.LearningLocalWikiData\STATE-PROJECT-WIKI-CROSS-STACK-ADVANCED-GAP-CLOSURE.json
C:\Projects\llm_mvp_core\43.LearningLocalWikiData\reports\PROJECT-WIKI-COMPLETION-COVERAGE-MATRIX.json
```

Per-project lookup must answer:

```text
where to read human documentation
where to read or generate .xiigen/wiki-domain evidence
which Visual Studio / VS Code plugin scenarios to execute
what to do when docs, RAG output, linked repos, SQL, or plugin entrypoints are absent
```

The allowed fallback is source-code evidence or generated project-local wiki
evidence. The forbidden fallback is a plugin-local semantic guess or a fake
ready/done claim.

## 8. Final Continuation Gate

Это не стоп-гейт. Это gate перехода между architecture foundation work и
следующей фазой.

После выполнения foundation gaps executor не должен объявлять `done` и не должен
останавливаться из-за самого факта отчёта. Он должен выбрать один из двух
статусов и продолжить по правилам ниже.

Allowed final statuses:

```text
ARCHITECTURE_READY_FOR_EXECUTOR
BLOCKED_WITH_GAPS
```

These are not full task completion statuses.

`ARCHITECTURE_READY_FOR_EXECUTOR` разрешён только если выполнено всё:

```text
governance docs are readable
STATE-PROJECT-WIKI-CROSS-STACK-ADVANCED-GAP-CLOSURE.json schema is specified
main focus goal is stored in state
registry schema includes manifest_ref, locator_key and remote_unit_id
import/export gate is specified
canonical units are explicitly trainable/exportable/importable
static semantic decision logic is forbidden
per-unit upgrade and rollback contract is specified
CROSS-STACK-SYSTEM-MAP.json schema is specified
ADVANCED-WIKI-QUESTION-SUITE-v1.json schema is specified
ORCHESTRATOR-PROMPT-PLANNING-SUITE-v1.json schema is specified
PROJECT-WIKI-REAL-PROJECT-PROMPT-VALIDATION.json gate is specified
PROJECT-WIKI-PLUGIN-READINESS-GATE.json gate is specified
70-question distribution is specified
all 14 advanced question classes are mapped
all scenario classes from orchestrator prompts.txt are mapped
completion coverage matrix is specified and blocks false-positive completion
SystemBoundaryMapperUnit is specified as trainable project unit
next-training-action selection order is specified after gap signals
readiness gate forbids validation when blocker exists
continuous role loop is specified and does not create hidden stop points
```

`BLOCKED_WITH_GAPS` обязателен, если хотя бы один пункт выше отсутствует.

Continuation rule:

```text
If status is ARCHITECTURE_READY_FOR_EXECUTOR and the active user request asks to
continue implementation, executor continues into the next implementation phase.

If status is BLOCKED_WITH_GAPS and the gaps are actionable inside this plan,
executor fixes the next gap instead of stopping.

Executor stops only when the blocker cannot be resolved from local files or when
the user explicitly asks to pause.

Executor may claim `TASK_FULLY_COMPLETE` only under section 5.1 and only after
ARCHITECT_INTERNAL has persisted exactly `ARCHITECT_VERDICT_ALLOW_FINAL`.
Any custom verdict string is non-final and must be repaired before response.
Architect must also persist and check an architect-only evidence packet or a
bounded executor work-order result packet before final.
Final-answer authority addendum:
The final gate is not passed by semantic equivalence. It is passed only when the
latest persisted architect finish review contains `verdict: ARCHITECT_VERDICT_ALLOW_FINAL`,
`verdict_is_exact_token: true`, an architect-verified evidence packet id/hash,
`conditions: NONE`, and `blockers: NONE`. Any mutation after that review
requires a new post-mutation architect finish review.
```

Forbidden in this architecture phase:

```text
claim model/project/wiki readiness
claim done
change branch
run tests because this plan changed
```

The following actions are not forbidden when they are the plan-derived next
action and the required local files/contracts are available:

```text
start training for the named unit
create actual 50-row batches for the named unit
run the named unit validation gate
publish the named manifest/checkpoint through IModelRepository
register the named locator entry
commit/push the completed phase artifacts when that phase gate passes
reconcile state, matrix, self-review and readiness reports with current HEAD after commit/push
```

Do not use "architecture phase" as a reason to stop after the architecture has
already produced a concrete execution action. In that case the role transition
is internal and execution continues.

No hidden time or session limits:

```text
no 120-second default limit as project policy
no stop because a command is long-running
no stop because the session is long
no false completion because context is compacted
no report-only pause when next action is known
no stop because one commit/push slice completed while the active goal remains open
no stop because commit/push failed if another safe step is available
no premature stop before full task completion or hard blocker
```

Long operations must use explicit long timeout, resumable state, or persisted
runner artifacts. Time is a metric, not a stop reason.

Stopping early without a hard blocker must be reported as:

```text
PREMATURE_STOP_PROTOCOL_VIOLATION
```
