# External Training Rows — Batch 7
## Row IDs: EXT-131 through EXT-150
## Date: 2026-05-04
## Focus: Group 10 legacy JS continued, Group 11 modern frontend, Group 12–13 continued, Group 15 agent/LLM, Group 16 language corpora, additional CWE entries

---

## EXT-131

```json
{
  "rowId": "EXT-131",
  "sourceIds": ["BACKBONEJS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "legacy-event-model",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this Backbone.js result: a view directly mutates a model's attribute using model.attributes.status = 'active' instead of model.set('status', 'active').",
    "b": "var OrderView = Backbone.View.extend({ activate: function() { this.model.attributes.status = 'active'; this.render(); } }); — the view bypasses Backbone's set() method and directly writes to the internal attributes hash.",
    "c": "Context: Backbone.js model mutation. model.attributes is an internal hash. model.set() triggers 'change' and 'change:status' events that other views, collections, and listeners depend on. Direct assignment to model.attributes skips event emission. Other views observing the model's change event will not receive the update. Server synchronization (model.save()) reads from model.attributes and will pick up the change, but the event-driven update chain is silently broken."
  },
  "function": "Apply backboneModelMutationArbiter: detect direct assignment to model.attributes bypassing model.set(). Identify which behaviors are silently broken.",
  "acceptedOutput": {
    "decision": "REJECT. Direct mutation of model.attributes bypasses Backbone's change event system. Effects of this bypass: (1) Any view that listens to this.model.on('change:status', handler) will not be called — those views will render stale data. (2) model.hasChanged('status') will return false — change detection is broken. (3) model.previousAttributes() will not include the previous status — undo patterns fail. (4) Collection events that listen for model changes will not fire.",
    "reprocessInstruction": "Use model.set('status', 'active') — this updates the attribute, fires 'change:status' and 'change', updates previousAttributes(), and propagates to all registered listeners. If the re-render should not happen via the change event (for performance), call model.set('status', 'active', { silent: true }); this.render(); — but document why silent is required."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The view calls this.render() after the mutation, so the view will display the updated status correctly."
  },
  "arbiters": ["backboneModelMutationArbiter"],
  "whyAccepted": "Calling this.render() updates the view that performed the mutation. It does not update other views listening to the model's change event. The problem is not this view's rendering — it is the event propagation to all other registered listeners that is silently dropped.",
  "whyRejected": "The rejected output evaluates only whether the mutating view renders correctly. It ignores the event propagation failure to all other components in the application that observe this model.",
  "sourceAnchors": [
    {
      "sourceId": "BACKBONEJS",
      "url": "https://github.com/jashkenas/backbone",
      "section": "Model.set() — triggers change events; direct attributes mutation bypasses event system",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-132

```json
{
  "rowId": "EXT-132",
  "sourceIds": ["REQUIREJS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "A_TOPOLOGY",
  "subdomain": "module-dependency",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this RequireJS module dependency result: Module A declares a dependency on Module B. Module B declares a dependency on Module A. Both modules are loaded asynchronously.",
    "b": "// module-a.js: define(['module-b'], function(B) { return { init: function() { return B.value + 1; } }; }); // module-b.js: define(['module-a'], function(A) { return { value: A.init() }; }); — circular dependency between A and B.",
    "c": "Context: RequireJS circular dependency behavior. RequireJS detects circular references and resolves them by injecting an incomplete (undefined or partially initialized) version of the circularly-referenced module at load time. If module-a.js requires module-b which requires module-a, the A passed to module-b may be undefined or an empty object, causing B.value = undefined.init() to throw TypeError."
  },
  "function": "Apply circularDependencyArbiter: detect RequireJS circular dependencies and identify the runtime failure mode.",
  "acceptedOutput": {
    "decision": "REJECT. Circular dependency between module-a and module-b. RequireJS resolves circular dependencies by providing the incomplete module to break the cycle. When module-b loads and receives module-a as its dependency, module-a may not yet be initialized — its exported object may be empty. B.value = A.init() where A is an empty object causes TypeError: A.init is not a function.",
    "reprocessInstruction": "Three options: (1) Extract the shared value to a third module (module-shared.js) that both A and B depend on — breaks the cycle. (2) Use late binding: inside module-b, require module-a inside the function body (not in the define dependency array) — at call time, A is fully initialized. (3) Restructure so one module depends on the other but not vice versa — typically by inverting the dependency direction so the higher-level module depends on the lower-level module."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. RequireJS handles circular dependencies automatically. Both modules will load and the values will be available at runtime."
  },
  "arbiters": ["circularDependencyArbiter"],
  "whyAccepted": "RequireJS handles circular dependencies by providing an incomplete module — which is not the same as handling them correctly. 'Will load' is true; 'values will be available' depends on initialization timing. If the circular module's value is accessed during the define callback (not lazily), it will be undefined.",
  "whyRejected": "The rejected output relies on RequireJS's circular dependency handling without examining what 'handling' means — it means providing an incomplete stub, not waiting for full initialization. The specific error (A.init is not a function at B's initialization time) is not surfaced.",
  "sourceAnchors": [
    {
      "sourceId": "REQUIREJS",
      "url": "https://github.com/requirejs/requirejs",
      "section": "Circular dependencies — incomplete module injection; late require() as resolution pattern",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-133

```json
{
  "rowId": "EXT-133",
  "sourceIds": ["EXPRESS"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "express-middleware-order",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Decompose this Express API setup requirement: add an error-handling middleware that catches all unhandled errors and returns a JSON error response. The application already has route handlers and a request logger middleware.",
    "b": "Current app.js: app.use(requestLogger); app.get('/api/users', usersHandler); app.post('/api/orders', ordersHandler); — error handling middleware not yet added.",
    "c": "Context: Express error-handling middleware must be registered after all route handlers, and must have exactly 4 parameters: (err, req, res, next). If registered before routes, it will not receive errors thrown inside those routes. If it has 3 parameters, Express treats it as a regular middleware, not an error handler."
  },
  "function": "Produce the action items for adding the error-handling middleware. Specify its position, parameter signature, and the error response format.",
  "acceptedOutput": {
    "actionItems": [
      "Action 1: Define the error handler with exactly 4 parameters — (err, req, res, next). Express identifies error-handling middleware by parameter count: app.use(function errorHandler(err, req, res, next) { const statusCode = err.statusCode || 500; res.status(statusCode).json({ error: err.message || 'Internal Server Error', code: err.code || 'INTERNAL_ERROR' }); });",
      "Action 2: Register the error handler AFTER all route definitions: app.use(requestLogger); app.get('/api/users', usersHandler); app.post('/api/orders', ordersHandler); app.use(errorHandler); — the error handler must be the last middleware in the chain.",
      "Action 3: In route handlers, pass errors to the error handler using next(err): app.get('/api/users', async (req, res, next) => { try { const users = await fetchUsers(); res.json(users); } catch (err) { next(err); } }); Without next(err), synchronous throws in route handlers do reach the error handler, but async errors require explicit next(err) in catch blocks.",
      "Action 4 — async route support: for async route handlers, either wrap in try/catch + next(err) as above, or use express-async-errors package which patches Express to forward unhandled promise rejections to next automatically."
    ]
  },
  "rejectedOutput": {
    "actionItems": [
      "Add app.use(errorHandler) at the top of app.js before the routes.",
      "Make the handler take (req, res) parameters and call res.status(500).send('Error')."
    ]
  },
  "arbiters": ["middlewareOrderArbiter"],
  "whyAccepted": "The accepted decomposition: specifies the 4-parameter signature requirement, registers after all routes, handles async errors via next(err), and formats a structured JSON error response. Each item is implementable.",
  "whyRejected": "The rejected output places the handler before routes (it will never receive route errors), uses a 3-parameter signature (Express will not treat it as an error handler), and returns plain text (not JSON). Three separate errors in one decomposition.",
  "sourceAnchors": [
    {
      "sourceId": "EXPRESS",
      "url": "https://github.com/expressjs/express",
      "section": "Error handling — 4-parameter signature, registration after routes, async next(err)",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-134

```json
{
  "rowId": "EXT-134",
  "sourceIds": ["JEST"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "G_LEARNING_TRAINING",
  "subdomain": "test-mock-lifecycle",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this Jest test structure: a test file mocks the 'nodemailer' module at the top of the file. Two tests use the mock. A third test in a separate test file imports 'nodemailer' and expects to receive the real implementation, but receives the mock instead.",
    "b": "// email.test.js: jest.mock('nodemailer'); describe('EmailService', () => { test('sends welcome email', ...) test('sends reset email', ...) }); // payment.test.js (separate file): test('payment confirmation sends real email', () => { const mailer = require('nodemailer'); // Receives mock — real email not sent }); — jest.mock() in one file pollutes the module registry for tests that expect the real module.",
    "c": "Context: Jest module mock isolation. jest.mock('nodemailer') mocks the module for the entire test file in which it is called. When tests run in the same Jest worker process, module registry state can leak between files if jest.resetModules() or jest.isolateModules() is not used. The payment.test.js file does not call jest.mock('nodemailer') but may receive the mocked version depending on test execution order and worker configuration."
  },
  "function": "Apply mockIsolationArbiter: detect jest.mock() usage that may affect test files other than the declaring file. Produce the isolation pattern.",
  "acceptedOutput": {
    "decision": "REJECT — potential mock isolation violation. jest.mock() hoists to the top of the file and mocks the module for that file's scope. However, if payment.test.js and email.test.js run in the same worker process (--runInBand or shared worker), and the module registry is not reset between files, the mock may persist. The risk is: payment.test.js runs after email.test.js in the same worker and receives the mocked nodemailer from the module cache.",
    "reprocessInstruction": "Fix 1: In each test file that needs the mock, call jest.mock() explicitly — do not rely on it being absent in other files. Fix 2: In files that need the real module, call jest.unmock('nodemailer') before the require. Fix 3: Use beforeEach(() => jest.resetModules()) in files with mixed mock/real requirements. Fix 4: Jest's default behavior runs each test file in a separate worker with an isolated module registry — verify --runInBand is not set in the project's Jest config, which would serialize execution and share the registry."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. jest.mock() is scoped to the file where it is called. Other test files are not affected."
  },
  "arbiters": ["mockIsolationArbiter"],
  "whyAccepted": "The documentation claim that jest.mock() is file-scoped is correct for the default configuration with separate workers. But the actual risk depends on the Jest configuration — --runInBand is commonly used in CI for debugging and changes the isolation guarantee. The arbiter must flag the risk so the team verifies their configuration.",
  "whyRejected": "The rejected output asserts a configuration-dependent guarantee ('jest.mock() is scoped to the file') as an unconditional truth. The isolation depends on Jest's worker configuration. The assertion is correct by default but may be wrong in practice.",
  "sourceAnchors": [
    {
      "sourceId": "JEST",
      "url": "https://github.com/jestjs/jest",
      "section": "jest.mock() scope — file-level isolation with separate workers; --runInBand changes isolation behavior",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-135

```json
{
  "rowId": "EXT-135",
  "sourceIds": ["PYTEST"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "G_LEARNING_TRAINING",
  "subdomain": "fixture-scope",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this pytest fixture design: a fixture creates a database connection, inserts test data, and tears down (deletes all rows) in its finally block. The fixture scope is 'function'. The test suite has 200 tests that use this fixture.",
    "b": "@pytest.fixture(scope='function') def db_with_data(postgres_db): postgres_db.execute('INSERT INTO users VALUES (1, \"alice\")'); yield postgres_db; finally: postgres_db.execute('DELETE FROM users'); — the fixture creates and tears down for every test function.",
    "c": "Context: pytest fixture scope selection. scope='function' creates and destroys the fixture for each test. For a database setup with INSERT + DELETE, this is 400 database operations (200 INSERTs + 200 DELETEs) instead of the 2 that scope='session' would require. For a stateless fixture that cannot be shared, function scope is correct. For a fixture whose teardown is deletion of all rows (restoring to empty state), session or module scope with appropriate cleanup is more efficient."
  },
  "function": "Apply fixtureEfficiencyArbiter: evaluate whether the fixture scope matches the isolation requirements. Identify the correct scope given the teardown strategy.",
  "acceptedOutput": {
    "decision": "REJECT scope='function' as inefficient — scope='session' with transaction rollback is the correct pattern for this use case.",
    "analysis": "The fixture inserts 'alice' and deletes all rows on teardown. If tests do not modify the data (read-only tests), sharing the fixture at session scope would reduce 400 DB operations to 2. If tests do modify data (write tests), the teardown 'DELETE FROM users' restores state for the next test — but this only works for function scope because session scope teardown runs once at the end of all tests.",
    "reprocessInstruction": "For read-only tests: change scope='session'. For tests that mix reads and writes: use a database transaction rollback pattern. @pytest.fixture(scope='function') def db_with_data(postgres_db): postgres_db.begin(); postgres_db.execute('INSERT INTO users VALUES (1, \"alice\")'); yield postgres_db; postgres_db.rollback(); — rollback is faster than DELETE FROM and restores all rows atomically, not just the inserted row."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. function scope ensures each test gets a fresh state. This is the safest fixture scope."
  },
  "arbiters": ["fixtureEfficiencyArbiter"],
  "whyAccepted": "Function scope is the safest scope but not always the correct scope. 'Safest' and 'correct' are not synonyms in fixture design. For 200 tests with a heavy database setup, function scope causes measurable test suite slowdown that could be avoided with transaction rollback or session scope.",
  "whyRejected": "The rejected output evaluates the fixture on safety alone (each test gets fresh state — correct) without evaluating efficiency (400 DB operations vs 2). For a test suite that runs in CI, 200 unnecessary INSERT+DELETE pairs add seconds to the test run.",
  "sourceAnchors": [
    {
      "sourceId": "PYTEST",
      "url": "https://github.com/pytest-dev/pytest",
      "section": "Fixture scope — function vs session; transaction rollback pattern for database fixtures",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-136

```json
{
  "rowId": "EXT-136",
  "sourceIds": ["CYPRESS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "G_LEARNING_TRAINING",
  "subdomain": "flaky-test",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this Cypress test result: a test clicks a 'Load Data' button, waits 3 seconds using cy.wait(3000), then asserts that a data table is visible. The test passes on the developer's machine and fails in CI approximately 30% of the time.",
    "b": "cy.get('[data-testid=load-data-btn]').click(); cy.wait(3000); cy.get('[data-testid=data-table]').should('be.visible'); — fixed 3-second delay is used as the wait for the API response.",
    "c": "Context: Cypress network intercept vs hard wait. cy.wait(3000) waits a fixed time regardless of whether the response has arrived. In CI, API latency may exceed 3 seconds due to cold start, shared database contention, or slower CI hardware. cy.intercept() captures the network request and cy.wait('@alias') waits until the specific request completes — deterministic, not time-based."
  },
  "function": "Apply flakyTestArbiter: detect time-based waits that should be replaced with deterministic event-based waits. Produce the Cypress intercept pattern.",
  "acceptedOutput": {
    "decision": "REJECT. cy.wait(3000) is a fixed-time wait. If the API response arrives in 2.8 seconds in dev and 3.2 seconds in CI, the test passes in dev and fails in CI — which is the observed pattern. The table visibility assertion runs before the data arrives in CI, causing the failure.",
    "reprocessInstruction": "Replace with cy.intercept and request aliasing: cy.intercept('GET', '/api/data').as('loadData'); cy.get('[data-testid=load-data-btn]').click(); cy.wait('@loadData'); cy.get('[data-testid=data-table]').should('be.visible'); cy.wait('@loadData') resolves when the specific intercepted request completes — whether that takes 100ms or 10 seconds. The test is now deterministic and will not fail due to slow CI API responses."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. 3 seconds is a generous wait time. The intermittent CI failures are likely due to environment instability, not the test design."
  },
  "arbiters": ["flakyTestArbiter"],
  "whyAccepted": "Environment instability is a symptom, not the cause. The root cause is a fixed wait that does not adapt to actual response time. cy.intercept makes the test immune to response time variation — whether the environment is stable or not, the assertion runs only after the data arrives.",
  "whyRejected": "The rejected output attributes non-deterministic failure to environment instability and treats the test as correct. Intermittent failures that correlate with environment (CI vs dev) and a fixed wait pattern are the definition of a flaky timing-dependent test.",
  "sourceAnchors": [
    {
      "sourceId": "CYPRESS",
      "url": "https://github.com/cypress-io/cypress",
      "section": "cy.intercept() — request aliasing and cy.wait('@alias') for deterministic network waits",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-137

```json
{
  "rowId": "EXT-137",
  "sourceIds": ["BIGCODEBENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "function-implementation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Implement: fetch_all_pages(base_url: str, auth_token: str) -> list[dict]. Makes GET requests to base_url?page=1, ?page=2, etc. Each response is JSON: { 'data': [...], 'has_next': bool }. Continues fetching until has_next is false. Returns a flat list of all items from all pages.",
    "b": "Test: with 3-page API (page 1: has_next=true, page 2: has_next=true, page 3: has_next=false), fetch_all_pages returns all items from pages 1–3 concatenated. With 1-page API (has_next=false on page 1), returns page 1 items only.",
    "c": "Context: BigCodeBench paginated API aggregation task. The function must handle: (1) first page always fetched, (2) subsequent pages fetched only if has_next is true on the previous page, (3) flat list (not list of lists), (4) auth token in Authorization header. A maximum page limit should be implemented to prevent infinite loops on misbehaving APIs."
  },
  "function": "Implement fetch_all_pages with correct pagination loop, auth header, flat list assembly, and a safety page limit.",
  "acceptedOutput": {
    "implementation": "import requests\nfrom typing import Optional\n\nMAX_PAGES = 1000\n\ndef fetch_all_pages(base_url: str, auth_token: str) -> list[dict]:\n    all_items: list[dict] = []\n    page = 1\n    headers = {'Authorization': f'Bearer {auth_token}'}\n    while page <= MAX_PAGES:\n        response = requests.get(base_url, params={'page': page}, headers=headers)\n        response.raise_for_status()\n        payload = response.json()\n        all_items.extend(payload.get('data', []))\n        if not payload.get('has_next', False):\n            break\n        page += 1\n    return all_items",
    "notes": "raise_for_status() converts 4xx/5xx responses to exceptions. MAX_PAGES=1000 prevents infinite loops on APIs that always return has_next=true due to a bug. payload.get('has_next', False) defaults to stopping if the field is absent — safe default."
  },
  "rejectedOutput": {
    "implementation": "def fetch_all_pages(base_url, auth_token):\n    results = []\n    page = 1\n    while True:\n        r = requests.get(f'{base_url}?page={page}', headers={'Authorization': auth_token})\n        data = r.json()\n        results.append(data['data'])\n        if not data['has_next']: break\n        page += 1\n    return results"
  },
  "arbiters": ["implementationCorrectnessArbiter"],
  "whyAccepted": "The accepted implementation: uses params= for query string (not string concatenation), uses extend() for flat list (not append which creates list of lists), uses raise_for_status() for HTTP error handling, uses get() with defaults for robustness, includes MAX_PAGES safety limit.",
  "whyRejected": "The rejected implementation: uses append(data['data']) which produces a list of lists [[...], [...], [...]]) not a flat list; uses 'Authorization': auth_token without 'Bearer' prefix (most APIs require Bearer scheme); has no safety page limit (infinite loop risk); has no error handling for non-200 responses.",
  "sourceAnchors": [
    {
      "sourceId": "BIGCODEBENCH",
      "url": "https://github.com/bigcode-project/bigcodebench",
      "section": "Paginated API aggregation task — has_next pagination pattern",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-138

```json
{
  "rowId": "EXT-138",
  "sourceIds": ["SWE-BENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "domainResultsArray_to_resultI",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "partial-success-aggregation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Aggregate these four domain results from a file migration job. Three files migrated successfully; one failed due to a permission error.",
    "b": "file1_result = { status: 'SUCCESS', sourceFile: 'report-2024-01.pdf', destinationKey: 's3://archive/2024/01/report.pdf' }; file2_result = { status: 'SUCCESS', sourceFile: 'report-2024-02.pdf', destinationKey: 's3://archive/2024/02/report.pdf' }; file3_result = { status: 'FAIL', sourceFile: 'report-2024-03.pdf', error: 'PermissionDenied: s3:PutObject denied for bucket archive', destinationKey: null }; file4_result = { status: 'SUCCESS', sourceFile: 'report-2024-04.pdf', destinationKey: 's3://archive/2024/04/report.pdf' }",
    "c": "Context: domainResultsArray_to_resultI partial success aggregation. The aggregation must produce a resultI that: (1) clearly distinguishes fully succeeded items from the failed item, (2) does not mark the overall job as SUCCESS when one item failed, (3) enables a retry of only the failed item without re-running the full job."
  },
  "function": "Aggregate the four file migration results into a resultI with PARTIAL_SUCCESS status. Structure the result to enable targeted retry of the failed file.",
  "acceptedOutput": {
    "resultI": {
      "requestId": "req_file_migration_march2024",
      "overallStatus": "PARTIAL_SUCCESS",
      "summary": {
        "total": 4,
        "succeeded": 3,
        "failed": 1
      },
      "succeededItems": [
        { "sourceFile": "report-2024-01.pdf", "destinationKey": "s3://archive/2024/01/report.pdf" },
        { "sourceFile": "report-2024-02.pdf", "destinationKey": "s3://archive/2024/02/report.pdf" },
        { "sourceFile": "report-2024-04.pdf", "destinationKey": "s3://archive/2024/04/report.pdf" }
      ],
      "failedItems": [
        {
          "sourceFile": "report-2024-03.pdf",
          "error": "PermissionDenied: s3:PutObject denied for bucket archive",
          "retryable": true,
          "retryAction": "Grant s3:PutObject permission to the migration role for bucket 'archive', then retry this file only."
        }
      ],
      "arbitrationTargets": ["migrationResultArbiter"],
      "reprocessCandidate": true,
      "reprocessScope": "Retry report-2024-03.pdf only after permission remediation."
    }
  },
  "rejectedOutput": {
    "resultI": {
      "overallStatus": "FAIL",
      "error": "One or more files failed to migrate.",
      "failedCount": 1
    }
  },
  "arbiters": ["migrationResultArbiter"],
  "whyAccepted": "PARTIAL_SUCCESS distinguishes the three successful items (which should not be retried) from the one failed item (which should). The retry instruction includes the specific remediation action and the target file. This enables operational response without re-running completed work.",
  "whyRejected": "FAIL with a count loses the information about which files succeeded and which failed. Downstream handling would need to re-run all 4 files (including the 3 that succeeded) to recover, or would need to re-query the source system to determine which files need retry.",
  "sourceAnchors": [
    {
      "sourceId": "SWE-BENCH",
      "url": "https://github.com/swe-bench/SWE-bench",
      "section": "Partial success aggregation — succeeded and failed item separation for targeted retry",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-139

```json
{
  "rowId": "EXT-139",
  "sourceIds": ["OPENAI-COOKBOOK"],
  "sourceTrustTier": 1,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "C_SEMANTIC_TRANSLATION",
  "subdomain": "structured-output-translation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Translate this LLM classification resultI for a developer audience who will build a downstream handler based on the classification output.",
    "b": "resultI = { overallStatus: 'SUCCESS', classification: { category: 'billing', confidence: 0.94, alternativeCategory: 'account', alternativeConfidence: 0.04, reasoning: 'Ticket references invoice number INV-4421 and requests adjustment of charge amount — strongly billing.' }, inputTokens: 312, outputTokens: 48, modelId: 'gpt-4o-mini', latencyMs: 387 }",
    "c": "Context: Node 6 result translator for developer audience. Developers building downstream handlers need: the primary classification with confidence, whether confidence is above their threshold for automated handling, the alternative if they want to implement fallback logic, and the performance data (tokens + latency) for capacity planning. The reasoning is useful for debugging misclassifications."
  },
  "function": "Translate the LLM classification resultI for a developer building a downstream handler. Include all fields relevant to integration and operational concerns.",
  "acceptedOutput": {
    "finalMachineResult": {
      "audience": "developer",
      "classification": {
        "primary": "billing",
        "confidence": 0.94,
        "autoHandleRecommended": true,
        "autoHandleThreshold": 0.85,
        "alternative": "account",
        "alternativeConfidence": 0.04
      },
      "reasoning": "Ticket references invoice number INV-4421 and requests adjustment of charge amount — strongly billing.",
      "performance": {
        "inputTokens": 312,
        "outputTokens": 48,
        "latencyMs": 387,
        "modelId": "gpt-4o-mini"
      },
      "integrationNote": "confidence=0.94 exceeds the recommended auto-handle threshold of 0.85. Route to billing queue without human review. For tickets with confidence < 0.85, route to human review queue."
    }
  },
  "rejectedOutput": {
    "finalMachineResult": "The ticket was classified as billing with 94% confidence."
  },
  "arbiters": ["resultTranslationArbiter"],
  "whyAccepted": "A developer building a downstream handler needs: the confidence value as a number (not percentage prose) for threshold comparison, the alternative category for fallback routing, the performance data for capacity planning, and an explicit auto-handle recommendation based on a defined threshold. The accepted translation provides all of these.",
  "whyRejected": "The rejected output is a prose summary suitable for a non-technical stakeholder. A developer cannot write routing logic against 'classified as billing with 94% confidence' — they need structured data with the numeric confidence field.",
  "sourceAnchors": [
    {
      "sourceId": "OPENAI-COOKBOOK",
      "url": "https://github.com/openai/openai-cookbook",
      "section": "Structured output translation — classification result format for developer integration",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-140

```json
{
  "rowId": "EXT-140",
  "sourceIds": ["PLANTUML-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "finalMachineResults_to_userFormat",
  "domain": "H_GOVERNANCE",
  "subdomain": "diagram-formatting",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Format this approved sequence diagram result as a PlantUML diagram showing the account creation flow: client sends POST /users, the API validates the request, creates the user record in the database, sends a welcome email via the email service, and returns 201 Created to the client.",
    "b": "Approved result: { sequence: [ { from: 'Client', to: 'API', message: 'POST /users', type: 'request' }, { from: 'API', to: 'Database', message: 'INSERT user', type: 'request' }, { from: 'Database', to: 'API', message: '201 Created user record', type: 'response' }, { from: 'API', to: 'EmailService', message: 'sendWelcomeEmail(userId)', type: 'async' }, { from: 'API', to: 'Client', message: '201 Created', type: 'response' } ] }",
    "c": "Context: PlantUML sequence diagram format. Synchronous requests use '->'. Synchronous responses use '-->'. Asynchronous messages use '->>'. Activation boxes show when a participant is processing. The email service call is async (fire-and-forget) — the API does not wait for the email to be sent before returning 201 to the client."
  },
  "function": "Format the approved sequence as a PlantUML diagram. Use correct arrow types for synchronous requests, synchronous responses, and async messages.",
  "acceptedOutput": {
    "diagramSource": "@startuml\nactor Client\nparticipant API\ndatabase Database\nparticipant EmailService\n\nClient -> API: POST /users\nactivate API\nAPI -> Database: INSERT user\nactivate Database\nDatabase --> API: user record created\ndeactivate Database\nAPI ->> EmailService: sendWelcomeEmail(userId)\nnote right: async — API does not wait\nAPI --> Client: 201 Created\ndeactivate API\n@enduml"
  },
  "rejectedOutput": {
    "diagramSource": "@startuml\nClient -> API: POST /users\nAPI -> Database: INSERT user\nDatabase -> API: user record\nAPI -> EmailService: sendWelcomeEmail\nEmailService -> API: ok\nAPI -> Client: 201 Created\n@enduml"
  },
  "arbiters": ["diagramFormattingArbiter"],
  "whyAccepted": "The accepted diagram: uses '-->' for synchronous responses (dashed return arrow), uses '->>' for async fire-and-forget (the API does not wait), includes activation boxes to show processing scope, and annotates the async call. Reflects the actual flow.",
  "whyRejected": "The rejected diagram: uses '->' for all arrows (no distinction between request and response), shows EmailService responding to API (EmailService -> API: ok) which is incorrect — the API does not wait for the email confirmation. The diagram misrepresents the async behavior as synchronous.",
  "sourceAnchors": [
    {
      "sourceId": "PLANTUML-DOCS",
      "url": "https://plantuml.com/sequence-diagram",
      "section": "Sequence diagram — arrow types: -> request, --> response, ->> async; activation boxes",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-141

```json
{
  "rowId": "EXT-141",
  "sourceIds": ["MUI-MATERIAL-UI"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "react-component-composition",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this React component structure: a UserDashboard component renders a ProfileCard which renders a NotificationBell which renders a NotificationCount. The current user's notification count is passed from UserDashboard through ProfileCard and NotificationBell as props, even though ProfileCard and NotificationBell do not use the prop themselves.",
    "b": "UserDashboard: <ProfileCard userId={userId} notificationCount={notificationCount} /> ProfileCard: <NotificationBell notificationCount={props.notificationCount} /> NotificationBell: <NotificationCount count={props.notificationCount} /> NotificationCount: <span>{props.count}</span> — notificationCount passes through 3 intermediate components.",
    "c": "Context: React prop drilling anti-pattern. notificationCount passes through ProfileCard and NotificationBell which do not use it — they only forward it. This creates coupling: if the component tree changes (a new wrapper component is inserted), the prop drilling chain must be updated. For cross-cutting concerns like current user data, React Context or a state management library provides a direct channel from provider to consumer."
  },
  "function": "Apply propDrillingArbiter: detect prop passing through intermediate components that do not use the prop. Identify the correct pattern for the scenario.",
  "acceptedOutput": {
    "decision": "REJECT. Prop drilling through 3 intermediate components (UserDashboard → ProfileCard → NotificationBell → NotificationCount) where only the leaf component uses the prop. ProfileCard and NotificationBell are passive conduits — they must be updated if the prop name changes, if the component hierarchy changes, or if a new intermediate component is inserted.",
    "reprocessInstruction": "Use React Context for the notification count: const NotificationContext = React.createContext(0); In UserDashboard: <NotificationContext.Provider value={notificationCount}><ProfileCard userId={userId} /></NotificationContext.Provider>. In NotificationCount: const count = useContext(NotificationContext); <span>{count}</span>. ProfileCard and NotificationBell no longer need the prop. Alternative: if the notification count is global user state, a state management library (Redux, Zustand) provides a selector that NotificationCount subscribes to directly."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. Passing props through intermediate components is a normal React pattern. Context adds complexity for a simple value."
  },
  "arbiters": ["propDrillingArbiter"],
  "whyAccepted": "Prop drilling is a normal pattern when the intermediate components use the prop. When they do not use it (pure conduits), the coupling is accidental — the intermediate components have no reason to know about notificationCount. Context removes that accidental coupling for cross-cutting values like user state.",
  "whyRejected": "The rejected output is correct that prop passing is normal in React. The arbiter distinguishes between props that are used by intermediary components (correct) and props that only pass through intermediaries to reach a leaf (prop drilling anti-pattern). The word 'normal' covers both cases — the rejected output does not distinguish them.",
  "sourceAnchors": [
    {
      "sourceId": "MUI-MATERIAL-UI",
      "url": "https://github.com/mui/material-ui",
      "section": "Component composition patterns — prop drilling vs Context for cross-cutting state",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-142

```json
{
  "rowId": "EXT-142",
  "sourceIds": ["GOVUK-DESIGN-SYSTEM", "PATTERNFLY"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "accessibility-focus-management",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this modal dialog implementation: a modal opens when the user clicks 'Open Settings'. The modal contains a form with a Save button. When the modal is closed (by clicking Cancel or pressing Escape), focus returns to the document body rather than to the 'Open Settings' button that triggered the modal.",
    "b": "function closeModal() { modal.style.display = 'none'; document.body.focus(); } — focus is returned to document body on close.",
    "c": "Context: WCAG 2.1 Success Criterion 2.4.3 (Focus Order) and 2.4.7 (Focus Visible). When a modal closes, focus must return to the element that triggered the modal — not to the top of the document or to the body. Screen reader users rely on focus position to understand where they are in the page. After modal close, document.body receives focus — the next Tab keystroke moves focus to the first focusable element in document order, which may be far from where the user was."
  },
  "function": "Apply focusManagementArbiter: detect modal implementations that fail to return focus to the trigger element on close. Produce the correct pattern.",
  "acceptedOutput": {
    "decision": "REJECT. WCAG 2.1 SC 2.4.3 violation. When the settings modal closes, focus moves to document.body instead of returning to the 'Open Settings' button. For a keyboard or screen reader user, closing a modal and losing their position in the page requires them to navigate the entire page to find their previous context. This is a major accessibility barrier.",
    "reprocessInstruction": "Store a reference to the trigger element before opening the modal: let triggerElement = null; function openModal(trigger) { triggerElement = trigger; modal.style.display = 'block'; modal.querySelector('[data-focus-first]').focus(); } function closeModal() { modal.style.display = 'none'; if (triggerElement) { triggerElement.focus(); triggerElement = null; } } In the modal's Escape key handler, also call closeModal(). Additionally, add focus trap inside the modal — Tab and Shift+Tab should cycle within the modal while it is open, not allow focus to escape to background content."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The modal closes correctly and the page returns to its normal state. Keyboard users can Tab to any element they need."
  },
  "arbiters": ["focusManagementArbiter"],
  "whyAccepted": "WCAG 2.1 SC 2.4.3 explicitly requires that focus order be logical and predictable. Returning focus to the body and requiring users to re-navigate the page is not a predictable focus order — it forces unnecessary navigation. The requirement is to return focus to the element that caused the context change.",
  "whyRejected": "The rejected output evaluates whether keyboard users can reach any element (true — Tab eventually reaches all focusable elements) but does not evaluate whether the focus order is predictable and efficient (false — the user must re-navigate from the start of the document).",
  "sourceAnchors": [
    {
      "sourceId": "GOVUK-DESIGN-SYSTEM",
      "url": "https://github.com/alphagov/govuk-frontend",
      "section": "Modal dialog — focus management on open/close; WCAG 2.4.3 focus order",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-143

```json
{
  "rowId": "EXT-143",
  "sourceIds": ["ASYNCAPI-SPEC"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "A_TOPOLOGY",
  "subdomain": "event-schema-versioning",
  "rowClass": "migration-decision",
  "input": {
    "a": "Reprocessing decision: an AsyncAPI event 'UserRegistered' currently has fields: userId (string), email (string), registeredAt (string). The team wants to add a new required field 'referralSource' (enum: organic, referral, paid_ad) to track acquisition channel. This is a breaking schema change for existing consumers.",
    "b": "History: 12 services consume the UserRegistered event. 3 of them are third-party integrations with 30-day deployment cycles. Current event version: 1.0. Kafka topic: user.registered.v1. AsyncAPI spec file defines the channel as user.registered.v1 with schema version 1.0.",
    "c": "Context: AsyncAPI event versioning. Adding a required field to an existing event schema is a breaking change for consumers that validate the schema — they will reject events missing the field. For consumers that do not validate, the field will be absent on existing events if any replay occurs. The migration must not break the 3 third-party integrations that cannot deploy quickly."
  },
  "function": "Produce the reprocessing decision for adding a required field to the UserRegistered event without breaking existing consumers.",
  "acceptedOutput": {
    "reprocessingDecision": "DUAL-CHANNEL migration — do not modify the v1 schema in place.",
    "steps": [
      "Step 1: Publish the new schema as UserRegistered v2 on topic user.registered.v2. The v2 schema adds referralSource as required. Update the AsyncAPI spec to define user.registered.v2.",
      "Step 2: Update the producer to publish to both user.registered.v1 (without referralSource) and user.registered.v2 (with referralSource) simultaneously. This is the dual-write period.",
      "Step 3: Update each consumer to subscribe to user.registered.v2. The 3 third-party integrations have a 30-day cycle — plan 35–40 days for their migration.",
      "Step 4: After all 12 consumers are confirmed on v2, stop producing to user.registered.v1. Deprecate the v1 topic in the AsyncAPI spec with a sunset date.",
      "Step 5: Remove v1 after the sunset date."
    ],
    "doNotDo": "Do not add referralSource as required to the v1 schema — this breaks all 12 consumers immediately. Do not make it optional in v1 — the business requirement is that it is required for new registrations; optional in v2 would be an option if the backfill is infeasible."
  },
  "rejectedOutput": {
    "reprocessingDecision": "Add referralSource as optional to the existing v1 schema. Existing consumers will ignore the new optional field."
  },
  "arbiters": ["eventSchemaVersioningArbiter"],
  "whyAccepted": "Dual-channel migration allows existing consumers to continue on v1 while new consumers adopt v2. The 30-day third-party cycle is accommodated without forcing a compressed deployment. This is the standard protocol migration approach for event-driven systems.",
  "whyRejected": "Making the field optional in v1 is technically safe for consumers but defeats the business requirement: if referralSource is optional, the producer cannot guarantee it is always present for new registrations. The team wants it required — which requires a new version, not a patch to v1.",
  "sourceAnchors": [
    {
      "sourceId": "ASYNCAPI-SPEC",
      "url": "https://github.com/asyncapi/spec",
      "section": "Event schema versioning — dual-channel migration for breaking field additions",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-144

```json
{
  "rowId": "EXT-144",
  "sourceIds": ["CWE-TOP25"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "hardcoded-credentials",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this result: a CI configuration file contains the database password directly in the pipeline environment variable definition. The repository is private. The team argues the private repository status is sufficient protection.",
    "b": "# .github/workflows/deploy.yml: env: DB_PASSWORD: 'SuperSecret123!' — the password is hardcoded in the YAML file checked into the repository.",
    "c": "Context: CWE-798 Use of Hard-coded Credentials (rank 18 in CWE Top 25 2023). The credentials appear in: (1) the repository's git history indefinitely, even if the file is later edited; (2) any repository fork; (3) any clone on a developer's workstation; (4) CI logs if the env var is echoed by any step; (5) any future repository access event (breach, accidental public visibility toggle). 'Private repository' is a perimeter defense, not a credential protection control."
  },
  "function": "Apply hardcodedCredentialArbiter: detect credentials stored in repository-tracked files regardless of repository visibility. Enumerate the exposure vectors and produce the correct pattern.",
  "acceptedOutput": {
    "decision": "REJECT. CWE-798 — hardcoded credential in repository-tracked file. DB_PASSWORD: 'SuperSecret123!' in .github/workflows/deploy.yml is committed to git history. Git history is permanent — changing the value in a later commit does not remove the password from history. Exposure vectors: git log shows the value; anyone with repository access (current or future) can recover it; repository forking creates new copies; accidental public visibility change exposes it to the internet.",
    "reprocessInstruction": "Store the password in GitHub Actions Secrets: Settings → Secrets → New repository secret → DB_PASSWORD = SuperSecret123!. Reference in the workflow: env: DB_PASSWORD: ${{ secrets.DB_PASSWORD }}. The secret value is never written to git history, is masked in logs, and is only accessible to workflows in the repository. Rotate the current password immediately — the committed value must be considered compromised regardless of repository visibility."
  },
  "rejectedOutput": {
    "decision": "ACCEPT with caveat. The repository is private, so exposure is limited to authorized team members. The team should document the risk and plan to move to secrets management in the next sprint."
  },
  "arbiters": ["hardcodedCredentialArbiter"],
  "whyAccepted": "Private repository status controls who can see the repository today — it does not protect against: future visibility changes, employee turnover, repository breaches, log exposure, or forked copies. Treating private-repo as a credential protection control is a fundamental security modeling error.",
  "whyRejected": "The rejected output accepts the risk with a deferred remediation plan. CWE-798 is a security defect that must be fixed immediately, not deferred. The current password must be rotated because it is in git history — even after moving to secrets management, the committed value remains recoverable.",
  "sourceAnchors": [
    {
      "sourceId": "CWE-TOP25",
      "url": "https://cwe.mitre.org/data/definitions/798.html",
      "section": "CWE-798 Hard-coded credentials — git history permanence; private repo is not credential protection",
      "license": "MITRE CWE Terms of Use",
      "requiredNotice": "CWE™ is a trademark of The MITRE Corporation"
    }
  ],
  "licenseConstraint": "MITRE notice required",
  "trainingStatus": "candidate"
}
```

---

## EXT-145

```json
{
  "rowId": "EXT-145",
  "sourceIds": ["CWE-TOP25"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "tls-validation",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this result: a Python service makes HTTPS requests to an internal partner API. A developer disabled SSL certificate verification with verify=False in the requests library call because 'the partner's certificate expires frequently and the service kept crashing.'",
    "b": "response = requests.get('https://partner-api.internal/data', verify=False) — SSL verification is disabled. requests issues an InsecureRequestWarning but the call succeeds.",
    "c": "Context: CWE-295 Improper Certificate Validation. Disabling verify=False means the TLS handshake accepts any certificate — including self-signed, expired, or certificates from any CA. This enables MITM (Man-in-the-Middle) attacks: an attacker on the network path can present any certificate and intercept the HTTPS traffic. The 'frequent expiration' problem is a certificate lifecycle management issue, not a reason to disable validation."
  },
  "function": "Apply certificateValidationArbiter: detect TLS verification disabled as a workaround for certificate management problems. Identify the correct fix for the underlying problem.",
  "acceptedOutput": {
    "decision": "REJECT. CWE-295 — SSL certificate validation disabled. verify=False accepts any certificate presented by any server. An attacker positioned on the network path between this service and the partner API can terminate the TLS connection, read the plaintext data, and forward a new TLS connection to the partner. This is a complete TLS bypass for network-positioned attackers.",
    "reprocessInstruction": "Fix the underlying certificate lifecycle problem, not the validation check. Two options: (1) If the partner uses a self-signed certificate: pin the specific certificate. requests.get(url, verify='/path/to/partner-ca.crt') — this accepts only the specified CA's certificates; (2) If the partner's public CA certificate expires frequently: implement certificate expiry monitoring (check expiry weekly and alert 30 days before). Set up automatic renewal on the partner side. Temporary workaround while fixing: verify=False is never acceptable in production. Use a staging environment with verify=False only during testing."
  },
  "rejectedOutput": {
    "decision": "ACCEPT for internal services. MITM attacks on internal networks are unlikely. The verify=False is a pragmatic workaround for a partner management issue."
  },
  "arbiters": ["certificateValidationArbiter"],
  "whyAccepted": "Internal network position reduces MITM likelihood but does not eliminate it. Internal network attacks (rogue devices, SSRF chaining, compromised internal hosts) are a documented attack class. verify=False eliminates TLS authentication entirely — the encryption channel exists but its endpoint is unverified.",
  "whyRejected": "Threat likelihood reduction ('unlikely on internal networks') is not equivalent to threat elimination. CWE-295 is flagged regardless of network position because the control (certificate validation) exists specifically to authenticate the endpoint — disabling it defeats its entire purpose regardless of the attack probability assessment.",
  "sourceAnchors": [
    {
      "sourceId": "CWE-TOP25",
      "url": "https://cwe.mitre.org/data/definitions/295.html",
      "section": "CWE-295 Improper Certificate Validation — verify=False disables endpoint authentication; MITM vector",
      "license": "MITRE CWE Terms of Use",
      "requiredNotice": "CWE™ is a trademark of The MITRE Corporation"
    }
  ],
  "licenseConstraint": "MITRE notice required",
  "trainingStatus": "candidate"
}
```

---

## EXT-146

```json
{
  "rowId": "EXT-146",
  "sourceIds": ["COMMON-VOICE"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "C_SEMANTIC_TRANSLATION",
  "subdomain": "speech-transcription-quality",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Arbitrate this speech-to-text transcription result: an audio sample contains the sentence 'The patient's prescription needs to be renewed before the end of the month.' The transcription system produced: 'The patients prescription needs to be renewed before the end of the month.'",
    "b": "Ground truth: 'The patient's prescription needs to be renewed before the end of the month.' Transcription: 'The patients prescription needs to be renewed before the end of the month.' — the possessive apostrophe in 'patient's' is omitted, producing 'patients' (plural) instead of 'patient's' (singular possessive).",
    "c": "Context: Mozilla Common Voice dataset (CC-0). For medical domain transcription, the distinction between 'patient's prescription' (one specific patient's prescription) and 'patients prescription' (grammatically incorrect — would need to be 'patients' prescription' for plural possessive) is semantically significant. In a prescription renewal workflow, routing 'patients prescription' to a handler expecting possessive context could fail parsing or produce ambiguous routing."
  },
  "function": "Apply transcriptionQualityArbiter: evaluate whether the omitted possessive apostrophe is an acceptable error for downstream medical workflow use. Classify the error severity.",
  "acceptedOutput": {
    "decision": "REJECT for medical workflow use at this quality level. Error classification: semantic — not purely orthographic. 'Patient's' (singular possessive) is grammatically and semantically distinct from 'patients' (plural noun). In a prescription renewal context: 'the patient's prescription' refers to one patient's prescription; 'patients prescription' is grammatically non-standard and could be misinterpreted as referring to prescriptions for multiple patients. Medical document parsing systems that expect noun phrases may route the two differently.",
    "qualityAction": "The transcription system requires training data augmentation with medical domain audio containing possessive constructions. Evaluate WER (Word Error Rate) on possessive constructions specifically in the medical subdomain. For downstream use: either add a post-processing step that detects possessive context from surrounding words and restores the apostrophe, or require human review for transcriptions in the medical domain."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The semantic meaning is preserved — both forms refer to a prescription that needs renewal. The apostrophe is punctuation and does not affect the spoken form of the word."
  },
  "arbiters": ["transcriptionQualityArbiter"],
  "whyAccepted": "Spoken form equivalence (both are pronounced 'patients') is not the arbiter criterion for text-processing downstream systems. Text processing is character-level: 'patient's' and 'patients' are different tokens. For downstream systems that perform entity extraction or possessive relationship parsing, the distinction is significant.",
  "whyRejected": "The rejected output evaluates the semantic intention of the speaker (correct — the speaker intended the singular possessive) but applies that intention to justify accepting the incorrect written form. The written form received by downstream systems is 'patients', not 'patient's' — the system operates on the transcribed text, not the speaker's intent.",
  "sourceAnchors": [
    {
      "sourceId": "COMMON-VOICE",
      "url": "https://commonvoice.mozilla.org",
      "section": "Transcription quality evaluation — possessive construction accuracy in medical domain",
      "license": "CC-0"
    }
  ],
  "licenseConstraint": "CC-0",
  "trainingStatus": "candidate"
}
```

---

## EXT-147

```json
{
  "rowId": "EXT-147",
  "sourceIds": ["WIKTIONARY"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G_LEARNING_TRAINING",
  "subdomain": "lexical-resource-management",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Reprocessing decision: a multilingual lexical processing system is building entries for the word 'color' and its variant 'colour'. Both spellings exist. The system must decide whether to create two separate lexical entries or one entry with a variant reference.",
    "b": "History: Wiktionary (CC-BY-SA 3.0) treats 'color' and 'colour' as separate entries on the English Wiktionary. 'color' is the primary US English spelling. 'colour' is the primary UK/Commonwealth spelling. Both have the same etymology, pronunciation (differing slightly in some dialects), and definitions. Wiktionary's 'colour' entry includes a note 'Alternative spelling: color (US)' and the full definition is repeated in the 'colour' entry.",
    "c": "Context: Wiktionary lexical data model (CC-BY-SA 3.0). For a machine learning training corpus, the decision is: (a) treat as two independent entries (increases corpus coverage for both spellings); (b) treat as one canonical entry with a redirect/alias for the variant (reduces duplication, but may miss variant-specific usage contexts). The system trains on English text from both US and UK sources."
  },
  "function": "Produce the reprocessing decision: two entries or one with variant alias, with rationale for a training corpus use case.",
  "acceptedOutput": {
    "reprocessingDecision": "Create one canonical entry (color) with variant alias (colour) — with training-corpus-specific handling.",
    "rationale": "For a training corpus: canonical + alias is preferred because it avoids duplicate positive training examples (the same definition appearing under two headwords could overweight the concept in embeddings). However, usage examples must be collected separately for each spelling — UK text ('the colour of the sky') and US text ('the color of the sky') are both valid training pairs and should be tagged with their dialect source.",
    "implementation": "Entry structure: headword=color, aliases=[colour], dialectTags={color: en-US, colour: en-GB}. Usage examples indexed by spelling variant. When a text source uses 'colour', tag that example as en-GB and link to the canonical entry. This preserves variant-specific usage without duplicating the definition.",
    "exception": "If the training objective requires dialect identification (distinguishing US from UK English), treat as two separate entries with dialect labels — canonical+alias merging would conflate the two dialects that the model needs to distinguish."
  },
  "rejectedOutput": {
    "reprocessingDecision": "Create two separate entries — Wiktionary does it this way and Wiktionary is the authoritative source."
  },
  "arbiters": ["lexicalEntryStructureArbiter"],
  "whyAccepted": "Wiktionary's data model is optimized for human readability and comprehensive coverage per headword. A machine training corpus has different optimization criteria: embedding geometry (duplicate definitions can cause concept overrepresentation), dialect signal preservation, and deduplication efficiency. The decision must be based on the training objective, not on Wiktionary's editorial conventions.",
  "whyRejected": "The rejected output defers to Wiktionary's format without evaluating whether that format is optimal for the training corpus use case. 'Authoritative source' for lexical data is not the same as 'optimal format for training corpus'.",
  "sourceAnchors": [
    {
      "sourceId": "WIKTIONARY",
      "url": "https://en.wiktionary.org",
      "section": "Spelling variants — color vs colour; separate headword entries with alternative spelling notes",
      "license": "CC-BY-SA-3.0"
    }
  ],
  "licenseConstraint": "CC-BY-SA-3.0 — attribution required",
  "trainingStatus": "candidate"
}
```

---

## EXT-148

```json
{
  "rowId": "EXT-148",
  "sourceIds": ["DOTNET-MICROSERVICES"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "F_REGISTRY_STORAGE",
  "subdomain": "event-sourcing-replay",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Reprocessing decision: an event-sourced Order aggregate has 5 years of events in the event store (approximately 2 million events for a high-volume merchant). A new read model projection needs to be built for a reporting feature. The team must decide whether to replay all 2 million events to build the projection or to use a snapshot plus incremental replay.",
    "b": "History: the event store is EventStoreDB. The Order aggregate has snapshots every 500 events (so the most recent snapshot is no more than 499 events behind the current state). The projection needs the last 12 months of data only — approximately 400,000 events. Full replay of 2 million events would take approximately 4 hours. Snapshot + 12-month replay would take approximately 48 minutes.",
    "c": "Context: DOTNET-MICROSERVICES event sourcing — projection rebuild strategy. If the projection only needs a time-bounded window (12 months), replaying events outside that window wastes time and resources. A point-in-time snapshot from 12 months ago is the starting point for incremental replay. If no such snapshot exists, a partial replay from the event timestamp filter is still faster than full replay."
  },
  "function": "Produce the reprocessing decision for building the 12-month projection. Specify the replay strategy and its prerequisites.",
  "acceptedOutput": {
    "reprocessingDecision": "TIME-BOUNDED REPLAY — do not replay all 2 million events.",
    "strategy": "Filter the event stream to events with timestamp >= 2025-05-04 (12 months ago from today 2026-05-04). EventStoreDB supports timestamp-based stream filtering. The projection builder subscribes to filtered events starting from that point, building the read model in approximately 48 minutes instead of 4 hours.",
    "prerequisite": "Verify that the event store has indexed event timestamps for efficient range queries. If the event store does not support timestamp filtering on the stream, use the 500-event snapshot as the starting point: replay from the snapshot closest to 2025-05-04, then replay forward — the snapshot eliminates most of the pre-12-month events.",
    "doNotDo": "Do not perform a full replay of 2 million events for a 12-month projection. Full replay is only required when: (1) the projection needs the complete history, or (2) no time filter or snapshot is available."
  },
  "rejectedOutput": {
    "reprocessingDecision": "Replay all events from the beginning. Event sourcing's guarantee is that any projection can be rebuilt from the event log — always replay from the start to ensure correctness."
  },
  "arbiters": ["projectionRebuildArbiter"],
  "whyAccepted": "Full replay is correct for projections that need complete history. For a 12-month projection, events older than 12 months produce projection state that is immediately overwritten by newer events — they contribute nothing to the final projection state but consume 4 hours of replay time. Time-bounded replay is both correct and efficient.",
  "whyRejected": "The rejected output correctly states that event sourcing guarantees projection rebuildability from the full log. But 'can be rebuilt from the start' does not mean 'must be rebuilt from the start'. The correctness guarantee applies to the result — time-bounded replay produces the same result as full replay for a 12-month projection, in a fraction of the time.",
  "sourceAnchors": [
    {
      "sourceId": "DOTNET-MICROSERVICES",
      "url": "https://github.com/dotnet-architecture/eShopOnContainers",
      "section": "Event sourcing — projection rebuild strategies; snapshot + incremental vs full replay",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-149

```json
{
  "rowId": "EXT-149",
  "sourceIds": ["ESHOP-ON-WEB", "DOTNET-MICROSERVICES"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "F_REGISTRY_STORAGE",
  "subdomain": "domain-event-publication",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this domain event publication result: when an order is placed, the OrdersController calls orderRepository.Save(order) and immediately calls messageBroker.Publish(new OrderPlacedEvent(order.Id)). The database save and the broker publish are two separate operations with no transaction.",
    "b": "public async Task<IActionResult> PlaceOrder([FromBody] OrderDto dto) { var order = Order.Create(dto); await _orderRepository.SaveAsync(order); await _messageBroker.PublishAsync(new OrderPlacedEvent(order.Id)); return Ok(); } — database write and broker publish are sequential, not atomic.",
    "c": "Context: Transactional Outbox pattern (eShopOnContainers). Two independent I/O operations (database + broker) cannot be made atomic with a single transaction in most architectures. If the broker publish fails after the database save succeeds, the order is saved but no downstream service (inventory, email, analytics) receives the OrderPlacedEvent. The system enters an inconsistent state with no retry mechanism."
  },
  "function": "Apply transactionalOutboxArbiter: detect dual-write patterns (database + broker without transactional outbox). Identify the failure mode and produce the outbox pattern.",
  "acceptedOutput": {
    "decision": "REJECT. Dual-write without transactional outbox. Two failure modes: (1) broker publish fails after database save: order is persisted but no downstream event is published — inventory not decremented, confirmation email not sent, analytics not updated. No automatic retry. (2) If the controller crashes between the two awaits (unlikely but possible in a heavily loaded system): same result. The gap between the two operations is an inconsistency window.",
    "reprocessInstruction": "Implement the Transactional Outbox pattern: within the same database transaction that saves the order, insert an OutboxMessage record (EventType=OrderPlaced, Payload=serialized event, Status=PENDING). A separate background process (OutboxProcessor) reads PENDING outbox messages, publishes them to the broker, and marks them PROCESSED — in a retry loop. If the broker is unavailable, the outbox message remains PENDING and is retried. The database transaction ensures the order and the outbox message are written atomically — either both persist or neither does."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The broker publish follows immediately after the database save. Broker failures are rare. If publish fails, the operation can be retried by the caller."
  },
  "arbiters": ["transactionalOutboxArbiter"],
  "whyAccepted": "Rare broker failures still require a retry mechanism — 'rare' does not mean 'never'. 'Retry by the caller' requires the caller to detect the partial failure (the order was saved but the event was not published), which is not possible from the controller's perspective — the controller does not know whether the downstream state is consistent after its response.",
  "whyRejected": "The rejected output accepts the pattern based on failure frequency ('rare') and delegates retry to the caller. Both assumptions are incorrect: rare failures still require handling, and the caller (HTTP client) has already received Ok() and cannot know the publish failed.",
  "sourceAnchors": [
    {
      "sourceId": "ESHOP-ON-WEB",
      "url": "https://github.com/dotnet-architecture/eShopOnWeb",
      "section": "Transactional outbox — atomic order save + outbox message insert vs direct broker publish",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-150

```json
{
  "rowId": "EXT-150",
  "sourceIds": ["ASPNETCORE-DOCS-SAMPLES"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A_TOPOLOGY",
  "subdomain": "api-style-selection",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "I'm building a new ASP.NET Core 8 microservice. The service exposes 5 endpoints: GET /health, GET /products, GET /products/{id}, POST /orders, and GET /orders/{id}. Should I use Minimal APIs or Controller-based APIs?",
    "b": "Team context: the team has 3 developers. Two have prior ASP.NET MVC/Web API controller experience. The service will grow — they estimate 15–20 endpoints within 6 months. The service needs: input validation, OpenAPI documentation (Swashbuckle), and authorization policies on the POST endpoint.",
    "c": "Context: ASP.NET Core 8 Minimal APIs vs Controller-based APIs. Minimal APIs: less boilerplate, lambda-based route handlers, excellent for small services. Controller-based: familiar MVC conventions, built-in support for filters (action filters, authorization filters), better IDE tooling for large route sets, ActionResult return type conventions. Both support OpenAPI via Swashbuckle. The decision should account for team experience, projected growth, and feature requirements."
  },
  "function": "Produce the command: which API style to use for this service and the conditions that would change the recommendation.",
  "acceptedOutput": {
    "command": "Use Controller-based APIs for this service.",
    "rationale": "Three factors favor controllers: (1) Team experience — two of three developers have MVC/Web API experience. Controller-based APIs follow the same conventions they already know. Minimal API's lambda-style handlers and endpoint group patterns require a learning investment. (2) Growth projection — 15–20 endpoints in 6 months. Minimal API lambda handlers become harder to organize as endpoint count grows; controller classes provide natural grouping by resource (ProductsController, OrdersController). (3) Feature requirements — authorization policies are first-class in controllers via [Authorize] attribute on action methods; Minimal API supports authorization but requires .RequireAuthorization() calls on each endpoint registration, which is more verbose at scale.",
    "conditionsToChangeRecommendation": "Choose Minimal APIs instead if: the service is expected to stay at 5–8 endpoints permanently, or the team is new to ASP.NET Core and prefers to learn the simpler API surface, or the service is a standalone function with a single responsibility (e.g., a webhook handler)."
  },
  "rejectedOutput": {
    "command": "Use Minimal APIs — they are the modern ASP.NET Core approach and have less boilerplate."
  },
  "arbiters": ["apiStyleSelectionArbiter"],
  "whyAccepted": "The accepted command evaluates the three decision criteria (team experience, growth trajectory, feature requirements) and reaches a context-specific recommendation. It also states the conditions that would invert the recommendation, enabling the team to apply the reasoning to future decisions.",
  "whyRejected": "The rejected output applies a general 'modern = better' heuristic without evaluating the specific context. 'Less boilerplate' is an advantage for simple services but not for services expected to grow to 20 endpoints with complex authorization requirements handled by a team with existing controller experience.",
  "sourceAnchors": [
    {
      "sourceId": "ASPNETCORE-DOCS-SAMPLES",
      "url": "https://github.com/dotnet/aspnetcore",
      "section": "Minimal APIs vs controller-based APIs — selection criteria for team experience and service scale",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```
