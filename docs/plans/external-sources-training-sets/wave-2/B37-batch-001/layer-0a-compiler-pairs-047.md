# Layer 0-A — Compiler Pairs (047)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-047-001 — "project closure" vocabulary full scan

**Input A:** "Run project closure activities." → Predicates: []
**Input B:** "Manage project closure sign-off." → Predicates: [] — "sign-off" alone → gap
**Input C:** "Document project closure for stakeholders." → ["DOCUMENT"]
**Input D:** "Review project closure outcomes." → ["QA"]

"project closure" = gap. All predicates from co-occurring tokens.

---

## B37-0A-047-002 — "benefits realization" vocabulary combinations

**Input A:** "Track benefits realization metrics." → Predicates: []
**Input B:** "Document benefits realization for stakeholders." → ["DOCUMENT"]
**Input C:** "Define benefits realization criteria." → ["DOCUMENT"]
**Input D:** "Build the benefits realization dashboard." → ["IMPLEMENT"]

"benefits realization" = gap vocabulary. SCL emitted for Input A.

---

## B37-0A-047-003 — "KPI dashboard" — "KPI" alone vs "build"

**Input A:** "Monitor KPI trends." → Predicates: [] — "KPI" alone → gap
**Input B:** "Track KPI performance." → Predicates: [] — "track" → NOT TEST → gap
**Input C:** "Build the KPI dashboard." → ["IMPLEMENT"] — "build" fires
**Input D:** "Design the KPI dashboard architecture." → ["ARCHITECTURE"] — "design" + "architecture"

"KPI" = gap vocabulary. Predicate fires from verb or noun co-occurrence.

---

## B37-0A-047-004 — "work breakdown structure" (WBS) — "structure" always fires

**Input A:** "Maintain the WBS." → Predicates: [] — "WBS" abbreviation alone → gap
**Input B:** "Maintain the work breakdown structure." → ["ARCHITECTURE"] — "structure" fires
**Input C:** "Create the work breakdown structure." → ["IMPLEMENT", "ARCHITECTURE"]
**Input D:** "Document the work breakdown structure." → ["DOCUMENT", "ARCHITECTURE"]

**Rule:** "WBS" abbreviation alone → gap. "work breakdown structure" → "structure" fires ARCHITECTURE.

---

## B37-0A-047-005 — Round 3 complete zero-predicate vocabulary list (037-060)

**All zero-predicate when used alone:**
escalation process, communication plan, project kickoff, steering committee, change control board,
scope creep, risk mitigation, quality gate, sign-off, post-mortem, waterfall, agile transformation,
SAFe, program increment, portfolio management, value stream, flow state, project closure,
benefits realization, KPI (monitoring/tracking), WBS (abbreviation only)

**Always fires ARCHITECTURE:**
work breakdown structure ("structure" token), design + any above, architecture + any above

**Always fires DOCUMENT:**
document/write/spec/describe/define/criteria/stakeholder + any above

**Always fires IMPLEMENT:**
build/create/implement/service/module + any above
