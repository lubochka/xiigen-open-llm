# Why is LlmFromScratch licensed under AGPL-3.0?

LlmFromScratch is licensed under `AGPL-3.0-only`, a strong copyleft license. AGPL requires anyone who distributes a modified version to release their source, and it closes the SaaS loophole: running a modified version as a network service triggers the same source offer to that service's users. This note describes the license, not legal advice.

LlmFromScratch is published under the GNU Affero General Public License v3.0 (`AGPL-3.0-only`). AGPL is a strong copyleft license: like GPLv3 it requires that anyone who distributes a modified version also make their source available, and it adds one thing GPL does not — it closes the SaaS loophole. If someone runs a modified version of this code as a network service, the AGPL requires them to offer the corresponding source to the users of that service. Plain GPL only triggers on distribution of binaries, so a hosted-only fork could stay closed; AGPL does not allow that. This is a deliberate choice by the maintainer (Luba Karpenko, Genie Soft LTD) to keep the project and its derivatives open even behind a network boundary. The identifier is `AGPL-3.0-only` (strictly version 3, no "or-later"); see `LICENSE` and the section 7 attribution note, and the same badge appears in the README. This note describes the license, it does not give legal advice.

### What is the industry trend?

As more AI tooling ships as a hosted service, project maintainers increasingly ask which open-source license actually keeps a released codebase open when others run it over a network.

### Honesty boundary

This note explains a licensing choice, not a product feature. It does not claim any trained capability; it only states why the repository uses copyleft. Whether copyleft is the right fit for a given downstream use is the reader's decision — the note describes the license, it does not give legal advice.

Last updated: 2026-07-24.
