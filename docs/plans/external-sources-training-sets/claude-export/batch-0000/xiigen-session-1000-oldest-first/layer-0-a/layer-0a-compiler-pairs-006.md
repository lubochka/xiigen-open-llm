# Layer 0-A - Compiler Pairs (006)
# Batch: XIIGen session export 1000 oldest-first | pair_kind: compiler
# Source: Claude export batch-0000; first-party private

---

## XIIGEN-SESSION-1000-0251 - Azure OCR definition and setup

```
source_id: xiigen-session-1000-0251
source_title: "Azure OCR definition and setup"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
domain: CODE_Q
phase: 3
sub_phase: 3.3
task_type: api-contract
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "3d75a0ce-e23d-42fe-ba05-af2ba4d920bd"
  conversation_name: "Azure OCR definition and setup"
  human_message_uuid: "019a7d08-45c1-719f-b46d-6918a46a542f"
  human_message_index: 2
  message_created_at: "2025-11-13T11:44:45.512619Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0251"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "How to define the first one? Read API?"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0251-prompt"
  SourcePath: "claude-export/batch-0000/3d75a0ce-e23d-42fe-ba05-af2ba4d920bd/messages/019a7d08-45c1-719f-b46d-6918a46a542f"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: "How to define the first one? Read API?"
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:api

---

## XIIGEN-SESSION-1000-0252 - Validating task report JSON data

```
source_id: xiigen-session-1000-0252
source_title: "Validating task report JSON data"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: repair
domain: BUG
phase: 3
sub_phase: 3.3
task_type: api-contract
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "1d5fb27e-6c1e-483b-8632-7eae41dc4767"
  conversation_name: "Validating task report JSON data"
  human_message_uuid: "019a54ab-20b9-70c7-93b3-b57ea16c0408"
  human_message_index: 4
  message_created_at: "2025-11-05T15:38:12.564801Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0252"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    For changes like this: 2025-11-05 12:20
    by Yaniv Refaelov
    FoundIn: → 1.4.0AreaPath: → Keshet\ContractsId: → 9612AreaId: → 82ReadyforQA: False → TrueStateChangeDate: 2025-11-05T12:20:33.05Z → 2025-11-05T13:29:10.14ZPriority: → 3ActivatedBy: → Jacob YchyeTags: → הסכמיםReproSteps: → 1. Failed בדוק את מספר ההסכמים, מספר בעלי זכות, מספר חברות ייצוג Expected Result Expectation for step: exists בפועל: הסכמים - מסך ראשי - חברות ייצוג - הסכום של כלל חברות הייצוג כולל גם את חברת הייצוג ...CommentCount: → 0IsDeleted: → FalseTeamProject: → KeshetResolvedBy: → Jacob YchyeIterationLevel3: → 1.5.0AssignedTo: → Jacob YchyeAreaLevel1: → KeshetWorkItemType: → BugIterationId: → 298Parent: → 3392IterationLevel1: → KeshetTitle: → הסכמים - מסך ראשי - חברות ייצוג - הסהכ כולל חברת ייצוג עצמיAreaLevel2: → ContractsPersonId: 215713895 → 215542125ValueArea: → BusinessCreatedBy: → Yaniv RefaelovIterationPath: → Keshet\Contracts\1.5.0Reason: Approved → FixedSeverity: → 3 - MediumNodeName: → ContractsResolvedReason: → FixedActivatedDate: → 2025-11-05T12:20:33.05ZCreatedDate: → 2025-11-05T11:34:59.677ZState: Active → ResolvedIterationLevel2: → ContractsResolvedDate: → 2025-11-05T13:29:10.14Z
    
    Can you please make a table view ?
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0252-prompt"
  SourcePath: "claude-export/batch-0000/1d5fb27e-6c1e-483b-8632-7eae41dc4767/messages/019a54ab-20b9-70c7-93b3-b57ea16c0408"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    For changes like this: 2025-11-05 12:20
    by Yaniv Refaelov
    FoundIn: → 1.4.0AreaPath: → Keshet\ContractsId: → 9612AreaId: → 82ReadyforQA: False → TrueStateChangeDate: 2025-11-05T12:20:33.05Z → 2025-11-05T13:29:10.14ZPriority: → 3ActivatedBy: → Jacob YchyeTags: → הסכמיםReproSteps: → 1. Failed בדוק את מספר ההסכמים, מספר בעלי זכות, מספר חברות ייצוג Expected Result Expectation for step: exists בפועל: הסכמים - מסך ראשי - חברות ייצוג - הסכום של כלל חברות הייצוג כולל גם את חברת הייצוג ...CommentCount: → 0IsDeleted: → FalseTeamProject: → KeshetResolvedBy: → Jacob YchyeIterationLevel3: → 1.5.0AssignedTo: → Jacob YchyeAreaLevel1: → KeshetWorkItemType: → BugIterationId: → 298Parent: → 3392IterationLevel1: → KeshetTitle: → הסכמים - מסך ראשי - חברות ייצוג - הסהכ כולל חברת ייצוג עצמיAreaLevel2: → ContractsPersonId: 215713895 → 215542125ValueArea: → BusinessCreatedBy: → Yaniv RefaelovIterationPath: → Keshet\Contracts\1.5.0Reason: Approved → FixedSeverity: → 3 - MediumNodeName: → ContractsResolvedReason: → FixedActivatedDate: → 2025-11-05T12:20:33.05ZCreatedDate: → 2025-11-05T11:34:59.677ZState: Active → ResolvedIterationLevel2: → ContractsResolvedDate: → 2025-11-05T13:29:10.14Z
    
    Can you please make a table view ?
  CompiledPredicateIds: ["API_CONTRACT", "ARCHITECTURE", "IMPLEMENT", "QA", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:contract
  - ARCHITECTURE:project
  - IMPLEMENT:fix
  - IMPLEMENT:bug
  - IMPLEMENT:failed
  - QA:qa

---

## XIIGEN-SESSION-1000-0253 - Validating task report JSON data

```
source_id: xiigen-session-1000-0253
source_title: "Validating task report JSON data"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: qa_release
domain: CODE_Q
phase: 6
sub_phase: 6.1
task_type: manual-qa
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "1d5fb27e-6c1e-483b-8632-7eae41dc4767"
  conversation_name: "Validating task report JSON data"
  human_message_uuid: "019a54b7-0836-75b2-bb16-44fda7e56ca6"
  human_message_index: 8
  message_created_at: "2025-11-05T15:51:12.716985Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0253"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    1. Ok, just remove the old values if they are empty  - just the current value - leave just the tasks and changes columns: State, ReadyforQA, ResolvedBy, Description, History 
    
    1. Map from PersonId to the Person name
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0253-prompt"
  SourcePath: "claude-export/batch-0000/1d5fb27e-6c1e-483b-8632-7eae41dc4767/messages/019a54b7-0836-75b2-bb16-44fda7e56ca6"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Manual QA and Acceptance Review"
  CompiledText: |
    1. Ok, just remove the old values if they are empty  - just the current value - leave just the tasks and changes columns: State, ReadyforQA, ResolvedBy, Description, History 
    
    1. Map from PersonId to the Person name
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - QA:qa

---

## XIIGEN-SESSION-1000-0254 - Validating task report JSON data

```
source_id: xiigen-session-1000-0254
source_title: "Validating task report JSON data"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
domain: CODE_Q
phase: 4
sub_phase: 4.1
task_type: backend-services
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "1d5fb27e-6c1e-483b-8632-7eae41dc4767"
  conversation_name: "Validating task report JSON data"
  human_message_uuid: "019a54c1-39d3-7184-ab06-95908dfeb2f5"
  human_message_index: 12
  message_created_at: "2025-11-05T16:02:20.838107Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0254"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    1. If Item was resolved by  - need to be undet Luba Karpenko like 9441
    2. No need to display cards like that: 📋 Work Item __#9457__
    📅 2025-11-05 12:00👤 Amit Neman (Technology)
    No key field changes
    🔗 View in Azure DevOps
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0254-prompt"
  SourcePath: "claude-export/batch-0000/1d5fb27e-6c1e-483b-8632-7eae41dc4767/messages/019a54c1-39d3-7184-ab06-95908dfeb2f5"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Backend Services"
  CompiledText: |
    1. If Item was resolved by  - need to be undet Luba Karpenko like 9441
    2. No need to display cards like that: 📋 Work Item __#9457__
    📅 2025-11-05 12:00👤 Amit Neman (Technology)
    No key field changes
    🔗 View in Azure DevOps
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - IMPLEMENT:log

---

## XIIGEN-SESSION-1000-0255 - Validating task report JSON data

```
source_id: xiigen-session-1000-0255
source_title: "Validating task report JSON data"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: qa_release
domain: CODE_Q
phase: 6
sub_phase: 6.1
task_type: manual-qa
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "1d5fb27e-6c1e-483b-8632-7eae41dc4767"
  conversation_name: "Validating task report JSON data"
  human_message_uuid: "019a54c4-de35-77d9-99a8-bcc6572e4130"
  human_message_index: 14
  message_created_at: "2025-11-05T16:06:19.498075Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0255"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    1. amit.neeman and Amit Neman (Technology) is the same employee all the data should be under one
    2. ReadyforQA
    True → False
    ResolvedBy
    Amit Neman (Technology) State
    Active → Resolved  - can be in the same row
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0255-prompt"
  SourcePath: "claude-export/batch-0000/1d5fb27e-6c1e-483b-8632-7eae41dc4767/messages/019a54c4-de35-77d9-99a8-bcc6572e4130"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Manual QA and Acceptance Review"
  CompiledText: |
    1. amit.neeman and Amit Neman (Technology) is the same employee all the data should be under one
    2. ReadyforQA
    True → False
    ResolvedBy
    Amit Neman (Technology) State
    Active → Resolved  - can be in the same row
  CompiledPredicateIds: ["IMPLEMENT", "QA", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - IMPLEMENT:log
  - QA:qa

---

## XIIGEN-SESSION-1000-0256 - Full-stack development requirements in Hebrew

```
source_id: xiigen-session-1000-0256
source_title: "Full-stack development requirements in Hebrew"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: qa_release
domain: CODE_Q
phase: 3
sub_phase: 3.3
task_type: api-contract
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "de4fbd15-26e6-4447-9aed-38a741033d26"
  conversation_name: "Full-stack development requirements in Hebrew"
  human_message_uuid: "019a9032-4784-76c6-a393-03fe20b85cdb"
  human_message_index: 0
  message_created_at: "2025-11-17T05:03:25.585047Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0256"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    תרגם בבקשה את קורות החיים לעברית בהדגשה של הדרישות הללו זו הדרישה לצוות כולו:  1.	צד לקוח יפותח בגרסה האחרונה של Angular  ושימוש בספריות  CLIENT  נוספות. כולל ידע ב JS מתקדם, HTML ו CSS. פיתוח ה Client יכלול תמיכה בנגישות והתאמה למובייל.
    2.	פיתוחי צד שרת,  יכללו כתיבת קוד ב C#, כולל כתיבת  web api, בפלטפורמת .Net Core  
    כתיבת הקוד יכולה לכלול כתיבת טסטים. 
    3.	הפיתוח מצריך היכרות ושימוש בשכבת תשתית מבוססת על הטכנולוגיות הנ"ל.
    4.	הפיתוח כולל תהליכי CI/CD ובכלל זה שימוש בספריות bundler כדוגמת , webpack ובתהליכי build & release ב- Gitlab/TFS.
    
    בדומה למה שעשו בגרסה הפחות מעודכנת
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0256-prompt"
  SourcePath: "claude-export/batch-0000/de4fbd15-26e6-4447-9aed-38a741033d26/messages/019a9032-4784-76c6-a393-03fe20b85cdb"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    תרגם בבקשה את קורות החיים לעברית בהדגשה של הדרישות הללו זו הדרישה לצוות כולו:  1.	צד לקוח יפותח בגרסה האחרונה של Angular  ושימוש בספריות  CLIENT  נוספות. כולל ידע ב JS מתקדם, HTML ו CSS. פיתוח ה Client יכלול תמיכה בנגישות והתאמה למובייל.
    2.	פיתוחי צד שרת,  יכללו כתיבת קוד ב C#, כולל כתיבת  web api, בפלטפורמת .Net Core  
    כתיבת הקוד יכולה לכלול כתיבת טסטים. 
    3.	הפיתוח מצריך היכרות ושימוש בשכבת תשתית מבוססת על הטכנולוגיות הנ"ל.
    4.	הפיתוח כולל תהליכי CI/CD ובכלל זה שימוש בספריות bundler כדוגמת , webpack ובתהליכי build & release ב- Gitlab/TFS.
    
    בדומה למה שעשו בגרסה הפחות מעודכנת
  CompiledPredicateIds: ["API_CONTRACT", "QA", "UI"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:api
  - QA:release
  - UI:ui

---

## XIIGEN-SESSION-1000-0257 - Kling video generation from Google Drive images

```
source_id: xiigen-session-1000-0257
source_title: "Kling video generation from Google Drive images"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
domain: CODE_Q
phase: 3
sub_phase: 3.3
task_type: api-contract
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "c937fcd5-ea71-4b36-9c99-5631af7e9e69"
  conversation_name: "Kling video generation from Google Drive images"
  human_message_uuid: "019a6ce7-e4ac-72d4-a5a7-2b3b75e6c246"
  human_message_index: 0
  message_created_at: "2025-11-10T08:35:28.206769Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0257"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "I would like to use piapi to communicate with the kling for video generation based on image through n8n http request node . My images are stored on google drive."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0257-prompt"
  SourcePath: "claude-export/batch-0000/c937fcd5-ea71-4b36-9c99-5631af7e9e69/messages/019a6ce7-e4ac-72d4-a5a7-2b3b75e6c246"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: "I would like to use piapi to communicate with the kling for video generation based on image through n8n http request node . My images are stored on google drive."
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:api
  - API_CONTRACT:request

---

## XIIGEN-SESSION-1000-0258 - Validating task report JSON data

```
source_id: xiigen-session-1000-0258
source_title: "Validating task report JSON data"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: repair
domain: BUG
phase: 3
sub_phase: 3.3
task_type: api-contract
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "6e8b95c5-7a7b-4bc7-81ba-9dbe696116e3"
  conversation_name: "Validating task report JSON data"
  human_message_uuid: "019a78b9-1ba5-71a9-97e3-7ae3d7a599c8"
  human_message_index: 0
  message_created_at: "2025-11-12T15:39:48.734827Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0258"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    Please follow SuccessfulClaudePrompt.txt instructions - validate at least several times the data is complete and covers the json data in tasksMainReport.json
    the expected results looks like successfulHtmlFormat or if there are no tasks then like successfulHtmlFormat2
    After you finish - please take over other chats typical fix requests
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0258-prompt"
  SourcePath: "claude-export/batch-0000/6e8b95c5-7a7b-4bc7-81ba-9dbe696116e3/messages/019a78b9-1ba5-71a9-97e3-7ae3d7a599c8"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    Please follow SuccessfulClaudePrompt.txt instructions - validate at least several times the data is complete and covers the json data in tasksMainReport.json
    the expected results looks like successfulHtmlFormat or if there are no tasks then like successfulHtmlFormat2
    After you finish - please take over other chats typical fix requests
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:request
  - IMPLEMENT:fix

---

## XIIGEN-SESSION-1000-0259 - Validating task report JSON data

```
source_id: xiigen-session-1000-0259
source_title: "Validating task report JSON data"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: repair
domain: BUG
phase: 3
sub_phase: 3.5
task_type: observability-design
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "6e8b95c5-7a7b-4bc7-81ba-9dbe696116e3"
  conversation_name: "Validating task report JSON data"
  human_message_uuid: "019a78d0-deae-72db-9cb9-e124d17946f1"
  human_message_index: 4
  message_created_at: "2025-11-12T16:05:45.795718Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0259"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    1. There is a broken html :
    
    <a href="#" data-vss-mention="version:2.0,22a4180a-5c3c-6738-9b76-2808ecf3bde9">@Arik Almog</a> שים בבקשה לב שהדרישה דורשת פיתוח מיוחד של פקד - כי עד כה גלשתי על פקד קיים ואיחדתי את התצורה של הנראות למה שיש לנו. לדעתי לא מעט זמן - יום עבודה
    
    <br/> <table width=""><tbody><tr><td style="vertical-align:top;padding:2px 7px;font-weight:bold;">2. </td><td style="vertical-align:top;padding:2px 7px;font-weight:bold;color:red;">Failed </td><td style="vertical-align:top;padding:2px 7px;width:701px...
    
    and more...
    1. Better to display changes in one row -
    2025-11-10 13:08StateNewActive2025-11-10 13:08Assigned ToLuba Karpenko2025-11-10 13:08Titleשינוי תצוגה במשתתפים2025-11-10 13:08Iteration PathKeshet\Programs\1.0.5Keshet\Programs\1.15.02025-11-10 13:08Area PathKeshet\Programs2025-11-10 13:08Parent48562025-11-10 13:08Ready for QAFalse2025-11-10 13:08Priority2
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0259-prompt"
  SourcePath: "claude-export/batch-0000/6e8b95c5-7a7b-4bc7-81ba-9dbe696116e3/messages/019a78d0-deae-72db-9cb9-e124d17946f1"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Observability and Operational Design"
  CompiledText: |
    1. There is a broken html :
    
    <a href="#" data-vss-mention="version:2.0,22a4180a-5c3c-6738-9b76-2808ecf3bde9">@Arik Almog</a> שים בבקשה לב שהדרישה דורשת פיתוח מיוחד של פקד - כי עד כה גלשתי על פקד קיים ואיחדתי את התצורה של הנראות למה שיש לנו. לדעתי לא מעט זמן - יום עבודה
    
    <br/> <table width=""><tbody><tr><td style="vertical-align:top;padding:2px 7px;font-weight:bold;">2. </td><td style="vertical-align:top;padding:2px 7px;font-weight:bold;color:red;">Failed </td><td style="vertical-align:top;padding:2px 7px;width:701px...
    
    and more...
    1. Better to display changes in one row -
    2025-11-10 13:08StateNewActive2025-11-10 13:08Assigned ToLuba Karpenko2025-11-10 13:08Titleשינוי תצוגה במשתתפים2025-11-10 13:08Iteration PathKeshet\Programs\1.0.5Keshet\Programs\1.15.02025-11-10 13:08Area PathKeshet\Programs2025-11-10 13:08Parent48562025-11-10 13:08Ready for QAFalse2025-11-10 13:08Priority2
  CompiledPredicateIds: ["IMPLEMENT", "QA", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - IMPLEMENT:failed
  - QA:qa

---

## XIIGEN-SESSION-1000-0260 - Facebook data pipeline to Azure AI Search

```
source_id: xiigen-session-1000-0260
source_title: "Facebook data pipeline to Azure AI Search"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
domain: CODE_Q
phase: 3
sub_phase: 3.3
task_type: api-contract
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "44d5a16e-627e-4c03-abc1-5c5486ef97f6"
  conversation_name: "Facebook data pipeline to Azure AI Search"
  human_message_uuid: "019a9caf-8273-7058-9003-811e1f9fd336"
  human_message_index: 0
  message_created_at: "2025-11-19T15:15:39.259677Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0260"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "I need a uml that takes social data from facebook api by a defined time: start and end - brings all the posts and the comments, and loads each post to Azure AI Search"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0260-prompt"
  SourcePath: "claude-export/batch-0000/44d5a16e-627e-4c03-abc1-5c5486ef97f6/messages/019a9caf-8273-7058-9003-811e1f9fd336"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: "I need a uml that takes social data from facebook api by a defined time: start and end - brings all the posts and the comments, and loads each post to Azure AI Search"
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:api

---

## XIIGEN-SESSION-1000-0261 - Validating task report JSON data

```
source_id: xiigen-session-1000-0261
source_title: "Validating task report JSON data"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: repair
domain: BUG
phase: 1
sub_phase: 1.3
task_type: interaction-navigation
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "2854e207-a2dd-4af6-993d-6eda34ecd665"
  conversation_name: "Validating task report JSON data"
  human_message_uuid: "019a696d-8b19-75d7-ba56-f20548457b76"
  human_message_index: 4
  message_created_at: "2025-11-09T16:22:58.085045Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0261"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    Additional Comments - Broken Html and missing work Item Id
    
    Comment IDWork ItemDateCommentAuthorURL#2025-11-09 09:56
    <a href="#" data-vss-mention="version:2.0,22a4180a-5c3c-6738-9b76-2808ecf3bde9">@Arik Almog</a> first draft of uml at attachments
    Luba KarpenkoView#2025-11-09 12:40
    <a href="#" data-vss-mention="version:2.0,22a4180a-5c3c-6738-9b76-2808ecf3bde9">@Arik Almog</a> please decide how urgent it is, I believe that dealing with default tooltip design will require serious time including all the fixes that will be needed later
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0261-prompt"
  SourcePath: "claude-export/batch-0000/2854e207-a2dd-4af6-993d-6eda34ecd665/messages/019a696d-8b19-75d7-ba56-f20548457b76"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Interaction and Navigation Model"
  CompiledText: |
    Additional Comments - Broken Html and missing work Item Id
    
    Comment IDWork ItemDateCommentAuthorURL#2025-11-09 09:56
    <a href="#" data-vss-mention="version:2.0,22a4180a-5c3c-6738-9b76-2808ecf3bde9">@Arik Almog</a> first draft of uml at attachments
    Luba KarpenkoView#2025-11-09 12:40
    <a href="#" data-vss-mention="version:2.0,22a4180a-5c3c-6738-9b76-2808ecf3bde9">@Arik Almog</a> please decide how urgent it is, I believe that dealing with default tooltip design will require serious time including all the fixes that will be needed later
  CompiledPredicateIds: ["IMPLEMENT", "SECURITY", "TEST", "UI"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - IMPLEMENT:fix
  - SECURITY:auth
  - UI:ui

---

## XIIGEN-SESSION-1000-0262 - Validating task report JSON data

```
source_id: xiigen-session-1000-0262
source_title: "Validating task report JSON data"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: repair
domain: BUG
phase: 3
sub_phase: 3.3
task_type: api-contract
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "f3ee8c34-b7ee-4fc9-81ef-a9387f77c5eb"
  conversation_name: "Validating task report JSON data"
  human_message_uuid: "019a70c2-3f5c-71a6-abdd-c0136135cec8"
  human_message_index: 4
  message_created_at: "2025-11-11T02:32:49.789707Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0262"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    1: Still Same: 
    a lot of same changes with different time stamps:
    2025-11-10T11:56:35.277Z
    by Luba Karpenko
    Status: New → Active Sprint: Keshet\Programs\1.0.5 → Keshet\Programs\1.15.0 Title: → make select participants one line Reason: New → Work started Added: Parent
    2025-11-10T11:58:32.023Z
    by Luba Karpenko
    Status: New → Active Sprint: Keshet\Programs\1.0.5 → Keshet\Programs\1.15.0 Title: → make select participants one line Reason: New → Work started Added: Pull Request
    2025-11-10 11:58 to 2025-11-10 12:00 (2 times)
    by Luba Karpenko
    Status: New → Active Sprint: Keshet\Programs\1.0.5 → Keshet\Programs\1.15.0 Title: → make select participants one line Reason: New → Work started Added: Fixed in Commit
    2025-11-10T13:01:10.997Z
    by Luba Karpenko
    Status: New → Active Sprint: Keshet\Programs\1.0.5 → Keshet\Programs\1.15.0 Title: → make select participants one line Reason: New → Work started Added: Integrated in release environment
    2025-11-10 12:04 to 2025-11-10 13:08 (3 times)
    by Luba Karpenko
    Status: New → Active Sprint: Keshet\Programs\1.0.5 → Keshet\Programs\1.15.0 Title: → make select participants one line Reason: New → Work started
    
    1. On Additional Commits:
    Additional Commits
    Commit IDDateCommentCommitterURL9657 -- This is not a commit it is Work Item Id - supposes to be in a separate row... Commit Id -> Need to be added 2025-11-10 13:00Merged PR 758: #9661 small fixLuba KarpenkoView ... Same for all items in additional commits
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0262-prompt"
  SourcePath: "claude-export/batch-0000/f3ee8c34-b7ee-4fc9-81ef-a9387f77c5eb/messages/019a70c2-3f5c-71a6-abdd-c0136135cec8"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    1: Still Same: 
    a lot of same changes with different time stamps:
    2025-11-10T11:56:35.277Z
    by Luba Karpenko
    Status: New → Active Sprint: Keshet\Programs\1.0.5 → Keshet\Programs\1.15.0 Title: → make select participants one line Reason: New → Work started Added: Parent
    2025-11-10T11:58:32.023Z
    by Luba Karpenko
    Status: New → Active Sprint: Keshet\Programs\1.0.5 → Keshet\Programs\1.15.0 Title: → make select participants one line Reason: New → Work started Added: Pull Request
    2025-11-10 11:58 to 2025-11-10 12:00 (2 times)
    by Luba Karpenko
    Status: New → Active Sprint: Keshet\Programs\1.0.5 → Keshet\Programs\1.15.0 Title: → make select participants one line Reason: New → Work started Added: Fixed in Commit
    2025-11-10T13:01:10.997Z
    by Luba Karpenko
    Status: New → Active Sprint: Keshet\Programs\1.0.5 → Keshet\Programs\1.15.0 Title: → make select participants one line Reason: New → Work started Added: Integrated in release environment
    2025-11-10 12:04 to 2025-11-10 13:08 (3 times)
    by Luba Karpenko
    Status: New → Active Sprint: Keshet\Programs\1.0.5 → Keshet\Programs\1.15.0 Title: → make select participants one line Reason: New → Work started
    
    1. On Additional Commits:
    Additional Commits
    Commit IDDateCommentCommitterURL9657 -- This is not a commit it is Work Item Id - supposes to be in a separate row... Commit Id -> Need to be added 2025-11-10 13:00Merged PR 758: #9661 small fixLuba KarpenkoView ... Same for all items in additional commits
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "QA", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:request
  - IMPLEMENT:fix
  - QA:release

---

## XIIGEN-SESSION-1000-0263 - AWS messaging alternatives to SQS for Nest.js

```
source_id: xiigen-session-1000-0263
source_title: "AWS messaging alternatives to SQS for Nest.js"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
domain: CODE_Q
phase: 4
sub_phase: 4.1
task_type: backend-services
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "4a773a49-aca9-4a46-a28d-4b9b73790d16"
  conversation_name: "AWS messaging alternatives to SQS for Nest.js"
  human_message_uuid: "019abb74-6252-7641-a9d4-1866c7399966"
  human_message_index: 0
  message_created_at: "2025-11-25T14:39:18.228199Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0263"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "which is the best alternative to sqs in aws in terms of simplicity of use and code support in nest.js?"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0263-prompt"
  SourcePath: "claude-export/batch-0000/4a773a49-aca9-4a46-a28d-4b9b73790d16/messages/019abb74-6252-7641-a9d4-1866c7399966"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Backend Services"
  CompiledText: "which is the best alternative to sqs in aws in terms of simplicity of use and code support in nest.js?"
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - IMPLEMENT:code

---

## XIIGEN-SESSION-1000-0264 - AWS messaging alternatives to SQS for Nest.js

```
source_id: xiigen-session-1000-0264
source_title: "AWS messaging alternatives to SQS for Nest.js"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
domain: CODE_Q
phase: 4
sub_phase: 4.1
task_type: backend-services
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "4a773a49-aca9-4a46-a28d-4b9b73790d16"
  conversation_name: "AWS messaging alternatives to SQS for Nest.js"
  human_message_uuid: "019abb79-4678-767c-bab3-a8101859b82f"
  human_message_index: 6
  message_created_at: "2025-11-25T14:44:38.791739Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0264"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Now prepare very very short summary with code examples"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0264-prompt"
  SourcePath: "claude-export/batch-0000/4a773a49-aca9-4a46-a28d-4b9b73790d16/messages/019abb79-4678-767c-bab3-a8101859b82f"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Backend Services"
  CompiledText: "Now prepare very very short summary with code examples"
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - IMPLEMENT:code

---

## XIIGEN-SESSION-1000-0265 - Validating task report JSON data

```
source_id: xiigen-session-1000-0265
source_title: "Validating task report JSON data"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: repair
domain: BUG
phase: 3
sub_phase: 3.5
task_type: observability-design
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "24ae3885-e290-44cb-be1a-6aab28986ac9"
  conversation_name: "Validating task report JSON data"
  human_message_uuid: "019a7bbe-d210-766e-a0bf-3c3198939fbe"
  human_message_index: 2
  message_created_at: "2025-11-13T05:44:54.590735Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0265"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    1. Fix broken html@<160FE745-B751-6B1A-8449-28AE9458DE8C> @<96DDFC25-69F2-6901-808D-276C52ED7EA2> 1. לגבי צ'אט, תהיה תקשורת במהלך אישור חשבוניות והתכתבות בהתייחס לחשבונית כזו או אחרת. זה לא יהיה ברמת תוכנית. 2. קופת
    2. Unite under one row - the same time change - 2025-11-12 15:19
    by Luba Karpenko
    State: New → Resolved
    2025-11-12 15:19
    by Luba Karpenko
    Reason: New → Fixed
    2025-11-12 15:19
    by Luba Karpenko
    AssignedTo: → Amit Neman (Technology)
    2025-11-12 15:19
    by Luba Karpenko
    IterationPath: → Keshet\Programs\1.15.0
    2025-11-12 15:19
    by Luba Karpenko
    AreaPath: → Keshet\Programs
    2025-11-12 15:19
    by Luba Karpenko
    Title: → עמוד תוכנית - תוכנית - הסכמים - לא מאפשר לקשר בעל זכות להסכם
    2025-11-12 15:19
    by Luba Karpenko
    Priority: → 2
    2025-11-12 15:19
    3. I don't see any comment, there were no comments?
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0265-prompt"
  SourcePath: "claude-export/batch-0000/24ae3885-e290-44cb-be1a-6aab28986ac9/messages/019a7bbe-d210-766e-a0bf-3c3198939fbe"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Observability and Operational Design"
  CompiledText: |
    1. Fix broken html@<160FE745-B751-6B1A-8449-28AE9458DE8C> @<96DDFC25-69F2-6901-808D-276C52ED7EA2> 1. לגבי צ'אט, תהיה תקשורת במהלך אישור חשבוניות והתכתבות בהתייחס לחשבונית כזו או אחרת. זה לא יהיה ברמת תוכנית. 2. קופת
    2. Unite under one row - the same time change - 2025-11-12 15:19
    by Luba Karpenko
    State: New → Resolved
    2025-11-12 15:19
    by Luba Karpenko
    Reason: New → Fixed
    2025-11-12 15:19
    by Luba Karpenko
    AssignedTo: → Amit Neman (Technology)
    2025-11-12 15:19
    by Luba Karpenko
    IterationPath: → Keshet\Programs\1.15.0
    2025-11-12 15:19
    by Luba Karpenko
    AreaPath: → Keshet\Programs
    2025-11-12 15:19
    by Luba Karpenko
    Title: → עמוד תוכנית - תוכנית - הסכמים - לא מאפשר לקשר בעל זכות להסכם
    2025-11-12 15:19
    by Luba Karpenko
    Priority: → 2
    2025-11-12 15:19
    3. I don't see any comment, there were no comments?
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - IMPLEMENT:fix
  - IMPLEMENT:log

---

## XIIGEN-SESSION-1000-0266 - Chipset and input field side-by-side layout

```
source_id: xiigen-session-1000-0266
source_title: "Chipset and input field side-by-side layout"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
domain: PLAN
phase: 2
sub_phase: 2.1
task_type: business-capability
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "9e447ed9-103f-4d17-9d3b-2089c49e9981"
  conversation_name: "Chipset and input field side-by-side layout"
  human_message_uuid: "019a6d52-23c7-7672-b6ce-f7b1c0bc20e5"
  human_message_index: 4
  message_created_at: "2025-11-10T10:31:31.020458Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0266"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Lets look at mat chip documentation - is there a way to set one line there?"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0266-prompt"
  SourcePath: "claude-export/batch-0000/9e447ed9-103f-4d17-9d3b-2089c49e9981/messages/019a6d52-23c7-7672-b6ce-f7b1c0bc20e5"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Business Capability Definition"
  CompiledText: "Lets look at mat chip documentation - is there a way to set one line there?"
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - DOCUMENT:documentation

---

## XIIGEN-SESSION-1000-0267 - Connecting Claude desktop with Visual Code and Figma

```
source_id: xiigen-session-1000-0267
source_title: "Connecting Claude desktop with Visual Code and Figma"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
domain: CODE_Q
phase: 1
sub_phase: 1.3
task_type: interaction-navigation
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "4fd4abaa-5e04-48a8-a505-237abecdc4f2"
  conversation_name: "Connecting Claude desktop with Visual Code and Figma"
  human_message_uuid: "019ac53c-6f4b-7133-9e8c-3f8329ae4368"
  human_message_index: 0
  message_created_at: "2025-11-27T12:14:23.576607Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0267"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "how to connect claude desktop, visual code and figma?"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0267-prompt"
  SourcePath: "claude-export/batch-0000/4fd4abaa-5e04-48a8-a505-237abecdc4f2/messages/019ac53c-6f4b-7133-9e8c-3f8329ae4368"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Interaction and Navigation Model"
  CompiledText: "how to connect claude desktop, visual code and figma?"
  CompiledPredicateIds: ["IMPLEMENT", "UI"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - IMPLEMENT:code
  - UI:figma

---

## XIIGEN-SESSION-1000-0268 - Connecting Claude desktop with Visual Code and Figma

```
source_id: xiigen-session-1000-0268
source_title: "Connecting Claude desktop with Visual Code and Figma"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
domain: CODE_Q
phase: 1
sub_phase: 1.3
task_type: interaction-navigation
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "4fd4abaa-5e04-48a8-a505-237abecdc4f2"
  conversation_name: "Connecting Claude desktop with Visual Code and Figma"
  human_message_uuid: "019ac53d-8692-76d6-bce3-353c8244dd08"
  human_message_index: 2
  message_created_at: "2025-11-27T12:15:35.067528Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0268"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "I want to be able to chat with claude in visual studio or desktop and it needs to have access to the code and to the figma all together"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0268-prompt"
  SourcePath: "claude-export/batch-0000/4fd4abaa-5e04-48a8-a505-237abecdc4f2/messages/019ac53d-8692-76d6-bce3-353c8244dd08"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Interaction and Navigation Model"
  CompiledText: "I want to be able to chat with claude in visual studio or desktop and it needs to have access to the code and to the figma all together"
  CompiledPredicateIds: ["IMPLEMENT", "UI"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - IMPLEMENT:code
  - UI:figma

---

## XIIGEN-SESSION-1000-0269 - Checking the filesystem

```
source_id: xiigen-session-1000-0269
source_title: "Checking the filesystem"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
domain: CODE_Q
phase: 3
sub_phase: 3.1
task_type: architecture-topology
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "fc783e2c-684b-4e0f-bd62-aecacf43f0db"
  conversation_name: "Checking the filesystem"
  human_message_uuid: "019ac54d-60f8-773b-8a91-8bc75b2b68f7"
  human_message_index: 2
  message_created_at: "2025-11-27T12:32:54.016361Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0269"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    "mcpServers": { "filesystem": { "command": "npx", "args": ["-y", "@modelcontextprotocol/server-filesystem", "/path/to/your/project"] },
    
    I have defined an access to the file system
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0269-prompt"
  SourcePath: "claude-export/batch-0000/fc783e2c-684b-4e0f-bd62-aecacf43f0db/messages/019ac54d-60f8-773b-8a91-8bc75b2b68f7"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Architecture and Component Topology"
  CompiledText: |
    "mcpServers": { "filesystem": { "command": "npx", "args": ["-y", "@modelcontextprotocol/server-filesystem", "/path/to/your/project"] },
    
    I have defined an access to the file system
  CompiledPredicateIds: ["ARCHITECTURE"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - ARCHITECTURE:project

---

## XIIGEN-SESSION-1000-0270 - Checking the filesystem

```
source_id: xiigen-session-1000-0270
source_title: "Checking the filesystem"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
domain: CODE_Q
phase: 3
sub_phase: 3.1
task_type: architecture-topology
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "fc783e2c-684b-4e0f-bd62-aecacf43f0db"
  conversation_name: "Checking the filesystem"
  human_message_uuid: "019ac54e-8053-7178-82da-3027887f62b5"
  human_message_index: 4
  message_created_at: "2025-11-27T12:34:07.585604Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0270"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    ```json
    "filesystem": {
          "command": "npx",
          "args": [
            "-y",
            "@modelcontextprotocol/server-filesystem",
            "C:/Users/luba.karpenko/Documents/Projects"
          ]
        },
    ```
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0270-prompt"
  SourcePath: "claude-export/batch-0000/fc783e2c-684b-4e0f-bd62-aecacf43f0db/messages/019ac54e-8053-7178-82da-3027887f62b5"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Architecture and Component Topology"
  CompiledText: |
    ```json
    "filesystem": {
          "command": "npx",
          "args": [
            "-y",
            "@modelcontextprotocol/server-filesystem",
            "C:/Users/luba.karpenko/Documents/Projects"
          ]
        },
    ```
  CompiledPredicateIds: ["ARCHITECTURE"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - ARCHITECTURE:project

---

## XIIGEN-SESSION-1000-0271 - Access to external projects and websites

```
source_id: xiigen-session-1000-0271
source_title: "Access to external projects and websites"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
domain: CODE_Q
phase: 3
sub_phase: 3.1
task_type: architecture-topology
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "d2cac529-ae50-4e9d-b7d2-0ae208d4d88b"
  conversation_name: "Access to external projects and websites"
  human_message_uuid: "019ac54a-cdfe-7180-938f-1c5e6bc69bcb"
  human_message_index: 0
  message_created_at: "2025-11-27T12:30:05.318095Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0271"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Do you have access to my projects? for example kst.programs.site?"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0271-prompt"
  SourcePath: "claude-export/batch-0000/d2cac529-ae50-4e9d-b7d2-0ae208d4d88b/messages/019ac54a-cdfe-7180-938f-1c5e6bc69bcb"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Architecture and Component Topology"
  CompiledText: "Do you have access to my projects? for example kst.programs.site?"
  CompiledPredicateIds: ["ARCHITECTURE"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - ARCHITECTURE:project

---

## XIIGEN-SESSION-1000-0272 - Access to external projects and websites

```
source_id: xiigen-session-1000-0272
source_title: "Access to external projects and websites"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
domain: CODE_Q
phase: 3
sub_phase: 3.1
task_type: architecture-topology
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "d2cac529-ae50-4e9d-b7d2-0ae208d4d88b"
  conversation_name: "Access to external projects and websites"
  human_message_uuid: "019ac54b-653c-74ce-8e59-628788cf354b"
  human_message_index: 2
  message_created_at: "2025-11-27T12:30:44.038189Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0272"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Do you have access to my code? C:\\Users\\luba.karpenko\\Documents\\Projects\\kst.programs.site"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0272-prompt"
  SourcePath: "claude-export/batch-0000/d2cac529-ae50-4e9d-b7d2-0ae208d4d88b/messages/019ac54b-653c-74ce-8e59-628788cf354b"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Architecture and Component Topology"
  CompiledText: "Do you have access to my code? C:\\Users\\luba.karpenko\\Documents\\Projects\\kst.programs.site"
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - ARCHITECTURE:project
  - IMPLEMENT:code

---

## XIIGEN-SESSION-1000-0273 - JSON config file not found in Azure DevOps pipeline

```
source_id: xiigen-session-1000-0273
source_title: "JSON config file not found in Azure DevOps pipeline"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: repair
domain: BUG
phase: 4
sub_phase: 4.3
task_type: infrastructure-deployment
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "05b1acfc-61ca-4631-a2b1-9f8141621d9d"
  conversation_name: "JSON config file not found in Azure DevOps pipeline"
  human_message_uuid: "019abb89-4500-76ca-aefc-daa2bee98fd4"
  human_message_index: 0
  message_created_at: "2025-11-25T15:02:06.947019Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0273"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    I have an error on deployment of the attached json pipeline on azure devops for deploying release on Deploy stage
    Error: NO JSON file matched with specific pattern: **/config/config.json.
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0273-prompt"
  SourcePath: "claude-export/batch-0000/05b1acfc-61ca-4631-a2b1-9f8141621d9d/messages/019abb89-4500-76ca-aefc-daa2bee98fd4"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Infrastructure and Deployment Code"
  CompiledText: |
    I have an error on deployment of the attached json pipeline on azure devops for deploying release on Deploy stage
    Error: NO JSON file matched with specific pattern: **/config/config.json.
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT", "QA", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - DOCUMENT:spec
  - IMPLEMENT:error
  - QA:release

---

## XIIGEN-SESSION-1000-0274 - Data validation and HTML report generation

```
source_id: xiigen-session-1000-0274
source_title: "Data validation and HTML report generation"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
domain: CODE_Q
phase: 5
sub_phase: 5.1
task_type: unit-tests
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "cbe74706-ed35-466b-8123-4bb724691d2b"
  conversation_name: "Data validation and HTML report generation"
  human_message_uuid: "019ab0ce-e5f0-7406-8c8d-2406e68d47ec"
  human_message_index: 2
  message_created_at: "2025-11-23T13:02:20.756603Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0274"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    1. Missing or invalid or double information - #97582025-11-20 05:45: Description:
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    View
    2. Same info several times - need just the latest2025-11-20 06:28: Description:
    המטרה של המשימה היא גם לייצר תשתיות וגם לייצר מכסה פרומפטים להמשך תהליך ... →
    המטרה של המשימה היא גם לייצר תשתיות וגם לייצר מכסה פרומפטים להמשך תהליך ... (by Tom Robissa) | 2025-11-20 09:40: Description:
    המטרה של המשימה היא גם לייצר תשתיות וגם לייצר מכסה פרומפטים להמשך תהליך ... →
    המטרה של המשימה היא גם לייצר תשתיות וגם לייצר מכסה פרומפטים להמשך תהליך ... (by Tom Robissa) | 2025-11-20 12:08: Description:
    המטרה של המשימה היא גם לייצר תשתיות וגם לייצר מכסה פרומפטים להמשך תהליך ... →
    המטרה של המשימה היא גם לייצר תשתיות וגם לייצר מכסה פרומפטים להמשך תהליך ... (by Tom Robissa) | 2025-11-20 12:12: Description:
    המטרה של המשימה היא גם לייצר תשתיות וגם לייצר מכסה פרומפטים להמשך תהליך ... →
    המטרה של המשימה היא גם לייצר תשתיות וגם לייצר מכסה פרומפטים להמשך תהליך ... (by Tom Robissa)
    3.
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0274-prompt"
  SourcePath: "claude-export/batch-0000/cbe74706-ed35-466b-8123-4bb724691d2b/messages/019ab0ce-e5f0-7406-8c8d-2406e68d47ec"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Unit Test Design"
  CompiledText: |
    1. Missing or invalid or double information - #97582025-11-20 05:45: Description:
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    __מטרה:__ 
    View
    2. Same info several times - need just the latest2025-11-20 06:28: Description:
    המטרה של המשימה היא גם לייצר תשתיות וגם לייצר מכסה פרומפטים להמשך תהליך ... →
    המטרה של המשימה היא גם לייצר תשתיות וגם לייצר מכסה פרומפטים להמשך תהליך ... (by Tom Robissa) | 2025-11-20 09:40: Description:
    המטרה של המשימה היא גם לייצר תשתיות וגם לייצר מכסה פרומפטים להמשך תהליך ... →
    המטרה של המשימה היא גם לייצר תשתיות וגם לייצר מכסה פרומפטים להמשך תהליך ... (by Tom Robissa) | 2025-11-20 12:08: Description:
    המטרה של המשימה היא גם לייצר תשתיות וגם לייצר מכסה פרומפטים להמשך תהליך ... →
    המטרה של המשימה היא גם לייצר תשתיות וגם לייצר מכסה פרומפטים להמשך תהליך ... (by Tom Robissa) | 2025-11-20 12:12: Description:
    המטרה של המשימה היא גם לייצר תשתיות וגם לייצר מכסה פרומפטים להמשך תהליך ... →
    המטרה של המשימה היא גם לייצר תשתיות וגם לייצר מכסה פרומפטים להמשך תהליך ... (by Tom Robissa)
    3.
  CompiledPredicateIds: ["TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - TEST:test

---

## XIIGEN-SESSION-1000-0275 - Data validation and HTML report generation

```
source_id: xiigen-session-1000-0275
source_title: "Data validation and HTML report generation"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: qa_release
domain: CODE_Q
phase: 3
sub_phase: 3.1
task_type: architecture-topology
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "cbe74706-ed35-466b-8123-4bb724691d2b"
  conversation_name: "Data validation and HTML report generation"
  human_message_uuid: "019ab0d7-eb69-75e2-842c-8069e6787e1e"
  human_message_index: 4
  message_created_at: "2025-11-23T13:12:11.898581Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0275"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    No need to repeat the same timestamp for each change, and no need to show None-> just the latest
    2025-11-20 13:34: TeamProject: None → Keshet (by Luba Karpenko) | 2025-11-20 13:34: AreaLevel1: None → Keshet (by Luba Karpenko) | 2025-11-20 13:34: WEF_FBE805A78155411C93F54E6DA9C66B5B_ExtensionMarker: None → True (by Luba Karpenko) | 2025-11-20 13:34: Parent: None → 9845 (by Luba Karpenko) | 2025-11-20 13:34: ValueArea: None → Business (by Luba Karpenko) | 2025-11-20 13:34: State: None → New (by Luba Karpenko) | 2025-11-20 13:34: WorkItemType: None → User Story (by Luba Karpenko) | 2025-11-20 13:34: CreatedBy: None → Luba Karpenko (by Luba Karpenko) | 2025-11-20 13:34: AssignedTo: None → Jacob Ychye (by Luba Karpenko) | 2025-11-20 13:34: IterationLevel2: None → Spotifat (by Luba Karpenko) | 2025-11-20 13:34: IterationLevel1: None → Keshet (by Luba Karpenko) | 2025-11-20 13:34: WEF_FBE805A78155411C93F54E6DA9C66B5B_Kanban.Column.Done: None → False (by Luba Karpenko) | 2025-11-20 13:34: WEF_04E7876E0B314FB9AA565C0A37D7AB80_ExtensionMarker: None → True (by Luba Karpenko) | 2025-11-20 13:34: AreaPath: None → Keshet\Spotifat (by Luba Karpenko) | 2025-11-20 13:34: AreaId: None → 85 (by Luba Karpenko) | 2025-11-20 13:34: WEF_FBE805A78155411C93F54E6DA9C66B5B_Kanban.Column: None → New (by Luba Karpenko) | 2025-11-20 13:34: BoardColumn: None → New (by Luba Karpenko) | 2025-11-20 13:34: NodeName: None → Spotifat (by Luba Karpenko) | 2025-11-20 13:34: IterationId: None → 98 (by Luba Karpenko) | 2025-11-20 13:34: Priority: None → 2 (by Luba Karpenko) | 2025-11-20 13:34: IsDeleted: None → False (by Luba Karpenko) | 2025-11-20 13:34: WEF_04E7876E0B314FB9AA565C0A37D7AB80_Kanban.Column: None → New (by Luba Karpenko) | 2025-11-20 13:34: ReadyforQA: None → False (by Luba Karpenko) | 2025-11-20 13:34: WEF_04E7876E0B314FB9AA565C0A37D7AB80_Kanban.Column.Done: None → False (by Luba Karpenko) | 2025-11-20 13:34: Id: None → 9859 (by Luba Karpenko) | 2025-11-20 13:34: AreaLevel2: None → Spotifat (by Luba Karpenko) | 2025-11-20 13:34: Title: None → טעינת נתונים ל Azure AI Foundry (by Luba Karpenko) | 2025-11-20 13:34: CreatedDate: None → 2025-11-19T16:21:15.197Z (by Luba Karpenko) | 2025-11-20 13:34: IterationPath: None → Spotifat (by Luba Karpenko) | 2025-11-20 13:34: BoardColumnDone: None → False (by Luba Karpenko)
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0275-prompt"
  SourcePath: "claude-export/batch-0000/cbe74706-ed35-466b-8123-4bb724691d2b/messages/019ab0d7-eb69-75e2-842c-8069e6787e1e"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Architecture and Component Topology"
  CompiledText: |
    No need to repeat the same timestamp for each change, and no need to show None-> just the latest
    2025-11-20 13:34: TeamProject: None → Keshet (by Luba Karpenko) | 2025-11-20 13:34: AreaLevel1: None → Keshet (by Luba Karpenko) | 2025-11-20 13:34: WEF_FBE805A78155411C93F54E6DA9C66B5B_ExtensionMarker: None → True (by Luba Karpenko) | 2025-11-20 13:34: Parent: None → 9845 (by Luba Karpenko) | 2025-11-20 13:34: ValueArea: None → Business (by Luba Karpenko) | 2025-11-20 13:34: State: None → New (by Luba Karpenko) | 2025-11-20 13:34: WorkItemType: None → User Story (by Luba Karpenko) | 2025-11-20 13:34: CreatedBy: None → Luba Karpenko (by Luba Karpenko) | 2025-11-20 13:34: AssignedTo: None → Jacob Ychye (by Luba Karpenko) | 2025-11-20 13:34: IterationLevel2: None → Spotifat (by Luba Karpenko) | 2025-11-20 13:34: IterationLevel1: None → Keshet (by Luba Karpenko) | 2025-11-20 13:34: WEF_FBE805A78155411C93F54E6DA9C66B5B_Kanban.Column.Done: None → False (by Luba Karpenko) | 2025-11-20 13:34: WEF_04E7876E0B314FB9AA565C0A37D7AB80_ExtensionMarker: None → True (by Luba Karpenko) | 2025-11-20 13:34: AreaPath: None → Keshet\Spotifat (by Luba Karpenko) | 2025-11-20 13:34: AreaId: None → 85 (by Luba Karpenko) | 2025-11-20 13:34: WEF_FBE805A78155411C93F54E6DA9C66B5B_Kanban.Column: None → New (by Luba Karpenko) | 2025-11-20 13:34: BoardColumn: None → New (by Luba Karpenko) | 2025-11-20 13:34: NodeName: None → Spotifat (by Luba Karpenko) | 2025-11-20 13:34: IterationId: None → 98 (by Luba Karpenko) | 2025-11-20 13:34: Priority: None → 2 (by Luba Karpenko) | 2025-11-20 13:34: IsDeleted: None → False (by Luba Karpenko) | 2025-11-20 13:34: WEF_04E7876E0B314FB9AA565C0A37D7AB80_Kanban.Column: None → New (by Luba Karpenko) | 2025-11-20 13:34: ReadyforQA: None → False (by Luba Karpenko) | 2025-11-20 13:34: WEF_04E7876E0B314FB9AA565C0A37D7AB80_Kanban.Column.Done: None → False (by Luba Karpenko) | 2025-11-20 13:34: Id: None → 9859 (by Luba Karpenko) | 2025-11-20 13:34: AreaLevel2: None → Spotifat (by Luba Karpenko) | 2025-11-20 13:34: Title: None → טעינת נתונים ל Azure AI Foundry (by Luba Karpenko) | 2025-11-20 13:34: CreatedDate: None → 2025-11-19T16:21:15.197Z (by Luba Karpenko) | 2025-11-20 13:34: IterationPath: None → Spotifat (by Luba Karpenko) | 2025-11-20 13:34: BoardColumnDone: None → False (by Luba Karpenko)
  CompiledPredicateIds: ["ARCHITECTURE", "QA", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - ARCHITECTURE:project
  - QA:qa
  - TEST:test

---

## XIIGEN-SESSION-1000-0276 - Data validation and HTML report generation

```
source_id: xiigen-session-1000-0276
source_title: "Data validation and HTML report generation"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
domain: CODE_Q
phase: 3
sub_phase: 3.3
task_type: api-contract
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "59efb61c-8e23-4840-854d-48550c9c8d94"
  conversation_name: "Data validation and HTML report generation"
  human_message_uuid: "019aba32-64cb-7506-9728-bb31afaac282"
  human_message_index: 2
  message_created_at: "2025-11-25T08:47:36.218767Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0276"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    1. broken  html
    תשלוף את נתוני השידור עבור האובייקטים הבאים: </div><div><div style="direction:rtl;"><ul><li>אות </li><li>קליפ </li><li>פתיח /סגיר </li><li>פילר (תוכניות + לוח ) </li><li>דוח - רשימות שידור לאקום&nbsp; </li> </ul><div><br> </div> </div> </div><div style="direction:rtl;"><p>לאחר קבלת הנתונים ושליחתם למערכת <em>תוכניות</em>
    1. Same information - no need in several lines with same info just the last one 2025-11-24 10:55
    by Luba Karpenko
    AuthorizedAs: Microsoft.VisualStudio.Services.TFS → Luba Karpenko CommentCount: 11 → 12 History: None → Associated with changeset 2261: #9662 #9943 add a call to content api - all programs with internal producing with a contractorCode
    2025-11-24 10:58
    by Luba Karpenko
    AuthorizedAs: Microsoft.VisualStudio.Services.TFS → Luba Karpenko CommentCount: 11 → 12 History: None → Associated with changeset 2261: #9662 #9943 add a call to content api - all programs with internal producing with a contractorCode
    2025-11-24 10:59
    by Luba Karpenko
    AuthorizedAs: Microsoft.VisualStudio.Services.TFS → Luba Karpenko CommentCount: 11 → 12 History: None → Associated with changeset 2261: #9662 #9943 add a call to content api - all programs with internal producing with a contractorCode
    2025-11-24 12:52
    by Luba Karpenko
    AuthorizedAs: Microsoft.VisualStudio.Services.TFS → Luba Karpenko CommentCount: 11 → 12 History: None → Associated with changeset 2261: #9662 #9943 add a call to content api - all programs with internal producing with a contractorCode
    2025-11-24 12:04
    by Luba Karpenko
    AuthorizedAs: Microsoft.VisualStudio.Services.TFS → Luba Karpenko CommentCount: 11 → 12 History: None → Associated with changeset 2261: #9662 #9943 add a call to content api - all programs with internal producing with a contractorCode
    2025-11-24 12:56
    by Luba Karpenko
    AuthorizedAs: Microsoft.VisualStudio.Services.TFS → Luba Karpenko CommentCount: 11 → 12 History: None → Associated with changeset 2261: #9662 #9943 add a call to content api - all programs with internal producing with a contractorCode
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0276-prompt"
  SourcePath: "claude-export/batch-0000/59efb61c-8e23-4840-854d-48550c9c8d94/messages/019aba32-64cb-7506-9728-bb31afaac282"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    1. broken  html
    תשלוף את נתוני השידור עבור האובייקטים הבאים: </div><div><div style="direction:rtl;"><ul><li>אות </li><li>קליפ </li><li>פתיח /סגיר </li><li>פילר (תוכניות + לוח ) </li><li>דוח - רשימות שידור לאקום&nbsp; </li> </ul><div><br> </div> </div> </div><div style="direction:rtl;"><p>לאחר קבלת הנתונים ושליחתם למערכת <em>תוכניות</em>
    1. Same information - no need in several lines with same info just the last one 2025-11-24 10:55
    by Luba Karpenko
    AuthorizedAs: Microsoft.VisualStudio.Services.TFS → Luba Karpenko CommentCount: 11 → 12 History: None → Associated with changeset 2261: #9662 #9943 add a call to content api - all programs with internal producing with a contractorCode
    2025-11-24 10:58
    by Luba Karpenko
    AuthorizedAs: Microsoft.VisualStudio.Services.TFS → Luba Karpenko CommentCount: 11 → 12 History: None → Associated with changeset 2261: #9662 #9943 add a call to content api - all programs with internal producing with a contractorCode
    2025-11-24 10:59
    by Luba Karpenko
    AuthorizedAs: Microsoft.VisualStudio.Services.TFS → Luba Karpenko CommentCount: 11 → 12 History: None → Associated with changeset 2261: #9662 #9943 add a call to content api - all programs with internal producing with a contractorCode
    2025-11-24 12:52
    by Luba Karpenko
    AuthorizedAs: Microsoft.VisualStudio.Services.TFS → Luba Karpenko CommentCount: 11 → 12 History: None → Associated with changeset 2261: #9662 #9943 add a call to content api - all programs with internal producing with a contractorCode
    2025-11-24 12:04
    by Luba Karpenko
    AuthorizedAs: Microsoft.VisualStudio.Services.TFS → Luba Karpenko CommentCount: 11 → 12 History: None → Associated with changeset 2261: #9662 #9943 add a call to content api - all programs with internal producing with a contractorCode
    2025-11-24 12:56
    by Luba Karpenko
    AuthorizedAs: Microsoft.VisualStudio.Services.TFS → Luba Karpenko CommentCount: 11 → 12 History: None → Associated with changeset 2261: #9662 #9943 add a call to content api - all programs with internal producing with a contractorCode
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:api
  - API_CONTRACT:contract
  - IMPLEMENT:code
  - SECURITY:auth

---

## XIIGEN-SESSION-1000-0277 - Project files access request

```
source_id: xiigen-session-1000-0277
source_title: "Project files access request"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
domain: CODE_Q
phase: 3
sub_phase: 3.1
task_type: architecture-topology
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "0e59c12d-55e2-476e-aa81-6fb85ceea45f"
  conversation_name: "Project files access request"
  human_message_uuid: "019ac551-632c-7271-b4e0-a509fc0d5a8a"
  human_message_index: 0
  message_created_at: "2025-11-27T12:37:16.781528Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0277"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Can you see my project files now?  C:\\Users\\luba.karpenko\\Documents\\Projects\\kst.programs.site"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0277-prompt"
  SourcePath: "claude-export/batch-0000/0e59c12d-55e2-476e-aa81-6fb85ceea45f/messages/019ac551-632c-7271-b4e0-a509fc0d5a8a"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Architecture and Component Topology"
  CompiledText: "Can you see my project files now?  C:\\Users\\luba.karpenko\\Documents\\Projects\\kst.programs.site"
  CompiledPredicateIds: ["ARCHITECTURE"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - ARCHITECTURE:project

---

## XIIGEN-SESSION-1000-0278 - Project files access request

```
source_id: xiigen-session-1000-0278
source_title: "Project files access request"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
domain: CODE_Q
phase: 3
sub_phase: 3.1
task_type: architecture-topology
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "0e59c12d-55e2-476e-aa81-6fb85ceea45f"
  conversation_name: "Project files access request"
  human_message_uuid: "019ac554-7e29-768e-8f2b-518a1f289ee3"
  human_message_index: 2
  message_created_at: "2025-11-27T12:40:40.241506Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0278"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    I need to edit the component 
    
    ```typescript
    InsuranceRowComponent
    ```
    
    to adjust the design to what I have in figma project Programs -> insurance (Destktop 18) view https://www.figma.com/design/8R83McB8qGlR0Cy32hYFrB/Program-Content?node-id=3638-620&m=dev
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0278-prompt"
  SourcePath: "claude-export/batch-0000/0e59c12d-55e2-476e-aa81-6fb85ceea45f/messages/019ac554-7e29-768e-8f2b-518a1f289ee3"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Architecture and Component Topology"
  CompiledText: |
    I need to edit the component 
    
    ```typescript
    InsuranceRowComponent
    ```
    
    to adjust the design to what I have in figma project Programs -> insurance (Destktop 18) view https://www.figma.com/design/8R83McB8qGlR0Cy32hYFrB/Program-Content?node-id=3638-620&m=dev
  CompiledPredicateIds: ["ARCHITECTURE", "UI"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - ARCHITECTURE:project
  - UI:figma

---

## XIIGEN-SESSION-1000-0279 - Project files access request

```
source_id: xiigen-session-1000-0279
source_title: "Project files access request"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
domain: CODE_Q
phase: 3
sub_phase: 3.5
task_type: observability-design
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "0e59c12d-55e2-476e-aa81-6fb85ceea45f"
  conversation_name: "Project files access request"
  human_message_uuid: "019ac55e-584d-761a-93da-47a46ea84305"
  human_message_index: 6
  message_created_at: "2025-11-27T12:51:25.981175Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0279"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "This is the checkbox https://www.figma.com/design/8R83McB8qGlR0Cy32hYFrB/Program-Content?node-id=3638-664&m=dev and it is missing from view"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0279-prompt"
  SourcePath: "claude-export/batch-0000/0e59c12d-55e2-476e-aa81-6fb85ceea45f/messages/019ac55e-584d-761a-93da-47a46ea84305"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Observability and Operational Design"
  CompiledText: "This is the checkbox https://www.figma.com/design/8R83McB8qGlR0Cy32hYFrB/Program-Content?node-id=3638-664&m=dev and it is missing from view"
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - UI:figma

---

## XIIGEN-SESSION-1000-0280 - Project files access request

```
source_id: xiigen-session-1000-0280
source_title: "Project files access request"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
domain: CODE_Q
phase: 3
sub_phase: 3.3
task_type: api-contract
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "0e59c12d-55e2-476e-aa81-6fb85ceea45f"
  conversation_name: "Project files access request"
  human_message_uuid: "019ac564-c77d-76c6-ba10-015c0af19b9a"
  human_message_index: 12
  message_created_at: "2025-11-27T12:58:27.626033Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0280"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    Nope no checkbox
    <insurance-row _ngcontent-ng-c2387102970="" _nghost-ng-c3779843325="" ng-reflect-insurance="[object Object]" ng-reflect-show-checkbox="false" class="ng-star-inserted"><div _ngcontent-ng-c3779843325="" class="insurance-row" id=""><div _ngcontent-ng-c3779843325="" class="insurance-col checkbox-col"><!--container--></div><div _ngcontent-ng-c3779843325="" class="insurance-col contract-col"><div _ngcontent-ng-c3779843325="" class="primary-text">רוקדים עם כוכבים- עונה 2</div><div _ngcontent-ng-c3779843325="" class="secondary-text">הפקה</div></div><div _ngcontent-ng-c3779843325="" class="insurance-col expiration-col"><div _ngcontent-ng-c3779843325="" class="primary-text">19/11/2025</div></div><div _ngcontent-ng-c3779843325="" class="insurance-col parties-col"><div _ngcontent-ng-c3779843325="" class="mat-mdc-tooltip-trigger primary-text" ng-reflect-message="טדי יזמות והפקה בע&quot;מ" aria-describedby="cdk-describedby-message-ng-1-2" cdk-describedby-host="ng-1"> טדי יזמות והפקה בע"מ </div><!--container--><div _ngcontent-ng-c3779843325="" class="secondary-text clickable"> רוקדים עם כוכבים – עונה 2 </div></div><div _ngcontent-ng-c3779843325="" class="insurance-col creation-col"><div _ngcontent-ng-c3779843325="" class="primary-text">16/11/2025</div><div _ngcontent-ng-c3779843325="" class="secondary-text">נשלח ע"י Jacob.Ychye@Keshet-tv.com</div></div><div _ngcontent-ng-c3779843325="" class="insurance-col icon-col"><svg _ngcontent-ng-c3779843325="" width="26" height="32" viewBox="0 0 26 32" fill="none" xmlns="http://www.w3.org/2000/svg"><path _ngcontent-ng-c3779843325="" d="M16.0003 0.000488281H2.66699C1.20033 0.000488281 0.0136583 1.20049 0.0136583 2.66716L0.000325203 29.3338C0.000325203 30.8005 1.18699 32.0005 2.65366 32.0005H23.3337C24.8003 32.0005 26.0003 30.8005 26.0003 29.3338V10.6672L16.0003 0.000488281ZM20.667 26.6672H5.33366V24.0005H20.667V26.6672ZM20.667 21.3338H5.33366V18.6672H20.667V21.3338ZM14.667 12.0005V2.00049L24.667 12.0005H14.667Z" fill="#4D48D8"></path></svg></div><div _ngcontent-ng-c3779843325="" class="insurance-col status-col"><div _ngcontent-ng-c3779843325="" class="status-badge status-pending"> ממתין לאישור </div></div></div></insurance-row>
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0280-prompt"
  SourcePath: "claude-export/batch-0000/0e59c12d-55e2-476e-aa81-6fb85ceea45f/messages/019ac564-c77d-76c6-ba10-015c0af19b9a"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    Nope no checkbox
    <insurance-row _ngcontent-ng-c2387102970="" _nghost-ng-c3779843325="" ng-reflect-insurance="[object Object]" ng-reflect-show-checkbox="false" class="ng-star-inserted"><div _ngcontent-ng-c3779843325="" class="insurance-row" id=""><div _ngcontent-ng-c3779843325="" class="insurance-col checkbox-col"><!--container--></div><div _ngcontent-ng-c3779843325="" class="insurance-col contract-col"><div _ngcontent-ng-c3779843325="" class="primary-text">רוקדים עם כוכבים- עונה 2</div><div _ngcontent-ng-c3779843325="" class="secondary-text">הפקה</div></div><div _ngcontent-ng-c3779843325="" class="insurance-col expiration-col"><div _ngcontent-ng-c3779843325="" class="primary-text">19/11/2025</div></div><div _ngcontent-ng-c3779843325="" class="insurance-col parties-col"><div _ngcontent-ng-c3779843325="" class="mat-mdc-tooltip-trigger primary-text" ng-reflect-message="טדי יזמות והפקה בע&quot;מ" aria-describedby="cdk-describedby-message-ng-1-2" cdk-describedby-host="ng-1"> טדי יזמות והפקה בע"מ </div><!--container--><div _ngcontent-ng-c3779843325="" class="secondary-text clickable"> רוקדים עם כוכבים – עונה 2 </div></div><div _ngcontent-ng-c3779843325="" class="insurance-col creation-col"><div _ngcontent-ng-c3779843325="" class="primary-text">16/11/2025</div><div _ngcontent-ng-c3779843325="" class="secondary-text">נשלח ע"י Jacob.Ychye@Keshet-tv.com</div></div><div _ngcontent-ng-c3779843325="" class="insurance-col icon-col"><svg _ngcontent-ng-c3779843325="" width="26" height="32" viewBox="0 0 26 32" fill="none" xmlns="http://www.w3.org/2000/svg"><path _ngcontent-ng-c3779843325="" d="M16.0003 0.000488281H2.66699C1.20033 0.000488281 0.0136583 1.20049 0.0136583 2.66716L0.000325203 29.3338C0.000325203 30.8005 1.18699 32.0005 2.65366 32.0005H23.3337C24.8003 32.0005 26.0003 30.8005 26.0003 29.3338V10.6672L16.0003 0.000488281ZM20.667 26.6672H5.33366V24.0005H20.667V26.6672ZM20.667 21.3338H5.33366V18.6672H20.667V21.3338ZM14.667 12.0005V2.00049L24.667 12.0005H14.667Z" fill="#4D48D8"></path></svg></div><div _ngcontent-ng-c3779843325="" class="insurance-col status-col"><div _ngcontent-ng-c3779843325="" class="status-badge status-pending"> ממתין לאישור </div></div></div></insurance-row>
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:contract

---

## XIIGEN-SESSION-1000-0281 - Project files access request

```
source_id: xiigen-session-1000-0281
source_title: "Project files access request"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: repair
domain: BUG
phase: 4
sub_phase: 4.1
task_type: backend-services
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "0e59c12d-55e2-476e-aa81-6fb85ceea45f"
  conversation_name: "Project files access request"
  human_message_uuid: "019ac566-7156-75aa-bd04-4aa45995d9ef"
  human_message_index: 14
  message_created_at: "2025-11-27T13:00:16.604738Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0281"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Ok great, now fix the style"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0281-prompt"
  SourcePath: "claude-export/batch-0000/0e59c12d-55e2-476e-aa81-6fb85ceea45f/messages/019ac566-7156-75aa-bd04-4aa45995d9ef"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Backend Services"
  CompiledText: "Ok great, now fix the style"
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - IMPLEMENT:fix

---

## XIIGEN-SESSION-1000-0282 - Project files access request

```
source_id: xiigen-session-1000-0282
source_title: "Project files access request"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
domain: CODE_Q
phase: 3
sub_phase: 3.5
task_type: observability-design
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "0e59c12d-55e2-476e-aa81-6fb85ceea45f"
  conversation_name: "Project files access request"
  human_message_uuid: "019ac568-c4c0-76be-9171-0058bb499461"
  human_message_index: 16
  message_created_at: "2025-11-27T13:02:49.059803Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0282"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "This is still very big checkbox... Needs to be exactly like in figma https://www.figma.com/design/8R83McB8qGlR0Cy32hYFrB/Program-Content?node-id=3638-620&m=dev"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0282-prompt"
  SourcePath: "claude-export/batch-0000/0e59c12d-55e2-476e-aa81-6fb85ceea45f/messages/019ac568-c4c0-76be-9171-0058bb499461"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Observability and Operational Design"
  CompiledText: "This is still very big checkbox... Needs to be exactly like in figma https://www.figma.com/design/8R83McB8qGlR0Cy32hYFrB/Program-Content?node-id=3638-620&m=dev"
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - UI:figma

---

## XIIGEN-SESSION-1000-0283 - Missing npm script build:dev error

```
source_id: xiigen-session-1000-0283
source_title: "Missing npm script build:dev error"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: repair
domain: BUG
phase: 3
sub_phase: 3.1
task_type: architecture-topology
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "3e619944-f7ec-483d-a969-903074217b8d"
  conversation_name: "Missing npm script build:dev error"
  human_message_uuid: "019abb5b-baed-73ce-8092-effab71c9f8c"
  human_message_index: 0
  message_created_at: "2025-11-25T14:12:22.603539Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0283"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    for the yaml I get an error on Build Dev
    C:\Windows\system32\cmd.exe /D /S /C "C:\hostedtoolcache\windows\node\22.14.0\x64\npm.cmd config list" 
    ; "global" config from C:\npm\prefix\etc\npmrc 
    cache = "C:\\npm\\cache" 
    ; "project" config from D:\a\1\s\.npmrc 
    loglevel = "error" 
    ; "env" config from environment 
    prefix = "C:\\npm\\prefix" 
    userconfig = "D:\\a\\1\\npm\\11505.npmrc" 
    ; node bin location = C:\hostedtoolcache\windows\node\22.14.0\x64\node.exe 
    ; node version = v22.14.0 
    ; npm local prefix = D:\a\1\s 
    ; npm version = 10.9.2 
    ; cwd = D:\a\1\s 
    ; HOME = C:\Users\VssAdministrator 
    ; Run `npm config ls -l` to show all defaults. 
    C:\Windows\system32\cmd.exe /D /S /C "C:\hostedtoolcache\windows\node\22.14.0\x64\npm.cmd run build:dev" 
    npm error Missing script: "build:dev" 
    npm error 
    npm error To see a list of scripts, run: 
    npm error   npm run 
    npm error A complete log of this run can be found in: C:\npm\cache\_logs\2025-11-25T14_07_41_235Z-debug-0.log 
    ##[warning]Couldn't find a debug log in the cache or working directory 
    ##[error]Error: Npm failed with return code: 1 
    Finishing: Build Dev
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0283-prompt"
  SourcePath: "claude-export/batch-0000/3e619944-f7ec-483d-a969-903074217b8d/messages/019abb5b-baed-73ce-8092-effab71c9f8c"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Architecture and Component Topology"
  CompiledText: |
    for the yaml I get an error on Build Dev
    C:\Windows\system32\cmd.exe /D /S /C "C:\hostedtoolcache\windows\node\22.14.0\x64\npm.cmd config list" 
    ; "global" config from C:\npm\prefix\etc\npmrc 
    cache = "C:\\npm\\cache" 
    ; "project" config from D:\a\1\s\.npmrc 
    loglevel = "error" 
    ; "env" config from environment 
    prefix = "C:\\npm\\prefix" 
    userconfig = "D:\\a\\1\\npm\\11505.npmrc" 
    ; node bin location = C:\hostedtoolcache\windows\node\22.14.0\x64\node.exe 
    ; node version = v22.14.0 
    ; npm local prefix = D:\a\1\s 
    ; npm version = 10.9.2 
    ; cwd = D:\a\1\s 
    ; HOME = C:\Users\VssAdministrator 
    ; Run `npm config ls -l` to show all defaults. 
    C:\Windows\system32\cmd.exe /D /S /C "C:\hostedtoolcache\windows\node\22.14.0\x64\npm.cmd run build:dev" 
    npm error Missing script: "build:dev" 
    npm error 
    npm error To see a list of scripts, run: 
    npm error   npm run 
    npm error A complete log of this run can be found in: C:\npm\cache\_logs\2025-11-25T14_07_41_235Z-debug-0.log 
    ##[warning]Couldn't find a debug log in the cache or working directory 
    ##[error]Error: Npm failed with return code: 1 
    Finishing: Build Dev
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "TEST", "UI"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - ARCHITECTURE:project
  - IMPLEMENT:code
  - IMPLEMENT:fix
  - IMPLEMENT:bug
  - IMPLEMENT:error
  - IMPLEMENT:failed
  - IMPLEMENT:log
  - UI:ui

---

## XIIGEN-SESSION-1000-0284 - Missing npm script build:dev error

```
source_id: xiigen-session-1000-0284
source_title: "Missing npm script build:dev error"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: repair
domain: BUG
phase: 3
sub_phase: 3.1
task_type: architecture-topology
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "3e619944-f7ec-483d-a969-903074217b8d"
  conversation_name: "Missing npm script build:dev error"
  human_message_uuid: "019abb61-fc4e-755e-b431-942440c53111"
  human_message_index: 2
  message_created_at: "2025-11-25T14:19:12.386753Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0284"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    Now I have a bug on build:prod 
    1. ; npm local prefix = D:\a\1\s 
    ; npm version = 10.9.2 
    ; cwd = D:\a\1\s 
    ; HOME = C:\Users\VssAdministrator 
    ; Run `npm config ls -l` to show all defaults. 
    C:\Windows\system32\cmd.exe /D /S /C "C:\hostedtoolcache\windows\node\22.14.0\x64\npm.cmd run build:prod" 
    ▲ [WARNING] src/app/components/chat/chat.scss exceeded maximum budget. Budget 2.00 kB was not met by 24.19 kB with a total of 26.19 kB. 
    > kst.social.media.analytics.site@1.0.0 build:prod 
    > ng build --aot --configuration=production 
    X [ERROR] src/app/components/chat/chat.scss exceeded maximum budget. Budget 4.00 kB was not met by 22.19 kB with a total of 26.19 kB. 
    > Building... 
    √ Building... 
    Initial chunk files   | Names         |  Raw size | Estimated transfer size 
    main-XUQR33CD.js      | main          | 395.79 kB |                98.89 kB 
    polyfills-5CFQRCPP.js | polyfills     |  34.59 kB |                11.33 kB 
    styles-MI55P2X6.css   | styles        |   2.89 kB |               819 bytes 
     | Initial total | 433.26 kB |               111.04 kB 
    Application bundle generation failed. [5.561 seconds] - 2025-11-25T14:17:44.209Z 
    ##[warning]Couldn't find a debug log in the cache or working directory 
    ##[error]Error: Npm failed with return code: 1 
    Finishing: Build Prod
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0284-prompt"
  SourcePath: "claude-export/batch-0000/3e619944-f7ec-483d-a969-903074217b8d/messages/019abb61-fc4e-755e-b431-942440c53111"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Architecture and Component Topology"
  CompiledText: |
    Now I have a bug on build:prod 
    1. ; npm local prefix = D:\a\1\s 
    ; npm version = 10.9.2 
    ; cwd = D:\a\1\s 
    ; HOME = C:\Users\VssAdministrator 
    ; Run `npm config ls -l` to show all defaults. 
    C:\Windows\system32\cmd.exe /D /S /C "C:\hostedtoolcache\windows\node\22.14.0\x64\npm.cmd run build:prod" 
    ▲ [WARNING] src/app/components/chat/chat.scss exceeded maximum budget. Budget 2.00 kB was not met by 24.19 kB with a total of 26.19 kB. 
    > kst.social.media.analytics.site@1.0.0 build:prod 
    > ng build --aot --configuration=production 
    X [ERROR] src/app/components/chat/chat.scss exceeded maximum budget. Budget 4.00 kB was not met by 22.19 kB with a total of 26.19 kB. 
    > Building... 
    √ Building... 
    Initial chunk files   | Names         |  Raw size | Estimated transfer size 
    main-XUQR33CD.js      | main          | 395.79 kB |                98.89 kB 
    polyfills-5CFQRCPP.js | polyfills     |  34.59 kB |                11.33 kB 
    styles-MI55P2X6.css   | styles        |   2.89 kB |               819 bytes 
     | Initial total | 433.26 kB |               111.04 kB 
    Application bundle generation failed. [5.561 seconds] - 2025-11-25T14:17:44.209Z 
    ##[warning]Couldn't find a debug log in the cache or working directory 
    ##[error]Error: Npm failed with return code: 1 
    Finishing: Build Prod
  CompiledPredicateIds: ["IMPLEMENT", "TEST", "UI"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - IMPLEMENT:code
  - IMPLEMENT:fix
  - IMPLEMENT:bug
  - IMPLEMENT:error
  - IMPLEMENT:failed
  - IMPLEMENT:log
  - UI:ui

---

## XIIGEN-SESSION-1000-0285 - Missing npm script build:dev error

```
source_id: xiigen-session-1000-0285
source_title: "Missing npm script build:dev error"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: repair
domain: BUG
phase: 4
sub_phase: 4.1
task_type: backend-services
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "3e619944-f7ec-483d-a969-903074217b8d"
  conversation_name: "Missing npm script build:dev error"
  human_message_uuid: "019abb62-f19b-70f4-8596-1a9df772acd5"
  human_message_index: 4
  message_created_at: "2025-11-25T14:20:15.423442Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0285"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "please check the budgets restrictions and other possible errors"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0285-prompt"
  SourcePath: "claude-export/batch-0000/3e619944-f7ec-483d-a969-903074217b8d/messages/019abb62-f19b-70f4-8596-1a9df772acd5"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Backend Services"
  CompiledText: "please check the budgets restrictions and other possible errors"
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - IMPLEMENT:error

---

## XIIGEN-SESSION-1000-0286 - S3 404 error with Angular routing on direct path access

```
source_id: xiigen-session-1000-0286
source_title: "S3 404 error with Angular routing on direct path access"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: repair
domain: BUG
phase: 3
sub_phase: 3.3
task_type: api-contract
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "e347a9a5-75c7-453f-a9be-374012835221"
  conversation_name: "S3 404 error with Angular routing on direct path access"
  human_message_uuid: "019b2cbf-8c40-73d3-a59b-2ff6e251c082"
  human_message_index: 0
  message_created_at: "2025-12-17T14:38:29.733875Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0286"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    I have an error when accessing the site hosted on s3 404 Not Found Code: NoSuchKey Message: The specified key does not exist. Key: daily-update RequestId: 1HJ94B4WRTBC18CD HostId: 6wLY9Hx1qOLbtb4wo24DslqbbBJ6Ax+YjnmGYSEQ6R2QDyg7ElEV7Z9Uu3Spy7EHw6I6ILk3hXw=
    
    this is an angular app if I enter the path https://rating-stage.keshet-tv.com/ it opens ok and switch to daily update but on entering straightly to https://rating-stage.keshet-tv.com/daily-update it fails with 404
    
    import { Routes } from '@angular/router'; import { DailyBroadcastEventsComponent } from './pages/daily-broadcast-events/daily-broadcast-events.component'; import { DistributionComponent } from './pages/distribution/distribution.component'; import { AuthGuard } from '../shared/guards/auth.guard'; export const routes: Routes = [ { path: '', redirectTo: '/daily-update', pathMatch: 'full' }, { path: 'daily-update', component: DailyBroadcastEventsComponent, canActivate: [AuthGuard], }, { path: 'distribution', component: DistributionComponent, canActivate: [AuthGuard], }, ];
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0286-prompt"
  SourcePath: "claude-export/batch-0000/e347a9a5-75c7-453f-a9be-374012835221/messages/019b2cbf-8c40-73d3-a59b-2ff6e251c082"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    I have an error when accessing the site hosted on s3 404 Not Found Code: NoSuchKey Message: The specified key does not exist. Key: daily-update RequestId: 1HJ94B4WRTBC18CD HostId: 6wLY9Hx1qOLbtb4wo24DslqbbBJ6Ax+YjnmGYSEQ6R2QDyg7ElEV7Z9Uu3Spy7EHw6I6ILk3hXw=
    
    this is an angular app if I enter the path https://rating-stage.keshet-tv.com/ it opens ok and switch to daily update but on entering straightly to https://rating-stage.keshet-tv.com/daily-update it fails with 404
    
    import { Routes } from '@angular/router'; import { DailyBroadcastEventsComponent } from './pages/daily-broadcast-events/daily-broadcast-events.component'; import { DistributionComponent } from './pages/distribution/distribution.component'; import { AuthGuard } from '../shared/guards/auth.guard'; export const routes: Routes = [ { path: '', redirectTo: '/daily-update', pathMatch: 'full' }, { path: 'daily-update', component: DailyBroadcastEventsComponent, canActivate: [AuthGuard], }, { path: 'distribution', component: DistributionComponent, canActivate: [AuthGuard], }, ];
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "IMPLEMENT", "SECURITY", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:request
  - DOCUMENT:spec
  - IMPLEMENT:code
  - IMPLEMENT:error
  - SECURITY:auth

---

## XIIGEN-SESSION-1000-0287 - Debugging API connectivity with command line tools

```
source_id: xiigen-session-1000-0287
source_title: "Debugging API connectivity with command line tools"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: repair
domain: BUG
phase: 3
sub_phase: 3.3
task_type: api-contract
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "7d87936b-cd34-47f1-b6b0-f02035b6b23b"
  conversation_name: "Debugging API connectivity with command line tools"
  human_message_uuid: "019bffc3-0071-700f-b000-102290c207c0"
  human_message_index: 0
  message_created_at: "2026-01-27T14:02:08.400853Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0287"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "how to debug with cmd or powershell connection to specific api - if it is blocked or not?"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0287-prompt"
  SourcePath: "claude-export/batch-0000/7d87936b-cd34-47f1-b6b0-f02035b6b23b/messages/019bffc3-0071-700f-b000-102290c207c0"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: "how to debug with cmd or powershell connection to specific api - if it is blocked or not?"
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "IMPLEMENT", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:api
  - DOCUMENT:spec
  - IMPLEMENT:bug

---

## XIIGEN-SESSION-1000-0288 - Debugging API connectivity with command line tools

```
source_id: xiigen-session-1000-0288
source_title: "Debugging API connectivity with command line tools"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: repair
domain: BUG
phase: 3
sub_phase: 3.3
task_type: api-contract
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "7d87936b-cd34-47f1-b6b0-f02035b6b23b"
  conversation_name: "Debugging API connectivity with command line tools"
  human_message_uuid: "019bffc4-f552-77c9-b438-115adbedbc33"
  human_message_index: 2
  message_created_at: "2026-01-27T14:04:16.627962Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0288"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    Invoke-WebRequest -Uri "https://api-notifications.keshet.dom/api/PushNotifications" -Method POST
    Invoke-WebRequest : {"type":"https://tools.ietf.org/html/rfc7231#section-6.5.13","title":"Unsupported Media
    Type","status":415,"traceId":"00-01920b91092740b8e1ee8ec099e30832-de12f09131893e61-00"}
    At line:1 char:1
    + Invoke-WebRequest -Uri "https://api-notifications.keshet.dom/api/Push ...
    + ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        + CategoryInfo          : InvalidOperation: (System.Net.HttpWebRequest:HttpWebRequest) [Invoke-WebRequest], WebExc
       eption
        + FullyQualifiedErrorId : WebCmdletWebResponseException,Microsoft.PowerShell.Commands.InvokeWebRequestCommand
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0288-prompt"
  SourcePath: "claude-export/batch-0000/7d87936b-cd34-47f1-b6b0-f02035b6b23b/messages/019bffc4-f552-77c9-b438-115adbedbc33"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    Invoke-WebRequest -Uri "https://api-notifications.keshet.dom/api/PushNotifications" -Method POST
    Invoke-WebRequest : {"type":"https://tools.ietf.org/html/rfc7231#section-6.5.13","title":"Unsupported Media
    Type","status":415,"traceId":"00-01920b91092740b8e1ee8ec099e30832-de12f09131893e61-00"}
    At line:1 char:1
    + Invoke-WebRequest -Uri "https://api-notifications.keshet.dom/api/Push ...
    + ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        + CategoryInfo          : InvalidOperation: (System.Net.HttpWebRequest:HttpWebRequest) [Invoke-WebRequest], WebExc
       eption
        + FullyQualifiedErrorId : WebCmdletWebResponseException,Microsoft.PowerShell.Commands.InvokeWebRequestCommand
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:api
  - API_CONTRACT:request
  - API_CONTRACT:response
  - IMPLEMENT:error

---

## XIIGEN-SESSION-1000-0289 - Debugging API connectivity with command line tools

```
source_id: xiigen-session-1000-0289
source_title: "Debugging API connectivity with command line tools"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: qa_release
domain: CODE_Q
phase: 3
sub_phase: 3.3
task_type: api-contract
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "7d87936b-cd34-47f1-b6b0-f02035b6b23b"
  conversation_name: "Debugging API connectivity with command line tools"
  human_message_uuid: "019bffc6-5b58-7188-b6fd-b3f306b88b05"
  human_message_index: 4
  message_created_at: "2026-01-27T14:05:48.304030Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0289"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    how do I create a request with a body:
    option.body
    {   applicationName: "spotify",   message: "דוח נתונים לתאריכים 24/1/2026 - 26/1/2026",   subMessage: "יש ללחוץ כאן להורדה",   icon: "database",   link: "https://s3.amazonaws.com/kmedia.keshet-tv.com/spotifat/data_reports/flatData_27_1_2026_16_3_0_undef…,   isAbsolute: true,   isExternal: true,   level: "USER_ONLY", }
    in powershell?
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0289-prompt"
  SourcePath: "claude-export/batch-0000/7d87936b-cd34-47f1-b6b0-f02035b6b23b/messages/019bffc6-5b58-7188-b6fd-b3f306b88b05"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    how do I create a request with a body:
    option.body
    {   applicationName: "spotify",   message: "דוח נתונים לתאריכים 24/1/2026 - 26/1/2026",   subMessage: "יש ללחוץ כאן להורדה",   icon: "database",   link: "https://s3.amazonaws.com/kmedia.keshet-tv.com/spotifat/data_reports/flatData_27_1_2026_16_3_0_undef…,   isAbsolute: true,   isExternal: true,   level: "USER_ONLY", }
    in powershell?
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:request

---

## XIIGEN-SESSION-1000-0290 - Debugging API connectivity with command line tools

```
source_id: xiigen-session-1000-0290
source_title: "Debugging API connectivity with command line tools"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: repair
domain: BUG
phase: 3
sub_phase: 3.3
task_type: api-contract
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "7d87936b-cd34-47f1-b6b0-f02035b6b23b"
  conversation_name: "Debugging API connectivity with command line tools"
  human_message_uuid: "019bffda-40eb-743e-8de1-822301556b3d"
  human_message_index: 8
  message_created_at: "2026-01-27T14:27:32.226990Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0290"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "some specific app can't make same request that works from powershell, how can I debug it, no event is called, no log is created"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0290-prompt"
  SourcePath: "claude-export/batch-0000/7d87936b-cd34-47f1-b6b0-f02035b6b23b/messages/019bffda-40eb-743e-8de1-822301556b3d"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: "some specific app can't make same request that works from powershell, how can I debug it, no event is called, no log is created"
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "IMPLEMENT", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:request
  - DOCUMENT:spec
  - IMPLEMENT:bug
  - IMPLEMENT:log

---

## XIIGEN-SESSION-1000-0291 - Debugging API connectivity with command line tools

```
source_id: xiigen-session-1000-0291
source_title: "Debugging API connectivity with command line tools"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: repair
domain: BUG
phase: 4
sub_phase: 4.1
task_type: backend-services
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "7d87936b-cd34-47f1-b6b0-f02035b6b23b"
  conversation_name: "Debugging API connectivity with command line tools"
  human_message_uuid: "019bffdd-9640-70a2-bc11-7187350f15c7"
  human_message_index: 12
  message_created_at: "2026-01-27T14:31:10.682952Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0291"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "I am running through iis, how can I run it in debug?"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0291-prompt"
  SourcePath: "claude-export/batch-0000/7d87936b-cd34-47f1-b6b0-f02035b6b23b/messages/019bffdd-9640-70a2-bc11-7187350f15c7"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Backend Services"
  CompiledText: "I am running through iis, how can I run it in debug?"
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - IMPLEMENT:bug

---

## XIIGEN-SESSION-1000-0292 - Resolving NestJS dependency version conflicts

```
source_id: xiigen-session-1000-0292
source_title: "Resolving NestJS dependency version conflicts"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: repair
domain: BUG
phase: 3
sub_phase: 3.3
task_type: api-contract
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "6e106b95-1f19-47c1-9242-7e1af73eeb6d"
  conversation_name: "Resolving NestJS dependency version conflicts"
  human_message_uuid: "019b2c5c-d46a-7602-9564-6a9e4da05b77"
  human_message_index: 0
  message_created_at: "2025-12-17T12:50:40.041172Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0292"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    Please examine my package.json and the build errors and suggest the best versions 
    combination:
    
    ```json
    {
      "name": "instagram-analytics-jobs",
      "version": "1.0.0",
      "description": "Social Media Analytics Job Processing System",
      "main": "dist/index.js",
      "scripts": {
        "build": "tsc",
        "start": "node dist/index.js",
        "test": "jest",
        "test:watch": "jest --watch",
        "test:e2e": "jest --config ./test/jest-e2e.json",
        "lint": "eslint . --ext .ts"
      },
      "dependencies": {
        "@nestjs/common": "^9.0.0",
        "@nestjs/core": "^9.0.0",
        "@nestjs/config": "^3.0.0",
        "@nestjs/axios": "^3.0.0",
        "@nestjs/swagger": "^6.3.0",
        "@nestjs/platform-express": "^9.0.0",
        "@elastic/elasticsearch": "^8.11.0",
        "axios": "^1.6.0",
        "class-validator": "^0.14.0",
        "class-transformer": "^0.5.1",
        "reflect-metadata": "^0.1.13",
        "rxjs": "^7.8.1"
      },
      "devDependencies": {
        "@nestjs/cli": "^10.0.0",
        "@nestjs/schematics": "^10.0.0",
        "@nestjs/testing": "^10.0.0",
        "@types/jest": "^29.5.0",
        "@types/node": "^20.0.0",
        "@typescript-eslint/eslint-plugin": "^6.0.0",
        "@typescript-eslint/parser": "^6.0.0",
        "eslint": "^8.42.0",
        "jest": "^29.5.0",
        "ts-jest": "^29.1.0",
        "ts-node": "^10.9.1",
        "typescript": "^5.1.3"
      },
      "jest": {
        "moduleFileExtensions": ["js", "json", "ts"],
        "rootDir": "src",
        "testRegex": ".*\\.spec\\.ts$",
        "transform": {
          "^.+\\.(t|j)s$": "ts-jest"
        },
        "collectCoverageFrom": [
          "**/*.(t|j)s"
        ],
        "coverageDirectory": "../coverage",
        "testEnvironment": "node"
      }
    }
    
    ```
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0292-prompt"
  SourcePath: "claude-export/batch-0000/6e106b95-1f19-47c1-9242-7e1af73eeb6d/messages/019b2c5c-d46a-7602-9564-6a9e4da05b77"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    Please examine my package.json and the build errors and suggest the best versions 
    combination:
    
    ```json
    {
      "name": "instagram-analytics-jobs",
      "version": "1.0.0",
      "description": "Social Media Analytics Job Processing System",
      "main": "dist/index.js",
      "scripts": {
        "build": "tsc",
        "start": "node dist/index.js",
        "test": "jest",
        "test:watch": "jest --watch",
        "test:e2e": "jest --config ./test/jest-e2e.json",
        "lint": "eslint . --ext .ts"
      },
      "dependencies": {
        "@nestjs/common": "^9.0.0",
        "@nestjs/core": "^9.0.0",
        "@nestjs/config": "^3.0.0",
        "@nestjs/axios": "^3.0.0",
        "@nestjs/swagger": "^6.3.0",
        "@nestjs/platform-express": "^9.0.0",
        "@elastic/elasticsearch": "^8.11.0",
        "axios": "^1.6.0",
        "class-validator": "^0.14.0",
        "class-transformer": "^0.5.1",
        "reflect-metadata": "^0.1.13",
        "rxjs": "^7.8.1"
      },
      "devDependencies": {
        "@nestjs/cli": "^10.0.0",
        "@nestjs/schematics": "^10.0.0",
        "@nestjs/testing": "^10.0.0",
        "@types/jest": "^29.5.0",
        "@types/node": "^20.0.0",
        "@typescript-eslint/eslint-plugin": "^6.0.0",
        "@typescript-eslint/parser": "^6.0.0",
        "eslint": "^8.42.0",
        "jest": "^29.5.0",
        "ts-jest": "^29.1.0",
        "ts-node": "^10.9.1",
        "typescript": "^5.1.3"
      },
      "jest": {
        "moduleFileExtensions": ["js", "json", "ts"],
        "rootDir": "src",
        "testRegex": ".*\\.spec\\.ts$",
        "transform": {
          "^.+\\.(t|j)s$": "ts-jest"
        },
        "collectCoverageFrom": [
          "**/*.(t|j)s"
        ],
        "coverageDirectory": "../coverage",
        "testEnvironment": "node"
      }
    }
    
    ```
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "IMPLEMENT", "TEST", "UI"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:schema
  - DOCUMENT:spec
  - IMPLEMENT:error
  - TEST:test
  - TEST:testing
  - TEST:coverage
  - UI:ui

---

## XIIGEN-SESSION-1000-0293 - Resolving NestJS dependency version conflicts

```
source_id: xiigen-session-1000-0293
source_title: "Resolving NestJS dependency version conflicts"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: repair
domain: BUG
phase: 3
sub_phase: 3.1
task_type: architecture-topology
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "6e106b95-1f19-47c1-9242-7e1af73eeb6d"
  conversation_name: "Resolving NestJS dependency version conflicts"
  human_message_uuid: "019b2c5e-a58e-7593-99b3-f01178c7876d"
  human_message_index: 2
  message_created_at: "2025-12-17T12:52:38.990452Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0293"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    I don't want to use legacy peer I want to fix the problem
    
    npm install
    ⠹Warning: Ignoring extra certs from `C:\Users\luba.karpenko\Documents\Projects\kst.spotify.site\certs\cert.crt`, load failed: error:80000003:system library::No such process
    npm error code ERESOLVE
    npm error ERESOLVE unable to resolve dependency tree
    npm error
    npm error While resolving: instagram-analytics-jobs@1.0.0
    npm error Found: @nestjs/common@9.4.3
    npm error node_modules/@nestjs/common
    npm error   @nestjs/common@"^9.0.0" from the root project
    npm error
    npm error Could not resolve dependency:
    npm error peer @nestjs/common@"^10.0.0" from @nestjs/testing@10.4.20
    npm error node_modules/@nestjs/testing
    npm error   dev @nestjs/testing@"^10.0.0" from the root project
    npm error
    npm error Fix the upstream dependency conflict, or retry
    npm error this command with --force or --legacy-peer-deps
    npm error to accept an incorrect (and potentially broken) dependency resolution.
    npm error
    npm error
    npm error For a full report see:
    npm error C:\Users\luba.karpenko\AppData\Local\npm-cache\_logs\2025-12-17T12_51_53_958Z-eresolve-report.txt
    npm error A complete log of this run can be found in: C:\Users\luba.karpenko\AppData\Local\npm-cache\_logs\2025-12-17T12_51_53_958Z-debug-0.log
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0293-prompt"
  SourcePath: "claude-export/batch-0000/6e106b95-1f19-47c1-9242-7e1af73eeb6d/messages/019b2c5e-a58e-7593-99b3-f01178c7876d"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Architecture and Component Topology"
  CompiledText: |
    I don't want to use legacy peer I want to fix the problem
    
    npm install
    ⠹Warning: Ignoring extra certs from `C:\Users\luba.karpenko\Documents\Projects\kst.spotify.site\certs\cert.crt`, load failed: error:80000003:system library::No such process
    npm error code ERESOLVE
    npm error ERESOLVE unable to resolve dependency tree
    npm error
    npm error While resolving: instagram-analytics-jobs@1.0.0
    npm error Found: @nestjs/common@9.4.3
    npm error node_modules/@nestjs/common
    npm error   @nestjs/common@"^9.0.0" from the root project
    npm error
    npm error Could not resolve dependency:
    npm error peer @nestjs/common@"^10.0.0" from @nestjs/testing@10.4.20
    npm error node_modules/@nestjs/testing
    npm error   dev @nestjs/testing@"^10.0.0" from the root project
    npm error
    npm error Fix the upstream dependency conflict, or retry
    npm error this command with --force or --legacy-peer-deps
    npm error to accept an incorrect (and potentially broken) dependency resolution.
    npm error
    npm error
    npm error For a full report see:
    npm error C:\Users\luba.karpenko\AppData\Local\npm-cache\_logs\2025-12-17T12_51_53_958Z-eresolve-report.txt
    npm error A complete log of this run can be found in: C:\Users\luba.karpenko\AppData\Local\npm-cache\_logs\2025-12-17T12_51_53_958Z-debug-0.log
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - ARCHITECTURE:project
  - IMPLEMENT:code
  - IMPLEMENT:fix
  - IMPLEMENT:bug
  - IMPLEMENT:error
  - IMPLEMENT:failed
  - IMPLEMENT:log
  - TEST:test
  - TEST:testing

---

## XIIGEN-SESSION-1000-0294 - Windows Server 2019 image retirement error

```
source_id: xiigen-session-1000-0294
source_title: "Windows Server 2019 image retirement error"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: repair
domain: BUG
phase: 3
sub_phase: 3.3
task_type: api-contract
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "a5eb6b21-ad01-4b01-851b-f3d3336dad52"
  conversation_name: "Windows Server 2019 image retirement error"
  human_message_uuid: "019bfa91-fdd5-7086-9916-77652179aa1e"
  human_message_index: 0
  message_created_at: "2026-01-26T13:50:30.377047Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0294"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    I have an error when deploying something on azure devops
    
    ```vsts-dtc-log-live
    ##[Error 1]
    Windows Server 2019 images are now retired. For more details, see https://devblogs.microsoft.com/devops/upcoming-updates-for-azure-pipelines-agents-images 
    
    ##[Error 2]
    The remote provider was unable to process the request. 
    
    ##[Error 3]
    Windows Server 2019 images are now retired. For more details, see https://devblogs.microsoft.com/devops/upcoming-updates-for-azure-pipelines-agents-images 
    ```
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0294-prompt"
  SourcePath: "claude-export/batch-0000/a5eb6b21-ad01-4b01-851b-f3d3336dad52/messages/019bfa91-fdd5-7086-9916-77652179aa1e"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    I have an error when deploying something on azure devops
    
    ```vsts-dtc-log-live
    ##[Error 1]
    Windows Server 2019 images are now retired. For more details, see https://devblogs.microsoft.com/devops/upcoming-updates-for-azure-pipelines-agents-images 
    
    ##[Error 2]
    The remote provider was unable to process the request. 
    
    ##[Error 3]
    Windows Server 2019 images are now retired. For more details, see https://devblogs.microsoft.com/devops/upcoming-updates-for-azure-pipelines-agents-images 
    ```
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:request
  - IMPLEMENT:provider
  - IMPLEMENT:error
  - IMPLEMENT:log

---

## XIIGEN-SESSION-1000-0295 - Fourth implementation prompt review

```
source_id: xiigen-session-1000-0295
source_title: "Fourth implementation prompt review"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
domain: CODE_Q
phase: 3
sub_phase: 3.1
task_type: architecture-topology
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "bdad4f94-dd20-40b3-91a2-a6dd5a2dfcf9"
  conversation_name: "Fourth implementation prompt review"
  human_message_uuid: "019b2aab-b4ef-75c6-b50c-433a6a70e372"
  human_message_index: 7
  message_created_at: "2025-12-17T04:57:34.884569Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0295"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "This is my previous step implementation - please update the project with required changes of get comments following \"get_comments_data_impl\" and return an updated zip file contains all the required files"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0295-prompt"
  SourcePath: "claude-export/batch-0000/bdad4f94-dd20-40b3-91a2-a6dd5a2dfcf9/messages/019b2aab-b4ef-75c6-b50c-433a6a70e372"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Architecture and Component Topology"
  CompiledText: "This is my previous step implementation - please update the project with required changes of get comments following \"get_comments_data_impl\" and return an updated zip file contains all the required files"
  CompiledPredicateIds: ["ARCHITECTURE", "UI"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - ARCHITECTURE:project
  - UI:ui

---

## XIIGEN-SESSION-1000-0296 - Fourth implementation prompt review

```
source_id: xiigen-session-1000-0296
source_title: "Fourth implementation prompt review"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
domain: CODE_Q
phase: 3
sub_phase: 3.1
task_type: architecture-topology
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "bdad4f94-dd20-40b3-91a2-a6dd5a2dfcf9"
  conversation_name: "Fourth implementation prompt review"
  human_message_uuid: "019b2aae-6fcf-7707-adc3-a82a8385ec3d"
  human_message_index: 9
  message_created_at: "2025-12-17T05:00:33.745968Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0296"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "This is my previous step implementation - please update the project with required changes of get comments following \"4th step result 3 2.zip\" and \"4th step result 3.md\" and return an updated zip file contains all the required files"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0296-prompt"
  SourcePath: "claude-export/batch-0000/bdad4f94-dd20-40b3-91a2-a6dd5a2dfcf9/messages/019b2aae-6fcf-7707-adc3-a82a8385ec3d"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Architecture and Component Topology"
  CompiledText: "This is my previous step implementation - please update the project with required changes of get comments following \"4th step result 3 2.zip\" and \"4th step result 3.md\" and return an updated zip file contains all the required files"
  CompiledPredicateIds: ["ARCHITECTURE", "UI"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - ARCHITECTURE:project
  - UI:ui

---

## XIIGEN-SESSION-1000-0297 - Fourth implementation prompt review

```
source_id: xiigen-session-1000-0297
source_title: "Fourth implementation prompt review"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: repair
domain: BUG
phase: 3
sub_phase: 3.3
task_type: api-contract
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "bdad4f94-dd20-40b3-91a2-a6dd5a2dfcf9"
  conversation_name: "Fourth implementation prompt review"
  human_message_uuid: "019b2ab4-bec7-72e5-b10d-242ea7bd7937"
  human_message_index: 11
  message_created_at: "2025-12-17T05:07:27.189514Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0297"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    npm run build
    > instagram-analytics-jobs@1.0.0 build
    > tsc
    src/jobs/config/plans/get-post-data.plan.ts:67:7 - error TS2353: Object literal may only specify known properties, and 'skipCondition' does not exist in type 'EnrichmentSpec'.
    67       skipCondition: {
             ~~~~~~~~~~~~~
    src/jobs/config/plans/get-post-data.plan.ts:69:36 - error TS2339: Property 'EXISTS' does not exist on type 'typeof FilterOperation'.
    69         operation: FilterOperation.EXISTS,
                                          ~~~~~~
    Found 2 errors in the same file, starting at: src/jobs/config/plans/get-post-data.plan.ts:67
    PS C:\Users\luba.karpenko\Documents\Projects\kst.social.temp.api> npm run build
    > instagram-analytics-jobs@1.0.0 build
    > tsc
    PS C:\Users\luba.karpenko\Documents\Projects\kst.social.temp.api> npm run build
    > instagram-analytics-jobs@1.0.0 build
    > tsc
    src/jobs/config/plans/get-comments-data.plan.ts:78:27 - error TS2339: Property 'FLATTEN_COMMENTS' does not exist on type 'typeof TransformType'.
    78       type: TransformType.FLATTEN_COMMENTS,
                                 ~~~~~~~~~~~~~~~~
    Found 1 error in src/jobs/config/plans/get-comments-data.plan.ts:78
    PS C:\Users\luba.karpenko\Documents\Projects\kst.social.temp.api>
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0297-prompt"
  SourcePath: "claude-export/batch-0000/bdad4f94-dd20-40b3-91a2-a6dd5a2dfcf9/messages/019b2ab4-bec7-72e5-b10d-242ea7bd7937"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    npm run build
    > instagram-analytics-jobs@1.0.0 build
    > tsc
    src/jobs/config/plans/get-post-data.plan.ts:67:7 - error TS2353: Object literal may only specify known properties, and 'skipCondition' does not exist in type 'EnrichmentSpec'.
    67       skipCondition: {
             ~~~~~~~~~~~~~
    src/jobs/config/plans/get-post-data.plan.ts:69:36 - error TS2339: Property 'EXISTS' does not exist on type 'typeof FilterOperation'.
    69         operation: FilterOperation.EXISTS,
                                          ~~~~~~
    Found 2 errors in the same file, starting at: src/jobs/config/plans/get-post-data.plan.ts:67
    PS C:\Users\luba.karpenko\Documents\Projects\kst.social.temp.api> npm run build
    > instagram-analytics-jobs@1.0.0 build
    > tsc
    PS C:\Users\luba.karpenko\Documents\Projects\kst.social.temp.api> npm run build
    > instagram-analytics-jobs@1.0.0 build
    > tsc
    src/jobs/config/plans/get-comments-data.plan.ts:78:27 - error TS2339: Property 'FLATTEN_COMMENTS' does not exist on type 'typeof TransformType'.
    78       type: TransformType.FLATTEN_COMMENTS,
                                 ~~~~~~~~~~~~~~~~
    Found 1 error in src/jobs/config/plans/get-comments-data.plan.ts:78
    PS C:\Users\luba.karpenko\Documents\Projects\kst.social.temp.api>
  CompiledPredicateIds: ["API_CONTRACT", "ARCHITECTURE", "DOCUMENT", "IMPLEMENT", "TEST", "UI"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:api
  - ARCHITECTURE:project
  - DOCUMENT:spec
  - IMPLEMENT:error
  - UI:ui

---

## XIIGEN-SESSION-1000-0298 - Design patterns для начинающих

```
source_id: xiigen-session-1000-0298
source_title: "Design patterns для начинающих"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
domain: CODE_Q
phase: 4
sub_phase: 4.2
task_type: frontend-client-integration
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "20f46922-ae37-4c93-981c-75e628c6a640"
  conversation_name: "Design patterns для начинающих"
  human_message_uuid: "019c000d-6f64-7585-a0cf-f727fb662497"
  human_message_index: 6
  message_created_at: "2026-01-27T15:23:26.491399Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0298"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "please add more Builder, Prototype, Command, State - and other maybe rarer..."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0298-prompt"
  SourcePath: "claude-export/batch-0000/20f46922-ae37-4c93-981c-75e628c6a640/messages/019c000d-6f64-7585-a0cf-f727fb662497"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Frontend and Client Integration"
  CompiledText: "please add more Builder, Prototype, Command, State - and other maybe rarer..."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - UI:ui

---

## XIIGEN-SESSION-1000-0299 - Изучение Next.js с нуля

```
source_id: xiigen-session-1000-0299
source_title: "Изучение Next.js с нуля"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
domain: CODE_Q
phase: 4
sub_phase: 4.2
task_type: frontend-client-integration
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "731c0e83-d67f-4227-aafd-f99a6e6b2d07"
  conversation_name: "Изучение Next.js с нуля"
  human_message_uuid: "019bf9f3-7746-768b-9aae-6e5f8b0fae0b"
  human_message_index: 8
  message_created_at: "2026-01-26T10:57:21.256066Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0299"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "погоди ты во первых не сделал то же на react и на next во вторых слишком простая реализация на react - нет redux  или что то более новое"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0299-prompt"
  SourcePath: "claude-export/batch-0000/731c0e83-d67f-4227-aafd-f99a6e6b2d07/messages/019bf9f3-7746-768b-9aae-6e5f8b0fae0b"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Frontend and Client Integration"
  CompiledText: "погоди ты во первых не сделал то же на react и на next во вторых слишком простая реализация на react - нет redux  или что то более новое"
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - UI:ux

---

## XIIGEN-SESSION-1000-0300 - Code documentation validation and UI specification

```
source_id: xiigen-session-1000-0300
source_title: "Code documentation validation and UI specification"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: external_source_integration
domain: PLAN
phase: 4
sub_phase: 4.2
task_type: frontend-client-integration
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "182b9f56-dc8a-4b25-8b7a-2208f23c3e8f"
  conversation_name: "Code documentation validation and UI specification"
  human_message_uuid: "019bffa6-80c4-74ab-a96e-be99a4d0806a"
  human_message_index: 0
  message_created_at: "2026-01-27T13:31:00.722656Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0300"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Please go over the code \"site*.zip\" and documentation \"spotifat-site-documentation*.zip\", and validate that the documentation contains all the code (source of truth!) which was implemented the correct way. Additionaly - add a file with all ui screens, user scenarios, participating controls:name, meaning, type, id, validations, possible values, code reference: file name, controller name)  Add filters, and validations possible actions as well 1. Please add business logic, when and from where it applied. Please save states to be able to continue from where you stopped"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0300-prompt"
  SourcePath: "claude-export/batch-0000/182b9f56-dc8a-4b25-8b7a-2208f23c3e8f/messages/019bffa6-80c4-74ab-a96e-be99a4d0806a"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Frontend and Client Integration"
  CompiledText: "Please go over the code \"site*.zip\" and documentation \"spotifat-site-documentation*.zip\", and validate that the documentation contains all the code (source of truth!) which was implemented the correct way. Additionaly - add a file with all ui screens, user scenarios, participating controls:name, meaning, type, id, validations, possible values, code reference: file name, controller name)  Add filters, and validations possible actions as well 1. Please add business logic, when and from where it applied. Please save states to be able to continue from where you stopped"
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT", "TEST", "UI"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - DOCUMENT:documentation
  - IMPLEMENT:code
  - IMPLEMENT:log
  - TEST:validation
  - UI:ui
  - UI:screen

---

