# Layer 0-A — Compiler Pairs (003)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: compiler

---

## B17-0A-003-001 — "connect" + "implement" → IMPLEMENT + API_CONTRACT

**Input:**
"Implement the Twilio SMS connector and connect it to the notification dispatch service."

**Compiler analysis:**
- "Implement" → IMPLEMENT (Rule 11)
- "connector" → API_CONTRACT (Rule 4: connector fires API_CONTRACT)
- "connect" + external service ("notification dispatch service") → API_CONTRACT (Rule 22)
- Integration-context vocabulary: "connector", "connect" → IMPLEMENT routes to 4.3 (Rule 3)

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**SubPhase:** 4.3
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-003-002 — "connect" alone (no build verb) → API_CONTRACT only

**Input:**
"Connect the Twilio SMS API to the notification service."

**Compiler analysis:**
- "connect" + API (external service) → API_CONTRACT (Rule 22)
- No build/implement/write/create/develop verb → IMPLEMENT does NOT fire
- "API" → API_CONTRACT (reinforces)

**CompiledPredicateIds:** ["API_CONTRACT"]
**SubPhase:** 3.3 (API_CONTRACT alone → 3.3, not 4.3)
**TransitionType:** documentation
**DomainId:** software-development

**Teaching point:** "connect" fires API_CONTRACT. Without a build verb, only API_CONTRACT fires → routes to 3.3.

---

## B17-0A-003-003 — "implement" present but without integration vocabulary → 4.1

**Input:**
"Implement the NotificationDispatchService class and add the send method."

**Compiler analysis:**
- "Implement" → IMPLEMENT
- "class", "method" → service/logic vocabulary (Rule 3)
- No integration-context vocabulary → IMPLEMENT routes to 4.1
- No API_CONTRACT triggers present

**CompiledPredicateIds:** ["IMPLEMENT"]
**SubPhase:** 4.1
**TransitionType:** feature_addition
**DomainId:** software-development

---

## B17-0A-003-004 — "implement" + "connector" is the canonical 4.3 signal

**Input:**
"Implement the TwilioSmsConnector class with send, receive, and delivery status methods."

**Compiler analysis:**
- "Implement" → IMPLEMENT
- "connector" → API_CONTRACT (Rule 4)
- "class" present but "connector" in the class name is the dominant integration signal → 4.3
- Integration vocabulary ("connector") co-present → IMPLEMENT routes to 4.3

**CompiledPredicateIds:** ["IMPLEMENT", "API_CONTRACT"]
**SubPhase:** 4.3
**TransitionType:** feature_addition
**DomainId:** software-development

**Teaching point:** "connector" in any position (class name, noun phrase) fires API_CONTRACT and shifts IMPLEMENT routing from 4.1 to 4.3.
