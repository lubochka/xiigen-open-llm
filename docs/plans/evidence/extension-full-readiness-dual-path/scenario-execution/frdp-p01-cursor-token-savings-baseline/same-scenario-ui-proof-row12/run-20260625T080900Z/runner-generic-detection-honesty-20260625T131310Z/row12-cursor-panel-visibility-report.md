# Row12 Cursor Panel Visibility Runner

status: PANEL_NOT_VISIBLE_WITH_HONEST_FAIL

Command invoked: `xiigenSmartSkills.panel`

What it verifies:
- visual text confirmed: FAIL
- panel title visible: PASS
- prompt textarea visible: PASS
- generic Cursor chat/login indicators absent: FAIL
- no submit performed: PASS
- screenshot captured: PASS

Visual verifier:
- visual_text_confirmed: False
- visual_text_extraction_method: uia_accessibility_snapshot_no_ocr
- visual_text_extraction_available: True
- visual_text_extraction_reliable: True
- xiigen_labels_text_confirmed: True
- generic_labels_text_confirmed: True
- generic_detection_inconclusive: False
- generic_detection_reason: UIA text confirmed one or more configured generic Cursor labels.
- window_title_contains_xiigen: True
- title_only_rejected: True
- generic_cursor_chat_or_login_detected: True
- manual_review_note: screenshot/manual review required because text extraction did not confirm XIIGen content.

Generic indicators detected:
- / for skills (uia_text)
- @ for context (uia_text)
- Log in (uia_text)

Fail markers:
- generic_cursor_chat_or_login_detected: / for skills, @ for context, Log in
- title_only_not_accepted: window title contains XIIGen but visual/UI text did not confirm required panel content

Screenshot: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\runner-generic-detection-honesty-20260625T131310Z\row12-cursor-panel.png

JSON report: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\runner-generic-detection-honesty-20260625T131310Z\row12-cursor-panel-visibility-report.json

Short temp runtime paths:
- user-data-dir: C:\Users\lubao\AppData\Local\Temp\xiigen-row12-32b9a4e2\ud
- extensions-dir: C:\Users\lubao\AppData\Local\Temp\xiigen-row12-32b9a4e2\ext
- copied diagnostics: C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-token-savings-baseline\same-scenario-ui-proof-row12\run-20260625T080900Z\runner-generic-detection-honesty-20260625T131310Z\row12-short-temp-runtime-paths.json

This is not a product/readiness claim. It does not perform textarea typing or submit.
