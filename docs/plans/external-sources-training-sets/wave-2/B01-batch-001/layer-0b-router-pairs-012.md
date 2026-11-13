# Layer 0-B — Router Pairs (012)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: router

---

## B01-0B-012-001 — Complete B01 routing decision table (canonical reference)

```
source_id: xiigen-b01-0b-012-table
source_title: XIIGen B01 router-012 — complete routing table reference
source_license_state: first_party
pair_kind: router
coverage_status: covered
```

### Complete B01 sub-phase ownership table

| Sub-phase | Name | Predicate owner | Phase |
|---|---|---|---|
| 1.1 | Screen Flow and UX Definition | UI | 1 |
| 1.2 | Wireframes and Prototypes | UI (specialized) | 1 |
| 1.3 | Design System | UI (specialized) | 1 |
| 1.4 | Accessibility | UI (specialized) | 1 |
| 1.5 | UI Testing | UI (specialized) | 1 |
| 2.1 | Business Requirements and Use Cases | DOCUMENT | 2 |
| 2.4 | Stakeholder Requirements | DOCUMENT (stakeholder context) | 2 |
| 3.1 | System Architecture | ARCHITECTURE | 3 |
| 4.1 | Implementation | IMPLEMENT | 4 |

Note on 1.2 through 1.5: In B01, all UI-predicate signals route to 1.1 as the
primary sub-phase. Sub-phases 1.2 through 1.5 are secondary routing targets
that require a more specific signal — they are not fired by the general UI
predicate alone in this batch.

---

## B01-0B-012-002 — RoutingDecision field schema (canonical)

```
source_id: xiigen-b01-0b-012-schema
source_title: XIIGen B01 router-012 — canonical RoutingDecision field schema
source_license_state: first_party
pair_kind: router
coverage_status: covered
```

### Canonical RoutingDecision schema for B01

Required fields (must always be present):
  DocumentId          — from the input SubPhaseRouterSourceDocument
  SourceStatus        — "ready" when routing succeeds; "knowledge_edge: ..." otherwise
  PredictedPhaseId    — phase number as string ("1", "2", "3", "4")
  PredictedSubPhaseId — sub-phase number as string ("1.1", "2.1", "3.1", "4.1")
  Confidence          — floating point (0.70 minimum for a matched predicate)
  NearestWrongSubPhaseScore — floating point; always lower than Confidence
  EvidencePredicates  — array of exactly the predicates that drove THIS decision
  KnowledgeEdge       — boolean (false for routed decisions; true for no-match)
  FailureClassification — null when routing succeeds; error code when failed

Prohibited fields (EM-9 guard — must never appear):
  dependency_order
  parallel_group
  domain_label
  is_primary

---

## B01-0B-012-003 — Three v6 corruption types at routing stage: summary

```
source_id: xiigen-b01-0b-012-corruption-summary
source_title: XIIGen B01 router-012 — v6 corruption summary at routing stage
source_license_state: first_party
pair_kind: router
coverage_status: covered
```

### v6 corruption type summary (routing stage)

v6-01 — UI predicate dropped at routing stage:
  Symptom: CompiledPredicateIds contains UI, but no 1.1 RoutingDecision produced.
  Cause: Router incorrectly skips UI routing; only routes DOCUMENT or nothing.
  Effect: ScreenInventoryFlow11 never invoked; screen mapping work not executed.
  Detection: DecisionCount < expected count; 1.1 missing from sub-phase list.

v6-02 — DOCUMENT suppressed when UI fires:
  Symptom: CompiledPredicateIds contains both UI and DOCUMENT; only 1.1 produced.
  Cause: Router treats UI as dominant and drops DOCUMENT routing.
  Effect: 2.1 specification work never invoked; requirements document not produced.
  Detection: DecisionCount == 1 when two predicates were compiled; 2.1 missing.

v6-03 — Wrong sub-phase for UI predicate:
  Symptom: UI predicate routes to 2.1 instead of 1.1.
  Cause: Router confuses UI and DOCUMENT routing targets.
  Effect: BusinessRequirementsFlow21 runs for a screen mapping request.
  Detection: PredictedSubPhaseId == "2.1" with EvidencePredicates == ["UI"].

All three corruptions are detectable at the RoutingDecision array level.
The correct response to detecting any v6 corruption is to reject the
RoutingDecision and re-run the router with strict predicate-to-sub-phase mapping.
