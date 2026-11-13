# Layer 0-B — Router Pairs (021)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-021-001 — API_CONTRACT route: "walk through integrating" → API_CONTRACT fires

A: "Walk through integrating the Salesforce CRM (Customer Relationship Management) with our support ticket system."
Router decision: Predicates = ["API_CONTRACT"] → APIContractFlow33
Evidence: "integrating" fires API_CONTRACT; "walk through" = gap warning alongside API_CONTRACT.

---

## B34-router-021-002 — API_CONTRACT route: "guide through integrating"

A: "Guide the backend developers through integrating the Google Maps API into the delivery tracking feature."
Router decision: Predicates = ["API_CONTRACT"] → APIContractFlow33
Evidence: Embedded "integrating … API" → API_CONTRACT; "guide through" = gap warning.

---

## B34-router-021-003 — Gap route: "walk through the API" (no integrate verb)

A: "Walk the new developers through the internal API documentation."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-onboard-001
Evidence: "walk through documentation" = onboarding/explanation intent → gap. No "integrate/connect" verb → no API_CONTRACT fires.

---

## B34-router-021-004 — API_CONTRACT route: "show how to integrate"

A: "Show the mobile team how to integrate push notifications via the Firebase API."
Router decision: Predicates = ["API_CONTRACT"] → APIContractFlow33
Evidence: Embedded "integrate … API" fires API_CONTRACT; "show" = gap warning.

---

## B34-router-021-005 — API_CONTRACT+IMPLEMENT route: "walk through integrating and building"

A: "Walk through integrating the payment gateway and implementing the order confirmation flow."
Router decision: Predicates = ["API_CONTRACT", "IMPLEMENT"] → APIContractFlow33 → ImplementationFlow41
Evidence: "integrate" → API_CONTRACT; "implementing the order confirmation flow" → IMPLEMENT; "walk through" = gap warning alongside both predicates.
