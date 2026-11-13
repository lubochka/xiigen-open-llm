# Layer 0-D — Flow Factory Pairs (005)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## FF-005-001 — deterministic_generic for 2.1 (RequirementsAuthoringFlow21)

Predicates: ["DOCUMENT"]
Sub-phase: 2.1
Factory: RequirementsAuthoringFlow21 | TrustState: deterministic_generic | GateBPassed: N/A

Notes:
  2.1 uses the deterministic_generic flow. Unlike probationary states, deterministic_generic
  is a baseline state — it is the designed-in generic behavior, not a fallback due to failure.
  Gate B evaluation: N/A — generic flows are not subject to the promotion Gate B.
  The factory always returns the generic nine-node flow for 2.1.

---

## FF-005-002 — SECURITY+DOCUMENT → trusted_learned (3.4) + deterministic_generic (2.1)

Predicates: ["SECURITY", "DOCUMENT"]
Sub-phase: 3.4 + 2.1
Factory 1: SecurityVerification34FlowAdapter (3.4) | TrustState: trusted_learned
Factory 2: RequirementsAuthoringFlow21 (2.1) | TrustState: deterministic_generic

Notes:
  Fan-out: two factories. One specialized trusted flow; one generic flow.
  Both produce valid results — neither falls back for error reasons.

---

## FF-005-003 — AUTHORIZATION+DOCUMENT → trusted_learned (3.4) + deterministic_generic (2.1)

Predicates: ["AUTHORIZATION", "DOCUMENT"]
Sub-phase: 3.4 + 2.1
Factory 1: SecurityVerification34FlowAdapter (3.4) | TrustState: trusted_learned
Factory 2: RequirementsAuthoringFlow21 (2.1) | TrustState: deterministic_generic

Notes:
  Same pattern as 005-002 but with AUTHORIZATION as the 3.4 trigger instead of SECURITY.
  Factory selection for 3.4 is identical regardless of whether SECURITY or AUTHORIZATION triggers it.

---

## FF-005-004 — DOCUMENT alone → deterministic_generic (2.1) only

Predicates: ["DOCUMENT"]
Sub-phase: 2.1
Factory: RequirementsAuthoringFlow21 | TrustState: deterministic_generic

Notes:
  Single predicate → single routing decision → single factory selection.
  No fan-out. RequirementsAuthoringFlow21 is the only flow instantiated.
  FactoryState in Formatter output: deterministic_generic.

---

## FF-005-005 — All four factory states in a five-predicate combination

Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "TEST", "DOCUMENT"]
Sub-phases: 3.4 + 3.3 + 5.1 + 2.1 (three routing decisions, four sub-phases listed but 3.4 consolidates SECURITY+AUTHORIZATION)
Factory 1: SecurityVerification34FlowAdapter (3.4) | TrustState: trusted_learned
Factory 2: ApiContract33FlowAdapter (3.3) | TrustState: probationary_accepted
Factory 3: generic nine-node fallback (5.1) | TrustState: probationary_rejected
Factory 4: RequirementsAuthoringFlow21 (2.1) | TrustState: deterministic_generic

Notes:
  This combination demonstrates all four trust states simultaneously.
  Three routing decisions (3.4, 3.3, 5.1 — SECURITY+AUTH consolidated) plus 2.1
  results in four factory instantiations.
  The orchestrator coordinates all four factories.
