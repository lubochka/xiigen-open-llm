# Layer 0-C — Knowledge-Edge Pairs (036)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-036-001 — SignedArtifact context boundary

### SignedArtifact terms: context only (no predicate)
| Term | Category | Notes |
|---|---|---|
| signature | Security artifact | No predicate alone |
| signing certificate | Certificate noun | No predicate |
| signed artifact | Release noun | No predicate |
| code signing | Signing process | No predicate |
| digital signature | Signature type | No predicate |
| signing key | Key noun | No predicate |
| cosign | Tool name | No predicate |

### SignedArtifact inputs producing knowledge_edge
| Input | Analysis |
|---|---|
| "Sign the release artifact." | "Sign" (action verb) not in predicate lexicon → [] |
| "Issue the signing certificate." | "Issue" not in lexicon → [] |
| "Generate the code signing key." | "Generate" not in lexicon → [] |

---

## B38-0C-036-002 — AttestationPolicy context boundary

### AttestationPolicy terms: context only
| Term | Category | Notes |
|---|---|---|
| attestation | Certification noun | No predicate alone |
| attestation policy | Policy noun | No predicate alone |
| attestation record | Record noun | No predicate |
| in-toto | Tool name | No predicate |
| SLSA (Supply chain Levels for Software Artifacts) | Standard noun | No predicate |
| attestation chain | Chain noun | No predicate |

### AttestationPolicy inputs producing knowledge_edge
| Input | Analysis |
|---|---|
| "Issue the attestation for the release." | "Issue" not in lexicon → [] |
| "Configure the SLSA attestation policy." | "Configure" not in lexicon → [] |

---

## B38-0C-036-003 — "sign" and "issue" as gap verbs

"Sign" (action: to sign a document/artifact) is not in any predicate lexicon.
"Issue" (action: to issue a certificate/attestation) is not in any predicate lexicon.
Both produce knowledge_edge when they are the only action verb.

| Sentence | PredicateSet |
|---|---|
| "Sign the release artifact." | [] → knowledge_edge |
| "Issue the attestation." | [] → knowledge_edge |
| "Verify the artifact signature and approve." | ["QA"] ("verify" and "approve" fire QA) |
| "Audit the security of the signing keys." | ["QA","SECURITY"] (audit+security co-fire) |
| "Document the signing policy criteria." | ["DOCUMENT"] (document fires; criteria consolidates) |
