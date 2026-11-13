---
artifact_type: dpo_legal_model_policy
authority_source: latest unquoted Luba work order for DPO legal build library
status: populated_now_provider_terms_future
last_updated: 2026-06-30
write_scope: C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\claude-*.md
---

# DPO Legal Model Policy

## Policy Scope

This policy governs model use for DPO data generation, DPO training, judging, and evaluation for `ILearningDynamicGraph`. It is a planning policy, not legal advice and not provider approval. Provider terms and model licenses were not checked in this session.

## Core Rule

No closed or hosted API output may be used for common model training, distillation, or DPO pair generation unless a current provider terms snapshot explicitly permits that exact use and Luba approves that route in a new current instruction.

## Closed API Prohibitions

| prohibited use | reason | required future override |
|---|---|---|
| Train common model on closed API outputs | Distillation and output-use terms can forbid training | Provider terms snapshot plus Luba approval |
| Use hosted API as hidden teacher for common DPO rows | Creates unverified teacher provenance | Provider terms snapshot plus legal review |
| Use closed API as judge without terms review | Judge outputs can become training signal | Evaluation-only terms and retention check |
| Store raw project code or private text in hosted provider logs | Conflicts with local and privacy boundary | Redaction plus provider retention approval |
| Treat public model access as training permission | Access is not the same as derivative training rights | License and terms proof |
| Use grounding/web-search/retrieval service mode for a train/judge call (RR6) | Output mixes third-party web content of foreign license; brand-denylist does not catch it | `service_mode=="plain"`, `grounding_used==false` (C17) |
| Treat model license (Apache-2.0) as provider-terms approval (RR6) | Model weights license ≠ host ToS (retention/output-use) | Provider terms snapshot with date; until then `terms_snapshot_ref=PENDING` |

## Candidate Conditions

| candidate type | allowed now | possible future allowed use | required proof |
|---|---|---|---|
| Self-hosted open-weight model | evidence-only planning mention | Teacher, judge, or base model if license permits | Model license, model card, version, checksum, allowed derivative use |
| Hosted open model endpoint | not training source now | Evaluation-only or teacher only if provider terms allow | Hosted terms, retention policy, output-use permission |
| Closed API provider | blocked for training now | Limited judge or evidence route only after explicit terms | Terms snapshot, no-distillation proof, Luba approval |
| Human or Luba labels | allowed planning route | Gold chosen and rejected examples | Consent, provenance, source id |
| Project-local deterministic output | allowed as rejected or baseline candidate | Low-quality local rejected rows or baseline comparison | Local source id, generation config, no private leak |

## Provenance Fields

> **CANON-RECONCILIATION (B-13 + шаг R5-teacher-call + RR6).** Канонический состав полей
> `TeacherProvenance` (**20 полей**, RR6: +`service_mode`/`grounding_used`) — **владелец `claude-plan-08-legal-teacher-gate.md §3.1`**;
> этот короткий policy-файл его НЕ переопределяет. Прежний список ниже нёс **отвергнутые
> дублирующие имена** (`teacher_provider`, `teacher_model_id`, `teacher_deployment_mode`,
> `judge_provider`, `judge_model_id`), которые B-13 заменил каноном
> (`hosting_provider`, `model_id`, `teacher_access_mode`, `judge_model` внутри записи). Эти
> поля заполняет реальный путь вызова `ITeacherClient`/`IJudgeClient`
> (`claude-plan-08 §3.6`). При любом расхождении главенствует владелец 08.

Канонические 20 полей `TeacherProvenance` (дословно по `claude-plan-08 §3.1`,
заполняются `ITeacherClient.RequestGoalAsync` / `IJudgeClient.JudgeAsync` §3.6):

```text
# 13 базовых:
teacher_access_mode
hosting_provider                 # (заменяет отвергнутый teacher_provider)
model_id                         # (заменяет отвергнутый teacher_model_id)
model_license
model_version                    # (заменяет отвергнутый teacher_model_version)
request_id
provider_terms_checked
teacher_output_used_for_training
closed_proprietary_model         # вычисляемый из denylist (НЕ доверять входному флагу)
source_ref
source_sha256
contamination_markers
judge_model                      # ВНУТРИ записи (заменяет отвергнутые judge_provider/judge_model_id)
# 5 NEW (T2/T3/T5):
terms_snapshot_ref
geography_export_constraint
source_category
raw_private_text_copied
redaction_policy_ref
# 2 RR6 (service-mode ban / grounding — отдельная ось от brand-denylist):
service_mode                     # plain|grounding|web_search|retrieval_tool|code_exec|unknown; для train/judge обязан быть plain; источник = profile.ServiceMode
grounding_used                   # bool: подмешан ли в выход сторонний веб-контент (grounding/search/retrieval); ИСТОЧНИК = transport-seam ProviderTransportResult.GroundingWasUsed (владелец claude-plan-08 §3.6.4-bis)
```

> **RR6 (2026-06-30).** Поля `service_mode`/`grounding_used` — отдельный запрет от
> brand-denylist: даже allowlist-permissive модель (Qwen3 Apache-2.0) в режиме
> grounding/search (Gemini Grounding / Google Search retrieval / любой web-retrieval
> у хостера) подмешивает в `chosen` сторонний веб-контент чужой лицензии → дистилляция
> нелегальна. `used_for_training && (service_mode≠plain ∨ grounding_used) ⇒ BLOCK`
> (ветка C17, владелец `claude-plan-08 §2.2 шаг 4b / §4`).

Поля уровня DPO-строки (`GraphDpoPair`, владелец **блок 07**, НЕ часть провенанса) —
`row_id`, `task_id`, `domain_tag`, `prompt_hash`, `source_ids`, `chosen_output_hash`,
`rejected_output_hash`, `chosen_authority`, `rejected_origin`, `allowed_use`,
`train_or_heldout`, `source_disjoint_group`, `reviewer`, `license_ref`,
`request_timestamp_utc` — живут на строке/манифесте, а не в `TeacherProvenance`.

## Provider Terms Gate

| gate | pass condition | fail condition |
|---|---|---|
| provider identity | Provider and model id are exact | Provider or model unknown |
| terms snapshot | Terms snapshot ref stored with date | No terms snapshot |
| output-use rights | Terms explicitly allow planned training or judging use | Terms silent or restrictive |
| privacy retention | Raw project text handling is allowed or avoided | Provider may retain sensitive content without approval |
| license compatibility | Open-weight license permits derivative training route | License absent or incompatible |
| service mode (RR6) | `service_mode=="plain"` and `grounding_used==false` for any train/judge call | grounding/web_search/retrieval mode → output carries third-party web content of foreign license (C17 BLOCK) |
| closed-judge terms (RR6) | closed `judge_model` accepted only with `provider_terms_checked==true` and non-PENDING `terms_snapshot_ref` | judge-only does NOT waive terms check (C11-blocked QUARANTINE) |
| Luba approval | Current instruction approves exact route | Old plan or transcript approval only |

## Allowed Planning Defaults

1. Prefer self-hosted open-weight candidates after license verification.
2. Prefer human or project-local labels for initial gold data.
3. Treat closed APIs as blocked for common training until proven otherwise.
4. Treat hosted APIs as conditional even when the model weights are open.
5. Separate `eval_only`, `evidence_only`, `train_personal`, and `train_common` in provenance.
6. Keep raw code and private project text local unless a future privacy gate explicitly allows external use.

## Real teacher/judge call path (shape — owner block 08 §3.6; R5-teacher-call / B-2,B-3)

The actual network call is NOT made directly anywhere in graph logic. It goes through
two egress ports owned by `claude-plan-08 §3.6`:

- `ITeacherClient.RequestGoalAsync(TeacherCallRequest, ct)` — gets `Y_goal` (becomes
  `chosen`). Runs `TeacherProvenanceGateUnit.Evaluate` **before any network byte**; a
  denylist `model_id` (Claude/OpenAI/Gemini) is `LEGAL_BLOCKED` with zero network calls.
- `IJudgeClient.JudgeAsync(JudgeCallRequest, ct)` — closed judge allowed only with
  `teacher_output_used_for_training=false`; anti-authoring guard blocks a judge that
  returns new text (`AUTHORED_NEW_TEXT_BLOCKED`), so a judge can never become a teacher.

Provider selection is data (`ProviderConfig` in `allowed-teacher-registry.json`), not a
brand hardcoded in logic. v1 default teacher profile: `qwen3-together-v1`
(`model_id=Qwen/Qwen3-235B`, `model_license=Apache-2.0`, neutral hosting provider
`Together`, `service_mode="plain"`). **RR6 (2026-06-30):** this profile ships
`provider_terms_checked=false`, `terms_snapshot_ref="PENDING_LUBA_OR_EXTERNAL_TERMS_REVIEW(...)"`,
`terms_check_status="pending_external"` — model license is known (Apache-2.0) but the
Together ToS were not checked this session, so `SelectProvider` will NOT pick it (C6
QUARANTINE) until the terms snapshot is recorded by an external/Luba step. See owner
`claude-plan-08 §3.6.3a`.
Auth is a seam (`IProviderCredentialResolver` reads a secret by name `CredentialKeyRef`;
the key value never appears in plan/code/registry/provenance). **Mock clients are
test-only**: the product DI returns `HttpTeacherClient`/`HttpJudgeClient`
(`product_path_uses_mock==false`).

The single network byte goes through one more seam — the **transport-seam**
`IProviderTransport.SendAsync(config, redactedRequest, resolvedSecret, ct)` →
`OperationResult<ProviderTransportResult>` (owner `claude-plan-08 §3.6.4-bis`). Its result
field `ProviderTransportResult.GroundingWasUsed` is the **single authoritative source** of
the RR6 provenance field `grounding_used`: `HttpTeacherClient`/`HttpJudgeClient` set
`provenance.grounding_used = transportResult.GroundingWasUsed` (never invent the flag, never
copy it from the request). Without this seam the RR6 field would be non-computable and the
service-mode gate (C17, table row "service mode (RR6)" above) would have no fact to stand
on; an answer carrying grounding metadata overrides a profile that merely claims
`service_mode=="plain"`.
