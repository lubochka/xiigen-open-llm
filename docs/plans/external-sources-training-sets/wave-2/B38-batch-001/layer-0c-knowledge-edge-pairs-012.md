# Layer 0-C — Knowledge-Edge Pairs (012)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B38-0C-012-001 — B38 knowledge edge summary

Scenario: B38 — Project delivery — release gates and go-live checklists
Sub-phase: 7.1 (Release and Deployment Automation)
DomainId: software-development

Core knowledge edges in B38 (vocabulary at predicate scope boundaries):

1. "service" = IMPLEMENT (not process noun in software domain)
2. "criteria" = DOCUMENT (not QA; not context word)
3. "build" = IMPLEMENT (not just a verb meaning "to make"; code artifact context)
4. "deployment" = GAP (not IMPLEMENT; operational, not code-authoring)
5. "audit" = QA or QA+SECURITY (not a generic context word)
6. "monitor" = GAP (not QA; not IMPLEMENT)
7. "execute" = GAP in execution context (not IMPLEMENT = code authoring ≠ running)
8. "launch" = GAP (not IMPLEMENT; launch ≠ build)
9. "rollout" = GAP (not IMPLEMENT; not QA)
10. "checklist" = GAP (not DOCUMENT; does not fire DOCUMENT)

---

## B38-0C-012-002 — Mastery test for B38 vocabulary

A correctly-trained model should:

1. Fire ["QA","IMPLEMENT"] for: "Review the payment service" — not ["QA"]
2. Fire ["QA","DOCUMENT"] for: "Validate the release criteria" — not ["QA"]
3. Fire [] for: "Execute the rollout" — not ["QA"] or ["IMPLEMENT"]
4. Fire ["QA","SECURITY"] for: "Security audit before approval" — not ["QA"]
5. Fire ["QA","IMPLEMENT"] for: "Audit the build artifacts and approve" — not ["QA","IMPLEMENT","DOCUMENT"]
6. Fire ["QA","IMPLEMENT","DOCUMENT","SECURITY"] for: "Review the payment service, validate release criteria, security audit, approve go-live" — all four

Mastery = answering all 6 correctly.
Partial mastery = 4-5 correct.
Retraining needed = below 4 correct.

---

## B38-0C-012-003 — B38 rules most frequently violated

By priority of violation frequency:

Rank 1: "deployment" → IMPLEMENT (wrong) instead of GAP
Correction: rule 4 — deployment operational context = GAP

Rank 2: Ignoring "service" as IMPLEMENT trigger
Correction: rule 2 — service = IMPLEMENT ALWAYS

Rank 3: Missing "criteria" as DOCUMENT trigger
Correction: rule 3 — criteria = DOCUMENT ALWAYS

Rank 4: Missing SECURITY when "security audit" present
Correction: rule 19 — security + audit → SECURITY co-fires

Rank 5: "launch" classified as IMPLEMENT
Correction: rule 5 — launch = GAP vocabulary

Rank 6: knowledge_edge confused with GapHandler
Correction: rule 18 — B38 zero-predicate = knowledge_edge, not GapHandler
