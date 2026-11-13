# Layer 0-D — Flow Factory Pairs (036)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: flow-factory-pairs

---

## B37-0D-036-001 — Factory: final cross-round factory map (all rounds 002-060)

**Unified B37 factory selection (all rounds):**

Zero predicates → no factory selected → knowledge_edge
[DOCUMENT] → DocumentationFlow (2.1)
[ARCHITECTURE] → ArchitectureFlow31 (3.1)
[IMPLEMENT] → BackendServiceFlow41 (4.1)
[QA] → QA sub-phase
[API_CONTRACT] → APIContractFlow33 (3.3)
[DOCUMENT, ARCHITECTURE] → 2.1 + 3.1
[DOCUMENT, IMPLEMENT] → 2.1 + 4.1
[ARCHITECTURE, IMPLEMENT] → 3.1 + 4.1
[API_CONTRACT, IMPLEMENT] → 3.3 + 4.1
[DOCUMENT, ARCHITECTURE, IMPLEMENT] → 2.1 + 3.1 + 4.1

---

## B37-0D-036-002 — Factory: certification exam (Round 4b)

Q1: "Monitor KPI." → Factory? A: no factory
Q2: "Build KPI dashboard service." → A: BackendServiceFlow41 (4.1)
Q3: "Manage project closure." → A: no factory
Q4: "Maintain work breakdown structure." → A: ArchitectureFlow31 (3.1)
Q5: "Track benefits realization." → A: no factory
Q6: "Document benefits for stakeholders." → A: DocumentationFlow (2.1)
Q7: "Map value streams." → A: no factory
Q8: "Design value stream architecture." → A: ArchitectureFlow31 (3.1)
Q9: "Run PI planning." → A: no factory
Q10: "Build PI tracking service." → A: BackendServiceFlow41 (4.1)

---

## B37-0D-036-003 — Factory: "WBS" abbreviation → no factory; "work breakdown structure" → 3.1

This boundary is the single most important factory exception in Round 4b.
"Maintain WBS." → no factory (knowledge_edge)
"Maintain work breakdown structure." → ArchitectureFlow31 (3.1) (ARCHITECTURE fires on "structure")

No other Round 4b term self-triggers a factory.

---

## B37-0D-036-004 — Factory: gap warnings and factory output are independent

**Independence rule:**
CapabilityGapWarnings (gap IDs) are forwarded regardless of factory selection.
When factory IS selected (predicate fired): gap warnings forwarded alongside factory output
When NO factory (zero predicates): gap warnings forwarded + SCL emitted

Gap warnings never influence factory selection. Factory selection never suppresses gap warnings.

---

## B37-0D-036-005 — Layer 0-D complete: 35 files (002-036)

Layer 0-D flow factory training complete for all B37 rounds.
Core principle: factory selection = predicate-determined, never PM-intent-determined.
Four real factories only. No PM-specific factories exist or will be created.
"work breakdown structure" is the sole Round 4b self-triggering exception.
