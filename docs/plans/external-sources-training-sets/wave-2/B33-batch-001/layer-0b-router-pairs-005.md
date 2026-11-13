# Layer 0-B — Router Pairs (005)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: router

---

## C0B-005-001 — Gap routing: DomainId assignment

All B33 gap requests → GapHandler | DomainId: "software-development"
Note: No "ai-development" domain registered. All AI agent gaps use "software-development".

---

## C0B-005-002 — "orchestrate" (verb) does not fire ARCHITECTURE

A: [] | "Orchestrate the AI agents to handle complex multi-step queries"
Correct: GapHandler (no predicate)
Reason: "orchestrate" as operational verb does not fire ARCHITECTURE. ARCHITECTURE requires: design, architect, structure, blueprint, diagram, define (structural context).

---

## C0B-005-003 — "train" (ML sense) does not fire any predicate

A: [] | "Train the model on new customer support data"
Correct: GapHandler
Reason: "train" (ML training) is AI/ML vocabulary gap. Not a software predicate trigger.

---

## C0B-005-004 — "fine-tune" does not fire IMPLEMENT

A: [] | "Fine-tune the LLM on domain-specific data"
Correct: GapHandler
Reason: "fine-tune" (ML sense) is AI gap. Does not fire IMPLEMENT. IMPLEMENT requires software code-writing verbs.

---

## C0B-005-005 — "evaluate" does not fire TEST in AI context

A: [] | "Evaluate the agent's accuracy on the test set"
Correct: GapHandler
Reason: "evaluate" (analytical) is gap in AI model evaluation context. TEST fires only on test-writing verbs (write, create, add). Evaluating model outputs ≠ writing tests.
