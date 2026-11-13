# Wave 2 Batch 004 - Technical Design Remainder Full Path Pair Set v1
# Date: 2026-05-05

---

## Purpose

Provide build-ready node-01 through node-09 pair coverage for Phase 3 remaining technical design.

---

## Node 01 Router Pairs

| source_document_id | predicted_phase_id | predicted_sub_phase_id | predicate_ids | confidence | failure_classification |
|---|---|---|---|---:|---|
| W2B004-3.2-A-001 | phase-3-technical-design | 3.2-data-schema-and-migration-design | CREATE_OP, UPDATE_OP, VALIDATE_OP | 0.82 | source_classified_or_gap_visible |
| W2B004-3.2-B-002 | phase-3-technical-design | 3.2-data-schema-and-migration-design | CREATE_OP, UPDATE_OP, VALIDATE_OP | 0.82 | source_classified_or_gap_visible |
| W2B004-3.2-C-003 | phase-3-technical-design | 3.2-data-schema-and-migration-design | CREATE_OP, UPDATE_OP, VALIDATE_OP | 0.82 | source_classified_or_gap_visible |
| W2B004-3.2-D-004 | phase-3-technical-design | 3.2-data-schema-and-migration-design | CREATE_OP, UPDATE_OP, VALIDATE_OP | 0.82 | source_classified_or_gap_visible |
| W2B004-3.5-A-001 | phase-3-technical-design | 3.5-infrastructure-and-deployment-topology | CREATE, PROCESS_OP, HANDLE_ERROR | 0.82 | source_classified_or_gap_visible |
| W2B004-3.5-B-002 | phase-3-technical-design | 3.5-infrastructure-and-deployment-topology | CREATE, PROCESS_OP, HANDLE_ERROR | 0.82 | source_classified_or_gap_visible |
| W2B004-3.5-C-003 | phase-3-technical-design | 3.5-infrastructure-and-deployment-topology | CREATE, PROCESS_OP, HANDLE_ERROR | 0.82 | source_classified_or_gap_visible |
| W2B004-3.5-D-004 | phase-3-technical-design | 3.5-infrastructure-and-deployment-topology | CREATE, PROCESS_OP, HANDLE_ERROR | 0.82 | source_classified_or_gap_visible |
| W2B004-3.1-A-001 | phase-3-technical-design | 3.1-architecture-and-component-topology | DECLARE, CREATE, RETURN | 0.82 | source_classified_or_gap_visible |
| W2B004-3.1-B-002 | phase-3-technical-design | 3.1-architecture-and-component-topology | DECLARE, CREATE, RETURN | 0.82 | source_classified_or_gap_visible |
| W2B004-3.1-C-003 | phase-3-technical-design | 3.1-architecture-and-component-topology | DECLARE, CREATE, RETURN | 0.82 | source_classified_or_gap_visible |
| W2B004-3.1-D-004 | phase-3-technical-design | 3.1-architecture-and-component-topology | DECLARE, CREATE, RETURN | 0.82 | source_classified_or_gap_visible |
| W2B004-3.3-A-001 | phase-3-technical-design | 3.3-api-contract-definition | DECLARE, SEND_OP, AUTH_OP | 0.82 | source_classified_or_gap_visible |
| W2B004-3.3-B-002 | phase-3-technical-design | 3.3-api-contract-definition | DECLARE, SEND_OP, AUTH_OP | 0.82 | source_classified_or_gap_visible |
| W2B004-3.3-C-003 | phase-3-technical-design | 3.3-api-contract-definition | DECLARE, SEND_OP, AUTH_OP | 0.82 | source_classified_or_gap_visible |
| W2B004-3.3-D-004 | phase-3-technical-design | 3.3-api-contract-definition | DECLARE, SEND_OP, AUTH_OP | 0.82 | source_classified_or_gap_visible |
| W2B004-3.4-A-001 | phase-3-technical-design | 3.4-security-and-authorization-model | AUTH_OP, VALIDATE_OP, FAIL | 0.82 | source_classified_or_gap_visible |
| W2B004-3.4-B-002 | phase-3-technical-design | 3.4-security-and-authorization-model | AUTH_OP, VALIDATE_OP, FAIL | 0.82 | source_classified_or_gap_visible |
| W2B004-3.4-C-003 | phase-3-technical-design | 3.4-security-and-authorization-model | AUTH_OP, VALIDATE_OP, FAIL | 0.82 | source_classified_or_gap_visible |
| W2B004-3.4-D-004 | phase-3-technical-design | 3.4-security-and-authorization-model | AUTH_OP, VALIDATE_OP, FAIL | 0.82 | source_classified_or_gap_visible |

---

## Node 02 Domain Translator Pairs

| pair_id | coverage_status | formal_statement | handoff_marker | confidence |
|---|---|---|---|---:|
| N02-W2B004-3.2-A-001 | source_gap | The Data schema and migration design packet activates CREATE_OP, UPDATE_OP, VALIDATE_OP. The required behavior is to preserve the Data schema and migration design responsibility and keep the source condition $(System.Collections.Hashtable.Gap) visible. | route_to_knowledge_edge_context_pack | 0.82 |
| N02-W2B004-3.2-B-002 | source_gap | The Data schema and migration design packet activates CREATE_OP, UPDATE_OP, VALIDATE_OP. The required behavior is to preserve the Data schema and migration design responsibility and keep the source condition $(System.Collections.Hashtable.Gap) visible. | route_to_knowledge_edge_context_pack | 0.82 |
| N02-W2B004-3.2-C-003 | source_gap | The Data schema and migration design packet activates CREATE_OP, UPDATE_OP, VALIDATE_OP. The required behavior is to preserve the Data schema and migration design responsibility and keep the source condition $(System.Collections.Hashtable.Gap) visible. | route_to_knowledge_edge_context_pack | 0.82 |
| N02-W2B004-3.2-D-004 | source_gap | The Data schema and migration design packet activates CREATE_OP, UPDATE_OP, VALIDATE_OP. The required behavior is to preserve the Data schema and migration design responsibility and keep the source condition $(System.Collections.Hashtable.Gap) visible. | route_to_knowledge_edge_context_pack | 0.82 |
| N02-W2B004-3.5-A-001 | source_gap | The Infrastructure and deployment topology packet activates CREATE, PROCESS_OP, HANDLE_ERROR. The required behavior is to preserve the Infrastructure and deployment topology responsibility and keep the source condition $(System.Collections.Hashtable.Gap) visible. | route_to_knowledge_edge_context_pack | 0.82 |
| N02-W2B004-3.5-B-002 | source_gap | The Infrastructure and deployment topology packet activates CREATE, PROCESS_OP, HANDLE_ERROR. The required behavior is to preserve the Infrastructure and deployment topology responsibility and keep the source condition $(System.Collections.Hashtable.Gap) visible. | route_to_knowledge_edge_context_pack | 0.82 |
| N02-W2B004-3.5-C-003 | source_gap | The Infrastructure and deployment topology packet activates CREATE, PROCESS_OP, HANDLE_ERROR. The required behavior is to preserve the Infrastructure and deployment topology responsibility and keep the source condition $(System.Collections.Hashtable.Gap) visible. | route_to_knowledge_edge_context_pack | 0.82 |
| N02-W2B004-3.5-D-004 | source_gap | The Infrastructure and deployment topology packet activates CREATE, PROCESS_OP, HANDLE_ERROR. The required behavior is to preserve the Infrastructure and deployment topology responsibility and keep the source condition $(System.Collections.Hashtable.Gap) visible. | route_to_knowledge_edge_context_pack | 0.82 |
| N02-W2B004-3.1-A-001 | covered | The Architecture and component topology reinforcement packet activates DECLARE, CREATE, RETURN. The required behavior is to preserve the Architecture and component topology reinforcement responsibility and keep the source condition $(System.Collections.Hashtable.Gap) visible. | ready_for_scheme_planner | 0.82 |
| N02-W2B004-3.1-B-002 | covered | The Architecture and component topology reinforcement packet activates DECLARE, CREATE, RETURN. The required behavior is to preserve the Architecture and component topology reinforcement responsibility and keep the source condition $(System.Collections.Hashtable.Gap) visible. | ready_for_scheme_planner | 0.82 |
| N02-W2B004-3.1-C-003 | covered | The Architecture and component topology reinforcement packet activates DECLARE, CREATE, RETURN. The required behavior is to preserve the Architecture and component topology reinforcement responsibility and keep the source condition $(System.Collections.Hashtable.Gap) visible. | ready_for_scheme_planner | 0.82 |
| N02-W2B004-3.1-D-004 | covered | The Architecture and component topology reinforcement packet activates DECLARE, CREATE, RETURN. The required behavior is to preserve the Architecture and component topology reinforcement responsibility and keep the source condition $(System.Collections.Hashtable.Gap) visible. | ready_for_scheme_planner | 0.82 |
| N02-W2B004-3.3-A-001 | covered | The API contract definition reinforcement packet activates DECLARE, SEND_OP, AUTH_OP. The required behavior is to preserve the API contract definition reinforcement responsibility and keep the source condition $(System.Collections.Hashtable.Gap) visible. | ready_for_scheme_planner | 0.82 |
| N02-W2B004-3.3-B-002 | covered | The API contract definition reinforcement packet activates DECLARE, SEND_OP, AUTH_OP. The required behavior is to preserve the API contract definition reinforcement responsibility and keep the source condition $(System.Collections.Hashtable.Gap) visible. | ready_for_scheme_planner | 0.82 |
| N02-W2B004-3.3-C-003 | covered | The API contract definition reinforcement packet activates DECLARE, SEND_OP, AUTH_OP. The required behavior is to preserve the API contract definition reinforcement responsibility and keep the source condition $(System.Collections.Hashtable.Gap) visible. | ready_for_scheme_planner | 0.82 |
| N02-W2B004-3.3-D-004 | covered | The API contract definition reinforcement packet activates DECLARE, SEND_OP, AUTH_OP. The required behavior is to preserve the API contract definition reinforcement responsibility and keep the source condition $(System.Collections.Hashtable.Gap) visible. | ready_for_scheme_planner | 0.82 |
| N02-W2B004-3.4-A-001 | covered | The Security and authorization model reinforcement packet activates AUTH_OP, VALIDATE_OP, FAIL. The required behavior is to preserve the Security and authorization model reinforcement responsibility and keep the source condition $(System.Collections.Hashtable.Gap) visible. | ready_for_scheme_planner | 0.82 |
| N02-W2B004-3.4-B-002 | covered | The Security and authorization model reinforcement packet activates AUTH_OP, VALIDATE_OP, FAIL. The required behavior is to preserve the Security and authorization model reinforcement responsibility and keep the source condition $(System.Collections.Hashtable.Gap) visible. | ready_for_scheme_planner | 0.82 |
| N02-W2B004-3.4-C-003 | covered | The Security and authorization model reinforcement packet activates AUTH_OP, VALIDATE_OP, FAIL. The required behavior is to preserve the Security and authorization model reinforcement responsibility and keep the source condition $(System.Collections.Hashtable.Gap) visible. | ready_for_scheme_planner | 0.82 |
| N02-W2B004-3.4-D-004 | covered | The Security and authorization model reinforcement packet activates AUTH_OP, VALIDATE_OP, FAIL. The required behavior is to preserve the Security and authorization model reinforcement responsibility and keep the source condition $(System.Collections.Hashtable.Gap) visible. | ready_for_scheme_planner | 0.82 |

---

## Node 03 Scheme Planner Pairs

| pair_id | input_set | plan_id | steps | dependency_edges | branch_paths | confidence |
|---|---|---|---|---|---|---:|
| N03-3.2-data-schema-and-migration-design | translated packets for Data schema and migration design | plan-3.2-data-schema-and-migration-design | s01 classify source state; s02 preserve predicates CREATE_OP, UPDATE_OP, VALIDATE_OP; s03 route source gap if present; s04 produce human-visible next action | s01 -> s02 -> s03 -> s04 | source gap branch remains separate from covered branch | 0.82 |
| N03-3.5-infrastructure-and-deployment-topology | translated packets for Infrastructure and deployment topology | plan-3.5-infrastructure-and-deployment-topology | s01 classify source state; s02 preserve predicates CREATE, PROCESS_OP, HANDLE_ERROR; s03 route source gap if present; s04 produce human-visible next action | s01 -> s02 -> s03 -> s04 | source gap branch remains separate from covered branch | 0.82 |
| N03-3.1-architecture-and-component-topology | translated packets for Architecture and component topology reinforcement | plan-3.1-architecture-and-component-topology | s01 classify source state; s02 preserve predicates DECLARE, CREATE, RETURN; s03 route source gap if present; s04 produce human-visible next action | s01 -> s02 -> s03 -> s04 | source gap branch remains separate from covered branch | 0.82 |
| N03-3.3-api-contract-definition | translated packets for API contract definition reinforcement | plan-3.3-api-contract-definition | s01 classify source state; s02 preserve predicates DECLARE, SEND_OP, AUTH_OP; s03 route source gap if present; s04 produce human-visible next action | s01 -> s02 -> s03 -> s04 | source gap branch remains separate from covered branch | 0.82 |
| N03-3.4-security-and-authorization-model | translated packets for Security and authorization model reinforcement | plan-3.4-security-and-authorization-model | s01 classify source state; s02 preserve predicates AUTH_OP, VALIDATE_OP, FAIL; s03 route source gap if present; s04 produce human-visible next action | s01 -> s02 -> s03 -> s04 | source gap branch remains separate from covered branch | 0.82 |

---

## Node 04 Processor Pairs

| pair_id | plan_id | processed_outcome | status | confidence |
|---|---|---|---|---:|
| N04-3.2-data-schema-and-migration-design | plan-3.2-data-schema-and-migration-design | Processor accepts covered evidence for Data schema and migration design and converts $(System.Collections.Hashtable.Gap) into a knowledge-edge outcome when not covered. | processed_with_source_gap_visibility | 0.82 |
| N04-3.5-infrastructure-and-deployment-topology | plan-3.5-infrastructure-and-deployment-topology | Processor accepts covered evidence for Infrastructure and deployment topology and converts $(System.Collections.Hashtable.Gap) into a knowledge-edge outcome when not covered. | processed_with_source_gap_visibility | 0.82 |
| N04-3.1-architecture-and-component-topology | plan-3.1-architecture-and-component-topology | Processor accepts covered evidence for Architecture and component topology reinforcement and converts $(System.Collections.Hashtable.Gap) into a knowledge-edge outcome when not covered. | processed_with_source_gap_visibility | 0.82 |
| N04-3.3-api-contract-definition | plan-3.3-api-contract-definition | Processor accepts covered evidence for API contract definition reinforcement and converts $(System.Collections.Hashtable.Gap) into a knowledge-edge outcome when not covered. | processed_with_source_gap_visibility | 0.82 |
| N04-3.4-security-and-authorization-model | plan-3.4-security-and-authorization-model | Processor accepts covered evidence for Security and authorization model reinforcement and converts $(System.Collections.Hashtable.Gap) into a knowledge-edge outcome when not covered. | processed_with_source_gap_visibility | 0.82 |

---

## Node 05 Applier Pairs

| pair_id | processor_output_id | applied_result | result_class | confidence |
|---|---|---|---|---:|
| N05-3.2-data-schema-and-migration-design | N04-3.2-data-schema-and-migration-design | Applied result preserves Data schema and migration design, predicates CREATE_OP, UPDATE_OP, VALIDATE_OP, and source condition $(System.Collections.Hashtable.Gap). | applied_with_gap_visibility | 0.82 |
| N05-3.5-infrastructure-and-deployment-topology | N04-3.5-infrastructure-and-deployment-topology | Applied result preserves Infrastructure and deployment topology, predicates CREATE, PROCESS_OP, HANDLE_ERROR, and source condition $(System.Collections.Hashtable.Gap). | applied_with_gap_visibility | 0.82 |
| N05-3.1-architecture-and-component-topology | N04-3.1-architecture-and-component-topology | Applied result preserves Architecture and component topology reinforcement, predicates DECLARE, CREATE, RETURN, and source condition $(System.Collections.Hashtable.Gap). | applied_with_gap_visibility | 0.82 |
| N05-3.3-api-contract-definition | N04-3.3-api-contract-definition | Applied result preserves API contract definition reinforcement, predicates DECLARE, SEND_OP, AUTH_OP, and source condition $(System.Collections.Hashtable.Gap). | applied_with_gap_visibility | 0.82 |
| N05-3.4-security-and-authorization-model | N04-3.4-security-and-authorization-model | Applied result preserves Security and authorization model reinforcement, predicates AUTH_OP, VALIDATE_OP, FAIL, and source condition $(System.Collections.Hashtable.Gap). | applied_with_gap_visibility | 0.82 |

---

## Node 06 Result Translator Pairs

| pair_id | applied_result_id | human_fragment | visible_statuses | confidence |
|---|---|---|---|---:|
| N06-3.2-data-schema-and-migration-design | N05-3.2-data-schema-and-migration-design | Data schema and migration design: preserve predicates CREATE_OP, UPDATE_OP, VALIDATE_OP. Source condition: schema migration plan needs concrete downstream source. Next action remains visible. | can_proceed_or_source_gap, next_action | 0.82 |
| N06-3.5-infrastructure-and-deployment-topology | N05-3.5-infrastructure-and-deployment-topology | Infrastructure and deployment topology: preserve predicates CREATE, PROCESS_OP, HANDLE_ERROR. Source condition: deployment topology source needs project-specific environment. Next action remains visible. | can_proceed_or_source_gap, next_action | 0.82 |
| N06-3.1-architecture-and-component-topology | N05-3.1-architecture-and-component-topology | Architecture and component topology reinforcement: preserve predicates DECLARE, CREATE, RETURN. Source condition: none. Next action remains visible. | can_proceed_or_source_gap, next_action | 0.82 |
| N06-3.3-api-contract-definition | N05-3.3-api-contract-definition | API contract definition reinforcement: preserve predicates DECLARE, SEND_OP, AUTH_OP. Source condition: none. Next action remains visible. | can_proceed_or_source_gap, next_action | 0.82 |
| N06-3.4-security-and-authorization-model | N05-3.4-security-and-authorization-model | Security and authorization model reinforcement: preserve predicates AUTH_OP, VALIDATE_OP, FAIL. Source condition: none. Next action remains visible. | can_proceed_or_source_gap, next_action | 0.82 |

---

## Node 07 Arbiter Selector Pairs

| pair_id | reviewed_fragment_id | selected_arbiters | selection_reason | confidence |
|---|---|---|---|---:|
| N07-3.2-data-schema-and-migration-design | N06-3.2-data-schema-and-migration-design | source_authority_arbiter, status_visibility_arbiter | Review source authority and verify status visibility for Data schema and migration design. | 0.82 |
| N07-3.5-infrastructure-and-deployment-topology | N06-3.5-infrastructure-and-deployment-topology | source_authority_arbiter, status_visibility_arbiter | Review source authority and verify status visibility for Infrastructure and deployment topology. | 0.82 |
| N07-3.1-architecture-and-component-topology | N06-3.1-architecture-and-component-topology | source_authority_arbiter, status_visibility_arbiter | Review source authority and verify status visibility for Architecture and component topology reinforcement. | 0.82 |
| N07-3.3-api-contract-definition | N06-3.3-api-contract-definition | source_authority_arbiter, status_visibility_arbiter | Review source authority and verify status visibility for API contract definition reinforcement. | 0.82 |
| N07-3.4-security-and-authorization-model | N06-3.4-security-and-authorization-model | source_authority_arbiter, status_visibility_arbiter | Review source authority and verify status visibility for Security and authorization model reinforcement. | 0.82 |

---

## Node 08 Arbiter Unit Pairs

| pair_id | selected_arbiters | verdict_text | verdict | confidence |
|---|---|---|---|---:|
| N08-3.2-data-schema-and-migration-design | source_authority_arbiter, status_visibility_arbiter | Pass when covered evidence is preserved. Concern when $(System.Collections.Hashtable.Gap) is present. Do not convert source gaps into invented local facts. | pass_or_concern | 0.82 |
| N08-3.5-infrastructure-and-deployment-topology | source_authority_arbiter, status_visibility_arbiter | Pass when covered evidence is preserved. Concern when $(System.Collections.Hashtable.Gap) is present. Do not convert source gaps into invented local facts. | pass_or_concern | 0.82 |
| N08-3.1-architecture-and-component-topology | source_authority_arbiter, status_visibility_arbiter | Pass when covered evidence is preserved. Concern when $(System.Collections.Hashtable.Gap) is present. Do not convert source gaps into invented local facts. | pass_or_concern | 0.82 |
| N08-3.3-api-contract-definition | source_authority_arbiter, status_visibility_arbiter | Pass when covered evidence is preserved. Concern when $(System.Collections.Hashtable.Gap) is present. Do not convert source gaps into invented local facts. | pass_or_concern | 0.82 |
| N08-3.4-security-and-authorization-model | source_authority_arbiter, status_visibility_arbiter | Pass when covered evidence is preserved. Concern when $(System.Collections.Hashtable.Gap) is present. Do not convert source gaps into invented local facts. | pass_or_concern | 0.82 |

---

## Node 09 Formatter Pairs

| pair_id | arbiter_verdict_id | final_human_result | sections_present | confidence |
|---|---|---|---|---:|
| N09-3.2-data-schema-and-migration-design | N08-3.2-data-schema-and-migration-design | Data schema and migration design is represented. If source is missing, the result names the source gap and asks for the missing authority instead of inventing content. | represented, source_gap_visible, next_action | 0.82 |
| N09-3.5-infrastructure-and-deployment-topology | N08-3.5-infrastructure-and-deployment-topology | Infrastructure and deployment topology is represented. If source is missing, the result names the source gap and asks for the missing authority instead of inventing content. | represented, source_gap_visible, next_action | 0.82 |
| N09-3.1-architecture-and-component-topology | N08-3.1-architecture-and-component-topology | Architecture and component topology reinforcement is represented. If source is missing, the result names the source gap and asks for the missing authority instead of inventing content. | represented, source_gap_visible, next_action | 0.82 |
| N09-3.3-api-contract-definition | N08-3.3-api-contract-definition | API contract definition reinforcement is represented. If source is missing, the result names the source gap and asks for the missing authority instead of inventing content. | represented, source_gap_visible, next_action | 0.82 |
| N09-3.4-security-and-authorization-model | N08-3.4-security-and-authorization-model | Security and authorization model reinforcement is represented. If source is missing, the result names the source gap and asks for the missing authority instead of inventing content. | represented, source_gap_visible, next_action | 0.82 |

---

## Phrasing B Rule

Every row in this file has a Phrasing B companion by replacing the sub-phase title with an equivalent surface wording while preserving the same B output. The companion count equals the original row count for each node section.
