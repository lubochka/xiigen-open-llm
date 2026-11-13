# Layer 2 — Nine-Node Flow Pairs (019)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: nine_node_flow (ImplementationFlow41 + APIContractFlow33)

## B15-nine-019 — NotificationService (IMPLEMENT+API_CONTRACT)

Request: "Build the notification service and integrate with the SendGrid email API and Twilio SMS API."

### Two flows: APIContractFlow33 then ImplementationFlow41

**Node 1 — DomainSplitterUnit**: Target: NotificationService (feature_addition) | A = SelectedLogicalTransition: TransitionType: feature_addition, SubPhaseIds: ["3.3", "4.1"], PredicateIds: ["IMPLEMENT", "API_CONTRACT"] | APIContractFlow33 runs first to define external API contracts; ImplementationFlow41 consumes the contract specs.

**Node 2 — DomainTranslatorUnit (API_CONTRACT)**: Gate B: must define contracts for: (1) SendGrid email API — endpoint, auth header, request payload (To, From, Subject, Body), response (MessageId); (2) Twilio SMS API — endpoint, account SID (Sender ID), auth token, request payload (To, From, Body), response (SmsId). Output: two ExternalApiContract documents.

**Node 3 — SchemePlannerUnit (API_CONTRACT → IMPLEMENT)**: probationary_accepted — API contracts approved; IEmailClient and ISmsClient interfaces derived from contract specs; implementation scope: NotificationDispatcher, ChannelRouter, DeliveryStatusTracker.

**Node 4 — DomainProcessorUnit (IMPLEMENT)**: WorkBranches:
- NotificationDispatcher: INotificationService.Send(NotificationRequest) → OperationResult<DeliveryId>; BR-NOTIF-001: channel selection based on user preference (email/SMS/push).
- ChannelRouter: routes to IEmailClient or ISmsClient; BR-NOTIF-002: at least one channel must be active per user.
- DeliveryStatusTracker: INotificationService.GetStatus(DeliveryId) → OperationResult<DeliveryStatus>.

**Node 5 — ApplierUnit**: Files generated: INotificationService.cs, NotificationService.cs, IEmailClient.cs, ISmsClient.cs, SendGridEmailClient.cs, TwilioSmsClient.cs, ChannelRouter.cs, DeliveryStatusTracker.cs.

**Node 6 — ResultTranslatorUnit**: 8 files, 4 interfaces, 2 business rules, 2 domain events (EmailSent, SmsSent). ExternalApiContracts: 2 (SendGrid, Twilio). Test surface: 12 recommended cases.

**Node 7 — ArbiterSelectorUnit**: Arbiter: api-contract-arbiter then implementation-completeness-arbiter.

**Node 8 — ArbiterUnit**: API contract verdict: PASS (SendGrid and Twilio contracts complete). Implementation verdict: PASS (IEmailClient and ISmsClient injected; channel routing confirmed; fabric-first confirmed).

**Node 9 — FormatterUnit**: Verdict: IMPLEMENT_APPROVED (IMPLEMENT+API_CONTRACT) | TransitionType: feature_addition | Files: 8 | ExternalContracts: 2 | TestSurface: 12 cases
