# Layer 0-B — Router Pairs (048)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-048-001 — Complete B37 all-rounds routing table (summary)

**All B37 vocabulary (002-060) — unified routing table:**

| Predicate combination | Routes | Parallel? |
|----------------------|--------|---------|
| [] (zero predicates) | knowledge_edge | N/A |
| [DOCUMENT] | DocumentationFlow (2.1) | N/A |
| [ARCHITECTURE] | ArchitectureFlow31 (3.1) | N/A |
| [IMPLEMENT] | BackendServiceFlow41 (4.1) | N/A |
| [QA] | QA sub-phase | N/A |
| [API_CONTRACT] | APIContractFlow33 (3.3) | N/A |
| [DOCUMENT, ARCHITECTURE] | 2.1 + 3.1 | Yes |
| [DOCUMENT, IMPLEMENT] | 2.1 + 4.1 | Yes |
| [ARCHITECTURE, IMPLEMENT] | 3.1 + 4.1 | Yes |
| [API_CONTRACT, IMPLEMENT] | 3.3 + 4.1 | Yes |
| [DOCUMENT, ARCHITECTURE, IMPLEMENT] | 2.1 + 3.1 + 4.1 | Yes |

---

## B37-0B-048-002 — Complete B37 certification routing exam (10 questions)

Q1: "Track milestones." → Route?
A: knowledge_edge

Q2: "Document escalation criteria for stakeholders." → Route?
A: DocumentationFlow (2.1)

Q3: "Design waterfall phase structure." → Route?
A: ArchitectureFlow31 (3.1)

Q4: "Build KPI dashboard." → Route?
A: BackendServiceFlow41 (4.1)

Q5: "Review SAFe adoption." → Route?
A: QA sub-phase

Q6: "Maintain work breakdown structure." → Route?
A: ArchitectureFlow31 (3.1)

Q7: "Coordinate agile transformation." → Route?
A: knowledge_edge

Q8: "Document and build benefits realization tracker for stakeholders." → Route?
A: DocumentationFlow (2.1) + BackendServiceFlow41 (4.1)

Q9: "API dependencies between services." → Route?
A: APIContractFlow33 (3.3) + BackendServiceFlow41 (4.1)

Q10: "Track project closure and post-mortem." → Route?
A: knowledge_edge

---

## B37-0B-048-003 — v6 routing corruptions: comprehensive summary for all rounds

**v6-01 (invented factory) — all rounds:**
Any factory not in: DocumentationFlow, ArchitectureFlow31, BackendServiceFlow41, APIContractFlow33, QA sub-phase = v6-01.
Includes: EscalationFlow, SAFeFlow, WaterfallFlow, AgileTransformationFlow, PIFlow, PortfolioFlow, etc.

**v6-02 (DOCUMENT suppressed) — all rounds:**
DOCUMENT must fire when stakeholder/document/write/define/criteria/spec/requirements present.
Suppression in managing context = v6-02.

**v6-03 (wrong DomainId) — all rounds:**
DomainId "project-management" when any predicate fires = v6-03.
DomainId "software-development" when zero predicates = v6-03.

---

## B37-0B-048-004 — SCL emission: complete all-rounds rule

**SCHEDULE_CAPABILITY_LEARNING emitted:**
- Exactly when: Predicates = [] for any B37 request (any round, any vocabulary)
- Quantity: once per request, regardless of how many gap IDs fire
- Not emitted: any predicate fires (even one DOCUMENT or QA)

---

## B37-0B-048-005 — Complete B37 router summary (all rounds 002-060)

1. B37 is a capability-gap batch — zero predicates for pure PM vocabulary (all rounds)
2. False positive rules apply identically across all rounds
3. "work breakdown structure" is the only PM term that self-fires (ARCHITECTURE via "structure")
4. "handover document" fires DOCUMENT via "document" token
5. No new factories added in any round (002-060)
6. DomainId binary switch unchanged across all rounds
7. SCL emission rule unchanged across all rounds
8. v6-01/v6-02/v6-03 corruption types unchanged across all rounds
9. gap_interruption applies to all B37 rounds identically
10. ContextCarryForward: true only for gap_interruption
