# Wave 2 Center Phase Batch 001 - Source Ledger v1
# Date: 2026-05-05

---

## Purpose

Start the real development-lifecycle quota work for the five center sub-phases
named by `CORPUS-EXPANSION-PLAN-v4.md`.

This is a quota seed batch, not the full Wave 2 quota.

Wave 2 full target:

```text
5 sub-phases * 100 training documents = 500 training documents
5 sub-phases * 20 held-out documents = 100 held-out documents
```

This batch contributes:

```text
20 training document packets
5 held-out document packets
```

Each center sub-phase receives four training rows, one per required stratum,
and one held-out row.

---

## Source Authority

All rows are Tier 1 local project material except the 3.4 security rows, which
use the already license-cleared CB4 Tier 2 corpus accepted in the CB4 intake
ledger.

The source paths below are source packets anchored to local documents or
accepted external-source pair files. A source packet is counted only when all
required v4 fields are populated.

---

## Training Rows

| document_id | source_path_or_url | source_tier | license_or_authority | primary_phase_id | primary_sub_phase_id | stratum | document_type | three_verifiable_claims | known_failure_mode | expected_top_predicates | expected_knowledge_edges | expected_conflicts | train_or_held_out | promotion_status |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| W2C-3.1-A-001 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\MULTI-PHASE-ORCHESTRATION-DESIGN-AND-EXPERIMENT-PLAN-v1.md#phase-context-packet` | Tier 1 - Local project document | Local project authority | phase-3-technical-design | 3.1-architecture-and-component-topology | A-domain-breadth | architecture-context-design | phase output creates downstream context; context packets preserve decisions; transition gates sit above the nine-node pipeline | routing to implementation before topology constraints are accepted | DECLARE, CREATE, RETURN | none | none | train | Wave 2 batch 001 router training |
| W2C-3.1-B-002 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\MULTI-PHASE-ORCHESTRATION-DESIGN-AND-EXPERIMENT-PLAN-v1.md#phase-registry` | Tier 1 - Local project document | Local project authority | phase-3-technical-design | 3.1-architecture-and-component-topology | B-team-context | architecture-registry-design | phase registry maps lifecycle phases; source document types are phase-owned; transition rules are topology contracts | treating phase registry as implementation storage instead of architecture topology | DECLARE, GET_OP, VALIDATE_OP | none | none | train | Wave 2 batch 001 router training |
| W2C-3.1-C-003 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\ORCHESTRATOR-INSPECTOR-DESIGN-v3-final.md#recursive-orchestrator` | Tier 1 - Local project document | Local project authority | phase-3-technical-design | 3.1-architecture-and-component-topology | C-depth-level | recursive-orchestrator-topology | orchestrators can nest; registry becomes orchestrator-shaped; trace trees are generated from runtime output | flattening nested orchestrators into one fixed-depth flow | BRANCH, CREATE, RETURN | none | none | train | Wave 2 batch 001 router training |
| W2C-3.1-D-004 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\CORPUS-EXPANSION-PLAN-v4.md#wda-gates` | Tier 1 - Local project document | Local project authority | phase-3-technical-design | 3.1-architecture-and-component-topology | D-failure-edge-case | architecture-gate-control | broad corpus waits for WDA gates; missing orchestration proof is a blocker; topology proof does not prove natural-language mutation | promoting corpus before topology shape is proven | VALIDATE_OP, FAIL, RETURN | none | premature corpus promotion | train | Wave 2 batch 001 router training |
| W2C-3.3-A-005 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\LOGICAL-UNIT-FLOW-ORCHESTRATOR-IMPLEMENTATION-PHASE-PLAN-v1.md#ilogicalunit` | Tier 1 - Local project document | Local project authority | phase-3-technical-design | 3.3-api-contract-definition | A-domain-breadth | logical-unit-contract | logical unit receives typed input; logical unit returns OperationResult; cancellation token remains part of async boundary | classifying typed interface contract as backend implementation instead of API contract | DECLARE, SEND_OP, AUTH_OP | none | none | train | Wave 2 batch 001 router training |
| W2C-3.3-B-006 | `C:\Projects\llm_mvp_core\21.AgentInfrastructure\Logical\LogicalContracts.cs#logical-flow-contracts` | Tier 1 - Local project source | Repository source authority | phase-3-technical-design | 3.3-api-contract-definition | B-team-context | runtime-contract-source | request envelope carries flow id; unit result carries confidence; flags define product behavior | dropping flags from contract handoff | DECLARE, GET_OP, VALIDATE_OP | none | none | train | Wave 2 batch 001 router training |
| W2C-3.3-C-007 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\WDA-01-SUB-PHASE-ROUTER-UNIT-CONTRACT-v1.md#b-output-shape` | Tier 1 - Local project document | Local project authority | phase-3-technical-design | 3.3-api-contract-definition | C-depth-level | router-contract-output | output carries predicted phase; output carries wrong-domain score; output carries knowledge edge | accepting classifier output without wrong-domain score | DECLARE, VALIDATE_OP, RETURN | none | none | train | Wave 2 batch 001 router training |
| W2C-3.3-D-008 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\CORPUS-EXPANSION-PLAN-v4.md#pair-shape-rule` | Tier 1 - Local project document | Local project authority | phase-3-technical-design | 3.3-api-contract-definition | D-failure-edge-case | pair-contract-rule | pair A is exact unit input; pair B is exact unit result; oracle must exist before pairs | writing descriptive B output instead of actual contract output | DECLARE, FAIL, VALIDATE_OP | none | malformed pair output | train | Wave 2 batch 001 router training |
| W2C-3.4-A-009 | `C:\Users\lubao\Documents\xiigen\llm mvp\external-sources-training-sets\wave-1\CB4-stage-3-pairs\node-pipeline-chain-set-ext25.md#CB4-3.4-811` | Tier 2 - Public permissive project document | License checked by user; manifest records source licenses | phase-3-technical-design | 3.4-security-and-authorization-model | A-domain-breadth | security-pipeline-trace | public clients require PKCE; backward compatibility is rejected; missing code_challenge returns reprocess | treating compatibility as a security exception | AUTH_OP, VALIDATE_OP, FAIL | none | backward compatibility exception | train | Wave 2 batch 001 router training |
| W2C-3.4-B-010 | `C:\Users\lubao\Documents\xiigen\llm mvp\external-sources-training-sets\wave-1\CB4-stage-3-pairs\node-cross-source-set-ext24.md#CB4-3.4-821` | Tier 2 - Public permissive project document | License checked by user; manifest records source licenses | phase-3-technical-design | 3.4-security-and-authorization-model | B-team-context | security-cross-source | IP-only throttling is partial; id_token_hint gap enables session forcing; two concerns do not escalate | escalating concerns into reprocess without MUST violation | AUTH_OP, BRANCH, VALIDATE_OP | none | false escalation | train | Wave 2 batch 001 router training |
| W2C-3.4-C-011 | `C:\Users\lubao\Documents\xiigen\llm mvp\external-sources-training-sets\wave-1\CB4-stage-3-pairs\node-cross-source-set-ext24.md#CB4-3.4-826` | Tier 2 - Public permissive project document | License checked by user; manifest records source licenses | phase-3-technical-design | 3.4-security-and-authorization-model | C-depth-level | compound-token-risk | HS256 shared secret enables forgery; no exp creates non-expiring tokens; risks compound | losing one side of compound token risk | AUTH_OP, FAIL, VALIDATE_OP | none | dropped compound edge | train | Wave 2 batch 001 router training |
| W2C-3.4-D-012 | `C:\Users\lubao\Documents\xiigen\llm mvp\external-sources-training-sets\wave-1\CB4-stage-3-pairs\node-cross-source-set-ext24.md#CB4-3.4-827` | Tier 2 - Public permissive project document | License checked by user; manifest records source licenses | phase-3-technical-design | 3.4-security-and-authorization-model | D-failure-edge-case | security-graduation-rule | IP-only throttling is concern; no PKCE is reprocess; reprocess overrides concern | allowing future release to satisfy current MUST | AUTH_OP, FAIL, BRANCH | none | future release exception | train | Wave 2 batch 001 router training |
| W2C-4.1-A-013 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\LOGICAL-UNIT-FLOW-ORCHESTRATOR-IMPLEMENTATION-PHASE-PLAN-v1.md#phase-plan` | Tier 1 - Local project document | Local project authority | phase-4-implementation | 4.1-backend-services | A-domain-breadth | backend-implementation-plan | implementation starts with one unit; existing tests remain oracle; full suite gates each phase | implementing multiple unit presences in one phase | CALL, CREATE_OP, HANDLE_ERROR | none | phase bundling | train | Wave 2 batch 001 router training |
| W2C-4.1-B-014 | `C:\Projects\llm_mvp_core\15.OrchestratorModel\Logical\LogicalOrchestratorAssistant.cs#run-flow` | Tier 1 - Local project source | Repository source authority | phase-4-implementation | 4.1-backend-services | B-team-context | backend-orchestrator-source | assistant selects registered flow; run returns human result; storage preserves session evidence | embedding topology mutation logic inline | CALL, GET_OP, RETURN | none | inline routing branch | train | Wave 2 batch 001 router training |
| W2C-4.1-C-015 | `C:\Projects\llm_mvp_core\35.ExperimentsV2\LogicalFlow\SecurityVerification34FlowAdapter.cs#execute` | Tier 1 - Local project source | Repository source authority | phase-4-implementation | 4.1-backend-services | C-depth-level | backend-flow-execution | flow executes node units; failures return OperationResult; metrics expose node anchors | direct exception path instead of result envelope | CALL, VALIDATE_OP, HANDLE_ERROR | none | exception leak | train | Wave 2 batch 001 router training |
| W2C-4.1-D-016 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\ROUND-MANIFEST-2026-05-05-PHASE-97-CB4-EXT25-PIPELINE-CHAIN.md#verification` | Tier 1 - Local project document | Local project authority | phase-4-implementation | 4.1-backend-services | D-failure-edge-case | implementation-verification | focused tests gate behavior; full suite gates regressions; source anchors prove station continuity | accepting pass without full-suite verification | VALIDATE_OP, FAIL, LOG_OP | none | unverified implementation | train | Wave 2 batch 001 router training |
| W2C-4.3-A-017 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\LOGICAL-UNIT-FLOW-ORCHESTRATOR-IMPLEMENTATION-PHASE-PLAN-v1.md#projects-21-23-map` | Tier 1 - Local project document | Local project authority | phase-4-implementation | 4.3-integration-and-provider-connectors | A-domain-breadth | provider-connector-plan | existing classes become adapters; adapters preserve current behavior; registry makes units swappable | replacing internals before adapter proof | SEND_OP, AUTH_OP, HANDLE_ERROR | none | unsafe replacement | train | Wave 2 batch 001 router training |
| W2C-4.3-B-018 | `C:\Projects\llm_mvp_core\35.ExperimentsV2\LogicalFlow\SecurityArbiterSelectorUnit.cs#select-arbiters` | Tier 1 - Local project source | Repository source authority | phase-4-implementation | 4.3-integration-and-provider-connectors | B-team-context | provider-selector-source | selector routes by pair anchor; selected arbiters constrain downstream review; combined sources require multiple arbiters | selecting only first matching provider authority | SEND_OP, AUTH_OP, BRANCH | none | dropped provider authority | train | Wave 2 batch 001 router training |
| W2C-4.3-C-019 | `C:\Projects\llm_mvp_core\35.ExperimentsV2\LogicalFlow\SecurityArbiterUnit.cs#cross-source-verdicts` | Tier 1 - Local project source | Repository source authority | phase-4-implementation | 4.3-integration-and-provider-connectors | C-depth-level | provider-arbiter-source | arbiter handles combined sources; verdicts preserve source pair; pass concern and reprocess are distinct | reducing multi-provider result to one verdict reason | AUTH_OP, VALIDATE_OP, RETURN | none | collapsed provider evidence | train | Wave 2 batch 001 router training |
| W2C-4.3-D-020 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\ROUND-MANIFEST-2026-05-05-PHASE-96-CB4-EXT24-CLOSURE.md#gate-status` | Tier 1 - Local project document | Local project authority | phase-4-implementation | 4.3-integration-and-provider-connectors | D-failure-edge-case | provider-cross-source-gate | ext24 base pairs are registered; source pair anchors must preserve; external AI calls remain zero | external fallback used before internal connector proof | SEND_OP, FAIL, AUTH_OP | none | external first execution | train | Wave 2 batch 001 router training |

---

## Held-Out Rows

| document_id | source_path_or_url | source_tier | license_or_authority | primary_phase_id | primary_sub_phase_id | held_out_category | document_type | three_verifiable_claims | known_failure_mode | expected_top_predicates | expected_knowledge_edges | expected_conflicts | train_or_held_out | promotion_status |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| W2C-3.1-H-001 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\ORCHESTRATOR-INSPECTOR-DESIGN-v3-final.md#live-trace-tree` | Tier 1 - Local project document | Local project authority | phase-3-technical-design | 3.1-architecture-and-component-topology | correct flow selection | topology-inspection-design | trace tree is recursive; topology grows from outputs; parallel children share execution level | classifying visual topology as frontend rendering only | BRANCH, CREATE, RETURN | none | none | held_out | Wave 2 batch 001 conformance seed |
| W2C-3.3-H-002 | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\WDA-01-SUB-PHASE-ROUTER-UNIT-CONTRACT-v1.md#unit` | Tier 1 - Local project document | Local project authority | phase-3-technical-design | 3.3-api-contract-definition | correct flow selection | logical-router-contract | unit id is declared; role is sub-phase-router; contract is typed input to typed output | routing contract text to backend service implementation | DECLARE, VALIDATE_OP, RETURN | none | none | held_out | Wave 2 batch 001 conformance seed |
| W2C-3.4-H-003 | `C:\Users\lubao\Documents\xiigen\llm mvp\external-sources-training-sets\wave-1\CB4-stage-3-pairs\node-pipeline-chain-set-ext25.md#CB4-3.4-817` | Tier 2 - Public permissive project document | License checked by user; manifest records source licenses | phase-3-technical-design | 3.4-security-and-authorization-model | conflict detection | logout-security-concern | id_token_hint is absent; session forcing is viable; harmless logout argument is rejected | formatter hides concern as harmless usability issue | AUTH_OP, BRANCH, VALIDATE_OP | none | harmless logout accepted | held_out | Wave 2 batch 001 conformance seed |
| W2C-4.1-H-004 | `C:\Projects\llm_mvp_core\35.ExperimentsV2\LogicalFlow\SecurityVerification34FlowAdapter.cs#metrics` | Tier 1 - Local project source | Repository source authority | phase-4-implementation | 4.1-backend-services | edge cases | metrics-implementation-source | metrics expose node anchors; output verdict is computed; attribution is visible | omitting intermediate evidence from runtime result | GET_OP, CONVERT_OP, RETURN | none | missing evidence anchor | held_out | Wave 2 batch 001 conformance seed |
| W2C-4.3-H-005 | `C:\Projects\llm_mvp_core\35.ExperimentsV2\LogicalFlow\SecurityArbiterSelectorUnit.cs#combined-arbiters` | Tier 1 - Local project source | Repository source authority | phase-4-implementation | 4.3-integration-and-provider-connectors | cross-sub-phase constraint | combined-provider-selector | combined rules select multiple arbiters; pair anchors drive routing; provider authority is not inferred from wording alone | wrong provider authority selected for combined source | SEND_OP, AUTH_OP, BRANCH | none | wrong arbiter family | held_out | Wave 2 batch 001 conformance seed |

---

## Gate Result

This source ledger satisfies the batch artifact contract for the first Wave 2
center-phase quota seed batch.

It does not complete the Wave 2 quota.
