# Layer 0-A — Compiler Pairs (058)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-058-001 — "KPI dashboard" + UI fires when UI explicitly present

**Input A:** "Build the KPI dashboard." → ["IMPLEMENT"] — "build" fires
**Input B:** "Build the KPI dashboard UI." → ["IMPLEMENT"] — "build" fires; "UI" fires if registered
**Input C:** "Monitor the KPI dashboard." → [] — monitoring alone → gap
**Input D:** "Review the KPI dashboard." → ["QA"] — "review" fires

"KPI dashboard" = gap. Predicate from co-occurring action token.

---

## B37-0A-058-002 — "agile transformation" + "SAFe" + "waterfall" — full round combined

**Input:** "Transition from waterfall delivery through agile transformation to SAFe at scale."

Token scan: all gap vocabulary.
Predicates: []
DomainId: "project-management"
SCL: emitted

---

## B37-0A-058-003 — "benefits realization" + "project closure" + "portfolio management"

**Input:** "Track benefits realization, manage project closure, and oversee portfolio management."

Token scan: all gap vocabulary.
Predicates: []
DomainId: "project-management"
SCL: emitted

---

## B37-0A-058-004 — Three-predicate combination in PM context

**Input:** "Define work breakdown structure, build the tracking service, and document the plan for stakeholders."

Token scan:
- "Define" → DOCUMENT fires
- "work breakdown structure" → "structure" fires ARCHITECTURE
- "build" → IMPLEMENT fires
- "tracking service" → "service" fires IMPLEMENT (already fired)
- "document" → DOCUMENT fires (already fired)
- "plan" → gap; "stakeholders" → DOCUMENT fires (already fired)

Predicates: ["DOCUMENT", "ARCHITECTURE", "IMPLEMENT"]
DomainId: "software-development"

---

## B37-0A-058-005 — Three-predicate combination routing

**From 058-004:**
DOCUMENT → DocumentationFlow (2.1)
ARCHITECTURE → ArchitectureFlow31 (3.1)
IMPLEMENT → BackendServiceFlow41 (4.1)

All three parallel eligible.
DomainId: "software-development"
SCL: NOT emitted
GapIds: manage-delivery-001, work-stream-001 forwarded (from "tracking" + "plan")
