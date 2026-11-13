# Layer 3 — Teaching Unit Pairs (019)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: teaching_unit

## TU-019-A — "dispatch" does not fire API_CONTRACT in notification context

A: "Build the notification service to dispatch email and SMS." Does API_CONTRACT fire?
B: No. "dispatch" is an action verb within IMPLEMENT scope — not an API_CONTRACT trigger. API_CONTRACT requires integrate/wire up/connect (verbs) or "contract" (keyword). Only ["IMPLEMENT"] fires.

## TU-019-B — "integrate with SendGrid" fires API_CONTRACT — one predicate despite two APIs

A: "Build the notification service and integrate with SendGrid and Twilio." How many predicates?
B: Two: ["IMPLEMENT", "API_CONTRACT"]. "integrate" fires API_CONTRACT once. Two external systems (SendGrid, Twilio) are named but deduplication applies — single API_CONTRACT predicate. DecisionCount: 2, not 3.

## TU-019-C — Three API triggers still one API_CONTRACT after deduplication

A: "Implement the notification service, integrate with email API, integrate with SMS API, and connect to push gateway." How many predicates and routing decisions?
B: Two predicates: ["IMPLEMENT", "API_CONTRACT"] — despite three API-firing verbs (integrate×2, connect×1). Deduplication collapses to single API_CONTRACT. DecisionCount: 2.

## TU-019-D — "secure the token" fires SECURITY alongside API_CONTRACT

A: "Build the notification service, wire up the email API, and secure the SMS dispatch tokens." How many predicates?
B: Three: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]. "wire up" → API_CONTRACT; "secure" → SECURITY; "build" → IMPLEMENT. Three independent predicates fire — no deduplication since they are different predicates.

## TU-019-E — IEmailClient and ISmsClient: fabric-first interfaces

A: NotificationService calls SendGrid and Twilio. Should it use HttpClient directly or inject interfaces?
B: Inject IEmailClient and ISmsClient via constructor (fabric-first). The concrete SendGridEmailClient and TwilioSmsClient implement the respective interfaces. NotificationService depends only on the interfaces — it never knows the concrete HTTP implementation details.
