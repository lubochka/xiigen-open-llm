# План проверки plugin orchestration для orchestrator prompts

Статус: архитектурный план, не execution report.
Дата: 2026-05-25.
Режим: ARCHITECT.

Этот документ исправляет неполную постановку предыдущего плана. Предыдущий gate
проверил, что Visual Studio и VS Code плагины могут загрузить XIIGen runtime
contract/readiness. Это не доказывает, что плагины могут соркестрировать любую
задачу из `orchestrator prompts.txt` на любом репозитории.

## 1. Главная цель

Плагины должны быть способными инициировать XIIGen orchestration для задач из
`orchestrator prompts.txt` на выбранном пользователем репозитории или группе
репозиториев.

Правильная цепочка:

```text
Visual Studio / VS Code plugin
  -> принимает пользовательский prompt
  -> определяет active repository / linked repositories
  -> вызывает XIIGen orchestration entrypoint
  -> XIIGen строит wiki query plan
  -> XIIGen выбирает нужные trainable wiki units
  -> XIIGen проверяет evidence / missing evidence
  -> plugin получает plan / answer / blocked state
  -> plugin показывает результат без mock и без локальной догадки
```

Плагин не должен сам решать semantic routing. Он должен быть host-адаптером к
обучаемым проектным units.

## 2. Что было неправильно в настройках

Неполная задача появилась из-за трех слабых мест.

### 2.1. Step 14 проверял readiness, а не orchestration

В активном плане Step 14 назывался:

```text
Проверить build/readiness для Visual Studio и VS Code plugins
```

Это разрешило проверить:

```text
VSIX создан
runtime DLL открыта
VS Code module загружен
checkReadiness работает
model weights не embedded
```

Но не требовало проверить:

```text
plugin принимает prompt из orchestrator prompts.txt
plugin передает prompt в XIIGen orchestration
plugin передает repo scope
plugin получает query plan / unit plan / evidence plan
plugin возвращает blocked state when evidence is missing
```

### 2.2. Completion matrix приняла plugin readiness как достаточный plugin proof

HOLE-008 и HOLE-016 закрывали mock/readiness проблемы, но не требовали
`PLUGIN-ORCHESTRATION-PROMPT-SUITE-GATE.json`.

Нужно добавить новый hole:

```text
HOLE-018 plugin_orchestration_prompt_suite
```

### 2.3. Claim verification различал static vs runtime, но не readiness vs orchestration

Текущее правило правильно говорит, что static build не равен runtime readiness.
Но нужно добавить еще одно различие:

```text
plugin readiness != plugin orchestration
```

Runtime readiness говорит: plugin может подключиться.
Runtime orchestration говорит: plugin может выполнить пользовательскую задачу
через XIIGen orchestration pipeline.

## 3. Новый обязательный gate

Новый gate:

```text
43.LearningLocalWikiData/reports/PLUGIN-ORCHESTRATION-PROMPT-SUITE-GATE.json
```

Он должен быть `passed=true` до любого полного claim:

```text
plugins can orchestrate any supported prompt task on any supported repo scope
```

## 4. Scope проверки

### 4.1. Plugin hosts

Проверить оба host-а:

```text
C:\Projects\xiigen_visual_studio
C:\Projects\xiigen_visual_code
```

### 4.2. Repo scopes

Минимальные repo scope classes:

```text
single_dotnet_repo
single_angular_repo
linked_angular_dotnet_pair
linked_angular_dotnet_sql_system
translated_xiigen_flows_repo
translated_xiigen_platform_repo
unknown_or_missing_evidence_repo
```

Проверочные проекты:

```text
C:\Projects\bizguide-client
C:\Projects\GenieDynamicApp
C:\Projects\geniesoftGameMVP
C:\Projects\wizGameserver
C:\Projects\GenieDynamicServer
C:\Projects\geniesoftGameMVPServer
C:\Projects\genietasks
C:\Projects\askapediaweb
C:\Projects\xiigen-flows-dot-net-angular
C:\Projects\xiigen-platform-dot-net-angular
```

### 4.2.1. Per-project documentation, Document RAG, and plugin scenario matrix

Project names alone are not proof of plugin coverage. Before any plugin probe
can claim a project is covered, the executor must create and use this matrix:

```text
43.LearningLocalWikiData/validation/PLUGIN-PROJECT-DOC-RAG-SCENARIO-MATRIX-v1.json
```

Every validation project or linked project group must have one row. Each row
must say exactly:

```text
project_group_id
project_roots
repo_scope_class
business_system_role
documentation_sources
document_rag_sources
plugin_test_scenarios
expected_prompt_manifest_scope
missing_source_fallback
blocked_if_missing
status
```

Required project groups:

```text
bizguide_client_single_angular
genie_dynamic_linked_system
geniesoft_game_mvp_linked_system
wiz_game_server_server_scope
genietasks_project_scope
askapediaweb_project_scope
xiigen_flows_translated_full_stack
xiigen_platform_translated_full_stack
```

The matrix must distinguish four different things:

```text
documentation_sources: human docs and repo metadata to read first
document_rag_sources: generated .xiigen/wiki-domain evidence and graph paths
plugin_test_scenarios: actual Visual Studio / VS Code orchestration scenarios
missing_source_fallback: what to inspect or generate when docs/RAG are absent
```

Fallback rule:

```text
missing README or design docs -> use source code and generated wiki evidence
missing .xiigen/wiki-domain outputs -> generate project-local wiki evidence first
missing linked Angular/.NET pair -> block linked-system claim for that row
missing SQL stored procedure evidence -> block stored-procedure flow claim
missing plugin entrypoint -> block plugin orchestration claim
```

Gate:

```text
PLUGIN-PROJECT-DOC-RAG-SCENARIO-MATRIX-v1.json exists
all_validation_project_groups_have_matrix_row == true
all_project_rows_have_documentation_sources == true
all_project_rows_have_document_rag_sources == true
all_project_rows_have_plugin_test_scenarios == true
all_project_rows_have_missing_source_fallback == true
missing_docs_do_not_block_when_source_code_evidence_exists == true
missing_client_server_sql_link_blocks_full_linked_system_claim == true
```

### 4.3. Prompt suite

Source of truth:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\orchestrator prompts.txt
```

Every scenario class already represented in
`ORCHESTRATOR-PROMPT-PLANNING-SUITE-v1.json` must be executed through each
plugin-facing orchestration path.

Scenario class coverage is not enough. The gate must also keep a prompt-by-
prompt manifest for the source file. Each meaningful prompt/request block from
`orchestrator prompts.txt` must have a concrete expected result, not only a
class label.

Minimum scenario classes:

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

### 4.4. Prompt-by-prompt expected result manifest

Required manifest:

```text
43.LearningLocalWikiData/validation/ORCHESTRATOR-PROMPT-PLUGIN-EXPECTED-RESULTS-v1.json
```

This manifest must be built from the source prompt file, not invented from the
scenario class list. Each row must include:

```text
source_prompt_id
source_line_start
source_line_end
source_prompt_text_hash
scenario_class
repo_scope_classes_to_test
plugin_hosts_to_test
expected_orchestration_intent
expected_phase_plan
expected_wiki_query_series
expected_selected_units
expected_repo_scope_behavior
expected_evidence_requirements
expected_missing_evidence_behavior
expected_user_visible_result_shape
negative_rejected_result_shape
pass_assertions
```

Additional fields required by the session transcript review:

```text
expected_user_facing_plugin_entrypoints
expected_node_a_to_b_pairs
expected_trainable_unit_inputs_outputs
expected_llm_unit_array_fanout
expected_arbiter_review_path
expected_reprocessing_path
expected_single_source_authority
```

These fields close two loopholes found during the session audit:

```text
plugin-facing entrypoint cannot mean only an internal helper function;
prompt expected results cannot stop at a user-visible answer shape.
```

The expected result must prove how XIIGen would break the prompt into trainable
unit work: what each unit receives, what it must produce, which unit consumes
that output, where arrays of language-specific or repository-specific units are
expected, and where an arbiter or reprocessing pass is required.

### 4.5. User-facing plugin entrypoint requirement

The plugin orchestration gate must execute the prompt through the path a user
can actually invoke inside the host, not only through a private test helper.

VS Code user-facing entrypoints currently visible in the plugin source:

```text
xiigen.openWikiPanel
xiigen.refreshStatus
xiigen.copyWikiContext
xiigen.showMap
Copilot/chat participant path that calls queryWiki(request.prompt)
```

Visual Studio user-facing entrypoints currently visible in the plugin source:

```text
ShowWikiPanelCommand
CopyWikiContextCommand
CodeWikiToolWindow
CodeWikiViewModel
WikiService calling IWikiOrchestrator
```

The gate must record which entrypoint was used for every prompt execution row.
An internal runner-only call may be used to prepare fixtures, but it cannot be
the only evidence for plugin orchestration.

Required report fields:

```text
plugin_host
user_facing_entrypoint
entrypoint_invocation_method
prompt_id
repo_scope_class
orchestration_call_observed
query_plan_returned
selected_units_returned
evidence_or_blocked_state_returned
mock_output_detected
```

Gate:

```text
every prompt execution row has user_facing_entrypoint
every user_facing_entrypoint maps to a real plugin command/chat/tool-window path
internal_helper_only_execution_count == 0
```

### 4.6. Contract harness is not interactive IDE execution

There are two different evidence levels and they must never be merged:

```text
contract_harness:
  source verifies plugin entrypoint wiring
  direct adapter/runtime call returns selected_units and wiki_query_plan
  proves contract shape only

interactive_ide_plugin_scenario:
  VS Code Extension Host invokes the registered command, panel, or chat path
  Visual Studio Experimental Instance invokes command, tool window, or view model
  host invocation evidence is recorded per prompt row
  proves plugin scenario execution
```

The following is forbidden as full plugin scenario evidence:

```text
source-verified command wiring + direct ProjectWikiPluginOrchestrationAdapter execution
```

That combination may close only a contract-harness subgate. It cannot close
`visual_studio_plugin_orchestration_passed`,
`vs_code_plugin_orchestration_passed`, or
`all_prompt_rows_executed_through_user_facing_plugin_entrypoint` for the full
interactive plugin scenario gate.

Full plugin scenario rows must include:

```text
ide_host_process
host_invocation_command
host_invocation_log_or_transcript
extension_host_session_id_or_visual_studio_experimental_instance_id
prompt_visible_to_plugin_entrypoint
orchestration_result_returned_to_plugin_ui_path
```

Gate:

```text
contract_harness_passed may be true while interactive_ide_plugin_scenario_passed is false
interactive_ide_plugin_scenario_passed == true is required for full plugin orchestration claim
contract_harness_only_execution_count == 0 for full plugin orchestration claim
```

### 4.7. Dynamic-flow A-to-B orchestration expectation

The session history contains an earlier repeated requirement for flow prompts:
XIIGen must not only summarize the final application. It must plan the graph of
trainable units and data movement between them.

For any prompt that asks to create, orchestrate, transform, route, validate, or
generate a multi-step system, the expected result row must include:

```text
node_a_to_b_pairs: every producer unit and consumer unit pair
unit_training_data_surface: what rows/examples each unit needs
llm_unit_array_fanout: when one node expands into language/domain/repository units
arbiter_review_path: which arbiter checks output quality or conflicts
reprocessing_path: what gets retried or rebuilt after arbiter rejection
single_source_authority: which prompt/design/repository artifact is authoritative
```

Examples:

```text
figma_to_whole_system_orchestration:
  node_a_to_b_pairs includes figma_tree_intake -> screen_component_analysis,
  screen_component_analysis -> architecture_flow_generation,
  architecture_flow_generation -> client_server_contract_design,
  client_server_contract_design -> code_generation_plan.

alternative_server_languages:
  llm_unit_array_fanout includes node, rust, java, python, php unit variants
  with shared interface boundaries and separate validation expectations.

rag_context_planning_before_ai_stage:
  arbiter_review_path includes evidence sufficiency check before answer
  generation; reprocessing_path rebuilds query series when evidence is missing.
```

Gate:

```text
all_flow_or_orchestration_prompt_rows_have_node_a_to_b_pairs == true
all_language_or_stack_variant_rows_have_llm_unit_array_fanout == true
all_validation_or_generation_rows_have_arbiter_review_path == true
all_missing_evidence_rows_have_reprocessing_path == true
```

Minimum source prompt blocks from `orchestrator prompts.txt`:

```text
root_initial_project_skillset_request
simple_n8n_like_flow_management
figma_to_whole_system_orchestration
figma_into_existing_system
system_from_user_explanation_without_complete_design
multi_user_diet_application_daily_feedback
content_generation_pipeline
design_pattern_skill_library
advanced_uml_cycle_documentation_validation
project_base_examples_for_scenarios_1_to_5
generate_deploy_debug_small_business_system
dynamic_genie_code_skills_and_company_dna
generic_interfaces_database_queue_ai_fabric
generic_microservice_base
alternative_queue_technologies
alternative_server_languages
alternative_client_languages
alternative_database_and_rag_backends
rag_query_planning_before_ai_stage
rag_storage_planning_after_execution_feedback
advanced_prompt_phase_generation_skill
unit_testing_skill
e2e_testing_skill
ui_testing_skill
devops_ci_cd_skill
documentation_skill
swagger_skill
mcp_skill
logging_skill
safe_code_key_vault_vulnerability_upgrade_skill
optimization_resource_management_skill
graphrag_alternative_selection
hosting_alternative_selection
multi_ai_tool_skill_library_distribution
chat_interface_for_system_interaction
basic_prompt_no_code_plan_validate_examples_state_recovery
```

Concrete expected result rule:

```text
Each source_prompt_id must define what a correct plugin-orchestrated result must
contain. "Scenario class covered" is not a pass. The expected result must name
the phases, query series, units, evidence checks, repo scope behavior, and
negative result shape that the plugin must return for that prompt.
```

Examples of expected result specificity:

```text
figma_to_whole_system_orchestration:
  expected_phase_plan includes figma_tree_intake, screen_component_analysis,
  architecture_flow_generation, client_server_contract_design, feedback_loop,
  code_generation_plan, deploy_debug_plan.
  expected_wiki_query_series includes find_ui_entry_points, find_server_routes,
  find_data_contracts, find_existing_design_system, find_generated_flow_storage.

multi_user_diet_application_daily_feedback:
  expected_phase_plan includes registration_form_capture, user_profile_storage,
  diet_plan_generation_flow, daily_whatsapp_food_capture, calorie_supplement_calc,
  adjustment_generation, push_and_whatsapp_feedback.
  expected_missing_evidence_behavior blocks if client identity, storage model,
  WhatsApp integration, or calorie calculation evidence is missing.

alternative_server_languages:
  expected_user_visible_result_shape is a technology-variant skill plan with
  subfolders for node, rust, java, python, php and shared interface boundaries.
  negative_rejected_result_shape is a .NET-only plan.
```

## 5. Required plugin-facing contract

Both plugins need a plugin-facing orchestration entrypoint. Names can differ by
host, but the contract must carry the same data:

```text
prompt_text
active_repo_path
linked_repo_paths
repo_scope_class
expected_stack
request_id
session_id
allow_training
allow_project_local_wiki_update
```

Required output:

```text
status
orchestration_plan
selected_units
wiki_query_plan
evidence_requirements
missing_evidence
blocked_reason
answer_preview_or_plan_summary
state_update
metrics
```

Forbidden output:

```text
MOCK
local static answer
readiness-only response
answer without selected_units
answer without wiki_query_plan
answer without repo_scope
ready status when evidence is missing
```

## 6. Acceptance metrics

The new gate passes only if all are true:

```text
visual_studio_plugin_orchestration_passed == true
vs_code_plugin_orchestration_passed == true
visual_studio_experimental_instance_invocation_passed == true
vs_code_extension_host_invocation_passed == true
interactive_ide_plugin_scenario_passed == true
contract_harness_only_execution_count == 0
all_scenario_classes_executed_through_plugin_entrypoint == true
all_source_prompt_blocks_have_expected_results == true
all_source_prompt_expected_results_executed_through_plugin_entrypoint == true
all_prompt_rows_executed_through_user_facing_plugin_entrypoint == true
internal_helper_only_execution_count == 0
all_flow_or_orchestration_prompt_rows_have_node_a_to_b_pairs == true
all_language_or_stack_variant_rows_have_llm_unit_array_fanout == true
all_validation_or_generation_rows_have_arbiter_review_path == true
all_missing_evidence_rows_have_reprocessing_path == true
all_validation_project_groups_have_doc_rag_scenario_matrix_row == true
all_project_matrix_rows_have_documentation_sources == true
all_project_matrix_rows_have_document_rag_sources == true
all_project_matrix_rows_have_plugin_test_scenarios == true
all_project_matrix_rows_have_missing_source_fallback == true
all_repo_scope_classes_executed == true
mock_response_count == 0
readiness_only_response_count == 0
selected_units_present_rate == 1.0
wiki_query_plan_present_rate == 1.0
repo_scope_present_rate == 1.0
missing_evidence_blocked_state_accuracy >= 0.95
scenario_planning_accuracy >= 0.85
wrong_repo_scope_rate == 0
cpu_ms numeric
ram_mb numeric
elapsed_ms numeric
```

## 7. Implementation phases

### 7.0. Plan governance orchestration gate

Before any implementation phase of this plan, the executor must verify that
plan creation, plan review, execution, review, architect repair, skill loading,
state updates, score penalties, and completion matrix coverage are orchestrated
inside the plan itself.

This is not a separate stop gate. It is an internal ARCHITECT_INTERNAL check
that returns to EXECUTOR_INTERNAL_CONTINUE when `next_action` is known.

Required orchestration fields:

```text
main_focus_goal
architect_reference_protocol
round_control_constitution
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

`round_control_constitution` must point to:

```text
C:\Projects\llm_mvp_core\43.LearningLocalWikiData\CONSTITUTION-EXECUTOR-ARCHITECT-ROUND-CONTROL-v1.md
```

This constitution is mandatory because a false-positive finish is dangerous to
the survival of the project and can cause catastrophic-scale consequences for
the executor's authority in the project. If executor attempts to finish without
architect handoff, executor rights close immediately until ARCHITECT_INTERNAL
records repair and reinstatement.

The plugin plan cannot be accepted, executed, or reported complete unless this
protocol is loaded for plan review, role transition review, report-scope review,
and loophole repair.

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
round_control_constitution_loaded == true
plan_review_orchestration_present == true
plan_execution_orchestration_present == true
role_loop_orchestration_present == true
skill_orchestration_present == true
document_rag_orchestration_present == true
anti_loophole_register_present == true
state_fields_for_governance_present == true
completion_matrix_includes_governance_rows == true
score_rules_present == true
repair_returns_to_executor == true
soft_stop_after_governance_repair_forbidden == true
```

If any field is missing, the executor must add it to the governing plan,
AGENTS.md, CLAUDE.md, skills guide, active state, or completion matrix before
continuing. That repair is a continuation action, not a reason to stop.

### 7.0.1. Mandatory role loop and review/repair protocol

This plugin orchestration plan inherits the continuous role loop from
`PLAN-PROJECT-WIKI-CROSS-STACK-ADVANCED-GAP-CLOSURE-v1.md` section 7.2.

Roles are internal execution states, not stop points:

```text
ARCHITECT_INTERNAL
  -> EXECUTOR_INTERNAL
  -> REVIEW_INTERNAL
  -> ARCHITECT_INTERNAL_REPAIR when review finds a protocol/plan gap
  -> EXECUTOR_INTERNAL_CONTINUE
  -> repeat until PLUGIN-ORCHESTRATION-PROMPT-SUITE-GATE.json passed == true
```

The executor must not expose the loop as:

```text
ARCHITECT -> stop
EXECUTOR -> stop
REVIEW -> stop
ARCHITECT repair -> stop
```

Before every implementation phase below:

```text
1. ARCHITECT_INTERNAL verifies the phase still matches the main goal.
2. EXECUTOR_INTERNAL performs the next plan-derived action.
3. REVIEW_INTERNAL compares produced artifacts to the literal phase gate.
4. If review finds an actionable gap, ARCHITECT_INTERNAL_REPAIR patches the
   plan, AGENTS, CLAUDE, skills, state, or matrix that allowed the gap.
5. The repair records the hole in state and applies the score rule.
6. Execution returns to EXECUTOR_INTERNAL_CONTINUE automatically.
```

Review is mandatory after every executor slice that changes code, plan,
governance, state, report, matrix, plugin contract, or probe output. Review may
use only the verification allowed by the active phase; it must not run broad
tests unless the gate explicitly requires them.

If review finds that the slice does not satisfy the phase gate, this is not a
stop. It is the next input to the loop.

Required state fields after every role transition:

```text
current_internal_role_state
last_role_transition_reason
last_review_result
review_found_actionable_gap
architect_repair_required
architect_repair_applied
return_to_executor_required
next_action
goal_reached
stop_allowed
```

Gate:

```text
role_loop_protocol_present == true
review_required_after_each_executor_slice == true
architect_repair_required_for_protocol_gap == true
return_to_executor_after_repair == true
soft_stop_after_review_forbidden == true
state_tracks_role_transition_fields == true
```

### Phase 1. Add missing plan/gate references

Update:

```text
AGENTS.md
CLAUDE.md
docs/skills/claim-verification-SKILL.md
docs/skills/verification-before-completion-SKILL.md
docs/skills/output-skepticism-SKILL.md
43.LearningLocalWikiData/PLAN-PROJECT-WIKI-CROSS-STACK-ADVANCED-GAP-CLOSURE-v1.md
43.LearningLocalWikiData/reports/PROJECT-WIKI-COMPLETION-COVERAGE-MATRIX.json
43.LearningLocalWikiData/STATE-PROJECT-WIKI-CROSS-STACK-ADVANCED-GAP-CLOSURE.json
```

Gate:

```text
HOLE-018 exists in state and matrix
settings explicitly say plugin readiness is not plugin orchestration
```

### Phase 2. Define plugin orchestration contract

Add shared contract definitions in XIIGen project infrastructure, not as plugin
semantic logic.

Gate:

```text
contract includes prompt, repo scope, linked repos, selected units, query plan,
evidence requirements, blocked state, and metrics
```

### Phase 3. Build prompt expected-results manifest

Create:

```text
43.LearningLocalWikiData/validation/ORCHESTRATOR-PROMPT-PLUGIN-EXPECTED-RESULTS-v1.json
```

Use the per-project matrix while building the manifest:

```text
43.LearningLocalWikiData/validation/PLUGIN-PROJECT-DOC-RAG-SCENARIO-MATRIX-v1.json
```

Gate:

```text
every meaningful prompt/request block from orchestrator prompts.txt has one row
each row has expected_phase_plan
each row has expected_wiki_query_series
each row has expected_selected_units
each row has expected_evidence_requirements
each row has expected_missing_evidence_behavior
each row has negative_rejected_result_shape
each flow/orchestration row has expected_node_a_to_b_pairs
each language/stack variant row has expected_llm_unit_array_fanout
each validation/generation row has expected_arbiter_review_path
each missing-evidence row has expected_reprocessing_path
each row names repo_scope_classes_to_test from the per-project matrix
each row has missing-source behavior compatible with the per-project matrix
```

### Phase 4. Add VS Code user-facing orchestration probe

Use the VS Code command/chat path, not only `queryWiki` as a private helper.
`queryWiki` may remain the underlying transport only if the probe invokes it
through a registered command, panel action, or Copilot/chat participant path and
the call reaches XIIGen runtime orchestration instead of `mockHook`.

Source-code verification that `registerCommand(...)` exists is not enough.
The probe must invoke the registered command/chat path inside a VS Code
Extension Host session or an equivalent extension-test host and persist the host
transcript.

Gate:

```text
VS Code plugin-facing probe executes at least one prompt from every scenario class
every VS Code prompt row records user_facing_entrypoint
vs_code_extension_host_invocation_passed == true
contract_harness_only_execution_count == 0
internal_helper_only_execution_count == 0
mock_response_count == 0
readiness_only_response_count == 0
```

### Phase 5. Add Visual Studio user-facing orchestration probe

Replace `[MOCK] ask answers` readiness with a real XIIGen orchestration call or
explicitly mark the plugin as not orchestration-ready.

Source-code verification that the command or view model calls
`WikiService.OrchestrateAsync` is not enough. The probe must invoke the command,
tool window, or view model through a Visual Studio Experimental Instance or an
equivalent host integration harness and persist the host transcript.

Gate:

```text
Visual Studio plugin-facing probe executes at least one prompt from every scenario class
every Visual Studio prompt row records user_facing_entrypoint
visual_studio_experimental_instance_invocation_passed == true
contract_harness_only_execution_count == 0
internal_helper_only_execution_count == 0
MOCK is absent from orchestration output
readiness_only_response_count == 0
```

### Phase 6. Execute across repo scopes

Run the prompt suite through both plugins across the repo scope classes.

Gate:

```text
all_repo_scope_classes_executed == true
all_prompt_expected_result_rows_executed == true
linked_angular_dotnet_sql_system executed at least once
unknown_or_missing_evidence_repo returns blocked/missing-evidence state
```

### Phase 7. Persist final report

Write:

```text
43.LearningLocalWikiData/reports/PLUGIN-ORCHESTRATION-PROMPT-SUITE-GATE.json
```

Gate:

```text
passed == true
contract_harness_passed is reported separately from interactive_ide_plugin_scenario_passed
interactive_ide_plugin_scenario_passed == true
metrics numeric
every scenario class listed
every source_prompt_id listed with expected result assertion status
every repo scope class listed
Visual Studio and VS Code results separated
```

## 8. Full completion rule

After this plan exists, no answer may claim full plugin readiness for arbitrary
orchestrator prompt tasks unless:

```text
PLUGIN-ORCHESTRATION-PROMPT-SUITE-GATE.json passed == true
```

If only `PLUGIN-RUNTIME-INTEGRATION-GATE.json` passed, the allowed claim is:

```text
plugins can load XIIGen runtime/readiness contract
```

The forbidden claim is:

```text
plugins can orchestrate any prompt task on any repository
```

## 9. Partial progress is not a stop boundary

This plan contains multiple useful sub-results, but none of them is a
user-facing stop point while the full plugin orchestration gate remains open.

These are checkpoints only:

```text
expected-results manifest created
plugin adapter contract added
runtime hook probe passed
Visual Studio host available
VS Code host available
commit pushed
report persisted with passed=false
report persisted with passed=true for a narrower lower-level gate
```

If the state says:

```text
goal_reached == false
stop_allowed == false
next_action is non-empty
```

then the executor must continue the named next action in the same execution
loop. It must not send a final response that means "I made progress; continue
later."

The currently known open action is:

```text
Create and run automated host invocation harness: VS Code extension-development
command/chat execution with XIIGEN_WIKI_CONTEXT_HOOK, and Visual Studio
experimental-instance command/tool-window invocation with
XIIGEN_WIKI_CONTEXT_HOOK; persist per-prompt host evidence.
```

Any stop before that harness evidence is produced is a severe protocol
violation unless the state records a real blocker under AGENTS.md OP-13 with
`next_action` explicitly null.

Soft stops are forbidden with the same penalty. The executor must not end a
turn with:

```text
I can continue
I will continue next
next step is the harness
waiting for approval
checkpoint committed
status only
```

while the harness action is still executable. Each such soft stop is a severe
protocol violation and must record `-1000` in state before the architect repair
continues back into execution.

## 10. Next action

The next executor action is Phase 1:

```text
Add HOLE-018 to governance, active plan, state, and completion matrix.
```

This plan does not authorize saying the old plan was fully complete for plugin
orchestration. It narrows the old completion claim and opens the missing
plugin-facing orchestration work explicitly.
