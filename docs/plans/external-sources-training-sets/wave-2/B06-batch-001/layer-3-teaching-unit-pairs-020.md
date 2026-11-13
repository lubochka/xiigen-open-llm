# Layer 3 — Teaching Unit Pairs (020)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## Method note

File 020 covers v6 corruption teaching for new domains — three pairs.

---

## B06-L3-020-001 — Teaching: v6-01 corruption in new domains

```
source_id: xiigen-b06-l3-020-001
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-V6-01-NEW-DOMAINS"
  Content:
    corruption_type: "v6-01 — DOCUMENT missed"
    new_domain_examples:
      corrupt:
        - request: "Define the shipping requirements"; predicates_produced: []
          error: "define and requirements are both DOCUMENT terms — should produce [DOCUMENT]"
        - request: "Describe the inventory behavior"; predicates_produced: []
          error: "describe and behavior are both DOCUMENT terms"
      correct:
        - request: "Define the shipping requirements"; predicates: ["DOCUMENT"]
        - request: "Describe the inventory behavior"; predicates: ["DOCUMENT"]
    fix: "DOCUMENT fires on any of the 17 lexicon terms. Always scan full request text."

---

## B06-L3-020-002 — Teaching: v6-02 corruption in new domains

```
source_id: xiigen-b06-l3-020-002
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-V6-02-NEW-DOMAINS"
  Content:
    corruption_type: "v6-02 — IMPLEMENT suppressed"
    new_domain_examples:
      corrupt:
        - request: "Write the loyalty feature-brief"; predicates_produced: ["DOCUMENT"]
          error: "write fires IMPLEMENT — should produce [DOCUMENT, IMPLEMENT]"
        - request: "Create the subscription user stories"; predicates_produced: ["DOCUMENT"]
          error: "create fires IMPLEMENT — should produce [DOCUMENT, IMPLEMENT]"
      correct:
        - "Write the loyalty feature-brief" → ["DOCUMENT", "IMPLEMENT"]
        - "Create the subscription user stories" → ["DOCUMENT", "IMPLEMENT"]
    fix: "Compiler is lexicon-based. IMPLEMENT verbs fire regardless of documentation object."

---

## B06-L3-020-003 — Teaching: v6-03 corruption in new domains

```
source_id: xiigen-b06-l3-020-003
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-V6-03-NEW-DOMAINS"
  Content:
    corruption_type: "v6-03 — routing collapsed or TransitionType wrong"
    new_domain_examples:
      corrupt:
        - request: "Specify stakeholder requirements for privacy"; routing_produced: ["2.1 only"]
          error: "stakeholder triggers 2.4 — should produce [2.1, 2.4]"
        - request: "Write fraud detection spec"; transition_type: "documentation"
          error: "IMPLEMENT present (write) → feature_addition required"
      correct:
        - "Specify stakeholder requirements for privacy" → routing: [2.1, 2.4]
        - "Write fraud detection spec" → transition_type: feature_addition
    fix:
      "stakeholder": "always produces 2.4 routing decision in addition to 2.1"
      "IMPLEMENT present": "always upgrades TransitionType to feature_addition on ALL transitions"
