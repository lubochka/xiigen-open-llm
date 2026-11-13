# Layer 0-C — Knowledge-Edge Pairs (054)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-054-001 — Knowledge-edge: verbs that DO NOT trigger predicates in PM context

**Gap verbs (all produce knowledge_edge when combined with PM vocabulary):**
run, manage, coordinate, facilitate, track, monitor, oversee, maintain, prepare, circulate, update, report, flag, share, submit, present, obtain, lead, roll out, transition, map, enforce, pass

**All these verbs + any B37 PM vocabulary = knowledge_edge (zero predicates)**

---

## B37-0C-054-002 — Knowledge-edge: verbs that DO trigger predicates

**Predicate-triggering verbs:**
- "review" / "validate" → QA
- "define" / "document" / "write" / "spec" / "describe" → DOCUMENT
- "design" → ARCHITECTURE
- "build" / "create" / "implement" → IMPLEMENT

**These verbs ALWAYS fire their predicate regardless of PM context.**

---

## B37-0C-054-003 — Knowledge-edge: nouns that DO NOT trigger predicates in PM context

**Gap nouns (PM managing):**
milestone, progress, status, update, report, blockers, escalation, CCB, programme board, steering committee, project kickoff, communication plan, scope creep, risk mitigation, quality gate, sign-off, post-mortem, waterfall, agile, SAFe, PI, portfolio, value stream, flow state, project closure, benefits realization, KPI, dashboard (without "build")

---

## B37-0C-054-004 — Knowledge-edge: nouns that DO trigger predicates

**Predicate-triggering nouns:**
- "stakeholder" → DOCUMENT
- "structure" → ARCHITECTURE (even in PM contexts: "governance structure", "accountability structure", "work breakdown structure")
- "service" / "module" → IMPLEMENT
- "architecture" → ARCHITECTURE
- "criteria" → DOCUMENT (often combined with "define")
- "requirements" / "specification" / "spec" → DOCUMENT
- "API" + "dependencies" → API_CONTRACT

**These nouns ALWAYS fire their predicate regardless of PM verb context.**

---

## B37-0C-054-005 — Knowledge-edge: "dashboard" noun boundary

"dashboard" alone → gap (zero predicates)
"build dashboard" → IMPLEMENT fires (from "build")
"design dashboard architecture" → ARCHITECTURE fires (from "design" + "architecture")
"KPI dashboard" alone → gap
"build KPI dashboard" → IMPLEMENT fires

"Dashboard" is a gap noun. It does not self-trigger. The predicate fires from the co-occurring verb.
