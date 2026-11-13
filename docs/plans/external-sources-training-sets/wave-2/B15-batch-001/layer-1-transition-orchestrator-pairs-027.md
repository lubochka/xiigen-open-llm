# Layer 1 — Transition Orchestrator Pairs (027)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B15-t1-027-001 — InvoiceService: IMPLEMENT single transition

A: "Implement an InvoiceService that generates PDF invoices."
B: TransitionType: feature_addition | SubPhaseIds: ["4.1"] | ContextCarryForward: true

---

## B15-t1-027-002 — InvoiceService: IMPLEMENT+DOCUMENT transition

A: "Implement InvoiceService + document the invoice generation workflow."
B: TransitionType: feature_addition | SubPhaseIds: ["4.1", "2.1"] | ContextCarryForward: true
Note: DOCUMENT (2.1) is parallel-eligible with IMPLEMENT (4.1); both listed in SubPhaseIds.

---

## B15-t1-027-003 — InvoiceService: IMPLEMENT+API_CONTRACT+SECURITY+DOCUMENT transition

A: "Implement InvoiceService + define data contracts + encrypt storage + document workflow."
B: TransitionType: feature_addition | SubPhaseIds: ["security", "3.3", "4.1", "2.1"] | ContextCarryForward: true

---

## B15-t1-027-004 — SubPhaseIds ordering rules

SubPhaseIds in transition record always listed in execution order:
1. "3.1" (ARCHITECTURE) — always first when present
2. "3.3" (API_CONTRACT)
3. "security" (SECURITY)
4. "4.1" (IMPLEMENT)
5. "5.1" or "5.3" (TEST)
6. "2.1" (DOCUMENT) — parallel position noted separately
7. "7.1" or "7.3" (QA) — always last when present

---

## B15-t1-027-005 — "generate" verb produces feature_addition transition

A: "Generate the InvoiceService with PDF export capability."
B: TransitionType: feature_addition | SubPhaseIds: ["4.1"] | ContextCarryForward: true
"generate" → IMPLEMENT. TransitionType: feature_addition (generate is not refactor).
