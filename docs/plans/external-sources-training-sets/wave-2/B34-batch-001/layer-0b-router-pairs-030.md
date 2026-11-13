# Layer 0-B — Router Pairs (030)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-030-001 — "present" + DOCUMENT routing

A: "Present the documentation plan and write the architecture decision records."
Router decision: Predicates = ["DOCUMENT"] → DocumentationFlow | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "present the documentation plan" = gap warning; "write the architecture decision records" = documentation action → DOCUMENT fires.

---

## B34-router-030-002 — "show how to" + API routing

A: "Show the team how to call the new reporting API endpoints."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001
Evidence: "show how to call" = "teach to X" pattern → gap. Infinitive "to call" = learning content.

---

## B34-router-030-003 — "show" + API action routing

A: "Show the API endpoints and integrate them into the reporting dashboard."
Router decision: Predicates = ["API_CONTRACT", "IMPLEMENT"] → APIContractFlow33 → ImplementationFlow41 | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "show the API endpoints" = gap warning; "integrate them into the dashboard" → API_CONTRACT fires (integrate always fires); "integrate + dashboard implementation" → IMPLEMENT follows. Ordering: API_CONTRACT before IMPLEMENT.

---

## B34-router-030-004 — guidance + API_CONTRACT routing (integrate exception)

A: "Walk the team through integrating the payment gateway."
Router decision: Predicates = ["API_CONTRACT"] → APIContractFlow33 | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "walk through" = guidance frame → gap warning; "integrating the payment gateway" → integrate fires API_CONTRACT even inside walk-through frame.

---

## B34-router-030-005 — TransitionType for Round 2 guidance+code scenarios

| Predicates | TransitionType | Notes |
|-----------|----------------|-------|
| [] | none | gap only |
| [IMPLEMENT] | feature_addition | build/deploy/update actions |
| [ARCHITECTURE] | documentation | design actions |
| [DOCUMENT] | documentation | document/write docs actions |
| [TEST] | documentation | write tests actions |
| [SECURITY] | documentation | harden/patch/audit actions |
| [API_CONTRACT] | documentation | integrate actions |
| [IMPLEMENT + any] | feature_addition | IMPLEMENT always dominates |
