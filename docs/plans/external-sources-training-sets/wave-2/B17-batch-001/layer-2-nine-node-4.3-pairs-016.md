# Layer 2 — Nine-Node 4.3 Pairs (016)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-016-001 — HubSpotCrmConnector: nine-node IntegrationConnectorFlow43

**ConnectorName:** HubSpotCrmConnector
**Input predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
**TransitionType:** feature_addition

### N01 — DomainSplitterUnit
```
Task: "Build the HubSpot CRM connector and implement the contact sync, deal pipeline, and webhook handlers with request signature validation."
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
SubPhase: 4.3 (IMPLEMENT), 3.3 (API_CONTRACT), 3.4 (SECURITY)
DecisionCount: 3
```

### N02 — DomainTranslatorUnit
```
Connector: HubSpotCrmConnector
API: HubSpot API v3
Scope: Three handlers — ContactSyncHandler, DealPipelineHandler, WebhookHandler
Authentication: HubSpot Private App access token (bearer)
```

### N03 — SchemePlannerUnit
```
IHubSpotCrmConnector:
  - UpsertContactAsync(contact: HubSpotContact) → OperationResult<ContactResult>
  - UpdateDealStageAsync(dealId: string, stageId: string) → OperationResult<DealResult>
  - HandleWebhookEvent(payload: string, signature: string) → OperationResult<HubSpotEvent>
```

### N04 — DomainProcessorUnit
```
Dependencies: HttpClient (injected), IHubSpotSignatureValidator
Config: HubSpotAccessToken, HubSpotPortalId, HubSpotWebhookSecret
```

### N05 — ApplierUnit
```
Files:
1. HubSpotCrmConnector.cs
2. IHubSpotCrmConnector.cs
3. HubSpotSignatureValidator.cs — webhook signature validation
4. HubSpotModels.cs
5. HubSpotConfig.cs
```

### N06 — ResultTranslatorUnit
```
UpsertContactAsync: POST /crm/v3/objects/contacts (create) or PATCH (update)
UpdateDealStageAsync: PATCH /crm/v3/objects/deals/{dealId}
HandleWebhookEvent: Validate HubSpot-Signature-v3 header (HMAC-SHA256), dispatch
```

### N07 — ArbiterSelectorUnit
```
ArbiterRequests:
  - completeness: 3 handlers implemented
  - security: webhook signature validation present (SECURITY predicate was in input)
```

### N08 — ArbiterUnit
```
Verdict: PASS
Findings:
  - UpsertContactAsync: upsert by email — PASS
  - UpdateDealStageAsync: deal stage transitions — PASS
  - HandleWebhookEvent: HubSpot-Signature-v3 HMAC validation — PASS
  - SecurityFlow: webhook signature validation reviewed — PASS
```

### N09 — FormatterUnit
```
HubSpotCrmConnector complete.
Files generated: 5
ConnectorName: HubSpotCrmConnector
SubPhase: 4.3 (primary), 3.4 (SecurityAuditFlow34 also ran)
DecisionCount: 3
```

---

## B17-L2-016-002 — Gate B for HubSpotCrmConnector

**Gate B:** Three handlers implemented, HMAC-SHA256 webhook signature validation present, filesGenerated >= 4, SecurityAuditFlow34 verdict = PASS.

**Assertion:** N08 verdict = PASS AND filesGenerated >= 4 AND hmacValidationPresent = true.
