# Layer 2 — Nine-Node 4.3 Pairs (017)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-017-001 — SegmentAnalyticsConnector: nine-node IntegrationConnectorFlow43

**ConnectorName:** SegmentAnalyticsConnector
**Input predicates:** ["IMPLEMENT", "API_CONTRACT"]
**TransitionType:** feature_addition

### N01 — DomainSplitterUnit
```
Task: "Build the Segment analytics connector and implement the track, identify, and group event API endpoints."
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3
```

### N02 — DomainTranslatorUnit
```
Connector: SegmentAnalyticsConnector
API: Segment HTTP Tracking API v1
Scope: Three event sinks — TrackEventHandler, IdentifyEventHandler, GroupEventHandler
Authentication: Write Key (HTTP Basic, username=write_key, password=empty)
Batching: Support batch endpoint (/v1/batch) for high-volume scenarios
```

### N03 — SchemePlannerUnit
```
ISegmentAnalyticsConnector:
  - TrackAsync(event: TrackEvent) → OperationResult<TrackResult>
  - IdentifyAsync(event: IdentifyEvent) → OperationResult<IdentifyResult>
  - GroupAsync(event: GroupEvent) → OperationResult<GroupResult>
  - BatchAsync(events: IReadOnlyList<SegmentEvent>) → OperationResult<BatchResult>
```

### N04 — DomainProcessorUnit
```
Dependencies: HttpClient (injected)
Config: SegmentWriteKey, SegmentApiEndpoint, BatchMaxSize
```

### N05 — ApplierUnit
```
Files:
1. SegmentAnalyticsConnector.cs
2. ISegmentAnalyticsConnector.cs
3. SegmentModels.cs (TrackEvent, IdentifyEvent, GroupEvent, SegmentEvent, BatchResult)
4. SegmentConfig.cs
```

### N06 — ResultTranslatorUnit
```
TrackAsync: POST /v1/track
IdentifyAsync: POST /v1/identify
GroupAsync: POST /v1/group
BatchAsync: POST /v1/batch (max 500 events per batch)
```

### N07 — ArbiterSelectorUnit
```
ArbiterRequests:
  - completeness: 4 methods implemented (track, identify, group, batch)
  - batch-limit: max 500 events per batch enforced
```

### N08 — ArbiterUnit
```
Verdict: PASS
Findings:
  - TrackAsync: POST /v1/track with userId, event, properties — PASS
  - IdentifyAsync: POST /v1/identify with traits — PASS
  - GroupAsync: POST /v1/group with groupId — PASS
  - BatchAsync: POST /v1/batch, max 500 events validated — PASS
```

### N09 — FormatterUnit
```
SegmentAnalyticsConnector complete.
Files generated: 4
ConnectorName: SegmentAnalyticsConnector
SubPhase: 4.3
```

---

## B17-L2-017-002 — Gate B for SegmentAnalyticsConnector

**Gate B:** All four methods implemented (Track, Identify, Group, Batch), batch size limit enforced, filesGenerated >= 3.

**Assertion:** N08 verdict = PASS AND filesGenerated >= 3 AND batchLimitEnforced = true.
