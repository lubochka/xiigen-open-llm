# Layer 0-D — Flow Factory Pairs (008)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: flow-factory-pairs

---

## B37-0D-008-001 — probationary_rejected: project-managing flow candidate fails Gate B

**Hypothetical candidate: ProjectManagingFlow**

Gate B definition: "ProjectManagingFlow correctly routes all B37 pure managing requests to managing-appropriate responses."

Gate B test 1: "Track and coordinate delivery blockers." → Expected: manages tracking and coordination. Actual: no registered predicate; cannot enter flow.

Gate B result: FAIL — The flow cannot be entered without a predicate. B37 managing vocabulary has no registered predicate. A ProjectManagingFlow cannot be accessed by any managing request.

Status: probationary_rejected — the flow candidate fails Gate B because it has no lexical entry point.

**Key learning:** A flow without a predicate entry point is unreachable. To create a managing flow, a predicate would first need to be registered for managing vocabulary (e.g., TRACK, COORDINATE). This would be a capability addition, not a flow factory addition.

---

## B37-0D-008-002 — Why no managing flow can exist without predicate registration

**The factory selection chain:**
1. Request arrives
2. Compiler scans tokens against predicate lexicons
3. Matching tokens → predicates emitted
4. Predicates → factory selection
5. Factory → flow instantiation

**Without step 3 (no predicate):**
Steps 4 and 5 are unreachable. The factory is never consulted. No flow is selected.

**To create a managing flow:**
1. Register TRACK predicate in lexicon (or COORDINATE, MANAGE, etc.)
2. Add factory mapping: TRACK → TrackingFlow
3. Create TrackingFlow with appropriate nodes

This requires a capability addition (new predicate + new flow). It cannot be done by the factory alone.

---

## B37-0D-008-003 — SCHEDULE_CAPABILITY_LEARNING as the interim factory output

For zero-predicate B37 requests, SCHEDULE_CAPABILITY_LEARNING serves as the factory output.

**SCHEDULE_CAPABILITY_LEARNING payload:**
- gap_ids: [list of gap IDs from the request]
- domain: "project-management"
- scenario: "B37 — Project managing"
- request_text: [original request]
- timestamp: [current time]
- batch: "B37-batch-001"

**Purpose:** This event is consumed by the learning pipeline to accumulate evidence for potential future predicate registration for managing vocabulary.

---

## B37-0D-008-004 — Factory map: B37-specific common patterns

**Most common B37 factory patterns:**

1. Pure managing request → knowledge_edge (no factory) [most common]
2. "stakeholder" in sentence → DocumentationFlow [most common predicate]
3. "structure" in governance sentence → ArchitectureFlow31 + DocumentationFlow [second most common]
4. "review" in managing sentence → ReviewFlow [third most common]
5. "build" or "service" or "module" → BackendServiceFlow41 [fourth most common]
6. "test" in estimation sentence → TestFlow5x [occasional]
7. "API dependencies" → APIContractFlow33 [occasional]

---

## B37-0D-008-005 — Factory output completeness check

**For every B37 request, the factory output must include:**

For zero-predicate requests:
- [x] Factory selection: None
- [x] Route: knowledge_edge
- [x] DomainId: "project-management"
- [x] Gap IDs: [all gap vocabulary mapped]
- [x] SCHEDULE_CAPABILITY_LEARNING: emitted

For predicate requests:
- [x] Factory selection: [matching factory per predicate]
- [x] Route: [sub-phase per predicate]
- [x] DomainId: "software-development"
- [x] Gap IDs: [gap vocabulary mapped — if any]
- [x] SCHEDULE_CAPABILITY_LEARNING: NOT emitted
- [x] Gap warnings forwarded to factory context
