# Layer 0-B — Router Pairs (012)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: router

---

## O1b-0B-012-001 — Complete routing table: single-predicate cases

A: O1b single-predicate routing table

B: | Predicate | Sub-phase | Flow | State |
   |-----------|-----------|------|-------|
   | SECURITY | 3.4 | SecurityVerification34FlowAdapter | trusted_learned |
   | AUTHORIZATION | 3.4 | SecurityVerification34FlowAdapter | trusted_learned |
   | API_CONTRACT | 3.3 | ApiContract33FlowAdapter | deterministic_generic |
   | IMPLEMENT | 4.1 | UniversalNineNodeDevelopmentFlow | deterministic_generic |
   | TEST | 5.1 | UnitTestingFlow51 (fallback) | probationary_rejected |
   | DOCUMENT | 2.1 | DocumentationFlow21 | deterministic_generic |
   | QA | (none) | — | — |
   | ARCHITECTURE | arch | ArchitectureFlow | deterministic_generic |

---

## O1b-0B-012-002 — Complete routing table: common multi-predicate cases

A: O1b multi-predicate routing table

B: | Predicates | Routing Decisions |
   |-----------|-------------------|
   | SECURITY+AUTHORIZATION | 3.4 (dedup) |
   | SECURITY+API_CONTRACT | 3.4+3.3 |
   | SECURITY+AUTHORIZATION+API_CONTRACT | 3.4+3.3 |
   | SECURITY+IMPLEMENT | 3.4+4.1 |
   | SECURITY+AUTHORIZATION+IMPLEMENT | 3.4+4.1 |
   | SECURITY+AUTHORIZATION+API_CONTRACT+IMPLEMENT | 3.4+3.3+4.1 |
   | SECURITY+TEST | 3.4+5.1 |
   | SECURITY+DOCUMENT | 3.4+2.1 |
   | IMPLEMENT+TEST | 4.1+5.1 |
   | DOCUMENT+API_CONTRACT | 2.1+3.3 |

---

## O1b-0B-012-003 — Routing rule: knowledge_edge produces no routing

A: TransitionType: knowledge_edge (any predicates: none)

B: RoutingDecisions: [] — knowledge_edge always means no routing decisions, no flow selection, no nine-node pipeline execution.

---

## O1b-0B-012-004 — Routing rule: dedup axiom restated

A: O1b routing dedup rule

B: Count of routing decisions = count of DISTINCT sub-phase targets. SECURITY and AUTHORIZATION share sub-phase 3.4. Any number of predicates mapping to 3.4 produces exactly one 3.4 routing decision.

---

## O1b-0B-012-005 — Routing rule: 3.4 state is always trusted_learned

A: What factory state does sub-phase 3.4 use in O1b?

B: Sub-phase 3.4 always uses state: trusted_learned with adapter: SecurityVerification34FlowAdapter. This applies for SECURITY alone, AUTHORIZATION alone, or SECURITY+AUTHORIZATION together. The state does not change based on which predicate triggered the 3.4 routing.
