# Layer 0-B — Router Pairs (049)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-049-001 — "consult on" + ARCHITECTURE routing

A: "Consult on the service decomposition strategy and architect the bounded context map."
Router decision: Predicates = ["ARCHITECTURE"] → ArchitectureFlow31 | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "consult on strategy" = gap warning; "architect the bounded context map" = requester's ARCHITECTURE action → ArchitectureFlow31.

---

## B34-router-049-002 — "get feedback on" + TEST routing

A: "Get feedback on the test strategy from the tech lead and write the missing integration tests."
Router decision: Predicates = ["TEST"] → UnitTestingFlow51 | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "get feedback on strategy" = gap warning; "write missing integration tests" = TEST → UnitTestingFlow51.

---

## B34-router-049-003 — "second opinion" + DOCUMENT routing

A: "Get a second opinion on the documentation outline and write the architecture decision record."
Router decision: Predicates = ["DOCUMENT"] → DocumentationFlow | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "second opinion on outline" = gap warning; "write architecture decision record" = DOCUMENT → DocumentationFlow.

---

## B34-router-049-004 — "sounding board" + API_CONTRACT routing

A: "Use the platform team as a sounding board for the webhook design and integrate the webhook handler."
Router decision: Predicates = ["API_CONTRACT"] → APIContractFlow33 | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "sounding board for webhook design" = gap warning; "integrate webhook handler" = direct integrate → API_CONTRACT → APIContractFlow33.

---

## B34-router-049-005 — "rubber duck" + SECURITY routing

A: "Rubber duck the session management design and then harden the token storage."
Router decision: Predicates = ["SECURITY"] → SecurityAuditFlow | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "rubber duck session management design" = gap warning; "harden token storage" = SECURITY → SecurityAuditFlow.
