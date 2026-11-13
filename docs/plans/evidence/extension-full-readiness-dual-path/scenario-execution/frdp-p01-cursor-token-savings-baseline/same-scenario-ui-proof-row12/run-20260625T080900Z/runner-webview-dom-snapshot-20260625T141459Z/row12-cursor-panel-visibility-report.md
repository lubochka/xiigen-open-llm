# Row12 Cursor Panel Visibility Runner

status: NEEDS_FIX

Command invoked: `xiigenSmartSkills.panel`

CDP/WebView DOM:
- port: 65031
- endpoint: http://127.0.0.1:65031/json
- version_endpoint: http://127.0.0.1:65031/json/version
- browser_version_available: True
- browser_websocket_debugger_url: ws://127.0.0.1:65031/devtools/browser/1c48dd73-ba80-45f5-8500-1bdefc33c642
- browser_websocket_debugger_url_present: True
- status: WEBVIEW_DOM_SNAPSHOT_CAPTURED
- target_count: 7
- merged_target_count: 7
- json_target_count: 3
- browser_target_count: 4
- candidate_target_count: 4
- browser_candidate_target_count: 3
- targets_path: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\runner-webview-dom-snapshot-20260625T141459Z\row12-cdp-targets.json
- browser_version_path: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\runner-webview-dom-snapshot-20260625T141459Z\row12-cdp-browser-version.json
- browser_target_gettargets_path: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\runner-webview-dom-snapshot-20260625T141459Z\row12-browser-cdp-target-gettargets.json
- query_path: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\runner-webview-dom-snapshot-20260625T141459Z\row12-cdp-webview-dom-query.json
- browser_target_gettargets_attempted: True
- browser_target_gettargets_available: True
- browser_target_gettargets_error:
- inspection_timing: before_process_close
- all_required_selectors_found: False
- all_required_text_found: False
- all_required_dom_visible: False

Panel hold:
- requested_ms: 30000
- effective_ms: 30000
- source: -PanelHoldMs runner parameter propagated through ROW12_PANEL_HOLD_MS and generated extension test
- fallback_5000_used: False

What it verifies:
- panel content labels confirmed: FAIL_REQUIRES_SCOPED_PANEL_CONTENT_PROOF
- visual text confirmed: FAIL_REQUIRES_SCOPED_PANEL_CONTENT_PROOF
- unscoped/window XIIGen text present: True
- generic Cursor chat/login indicators absent: FAIL
- no submit performed: PASS
- screenshot captured: PASS

Visual verifier:
- panel_content_labels_confirmed: False
- panel_content_confirmation_source: none: CDP/webview DOM inspection did not confirm the required selectors and text. Screenshot and whole-window UIA remain diagnostics only.
- panel_content_confirmation_method: none
- panel_content_proof_required: scoped read-only CDP/webview DOM inspection confirming #xiigen-smart-skills-prompt, #xiigen-smart-skills-submit, XIIGen Smart Skills, and XIIGen task prompt
- visual_text_confirmed: False
- visual_text_extraction_method: unscoped_whole_window_uia_accessibility_snapshot_no_ocr
- visual_text_extraction_available: True
- visual_text_extraction_reliable: False
- window_title_contains_xiigen: True
- uia_unscoped_xiigen_text_present: True
- uia_unscoped_text_is_panel_content_proof: False
- uia_unscoped_has_xiigen_smart_skills_text: True
- uia_unscoped_has_prompt_label_text: True
- uia_unscoped_has_submit_to_llm_mvp_core_text: True
- uia_unscoped_has_xiigen_textarea_accessible_text: False
- generic_labels_text_confirmed: True
- generic_detection_inconclusive: False
- generic_detection_reason: UIA text confirmed one or more configured generic Cursor labels.
- title_only_rejected: True
- generic_cursor_chat_or_login_detected: True
- manual_review_note: panel content remains unconfirmed because this run has only whole-window UIA text and a screenshot without OCR or scoped DOM/UIA provenance.

Why window/unscoped text is not enough:
The native Cursor window title can contain XIIGen because the tab or command name mentions it, and whole-window UIA text can include titles, tabs, menus, hidden controls, or off-panel accessibility names. Row12 panel-content proof requires visible panel content from OCR, DOM/webview inspection, or UIA scoped to the XIIGen panel itself.

CDP targets summary:
- [0] source=json_list; id=23877097A257E9E4BE17C77A9307FDEB; targetId=; type=page; attached=; openerId=; candidate=True; title=[Extension Development Host] XIIGen Smart Skills - cursor-xiigen-smart-skills - Cursor; url=vscode-file://vscode-app/c:/Users/lubao/AppData/Local/Programs/cursor/resources/app/out/vs/code/electron-sandbox/workbench/workbench.html; reasons=xiigen,smart-skills
- [1] source=json_list; id=AAB126C7F193367D77235FEC45ACD303; targetId=; type=iframe; attached=; openerId=; candidate=True; title=vscode-webview://1tceov6d5fqk788ajtqabnn09fmm0eka6s7t4blua9sg5deatg3j/index.html?id=1ce8ec43-b00e-4b28-aa83-2a3dd913ad75&amp;origin=17591af6-98e3-42f9-8601-6bf64aa4dcdd&amp;swVersion=4&amp;extensionId=xiigen.cursor-xiigen-smart-skills&amp;platform=electron&amp;vscode-resource-base-authority=vscode-resource.vscode-cdn.net&amp;parentOrigin=vscode-file%3A%2F%2Fvscode-app; url=vscode-webview://1tceov6d5fqk788ajtqabnn09fmm0eka6s7t4blua9sg5deatg3j/index.html?id=1ce8ec43-b00e-4b28-aa83-2a3dd913ad75&origin=17591af6-98e3-42f9-8601-6bf64aa4dcdd&swVersion=4&extensionId=xiigen.cursor-xiigen-smart-skills&platform=electron&vscode-resource-base-authority=vscode-resource.vscode-cdn.net&parentOrigin=vscode-file%3A%2F%2Fvscode-app; reasons=vscode-webview,xiigen
- [2] source=json_list; id=DA16D92804D21E9B15DEBED3DDEAA81B; targetId=; type=service_worker; attached=; openerId=; candidate=True; title=Service Worker vscode-webview://1tceov6d5fqk788ajtqabnn09fmm0eka6s7t4blua9sg5deatg3j/service-worker.js?v=4&amp;vscode-resource-base-authority=vscode-resource.vscode-cdn.net&amp;id=1ce8ec43-b00e-4b28-aa83-2a3dd913ad75&amp;remoteAuthority=; url=vscode-webview://1tceov6d5fqk788ajtqabnn09fmm0eka6s7t4blua9sg5deatg3j/service-worker.js?v=4&vscode-resource-base-authority=vscode-resource.vscode-cdn.net&id=1ce8ec43-b00e-4b28-aa83-2a3dd913ad75&remoteAuthority=; reasons=vscode-webview
- [0] source=browser_target_gettargets; id=DA16D92804D21E9B15DEBED3DDEAA81B; targetId=DA16D92804D21E9B15DEBED3DDEAA81B; type=service_worker; attached=False; openerId=; candidate=True; title=Service Worker vscode-webview://1tceov6d5fqk788ajtqabnn09fmm0eka6s7t4blua9sg5deatg3j/service-worker.js?v=4&vscode-resource-base-authority=vscode-resource.vscode-cdn.net&id=1ce8ec43-b00e-4b28-aa83-2a3dd913ad75&remoteAuthority=; url=vscode-webview://1tceov6d5fqk788ajtqabnn09fmm0eka6s7t4blua9sg5deatg3j/service-worker.js?v=4&vscode-resource-base-authority=vscode-resource.vscode-cdn.net&id=1ce8ec43-b00e-4b28-aa83-2a3dd913ad75&remoteAuthority=; reasons=vscode-webview
- [1] source=browser_target_gettargets; id=4A1A200F8E7B1B63B8F7B6AC4315D369; targetId=4A1A200F8E7B1B63B8F7B6AC4315D369; type=worker; attached=False; openerId=; candidate=False; title=; url=; reasons=
- [2] source=browser_target_gettargets; id=23877097A257E9E4BE17C77A9307FDEB; targetId=23877097A257E9E4BE17C77A9307FDEB; type=page; attached=False; openerId=; candidate=True; title=[Extension Development Host] XIIGen Smart Skills - cursor-xiigen-smart-skills - Cursor; url=vscode-file://vscode-app/c:/Users/lubao/AppData/Local/Programs/cursor/resources/app/out/vs/code/electron-sandbox/workbench/workbench.html; reasons=xiigen,smart-skills
- [3] source=browser_target_gettargets; id=AAB126C7F193367D77235FEC45ACD303; targetId=AAB126C7F193367D77235FEC45ACD303; type=iframe; attached=False; openerId=; candidate=True; title=vscode-webview://1tceov6d5fqk788ajtqabnn09fmm0eka6s7t4blua9sg5deatg3j/index.html?id=1ce8ec43-b00e-4b28-aa83-2a3dd913ad75&origin=17591af6-98e3-42f9-8601-6bf64aa4dcdd&swVersion=4&extensionId=xiigen.cursor-xiigen-smart-skills&platform=electron&vscode-resource-base-authority=vscode-resource.vscode-cdn.net&parentOrigin=vscode-file%3A%2F%2Fvscode-app; url=vscode-webview://1tceov6d5fqk788ajtqabnn09fmm0eka6s7t4blua9sg5deatg3j/index.html?id=1ce8ec43-b00e-4b28-aa83-2a3dd913ad75&origin=17591af6-98e3-42f9-8601-6bf64aa4dcdd&swVersion=4&extensionId=xiigen.cursor-xiigen-smart-skills&platform=electron&vscode-resource-base-authority=vscode-resource.vscode-cdn.net&parentOrigin=vscode-file%3A%2F%2Fvscode-app; reasons=vscode-webview,xiigen

CDP selector/text results:
- target_index=1; source=json_list; id=AAB126C7F193367D77235FEC45ACD303; targetId=; status=WEBVIEW_DOM_SNAPSHOT_CAPTURED; selectors={"#xiigen-smart-skills-prompt":false,"#xiigen-smart-skills-submit":false}; text={"XIIGen Smart Skills":false,"XIIGen task prompt":false}; error=
- target_index=2; source=json_list; id=DA16D92804D21E9B15DEBED3DDEAA81B; targetId=; status=WEBVIEW_DOM_EMPTY_OR_WRONG_SURFACE; selectors={"#xiigen-smart-skills-prompt":false,"#xiigen-smart-skills-submit":false}; text={"XIIGen Smart Skills":false,"XIIGen task prompt":false}; error=
- target_index=0; source=browser_target_gettargets; id=DA16D92804D21E9B15DEBED3DDEAA81B; targetId=DA16D92804D21E9B15DEBED3DDEAA81B; status=WEBVIEW_DOM_EMPTY_OR_WRONG_SURFACE; selectors={"#xiigen-smart-skills-prompt":false,"#xiigen-smart-skills-submit":false}; text={"XIIGen Smart Skills":false,"XIIGen task prompt":false}; error=
- target_index=3; source=browser_target_gettargets; id=AAB126C7F193367D77235FEC45ACD303; targetId=AAB126C7F193367D77235FEC45ACD303; status=WEBVIEW_DOM_SNAPSHOT_CAPTURED; selectors={"#xiigen-smart-skills-prompt":false,"#xiigen-smart-skills-submit":false}; text={"XIIGen Smart Skills":false,"XIIGen task prompt":false}; error=

Generic indicators detected:
- / for skills (uia_text)
- @ for context (uia_text)
- Log in (uia_text)

Fail markers:
- panel_content_labels_confirmed=false: no screenshot OCR, DOM, or scoped UIA source confirmed visible XIIGen panel content
- uia_unscoped_xiigen_text_not_accepted: whole-window UIA text may come from title, tabs, menus, or hidden/off-panel controls
- generic_cursor_chat_or_login_detected: / for skills, @ for context, Log in
- title_only_not_accepted: window title contains XIIGen but does not prove visible panel content labels

Screenshot: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\runner-webview-dom-snapshot-20260625T141459Z\row12-cursor-panel.png

JSON report: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\runner-webview-dom-snapshot-20260625T141459Z\row12-cursor-panel-visibility-report.json

Short temp runtime paths:
- user-data-dir: C:\Users\lubao\AppData\Local\Temp\xiigen-row12-3d16168b\ud
- extensions-dir: C:\Users\lubao\AppData\Local\Temp\xiigen-row12-3d16168b\ext
- copied diagnostics: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\runner-webview-dom-snapshot-20260625T141459Z\row12-short-temp-runtime-paths.json

This is not a product/readiness claim. It does not perform textarea typing or submit.
