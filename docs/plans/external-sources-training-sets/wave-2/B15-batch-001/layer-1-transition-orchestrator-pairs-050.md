# Layer 1 — Transition Orchestrator Pairs (050)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B15-t1-050-001 — Anti-pattern: SubPhaseIds missing "4.1" (forbidden)

SubPhaseIds must always include "4.1" — IMPLEMENT is always present in B15.
Anti-pattern: SubPhaseIds: ["security", "5.1"] (missing "4.1").
Correct: SubPhaseIds: ["security", "4.1", "5.1"].

---

## B15-t1-050-002 — Anti-pattern: "7.1" not last in SubPhaseIds (forbidden)

QA ("7.1" or "7.3") must always be the last entry in SubPhaseIds when present.
Anti-pattern: SubPhaseIds: ["7.1", "4.1", "5.1"] (QA before IMPLEMENT).
Correct: SubPhaseIds: ["4.1", "5.1", "7.1"] (QA last).

---

## B15-t1-050-003 — Anti-pattern: duplicate entries in SubPhaseIds (forbidden)

SubPhaseIds must never contain duplicate entries after deduplication.
Anti-pattern: SubPhaseIds: ["security", "security", "4.1"] (two "security" entries).
Anti-pattern: SubPhaseIds: ["3.3", "2.1", "4.1", "2.1"] (two "2.1" entries from "define contracts" + "document").
Correct: deduplication applied → each sub-phase ID appears once.

---

## B15-t1-050-004 — Anti-pattern: wrong ordering of "security" and "3.3" (forbidden)

SubPhaseIds: ["security", "3.3", "4.1"] is incorrect — "3.3" must come before "security".
Correct: SubPhaseIds: ["3.3", "security", "4.1"].
Rule: ARCHITECTURE → API_CONTRACT → SECURITY → IMPLEMENT — same as routing order.

---

## B15-t1-050-005 — Complete valid SubPhaseIds patterns for B15 (reference)

Single: ["4.1"]
Two-phase: ["3.1", "4.1"] | ["3.3", "4.1"] | ["security", "4.1"] | ["4.1", "5.1"] | ["4.1", "2.1"] | ["4.1", "7.1"]
Three-phase: ["3.1", "4.1", "5.1"] | ["3.3", "security", "4.1"] | ["security", "4.1", "5.1"] | ["4.1", "5.1", "2.1"] | ["4.1", "5.1", "7.1"]
