# Pass P45 — DPO data provenance + legal/teacher gate (extension learning path)

Честный фрейм прохода. Проверял НЕ количество/авторскую природу DPO-строк (это P24), НЕ
исполненные числа обучения (P25), НЕ аудит trainable-архитектуры (P38), а **юридико-провенансный
слой обучающих данных, которые кормят три расширения**: есть ли лицензионно-исходный гейт для
common-обучения, есть ли teacher/distillation-гейт (allowlist семей → каноническая лицензия,
terms_snapshot, разведение vendor-hosting vs neutral-reseller), и совпадает ли фактически
промоутнутый common-корпус с собственными правилами репозитория о происхождении данных.

Метод (bounded): грепы кода `43.SmartSkillsDomain` + `11.DPO` по `licen|terms_snapshot|distill|
teacher|allowlist|canonical_license|redact|provenance`; грепы всего репо по
`canonical_license|terms_snapshot|teacher_allowlist|distillation`; чтение карточек/манифестов трёх
промоутнутых common-корпусов (seed-v1, orchestration-reasoning-common, skill-selection-v1);
чтение эталонной схемы `design-conversion/real-s3-v1/source-manifest.schema.json` (контраст);
чтение исполненного `source-license-redaction-boundary-review.v1.json` (LEARN-02),
`provenance-privacy-source-route.json` (phase-07) и кода `ResultGapDpoWriters.cs`; грепы license-полей
по всему `training-data`.

## Что я НЕ переоцениваю в минус (честные положительные факты)

1. **Лицензионно-исходный гейт в проекте СУЩЕСТВУЕТ в двух формах.** (а) Исполненный ревью-артефакт
   `artifacts/extension-runtime-token-quality-learnability/source-license-redaction-boundary-review.v1.json`
   (LEARN-02): классифицировал 12 кандидатов, `common_source_rule` дословно требует
   «MIT/Apache-2.0 source verification or explicit Luba approval, plus redaction and source-disjoint
   split», вердикт `PASS_BOUNDARY_REVIEW_COMMON_TRAINING_BLOCKED`. (б) Код-структура маршрутизации
   `ResultGapDpoWriters.cs:7-26` — `ResultGapSourceDescriptor(LicenseExpression, PrivacyScope,
   UserApprovalRecorded)` и `ResultGapSourceRoute(CommonTrainingAllowed, UserAdaptiveAllowed,
   Quarantined, SourceIdUsedAsTrainingFeature)`, с исполненной таблицей маршрутов
   `provenance-privacy-source-route.json` (approved_common / adaptive_only / quarantine × 2).
   То есть проект УМЕЕТ выражать лицензию/маршрут источника — это не «ничего нет».
2. **Redaction/privacy-самофлаги реально проставлены** во всех трёх корпусах
   (`raw_private_text_copied:false`, `technical_execution_refs_excluded:true`).
3. **Домен DesignConversion имеет полноценную схему источника** с блоками `license`+`provenance`+
   `privacy` — проект знает, как выглядит правильный лицензионный манифест.

Но все три положительных факта НЕ покрывают фактически промоутнутые/обученные common-корпуса трёх
расширений. Ниже — где именно рвётся.

## Центральные факты прохода

- **В коде обучающего пути расширений нет ни teacher-allowlist, ни canonical_license, ни
  terms_snapshot, ни provider-disambiguation.** Грепы `canonical_license|terms_snapshot|
  teacher_allowlist|distillation` по всему `C:\Projects\llm_mvp_core` дают попадания ТОЛЬКО в
  тексте правил (`CLAUDE.md`,`AGENTS.md`), в устаревших экспериментальных worktree
  (`.claude/worktrees/*`,`experiments-v2/*` — teaching-эксперименты, не extension-путь) и в одном
  skill-файле. В `43.SmartSkillsDomain` и `11.DPO` — 0 (совпадения там только `RedactPath`/
  `raw_prompt_redacted` и `source_base_commit`). Вся distillation-teacher-машинерия CLAUDE.md
  (C4 identity-match, C5 permissive-license, `family -> canonical_license` инвариант,
  Moonshot/Kimi vs DeepSeek vs Z.ai provider-disambiguation, terms_snapshot) для extension-пути
  НЕ реализована.

- **Промоутнутый orchestration-reasoning COMMON-корпус построен из вывода МОДЕЛИ Sonnet (Claude) —
  это дистилляция — и это прямо противоречит собственной маршрут-таблице проекта.**
  `orchestration-reasoning-common-corpus-manifest.json:31-46`: три `source_refs` =
  `sonnet-local-analyzes:.clinerules-analysis.md` / `.gitignore-analysis.md` /
  `.gitattributes-analysis.md`, все `model_leg:"common"`. При этом
  `provenance-privacy-source-route.json:33-56` объявляет `external_vendor_output` и `unknown`
  источники → `trainingRoute:"quarantine"`, `commonTrainingAllowed:false`. Вывод модели Sonnet —
  это external vendor output; по правилам репо он должен быть в карантине, а не в common-корпусе.

- **Все лицензионные поля в `training-data` живут ТОЛЬКО в `design-conversion/real-s3-v1/*`, и даже
  там они КРАСНЫЕ.** Греп `license_or_authority_ref|allows_training|terms_snapshot` по всему
  `training-data` → все хиты только в DesignConversion; значения:
  `codex-source-manifest.authority-review.json` `allows_training:false` (`:95/:161/:217/:278/:331`),
  `license_or_authority_ref:"not_proven"` (`:176/:237/:293/:329`), есть
  `codex-source-owner-license-blocker.json`. Три extension-корпуса не имеют ни одного лицензионного
  поля вообще.

- **Исполненный boundary-review (LEARN-02) не покрывает промоутнутые корпуса.** Греп
  `seed-v1|orchestration-reasoning-common|skill-selection-v1|corpus-snapshot` по
  `source-license-redaction-boundary-review.v1.md` → No matches. Ревью покрыло только 12 строк
  `local-dpo-candidates-v1.jsonl` (session-derived), из которых 0 допущены в common
  (`USER_LOCAL_ONLY:7`, `USER_LOCAL_ONLY_PRIVATE_BEHAVIOR:2`, `USER_LOCAL_ONLY_PRIVATE_CANDIDATE:1`,
  `QUARANTINE_FOR_COMMON:2`).

## Новые дыры P45 (дедуп против P24/P25/P26/P38/P43)

### G1 — Промоутнутые common-корпуса трёх расширений не имеют лицензионно-исходного манифеста
- Real-status: **MISSING GATE / provenance-legal-hole на промоутнутых common-данных**
- Evidence:
  - `training-data/smart-skills/semantic-quality/seed-v1/smart-skills-dpo-seed-dataset-card.json:23`
    `common_or_adaptive:"common"`; `:51-107` `source_refs` = только внутренние
    `artifacts/dpo-quality/*` (нет license/terms_snapshot/source_uri/reviewer_ref).
  - `training-data/skill-selection/skill-selection-v1/training-manifest.json:88-119`
    `row_schema_fields` содержит `external_ai_used`/`raw_private_text_copied`, но НЕ license/terms/
    source_owner; `:134` `common_leg_row_count:88`.
  - `training-data/orchestration-reasoning/orchestration-reasoning-common-corpus-manifest.json:31-47`
    `source_refs`/`model_leg:common` без license.
  - Контраст `training-data/design-conversion/real-s3-v1/source-manifest.schema.json:195-224`
    требует блок `license{license_or_authority_ref,rights_scope,allows_training,allows_evaluation,
    contains_user_private_data}` + `:152-193` `provenance` + `:226-259` `privacy`.
- Gap: обучающий путь, кормящий три расширения, промоутит строки, объявленные `common`, БЕЗ
  лицензионно-исходного гейта, которого CLAUDE.md требует для common-training-материала; единственный
  домен с такой схемой — DesignConversion. Отдельно от P24 (авторская природа/счёт/training_allowed) —
  здесь именно отсутствие ЮРИДИЧЕСКОГО license-манифеста на общих данных.
- Closure: добавить обязательный source-license+provenance манифест на каждый common-корпус
  (`license_or_authority_ref`,`rights_scope`,`allows_training`,`terms_snapshot`,`reviewer_ref`,
  `source_uri_or_local_ref`); портировать схему DesignConversion на extension-корпуса; блокировать
  промоушен корпуса без прохождения license-гейта.

### G2 — Distillation-teacher-гейт отсутствует; sonnet(vendor-model)-derived строки в COMMON-корпусе нарушают собственное правило карантина
- Real-status: **LEGAL VIOLATION IN PROMOTED CORPUS / teacher-gate missing**
- Evidence: `orchestration-reasoning-common-corpus-manifest.json:33/38/43`
  `source_ref:"sonnet-local-analyzes:*"`, `model_leg:"common"` (3 строки в common);
  vs `artifacts/extension-runtime-token-quality-learnability/phase-07-resultgap-correction-dpo/
  20260623-085644/provenance-privacy-source-route.json:33-56` `external_vendor_output`+`unknown`
  → `trainingRoute:"quarantine"`,`commonTrainingAllowed:false`; грепы
  `canonical_license|terms_snapshot|teacher_allowlist|distillation` в `43.SmartSkillsDomain`/`11.DPO`
  → 0.
- Gap: вывод модели-вендора (Sonnet) используется как COMMON-обучение без teacher-license/terms-гейта,
  прямо против собственного правила проекта «external_vendor_output → quarantine»; вся C4/C5/
  `family->canonical_license`/`terms_snapshot`/provider-disambiguation-машинерия CLAUDE.md не
  реализована для extension-пути. Ново (ни P24/P25/P38 не касались teacher/distillation-происхождения).
- Closure: реализовать teacher-allowlist с `family->canonical_license`+`terms_snapshot`+provider-
  disambiguation; переклассифицировать sonnet-derived строки как `external_vendor_output` →
  quarantine ЛИБО получить записанные terms/approval; убрать/перемаршрутизировать их из common-корпуса.

### G3 — Существующий license-гейт (LEARN-02 boundary review + ResultGapSourceRoute код) покрывает только 12 session-derived кандидатов и не применён к промоутнутым/обученным корпусам
- Real-status: **GATE EXISTS BUT UNAPPLIED TO SHIPPED DATA / coverage hole**
- Evidence: `source-license-redaction-boundary-review.v1.json:4` verdict
  `PASS_BOUNDARY_REVIEW_COMMON_TRAINING_BLOCKED`, `:5` `source_file=...local-dpo-candidates-v1.jsonl`,
  `:6` `row_count:12`, `:8` `common_training_allowed:false`, `:28-33` summary (0 допущено в common),
  `:34-41` `validation_requirements_for_future_common_use`; греп promoted-corpus имён по
  `...boundary-review.v1.md` → No matches; код-структура `ResultGapDpoWriters.cs:7-26` живёт только
  на ResultGap/Correction runtime-feedback пути (по P24 ≈0 принятых строк), а seed/orchestration/
  skill-selection корпуса — статичный committed JSONL, грузимый жёстким путём, мимо ResultGapSourceRoute.
- Gap: единственный реальный юридический гейт дал 0 допущенных в common строк и покрывает лишь
  session-derived кандидатов; корпуса, которые РЕАЛЬНО промоутнуты и обучены, обошли его — гейт
  декоративен относительно поставляемых данных. Отдельно от P26 (promotion-gate/checkpoint) —
  здесь именно SOURCE-INTAKE license-гейт и его непокрытие промо-данных.
- Closure: прогнать boundary review + source-route гейт по seed-v1/orchestration-reasoning-common/
  skill-selection корпусам; записать license/approval на корпус; блокировать промоушен любого корпуса
  без PASS boundary review; зафиксировать, что текущие промо-common-данные не имеют проходного ревью.

### G4 — Локально-проектные файлы (.clinerules/.gitignore/.gitattributes analyses) отправлены в COMMON-ногу без записанного «redacted AND approved for common»
- Real-status: **LEG-ASSIGNMENT PROVENANCE HOLE**
- Evidence: `orchestration-reasoning-common-corpus-manifest.json:17-21` `included_common_row_ids`
  (`or01u-sonnet-clinerules-001`/`-gitignore-001`/`-gitattributes-001`), `:33-46` `model_leg:"common"`;
  только `.cursorrules` отложен в adaptive (`:22-30` `excluded_adaptive`, `deferred_reason`);
  CLAUDE.md: «Private user prompts, chats, and local project files train the adaptive user leg unless
  they are explicitly redacted and approved for common training»; артефакта common-approval на эти 3
  строки нет.
- Gap: анализы локальных конфиг-файлов проекта попали в COMMON-ногу лишь с самофлагом
  `raw_private_text_copied:false` и без записанного common-approval; CLAUDE.md по умолчанию
  относит такой материал к adaptive-ноге без явного одобрения — риск попадания приватного/локального
  материала в общий (shared) common-модель. Отдельно от G2 (там vendor-model-происхождение анализа) —
  здесь происхождение АНАЛИЗИРУЕМОГО субъекта (локальные файлы) и неверная нога.
- Closure: на каждую строку до назначения в common-ногу записывать явный redaction+common-approval,
  либо перенести .clinerules/.gitignore/.gitattributes analyses в adaptive-ногу; добавить
  leg-assignment гейт, по умолчанию отправляющий local-file-derived строки в adaptive.

### G5 — Единственный домен с license/provenance-аппаратом (DesignConversion) сам КРАСНЫЙ и не разделяется с расширениями
- Real-status: **REFERENCE GATE RED + domain-local (not shared)**
- Evidence: `training-data/design-conversion/real-s3-v1/codex-source-manifest.authority-review.json:95/
  :161/:217/:278` `allows_training:false`, `:176/:237/:293/:329` `license_or_authority_ref:"not_proven"`,
  `:109` `blocking_reason` (dimensions not_collected, captured_utc/license not_proven);
  `codex-source-owner-license-blocker.json` присутствует; `source-manifest.schema.json:8-11`
  `placeholder_not_real:true`/`evidence_status:"schema_only_not_real_evidence"`; греп license-полей по
  `training-data` вне design-conversion → 0.
- Gap: эталонный юридический аппарат репозитория (а) сам не прошёл лицензию (все `not_proven`,
  `allows_training:false`, owner-license-blocker), значит даже design-to-code training-данные
  юридически НЕ очищены; (б) он domain-local — не подключён и не переиспользован тремя расширениями.
  Отдельно от P26/P38 (там про PROMOTION-gate DesignConversion как test-only) — здесь про
  SOURCE-LICENSE/owner-approval состояние данных (красное) и его неразделяемость.
- Closure: закрыть DesignConversion source-license гейт (записать реальный
  `license_or_authority_ref`/owner-approval, снять `not_proven`/`allows_training:false`) или явно
  пометить домен как non-training до одобрения; вынести license/provenance-схему в общий контракт и
  подключить к трём расширениям; тест паритета «каждый промоутнутый корпус имеет проходной
  source-license манифест».

## Итог P45

Провенанс/юридический слой обучающего пути расширений — самый слабый из проверенных: teacher/
distillation-гейт в коде отсутствует полностью; фактически промоутнутый common-корпус содержит
vendor-model(Sonnet)-derived строки против собственного правила карантина; лицензионные манифесты
есть лишь у DesignConversion и там они КРАСНЫЕ; единственный исполненный license-ревью покрыл 12
session-строк (0 допущено в common) и не касался промо-корпусов. 5 новых дыр (G1–G5) занесены в мастер.
