# Layer 0-D — Flow Factory Pairs (025)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: flow-factory-pairs

---

## B37-0D-025-001 — Round 2 factory selection: zero predicates → no factory

**Zero-predicate requests (Round 2 vocabulary) → no factory selected:**

"Maintain the Gantt chart." → Predicates: [] → knowledge_edge → NO factory selected
"Manage resource allocation." → Predicates: [] → knowledge_edge → NO factory selected
"Run capacity planning." → Predicates: [] → knowledge_edge → NO factory selected

Zero predicates = no flow factory. This is the baseline for all Round 2 vocabulary.

---

## B37-0D-025-002 — Round 2 factory selection: DOCUMENT fires → DocumentationFlow

"Document the status report format for stakeholders." → ["DOCUMENT"] → DocumentationFlow (2.1)
"Define progress update criteria for stakeholders." → ["DOCUMENT"] → DocumentationFlow (2.1)
"Describe the milestone tracking process." → ["DOCUMENT"] → DocumentationFlow (2.1)

All three: DocumentationFlow (2.1) selected. DomainId: "software-development".

---

## B37-0D-025-003 — Round 2 factory selection: ARCHITECTURE fires → ArchitectureFlow31

"Design the resource allocation structure." → ["ARCHITECTURE"] → ArchitectureFlow31 (3.1)
"Design the capacity planning architecture." → ["ARCHITECTURE"] → ArchitectureFlow31 (3.1)
"Define the dependency management architecture structure." → ["DOCUMENT", "ARCHITECTURE"] → DocumentationFlow (2.1) + ArchitectureFlow31 (3.1)

---

## B37-0D-025-004 — Round 2 factory selection: IMPLEMENT fires → BackendServiceFlow41

"Build the Gantt chart service." → ["IMPLEMENT"] → BackendServiceFlow41 (4.1)
"Create the resource allocation module." → ["IMPLEMENT"] → BackendServiceFlow41 (4.1)
"Implement the SLA tracking service." → ["IMPLEMENT"] → BackendServiceFlow41 (4.1)

All three: BackendServiceFlow41 (4.1). "build"/"create"/"implement" + "service"/"module" = IMPLEMENT.

---

## B37-0D-025-005 — Round 2 absent factories

**Factories that do NOT exist for Round 2 vocabulary:**
GanttChartFlow, MilestoneTrackingFlow, ResourceAllocationFlow, CapacityPlanningFlow, DependencyMappingFlow, CriticalPathFlow, BudgetTrackingFlow, VendorManagementFlow, SLATrackingFlow, StatusReportFlow, ProgressUpdateFlow

None exist. Any output naming these = v6-01 corruption.
