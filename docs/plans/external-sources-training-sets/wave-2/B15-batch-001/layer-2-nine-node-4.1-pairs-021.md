# Layer 2 — Nine-Node Flow Pairs (021)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: nine_node_flow (ImplementationFlow41 + DocumentationFlow21)

## B15-nine-021 — ReportingService (IMPLEMENT+DOCUMENT)

Request: "Build the reporting service and document the report generation API with endpoint specs and sample payloads."

### Two flows: ImplementationFlow41 parallel with DocumentationFlow21

**Node 1 — DomainSplitterUnit**: Target: ReportingService (feature_addition) | A = SelectedLogicalTransition: TransitionType: feature_addition, SubPhaseIds: ["4.1", "2.1"], PredicateIds: ["IMPLEMENT", "DOCUMENT"] | ImplementationFlow41 and DocumentationFlow21 run in parallel.

**Node 2 — DomainTranslatorUnit**: Gate B: service must implement: (1) Report Generation — build report from data source query; (2) Scheduling — run reports on cron schedule; (3) Report Delivery — emit report to destination (email/storage). Documentation must cover: endpoint specs, request/response schemas, sample payloads, scheduling config.

**Node 3 — SchemePlannerUnit**: probationary_accepted — scope valid; IReportDataSource interface approved; IReportDelivery interface approved.

**Node 4 — DomainProcessorUnit**: WorkBranches:
- ReportGenerator: IReportingService.GenerateReport(ReportRequest) → OperationResult<ReportResult>; BR-RPT-001: report must specify data source and output format.
- ReportScheduler: IReportingService.ScheduleReport(ScheduleConfig) → OperationResult<ScheduleId>; BR-RPT-002: cron expression must be valid.
- ReportDelivery: IReportDelivery.Deliver(ReportResult, DeliveryTarget) → OperationResult<DeliveryConfirmation>.

**Node 5 — ApplierUnit**: Implementation files: IReportingService.cs, ReportingService.cs, IReportDataSource.cs, IReportDelivery.cs, ReportScheduler.cs, ReportGenerator.cs.
Documentation output: API spec document for /reports/generate, /reports/schedule endpoints with OpenAPI (Open API Specification) format, sample payloads.

**Node 6 — ResultTranslatorUnit**: 6 implementation files + 1 API spec document. 3 interfaces, 2 business rules. Test surface: 9 recommended cases.

**Node 7 — ArbiterSelectorUnit**: Arbiters: implementation-completeness-arbiter and documentation-completeness-arbiter (parallel).

**Node 8 — ArbiterUnit**: Implementation verdict: PASS. Documentation verdict: PASS (endpoint specs complete, sample payloads included).

**Node 9 — FormatterUnit**: Verdict: IMPLEMENT_APPROVED (IMPLEMENT+DOCUMENT) | TransitionType: feature_addition | Files: 7 (6 impl + 1 doc) | TestSurface: 9 cases
