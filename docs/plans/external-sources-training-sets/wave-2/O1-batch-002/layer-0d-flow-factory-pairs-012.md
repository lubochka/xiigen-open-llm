# Layer 0-D — Flow Factory Pairs (012)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## O1b-0D-012-001 — Complete factory map: sub-phase to adapter

A: O1b complete factory map

B: | Sub-phase | Predicate(s) | Adapter | State | Fallback |
   |-----------|-------------|---------|-------|---------|
   | 3.4 | SECURITY, AUTHORIZATION | SecurityVerification34FlowAdapter | trusted_learned | No |
   | 3.3 | API_CONTRACT | ApiContract33FlowAdapter | deterministic_generic | No |
   | 4.1 | IMPLEMENT | UniversalNineNodeDevelopmentFlow | deterministic_generic | No |
   | 5.1 | TEST | UnitTestingFlow51 → GenericFallbackFlow | probationary_rejected | Yes |
   | 2.1 | DOCUMENT | DocumentationFlow21 | deterministic_generic | No |

---

## O1b-0D-012-002 — Factory state definitions

A: O1b factory state definitions

B: trusted_learned: Specialized adapter, Gate B passed, production-ready. deterministic_generic: Standard generic flow, no specialization, no Gate B required. probationary_accepted: Specialized adapter, Gate B passed in current session, ready to use. probationary_rejected: Specialized adapter, Gate B failed, generic fallback active. Fallback: Generic flow substituted when specialized adapter is probationary_rejected.

---

## O1b-0D-012-003 — Factory map: v6-01 prevention

A: v6-01 prevention rule

B: RULE: Never invent adapter names. The only valid adapters for O1b are: SecurityVerification34FlowAdapter, ApiContract33FlowAdapter, UniversalNineNodeDevelopmentFlow, UnitTestingFlow51, DocumentationFlow21, GenericFallbackFlow. Any other name is v6-01 corruption.

---

## O1b-0D-012-004 — Factory map: knowledge_edge has no factory

A: What factory is selected for knowledge_edge routing?

B: NONE. knowledge_edge transitions produce zero routing decisions and zero factory selections. No adapter is instantiated. No flow runs. This is the correct behavior — knowledge_edge means the prompt cannot be routed.

---

## O1b-0D-012-005 — Complete factory selection algorithm

A: O1b factory selection algorithm

B: Algorithm: (1) Receive routing decisions (list of distinct sub-phases). (2) For each sub-phase, look up canonical adapter and state in factory map. (3) If state is probationary_rejected, substitute GenericFallbackFlow and set fallback: true. (4) Return FlowSelections ordered: trusted_learned first, then deterministic_generic (sub-phase order), then probationary_accepted, then probationary_rejected last. (5) Zero routing decisions → zero factory selections.
