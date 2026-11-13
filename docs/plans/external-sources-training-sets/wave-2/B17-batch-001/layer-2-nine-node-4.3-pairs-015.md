# Layer 2 — Nine-Node 4.3 Pairs (015)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-015-001 — FedExShippingConnector: nine-node IntegrationConnectorFlow43

**ConnectorName:** FedExShippingConnector
**Input predicates:** ["IMPLEMENT", "API_CONTRACT"]
**TransitionType:** feature_addition

### N01 — DomainSplitterUnit
```
Task: "Build the FedEx shipping connector and implement the rate quote, label generation, and tracking API endpoints."
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3
```

### N02 — DomainTranslatorUnit
```
Connector: FedExShippingConnector
API: FedEx Web Services REST API v1
Scope: Three handlers — RateQuoteHandler, ShipmentLabelHandler, TrackingHandler
Authentication: OAuth 2.0 client credentials
```

### N03 — SchemePlannerUnit
```
IFedExShippingConnector:
  - GetRateQuoteAsync(request: RateRequest) → OperationResult<RateQuote>
  - CreateShipmentAsync(request: ShipmentRequest) → OperationResult<ShipmentLabel>
  - TrackShipmentAsync(trackingNumber: string) → OperationResult<TrackingResult>
```

### N04 — DomainProcessorUnit
```
Dependencies: HttpClient (injected), IFedExAuthService
Config: FedExClientId, FedExClientSecret, FedExAccountNumber, FedExApiBaseUrl
```

### N05 — ApplierUnit
```
Files:
1. FedExShippingConnector.cs
2. IFedExShippingConnector.cs
3. FedExAuthService.cs — OAuth token management
4. FedExModels.cs (RateRequest, RateQuote, ShipmentRequest, ShipmentLabel, TrackingResult)
5. FedExConfig.cs
```

### N06 — ResultTranslatorUnit
```
GetRateQuoteAsync: POST /rate/v1/rates/quotes
CreateShipmentAsync: POST /ship/v1/shipments
TrackShipmentAsync: GET /track/v1/trackingnumbers/{trackingNumber}
```

### N07 — ArbiterSelectorUnit
```
ArbiterRequests:
  - completeness: all 3 handlers implemented
  - auth: OAuth token refresh on 401 response
```

### N08 — ArbiterUnit
```
Verdict: PASS
Findings:
  - GetRateQuoteAsync: rate quote with service options — PASS
  - CreateShipmentAsync: label generation with tracking number — PASS
  - TrackShipmentAsync: real-time tracking status — PASS
  - OAuth: token refresh implemented — PASS
```

### N09 — FormatterUnit
```
FedExShippingConnector complete.
Files generated: 5
ConnectorName: FedExShippingConnector
SubPhase: 4.3
```

---

## B17-L2-015-002 — Gate B for FedExShippingConnector

**Gate B:** All three handlers implemented, OAuth token refresh on 401 present, filesGenerated >= 4.

**Assertion:** N08 verdict = PASS AND filesGenerated >= 4 AND oauthRefreshPresent = true.
