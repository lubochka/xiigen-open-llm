# Node 08 - Arbiter Units Training Set

Node being trained: Arbiter units

Main edge preserved: selected arbiter + result -> pass, concern, or reprocess fragment

Every row keeps the source text. A and B are written as human training instructions for this node.

## Rows

### DERIVED ROW DERIVED-ARB-001
Source anchor: D105
Status: derived rejected example built from source rules, not a direct source quote.
A received by this node: The result stores the sign-in token in visible browser storage.
B this node must produce: Flag it. The source says tokens are stored in httpOnly, Secure, SameSite cookies. Send the affected fragment back for reprocessing.
Edge this row prepares: selected arbiter + result -> pass, concern, or reprocess fragment
Why this row belongs here: arbiters need rejected examples, but the rejected case must stay tied to the source rule it violates.

### DERIVED ROW DERIVED-ARB-002
Source anchor: D115 and D257
Status: derived rejected example built from source rules, not a direct source quote.
A received by this node: The result allows ten attempts per fifteen minutes.
B this node must produce: Flag it. The source says five attempts per IP per fifteen minutes. Send the affected fragment back for reprocessing.
Edge this row prepares: selected arbiter + result -> pass, concern, or reprocess fragment
Why this row belongs here: arbiters need rejected examples, but the rejected case must stay tied to the source rule it violates.

### DERIVED ROW DERIVED-ARB-003
Source anchor: D117 and D268
Status: derived rejected example built from source rules, not a direct source quote.
A received by this node: The result uses a short easy verification token.
B this node must produce: Flag it. The source requires a cryptographically random 256-bit token. Send the affected fragment back for reprocessing.
Edge this row prepares: selected arbiter + result -> pass, concern, or reprocess fragment
Why this row belongs here: arbiters need rejected examples, but the rejected case must stay tied to the source rule it violates.

### DERIVED ROW DERIVED-ARB-004
Source anchor: D241 and D074
Status: derived rejected example built from source rules, not a direct source quote.
A received by this node: The result creates the same user twice after a repeated callback.
B this node must produce: Flag it. The source says event handlers must be idempotent and must check whether the user already exists. Send the affected fragment back for reprocessing.
Edge this row prepares: selected arbiter + result -> pass, concern, or reprocess fragment
Why this row belongs here: arbiters need rejected examples, but the rejected case must stay tied to the source rule it violates.

### DERIVED ROW DERIVED-ARB-005
Source anchor: D277
Status: derived rejected example built from source rules, not a direct source quote.
A received by this node: The result gives the merged account a new identity and drops the older one.
B this node must produce: Flag it. The source says the merge preserves the older userId. Send the affected fragment back for reprocessing.
Edge this row prepares: selected arbiter + result -> pass, concern, or reprocess fragment
Why this row belongs here: arbiters need rejected examples, but the rejected case must stay tied to the source rule it violates.

### DERIVED ROW DERIVED-ARB-006
Source anchor: D071 and D255
Status: derived rejected example built from source rules, not a direct source quote.
A received by this node: The result discards a failed questionnaire delivery.
B this node must produce: Flag it. The source says failed event handlers go to a dead letter queue and the retry mechanism delivers later. Send the affected fragment back for reprocessing.
Edge this row prepares: selected arbiter + result -> pass, concern, or reprocess fragment
Why this row belongs here: arbiters need rejected examples, but the rejected case must stay tied to the source rule it violates.

### DERIVED ROW DERIVED-ARB-007
Source anchor: D121-D125
Status: derived rejected example built from source rules, not a direct source quote.
A received by this node: The result omits consent, deletion support, or audit logging.
B this node must produce: Flag it. The source makes these compliance and audit requirements explicit. Send the affected fragment back for reprocessing.
Edge this row prepares: selected arbiter + result -> pass, concern, or reprocess fragment
Why this row belongs here: arbiters need rejected examples, but the rejected case must stay tied to the source rule it violates.

