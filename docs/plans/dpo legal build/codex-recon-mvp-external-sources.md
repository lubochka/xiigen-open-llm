# Codex Recon — MVP "external sources learning 2": лицензии / provenance / формат

Узкая под-задача: **обучение на внешних источниках — лицензии, provenance, формат пар**, как evidence для легального гейта teacher-моделей в плане ILearningDynamicGraph. Сбор evidence для плана; не разрешение на реализацию.

corpus=mvp · slug=mvp-external-sources

---

## 1. Что это и где (точные ключевые пути)

Корень области: `C:/Users/lubao/Documents/xiigen/llm mvp/external sources learning 2/`

Прочитанные файлы (3–8, защита от перегруза):

- `external-sources-extracted/EXTERNAL-SOURCE-REGISTRY-v7.md` (v7.1, 2026-05-10) — реестр 14 семейств внешних источников с лицензиями; правила DPO-пар; License States; gap-map.
- `external-sources-extracted/XIIGEN-DPO-REGISTRY-GUIDE-v2-EXTERNAL-SOURCES.md` (v2.0) — рабочий процесс внешних источников: **License Gate (Part 2)**, формат пары, coverage labels, измерение.
- `external-sources-extracted/EXTERNAL-SOURCES-ORCHESTRATION-PIPELINE-PLAN-v7.md` (v7.0, 2026-05-08) — конвейер `source registry -> license and authority check -> ... -> manifest/zip`; станции = контракты `ILogicalUnit`/`ILogicalOrchestrator`.
- `INSTRUCTION-TUNING-SOURCE-ASSESSMENT-v1.md` (2026-05-11) — гейт для instruction/chat-корпусов (OASST, Dolly, Alpaca, ShareGPT, WizardLM, UltraChat, FLAN); **прямо про provenance/model-output risk**.
- `INSTRUCTION-TUNING-ORCHESTRATION-PILOT-RUNBOOK-v1.md` (2026-05-11) — runbook пилота: только OASST1/OASST2 (Apache-2.0), все строки `model_review_required`, ассистент-ответы не используются как chosen.

Рядом (не читались, отметить как доисследование): `documentation/CODE-DOC-EXTERNAL-SOURCE-REGISTRY-v1.md`, `WHAT-TO-DO-WHAT-NOT-TO-DO-v13.md`, `architecture-source-cache/...` (закэшированные docs внешних repo, в т.ч. MIT/Apache), `architecture-source-pilot/session-rows/*.jsonl`, `instruction-tuning-pilot/session-rows/oasst1|oasst2-session-rows-200.jsonl`, `new sets/*` и `new alternative sets/*` (готовые ряды узлов/доменов), `build_instruction_tuning_orchestration_pilot.py` (упомянут в runbook).

---

## 2. Как устроено / работает (кратко)

**Лицензионный гейт (ядро под-задачи).**
- Принимаются для обучающего корпуса: **MIT, Apache 2.0** (для семейств 9–14 фильтр 2026-05-10 строго MIT/Apache-2.0); **CC BY 4.0 — только с атрибуцией** (`attribution_text`, метаданные, не в тело B); **RFC/IETF, MITRE CWE, NIST SP — public domain**, оформляются как `external_license_checked`, `attribution_required:false`.
- Запрещены: **CC BY-SA (share-alike несовместим с закрытым корпусом — явно исключены OWASP, StackOverflow, Dolly), AGPL, GPL, unknown/unlicensed**. Состояние `verify_before_use` не может попасть в обучающие строки.
- License States (v7): `first_party | external_license_checked | attribution_required | source_gap | corruption_case | verify_before_use`.

**Provenance / teacher-output risk.**
- Instruction-tuning assessment явно блокирует источники с **model-output provenance risk**: ShareGPT (BLOCK by default — фрагментарная лицензия + риск, что это вывод модели), WizardLM/Evol-Instruct (BLOCK, часто из ShareGPT/model outputs — только как идея метода), UltraChat (synthetic, REVIEW REQUIRED), Alpaca (CC-BY-NC/research-only — BLOCK), Dolly (CC-BY-SA-3.0 — QUARANTINE), FLAN (METHOD-ONLY до per-task license ledger).
- Ключевое правило provenance: внешний chat-корпус даёт **только A-side форму запроса** (`external_role:"a_side_style_only"`); **chosen-сторона обязана исходить из XIIGen-контрактов/кода/реестров/одобренных стандартов**, ассистент-ответ источника **никогда не копируется как chosen** (и как authority). Это и есть анти-distillation барьер на уровне данных.

**Формат пары (universal).** `Pair[ID] / A (рантайм-вход) / B (chosen) / Reason [WRONG: ...] / Phrasing-B (rejected: Type A|B|C|corruption_case)`. Гейт `overlap_ratio < 0.60`. Обязательны coverage labels на строку: `source_id, source_title, source_license_state, attribution_required, contract_signature, transition_type, phase, sub_phase, task_type, node_role, coverage_status, pair_kind, rejected_type, overlap_ratio`. Запрещён шаблон rejected "Do not generate a capability for sub-phase X".

**Конвейер.** `source registry -> license and authority check -> prompt/evidence extraction -> DPO quality gate -> Stage 2..9 -> manifest/zip`. Каждая станция отображается на контракт-сигнатуру; источник без `supported_contract_signatures` в набор не входит.

---

## 3. Релевантность к ILearningDynamicGraph (DPO / обучение / граф / оркестрация / измерение / teacher-provenance)

- **Teacher / provenance гейт (прямое попадание).** Это и есть готовая легальная основа: разрешённые семейства лицензий, список заблокированных/карантинных источников, и принцип «вывод чужой модели нельзя как chosen/authority». Соответствует Legal Teacher / Provenance Gate из `trainable-architecture-decision-SKILL.md` (Claude/OpenAI/Gemini как distillation teachers запрещены). Здесь distillation-teacher вообще не используется — chosen строится из XIIGen-контрактов и permissive/public-domain стандартов; teacher-вывод проприетарной модели как обучающая цель отсутствует. Для плана: перенести поля provenance скила (`teacher_access_mode, hosting_provider, model_id, model_license, provider_terms_checked, teacher_output_used_for_training, closed_proprietary_model, request_id/model_version`) в реестр строк — сейчас их нет.
- **DPO.** Формат пар (chosen/rejected/[WRONG:]/overlap<0.60/coverage labels) — прямой шаблон для авто-построения DPO под узлы графа.
- **Граф/оркестрация.** Станции конвейера = узлы графа (`DomainSplitterUnit`, `DomainTranslatorUnit`, `SchemePlannerUnit`, `DomainProcessorUnit`, `ApplierUnit`, `ResultTranslatorUnit`, `ArbiterSelectorUnit`, `ArbiterUnit`, `FormatterUnit`) с контрактом A->B (выход станции = вход следующей). Это совпадает с Graph-Style/DynamicGraph Reconstruction Gate (nodes/edges, input/output contracts, fan-out/fan-in).
- **Измерение.** Заданы метрики: routing accuracy ≥0.80, dependency order ≥0.75, held-out accuracy ≥0.65, delta ≥+0.10 над baseline 0.422, regression ≤0.05.

---

## 4. Классификация (уровень vs детерминистический хелпер)

Это **наборы данных и реестры/гайды**, не исполняемый код. Поэтому классифицируется их **назначение для обучения** (правило скила: внешний материал — common vs user-adaptive, license/source gate; и «всё остальное — internal detail, если не обёрнуто разрешённым уровнем»).

- **Целевые контракты обучения = разрешённые уровни.** Реестр маппит источники на `ILogicalUnit` (узлы 01–09, compiler/router/teaching units), `ILogicalOrchestrator` (`LogicalTransitionOrchestrator`, `TeachingOrchestrator`, `AdaptiveLogicalOrchestrator`) и `ILogicalFlow` (`OrchestrationRoutingFlow`, `ILogicalFlowFactory<TInput>`). Это согласуется с CLAUDE.md / `trainable-architecture-decision-SKILL.md`: лицензированные пары — обучающий вход для разрешённых уровней.
- **`PromptRoutingInputCompilerUnit` / `MultiSubPhaseRouterUnit` сами по себе — не полная оркестрация.** Assessment (правка 2026-05-11) явно: батч только из этих юнитов = `PAIRSET_READY_ROUTING_FLOW_PENDING`; routing-обучение должно целиться в полный `OrchestrationRoutingFlow : ILogicalFlow` / `AdaptiveLogicalOrchestrator`. Это ровно правило скила «standalone retrieval/router нельзя выдавать за всю trainable-архитектуру».
- **Девятипредикатный лексикон (nine-predicate lexicon).** Назван как `internal_contract_reference`, отделён от внешних источников. Классификация: **внутренний детерминистический хелпер/контракт** (фичи/лейблы предикатов), не отдельный trainable-уровень; легитимен как вход разрешённого юнита. Доисследовать: где он реализован в коде и не становится ли он финальной authority вместо обученного юнита (deterministic-boundary check).
- **Семейство — leg обучения.** First-party (Family 1–2, XIIGen) — user-adaptive/проектное; внешние MIT/Apache/public-domain (Family 3–14, RFC/CWE/NIST) — кандидаты для common leg. Источники с непроверенной лицензией/provenance (ShareGPT/WizardLM/UltraChat/Alpaca/Dolly/FLAN-raw) — **evidence-only / quarantine**, не common-training, что прямо соответствует External learning material checklist скила.

Вердикт по под-задаче (по `trainable-architecture-decision-SKILL.md`): **OK для легального/provenance гейта в части evidence** — лицензионная и provenance-дисциплина согласована со скилами; teacher-distillation проприетарных моделей отсутствует. Открытый риск к плану: provenance-поля teacher-гейта не присутствуют как обязательные поля строки реестра (сейчас гейт держится на license_state + `a_side_style_only`).

---

## 5. Пробелы / что доисследовать

1. **Provenance-поля teacher-гейта не формализованы в реестре строк.** Нет `teacher_access_mode / model_license / teacher_output_used_for_training / closed_proprietary_model`. Для плана: добавить их в coverage labels или в Stage 0 регистрацию.
2. **Фактическая чистота .jsonl рядов не проверялась.** `architecture-source-pilot/session-rows/*.jsonl`, `instruction-tuning-pilot/session-rows/oasst1|oasst2-session-rows-200.jsonl`, `new sets/*`, `new alternative sets/*` — нужно подтвердить: `source_license_state` на каждой строке, отсутствие скопированных ассистент-ответов как chosen, `overlap_ratio<0.60`, отсутствие mojibake.
3. **`architecture-source-cache/`** — закэшированные docs внешних repo: подтвердить наличие LICENSE/ссылки на каждый источник (правило скила «license file/ref to verify»), а не только декларацию MIT/Apache в реестре. Заметка: в кэше есть `docs/ai/tools/claude-code.md`, `ollama.md` и пр. — это **контентные docs о моделях**, не teacher-выводы; убедиться, что они используются как авторитет/материал, а не как distillation-цели.
4. **`CODE-DOC-EXTERNAL-SOURCE-REGISTRY-v1.md` и `WHAT-TO-DO-WHAT-NOT-TO-DO-v13.md` не читались** — могут содержать дополнительные/иные правила лицензий и provenance; сверить на конфликт с v7/ v2.
5. **CC BY 4.0 атрибуция в проде.** Проверить, что для CC BY 4.0 источников (например Google AIP-180) `attribution_text` реально присутствует в метаданных принятых строк, а не только описан в гайде.
6. **Привязка реестра к коду llm_mvp_core.** Не проверялось, существуют ли реально классы станций (`SchemePlannerUnit` и т.д.) и `OrchestrationRoutingFlow` в репозитории и где их манифест/локатор — нужно для маппинга «источник -> контракт -> обученный узел графа».
