# Layer 0-C — Knowledge-Edge Pairs (056)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: AWS S3 connector

---

## B17-0C-056-001 — AWS S3: non-trigger terms

**Proper nouns — no predicate:** S3, AWS, Amazon
**Domain nouns — no predicate:** bucket, object, key, prefix, lifecycle, policy, ACL (Access Control List), versioning, replication, multipart, presigned URL
**Acronyms — no predicate:** S3 (Simple Storage Service), ACL (Access Control List), IAM (Identity and Access Management)

---

## B17-0C-056-002 — AWS S3: presigned URL alone does not fire SECURITY

"Generate a presigned URL for object download" → no SECURITY.
"Generate a presigned URL access token" → SECURITY fires (compound contains "token").
"Use IAM access token for bucket operations" → SECURITY fires (compound contains "token").
"Use IAM role" alone → no SECURITY (no "token").

---

## B17-0C-056-003 — AWS S3: STS expansion fires SECURITY

"STS" stands for "Security Token Service" — the expansion contains "Token".
When the expansion is written: "AWS STS (Security Token Service) temporary access token" → SECURITY fires twice (both from expansion and from "access token") — but fires once (deduplication).
When abbreviation only: "STS temporary credentials" → no SECURITY (no "token" visible).

---

## B17-0C-056-004 — AWS S3: "upload" and "download" are not IMPLEMENT

"Upload files to S3" → no predicate.
"Download objects from S3" → no predicate.
"Build the S3 connector to handle file uploads and downloads" → IMPLEMENT fires from "Build"; "connector" fires API_CONTRACT.
