# Security Policy

## Supported versions

This repository is published with a fresh history and is developed on a single
default branch — there are no long-lived release branches. The supported version
is the current state of the default branch. Fixes land on the default branch;
older tags are not back-ported.

## Reporting a vulnerability

Please do not open a public issue for a security vulnerability, and do not
include secrets, credentials, or personal data in a report.

Use GitHub private vulnerability reporting for this repository (the repository
"Security" tab, then "Report a vulnerability"). Reports stay private to the
maintainer until a fix is available. We aim to acknowledge a report within a
reasonable time and to coordinate the fix and disclosure with the reporter.

<!-- A dedicated security mailbox may be listed here if the maintainer sets one
     up. A personal email address is deliberately NOT published. -->

## Scope

This policy covers the source of the from-scratch machine-learning stack in this
repository: the numbered subprojects (`00.*`–`44.*`) and the `tools/` directory.
Generated artifacts and run outputs are not part of the published release.

## Secrets and personal data

This repository is scrubbed of secrets, tokens, and personal data before release
and is re-published with a fresh history. If you believe a real secret or a
personal datum is still present, report it privately as described above rather
than in a public issue, and do not paste the secret value into the report.
