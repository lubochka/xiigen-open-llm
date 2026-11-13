# External Training Rows — Batch 6
## Row IDs: EXT-111 through EXT-130
## Date: 2026-05-04
## Focus: Group 9 SQL/EF, Group 10 legacy JS, Group 13 test frameworks, Group 15 agent/LLM, Group 16 language corpora, CWE 15–17

---

## EXT-111

```json
{
  "rowId": "EXT-111",
  "sourceIds": ["SQL-SERVER-SAMPLES", "ADVENTUREWORKS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "stored-procedure-query-efficiency",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this SQL result: a stored procedure fetches the top 100 orders for a sales report. For each order it executes a second query to retrieve the customer name. The procedure uses a cursor to iterate each order row and executes the customer lookup inside the cursor body.",
    "b": "CREATE PROCEDURE GetTopOrdersWithCustomer AS BEGIN DECLARE @OrderId INT; DECLARE cur CURSOR FOR SELECT TOP 100 SalesOrderID FROM Sales.SalesOrderHeader ORDER BY TotalDue DESC; OPEN cur; FETCH NEXT FROM cur INTO @OrderId; WHILE @@FETCH_STATUS = 0 BEGIN SELECT c.FirstName + ' ' + c.LastName FROM Sales.Customer sc JOIN Person.Person c ON c.BusinessEntityID = sc.PersonID WHERE sc.CustomerID = (SELECT CustomerID FROM Sales.SalesOrderHeader WHERE SalesOrderID = @OrderId); FETCH NEXT FROM cur INTO @OrderId; END; CLOSE cur; DEALLOCATE cur; END",
    "c": "Context: AdventureWorks Sales schema. This is a classic N+1 query anti-pattern implemented in T-SQL with an explicit cursor. 100 orders = 101 database round-trips: 1 to fetch the order list, 100 to look up customer names. For a sales report that runs frequently, this causes significant server load. The join should be done in the initial SELECT."
  },
  "function": "Apply storedProcedureEfficiencyArbiter: detect cursor-based row-by-row processing where a single set-based JOIN would be correct. Produce the rewritten query.",
  "acceptedOutput": {
    "decision": "REJECT. Cursor with per-row SELECT executes 101 queries for 100 orders (N+1 pattern). The customer name is available via a join in the original query. Cursor overhead on SQL Server includes: lock acquisition per FETCH, per-row plan execution, and inability to use batch processing optimizations.",
    "reprocessInstruction": "Rewrite as a single set-based query: SELECT TOP 100 soh.SalesOrderID, soh.TotalDue, p.FirstName + ' ' + p.LastName AS CustomerName FROM Sales.SalesOrderHeader soh JOIN Sales.Customer sc ON sc.CustomerID = soh.CustomerID JOIN Person.Person p ON p.BusinessEntityID = sc.PersonID ORDER BY soh.TotalDue DESC; This executes as one query, uses the existing indexes on CustomerID and BusinessEntityID, and returns all 100 rows in one round-trip."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The cursor is explicit and easy to understand. Each iteration fetches exactly the data it needs."
  },
  "arbiters": ["storedProcedureEfficiencyArbiter"],
  "whyAccepted": "N+1 is a correctness issue at scale, not just a style preference. For a report that runs on every sales dashboard refresh, 100 extra queries per report adds measurable latency and increases CPU load on SQL Server. Set-based operations are the fundamental design principle of relational query engines.",
  "whyRejected": "Readability does not justify N+1 execution. The set-based rewrite is equally readable and executes in one round-trip. 'Fetches exactly what it needs' ignores that it fetches in 100 separate operations where one would be sufficient.",
  "sourceAnchors": [
    {
      "sourceId": "SQL-SERVER-SAMPLES",
      "url": "https://github.com/microsoft/sql-server-samples",
      "section": "AdventureWorks stored procedure patterns — cursor vs set-based operations",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-112

```json
{
  "rowId": "EXT-112",
  "sourceIds": ["EF6-SOURCE", "EFCORE-SOURCE"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "A_TOPOLOGY",
  "subdomain": "orm-migration",
  "rowClass": "migration-decision",
  "input": {
    "a": "Reprocessing decision: a .NET Framework 4.7 application uses Entity Framework 6 with an EDMX designer file (Database First). The team wants to migrate to .NET 8 and EF Core 8. The application has 45 entities, 12 stored procedures mapped in the EDMX, and 3 developers who know the EDMX designer.",
    "b": "History: EF6 EDMX was last updated 2023-11-01. EF Core 8 does not support EDMX — no direct migration path exists for EDMX files. EF Core supports Code First (DbContext + entity classes) and Scaffold-DbContext for Database First (generates C# classes from the existing schema). The 12 stored procedure mappings must be re-implemented as FromSqlRaw or Dapper calls.",
    "c": "Context: EF6 EDMX (Entity Data Model XML) is not supported in EF Core. The migration requires converting entity definitions from EDMX XML to C# POCO (Plain Old CLR Object) classes and a DbContext. Scaffold-DbContext can generate the initial C# classes from the existing database, reducing manual work for 45 entities. Stored procedures require manual re-implementation."
  },
  "function": "Produce the reprocessing decision for the EDMX-to-EF Core migration. Specify the two viable paths, their effort estimates, and the blocking prerequisite.",
  "acceptedOutput": {
    "reprocessingDecision": "PROCEED — path selection required before starting.",
    "viablePaths": [
      {
        "path": "A — Scaffold-DbContext (recommended)",
        "description": "Run dotnet ef dbcontext scaffold against the existing database to generate C# entity classes and DbContext. Review and clean up generated code. Eliminates manual entity class authoring for all 45 entities. Stored procedures (12) must be re-implemented manually as DbContext.FromSqlRaw<T> calls or replaced with Dapper queries.",
        "effortEstimate": "2–3 weeks: 1 week scaffold + cleanup; 1–2 weeks for stored procedure re-implementation and integration testing.",
        "blockingPrerequisite": "The existing database schema must be accessible from the development environment for scaffolding to run."
      },
      {
        "path": "B — Manual Code First",
        "description": "Write C# entity classes and DbContext manually using the EDMX XML as reference. Higher effort for 45 entities but gives developers direct control over the data model.",
        "effortEstimate": "4–6 weeks.",
        "blockingPrerequisite": "None beyond team availability."
      }
    ],
    "doNotDo": "Do not attempt to convert the EDMX XML to Code First manually by parsing the EDMX file — the structure diverges significantly between EF6 EDMX and EF Core conventions. Use Scaffold-DbContext for the initial model."
  },
  "rejectedOutput": {
    "reprocessingDecision": "Block the migration. EF Core does not support EDMX. The migration cannot proceed until Scaffold-DbContext support for EDMX import is added to EF Core."
  },
  "arbiters": ["migrationPathArbiter"],
  "whyAccepted": "The absence of EDMX import in EF Core is not a blocker — it is a path constraint. Scaffold-DbContext provides a concrete, documented alternative that generates equivalent C# artifacts from the database itself. The EDMX file is not needed as input.",
  "whyRejected": "The rejected output treats a missing direct migration path as a hard blocker. This confuses 'no direct path' with 'no path'. The team can reach EF Core 8 via database scaffolding regardless of the EDMX limitation.",
  "sourceAnchors": [
    {
      "sourceId": "EF6-SOURCE",
      "url": "https://github.com/dotnet/ef6",
      "section": "EDMX model format — entity type mapping in XML",
      "license": "Apache-2.0"
    },
    {
      "sourceId": "EFCORE-SOURCE",
      "url": "https://github.com/dotnet/efcore",
      "section": "Scaffold-DbContext command — reverse engineering from existing database",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-113

```json
{
  "rowId": "EXT-113",
  "sourceIds": ["JQUERY"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "legacy-dom-coupling",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this jQuery result: a form submission handler is attached using $('#submit-btn').click(handler). The button's ID is 'submit-btn'. The handler reads the form field values using $('#user-email').val() and $('#user-name').val().",
    "b": "$(document).ready(function() { $('#submit-btn').click(function() { var email = $('#user-email').val(); var name = $('#user-name').val(); $.ajax({ url: '/api/register', method: 'POST', data: { email: email, name: name } }); }); }); — the handler is registered in a separate JS file from the HTML template that defines the IDs.",
    "c": "Context: jQuery DOM coupling anti-pattern. The JavaScript is coupled to three specific HTML element IDs: submit-btn, user-email, user-name. If any HTML element is renamed, moved to a different template, or wrapped in a new container, the JavaScript silently breaks — .val() on a non-existent element returns undefined, and $.ajax sends {email: undefined, name: undefined} with no error thrown."
  },
  "function": "Apply domCouplingArbiter: detect jQuery selectors targeting unstable HTML IDs with no defensive check for element existence. Produce the correct pattern.",
  "acceptedOutput": {
    "decision": "REJECT. Three ID-based selectors are coupled to HTML that lives in a separate file. Silent failure mode: if any ID is missing, jQuery returns an empty jQuery object (length 0), .val() returns undefined, and the form submission sends undefined fields to the API with no browser error or console warning by default.",
    "reprocessInstruction": "Two fixes required. Fix 1 — use data attributes instead of IDs to decouple behavior from styling concerns: <button data-action='register-submit'>. $('[data-action=register-submit]').click(handler). Fix 2 — add existence guard: var $btn = $('[data-action=register-submit]'); if (!$btn.length) return; This prevents silent failure when the element is absent. Fix 3 — use the form submit event instead of button click to handle keyboard submission: $('form[data-form=register]').on('submit', handler)."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. jQuery's ID selectors are fast (document.getElementById internally). The handler correctly reads the form values and submits them."
  },
  "arbiters": ["domCouplingArbiter"],
  "whyAccepted": "Fast selector performance is irrelevant to the coupling problem. The audit concern is silent failure when HTML changes — a correctness issue, not a performance issue. ID-based coupling fails silently across file boundaries.",
  "whyRejected": "The rejected output evaluates selector performance and functional correctness when all IDs exist. It does not evaluate the fragility of coupling across file boundaries or the silent failure mode when elements are absent.",
  "sourceAnchors": [
    {
      "sourceId": "JQUERY",
      "url": "https://github.com/jquery/jquery",
      "section": "jQuery selector return value — empty jQuery object on no match, .val() returns undefined",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-114

```json
{
  "rowId": "EXT-114",
  "sourceIds": ["KNOCKOUTJS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "legacy-computed-observable",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this Knockout.js result: a computed observable is used to update a CSS class on a DOM element directly. The computed function reads a boolean observable 'isValid' and calls document.getElementById('status-icon').className = isValid() ? 'icon-ok' : 'icon-error'.",
    "b": "self.statusDisplay = ko.computed(function() { var valid = self.isValid(); document.getElementById('status-icon').className = valid ? 'icon-ok' : 'icon-error'; return valid; }); — the computed observable has a DOM side effect inside its body.",
    "c": "Context: Knockout.js computed observables are meant to be pure — they read observables and return a derived value. DOM mutation inside a computed observable bypasses Knockout's binding system, creating a side channel that does not participate in the declarative binding lifecycle. If the status-icon element does not exist at the time the computed evaluates (e.g., during initial data binding before the DOM is ready), document.getElementById returns null and className assignment throws a TypeError."
  },
  "function": "Apply computedObservablePurityArbiter: detect imperative DOM mutations inside computed observable bodies. Produce the declarative binding equivalent.",
  "acceptedOutput": {
    "decision": "REJECT. ko.computed must be pure — it should read observables and return a derived value without side effects. Mutating document.getElementById().className inside a computed: (1) bypasses Knockout's dependency tracking for that DOM property; (2) executes before DOM binding is complete on first evaluation, causing a null reference if the element does not exist yet; (3) creates a second update path for the same DOM property that Knockout's data-bind cannot see or manage.",
    "reprocessInstruction": "Replace the computed DOM mutation with a declarative binding. Define a pure computed: self.statusIconClass = ko.computed(function() { return self.isValid() ? 'icon-ok' : 'icon-error'; }); In the HTML, bind the CSS class declaratively: <span id='status-icon' data-bind='css: statusIconClass'></span>. Knockout manages the DOM update through its binding system, after the DOM is ready."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The computed observable correctly responds to changes in isValid() and updates the icon. The function achieves the intended visual feedback."
  },
  "arbiters": ["computedObservablePurityArbiter"],
  "whyAccepted": "Correct functional output at runtime does not make the pattern correct. The DOM mutation side effect creates a null-reference risk during initialization and a second unmanaged update path. Declarative bindings exist precisely so Knockout can manage update timing and lifecycle.",
  "whyRejected": "The rejected output evaluates only whether the icon updates correctly when isValid changes, not whether the update pathway is architecturally correct. A side effect that works most of the time is not acceptable when the initialization timing failure mode causes an uncaught TypeError.",
  "sourceAnchors": [
    {
      "sourceId": "KNOCKOUTJS",
      "url": "https://github.com/knockout/knockout",
      "section": "Computed observables — pure vs impure; declarative binding vs imperative DOM mutation",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-115

```json
{
  "rowId": "EXT-115",
  "sourceIds": ["PLAYWRIGHT"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "G_LEARNING_TRAINING",
  "subdomain": "e2e-test-decomposition",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Decompose this Playwright test requirement: write an end-to-end test that verifies a logged-in user can add a product to the cart, see the cart badge update to '1', and then proceed to checkout and see the correct item in the order summary.",
    "b": "The application runs at http://localhost:3000. There is a login page at /login, a product listing at /products, a cart accessible via a cart icon in the nav, and a checkout flow at /checkout. The test account credentials are test@example.com / TestPass123!.",
    "c": "Context: Playwright end-to-end test decomposition. The test covers three user-visible checkpoints: (1) add to cart and verify badge, (2) open cart and verify item, (3) proceed to checkout and verify order summary. Each checkpoint is a discrete verification step. The test should use Playwright's page object pattern if the application has reusable navigation patterns."
  },
  "function": "Decompose the requirement into ordered Playwright action items with assertions at each checkpoint. Produce a test structure with setup, actions, and assertions.",
  "acceptedOutput": {
    "actionItems": [
      "Setup: navigate to /login, fill email with 'test@example.com', fill password with 'TestPass123!', click the login button, wait for navigation to /products.",
      "Action 1: On /products, locate the first product card. Click 'Add to cart'. Assert: the cart badge in the nav shows the text '1' (await expect(page.locator('[data-testid=cart-badge]')).toHaveText('1')).",
      "Action 2: Click the cart icon to open the cart. Assert: the cart contains one item matching the product name clicked in Action 1. Use a data-testid selector or product heading text to verify the correct item is shown.",
      "Action 3: Click 'Proceed to checkout'. Wait for navigation to /checkout. Assert: the order summary section contains the product name. Assert: the quantity shown is 1.",
      "Assertions summary: 3 explicit assertions at 3 checkpoints. No assertion on intermediate navigation URLs — only on visible state that the user would observe.",
      "Test isolation: use beforeEach to log in, so the test does not depend on prior test state. Clear the cart via API call or storage reset before the test if the framework supports it."
    ]
  },
  "rejectedOutput": {
    "actionItems": [
      "Write a test that clicks add-to-cart and then checks the checkout page."
    ]
  },
  "arbiters": ["testDecompositionArbiter"],
  "whyAccepted": "The accepted decomposition identifies three discrete checkpoints with specific locator strategies, assertion types, and test isolation requirements. Each action item maps to one user-observable state change. The isolation instruction prevents test interdependency.",
  "whyRejected": "The rejected output collapses three checkpoints into one vague instruction. It does not specify locators, assertion methods, intermediate verifications, or test isolation. A developer cannot implement a Playwright test from this decomposition without re-analyzing the requirement.",
  "sourceAnchors": [
    {
      "sourceId": "PLAYWRIGHT",
      "url": "https://github.com/microsoft/playwright",
      "section": "Test structure — actions, assertions, and test isolation with beforeEach",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-116

```json
{
  "rowId": "EXT-116",
  "sourceIds": ["NUNIT"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "G_LEARNING_TRAINING",
  "subdomain": "test-isolation",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this NUnit test structure: a test class has a static field 'processedCount' that is incremented in each test method. The field is used to assert that a total number of records was processed across all tests. The field is initialized in the class definition (not in a Setup method).",
    "b": "[TestFixture] public class RecordProcessorTests { private static int processedCount = 0; [Test] public void ProcessBatch_SingleRecord_ShouldProcess() { var result = processor.Process(singleRecord); processedCount += result.Count; Assert.That(processedCount, Is.GreaterThan(0)); } [Test] public void ProcessBatch_MultipleRecords_ShouldProcessAll() { var result = processor.Process(multipleRecords); processedCount += result.Count; Assert.That(processedCount, Is.EqualTo(4)); } }",
    "c": "Context: NUnit test isolation. Static state shared between tests creates test order dependency — the second test's assertion (processedCount == 4) depends on the first test having run and added its count first. NUnit does not guarantee test execution order within a fixture unless [Order] is applied. If tests run in a different order, the second test fails because processedCount starts at 1 (only its own batch), not at 4."
  },
  "function": "Apply testIsolationArbiter: detect shared mutable state between test methods. Identify the order-dependency risk and produce the isolated equivalent.",
  "acceptedOutput": {
    "decision": "REJECT. processedCount is static mutable state shared across test methods. The ProcessBatch_MultipleRecords assertion (processedCount == 4) assumes ProcessBatch_SingleRecord ran first and contributed 1 to the count. NUnit does not guarantee execution order by default. If the multi-record test runs first, processedCount will be 3, not 4, and the test fails non-deterministically.",
    "reprocessInstruction": "Each test must be self-contained. Remove the static processedCount field. Assert on the return value of the specific call under test: [Test] public void ProcessBatch_SingleRecord_ShouldProcess() { var result = processor.Process(singleRecord); Assert.That(result.Count, Is.EqualTo(1)); } [Test] public void ProcessBatch_MultipleRecords_ShouldProcessAll() { var result = processor.Process(multipleRecords); Assert.That(result.Count, Is.EqualTo(3)); } If cumulative behavior must be tested, do it in a single test that calls Process twice."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. Both tests pass when run in the default order. The static field correctly accumulates the count across test executions."
  },
  "arbiters": ["testIsolationArbiter"],
  "whyAccepted": "Tests that pass only in a specific execution order are not reliable tests. 'Pass when run in the default order' is not a test correctness criterion — the order is not guaranteed. Non-deterministic failures discovered in CI are more expensive to diagnose than isolated test rewrites.",
  "whyRejected": "The rejected output validates the tests against the current run order, which happens to work. It does not evaluate the structural dependency on execution order, which is the defect.",
  "sourceAnchors": [
    {
      "sourceId": "NUNIT",
      "url": "https://github.com/nunit/nunit",
      "section": "Test isolation — shared state between test methods causes order-dependent failures",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-117

```json
{
  "rowId": "EXT-117",
  "sourceIds": ["LANGGRAPH"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "A_TOPOLOGY",
  "subdomain": "graph-state-management",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Reprocessing decision: a LangGraph agent processes document review requests. Each request involves 4 nodes: extract (pulls key clauses), analyze (applies compliance rules), summarize (condenses findings), format (formats for output). The graph runs on every request. The team is deciding whether to checkpoint state after the analyze node so that failed or timeout requests can resume from that point.",
    "b": "History: without checkpointing, a timeout at the format node requires re-running all 4 nodes. With checkpointing after analyze, a timeout at format resumes from the analyze output — re-running only summarize and format. The analyze node calls an external LLM for compliance analysis (cost: $0.12 per run). The format node is a deterministic string formatter (cost: near-zero). The extract node also calls an LLM (cost: $0.08 per run).",
    "c": "Context: LangGraph MemorySaver or external checkpoint store. Checkpointing adds latency (state serialization + storage write) but saves re-computation cost on failure. The decision should be based on: failure rate, re-run cost saved, and checkpoint overhead. If failures are rare and the nodes are fast, checkpointing may cost more in steady-state latency than it saves on failure recovery."
  },
  "function": "Produce the reprocessing decision: whether to checkpoint after analyze, and the quantitative rationale.",
  "acceptedOutput": {
    "reprocessingDecision": "CHECKPOINT after analyze — recommended.",
    "rationale": "The analyze node costs $0.12 (LLM call) and the extract node costs $0.08. A timeout at format wastes $0.20 per occurrence without checkpointing. With checkpointing after analyze, a format failure reprocesses only summarize + format at near-zero cost. At a failure rate of 2% on 500 requests/day: expected daily waste without checkpointing = 0.02 × 500 × $0.20 = $2.00/day. Checkpoint overhead (state write after analyze) adds approximately 50–100ms per request. At 500 requests/day, that is acceptable latency overhead for $2.00/day in savings.",
    "checkpointPlacement": "Place the checkpoint after the analyze node, not after extract. Analyze produces the compliance findings that are expensive to recompute. Extract can re-run cheaply if analyze fails (rare — analyze runs after extract succeeds). No checkpoint is needed after format — format failures can always re-run from the analyze checkpoint."
  },
  "rejectedOutput": {
    "reprocessingDecision": "Do not checkpoint. Checkpointing adds complexity and latency. The graph is simple enough to re-run from the start on any failure."
  },
  "arbiters": ["checkpointPlacementArbiter"],
  "whyAccepted": "The accepted decision is based on a cost-failure-rate calculation. Checkpointing is justified when the expected cost of re-runs exceeds the checkpoint overhead — which it does at 2% failure rate on the $0.20 per-run cost of the two LLM nodes.",
  "whyRejected": "The rejected output dismisses checkpointing as complexity without a cost analysis. 'Simple enough to re-run' is correct for cheap deterministic operations but incorrect for $0.20 LLM calls. The decision must be based on the economics of failure recovery, not graph complexity.",
  "sourceAnchors": [
    {
      "sourceId": "LANGGRAPH",
      "url": "https://github.com/langchain-ai/langgraph",
      "section": "Checkpointing — MemorySaver, checkpoint placement strategy, resume from checkpoint",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-118

```json
{
  "rowId": "EXT-118",
  "sourceIds": ["AUTOGEN"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "A_TOPOLOGY",
  "subdomain": "multi-agent-routing",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this AutoGen multi-agent routing result: a user asks 'What is the revenue impact of switching to a subscription pricing model?' The orchestrator routes this to the DataAnalystAgent because the question contains the word 'revenue'.",
    "b": "Routing rule: if message.lower().contains('revenue') or message.lower().contains('data') or message.lower().contains('numbers'): route_to(DataAnalystAgent). The question 'What is the revenue impact...' matches 'revenue' and is routed to DataAnalystAgent.",
    "c": "Context: AutoGen multi-agent routing. Keyword-based routing is a syntactic heuristic — it matches surface tokens, not intent. The question 'revenue impact of switching to subscription pricing' is a strategic business modeling question requiring business strategy analysis (PricingStrategyAgent, BusinessModelAgent) plus financial modeling (DataAnalystAgent). Routing to only DataAnalystAgent misses the strategic framing that is necessary to answer the question correctly."
  },
  "function": "Apply multiAgentRoutingArbiter: detect keyword-based routing that misclassifies intent-driven questions. Identify the correct agent composition and routing rationale.",
  "acceptedOutput": {
    "decision": "REJECT. The routing rule matches 'revenue' as a surface token and assigns the question to DataAnalystAgent. The question is: 'What is the revenue impact of switching to a subscription pricing model?' This requires: (1) business model analysis — what a subscription model means for the product and customer segment (BusinessModelAgent); (2) pricing strategy — what price points are viable given the market (PricingStrategyAgent); (3) financial modeling — what revenue scenarios look like under different churn and conversion assumptions (DataAnalystAgent). A single DataAnalystAgent can model the revenue curves but cannot frame the strategic preconditions.",
    "reprocessInstruction": "Route to a sequential agent chain or a coordinating orchestrator: BusinessModelAgent → PricingStrategyAgent → DataAnalystAgent → synthesize. Alternatively, route to a StrategicAdvisorAgent that has access to all three domains. Do not route based on token presence alone; classify intent first."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The question mentions revenue, which is a data analysis domain. DataAnalystAgent is the correct destination."
  },
  "arbiters": ["multiAgentRoutingArbiter"],
  "whyAccepted": "The keyword 'revenue' appears in questions about financial data (DataAnalystAgent) and in strategic business modeling questions (BusinessModelAgent + PricingStrategyAgent). Syntactic matching cannot distinguish intent. The question's structure — 'impact of switching to a model' — signals strategic framing, not data retrieval.",
  "whyRejected": "The rejected output accepts keyword routing without evaluating intent. Revenue as a keyword in a data retrieval question ('what was last quarter revenue?') correctly routes to DataAnalystAgent. Revenue in a strategy framing question ('impact of switching model') routes incorrectly.",
  "sourceAnchors": [
    {
      "sourceId": "AUTOGEN",
      "url": "https://github.com/microsoft/autogen",
      "section": "Agent routing — intent-based vs keyword-based routing in multi-agent conversations",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-119

```json
{
  "rowId": "EXT-119",
  "sourceIds": ["TATOEBA"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "C_SEMANTIC_TRANSLATION",
  "subdomain": "multilingual-translation-quality",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Decompose this translation command: translate the English sentence 'The flight was delayed because of a technical problem with the aircraft' into French. Produce the action items for the translation quality check.",
    "b": "Source: English. Target: French. The sentence contains: a passive construction ('was delayed'), a causal connective ('because of'), and a compound noun phrase ('technical problem with the aircraft'). These are three independently verifiable translation components.",
    "c": "Context: Tatoeba multilingual sentence pair corpus (CC-BY 2.0). Translation quality for complex sentences requires verifying: (1) grammatical structure preservation, (2) causal connective correctness, (3) noun phrase accuracy, and (4) register consistency. A literal word-for-word translation often fails on passive construction and causal connectives in French."
  },
  "function": "Produce the action items for translating and verifying the sentence. Specify the accepted translation and the verification criteria for each component.",
  "acceptedOutput": {
    "actionItems": [
      "Translate: 'Le vol a été retardé en raison d'un problème technique avec l'appareil.'",
      "Verify 1 — passive construction: 'a été retardé' is the passé composé passive (avoir + été + past participle). Correct. 'était retardé' (imparfait passive) would suggest ongoing delay rather than a completed event — incorrect for a specific flight delay.",
      "Verify 2 — causal connective: 'en raison de' (formal, written French for 'because of') is correct. 'à cause de' is also acceptable (slightly more informal). 'parce que' would require a full clause ('parce qu'il y avait un problème') — not used with a noun phrase.",
      "Verify 3 — noun phrase: 'un problème technique avec l'appareil' — 'appareil' (aircraft/device) is correct in aviation context. 'avion' is also correct and slightly more common in informal French. 'l'aéronef' is formal/technical French — acceptable in official communications.",
      "Verify 4 — register: the translation is neutral-formal, consistent with the source. No colloquialisms introduced."
    ]
  },
  "rejectedOutput": {
    "actionItems": [
      "Translate: 'Le vol était retardé parce que d'un problème technique avec l'avion.'",
      "Verify: the translation conveys the same meaning as the source."
    ]
  },
  "arbiters": ["translationQualityArbiter"],
  "whyAccepted": "The accepted translation uses the correct tense (passé composé for a completed event), the correct causal connective for a noun phrase ('en raison de'), and the correct register. Each component is verified independently.",
  "whyRejected": "The rejected translation has two grammatical errors: 'était retardé' (imparfait) implies an ongoing/background state, not a completed event; 'parce que d'un' is ungrammatical — 'parce que' requires a clause. The verification step 'conveys the same meaning' is not decomposed into falsifiable checks.",
  "sourceAnchors": [
    {
      "sourceId": "TATOEBA",
      "url": "https://tatoeba.org",
      "section": "English-French sentence pair — aviation delay sentence with passive and causal connective",
      "license": "CC-BY-2.0"
    }
  ],
  "licenseConstraint": "CC-BY-2.0 — attribution required: Tatoeba (tatoeba.org)",
  "trainingStatus": "candidate"
}
```

---

## EXT-120

```json
{
  "rowId": "EXT-120",
  "sourceIds": ["BIGCODEBENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "function-implementation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Implement: fetch_with_timeout(url: str, timeout_seconds: float = 5.0) -> dict. Makes an HTTP GET request to the URL. Returns a dict with keys: 'status_code' (int), 'body' (str), 'elapsed_seconds' (float). If the request times out, returns {'status_code': 0, 'body': 'TIMEOUT', 'elapsed_seconds': timeout_seconds}. If a non-timeout exception occurs, returns {'status_code': -1, 'body': str(exception), 'elapsed_seconds': elapsed}.",
    "b": "Test: fetch_with_timeout('http://httpbin.org/get', 5.0) returns dict with status_code 200 and 'elapsed_seconds' > 0. fetch_with_timeout('http://httpbin.org/delay/10', 1.0) returns {'status_code': 0, 'body': 'TIMEOUT', 'elapsed_seconds': 1.0}.",
    "c": "Context: BigCodeBench function implementation. The function must handle three cases: success, timeout (requests.Timeout or requests.ConnectTimeout), and other exceptions. The 'elapsed_seconds' field must be populated from the requests.Response.elapsed attribute on success, and from the measured wall time on timeout or exception."
  },
  "function": "Implement fetch_with_timeout that handles success, timeout, and general exception cases with the specified return structure.",
  "acceptedOutput": {
    "implementation": "import time\nimport requests\n\ndef fetch_with_timeout(url: str, timeout_seconds: float = 5.0) -> dict:\n    start = time.monotonic()\n    try:\n        response = requests.get(url, timeout=timeout_seconds)\n        return {\n            'status_code': response.status_code,\n            'body': response.text,\n            'elapsed_seconds': response.elapsed.total_seconds()\n        }\n    except (requests.Timeout, requests.ConnectTimeout):\n        return {\n            'status_code': 0,\n            'body': 'TIMEOUT',\n            'elapsed_seconds': timeout_seconds\n        }\n    except Exception as e:\n        elapsed = time.monotonic() - start\n        return {\n            'status_code': -1,\n            'body': str(e),\n            'elapsed_seconds': elapsed\n        }",
    "notes": "Uses time.monotonic() for wall-time measurement in the exception cases. response.elapsed.total_seconds() is used on success because it measures server round-trip time excluding connection overhead in some implementations. Timeout case returns timeout_seconds as elapsed because the actual wall time will be approximately equal and timeout_seconds is the documented contract."
  },
  "rejectedOutput": {
    "implementation": "import requests\n\ndef fetch_with_timeout(url, timeout_seconds=5.0):\n    try:\n        r = requests.get(url, timeout=timeout_seconds)\n        return {'status_code': r.status_code, 'body': r.text, 'elapsed_seconds': 0}\n    except:\n        return {'status_code': -1, 'body': 'error', 'elapsed_seconds': 0}"
  },
  "arbiters": ["implementationCorrectnessArbiter"],
  "whyAccepted": "The accepted implementation: distinguishes timeout exceptions (status_code 0) from general exceptions (status_code -1); uses response.elapsed for success; measures wall time for exception cases; uses type annotation. All three return cases match the specification.",
  "whyRejected": "The rejected implementation: returns elapsed_seconds=0 always (fails the elapsed_seconds > 0 test); does not distinguish timeout from general exceptions (returns -1 for both, but the spec requires 0 for timeout); uses bare except (catches KeyboardInterrupt); discards the exception message.",
  "sourceAnchors": [
    {
      "sourceId": "BIGCODEBENCH",
      "url": "https://github.com/bigcode-project/bigcodebench",
      "section": "HTTP request with timeout and error classification task instances",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-121

```json
{
  "rowId": "EXT-121",
  "sourceIds": ["SWE-BENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "domainResultsArray_to_resultI",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "multi-tool-result-aggregation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Aggregate these three domain results from a CI pipeline check for a pull request. The requestor wants to know if the PR is safe to merge.",
    "b": "test_result = { status: 'SUCCESS', passed: 142, failed: 0, skipped: 3 }; lint_result = { status: 'WARN', violations: [ { rule: 'max-line-length', count: 7, severity: 'warning' } ], errors: 0 }; typecheck_result = { status: 'SUCCESS', errors: 0, filesChecked: 34 }.",
    "c": "Context: domainResultsArray_to_resultI. The aggregation must produce an overallStatus that reflects the merge readiness. WARN from lint with zero errors and zero test failures is a non-blocking advisory — it should not prevent merge but must be surfaced. The merged resultI feeds the arbiter which enforces merge gates."
  },
  "function": "Aggregate the three CI domain results into a resultI. Determine the overallStatus and surface the lint advisory without blocking the merge.",
  "acceptedOutput": {
    "resultI": {
      "requestId": "req_pr_check_4471",
      "overallStatus": "PASS_WITH_ADVISORY",
      "mergeReady": true,
      "summary": {
        "tests": { "status": "SUCCESS", "passed": 142, "failed": 0, "skipped": 3 },
        "lint": { "status": "WARN", "violations": 7, "severity": "warning", "errors": 0, "blocksMerge": false },
        "typecheck": { "status": "SUCCESS", "errors": 0, "filesChecked": 34 }
      },
      "advisory": "7 line-length warnings in lint. Non-blocking — no merge gate failed. Recommend addressing before next PR to prevent accumulation.",
      "arbitrationTargets": ["ciMergeGateArbiter"],
      "mergeBlockers": []
    }
  },
  "rejectedOutput": {
    "resultI": {
      "overallStatus": "FAIL",
      "mergeReady": false,
      "reason": "lint_result status is WARN — not SUCCESS. All tools must return SUCCESS for merge to proceed."
    }
  },
  "arbiters": ["ciMergeGateArbiter"],
  "whyAccepted": "WARN with zero errors is an advisory state, not a failure. Treating lint warnings as merge blockers would halt all PRs with any style advisory, including PRs that explicitly relaxed a rule for a specific file. The resultI correctly distinguishes merge-blocking failures from advisory warnings.",
  "whyRejected": "The rejected output maps any non-SUCCESS status to merge blocked. This is a policy error: the lint tool explicitly uses WARN (not FAIL) to signal non-blocking advisories. Promoting all non-SUCCESS results to FAIL ignores the tool's severity model.",
  "sourceAnchors": [
    {
      "sourceId": "SWE-BENCH",
      "url": "https://github.com/swe-bench/SWE-bench",
      "section": "Multi-tool CI result aggregation — WARN vs FAIL distinction in merge gate policy",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-122

```json
{
  "rowId": "EXT-122",
  "sourceIds": ["OPENASSISTANT-OASST1"],
  "sourceTrustTier": 1,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "C_SEMANTIC_TRANSLATION",
  "subdomain": "result-translation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Translate this resultI for an end user (non-technical consumer). The user asked: 'Why is my order showing as delayed?' The system performed a lookup and produced a resultI with technical status codes.",
    "b": "resultI = { overallStatus: 'SUCCESS', orderId: 'ORD-88421', orderStatus: 'CARRIER_DELAY', carrierCode: 'UPS', trackingNumber: '1Z999AA1012345678', estimatedDeliveryDate: '2026-05-07', delayReason: 'WEATHER_EVENT', delayReasonDetail: 'Winter storm advisory — Louisville, KY hub, 2026-05-04', affectedFacility: 'UPS_LOUISVILLE_HUB_KY' }",
    "c": "Context: Node 6 result translator for end-user audience. End users need: what happened, what to expect, whether action is required. They do not need: carrier codes, internal facility names, technical status enumerations. The translation must be empathetic in tone and must not expose internal routing details."
  },
  "function": "Translate the resultI for the end user. Use plain language, provide the key facts (what happened, new delivery date), and state whether action is required.",
  "acceptedOutput": {
    "finalMachineResult": {
      "audience": "end_user",
      "message": "Your order (ORD-88421) is on its way, but it's been delayed by a winter storm that's affecting the shipping carrier's facility in Kentucky. We expect it to arrive by May 7, 2026.",
      "actionRequired": false,
      "actionMessage": "You don't need to do anything — your package is still moving through the carrier's network. We'll update you if the delivery date changes.",
      "trackingAvailable": true,
      "trackingNote": "You can track your package with the shipping carrier using your tracking number."
    }
  },
  "rejectedOutput": {
    "finalMachineResult": "Order status: CARRIER_DELAY. Carrier: UPS. Tracking: 1Z999AA1012345678. Delay reason: WEATHER_EVENT at UPS_LOUISVILLE_HUB_KY. Estimated delivery: 2026-05-07. No action required."
  },
  "arbiters": ["resultTranslationArbiter"],
  "whyAccepted": "The accepted translation uses conversational language, explains the delay in human terms (winter storm, Kentucky), names the updated delivery date in a readable format (May 7), and explicitly answers 'do I need to do anything?' An end user can act on this response.",
  "whyRejected": "The rejected output is a direct field projection using internal enumeration values (CARRIER_DELAY, WEATHER_EVENT, UPS_LOUISVILLE_HUB_KY). These identifiers are meaningless to end users and the format reads as raw system output, not as a customer-facing message.",
  "sourceAnchors": [
    {
      "sourceId": "OPENASSISTANT-OASST1",
      "url": "https://huggingface.co/datasets/OpenAssistant/oasst1",
      "section": "Conversational assistant response format — user-facing clarity in status explanation",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-123

```json
{
  "rowId": "EXT-123",
  "sourceIds": ["RUST-RFCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "G_LEARNING_TRAINING",
  "subdomain": "proposal-evaluation",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Arbitrate this Rust RFC proposal draft: 'Add a #[stable_abi] attribute to mark types whose memory layout must not change across Rust versions.' The motivation section is two sentences. The alternatives section lists 'keep the current approach' with no elaboration. The unresolved questions section is empty.",
    "b": "RFC text excerpt: 'Motivation: Rust's ABI is unstable by default. Some types need stable ABI for FFI interoperability.' Alternatives: 'We could keep the current approach.' Unresolved questions: (none listed).",
    "c": "Context: Rust RFC process requires that a proposal be complete enough for informed discussion. Per Rust RFC template: the motivation must explain why the current state is unsatisfactory with concrete examples; alternatives must evaluate at least two non-trivial alternatives with tradeoffs; unresolved questions must identify open design questions that the RFC process should resolve before stabilization."
  },
  "function": "Apply proposalCompletenessArbiter: evaluate whether the RFC has sufficient motivation, alternatives analysis, and unresolved questions identification to proceed to RFC discussion.",
  "acceptedOutput": {
    "decision": "REJECT — incomplete proposal. Three gaps block RFC discussion.",
    "gap1": "Motivation is insufficient. Two sentences describe the problem abstractly. The motivation must include: (1) a concrete failing example — what breaks today because ABI is unstable, (2) which crate categories are affected (FFI wrappers, C-compatible types, dynamic linking), (3) why the current workarounds (repr(C), extern 'C' types) are insufficient for the proposed use case.",
    "gap2": "Alternatives section lists only 'keep the current approach' with no analysis. Required: at minimum, evaluate (a) using repr(C) consistently — when is it insufficient, (b) proc-macro or build-script-based ABI verification as an alternative to a language attribute, (c) the existing cbindgen/abi-checker tooling approach.",
    "gap3": "Unresolved questions is empty. At minimum for an ABI-stability feature: (a) which types qualify — only structs or also enums? (b) what happens when a #[stable_abi] type contains a field whose type changes? (c) how does #[stable_abi] interact with generic parameters? These must be raised for community input."
  },
  "rejectedOutput": {
    "decision": "ACCEPT for discussion. The core idea is clear and the motivation is plausible. The community discussion will surface the missing details."
  },
  "arbiters": ["proposalCompletenessArbiter"],
  "whyAccepted": "Accepting an incomplete RFC for discussion creates ambiguity about what is being accepted. Productive RFC discussion requires that participants evaluate a specific, concrete proposal. Community discussion cannot substitute for the proposer's analysis of alternatives and unresolved questions.",
  "whyRejected": "The rejected output shifts the work of completing the proposal to the community discussion phase. Per the Rust RFC process, the proposer is responsible for motivation, alternatives, and unresolved questions before the proposal enters discussion — not after.",
  "sourceAnchors": [
    {
      "sourceId": "RUST-RFCS",
      "url": "https://github.com/rust-lang/rfcs",
      "section": "RFC template — motivation, alternatives, and unresolved questions sections",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-124

```json
{
  "rowId": "EXT-124",
  "sourceIds": ["PYTHON-PEPS"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G_LEARNING_TRAINING",
  "subdomain": "language-proposal-lifecycle",
  "rowClass": "migration-decision",
  "input": {
    "a": "Reprocessing decision: PEP 654 (Exception Groups and except*) was accepted and implemented in Python 3.11. A codebase still uses bare 'except Exception as e:' handlers to catch multiple exception types from async task groups, then re-raises the first exception and discards the rest.",
    "b": "History: Python 3.11 (PEP 654) introduced ExceptionGroup and the except* syntax. asyncio.TaskGroup now raises ExceptionGroup when multiple tasks fail concurrently. Code that catches the ExceptionGroup with 'except Exception as e:' receives the group but re-raises only one exception, discarding others. Three async task failures produce one logged exception.",
    "c": "Context: The reprocessing decision is whether to migrate exception handling to except* before upgrading to Python 3.11+. The migration is not optional if any code uses asyncio.TaskGroup and expects to see all failures — the current pattern silently discards concurrent failures."
  },
  "function": "Produce the reprocessing decision for migrating bare except Exception handlers to except* for asyncio.TaskGroup usage.",
  "acceptedOutput": {
    "reprocessingDecision": "MIGRATE before upgrading to Python 3.11.",
    "impact": "asyncio.TaskGroup raises ExceptionGroup when multiple tasks fail. Catching ExceptionGroup with 'except Exception as e:' receives the group object correctly — no TypeError — but re-raising 'e' re-raises the ExceptionGroup, not the individual task exceptions. Downstream handlers expecting a specific exception type (e.g., except ValueError) will not match an ExceptionGroup.",
    "migrationPattern": "Replace: try: async with asyncio.TaskGroup() as tg: tg.create_task(task1()); tg.create_task(task2()) except Exception as e: log_error(e); raise — with: try: async with asyncio.TaskGroup() as tg: tg.create_task(task1()); tg.create_task(task2()) except* ValueError as eg: for exc in eg.exceptions: log_error(exc) except* IOError as eg: handle_io_failures(eg.exceptions)",
    "migrationScope": "Audit all asyncio.TaskGroup usages. For TaskGroup patterns: migrate to except*. For non-concurrent exception handlers (single-task try/except), bare except Exception remains correct — except* is not required."
  },
  "rejectedOutput": {
    "reprocessingDecision": "No migration needed. The existing except Exception handler catches ExceptionGroup without error. The current behavior is backward compatible."
  },
  "arbiters": ["languageFeatureMigrationArbiter"],
  "whyAccepted": "Catching ExceptionGroup without TypeError is backward compatible — the handler runs. But re-raising or inspecting the caught exception as a single exception loses the concurrent failure information. The behavior change is semantic, not syntactic. 'No error thrown' does not mean 'behaves correctly'.",
  "whyRejected": "The rejected output conflates syntactic compatibility (no exception raised by the handler) with semantic correctness (all task failures are observed and handled). Silent data loss — discarded concurrent exceptions — is not backward compatibility.",
  "sourceAnchors": [
    {
      "sourceId": "PYTHON-PEPS",
      "url": "https://peps.python.org/pep-0654/",
      "section": "PEP 654 — ExceptionGroup, except* syntax, asyncio.TaskGroup interaction",
      "license": "Public domain"
    }
  ],
  "licenseConstraint": "none",
  "trainingStatus": "candidate"
}
```

---

## EXT-125

```json
{
  "rowId": "EXT-125",
  "sourceIds": ["SELENIUM"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "G_LEARNING_TRAINING",
  "subdomain": "browser-test-decomposition",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Decompose this Selenium test requirement: verify that the user profile page correctly displays a user's name and email after the user updates their profile settings.",
    "b": "Application: profile page at /profile. Edit profile page at /profile/edit. Fields: display_name (text input), email (email input). Save button updates via POST to /api/profile. After save, redirects to /profile with updated values displayed.",
    "c": "Context: Selenium WebDriver test decomposition. The test must: (1) arrange pre-test state (navigate to edit, change values), (2) act (click save), (3) assert post-save displayed values on the profile page. The assertion must target the displayed elements, not the form inputs. A common mistake is asserting that the form fields contain the new values rather than verifying that the profile display shows the updated values."
  },
  "function": "Produce the decomposed Selenium action items with distinct arrange, act, and assert phases. Identify the correct assertion targets.",
  "acceptedOutput": {
    "actionItems": [
      "Arrange 1: driver.get('/profile/edit'). Wait for the edit form to be present: WebDriverWait(driver, 10).until(EC.presence_of_element_located((By.ID, 'display_name'))).",
      "Arrange 2: Clear and fill display_name field: driver.find_element(By.ID, 'display_name').clear(); driver.find_element(By.ID, 'display_name').send_keys('Updated Name').",
      "Arrange 3: Clear and fill email field: driver.find_element(By.ID, 'email').clear(); driver.find_element(By.ID, 'email').send_keys('updated@example.com').",
      "Act: Click save button: driver.find_element(By.CSS_SELECTOR, 'button[type=submit]').click(). Wait for redirect to /profile: WebDriverWait(driver, 10).until(EC.url_contains('/profile')).",
      "Assert 1: Verify displayed name on profile page. Target: the profile display element, not the form input. driver.find_element(By.CSS_SELECTOR, '[data-testid=profile-name]').text == 'Updated Name'.",
      "Assert 2: Verify displayed email. driver.find_element(By.CSS_SELECTOR, '[data-testid=profile-email]').text == 'updated@example.com'.",
      "Note: use explicit waits (WebDriverWait) after navigation — never time.sleep(). Assert on the /profile page display elements, not on the /profile/edit form fields which may persist in browser cache."
    ]
  },
  "rejectedOutput": {
    "actionItems": [
      "Go to the edit profile page.",
      "Fill in the name and email.",
      "Click save.",
      "Check that the fields show the new values."
    ]
  },
  "arbiters": ["testDecompositionArbiter"],
  "whyAccepted": "The accepted decomposition specifies locator strategies, explicit wait patterns, and correct assertion targets (profile display elements, not form inputs after redirect). The arrange/act/assert phases are distinct. A developer can implement this without re-reading the requirement.",
  "whyRejected": "The rejected output omits: locator strategies, explicit wait requirements, the distinction between form input fields and profile display elements, and the assertion mechanism. 'Check that the fields show the new values' is ambiguous — it could mean the form fields (wrong target) or the profile display (correct target).",
  "sourceAnchors": [
    {
      "sourceId": "SELENIUM",
      "url": "https://github.com/SeleniumHQ/selenium",
      "section": "WebDriver — explicit waits, locator strategies, assertion on displayed elements vs form inputs",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-126

```json
{
  "rowId": "EXT-126",
  "sourceIds": ["CWE-TOP25"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "authentication",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this API result: an internal admin endpoint '/api/internal/users' returns a list of all user accounts including email, hashed passwords, and account status. The endpoint is protected by checking that the request originates from the internal network (IP range 10.0.0.0/8).",
    "b": "app.get('/api/internal/users', (req, res) => { const clientIp = req.headers['x-forwarded-for'] || req.socket.remoteAddress; if (!isInternalIp(clientIp)) return res.status(403).json({error: 'Forbidden'}); return db.query('SELECT * FROM users'); }); — authentication relies on IP address check via X-Forwarded-For header.",
    "c": "Context: CWE-306 Missing Authentication for Critical Function (rank 13 in CWE Top 25 2023). IP-based access control via X-Forwarded-For is not authentication — it is a network-layer hint that is trivially forged. Any client can set X-Forwarded-For: 10.0.0.1 in their request headers. The endpoint returns all user records including hashed passwords without any credential verification."
  },
  "function": "Apply missingAuthenticationArbiter: detect critical function endpoints that rely solely on network origin checks without credential authentication.",
  "acceptedOutput": {
    "decision": "REJECT. CWE-306 — Missing Authentication for Critical Function. The endpoint at /api/internal/users returns all user records including hashed passwords. It relies exclusively on the X-Forwarded-For header for access control. X-Forwarded-For is a client-controlled header — any external caller can set X-Forwarded-For: 10.0.0.1 to bypass the IP check. This is not authentication; it is a forgeable network hint.",
    "reprocessInstruction": "Replace IP-based access control with credential authentication: (1) require a bearer token or API key in the Authorization header, validated against a secrets store, not derived from request headers; (2) if the endpoint must remain network-restricted, combine IP restriction at the infrastructure layer (load balancer or service mesh policy) with token authentication — never rely on IP alone for access to data that includes credentials."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The endpoint is internal-only — external callers cannot reach the internal IP range. The IP check enforces the network boundary."
  },
  "arbiters": ["missingAuthenticationArbiter"],
  "whyAccepted": "The rejected logic assumes external callers cannot reach the endpoint. But X-Forwarded-For is a request header, not a network-layer control. An SSRF (Server-Side Request Forgery) vulnerability elsewhere in the application, a misconfigured proxy, or a compromised internal host can all send requests with a forged X-Forwarded-For header. Authentication must be credential-based.",
  "whyRejected": "The rejected output conflates network topology (internal-only route) with access control (authenticated identity). Network topology reduces the attack surface but does not eliminate it — especially for applications behind proxies or load balancers that forward X-Forwarded-For.",
  "sourceAnchors": [
    {
      "sourceId": "CWE-TOP25",
      "url": "https://cwe.mitre.org/data/definitions/306.html",
      "section": "CWE-306 Missing Authentication for Critical Function — IP-based origin check not equivalent to authentication",
      "license": "MITRE CWE Terms of Use",
      "requiredNotice": "CWE™ is a trademark of The MITRE Corporation"
    }
  ],
  "licenseConstraint": "MITRE notice required",
  "trainingStatus": "candidate"
}
```

---

## EXT-127

```json
{
  "rowId": "EXT-127",
  "sourceIds": ["CWE-TOP25"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "authorization",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this authorization result: a document management API allows authenticated users to download documents by ID. The download endpoint retrieves the document if the user is authenticated. There is no check that the requesting user is the owner of or has been granted access to the specific document.",
    "b": "GET /api/documents/:id/download — handler: async (req, res) => { if (!req.user) return res.status(401).end(); const doc = await db.documents.findById(req.params.id); if (!doc) return res.status(404).end(); return res.download(doc.filePath); } — authentication is checked (req.user exists) but authorization (does this user have access to this document?) is not.",
    "c": "Context: CWE-862 Missing Authorization (rank 11 in CWE Top 25 2023). Authentication (who are you?) and authorization (what are you allowed to do?) are separate checks. A user who is authenticated can enumerate document IDs and download documents they do not own. This is an Insecure Direct Object Reference (IDOR) vulnerability."
  },
  "function": "Apply missingAuthorizationArbiter: detect authenticated endpoints that perform object retrieval without verifying the requesting user's access rights to the specific object.",
  "acceptedOutput": {
    "decision": "REJECT. CWE-862 / IDOR (Insecure Direct Object Reference). The endpoint authenticates the user (checks req.user) but does not authorize access to the specific document. Any authenticated user can increment the document ID in the URL and download any document in the system. The user's identity is verified but their access rights to the requested document are not.",
    "reprocessInstruction": "Add ownership or permission check before serving the file: const doc = await db.documents.findById(req.params.id); if (!doc) return res.status(404).end(); if (doc.ownerId !== req.user.id && !doc.sharedWith.includes(req.user.id)) return res.status(403).end(); return res.download(doc.filePath); The authorization check (owner or explicit share) must occur after the document is retrieved and before the file is served. Return 404 for non-existent documents and 403 for unauthorized access to existing documents — do not return 404 for both (information leakage about document existence depends on context)."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The endpoint checks req.user, so only authenticated users can download documents. Unauthenticated access is blocked."
  },
  "arbiters": ["missingAuthorizationArbiter"],
  "whyAccepted": "Authentication prevents anonymous access but does not prevent authenticated users from accessing each other's documents. The two checks address different security controls. IDOR is consistently one of the most common real-world vulnerabilities because developers conflate 'user is logged in' with 'user can access this resource'.",
  "whyRejected": "The rejected output accurately describes what the authentication check does (blocks unauthenticated users) but does not evaluate whether the check is sufficient. CWE-862 is specifically about the gap between authentication and object-level authorization.",
  "sourceAnchors": [
    {
      "sourceId": "CWE-TOP25",
      "url": "https://cwe.mitre.org/data/definitions/862.html",
      "section": "CWE-862 Missing Authorization — IDOR: authenticated user accessing another user's object",
      "license": "MITRE CWE Terms of Use",
      "requiredNotice": "CWE™ is a trademark of The MITRE Corporation"
    }
  ],
  "licenseConstraint": "MITRE notice required",
  "trainingStatus": "candidate"
}
```

---

## EXT-128

```json
{
  "rowId": "EXT-128",
  "sourceIds": ["DAPPER", "EFCORE-SOURCE"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "data-access-injection",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this Dapper data access result: a search endpoint accepts a user-supplied name parameter and uses it in a Dapper query via string concatenation.",
    "b": "public async Task<IEnumerable<Customer>> SearchAsync(string name) { var sql = \"SELECT * FROM Customers WHERE Name LIKE '%\" + name + \"%'\"; return await _connection.QueryAsync<Customer>(sql); } — the name parameter is concatenated directly into the SQL string.",
    "c": "Context: Dapper parameterized query API. Dapper supports parameterized queries via anonymous object: connection.QueryAsync<T>(sql, new { name = '%' + name + '%' }) with the SQL using @name as a placeholder. String concatenation into Dapper queries produces the same SQL injection vulnerability as raw ADO.NET string concatenation — Dapper's API does not protect against injection when parameters are concatenated rather than passed as named parameters."
  },
  "function": "Apply sqlInjectionArbiter: detect Dapper queries that use string concatenation for user-supplied values instead of named parameters.",
  "acceptedOutput": {
    "decision": "REJECT. SQL injection via string concatenation in Dapper. 'LIKE '%' + name + '%'' concatenates the user-supplied name directly into the SQL text. A name value of \"'; DROP TABLE Customers;--\" produces: SELECT * FROM Customers WHERE Name LIKE '%'; DROP TABLE Customers;--%'. Dapper executes the SQL as written — it does not parameterize concatenated values.",
    "reprocessInstruction": "Use Dapper's named parameter syntax: var sql = \"SELECT * FROM Customers WHERE Name LIKE @namePattern\"; var result = await _connection.QueryAsync<Customer>(sql, new { namePattern = \"%\" + name + \"%\" }); The @namePattern placeholder is sent as a separate parameter — the database driver handles quoting and escaping. The LIKE wildcard characters (%) are part of the parameter value, not the SQL structure."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. Dapper is a micro-ORM that abstracts raw SQL. The abstraction layer protects against injection."
  },
  "arbiters": ["sqlInjectionArbiter"],
  "whyAccepted": "Dapper is a SQL execution library, not an ORM query builder. It executes SQL strings as provided. 'Abstraction layer' in Dapper means typed result mapping, not query parameterization. Parameterization is the caller's responsibility and is achieved by using @parameter placeholders with an anonymous parameter object.",
  "whyRejected": "The rejected output conflates abstraction layer (Dapper provides typed mapping) with injection protection (requires named parameters). Dapper's documentation explicitly warns that string concatenation into SQL produces the same injection risk as raw ADO.NET.",
  "sourceAnchors": [
    {
      "sourceId": "DAPPER",
      "url": "https://github.com/DapperLib/Dapper",
      "section": "Parameterized queries — @parameter syntax with anonymous object vs string concatenation",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-129

```json
{
  "rowId": "EXT-129",
  "sourceIds": ["WIDEWORLDIMPORTERS"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "multi-table-transaction",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Decompose this command: implement a stored procedure that records a new sales order in the WideWorldImporters schema. The order has a header (customer, date, salesperson) and one or more line items (stock item, quantity, unit price). Both the header and all line items must be committed atomically — no partial order should be possible.",
    "b": "Schema: Sales.Orders (OrderID PK, CustomerID, OrderDate, SalespersonPersonID); Sales.OrderLines (OrderLineID PK, OrderID FK, StockItemID, Quantity, UnitPrice). Both tables exist. The stored procedure receives: @CustomerId, @OrderDate, @SalespersonId, and a table-valued parameter @OrderLines (StockItemID, Quantity, UnitPrice).",
    "c": "Context: WideWorldImporters SQL Server sample database (MIT). The stored procedure must use an explicit transaction to ensure atomicity. On failure during OrderLines insertion, the Orders header insert must also be rolled back. The procedure must return the new OrderID on success."
  },
  "function": "Produce the action items for implementing the atomic sales order stored procedure. Include transaction scope, error handling, and return value.",
  "acceptedOutput": {
    "actionItems": [
      "Action 1: Declare @NewOrderId INT to capture the identity value from the Orders insert.",
      "Action 2: Begin an explicit transaction: BEGIN TRANSACTION;",
      "Action 3: Insert the order header: INSERT INTO Sales.Orders (CustomerID, OrderDate, SalespersonPersonID) VALUES (@CustomerId, @OrderDate, @SalespersonId); SET @NewOrderId = SCOPE_IDENTITY();",
      "Action 4: Insert all order lines from the table-valued parameter in a single set-based INSERT: INSERT INTO Sales.OrderLines (OrderID, StockItemID, Quantity, UnitPrice) SELECT @NewOrderId, StockItemID, Quantity, UnitPrice FROM @OrderLines;",
      "Action 5: Wrap both inserts in TRY/CATCH. If CATCH fires: ROLLBACK TRANSACTION; THROW; to re-raise the original error to the caller.",
      "Action 6: On success: COMMIT TRANSACTION; SELECT @NewOrderId AS NewOrderId;",
      "Transaction scope note: the @NewOrderId SCOPE_IDENTITY() must be captured inside the transaction before the commit. Capturing it after COMMIT is still correct but is a race condition in a high-concurrency environment if the session identity changes."
    ]
  },
  "rejectedOutput": {
    "actionItems": [
      "Insert into Sales.Orders.",
      "Loop through @OrderLines and insert each row.",
      "Return the new order ID."
    ]
  },
  "arbiters": ["transactionAtomicityArbiter"],
  "whyAccepted": "The accepted decomposition specifies: explicit transaction scope, TRY/CATCH rollback on failure, set-based OrderLines insert (avoids cursor N+1), and the SCOPE_IDENTITY() capture position. Each action item is implementable without re-reading the requirement.",
  "whyRejected": "The rejected decomposition omits transaction scope (no atomicity guarantee), uses a loop (cursor anti-pattern for set-based inserts), and does not specify error handling. Partial order insertion under failure is the defect this procedure must prevent.",
  "sourceAnchors": [
    {
      "sourceId": "WIDEWORLDIMPORTERS",
      "url": "https://github.com/microsoft/sql-server-samples/tree/master/samples/databases/wide-world-importers",
      "section": "WideWorldImporters schema — Sales.Orders and Sales.OrderLines with atomic insert pattern",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-130

```json
{
  "rowId": "EXT-130",
  "sourceIds": ["PROMPT-ENGINEERING-GUIDE"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A_TOPOLOGY",
  "subdomain": "prompt-strategy-selection",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "I need to classify customer support tickets into one of five categories: billing, technical, account, feature-request, or compliment. I have 2,000 labeled examples. The model I'm using is GPT-4o-mini. I want to minimize token cost while maintaining >90% accuracy on a held-out set.",
    "b": "Available strategies: zero-shot (no examples in prompt), few-shot (3–8 examples in prompt), fine-tuning (train a model on the labeled data). My labeled dataset has 2,000 examples. Inference will run on ~500 tickets/day.",
    "c": "Context: Prompt Engineering Guide — few-shot vs zero-shot vs fine-tuning selection. Zero-shot is cheapest per call but may not reach 90% accuracy on a 5-class ticket classification task. Few-shot adds 3–8 example tokens per call. Fine-tuning has a one-time training cost but produces a smaller, cheaper model for inference. With 2,000 examples, fine-tuning is viable."
  },
  "function": "Produce the command: which prompt strategy to use and why. Quantify the tradeoff between per-call cost and accuracy for this specific scenario.",
  "acceptedOutput": {
    "command": "Evaluate in this order: (1) few-shot first, (2) fine-tuning if few-shot does not reach 90%.",
    "fewShotRationale": "Few-shot with 5–7 diverse examples (one per category + 1–2 edge cases) is the fastest to implement and typically achieves >85% accuracy on well-defined classification tasks with clear category boundaries. For ticket classification, include one example per category in the prompt. Estimated prompt overhead: 500–700 tokens per call. At 500 calls/day on GPT-4o-mini: marginal cost per call is approximately $0.0003–0.0004 — negligible.",
    "finetuningRationale": "If few-shot accuracy is below 90% on the held-out set: fine-tune GPT-4o-mini on the 2,000 examples. A fine-tuned model typically reaches 93–97% accuracy on well-labeled classification data. The fine-tuned model can use zero-shot prompts (no examples), reducing per-call token cost by 500–700 tokens. With 500 calls/day, fine-tuning pays back its one-time training cost in approximately 30–90 days depending on pricing.",
    "doNotUse": "Zero-shot alone is not recommended for a 5-class task requiring >90% accuracy without first testing it — zero-shot accuracy on narrow domain categories (billing vs account vs feature-request) is typically 70–80% on a first-pass prompt."
  },
  "rejectedOutput": {
    "command": "Use zero-shot prompting. It is the simplest approach and requires no labeled examples in the prompt."
  },
  "arbiters": ["promptStrategyArbiter"],
  "whyAccepted": "The accepted command starts with the lowest-overhead viable strategy (few-shot) and provides a measurable fallback criterion (90% on held-out set) for escalating to fine-tuning. Both options are quantified.",
  "whyRejected": "Zero-shot is simplest to implement but 'simplest' is not the optimization criterion — the user specified >90% accuracy. Zero-shot on a 5-class ticket classification task with domain-specific categories (billing vs account vs feature-request which overlap in practice) will likely not meet the accuracy bar without testing. Starting with zero-shot wastes evaluation time before trying the more likely-to-succeed approach.",
  "sourceAnchors": [
    {
      "sourceId": "PROMPT-ENGINEERING-GUIDE",
      "url": "https://github.com/dair-ai/Prompt-Engineering-Guide",
      "section": "Few-shot vs zero-shot vs fine-tuning — selection criteria based on accuracy target and dataset size",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```
