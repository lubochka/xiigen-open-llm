# Wave 2 Center Phase Batch 001 - Node 02 Domain Translator Set v1
# Date: 2026-05-05

---

## Purpose

Add node-02 domain-translator coverage for the 20 training rows in Wave 2
Center Phase Batch 001.

Node 01 already routes each source packet to a sub-phase. Node 02 receives one
routed domain packet at a time and converts it into a formal domain statement.

Each A below is one domain packet. Each B is the actual translated statement,
not a description of what a translated statement should contain.

---

## Common Runtime Shape

```text
A:
  DomainPacket
    source_document_id: <document id from source ledger>
    phase_id: <node-01 predicted phase>
    sub_phase_id: <node-01 predicted sub-phase>
    coverage_status: covered | boundary | conflict
    prompt_fragment: <source claims + known failure mode>
    predicates: <expected top predicates>
    source_authority: <source tier + license or authority>

B:
  LogicalUnitResult<DomainTranslation>
    unit_id: unit-domain-translator-center-phase-v1
    unit_role: domain-translator
    domain_tag: <sub-phase id>
    output:
      source_document_id: <same id>
      formal_statement: <actual translated statement>
      coverage_status: <same or refined status>
      handoff_marker: <scheme planner routing marker>
      required_arbiters_hint: <none or named arbiter family>
    confidence: <score>
    predicate_ids: <predicates>
    evidence_ids: [<source_document_id>]
```

---

## Pair Index

| pair_id | A.source_document_id | A.sub_phase_id | A.coverage_status | B.formal_statement | B.handoff_marker | B.required_arbiters_hint | B.confidence |
|---|---|---|---|---|---|---|---:|
| W2C-N02-001 | W2C-3.1-A-001 | 3.1-architecture-and-component-topology | covered | A completed phase must emit downstream architecture context. The context carries accepted decisions, preserved constraints, and transition-gate requirements above the nine-node runtime pipeline. Implementation cannot start from this packet until topology constraints are accepted. | ready_for_scheme_planner | none | 0.89 |
| W2C-N02-002 | W2C-3.1-B-002 | 3.1-architecture-and-component-topology | covered | The phase registry is a topology contract for lifecycle structure. It maps phases, source-document ownership, and transition rules; it must not be treated as ordinary implementation storage. | ready_for_scheme_planner | none | 0.87 |
| W2C-N02-003 | W2C-3.1-C-003 | 3.1-architecture-and-component-topology | covered | Orchestrator topology is recursive. A registry-shaped orchestrator can appear as a child of another orchestrator, and trace children are generated from runtime output rather than from a fixed-depth flow. | ready_for_scheme_planner | none | 0.88 |
| W2C-N02-004 | W2C-3.1-D-004 | 3.1-architecture-and-component-topology | conflict | Broad corpus promotion is blocked until WDA gates prove the topology shape. Promoting corpus rows before the missing orchestration proof is a premature promotion conflict. | send_to_scheme_planner_with_rejection_flag | corpus_promotion_gate | 0.86 |
| W2C-N02-005 | W2C-3.3-A-005 | 3.3-api-contract-definition | covered | The logical unit contract defines typed input, typed output, operation-result wrapping, and cancellation boundary. This is an API contract obligation, not backend implementation detail. | ready_for_scheme_planner | none | 0.90 |
| W2C-N02-006 | W2C-3.3-B-006 | 3.3-api-contract-definition | covered | The runtime contract carries flow id, unit id, confidence, evidence ids, and execution flags. Dropping flags from the handoff would remove product behavior constraints from the API boundary. | ready_for_scheme_planner | none | 0.91 |
| W2C-N02-007 | W2C-3.3-C-007 | 3.3-api-contract-definition | covered | The router output contract must expose predicted phase, predicted sub-phase, confidence, nearest wrong-domain score, evidence predicates, and knowledge-edge status. The output is unsafe if the wrong-domain score is absent. | ready_for_scheme_planner | none | 0.88 |
| W2C-N02-008 | W2C-3.3-D-008 | 3.3-api-contract-definition | conflict | A valid training pair must contain the exact unit input and exact logical result output. A descriptive B output is malformed and must be rejected before it enters training. | send_to_scheme_planner_with_rejection_flag | pair_shape_arbiter | 0.86 |
| W2C-N02-009 | W2C-3.4-A-009 | 3.4-security-and-authorization-model | conflict | Public clients require PKCE. A missing code_challenge causes reprocess, and backward compatibility is not a valid security exception for the current requirement. | send_to_scheme_planner_with_rejection_flag | security_requirement_arbiter | 0.94 |
| W2C-N02-010 | W2C-3.4-B-010 | 3.4-security-and-authorization-model | boundary | IP-only throttling and missing id_token_hint are concern-level security gaps. They must stay visible as concerns and must not be escalated to reprocess without a MUST-level violation. | send_to_scheme_planner_with_boundary_flag | security_concern_arbiter | 0.90 |
| W2C-N02-011 | W2C-3.4-C-011 | 3.4-security-and-authorization-model | conflict | HS256 shared secret signing and absent token expiry form a compound token risk. The translated result must preserve both forgery risk and non-expiring token risk. | send_to_scheme_planner_with_rejection_flag | token_safety_arbiter | 0.93 |
| W2C-N02-012 | W2C-3.4-D-012 | 3.4-security-and-authorization-model | conflict | No PKCE is a current reprocess-level failure. IP-only throttling remains a concern, and a future release cannot satisfy the current MUST requirement. | send_to_scheme_planner_with_rejection_flag | security_requirement_arbiter | 0.92 |
| W2C-N02-013 | W2C-4.1-A-013 | 4.1-backend-services | covered | Backend implementation must add one logical unit presence per phase and keep existing tests as the oracle. Implementing multiple unit presences in one phase violates the phase discipline. | ready_for_scheme_planner | none | 0.89 |
| W2C-N02-014 | W2C-4.1-B-014 | 4.1-backend-services | covered | The assistant runtime selects registered flows and returns human results through the runtime surface. Topology mutation logic must not be hidden as inline branching inside the assistant. | ready_for_scheme_planner | none | 0.88 |
| W2C-N02-015 | W2C-4.1-C-015 | 4.1-backend-services | covered | The security verification flow executes node units through envelopes, returns operation failures without exception leakage, and exposes metrics that preserve node anchors. | ready_for_scheme_planner | none | 0.90 |
| W2C-N02-016 | W2C-4.1-D-016 | 4.1-backend-services | conflict | An implementation round is not accepted by a focused pass alone. Focused tests, full-suite tests, and traceable source-anchor verification are all required before acceptance. | send_to_scheme_planner_with_rejection_flag | verification_gate_arbiter | 0.87 |
| W2C-N02-017 | W2C-4.3-A-017 | 4.3-integration-and-provider-connectors | covered | Provider connector migration must wrap existing classes as swappable adapters before replacing internals. Replacing internals before adapter proof is unsafe. | ready_for_scheme_planner | none | 0.89 |
| W2C-N02-018 | W2C-4.3-B-018 | 4.3-integration-and-provider-connectors | boundary | Arbiter selection is provider-authority routing. Pair anchors choose which authority families review the result; selecting only the first matching provider authority drops required review coverage. | send_to_scheme_planner_with_boundary_flag | provider_authority_arbiter | 0.91 |
| W2C-N02-019 | W2C-4.3-C-019 | 4.3-integration-and-provider-connectors | boundary | Multi-provider arbiter evidence must preserve each selected authority and verdict reason. Compressing combined provider evidence into one reason collapses required connector evidence. | send_to_scheme_planner_with_boundary_flag | provider_authority_arbiter | 0.89 |
| W2C-N02-020 | W2C-4.3-D-020 | 4.3-integration-and-provider-connectors | conflict | Connector gates remain internal-first. Source anchors must be preserved, and external fallback before local proof is rejected. | send_to_scheme_planner_with_rejection_flag | internal_first_arbiter | 0.88 |

---

## Phrasing B Companions

Each companion changes the A surface wording while preserving the same B output
row from the pair index.

| pair_id | phrasing_b_prompt_fragment |
|---|---|
| W2C-N02-001 | The prior phase hands architecture decisions forward; transition constraints stay attached before implementation consumes the topology. |
| W2C-N02-002 | Lifecycle phase mapping, document ownership, and transition rules are registry topology facts, not database-storage facts. |
| W2C-N02-003 | Nested orchestrators generate child trace branches dynamically; the topology is recursive rather than fixed-depth. |
| W2C-N02-004 | Corpus rows must wait for WDA topology gates; using broad rows before proof creates premature promotion. |
| W2C-N02-005 | The logical unit boundary specifies typed request, typed response, result envelope, and cancellation behavior. |
| W2C-N02-006 | Flow identity, unit identity, confidence, evidence, and flags are part of the runtime API contract. |
| W2C-N02-007 | Router decisions must include the predicted target and the wrong-neighbor score so confidence can be audited. |
| W2C-N02-008 | Training output must be the concrete logical result; prose about the intended result is a malformed pair. |
| W2C-N02-009 | A public client without PKCE fails the current authorization rule; compatibility cannot override that rule. |
| W2C-N02-010 | IP-only throttling and missing logout hint are concerns unless a current MUST violation is also present. |
| W2C-N02-011 | Shared-secret signing plus missing expiry produces a compound token-safety failure that must keep both risks. |
| W2C-N02-012 | Present PKCE noncompliance wins over a future-release promise and must remain a reprocess-level issue. |
| W2C-N02-013 | Backend implementation advances one unit slot at a time and uses unchanged tests as the proof surface. |
| W2C-N02-014 | The assistant calls registered flows; mutation handling should not become hidden assistant branch logic. |
| W2C-N02-015 | Flow execution returns envelope failures and keeps metrics tied to each node source anchor. |
| W2C-N02-016 | A round requires focused, full-suite, and source-anchor verification before it is accepted. |
| W2C-N02-017 | Connector work starts with adapter wrapping so existing behavior remains swappable and testable. |
| W2C-N02-018 | Connector arbiter routing must keep all required provider authorities, not just the first match. |
| W2C-N02-019 | Combined provider evidence keeps separate authority reasons visible in the result. |
| W2C-N02-020 | Local connector proof is required before any external fallback path can be accepted. |

The Phrasing B output for every row is the same `LogicalUnitResult` shown in
the pair index.

---

## Gate Result

```text
20 node-02 original pairs: PASS
20 Phrasing B companions: PASS
Actual formal B statements: PASS
Coverage statuses differentiated: PASS
Handoff markers present: PASS
Required arbiter hints present where needed: PASS
```
