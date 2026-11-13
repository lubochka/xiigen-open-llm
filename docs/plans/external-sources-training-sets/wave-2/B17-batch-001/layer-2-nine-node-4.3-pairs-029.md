# Layer 2 — Nine-Node 4.3 Pairs (029)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-029-001 — CloudinaryConnector nine-node flow

**A (Input):** SelectedLogicalTransition: "Build the Cloudinary media upload connector with signed URL webhook callbacks and API secret token validation."

**B (Nine-node execution):**
```
N01: ["IMPLEMENT", "API_CONTRACT", "SECURITY"] — media upload, transformation, delivery webhook
N02: Cloudinary Upload API v1 — 3 handlers: UploadMediaHandler, TransformMediaHandler, HandleUploadWebhookHandler
N03: ICloudinaryConnector: UploadMediaAsync, TransformMediaAsync, HandleUploadWebhookAsync
N04: HttpClient (injected), ICloudinarySignatureValidator (HMAC-SHA256 API secret), ICloudinaryCredentialStore (CloudName + ApiKey + ApiSecret)
N05: 4 files (connector, interface, signature validator, models)
N06: POST /v1_1/{cloud_name}/image/upload; GET /v1_1/{cloud_name}/image/{public_id}; webhook POST callbacks; HMAC-SHA256 with API secret
N07: ArbiterSelectorUnit selects completeness + security review
N08: PASS — 3 handlers implemented, HMAC-SHA256 API secret validation present, credential store injected, filesGenerated: 4
N09: CloudinaryConnector complete, 4 files generated
```

Gate B: 3 handlers + HMAC-SHA256 API secret validation + injected credential store
