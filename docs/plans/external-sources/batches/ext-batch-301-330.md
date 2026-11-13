# External Training Rows — Batch 11
## Row IDs: EXT-211 through EXT-230
## Date: 2026-05-04
## Focus: Dapper, EF Core, BDD, Mermaid, CQRS, RFC 9457, CWE-119, language corpora, additional families 3/5/6/7/8

---

## EXT-211

```json
{
  "rowId": "EXT-211",
  "sourceIds": ["DAPPER"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "dapper-query-efficiency",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Evaluate this Dapper data access code that loads orders with their line items.",
    "b": "Implementation A: var orders = conn.Query<Order>('SELECT * FROM Orders').ToList(); foreach (var order in orders) { order.LineItems = conn.Query<LineItem>('SELECT * FROM LineItems WHERE OrderId = @Id', new { order.Id }).ToList(); } Implementation B: var sql = 'SELECT o.*, l.* FROM Orders o LEFT JOIN LineItems l ON l.OrderId = o.Id'; var orderDict = new Dictionary<int, Order>(); var result = conn.Query<Order, LineItem, Order>(sql, (order, lineItem) => { if (!orderDict.TryGetValue(order.Id, out var existing)) { existing = order; existing.LineItems = new List<LineItem>(); orderDict[order.Id] = existing; } if (lineItem != null) existing.LineItems.Add(lineItem); return existing; }, splitOn: 'LineItemId').Distinct();",
    "c": "Context: Dapper multi-mapping. For 500 orders with average 10 line items: Implementation A issues 1 query for orders + 500 queries for line items = 501 round trips. Implementation B issues 1 query with a JOIN, returning 5,000 rows. Dapper splits the result into Order and LineItem objects using the splitOn parameter."
  },
  "function": "Arbitrate: which implementation is correct and why?",
  "acceptedOutput": {
    "verdict": "Implementation B is correct.",
    "reasoning": "Implementation A is the N+1 (N-plus-one) query anti-pattern: 1 query for N orders generates N additional queries for line items — 501 database round trips for 500 orders. Implementation B uses Dapper multi-mapping to load orders and line items in a single query with a JOIN. The dictionary deduplication pattern (orderDict) handles the one-to-many relationship: when the same OrderId appears on multiple rows (one per LineItem), the dictionary ensures one Order object accumulates all its LineItems rather than creating duplicate Order objects.",
    "performanceImpact": "For 500 orders with 10 line items each: Implementation A = 501 round trips. Implementation B = 1 round trip returning 5,000 rows. At 1ms round-trip latency, Implementation A = 501ms minimum. Implementation B = 1ms + row deserialization time (typically under 10ms for 5,000 rows)."
  },
  "rejectedOutput": {
    "verdict": "Both implementations are correct. Implementation A is simpler and easier to read. For small datasets, the performance difference is negligible."
  },
  "arbiters": ["databaseQueryArbiter", "nPlusOneDetectionArbiter"],
  "whyAccepted": "The N+1 query pattern is a correctness issue at scale, not just a performance optimization. Implementation A issues 501 database round trips; Implementation B issues 1. The rejected verdict's 'small dataset' caveat does not apply — the question does not specify a small dataset, and the code pattern scales with order count.",
  "whyRejected": "Labeling N+1 as 'simpler' without noting its scalability cost is misleading. At 500 orders with 1ms round-trip: 501ms query time vs 1ms. At 5,000 orders: 5,001ms vs 1ms. The 'simpler' implementation becomes unacceptable as data grows.",
  "sourceAnchors": [
    {
      "sourceId": "DAPPER",
      "url": "https://github.com/DapperLib/Dapper",
      "section": "Multi-mapping — splitOn parameter; one-to-many relationship loading; N+1 anti-pattern",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-212

```json
{
  "rowId": "EXT-212",
  "sourceIds": ["EFCORE-SOURCE"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G_LEARNING_TRAINING",
  "subdomain": "efcore-owned-entity-migration",
  "rowClass": "migration-decision",
  "input": {
    "a": "An EF Core 6 (Entity Framework Core 6) codebase stores Address as a separate table with a foreign key to Customer. The team is upgrading to EF Core 8 and a reviewer suggested migrating Address to an owned entity. Should we migrate?",
    "b": "Current schema: Customers table (CustomerId, Name, Email) and Addresses table (AddressId, CustomerId FK, Street, City, PostalCode, Country). EF Core 6 configuration: modelBuilder.Entity<Customer>().HasOne(c => c.Address).WithOne().HasForeignKey<Address>(a => a.CustomerId). Address is never accessed without its Customer.",
    "c": "EF Core owned entities: modelBuilder.Entity<Customer>().OwnsOne(c => c.Address). By default, EF Core 8 stores owned entities in the same table as the owner (table splitting) or in a separate table with OwnsOne().ToTable('Addresses'). Owned entities are: always loaded with the owner (no lazy loading surprise), cannot exist without an owner, and do not have their own DbSet. EF Core 8 also supports JSON column storage for owned entities: .ToJson()."
  },
  "function": "Decide: migrate Address to an owned entity or preserve the separate table relationship.",
  "acceptedOutput": {
    "decision": "MIGRATE to owned entity.",
    "rationale": "The Address entity satisfies all owned entity criteria: (1) It is always accessed with its Customer — no standalone Address queries exist. (2) It cannot exist without a Customer (1:1 relationship with FK). (3) There is no DbSet<Address> — it is not queried independently. Owned entity semantics make these constraints explicit in the model. EF Core 8 table-splitting stores Address columns in the Customers table, eliminating the JOIN for address data. For high-read customer profiles, this reduces query complexity.",
    "migrationSteps": [
      "Replace HasOne/WithOne/HasForeignKey configuration with OwnsOne(c => c.Address).",
      "Remove the Addresses table via migration: migrationBuilder.DropTable('Addresses'); — and add address columns to Customers.",
      "Remove DbSet<Address> from DbContext if present.",
      "Test: verify Customer queries include Address data without explicit Include()."
    ],
    "alternativeForSeparateTable": "If the team prefers to keep the separate table (e.g., for schema readability), use OwnsOne(c => c.Address, a => a.ToTable('Addresses')) — preserves separate table while enforcing owned entity semantics."
  },
  "rejectedOutput": {
    "decision": "DO NOT MIGRATE. The separate table structure is more flexible. Owned entities are harder to query and cannot be shared between multiple owners. The existing HasOne/WithOne configuration works correctly."
  },
  "arbiters": ["efCoreMigrationArbiter"],
  "whyAccepted": "The 'cannot be shared between multiple owners' caveat does not apply here — Address belongs exclusively to one Customer. The 'harder to query' concern is the opposite of reality: owned entities are always loaded with the owner without explicit Include() calls. The migration aligns the model with stated access patterns.",
  "whyRejected": "The rejected verdict uses generic owned entity limitations that do not apply to this specific case. Address is 1:1 with Customer, always loaded with Customer, and not shared — the objections are inapplicable.",
  "sourceAnchors": [
    {
      "sourceId": "EFCORE-SOURCE",
      "url": "https://github.com/dotnet/efcore",
      "section": "Owned entities — OwnsOne, table splitting, always loaded with owner; EF Core 8 JSON column storage",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-213

```json
{
  "rowId": "EXT-213",
  "sourceIds": ["CUCUMBER-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "B_ROUTING",
  "subdomain": "bdd-step-definition-design",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Our BDD (Behaviour-Driven Development) test suite has a scenario: 'Given the user is logged in as an admin, When the user navigates to the reports page, Then the monthly revenue report is displayed.' The step 'Given the user is logged in as an admin' is duplicated across 47 scenarios. Should we use a Background block or keep the Given step in each scenario?",
    "b": "Current state: 47 scenarios each begin with 'Given the user is logged in as an admin'. The step definition calls a login API and sets up a session. The login takes 800ms. Total test time contribution from login alone: 47 × 800ms = 37.6 seconds.",
    "c": "Context: Cucumber Background block. A Background block runs before every scenario in the feature file. Steps in Background appear once in the feature file and execute for each scenario that follows. Background is appropriate for steps that are true preconditions for all scenarios in the feature — not merely convenient defaults. If some scenarios need a non-admin user, Background is inappropriate because it forces all scenarios into a single precondition."
  },
  "function": "Produce the command: Background block or per-scenario Given step.",
  "acceptedOutput": {
    "command": "Use a Background block for the admin login step, contingent on all 47 scenarios requiring admin access.",
    "implementation": "Background:\n  Given the user is logged in as an admin\n\nScenario: Monthly revenue report is visible\n  When the user navigates to the reports page\n  Then the monthly revenue report is displayed\n\nScenario: Export report as PDF\n  When the user exports the monthly revenue report\n  Then a PDF file is downloaded",
    "rationale": "Background eliminates 47 duplicate 'Given the user is logged in as an admin' lines. Feature file readability improves — scenarios focus on their specific When/Then without repeating the precondition. If future scenarios need non-admin users, move them to a separate feature file with a different Background.",
    "sessionOptimization": "Consider also sharing the login session across scenarios using a BeforeAll hook for the feature — one login for all 47 scenarios instead of one per scenario. Reduces 37.6s to a single 800ms login."
  },
  "rejectedOutput": {
    "command": "Keep the Given step in each scenario. Background hides important context — a reader must scroll to the top of the feature file to understand the precondition. Each scenario should be self-contained."
  },
  "arbiters": ["bddDesignArbiter"],
  "whyAccepted": "Cucumber Background is designed for exactly this case — a precondition shared by all scenarios in a feature file. The 'reader must scroll to top' concern is addressed by the Background block's prominent placement at the top of the feature file. 47 duplicate Given steps obscure the distinct When/Then logic of each scenario.",
  "whyRejected": "The self-contained argument applies to scenarios with different preconditions. When all scenarios share an identical precondition, repeating it 47 times adds noise without information. Background centralizes the shared precondition in a named block that is visually distinct from scenario steps.",
  "sourceAnchors": [
    {
      "sourceId": "CUCUMBER-DOCS",
      "url": "https://github.com/cucumber/cucumber",
      "section": "Background — shared precondition for all scenarios in feature; vs per-scenario Given duplication",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-214

```json
{
  "rowId": "EXT-214",
  "sourceIds": ["SPECFLOW-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "B_ROUTING",
  "subdomain": "specflow-scenario-outline",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A SpecFlow feature file has 5 nearly identical scenarios that test the same discount calculation logic with different input values: 0%, 10%, 20%, 50%, and 100% discount applied to a $100 order. Should we use a Scenario Outline with Examples table or 5 separate Scenario blocks?",
    "b": "Current feature file has 5 scenarios, each with: Given an order total of $100, When a X% discount is applied, Then the final price is $Y. The only variation between scenarios is the discount percentage and expected final price.",
    "c": "Context: SpecFlow Scenario Outline (equivalent to Cucumber Scenario Outline). A Scenario Outline defines the scenario template once with placeholder variables (<discount>, <expected_price>). The Examples table provides rows of values. SpecFlow generates one test case per row. The 5-row Examples table replaces 5 separate Scenario blocks with one Scenario Outline plus 5 data rows."
  },
  "function": "Produce the command: Scenario Outline or 5 separate Scenario blocks.",
  "acceptedOutput": {
    "command": "Use a Scenario Outline with an Examples table.",
    "implementation": "Scenario Outline: Discount is applied to order total\n  Given an order total of $100\n  When a <discount>% discount is applied\n  Then the final price is $<expected_price>\n\n  Examples:\n    | discount | expected_price |\n    | 0        | 100.00         |\n    | 10       | 90.00          |\n    | 20       | 80.00          |\n    | 50       | 50.00          |\n    | 100      | 0.00           |",
    "rationale": "One Scenario Outline template + 5 data rows replaces 5 Scenario blocks. Adding a 6th discount tier (e.g., 15%) requires adding one row to the Examples table. With 5 separate scenarios, adding a 6th requires copying an entire Scenario block and updating values in 3 places (discount in When, price in Then, scenario name).",
    "testNaming": "SpecFlow names generated tests as 'Discount is applied to order total (discount: 0, expected_price: 100.00)' — each row produces a distinctly named test case."
  },
  "rejectedOutput": {
    "command": "Keep 5 separate Scenario blocks. Scenario Outlines are harder to read because you must cross-reference the template with the Examples table. Individual scenarios are more explicit."
  },
  "arbiters": ["bddDesignArbiter"],
  "whyAccepted": "Parameterized scenarios via Scenario Outline are the idiomatic mechanism for data-driven BDD tests. The 'harder to read' concern applies to complex scenarios with many variables. A 2-column Examples table with 5 rows is straightforward. Adding new test cases requires one data row, not one full scenario block.",
  "whyRejected": "5 separate scenarios for identical logic with different values violates the DRY (Don't Repeat Yourself) principle at the feature file level. If the discount calculation formula changes, the When/Then steps must be updated in 5 places. A Scenario Outline updates one template.",
  "sourceAnchors": [
    {
      "sourceId": "SPECFLOW-DOCS",
      "url": "https://github.com/SpecFlowOSS/SpecFlow",
      "section": "Scenario Outline — parameterized scenarios with Examples table; data-driven BDD testing",
      "license": "BSD-3-Clause"
    }
  ],
  "licenseConstraint": "BSD-3-Clause",
  "trainingStatus": "candidate"
}
```

---

## EXT-215

```json
{
  "rowId": "EXT-215",
  "sourceIds": ["MERMAID"],
  "sourceTrustTier": 1,
  "datasetFamily": "finalMachineResults_to_userFormat",
  "domain": "C_SEMANTIC_TRANSLATION",
  "subdomain": "diagram-type-selection",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Format this system description as a diagram: 'When a user places an order, the Order Service publishes an OrderPlaced event. The Inventory Service consumes this event and reserves stock. The Notification Service consumes this event and sends a confirmation email. If stock is insufficient, the Inventory Service publishes an InsufficientStock event, which the Order Service consumes to cancel the order.'",
    "b": "Diagram candidates: (1) Mermaid sequence diagram — shows time-ordered message exchanges between named participants. (2) Mermaid flowchart — shows decision flow and process steps as boxes and arrows.",
    "c": "Context: The description is a choreographed event flow between three services. The key relationships are: which service publishes which event, which service consumes which event, and what happens in response. A sequence diagram shows the time-ordered message sequence between named actors. A flowchart shows decision branching but does not naturally represent multiple independent consumers of a single event."
  },
  "function": "Produce the formatted diagram for the system description.",
  "acceptedOutput": {
    "diagramType": "Mermaid sequence diagram",
    "diagram": "sequenceDiagram\n    actor User\n    participant OrderSvc as Order Service\n    participant InventorySvc as Inventory Service\n    participant NotifySvc as Notification Service\n\n    User->>OrderSvc: Place order\n    OrderSvc-->>InventorySvc: OrderPlaced event\n    OrderSvc-->>NotifySvc: OrderPlaced event\n    InventorySvc->>InventorySvc: Reserve stock\n    NotifySvc->>User: Send confirmation email\n\n    alt Insufficient stock\n        InventorySvc-->>OrderSvc: InsufficientStock event\n        OrderSvc->>OrderSvc: Cancel order\n        OrderSvc->>User: Order cancelled notification\n    end",
    "rationale": "A sequence diagram places each service as a named participant. The dashed arrows (-->> for async events) visually distinguish event publishing from synchronous calls. The 'alt' block represents the conditional InsufficientStock path. Both Inventory and Notification Services are shown as independent consumers of the same OrderPlaced event."
  },
  "rejectedOutput": {
    "diagramType": "Mermaid flowchart",
    "diagram": "flowchart TD\n    A[User places order] --> B[Order Service publishes OrderPlaced]\n    B --> C[Inventory Service reserves stock]\n    B --> D[Notification Service sends email]\n    C -->|Insufficient stock| E[Inventory publishes InsufficientStock]\n    E --> F[Order Service cancels order]"
  },
  "arbiters": ["diagramSelectionArbiter"],
  "whyAccepted": "A sequence diagram is the correct diagram type for event-choreography between services. It shows which actor sends each message, in what order, and to which recipient. The parallel consumption of OrderPlaced by both Inventory and Notification Services is represented as two simultaneous dashed arrows from OrderSvc.",
  "whyRejected": "The flowchart collapses the actor-message structure into process boxes. 'B → C' implies Order Service directly triggers Inventory Service, obscuring the event-driven nature. The event (OrderPlaced) appears as a box rather than as a message on a communication channel. Flowcharts are better suited to decision-tree processes than multi-actor message flows.",
  "sourceAnchors": [
    {
      "sourceId": "MERMAID",
      "url": "https://github.com/mermaid-js/mermaid",
      "section": "Sequence diagram — participants, async message arrows, alt blocks for conditional flows",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-216

```json
{
  "rowId": "EXT-216",
  "sourceIds": ["PLANTUML-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "finalMachineResults_to_userFormat",
  "domain": "C_SEMANTIC_TRANSLATION",
  "subdomain": "component-diagram-structuring",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Format this architecture description as a PlantUML component diagram: 'The API Gateway routes requests to the Auth Service, Order Service, and Product Service. The Order Service calls the Product Service to check availability. The Order Service and Product Service both read from a shared PostgreSQL database. The Auth Service has its own Redis cache.'",
    "b": "Component diagram candidates: (A) Flat diagram with all components at the same level. (B) Grouped diagram with components organized into 'External Facing' and 'Internal Services' packages.",
    "c": "Context: PlantUML component diagrams support package grouping with 'package' or 'node' blocks. For a 5-component diagram, grouping clarifies which components are exposed to external traffic (API Gateway) vs internal-only (shared database). Dependencies between components use arrows with direction and optional labels."
  },
  "function": "Produce the PlantUML component diagram for the architecture description.",
  "acceptedOutput": {
    "diagram": "@startuml\npackage \"Edge\" {\n  [API Gateway]\n}\n\npackage \"Services\" {\n  [Auth Service]\n  [Order Service]\n  [Product Service]\n}\n\npackage \"Data\" {\n  database \"PostgreSQL\" as PG\n  [Redis Cache]\n}\n\n[API Gateway] --> [Auth Service] : authenticate\n[API Gateway] --> [Order Service] : route orders\n[API Gateway] --> [Product Service] : route products\n[Order Service] --> [Product Service] : check availability\n[Order Service] --> PG : read/write\n[Product Service] --> PG : read/write\n[Auth Service] --> [Redis Cache] : session cache\n@enduml",
    "rationale": "Package grouping (Edge, Services, Data) visually separates architectural tiers. The shared PostgreSQL is in the Data package — two arrows pointing to it from Order Service and Product Service make the sharing explicit. Redis is co-located with the Auth Service in the Data package to show it is Auth-specific."
  },
  "rejectedOutput": {
    "diagram": "@startuml\n[API Gateway] -> [Auth Service]\n[API Gateway] -> [Order Service]\n[API Gateway] -> [Product Service]\n[Order Service] -> [Product Service]\n[Order Service] -> [PostgreSQL]\n[Product Service] -> [PostgreSQL]\n[Auth Service] -> [Redis]\n@enduml"
  },
  "arbiters": ["diagramStructureArbiter"],
  "whyAccepted": "Package grouping organizes 7 components into 3 tiers, making the architectural separation legible at a glance. Arrows carry labels that clarify the nature of each dependency. The database is typed with 'database' notation (cylinder shape) rather than a generic component box.",
  "whyRejected": "The flat diagram shows all 7 components at the same level without structural grouping. The architectural tier separation (edge vs services vs data) is not represented. All arrows are unlabeled, so the distinction between 'authenticate', 'route orders', and 'check availability' is lost.",
  "sourceAnchors": [
    {
      "sourceId": "PLANTUML-DOCS",
      "url": "https://github.com/plantuml/plantuml",
      "section": "Component diagram — package grouping, database notation, labeled arrows",
      "license": "GPL-3.0"
    }
  ],
  "licenseConstraint": "GPL-3.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-217

```json
{
  "rowId": "EXT-217",
  "sourceIds": ["WIKTIONARY"],
  "sourceTrustTier": 2,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "C_SEMANTIC_TRANSLATION",
  "subdomain": "technical-term-disambiguation",
  "rowClass": "negative-example",
  "input": {
    "a": "Execute: translate the sentence 'The library provides a high-level interface to the socket API' from English to French for a software documentation context.",
    "b": "Key terms requiring disambiguation: 'library' (French: bibliothèque for software library, NOT for book library in this context), 'interface' (French: interface — same word, but genre: feminine 'une interface'), 'socket' (French: socket — adopted English technical term, or 'connecteur réseau' in formal contexts), 'API' (acronyme English, used as-is in French technical documentation).",
    "c": "Wiktionary corpus: 'bibliothèque' in French has two primary senses: (1) physical or digital collection of books (bibliothèque de livres), (2) software library (bibliothèque logicielle, e.g., bibliothèque Python). The software sense is the correct one here. 'Socket' in French technical documentation is typically kept as 'socket' (masculine, 'le socket') rather than translated."
  },
  "function": "Produce the French translation of the sentence for software documentation.",
  "acceptedOutput": {
    "translation": "La bibliothèque fournit une interface de haut niveau pour l'API socket.",
    "annotations": {
      "library": "bibliothèque — software library sense (not book collection). In French software docs, 'bibliothèque' is the standard term for library.",
      "interface": "interface — feminine noun in French (une interface, l'interface). 'de haut niveau' = high-level.",
      "socket": "socket — adopted English term, used as-is in French technical documentation. Masculine: 'le socket', 'l'API socket'.",
      "API": "API — acronym kept in English form; treated as feminine in French (l'API) following standard French technical style."
    }
  },
  "rejectedOutput": {
    "translation": "La librairie fournit une interface de haut niveau pour l'API de prise."
  },
  "arbiters": ["technicalTranslationArbiter"],
  "whyAccepted": "The accepted translation uses 'bibliothèque' (correct technical term for software library in French) and 'socket' (the adopted English term used in French technical documentation). It correctly treats 'API' as a feminine acronym following French technical convention.",
  "whyRejected": "'Librairie' in French means 'bookshop' (a physical store that sells books) — it does not mean software library. Using 'librairie' for 'library' is a false cognate error. 'Prise' means 'socket' in the electrical outlet sense, not in the network programming sense — 'l'API de prise' is a mistranslation.",
  "sourceAnchors": [
    {
      "sourceId": "WIKTIONARY",
      "url": "https://www.wiktionary.org",
      "section": "bibliothèque — software library sense vs book collection; librairie — bookshop false cognate",
      "license": "CC-BY-SA-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-SA-4.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-218

```json
{
  "rowId": "EXT-218",
  "sourceIds": ["DIATAXIS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "documentation-type-classification",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Classify this documentation piece: 'Setting up your development environment. This guide walks you through installing Node.js, configuring your .env file, and running the application locally for the first time.' Is it a tutorial or a how-to guide?",
    "b": "Two classification agents submitted verdicts: Agent 1: TUTORIAL — it guides a new user step-by-step through a setup process for the first time. Agent 2: HOW-TO GUIDE — it addresses a specific goal (set up dev environment) with concrete steps.",
    "c": "Diataxis framework definitions: Tutorial — learning-oriented; designed for beginners who are learning, not accomplishing a specific task; takes the learner through a safe exploration of the domain; success is measured by what the learner has learned, not by what they have produced. How-to guide — task-oriented; designed for a practitioner who knows what they want to achieve; provides steps to accomplish a specific goal; success is measured by whether the goal is achieved. Key distinction: 'for the first time' and 'new user orientation' suggests tutorial; 'specific actionable goal with steps' suggests how-to guide."
  },
  "function": "Arbitrate: tutorial or how-to guide?",
  "acceptedOutput": {
    "verdict": "HOW-TO GUIDE.",
    "reasoning": "The document targets a practitioner with a specific goal: get the development environment running. The phrase 'for the first time' describes the context (first setup), not the purpose. The reader is not learning about development environments conceptually — they are accomplishing the specific task of running the application locally. A Diataxis tutorial would include explanations of why each step matters and safe-to-fail exploration; this document provides the steps without conceptual explanation. The steps (install Node.js, configure .env, run application) are goal-directed with a concrete success criterion: application runs locally.",
    "tutorialIndicators": "A tutorial version would include: 'Why do we need a .env file? Environment variables allow...' and 'Let's explore what happens if we skip the .env configuration...'"
  },
  "rejectedOutput": {
    "verdict": "TUTORIAL — the document is written for first-time users learning the setup process."
  },
  "arbiters": ["documentationClassificationArbiter"],
  "whyAccepted": "The Diataxis tutorial/how-to distinction is purpose, not audience experience level. A first-time user can follow a how-to guide. The key marker is whether the document prioritizes learning (tutorial) or accomplishment (how-to guide). This document has no conceptual explanation — it only provides steps to reach the goal, which is the defining characteristic of a how-to guide.",
  "whyRejected": "'For the first time' describes the reader's history with the task, not the document's purpose. A how-to guide can be followed by a first-time user. The rejected verdict conflates beginner audience with tutorial purpose — Diataxis explicitly separates these.",
  "sourceAnchors": [
    {
      "sourceId": "DIATAXIS",
      "url": "https://diataxis.fr",
      "section": "Tutorial vs How-to guide — learning-oriented vs task-oriented; first-time use vs first-time learner",
      "license": "CC-BY-SA-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-SA-4.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-219

```json
{
  "rowId": "EXT-219",
  "sourceIds": ["ASPNETCORE-DOCS-SAMPLES"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A_TOPOLOGY",
  "subdomain": "blazor-hosting-model-selection",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "We're building an internal HR (Human Resources) dashboard for 200 employees. It needs real-time updates (employee status changes, leave requests). Should we use Blazor Server or Blazor WebAssembly?",
    "b": "Constraints: (1) All users are on the corporate network with reliable low-latency connections. (2) The dashboard integrates with an internal SQL Server database via EF Core. (3) We need real-time push updates when employee data changes. (4) Initial load time should be under 2 seconds.",
    "c": "Context: Blazor Server — runs on the server, uses SignalR websocket for UI updates. Low initial download (no .NET runtime in browser). Server-side DB access is direct — no API layer needed. Latency-sensitive for UI interactions (each click requires a round-trip to the server). Blazor WebAssembly — downloads .NET runtime to the browser (3–10 MB initial load). All logic runs client-side after load. DB access requires an API layer. Native offline support possible."
  },
  "function": "Produce the command: Blazor Server or Blazor WebAssembly.",
  "acceptedOutput": {
    "command": "Use Blazor Server.",
    "rationale": "Four constraints align with Blazor Server: (1) Corporate network — reliable low-latency connection eliminates the latency-sensitive SignalR concern. (2) EF Core integration — Blazor Server can use EF Core directly via DbContext injection, no REST API layer required. (3) Real-time updates — SignalR (already used by Blazor Server for UI) can also push server-initiated events to the client without additional infrastructure. (4) Initial load — Blazor Server initial download is under 100KB (no .NET runtime); page is interactive in under 1 second on a local network.",
    "signaRForRealtime": "Add a background service that monitors the database for changes (EF Core interceptors or SQL Server change tracking) and pushes updates to connected Blazor Server clients via IHubContext<BlazorHub>."
  },
  "rejectedOutput": {
    "command": "Use Blazor WebAssembly. It runs entirely in the browser, reducing server load. Real-time updates can be added via SignalR client."
  },
  "arbiters": ["blazorHostingArbiter"],
  "whyAccepted": "All four constraints (corporate network, EF Core access, real-time push, fast initial load) favor Blazor Server. Blazor WebAssembly requires an API layer for DB access, adds 3–10 MB initial download, and requires a separate SignalR hub for real-time — three additional architecture layers vs zero for Blazor Server in this scenario.",
  "whyRejected": "Blazor WebAssembly's server load reduction is irrelevant for a 200-user internal dashboard. The 3–10 MB initial download violates the 2-second load target on typical corporate connections. The EF Core access via API requirement adds significant development overhead without benefit in this scenario.",
  "sourceAnchors": [
    {
      "sourceId": "ASPNETCORE-DOCS-SAMPLES",
      "url": "https://github.com/dotnet/AspNetCore.Docs.Samples",
      "section": "Blazor Server vs WebAssembly — hosting model selection; SignalR for real-time; EF Core integration",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-220

```json
{
  "rowId": "EXT-220",
  "sourceIds": ["DOTNET-MICROSERVICES"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "outbox-pattern-implementation",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Review this Order Service implementation that saves an order and publishes an event.",
    "b": "Implementation A: using var transaction = await _db.Database.BeginTransactionAsync(); try { _db.Orders.Add(order); await _db.SaveChangesAsync(); await _messageBus.PublishAsync(new OrderPlacedEvent(order.Id)); await transaction.CommitAsync(); } catch { await transaction.RollbackAsync(); throw; } Implementation B: using var transaction = await _db.Database.BeginTransactionAsync(); _db.Orders.Add(order); _db.OutboxMessages.Add(new OutboxMessage { Type = 'OrderPlaced', Payload = JsonSerializer.Serialize(new OrderPlacedEvent(order.Id)) }); await _db.SaveChangesAsync(); await transaction.CommitAsync(); // A background worker reads OutboxMessages and publishes to the message bus",
    "c": "Context: the transactional outbox pattern. Message bus publish (_messageBus.PublishAsync) is not transactional — if the database commits but the publish fails, the event is lost. If the database rollbacks but the publish already succeeded, a phantom event is published. Wrapping both in a database transaction does not make the message bus transactional — it only controls the database portion."
  },
  "function": "Arbitrate: which implementation is correct?",
  "acceptedOutput": {
    "verdict": "Implementation B is correct — transactional outbox pattern.",
    "reasoning": "Implementation A has a dual-write consistency problem: the database commit and message bus publish are two separate operations with no atomicity guarantee. Failure scenarios: (1) DB commits, publish fails → order saved, event lost → Inventory Service never receives OrderPlaced → order never fulfilled. (2) DB rollbacks, publish already sent → event published for an order that does not exist → Inventory Service processes a ghost order. Implementation B writes the outbox message in the same database transaction as the order. The outbox table write and order write are atomic. The background worker reads the outbox and publishes — if publish fails, the outbox message remains unprocessed and the worker retries.",
    "outboxWorkerPattern": "The background worker uses a polling loop or CDC (Change Data Capture) trigger. It reads OutboxMessages WHERE processed = false, publishes each, marks processed = true. At-least-once delivery — the message bus must handle idempotent consumers."
  },
  "rejectedOutput": {
    "verdict": "Implementation A is correct. The database transaction wraps both the order save and the publish — they are atomic together."
  },
  "arbiters": ["distributedConsistencyArbiter", "outboxPatternArbiter"],
  "whyAccepted": "The database transaction in Implementation A controls only the database operations. _messageBus.PublishAsync is an external I/O call — it is not part of the database transaction. The rejected verdict contains a factual error: wrapping a message bus publish in a database transaction does not make the publish transactional.",
  "whyRejected": "Database transactions are scoped to the database. External I/O calls within a transaction block are not covered by transaction rollback. This is a fundamental distributed systems constraint, not a framework limitation.",
  "sourceAnchors": [
    {
      "sourceId": "DOTNET-MICROSERVICES",
      "url": "https://github.com/dotnet-architecture/eShopOnContainers",
      "section": "Transactional outbox — OutboxMessage table, background worker, dual-write consistency problem",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-221

```json
{
  "rowId": "EXT-221",
  "sourceIds": ["BIGCODEBENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "merge-sorted-streams",
  "rowClass": "negative-example",
  "input": {
    "a": "Execute: merge K sorted iterators into a single sorted output iterator. Each input iterator yields integers in ascending order. Memory constraint: do not load all values into memory simultaneously.",
    "b": "Function signature: merge_sorted_streams(streams: list[Iterator[int]]) -> Iterator[int]. Example: streams = [iter([1,4,7]), iter([2,5,8]), iter([3,6,9])]. Expected output: 1,2,3,4,5,6,7,8,9.",
    "c": "Algorithm context: K-way merge using a min-heap (priority queue). A min-heap of size K stores one element per stream. Each heap entry is a tuple (value, stream_index) so ties are broken by stream index. Each iteration: pop the minimum from the heap, yield it, advance the corresponding stream, push the next value from that stream onto the heap. Time complexity: O(N log K) where N is total elements across all streams. Memory: O(K) — only K values in the heap at any time."
  },
  "function": "Implement merge_sorted_streams as a generator using a min-heap.",
  "acceptedOutput": {
    "implementation": "import heapq\n\ndef merge_sorted_streams(streams):\n    heap = []\n    iterators = [iter(s) for s in streams]\n    for i, it in enumerate(iterators):\n        try:\n            val = next(it)\n            heapq.heappush(heap, (val, i, it))\n        except StopIteration:\n            pass  # empty stream: skip\n    while heap:\n        val, i, it = heapq.heappop(heap)\n        yield val\n        try:\n            next_val = next(it)\n            heapq.heappush(heap, (next_val, i, it))\n        except StopIteration:\n            pass  # stream exhausted: do not push",
    "complexity": "Time: O(N log K), Memory: O(K). For K=3 streams with N=9 total elements: heap never exceeds 3 entries."
  },
  "rejectedOutput": {
    "implementation": "def merge_sorted_streams(streams):\n    all_values = []\n    for stream in streams:\n        all_values.extend(list(stream))\n    all_values.sort()\n    return iter(all_values)"
  },
  "arbiters": ["algorithmEfficiencyArbiter"],
  "whyAccepted": "The accepted implementation uses a min-heap of size K — memory is O(K) regardless of N. It is a generator — it yields one element at a time, never materializing the full output. For very large streams (gigabytes of sorted data), O(K) memory is the correct bound.",
  "whyRejected": "The rejected implementation loads all values from all streams into memory (O(N)), sorts them (O(N log N) — worse than O(N log K) for large K), and returns a non-lazy iterator. This violates the memory constraint: for large N, loading all values simultaneously is infeasible.",
  "sourceAnchors": [
    {
      "sourceId": "BIGCODEBENCH",
      "url": "https://github.com/bigcode-project/bigcodebench",
      "section": "Task: merge_sorted_streams — K-way merge with min-heap O(K) memory vs sort-all O(N) memory",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-222

```json
{
  "rowId": "EXT-222",
  "sourceIds": ["SWE-BENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "domainResultsArray_to_resultI",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "infrastructure-drift-detection",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Aggregate results from three analysis agents for a pull request that adds a new microservice deployment manifest to the repository.",
    "b": "Agent results: Agent 1 (Security Agent): WARN — the Kubernetes Deployment has no resource limits (no limits.cpu, no limits.memory). Agent 2 (Test Agent): PASS — unit tests for the new service pass. Agent 3 (Configuration Agent): WARN — the deployment manifest uses 'latest' image tag instead of a pinned version digest.",
    "c": "Context: infrastructure deployment review policy. Resource limits absence: a pod without resource limits can consume unlimited CPU/memory, causing node eviction of other pods (noisy neighbor problem). 'Latest' image tag: the image pulled at deployment time may differ from the image tested — 'latest' is mutable and can change between CI and production deployment. Both WARNs represent production reliability risks. Policy: two or more WARNs on infrastructure manifests = REQUIRES_MANUAL_REVIEW."
  },
  "function": "Produce the aggregated result for this pull request.",
  "acceptedOutput": {
    "aggregatedResult": "REQUIRES_MANUAL_REVIEW",
    "resultCode": "INFRASTRUCTURE_POLICY_VIOLATIONS",
    "summary": "Two infrastructure policy violations prevent automated merge approval: (1) Missing resource limits — the deployment can consume unbounded CPU and memory, risking node-level resource exhaustion and eviction of co-located pods. (2) Mutable image tag ('latest') — the deployed image may differ from the tested image if the registry upstream changes between CI and production deployment.",
    "requiredActions": [
      "Add resource limits to the Deployment: resources: { limits: { cpu: '500m', memory: '512Mi' }, requests: { cpu: '100m', memory: '128Mi' } }",
      "Replace 'latest' image tag with a pinned digest: image: myrepo/myservice@sha256:<digest>"
    ],
    "passingAgents": ["Test Agent"],
    "warningAgents": ["Security Agent (no resource limits)", "Configuration Agent (mutable image tag)"]
  },
  "rejectedOutput": {
    "aggregatedResult": "PASS_WITH_ADVISORY",
    "summary": "Tests pass. Two warnings noted — resource limits and image tag. Recommend addressing in a follow-up PR."
  },
  "arbiters": ["infrastructureReviewArbiter"],
  "whyAccepted": "Two WARNs on an infrastructure deployment manifest meet the REQUIRES_MANUAL_REVIEW threshold per policy. Neither warning is a cosmetic concern: missing resource limits is a production reliability defect; a mutable image tag breaks deployment reproducibility. 'Follow-up PR' deferral is inappropriate — the manifest will go to production in the state submitted.",
  "whyRejected": "PASS_WITH_ADVISORY treats infrastructure policy violations as non-blocking advisories. For deployment manifests, a resource limits absence is not advisory — it is a reliability defect that will affect production once deployed. The 'follow-up PR' pattern allows known defects to reach production.",
  "sourceAnchors": [
    {
      "sourceId": "SWE-BENCH",
      "url": "https://github.com/princeton-nlp/SWE-bench",
      "section": "Infrastructure manifest review — resource limits absence, mutable image tag; REQUIRES_MANUAL_REVIEW threshold",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-223

```json
{
  "rowId": "EXT-223",
  "sourceIds": ["OPENAPI-SPEC"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "openapi-schema-composition",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An API returns a 'Shape' response that can be either a Circle (with 'radius') or a Rectangle (with 'width' and 'height'). The discriminator field is 'type' ('circle' or 'rectangle'). Should the OpenAPI schema use oneOf, anyOf, or allOf?",
    "b": "Current schema attempt: { anyOf: [ { $ref: '#/components/schemas/Circle' }, { $ref: '#/components/schemas/Rectangle' } ] } — the API developer chose anyOf because 'it seemed more flexible.'",
    "c": "OpenAPI 3.x composition keywords: oneOf — validates against exactly one of the listed schemas. anyOf — validates against one or more of the listed schemas. allOf — validates against all of the listed schemas (used for inheritance/composition). For a discriminated union where a value is either a Circle or a Rectangle but never both, oneOf is semantically correct. anyOf allows a value to match multiple schemas simultaneously, which is incorrect for a mutually exclusive type union."
  },
  "function": "Arbitrate: which composition keyword is correct for this schema?",
  "acceptedOutput": {
    "verdict": "Use oneOf with a discriminator.",
    "schema": "Shape:\n  oneOf:\n    - $ref: '#/components/schemas/Circle'\n    - $ref: '#/components/schemas/Rectangle'\n  discriminator:\n    propertyName: type\n    mapping:\n      circle: '#/components/schemas/Circle'\n      rectangle: '#/components/schemas/Rectangle'\n\nCircle:\n  type: object\n  required: [type, radius]\n  properties:\n    type:\n      type: string\n      enum: [circle]\n    radius:\n      type: number\n\nRectangle:\n  type: object\n  required: [type, width, height]\n  properties:\n    type:\n      type: string\n      enum: [rectangle]\n    width:\n      type: number\n    height:\n      type: number",
    "reasoning": "oneOf enforces mutual exclusivity — a shape is either a Circle or a Rectangle, never both. The discriminator.propertyName='type' tells OpenAPI tooling (code generators, validators) which field to use for type dispatch. anyOf would allow a JSON object with both 'radius' and 'width'/'height' to validate successfully, which violates the API contract."
  },
  "rejectedOutput": {
    "verdict": "Keep anyOf. anyOf is more flexible — it allows future schemas to overlap if needed. It also validates the same way as oneOf for non-overlapping schemas."
  },
  "arbiters": ["openApiSchemaArbiter"],
  "whyAccepted": "oneOf is the correct semantic for mutually exclusive type unions. The discriminator keyword is only valid with oneOf in OpenAPI 3.x. Code generators use the discriminator to generate correct type dispatch logic — this requires oneOf semantics.",
  "whyRejected": "anyOf does not guarantee mutual exclusivity. For a Circle response body, anyOf would allow a JSON object with both circle and rectangle fields to pass validation if it satisfies at least one schema. The discriminator keyword is not semantically meaningful with anyOf. 'More flexible' is not a design goal when mutual exclusivity is a contract requirement.",
  "sourceAnchors": [
    {
      "sourceId": "OPENAPI-SPEC",
      "url": "https://github.com/OAI/OpenAPI-Specification",
      "section": "Schema composition — oneOf vs anyOf for discriminated unions; discriminator.propertyName",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-224

```json
{
  "rowId": "EXT-224",
  "sourceIds": ["MICROSOFT-LEARN-ARCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A_TOPOLOGY",
  "subdomain": "cqrs-read-write-separation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Our e-commerce product catalog has a write model (product creation, price updates, inventory changes) and a read model (product search, filtered listings, product detail pages). Read traffic is 50× write traffic. The same SQL queries handle both reads and writes, and the read queries are slow due to complex joins for search. Should we implement CQRS?",
    "b": "Current architecture: single ProductService with a single ProductRepository reading/writing the same SQL Server tables. Read queries join Products, Categories, Inventory, and Pricing tables. Read p99 (99th percentile) latency: 850ms. Write operations: p99 latency 120ms.",
    "c": "CQRS (Command Query Responsibility Segregation): separates the write model (commands change state, return no data) from the read model (queries return data, change no state). The read model can be a denormalized projection optimized for query patterns — an Elasticsearch index, a Redis cache, or a denormalized SQL read-only replica. The read and write models are kept in sync via events or change data capture."
  },
  "function": "Produce the command: implement CQRS or optimize the existing queries.",
  "acceptedOutput": {
    "command": "Implement CQRS with a separated read model.",
    "rationale": "50× read-to-write ratio means optimization effort on the read model has 50× the impact of write optimization. The 850ms p99 read latency indicates the join complexity is inherent to the normalized schema — index optimization has limited impact on multi-table joins. CQRS read model options for product search: (1) Elasticsearch index with denormalized product documents (name, category, price, inventory — no joins). (2) Materialized SQL view refreshed on product change events. (3) Redis cache with pre-computed product detail objects.",
    "syncStrategy": "Publish ProductUpdated events when the write model changes. A projection service consumes events and updates the read model (Elasticsearch document or Redis cache). Eventual consistency: the read model lags write model by milliseconds to seconds.",
    "quantifiedBenefit": "Elasticsearch product search: sub-50ms p99 for filtered queries with full-text search. Redis product detail: sub-5ms p99 for cached product pages. Write model latency unchanged."
  },
  "rejectedOutput": {
    "command": "Add database indexes and optimize the existing queries. CQRS adds significant complexity. Query optimization is simpler and should be tried first."
  },
  "arbiters": ["architecturePatternArbiter"],
  "whyAccepted": "The 850ms p99 read latency on normalized SQL joins with 4 tables suggests that index optimization has already been applied (typical for a production system with active DBA oversight). The 50× read-to-write ratio makes a denormalized read model the correct architectural decision, not a premature optimization.",
  "whyRejected": "Index optimization is the correct first step for a 50ms query, not an 850ms query. At 850ms, the bottleneck is join cardinality and result set size, not index lookup. Adding indexes to 4-table joins improves performance by 20–40% — not the 17× improvement needed to reach 50ms p99.",
  "sourceAnchors": [
    {
      "sourceId": "MICROSOFT-LEARN-ARCH",
      "url": "https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs",
      "section": "CQRS pattern — read/write model separation; 50x read traffic ratio; Elasticsearch projection",
      "license": "CC-BY-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-225

```json
{
  "rowId": "EXT-225",
  "sourceIds": ["OPENASSISTANT-OASST1"],
  "sourceTrustTier": 2,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "C_SEMANTIC_TRANSLATION",
  "subdomain": "database-migration-failure-communication",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Translate this machine result into a user-facing message: { status: 'MIGRATION_FAILED', migrationName: '20260504_AddUserRolesTable', errorCode: 'DUPLICATE_COLUMN', affectedTable: 'Users', columnName: 'RoleId', environment: 'production' }",
    "b": "Target audience: senior developer who ran the migration from the CI/CD (Continuous Integration/Continuous Delivery) pipeline. The developer is familiar with database migrations but may not know why this specific migration failed.",
    "c": "DUPLICATE_COLUMN error: the column 'RoleId' already exists in the 'Users' table. This typically happens when: (1) the migration was partially applied manually before the automated run, (2) a previous migration added the same column under a different migration name, (3) the database schema has diverged from the migration history."
  },
  "function": "Produce the final user-facing message for the database migration failure.",
  "acceptedOutput": {
    "userMessage": "Database migration failed in production: 20260504_AddUserRolesTable",
    "detail": "The migration failed because column 'RoleId' already exists in the 'Users' table (DUPLICATE_COLUMN). The migration attempted to add a column that is already present in the production schema.",
    "likelyCauses": [
      "The column was added manually or via a hotfix before this migration ran",
      "A previous migration added 'RoleId' under a different migration name not tracked in this history",
      "The production schema has diverged from the migration history"
    ],
    "nextSteps": [
      "Verify the current schema: SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'RoleId'",
      "Check migration history: SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId",
      "If the column exists with the correct definition, mark the migration as applied: INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('20260504_AddUserRolesTable', '8.0.0')",
      "Do NOT drop the existing column and re-add it — data loss risk"
    ],
    "severity": "PRODUCTION_BLOCKED — the migration failure has left the production database in a state that may block application startup"
  },
  "rejectedOutput": {
    "userMessage": "Migration 20260504_AddUserRolesTable failed with error DUPLICATE_COLUMN on table Users column RoleId. Check the database."
  },
  "arbiters": ["userCommunicationArbiter", "databaseOpsArbiter"],
  "whyAccepted": "The accepted message explains what DUPLICATE_COLUMN means, provides the three most likely causes specific to this error code, and gives safe next steps including the warning against dropping the column. The PRODUCTION_BLOCKED severity indicator alerts the developer to urgency.",
  "whyRejected": "The rejected message repeats the error code without explanation and gives no actionable guidance. A developer who does not know why the column is duplicated receives no useful information for diagnosis.",
  "sourceAnchors": [
    {
      "sourceId": "OPENASSISTANT-OASST1",
      "url": "https://huggingface.co/datasets/OpenAssistant/oasst1",
      "section": "Database migration failure communication — DUPLICATE_COLUMN cause analysis and safe remediation steps",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-226

```json
{
  "rowId": "EXT-226",
  "sourceIds": ["GOOGLE-AIP"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "api-filtering-design",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "We're designing a product list endpoint that supports filtering by category, price range, and availability. Two approaches: (A) separate query parameters per filter: GET /products?category=electronics&min_price=10&max_price=500&available=true. (B) a single filter expression: GET /products?filter=category='electronics' AND price>=10 AND price<=500 AND available=true.",
    "b": "The API will be consumed by a mobile app, a web frontend, and partner integrations via the public API. The product catalog has 20 filterable attributes. Partners have requested complex filter combinations (OR conditions, nested predicates).",
    "c": "Google AIP-160 defines a Common Expression Language (CEL)-based filter expression standard for Google APIs. AIP-160 filter expressions support: field comparison (field='value'), logical operators (AND, OR, NOT), range queries (price>=10), and nested conditions. Separate query parameters: each filter attribute requires a documented query parameter. For 20 attributes, this means 20+ query parameters. OR conditions require multiple values per parameter or multiple requests."
  },
  "function": "Arbitrate: separate query parameters or AIP-160 filter expression?",
  "acceptedOutput": {
    "verdict": "Use AIP-160 filter expression for the partner API.",
    "reasoning": "Three factors favor AIP-160 filter expression: (1) 20 filterable attributes = 20+ query parameters to document, validate, and version independently. A single 'filter' parameter replaces all 20. (2) Partner OR conditions: filter=category='electronics' OR category='computers' is natural in AIP-160; with separate parameters, OR requires ?category=electronics&category=computers — non-standard multi-value behavior. (3) AIP-160 is a published standard — partner developers may already know CEL from Google APIs.",
    "mobileWebConsideration": "For the mobile app and web frontend, provide a simplified parameter helper alongside the filter expression: short-circuit the common cases (single category + price range) with pre-built filter templates. Do not expose raw filter expression to end-user UI — use it in the API contract for machine consumers.",
    "validation": "Validate filter expressions on the server before execution — reject expressions that reference non-existent fields or use unsupported operators. Return a 400 with the invalid filter expression and error message."
  },
  "rejectedOutput": {
    "verdict": "Use separate query parameters. Filter expressions are complex to parse and validate. Query parameters are simpler for clients to construct and are universally understood."
  },
  "arbiters": ["apiDesignArbiter"],
  "whyAccepted": "Separate parameters scale to 4-5 filters. At 20 attributes with OR combinations, the parameter surface is unmanageable. AIP-160 is the correct design for a complex filter surface. The 'simpler for clients' argument is true for simple cases; for the partner use case (complex predicates), filter expressions are simpler than constructing 20+ parameters.",
  "whyRejected": "At 20 attributes, 'universally understood query parameters' requires partners to understand 20 parameter names, their interaction semantics, and the non-standard OR convention. Filter expression syntax is more complex for simple cases but simpler for complex cases — the partner requirement for complex filters tips the balance.",
  "sourceAnchors": [
    {
      "sourceId": "GOOGLE-AIP",
      "url": "https://google.aip.dev/160",
      "section": "AIP-160: Filtering — CEL expression vs per-field query parameters; OR conditions; 20-attribute surface",
      "license": "CC-BY-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-227

```json
{
  "rowId": "EXT-227",
  "sourceIds": ["TATOEBA"],
  "sourceTrustTier": 2,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "C_SEMANTIC_TRANSLATION",
  "subdomain": "en-es-technical-translation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Decompose this localization task: translate the UI string 'Your request has been queued and will be processed within 5 minutes.' from English to Spanish for a Latin American audience.",
    "b": "Technical context: the string appears in a task scheduling UI. 'Queued' refers to a computer science queue (cola), not a physical line. 'Processed' refers to computer processing (procesado), not food processing. Time estimate: '5 minutes' is a UI-displayed estimate.",
    "c": "Latin American Spanish vs Castilian Spanish: 'vosotros' is not used in Latin American Spanish (use 'ustedes' instead). The formal/informal register for UI strings in Latin American SaaS products is typically 'usted' (formal) or informal depending on the product tone. This product uses an informal register throughout."
  },
  "function": "Produce the action items for the localization translation task.",
  "acceptedOutput": {
    "actionItems": [
      "Action 1: Translate 'queued' as 'en cola' (in queue) — 'Tu solicitud está en cola'. Do not use 'encolada' (less natural in Latin American colloquial Spanish for this context).",
      "Action 2: Translate 'will be processed' as 'será procesada' — future tense, passive voice matches the English structure. 'procesada' agrees with 'solicitud' (feminine).",
      "Action 3: Informal register (consistent with product tone): use 'Tu' (informal you, subject implied) instead of 'Su' (formal). Full string: 'Tu solicitud está en cola y será procesada en los próximos 5 minutos.'",
      "Action 4: 'within 5 minutes' → 'en los próximos 5 minutos' (in the next 5 minutes) — more natural than 'dentro de 5 minutos' for UI context in Latin American Spanish.",
      "Action 5: Verify character encoding: the string contains no special characters (ó in 'próximos' — ensure UTF-8 encoding in the localization file)."
    ]
  },
  "rejectedOutput": {
    "actionItems": [
      "Translate directly: 'Tu petición ha sido encolada y será procesada dentro de 5 minutos.'",
      "This is a literal translation from English."
    ]
  },
  "arbiters": ["localizationArbiter"],
  "whyAccepted": "The accepted decomposition addresses register (informal), regional preference (Latin American vs Castilian), noun agreement ('procesada' feminine), and idiomatic time expression ('en los próximos 5 minutos'). Each decision is documented with reasoning.",
  "whyRejected": "'Petición' (petition/formal request) is more formal than 'solicitud' and is less natural for a UI task queue context in Latin American Spanish. 'Encolada' as a past participle is grammatically correct but reads less naturally than 'está en cola' for a status message. 'Dentro de 5 minutos' is correct but 'en los próximos 5 minutos' is more idiomatic for UI status messages.",
  "sourceAnchors": [
    {
      "sourceId": "TATOEBA",
      "url": "https://tatoeba.org",
      "section": "EN-ES translation — task queue UI string; informal register; Latin American Spanish idiom",
      "license": "CC-BY-2.0"
    }
  ],
  "licenseConstraint": "CC-BY-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-228

```json
{
  "rowId": "EXT-228",
  "sourceIds": ["PYTHON-PEPS"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G_LEARNING_TRAINING",
  "subdomain": "pep526-variable-annotation-migration",
  "rowClass": "migration-decision",
  "input": {
    "a": "A Python 3.5 codebase uses inline comments to document variable types: x = []  # type: list[int]. The team is on Python 3.6+ and a reviewer suggested migrating to PEP 526 variable annotations. Should we migrate?",
    "b": "PEP 526 (Python 3.6+) introduces variable annotation syntax: x: list[int] = []. Type comments (# type: list[int]) are PEP 484 syntax for Python 2/3 compatibility. The codebase is Python 3.6 only — there is no Python 2 requirement. mypy supports both syntaxes. 847 type comments exist in the codebase.",
    "c": "Migration scope: pyupgrade --py36-plus can automatically migrate type comments to PEP 526 annotations across the codebase in one pass. The tool handles the common cases: variable assignments, function arguments (not applicable — PEP 526 is for variables; function argument types use PEP 3107 annotations). Risk: pyupgrade handles 95%+ of cases; edge cases require manual review."
  },
  "function": "Decide: migrate to PEP 526 variable annotations or preserve type comments.",
  "acceptedOutput": {
    "decision": "MIGRATE using pyupgrade.",
    "rationale": "Three reasons favor migration: (1) Python 2 compatibility is not a requirement — type comments exist only for Py2/3 compatibility, which is irrelevant on Python 3.6+. (2) PEP 526 annotations are evaluated by the runtime and accessible via __annotations__ — type comments are purely in the AST (Abstract Syntax Tree) comments and not accessible at runtime. Tools like dataclasses, attrs, and Pydantic use __annotations__. (3) pyupgrade automates the migration — 847 manual changes become one command.",
    "command": "pyupgrade --py36-plus **/*.py",
    "gate": "Zero failing type annotations reported by mypy after migration. All 847 type comments converted (verify with grep -r '# type:' to confirm none remain). Tests green."
  },
  "rejectedOutput": {
    "decision": "DO NOT MIGRATE. Type comments work correctly with mypy. 847 changes is a significant migration risk. The existing syntax is understood by all team members."
  },
  "arbiters": ["languageModernizationArbiter"],
  "whyAccepted": "The 'significant migration risk' of 847 changes is eliminated by using pyupgrade — the tool handles the migration automatically with known reliability. The runtime accessibility of PEP 526 annotations (via __annotations__) is a concrete benefit over type comments that becomes relevant when using dataclasses or Pydantic.",
  "whyRejected": "Type comments are a Python 2 compatibility mechanism. Preserving them in a Python 3.6-only codebase indefinitely is technical debt without benefit. The 847 manual change count is addressed by automation, not by avoiding migration.",
  "sourceAnchors": [
    {
      "sourceId": "PYTHON-PEPS",
      "url": "https://peps.python.org/pep-0526/",
      "section": "PEP 526 — Variable Annotations; migration from type comments; pyupgrade automation",
      "license": "CC-BY-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-229

```json
{
  "rowId": "EXT-229",
  "sourceIds": ["CWE-TOP25"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "buffer-overflow-bounds-checking",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Review this C# implementation of a fixed-size buffer fill operation.",
    "b": "Implementation A: public void FillBuffer(byte[] source, byte[] destination) { for (int i = 0; i < source.Length; i++) { destination[i] = source[i]; } } Implementation B: public OperationResult<int> FillBuffer(byte[] source, byte[] destination) { if (source.Length > destination.Length) { return OperationResult<int>.Fail($'Source ({source.Length} bytes) exceeds destination capacity ({destination.Length} bytes)', 'BUFFER_OVERFLOW'); } Buffer.BlockCopy(source, 0, destination, 0, source.Length); return OperationResult<int>.Ok(source.Length); }",
    "c": "Context: CWE-119 (Improper Restriction of Operations within the Bounds of a Memory Buffer). In C#, accessing an array beyond its length throws IndexOutOfRangeException. For source.Length > destination.Length, Implementation A throws at runtime when i reaches destination.Length. Implementation B checks bounds before the copy and returns a structured error. Buffer.BlockCopy is an optimized bulk memory copy, more efficient than element-by-element loop for byte arrays."
  },
  "function": "Arbitrate: which implementation is correct and why?",
  "acceptedOutput": {
    "verdict": "Implementation B is correct.",
    "reasoning": "Implementation A copies bytes element-by-element without checking whether the source fits in the destination. When source.Length > destination.Length, it throws IndexOutOfRangeException at the i == destination.Length iteration — a CWE-119 (Improper Restriction of Operations within the Bounds of a Memory Buffer) pattern. The exception propagates as an uncaught runtime error. Implementation B validates the bounds before the copy and returns a structured OperationResult failure. The caller can handle BUFFER_OVERFLOW without catching exceptions. Buffer.BlockCopy performs an optimized memory block copy in a single native call.",
    "performanceNote": "Buffer.BlockCopy outperforms element-by-element loop for arrays > ~100 bytes due to SIMD (Single Instruction, Multiple Data) vectorization in the runtime."
  },
  "rejectedOutput": {
    "verdict": "Both are acceptable. Implementation A will throw a clear exception if the buffer overflows. The caller should catch IndexOutOfRangeException. In C#, the runtime prevents actual memory corruption, so CWE-119 does not apply."
  },
  "arbiters": ["bufferSafetyArbiter", "cweComplianceArbiter"],
  "whyAccepted": "CWE-119 applies to C# even though the runtime prevents memory corruption — the classification covers improper bounds checking that results in unexpected program termination or data loss, not only memory corruption. The rejected verdict's 'the caller should catch' advice violates the OperationResult<T> pattern: business logic failures should be represented as structured results, not exceptions.",
  "whyRejected": "The 'runtime prevents memory corruption' argument conflates memory safety (C# is memory-safe) with bounds validation (the operation should validate bounds before executing). A thrown IndexOutOfRangeException in a buffer fill operation represents a protocol error that the caller may not be prepared to catch. Structured OperationResult error propagation is the correct pattern.",
  "sourceAnchors": [
    {
      "sourceId": "CWE-TOP25",
      "url": "https://cwe.mitre.org/data/definitions/119.html",
      "section": "CWE-119: Improper Restriction of Operations within Memory Buffer — bounds check before copy",
      "license": "MITRE notice required"
    }
  ],
  "licenseConstraint": "MITRE notice required",
  "trainingStatus": "candidate"
}
```

---

## EXT-230

```json
{
  "rowId": "EXT-230",
  "sourceIds": ["IETF-RFCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "finalMachineResults_to_userFormat",
  "domain": "C_SEMANTIC_TRANSLATION",
  "subdomain": "rfc9457-problem-details",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Format this API error result as an HTTP response body for a payment amount validation failure: { errorCode: 'AMOUNT_BELOW_MINIMUM', minimumAmount: 0.50, submittedAmount: 0.10, currency: 'USD' }",
    "b": "Two format candidates: (A) Plain JSON: { 'error': 'Amount below minimum', 'code': 'AMOUNT_BELOW_MINIMUM' }. (B) RFC 9457 Problem Details: Content-Type: application/problem+json with fields: type, title, status, detail, and extension members.",
    "c": "RFC 9457 (Problem Details for HTTP APIs) supersedes RFC 7807. It defines a standard JSON format for API error responses: { type: URI identifying the problem type, title: short human-readable summary, status: HTTP status code, detail: human-readable explanation, and extension members for problem-specific data }. Content-Type must be application/problem+json. Clients that understand RFC 9457 can parse errors generically."
  },
  "function": "Produce the formatted HTTP error response body for the validation failure.",
  "acceptedOutput": {
    "httpStatus": 422,
    "contentType": "application/problem+json",
    "body": {
      "type": "https://api.payments.example.com/problems/amount-below-minimum",
      "title": "Payment Amount Below Minimum",
      "status": 422,
      "detail": "The submitted amount of $0.10 USD is below the minimum payment amount of $0.50 USD.",
      "minimumAmount": 0.50,
      "submittedAmount": 0.10,
      "currency": "USD"
    },
    "rationale": "RFC 9457 format: 'type' is a URI that uniquely identifies this problem class (clients can dereference it for documentation). 'title' is a short, stable label. 'detail' is the human-readable explanation including the specific values. 'minimumAmount', 'submittedAmount', 'currency' are RFC 9457 extension members — machine-readable data the client can use programmatically (display the minimum, offer to adjust the amount). HTTP 422 Unprocessable Entity: the request was well-formed but semantically invalid."
  },
  "rejectedOutput": {
    "httpStatus": 400,
    "contentType": "application/json",
    "body": {
      "error": "Amount below minimum",
      "code": "AMOUNT_BELOW_MINIMUM"
    }
  },
  "arbiters": ["apiErrorFormatArbiter", "rfcComplianceArbiter"],
  "whyAccepted": "RFC 9457 Problem Details provides a machine-parseable error format. The extension members (minimumAmount, submittedAmount) allow the client to display 'Minimum payment is $0.50' without string parsing. The 'type' URI enables clients to handle specific error types programmatically. 422 is more semantically correct than 400 for a valid-format-but-invalid-value error.",
  "whyRejected": "The plain JSON format with 'error' and 'code' is not standardized — each API invents its own error schema. Clients must handle error formats per-API. The minimum amount is not included, so the client cannot display it without another API call or hardcoding. 400 is less specific than 422 for semantic validation errors.",
  "sourceAnchors": [
    {
      "sourceId": "IETF-RFCS",
      "url": "https://www.rfc-editor.org/rfc/rfc9457",
      "section": "RFC 9457: Problem Details for HTTP APIs — type, title, status, detail, extension members; application/problem+json",
      "license": "IETF Trust provisions"
    }
  ],
  "licenseConstraint": "IETF Trust provisions",
  "trainingStatus": "candidate"
}
```
