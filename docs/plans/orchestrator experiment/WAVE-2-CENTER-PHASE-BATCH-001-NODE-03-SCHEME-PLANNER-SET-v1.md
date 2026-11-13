# Wave 2 Center Phase Batch 001 - Node 03 Scheme Planner Set v1
# Date: 2026-05-05

---

## Purpose

Add node-03 scheme-planner coverage for the five center sub-phase groups in
Wave 2 Center Phase Batch 001.

Node 03 receives the translated statements for one run as a set. It does not
receive one statement at a time. Its B output is the actual ordered plan with
dependencies, parallel groups, boundary paths, and rejection paths.

---

## Common Runtime Shape

```text
A:
  SchemePlannerInput
    run_id: <plan id>
    phase_id: <phase>
    sub_phase_id: <sub-phase>
    translated_statements: [node-02 outputs for the group]
    handoff_markers: [node-02 markers]
    required_arbiters_hints: [node-02 arbiter hints]

B:
  LogicalUnitResult<SchemePlan>
    unit_id: unit-scheme-planner-center-phase-v1
    unit_role: scheme-planner
    domain_tag: <sub-phase id>
    output:
      plan_id: <plan id>
      steps: <actual ordered steps>
      dependency_edges: <actual dependency edges>
      parallel_groups: <actual parallel groups>
      blocked_or_rejected_paths: <actual blocked paths>
      knowledge_edges: []
    confidence: <score>
```

---

## Pair Index

| pair_id | A.translated_statement_ids | B.plan_id | B.steps | B.dependency_edges | B.parallel_groups | B.blocked_or_rejected_paths | B.confidence |
|---|---|---|---|---|---|---|---:|
| W2C-N03-001 | W2C-N02-001, W2C-N02-002, W2C-N02-003, W2C-N02-004 | plan-3.1-architecture-topology | s01 preserve phase-output context; s02 register lifecycle topology contract; s03 model recursive orchestrator trace generation; s04 reject premature corpus promotion until WDA gates pass | s01 -> s02 because context categories define registry obligations; s02 -> s03 because recursion extends registered topology; s04 depends on s01,s02,s03 because promotion is valid only after topology obligations are proven | s01 and s02 may be prepared together; s03 waits for s02; s04 is a final gate | s04 rejects premature corpus promotion | 0.90 |
| W2C-N03-002 | W2C-N02-005, W2C-N02-006, W2C-N02-007, W2C-N02-008 | plan-3.3-api-contract | s01 define typed logical-unit boundary; s02 preserve runtime flags and evidence fields; s03 require router output audit fields; s04 reject descriptive B outputs | s01 -> s02 because runtime fields attach to the contract; s02 -> s03 because router outputs must expose those fields; s04 depends on s01,s02,s03 because malformed pairs violate the whole contract | s01 and s02 may start together; s03 follows s02; s04 is a validation gate | s04 rejects malformed descriptive pair output | 0.91 |
| W2C-N03-003 | W2C-N02-009, W2C-N02-010, W2C-N02-011, W2C-N02-012 | plan-3.4-security-authorization | s01 enforce public-client PKCE as reprocess path; s02 preserve concern-level throttling and logout gaps without false escalation; s03 preserve compound token-safety failure; s04 reject future-release exception for current PKCE MUST | s01 and s04 share the PKCE requirement; s02 is a parallel concern path; s03 is an independent token-safety rejection path; final review depends on s01,s02,s03,s04 | s01, s02, and s03 may start together; s04 joins the PKCE branch before final review | s01 rejects missing PKCE; s03 rejects compound token risk; s04 rejects future-release exception | 0.93 |
| W2C-N03-004 | W2C-N02-013, W2C-N02-014, W2C-N02-015, W2C-N02-016 | plan-4.1-backend-services | s01 implement one logical unit presence; s02 keep assistant routing through registered flows; s03 execute flow units through operation envelopes and node metrics; s04 require focused, full-suite, and source-anchor verification | s01 -> s02 because assistant routing uses registered unit presence; s02 -> s03 because flow execution follows assistant selection; s04 depends on s01,s02,s03 because verification covers the complete implementation path | s01 and s02 may be prepared together; s03 follows s02; s04 is final acceptance | s04 rejects focused-only acceptance | 0.90 |
| W2C-N03-005 | W2C-N02-017, W2C-N02-018, W2C-N02-019, W2C-N02-020 | plan-4.3-integration-provider-connectors | s01 wrap existing providers as adapters before replacement; s02 select all required provider-authority arbiters; s03 preserve multi-provider evidence per authority; s04 reject external fallback before internal proof | s01 -> s02 because adapter proof establishes provider authority; s02 -> s03 because selected authorities define evidence preservation; s04 depends on s01,s02,s03 because internal proof is required before fallback | s01 and s02 may start together; s03 waits for s02; s04 is final gate | s04 rejects external-first execution | 0.91 |

---

## Phrasing B Companions

Each companion changes the translated statement set wording while preserving
the same plan output.

| pair_id | phrasing_b_statement_set |
|---|---|
| W2C-N03-001 | Architecture context, lifecycle registry obligations, recursive runtime traces, and the WDA promotion gate are presented with different wording but the same dependency order. |
| W2C-N03-002 | Typed unit contracts, runtime evidence fields, router audit fields, and malformed-pair rejection are restated as API-boundary obligations. |
| W2C-N03-003 | PKCE enforcement, concern-level gaps, compound token risk, and future-release rejection are restated as security-model planning statements. |
| W2C-N03-004 | Single-unit implementation, registered assistant flow selection, envelope-based execution, and full verification are restated as backend service planning statements. |
| W2C-N03-005 | Adapter-first connector proof, multi-authority arbiter selection, evidence preservation, and internal-first fallback rejection are restated as connector planning statements. |

The Phrasing B output for each row is the same `SchemePlan` shown in the pair
index.

---

## Gate Result

```text
5 node-03 original pairs: PASS
5 Phrasing B companions: PASS
Actual ordered plans: PASS
Dependencies explicit: PASS
Parallel groups explicit: PASS
Rejected paths explicit: PASS
Knowledge edges: none for this batch
```
