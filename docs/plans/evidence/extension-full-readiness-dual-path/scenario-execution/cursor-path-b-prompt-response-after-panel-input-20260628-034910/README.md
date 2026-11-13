# Cursor Path B prompt-response UI scenario

Checked: local Cursor Extension Development Host, XIIGen Smart Skills panel, exact prompt from scenario.json, and Submit button path.

Prompt source: exact_prompt.

Steps: found and focused the Cursor window, captured the window before typing, pasted the exact prompt into the textarea, captured the before-submit state, clicked Submit, waited up to 60 seconds for visible response/status, and captured the after-submit state.

Result: ACCEPTED_UI_PROMPT_SUBMIT_STATUS

Proven: Prompt typed, Submit clicked, and the after-submit screenshot shows the visible status `Submitting prompt to llm_mvp_core...`.

Not proven: a final visible core response caused by this submit. The Core API response block is already visible before typing and before submit in screenshots 01 and 02, so screenshot 03 only supports the submit-status claim. Token savings, semantic quality improvement, learnability/training, checkpoint promotion, and all-host readiness are not claimed.

Blocker: none

Screenshots are in the screenshots folder.
