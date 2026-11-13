# Redaction note

Raw private UI labels, chat text, account data, password/PIN values, previous clipboard contents, and the raw scenario prompt were not written to evidence. The runner loaded the prompt in memory and compared it by hash/boolean probes. Logs store prompt length/hash and redacted UI summaries only. The runner may set the clipboard to the safe scenario prompt for paste fallback, but it does not read or store prior clipboard contents.
