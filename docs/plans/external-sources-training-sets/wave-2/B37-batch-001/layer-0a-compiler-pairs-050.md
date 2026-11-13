# Layer 0-A — Compiler Pairs (050)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-050-001 — All rounds combined: zero-predicate certification test

**Input:** "Track milestones, coordinate blockers, manage risk log, run escalation process, facilitate project kickoff, coordinate change control board, monitor waterfall delivery, manage agile transformation, track SAFe adoption, and oversee program increment planning."

All tokens: gap vocabulary across all B37 rounds.
Predicates: []
DomainId: "project-management"
SCL: emitted (once)

---

## B37-0A-050-002 — All rounds combined: single DOCUMENT trigger

**Input:** "Track milestones, coordinate blockers, manage risk log, run escalation process, manage agile transformation, and document the communication plan."

Token scan:
- All tokens except "document" → gap
- "document" → DOCUMENT fires

Predicates: ["DOCUMENT"]
DomainId: "software-development"
SCL: NOT emitted
GapIds: track-001, blockers-001, coordinate-001, raid-log-001, manage-delivery-001 forwarded

---

## B37-0A-050-003 — All rounds combined: IMPLEMENT fires on service noun

**Input:** "Coordinate the agile transformation, manage SAFe adoption, and build the program increment tracking service."

Token scan:
- "Coordinate" → gap; "agile transformation" → gap
- "manage" → gap; "SAFe adoption" → gap
- "build" → IMPLEMENT fires; "program increment" → gap; "tracking service" → "service" fires IMPLEMENT

Multiple IMPLEMENT triggers → ONE IMPLEMENT predicate
Predicates: ["IMPLEMENT"]
DomainId: "software-development"

---

## B37-0A-050-004 — All rounds combined: ARCHITECTURE fires on "structure" noun

**Input:** "Review the waterfall phase structure and the work breakdown structure."

Token scan:
- "Review" → QA fires
- "waterfall phase structure" → "structure" fires ARCHITECTURE
- "work breakdown structure" → "structure" fires ARCHITECTURE (already fired)

Multiple QA and ARCHITECTURE triggers → ONE each
Predicates: ["QA", "ARCHITECTURE"]
DomainId: "software-development"

---

## B37-0A-050-005 — Predicate deduplication rule for all B37 rounds

**Rule confirmed across all rounds (002-060):**

Multiple occurrences of the same predicate trigger within one input → ONE predicate in the output list.

Examples:
- "document...document...stakeholders" → ["DOCUMENT"] (not ["DOCUMENT", "DOCUMENT"])
- "build...service...module" → ["IMPLEMENT"] (not ["IMPLEMENT", "IMPLEMENT", "IMPLEMENT"])
- "structure...structure" → ["ARCHITECTURE"] (not ["ARCHITECTURE", "ARCHITECTURE"])
- "review...validate" → ["QA"] (not ["QA", "QA"])

Distinct predicates combine: "document" + "build" → ["DOCUMENT", "IMPLEMENT"]
