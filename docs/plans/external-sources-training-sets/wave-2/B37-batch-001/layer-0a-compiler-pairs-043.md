# Layer 0-A — Compiler Pairs (043)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-043-001 — "KPI dashboard" fires IMPLEMENT when built

**Input A:** "Monitor the KPI dashboard."
- "Monitor" → gap
- "KPI dashboard" → gap; "KPI" alone → gap; "dashboard" alone → gap
- Predicates: []
- DomainId: "project-management"

**Input B:** "Build the KPI dashboard for the programme."
- "Build" → IMPLEMENT fires
- "KPI dashboard" → gap
- "programme" → gap

Predicates: ["IMPLEMENT"]
DomainId: "software-development"

**Input C:** "Build the KPI dashboard UI for stakeholders."
- "Build" → IMPLEMENT fires
- "UI" → UI fires (if UI predicate registered)
- "KPI dashboard" → gap
- "stakeholders" → DOCUMENT fires

Predicates: ["IMPLEMENT", "DOCUMENT"] (UI fires if registered; otherwise ["IMPLEMENT", "DOCUMENT"])
DomainId: "software-development"

---

## B37-0A-043-002 — Complete DomainId reference for 049-060 vocabulary

**Zero-predicate DomainId "project-management":**
- waterfall methodology alone
- agile transformation alone (no "define")
- SAFe coordination alone (no "implement")
- program increment alone (no "define")
- portfolio management alone
- value stream alone (no "design")
- flow state alone
- project closure alone
- benefits realization alone
- KPI monitoring alone (no "build")

**DomainId "software-development" immediately when:**
- "design waterfall" → ARCHITECTURE
- "define agile/program-increment" → DOCUMENT
- "implement SAFe" → IMPLEMENT
- "work breakdown structure" → ARCHITECTURE (on "structure")
- "build KPI dashboard" → IMPLEMENT

---

## B37-0A-043-003 — "work breakdown structure" (WBS) + DOCUMENT combo

**Input:** "Document the work breakdown structure for stakeholders."

Token scan:
- "Document" → DOCUMENT fires
- "work breakdown structure" → "structure" fires ARCHITECTURE
- "stakeholders" → DOCUMENT fires

Multiple DOCUMENT triggers → ONE DOCUMENT predicate
Predicates: ["DOCUMENT", "ARCHITECTURE"]
DomainId: "software-development"

---

## B37-0A-043-004 — "program increment" planning vocabulary scan

**Input A:** "Run the program increment planning ceremony."
- "Run" → gap
- "program increment planning" → gap
- "ceremony" → gap
- Predicates: []
- DomainId: "project-management"

**Input B:** "Define program increment goals and document acceptance criteria."
- "Define" → DOCUMENT fires
- "program increment goals" → gap
- "document" → DOCUMENT fires
- "acceptance criteria" → "criteria" fires DOCUMENT

Multiple DOCUMENT → ONE DOCUMENT
Predicates: ["DOCUMENT"]
DomainId: "software-development"

---

## B37-0A-043-005 — "portfolio management" with predicate co-occurrence

**Input A:** "Track portfolio management metrics." → Predicates: []
**Input B:** "Design the portfolio management structure." → ["ARCHITECTURE"]
**Input C:** "Document portfolio management KPIs for stakeholders." → ["DOCUMENT"]
**Input D:** "Build the portfolio management dashboard." → ["IMPLEMENT"]

All four confirm: "portfolio management" = gap vocabulary. The predicate comes from the co-occurring token, not from "portfolio management" itself.
