# Layer 0-C — Knowledge-Edge Pairs (002)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B38-0C-002-001 — Rule: "service" fires IMPLEMENT everywhere

Rule statement:
"service" → IMPLEMENT ALWAYS
Even in release review sentences where the user meant "the service being released", the word "service" fires the IMPLEMENT predicate.

This is B38 Core Rule 2. It is absolute and has no exceptions.

---

## B38-0C-002-002 — Release-context examples where "service" still fires IMPLEMENT

Example 1 (sign-off context):
"Sign off on the order service release."
- "sign off" → QA
- "service" → IMPLEMENT (fires; user meant "the service being released")
- Result: ["QA", "IMPLEMENT"]

Example 2 (checklist context):
"Complete the service deployment checklist."
- "service" → IMPLEMENT (fires; user described a process for the service)
- "deployment" → GAP
- "checklist" → GAP
- Result: ["IMPLEMENT"] with gap warnings

Example 3 (approval context):
"The service has been approved for go-live."
- "service" → IMPLEMENT
- "approved" → QA
- "go-live" → GAP
- Result: ["QA", "IMPLEMENT"]

Example 4 (monitoring context — IMPLEMENT still fires):
"Monitor the auth service post-release."
- "Monitor" → GAP (rule 7)
- "service" → IMPLEMENT
- Result: ["IMPLEMENT"] with gap warning on monitor

---

## B38-0C-002-003 — Why this rule exists (knowledge edge rationale)

The IMPLEMENT predicate is a signal that code is involved.
When the word "service" appears in any software-development context, it almost always refers to a software service — a code artifact.
The compiler fires IMPLEMENT to ensure code-level review flows (Phase 4.1) are invoked whenever a service is mentioned.

Without this rule, a sentence like "approve the payment service" would only route to 7.1 (QA) and miss the 4.1 (IMPLEMENT) phase. The service code would not be reviewed at the implementation level.

The rule accepts some false positives (e.g., "customer service department") in exchange for ensuring no service code is missed. In a software-development domain (DomainId: software-development), the false positive rate for "service" = non-code-service is low.

---

## B38-0C-002-004 — "service" + other IMPLEMENT terms (no duplication)

"payment service build" → "service" (IMPLEMENT) + "build" (IMPLEMENT) → one IMPLEMENT predicate
"notification service pipeline" → "service" (IMPLEMENT) + "pipeline" (IMPLEMENT) → one IMPLEMENT predicate

Multiple IMPLEMENT-firing terms consolidate to a single IMPLEMENT predicate.
DecisionCount does not increase with additional IMPLEMENT triggers.
The predicate set is ["IMPLEMENT"], not ["IMPLEMENT", "IMPLEMENT"].
