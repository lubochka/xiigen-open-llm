# Row12 Webview DOM Scope Diagnostic

- status: DOM_ROUTE_UNAVAILABLE
- created_utc: 20260625T133145Z
- files_changed: none
- product_code_changed: false
- runner_changed: false
- no_git_action_run: true
- diagnostic_method: Read-only static runner launch inspection, current Cursor process command-line grep, and Cursor-owned TCP listener count. No submit, no typing, no TEST_ONLY handler.
- runner_has_remote_debugging_arg: False
- runner_has_open_devtools_arg: False
- row12_related_cursor_process_count: 11
- row12_related_remote_debugging_process_count: 0
- all_cursor_remote_debugging_process_count: 0
- cursor_owned_tcp_listener_count: 0
- cdp_json_list_success_count: 0
- xiigen_or_webview_target_candidate_count: 0
- selector_dom_query_executed: false
- selector_dom_query_not_executed_reason: No remote-debugging arg in runner or Row12 Cursor processes, and no Cursor-owned listening TCP port was present, so there was no reachable scoped Chromium CDP target to query.

Selectors/text requested:
- #xiigen-smart-skills-prompt: not found; no scoped DOM target available to query
- #xiigen-smart-skills-submit: not found; no scoped DOM target available to query
- XIIGen Smart Skills: not found through DOM; only prior unscoped/title/UIA evidence exists
- XIIGen task prompt: not found; no scoped DOM target available to query

Meaning: current Row12 evidence cannot distinguish hidden DOM from not-created/not-painted. It proves the current runner launch has no scoped webview DOM route.

Smallest next non-product fix: add runner-only read-only diagnostics to launch Cursor with a fixed localhost --remote-debugging-port, list /json targets, attach only to the vscode-webview/XIIGen target, query the required selectors/text, and record the result.
