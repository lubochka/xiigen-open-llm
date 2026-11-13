# Layer 0-A — Compiler Pairs (054)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-054-001 — "waterfall" vocabulary: methodology vs delivery vs phase

**Input A:** "Use waterfall methodology." → []
**Input B:** "Manage waterfall delivery." → []
**Input C:** "Track waterfall phase progress." → []
**Input D:** "Design the waterfall phase structure." → ["ARCHITECTURE"] — "design" + "structure"
**Input E:** "Define waterfall delivery criteria." → ["DOCUMENT"] — "define" + "criteria"

"waterfall" = gap in all five. Predicates from co-occurring tokens only.

---

## B37-0A-054-002 — "benefits realization" with review predicate

**Input A:** "Validate benefits realization outcomes." → ["QA"] — "validate" fires QA
**Input B:** "Track benefits realization." → []
**Input C:** "Define benefits realization metrics." → ["DOCUMENT"]
**Input D:** "Build the benefits realization tracker." → ["IMPLEMENT"]

"benefits realization" = gap. Validate → QA (same rule as review → QA).

---

## B37-0A-054-003 — "KPI dashboard" + ARCHITECTURE combo

**Input A:** "Build the KPI dashboard." → ["IMPLEMENT"]
**Input B:** "Design the KPI dashboard architecture." → ["ARCHITECTURE"]
**Input C:** "Build and design the KPI dashboard architecture." → ["IMPLEMENT", "ARCHITECTURE"]
**Input D:** "Monitor KPI dashboard metrics." → []

"KPI dashboard" = gap. Predicates from build/design tokens.

---

## B37-0A-054-004 — "work breakdown structure" with all predicate types

**Input A:** "Maintain the work breakdown structure." → ["ARCHITECTURE"] — "structure" fires
**Input B:** "Create the work breakdown structure." → ["IMPLEMENT", "ARCHITECTURE"]
**Input C:** "Document the work breakdown structure." → ["DOCUMENT", "ARCHITECTURE"]
**Input D:** "Review the work breakdown structure." → ["QA", "ARCHITECTURE"]

"structure" fires ARCHITECTURE in ALL inputs. Additional predicates from co-occurring tokens.

---

## B37-0A-054-005 — "project closure" + "sign-off" combined

**Input:** "Manage project closure, obtain sign-off from the programme board, and track benefits realization."

Token scan:
- "Manage" → gap; "project closure" → gap
- "obtain" → gap; "sign-off" → gap; "programme board" → gap
- "track" → NOT TEST → gap; "benefits realization" → gap

Predicates: []
DomainId: "project-management"
SCL: emitted
