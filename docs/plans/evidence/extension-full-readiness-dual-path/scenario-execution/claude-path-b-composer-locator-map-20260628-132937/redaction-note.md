# Redaction note

Evidence packet stores redacted UI Automation metadata only. Raw private Claude text, chat content, account text, prompt text, clipboard text and screenshots were not saved.

Names were used in memory only for two bounded classifications:

- safe route detection: XIIGen / Smart Skills / llm_mvp_core terms;
- generic composer detection: composer/prompt/message style labels.

Candidate logs contain safe buckets instead of raw labels, plus hashes for runtime id, class and automation id.
