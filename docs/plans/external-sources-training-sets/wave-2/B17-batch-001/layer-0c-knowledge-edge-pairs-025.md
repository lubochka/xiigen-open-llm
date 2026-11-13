# Layer 0-C — Knowledge-Edge Pairs (025)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Twilio SMS connector edge cases

---

## B17-0C-025-001 — Twilio: integration vocabulary edges

"connector" shifts IMPLEMENT from 4.1 to 4.3. Without it:
- "Build Twilio SMS service" → IMPLEMENT → 4.1 (no integration vocab)
- "Build Twilio SMS connector" → IMPLEMENT → 4.3 ("connector" is integration vocab)
- "Build Twilio SMS webhook handler" → IMPLEMENT → 4.3 ("webhook" is integration vocab)
"Twilio" and "SMS" are proper nouns and domain nouns — transparent to routing.

---

## B17-0C-025-002 — Twilio: "SMS" and "Messaging" are domain nouns

Domain nouns that fire NO predicate for Twilio:
- SMS (product acronym)
- Messaging (product domain)
- telephony (domain noun)
- delivery status (domain phrase)
- phone number (domain phrase)

Only structural vocabulary (connector, webhook, API, endpoint, SDK, gateway, external service, wire up) routes IMPLEMENT to 4.3.

---

## B17-0C-025-003 — Twilio: multiple "token" occurrences

"Build Twilio connector with authentication token and API key token."
- "token" appears twice → SECURITY fires ONCE (predicate sets, not multisets)
- "authentication" reinforces SECURITY → still fires once
- Predicate sets: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

Rule: Predicates are sets. Multiple trigger occurrences produce exactly one instance of the predicate.

---

## B17-0C-025-004 — Twilio: "connect" with and without build verb

| Input | IMPLEMENT? | Why |
|---|---|---|
| "Connect and build the Twilio connector" | Yes | "build" is the build verb; "connect" reinforces API_CONTRACT |
| "Connect the Twilio external service" | No | "connect" alone is not a build verb; fires API_CONTRACT only |
| "Wire up and build the Twilio webhook" | Yes | "build" is the build verb |
| "Wire up Twilio endpoints" | No | "wire up" alone = API_CONTRACT only |
