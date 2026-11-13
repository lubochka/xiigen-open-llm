# Layer 0-D — Flow Factory Pairs (032)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: flow-factory-pairs

---

## B37-0D-032-001 — Factory selection: multi-predicate combos (Round 3a + 4b)

"Define escalation process structure." → [DOCUMENT, ARCHITECTURE] → DocumentationFlow (2.1) + ArchitectureFlow31 (3.1)
"Create communication plan document." → [IMPLEMENT, DOCUMENT] → BackendServiceFlow41 (4.1) + DocumentationFlow (2.1)
"Design and build waterfall tracking architecture and service." → [ARCHITECTURE, IMPLEMENT] → ArchitectureFlow31 (3.1) + BackendServiceFlow41 (4.1)

---

## B37-0D-032-002 — Factory selection: triple predicate

"Define work breakdown structure, build tracking service, and document for stakeholders." → [DOCUMENT, ARCHITECTURE, IMPLEMENT] → DocumentationFlow (2.1) + ArchitectureFlow31 (3.1) + BackendServiceFlow41 (4.1)

All three parallel eligible.

---

## B37-0D-032-003 — Factory selection: API_CONTRACT in PM context

"Map value stream API dependencies between services." → [API_CONTRACT, IMPLEMENT] → APIContractFlow33 (3.3) + BackendServiceFlow41 (4.1)
"Resolve portfolio API integration dependencies." → [API_CONTRACT, IMPLEMENT] → APIContractFlow33 (3.3) + BackendServiceFlow41 (4.1)

"API" + "dependencies" → API_CONTRACT; "service/services" → IMPLEMENT. PM vocabulary (value stream, portfolio) → gap warnings only.

---

## B37-0D-032-004 — Factory selection: gap warnings forwarded alongside factory

**When predicate fires in PM context:**
- Factory: based on predicate
- DomainId: "software-development"
- CapabilityGapWarnings: gap IDs from PM vocabulary → STILL forwarded
- SCL: NOT emitted

Example: "Manage waterfall delivery and build the tracking service."
Predicate: [IMPLEMENT] → BackendServiceFlow41 (4.1)
Gap warnings: gap-project-managing-manage-delivery-001 (from "manage waterfall delivery")
SCL: NOT emitted

---

## B37-0D-032-005 — Factory selection: v6 corruptions for Rounds 3a + 4b

**v6-01 (invented factory):**
- "Run escalation process" → EscalationFlow — invented; correct: no factory (knowledge_edge)
- "Manage waterfall" → WaterfallFlow — invented; correct: no factory
- "Coordinate SAFe" → SAFeFlow — invented; correct: no factory
- "Track benefits realization" → BenefitsRealizationFlow — invented; correct: no factory

All four: correct output is knowledge_edge (no factory). v6-01 = selecting an invented PM-specific factory.
