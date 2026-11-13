# Layer 0-C — Knowledge-Edge Pairs (014)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Mailgun email connector edges

---

## B17-0C-014-001 — Edge: "webhook" fires API_CONTRACT in email context

**Assertion:** "webhook" fires API_CONTRACT regardless of whether the context is payment, email, shipping, or any other domain.

**Email connector evidence:**
| Input fragment | API_CONTRACT fires? |
|---|---|
| "Mailgun delivery webhook" | Yes |
| "bounce webhook handler" | Yes |
| "unsubscribe webhook endpoint" | Yes |
| "implement the webhook listener" | Yes |

**Rule 5 is domain-independent:** The word "webhook" fires API_CONTRACT in ALL connector domains, not only payment connectors.

---

## B17-0C-014-002 — Edge: "integrate" does not become IMPLEMENT in email context

**Assertion:** Rule 1 ("integrate" → API_CONTRACT ONLY, never IMPLEMENT) applies to email connector requests exactly as it applies to payment connector requests.

**Evidence:**
| Input | Predicates |
|---|---|
| "Integrate Mailgun" | ["API_CONTRACT"] |
| "Integrate the email API" | ["API_CONTRACT"] |
| "Integrate Mailgun for transactional email" | ["API_CONTRACT"] |

**The domain (email vs payment) does not change Rule 1.** "integrate" always → API_CONTRACT only.

---

## B17-0C-014-003 — Edge: "HMAC" in email webhook context → SECURITY

**Input:** "Create the Mailgun webhook connector and implement HMAC-SHA256 signature validation."

**Edge assertions:**
- "connector" → API_CONTRACT (Rule 4)
- "webhook" → API_CONTRACT (Rule 5; reinforces)
- "HMAC-SHA256 signature validation" → SECURITY (Rule 8: validate signature fires SECURITY)
- "Create" → IMPLEMENT (Rule 11)

**Result:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

**Teaching point:** HMAC signature validation fires SECURITY in email connectors just as it does in payment connectors. The underlying security mechanism is the same; the predicate rule is domain-independent.

---

## B17-0C-014-004 — Edge: internal email service vs external email connector (4.1 vs 4.3)

**Contrast:**

**Internal email service (→ 4.1):**
- "Build the EmailNotificationService with retry logic" → IMPLEMENT → 4.1 (no integration vocabulary)

**External email connector (→ 4.3):**
- "Build the Mailgun email connector" → IMPLEMENT → 4.3 (connector is integration vocabulary)
- "Build the email API client" → IMPLEMENT → 4.3 (API is integration vocabulary)
- "Build the transactional email webhook handler" → IMPLEMENT → 4.3 (webhook is integration vocabulary)

**Decision rule:** The service/logic vocabulary alone routes to 4.1. Integration vocabulary (connector, API, webhook, external service) routes to 4.3 and overrides service/logic vocabulary when both are present.
