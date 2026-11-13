# Layer 0-A - Compiler Pairs (004)
# Batch: XIIGen session export 1000 oldest-first | pair_kind: compiler
# Source: Claude export batch-0000; first-party private

---

## XIIGEN-SESSION-1000-0151 - Employee task summary report

```
source_id: xiigen-session-1000-0151
source_title: "Employee task summary report"
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
  conversation_uuid: "276726ac-10f1-4ba6-b8f6-5b485c202d2d"
  conversation_name: "Employee task summary report"
  human_message_uuid: "8a18d519-ac4f-47a8-b2cd-26ee1874396d"
  human_message_index: 12
  message_created_at: "2025-10-26T12:58:30.019881Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0151"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    No need in double information - group by 
    Commits:
    * Tom Robissa (2025-10-23 09:00): bug-fix #9464 #9479 snackbar same as contracts system (+0 ~2 -0)
    * Tom Robissa (2025-10-23 09:00): bug-fix #9464 #9479 snackbar same as contracts system (+0 ~2 -0)
    * Tom Robissa (2025-10-23 09:00): bug-fix #9464 #9479 snackbar same as contracts system (+0 ~2 -0)
    * Tom Robissa (2025-10-23 09:00): bug-fix #9464 #9479 snackbar same as contracts system (+0 ~2 -0)
    * Tom Robissa (2025-10-23 09:00): bug-fix #9464 #9479 snackbar same as contracts system (+0 ~2 -0)
    * Tom Robissa (2025-10-23 09:00): bug-fix #9464 #9479 snackbar same as contracts system (+0 ~2 -0)
    * Tom Robissa (2025-10-23 09:00): bug-fix #9464 #9479 snackbar same as contracts system (+0 ~2 -0)
    * Tom Robissa (2025-10-23 09:00): bug-fix #9464 #9479 snackbar same as contracts system (+0 ~2 -0)
    * Tom Robissa (2025-10-23 09:00): bug-fix #9464 #9479 snackbar same as contracts system (+0 ~2 -0)
    * Tom Robissa (2025-10-23 09:00): bug-fix #9464 #9479 snackbar same as contracts system (+0 ~2 -0)
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0151-prompt"
  SourcePath: "claude-export/batch-0000/276726ac-10f1-4ba6-b8f6-5b485c202d2d/messages/8a18d519-ac4f-47a8-b2cd-26ee1874396d"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    No need in double information - group by 
    Commits:
    * Tom Robissa (2025-10-23 09:00): bug-fix #9464 #9479 snackbar same as contracts system (+0 ~2 -0)
    * Tom Robissa (2025-10-23 09:00): bug-fix #9464 #9479 snackbar same as contracts system (+0 ~2 -0)
    * Tom Robissa (2025-10-23 09:00): bug-fix #9464 #9479 snackbar same as contracts system (+0 ~2 -0)
    * Tom Robissa (2025-10-23 09:00): bug-fix #9464 #9479 snackbar same as contracts system (+0 ~2 -0)
    * Tom Robissa (2025-10-23 09:00): bug-fix #9464 #9479 snackbar same as contracts system (+0 ~2 -0)
    * Tom Robissa (2025-10-23 09:00): bug-fix #9464 #9479 snackbar same as contracts system (+0 ~2 -0)
    * Tom Robissa (2025-10-23 09:00): bug-fix #9464 #9479 snackbar same as contracts system (+0 ~2 -0)
    * Tom Robissa (2025-10-23 09:00): bug-fix #9464 #9479 snackbar same as contracts system (+0 ~2 -0)
    * Tom Robissa (2025-10-23 09:00): bug-fix #9464 #9479 snackbar same as contracts system (+0 ~2 -0)
    * Tom Robissa (2025-10-23 09:00): bug-fix #9464 #9479 snackbar same as contracts system (+0 ~2 -0)
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:contract
  - IMPLEMENT:fix
  - IMPLEMENT:bug

---

## XIIGEN-SESSION-1000-0152 - Employee activity summary report

```
source_id: xiigen-session-1000-0152
source_title: "Employee activity summary report"
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
  conversation_uuid: "660bf959-24f9-40bc-9008-424c7882cbbf"
  conversation_name: "Employee activity summary report"
  human_message_uuid: "c7668030-6810-4d94-b9ef-1761bf4b6b52"
  human_message_index: 0
  message_created_at: "2025-10-24T13:16:57.052068Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0152"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    Please make a summary 
    for each employee what has been done: 
    Which tasks he have been doing 
    for each task - what is the task about (area, title, description) short ,what was previous status, what is the latest status,  if there are comments - please write them down, if there are code or pr connected, give a link - short information, just the important one. Do not count the change in fields as activity, just a status change
    No need to display all the status changes - just the one at the beginning and at the end
    do not cut comments
    No need:
    Status Changes:
    * 2025-10-22 08:14: Active → Resolved (initial)
    * 2025-10-22 08:14: Active → Resolved (final)
    * 2025-10-22 12:24: Active → Closed (initial)
    * ... (3 intermediate changes)
    * 2025-10-22 13:52: Active → Closed (final)
    Just the previous and current
    If there is no description no need to add it - but if there is a parent with description in related complete it from the parent
    Add section of related tasks with numbers and their relation type
    add to the childs - title, current status
    example to successful report format: # Employee Activity Summary Report
    Generated: 2025-10-24 13:11:52
    ================================================================================
    
    ## Arik Almog
    --------------------------------------------------------------------------------
    
    ### Task #7146: תוספות למסך שידורים
    **Area:** Keshet\Keshet TV
    **Type:** Feature
    
    **Previous Status:** Active
    **Current Status:** Resolved
    
    **Related Tasks:**
    - #9324 (Child) - תוספות תמהיל אורכים למסך שידורים | Status: Resolved
    - #9412 (Child) - עדכון חישוב מיקומים 1-3 99 והוספת אחוז מיקומים | Status: Resolved
    - #9413 (Child) - מסך שידורים להזמנות לוח: להסיר עמודות לא בשימוש | Status: Resolved
    
    **Task URL:** https://dev.azure.com/KESHETDEV/Keshet/_workitems/edit/7146
    
    ------------------------------------------------------------
    
    ## Jacob Ychye
    --------------------------------------------------------------------------------
    
    ### Task #9484: הסכמים - ביטוח - היסטוריית התראות - התראה מוצגת ללא טקסט
    **Area:** Keshet\Contracts
    **Type:** Bug
    
    **Previous Status:** Active
    **Current Status:** Resolved
    
    **Comments:**
    - Jacob Ychye (2025-10-22 11:10): הבעיה הייתה בפונקציה החדשה שכתבתי לשליחה של WEB SOCKET 
    הפונקציה גם הכניסה רשומות ל גDB 
    לטבלת [PushNotifications]
    
    השינוי בוצע בNOTIFICATION API 
    
    ועכשיו צריך לבדוק את הביטוחים ונשלח התראת WEB SOCKET כמו שצריך
    בכל שינוי שנעשה 
    וזה מתעדכן במסך השני ללא צורך ברענון המסך
    
    **Related Tasks:**
    - #9483 (Tested By) - ביטוח - היסטוריית התראות | Status: Design
    - #8281 (Parent) - תמלול ביטוח להסכם | Status: Resolved
    
    **Task URL:** https://dev.azure.com/KESHETDEV/Keshet/_workitems/edit/9484
    
    ------------------------------------------------------------
    
    ```ini
    
    ```
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0152-prompt"
  SourcePath: "claude-export/batch-0000/660bf959-24f9-40bc-9008-424c7882cbbf/messages/c7668030-6810-4d94-b9ef-1761bf4b6b52"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    Please make a summary 
    for each employee what has been done: 
    Which tasks he have been doing 
    for each task - what is the task about (area, title, description) short ,what was previous status, what is the latest status,  if there are comments - please write them down, if there are code or pr connected, give a link - short information, just the important one. Do not count the change in fields as activity, just a status change
    No need to display all the status changes - just the one at the beginning and at the end
    do not cut comments
    No need:
    Status Changes:
    * 2025-10-22 08:14: Active → Resolved (initial)
    * 2025-10-22 08:14: Active → Resolved (final)
    * 2025-10-22 12:24: Active → Closed (initial)
    * ... (3 intermediate changes)
    * 2025-10-22 13:52: Active → Closed (final)
    Just the previous and current
    If there is no description no need to add it - but if there is a parent with description in related complete it from the parent
    Add section of related tasks with numbers and their relation type
    add to the childs - title, current status
    example to successful report format: # Employee Activity Summary Report
    Generated: 2025-10-24 13:11:52
    ================================================================================
    
    ## Arik Almog
    --------------------------------------------------------------------------------
    
    ### Task #7146: תוספות למסך שידורים
    **Area:** Keshet\Keshet TV
    **Type:** Feature
    
    **Previous Status:** Active
    **Current Status:** Resolved
    
    **Related Tasks:**
    - #9324 (Child) - תוספות תמהיל אורכים למסך שידורים | Status: Resolved
    - #9412 (Child) - עדכון חישוב מיקומים 1-3 99 והוספת אחוז מיקומים | Status: Resolved
    - #9413 (Child) - מסך שידורים להזמנות לוח: להסיר עמודות לא בשימוש | Status: Resolved
    
    **Task URL:** https://dev.azure.com/KESHETDEV/Keshet/_workitems/edit/7146
    
    ------------------------------------------------------------
    
    ## Jacob Ychye
    --------------------------------------------------------------------------------
    
    ### Task #9484: הסכמים - ביטוח - היסטוריית התראות - התראה מוצגת ללא טקסט
    **Area:** Keshet\Contracts
    **Type:** Bug
    
    **Previous Status:** Active
    **Current Status:** Resolved
    
    **Comments:**
    - Jacob Ychye (2025-10-22 11:10): הבעיה הייתה בפונקציה החדשה שכתבתי לשליחה של WEB SOCKET 
    הפונקציה גם הכניסה רשומות ל גDB 
    לטבלת [PushNotifications]
    
    השינוי בוצע בNOTIFICATION API 
    
    ועכשיו צריך לבדוק את הביטוחים ונשלח התראת WEB SOCKET כמו שצריך
    בכל שינוי שנעשה 
    וזה מתעדכן במסך השני ללא צורך ברענון המסך
    
    **Related Tasks:**
    - #9483 (Tested By) - ביטוח - היסטוריית התראות | Status: Design
    - #8281 (Parent) - תמלול ביטוח להסכם | Status: Resolved
    
    **Task URL:** https://dev.azure.com/KESHETDEV/Keshet/_workitems/edit/9484
    
    ------------------------------------------------------------
    
    ```ini
    
    ```
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:api
  - API_CONTRACT:contract
  - IMPLEMENT:code
  - IMPLEMENT:bug
  - TEST:test

---

## XIIGEN-SESSION-1000-0153 - Employee activity summary report

```
source_id: xiigen-session-1000-0153
source_title: "Employee activity summary report"
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
  conversation_uuid: "660bf959-24f9-40bc-9008-424c7882cbbf"
  conversation_name: "Employee activity summary report"
  human_message_uuid: "aea501d9-6043-4b57-8dc1-51d00eefd89d"
  human_message_index: 2
  message_created_at: "2025-10-26T06:54:31.537092Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0153"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    in tasks that contain pr/commits can you bring the names for the prs/commits ? Maybe from task changes? 
    Task #9151: SITE - Add early distribution toggle from program page
    Area: Keshet\Distribution Team Type: Task
    Previous Status: Closed Current Status: Closed
    Code/PR Links:
    * vstfs:///Git/Commit/7a929425-38ed-4005-9b0a-d1196cd6b4e9%2f2c105bee-aabf-4940-ad30-bec3924f6483%2f6938a83e3fcde8cfe49e90af5da8b4600ac89cd8
    * vstfs:///Git/PullRequestId/7a929425-38ed-4005-9b0a-d1196cd6b4e9%2F2c105bee-aabf-4940-ad30-bec3924f6483%2F674
    Related Tasks:
    * #7832 (Parent) - אפשרות בחירה של הפצה מוקדמת בקטלוג ובעמוד תוכנית | Status: Closed
    Task URL: https://dev.azure.com/KESHETDEV/Keshet/_workitems/edit/9151
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0153-prompt"
  SourcePath: "claude-export/batch-0000/660bf959-24f9-40bc-9008-424c7882cbbf/messages/aea501d9-6043-4b57-8dc1-51d00eefd89d"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    in tasks that contain pr/commits can you bring the names for the prs/commits ? Maybe from task changes? 
    Task #9151: SITE - Add early distribution toggle from program page
    Area: Keshet\Distribution Team Type: Task
    Previous Status: Closed Current Status: Closed
    Code/PR Links:
    * vstfs:///Git/Commit/7a929425-38ed-4005-9b0a-d1196cd6b4e9%2f2c105bee-aabf-4940-ad30-bec3924f6483%2f6938a83e3fcde8cfe49e90af5da8b4600ac89cd8
    * vstfs:///Git/PullRequestId/7a929425-38ed-4005-9b0a-d1196cd6b4e9%2F2c105bee-aabf-4940-ad30-bec3924f6483%2F674
    Related Tasks:
    * #7832 (Parent) - אפשרות בחירה של הפצה מוקדמת בקטלוג ובעמוד תוכנית | Status: Closed
    Task URL: https://dev.azure.com/KESHETDEV/Keshet/_workitems/edit/9151
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:request
  - IMPLEMENT:code

---

## XIIGEN-SESSION-1000-0154 - Employee activity summary report

```
source_id: xiigen-session-1000-0154
source_title: "Employee activity summary report"
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
  conversation_uuid: "660bf959-24f9-40bc-9008-424c7882cbbf"
  conversation_name: "Employee activity summary report"
  human_message_uuid: "3036a32a-8715-4ff4-b961-f5ded9995299"
  human_message_index: 6
  message_created_at: "2025-10-26T13:02:11.050793Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0154"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    Task #9198: SITE - Add catalog filter early distribution programs
    Area: Keshet\Distribution Team Type: Task
    Previous Status: Closed Current Status: Closed
    Code/PR Links:
    * PR #674
    * Commit ae3e5c78
    //No comments in this section, why? You should have the data in commits file and connect the tasks by the commit Id and or work Item
    Related Tasks:
    * #7832 (Parent) - אפשרות בחירה של הפצה מוקדמת בקטלוג ובעמוד תוכנית | Status: Closed
    Task URL: https://dev.azure.com/KESHETDEV/Keshet/_workitems/edit/9198
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0154-prompt"
  SourcePath: "claude-export/batch-0000/660bf959-24f9-40bc-9008-424c7882cbbf/messages/3036a32a-8715-4ff4-b961-f5ded9995299"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Observability and Operational Design"
  CompiledText: |
    Task #9198: SITE - Add catalog filter early distribution programs
    Area: Keshet\Distribution Team Type: Task
    Previous Status: Closed Current Status: Closed
    Code/PR Links:
    * PR #674
    * Commit ae3e5c78
    //No comments in this section, why? You should have the data in commits file and connect the tasks by the commit Id and or work Item
    Related Tasks:
    * #7832 (Parent) - אפשרות בחירה של הפצה מוקדמת בקטלוג ובעמוד תוכנית | Status: Closed
    Task URL: https://dev.azure.com/KESHETDEV/Keshet/_workitems/edit/9198
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - IMPLEMENT:code
  - IMPLEMENT:log

---

## XIIGEN-SESSION-1000-0155 - Azure DevOps task retrieval with n8n

```
source_id: xiigen-session-1000-0155
source_title: "Azure DevOps task retrieval with n8n"
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
  conversation_uuid: "b464f7ad-93ce-45ae-8d1d-7d9423f23fa9"
  conversation_name: "Azure DevOps task retrieval with n8n"
  human_message_uuid: "028ec85f-0e15-4812-bea1-a7c805760bf2"
  human_message_index: 2
  message_created_at: "2025-10-24T05:31:31.053777Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0155"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "do the same with http request node"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0155-prompt"
  SourcePath: "claude-export/batch-0000/b464f7ad-93ce-45ae-8d1d-7d9423f23fa9/messages/028ec85f-0e15-4812-bea1-a7c805760bf2"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: "do the same with http request node"
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:request

---

## XIIGEN-SESSION-1000-0156 - Azure DevOps task retrieval with n8n

```
source_id: xiigen-session-1000-0156
source_title: "Azure DevOps task retrieval with n8n"
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
  conversation_uuid: "b464f7ad-93ce-45ae-8d1d-7d9423f23fa9"
  conversation_name: "Azure DevOps task retrieval with n8n"
  human_message_uuid: "9b8c7852-ae74-42f5-9bf3-a42c71770284"
  human_message_index: 10
  message_created_at: "2025-10-24T05:47:07.473572Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0156"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    for the code I don't get any result:
    const allIds = new Set();
    const taskIds = [];
    for (const item of $input.all()) {
      const workItem = item.json;
      
      // Add the task itself
      allIds.add(workItem.id);
      taskIds.push(workItem.id);
      
      // Add all related work items
      if (workItem.relations) {
        for (const relation of workItem.relations) {
          // Only process work item relations, not hyperlinks
          if (relation.url && relation.url.includes('workItems')) {
            const relatedId = relation.url.split('/').pop();
            allIds.add(parseInt(relatedId));
          }
        }
      }
    }
    return [{ 
      json: { 
        allIds: Array.from(allIds),
        taskIds: taskIds
      } 
    }];
    ```
    ### Step 4: HTTP Request - Get All Work Items with Full Details
    **Configuration:**
    - **Method**: GET
    - **URL**: 
    ```
    https://dev.azure.com/{organization}/{project}/_apis/wit/workitems?ids={{$json.allIds.join(',')}}&fields=System.Id,System.WorkItemType,System.Title,System.AssignedTo,System.State,System.Tags,System.AreaPath,System.ChangedDate,System.Description,System.CommentCount&api-version=7.0
    ```
    **Or if you want ALL fields, use:**
    ```
    https://dev.azure.com/{organization}/{project}/_apis/wit/workitems?ids={{$json.allIds.join(',')}}&api-version=7.0
    
    This is my input to the code:
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0156-prompt"
  SourcePath: "claude-export/batch-0000/b464f7ad-93ce-45ae-8d1d-7d9423f23fa9/messages/9b8c7852-ae74-42f5-9bf3-a42c71770284"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    for the code I don't get any result:
    const allIds = new Set();
    const taskIds = [];
    for (const item of $input.all()) {
      const workItem = item.json;
      
      // Add the task itself
      allIds.add(workItem.id);
      taskIds.push(workItem.id);
      
      // Add all related work items
      if (workItem.relations) {
        for (const relation of workItem.relations) {
          // Only process work item relations, not hyperlinks
          if (relation.url && relation.url.includes('workItems')) {
            const relatedId = relation.url.split('/').pop();
            allIds.add(parseInt(relatedId));
          }
        }
      }
    }
    return [{ 
      json: { 
        allIds: Array.from(allIds),
        taskIds: taskIds
      } 
    }];
    ```
    ### Step 4: HTTP Request - Get All Work Items with Full Details
    **Configuration:**
    - **Method**: GET
    - **URL**: 
    ```
    https://dev.azure.com/{organization}/{project}/_apis/wit/workitems?ids={{$json.allIds.join(',')}}&fields=System.Id,System.WorkItemType,System.Title,System.AssignedTo,System.State,System.Tags,System.AreaPath,System.ChangedDate,System.Description,System.CommentCount&api-version=7.0
    ```
    **Or if you want ALL fields, use:**
    ```
    https://dev.azure.com/{organization}/{project}/_apis/wit/workitems?ids={{$json.allIds.join(',')}}&api-version=7.0
    
    This is my input to the code:
  CompiledPredicateIds: ["API_CONTRACT", "ARCHITECTURE", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:api
  - API_CONTRACT:request
  - ARCHITECTURE:project
  - IMPLEMENT:code

---

## XIIGEN-SESSION-1000-0157 - Azure DevOps task retrieval with n8n

```
source_id: xiigen-session-1000-0157
source_title: "Azure DevOps task retrieval with n8n"
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
  conversation_uuid: "b464f7ad-93ce-45ae-8d1d-7d9423f23fa9"
  conversation_name: "Azure DevOps task retrieval with n8n"
  human_message_uuid: "b35f3a31-6337-455a-9f75-a4e98bd90e5a"
  human_message_index: 12
  message_created_at: "2025-10-24T05:51:02.013722Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0157"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    I have a problem with the code:
    const taskIds = $('Step 3').first().json.taskIds;
    const formattedItems = [];
    for (const item of $input.all()) {
      const workItem = item.json;
      const fields = workItem.fields;
      
      // Determine if this is an original task or a related item
      const isOriginalTask = taskIds.includes(workItem.id);
      
      formattedItems.push({
        json: {
          'ID': workItem.id,
          'Work Item Type': fields['System.WorkItemType'],
          'Title': fields['System.Title'],
          'Assigned To': fields['System.AssignedTo']?.displayName || 'Unassigned',
          'State': fields['System.State'],
          'Tags': fields['System.Tags'] || '',
          'Area Path': fields['System.AreaPath'],
          'Changed Date': fields['System.ChangedDate'],
          'Description': fields['System.Description'] || '',
          'Comment Count': fields['System.CommentCount'] || 0,
          'Is Original Task': isOriginalTask,
          'Relation Type': isOriginalTask ? 'Original Task (Yesterday)' : 'Connected Item'
        }
      });
    }
    return formattedItems;
    My input is:
    const taskIds = $('Step 3').first().json.taskIds;
    const formattedItems = [];
    for (const item of $input.all()) {
      const workItem = item.json;
      const fields = workItem.fields;
      
      // Determine if this is an original task or a related item
      const isOriginalTask = taskIds.includes(workItem.id);
      
      formattedItems.push({
        json: {
          'ID': workItem.id,
          'Work Item Type': fields['System.WorkItemType'],
          'Title': fields['System.Title'],
          'Assigned To': fields['System.AssignedTo']?.displayName || 'Unassigned',
          'State': fields['System.State'],
          'Tags': fields['System.Tags'] || '',
          'Area Path': fields['System.AreaPath'],
          'Changed Date': fields['System.ChangedDate'],
          'Description': fields['System.Description'] || '',
          'Comment Count': fields['System.CommentCount'] || 0,
          'Is Original Task': isOriginalTask,
          'Relation Type': isOriginalTask ? 'Original Task (Yesterday)' : 'Connected Item'
        }
      });
    }
    return formattedItems;
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0157-prompt"
  SourcePath: "claude-export/batch-0000/b464f7ad-93ce-45ae-8d1d-7d9423f23fa9/messages/b35f3a31-6337-455a-9f75-a4e98bd90e5a"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Backend Services"
  CompiledText: |
    I have a problem with the code:
    const taskIds = $('Step 3').first().json.taskIds;
    const formattedItems = [];
    for (const item of $input.all()) {
      const workItem = item.json;
      const fields = workItem.fields;
      
      // Determine if this is an original task or a related item
      const isOriginalTask = taskIds.includes(workItem.id);
      
      formattedItems.push({
        json: {
          'ID': workItem.id,
          'Work Item Type': fields['System.WorkItemType'],
          'Title': fields['System.Title'],
          'Assigned To': fields['System.AssignedTo']?.displayName || 'Unassigned',
          'State': fields['System.State'],
          'Tags': fields['System.Tags'] || '',
          'Area Path': fields['System.AreaPath'],
          'Changed Date': fields['System.ChangedDate'],
          'Description': fields['System.Description'] || '',
          'Comment Count': fields['System.CommentCount'] || 0,
          'Is Original Task': isOriginalTask,
          'Relation Type': isOriginalTask ? 'Original Task (Yesterday)' : 'Connected Item'
        }
      });
    }
    return formattedItems;
    My input is:
    const taskIds = $('Step 3').first().json.taskIds;
    const formattedItems = [];
    for (const item of $input.all()) {
      const workItem = item.json;
      const fields = workItem.fields;
      
      // Determine if this is an original task or a related item
      const isOriginalTask = taskIds.includes(workItem.id);
      
      formattedItems.push({
        json: {
          'ID': workItem.id,
          'Work Item Type': fields['System.WorkItemType'],
          'Title': fields['System.Title'],
          'Assigned To': fields['System.AssignedTo']?.displayName || 'Unassigned',
          'State': fields['System.State'],
          'Tags': fields['System.Tags'] || '',
          'Area Path': fields['System.AreaPath'],
          'Changed Date': fields['System.ChangedDate'],
          'Description': fields['System.Description'] || '',
          'Comment Count': fields['System.CommentCount'] || 0,
          'Is Original Task': isOriginalTask,
          'Relation Type': isOriginalTask ? 'Original Task (Yesterday)' : 'Connected Item'
        }
      });
    }
    return formattedItems;
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - IMPLEMENT:code

---

## XIIGEN-SESSION-1000-0158 - Azure DevOps task retrieval with n8n

```
source_id: xiigen-session-1000-0158
source_title: "Azure DevOps task retrieval with n8n"
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
  conversation_uuid: "b464f7ad-93ce-45ae-8d1d-7d9423f23fa9"
  conversation_name: "Azure DevOps task retrieval with n8n"
  human_message_uuid: "a91872fe-b79b-462a-8a9c-c5d4539ae9e7"
  human_message_index: 14
  message_created_at: "2025-10-24T05:51:44.089979Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0158"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Cannot assign to read only property 'name' of object 'Error: Referenced node doesn't exist'"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0158-prompt"
  SourcePath: "claude-export/batch-0000/b464f7ad-93ce-45ae-8d1d-7d9423f23fa9/messages/a91872fe-b79b-462a-8a9c-c5d4539ae9e7"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Backend Services"
  CompiledText: "Cannot assign to read only property 'name' of object 'Error: Referenced node doesn't exist'"
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - IMPLEMENT:error

---

## XIIGEN-SESSION-1000-0159 - Azure DevOps task retrieval with n8n

```
source_id: xiigen-session-1000-0159
source_title: "Azure DevOps task retrieval with n8n"
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
  conversation_uuid: "b464f7ad-93ce-45ae-8d1d-7d9423f23fa9"
  conversation_name: "Azure DevOps task retrieval with n8n"
  human_message_uuid: "8efdff2d-3791-4c0b-8c6f-e332fe3ec012"
  human_message_index: 24
  message_created_at: "2025-10-24T06:00:43.271599Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0159"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    can I somehow change that query to return comments?
    https://dev.azure.com/{organization}/{project}/_apis/wit/workitems?ids={{$json.allIds.join(',')}}&fields=System.Id,System.WorkItemType,System.Title,System.AssignedTo,System.State,System.Tags,System.AreaPath,System.ChangedDate,System.Description,System.CommentCount&api-version=7.0
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0159-prompt"
  SourcePath: "claude-export/batch-0000/b464f7ad-93ce-45ae-8d1d-7d9423f23fa9/messages/8efdff2d-3791-4c0b-8c6f-e332fe3ec012"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    can I somehow change that query to return comments?
    https://dev.azure.com/{organization}/{project}/_apis/wit/workitems?ids={{$json.allIds.join(',')}}&fields=System.Id,System.WorkItemType,System.Title,System.AssignedTo,System.State,System.Tags,System.AreaPath,System.ChangedDate,System.Description,System.CommentCount&api-version=7.0
  CompiledPredicateIds: ["API_CONTRACT", "ARCHITECTURE"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:api
  - ARCHITECTURE:project

---

## XIIGEN-SESSION-1000-0160 - Azure DevOps task retrieval with n8n

```
source_id: xiigen-session-1000-0160
source_title: "Azure DevOps task retrieval with n8n"
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
  conversation_uuid: "b464f7ad-93ce-45ae-8d1d-7d9423f23fa9"
  conversation_name: "Azure DevOps task retrieval with n8n"
  human_message_uuid: "dd32c2cb-b209-451f-a50b-9c52d098b7c6"
  human_message_index: 26
  message_created_at: "2025-10-24T06:07:48.226622Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0160"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    I get error Bad request - please check your parameters [item 0]
    The requested version "7.0" of the resource is under preview. The -preview flag must be supplied in the api-version for such requests. For example: "7.0-preview" for https://dev.azure.com/{organization}/{project}/_apis/wit/workItems/{{$json.workItemId}}/comments?api-version=7.0
    and input: 
    https://dev.azure.com/{organization}/{project}/_apis/wit/workItems/{{$json.workItemId}}/comments?api-version=7.0
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0160-prompt"
  SourcePath: "claude-export/batch-0000/b464f7ad-93ce-45ae-8d1d-7d9423f23fa9/messages/dd32c2cb-b209-451f-a50b-9c52d098b7c6"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    I get error Bad request - please check your parameters [item 0]
    The requested version "7.0" of the resource is under preview. The -preview flag must be supplied in the api-version for such requests. For example: "7.0-preview" for https://dev.azure.com/{organization}/{project}/_apis/wit/workItems/{{$json.workItemId}}/comments?api-version=7.0
    and input: 
    https://dev.azure.com/{organization}/{project}/_apis/wit/workItems/{{$json.workItemId}}/comments?api-version=7.0
  CompiledPredicateIds: ["API_CONTRACT", "ARCHITECTURE", "IMPLEMENT", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:api
  - API_CONTRACT:request
  - ARCHITECTURE:project
  - IMPLEMENT:error

---

## XIIGEN-SESSION-1000-0161 - Azure DevOps task retrieval with n8n

```
source_id: xiigen-session-1000-0161
source_title: "Azure DevOps task retrieval with n8n"
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
  conversation_uuid: "b464f7ad-93ce-45ae-8d1d-7d9423f23fa9"
  conversation_name: "Azure DevOps task retrieval with n8n"
  human_message_uuid: "f54d1c82-ada8-4152-8d62-ee2bf2acfdda"
  human_message_index: 28
  message_created_at: "2025-10-24T06:13:10.928807Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0161"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    For the result:
    "Description": "",
    "Comment Count": 0,
    "Comments": "",
    "Comments Detail": [
    ],
    "Is Original Task": false,
    "Relation Type": "Connected Item"
    },
    {
    "ID": 3392,
    "Work Item Type": "Feature",
    "Title": "עמוד ראשי - נראות",
    "Assigned To": "Unassigned",
    "State": "Closed",
    "Tags": "",
    "Area Path": "Keshet\\Contracts",
    "Changed Date": "2024-12-16T14:43:52.743Z",
    "Description": "",
    "Comment Count": 0,
    "Comments": "",
    "Comments Detail": [
    ],
    "Is Original Task": false,
    "Relation Type": "Connected Item"
    },
    {
    "ID": 3922,
    "Work Item Type": "Feature",
    "Title": "הוספת הכנסה חד פעמית לחוזה",
    "Assigned To": "Shizaf Shunari",
    "State": "Closed",
    "Tags": "",
    "Area Path": "Keshet\\Tamlugim",
    "Changed Date": "2024-11-25T09:52:36.933Z",
    "Description": "<div>אירנה שופסל </div><div style=\"direction:rtl;\">בכל הסכם המשתמש יכול להוסיף הכנסה נוספת כאשר הכנסה חד פעמית&nbsp;<span style=\"background-color:rgb(255, 128, 255);\">אין מחזוריות</span> </div><div style=\"direction:rtl;\">המשתמש יוכל לסמן מי מההכנסות רלוונטיות לכל חוזה בהסכם. </div><div style=\"direction:rtl;\">בזמן החישוב צריך להוסיף את כל ההכנסות הרלוונטיות כמו הכנסת AVOD. (רק לחוזה שסומן שמקבל את ההכנסה) </div><div style=\"direction:rtl;\"><span style=\"background-color:rgb(255, 255, 128);\">בחישוב:</span> </div><div style=\"direction:rtl;\"><span style=\"background-color:rgb(255, 255, 128);\"><ol><li><span style=\"background-color:rgb(255, 255, 255);\">האם הכנסת חד&quot;פ נכנס למשפך כמו AVOD? </span><span style=\"background-color:rgb(255, 128, 255);\">כן</span> </li><li>האם AVOD יכול לעבור להיות מוגדר ומחושב יחד עם ההכנסות החד&quot;פ? <span style=\"background-color:rgb(255, 128, 255);\">כן</span> </li><li>איך יוכנסו ההכנסות האלו לחשבונית PDF? <span style=\"background-color:rgb(255, 128, 255);\">לפי סוגים</span> </li><ol style=\"list-style:lower-alpha;\"><li>AVOD מוכנס כפלטפורמה </li><li>אפשר להוסיף &quot;פלטפורמה&quot; הכנסה חד&quot;פ </li> </ol> </ol></span> </div><div style=\"direction:rtl;\"><span><br></span> </div><div style=\"direction:rtl;\">UI&nbsp; </div><div style=\"direction:rtl;\">במידע כללי של הסכם המשתמש יוכל לנהל את ההכנסות.&nbsp; לא ניתן למחוק הכנסה לאחר שבוצע חישוב. </div><div style=\"direction:rtl;\">בתנאים נוספים של חוזה המשתמש יוכל לבחור את ההכנסות הרלוונטיות. </div><div style=\"direction:rtl;\"><span style=\"display:inline !important;background-color:rgb(255, 255, 128);\">בדומה למסך מכירת פורמט:</span><br> </div><div style=\"direction:rtl;\"><span style=\"display:inline !important;background-color:rgb(255, 255, 128);\"><img src=\"https://dev.azure.com/KESHETDEV/7a929425-38ed-4005-9b0a-d1196cd6b4e9/_apis/wit/attachments/68ddd2ef-801d-49f1-a13e-6466d7449de9?fileName=image.png\" alt=Image><br></span> </div><div style=\"direction:rtl;\"><br> </div>",
    "Comment Count": 0,
    "Comments": "",
    "Comments Detail": [
    ],
    "Is Original Task": false,
    "Relation Type": "Connected Item"
    }
    ]
    
    I want to get the history of changes for today
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0161-prompt"
  SourcePath: "claude-export/batch-0000/b464f7ad-93ce-45ae-8d1d-7d9423f23fa9/messages/f54d1c82-ada8-4152-8d62-ee2bf2acfdda"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    For the result:
    "Description": "",
    "Comment Count": 0,
    "Comments": "",
    "Comments Detail": [
    ],
    "Is Original Task": false,
    "Relation Type": "Connected Item"
    },
    {
    "ID": 3392,
    "Work Item Type": "Feature",
    "Title": "עמוד ראשי - נראות",
    "Assigned To": "Unassigned",
    "State": "Closed",
    "Tags": "",
    "Area Path": "Keshet\\Contracts",
    "Changed Date": "2024-12-16T14:43:52.743Z",
    "Description": "",
    "Comment Count": 0,
    "Comments": "",
    "Comments Detail": [
    ],
    "Is Original Task": false,
    "Relation Type": "Connected Item"
    },
    {
    "ID": 3922,
    "Work Item Type": "Feature",
    "Title": "הוספת הכנסה חד פעמית לחוזה",
    "Assigned To": "Shizaf Shunari",
    "State": "Closed",
    "Tags": "",
    "Area Path": "Keshet\\Tamlugim",
    "Changed Date": "2024-11-25T09:52:36.933Z",
    "Description": "<div>אירנה שופסל </div><div style=\"direction:rtl;\">בכל הסכם המשתמש יכול להוסיף הכנסה נוספת כאשר הכנסה חד פעמית&nbsp;<span style=\"background-color:rgb(255, 128, 255);\">אין מחזוריות</span> </div><div style=\"direction:rtl;\">המשתמש יוכל לסמן מי מההכנסות רלוונטיות לכל חוזה בהסכם. </div><div style=\"direction:rtl;\">בזמן החישוב צריך להוסיף את כל ההכנסות הרלוונטיות כמו הכנסת AVOD. (רק לחוזה שסומן שמקבל את ההכנסה) </div><div style=\"direction:rtl;\"><span style=\"background-color:rgb(255, 255, 128);\">בחישוב:</span> </div><div style=\"direction:rtl;\"><span style=\"background-color:rgb(255, 255, 128);\"><ol><li><span style=\"background-color:rgb(255, 255, 255);\">האם הכנסת חד&quot;פ נכנס למשפך כמו AVOD? </span><span style=\"background-color:rgb(255, 128, 255);\">כן</span> </li><li>האם AVOD יכול לעבור להיות מוגדר ומחושב יחד עם ההכנסות החד&quot;פ? <span style=\"background-color:rgb(255, 128, 255);\">כן</span> </li><li>איך יוכנסו ההכנסות האלו לחשבונית PDF? <span style=\"background-color:rgb(255, 128, 255);\">לפי סוגים</span> </li><ol style=\"list-style:lower-alpha;\"><li>AVOD מוכנס כפלטפורמה </li><li>אפשר להוסיף &quot;פלטפורמה&quot; הכנסה חד&quot;פ </li> </ol> </ol></span> </div><div style=\"direction:rtl;\"><span><br></span> </div><div style=\"direction:rtl;\">UI&nbsp; </div><div style=\"direction:rtl;\">במידע כללי של הסכם המשתמש יוכל לנהל את ההכנסות.&nbsp; לא ניתן למחוק הכנסה לאחר שבוצע חישוב. </div><div style=\"direction:rtl;\">בתנאים נוספים של חוזה המשתמש יוכל לבחור את ההכנסות הרלוונטיות. </div><div style=\"direction:rtl;\"><span style=\"display:inline !important;background-color:rgb(255, 255, 128);\">בדומה למסך מכירת פורמט:</span><br> </div><div style=\"direction:rtl;\"><span style=\"display:inline !important;background-color:rgb(255, 255, 128);\"><img src=\"https://dev.azure.com/KESHETDEV/7a929425-38ed-4005-9b0a-d1196cd6b4e9/_apis/wit/attachments/68ddd2ef-801d-49f1-a13e-6466d7449de9?fileName=image.png\" alt=Image><br></span> </div><div style=\"direction:rtl;\"><br> </div>",
    "Comment Count": 0,
    "Comments": "",
    "Comments Detail": [
    ],
    "Is Original Task": false,
    "Relation Type": "Connected Item"
    }
    ]
    
    I want to get the history of changes for today
  CompiledPredicateIds: ["API_CONTRACT", "UI"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:api
  - API_CONTRACT:contract
  - UI:ui

---

## XIIGEN-SESSION-1000-0162 - Azure DevOps task retrieval with n8n

```
source_id: xiigen-session-1000-0162
source_title: "Azure DevOps task retrieval with n8n"
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
  conversation_uuid: "b464f7ad-93ce-45ae-8d1d-7d9423f23fa9"
  conversation_name: "Azure DevOps task retrieval with n8n"
  human_message_uuid: "16b1a43f-47d2-4444-95da-e0a5802a7e4d"
  human_message_index: 30
  message_created_at: "2025-10-24T06:20:13.499083Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0162"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    for this code I get an error:
    Cannot assign to read only property 'name' of object 'Error: Referenced node is unexecuted'
    const taskIds = $('Get all the connected tasks ids').first().json.taskIds;  // Replace 'Code' with your Step 3 node name
    const workItems = $('Get all the tasks with relations needed data').all();  // Replace with your work items step
    const updates = $input.all();  // Current input with updates
    // Get today's date range
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const tomorrow = new Date(today);
    tomorrow.setDate(tomorrow.getDate() + 1);
    // Create a map of updates by work item ID
    const updatesMap = {};
    for (const item of updates) {
      const workItemId = item.json.workItemId;
      const allUpdates = item.json.value || [];
      
      // Filter for today's updates only
      const todayUpdates = allUpdates.filter(update => {
        const revisedDate = new Date(update.revisedDate);
        return revisedDate >= today && revisedDate < tomorrow;
      });
      
      updatesMap[workItemId] = todayUpdates;
    }
    const formattedItems = [];
    // Process work items
    for (const item of workItems) {
      const response = item.json;
      
      if (response.value && Array.isArray(response.value)) {
        for (const workItem of response.value) {
          const fields = workItem.fields;
          const isOriginalTask = taskIds.includes(workItem.id);
          
          // Get today's updates for this work item
          const todayUpdates = updatesMap[workItem.id] || [];
          let changesText = '';
          let changesList = [];
          
          for (const update of todayUpdates) {
            const changedBy = update.revisedBy?.displayName || 'Unknown';
            const changedDate = update.revisedDate;
            const changes = [];
            
            // Check field changes
            if (update.fields) {
              for (const [fieldName, change] of Object.entries(update.fields)) {
                changes.push({
                  field: fieldName,
                  oldValue: change.oldValue,
                  newValue: change.newValue
                });
              }
            }
            
            changesList.push({
              id: update.id,
              rev: update.rev,
              changedBy: changedBy,
              changedDate: changedDate,
              changes: changes
            });
            
            // Build text summary
            changesText += `[${changedBy}] ${changedDate}:\n`;
            for (const change of changes) {
              changesText += `  - ${change.field}: "${change.oldValue}" → "${change.newValue}"\n`;
            }
            changesText += '\n';
          }
          
          formattedItems.push({
            json: {
              'ID': workItem.id,
              'Work Item Type': fields['System.WorkItemType'],
              'Title': fields['System.Title'],
              'Assigned To': fields['System.AssignedTo']?.displayName || 'Unassigned',
              'State': fields['System.State'],
              'Tags': fields['System.Tags'] || '',
              'Area Path': fields['System.AreaPath'],
              'Changed Date': fields['System.ChangedDate'],
              'Description': fields['System.Description'] || '',
              'Comment Count': fields['System.CommentCount'] || 0,
              'Today Changes Count': todayUpdates.length,
              'Today Changes': changesText,
              'Today Changes Detail': changesList,
              'Is Original Task': isOriginalTask,
              'Relation Type': isOriginalTask ? 'Original Task (Yesterday)' : 'Connected Item'
            }
          });
        }
      }
    }
    return formattedItems;
    
    This is my input:
    "id": "0131d70e-e619-61e5-91b8-ba8444d4107a",
    "name": "Yaniv Refaelov <Yaniv.Refaelov@Keshet-tv.com>",
    "displayName": "Yaniv Refaelov",
    "url": "https://spsprodweu3.vssps.visualstudio.com/A8eaa194f-8d18-456e-9ed5-e37b70b9c324/_apis/Identities/0131d70e-e619-61e5-91b8-ba8444d4107a",
    "_links": {
    "avatar": {
    "href": "https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/aad.MDEzMWQ3MGUtZTYxOS03MWU1LTkxYjgtYmE4NDQ0ZDQxMDdh"
    }
    },
    "uniqueName": "Yaniv.Refaelov@Keshet-tv.com",
    "imageUrl": "https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/aad.MDEzMWQ3MGUtZTYxOS03MWU1LTkxYjgtYmE4NDQ0ZDQxMDdh",
    "descriptor": "aad.MDEzMWQ3MGUtZTYxOS03MWU1LTkxYjgtYmE4NDQ0ZDQxMDdh"
    },
    "revisedDate": "2025-03-25T15:02:46.863Z",
    "fields": {
    "System.Rev": {
    "oldValue": 1,
    "newValue": 2
    },
    "System.AuthorizedDate": {
    "oldValue": "2025-03-25T14:43:15.66Z",
    "newValue": "2025-03-25T14:48:42.48Z"
    },
    "System.RevisedDate": {
    "oldValue": "2025-03-25T14:48:42.48Z",
    "newValue": "2025-03-25T15:02:46.863Z"
    },
    "System.ChangedDate": {
    "oldValue": "2025-03-25T14:43:15.66Z",
    "newValue": "2025-03-25T14:48:42.48Z"
    },....
    
    Step names:
    Step 3
    Get all the connected tasks ids
    Step 4
    Get all the tasks with relations needed data
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0162-prompt"
  SourcePath: "claude-export/batch-0000/b464f7ad-93ce-45ae-8d1d-7d9423f23fa9/messages/16b1a43f-47d2-4444-95da-e0a5802a7e4d"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    for this code I get an error:
    Cannot assign to read only property 'name' of object 'Error: Referenced node is unexecuted'
    const taskIds = $('Get all the connected tasks ids').first().json.taskIds;  // Replace 'Code' with your Step 3 node name
    const workItems = $('Get all the tasks with relations needed data').all();  // Replace with your work items step
    const updates = $input.all();  // Current input with updates
    // Get today's date range
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const tomorrow = new Date(today);
    tomorrow.setDate(tomorrow.getDate() + 1);
    // Create a map of updates by work item ID
    const updatesMap = {};
    for (const item of updates) {
      const workItemId = item.json.workItemId;
      const allUpdates = item.json.value || [];
      
      // Filter for today's updates only
      const todayUpdates = allUpdates.filter(update => {
        const revisedDate = new Date(update.revisedDate);
        return revisedDate >= today && revisedDate < tomorrow;
      });
      
      updatesMap[workItemId] = todayUpdates;
    }
    const formattedItems = [];
    // Process work items
    for (const item of workItems) {
      const response = item.json;
      
      if (response.value && Array.isArray(response.value)) {
        for (const workItem of response.value) {
          const fields = workItem.fields;
          const isOriginalTask = taskIds.includes(workItem.id);
          
          // Get today's updates for this work item
          const todayUpdates = updatesMap[workItem.id] || [];
          let changesText = '';
          let changesList = [];
          
          for (const update of todayUpdates) {
            const changedBy = update.revisedBy?.displayName || 'Unknown';
            const changedDate = update.revisedDate;
            const changes = [];
            
            // Check field changes
            if (update.fields) {
              for (const [fieldName, change] of Object.entries(update.fields)) {
                changes.push({
                  field: fieldName,
                  oldValue: change.oldValue,
                  newValue: change.newValue
                });
              }
            }
            
            changesList.push({
              id: update.id,
              rev: update.rev,
              changedBy: changedBy,
              changedDate: changedDate,
              changes: changes
            });
            
            // Build text summary
            changesText += `[${changedBy}] ${changedDate}:\n`;
            for (const change of changes) {
              changesText += `  - ${change.field}: "${change.oldValue}" → "${change.newValue}"\n`;
            }
            changesText += '\n';
          }
          
          formattedItems.push({
            json: {
              'ID': workItem.id,
              'Work Item Type': fields['System.WorkItemType'],
              'Title': fields['System.Title'],
              'Assigned To': fields['System.AssignedTo']?.displayName || 'Unassigned',
              'State': fields['System.State'],
              'Tags': fields['System.Tags'] || '',
              'Area Path': fields['System.AreaPath'],
              'Changed Date': fields['System.ChangedDate'],
              'Description': fields['System.Description'] || '',
              'Comment Count': fields['System.CommentCount'] || 0,
              'Today Changes Count': todayUpdates.length,
              'Today Changes': changesText,
              'Today Changes Detail': changesList,
              'Is Original Task': isOriginalTask,
              'Relation Type': isOriginalTask ? 'Original Task (Yesterday)' : 'Connected Item'
            }
          });
        }
      }
    }
    return formattedItems;
    
    This is my input:
    "id": "0131d70e-e619-61e5-91b8-ba8444d4107a",
    "name": "Yaniv Refaelov <Yaniv.Refaelov@Keshet-tv.com>",
    "displayName": "Yaniv Refaelov",
    "url": "https://spsprodweu3.vssps.visualstudio.com/A8eaa194f-8d18-456e-9ed5-e37b70b9c324/_apis/Identities/0131d70e-e619-61e5-91b8-ba8444d4107a",
    "_links": {
    "avatar": {
    "href": "https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/aad.MDEzMWQ3MGUtZTYxOS03MWU1LTkxYjgtYmE4NDQ0ZDQxMDdh"
    }
    },
    "uniqueName": "Yaniv.Refaelov@Keshet-tv.com",
    "imageUrl": "https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/aad.MDEzMWQ3MGUtZTYxOS03MWU1LTkxYjgtYmE4NDQ0ZDQxMDdh",
    "descriptor": "aad.MDEzMWQ3MGUtZTYxOS03MWU1LTkxYjgtYmE4NDQ0ZDQxMDdh"
    },
    "revisedDate": "2025-03-25T15:02:46.863Z",
    "fields": {
    "System.Rev": {
    "oldValue": 1,
    "newValue": 2
    },
    "System.AuthorizedDate": {
    "oldValue": "2025-03-25T14:43:15.66Z",
    "newValue": "2025-03-25T14:48:42.48Z"
    },
    "System.RevisedDate": {
    "oldValue": "2025-03-25T14:48:42.48Z",
    "newValue": "2025-03-25T15:02:46.863Z"
    },
    "System.ChangedDate": {
    "oldValue": "2025-03-25T14:43:15.66Z",
    "newValue": "2025-03-25T14:48:42.48Z"
    },....
    
    Step names:
    Step 3
    Get all the connected tasks ids
    Step 4
    Get all the tasks with relations needed data
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "UI"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:api
  - API_CONTRACT:response
  - IMPLEMENT:code
  - IMPLEMENT:error
  - SECURITY:auth
  - UI:ui

---

## XIIGEN-SESSION-1000-0163 - Azure DevOps task retrieval with n8n

```
source_id: xiigen-session-1000-0163
source_title: "Azure DevOps task retrieval with n8n"
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
  conversation_uuid: "b464f7ad-93ce-45ae-8d1d-7d9423f23fa9"
  conversation_name: "Azure DevOps task retrieval with n8n"
  human_message_uuid: "a2020eec-0184-4bdd-bf1b-da0ad6be7bd4"
  human_message_index: 32
  message_created_at: "2025-10-24T06:26:22.303999Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0163"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    I got the input:
    const formattedItems = [];
    // Get today's date range  
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const tomorrow = new Date(today);
    tomorrow.setDate(tomorrow.getDate() + 1);
    for (const item of $input.all()) {
      const workItemId = item.json.workItemId;
      
      // The updates are in item.json.value or directly in item.json
      let allUpdates = [];
      if (item.json.value && Array.isArray(item.json.value)) {
        allUpdates = item.json.value;
      } else if (item.json.fields) {
        // Single update object
        allUpdates = [item.json];
      }
      
      // Filter for today's updates only
      const todayUpdates = allUpdates.filter(update => {
        const revisedDate = new Date(update.revisedDate);
        return revisedDate >= today && revisedDate < tomorrow;
      });
      
      let changesText = '';
      let changesList = [];
      
      for (const update of todayUpdates) {
        const changedBy = update.revisedBy?.displayName || 'Unknown';
        const changedDate = update.revisedDate;
        const changes = [];
        
        // Check field changes
        if (update.fields) {
          for (const [fieldName, change] of Object.entries(update.fields)) {
            changes.push({
              field: fieldName,
              oldValue: change.oldValue,
              newValue: change.newValue
            });
          }
        }
        
        changesList.push({
          rev: update.rev,
          changedBy: changedBy,
          changedDate: changedDate,
          changes: changes
        });
        
        // Build text summary
        changesText += `[${changedBy}] ${changedDate}:\n`;
        for (const change of changes) {
          changesText += `  - ${change.field}: "${change.oldValue}" → "${change.newValue}"\n`;
        }
        changesText += '\n';
      }
      
      formattedItems.push({
        json: {
          'Work Item ID': workItemId,
          'Today Changes Count': todayUpdates.length,
          'Today Changes': changesText,
          'Today Changes Detail': changesList
        }
      });
    }
    return formattedItems;
    
    How do I filter only updates that happened today with a code node?
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0163-prompt"
  SourcePath: "claude-export/batch-0000/b464f7ad-93ce-45ae-8d1d-7d9423f23fa9/messages/a2020eec-0184-4bdd-bf1b-da0ad6be7bd4"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Frontend and Client Integration"
  CompiledText: |
    I got the input:
    const formattedItems = [];
    // Get today's date range  
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const tomorrow = new Date(today);
    tomorrow.setDate(tomorrow.getDate() + 1);
    for (const item of $input.all()) {
      const workItemId = item.json.workItemId;
      
      // The updates are in item.json.value or directly in item.json
      let allUpdates = [];
      if (item.json.value && Array.isArray(item.json.value)) {
        allUpdates = item.json.value;
      } else if (item.json.fields) {
        // Single update object
        allUpdates = [item.json];
      }
      
      // Filter for today's updates only
      const todayUpdates = allUpdates.filter(update => {
        const revisedDate = new Date(update.revisedDate);
        return revisedDate >= today && revisedDate < tomorrow;
      });
      
      let changesText = '';
      let changesList = [];
      
      for (const update of todayUpdates) {
        const changedBy = update.revisedBy?.displayName || 'Unknown';
        const changedDate = update.revisedDate;
        const changes = [];
        
        // Check field changes
        if (update.fields) {
          for (const [fieldName, change] of Object.entries(update.fields)) {
            changes.push({
              field: fieldName,
              oldValue: change.oldValue,
              newValue: change.newValue
            });
          }
        }
        
        changesList.push({
          rev: update.rev,
          changedBy: changedBy,
          changedDate: changedDate,
          changes: changes
        });
        
        // Build text summary
        changesText += `[${changedBy}] ${changedDate}:\n`;
        for (const change of changes) {
          changesText += `  - ${change.field}: "${change.oldValue}" → "${change.newValue}"\n`;
        }
        changesText += '\n';
      }
      
      formattedItems.push({
        json: {
          'Work Item ID': workItemId,
          'Today Changes Count': todayUpdates.length,
          'Today Changes': changesText,
          'Today Changes Detail': changesList
        }
      });
    }
    return formattedItems;
    
    How do I filter only updates that happened today with a code node?
  CompiledPredicateIds: ["IMPLEMENT", "UI"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - IMPLEMENT:code
  - UI:ui

---

## XIIGEN-SESSION-1000-0164 - Azure DevOps task retrieval with n8n

```
source_id: xiigen-session-1000-0164
source_title: "Azure DevOps task retrieval with n8n"
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
  conversation_uuid: "b464f7ad-93ce-45ae-8d1d-7d9423f23fa9"
  conversation_name: "Azure DevOps task retrieval with n8n"
  human_message_uuid: "84e989b7-3246-4909-b9a4-98caa67d072d"
  human_message_index: 34
  message_created_at: "2025-10-24T06:27:03.485032Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0164"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    I get this input, How do I filter only updates that happened today with a code node?:
    "avatar": {
    "href": "https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/aad.MDEzMWQ3MGUtZTYxOS03MWU1LTkxYjgtYmE4NDQ0ZDQxMDdh"
    }
    },
    "id": "0131d70e-e619-61e5-91b8-ba8444d4107a",
    "uniqueName": "Yaniv.Refaelov@Keshet-tv.com",
    "imageUrl": "https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/aad.MDEzMWQ3MGUtZTYxOS03MWU1LTkxYjgtYmE4NDQ0ZDQxMDdh",
    "descriptor": "aad.MDEzMWQ3MGUtZTYxOS03MWU1LTkxYjgtYmE4NDQ0ZDQxMDdh"
    }
    },
    "Microsoft.VSTS.Common.Priority": {
    "newValue": 2
    },
    "Microsoft.VSTS.TCM.AutomationStatus": {
    "newValue": "Not Automated"
    },
    "Microsoft.VSTS.TCM.Steps": {
    "newValue": "<steps id=\"0\" last=\"9\"><step id=\"2\" type=\"ValidateStep\"><parameterizedString isformatted=\"true\">&lt;DIV&gt;&lt;P&gt;הכנס למסך זכויות שידור&lt;/P&gt;&lt;/DIV&gt;</parameterizedString><parameterizedString isformatted=\"true\">&lt;DIV&gt;&lt;P&gt;יש 2 רשימות בחירה:&lt;BR/&gt;1.זכויות שידור לתוכנית&lt;BR/&gt;2.זכויות שידור לפרק&lt;BR/&gt;אפשרות להוספת חלון שידור מקדים&lt;BR/&gt;קישור הסכם&lt;BR/&gt;אפשרות להוספת זכות שידור&lt;/P&gt;&lt;/DIV&gt;</parameterizedString><description/></step><step id=\"3\" type=\"ValidateStep\"><parameterizedString isformatted=\"true\">&lt;DIV&gt;&lt;DIV&gt;&lt;P&gt;לחץ רשימת הבחירה של זכויות שידור לתוכנית&lt;/P&gt;&lt;/DIV&gt;&lt;/DIV&gt;</parameterizedString><parameterizedString isformatted=\"true\">&lt;DIV&gt;&lt;P&gt;נפתחת רשימה עם 3 ערכים:&lt;BR/&gt;1.תקופה&lt;BR/&gt;2.שנים&lt;BR/&gt;3.ללא הגבלה&lt;/P&gt;&lt;/DIV&gt;</parameterizedString><description/></step><step id=\"4\" type=\"ValidateStep\"><parameterizedString isformatted=\"true\">&lt;DIV&gt;&lt;DIV&gt;&lt;P&gt;לחץ רשימת הבחירה של זכויות שידור לפרק&lt;/P&gt;&lt;/DIV&gt;&lt;/DIV&gt;</parameterizedString><parameterizedString isformatted=\"true\">&lt;DIV&gt;&lt;P&gt;נפתחת רשימה עם 3 ערכים:&lt;BR/&gt;1.סבבים (RUNS)&lt;BR/&gt;2.שידורים&lt;BR/&gt;3.ללא הגבלה&lt;BR/&gt;&lt;/P&gt;&lt;/DIV&gt;</parameterizedString><description/></step><step id=\"5\" type=\"ValidateStep\"><parameterizedString isformatted=\"true\">&lt;DIV&gt;&lt;P&gt;לחץ על הוסף חלון (חלונות שידור מקדימים)&lt;/P&gt;&lt;/DIV&gt;</parameterizedString><parameterizedString isformatted=\"true\">&lt;DIV&gt;&lt;P&gt;מתווספת רשומה עם 2 שדות לעריכה+אפשרות למחיקת רשומה:&lt;BR/&gt;1.חלון&lt;BR/&g....
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0164-prompt"
  SourcePath: "claude-export/batch-0000/b464f7ad-93ce-45ae-8d1d-7d9423f23fa9/messages/84e989b7-3246-4909-b9a4-98caa67d072d"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    I get this input, How do I filter only updates that happened today with a code node?:
    "avatar": {
    "href": "https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/aad.MDEzMWQ3MGUtZTYxOS03MWU1LTkxYjgtYmE4NDQ0ZDQxMDdh"
    }
    },
    "id": "0131d70e-e619-61e5-91b8-ba8444d4107a",
    "uniqueName": "Yaniv.Refaelov@Keshet-tv.com",
    "imageUrl": "https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/aad.MDEzMWQ3MGUtZTYxOS03MWU1LTkxYjgtYmE4NDQ0ZDQxMDdh",
    "descriptor": "aad.MDEzMWQ3MGUtZTYxOS03MWU1LTkxYjgtYmE4NDQ0ZDQxMDdh"
    }
    },
    "Microsoft.VSTS.Common.Priority": {
    "newValue": 2
    },
    "Microsoft.VSTS.TCM.AutomationStatus": {
    "newValue": "Not Automated"
    },
    "Microsoft.VSTS.TCM.Steps": {
    "newValue": "<steps id=\"0\" last=\"9\"><step id=\"2\" type=\"ValidateStep\"><parameterizedString isformatted=\"true\">&lt;DIV&gt;&lt;P&gt;הכנס למסך זכויות שידור&lt;/P&gt;&lt;/DIV&gt;</parameterizedString><parameterizedString isformatted=\"true\">&lt;DIV&gt;&lt;P&gt;יש 2 רשימות בחירה:&lt;BR/&gt;1.זכויות שידור לתוכנית&lt;BR/&gt;2.זכויות שידור לפרק&lt;BR/&gt;אפשרות להוספת חלון שידור מקדים&lt;BR/&gt;קישור הסכם&lt;BR/&gt;אפשרות להוספת זכות שידור&lt;/P&gt;&lt;/DIV&gt;</parameterizedString><description/></step><step id=\"3\" type=\"ValidateStep\"><parameterizedString isformatted=\"true\">&lt;DIV&gt;&lt;DIV&gt;&lt;P&gt;לחץ רשימת הבחירה של זכויות שידור לתוכנית&lt;/P&gt;&lt;/DIV&gt;&lt;/DIV&gt;</parameterizedString><parameterizedString isformatted=\"true\">&lt;DIV&gt;&lt;P&gt;נפתחת רשימה עם 3 ערכים:&lt;BR/&gt;1.תקופה&lt;BR/&gt;2.שנים&lt;BR/&gt;3.ללא הגבלה&lt;/P&gt;&lt;/DIV&gt;</parameterizedString><description/></step><step id=\"4\" type=\"ValidateStep\"><parameterizedString isformatted=\"true\">&lt;DIV&gt;&lt;DIV&gt;&lt;P&gt;לחץ רשימת הבחירה של זכויות שידור לפרק&lt;/P&gt;&lt;/DIV&gt;&lt;/DIV&gt;</parameterizedString><parameterizedString isformatted=\"true\">&lt;DIV&gt;&lt;P&gt;נפתחת רשימה עם 3 ערכים:&lt;BR/&gt;1.סבבים (RUNS)&lt;BR/&gt;2.שידורים&lt;BR/&gt;3.ללא הגבלה&lt;BR/&gt;&lt;/P&gt;&lt;/DIV&gt;</parameterizedString><description/></step><step id=\"5\" type=\"ValidateStep\"><parameterizedString isformatted=\"true\">&lt;DIV&gt;&lt;P&gt;לחץ על הוסף חלון (חלונות שידור מקדימים)&lt;/P&gt;&lt;/DIV&gt;</parameterizedString><parameterizedString isformatted=\"true\">&lt;DIV&gt;&lt;P&gt;מתווספת רשומה עם 2 שדות לעריכה+אפשרות למחיקת רשומה:&lt;BR/&gt;1.חלון&lt;BR/&g....
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:api
  - IMPLEMENT:code
  - TEST:test

---

## XIIGEN-SESSION-1000-0165 - Azure DevOps task retrieval with n8n

```
source_id: xiigen-session-1000-0165
source_title: "Azure DevOps task retrieval with n8n"
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
  conversation_uuid: "b464f7ad-93ce-45ae-8d1d-7d9423f23fa9"
  conversation_name: "Azure DevOps task retrieval with n8n"
  human_message_uuid: "93f31ef2-4f0a-4dee-88fa-335a34ea649e"
  human_message_index: 36
  message_created_at: "2025-10-24T06:29:58.114780Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0165"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    Now I want to unite the data of updates with the data of tasks and related
    
    Updates:
    [
    {
    "ID": 80,
    "Work Item Type": "Test Case",
    "Title": "בדיקת ממש\"ק משתמש - ( UI ) מסך רשימת ההסכמים ",
    "Assigned To": "Tatiana Peysik",
    "State": "Design",
    "Tags": "רשימת הסכמים",
    "Area Path": "Keshet\\Tamlugim",
    "Changed Date": "2025-08-17T08:54:34.417Z",
    "Description": "",
    "Comment Count": 0,
    "Today Changes Count": 0,
    "Today Changes": "",
    "Today Changes Detail": [
    ],
    "Is Original Task": false,
    "Relation Type": "Connected Item"
    },
    {
    "ID": 1058,
    "Work Item Type": "Feature",
    "Title": "Broadcasting Rights",
    "Assigned To": "Unassigned",
    "State": "Closed",
    "Tags": "",
    "Area Path": "Keshet\\Contracts",
    "Changed Date": "2024-11-25T09:51:37.35Z",
    "Description": "",
    "Comment Count": 0,
    "Today Changes Count": 0,
    "Today Changes": "",
    "Today Changes Detail": [
    ],
    "Is Original Task": false,
    "Relation Type": "Connected Item"
    },
    {
    "ID": 1236,...
    
    Tasks and related:
    
    [
    {
    "ID": 80,
    "Work Item Type": "Test Case",
    "Title": "בדיקת ממש\"ק משתמש - ( UI ) מסך רשימת ההסכמים ",
    "Assigned To": "Tatiana Peysik",
    "State": "Design",
    "Tags": "רשימת הסכמים",
    "Area Path": "Keshet\\Tamlugim",
    "Changed Date": "2025-08-17T08:54:34.417Z",
    "Description": "",
    "Comment Count": 0,
    "Comments": "",
    "Comments Detail": [
    ],
    "Is Original Task": false,
    "Relation Type": "Connected Item"
    },
    {
    "ID": 1058,
    "Work Item Type": "Feature",
    "Title": "Broadcasting Rights",
    "Assigned To": "Unassigned",
    "State": "Closed",
    "Tags": "",
    "Area Path": "Keshet\\Contracts",
    "Changed Date": "2024-11-25T09:51:37.35Z",
    "Description": "",
    "Comment Count": 0,
    "Comments": "",
    "Comments Detail": [
    ],
    "Is Original Task": false,
    "Relation Type": "Connected Item"
    },
    {
    "ID": 1236,
    
    The code that unites tasks and related:
    const taskIds = $('Get all the connected tasks ids').first().json.taskIds;  // Replace 'Code' with Step 3 node name
    const workItems = $('Get all the tasks with relations needed data').all();  // Replace with Step 4 node name (work items)
    const commentsData = $input.all();  // Current input with comments
    // Create a map of comments by work item ID
    const commentsMap = {};
    for (const item of commentsData) {
      const workItemId = item.json.workItemId || item.json.id;
      const comments = item.json.comments || item.json;
      
      if (comments && comments.comments) {
        commentsMap[workItemId] = comments.comments;
      }
    }
    const formattedItems = [];
    // Process work items
    for (const item of workItems) {
      const response = item.json;
      
      if (response.value && Array.isArray(response.value)) {
        for (const workItem of response.value) {
          const fields = workItem.fields;
          const isOriginalTask = taskIds.includes(workItem.id);
          
          // Get comments for this work item
          const comments = commentsMap[workItem.id] || [];
          let commentsText = '';
          let commentsList = [];
          
          for (const comment of comments) {
            commentsList.push({
              id: comment.id,
              text: comment.text,
              createdBy: comment.createdBy?.displayName || 'Unknown',
              createdDate: comment.createdDate
            });
            
            commentsText += `[${comment.createdBy?.displayName}] ${comment.createdDate}: ${comment.text}\n\n`;
          }
          
          formattedItems.push({
            json: {
              'ID': workItem.id,
              'Work Item Type': fields['System.WorkItemType'],
              'Title': fields['System.Title'],
              'Assigned To': fields['System.AssignedTo']?.displayName || 'Unassigned',
              'State': fields['System.State'],
              'Tags': fields['System.Tags'] || '',
              'Area Path': fields['System.AreaPath'],
              'Changed Date': fields['System.ChangedDate'],
              'Description': fields['System.Description'] || '',
              'Comment Count': fields['System.CommentCount'] || 0,
              'Comments': commentsText,
              'Comments Detail': commentsList,
              'Is Original Task': isOriginalTask,
              'Relation Type': isOriginalTask ? 'Original Task (Yesterday)' : 'Connected Item'
            }
          });
        }
      }
    }
    return formattedItems;
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0165-prompt"
  SourcePath: "claude-export/batch-0000/b464f7ad-93ce-45ae-8d1d-7d9423f23fa9/messages/93f31ef2-4f0a-4dee-88fa-335a34ea649e"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    Now I want to unite the data of updates with the data of tasks and related
    
    Updates:
    [
    {
    "ID": 80,
    "Work Item Type": "Test Case",
    "Title": "בדיקת ממש\"ק משתמש - ( UI ) מסך רשימת ההסכמים ",
    "Assigned To": "Tatiana Peysik",
    "State": "Design",
    "Tags": "רשימת הסכמים",
    "Area Path": "Keshet\\Tamlugim",
    "Changed Date": "2025-08-17T08:54:34.417Z",
    "Description": "",
    "Comment Count": 0,
    "Today Changes Count": 0,
    "Today Changes": "",
    "Today Changes Detail": [
    ],
    "Is Original Task": false,
    "Relation Type": "Connected Item"
    },
    {
    "ID": 1058,
    "Work Item Type": "Feature",
    "Title": "Broadcasting Rights",
    "Assigned To": "Unassigned",
    "State": "Closed",
    "Tags": "",
    "Area Path": "Keshet\\Contracts",
    "Changed Date": "2024-11-25T09:51:37.35Z",
    "Description": "",
    "Comment Count": 0,
    "Today Changes Count": 0,
    "Today Changes": "",
    "Today Changes Detail": [
    ],
    "Is Original Task": false,
    "Relation Type": "Connected Item"
    },
    {
    "ID": 1236,...
    
    Tasks and related:
    
    [
    {
    "ID": 80,
    "Work Item Type": "Test Case",
    "Title": "בדיקת ממש\"ק משתמש - ( UI ) מסך רשימת ההסכמים ",
    "Assigned To": "Tatiana Peysik",
    "State": "Design",
    "Tags": "רשימת הסכמים",
    "Area Path": "Keshet\\Tamlugim",
    "Changed Date": "2025-08-17T08:54:34.417Z",
    "Description": "",
    "Comment Count": 0,
    "Comments": "",
    "Comments Detail": [
    ],
    "Is Original Task": false,
    "Relation Type": "Connected Item"
    },
    {
    "ID": 1058,
    "Work Item Type": "Feature",
    "Title": "Broadcasting Rights",
    "Assigned To": "Unassigned",
    "State": "Closed",
    "Tags": "",
    "Area Path": "Keshet\\Contracts",
    "Changed Date": "2024-11-25T09:51:37.35Z",
    "Description": "",
    "Comment Count": 0,
    "Comments": "",
    "Comments Detail": [
    ],
    "Is Original Task": false,
    "Relation Type": "Connected Item"
    },
    {
    "ID": 1236,
    
    The code that unites tasks and related:
    const taskIds = $('Get all the connected tasks ids').first().json.taskIds;  // Replace 'Code' with Step 3 node name
    const workItems = $('Get all the tasks with relations needed data').all();  // Replace with Step 4 node name (work items)
    const commentsData = $input.all();  // Current input with comments
    // Create a map of comments by work item ID
    const commentsMap = {};
    for (const item of commentsData) {
      const workItemId = item.json.workItemId || item.json.id;
      const comments = item.json.comments || item.json;
      
      if (comments && comments.comments) {
        commentsMap[workItemId] = comments.comments;
      }
    }
    const formattedItems = [];
    // Process work items
    for (const item of workItems) {
      const response = item.json;
      
      if (response.value && Array.isArray(response.value)) {
        for (const workItem of response.value) {
          const fields = workItem.fields;
          const isOriginalTask = taskIds.includes(workItem.id);
          
          // Get comments for this work item
          const comments = commentsMap[workItem.id] || [];
          let commentsText = '';
          let commentsList = [];
          
          for (const comment of comments) {
            commentsList.push({
              id: comment.id,
              text: comment.text,
              createdBy: comment.createdBy?.displayName || 'Unknown',
              createdDate: comment.createdDate
            });
            
            commentsText += `[${comment.createdBy?.displayName}] ${comment.createdDate}: ${comment.text}\n\n`;
          }
          
          formattedItems.push({
            json: {
              'ID': workItem.id,
              'Work Item Type': fields['System.WorkItemType'],
              'Title': fields['System.Title'],
              'Assigned To': fields['System.AssignedTo']?.displayName || 'Unassigned',
              'State': fields['System.State'],
              'Tags': fields['System.Tags'] || '',
              'Area Path': fields['System.AreaPath'],
              'Changed Date': fields['System.ChangedDate'],
              'Description': fields['System.Description'] || '',
              'Comment Count': fields['System.CommentCount'] || 0,
              'Comments': commentsText,
              'Comments Detail': commentsList,
              'Is Original Task': isOriginalTask,
              'Relation Type': isOriginalTask ? 'Original Task (Yesterday)' : 'Connected Item'
            }
          });
        }
      }
    }
    return formattedItems;
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "TEST", "UI"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:contract
  - API_CONTRACT:response
  - IMPLEMENT:code
  - TEST:test
  - UI:ui

---

## XIIGEN-SESSION-1000-0166 - Azure DevOps task retrieval with n8n

```
source_id: xiigen-session-1000-0166
source_title: "Azure DevOps task retrieval with n8n"
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
  conversation_uuid: "b464f7ad-93ce-45ae-8d1d-7d9423f23fa9"
  conversation_name: "Azure DevOps task retrieval with n8n"
  human_message_uuid: "7bc67616-e427-462e-92dd-b41b7d2d3106"
  human_message_index: 38
  message_created_at: "2025-10-24T06:33:00.510704Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0166"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    I get an error:
    Cannot assign to read only property 'name' of object 'Error: Referenced node is unexecuted'
    My step names:
    Step 3
    Get all the connected tasks ids
    Step 4
    Get all the tasks with relations needed data
    Formatted comments result
    Format comments data
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0166-prompt"
  SourcePath: "claude-export/batch-0000/b464f7ad-93ce-45ae-8d1d-7d9423f23fa9/messages/7bc67616-e427-462e-92dd-b41b7d2d3106"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Backend Services"
  CompiledText: |
    I get an error:
    Cannot assign to read only property 'name' of object 'Error: Referenced node is unexecuted'
    My step names:
    Step 3
    Get all the connected tasks ids
    Step 4
    Get all the tasks with relations needed data
    Formatted comments result
    Format comments data
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - IMPLEMENT:error

---

## XIIGEN-SESSION-1000-0167 - Azure DevOps task retrieval with n8n

```
source_id: xiigen-session-1000-0167
source_title: "Azure DevOps task retrieval with n8n"
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
  conversation_uuid: "b464f7ad-93ce-45ae-8d1d-7d9423f23fa9"
  conversation_name: "Azure DevOps task retrieval with n8n"
  human_message_uuid: "8c8e9531-dad0-4463-868a-e861d5dde22a"
  human_message_index: 40
  message_created_at: "2025-10-24T06:34:37.832825Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0167"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    for this code:
    const taskIds = $('Get all the connected tasks ids').first().json.taskIds;
    const workItems = $('Get all the tasks with relations needed data').all();
    const commentsData = $('Format comments data').all();  // Replace with your comments node name
    const updatesData = $input.all();  // Current input with updates
    // Create a map of comments by work item ID
    const commentsMap = {};
    for (const item of commentsData) {
      const workItemId = item.json.workItemId || item.json.id;
      const comments = item.json.comments || item.json;
      
      if (comments && comments.comments) {
        commentsMap[workItemId] = comments.comments;
      }
    }
    // Create a map of updates by work item ID
    const updatesMap = {};
    for (const item of updatesData) {
      const workItemId = item.json.ID || item.json.workItemId;
      updatesMap[workItemId] = {
        todayChangesCount: item.json['Today Changes Count'] || 0,
        todayChanges: item.json['Today Changes'] || '',
        todayChangesDetail: item.json['Today Changes Detail'] || []
      };
    }
    const formattedItems = [];
    // Process work items
    for (const item of workItems) {
      const response = item.json;
      
      if (response.value && Array.isArray(response.value)) {
        for (const workItem of response.value) {
          const fields = workItem.fields;
          const isOriginalTask = taskIds.includes(workItem.id);
          
          // Get comments for this work item
          const comments = commentsMap[workItem.id] || [];
          let commentsText = '';
          let commentsList = [];
          
          for (const comment of comments) {
            commentsList.push({
              id: comment.id,
              text: comment.text,
              createdBy: comment.createdBy?.displayName || 'Unknown',
              createdDate: comment.createdDate
            });
            
            commentsText += `[${comment.createdBy?.displayName}] ${comment.createdDate}: ${comment.text}\n\n`;
          }
          
          // Get updates for this work item
          const updates = updatesMap[workItem.id] || {
            todayChangesCount: 0,
            todayChanges: '',
            todayChangesDetail: []
          };
          
          formattedItems.push({
            json: {
              'ID': workItem.id,
              'Work Item Type': fields['System.WorkItemType'],
              'Title': fields['System.Title'],
              'Assigned To': fields['System.AssignedTo']?.displayName || 'Unassigned',
              'State': fields['System.State'],
              'Tags': fields['System.Tags'] || '',
              'Area Path': fields['System.AreaPath'],
              'Changed Date': fields['System.ChangedDate'],
              'Description': fields['System.Description'] || '',
              'Comment Count': fields['System.CommentCount'] || 0,
              'Comments': commentsText,
              'Comments Detail': commentsList,
              'Today Changes Count': updates.todayChangesCount,
              'Today Changes': updates.todayChanges,
              'Today Changes Detail': updates.todayChangesDetail,
              'Is Original Task': isOriginalTask,
              'Relation Type': isOriginalTask ? 'Original Task (Yesterday)' : 'Connected Item'
            }
          });
        }
      }
    }
    return formattedItems;
    
    and steps 
    Step 3
    Get all the connected tasks ids
    Step 4
    Get all the tasks with relations needed data
    Formatted comments result
    Format comments data
    I get Cannot assign to read only property 'name' of object 'Error: Referenced node is unexecuted'
    
    why?
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0167-prompt"
  SourcePath: "claude-export/batch-0000/b464f7ad-93ce-45ae-8d1d-7d9423f23fa9/messages/8c8e9531-dad0-4463-868a-e861d5dde22a"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    for this code:
    const taskIds = $('Get all the connected tasks ids').first().json.taskIds;
    const workItems = $('Get all the tasks with relations needed data').all();
    const commentsData = $('Format comments data').all();  // Replace with your comments node name
    const updatesData = $input.all();  // Current input with updates
    // Create a map of comments by work item ID
    const commentsMap = {};
    for (const item of commentsData) {
      const workItemId = item.json.workItemId || item.json.id;
      const comments = item.json.comments || item.json;
      
      if (comments && comments.comments) {
        commentsMap[workItemId] = comments.comments;
      }
    }
    // Create a map of updates by work item ID
    const updatesMap = {};
    for (const item of updatesData) {
      const workItemId = item.json.ID || item.json.workItemId;
      updatesMap[workItemId] = {
        todayChangesCount: item.json['Today Changes Count'] || 0,
        todayChanges: item.json['Today Changes'] || '',
        todayChangesDetail: item.json['Today Changes Detail'] || []
      };
    }
    const formattedItems = [];
    // Process work items
    for (const item of workItems) {
      const response = item.json;
      
      if (response.value && Array.isArray(response.value)) {
        for (const workItem of response.value) {
          const fields = workItem.fields;
          const isOriginalTask = taskIds.includes(workItem.id);
          
          // Get comments for this work item
          const comments = commentsMap[workItem.id] || [];
          let commentsText = '';
          let commentsList = [];
          
          for (const comment of comments) {
            commentsList.push({
              id: comment.id,
              text: comment.text,
              createdBy: comment.createdBy?.displayName || 'Unknown',
              createdDate: comment.createdDate
            });
            
            commentsText += `[${comment.createdBy?.displayName}] ${comment.createdDate}: ${comment.text}\n\n`;
          }
          
          // Get updates for this work item
          const updates = updatesMap[workItem.id] || {
            todayChangesCount: 0,
            todayChanges: '',
            todayChangesDetail: []
          };
          
          formattedItems.push({
            json: {
              'ID': workItem.id,
              'Work Item Type': fields['System.WorkItemType'],
              'Title': fields['System.Title'],
              'Assigned To': fields['System.AssignedTo']?.displayName || 'Unassigned',
              'State': fields['System.State'],
              'Tags': fields['System.Tags'] || '',
              'Area Path': fields['System.AreaPath'],
              'Changed Date': fields['System.ChangedDate'],
              'Description': fields['System.Description'] || '',
              'Comment Count': fields['System.CommentCount'] || 0,
              'Comments': commentsText,
              'Comments Detail': commentsList,
              'Today Changes Count': updates.todayChangesCount,
              'Today Changes': updates.todayChanges,
              'Today Changes Detail': updates.todayChangesDetail,
              'Is Original Task': isOriginalTask,
              'Relation Type': isOriginalTask ? 'Original Task (Yesterday)' : 'Connected Item'
            }
          });
        }
      }
    }
    return formattedItems;
    
    and steps 
    Step 3
    Get all the connected tasks ids
    Step 4
    Get all the tasks with relations needed data
    Formatted comments result
    Format comments data
    I get Cannot assign to read only property 'name' of object 'Error: Referenced node is unexecuted'
    
    why?
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:response
  - IMPLEMENT:code
  - IMPLEMENT:error

---

## XIIGEN-SESSION-1000-0168 - Azure DevOps task retrieval with n8n

```
source_id: xiigen-session-1000-0168
source_title: "Azure DevOps task retrieval with n8n"
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
  conversation_uuid: "b464f7ad-93ce-45ae-8d1d-7d9423f23fa9"
  conversation_name: "Azure DevOps task retrieval with n8n"
  human_message_uuid: "4e155d3d-837d-4b4c-bb82-55bd4c7b04a9"
  human_message_index: 48
  message_created_at: "2025-10-24T07:02:52.930876Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0168"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    This is my last code node  code and the json of the project, please let me understand again how to fix the code of it
    const taskIds = $('Get all the connected tasks ids').first().json.taskIds;
    const workItems = $('Get all the tasks with relations needed data').all();
    const commentsData = $('Format comments data').all();  // Replace with your comments node name
    const updatesData = $input.all();  // Current input with updates
    // Create a map of comments by work item ID
    const commentsMap = {};
    for (const item of commentsData) {
      const workItemId = item.json.workItemId || item.json.id;
      const comments = item.json.comments || item.json;
      
      if (comments && comments.comments) {
        commentsMap[workItemId] = comments.comments;
      }
    }
    // Create a map of updates by work item ID
    const updatesMap = {};
    for (const item of updatesData) {
      const workItemId = item.json.ID || item.json.workItemId;
      updatesMap[workItemId] = {
        todayChangesCount: item.json['Today Changes Count'] || 0,
        todayChanges: item.json['Today Changes'] || '',
        todayChangesDetail: item.json['Today Changes Detail'] || []
      };
    }
    const formattedItems = [];
    // Process work items
    for (const item of workItems) {
      const response = item.json;
      
      if (response.value && Array.isArray(response.value)) {
        for (const workItem of response.value) {
          const fields = workItem.fields;
          const isOriginalTask = taskIds.includes(workItem.id);
          
          // Get comments for this work item
          const comments = commentsMap[workItem.id] || [];
          let commentsText = '';
          let commentsList = [];
          
          for (const comment of comments) {
            commentsList.push({
              id: comment.id,
              text: comment.text,
              createdBy: comment.createdBy?.displayName || 'Unknown',
              createdDate: comment.createdDate
            });
            
            commentsText += `[${comment.createdBy?.displayName}] ${comment.createdDate}: ${comment.text}\n\n`;
          }
          
          // Get updates for this work item
          const updates = updatesMap[workItem.id] || {
            todayChangesCount: 0,
            todayChanges: '',
            todayChangesDetail: []
          };
          
          formattedItems.push({
            json: {
              'ID': workItem.id,
              'Work Item Type': fields['System.WorkItemType'],
              'Title': fields['System.Title'],
              'Assigned To': fields['System.AssignedTo']?.displayName || 'Unassigned',
              'State': fields['System.State'],
              'Tags': fields['System.Tags'] || '',
              'Area Path': fields['System.AreaPath'],
              'Changed Date': fields['System.ChangedDate'],
              'Description': fields['System.Description'] || '',
              'Comment Count': fields['System.CommentCount'] || 0,
              'Comments': commentsText,
              'Comments Detail': commentsList,
              'Today Changes Count': updates.todayChangesCount,
              'Today Changes': updates.todayChanges,
              'Today Changes Detail': updates.todayChangesDetail,
              'Is Original Task': isOriginalTask,
              'Relation Type': isOriginalTask ? 'Original Task (Yesterday)' : 'Connected Item'
            }
          });
        }
      }
    }
    return formattedItems;
    
    and the input schema of this node:
    Merge
    134 items
    ID
    80
    Work Item Type
    Test Case
    Title
    בדיקת ממש"ק משתמש - ( UI ) מסך רשימת ההסכמים
    Assigned To
    Tatiana Peysik
    State
    Design
    Tags
    רשימת הסכמים
    Area Path
    Keshet\Tamlugim
    Changed Date
    2025-08-17T08:54:34.417Z
    Description
    Comment Count
    0
    Is Original Task
    false
    Relation Type
    Connected Item
    Today Changes Count
    0
    Today Changes
    Today Changes Detail
    Get all tasks details
    67 items
    Format comments data
    67 items
    Get all the tasks with relations needed data
    1 item
    Filter Yesterday only
    37 items
    Get all the connected tasks ids
    1 item
    Merge Comments and history of changes
    134 items
    Get all the tasks with relations
    1 item
    Get comments
    67 items
    History of changes
    67 items
    Get all the tasks
    1 item
    Format input into array of ids
    67 items
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0168-prompt"
  SourcePath: "claude-export/batch-0000/b464f7ad-93ce-45ae-8d1d-7d9423f23fa9/messages/4e155d3d-837d-4b4c-bb82-55bd4c7b04a9"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    This is my last code node  code and the json of the project, please let me understand again how to fix the code of it
    const taskIds = $('Get all the connected tasks ids').first().json.taskIds;
    const workItems = $('Get all the tasks with relations needed data').all();
    const commentsData = $('Format comments data').all();  // Replace with your comments node name
    const updatesData = $input.all();  // Current input with updates
    // Create a map of comments by work item ID
    const commentsMap = {};
    for (const item of commentsData) {
      const workItemId = item.json.workItemId || item.json.id;
      const comments = item.json.comments || item.json;
      
      if (comments && comments.comments) {
        commentsMap[workItemId] = comments.comments;
      }
    }
    // Create a map of updates by work item ID
    const updatesMap = {};
    for (const item of updatesData) {
      const workItemId = item.json.ID || item.json.workItemId;
      updatesMap[workItemId] = {
        todayChangesCount: item.json['Today Changes Count'] || 0,
        todayChanges: item.json['Today Changes'] || '',
        todayChangesDetail: item.json['Today Changes Detail'] || []
      };
    }
    const formattedItems = [];
    // Process work items
    for (const item of workItems) {
      const response = item.json;
      
      if (response.value && Array.isArray(response.value)) {
        for (const workItem of response.value) {
          const fields = workItem.fields;
          const isOriginalTask = taskIds.includes(workItem.id);
          
          // Get comments for this work item
          const comments = commentsMap[workItem.id] || [];
          let commentsText = '';
          let commentsList = [];
          
          for (const comment of comments) {
            commentsList.push({
              id: comment.id,
              text: comment.text,
              createdBy: comment.createdBy?.displayName || 'Unknown',
              createdDate: comment.createdDate
            });
            
            commentsText += `[${comment.createdBy?.displayName}] ${comment.createdDate}: ${comment.text}\n\n`;
          }
          
          // Get updates for this work item
          const updates = updatesMap[workItem.id] || {
            todayChangesCount: 0,
            todayChanges: '',
            todayChangesDetail: []
          };
          
          formattedItems.push({
            json: {
              'ID': workItem.id,
              'Work Item Type': fields['System.WorkItemType'],
              'Title': fields['System.Title'],
              'Assigned To': fields['System.AssignedTo']?.displayName || 'Unassigned',
              'State': fields['System.State'],
              'Tags': fields['System.Tags'] || '',
              'Area Path': fields['System.AreaPath'],
              'Changed Date': fields['System.ChangedDate'],
              'Description': fields['System.Description'] || '',
              'Comment Count': fields['System.CommentCount'] || 0,
              'Comments': commentsText,
              'Comments Detail': commentsList,
              'Today Changes Count': updates.todayChangesCount,
              'Today Changes': updates.todayChanges,
              'Today Changes Detail': updates.todayChangesDetail,
              'Is Original Task': isOriginalTask,
              'Relation Type': isOriginalTask ? 'Original Task (Yesterday)' : 'Connected Item'
            }
          });
        }
      }
    }
    return formattedItems;
    
    and the input schema of this node:
    Merge
    134 items
    ID
    80
    Work Item Type
    Test Case
    Title
    בדיקת ממש"ק משתמש - ( UI ) מסך רשימת ההסכמים
    Assigned To
    Tatiana Peysik
    State
    Design
    Tags
    רשימת הסכמים
    Area Path
    Keshet\Tamlugim
    Changed Date
    2025-08-17T08:54:34.417Z
    Description
    Comment Count
    0
    Is Original Task
    false
    Relation Type
    Connected Item
    Today Changes Count
    0
    Today Changes
    Today Changes Detail
    Get all tasks details
    67 items
    Format comments data
    67 items
    Get all the tasks with relations needed data
    1 item
    Filter Yesterday only
    37 items
    Get all the connected tasks ids
    1 item
    Merge Comments and history of changes
    134 items
    Get all the tasks with relations
    1 item
    Get comments
    67 items
    History of changes
    67 items
    Get all the tasks
    1 item
    Format input into array of ids
    67 items
  CompiledPredicateIds: ["API_CONTRACT", "ARCHITECTURE", "IMPLEMENT", "TEST", "UI"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:schema
  - API_CONTRACT:response
  - ARCHITECTURE:project
  - IMPLEMENT:code
  - IMPLEMENT:fix
  - TEST:test
  - UI:ui

---

## XIIGEN-SESSION-1000-0169 - Azure DevOps task retrieval with n8n

```
source_id: xiigen-session-1000-0169
source_title: "Azure DevOps task retrieval with n8n"
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
  conversation_uuid: "b464f7ad-93ce-45ae-8d1d-7d9423f23fa9"
  conversation_name: "Azure DevOps task retrieval with n8n"
  human_message_uuid: "413ba8ed-4e92-40c6-a19f-ac2e0a3d3182"
  human_message_index: 54
  message_created_at: "2025-10-24T07:20:19.176274Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0169"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    I have an input query {
      "query": "SELECT [System.Id] FROM WorkItems WHERE [System.WorkItemType] = 'Task' AND ([System.CreatedDate] >= @Today-1 AND [System.CreatedDate] < @Today) OR ([System.ChangedDate] >= @Today-1 AND [System.ChangedDate] < @Today)"
    }
    
    and I want to use the code input Input.Variables
    1 item
    fromDate
    2025-10-23
    toDate
    2025-10-24
    fromDateISO
    2025-10-23T00:00:00.000Z
    toDateISO
    2025-10-24T00:00:00.000Z
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0169-prompt"
  SourcePath: "claude-export/batch-0000/b464f7ad-93ce-45ae-8d1d-7d9423f23fa9/messages/413ba8ed-4e92-40c6-a19f-ac2e0a3d3182"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Backend Services"
  CompiledText: |
    I have an input query {
      "query": "SELECT [System.Id] FROM WorkItems WHERE [System.WorkItemType] = 'Task' AND ([System.CreatedDate] >= @Today-1 AND [System.CreatedDate] < @Today) OR ([System.ChangedDate] >= @Today-1 AND [System.ChangedDate] < @Today)"
    }
    
    and I want to use the code input Input.Variables
    1 item
    fromDate
    2025-10-23
    toDate
    2025-10-24
    fromDateISO
    2025-10-23T00:00:00.000Z
    toDateISO
    2025-10-24T00:00:00.000Z
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - IMPLEMENT:code

---

## XIIGEN-SESSION-1000-0170 - Azure DevOps task retrieval with n8n

```
source_id: xiigen-session-1000-0170
source_title: "Azure DevOps task retrieval with n8n"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: external_source_integration
domain: CODE_Q
phase: 3
sub_phase: 3.3
task_type: api-contract
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "b464f7ad-93ce-45ae-8d1d-7d9423f23fa9"
  conversation_name: "Azure DevOps task retrieval with n8n"
  human_message_uuid: "46c89f8e-19ec-40d5-94bc-f56e2982eda5"
  human_message_index: 56
  message_created_at: "2025-10-24T07:23:19.608432Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0170"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    I would like to edit the code to filters only from date to to date
    
    // Get yesterday's date range (start and end of yesterday in UTC)
    const now = new Date();
    const yesterdayStart = new Date(Date.UTC(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDate() - 1, 0, 0, 0, 0));
    const yesterdayEnd = new Date(Date.UTC(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDate() - 1, 23, 59, 59, 999));
    console.log('Filtering for date range:');
    console.log('Start:', yesterdayStart.toISOString());
    console.log('End:', yesterdayEnd.toISOString());
    const filteredItems = [];
    for (const item of $input.all()) {
      const workItemId = item.json.workItemId;
      
      // Get all updates from the response
      let allUpdates = [];
      if (item.json.value && Array.isArray(item.json.value)) {
        allUpdates = item.json.value;
      } else if (item.json.revisedDate) {
        // Single update object
        allUpdates = [item.json];
      }
      
      // Filter for yesterday's updates only
      const yesterdayUpdates = allUpdates.filter(update => {
        if (!update.revisedDate) return false;
        
        const revisedDate = new Date(update.revisedDate);
        return revisedDate >= yesterdayStart && revisedDate <= yesterdayEnd;
      });
      
      // Only add items that have updates yesterday
      if (yesterdayUpdates.length > 0) {
        filteredItems.push({
          json: {
            workItemId: workItemId,
            updatesCount: yesterdayUpdates.length,
            updates: yesterdayUpdates
          }
        });
      }
    }
    console.log(`Total items with yesterday's updates: ${filteredItems.length}`);
    return filteredItems;
    
    Input.Variables
    1 item
    fromDate
    2025-10-23
    toDate
    2025-10-24
    fromDateISO
    2025-10-23T00:00:00.000Z
    toDateISO
    2025-10-24T00:00:00.000Z
    
    My input scheme:
    Get comments
    67 items
    totalCount
    0
    count
    0
    comments
    comments[0]
    mentions
    mentions[0]
    artifactId
    22a4180a-5c3c-6738-9b76-2808ecf3bde9
    artifactType
    Person
    commentId
    14440639
    targetId
    22a4180a-5c3c-6738-9b76-2808ecf3bde9
    workItemId
    7561
    id
    14189761
    version
    1
    text
    TF237027: Created from clone operation. Clone Operation Id: 140 Source Work Item Id: 7560
    createdBy
    displayName
    Microsoft.VisualStudio.Services.TFS
    url
    https://spsprodweu3.vssps.visualstudio.com/A8eaa194f-8d18-456e-9ed5-e37b70b9c324/_apis/Identities/00000002-0000-8888-8000-000000000000
    _links
    avatar
    href
    https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/s2s.MDAwMDAwMDItMDAwMC04ODg4LTgwMDAtMDAwMDAwMDAwMDAwQDJjODk1OTA4LTA0ZTAtNDk1Mi04OWZkLTU0YjAwNDZkNjI4OA
    id
    00000002-0000-8888-8000-000000000000
    uniqueName
    00000002-0000-8888-8000-000000000000@2c895908-04e0-4952-89fd-54b0046d6288
    imageUrl
    https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/s2s.MDAwMDAwMDItMDAwMC04ODg4LTgwMDAtMDAwMDAwMDAwMDAwQDJjODk1OTA4LTA0ZTAtNDk1Mi04OWZkLTU0YjAwNDZkNjI4OA
    descriptor
    s2s.MDAwMDAwMDItMDAwMC04ODg4LTgwMDAtMDAwMDAwMDAwMDAwQDJjODk1OTA4LTA0ZTAtNDk1Mi04OWZkLTU0YjAwNDZkNjI4OA
    createdDate
    2025-04-10T11:19:20.34Z
    modifiedBy
    displayName
    Microsoft.VisualStudio.Services.TFS
    Format input into array of ids
    67 items
    Get all the tasks with relations needed data
    1 item
    Get all the connected tasks ids
    1 item
    Get all the tasks with relations
    1 item
    Get all the tasks
    1 item
    Input.Variables
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0170-prompt"
  SourcePath: "claude-export/batch-0000/b464f7ad-93ce-45ae-8d1d-7d9423f23fa9/messages/46c89f8e-19ec-40d5-94bc-f56e2982eda5"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    I would like to edit the code to filters only from date to to date
    
    // Get yesterday's date range (start and end of yesterday in UTC)
    const now = new Date();
    const yesterdayStart = new Date(Date.UTC(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDate() - 1, 0, 0, 0, 0));
    const yesterdayEnd = new Date(Date.UTC(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDate() - 1, 23, 59, 59, 999));
    console.log('Filtering for date range:');
    console.log('Start:', yesterdayStart.toISOString());
    console.log('End:', yesterdayEnd.toISOString());
    const filteredItems = [];
    for (const item of $input.all()) {
      const workItemId = item.json.workItemId;
      
      // Get all updates from the response
      let allUpdates = [];
      if (item.json.value && Array.isArray(item.json.value)) {
        allUpdates = item.json.value;
      } else if (item.json.revisedDate) {
        // Single update object
        allUpdates = [item.json];
      }
      
      // Filter for yesterday's updates only
      const yesterdayUpdates = allUpdates.filter(update => {
        if (!update.revisedDate) return false;
        
        const revisedDate = new Date(update.revisedDate);
        return revisedDate >= yesterdayStart && revisedDate <= yesterdayEnd;
      });
      
      // Only add items that have updates yesterday
      if (yesterdayUpdates.length > 0) {
        filteredItems.push({
          json: {
            workItemId: workItemId,
            updatesCount: yesterdayUpdates.length,
            updates: yesterdayUpdates
          }
        });
      }
    }
    console.log(`Total items with yesterday's updates: ${filteredItems.length}`);
    return filteredItems;
    
    Input.Variables
    1 item
    fromDate
    2025-10-23
    toDate
    2025-10-24
    fromDateISO
    2025-10-23T00:00:00.000Z
    toDateISO
    2025-10-24T00:00:00.000Z
    
    My input scheme:
    Get comments
    67 items
    totalCount
    0
    count
    0
    comments
    comments[0]
    mentions
    mentions[0]
    artifactId
    22a4180a-5c3c-6738-9b76-2808ecf3bde9
    artifactType
    Person
    commentId
    14440639
    targetId
    22a4180a-5c3c-6738-9b76-2808ecf3bde9
    workItemId
    7561
    id
    14189761
    version
    1
    text
    TF237027: Created from clone operation. Clone Operation Id: 140 Source Work Item Id: 7560
    createdBy
    displayName
    Microsoft.VisualStudio.Services.TFS
    url
    https://spsprodweu3.vssps.visualstudio.com/A8eaa194f-8d18-456e-9ed5-e37b70b9c324/_apis/Identities/00000002-0000-8888-8000-000000000000
    _links
    avatar
    href
    https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/s2s.MDAwMDAwMDItMDAwMC04ODg4LTgwMDAtMDAwMDAwMDAwMDAwQDJjODk1OTA4LTA0ZTAtNDk1Mi04OWZkLTU0YjAwNDZkNjI4OA
    id
    00000002-0000-8888-8000-000000000000
    uniqueName
    00000002-0000-8888-8000-000000000000@2c895908-04e0-4952-89fd-54b0046d6288
    imageUrl
    https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/s2s.MDAwMDAwMDItMDAwMC04ODg4LTgwMDAtMDAwMDAwMDAwMDAwQDJjODk1OTA4LTA0ZTAtNDk1Mi04OWZkLTU0YjAwNDZkNjI4OA
    descriptor
    s2s.MDAwMDAwMDItMDAwMC04ODg4LTgwMDAtMDAwMDAwMDAwMDAwQDJjODk1OTA4LTA0ZTAtNDk1Mi04OWZkLTU0YjAwNDZkNjI4OA
    createdDate
    2025-04-10T11:19:20.34Z
    modifiedBy
    displayName
    Microsoft.VisualStudio.Services.TFS
    Format input into array of ids
    67 items
    Get all the tasks with relations needed data
    1 item
    Get all the connected tasks ids
    1 item
    Get all the tasks with relations
    1 item
    Get all the tasks
    1 item
    Input.Variables
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:api
  - API_CONTRACT:response
  - IMPLEMENT:code
  - IMPLEMENT:log

---

## XIIGEN-SESSION-1000-0171 - Azure DevOps task retrieval with n8n

```
source_id: xiigen-session-1000-0171
source_title: "Azure DevOps task retrieval with n8n"
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
  conversation_uuid: "b464f7ad-93ce-45ae-8d1d-7d9423f23fa9"
  conversation_name: "Azure DevOps task retrieval with n8n"
  human_message_uuid: "782fa87d-c24f-495c-9738-95307f54ed02"
  human_message_index: 58
  message_created_at: "2025-10-24T07:33:30.025581Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0171"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    This is my comment input:
    "id": "fdda7713-4f50-6a57-95a5-fef91774f4eb",
    "uniqueName": "sivan.shtrausberg@keshet-tv.com",
    "imageUrl": "https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/aad.ZmRkYTc3MTMtNGY1MC03YTU3LTk1YTUtZmVmOTE3NzRmNGVi",
    "descriptor": "aad.ZmRkYTc3MTMtNGY1MC03YTU3LTk1YTUtZmVmOTE3NzRmNGVi"
    },
    "createdDate": "2025-08-11T06:16:22.053Z",
    "modifiedBy": {
    "displayName": "Sivan Shtrausberg",
    "url": "https://spsprodweu3.vssps.visualstudio.com/A8eaa194f-8d18-456e-9ed5-e37b70b9c324/_apis/Identities/fdda7713-4f50-6a57-95a5-fef91774f4eb",
    "_links": {
    "avatar": {
    "href": "https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/aad.ZmRkYTc3MTMtNGY1MC03YTU3LTk1YTUtZmVmOTE3NzRmNGVi"
    }
    },
    "id": "fdda7713-4f50-6a57-95a5-fef91774f4eb",
    "uniqueName": "sivan.shtrausberg@keshet-tv.com",
    "imageUrl": "https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/aad.ZmRkYTc3MTMtNGY1MC03YTU3LTk1YTUtZmVmOTE3NzRmNGVi",
    "descriptor": "aad.ZmRkYTc3MTMtNGY1MC03YTU3LTk1YTUtZmVmOTE3NzRmNGVi"
    },
    "modifiedDate": "2025-08-11T06:16:22.053Z",
    "format": "html",
    "renderedText": "",
    "url": "https://dev.azure.com/KESHETDEV/7a929425-38ed-4005-9b0a-d1196cd6b4e9/_apis/wit/workItems/4263/comments/14460296"
    }
    ]
    },
    {
    "totalCount": 0,
    "count": 0,
    "comments": [
    ]
    },
    {
    "totalCount": 0,
    "count": 0,....
    
    I want to filter only modifiedDate or CreatedDate from Input start date and end date. 
    please prepare the code for that
    
    Input.Variables
    1 item
    fromDate
    2025-10-17
    toDate
    2025-10-24
    fromDateISO
    2025-10-17T00:00:00.000Z
    toDateISO
    2025-10-24T00:00:00.000Z
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0171-prompt"
  SourcePath: "claude-export/batch-0000/b464f7ad-93ce-45ae-8d1d-7d9423f23fa9/messages/782fa87d-c24f-495c-9738-95307f54ed02"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    This is my comment input:
    "id": "fdda7713-4f50-6a57-95a5-fef91774f4eb",
    "uniqueName": "sivan.shtrausberg@keshet-tv.com",
    "imageUrl": "https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/aad.ZmRkYTc3MTMtNGY1MC03YTU3LTk1YTUtZmVmOTE3NzRmNGVi",
    "descriptor": "aad.ZmRkYTc3MTMtNGY1MC03YTU3LTk1YTUtZmVmOTE3NzRmNGVi"
    },
    "createdDate": "2025-08-11T06:16:22.053Z",
    "modifiedBy": {
    "displayName": "Sivan Shtrausberg",
    "url": "https://spsprodweu3.vssps.visualstudio.com/A8eaa194f-8d18-456e-9ed5-e37b70b9c324/_apis/Identities/fdda7713-4f50-6a57-95a5-fef91774f4eb",
    "_links": {
    "avatar": {
    "href": "https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/aad.ZmRkYTc3MTMtNGY1MC03YTU3LTk1YTUtZmVmOTE3NzRmNGVi"
    }
    },
    "id": "fdda7713-4f50-6a57-95a5-fef91774f4eb",
    "uniqueName": "sivan.shtrausberg@keshet-tv.com",
    "imageUrl": "https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/aad.ZmRkYTc3MTMtNGY1MC03YTU3LTk1YTUtZmVmOTE3NzRmNGVi",
    "descriptor": "aad.ZmRkYTc3MTMtNGY1MC03YTU3LTk1YTUtZmVmOTE3NzRmNGVi"
    },
    "modifiedDate": "2025-08-11T06:16:22.053Z",
    "format": "html",
    "renderedText": "",
    "url": "https://dev.azure.com/KESHETDEV/7a929425-38ed-4005-9b0a-d1196cd6b4e9/_apis/wit/workItems/4263/comments/14460296"
    }
    ]
    },
    {
    "totalCount": 0,
    "count": 0,
    "comments": [
    ]
    },
    {
    "totalCount": 0,
    "count": 0,....
    
    I want to filter only modifiedDate or CreatedDate from Input start date and end date. 
    please prepare the code for that
    
    Input.Variables
    1 item
    fromDate
    2025-10-17
    toDate
    2025-10-24
    fromDateISO
    2025-10-17T00:00:00.000Z
    toDateISO
    2025-10-24T00:00:00.000Z
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:api
  - IMPLEMENT:code

---

## XIIGEN-SESSION-1000-0172 - Azure DevOps task retrieval with n8n

```
source_id: xiigen-session-1000-0172
source_title: "Azure DevOps task retrieval with n8n"
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
  conversation_uuid: "b464f7ad-93ce-45ae-8d1d-7d9423f23fa9"
  conversation_name: "Azure DevOps task retrieval with n8n"
  human_message_uuid: "ae9c02d0-7e42-4aeb-aa15-085f2e6dbf30"
  human_message_index: 60
  message_created_at: "2025-10-24T07:35:45.819865Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0172"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    This is my history of changes, I would like to filter only from start to end date like in pervious case
    195 items
    [
    {
    "count": 7,
    "value": [
    {
    "id": 1,
    "workItemId": 654,
    "rev": 1,
    "revisedBy": {
    "id": "83ac2fff-95df-64c8-9bd6-ac394552d1b7",
    "name": "Yevgeny San <Yevgeny.San@keshet-tv.com>",
    "displayName": "Yevgeny San",
    "url": "https://spsprodweu3.vssps.visualstudio.com/A8eaa194f-8d18-456e-9ed5-e37b70b9c324/_apis/Identities/83ac2fff-95df-64c8-9bd6-ac394552d1b7",
    "_links": {
    "avatar": {
    "href": "https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/aad.ODNhYzJmZmYtOTVkZi03NGM4LTliZDYtYWMzOTQ1NTJkMWI3"
    }
    },
    "uniqueName": "Yevgeny.San@keshet-tv.com",
    "imageUrl": "https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/aad.ODNhYzJmZmYtOTVkZi03NGM4LTliZDYtYWMzOTQ1NTJkMWI3",
    "inactive": true,
    "descriptor": "aad.ODNhYzJmZmYtOTVkZi03NGM4LTliZDYtYWMzOTQ1NTJkMWI3"
    },
    "revisedDate": "2021-07-11T07:01:04.003Z",
    "fields": {
    "System.Id": {
    "newValue": 654
    },
    "System.AreaId": {
    "newValue": 6
    },
    "System.NodeName": {
    "newValue": "Keshet"
    },
    "System.AreaLevel1": {
    "newValue": "Keshet"
    },
    "System.Rev": {
    "newValue": 1
    },
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0172-prompt"
  SourcePath: "claude-export/batch-0000/b464f7ad-93ce-45ae-8d1d-7d9423f23fa9/messages/ae9c02d0-7e42-4aeb-aa15-085f2e6dbf30"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    This is my history of changes, I would like to filter only from start to end date like in pervious case
    195 items
    [
    {
    "count": 7,
    "value": [
    {
    "id": 1,
    "workItemId": 654,
    "rev": 1,
    "revisedBy": {
    "id": "83ac2fff-95df-64c8-9bd6-ac394552d1b7",
    "name": "Yevgeny San <Yevgeny.San@keshet-tv.com>",
    "displayName": "Yevgeny San",
    "url": "https://spsprodweu3.vssps.visualstudio.com/A8eaa194f-8d18-456e-9ed5-e37b70b9c324/_apis/Identities/83ac2fff-95df-64c8-9bd6-ac394552d1b7",
    "_links": {
    "avatar": {
    "href": "https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/aad.ODNhYzJmZmYtOTVkZi03NGM4LTliZDYtYWMzOTQ1NTJkMWI3"
    }
    },
    "uniqueName": "Yevgeny.San@keshet-tv.com",
    "imageUrl": "https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/aad.ODNhYzJmZmYtOTVkZi03NGM4LTliZDYtYWMzOTQ1NTJkMWI3",
    "inactive": true,
    "descriptor": "aad.ODNhYzJmZmYtOTVkZi03NGM4LTliZDYtYWMzOTQ1NTJkMWI3"
    },
    "revisedDate": "2021-07-11T07:01:04.003Z",
    "fields": {
    "System.Id": {
    "newValue": 654
    },
    "System.AreaId": {
    "newValue": 6
    },
    "System.NodeName": {
    "newValue": "Keshet"
    },
    "System.AreaLevel1": {
    "newValue": "Keshet"
    },
    "System.Rev": {
    "newValue": 1
    },
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:api

---

## XIIGEN-SESSION-1000-0173 - Azure DevOps task retrieval with n8n

```
source_id: xiigen-session-1000-0173
source_title: "Azure DevOps task retrieval with n8n"
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
  conversation_uuid: "b464f7ad-93ce-45ae-8d1d-7d9423f23fa9"
  conversation_name: "Azure DevOps task retrieval with n8n"
  human_message_uuid: "827edc82-2c0e-4f71-bad0-d7979ead0304"
  human_message_index: 62
  message_created_at: "2025-10-24T07:44:09.360451Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0173"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    I have updated workflow - removed merge and code, I would like to combine the tasks, related, comments and changes unite into one structure, where each task will have it's comment and changes
    how do I do that?
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0173-prompt"
  SourcePath: "claude-export/batch-0000/b464f7ad-93ce-45ae-8d1d-7d9423f23fa9/messages/827edc82-2c0e-4f71-bad0-d7979ead0304"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Backend Services"
  CompiledText: |
    I have updated workflow - removed merge and code, I would like to combine the tasks, related, comments and changes unite into one structure, where each task will have it's comment and changes
    how do I do that?
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - IMPLEMENT:code

---

## XIIGEN-SESSION-1000-0174 - Azure DevOps task retrieval with n8n

```
source_id: xiigen-session-1000-0174
source_title: "Azure DevOps task retrieval with n8n"
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
  conversation_uuid: "b464f7ad-93ce-45ae-8d1d-7d9423f23fa9"
  conversation_name: "Azure DevOps task retrieval with n8n"
  human_message_uuid: "61711730-f832-4dde-b213-d4c6fb4f71ed"
  human_message_index: 64
  message_created_at: "2025-10-24T07:57:22.591605Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0174"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    I need a code that output the result of 
    commentId
    14640138
    id
    14638187
    version
    1
    text
    <div>tech details :</div><div><ol type=1 start=2 style="margin-bottom:0cm;color:rgb(36, 36, 36);font-size:15px;margin-top:0cm;"><li style="margin:0cm;font-size:12pt;font-family:Aptos, sans-serif;"><span style="font-size:11pt;margin:0px;"><a href="https://eur02.safelinks.protection.outlook.com/?url=https:%2f%2fdev.azure.com%2fKESHETDEV%2fKeshet%2f_workitems%2fedit%2f9317&amp;data=05%7c02%7cLuba.Karpenko%40keshet-tv.com%7c539193f63ad2421d479108de0026442a%7c94c43ca58a8942a8b6d6f1a37cdff68f%7c0%7c0%7c638948361529208573%7cUnknown%7cTWFpbGZsb3d8eyJFbXB0eU1hcGkiOnRydWUsIlYiOiIwLjAuMDAwMCIsIlAiOiJXaW4...
    createdBy
    displayName
    Luba Karpenko
    url
    https://spsprodweu3.vssps.visualstudio.com/A8eaa194f-8d18-456e-9ed5-e37b70b9c324/_apis/Identities/96ddfc25-69f2-6901-808d-276c52ed7ea2
    _links
    avatar
    href
    https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/aad.OTZkZGZjMjUtNjlmMi03OTAxLTgwOGQtMjc2YzUyZWQ3ZWEy
    id
    96ddfc25-69f2-6901-808d-276c52ed7ea2
    uniqueName
    Luba.Karpenko@Keshet-tv.com
    imageUrl
    https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/aad.OTZkZGZjMjUtNjlmMi03OTAxLTgwOGQtMjc2YzUyZWQ3ZWEy
    descriptor
    aad.OTZkZGZjMjUtNjlmMi03OTAxLTgwOGQtMjc2YzUyZWQ3ZWEy
    createdDate
    2025-10-21T08:07:39.243Z
    modifiedBy
    displayName
    Luba Karpenko
    url
    https://spsprodweu3.vssps.visualstudio.com/A8eaa194f-8d18-456e-9ed5-e37b70b9c324/_apis/Identities/96ddfc25-69f2-6901-808d-276c52ed7ea2
    _links
    avatar
    href
    https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/aad.OTZkZGZjMjUtNjlmMi03OTAxLTgwOGQtMjc2YzUyZWQ3ZWEy
    id
    96ddfc25-69f2-6901-808d-276c52ed7ea2
    uniqueName
    Luba.Karpenko@Keshet-tv.com
    imageUrl
    https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/aad.OTZkZGZjMjUtNjlmMi03OTAxLTgwOGQtMjc2YzUyZWQ3ZWEy
    descriptor
    aad.OTZkZGZjMjUtNjlmMi03OTAxLTgwOGQtMjc2YzUyZWQ3ZWEy
    modifiedDate
    2025-10-21T08:07:39.243Z
    format
    html
    renderedText
    url
    https://dev.azure.com/KESHETDEV/7a929425-38ed-4005-9b0a-d1196cd6b4e9/_apis/wit/workItems/7477/comments/14638187
    comments[1]
    into array of comments where WorkItemId called ID
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0174-prompt"
  SourcePath: "claude-export/batch-0000/b464f7ad-93ce-45ae-8d1d-7d9423f23fa9/messages/61711730-f832-4dde-b213-d4c6fb4f71ed"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    I need a code that output the result of 
    commentId
    14640138
    id
    14638187
    version
    1
    text
    <div>tech details :</div><div><ol type=1 start=2 style="margin-bottom:0cm;color:rgb(36, 36, 36);font-size:15px;margin-top:0cm;"><li style="margin:0cm;font-size:12pt;font-family:Aptos, sans-serif;"><span style="font-size:11pt;margin:0px;"><a href="https://eur02.safelinks.protection.outlook.com/?url=https:%2f%2fdev.azure.com%2fKESHETDEV%2fKeshet%2f_workitems%2fedit%2f9317&amp;data=05%7c02%7cLuba.Karpenko%40keshet-tv.com%7c539193f63ad2421d479108de0026442a%7c94c43ca58a8942a8b6d6f1a37cdff68f%7c0%7c0%7c638948361529208573%7cUnknown%7cTWFpbGZsb3d8eyJFbXB0eU1hcGkiOnRydWUsIlYiOiIwLjAuMDAwMCIsIlAiOiJXaW4...
    createdBy
    displayName
    Luba Karpenko
    url
    https://spsprodweu3.vssps.visualstudio.com/A8eaa194f-8d18-456e-9ed5-e37b70b9c324/_apis/Identities/96ddfc25-69f2-6901-808d-276c52ed7ea2
    _links
    avatar
    href
    https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/aad.OTZkZGZjMjUtNjlmMi03OTAxLTgwOGQtMjc2YzUyZWQ3ZWEy
    id
    96ddfc25-69f2-6901-808d-276c52ed7ea2
    uniqueName
    Luba.Karpenko@Keshet-tv.com
    imageUrl
    https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/aad.OTZkZGZjMjUtNjlmMi03OTAxLTgwOGQtMjc2YzUyZWQ3ZWEy
    descriptor
    aad.OTZkZGZjMjUtNjlmMi03OTAxLTgwOGQtMjc2YzUyZWQ3ZWEy
    createdDate
    2025-10-21T08:07:39.243Z
    modifiedBy
    displayName
    Luba Karpenko
    url
    https://spsprodweu3.vssps.visualstudio.com/A8eaa194f-8d18-456e-9ed5-e37b70b9c324/_apis/Identities/96ddfc25-69f2-6901-808d-276c52ed7ea2
    _links
    avatar
    href
    https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/aad.OTZkZGZjMjUtNjlmMi03OTAxLTgwOGQtMjc2YzUyZWQ3ZWEy
    id
    96ddfc25-69f2-6901-808d-276c52ed7ea2
    uniqueName
    Luba.Karpenko@Keshet-tv.com
    imageUrl
    https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/aad.OTZkZGZjMjUtNjlmMi03OTAxLTgwOGQtMjc2YzUyZWQ3ZWEy
    descriptor
    aad.OTZkZGZjMjUtNjlmMi03OTAxLTgwOGQtMjc2YzUyZWQ3ZWEy
    modifiedDate
    2025-10-21T08:07:39.243Z
    format
    html
    renderedText
    url
    https://dev.azure.com/KESHETDEV/7a929425-38ed-4005-9b0a-d1196cd6b4e9/_apis/wit/workItems/7477/comments/14638187
    comments[1]
    into array of comments where WorkItemId called ID
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:api
  - IMPLEMENT:code

---

## XIIGEN-SESSION-1000-0175 - Azure DevOps task retrieval with n8n

```
source_id: xiigen-session-1000-0175
source_title: "Azure DevOps task retrieval with n8n"
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
  conversation_uuid: "b464f7ad-93ce-45ae-8d1d-7d9423f23fa9"
  conversation_name: "Azure DevOps task retrieval with n8n"
  human_message_uuid: "648e62b6-4105-4d25-b142-3b62a14ffc90"
  human_message_index: 66
  message_created_at: "2025-10-24T07:58:49.138109Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0175"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    this is my structure of input:
    "url": "https://spsprodweu3.vssps.visualstudio.com/A8eaa194f-8d18-456e-9ed5-e37b70b9c324/_apis/Identities/96ddfc25-69f2-6901-808d-276c52ed7ea2",
    "_links": {
    "avatar": {
    "href": "https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/aad.OTZkZGZjMjUtNjlmMi03OTAxLTgwOGQtMjc2YzUyZWQ3ZWEy"
    }
    },
    "id": "96ddfc25-69f2-6901-808d-276c52ed7ea2",
    "uniqueName": "Luba.Karpenko@Keshet-tv.com",
    "imageUrl": "https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/aad.OTZkZGZjMjUtNjlmMi03OTAxLTgwOGQtMjc2YzUyZWQ3ZWEy",
    "descriptor": "aad.OTZkZGZjMjUtNjlmMi03OTAxLTgwOGQtMjc2YzUyZWQ3ZWEy"
    },
    "modifiedDate": "2025-10-21T08:07:39.243Z",
    "format": "html",
    "renderedText": "",
    "url": "https://dev.azure.com/KESHETDEV/7a929425-38ed-4005-9b0a-d1196cd6b4e9/_apis/wit/workItems/7477/comments/14638187"
    }
    ]
    },
    {
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0175-prompt"
  SourcePath: "claude-export/batch-0000/b464f7ad-93ce-45ae-8d1d-7d9423f23fa9/messages/648e62b6-4105-4d25-b142-3b62a14ffc90"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    this is my structure of input:
    "url": "https://spsprodweu3.vssps.visualstudio.com/A8eaa194f-8d18-456e-9ed5-e37b70b9c324/_apis/Identities/96ddfc25-69f2-6901-808d-276c52ed7ea2",
    "_links": {
    "avatar": {
    "href": "https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/aad.OTZkZGZjMjUtNjlmMi03OTAxLTgwOGQtMjc2YzUyZWQ3ZWEy"
    }
    },
    "id": "96ddfc25-69f2-6901-808d-276c52ed7ea2",
    "uniqueName": "Luba.Karpenko@Keshet-tv.com",
    "imageUrl": "https://dev.azure.com/KESHETDEV/_apis/GraphProfile/MemberAvatars/aad.OTZkZGZjMjUtNjlmMi03OTAxLTgwOGQtMjc2YzUyZWQ3ZWEy",
    "descriptor": "aad.OTZkZGZjMjUtNjlmMi03OTAxLTgwOGQtMjc2YzUyZWQ3ZWEy"
    },
    "modifiedDate": "2025-10-21T08:07:39.243Z",
    "format": "html",
    "renderedText": "",
    "url": "https://dev.azure.com/KESHETDEV/7a929425-38ed-4005-9b0a-d1196cd6b4e9/_apis/wit/workItems/7477/comments/14638187"
    }
    ]
    },
    {
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:api

---

## XIIGEN-SESSION-1000-0176 - Azure DevOps task retrieval with n8n

```
source_id: xiigen-session-1000-0176
source_title: "Azure DevOps task retrieval with n8n"
source_license_state: first_party_private
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: external_source_integration
domain: CODE_Q
phase: 3
sub_phase: 3.3
task_type: api-contract
coverage_status: covered
pair_kind: compiler
conversation_trace:
  export_batch: batch-0000
  conversation_uuid: "b464f7ad-93ce-45ae-8d1d-7d9423f23fa9"
  conversation_name: "Azure DevOps task retrieval with n8n"
  human_message_uuid: "75817508-766a-4ccc-b6ee-b0dcbe5be951"
  human_message_index: 68
  message_created_at: "2025-10-24T08:01:21.699983Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0176"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    I would like to format the code changes in very similar manner and arrat where the ID is workItemId:
    }
    },
    "System.PersonId": {
    "oldValue": 215761286,
    "newValue": 215224021
    },
    "System.Watermark": {
    "oldValue": 52012,
    "newValue": 52025
    }
    },
    "relations": {
    "added": [
    {
    "rel": "ArtifactLink",
    "url": "vstfs:///Git/Ref/7a929425-38ed-4005-9b0a-d1196cd6b4e9%2F2c105bee-aabf-4940-ad30-bec3924f6483%2FGB4263_early_distribution",
    "attributes": {
    "authorizedDate": "2025-09-04T14:22:29.493Z",
    "id": 22212543,
    "resourceCreatedDate": "2025-09-04T14:22:29.493Z",
    "resourceModifiedDate": "2025-09-04T14:22:29.493Z",
    "revisedDate": "9999-01-01T00:00:00Z",
    "name": "Branch"
    }
    }
    ]
    },
    "url": "https://dev.azure.com/KESHETDEV/7a929425-38ed-4005-9b0a-d1196cd6b4e9/_apis/wit/workItems/4263/updates/40"
    }
    ]
    },
    {
    "count": 1,
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0176-prompt"
  SourcePath: "claude-export/batch-0000/b464f7ad-93ce-45ae-8d1d-7d9423f23fa9/messages/75817508-766a-4ccc-b6ee-b0dcbe5be951"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    I would like to format the code changes in very similar manner and arrat where the ID is workItemId:
    }
    },
    "System.PersonId": {
    "oldValue": 215761286,
    "newValue": 215224021
    },
    "System.Watermark": {
    "oldValue": 52012,
    "newValue": 52025
    }
    },
    "relations": {
    "added": [
    {
    "rel": "ArtifactLink",
    "url": "vstfs:///Git/Ref/7a929425-38ed-4005-9b0a-d1196cd6b4e9%2F2c105bee-aabf-4940-ad30-bec3924f6483%2FGB4263_early_distribution",
    "attributes": {
    "authorizedDate": "2025-09-04T14:22:29.493Z",
    "id": 22212543,
    "resourceCreatedDate": "2025-09-04T14:22:29.493Z",
    "resourceModifiedDate": "2025-09-04T14:22:29.493Z",
    "revisedDate": "9999-01-01T00:00:00Z",
    "name": "Branch"
    }
    }
    ]
    },
    "url": "https://dev.azure.com/KESHETDEV/7a929425-38ed-4005-9b0a-d1196cd6b4e9/_apis/wit/workItems/4263/updates/40"
    }
    ]
    },
    {
    "count": 1,
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "SECURITY"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:api
  - IMPLEMENT:code change
  - IMPLEMENT:code
  - SECURITY:auth

---

## XIIGEN-SESSION-1000-0177 - Azure DevOps task retrieval with n8n

```
source_id: xiigen-session-1000-0177
source_title: "Azure DevOps task retrieval with n8n"
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
  conversation_uuid: "b464f7ad-93ce-45ae-8d1d-7d9423f23fa9"
  conversation_name: "Azure DevOps task retrieval with n8n"
  human_message_uuid: "9691ea96-ee08-4ddd-8fb9-9327b82547ad"
  human_message_index: 70
  message_created_at: "2025-10-24T08:24:07.756524Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0177"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    I would like to format this result:
    
    "Microsoft.VSTS.TCM.ReproSteps": "<br><table width=\"\"><tr><td style=\"vertical-align:top;padding:2px 7px;font-weight:bold;\">4. </td><td style=\"vertical-align:top;padding:2px 7px;font-weight:bold;color:red;\">Failed </td><td style=\"vertical-align:top;padding:2px 7px;width:712px;\"><div><div><p> במסך מסמכים מצורפים הוסף מסמך להסכם + לחץ הבא </p> </div> </div><div style=\"padding-top:10px;\">Expected Result </div><div><div><p>מוצג מסך שווי מדיה - נמצא טוגל להעתקה מהסכם אב(כבוי ברירת מחדל) </p><p style=\"direction:rtl;\"><b>בפועל:</b> </p><p style=\"direction:rtl;\"><b>הסכמים - הסכם אב - שווי מדיה - ביצירת הסכם עם הסכם אב מקושר - כשמגיעים לשלב של שווי מדיה - הטוגל של העתקה מהסכם אב דולק כשמגיעים לסעיף בלי שלחצתי עליו (בפועל ברירת מחדל זה שהטוגל כבוי)</b> </p><p style=\"direction:rtl;\"><b>ראה סרטון</b> </p><p style=\"direction:rtl;\"><b><br></b> </p> </div> </div> </td></tr><tr><td style=\"vertical-align:top;padding:2px 7px;font-weight:bold;\"><br> </td><td style=\"vertical-align:top;padding:2px 7px;font-weight:bold;color:rgb(29, 108, 255);\"> </td><td style=\"vertical-align:top;padding:2px 7px;\"><br> </td></tr></table><hr style=\"border-color:white;\"><table><tbody><tr><td style=\"vertical-align:top;padding:2px 7px;font-weight:bold;\">Test Configuration: </td><td style=\"vertical-align:top;padding:2px 7px 2px 100px;\">Windows 10 </td></tr></tbody></table><hr style=\"border-color:white;\"><br>",
    "System.Tags": "הסכמים"
    },
    "multilineFieldsFormat": {
    "Microsoft.VSTS.TCM.ReproSteps": "html"
    },
    "relations": [
    {
    "rel": "Microsoft.VSTS.Common.TestedBy-Forward",
    "url": "https://dev.azure.com/KESHETDEV/7a929425-38ed-4005-9b0a-d1196cd6b4e9/_apis/wit/workItems/9502",
    "attributes": {
    "isLocked": false,
    "name": "Tested By"
    }
    },
    {
    "rel": "ArtifactLink",
    "url": "vstfs:///TestManagement/TcmResult/4057.100000",
    "attributes": {
    "authorizedDate": "2025-10-23T11:22:01.127Z",
    "id": 22619439,
    "resourceCreatedDate": "2025-10-23T11:22:01.127Z",
    "resourceModifiedDate": "2025-10-23T11:22:01.127Z",
    "revisedDate": "9999-01-01T00:00:00Z",
    "name": "Test Result"
    }
    },
    {
    "rel": "AttachedFile",
    "url": "https://dev.azure.com/KESHETDEV/7a929425-38ed-4005-9b0a-d1196cd6b4e9/_apis/wit/attachments/d9b08076-7d6b-4c7f-97da-45c3e5d7bd6c",
    "attributes": {
    "authorizedDate": "2025-10-23T11:22:01.127Z",
    "id": 22619440,
    "resourceCreatedDate": "2025-10-23T11:20:29.183Z",
    "resourceModifiedDate": "2025-10-23T11:20:29.183Z",
    "revisedDate": "9999-01-01T00:00:00Z",
    "resourceSize": 681828,
    "name": "2025-10-23_14h04_43.mp4"
    }
    }
    ],
    "url": "https://dev.azure.com/KESHETDEV/7a929425-38ed-4005-9b0a-d1196cd6b4e9/_apis/wit/workItems/9503"
    }
    ]
    }
    ]
    
    to the format of array that contains:
    workItemId if one of the type: task, bug,test,story, feature... (all possible formats at the board
    If it is a commit or pull request it's id 
    
    repositoryName:
    type: task/entity/... commit/pull request
    relationType: rel + attributes.length?' '+ attributes.name:''
    relation type
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0177-prompt"
  SourcePath: "claude-export/batch-0000/b464f7ad-93ce-45ae-8d1d-7d9423f23fa9/messages/9691ea96-ee08-4ddd-8fb9-9327b82547ad"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    I would like to format this result:
    
    "Microsoft.VSTS.TCM.ReproSteps": "<br><table width=\"\"><tr><td style=\"vertical-align:top;padding:2px 7px;font-weight:bold;\">4. </td><td style=\"vertical-align:top;padding:2px 7px;font-weight:bold;color:red;\">Failed </td><td style=\"vertical-align:top;padding:2px 7px;width:712px;\"><div><div><p> במסך מסמכים מצורפים הוסף מסמך להסכם + לחץ הבא </p> </div> </div><div style=\"padding-top:10px;\">Expected Result </div><div><div><p>מוצג מסך שווי מדיה - נמצא טוגל להעתקה מהסכם אב(כבוי ברירת מחדל) </p><p style=\"direction:rtl;\"><b>בפועל:</b> </p><p style=\"direction:rtl;\"><b>הסכמים - הסכם אב - שווי מדיה - ביצירת הסכם עם הסכם אב מקושר - כשמגיעים לשלב של שווי מדיה - הטוגל של העתקה מהסכם אב דולק כשמגיעים לסעיף בלי שלחצתי עליו (בפועל ברירת מחדל זה שהטוגל כבוי)</b> </p><p style=\"direction:rtl;\"><b>ראה סרטון</b> </p><p style=\"direction:rtl;\"><b><br></b> </p> </div> </div> </td></tr><tr><td style=\"vertical-align:top;padding:2px 7px;font-weight:bold;\"><br> </td><td style=\"vertical-align:top;padding:2px 7px;font-weight:bold;color:rgb(29, 108, 255);\"> </td><td style=\"vertical-align:top;padding:2px 7px;\"><br> </td></tr></table><hr style=\"border-color:white;\"><table><tbody><tr><td style=\"vertical-align:top;padding:2px 7px;font-weight:bold;\">Test Configuration: </td><td style=\"vertical-align:top;padding:2px 7px 2px 100px;\">Windows 10 </td></tr></tbody></table><hr style=\"border-color:white;\"><br>",
    "System.Tags": "הסכמים"
    },
    "multilineFieldsFormat": {
    "Microsoft.VSTS.TCM.ReproSteps": "html"
    },
    "relations": [
    {
    "rel": "Microsoft.VSTS.Common.TestedBy-Forward",
    "url": "https://dev.azure.com/KESHETDEV/7a929425-38ed-4005-9b0a-d1196cd6b4e9/_apis/wit/workItems/9502",
    "attributes": {
    "isLocked": false,
    "name": "Tested By"
    }
    },
    {
    "rel": "ArtifactLink",
    "url": "vstfs:///TestManagement/TcmResult/4057.100000",
    "attributes": {
    "authorizedDate": "2025-10-23T11:22:01.127Z",
    "id": 22619439,
    "resourceCreatedDate": "2025-10-23T11:22:01.127Z",
    "resourceModifiedDate": "2025-10-23T11:22:01.127Z",
    "revisedDate": "9999-01-01T00:00:00Z",
    "name": "Test Result"
    }
    },
    {
    "rel": "AttachedFile",
    "url": "https://dev.azure.com/KESHETDEV/7a929425-38ed-4005-9b0a-d1196cd6b4e9/_apis/wit/attachments/d9b08076-7d6b-4c7f-97da-45c3e5d7bd6c",
    "attributes": {
    "authorizedDate": "2025-10-23T11:22:01.127Z",
    "id": 22619440,
    "resourceCreatedDate": "2025-10-23T11:20:29.183Z",
    "resourceModifiedDate": "2025-10-23T11:20:29.183Z",
    "revisedDate": "9999-01-01T00:00:00Z",
    "resourceSize": 681828,
    "name": "2025-10-23_14h04_43.mp4"
    }
    }
    ],
    "url": "https://dev.azure.com/KESHETDEV/7a929425-38ed-4005-9b0a-d1196cd6b4e9/_apis/wit/workItems/9503"
    }
    ]
    }
    ]
    
    to the format of array that contains:
    workItemId if one of the type: task, bug,test,story, feature... (all possible formats at the board
    If it is a commit or pull request it's id 
    
    repositoryName:
    type: task/entity/... commit/pull request
    relationType: rel + attributes.length?' '+ attributes.name:''
    relation type
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:api
  - API_CONTRACT:request
  - IMPLEMENT:bug
  - IMPLEMENT:failed
  - SECURITY:auth
  - TEST:test

---

## XIIGEN-SESSION-1000-0178 - Azure DevOps task retrieval with n8n

```
source_id: xiigen-session-1000-0178
source_title: "Azure DevOps task retrieval with n8n"
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
  conversation_uuid: "b464f7ad-93ce-45ae-8d1d-7d9423f23fa9"
  conversation_name: "Azure DevOps task retrieval with n8n"
  human_message_uuid: "74cb1f29-e4dd-4490-aec5-3ef66f9edd3d"
  human_message_index: 72
  message_created_at: "2025-10-24T10:17:23.681228Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0178"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "can I create some variable through the code execution on n8n?"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0178-prompt"
  SourcePath: "claude-export/batch-0000/b464f7ad-93ce-45ae-8d1d-7d9423f23fa9/messages/74cb1f29-e4dd-4490-aec5-3ef66f9edd3d"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Backend Services"
  CompiledText: "can I create some variable through the code execution on n8n?"
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - IMPLEMENT:code

---

## XIIGEN-SESSION-1000-0179 - Top JSON viewer extensions

```
source_id: xiigen-session-1000-0179
source_title: "Top JSON viewer extensions"
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
  conversation_uuid: "54ae4e95-f47a-409d-a79f-1130e8c0971b"
  conversation_name: "Top JSON viewer extensions"
  human_message_uuid: "1f4553b6-82d2-4bda-b595-1ec73974df5f"
  human_message_index: 1
  message_created_at: "2025-10-27T05:59:04.995113Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0179"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "to Visual Studio Code?"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0179-prompt"
  SourcePath: "claude-export/batch-0000/54ae4e95-f47a-409d-a79f-1130e8c0971b/messages/1f4553b6-82d2-4bda-b595-1ec73974df5f"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Backend Services"
  CompiledText: "to Visual Studio Code?"
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - IMPLEMENT:code

---

## XIIGEN-SESSION-1000-0180 - Employee task summary report

```
source_id: xiigen-session-1000-0180
source_title: "Employee task summary report"
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
  conversation_uuid: "acd96646-c7ce-42cf-b6f8-c3fffab787f6"
  conversation_name: "Employee task summary report"
  human_message_uuid: "98673ac9-ee99-4dd8-b5fb-ec484e1f29ef"
  human_message_index: 0
  message_created_at: "2025-10-26T16:18:22.264691Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0180"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    Please make a summary 
    for each employee what has been done: 
    Which tasks he have been doing 
    for each task - what is the task about (area, title, description) short ,what was previous status, what is the latest status,  if there are comments - please write them down, if there are code or pr connected, give a link - short information, just the important one. Do not count the change in fields as activity, just a status change
    
    No need to display all the status changes - just the one at the beginning and at the end
    do not cut comments
    
    No need:
    Status Changes:
    * 2025-10-22 08:14: Active → Resolved (initial)
    * 2025-10-22 08:14: Active → Resolved (final)
    * 2025-10-22 12:24: Active → Closed (initial)
    * ... (3 intermediate changes)
    * 2025-10-22 13:52: Active → Closed (final)
    Just the previous and current: Active -> the old value on 08:14 and end status: Closed -> NewValue on 13:52 -> or by current value on task
    If there is no description no need to add it - but if there is a parent with description in related complete it from the parent
    
    Add section of related tasks with numbers and their relation type
    
    Add to the childs - title, current status
    
    In tasks that contain pr/commits can you bring the names for the prs/commits ? Maybe from task changes? 
    
    Add commits Info - comments for related tasks. And add summary of not connected to work Items commits
    
    No need in double information - group by 
    
    One report should be structured as successful claude report
    
    And another an html in the following format:
    Report Features:
    Main Tasks Table: (all the tasks from taskMainReport.json)
    
    Task ID (clickable link to Azure DevOps)
    Title (in Hebrew)
    Area
    Type (Bug, Feature, User Story, etc.)
    Description (truncated to 150 chars)
    Previous Status - follow the task changes - first change of status old value (color-coded badge)
    Current Status (color-coded badge)
    Related Tasks (with relation type) with their current status and title (from the tasksMainReport.json -> relatedFormatted by Type of WorkItem -> All that not WorkItem should be on column Code/PR -> validate they do appear there, even if commit doesn't have workItemId)
    Code/PR (clickable links to commits/PRs) with the comments on commit/pr (from commitsInfo.json by work Item Id or by a commit Id in tasksMainReport.json -> relatedFormatted by Type commit or pull request)
    Comments on tasks  with the user and the comment full text (from the comments.json -> to the task or the related tasks by workItemId or sourceWorkItemId in related) - give the whole comment, author and time, ordered by time descending
    
    Go over files several times and validate that all the information is included
    Unconnected Commits Table:
    
    Commit ID
    Date
    Message
    URL (clickable link)
    
    Do Remove Duplicates -> for each commit id only one line
    
    Design Features:
    ✅ Color-coded status badges
    ✅ Hover effects on rows
    ✅ Clickable links to Azure DevOps
    ✅ Clean, professional styling
    ✅ Responsive design
    ✅ Truncated text for readability
    
    I need two reports -> one in text format - example to successful report
    Another in Html formt -> successfulHtmlFormat
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0180-prompt"
  SourcePath: "claude-export/batch-0000/acd96646-c7ce-42cf-b6f8-c3fffab787f6/messages/98673ac9-ee99-4dd8-b5fb-ec484e1f29ef"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    Please make a summary 
    for each employee what has been done: 
    Which tasks he have been doing 
    for each task - what is the task about (area, title, description) short ,what was previous status, what is the latest status,  if there are comments - please write them down, if there are code or pr connected, give a link - short information, just the important one. Do not count the change in fields as activity, just a status change
    
    No need to display all the status changes - just the one at the beginning and at the end
    do not cut comments
    
    No need:
    Status Changes:
    * 2025-10-22 08:14: Active → Resolved (initial)
    * 2025-10-22 08:14: Active → Resolved (final)
    * 2025-10-22 12:24: Active → Closed (initial)
    * ... (3 intermediate changes)
    * 2025-10-22 13:52: Active → Closed (final)
    Just the previous and current: Active -> the old value on 08:14 and end status: Closed -> NewValue on 13:52 -> or by current value on task
    If there is no description no need to add it - but if there is a parent with description in related complete it from the parent
    
    Add section of related tasks with numbers and their relation type
    
    Add to the childs - title, current status
    
    In tasks that contain pr/commits can you bring the names for the prs/commits ? Maybe from task changes? 
    
    Add commits Info - comments for related tasks. And add summary of not connected to work Items commits
    
    No need in double information - group by 
    
    One report should be structured as successful claude report
    
    And another an html in the following format:
    Report Features:
    Main Tasks Table: (all the tasks from taskMainReport.json)
    
    Task ID (clickable link to Azure DevOps)
    Title (in Hebrew)
    Area
    Type (Bug, Feature, User Story, etc.)
    Description (truncated to 150 chars)
    Previous Status - follow the task changes - first change of status old value (color-coded badge)
    Current Status (color-coded badge)
    Related Tasks (with relation type) with their current status and title (from the tasksMainReport.json -> relatedFormatted by Type of WorkItem -> All that not WorkItem should be on column Code/PR -> validate they do appear there, even if commit doesn't have workItemId)
    Code/PR (clickable links to commits/PRs) with the comments on commit/pr (from commitsInfo.json by work Item Id or by a commit Id in tasksMainReport.json -> relatedFormatted by Type commit or pull request)
    Comments on tasks  with the user and the comment full text (from the comments.json -> to the task or the related tasks by workItemId or sourceWorkItemId in related) - give the whole comment, author and time, ordered by time descending
    
    Go over files several times and validate that all the information is included
    Unconnected Commits Table:
    
    Commit ID
    Date
    Message
    URL (clickable link)
    
    Do Remove Duplicates -> for each commit id only one line
    
    Design Features:
    ✅ Color-coded status badges
    ✅ Hover effects on rows
    ✅ Clickable links to Azure DevOps
    ✅ Clean, professional styling
    ✅ Responsive design
    ✅ Truncated text for readability
    
    I need two reports -> one in text format - example to successful report
    Another in Html formt -> successfulHtmlFormat
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:request
  - IMPLEMENT:code
  - IMPLEMENT:bug
  - SECURITY:auth
  - TEST:test

---

## XIIGEN-SESSION-1000-0181 - Employee task summary report

```
source_id: xiigen-session-1000-0181
source_title: "Employee task summary report"
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
  conversation_uuid: "dab30477-9664-466e-a9cc-0b74075d6944"
  conversation_name: "Employee task summary report"
  human_message_uuid: "f4dfb215-01c9-4477-9469-84f1f92e14e3"
  human_message_index: 0
  message_created_at: "2025-10-26T16:32:47.811191Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0181"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    Please make a summary 
    for each employee what has been done: 
    Which tasks he have been doing 
    for each task - what is the task about (area, title, description) short ,what was previous status, what is the latest status,  if there are comments - please write them down, if there are code or pr connected, give a link - short information, just the important one. Do not count the change in fields as activity, just a status change
    
    No need to display all the status changes - just the one at the beginning and at the end
    do not cut comments
    
    No need:
    Status Changes:
    * 2025-10-22 08:14: Active → Resolved (initial)
    * 2025-10-22 08:14: Active → Resolved (final)
    * 2025-10-22 12:24: Active → Closed (initial)
    * ... (3 intermediate changes)
    * 2025-10-22 13:52: Active → Closed (final)
    Just the previous and current: Active -> the old value on 08:14 and end status: Closed -> NewValue on 13:52 -> or by current value on task
    If there is no description no need to add it - but if there is a parent with description in related complete it from the parent
    
    Add section of related tasks with numbers and their relation type
    
    Add to the childs - title, current status
    
    In tasks that contain pr/commits can you bring the names for the prs/commits ? Maybe from task changes? 
    
    Add commits Info - comments for related tasks. And add summary of not connected to work Items commits
    
    No need in double information - group by 
    
    One report should be structured as successful claude report
    
    And another an html in the following format:
    Report Features:
    Main Tasks Table: (all the tasks from taskMainReport.json - except tasks in status New)
    
    Task ID (clickable link to Azure DevOps)
    Title (in Hebrew)
    Area
    Type (Bug, Feature, User Story, etc.)
    Description (truncated to 150 chars)
    Previous Status - follow the task changes - first change of status old value (color-coded badge)
    Current Status (color-coded badge)
    Related Tasks (with relation type) with their current status and title (from the tasksMainReport.json -> relatedFormatted by Type of WorkItem -> All that not WorkItem should be on column Code/PR -> validate they do appear there, even if commit doesn't have workItemId)
    Code/PR (clickable links to commits/PRs) with the comments on commit/pr (from commitsInfo.json by work Item Id or the workItemId can be part of a comment/text of the commit or by a commit Id in tasksMainReport.json -> relatedFormatted by Type commit or pull request)
    Comments on tasks  with the user and the comment full text (from the comments.json -> to the task or the related tasks by workItemId or sourceWorkItemId in related) - give the whole comment, author and time, ordered by time descending
    
    Go over files several times and validate that all the information is included
    Unconnected Commits Table:
    
    Commit ID
    Date
    Message
    URL (clickable link)
    
    Do Remove Duplicates -> for each commit id only one line
    
    Design Features:
    ✅ Color-coded status badges
    ✅ Hover effects on rows
    ✅ Clickable links to Azure DevOps
    ✅ Clean, professional styling
    ✅ Responsive design
    ✅ Truncated text for readability
    
    I need two reports -> one in text format - example to successful report
    Another in Html formt -> successfulHtmlFormat
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0181-prompt"
  SourcePath: "claude-export/batch-0000/dab30477-9664-466e-a9cc-0b74075d6944/messages/f4dfb215-01c9-4477-9469-84f1f92e14e3"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    Please make a summary 
    for each employee what has been done: 
    Which tasks he have been doing 
    for each task - what is the task about (area, title, description) short ,what was previous status, what is the latest status,  if there are comments - please write them down, if there are code or pr connected, give a link - short information, just the important one. Do not count the change in fields as activity, just a status change
    
    No need to display all the status changes - just the one at the beginning and at the end
    do not cut comments
    
    No need:
    Status Changes:
    * 2025-10-22 08:14: Active → Resolved (initial)
    * 2025-10-22 08:14: Active → Resolved (final)
    * 2025-10-22 12:24: Active → Closed (initial)
    * ... (3 intermediate changes)
    * 2025-10-22 13:52: Active → Closed (final)
    Just the previous and current: Active -> the old value on 08:14 and end status: Closed -> NewValue on 13:52 -> or by current value on task
    If there is no description no need to add it - but if there is a parent with description in related complete it from the parent
    
    Add section of related tasks with numbers and their relation type
    
    Add to the childs - title, current status
    
    In tasks that contain pr/commits can you bring the names for the prs/commits ? Maybe from task changes? 
    
    Add commits Info - comments for related tasks. And add summary of not connected to work Items commits
    
    No need in double information - group by 
    
    One report should be structured as successful claude report
    
    And another an html in the following format:
    Report Features:
    Main Tasks Table: (all the tasks from taskMainReport.json - except tasks in status New)
    
    Task ID (clickable link to Azure DevOps)
    Title (in Hebrew)
    Area
    Type (Bug, Feature, User Story, etc.)
    Description (truncated to 150 chars)
    Previous Status - follow the task changes - first change of status old value (color-coded badge)
    Current Status (color-coded badge)
    Related Tasks (with relation type) with their current status and title (from the tasksMainReport.json -> relatedFormatted by Type of WorkItem -> All that not WorkItem should be on column Code/PR -> validate they do appear there, even if commit doesn't have workItemId)
    Code/PR (clickable links to commits/PRs) with the comments on commit/pr (from commitsInfo.json by work Item Id or the workItemId can be part of a comment/text of the commit or by a commit Id in tasksMainReport.json -> relatedFormatted by Type commit or pull request)
    Comments on tasks  with the user and the comment full text (from the comments.json -> to the task or the related tasks by workItemId or sourceWorkItemId in related) - give the whole comment, author and time, ordered by time descending
    
    Go over files several times and validate that all the information is included
    Unconnected Commits Table:
    
    Commit ID
    Date
    Message
    URL (clickable link)
    
    Do Remove Duplicates -> for each commit id only one line
    
    Design Features:
    ✅ Color-coded status badges
    ✅ Hover effects on rows
    ✅ Clickable links to Azure DevOps
    ✅ Clean, professional styling
    ✅ Responsive design
    ✅ Truncated text for readability
    
    I need two reports -> one in text format - example to successful report
    Another in Html formt -> successfulHtmlFormat
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:request
  - IMPLEMENT:code
  - IMPLEMENT:bug
  - SECURITY:auth
  - TEST:test

---

## XIIGEN-SESSION-1000-0182 - Employee task summary report

```
source_id: xiigen-session-1000-0182
source_title: "Employee task summary report"
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
  conversation_uuid: "fdbeaa77-25db-488f-8359-2fdef38a83f8"
  conversation_name: "Employee task summary report"
  human_message_uuid: "61205674-8960-4e7d-b982-b606d0224913"
  human_message_index: 0
  message_created_at: "2025-10-26T17:20:13.735012Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0182"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    Please make a summary 
    for each employee what has been done: 
    Which tasks he have been doing 
    for each task - what is the task about (area, title, description) short ,what was previous status, what is the latest status,  if there are comments - please write them down, if there are code or pr connected, give a link - short information, just the important one. Do not count the change in fields as activity, just a status change
    
    No need to display all the status changes - just the one at the beginning and at the end
    do not cut comments
    
    No need:
    Status Changes:
    * 2025-10-22 08:14: Active → Resolved (initial)
    * 2025-10-22 08:14: Active → Resolved (final)
    * 2025-10-22 12:24: Active → Closed (initial)
    * ... (3 intermediate changes)
    * 2025-10-22 13:52: Active → Closed (final)
    Just the previous and current: Active -> the old value on 08:14 and end status: Closed -> NewValue on 13:52 -> or by current value on task
    If there is no description no need to add it - but if there is a parent with description in related complete it from the parent
    
    Add section of related tasks with numbers and their relation type
    
    Add to the childs - title, current status
    
    In tasks that contain pr/commits can you bring the names for the prs/commits ? Maybe from task changes? 
    
    Add commits Info - comments for related tasks. And add summary of not connected to work Items commits
    
    No need in double information - group by 
    
    One report should be structured as successful claude report
    
    And another an html in the following format:
    Report Features:
    Main Tasks Table: (all the tasks from taskMainReport.json)
    
    Task ID (clickable link to Azure DevOps)
    Title (in Hebrew)
    Area
    Type (Bug, Feature, User Story, etc.)
    Description (truncated to 250 chars)
    Previous Status - follow the Changes field of the task and complete with missing from tasksChanges.json  - first change of status old value (color-coded badge)
    Current Status (color-coded badge)
    Related Tasks (with relation type) with their current status and title (from the tasksMainReport.json -> relatedFormatted by Type of WorkItem -> All that not WorkItem should be on column Code/PR -> validate they do appear there, even if commit doesn't have workItemId)
    Code/PR (clickable links to commits/PRs) with the comments on commit/pr (follow the Commits and Related Code field of the task and complete missing from commitsInfo.json by work Item Id or the workItemId can be part of a comment/text of the commit or by a commit Id in tasksMainReport.json -> relatedFormatted by Type commit or pull request)
    Comments on tasks  with the user and the comment full text (follow the comments array of the task and complete with additional from the comments.json -> to the task or the related tasks by workItemId or sourceWorkItemId in related) - give the whole comment, author and time, ordered by time descending
    AdditionalComments : display changes during the day in the task in task Changes field by comparing old value to new -> interesting in textual field edits, added realtion, changed sprint
    
    Go over files several times and validate that all the information is included
    Unconnected Commits Table:
    
    Commit ID
    Date
    Message
    URL (clickable link)
    
    Do Remove Duplicates -> for each commit id only one line
    
    Design Features:
    ✅ Color-coded status badges
    ✅ Hover effects on rows
    ✅ Clickable links to Azure DevOps
    ✅ Clean, professional styling
    ✅ Responsive design
    ✅ Truncated text for readability
    
    I need two reports -> one in text format - example to successful report
    Another in Html formt -> successfulHtmlFormat
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0182-prompt"
  SourcePath: "claude-export/batch-0000/fdbeaa77-25db-488f-8359-2fdef38a83f8/messages/61205674-8960-4e7d-b982-b606d0224913"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    Please make a summary 
    for each employee what has been done: 
    Which tasks he have been doing 
    for each task - what is the task about (area, title, description) short ,what was previous status, what is the latest status,  if there are comments - please write them down, if there are code or pr connected, give a link - short information, just the important one. Do not count the change in fields as activity, just a status change
    
    No need to display all the status changes - just the one at the beginning and at the end
    do not cut comments
    
    No need:
    Status Changes:
    * 2025-10-22 08:14: Active → Resolved (initial)
    * 2025-10-22 08:14: Active → Resolved (final)
    * 2025-10-22 12:24: Active → Closed (initial)
    * ... (3 intermediate changes)
    * 2025-10-22 13:52: Active → Closed (final)
    Just the previous and current: Active -> the old value on 08:14 and end status: Closed -> NewValue on 13:52 -> or by current value on task
    If there is no description no need to add it - but if there is a parent with description in related complete it from the parent
    
    Add section of related tasks with numbers and their relation type
    
    Add to the childs - title, current status
    
    In tasks that contain pr/commits can you bring the names for the prs/commits ? Maybe from task changes? 
    
    Add commits Info - comments for related tasks. And add summary of not connected to work Items commits
    
    No need in double information - group by 
    
    One report should be structured as successful claude report
    
    And another an html in the following format:
    Report Features:
    Main Tasks Table: (all the tasks from taskMainReport.json)
    
    Task ID (clickable link to Azure DevOps)
    Title (in Hebrew)
    Area
    Type (Bug, Feature, User Story, etc.)
    Description (truncated to 250 chars)
    Previous Status - follow the Changes field of the task and complete with missing from tasksChanges.json  - first change of status old value (color-coded badge)
    Current Status (color-coded badge)
    Related Tasks (with relation type) with their current status and title (from the tasksMainReport.json -> relatedFormatted by Type of WorkItem -> All that not WorkItem should be on column Code/PR -> validate they do appear there, even if commit doesn't have workItemId)
    Code/PR (clickable links to commits/PRs) with the comments on commit/pr (follow the Commits and Related Code field of the task and complete missing from commitsInfo.json by work Item Id or the workItemId can be part of a comment/text of the commit or by a commit Id in tasksMainReport.json -> relatedFormatted by Type commit or pull request)
    Comments on tasks  with the user and the comment full text (follow the comments array of the task and complete with additional from the comments.json -> to the task or the related tasks by workItemId or sourceWorkItemId in related) - give the whole comment, author and time, ordered by time descending
    AdditionalComments : display changes during the day in the task in task Changes field by comparing old value to new -> interesting in textual field edits, added realtion, changed sprint
    
    Go over files several times and validate that all the information is included
    Unconnected Commits Table:
    
    Commit ID
    Date
    Message
    URL (clickable link)
    
    Do Remove Duplicates -> for each commit id only one line
    
    Design Features:
    ✅ Color-coded status badges
    ✅ Hover effects on rows
    ✅ Clickable links to Azure DevOps
    ✅ Clean, professional styling
    ✅ Responsive design
    ✅ Truncated text for readability
    
    I need two reports -> one in text format - example to successful report
    Another in Html formt -> successfulHtmlFormat
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:request
  - IMPLEMENT:code
  - IMPLEMENT:bug
  - SECURITY:auth
  - TEST:test

---

## XIIGEN-SESSION-1000-0183 - Employee task summary report

```
source_id: xiigen-session-1000-0183
source_title: "Employee task summary report"
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
  conversation_uuid: "2491aa9c-196a-4715-a196-4149b7e19fbd"
  conversation_name: "Employee task summary report"
  human_message_uuid: "82739c5f-1119-404a-9276-8ab27dae3270"
  human_message_index: 0
  message_created_at: "2025-10-26T15:56:18.980869Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0183"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    Please make a summary 
    for each employee what has been done: 
    Which tasks he have been doing 
    for each task - what is the task about (area, title, description) short ,what was previous status, what is the latest status,  if there are comments - please write them down, if there are code or pr connected, give a link - short information, just the important one. Do not count the change in fields as activity, just a status change
    
    No need to display all the status changes - just the one at the beginning and at the end
    do not cut comments
    
    No need:
    Status Changes:
    * 2025-10-22 08:14: Active → Resolved (initial)
    * 2025-10-22 08:14: Active → Resolved (final)
    * 2025-10-22 12:24: Active → Closed (initial)
    * ... (3 intermediate changes)
    * 2025-10-22 13:52: Active → Closed (final)
    Just the previous and current
    If there is no description no need to add it - but if there is a parent with description in related complete it from the parent
    
    Add section of related tasks with numbers and their relation type
    
    Add to the childs - title, current status
    
    In tasks that contain pr/commits can you bring the names for the prs/commits ? Maybe from task changes? 
    
    Add commits Info - comments for related tasks. And add summary of not connected to work Items commits
    
    No need in double information - group by 
    
    One report should be structured as successful claude report
    
    And another an html in the following format:
    Report Features:
    Main Tasks Table: (all the tasks from taskMainReport.json)
    
    Task ID (clickable link to Azure DevOps)
    Title (in Hebrew)
    Area
    Type (Bug, Feature, User Story, etc.)
    Description (truncated to 150 chars)
    Previous Status - follow the task changes - first change of status old value (color-coded badge)
    Current Status (color-coded badge)
    Related Tasks (with relation type) with their current status and title (from the tasksMainReport.json -> relatedFormatted)
    Code/PR (clickable links to commits/PRs) with the comments on commit/pr (from commitsInfo.json by work Item Id or by a commit Id in tasksMainReport.json)
    Comments on tasks  with the user and the comment full text (from the comments.json -> to the task or the related tasks by workItemId or sourceWorkItemId in related) - give the whole comment, author and time, ordered by time descending
    
    Go over files several times and validate that all the information is included
    Unconnected Commits Table:
    
    Commit ID
    Date
    Message
    URL (clickable link)
    
    Design Features:
    ✅ Color-coded status badges
    ✅ Hover effects on rows
    ✅ Clickable links to Azure DevOps
    ✅ Clean, professional styling
    ✅ Responsive design
    ✅ Truncated text for readability
    
    I need two reports -> one in text format - example to successful report
    Another in Html formt -> successfulHtmlFormat
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0183-prompt"
  SourcePath: "claude-export/batch-0000/2491aa9c-196a-4715-a196-4149b7e19fbd/messages/82739c5f-1119-404a-9276-8ab27dae3270"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Backend Services"
  CompiledText: |
    Please make a summary 
    for each employee what has been done: 
    Which tasks he have been doing 
    for each task - what is the task about (area, title, description) short ,what was previous status, what is the latest status,  if there are comments - please write them down, if there are code or pr connected, give a link - short information, just the important one. Do not count the change in fields as activity, just a status change
    
    No need to display all the status changes - just the one at the beginning and at the end
    do not cut comments
    
    No need:
    Status Changes:
    * 2025-10-22 08:14: Active → Resolved (initial)
    * 2025-10-22 08:14: Active → Resolved (final)
    * 2025-10-22 12:24: Active → Closed (initial)
    * ... (3 intermediate changes)
    * 2025-10-22 13:52: Active → Closed (final)
    Just the previous and current
    If there is no description no need to add it - but if there is a parent with description in related complete it from the parent
    
    Add section of related tasks with numbers and their relation type
    
    Add to the childs - title, current status
    
    In tasks that contain pr/commits can you bring the names for the prs/commits ? Maybe from task changes? 
    
    Add commits Info - comments for related tasks. And add summary of not connected to work Items commits
    
    No need in double information - group by 
    
    One report should be structured as successful claude report
    
    And another an html in the following format:
    Report Features:
    Main Tasks Table: (all the tasks from taskMainReport.json)
    
    Task ID (clickable link to Azure DevOps)
    Title (in Hebrew)
    Area
    Type (Bug, Feature, User Story, etc.)
    Description (truncated to 150 chars)
    Previous Status - follow the task changes - first change of status old value (color-coded badge)
    Current Status (color-coded badge)
    Related Tasks (with relation type) with their current status and title (from the tasksMainReport.json -> relatedFormatted)
    Code/PR (clickable links to commits/PRs) with the comments on commit/pr (from commitsInfo.json by work Item Id or by a commit Id in tasksMainReport.json)
    Comments on tasks  with the user and the comment full text (from the comments.json -> to the task or the related tasks by workItemId or sourceWorkItemId in related) - give the whole comment, author and time, ordered by time descending
    
    Go over files several times and validate that all the information is included
    Unconnected Commits Table:
    
    Commit ID
    Date
    Message
    URL (clickable link)
    
    Design Features:
    ✅ Color-coded status badges
    ✅ Hover effects on rows
    ✅ Clickable links to Azure DevOps
    ✅ Clean, professional styling
    ✅ Responsive design
    ✅ Truncated text for readability
    
    I need two reports -> one in text format - example to successful report
    Another in Html formt -> successfulHtmlFormat
  CompiledPredicateIds: ["IMPLEMENT", "SECURITY", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - IMPLEMENT:code
  - IMPLEMENT:bug
  - SECURITY:auth
  - TEST:test

---

## XIIGEN-SESSION-1000-0184 - Employee task summary report

```
source_id: xiigen-session-1000-0184
source_title: "Employee task summary report"
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
  conversation_uuid: "d0ec02bc-0b7b-45a0-acda-9f158a3455fb"
  conversation_name: "Employee task summary report"
  human_message_uuid: "ad319e5a-18bd-4778-ab6f-4ef313fcd796"
  human_message_index: 0
  message_created_at: "2025-10-27T06:13:26.161580Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0184"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    Please make a summary 
    for each employee what has been done: 
    Which tasks he have been doing 
    for each task - what is the task about (area, title, description) short ,what was previous status, what is the latest status,  if there are comments - please write them down, if there are code or pr connected, give a link - short information, just the important one. Do not count the change in fields as activity, just a status change
    
    No need to display all the status changes - just the one at the beginning and at the end
    do not cut comments
    
    No need:
    Status Changes:
    * 2025-10-22 08:14: Active → Resolved (initial)
    * 2025-10-22 08:14: Active → Resolved (final)
    * 2025-10-22 12:24: Active → Closed (initial)
    * ... (3 intermediate changes)
    * 2025-10-22 13:52: Active → Closed (final)
    Just the previous and current: Active -> the old value on 08:14 and end status: Closed -> NewValue on 13:52 -> or by current value on task
    If there is no description no need to add it - but if there is a parent with description in related complete it from the parent
    
    Add section of related tasks with numbers and their relation type
    
    Add to the childs - title, current status
    
    In tasks that contain pr/commits can you bring the names for the prs/commits ? Maybe from task changes? 
    
    Add commits Info - comments for related tasks. And add summary of not connected to work Items commits
    
    No need in double information - group by 
    
    One report should be structured as successful claude report
    
    And another an html in the following format:
    Report Features:
    Main Tasks Table: (all the tasks from taskMainReport.json)
    
    Task ID (clickable link to Azure DevOps)
    Title (in Hebrew)
    Area
    Type (Bug, Feature, User Story, etc.)
    Description (truncated to 250 chars)
    Previous Status - follow the Changes field of the task and complete with missing from tasksChanges.json  - first change of status old value (color-coded badge)
    Current Status (color-coded badge)
    Related Tasks (with relation type) with their current status and title (from the tasksMainReport.json -> relatedFormatted by Type of WorkItem -> All that not WorkItem should be on column Code/PR with the Commits and RelatedCode - > let us know the comment/text, time and link to the item)
    Code/PR (clickable links to commits/PRs) with the comments on commit/pr (follow the Commits and RelatedCode fields of the task )
    Comments on tasks  with the user and the comment full text (follow the comments array of the task ) - give the whole comment, author and time, ordered by time descending, if the comment contains html tags then please fix the html to display good.
    AdditionalComments : display changes during the day in the task in task Changes field by comparing old value to new -> interesting in textual field edits, added realtion (explan what exactly: work Item with Id and Parent/Child or Commit/Pr with the appropriate link and comments), changed sprint
    
    Go over files several times and validate that all the information is included
    Unconnected Commits Table from not Assigned Commits array:
    
    Commit ID
    Date with time
    Comment
    Commiter Name
    URL (clickable link)
    
    Do Remove Duplicates -> for each commit id only one line
    
    Design Features:
    ✅ Color-coded status badges
    ✅ Hover effects on rows
    ✅ Clickable links to Azure DevOps
    ✅ Clean, professional styling
    ✅ Responsive design
    ✅ Truncated text for readability
    
    I need two reports -> one in text format - example to successful report
    Another in Html formt -> successfulHtmlFormat
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0184-prompt"
  SourcePath: "claude-export/batch-0000/d0ec02bc-0b7b-45a0-acda-9f158a3455fb/messages/ad319e5a-18bd-4778-ab6f-4ef313fcd796"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Backend Services"
  CompiledText: |
    Please make a summary 
    for each employee what has been done: 
    Which tasks he have been doing 
    for each task - what is the task about (area, title, description) short ,what was previous status, what is the latest status,  if there are comments - please write them down, if there are code or pr connected, give a link - short information, just the important one. Do not count the change in fields as activity, just a status change
    
    No need to display all the status changes - just the one at the beginning and at the end
    do not cut comments
    
    No need:
    Status Changes:
    * 2025-10-22 08:14: Active → Resolved (initial)
    * 2025-10-22 08:14: Active → Resolved (final)
    * 2025-10-22 12:24: Active → Closed (initial)
    * ... (3 intermediate changes)
    * 2025-10-22 13:52: Active → Closed (final)
    Just the previous and current: Active -> the old value on 08:14 and end status: Closed -> NewValue on 13:52 -> or by current value on task
    If there is no description no need to add it - but if there is a parent with description in related complete it from the parent
    
    Add section of related tasks with numbers and their relation type
    
    Add to the childs - title, current status
    
    In tasks that contain pr/commits can you bring the names for the prs/commits ? Maybe from task changes? 
    
    Add commits Info - comments for related tasks. And add summary of not connected to work Items commits
    
    No need in double information - group by 
    
    One report should be structured as successful claude report
    
    And another an html in the following format:
    Report Features:
    Main Tasks Table: (all the tasks from taskMainReport.json)
    
    Task ID (clickable link to Azure DevOps)
    Title (in Hebrew)
    Area
    Type (Bug, Feature, User Story, etc.)
    Description (truncated to 250 chars)
    Previous Status - follow the Changes field of the task and complete with missing from tasksChanges.json  - first change of status old value (color-coded badge)
    Current Status (color-coded badge)
    Related Tasks (with relation type) with their current status and title (from the tasksMainReport.json -> relatedFormatted by Type of WorkItem -> All that not WorkItem should be on column Code/PR with the Commits and RelatedCode - > let us know the comment/text, time and link to the item)
    Code/PR (clickable links to commits/PRs) with the comments on commit/pr (follow the Commits and RelatedCode fields of the task )
    Comments on tasks  with the user and the comment full text (follow the comments array of the task ) - give the whole comment, author and time, ordered by time descending, if the comment contains html tags then please fix the html to display good.
    AdditionalComments : display changes during the day in the task in task Changes field by comparing old value to new -> interesting in textual field edits, added realtion (explan what exactly: work Item with Id and Parent/Child or Commit/Pr with the appropriate link and comments), changed sprint
    
    Go over files several times and validate that all the information is included
    Unconnected Commits Table from not Assigned Commits array:
    
    Commit ID
    Date with time
    Comment
    Commiter Name
    URL (clickable link)
    
    Do Remove Duplicates -> for each commit id only one line
    
    Design Features:
    ✅ Color-coded status badges
    ✅ Hover effects on rows
    ✅ Clickable links to Azure DevOps
    ✅ Clean, professional styling
    ✅ Responsive design
    ✅ Truncated text for readability
    
    I need two reports -> one in text format - example to successful report
    Another in Html formt -> successfulHtmlFormat
  CompiledPredicateIds: ["IMPLEMENT", "SECURITY", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - IMPLEMENT:code
  - IMPLEMENT:fix
  - IMPLEMENT:bug
  - SECURITY:auth
  - TEST:test

---

## XIIGEN-SESSION-1000-0185 - Merging n8n workflow data sources

```
source_id: xiigen-session-1000-0185
source_title: "Merging n8n workflow data sources"
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
  conversation_uuid: "3d899772-4fd9-421c-bfd1-5b792be1307f"
  conversation_name: "Merging n8n workflow data sources"
  human_message_uuid: "66c5806b-e451-44eb-a384-da9b70a4c6de"
  human_message_index: 0
  message_created_at: "2025-10-26T16:11:14.697979Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0185"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "I have a n8n workflow - I need to unite the 4 output formats from 4 paths in it into one: all the tasks from the mainTasks.json need to have all the comments(comments.json), task changes(taskChanges.json) and related commits(tasksChanges.json) - how should I do that using code node?"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0185-prompt"
  SourcePath: "claude-export/batch-0000/3d899772-4fd9-421c-bfd1-5b792be1307f/messages/66c5806b-e451-44eb-a384-da9b70a4c6de"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Backend Services"
  CompiledText: "I have a n8n workflow - I need to unite the 4 output formats from 4 paths in it into one: all the tasks from the mainTasks.json need to have all the comments(comments.json), task changes(taskChanges.json) and related commits(tasksChanges.json) - how should I do that using code node?"
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - IMPLEMENT:code

---

## XIIGEN-SESSION-1000-0186 - Merging n8n workflow data sources

```
source_id: xiigen-session-1000-0186
source_title: "Merging n8n workflow data sources"
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
  conversation_uuid: "3d899772-4fd9-421c-bfd1-5b792be1307f"
  conversation_name: "Merging n8n workflow data sources"
  human_message_uuid: "ec685a4f-56d3-4e05-bbbb-efc2603ffddc"
  human_message_index: 2
  message_created_at: "2025-10-26T16:29:51.748211Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0186"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "In first one: Cannot assign to read only property 'name' of object 'Error: Referenced node is unexecuted'"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0186-prompt"
  SourcePath: "claude-export/batch-0000/3d899772-4fd9-421c-bfd1-5b792be1307f/messages/ec685a4f-56d3-4e05-bbbb-efc2603ffddc"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Backend Services"
  CompiledText: "In first one: Cannot assign to read only property 'name' of object 'Error: Referenced node is unexecuted'"
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - IMPLEMENT:error

---

## XIIGEN-SESSION-1000-0187 - N8n workflow with Anthropic API

```
source_id: xiigen-session-1000-0187
source_title: "N8n workflow with Anthropic API"
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
  conversation_uuid: "764493aa-1171-4d14-b124-ca3bd6136307"
  conversation_name: "N8n workflow with Anthropic API"
  human_message_uuid: "ca287f10-ce1e-4ec9-bfb2-149e2e0445d4"
  human_message_index: 0
  message_created_at: "2025-10-27T06:08:11.829655Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0187"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "i have an n8n flow I would like for each task to analyze it with antropic api with a specific prompt, how should I do that?"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0187-prompt"
  SourcePath: "claude-export/batch-0000/764493aa-1171-4d14-b124-ca3bd6136307/messages/ca287f10-ce1e-4ec9-bfb2-149e2e0445d4"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: "i have an n8n flow I would like for each task to analyze it with antropic api with a specific prompt, how should I do that?"
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:api
  - DOCUMENT:spec

---

## XIIGEN-SESSION-1000-0188 - Employee task summary report

```
source_id: xiigen-session-1000-0188
source_title: "Employee task summary report"
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
  conversation_uuid: "c577d712-2788-4b42-839c-d49543a2db1d"
  conversation_name: "Employee task summary report"
  human_message_uuid: "4f991033-f998-4147-8675-d07c947291ca"
  human_message_index: 0
  message_created_at: "2025-10-26T15:37:52.866882Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0188"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    Please make a summary 
    for each employee what has been done: 
    Which tasks he have been doing 
    for each task - what is the task about (area, title, description) short ,what was previous status, what is the latest status,  if there are comments - please write them down, if there are code or pr connected, give a link - short information, just the important one. Do not count the change in fields as activity, just a status change
    
    No need to display all the status changes - just the one at the beginning and at the end
    do not cut comments
    
    No need:
    Status Changes:
    * 2025-10-22 08:14: Active → Resolved (initial)
    * 2025-10-22 08:14: Active → Resolved (final)
    * 2025-10-22 12:24: Active → Closed (initial)
    * ... (3 intermediate changes)
    * 2025-10-22 13:52: Active → Closed (final)
    Just the previous and current
    If there is no description no need to add it - but if there is a parent with description in related complete it from the parent
    
    Add section of related tasks with numbers and their relation type
    
    Add to the childs - title, current status
    
    In tasks that contain pr/commits can you bring the names for the prs/commits ? Maybe from task changes? 
    
    Add commits Info - comments for related tasks. And add summary of not connected to work Items commits
    
    No need in double information - group by 
    
    One report should be structured as successful claude report
    
    And another an html in the following format:
    Report Features:
    Main Tasks Table:
    
    Task ID (clickable link to Azure DevOps)
    Title (in Hebrew)
    Area
    Type (Bug, Feature, User Story, etc.)
    Description (truncated to 250 chars)
    Previous Status - follow the task changes - first change of status old value (color-coded badge)
    Current Status (color-coded badge)
    Related Tasks (with relation type) with their current status and title
    Code/PR (clickable links to commits/PRs) with the comments on commit/pr
    Comments on tasks  with the user and the comment full text
    
    Unconnected Commits Table:
    
    Commit ID
    Date
    Message
    URL (clickable link)
    
    Design Features:
    ✅ Color-coded status badges
    ✅ Hover effects on rows
    ✅ Clickable links to Azure DevOps
    ✅ Clean, professional styling
    ✅ Responsive design
    ✅ Truncated text for readability
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0188-prompt"
  SourcePath: "claude-export/batch-0000/c577d712-2788-4b42-839c-d49543a2db1d/messages/4f991033-f998-4147-8675-d07c947291ca"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Backend Services"
  CompiledText: |
    Please make a summary 
    for each employee what has been done: 
    Which tasks he have been doing 
    for each task - what is the task about (area, title, description) short ,what was previous status, what is the latest status,  if there are comments - please write them down, if there are code or pr connected, give a link - short information, just the important one. Do not count the change in fields as activity, just a status change
    
    No need to display all the status changes - just the one at the beginning and at the end
    do not cut comments
    
    No need:
    Status Changes:
    * 2025-10-22 08:14: Active → Resolved (initial)
    * 2025-10-22 08:14: Active → Resolved (final)
    * 2025-10-22 12:24: Active → Closed (initial)
    * ... (3 intermediate changes)
    * 2025-10-22 13:52: Active → Closed (final)
    Just the previous and current
    If there is no description no need to add it - but if there is a parent with description in related complete it from the parent
    
    Add section of related tasks with numbers and their relation type
    
    Add to the childs - title, current status
    
    In tasks that contain pr/commits can you bring the names for the prs/commits ? Maybe from task changes? 
    
    Add commits Info - comments for related tasks. And add summary of not connected to work Items commits
    
    No need in double information - group by 
    
    One report should be structured as successful claude report
    
    And another an html in the following format:
    Report Features:
    Main Tasks Table:
    
    Task ID (clickable link to Azure DevOps)
    Title (in Hebrew)
    Area
    Type (Bug, Feature, User Story, etc.)
    Description (truncated to 250 chars)
    Previous Status - follow the task changes - first change of status old value (color-coded badge)
    Current Status (color-coded badge)
    Related Tasks (with relation type) with their current status and title
    Code/PR (clickable links to commits/PRs) with the comments on commit/pr
    Comments on tasks  with the user and the comment full text
    
    Unconnected Commits Table:
    
    Commit ID
    Date
    Message
    URL (clickable link)
    
    Design Features:
    ✅ Color-coded status badges
    ✅ Hover effects on rows
    ✅ Clickable links to Azure DevOps
    ✅ Clean, professional styling
    ✅ Responsive design
    ✅ Truncated text for readability
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - IMPLEMENT:code
  - IMPLEMENT:bug
  - TEST:test

---

## XIIGEN-SESSION-1000-0189 - Azure DevOps commits retrieval

```
source_id: xiigen-session-1000-0189
source_title: "Azure DevOps commits retrieval"
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
  conversation_uuid: "57a37478-cd10-4b64-8a3a-4fb81b505ba6"
  conversation_name: "Azure DevOps commits retrieval"
  human_message_uuid: "aba77590-280c-4ed1-8f6b-727ac304193b"
  human_message_index: 0
  message_created_at: "2025-10-26T06:58:25.533847Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0189"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "I would like to retreive latest commits (between start date and end date) for each repository with the connected tasks data from azure devops using http request node at n8n?"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0189-prompt"
  SourcePath: "claude-export/batch-0000/57a37478-cd10-4b64-8a3a-4fb81b505ba6/messages/aba77590-280c-4ed1-8f6b-727ac304193b"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: "I would like to retreive latest commits (between start date and end date) for each repository with the connected tasks data from azure devops using http request node at n8n?"
  CompiledPredicateIds: ["API_CONTRACT", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:request
  - TEST:test

---

## XIIGEN-SESSION-1000-0190 - Azure DevOps commits retrieval

```
source_id: xiigen-session-1000-0190
source_title: "Azure DevOps commits retrieval"
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
  conversation_uuid: "57a37478-cd10-4b64-8a3a-4fb81b505ba6"
  conversation_name: "Azure DevOps commits retrieval"
  human_message_uuid: "eee1a164-670d-43c4-a93c-797e91275182"
  human_message_index: 2
  message_created_at: "2025-10-26T07:02:01.829493Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0190"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    Ok I got the repos: how do I get the commits?
    ### 2. **Get Commits with Date Range** **HTTP Request Node:** ``` Method: GET URL: https://dev.azure.com/{{$json.organization}}/{{$json.project}}/_apis/git/repositories/{{$json.repositoryId}}/commits Query Parameters: - searchCriteria.fromDate: {{$json.fromDate}} // e.g., "2025-10-01T00:00:00Z" - searchCriteria.toDate: {{$json.toDate}} // e.g., "2025-10-26T23:59:59Z" - api-version: 7.1
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0190-prompt"
  SourcePath: "claude-export/batch-0000/57a37478-cd10-4b64-8a3a-4fb81b505ba6/messages/eee1a164-670d-43c4-a93c-797e91275182"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    Ok I got the repos: how do I get the commits?
    ### 2. **Get Commits with Date Range** **HTTP Request Node:** ``` Method: GET URL: https://dev.azure.com/{{$json.organization}}/{{$json.project}}/_apis/git/repositories/{{$json.repositoryId}}/commits Query Parameters: - searchCriteria.fromDate: {{$json.fromDate}} // e.g., "2025-10-01T00:00:00Z" - searchCriteria.toDate: {{$json.toDate}} // e.g., "2025-10-26T23:59:59Z" - api-version: 7.1
  CompiledPredicateIds: ["API_CONTRACT", "ARCHITECTURE"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:api
  - API_CONTRACT:request
  - ARCHITECTURE:project

---

## XIIGEN-SESSION-1000-0191 - Azure DevOps commits retrieval

```
source_id: xiigen-session-1000-0191
source_title: "Azure DevOps commits retrieval"
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
  conversation_uuid: "57a37478-cd10-4b64-8a3a-4fb81b505ba6"
  conversation_name: "Azure DevOps commits retrieval"
  human_message_uuid: "2fbf7cb7-93d9-43b7-996e-9679989b1d7c"
  human_message_index: 4
  message_created_at: "2025-10-26T11:51:15.845598Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0191"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    The resource you are requesting could not be found [item 0]
    The controller for path '/Keshet/_apis/git/repositories/ebf71d2e-163c-4eb4-bdd4-4f0bdd793831/commits/a3209314ad3ef15600a0d3f565d92c6c1067bc77/workitems' was not found or does not implement IController. html { height: 100%; } body { font-family: "-apple-system", BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Helvetica, Ubuntu, Arial, sans-serif, "Apple Color Emoji", "Segoe UI Emoji", "Segoe UI Symbol"; font-size: 14px; height: 100%; margin: 0; background-color: #fff; } .container { height: 100%; display: flex; flex-grow: 1; flex-direction: column; } .header { font-size: 14px; line-height: 20px; padding-right: 40px; display: flex; box-shadow: 0px 1px 0px rgba(0, 0, 0, 0.08); } .header > div, .header > a { margin: 14px 0px 14px 32px; text-decoration: none; color: rgba(0, 0, 0, 0.55); } .header > div:first-child { flex-grow: 1; margin-left: 14px; color: #0078d4; display: flex; } .header > div:first-child > div { margin-left: 14px; } .content-container { display: flex; flex-direction: column; justify-content: center; margin-top: 126px; } .content { display: flex; justify-content: center; align-items: center; } .error-info { display: flex; flex-direction: column; width: 500px; } .title { font-size: 28px; font-weight: bold; line-height: 40px; letter-spacing: -0.04em; color: #212121; margin-bottom: 16px; } .details { font-size: 14px; line-height: 20px; colhttps://dev.azure.com/KESHETDEV/Keshet/_apis/git/repositories/{{$json.commit.repoid}}/commits/{{$json.commit.commitId}}/workitems?api-version=7.1or: #212121; margin-bottom: 16px; } .details a { text-decoration: none; color: #0078d4; }
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0191-prompt"
  SourcePath: "claude-export/batch-0000/57a37478-cd10-4b64-8a3a-4fb81b505ba6/messages/2fbf7cb7-93d9-43b7-996e-9679989b1d7c"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    The resource you are requesting could not be found [item 0]
    The controller for path '/Keshet/_apis/git/repositories/ebf71d2e-163c-4eb4-bdd4-4f0bdd793831/commits/a3209314ad3ef15600a0d3f565d92c6c1067bc77/workitems' was not found or does not implement IController. html { height: 100%; } body { font-family: "-apple-system", BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Helvetica, Ubuntu, Arial, sans-serif, "Apple Color Emoji", "Segoe UI Emoji", "Segoe UI Symbol"; font-size: 14px; height: 100%; margin: 0; background-color: #fff; } .container { height: 100%; display: flex; flex-grow: 1; flex-direction: column; } .header { font-size: 14px; line-height: 20px; padding-right: 40px; display: flex; box-shadow: 0px 1px 0px rgba(0, 0, 0, 0.08); } .header > div, .header > a { margin: 14px 0px 14px 32px; text-decoration: none; color: rgba(0, 0, 0, 0.55); } .header > div:first-child { flex-grow: 1; margin-left: 14px; color: #0078d4; display: flex; } .header > div:first-child > div { margin-left: 14px; } .content-container { display: flex; flex-direction: column; justify-content: center; margin-top: 126px; } .content { display: flex; justify-content: center; align-items: center; } .error-info { display: flex; flex-direction: column; width: 500px; } .title { font-size: 28px; font-weight: bold; line-height: 40px; letter-spacing: -0.04em; color: #212121; margin-bottom: 16px; } .details { font-size: 14px; line-height: 20px; colhttps://dev.azure.com/KESHETDEV/Keshet/_apis/git/repositories/{{$json.commit.repoid}}/commits/{{$json.commit.commitId}}/workitems?api-version=7.1or: #212121; margin-bottom: 16px; } .details a { text-decoration: none; color: #0078d4; }
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "TEST", "UI"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:api
  - API_CONTRACT:request
  - IMPLEMENT:error
  - UI:ui

---

## XIIGEN-SESSION-1000-0192 - Azure DevOps commits retrieval

```
source_id: xiigen-session-1000-0192
source_title: "Azure DevOps commits retrieval"
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
  conversation_uuid: "57a37478-cd10-4b64-8a3a-4fb81b505ba6"
  conversation_name: "Azure DevOps commits retrieval"
  human_message_uuid: "3b6b2ede-fb05-4214-b57b-ec85cb948267"
  human_message_index: 6
  message_created_at: "2025-10-26T11:52:26.394477Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0192"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    The resource you are requesting could not be found [item 0]
    The controller for path '/Keshet/_apis/git/repositories/ebf71d2e-163c-4eb4-bdd4-4f0bdd793831/commits/a3209314ad3ef15600a0d3f565d92c6c1067bc77/workItems' was not found or does not implement IController. html { height: 100%; } body { font-family: "-apple-system", BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Helvetica, Ubuntu, Arial, sans-serif, "Apple Color Emoji", "Segoe UI Emoji", "Segoe UI Symbol"; font-size: 14px; height: 100%; margin: 0; background-color: #fff; } .container { height: 100%; display: flex; flex-grow: 1; flex-direction: column; } .header { font-size: 14px; line-height: 20px; padding-right: 40px; display: flex; box-shadow: 0px 1px 0px rgba(0, 0, 0, 0.08); } .header > div, .header > a { margin: 14px 0px 14px 32px; text-decoration: none; color: rgba(0, 0, 0, 0.55); } .header > div:first-child { flex-grow: 1; margin-left: 14px; color: #0078d4; display: flex; } .header > div:first-child > div { margin-left: 14px; } .content-container { display: flex; flex-direction: column; justify-content: center; margin-top: 126px; } .content { display: flex; justify-content: center; align-items: center; } .error-info { display: flex; flex-direction: column; width: 500px; } .title { font-size: 28px; font-weight: bold; line-height: 40px; letter-spacing: -0.04em; color: #212121; margin-bottom: 16px; } .details { font-size: 14px; line-height: 20px; color: #212121; margin-bottom: 16px; } .details a { text-decoration: none; color: #0078d4; }
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0192-prompt"
  SourcePath: "claude-export/batch-0000/57a37478-cd10-4b64-8a3a-4fb81b505ba6/messages/3b6b2ede-fb05-4214-b57b-ec85cb948267"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    The resource you are requesting could not be found [item 0]
    The controller for path '/Keshet/_apis/git/repositories/ebf71d2e-163c-4eb4-bdd4-4f0bdd793831/commits/a3209314ad3ef15600a0d3f565d92c6c1067bc77/workItems' was not found or does not implement IController. html { height: 100%; } body { font-family: "-apple-system", BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Helvetica, Ubuntu, Arial, sans-serif, "Apple Color Emoji", "Segoe UI Emoji", "Segoe UI Symbol"; font-size: 14px; height: 100%; margin: 0; background-color: #fff; } .container { height: 100%; display: flex; flex-grow: 1; flex-direction: column; } .header { font-size: 14px; line-height: 20px; padding-right: 40px; display: flex; box-shadow: 0px 1px 0px rgba(0, 0, 0, 0.08); } .header > div, .header > a { margin: 14px 0px 14px 32px; text-decoration: none; color: rgba(0, 0, 0, 0.55); } .header > div:first-child { flex-grow: 1; margin-left: 14px; color: #0078d4; display: flex; } .header > div:first-child > div { margin-left: 14px; } .content-container { display: flex; flex-direction: column; justify-content: center; margin-top: 126px; } .content { display: flex; justify-content: center; align-items: center; } .error-info { display: flex; flex-direction: column; width: 500px; } .title { font-size: 28px; font-weight: bold; line-height: 40px; letter-spacing: -0.04em; color: #212121; margin-bottom: 16px; } .details { font-size: 14px; line-height: 20px; color: #212121; margin-bottom: 16px; } .details a { text-decoration: none; color: #0078d4; }
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "TEST", "UI"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:api
  - API_CONTRACT:request
  - IMPLEMENT:error
  - UI:ui

---

## XIIGEN-SESSION-1000-0193 - Azure DevOps commits retrieval

```
source_id: xiigen-session-1000-0193
source_title: "Azure DevOps commits retrieval"
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
  conversation_uuid: "57a37478-cd10-4b64-8a3a-4fb81b505ba6"
  conversation_name: "Azure DevOps commits retrieval"
  human_message_uuid: "67da0b0a-514e-49ed-8cbe-2347ec5558ab"
  human_message_index: 8
  message_created_at: "2025-10-26T11:53:17.366052Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0193"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Nope everything is here https://dev.azure.com/KESHETDEV/Keshet/_apis/git/repositories/{{$json.commit.repoid}}/commits/{{$json.commit.commitId}}/workItems?api-version=7.1"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0193-prompt"
  SourcePath: "claude-export/batch-0000/57a37478-cd10-4b64-8a3a-4fb81b505ba6/messages/67da0b0a-514e-49ed-8cbe-2347ec5558ab"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: "Nope everything is here https://dev.azure.com/KESHETDEV/Keshet/_apis/git/repositories/{{$json.commit.repoid}}/commits/{{$json.commit.commitId}}/workItems?api-version=7.1"
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:api

---

## XIIGEN-SESSION-1000-0194 - Azure DevOps commits retrieval

```
source_id: xiigen-session-1000-0194
source_title: "Azure DevOps commits retrieval"
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
  conversation_uuid: "57a37478-cd10-4b64-8a3a-4fb81b505ba6"
  conversation_name: "Azure DevOps commits retrieval"
  human_message_uuid: "4d2beab2-a486-4543-a8e7-d03744b53247"
  human_message_index: 14
  message_created_at: "2025-10-26T12:44:34.180663Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0194"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "You’re hitting a non-existent endpoint. Azure DevOps doesn’t expose`/commits/{commitId}/workitems` (even in 7.x). That’s why your URL 404’s."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0194-prompt"
  SourcePath: "claude-export/batch-0000/57a37478-cd10-4b64-8a3a-4fb81b505ba6/messages/4d2beab2-a486-4543-a8e7-d03744b53247"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: "You’re hitting a non-existent endpoint. Azure DevOps doesn’t expose`/commits/{commitId}/workitems` (even in 7.x). That’s why your URL 404’s."
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:endpoint

---

## XIIGEN-SESSION-1000-0195 - Employee task summary report

```
source_id: xiigen-session-1000-0195
source_title: "Employee task summary report"
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
  conversation_uuid: "427a9859-15e0-42ac-9d04-b99f63461ada"
  conversation_name: "Employee task summary report"
  human_message_uuid: "526689bd-8f2c-4114-beb4-3665d3e6c2a4"
  human_message_index: 0
  message_created_at: "2025-10-26T17:31:49.207274Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0195"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    Please make a summary 
    for each employee what has been done: 
    Which tasks he have been doing 
    for each task - what is the task about (area, title, description) short ,what was previous status, what is the latest status,  if there are comments - please write them down, if there are code or pr connected, give a link - short information, just the important one. Do not count the change in fields as activity, just a status change
    
    No need to display all the status changes - just the one at the beginning and at the end
    do not cut comments
    
    No need:
    Status Changes:
    * 2025-10-22 08:14: Active → Resolved (initial)
    * 2025-10-22 08:14: Active → Resolved (final)
    * 2025-10-22 12:24: Active → Closed (initial)
    * ... (3 intermediate changes)
    * 2025-10-22 13:52: Active → Closed (final)
    Just the previous and current: Active -> the old value on 08:14 and end status: Closed -> NewValue on 13:52 -> or by current value on task
    If there is no description no need to add it - but if there is a parent with description in related complete it from the parent
    
    Add section of related tasks with numbers and their relation type
    
    Add to the childs - title, current status
    
    In tasks that contain pr/commits can you bring the names for the prs/commits ? Maybe from task changes? 
    
    Add commits Info - comments for related tasks. And add summary of not connected to work Items commits
    
    No need in double information - group by 
    
    One report should be structured as successful claude report
    
    And another an html in the following format:
    Report Features:
    Main Tasks Table: (all the tasks from taskMainReport.json)
    
    Task ID (clickable link to Azure DevOps)
    Title (in Hebrew)
    Area
    Type (Bug, Feature, User Story, etc.)
    Description (truncated to 250 chars)
    Previous Status - follow the Changes field of the task and complete with missing from tasksChanges.json  - first change of status old value (color-coded badge)
    Current Status (color-coded badge)
    Related Tasks (with relation type) with their current status and title (from the tasksMainReport.json -> relatedFormatted by Type of WorkItem -> All that not WorkItem should be on column Code/PR -> validate they do appear there, even if commit doesn't have workItemId)
    Code/PR (clickable links to commits/PRs) with the comments on commit/pr (follow the Commits and RelatedCode fields of the task and complete missing from commitsInfo.json by work Item Id or the workItemId can be part of a comment work Items or Comment of the commit or by a commit Id in tasksMainReport.json -> relatedFormatted by Type commit or pull request)
    Comments on tasks  with the user and the comment full text (follow the comments array of the task and complete with additional from the comments.json -> to the task or the related tasks by workItemId or sourceWorkItemId in related) - give the whole comment, author and time, ordered by time descending
    AdditionalComments : display changes during the day in the task in task Changes field by comparing old value to new -> interesting in textual field edits, added realtion (explan what exactly: work Item with Id and Parent/Child or Commit/Pr with the appropriate link and comments), changed sprint
    
    Go over files several times and validate that all the information is included
    Unconnected Commits Table:
    
    Commit ID
    Date
    Message
    URL (clickable link)
    
    Do Remove Duplicates -> for each commit id only one line
    
    Design Features:
    ✅ Color-coded status badges
    ✅ Hover effects on rows
    ✅ Clickable links to Azure DevOps
    ✅ Clean, professional styling
    ✅ Responsive design
    ✅ Truncated text for readability
    
    I need two reports -> one in text format - example to successful report
    Another in Html formt -> successfulHtmlFormat
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0195-prompt"
  SourcePath: "claude-export/batch-0000/427a9859-15e0-42ac-9d04-b99f63461ada/messages/526689bd-8f2c-4114-beb4-3665d3e6c2a4"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    Please make a summary 
    for each employee what has been done: 
    Which tasks he have been doing 
    for each task - what is the task about (area, title, description) short ,what was previous status, what is the latest status,  if there are comments - please write them down, if there are code or pr connected, give a link - short information, just the important one. Do not count the change in fields as activity, just a status change
    
    No need to display all the status changes - just the one at the beginning and at the end
    do not cut comments
    
    No need:
    Status Changes:
    * 2025-10-22 08:14: Active → Resolved (initial)
    * 2025-10-22 08:14: Active → Resolved (final)
    * 2025-10-22 12:24: Active → Closed (initial)
    * ... (3 intermediate changes)
    * 2025-10-22 13:52: Active → Closed (final)
    Just the previous and current: Active -> the old value on 08:14 and end status: Closed -> NewValue on 13:52 -> or by current value on task
    If there is no description no need to add it - but if there is a parent with description in related complete it from the parent
    
    Add section of related tasks with numbers and their relation type
    
    Add to the childs - title, current status
    
    In tasks that contain pr/commits can you bring the names for the prs/commits ? Maybe from task changes? 
    
    Add commits Info - comments for related tasks. And add summary of not connected to work Items commits
    
    No need in double information - group by 
    
    One report should be structured as successful claude report
    
    And another an html in the following format:
    Report Features:
    Main Tasks Table: (all the tasks from taskMainReport.json)
    
    Task ID (clickable link to Azure DevOps)
    Title (in Hebrew)
    Area
    Type (Bug, Feature, User Story, etc.)
    Description (truncated to 250 chars)
    Previous Status - follow the Changes field of the task and complete with missing from tasksChanges.json  - first change of status old value (color-coded badge)
    Current Status (color-coded badge)
    Related Tasks (with relation type) with their current status and title (from the tasksMainReport.json -> relatedFormatted by Type of WorkItem -> All that not WorkItem should be on column Code/PR -> validate they do appear there, even if commit doesn't have workItemId)
    Code/PR (clickable links to commits/PRs) with the comments on commit/pr (follow the Commits and RelatedCode fields of the task and complete missing from commitsInfo.json by work Item Id or the workItemId can be part of a comment work Items or Comment of the commit or by a commit Id in tasksMainReport.json -> relatedFormatted by Type commit or pull request)
    Comments on tasks  with the user and the comment full text (follow the comments array of the task and complete with additional from the comments.json -> to the task or the related tasks by workItemId or sourceWorkItemId in related) - give the whole comment, author and time, ordered by time descending
    AdditionalComments : display changes during the day in the task in task Changes field by comparing old value to new -> interesting in textual field edits, added realtion (explan what exactly: work Item with Id and Parent/Child or Commit/Pr with the appropriate link and comments), changed sprint
    
    Go over files several times and validate that all the information is included
    Unconnected Commits Table:
    
    Commit ID
    Date
    Message
    URL (clickable link)
    
    Do Remove Duplicates -> for each commit id only one line
    
    Design Features:
    ✅ Color-coded status badges
    ✅ Hover effects on rows
    ✅ Clickable links to Azure DevOps
    ✅ Clean, professional styling
    ✅ Responsive design
    ✅ Truncated text for readability
    
    I need two reports -> one in text format - example to successful report
    Another in Html formt -> successfulHtmlFormat
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:request
  - IMPLEMENT:code
  - IMPLEMENT:bug
  - SECURITY:auth
  - TEST:test

---

## XIIGEN-SESSION-1000-0196 - Employee task summary report

```
source_id: xiigen-session-1000-0196
source_title: "Employee task summary report"
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
  conversation_uuid: "f720ee98-e6df-49cb-81a5-db30375b8cf8"
  conversation_name: "Employee task summary report"
  human_message_uuid: "d0a872e2-12d1-4f5e-a2f4-126a983bfb48"
  human_message_index: 0
  message_created_at: "2025-10-26T16:04:22.501504Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0196"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    Please make a summary 
    for each employee what has been done: 
    Which tasks he have been doing 
    for each task - what is the task about (area, title, description) short ,what was previous status, what is the latest status,  if there are comments - please write them down, if there are code or pr connected, give a link - short information, just the important one. Do not count the change in fields as activity, just a status change
    
    No need to display all the status changes - just the one at the beginning and at the end
    do not cut comments
    
    No need:
    Status Changes:
    * 2025-10-22 08:14: Active → Resolved (initial)
    * 2025-10-22 08:14: Active → Resolved (final)
    * 2025-10-22 12:24: Active → Closed (initial)
    * ... (3 intermediate changes)
    * 2025-10-22 13:52: Active → Closed (final)
    Just the previous and current: Active -> the old value on 08:14 and end status: Closed -> NewValue on 13:52 -> or by current value on task
    If there is no description no need to add it - but if there is a parent with description in related complete it from the parent
    
    Add section of related tasks with numbers and their relation type
    
    Add to the childs - title, current status
    
    In tasks that contain pr/commits can you bring the names for the prs/commits ? Maybe from task changes? 
    
    Add commits Info - comments for related tasks. And add summary of not connected to work Items commits
    
    No need in double information - group by 
    
    One report should be structured as successful claude report
    
    And another an html in the following format:
    Report Features:
    Main Tasks Table: (all the tasks from taskMainReport.json)
    
    Task ID (clickable link to Azure DevOps)
    Title (in Hebrew)
    Area
    Type (Bug, Feature, User Story, etc.)
    Description (truncated to 150 chars)
    Previous Status - follow the task changes - first change of status old value (color-coded badge)
    Current Status (color-coded badge)
    Related Tasks (with relation type) with their current status and title (from the tasksMainReport.json -> relatedFormatted by Type of WorkItem)
    Code/PR (clickable links to commits/PRs) with the comments on commit/pr (from commitsInfo.json by work Item Id or by a commit Id in tasksMainReport.json -> relatedFormatted by Type commit or pull request)
    Comments on tasks  with the user and the comment full text (from the comments.json -> to the task or the related tasks by workItemId or sourceWorkItemId in related) - give the whole comment, author and time, ordered by time descending
    
    Go over files several times and validate that all the information is included
    Unconnected Commits Table:
    
    Commit ID
    Date
    Message
    URL (clickable link)
    
    Design Features:
    ✅ Color-coded status badges
    ✅ Hover effects on rows
    ✅ Clickable links to Azure DevOps
    ✅ Clean, professional styling
    ✅ Responsive design
    ✅ Truncated text for readability
    
    I need two reports -> one in text format - example to successful report
    Another in Html formt -> successfulHtmlFormat
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0196-prompt"
  SourcePath: "claude-export/batch-0000/f720ee98-e6df-49cb-81a5-db30375b8cf8/messages/d0a872e2-12d1-4f5e-a2f4-126a983bfb48"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    Please make a summary 
    for each employee what has been done: 
    Which tasks he have been doing 
    for each task - what is the task about (area, title, description) short ,what was previous status, what is the latest status,  if there are comments - please write them down, if there are code or pr connected, give a link - short information, just the important one. Do not count the change in fields as activity, just a status change
    
    No need to display all the status changes - just the one at the beginning and at the end
    do not cut comments
    
    No need:
    Status Changes:
    * 2025-10-22 08:14: Active → Resolved (initial)
    * 2025-10-22 08:14: Active → Resolved (final)
    * 2025-10-22 12:24: Active → Closed (initial)
    * ... (3 intermediate changes)
    * 2025-10-22 13:52: Active → Closed (final)
    Just the previous and current: Active -> the old value on 08:14 and end status: Closed -> NewValue on 13:52 -> or by current value on task
    If there is no description no need to add it - but if there is a parent with description in related complete it from the parent
    
    Add section of related tasks with numbers and their relation type
    
    Add to the childs - title, current status
    
    In tasks that contain pr/commits can you bring the names for the prs/commits ? Maybe from task changes? 
    
    Add commits Info - comments for related tasks. And add summary of not connected to work Items commits
    
    No need in double information - group by 
    
    One report should be structured as successful claude report
    
    And another an html in the following format:
    Report Features:
    Main Tasks Table: (all the tasks from taskMainReport.json)
    
    Task ID (clickable link to Azure DevOps)
    Title (in Hebrew)
    Area
    Type (Bug, Feature, User Story, etc.)
    Description (truncated to 150 chars)
    Previous Status - follow the task changes - first change of status old value (color-coded badge)
    Current Status (color-coded badge)
    Related Tasks (with relation type) with their current status and title (from the tasksMainReport.json -> relatedFormatted by Type of WorkItem)
    Code/PR (clickable links to commits/PRs) with the comments on commit/pr (from commitsInfo.json by work Item Id or by a commit Id in tasksMainReport.json -> relatedFormatted by Type commit or pull request)
    Comments on tasks  with the user and the comment full text (from the comments.json -> to the task or the related tasks by workItemId or sourceWorkItemId in related) - give the whole comment, author and time, ordered by time descending
    
    Go over files several times and validate that all the information is included
    Unconnected Commits Table:
    
    Commit ID
    Date
    Message
    URL (clickable link)
    
    Design Features:
    ✅ Color-coded status badges
    ✅ Hover effects on rows
    ✅ Clickable links to Azure DevOps
    ✅ Clean, professional styling
    ✅ Responsive design
    ✅ Truncated text for readability
    
    I need two reports -> one in text format - example to successful report
    Another in Html formt -> successfulHtmlFormat
  CompiledPredicateIds: ["API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:request
  - IMPLEMENT:code
  - IMPLEMENT:bug
  - SECURITY:auth
  - TEST:test

---

## XIIGEN-SESSION-1000-0197 - Pull request commits and work items retrieval

```
source_id: xiigen-session-1000-0197
source_title: "Pull request commits and work items retrieval"
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
  conversation_uuid: "a8c6c491-697c-43e1-af4d-8393a49baa43"
  conversation_name: "Pull request commits and work items retrieval"
  human_message_uuid: "bd67aa3e-2e2a-4965-aab3-a8c79f1af621"
  human_message_index: 0
  message_created_at: "2025-10-27T06:47:21.595133Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0197"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "I need to get for pull request all the connected commits and work Items using get http request n8n node, like we do in current diagram for comments/commits"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0197-prompt"
  SourcePath: "claude-export/batch-0000/a8c6c491-697c-43e1-af4d-8393a49baa43/messages/bd67aa3e-2e2a-4965-aab3-a8c79f1af621"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: "I need to get for pull request all the connected commits and work Items using get http request n8n node, like we do in current diagram for comments/commits"
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:request

---

## XIIGEN-SESSION-1000-0198 - Pull request commits and work items retrieval

```
source_id: xiigen-session-1000-0198
source_title: "Pull request commits and work items retrieval"
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
  conversation_uuid: "a8c6c491-697c-43e1-af4d-8393a49baa43"
  conversation_name: "Pull request commits and work items retrieval"
  human_message_uuid: "32bfb60c-4c47-4c2f-878a-59ef47f25e75"
  human_message_index: 2
  message_created_at: "2025-10-27T11:22:32.408107Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0198"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "how do I get pull requests with work Items?"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0198-prompt"
  SourcePath: "claude-export/batch-0000/a8c6c491-697c-43e1-af4d-8393a49baa43/messages/32bfb60c-4c47-4c2f-878a-59ef47f25e75"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: "how do I get pull requests with work Items?"
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:request

---

## XIIGEN-SESSION-1000-0199 - Pull request commits and work items retrieval

```
source_id: xiigen-session-1000-0199
source_title: "Pull request commits and work items retrieval"
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
  conversation_uuid: "a8c6c491-697c-43e1-af4d-8393a49baa43"
  conversation_name: "Pull request commits and work items retrieval"
  human_message_uuid: "0e5fc82c-66cd-477a-87f3-ba3cdf90e5df"
  human_message_index: 4
  message_created_at: "2025-10-27T11:29:18.786486Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0199"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    this get works https://dev.azure.com/KESHETDEV/Keshet/_apis/git/repositories/{{$json.repositoryId}}/pullrequests/{{$json.pullRequestId}}/workitems?api-version=7.0
    
    But I need it to return a pull request Id as well
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0199-prompt"
  SourcePath: "claude-export/batch-0000/a8c6c491-697c-43e1-af4d-8393a49baa43/messages/0e5fc82c-66cd-477a-87f3-ba3cdf90e5df"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "API Contract Definition"
  CompiledText: |
    this get works https://dev.azure.com/KESHETDEV/Keshet/_apis/git/repositories/{{$json.repositoryId}}/pullrequests/{{$json.pullRequestId}}/workitems?api-version=7.0
    
    But I need it to return a pull request Id as well
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - API_CONTRACT:api
  - API_CONTRACT:request

---

## XIIGEN-SESSION-1000-0200 - Employee task summary report

```
source_id: xiigen-session-1000-0200
source_title: "Employee task summary report"
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
  conversation_uuid: "bae60b65-360a-4cab-ae7a-b0873fbe0ac7"
  conversation_name: "Employee task summary report"
  human_message_uuid: "13812018-f8f7-49f2-81d4-d7be5013735b"
  human_message_index: 0
  message_created_at: "2025-10-26T13:16:15.421568Z"
```

A:
  run_id: "XIIGEN-SESSION-1000-0200"
  scenario_id: "adaptive-logical-orchestration"
  user_request: |
    Please make a summary 
    for each employee what has been done: 
    Which tasks he have been doing 
    for each task - what is the task about (area, title, description) short ,what was previous status, what is the latest status,  if there are comments - please write them down, if there are code or pr connected, give a link - short information, just the important one. Do not count the change in fields as activity, just a status change
    
    No need to display all the status changes - just the one at the beginning and at the end
    do not cut comments
    
    No need:
    Status Changes:
    * 2025-10-22 08:14: Active → Resolved (initial)
    * 2025-10-22 08:14: Active → Resolved (final)
    * 2025-10-22 12:24: Active → Closed (initial)
    * ... (3 intermediate changes)
    * 2025-10-22 13:52: Active → Closed (final)
    Just the previous and current
    If there is no description no need to add it - but if there is a parent with description in related complete it from the parent
    
    Add section of related tasks with numbers and their relation type
    
    Add to the childs - title, current status
    
    In tasks that contain pr/commits can you bring the names for the prs/commits ? Maybe from task changes? 
    
    Add commits Info - comments for related tasks. And add summary of not connected to work Items commits
    
    No need in double information - group by
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "XIIGEN-SESSION-1000-0200-prompt"
  SourcePath: "claude-export/batch-0000/bae60b65-360a-4cab-ae7a-b0873fbe0ac7/messages/13812018-f8f7-49f2-81d4-d7be5013735b"
  SourceTier: "claude_export_session"
  LicenseOrAuthority: "first_party_private"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Backend Services"
  CompiledText: |
    Please make a summary 
    for each employee what has been done: 
    Which tasks he have been doing 
    for each task - what is the task about (area, title, description) short ,what was previous status, what is the latest status,  if there are comments - please write them down, if there are code or pr connected, give a link - short information, just the important one. Do not count the change in fields as activity, just a status change
    
    No need to display all the status changes - just the one at the beginning and at the end
    do not cut comments
    
    No need:
    Status Changes:
    * 2025-10-22 08:14: Active → Resolved (initial)
    * 2025-10-22 08:14: Active → Resolved (final)
    * 2025-10-22 12:24: Active → Closed (initial)
    * ... (3 intermediate changes)
    * 2025-10-22 13:52: Active → Closed (final)
    Just the previous and current
    If there is no description no need to add it - but if there is a parent with description in related complete it from the parent
    
    Add section of related tasks with numbers and their relation type
    
    Add to the childs - title, current status
    
    In tasks that contain pr/commits can you bring the names for the prs/commits ? Maybe from task changes? 
    
    Add commits Info - comments for related tasks. And add summary of not connected to work Items commits
    
    No need in double information - group by
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

Predicate evidence:
  - IMPLEMENT:code
  - TEST:test

---

