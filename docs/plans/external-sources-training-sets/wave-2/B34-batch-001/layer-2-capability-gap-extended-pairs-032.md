# Layer 2 — Capability Gap Extended Pairs (032)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B34-gap-032 — "explain and build" compound: extended execution

A: "Explain the observer pattern to the mobile team and build the push notification service."
B: Predicates: ["IMPLEMENT"] | GapWarning: gap-skills-guidance-train-001 | DomainId: software-development
GapHandler: SUPPRESSED | SCHEDULE_CAPABILITY_LEARNING: NOT EMITTED
ImplementationFlow41: push notification service — FCM (Firebase Cloud Messaging) integration, device token storage, notification dispatch. APPROVED.
Verdict: IMPLEMENT_APPROVED + GAP_WARNING

A: "Explain the contract testing approach and write the provider contract tests."
B: Predicates: ["TEST"] | GapWarning: gap-skills-guidance-train-001 | DomainId: software-development
GapHandler: SUPPRESSED | SCHEDULE_CAPABILITY_LEARNING: NOT EMITTED
UnitTestingFlow51: provider contract tests written — consumer-driven contract format, Pact (Pact contract testing) compatible. TEST_PASS.
Verdict: TEST_PASS + GAP_WARNING

A: "Explain the REST (Representational State Transfer) naming conventions and document the endpoint reference."
B: Predicates: ["DOCUMENT"] | GapWarning: gap-skills-guidance-train-001 | DomainId: software-development
DocumentationFlow: endpoint reference written. DOCUMENT_APPROVED.
Verdict: DOCUMENT_APPROVED + GAP_WARNING

A: "Explain the XSS (Cross-Site Scripting) attack surface to the team and implement the content security policy headers."
B: Predicates: ["IMPLEMENT", "SECURITY"] | GapWarning: gap-skills-guidance-train-001 | DomainId: software-development
Verdict: IMPLEMENT_APPROVED + SECURITY_APPROVED + GAP_WARNING
