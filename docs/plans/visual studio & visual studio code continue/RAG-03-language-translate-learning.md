# RAG-03 — Папка «language translate learning» (перевод промпта: иврит/русский → английский)

> Часть подготовки плана ПРОДОЛЖЕНИЯ расширений Visual Studio и VS Code
> (офлайн локальная модель: документация кода + design-to-code). Эта папка —
> ТРЕТЬЯ из 12 (#3/12). Роль: **capability перевода** — перевести промпт
> пользователя с другого языка (например иврита) на английский ПЕРЕД
> оркестрацией.

- **Имя папки:** `language translate learning`
- **Физический путь:** `C:/Users/lubao/Documents/xiigen/llm mvp/language translate learning`
- **Роль (по словам Luba):** перевод промпта с других языков (например иврита)
  на английский. Релевантность **HIGH** — это активный P0 пилот по RAG-01.
- **Всего файлов:** **1154** (реально посчитано `find . -type f`), **233** папки.
  ⚠️ Оценка задания была ~409 — реальность **в ~2.8 раза больше**, потому что
  дата-папка `translation-runs/2026-05-11/` содержит 824 файла настоящего DPO-прогона
  (чекпоинты, токенайзеры, policy-веса) — это не было учтено в оценке ~409.
- **Статус прохода:** это **проход 1/10** (MAP + MANIFEST + верхнеуровневые гайды).
  Проходы 2–9 идут вглубь в `translation-runs/` и тела гайдов; проход 10 —
  свод полноты + executive summary.

> **[BRANCH-STATUS OVERLAY 2026-07-28]** (оверлей C3-R03, Цикл 3 документного обновления; добавка — ничего ниже не удалено и не переписано). Свежестная отметка: ветка переводов `language_translate` статична с 2026-05-12; её вершина `f6032aff8` переподтверждена 2026-07-28 командой `git ls-remote` (запрос серверу о вершине ветки, ничего не скачивая). Содержание этого индекса по состоянию ветки остаётся точным. Главная ветка `main` с тех пор двинулась (24 июля впитала линии design-to-code и DPO-обучения, PR #38/#39), но из `language_translate` ничего сверх её майского состояния не впитывала — все сравнения «ветка против main» в этом файле читать как исторический срез. Авторитетная таблица веток — `BRANCH-STATUS-CURRENT-2026-07-28.md` в этой же папке.

## Кросс-линк с 27.TranslationDomain (пилот из RAG-01)

RAG-01 (сравнение веток) установил: `language_translate` — **единственный
активный P0 пилот**; активный домен — `27.TranslationDomain` (полный
`ILearningMachine<TranslationDomain>` внутри `llm_mvp_core`). Его runner-report
показывает accuracy **0.3333** (унаследована байт-в-байт из `main`, обучена на 1
строке), `reward_margin=0.03` **захардкожена** (дефект R-19), а **seed-корпус
ОТСУТСТВУЕТ на HEAD**.

**Эта папка — то место, где живёт фактический учебный материал этого пилота:**
seed-корпус, DPO-пары, реестры источников/лицензий, и — что важнее всего —
**настоящий DPO-прогон** (`translation-runs/2026-05-11/`), где TensorCore
DPO-тренер реально запускался, сохранял чекпоинты и считал reward margin.

**Важная находка прохода 1 (связь с историей 0.3333 / R-19):** в дата-прогоне
`2026-05-11` реальные метрики тренера НЕ захардкожены и НЕ равны 0.3333/0.03:
- `batch-001` (50 пар): `AverageAccuracy=0.58`, `AverageRewardMargin=+0.00586`,
  вердикт **REPAIR_BATCH** (accuracy 0.580 ниже гейта 0.60), `accepted_batches=0`;
  источник помечен `SYNTHETIC_SEED` (не настоящий Tatoeba).
- `batch-005/dpo-run-200` (кумулятивно 200 пар): `AverageAccuracy=0.29`,
  `AverageRewardMargin=-0.0152` (**отрицательная** — регресс при росте объёма).
- Модель крошечная: `ContextLen=128`, `VocabSize=2048`.

То есть в этой папке лежит **реальная механика обучения** (реальные accuracy
0.58/0.29, реальные reward margins около нуля/отрицательные), а `0.3333`/`0.03`
в runner-report домена — это отдельное (позднее, 1-строчное, унаследованное из
main) число. Проходы 2–9 должны сверить, какой именно seed из этой папки должен
попасть в `27.TranslationDomain` и почему он «отсутствует на HEAD».

---

## Полный манифест / структура (все 1154 файла учтены)

### A. Верхнеуровневые файлы (23 шт.) — гайды, планы, реестры, скрипт, seed

Порядок: гайды/планы (.md) → скрипт (.py) → seed-наборы (.jsonl) → архив (.zip).

| # | Файл | Строк | Что это | Рел. |
|---|---|---:|---|---|
| 1 | `TRANSLATION-TRAINING-PLAN-v1.md` | 377 | Мастер-план обучения «по одному батчу»; фазы 0–6, Gate B, LoRA/токенайзер/PromptBuilder | HIGH |
| 2 | `TRANSLATION-FOUR-LEVEL-LLM-TRAINING-PLAN-v1.md` | 376 | Активный операционный план: 4 уровня × 4 направления, стрим=направление×тема×уровень, батч=50+50 | HIGH |
| 3 | `TRANSLATION-TOPIC-ROUTER-ORCHESTRATION-PLAN-v1.md` | 489 | Обучаемый роутер `TranslationRoutingFlow : ILogicalFlow`; R1–R4, темы D01–D06, адаптеры | HIGH |
| 4 | `TRANSLATION-ROUTING-FLOW-DPO-GUIDANCE-v1.md` | 209 | Форма DPO-пар для роутера; R1–R4, flow-vs-unit пары, гейты промоушна | HIGH |
| 5 | `TRANSLATION-SOURCE-REGISTRY-v3.md` | 189 | **Каноничный** реестр источников/лицензий (TR-S01..S04, quarantine, block) | HIGH |
| 6 | `TRANSLATION-SOURCE-REGISTRY-v2.md` | 322 | Историч. реестр (HF-ID, HeDC4/Knesset/ParaNames, CCMatrix); **superseded v3** | MEDIUM |
| 7 | `TRANSLATION-SOURCE-REGISTRY-v1.md` | 199 | Историч. реестр (OpenSubtitles ещё ACCEPT); **superseded v3** | LOW |
| 8 | `TRANSLATION-DPO-REGISTRY-GUIDE-v1.md` | 355 | Семейства пар T01–T09, форма пары, хард-гейты, overlap-гейт | HIGH |
| 9 | `DPO-PAIR-AUTHORING-GUIDANCE-TRANSLATION-v1.md` | 270 | Как писать пары: reward margin, 4 типа ошибок A–D, скрипт-специфика RU/HE, [WRONG:] | HIGH |
| 10 | `EXAMINATION-METHOD-TRANSLATION-v1.md` | 300 | Метод экзамена источника: Layer 0-A..5, дерево семейств, длины токенов, FM-TR-01..07 | HIGH |
| 11 | `TRANSLATION-DPO-REJECTED-PROMPTS-v1.md` | 309 | Библиотека промптов для rejected-стороны (создать/аудит/починить), примеры good/bad | HIGH |
| 12 | `TRANSLATION-TOKENIZER-GUIDE-v1.md` | 214 | BPE-токенайзер 3 скриптов (кир/иврит/латиница), покрытие ≥95/90/97%, extension | HIGH |
| 13 | `TRANSLATION-50-PAIR-ABLATION-RUNBOOK-v1.md` | 344 | Механика одного 50-парного батча: inventory→split→pairs→train→eval→decision | HIGH |
| 14 | `EXECUTOR-PROMPT-TRANSLATION-50-PAIR-LEARNING-v1.md` | 856 | **Текущий операционный промпт-исполнитель** (самый большой файл) | HIGH |
| 15 | `TRANSLATION-RUNBOOK-v1.md` | 371 | Историч. хендофф-руководство + шаблоны validate/evaluate python-скриптов | MEDIUM |
| 16 | `TRANSLATION-EXECUTOR-HARD-RESET-v1.md` | 185 | Жёсткий role-lock исполнителя: только learning-set, «missing runner ≠ stop», запрещённые claims | HIGH |
| 17 | `TRANSLATION-LOOK-LIKE-RUN-REPORT-TEMPLATE-v1.md` | 201 | Шаблон человекочитаемого `LOOK-LIKE-RUN-REPORT.md` на каждый батч | MEDIUM |
| 18 | `TRANSLATION-SMALL-MODEL-RATIONALE-v1.md` | 203 | Почему маленькой модели достаточно (роутинг+адаптеры+50-парные батчи) | MEDIUM |
| 19 | `prepare_translation_learning_sets.py` | 329 | **Реальный** гейт-скрипт: маршрутизирует пары в train-ready/repair/blocked; зеркалит C# `TranslationPairValidator` | HIGH |
| 20 | `training-pairs-train-ready.jsonl` | 50 | **Seed-корпус: 50 пар** (id `ru-en-general-l1-b029-*`, RU_EN, train_ready) | HIGH |
| 21 | `training-pairs-repair-required.jsonl` | 0 | Пусто (нет пар на починку на верхнем уровне) | LOW |
| 22 | `training-pairs-blocked.jsonl` | 0 | Пусто (нет заблокированных на верхнем уровне) | LOW |
| 23 | `translation-runs.zip` | ~68 МБ | Zip-бэкап дерева `translation-runs/` (273070 строк) | LOW (артефакт) |

### B. `translation-runs/` — дерево прогонов (1131 файл, все учтены по паттерну)

`translation-runs/DOCUMENT-USE-LOG.md` (1 файл) — лог чтения 11 документов перед
первым прогоном (`router-r1-direction/batch-001`). Внутри — 5 стримов:

| Стрим (папка) | Батчей | Файлов | Что это | Режим |
|---|---:|---:|---|---|
| `router-r1-direction/` | 3 | 32 | Роутер R1: определение направления (RU_EN/EN_RU/HE_EN/EN_HE) | pairset-only |
| `router-r2-topic/` | 3 | 33 | Роутер R2: определение темы (general vs named_entity …) | pairset-only |
| `translator-en-ru-general/` | 18 | 116 | Переводчик EN→RU general, 18 батчей | pairset-only |
| `translator-ru-en-general/` | 20 | 125 | Переводчик RU→EN general, 20 батчей (=1000 пар, `stream_complete:true`) | pairset-only |
| `2026-05-11/` (дата-прогон) | 29 | **824** | **Настоящий DPO-прогон** RU→EN L1 (чекпоинты, токенайзеры, policy-веса) | **REAL DPO** |

Сумма файлов: 23 (верх) + 1 (use-log) + 32 + 33 + 116 + 125 + 824 = **1154** ✓

#### B.1 Схема на батч в «новых» стримах (router-r1/r2, translator-en-ru/ru-en)
Повторяющиеся имена файлов в каждом `batch-NNN/` (pairset-режим, DPO НЕ запускался):
- `translation-pairs-train-50.jsonl` / `translation-router-train-50.jsonl` — 50 обучающих пар;
- `translation-router-heldout-50.jsonl` — 50 held-out (у роутеров);
- `translation-pairs-validation.json` / `translation-batch-summary.json` — итог валидации/структурная точность (`structural_accuracy:1.0`, `gate_result:ACCEPTED`);
- `source-inventory.jsonl` — кандидаты-строки до фильтра;
- `translation-error-ledger.jsonl`, `translation-sample-review.md` — леджер ошибок и ручной обзор (у роутеров);
- `training-pairs-repair-required.jsonl` — забракованные гейтами;
- `STREAM-STATE.json` — кумулятивное состояние стрима (`batches_complete`, `cumulative_train_pairs`, `next_action`);
- `NEXT-BATCH-DECISION.md`, `LOOK-LIKE-RUN-REPORT.md` — решение о следующем батче + человекочитаемый отчёт.

#### B.2 Схема на батч в дата-прогоне `2026-05-11/ru-en-general/level-1/batch-001..029` (REAL DPO)
Каждый `batch-NNN/` содержит рамочные файлы (`STREAM-STATE.json`,
`LOOK-LIKE-RUN-REPORT.md`, `NEXT-BATCH-DECISION.md`, `DOCUMENT-USE-LOG.md`,
`training-pairs-{train-ready,repair-required,blocked}.jsonl`,
`translation-train-50.jsonl`, `translation-heldout-50.jsonl`,
`translation-batch-summary.json`) **плюс** подпапку `dpo-run/<run-id>/` с
настоящими артефактами тренера:
- `bpe-tokenizer.json` — обученный BPE-токенайзер прогона;
- `dpo-pairs-normalized.jsonl` — нормализованные DPO-пары, поданные в тренер;
- `checkpoints/dpo_step_000001..000005.json` — чекпоинты по шагам;
- `checkpoints/policy_step_000005.bin` — **реальные веса policy-адаптера** (бинарь);
- `progress.jsonl` — событийный лог (per-batch loss/accuracy/reward margin);
- `batch-summary.jsonl`, `internal-train-ready-dpo-summary.{json,md}` — сводка прогона (RunId, Sha256, PairCount, AverageLoss/Accuracy/RewardMargin, TotalElapsedMs).
- Некоторые батчи имеют варианты прогонов: `batch-005` → `dpo-run` + `dpo-run-150` + `dpo-run-200` (кумулятив 150/200 пар); `batch-006` → `dpo-run` + `dpo-run-3step` + `dpo-run-repeat` (эксперименты с batch_size/повтором). Всего ~59 dpo-run-директорий (по счётчику `progress.jsonl=59`, `policy_step_000005.bin=51`).

Все ~824 файла дата-прогона учтены этой схемой (рамка батча + dpo-run/checkpoints × 29 батчей × варианты прогонов).

---

## Подробный разбор верхнеуровневых гайдов/планов/реестров

### 1. `TRANSLATION-FOUR-LEVEL-LLM-TRAINING-PLAN-v1.md` (АКТИВНЫЙ операционный план) — HIGH
- **Что это:** текущий план обучения 4 параллельных адаптеров: `RU_EN`, `EN_RU`, `HE_EN`, `EN_HE`.
- **Ключевое:**
  - Стрим = `направление × тема × уровень`; батч = **50 новых train + 50 held-out**. 50 — это размер батча, НЕ размер уровня.
  - **4 уровня:** L1 direction+core sentence; L2 meaning/syntax/completeness; L3 morphology/register/named-entity; L4 robustness/round-trip/real-use. У каждого — свой pair-mix на 50 и promotion-gate.
  - Минимум первого прохода: `4 напр × 1 тема × 4 уровня × 50 = 800 пар` (не обучать все 800 сразу).
  - Роутинг обязан быть логическим контрактом: `TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>` + `TranslationRoutingOrchestrator : ILogicalOrchestrator` — **не хардкод-классификатор** (совпадает с R4/allowed-levels в CLAUDE.md).
  - Гейт роутинга перед обучением переводчика: direction ≥99%, topic ≥95%, adapter ≥95%, false-low-risk=0.
  - Форма пары (JSONL): `id/domain/direction/topic_domain/register/risk_class/level/family/adapter_id/source_id/source_license_state/prompt/a_side/chosen/rejected/compiled_prompt_in_training:false/heldout_group`.
  - Failure rules: стоп при отрицательном margin, wrong-direction>2%, copy-source>2%, порче иврит-скрипта, отсутствии `[WRONG:]`, chosen==rejected. Проваленные батчи не удалять — в `translation-runs/rejected/`.
- **Теги:** `#активный-план #4-уровня #4-направления #роутинг-flow #50-парный-батч`
- **Кросс:** #3 (роутер-план), #13 (ablation runbook), #14 (executor prompt).

### 2. `TRANSLATION-TRAINING-PLAN-v1.md` (v1.1, историч. дизайн-контекст) — HIGH
- **Что это:** более ранний фазовый план (Фазы 0–6), сохранён как исторический дизайн; текущая семантика — стрим-based (см. поправку 2026-05-11 внутри).
- **Ключевое:**
  - **Инфраструктура:** мультиязычный BPE (кир/иврит/латиница), покрытие RU≥95/HE≥90/EN≥97; LoRA для перевода: **Rank=4, Alpha=8, Dropout=0.05**, target `q_proj,v_proj` (отдельные веса, НЕ делить с оркестрацией); `BuildTranslationLoraConfig()`.
  - **`TranslationPromptBuilder`** (`18.CapabilityTraining/Translation/` или новый `18.TranslationTraining`): 4 системных промпта по направлениям; `Build(direction, source)` → `OperationResult<string>`; `compiled_prompt_in_training:false` включает этот путь вместо `SemanticTeachingPromptCompiler`.
  - **Метрики:** BLEU-4, chrF, DPO reward margin, DPO accuracy; регресс-гейт BLEU≤2/chrF≤3, margin>0.
  - **Фазы:** 0 pre-conditions (PC-TR-01..07: покрытие токенайзера, baseline BLEU FLORES-200, OpusFilter, PromptBuilder unit-tested); 1 RU↔EN core; 2 HE↔EN core; 3 register/idiom/NE combo; 4 back-translation (PC-T07-01: BLEU≥25 до T07); 5 combo-promotion (112 пар); 6 large-volume/QUARANTINE (CCMatrix/opus-books/wikimatrix).
  - **Изоляция:** пары перевода НИКОГДА не мешать с оркестрационным baseline (RUN-416/V246); отдельный адаптер, файлы, леджер.
- **Теги:** `#историч-дизайн #LoRA #PromptBuilder #фазы-0-6 #изоляция-от-оркестрации`
- **Кросс:** #5 (registry v3), #2 (активный план), #12 (токенайзер).

### 3. `TRANSLATION-TOPIC-ROUTER-ORCHESTRATION-PLAN-v1.md` — HIGH
- **Что это:** архитектура обучаемого роутера перевода (не helper/regex/enum-switch/lookup).
- **Ключевое:**
  - Контракты (record): `TranslationRoutingRequest(SourceText, SourceLanguageHint?, TargetLanguage, UserDomainHint?, UserRegisterHint?, Metadata)`; `TranslationRoutingDecision(Direction, TopicDomain, Register, RiskClass, TranslationLevel, AdapterId, Confidence, EvidenceTerms, RejectedAdapters, DecisionReason)`.
  - **6 внутренних узлов** (`ILogicalUnit`): RequestCompiler, DirectionRouter, TopicRouter, RegisterRiskRouter, AdapterSelector, RoutingArbiter. Публичная обучаемая поверхность — сам flow/orchestrator, а не изолированные метки.
  - **Темы D01–D06:** D01 general (Tatoeba); D02 named_entity (ParaNames); D03 legal_diplomatic (UN corpus, только RU/EN; Knesset — support-only); D04 software_ui (REVIEW_REQUIRED); D05 literary (REVIEW_REQUIRED_PER_TITLE); D06 medical/finance (BLOCKED_FOR_FIRST_CURRICULUM).
  - **Роутер-уровни R1–R4:** R1 direction, R2 topic, R3 register/risk, R4 adapter selection; по 50 DPO-строк на ответственность; chosen — полный `TranslationRoutingDecision`, rejected — соседний неверный маршрут с `[WRONG:]`.
  - Runtime-flow: `Input → RequestCompiler → RoutingOrchestrator → RoutingFlow(6 узлов) → PromptBuilder → адаптер → QualityArbiter → Output/repair`. При низкой уверенности: general только для normal-risk, иначе `needs_review`.
  - Allowed register: neutral/colloquial/formal_spoken/formal_written/legal_diplomatic/ui_short/named_entity. Allowed risk: normal/format_sensitive/named_entity_sensitive/formal_high_precision/high_stakes_blocked.
- **Теги:** `#роутер #ILogicalFlow #6-узлов #темы-D01-D06 #R1-R4`
- **Кросс:** #4 (роутер DPO), #2 (активный план).

### 4. `TRANSLATION-ROUTING-FLOW-DPO-GUIDANCE-v1.md` — HIGH
- **Что это:** форма DPO-пар именно для роутинг-слоя.
- **Ключевое:** форма пары `{id, domain:TRANSLATION_ROUTING, contract_signature, router_level:R1.., a_side{source_text, hints, metadata}, chosen{полное решение}, rejected{соседний неверный маршрут + [WRONG:]}}`. Правила: chosen — полное решение (не только adapter_id); rejected — правдоподобный соседний маршрут; unit-level пары должны нести `flow_consequence`. Promotion: direction≥99/topic≥95/adapter≥95/false-low-risk=0/margin>0/нет mojibake. Если переводчик улучшается, а роутер-гейт падает — отклонить промоушн переводчика.
- **Теги:** `#роутер-DPO #flow-vs-unit #promotion-gate`

### 5. `TRANSLATION-SOURCE-REGISTRY-v3.md` (КАНОНИЧНЫЙ реестр) — HIGH
- **Что это:** текущий авторитет по лицензиям источников (v1/v2 — историч.).
- **Ключевое — состояния:** ACCEPT / ACCEPT_SUPPORT_ONLY / QUARANTINE / REVIEW_REQUIRED / METHOD_ONLY / BLOCK.
  - **Clean first-run:** TR-S01 Tatoeba (ACCEPT, CC-BY 2.0, RU-EN/HE-EN); TR-S02 ParaNames (ACCEPT, data CC-BY 4.0 / repo MIT, только named entities); TR-S03 Knesset (ACCEPT_SUPPORT_ONLY, только токенайзер/HE-морфология/formal_spoken, НЕ chosen-authority); TR-S04 UN Parallel Corpus (ACCEPT, public domain, только RU-EN/EN-RU legal_diplomatic).
  - **Quarantine:** TR-Q01 FLORES-200 (eval-only), TR-Q02 Wikipedia/Wiktionary/WikiMatrix (CC-BY-SA).
  - **Review-required:** TR-R01 OpenSubtitles, TR-R02 CCMatrix/CCAligned/ParaCrawl, TR-R03 OPUS Books (per-title), TR-R04 KDE/GNOME/Ubuntu UI, TR-R05 медицина/юр/финансы.
  - **Block:** MIT OCW (NC-SA), Stack Exchange, TED, MUSE-NC, копирайтные Bible-переводы.
  - **First-run policy:** только TR-S01 Tatoeba, TR-S02 ParaNames, TR-S04 UN. Всё прочее — вне train-ready до обновления реестра.
- **Теги:** `#каноничный-реестр #лицензии #ACCEPT-Tatoeba-ParaNames-UN #first-run`
- **Кросс:** #6/#7 (историч. v2/v1), #10 (метод экзамена Layer 0-A), #19 (гейт-скрипт).

### 6. `TRANSLATION-SOURCE-REGISTRY-v2.md` — MEDIUM (superseded v3)
- **Что это:** историч. реестр; добавил HF-ID для каждого источника, HeDC4/Knesset/ParaNames (все коммерч.-чистые), CCMatrix (CC-BY, Meta), opus-books (литература, per-title). Удалил Europarl (нет RU-EN). Разделил OpenSubtitles raw (копирайт) vs OPUS-processed (CC-BY). TED → BLOCK.
- **Важно:** предупреждение вверху — «не использовать для текущих решений; OpenSubtitles НЕ train-ready в v3».
- **Теги:** `#историч #HF-ID #CCMatrix #opus-books #superseded`

### 7. `TRANSLATION-SOURCE-REGISTRY-v1.md` — LOW (superseded v3)
- **Что это:** самый ранний реестр, где OpenSubtitles 2018 ещё был ACCEPT (CC-BY 4.0), ParaCrawl v9 ACCEPT (CC0). Полное дерево OPUS-подкорпусов, монолингвы (mC4/CC-100/CulturaX BLOCK-NC), иврит-специфика (Oscar/HeWiki/Ben-Yehuda), RNC BLOCK.
- **Теги:** `#историч #OpenSubtitles-был-ACCEPT #superseded`

### 8. `TRANSLATION-DPO-REGISTRY-GUIDE-v1.md` — HIGH
- **Что это:** каталог семейств DPO-пар T01–T09 + форма пары + хард-гейты.
- **Ключевое:**
  - 3 gateway-вопроса (кто producer chosen? кто consumer/чему учит? новое семейство или версия?).
  - 4 механизма: `translation_accuracy`, `register_appropriateness`, `morphological_accuracy`, `back_translation_consistency`.
  - **Семейства:** T01 core sentence; T02 register; T03 morphology (RU: падеж/аспект/согласование; HE: род/смихут/артикль/биньян); T04 idiom; T05 false-cognate (только RU_EN/HE_EN); T06 RTL/иврит (только EN_HE); T07 back-translation round-trip; T08 length/completeness; T09 named-entity (RU→EN BGN/PCGN, HE→EN LoC).
  - Форма пары: `pair_id, source_family, direction, mechanism, source_path/id, source_license_state, a_side, chosen, rejected(+[WRONG:]), rejected_type, quality_status, pair_kind, source_canonicality, canonical_confirmed_via, batch_isolation, compiled_prompt_in_training:false, eval_only`.
  - **Хард-гейты:** chosen==rejected; mojibake-коды (U+00C3/U+00C2/…); нет `[WRONG:]`; source BLOCK/REVIEW; нет direction; a_side/chosen на неверном языке.
  - **Overlap-гейт:** `overlap_ratio>0.60 → discard` (при `len(chosen)+len(rejected)≥12`).
- **Теги:** `#семейства-T01-T09 #форма-пары #хард-гейты #overlap`
- **Кросс:** #10 (метод экзамена ссылается сюда), #9 (authoring).

### 9. `DPO-PAIR-AUTHORING-GUIDANCE-TRANSLATION-v1.md` — HIGH
- **Что это:** как писать пары (в отличие от оркестрационного authoring).
- **Ключевое:**
  - `reward_margin = log P(chosen|a) − log P(rejected|a)`; целевой **margin>0.05** после ablation; «sweet spot» rejected — то, что наивная модель реально бы выдала.
  - **4 типа ошибок:** A semantic (≥40% батча), B morphological (20–30%), C register (15–25%), D structural (10–15%).
  - Композиция 64-парного батча по направлениям×семействам; не мешать направления в под-батче.
  - Скрипт-специфика: RU (UTF-8, стресс-марки опц., Ё≠Е, аспект — top-target для EN_RU); HE (логический порядок не визуальный, никуд по регистру, гершайим, 5 финальных букв, биньян — top-target для EN_HE, Knesset = `formal spoken`).
  - `[WRONG:]`-коды: mistranslation/false_cognate/literal_calque/omission/addition/morphological/register_shift/rtl_error/back_translation_divergence/named_entity. Макс 60 слов.
  - Token-overlap test (python) + диагностическая таблица симптом→причина→фикс.
- **Теги:** `#reward-margin #4-типа-ошибок #скрипт-специфика #WRONG-коды`

### 10. `EXAMINATION-METHOD-TRANSLATION-v1.md` — HIGH
- **Что это:** пошаговый метод «прочитать источник → извлечь валидную DPO-пару».
- **Ключевое:** Layer 0-A (квалификация источника: реестр→скрипт/кодировка→язык→длина; **пределы длины:** до extension токенайзера HE≤15/RU≤20 слов, после — ≤32 токена = ContextLen/2); Layer 0-B (дерево назначения семейства T01–T09, самое специфичное); Layer 1 A-side (verbatim, без металанга); Layer 2 chosen (verbatim из корпуса, иначе review_required); Layer 3 rejected (категория ошибки→конструкция→`[WRONG:]`); Layer 4 гейты 1–7; Layer 5 запись JSON. FM-TR-01..07 (chosen из MT, garbled rejected, вагуе [WRONG:], иврит-кодировка, MT-origin, register mixing, одна модель для обеих сторон back-translation). Confidence-метки HIGH/MEDIUM/LOW/BLOCK.
- **Теги:** `#метод-экзамена #Layer-0A-5 #длины-токенов #FM-TR`

### 11. `TRANSLATION-DPO-REJECTED-PROMPTS-v1.md` — HIGH
- **Что это:** копипаст-промпты для создания/аудита/починки rejected-стороны.
- **Ключевое:** Prompt 1 (создать translator rejected), Prompt 2 (создать routing-flow rejected), Prompt 3 (аудит: hard-reject список 1–8, accept список 1–5), Prompt 4 (починить слабый rejected). Примеры good (omission/register-shift/wrong-adapter) и bad (garbage/same/too-many-errors/annotation-без-последствия). Minimal quality rule: ревьюер должен уметь дописать «chosen лучше, потому что rejected делает ошибку X, и если модель выучит rejected — провалится так Y».
- **Теги:** `#rejected-промпты #аудит #good-bad-примеры`

### 12. `TRANSLATION-TOKENIZER-GUIDE-v1.md` — HIGH
- **Что это:** требования к BPE-токенайзеру 3 скриптов; расширение `01.Tokenizer/BpeTokenizer.cs`.
- **Ключевое:** диапазоны Unicode (латиница/кириллица U+0400-04FF/иврит U+0590-05FF+U+FB1D-FB4F); покрытие EN≥97/RU≥95/HE≥90 на FLORES-200; extension (собрать mc4-ru/en 500K + oscar-he 200K → обучить/расширить BPE 64–100K; тест `Tokenizer_MeetsScriptCoverage`). Иврит: никуд как combining (NFD-нормализация), 5 финальных букв как отдельные code-points, гершайим/гереш как single tokens. RU: Ё≠Е, Ъ/Ь не отрезать, BPE на полных формах. Byte-fallback: >10% байт-токенов → discard. Mixed-script per-token, не per-sentence.
- **Теги:** `#BPE #3-скрипта #покрытие #иврит-никуд-финальные #Ё`

### 13. `TRANSLATION-50-PAIR-ABLATION-RUNBOOK-v1.md` — HIGH
- **Что это:** точная механика одного 50-парного батча (референс; операционка — #14).
- **Ключевое:** входы (registry v3, план, ACCEPT-источник, направление, тема, уровень); выходная папка `translation-runs/<дата>/<напр>/[<тема>/]level-<N>/batch-<NNN>/` с 12 обязательными файлами; Шаги 1–7 (inventory→split 50/50→build pairs→train один батч (перед этим роутер-гейт!)→held-out eval→decision→что делать при провале). Decision-состояния: ACCEPT_BATCH/REPAIR_BATCH/REJECT_BATCH/TOKENIZER_BLOCKED/MODEL_CAPACITY_BLOCKED. Promotion: margin>0, accuracy≥0.60, BLEU/chrF без регресса, script≥98%, copy≤2%, wrong-dir=0(L1), ручной обзор ≥8/10. First-run порядок: R1→R2→RU_EN L1→EN_RU→HE_EN→EN_HE.
- **Теги:** `#ablation-runbook #12-файлов #decision-states #promotion-gate`

### 14. `EXECUTOR-PROMPT-TRANSLATION-50-PAIR-LEARNING-v1.md` — HIGH (не прочитан целиком в проходе 1)
- **Что это:** текущий операционный промпт-исполнитель (856 строк — самый большой .md). Именно он выполняет один 50-парный цикл. Проход 2–9 должны разобрать его тело.
- **Проход 1 знает по кросс-ссылкам:** он «намеренно разрешает только один новый 50-парный батч за прогон, чтобы каждый инкремент измерялся»; требует читать `TRANSLATION-EXECUTOR-HARD-RESET-v1.md` перед батчем.
- **Теги:** `#executor-prompt #операционка #TODO-проход-2-9-полное-тело`

### 15. `TRANSLATION-RUNBOOK-v1.md` — MEDIUM
- **Что это:** историч. хендофф + шаблоны python-скриптов.
- **Ключевое:** поправка 2026-05-11 (использовать #14; стрим=напр×тема×уровень; батч 50+50; registry v3; при отсутствии runner — писать `PAIRSET_READY_TRAINING_RUNNER_MISSING` + `RUNNER-CONTRACT.md`, не «awaiting direction»). Содержит **встроенный шаблон `validate_translation_pairs.py`** (гейты: chosen==rejected, mojibake, `[WRONG:]`, negation-templates, blocked/review/quarantine-источники, overlap OVERLAP_THRESHOLD=0.50) и шаблон `evaluate_translation.py` (sacrebleu BLEU/chrF). Формат `run-state.json`.
- **Теги:** `#историч-runbook #validate-скрипт #evaluate-скрипт #run-state`

### 16. `TRANSLATION-EXECUTOR-HARD-RESET-v1.md` — HIGH
- **Что это:** жёсткий role-lock исполнителя.
- **Ключевое:** исполнитель — ТОЛЬКО learning-set (не имплементит `TranslationRoutingFlow`/адаптеры/тесты без явной просьбы; не выдаёт статик-валидацию за translation accuracy). «Missing runner ≠ stop»: продолжать в `PAIRSET_READY_TRAINING_RUNNER_MISSING`, писать все артефакты, `metric_mode:not_run`, НЕ выдумывать CPU/RAM/loss/BLEU/margin. Запрещённые claims (список) → взамен `structural_gate_pass_rate`, `pair_validation_pass_rate`, `source_license_state:SYNTHETIC_SEED`. Реальные метрики для acceptance (список ~15). При блокере — конкретный NEXT ACTION, не вопрос.
- **Теги:** `#hard-reset #role-lock #missing-runner-не-stop #запрещённые-claims #SYNTHETIC_SEED`
- **Связь с 27.TranslationDomain:** именно этот режим объясняет, почему «новые» стримы (translator-ru-en 20 батчей=1000 пар) в `structural_accuracy:1.0`/pairset-режиме, а не с реальными DPO-метриками — тренер по ним не запускался.

### 17. `TRANSLATION-LOOK-LIKE-RUN-REPORT-TEMPLATE-v1.md` — MEDIUM
- **Что это:** шаблон человекочитаемого `LOOK-LIKE-RUN-REPORT.md` на каждый батч (Identity, Pair Counts, Resource, Router Metrics, Translation Metrics, Quality Interpretation, Sample Held-Out, Next Batch Decision). Правила интерпретации Accept/Repair/Reject + обязательный человеческий вывод.
- **Теги:** `#шаблон-отчёта #человекочитаемо`

### 18. `TRANSLATION-SMALL-MODEL-RATIONALE-v1.md` — MEDIUM
- **Что это:** обоснование, почему маленькой модели достаточно.
- **Ключевое:** архитектура делит задачу (роутинг+direction-адаптеры+topic-стримы+level-гейты+50-парные батчи+held-out после каждого). Роутинг сужает пространство; адаптеры делают обучение обратимым; DPO учит preference-границы, не язык с нуля. Что учить первым: контроль (direction→adapter→target-only→no-copy→no-wrong-dir→meaning→entities→no-omission). Не претендует на: legal/medical/literary/unrestricted без ревью. Требуемые доказательства перед доверием: accepted батчи, margin>0, non-regress BLEU/chrF, `LOOK-LIKE-RUN-REPORT.md`, `STREAM-STATE.json`.
- **Теги:** `#small-model #обоснование #роутинг-сужает`

### 19. `prepare_translation_learning_sets.py` — HIGH (реальный код, не шаблон)
- **Что это:** рабочий гейт-скрипт: читает JSONL пар, применяет хард-гейты, маршрутизирует в `training-pairs-{train-ready,repair-required,blocked}.jsonl`. Зеркалит C# `TranslationPairValidator.Validate()` (любое изменение — синхронно в обе стороны).
- **Ключевое:** таблицы `BLOCKED_SOURCES` (culturax/rnc/cc100-he-unreviewed/tedtalks/sharedalign-raw), `REVIEW_REQUIRED_SOURCES`, `QUARANTINE_SOURCES` (wikimatrix/hewiki/flores200/ntrex128/opus-books-bysa), `KNOWN_DIRECTIONS={RU_EN,EN_RU,HE_EN,EN_HE}`, `NEGATION_TEMPLATES`. (Проходы 2–9 — разобрать тело функции validate целиком.)
- **Теги:** `#гейт-скрипт #зеркалит-C#-TranslationPairValidator #источники`

### 20. `training-pairs-train-ready.jsonl` (SEED-КОРПУС, 50 пар) — HIGH
- **Что это:** **seed-корпус** — 50 train-ready пар. id `ru-en-general-l1-b029-001..` → это выгрузка **batch-029** дата-прогона RU→EN L1.
- **Форма строки (реальные ключи):** `id, source, direction, pair_family, prompt, a_side, chosen, rejected, validation_outcome, validation_errors`. (Отличается от «канонической» формы гайда #8 — это упрощённый seed-формат.)
- **Пример:** `direction:RU_EN`, `pair_family:general-adverb-translation`, a_side «Он бесстрашно двинулся через враждебную толпу…», chosen «He intrepidly advanced…», rejected начинается прямо с `[WRONG: fearless_quality_missed — …]` (пара на тонкость перевода наречия).
- **Связь с 27.TranslationDomain:** это тот класс seed-материала, который RAG-01 назвал «отсутствующим на HEAD». Проход 2–9 должен сверить: этот ли seed нужен домену, и не он ли даёт accuracy 0.3333 при обучении на 1 строке.
- **Теги:** `#seed-корпус #50-пар #RU_EN #b029 #связь-с-пилотом`

### 21–23. `training-pairs-repair-required.jsonl` (0), `training-pairs-blocked.jsonl` (0), `translation-runs.zip` (~68 МБ) — LOW
- Первые два пусты (нет пар на починку/блок на верхнем уровне). Zip — бэкап дерева прогонов (артефакт, не читать построчно).

---

## Находки прохода 1 по фактическим прогонам (важно для плана)

1. **Два режима прогонов сосуществуют:**
   - **REAL DPO** — только дата-папка `2026-05-11/ru-en-general/level-1/` (29 батчей, реальные чекпоинты `policy_step_000005.bin`, реальные loss/accuracy/margin). Здесь тренер `TensorCore DPO` реально работал.
   - **PAIRSET-ONLY** — все «новые» стримы (router-r1/r2, translator-en-ru 18 батчей, translator-ru-en 20 батчей=1000 пар, `stream_complete:true`). Тут `structural_accuracy:1.0`/`gate_result:ACCEPTED` — это ГОТОВНОСТЬ НАБОРА, а не обученная модель (режим `PAIRSET_READY`, см. #16).
2. **Реальные метрики тренера скромные:** batch-001 accuracy 0.58 (< гейт 0.60 → REPAIR_BATCH, `accepted_batches:0`); cumulative-200 accuracy 0.29, margin −0.0152 (регресс). Источник был `SYNTHETIC_SEED`, а не настоящий Tatoeba — заметка требует заменить синтетику на реальные Tatoeba ACCEPT-пары.
3. **Ни один батч не «принят»** по строгим гейтам (`accepted_batches:0` в дата-прогоне; «новые» стримы приняты только структурно, без обучения). То есть capability перевода **не доказана как обученная** — согласуется с историей 0.3333/R-19 из RAG-01.

---

## Рамка для следующих проходов

- **Проходы 2–9 (вглубь):**
  - (2–3) Полное тело `EXECUTOR-PROMPT-TRANSLATION-50-PAIR-LEARNING-v1.md` (856 строк) и `prepare_translation_learning_sets.py` (тело validate).
  - (4–5) Дата-прогон `2026-05-11/` — разобрать все 29 батчей: реальные loss/accuracy/margin по батчам, варианты dpo-run (150/200/3step/repeat), содержимое `policy_step_000005.bin`/токенайзеров, что реально обучилось.
  - (6) Стрим `translator-ru-en-general` (20 батчей) — тела `translation-pairs-train-50.jsonl`, focus каждого батча, `STREAM-STATE`.
  - (7) Стрим `translator-en-ru-general` (18 батчей).
  - (8) Роутер-стримы R1/R2 (по 3 батча) — тела train/heldout, error-ledger, sample-review.
  - (9) Seed-корпус (50 пар) построчно + сверка с `27.TranslationDomain` (какой seed «отсутствует на HEAD»).
- **Проход 10:** свод полноты (все 1154 файла × статус) + executive summary + прямой ответ, что из этой папки нужно перенести/подключить, чтобы продолжить пилот перевода в расширениях.

---
*RAG-03, проход 1/10. Составлено эвиденс-байнд: каждый верхнеуровневый .md/.py открыт и прочитан; дерево `translation-runs/` промаплено `find`/выборочными чтениями state/summary. Записан только этот файл; всё прочее — read-only; git-write команды не запускались.*


## Pass 2/10 — реальный DPO-прогон 2026-05-11 (ground truth обучения)

Проход 2 из 10. Область: единственная папка `translation-runs/2026-05-11/`
(824 файла из 1154). Всё только чтение. Цель — установить, обучалась ли реально
модель перевода, и что именно доказывает этот прогон для плана продолжения
расширений VS / VS Code (пилот P0 = перевод).

Метод: прочитана полная структура дерева прогонов; прочитаны ВСЕ 58
`internal-train-ready-dpo-summary.json` (держат измеренные метрики), все 25
`STREAM-STATE.json` (держат вердикт accept/repair и счётчик принятых батчей),
`translation-batch-summary.json` + `NEXT-BATCH-DECISION.md` +
`LOOK-LIKE-RUN-REPORT.md` батча-001, нормализованные DPO-пары, чекпоинт-json,
BPE-токенизатор. НЕ прочитаны построчно: тела всех отдельных пар в 290 jsonl и
бинарные веса (59 `.bin`, все одного размера — см. ниже).

### 1. Структура дерева прогона

```
translation-runs/2026-05-11/
  ru-en-general/            (единственный поток/stream)
    level-1/                (единственный уровень; level-2+ так и не начат)
      batch-001 .. batch-029        (29 батчей = 29 попыток по 50 пар)
        STREAM-STATE.json           (accepted_batches, latest_verdict, статус потока)
        training-pairs-train-ready.jsonl   (50 пар, прошедших 9 гейтов валидации)
        translation-train-50.jsonl / translation-heldout-50.jsonl (50 train + 50 held-out)
        training-pairs-blocked.jsonl / -repair-required.jsonl (обычно пустые = 0)
        translation-batch-summary.json     (ТОЛЬКО в batch-001)
        LOOK-LIKE-RUN-REPORT.md / NEXT-BATCH-DECISION.md (ТОЛЬКО в batch-001)
        dpo-run[/-150 /-200 /-3step /-repeat]/  (варианты прогона тренера)
          <RunId>/                          (напр. ru-en-l1-batch001, ru-en-l1-cumulative-200)
            bpe-tokenizer.json              (BPE 2048, merges; ~42-45K символов корпуса)
            dpo-pairs-normalized.jsonl      (вход тренера: Prompt/Chosen/Rejected)
            progress.jsonl                  (событийный лог: start / batch_complete×N / complete)
            batch-summary.jsonl             (по-батчевые: Loss/ChosenReward/RejectedReward/Margin/Acc/CPU/RAM)
            internal-train-ready-dpo-summary.json/.md  (итог: Avg/Final Accuracy+Margin+Loss, elapsed)
            checkpoints/
              dpo_step_000001..5.json       (по-шаговые метрики)
              policy_step_000005.bin        (веса политики; 1 188 744 байт)
```

Варианты `dpo-run`:
- `dpo-run` — базовый прогон батча (batch-size 10, 5 шагов по 10 пар).
- `dpo-run-150` / `dpo-run-200` (только batch-005) — КУМУЛЯТИВНЫЕ эксперименты:
  тренировка на 150 и 200 накопленных парах (`ru-en-l1-cumulative-150/200`).
- `dpo-run-3step` / `dpo-run-repeat` (только batch-006) — эксперименты с числом
  шагов и повтором.
- Суффиксы `-v1..-v9`, `-r`/`-r2..r4` внутри `dpo-run` (batch-020, 024, 025,
  026, 028, 029) — это ПОВТОРНЫЕ попытки «починить» батч (repair cycles):
  батч 025 перебирался 9 раз (v1..v9), батч 028 — 7 раз (v1..v7), пока
  очередная попытка не давала accuracy >= 0.6.

Итого 58 отдельных прогонов тренера, 59 бинарных чекпоинтов (batch-002 записал 2
`policy_step.bin` — 2 шага; batch-029-v4 записал 0 — прерван/пустой).

### 2. Реальная траектория метрик по всем 58 прогонам

Источник — измеренные значения `InternalTrainReadyDpoRunner` (не хардкод домена).
Метрики: AvgAcc/FinAcc — доля пар, где модель предпочла chosen > rejected (в
среднем по шагам / на последнем шаге); Margin — средняя разница
log-prob(chosen) − log-prob(rejected).

| RunId | Pairs | BS×BC | AvgAcc | FinAcc | AvgMargin | вердикт потока |
|---|---|---|---|---|---|---|
| batch001 | 50 | 10×5 | 0.580 | 0.500 | +0.00586 | REPAIR |
| batch002 (cum-100) | 100 | 50×2 | 0.480 | 0.280 | −0.00158 | REPAIR |
| batch003 | 50 | 50×1 | 0.620 | 0.620 | +0.01539 | **ACCEPT → 1/50** |
| batch004 | 50 | 50×1 | 0.520 | 0.520 | −0.00107 | REPAIR |
| **cumulative-150** | 150 | 50×3 | 0.507 | 0.460 | +0.00283 | эксперимент |
| **cumulative-200** | 200 | 50×4 | **0.290** | 0.200 | **−0.01521** | эксперимент (РЕГРЕСС) |
| batch005 | 50 | 50×1 | 0.380 | 0.380 | −0.00945 | REPAIR |
| batch006-3step | 30 | 10×3 | 0.500 | 0.500 | −0.00317 | эксперимент |
| batch006-repeat/-bs10 | 50 | 10×5 | 0.620 | 0.400 | +0.01539 | эксперимент |
| batch007 | 50 | 10×5 | 0.380 | 0.300 | −0.00945 | REPAIR |
| batch008 | 50 | 10×5 | 0.460 | 0.300 | +0.00242 | REPAIR |
| batch009 | 50 | 10×5 | 0.780 | 0.700 | +0.01424 | **ACCEPT → 2/100** |
| batch010 | 50 | 10×5 | **0.080** | 0.100 | −0.03556 | REPAIR |
| batch011 | 50 | 10×5 | 0.640 | 0.500 | +0.00736 | **ACCEPT → 3/150** |
| batch012 | 50 | 10×5 | **1.000** | 1.000 | **+0.07502** | **ACCEPT → 4/200 (target reached)** |
| batch013 | 50 | 10×5 | **0.000** | 0.000 | −0.08531 | REPAIR |
| batch014 | 50 | 10×5 | 0.020 | 0.000 | −0.05403 | REPAIR |
| batch015 | 50 | 10×5 | 0.780 | 1.000 | +0.02460 | **ACCEPT → 5/250** |
| batch016 | 50 | 10×5 | 0.800 | 0.800 | +0.02111 | **ACCEPT → 6/300** |
| batch017 | 50 | 10×5 | 0.080 | 0.000 | −0.04670 | REPAIR |
| batch018 | 50 | 10×5 | **1.000** | 1.000 | +0.05668 | **ACCEPT → 7/350** |
| batch019 | 50 | 10×5 | 0.960 | 1.000 | +0.04039 | **ACCEPT → 8/400** |
| batch020-v5 | 50 | 10×5 | 0.760 | 0.700 | +0.01935 | **ACCEPT → 9/450** (v1/r/r2/r3/r4 = 0.04–0.58 провалы) |
| batch021-v1 | 50 | 10×5 | 0.940 | 1.000 | +0.04306 | **ACCEPT → 10/500** |
| batch022-v1 | 50 | 10×5 | 0.740 | 0.700 | +0.01826 | **ACCEPT → 11/550** |
| batch023-v1 | 50 | 10×5 | 0.940 | 1.000 | +0.03592 | **ACCEPT → 12/600** |
| batch024-v3 | 50 | 10×5 | 0.680 | 0.700 | +0.01423 | **ACCEPT → 13/650** (v1=0.32, v2=0.36 провалы) |
| batch025-v9 | 50 | 10×5 | 0.900 | 1.000 | +0.02767 | **ACCEPT → 14/700** (v1..v8 = 0.00–0.58 провалы) |
| batch026-v2 | 50 | 10×5 | 0.620 | 0.600 | +0.01000 | **ACCEPT → 15/750** (v1=0.08 провал) |
| batch027-v1 | 50 | 10×5 | 0.940 | 0.900 | +0.03165 | **ACCEPT → 16/800** |
| batch028-v7 | 50 | 10×5 | 0.700 | 0.700 | +0.01661 | **ACCEPT → 17/850** (v1..v6 = 0.16–0.50 провалы) |
| batch029-v1..v7 | 50 | 10×5 | 0.06–0.58 | — | −0.025..+0.006 | REPAIR — не принят, поток остановлен |

Форма траектории — НЕ монотонный рост. Accuracy на 50-парных батчах скачет по
всему диапазону 0.00 ↔ 1.00 между соседними батчами (пример: batch012=1.00,
batch013=0.00, batch014=0.02). Это дисперсия/переобучение крошечной модели, а не
стабильное обучение. Итоговое состояние потока (последний STREAM-STATE, batch-028):
**accepted_batches = 17, accepted_train_pairs_total = 850**, `stream_target_reached`.
batch-029 (последний по времени, 2026-05-12 06:18) STREAM-STATE не имеет — лучшая
из его попыток v3 дала 0.58 (< гейт 0.6), поток закончился на REPAIR.

**Ключевая фальсификация (регресс на накопленных данных).** Единственный раз,
когда пары НЕ подгонялись по одной, а накапливались (эксперимент batch-005):
150 пар → acc 0.507 (margin +0.003), 200 пар → **acc 0.290, margin −0.0152**.
То есть «принятый» по-батчевый сигнал НЕ держится при объединении пар — модель на
200 накопленных парах хуже монетки. Это тот же регресс, что зафиксировал проход 1
(cumulative-200 acc 0.29). Итог «17 принятых / 850 пар» — это сумма прошедших
per-batch гейт попыток, а НЕ модель, обученная на 850 парах.

### 3. Что реально в чекпоинтах/весах

- Все 59 `policy_step_*.bin` весят **ровно 1 188 744 байт** (~1.16 МБ). Это
  крошечная модель из STREAM-STATE batch-001: `hidden_dim=64, num_layers=1,
  num_heads=4, context_len=128, bpe_vocab=2048`. Каждый прогон пишет один
  финальный чекпоинт (для 5-шаговых — `policy_step_000005.bin`).
- `dpo_step_00000N.json` — по-шаговые метрики, напр. batch-012 шаг 5:
  `{"Step":5,"Loss":0.6608,"ChosenReward":0.0695,"RejectedReward":0.0037,
  "RewardMargin":0.0658,"Accuracy":1}`.
- `bpe-tokenizer.json` — реальный BPE-мёрдж-словарь (2048), формат
  `{"Merges":[{"A":32,"B":116,"Result":256}, ...]}` поверх байтов UTF-8. Корпус
  токенизатора 42–45 тыс. символов, обучается на пары каждого батча.
- `dpo-pairs-normalized.jsonl` — ВХОД тренера: `{"Prompt","Chosen","Rejected"}`.
  Prompt — ПОСТОЯННАЯ строка «Translate the Russian word to natural English.»
  **Русское слово-источник (`a_side`) в Prompt НЕ попадает** — оно есть только в
  исходном `training-pairs-train-ready.jsonl`, но нормализатор его отбрасывает.
- `training-pairs-train-ready.jsonl` — исходные пары: `id, source=SYNTHETIC_SEED,
  direction=RU_EN, pair_family (T01/T05/T08/CORR...), a_side (рус.), chosen (англ.),
  rejected (`[WRONG: ...]` длинный неверный вариант), validation_outcome`.
- `progress.jsonl` — событийный лог: `start → batch_complete×N → complete` с
  ElapsedMs (батч ~2–3 с, полный прогон ~12–26 с; исключения — cumulative-200 ~80 с).
- Оценочные метрики (BLEU/chrF/target_script_accuracy/copied_source_rate/
  wrong_direction_rate/named_entity/completeness) во ВСЕХ прогонах = `not_run`:
  «Inference evaluator … not yet implemented».

### 4. Честный вердикт: обучалась ли модель перевода?

**ДА, обучение технически реально:** тренер `InternalTrainReadyDpoRunner`
собирается и запускается, считает настоящие DPO-метрики, пишет настоящие веса.
17 батчей действительно прошли строгий гейт (AvgAccuracy >= 0.6). Отчётность
честная: batch-001 прямо пишет `not_run` по всем метрикам качества, «router R1
not yet trained», «capacity issue: HiddenDim=64», «SYNTHETIC_SEED not Tatoeba».

**НО принятый сигнал не является качеством перевода — это артефакт длины/токена:**
1. Тренер видит постоянный Prompt без русского источника → он физически не может
   обусловиться на вход; он лишь учится предпочитать `Chosen` над `Rejected`.
2. `Rejected` во всех парах — намеренно абсурдный 50-словный этимологический
   разбор с тегом `[WRONG:]`, тогда как `Chosen` — короткий ответ («Да!»→«Yes!»).
   Модель учит «короткий/чистый ответ хорош, длинная простыня плоха» —
   дискриминатор длины/стиля, а не переводчик.
3. STREAM-STATE прямо это подтверждают. batch-012 (acc 1.000): «BREAKTHROUGH
   FORMULA: single-word Russian → single-word English chosen vs 50+ word etymology
   rejected. **Maximum length contrast**». batch-028: 6 циклов ремонта — оператор
   вручную подбирал, какие слова (Wrathfully/Woefully/Mockingly) дают нужный
   BPE-токен 716/933, чтобы accuracy перевалила за 0.6: «produce-hint cancellation
   scales with BPE token dominance». Это ИНЖЕНЕРИЯ ТОКЕНИЗАТОРА ради метрики
   (reward-hacking гейта), а не обучение переводу.
4. Качество перевода (BLEU/chrF/script) не измерялось НИКОГДА — inference-эвалуатор
   не реализован. Данные-пары при этом настоящие переводные (held-out review
   batch-001: «Добро пожаловать!»→«Welcome!», 10/10 структурно валидны) — но
   тренер это качество не использует.

**Что это доказывает для плана:**
- Опровергает хардкод домена `0.3333 / 0.03` (проход 1): реальные числа —
  17 принятых батчей, AvgAccuracy до 1.0, положительные маржины до +0.075. Домен
  27.TranslationDomain НЕ отражает этот прогон.
- Но НЕ доказывает, что модель умеет переводить. Доказано только: (a) DPO-тренер
  и чекпоинты работают; (b) на подогнанных по длине/токену парах достигается
  per-batch preference-accuracy >= 0.6; (c) на накопленных данных (200 пар) сигнал
  регрессирует до 0.29. `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` — верхняя граница:
  скелет тренировки честен, но переводного качества нет.
- Источник так и остался `SYNTHETIC_SEED` (строка встречается 2 раза, `tatoeba`
  один раз как `synthetic-seed-tatoeba-style`); реальный Tatoeba ACCEPT-корпус,
  рекомендованный ещё в NEXT-BATCH-DECISION батча-001, так и не подключён.
- Гейт приёмки (per-batch AvgAcc>=0.6) — геймящийся: он ловит контраст длины, а
  не верность перевода; отсутствие inference-эвалуатора делает любой «ACCEPT»
  неполным доказательством. Для плана продолжения расширений это означает: пилот
  перевода в состоянии «честный необученный скелет», требуется (1) реальный
  parallel-корпус, (2) подача источника в Prompt, (3) inference-эвалуатор
  BLEU/chrF/script перед любым заявлением «модель обучена».

### Что осталось непрочитанным в этом проходе

Тела отдельных пар во всех 290 `*.jsonl` (прочитаны образцы batch-001/012/029 и
held-out review 10 пар); бинарные веса 59 `.bin` (только размеры/идентичность);
полные `batch-summary.jsonl` каждого прогона (прочитан batch-001 целиком, у
остальных — итоговые `internal-...-summary.json`). Регресс на кумулятивных данных
подтверждён только экспериментом batch-005 (150/200); отдельного «финального»
прогона на всех 850 принятых парах в дереве нет.


---

## Pass 3/10 — pairset-потоки: R1 direction router, R2 topic router, translator en-ru / ru-en

**Что читалось (READ-ONLY):** четыре потока в `translation-runs/`, где DPO-тренер
НИКОГДА не запускался (все `metric_mode: not_run` / `structural_accuracy=1.0`,
только наборы пар). По каждому потоку прочитаны все STREAM-STATE.json,
translation-batch-summary.json, heldout-evaluation.json, NEXT-BATCH-DECISION.md,
sample-review.md; сами пары просэмплированы в 2-3 батчах на поток + анализ длины
по всем 50 парам батча. Верхнеуровневый `translation-runs/DOCUMENT-USE-LOG.md`
прочитан целиком.

### Итоговая таблица потоков (точные числа)

| Поток | Батчей | Train пар | Heldout пар | Источник | Тренер | Дефект reward-hacking |
|---|---|---|---|---|---|---|
| router-r1-direction | 3 | 150 | 150 | `synthetic-router-seed` / SYNTHETIC_SEED | не запускался (`pending_neural_training`) | чистый от length-хака пасса 2, НО confidence+evidence-count утечка; задача детерминированная |
| router-r2-topic | 3 | 150 | 150 | `synthetic-router-seed` / SYNTHETIC_SEED | не запускался | **циклическая метрика** (keyword-матчер судит keyword-матчер) + confidence/evidence утечка |
| translator-ru-en-general | 20 | 1000 | 0 | `synthetic-translator-seed` / SYNTHETIC_SEED | не запускался (structural-only) | **чистый** — реальный source-in-prompt, настоящие семантические дистракторы, длина сбалансирована |
| translator-en-ru-general | 18 | 900 | 50 (только b018) | b001-017 `original`/clean; **b018 `TR-S01`/ACCEPT (реальный)** | не запускался (`PAIRSET_READY_TRAINING_RUNNER_MISSING`) | b018 самый зрелый/честный; в b001-017 **умеренный перекос длины** (rejected длиннее 33/50) |

Всего по 4 потокам: **2200 train пар + 350 heldout пар**. Ни один не прошёл
обучение — «ACCEPTED» здесь означает только «пары well-formed», не «модель
обучена».

---

### Поток 1 — router-r1-direction (R1, детекция направления)

**Что учат пары:** по тексту-источнику выбрать направление перевода из четырёх —
RU_EN / EN_RU / HE_EN / EN_HE — и соответствующий adapter_id. Распределение по
батчам ровное: ~13 RU_EN, 12 EN_RU, 10-12 HE_EN, 13-15 EN_HE.

**Формат пары (train):** богатый объект-РЕШЕНИЕ, НЕ просто строка.
- `a_side.source_text` — реальная, меняющаяся от пары к паре фраза («Привет, как
  дела?», «Я хочу пить воду.», «Книга лежит на столе.») + `target_language`.
- `chosen` = {direction, topic_domain, register, risk_class, translation_level,
  adapter_id, **confidence**, **evidence_terms[]**, decision_reason}.
- `rejected` = такой же объект с неверным направлением; в `decision_reason` вшита
  метка `[WRONG: wrong_direction - <функциональное последствие>]`.
- heldout-строки НЕ содержат chosen/rejected: только `expected_direction` +
  `expected_adapter_id` + `script_class` (это eval-строки для оракула).

**Per-batch вердикты:** b001 baseline «чистый скрипт» → ACCEPTED, actual
direction_accuracy=1.00 (50/50); b002 «сложнее — вкрапления латиницы,
транслитерации, короткие реплики» → 1.00; b003 «code-switch Cyrillic+Latin, tie-
breaking, бренды, культурные loanwords» → 1.00, поток R1 завершён, переход к R2.
Ручной sample-review b001: 10/10 pass.

**Представительные примеры (цитаты):**
- `TR-ROUTE-R1-0001`: source «Привет, как дела?» → chosen direction=RU_EN
  conf=0.96 evidence=[cyrillic_script, target_language=en, greeting_pattern];
  rejected direction=EN_RU conf=0.61, `[WRONG: wrong_direction - source text
  contains Cyrillic script confirming Russian origin; routing to EN_RU sends
  Cyrillic input to a decoder that expects English source text]`.
- `TR-ROUTE-R1-0041` (из ревью): source «She studies Hebrew at the university.» →
  chosen EN_HE; rejected HE_EN «маршрутизирует по слову 'Hebrew' как будто это
  иврит-скрипт» — сильная граница «keyword-vs-script».
- heldout `HO-0026`: source «בוקר טוב, מה שלומך?» expected_direction=HE_EN,
  script_class=hebrew.

**Вердикт reward-hacking — ЧАСТИЧНО дефектный, но по-другому, чем пасс 2:**
1. Пары СТРУКТУРНО ЛУЧШЕ пасса 2: реальный меняющийся источник в промпте,
   настоящие дистракторы, `[WRONG:]`-аннотации, БЕЗ length-хака (chosen и rejected
   decision_reason сопоставимой длины).
2. НО «direction_accuracy=1.00» — это ДЕТЕРМИНИРОВАННЫЙ оракул:
   `inference_method = "TranslationRoutingFlow deterministic R1 script-detection"`.
   Кириллица→RU, иврит→HE, латиница→EN решается 3-строчным `if`. Тренер не
   запускался: `dpo_reward_margin = pending_neural_training`. То есть 1.00 не
   доказывает обученный роутер — доказывает, что скрипт-детектор согласен сам с
   собой.
3. **Confidence-артефакт (утечка):** chosen всегда conf 0.95-0.97, rejected
   всегда 0.55-0.61. DPO-тренер может выучить «предпочитай сторону с бОльшим
   полем confidence», а не логику направления.
4. **Feature-count артефакт:** у chosen `evidence_terms` всегда богаче, чем у
   rejected. Ещё один шорткат.
5. Источник — SYNTHETIC_SEED.
Вывод по данным: **чисты от length-хака, но несут confidence/evidence-count
утечку и покрывают тривиальную (детерминированную скриптом) задачу → низкая
тренировочная ценность как есть**. Для плана: если R1 нужен как обучаемый юнит,
убрать confidence/evidence_terms из входа модели (иначе leakage), либо признать
R1 детерминированным baseline-оракулом (что честнее).

---

### Поток 2 — router-r2-topic (R2, детекция темы)

**Что учат пары:** после известного направления определить тему —
medical / legal / technical / cultural / personal / general — и адаптер темы.
Покрытие: все 6 тем × все 4 направления в каждом батче.

**Формат пары:** `a_side` = {source_text (реальная фраза), direction,
target_language, user_domain_hint}. `chosen` = {topic_domain, adapter_id,
confidence, **evidence_terms** (сматченные ключевые слова), decision_reason}.
`rejected` = почти всегда «дефолт»: {general/technical, adapter, conf низкий,
`evidence_terms: []`, «No specialised domain keywords detected»}. Отдельное поле
`wrong_annotation: [WRONG: wrong_topic_domain - <последствие>]`. heldout несёт
только `chosen.topic_domain + adapter_id`.

**Per-batch вердикты:** b001/b002/b003 все topic_accuracy=1.00 (50/50),
`gate_threshold: 0.99`, ACCEPTED; b003 → `stream_complete=true`,
`next_action: begin_translator_streams`.

**Представительные примеры (цитаты):**
- `TR-ROUTE-R2-0101`: source «Врач назначил мне антибиотики от инфекции.» →
  chosen medical conf=0.92 evidence=[врач, антибиотик, инфекц]; rejected general
  evidence=[] «No specialised domain keywords detected; defaulting to general».
- `TR-ROUTE-R2-0102`: source «У пациента диагностировали хроническое заболевание
  лёгких.» → chosen medical evidence=[пациент, диагноз, болезн]; rejected
  technical evidence=[] «No technical domain keywords found».
- ревью: `0111` «Магазин закрывается в девять…» → chosen general
  (`no_specialised_keywords_matched`), rejected technical пустой.

**Вердикт reward-hacking — ХУЖЕ по интегритету, чем R1:**
1. **Циклическая (self-referential) метрика:** «инференс»
   TranslationTopicRoutingFlow — тот же keyword-матчер, что СТРОИЛ пары. Он
   сматчит те же ключевые слова → 100% по построению. topic_accuracy=1.00
   доказывает «keyword-lookup == keyword-lookup», а не обученную детекцию темы.
   Это близкий родственник reward-hacking — measurement circularity.
2. **Дистрактор вырожден:** rejected почти всегда «нет ключевых слов → default»
   с `evidence_terms: []`. DPO-сигнал тривиализуется до «выбирай сторону, у
   которой массив evidence_terms НЕ пустой». Это прямое `R4 NO HIDDEN
   DETERMINISM`: детерминированный словарный lookup, наряженный в обучаемый
   тематический роутер.
3. Тот же confidence-артефакт (0.90-0.97 vs 0.45-0.55).
4. Источник SYNTHETIC_SEED.
Отличие от R1: тема — ЗАКОННО обучаемая семантическая задача (в отличие от
детекции скрипта). Но данные как построены учат словарь. **Не годны как есть для
доказательства обученного роутера**; для плана нужно: (a) дистракторы = ПРАВДОПОДОБНО
неверная тема (medical↔technical на реальном тексте), а не пустой-evidence дефолт;
(b) eval, НЕЗАВИСИМЫЙ от keyword-матчера; (c) реальный корпус.

---

### Поток 3 — translator-ru-en-general (перевод RU→EN)

**Что учат пары:** предпочесть естественный/корректный английский перевод
дефектному переводу ТОГО ЖЕ русского источника. 20 батчей по тематикам
(daily life, travel, sports, city services, work…), 1000 пар (TR-RU-EN-GEN-0101…2050).

**Формат пары (плоский, простой):** `source` (реальная RU-фраза),
`chosen_translation` (верный EN), `rejected_translation` (дефектный EN того же
источника), `defect_type` (7 типов), `defect_description`. Источник
`synthetic-translator-seed` / SYNTHETIC_SEED. **Heldout-файлов в этом потоке нет
вообще (0).**

**7 типов дефектов (распределение ~ровное на батч):** literal_translation,
grammar_error, wrong_register, missing_idiom, false_friend, omission,
wrong_word_choice.

**Per-batch вердикты:** каждый батч `structural_accuracy=1.00` (валидация
сэмплирует 20 из 50), gate 0.99, ACCEPTED; проверки: `chosen_ne_rejected`,
`defect_type_in_allowed_set`, `no_empty_fields`. b020 → `stream_complete=true`,
1000 пар.

**Представительные примеры (цитаты) — качество высокое:**
- `0101` literal: src «Мне двадцать лет.» → chosen «I am twenty years old.» /
  rejected «To me twenty years.»
- `false_friend`: src «Конечно, я помогу тебе.» → chosen «Of course, I will help
  you.» / rejected «Concretely, I will help you.» (конечно ≠ конкретно).
- `missing_idiom`: src «Он бьёт баклуши весь день.» → chosen «He has been loafing
  around all day.» / rejected «He has been beating wooden pegs all day.»
- `omission`: src «Он ведь умный человек.» → chosen «He is, after all, a smart
  person.» / rejected «He is a smart person.» (тут chosen ДЛИННЕЕ — обратно хаку
  пасса 2).
- `wrong_word_choice`: src «Она приготовила вкусный обед.» → chosen «…a delicious
  lunch.» / rejected «…a tasty dinner.» (обед ≠ ужин).

**Анализ длины (все 50 пар b001):** avg chosen 4.88 слов vs rejected 5.24;
rejected длиннее 19/50, короче 14/50, равно 17/50 — **сбалансировано, БЕЗ
систематического length-хака**.

**Вердикт reward-hacking — ГОДНЫ (лучшие данные в папке):**
Свободны от дефекта пасса 2. Реальный меняющийся источник в промпте, настоящие
минимальные-парные СЕМАНТИЧЕСКИЕ дистракторы (а не длина/токенайзер), 7
осмысленных типов ошибок, длина сбалансирована. Остаточные оговорки: (a) тренер
не запускался — валидация ТОЛЬКО структурная (20/50), «1000 ACCEPTED» = «1000
well-formed пар», не «обучен перевод»; (b) источник всё ещё SYNTHETIC_SEED
(авторские, не gold parallel corpus типа Tatoeba); (c) в потоке НЕТ heldout-сплита
→ нет eval-строк; (d) обе стороны само-авторские → «chosen верен» ничем внешним
не подтверждён.

---

### Поток 4 — translator-en-ru-general (перевод EN→RU, кириллический вывод)

**Что учат пары:** предпочесть корректный русский перевод дефектному переводу
того же английского источника. 18 батчей, 900 train + 50 heldout (только b018).

**Формат ЭВОЛЮЦИОНИРОВАЛ:**
- b001-017 — простой плоский формат: `source`, `chosen_translation`,
  `rejected_translation`, `defect_type`, `defect_description`; `source_id:
  "original"`, `source_license_state: "clean"` (по сути синтетика).
- **b018 — зрелый формат:** `pair_id`, `pair_family` (T01 базовый / T08 omission /
  T09 named-entity / corruption), `source_text`, `chosen`, `rejected`,
  `defect_annotation [WRONG:]`, `routing_labels{direction,topic,register,risk,
  level,adapter}`, **реальный `source_id: TR-S01`, `source_license_state:
  ACCEPT`** (Tatoeba EN-RU health_body), split train+heldout.

**Per-batch вердикты:** b001-017 `structural_accuracy=1.00` ACCEPTED; b018
`status: PAIRSET_READY_TRAINING_RUNNER_MISSING`, `metric_mode: not_run`, ручной
review 10 heldout → 9/10 PASS (1 репейр аннотации в row 0036 — реальный найденный
баг: текст аннотации ссылался на «nausea and photosensitivity» вместо «sneezing
and feverish»; контент пары верный). error-ledger: 0 structural errors.

**Представительные примеры (цитаты):**
- b001 `0101` literal: src «Good morning!» → chosen «Доброе утро!» / rejected
  «Хорошее утро!» (кальковый literal, не существует как приветствие).
- b001 `0103` grammar: src «I am fine, thank you.» → chosen «Всё хорошо, спасибо.»
  / rejected «Я есть хорошо, спасибо.» (калька англ. связки to be).
- b018 `0001` (реальный TR-S01): src «I have a headache.» → chosen «У меня болит
  голова.» / rejected «Я имею боль головы.» + `[WRONG: literal_translation -
  literal calque…; dative impersonal required]`.
- b018 `0041` T09 named-entity: src «The Red Cross clinic…» → chosen «Клиника
  Красного Креста…» / rejected «Клиника Красного Пересечения…» (буквальный
  перевод имени собственного).
- b018 `0046` corruption/copied_source: rejected = английский источник СКОПИРОВАН
  дословно в русское поле → негативный контроль на copied_source_rate.

**Анализ длины — ВАЖНАЯ находка:**
- b001 (синтетика): avg chosen 2.52 слов vs rejected 3.34; **rejected длиннее
  33/50 = 66%**, короче 4/50, равно 13/50. Это УМЕРЕННЫЙ перекос длины: русские
  кальки английского добавляют служебные слова, которые естественный русский
  опускает. НЕ инженерный short-good/long-bad пасса 2, но length-only DPO-сигнал
  мог бы эксплуатировать корреляцию ~66% раз в ранних синтетических батчах.
- b018 (зрелый): avg chosen 4.42 vs rejected 4.70; длиннее 18, короче 12, равно
  20 — **сбалансировано**.

**Вердикт reward-hacking — ГОДНЫ; b018 самый зрелый/честный артефакт папки:**
Настоящие дистракторы-кальки того же источника. b018 добавляет негативные
контроли, которых НЕ было ни в роутерах, ни в пассе 2: T08 completeness, T09
named-entity, corruption/copied_source. b018 ТОЧНО СПЕЦИФИЦИРУЕТ отсутствующий
инференс-эвалуатор, о котором говорил пасс 2 — `RUNNER-CONTRACT.md`: bleu/chrf,
target_script_accuracy, copied_source_rate, wrong_direction_rate,
named_entity_preservation, completeness_score, dpo_reward_margin, dpo_accuracy —
но `metric_mode: not_run`, все метрики null, `"Do not invent metric values"`
(честно). Оговорки: (a) тренер не запускался; (b) b001-017 синтетика + умеренный
перекос длины → для плана: length-контроль дистракторов или length-balancing
проверка на ранних EN→RU батчах; (c) реальный корпус только в b018 (TR-S01).

---

### Сводный вердикт reward-hacking (сравнение с пассом 2)

**Пасс 2 (единственный реальный DPO-прогон 2026-05-11):** тренер РАБОТАЛ, но
reward-hacked — константный промпт, инженерная short-good/long-bad длина,
tokenizer-engineering, SYNTHETIC_SEED. Потолок = честный необученный скелет.

**Эти 4 pairset-потока:** тренер НЕ запускался ни разу (structural-only /
not_run) → reward-hacked МЕТРИК не породили. Качество ДАННЫХ расслаивается:
- **translator RU-EN + EN-RU — ГОДНЫ:** реальный source-in-prompt, настоящие
  семантические дистракторы, 7 типов дефектов, негативные контроли в b018.
  Дефекта пасса 2 НЕТ. Остаток риска: SYNTHETIC_SEED (RU-EN + EN-RU b001-017),
  умеренный перекос длины в синтетических EN-RU батчах, отсутствие внешнего gold.
- **router R1 + R2 — НЕ ГОДНЫ как есть:** confidence-поле и evidence-count утечка
  в обеих; R1 — детерминированная (скрипт) задача, низкая ценность; R2 — метрика
  циклична (keyword-матчер судит keyword-матчер), дистрактор вырожден в пустой-
  evidence дефолт → нарушение R4 (скрытый детерминизм).

**Практический вывод для плана продолжения расширений (P0 translation pilot):**
переводные наборы (1900 пар RU-EN + EN-RU) — реальный обучаемый актив, если
подключить (1) DPO-runner по RUNNER-CONTRACT.md, (2) реальный parallel-корпус
вместо SYNTHETIC_SEED (TR-S01 уже размечен ACCEPT в b018), (3) инференс-эвалуатор
(bleu/chrf/target_script/copied_source) ПЕРЕД любым заявлением «модель обучена».
Роутерные наборы (R1/R2) требуют переработки дистракторов и независимого eval,
иначе останутся детерминированными baseline-оракулами.

### Что осталось непрочитанным (для пассов 4-9)

- Тела пар вне просэмплированных 2-3 батчей/поток: ~2200 train + 350 heldout;
  не читались построчно R1-b002/003, R2-b002/003, RU-EN b002-020, EN-RU b002-017
  и остаток строк EN-RU b018.
- Существуют ли HE_EN / EN_HE translator-потоки? (в дереве их каталогов НЕТ —
  Hebrew translator был заблокирован токенайзер-гейтом; подтвердить отсутствие
  явно в отдельном пассе).
- Полный `RUNNER-CONTRACT.md` (прочитана голова ~60 строк) — точная спецификация
  отсутствующего runner для P0-пилота.
- Верхнеуровневый `training-pairs-train-ready.jsonl` (49980 байт) — сводный
  train-ready набор — не читался.
- 20 верхнеуровневых guide-доков (FOUR-LEVEL-TRAINING-PLAN, TOPIC-ROUTER-
  ORCHESTRATION-PLAN, DPO-PAIR-AUTHORING-GUIDANCE, EXAMINATION-METHOD, SOURCE-
  REGISTRY v1/v2/v3, TOKENIZER-GUIDE, ABLATION-RUNBOOK) — пасс 1 отметил их
  наличие, но роутинг/тренинг-планы нужны deep-read для плана расширений.
- `prepare_translation_learning_sets.py` (генератор) — подтвердить, инжектятся ли
  confidence/length-артефакты систематически на этапе генерации.

---

## Pass 4/10 — design/guide bodies + generator script (root-cause of the artifacts)

**Что читалось построчно (READ-ONLY):** 12 верхнеуровневых доков целиком —
FOUR-LEVEL-TRAINING-PLAN, TOPIC-ROUTER-ORCHESTRATION-PLAN, ROUTING-FLOW-DPO-
GUIDANCE, TRANSLATION-TRAINING-PLAN (v1.1), DPO-REGISTRY-GUIDE (T01-T09), DPO-PAIR-
AUTHORING-GUIDANCE, TOKENIZER-GUIDE, SMALL-MODEL-RATIONALE, EXAMINATION-METHOD,
LOOK-LIKE-RUN-REPORT-TEMPLATE, 50-PAIR-ABLATION-RUNBOOK, RUNBOOK, EXECUTOR-HARD-
RESET; и главное — **весь `prepare_translation_learning_sets.py` (330 строк)**.

### ГЛАВНАЯ НАХОДКА (root-cause): генератор — это ВАЛИДАТОР/ГЕЙТ, а НЕ синтезатор

`prepare_translation_learning_sets.py` **НЕ инжектит ни confidence-артефакт
(chosen 0.95 vs rejected 0.55), ни length-перекос** — ни систематически, ни в
каких-то потоках. Он вообще не создаёт пары. Он читает готовые JSONL-пары и
раскладывает их по трём файлам (`train-ready` / `repair-required` / `blocked`) по
9 жёстким гейтам. Доказательство — что делает скрипт (validate_pair, строки
141-211):

| Gate | Проверка | Артефакт? |
|---|---|---|
| 1 | `chosen.strip()==rejected.strip()` → repair | нет |
| 2 | mojibake-маркеры `Ã Â â€ Ã¢` в любом поле → repair | нет |
| 3 | в `rejected` нет `[WRONG:` → repair | нет |
| 4 | negation-шаблон в rejected (do not translate / n/a / …) → repair | нет |
| 5 | `direction` пуст или не в {RU_EN,EN_RU,HE_EN,EN_HE} → repair | нет |
| 6 | source_id в BLOCKED_SOURCES (culturax, rnc, cc100-he-unreviewed, tedtalks, sharedalign-raw) → **blocked** | нет |
| 7 | source_id в REVIEW_REQUIRED (cc100-ru, cc100-he, mnlp-he-unreviewed, ccaligned-unreviewed) → repair | нет |
| 8 | QUARANTINE (wikimatrix, hewiki, flores200, ntrex128, opus-books-bysa) без `batch_isolation:true` → repair | нет |
| 9 | whitespace-overlap(chosen,rejected минус a_side) > 0.50 при ≥12 токенов → repair | косвенно |

- **Поля `confidence`/`evidence_terms` скрипт вообще не читает и не пишет.** Он
  оперирует только `a_side`/`chosen`/`rejected`/`direction`/`source_id`/
  `batch_isolation` как СТРОКАМИ. Структурные роутерные объекты
  (`chosen.confidence`) он бы даже не разобрал корректно (`chosen.get` вернул бы
  dict, `.strip()` упал бы) — значит генератор писался под **строковые translator-
  пары**, а НЕ под роутерные. Роутерные R1/R2 через этот гейт не проходят вообще.
- **Length-баланс не проверяется.** Единственный «длинно-смежный» гейт — №9
  (overlap), и он set-based (пересечение множеств токенов), а не длина. Он ОТБРАКует
  пары, где chosen≈rejected (учит артефакт «различай что угодно»), т.е. косвенно
  ПООЩРЯЕТ расхождение сторон, но НЕ вводит short-good/long-bad перекос пасса 2.
- **Порог 0.50, а не 0.60** — сознательная поправка на whitespace-аппроксимацию
  BPE (комментарий строки 86-90 и в RUNBOOK строки 171-178: «restore to 0.60 once
  BPE tokenizer callable from Python»). Это известный технический долг, не артефакт.
- Побочно: `sys.exit(1)` если есть blocked-пары (для CI-детекта лицензий).

**Вывод root-cause (точный):** артефакты **не в генераторе**. Они рождаются на
этапе АВТОРИНГА пар:
1. **Confidence-артефакт — заложен в ДИЗАЙН-ШАБЛОН роутерных пар.**
   `TRANSLATION-ROUTING-FLOW-DPO-GUIDANCE-v1.md` (строки 51-73) даёт эталон, где
   `chosen.confidence = 0.94`, `rejected.confidence = 0.88`, и у chosen
   `evidence_terms` из 3 элементов + `rejected_adapters`, а у rejected — 1 элемент
   и нет `rejected_adapters`. Т.е. **гайд сам семенит утечку**: скалярное поле
   confidence + асимметрия числа полей, где chosen всегда «богаче». Авторы пар
   (пасс 3: 0.95-0.97 vs 0.55-0.61) лишь УСИЛИЛИ разрыв против шаблона (0.94/0.88).
2. **Length-перекос EN→RU (пасс 3: rejected длиннее 66% в b001-017)** — естественный
   побочный эффект синтетического авторинга (русские кальки английского добавляют
   служебные слова), а НЕ инъекция. Генератор его не ловит (нет length-гейта) и не
   вносит.

**Дыра гейт-сета (для плана):** у генератора НЕТ (a) гейта против confidence/
feature-count leakage, (b) length-balance гейта. Плюс он не покрывает роутерные
структурные пары. Поэтому именно валидатор пропустил артефакты, найденные пассами
2/3. Фикс: добавить leakage-гейт (запретить confidence/evidence_terms как вход
модели ИЛИ вычищать их из тренировочного представления) и length-skew гейт;
починить роутерный шаблон в ROUTING-FLOW-DPO-GUIDANCE (убрать confidence из
chosen/rejected или уравнять богатство полей).

### 4-уровневый дизайн обучения (FOUR-LEVEL-TRAINING-PLAN, ACTIVE — операционный)

Обучаются **4 параллельных адаптера направления** (RU_EN, EN_RU, HE_EN, EN_HE),
НЕ один смешанный. Перед генерацией — обучаемый **routing-flow** (не hardcoded
switch): `TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>` +
`TranslationRoutingOrchestrator : ILogicalOrchestrator`. Поток = `направление ×
topic × level`; батч = **50 train + 50 heldout** (50 — размер батча, НЕ размер
уровня). Первый проход = 4 направления × 1 topic(general) × 4 уровня × 50 = 800 пар.

| Level | Что учит | Микс на 50 | Promotion-гейт |
|---|---|---|---|
| L1 Direction/core | слушаться направления, чистый target | 35 T01 + 5 T08 + 5 T09 + 5 corruption | target-script ≥98%, margin>0, chrF/BLEU не регрессируют, ≤1 copied-source/50 |
| L2 Meaning/syntax/completeness | порядок клауз, отрицание, предлоги, полнота | 20 T01 + 10 T08 + 10 T03 + 5 T04 + 5 Type-C | BLEU не падает >1 от L1, chrF стабилен, аудит 10 без systematic drop, margin>0 |
| L3 Morphology/register/NE | рус. падеж/аспект; ивр. род/смихут/биньян/RTL; регистр; имена | 15 T03 + 10 T02 + 10 T09 + 5 T06 + 10 стабилизаторы | NE-preservation ≥95%, ивр. без визуал-порчи, рус. Cyrillic-чек, margin>0 по morph и NE отдельно |
| L4 Robustness/round-trip | стойкость к реальным промптам | 15 T07 + 10 T08 + 10 T04 + 10 T02 + 5 hard-neg | round-trip ≥ baseline, BLEU/chrF без регресса, 0 wrong-direction/50, аудит ≥8/10 |

**Level score 0-4**: 0=провал/wrong-direction/порча скрипта; 4=production-candidate.
Текущий уровень направления = высший, чей гейт прошёл. **Failure-rules:** стоп
батча если margin<0 после первого чекпоинта, wrong-direction>2%, copy-source>2%,
ивр. визуал-порча, `chosen==rejected`, нет `[WRONG:]`, ИЛИ **BLEU/chrF рушится при
растущем DPO-margin (= пары учат preference-артефакты, не перевод)** — это ровно
дефект пасса 2, встроенный в failure-правило.
**Router-gate ПЕРЕД тренировкой транслятора:** direction ≥99%, topic ≥95%,
adapter-id ≥95%, false-low-risk = 0. Роутер провалился → транслятор не тренируется.

### Дизайн роутера R1-R4 (TOPIC-ROUTER-ORCHESTRATION-PLAN + ROUTING-FLOW-DPO-GUIDANCE, ACTIVE)

Роутер = обучаемая оркестрация, НЕ classifier/regex/switch. Публичная цель обучения
— весь пакет `TranslationRoutingDecision`(Direction, TopicDomain, Register,
RiskClass, TranslationLevel, AdapterId, **Confidence**, EvidenceTerms,
RejectedAdapters, DecisionReason), а не только AdapterId. Внутренние узлы (могут
быть `ILogicalUnit`): RequestCompiler → DirectionRouter → TopicRouter →
RegisterRiskRouter → AdapterSelector → RoutingArbiter.

| Router level | Задача | Rejected = правдоподобный соседний неверный маршрут |
|---|---|---|
| R1 | направление RU_EN/EN_RU/HE_EN/EN_HE | обратное направление, неверный скрипт-источник |
| R2 | тема general/named_entity/legal_diplomatic/software_ui*/literary*/high_stakes_blocked | UN-формал как general; NE как обычное предложение; high-stakes как normal |
| R3 | регистр+риск (neutral/colloquial/formal_spoken/formal_written/legal/ui_short/named_entity) | formal→colloquial; NE-sensitive→normal; high-risk пропущен вместо блока |
| R4 | выбор адаптера | соседний адаптер направления/темы; general для legal |

Topic-домены: D01 general(Tatoeba, ACCEPT) · D02 named_entity(ParaNames, ACCEPT) ·
D03 legal_diplomatic(UN corpus только RU/EN; Knesset — support-only, НЕ chosen-
authority) · D04 software_ui = REVIEW_REQUIRED · D05 literary = REVIEW_REQUIRED_
PER_TITLE · D06 medical/finance = **BLOCKED_FOR_FIRST_CURRICULUM**. При низкой
уверенности: НЕ угадывать спец-адаптер — general только для normal-risk, для
формал/high-risk → `needs_review`. **Важно для пасса 3-вердикта:** гайд ЯВНО пишет
`confidence` в chosen(0.94) и rejected(0.88) → это и есть источник утечки, найденной
в R1/R2. Promotion роутера требует «no `chosen==rejected`, every rejected has
`[WRONG:]`, no mojibake» — но **гейта против confidence-leakage в гайде нет**.

### Таксономия дефектов T01-T09 (DPO-REGISTRY-GUIDE, ACTIVE) + 4 механизма

4 механизма: `translation_accuracy`, `register_appropriateness`,
`morphological_accuracy`, `back_translation_consistency`.

| Код | Семейство | Rejected-дефект (пример `[WRONG:]`) |
|---|---|---|
| T01 | Sentence core | mistranslation/omission/literal calque (min 8 пар/направление/батч) |
| T02 | Register preservation | register_shift (formal→colloquial) |
| T03 | Morphological | рус.: падеж после предлога, аспект (perf/imperf), кратк./полн. прилаг., одуш.; ивр.: род, смихут(construct), ha-propagation, биньян |
| T04 | Idiom/fixed expr | literal_calque («ни рыба ни мясо»→«neither fish nor meat») |
| T05 | False cognate | «магазин»≠magazine, «симпатичный»≠sympathetic (только RU_EN, HE_EN) |
| T06 | RTL/script (иврит) | rtl_error: цифры без bidi-маркера, латин. кавычки вместо ивритских (EN_HE only) |
| T07 | Back-translation | back_translation_divergence (round-trip теряет смысл) |
| T08 | Length/completeness | omission (клауза выпала) / addition (добавлен хедж) |
| T09 | Named entity | wrong transliteration scheme (RU→EN=BGN/PCGN; HE→EN=LOC) |

**Type A-D (AUTHORING-GUIDANCE, доля батча):** A semantic ≥40% (сильный margin),
B morphological 20-30% (слабый margin — не раньше 8 semantic-пар), C register 15-25%,
D structural 10-15%. Target margin/пара >0.05. **Hard-гейты пары** (=те же 9, что в
скрипте): identity, mojibake, `[WRONG:]`-presence, negation-template, direction,
license, overlap>0.60(концептуально)/0.50(скрипт), язык chosen/a_side.
**Overlap-формула:** `shared=(chosen∩rejected)−a_side`, `total=(chosen∪rejected)−
a_side`, при <12 токенов гейт не применяется.

### Токенайзер + small-model rationale (ACTIVE)

**TOKENIZER-GUIDE:** BPE на 3 скрипта (Latin U+0020-024F, Cyrillic U+0400-04FF,
Hebrew U+0590-05FF + U+FB1D-FB4F). Coverage-цели (Phase-0 pre-condition, ДО любой
LoRA): EN ≥97%, RU ≥95%, **HE ≥90%**. Coverage = доля токенов НЕ разбитых в
`<byte_XX>`. Иврит: NFD-нормализация, 5 финальных букв (ך ם ן ף ץ) — отдельные
кодпойнты, gershayim/geresh — единые токены. Рус.: Ё≠Е не сливать, Ъ/Ь не отрезать.
Script-detect **потокенно, НЕ по предложению** (iPhone внутри рус. = Latin). Это
объясняет, ПОЧЕМУ HE_EN/EN_HE translator-потоков нет (пасс 3): иврит заблокирован
токенайзер-гейтом (`TOKENIZER_BLOCKED`) до расширения словаря.
**SMALL-MODEL-RATIONALE:** малая модель достаточна ТОЛЬКО потому, что задача
раздроблена (routing + adapters + topic-streams + 50-пар батчи + heldout после
каждого). DPO учит ГРАНИЦЫ предпочтений, не язык с нуля. Claim НЕ распространяется
на legal/medical/literary production. Требуемые доказательства: accepted router+
translator батчи, margin>0, non-regressive BLEU/chrF, LOOK-LIKE-отчёт, STREAM-STATE.

### Метод экзамена (EXAMINATION-METHOD, ACTIVE) и длина-контекст

Слои 0A(квалификация источника: registry BLOCK/REVIEW/QUARANTINE/ACCEPT + encoding +
язык) → 0B(дерево семейств: idiom→T04, false-cognate→T05, RTL→T06, round-trip→T07,
NE→T09, register→T02, morph→T03, omission→T08, default→T01; при совпадении берётся
самое специфичное) → 1(a_side дословно, без «Translate the following») → 2(chosen —
верифицированный parallel-corpus, НЕ raw-MT) → 3(rejected + `[WRONG:code — конкретный
токен]`) → 4(7 гейтов) → 5(pair-record). **Длина-фильтр (важно):** ≥4 токена; до
расширения токенайзера макс **15 HE / 20 RU** токенов-источника (ContextLen=64, без
merge каждый символ = 2 base-токена); после расширения — макс 32 (ContextLen/2).
256 — только для будущего длинного контекста. FM-TR-01..07: chosen из MT без ревью,
garbled rejected, vague `[WRONG:]`, ивр. mojibake, MT-артефакт как chosen, микс
регистров, один чекпоинт для обеих сторон round-trip.

### Runner-contract: точный список отсутствующего эвалуатора (ACTIVE — HARD-RESET + RUNBOOK)

**Missing evaluator = инференс-путь, который переводит heldout-источник и меряет
качество.** Конкретно в `RUNBOOK` шаблон `evaluate_translation.py` имеет
**незашитый вызов** `# hypotheses.append(translate(item["a_side"], checkpoint))` —
функция `translate()` НЕ подключена к `09.Inference/TextGenerator.cs` с загруженным
translation-LoRA чекпоинтом. Это и есть «отсутствующий runner», о котором пасс 2/3.
**Метрики, которые обязан выдать runner для ACCEPT-батча** (HARD-RESET «Actual
Metrics Required», LOOK-LIKE «Translation Metrics», ABLATION-RUNBOOK Step 5):
rows trained/held-out, elapsed, CPU avg/%, RAM delta/peak, training loss,
**DPO reward margin, DPO accuracy** (гейт ≥0.60), **BLEU, chrF** (sacrebleu),
target-script accuracy (≥98%), copied-source rate (≤2%), wrong-direction rate
(0 на L1, ≤2% L2-4), named-entity preservation, completeness score, avg latency.
Пока эвалуатора нет → режим **`PAIRSET_READY_TRAINING_RUNNER_MISSING`**,
`metric_mode:not_run`, метрики null, `RUNNER-CONTRACT.md` пишется, **«Do not invent
CPU/RAM/loss/BLEU/chrF»**. Запрещённые claims (HARD-RESET): «translation accuracy»/
«DPO accuracy»/«BLEU» когда прогон не запускался; вместо — `structural_gate_pass_rate`,
`pair_validation_pass_rate`, `source_license_state: SYNTHETIC_SEED`.

### Карта active vs superseded (для плана расширений)

| Док | Статус | Примечание |
|---|---|---|
| FOUR-LEVEL-TRAINING-PLAN-v1 | **ACTIVE** | операционный дизайн (называется «Current operational plan») |
| TOPIC-ROUTER-ORCHESTRATION-PLAN-v1 | **ACTIVE** | дизайн роутера |
| ROUTING-FLOW-DPO-GUIDANCE-v1 | **ACTIVE** (+источник confidence-артефакта) | шаблон роутерных пар |
| DPO-REGISTRY-GUIDE-v1 (T01-T09) | **ACTIVE** | таксономия + routing-addendum |
| DPO-PAIR-AUTHORING-GUIDANCE-v1 | **ACTIVE** | авторинг translator-пар, Type A-D |
| TOKENIZER-GUIDE-v1 | **ACTIVE** | Phase-0 pre-condition; объясняет HE-блок |
| SMALL-MODEL-RATIONALE-v1 | **ACTIVE** | обоснование |
| EXAMINATION-METHOD-v1 | **ACTIVE** | метод экзамена; ссылается на SOURCE-REGISTRY-v3 |
| LOOK-LIKE-RUN-REPORT-TEMPLATE-v1 | **ACTIVE** | шаблон отчёта на батч |
| 50-PAIR-ABLATION-RUNBOOK-v1 | **ACTIVE** | 50-пар stream-референс |
| TRANSLATION-TRAINING-PLAN-v1.1 | **ЧАСТИЧНО SUPERSEDED** | «retained as historical design context»; 16/24/32/64-ablation → заменён 50-пар stream; НО инфра (LoRA Rank=4/Alpha=8/Dropout=0.05, TranslationPromptBuilder, phase-гейты) — референс |
| RUNBOOK-v1 | **ЧАСТИЧНО SUPERSEDED** | «historical handoff»; исполнять через EXECUTOR-PROMPT; тело ещё ссылается на REGISTRY-v1 (устар.), но несёт validate/evaluate-скрипты |
| EXECUTOR-HARD-RESET-v1 | **ACTIVE** | role-lock, forbidden-claims, RUNNER_MISSING-режим |
| SOURCE-REGISTRY v1/v2 | **SUPERSEDED** → v3 | v3 — текущий |

### Итог пасса 4 (для P0 translation pilot)

1. **Root-cause подтверждён:** confidence/length-артефакты **не инжектит
   генератор** — он валидатор без гейтов против них; артефакт confidence заложен в
   ДИЗАЙН-шаблон роутерных пар (ROUTING-FLOW-DPO-GUIDANCE стр.51-73), артефакт длины
   — побочка синтетического авторинга EN→RU.
2. **Дизайн-слой ЗДОРОВ:** 4-level curriculum, T01-T09, router R1-R4, examination,
   tokenizer, small-model-rationale — согласованы и operational. Failure-rule даже
   ЯВНО ловит «BLEU падает при растущем margin» (дефект пасса 2).
3. **Три конкретные дыры для плана:** (a) генератору добавить leakage-гейт +
   length-skew гейт + поддержку роутерных структурных пар; (b) починить роутерный
   шаблон (убрать/уравнять confidence и evidence_terms) — иначе R1/R2 навсегда
   baseline-оракулы; (c) подключить `translate()` из `09.Inference/TextGenerator.cs`
   → это и закрывает «missing evaluator» (BLEU/chrF/script/copied-source), без
   которого нельзя заявлять «модель обучена».


---

## Pass 5/10 — runner-contract + executor-prompt + seed corpus + rejected + zip + Hebrew gap

Проход 5 из 10. Область (всё READ-ONLY): оставшиеся верхнеуровневые специфики
для «no-neglect» — (1) физический `RUNNER-CONTRACT.md`, (2) полное тело
`EXECUTOR-PROMPT-...-v1.md` (856 строк), (3) seed-корпус
`training-pairs-train-ready.jsonl` построчно, (4) `TRANSLATION-DPO-REJECTED-PROMPTS-v1.md`,
(5) `translation-runs.zip` (68 МБ) — список содержимого, (6) явное подтверждение
отсутствия HE_EN/EN_HE translator-потоков (роль папки по Luba — иврит→английский,
поэтому эта дыра критична). Записан только этот файл.

**Покрытые файлы:** `EXECUTOR-PROMPT-TRANSLATION-50-PAIR-LEARNING-v1.md` (856
строк, целиком); `translation-runs/translator-en-ru-general/batch-018/RUNNER-CONTRACT.md`
(90 строк, единственный физический экземпляр); `training-pairs-train-ready.jsonl`
(50 строк, целиком); `TRANSLATION-DPO-REJECTED-PROMPTS-v1.md` (309 строк,
целиком); `translation-runs.zip` (список, без распаковки); grep-подтверждение
Hebrew по всему дереву.

### 1. RUNNER-CONTRACT.md — точный интерфейс отсутствующего runner (HIGH)

**Важно:** физически `RUNNER-CONTRACT.md` существует в ЕДИНСТВЕННОМ месте —
`translation-runs/translator-en-ru-general/batch-018/`. Все остальные упоминания
(HARD-RESET, RUNBOOK, ABLATION-RUNBOOK, batch-001 DOCUMENT-USE-LOG, batch-018
NEXT-BATCH-DECISION) — это ссылки на обязанность его писать, а не сам контракт.
То есть точная спецификация runner зафиксирована ровно один раз — для EN→RU
batch-018 (самый зрелый артефакт, см. пасс 3). Это и есть тот контракт, который
должен реализовать «отсутствующий DPO-переводческий runner».

**Интерфейс (что runner обязан принять и выдать):**
- **Входные файлы:** `translation-train-50.jsonl` (50 DPO-пар train),
  `translation-heldout-50.jsonl` (50 held-out).
- **Обязательные поля строки:** `pair_id, source_text, chosen, rejected,
  defect_type` (7 значений: literal_translation/grammar_error/wrong_register/
  missing_idiom/false_friend/omission/wrong_word_choice), `direction (EN_RU),
  adapter_id (adapter-en-ru-general), translation_level (1)`.
- **Выходные метрики — per-batch training (14):** `rows_trained, rows_held_out,
  elapsed_time_seconds, train_time_seconds, evaluation_time_seconds, peak_ram_mb,
  average_ram_mb, average_cpu_percent, peak_cpu_percent, disk_written_mb,
  checkpoint_size_mb, training_loss, dpo_accuracy (0-1), dpo_reward_margin`
  (>0 = chosen>rejected).
- **Выходные метрики — per-batch translation quality (6):** `bleu` ИЛИ `chrf`
  (хотя бы одна, назвать primary), `target_script_accuracy` (доля строк в
  кириллице), `copied_source_rate` (доля выводов = источник дословно),
  `wrong_direction_rate` (доля выводов на английском вместо русского),
  `named_entity_preservation` (доля T09 с корректной сущностью),
  `completeness_score` (доля T08 с полным содержанием chosen).
- **Per-row held-out output (10 полей на строку):** `pair_id, source_text,
  reference(=chosen), model_output, bleu_score, chrf_score, script_correct(bool),
  direction_correct(bool), verdict(pass|fail), error_category(string|null)`. Это
  и есть недостающий **инференс-путь** — `model_output` требует реального
  `translate(source, checkpoint)` (тот самый незашитый вызов из RUNBOOK, пасс 4).
- **Выходные файлы:** обновить `translation-batch-summary.json` (metric_mode
  `not_run`→`measured`), заполнить `translation-heldout-evaluation.json`,
  обновить `LOOK-LIKE-RUN-REPORT.md`.

**Promotion Gate (строже, чем per-batch гейт 0.60 в других доках!):**
`dpo_reward_margin>0` И **`dpo_accuracy>=0.75`** (non-regressive) И
`target_script_accuracy>=0.95` И `copied_source_rate<=0.02` И
**`wrong_direction_rate==0.0`** И **`dotnet test >=1425 тестов, 0 провалов`
(Gate B)**. Плюс `TOKENIZER_CHECK_PENDING`: перед тренировкой проверить
кирилл. покрытие ≥95% инструментом; и C#-гейт `EnRuGeneral_Batch018_StructuralGatePasses`
должен пройти ДО старта тренировки. Notes: «Do not merge batch-019 into
batch-018 runner execution».
- **Тег:** `#runner-contract #14+6+10-метрик #gate-0.75-и-dotnet-test #единственный-экземпляр-b018`

### 2. EXECUTOR-PROMPT (856 строк) — операционная дисциплина одного 50-парного цикла (HIGH)

Полностью прочитан. Это точный SOP для ОДНОГО батча (не для потока целиком).
Ключевая структура — **обязательный 12-шаговый Document Use Protocol** (нельзя
читать доки «как плоскую папку»; каждый шаг пишет строку в `DOCUMENT-USE-LOG.md`,
иначе тренировка запрещена):

| Шаг | Док | Что извлечь |
|---|---|---|
| 0 | `translation-runs/` | активный поток, последний accepted-батч, вердикт последнего NEXT-BATCH-DECISION, последние метрики. Если прогонов нет → начать с `router-r1-direction/batch-001` |
| 1 | SMALL-MODEL-RATIONALE | мотивация; **НЕ использовать как гейт приёмки** |
| 2 | SOURCE-REGISTRY-**v3** | ACCEPT/SUPPORT/REVIEW/QUARANTINE/BLOCK; можно ли быть chosen-authority; **запрет отката на v1/v2** |
| 3 | TOPIC-ROUTER-ORCHESTRATION-PLAN | router-vs-translator, stream_id, метки, нужен ли routing-flow-гейт; **запрет static switch / filename classifier / language-code lookup** |
| 4 | ROUTING-FLOW-DPO-GUIDANCE | только для router-батчей: R1-R4, форма chosen, соседние rejected-маршруты |
| 5 | FOUR-LEVEL-TRAINING-PLAN | текущий уровень, микс пар, метрики; «один уровень ≠ 50 пар» |
| 6 | 50-PAIR-ABLATION-RUNBOOK | механика батча (dir-format, split, shape, гейты, decision); **не заменять старым RUNBOOK-v1** |
| 7 | DPO-REGISTRY-GUIDE | выбор семейства T01-T09; **не выдумывать новые семейства** |
| 8 | EXAMINATION-METHOD | построчная квалификация источника; провалившие строки НЕ добивают квоту 50 |
| 9 | REJECTED-PROMPTS | создать/аудит/починить rejected; отвергать random/identical/generic `[WRONG: bad]` |
| 10 | TOKENIZER-GUIDE | покрытие кир/иврит/лат; **если покрытие блокирует → стоп `TOKENIZER_BLOCKED`** |
| 11 | LOOK-LIKE-REPORT-TEMPLATE | человекочит. отчёт после eval |
| 12 | REGISTRY-v1/v2, TRAINING-PLAN-v1, RUNBOOK-v1 | historical, evidence-only; при конфликте побеждают текущие |

**Non-Negotiable Architecture Rule (стр.296-321):** роутер = ОБУЧАЕМАЯ
оркестрация `TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>`
(+ опц. `TranslationRoutingOrchestrator : ILogicalOrchestrator`). Явно запрещены:
hardcoded switch, filename classifier, language-code lookup, static source-
registry shortcut, direct adapter selection. (Прямой R4 из CLAUDE.md — совпадает.)

**Difficulty Ladder (стр.419-440) — прямо объясняет отсутствие иврита:** Router
ladder R1→R2→R3→R4; Translator ladder L1→L2→L3→L4. **Жёсткая последовательность:
«Do not run translator Level 1 until router R1 passes… Do not run he-en /
en-he… ».** Default First Sequence (стр.790-806): 1) router-r1 → 2) router-r2 →
5) **ru-en-general-l1** → 7) **en-ru-general-l1** → 8) **he-en-general-l1** →
9) **en-he-general-l1**. То есть Hebrew-переводчик стоит ШАГАМИ 8-9, ПОСЛЕ
завершения RU_EN и EN_RU Level-1 — которые так и не были «приняты на уровне
потока» (пасс 2: accepted_batches формально 17, но регресс на 200 парах; RU→EN
L1 стрим закончился REPAIR на batch-029). Поэтому очередь до иврита не дошла.

**Stop Conditions (стр.831-856):** стоп батча и запись
`REPAIR_BATCH|REJECT_BATCH|PAIRSET_READY_TRAINING_PENDING|PAIRSET_READY_TRAINING_RUNNER_MISSING`,
если: источник не ACCEPT; <50 чистых train/heldout; `chosen==rejected`; нет
`[WRONG:]`; mojibake; **margin стал отрицательным**; wrong-direction>2%;
copied-source>2%; **порча иврит-скрипта**; router false-low-risk≠0. «Never patch
a failed gate by adding more pairs in the same run.» И финал (стр.853): **«Do not
end with "awaiting your direction"»** при отсутствии runner — писать
pairset/ledger + `metric_mode:"not_run"` + точный next-action контракт. Это и
есть источник режима `PAIRSET_READY_TRAINING_RUNNER_MISSING`, который объясняет
пассы 2/3.
- **Тег:** `#executor-prompt-856 #12-шагов-doc-protocol #difficulty-ladder-иврит-шаги-8-9 #stop-conditions`

### 3. SEED-КОРПУС `training-pairs-train-ready.jsonl` (50 строк) — построчно (КРИТИЧНО)

Это тот самый верхнеуровневый seed (~50 КБ), который RAG-01 назвал
«отсутствующим на HEAD». **Прочитаны все 50 строк.**

**Что это по факту:** дамп train-ready пар **batch-029** (все id
`ru-en-general-l1-b029-001..050`). По пассу 2 **batch-029 — это ПОСЛЕДНИЙ,
ПРОВАЛИВШИЙСЯ батч** реального DPO-прогона (лучшая попытка v3 = 0.58 < гейт 0.60,
поток остановлен на REPAIR). Т.е. верхнеуровневый «seed» = выгрузка НЕ принятого
батча.

**Форма строки (реальные ключи):** `id, source:"ru-en-general", direction:"RU_EN",
pair_family:"general-adverb-translation", prompt:"Translate the following Russian
sentence to English:", a_side` (рус. предложение), `chosen` (англ.), `rejected,
validation_outcome:"train_ready", validation_errors:[]`. Все 50 —
`validation_outcome:train_ready`, 0 ошибок валидации.

**Однородность:** ВСЕ 50 пар — одно узкое семейство `general-adverb-translation`
(перевод оттенка одного русского наречия). Все `chosen` по одному шаблону
«He/She [adverb-ly] [verb-ed]…!» с восклицательным знаком.

**Качество — двойственное. Реальные ли это переводы?**
- `a_side` (русские предложения) — **настоящие, естественные, литературные**
  (не MT-мусор). `chosen` (английские) — **корректные, беглые, точные** переводы.
  Т.е. как «правильный ответ» — да, реальные качественные переводы.
- **НО `rejected` ДЕФЕКТЕН и воспроизводит reward-hack пасса 2.** `rejected` —
  это НЕ неверный перевод. Это англоязычное **мета-лингвистическое ЭССЕ на 50-80
  слов**, которое НАЧИНАЕТСЯ с `[WRONG: <quality>_lost — …]`, разбирает семантику
  наречия и заканчивается одним восклицаемым словом.

**Цитаты (реальные строки):**
- b029-001: a_side «Он бесстрашно двинулся через враждебную толпу…»; chosen «He
  intrepidly advanced through the hostile crowd, betraying not a trace of fear!»;
  rejected `[WRONG: fearless_quality_missed — «Бесстрашно» describes a form of
  advance… A formally calibrated English speaker would produce: 'Intrepidly!']`.
- b029-003: a_side «Он злобно смотрел на них из тени коридора…»; chosen «He looked
  at them balefully…!»; rejected `[WRONG: malevolence_register_lost — «Злобно»…
  'Balefully!']`.
- b029-029: a_side «Она набожно перекрестилась…»; chosen «She devoutly crossed
  herself…!»; rejected `[WRONG: religious_sincerity_lost — «Набожно»…
  'Devoutly!']`.

**Вердикт (для плана — центральный):**
1. Это **классический length/format-контраст**: chosen ≈ чистое 8-12-словное
   предложение; rejected ≈ 60-80-словное эссе. DPO учит «короткое чистое
   предложение хорошо, длинная простыня плоха» — дискриминатор длины/формата, НЕ
   верность перевода. Ровно дефект пасса 2, теперь подтверждённый на УРОВНЕ
   ИСХОДНЫХ ДАННЫХ верхнего seed.
2. Эти пары **нарушают собственные правила папки** (см. п.4): rejected обязан быть
   «одним правдоподобным неверным ПЕРЕВОДОМ» с одной первичной ошибкой, «Do not
   include explanation outside the rejected text», и hard-reject гейт 8 «too easy
   / only teaches bad-is-bad». Эссе = 100% объяснение, 0% перевод → под аудитом
   Prompt 3 это `HARD_REJECT_PAIR`. Но валидатор (`prepare_translation_learning_sets.py`,
   пасс 4) их пропустил — его 9 гейтов проверяют только chosen≠rejected,
   отсутствие mojibake, наличие `[WRONG:]`, direction, source, overlap<0.50;
   эссе-rejected все их проходит.
3. Источник `ru-en-general` = **синтетика** (не Tatoeba/gold).
4. **Прямой ответ на RAG-01 «seed отсутствует на HEAD»:** seed, физически лежащий
   на верхнем уровне этой папки — это ДЕФЕКТНЫЙ reward-hacked дамп провального
   batch-029, а НЕ чистый gold-parallel. **Его нельзя переносить в
   `27.TranslationDomain` как обучающий seed.** По-настоящему пригодные
   translator-данные — в потоках `translator-ru-en-general` (1000 пар) и
   `translator-en-ru-general` b018 (TR-S01 ACCEPT) (пасс 3: «годны»), а НЕ этот
   верхнеуровневый файл. Если домен обучали именно этим seed (или его 1 строкой),
   это согласуется с историей 0.3333/деградации.
- **Тег:** `#seed-корпус-b029-провальный #reward-hack-length-эссе #real-chosen-defective-rejected #не-переносить-в-домен`

### 4. TRANSLATION-DPO-REJECTED-PROMPTS-v1.md — что отвергается и почему (HIGH)

Библиотека copy-paste промптов для создания/аудита/починки rejected-стороны;
применяется И к routing-flow, И к translator DPO. 4 промпта: (1) создать
translator-rejected, (2) создать routing-flow-rejected, (3) **аудит**, (4)
починить слабый rejected.

**Что отвергается (Prompt 3 — Audit, «Hard reject» список из 8):**
1. `chosen == rejected`; 2. нет `[WRONG: ...]`; 3. random garbage / не на целевом
языке; 4. только пунктуация/регистр отличаются; 5. mojibake; 6. **более одной
первичной ошибки** в запутанном виде; 7. rejected случайно ЛУЧШЕ chosen;
8. **слишком лёгкий — учит лишь «плохой вывод плох»**.
**Accept только если:** rejected правдоподобен; ровно одна первичная ошибка;
`[WRONG:]` называет тип ошибки И функциональное последствие; модель может выучить
переносимую границу.

**Allowed error types (12):** wrong_direction, copied_source, mistranslation,
omission, addition, negation_error, morphology_error, register_shift,
idiom_calque, false_cognate, named_entity_corruption, rtl_or_script_error.

**Правила Prompt 1 (translator rejected), прямо нарушаемые seed-корпусом п.3:**
«Produce exactly one rejected **translation**»; «Introduce exactly one primary
error type»; «**Do not include explanation outside the rejected text**»; формат
`rejected: "<wrong translation>\n[WRONG: ...]"` — т.е. `[WRONG:]` ПРИКЛЕИВАЕТСЯ к
неверному переводу, а не заменяет его собой.

**Примеры good/bad (эталоны):** Good — omission («…approved the proposal.»
роняет budget-review clause), register_shift (legal→«pretty soon»), routing
wrong_adapter. Bad — «asdf table moon blue» (random), same-as-chosen (нет
сигнала), «Home going mine yesterday…» (слишком много ошибок сразу),
`[WRONG: omission]` без последствия. **Minimal Quality Rule:** пара хороша, если
ревьюер может дописать «chosen лучше, потому что rejected делает ошибку X, и если
модель выучит rejected, она провалится так Y».
**Связь с seed п.3:** seed-эссе не проходят ни один пример «good» и попадают под
bad-класс «too easy» + «explanation outside rejected text». Гайд ПРАВИЛЬНЫЙ —
seed его игнорирует. Для плана: аудит Prompt 3 нужно ВСТРОИТЬ в валидатор
(сейчас он не запускается на этапе генерации).
- **Тег:** `#rejected-8-hard-reject #12-error-types #minimal-quality-rule #seed-нарушает-этот-гайд`

### 5. translation-runs.zip (68 МБ) — бэкап, НЕ уникальный (LOW / артефакт)

Список содержимого прочитан без распаковки. **1131 файл в zip = 1131 файл в
живом дереве `translation-runs/`** (точное совпадение). Top-level в архиве —
ТОЛЬКО `translation-runs/` (нет ни одного из 22 верхнеуровневых гайдов/планов/
seed). Uncompressed 78.5 МБ → 68 МБ zip (данные уже плотные: JSON + бинарные
`.bin`). Дата zip 2026-05-29 (позже прогонов 2026-05-11/12). Структура (batch-001
dpo-run с checkpoints/policy_step_000005.bin 1 188 744 байт, bpe-tokenizer.json,
dpo-pairs-normalized.jsonl, progress.jsonl) идентична живому дереву (пасс 2).
**Вывод:** zip — обычный сжатый снимок/бэкап дерева прогонов, уникального
материала не несёт; для плана не читать построчно, трактовать как артефакт.
- **Тег:** `#zip-бэкап #1131=1131 #только-translation-runs #нет-уникального`

### 6. Подтверждение: HE_EN / EN_HE translator-потоки ОТСУТСТВУЮТ (важно — Luba назвала иврит)

Роль папки по Luba — перевод с иврита (и русского) на английский. Проверка дерева
даёт **однозначный вывод: настоящего Hebrew→English (и EN→HE) переводческого
обучения НЕТ ВООБЩЕ.**

**Доказательства (grep/find, read-only):**
- **Директории потоков** (`find translation-runs -maxdepth 2 -type d`): существуют
  только `2026-05-11/ru-en-general`, `router-r1-direction`, `router-r2-topic`,
  `translator-en-ru-general`, `translator-ru-en-general`. **НЕТ ни
  `he-en-general`, ни `en-he-general`, ни `2026-*/he-en-*`.**
- **`HE_EN`/`EN_HE` в jsonl** встречаются ТОЛЬКО в 6 router-файлах
  (`router-r1-direction/batch-001..003` и `router-r2-topic/batch-001..003`,
  train+heldout), по ~25 строк на файл — это метки НАПРАВЛЕНИЯ, которые роутер
  учится РАСПОЗНАВАТЬ, а не переводческие пары. Ни в одном translator-потоке
  (`translator-ru-en`, `translator-en-ru`, дата-прогон `ru-en-general`) их нет.
- **Реальный иврит-скрипт (`\p{Hebrew}`, U+0590-05FF) в train-файлах** найден
  РОВНО в 6 файлах — те же router-r1/r2 train-50 (иврит как ВХОД-источник для
  классификации направления/темы). В переводческих train-парах иврита нет.
- **Ноль checkpoints / dpo-run / policy-весов** для иврита: единственный REAL DPO
  каталог — `2026-05-11/ru-en-general/level-1` (пасс 2). Hebrew adapter не
  обучался ни разу.

**Причина отсутствия (из пасса 4 + пасс 5):** (a) TOKENIZER-GUIDE — Hebrew
coverage ≥90% это Phase-0 pre-condition; при непокрытии стрим стоит
`TOKENIZER_BLOCKED` (executor Step 10); (b) Difficulty Ladder executor-prompt
ставит `he-en-general-l1`/`en-he-general-l1` ШАГАМИ 8-9, ПОСЛЕ приёмки RU_EN и
EN_RU Level-1, которая не состоялась. Итог: **иврит-переводчик — чистая дыра
(0 пар, 0 прогонов, 0 чекпоинтов).** Для P0-пилота расширений, где Luba явно
назвала иврит→английский, это означает: capability иврита нужно строить с нуля
(токенайзер-расширение под иврит → ACCEPT-корпус HE-EN, напр. Tatoeba HE-EN /
ParaNames → router-гейт → he-en-general L1), и заявлять «поддержка иврита» сейчас
НЕЛЬЗЯ.
- **Тег:** `#иврит-полностью-отсутствует #только-router-labels #tokenizer-blocked #ladder-шаги-8-9 #дыра-P0`

### Что осталось для пассов 6-9 (no-neglect)

- Построчные тела translator-пар вне просэмплированных батчей (пасс 3 брал
  образцы): `translator-ru-en-general` b002-020, `translator-en-ru-general`
  b002-017 + остаток b018.
- Тела router R1/R2 b001-003 целиком (пасс 3 — образцы) для подтверждения
  confidence/evidence-count-утечки построчно.
- Полный `batch-summary.jsonl`/`progress.jsonl` по каждому из 58 dpo-run (пасс 2
  свёл `internal-...-summary.json`).
- Проход 10: матрица полноты всех 1154 файлов × статус + прямой перенос-план.

---
*RAG-03, проход 5/10. Evidence-bound: RUNNER-CONTRACT.md, EXECUTOR-PROMPT (856
строк), seed-корпус (50 строк) и REJECTED-PROMPTS прочитаны целиком; zip
пролистан без распаковки (1131=1131); отсутствие иврит-потоков подтверждено
find + grep + `\p{Hebrew}`. Записан только этот RAG-файл; всё прочее read-only;
git-write не запускался.*

## Pass 6/10 — source/license registries + usable-pair quality assessment

Проход 6 из 10. Область (всё READ-ONLY, записан только этот RAG-файл): (1) глубоко
прочитаны три реестра источников/лицензий — `TRANSLATION-SOURCE-REGISTRY-v3.md`
(каноничный, целиком), `-v2.md` и `-v1.md` (исторические, целиком); (2) глубже
просэмплированы два ГОДНЫХ переводческих потока пасса 3 —
`translator-ru-en-general` (1000 пар, все 20 батчей проанализированы на длину,
батч b012 прочитан построчно) и `translator-en-ru-general` b018 (50 train + 50
heldout прочитаны целиком) + агрегатный анализ длины по всем 900 EN→RU-парам;
(3) grep по всему дереву данных на физическое присутствие каждого реестрового
источника. Цель — легальный гейт (какой корпус вообще может обучать общую
модель) + реальный размер обучаемого актива.

### 1. Реестры источников — v3 каноничен, гейт ужесточался v1→v2→v3 (HIGH)

**Версионирование:** v3 (2026-05-11) — единственный действующий; v1 и v2 в шапке
жёстко проштампованы «Superseded … historical evidence only … OpenSubtitles is
not train-ready». То есть при конфликте лицензий **побеждает v3**; v1/v2 читать
только как историю.

**Дрейф решений (важно для юридической чистоты — гейт ЗАТЯГИВАЛСЯ, не ослаблялся):**
- **v1:** OpenSubtitles-2018 и ParaCrawl-v9 = ACCEPT; CCAligned = «CC0 claimed» →
  REVIEW; OpenSubtitles ACCEPT «spoken register». Самый мягкий гейт.
- **v2:** добавлены HF-датасет-ID; OpenSubtitles OPUS-версия → ACCEPT (CC-BY 4.0)
  с OpusFilter; CCAligned исправлен на Apache-2.0 ACCEPT; CCMatrix (CC-BY) ACCEPT
  с фильтром; opus-books ACCEPT для PD/CC-BY; Europarl удалён (RU не язык ЕС);
  TED → BLOCK (NC).
- **v3:** гейт СТРОЖЕ v2 — **OpenSubtitles ПОНИЖЕН до REVIEW_REQUIRED** («raw
  subtitles are studio/copyright text; OPUS availability is not enough»);
  **CCMatrix/CCAligned/ParaCrawl объединены в REVIEW_REQUIRED** (web-mined, mixed
  rights + MT-origin); введены новые состояния `ACCEPT_SUPPORT_ONLY`,
  `METHOD_ONLY`, `QUARANTINE_EVAL_ONLY` и пер-title/пер-component гейты.
  **Вывод для плана: доверять только v3; всё, что v1/v2 звали ACCEPT из
  web-mined/subtitles, в v3 уже НЕ train-ready.**

**Каноничная таблица v3 источник → лицензия → вердикт (юридический гейт):**

| ID | Источник | Лицензия | Вердикт v3 | Роль / что разрешено |
|---|---|---|---|---|
| **TR-S01** | **Tatoeba Parallel Sentences** | **CC-BY 2.0 FR (атрибуция обязательна)** | **ACCEPT** | Человеческий/community parallel-корпус; chosen-authority; RU-EN, HE-EN; семейства T01-T04, T09. **Единственный реальный корпус пилота.** |
| TR-S02 | ParaNames | data CC-BY 4.0 / код MIT | ACCEPT | ТОЛЬКО именованные сущности (T09), transliteration; НЕ авторитет предложений |
| TR-S03 | Knesset Hebrew Corpus | public-sector/open (сверить релиз) | ACCEPT_SUPPORT_ONLY | Монолингв. иврит: токенайзер, language-id, морфология; register `formal_spoken`; НЕ chosen-side |
| TR-S04 | UN Parallel Corpus | Public Domain (UN DGACM) | ACCEPT (ТОЛЬКО RU-EN/EN-RU `legal_diplomatic`) | Формальный дипломат./юр. корпус; **иврита НЕТ** |
| TR-Q01 | FLORES-200 | CC-BY-SA 4.0 | QUARANTINE_EVAL_ONLY | Только held-out BLEU/chrF; НЕ train |
| TR-Q02 | Wikipedia/Wiktionary/WikiMatrix | CC-BY-SA | QUARANTINE | Изолир. eval/лексический стресс; НЕ clean train |
| TR-R01 | OPUS OpenSubtitles | (raw = studio copyright) | REVIEW_REQUIRED | Нужны письм. доказ-ва лицензии + bicleaner (**понижен из ACCEPT в v2**) |
| TR-R02 | CCMatrix / CCAligned / ParaCrawl | web-mined, mixed | REVIEW_REQUIRED | Нужны per-release license + bicleaner + spot-check 100 строк ≥90% |
| TR-R03 | OPUS Books | mixed PD/CC-BY/CC-BY-SA | REVIEW_REQUIRED_PER_TITLE | Пер-title ledger; PD/CC-BY→ACCEPT, CC-BY-SA→QUARANTINE |
| TR-R04 | KDE/GNOME/Ubuntu UI-строки | per-component varies | REVIEW_REQUIRED | `software_ui` домен только после per-component лицензии |
| TR-R05 | Medical/Legal/Finance корпуса | mixed/unclear | REVIEW_REQUIRED | Только domain-router стресс после чистой реестровой записи |

**BLOCK (не для train и не для eval):** MIT OpenCourseWare (CC-BY-NC-SA),
Stack Exchange dump (terms risk), TED Talks (NC), MUSE CC-BY-NC, Bible-корпуса без
translation-level лицензии, CulturaX (CC-BY-NC), Russian National Corpus /RNC/
(research-only), OpenAssistant OASST (Apache-2.0, но instruction-following, НЕ
parallel — держать в orchestration-домене, не переносить в перевод).

**ACCEPT-permissive реальный корпус для пилота (то, чем пилот ДОЛЖЕН обучаться):**
`TR-S01` Tatoeba (предложения), `TR-S02` ParaNames (сущности T09), `TR-S04` UN
(RU/EN legal_diplomatic). First-Run Policy v3 (стр.177-189): первый DPO-прогон
может использовать ТОЛЬКО TR-S01 + TR-S02 + TR-S04; TR-S03 Knesset — лишь
токенайзер/иврит-support. Всё прочее — вне train-ready до записи точных лицензий.

**Что такое TR-S01 (кросс-линк с пассом 5):** `TR-S01` = **Tatoeba Parallel
Sentences, CC-BY 2.0 FR (атрибуция обязательна), ACCEPT** — человеческий/community
parallel-корпус, RU-EN и HE-EN. Пасс 5 нашёл, что b018 размечен `TR-S01`/`ACCEPT`
(Tatoeba EN-RU, topic `health_body`). **Это единственная строка данных во всей
папке, привязанная к реальному лицензированному корпусу** (см. п.3 ниже).

### 2. Качество ГОДНЫХ переводческих пар — реальный размер обучаемого актива (HIGH)

Пасс 3 назвал `translator-ru-en-general` и `translator-en-ru-general` b018
«годными». Пасс 6 копнул глубже: построчный b012 (RU→EN, тема «деньги/финансы»),
b018 целиком (train+heldout) и **количественный анализ длины по всем 1900 парам**.

**RU→EN поток (1000 пар, 20 батчей) — лучшие данные папки, подтверждено:**
- **chosen — реальные корректные, беглые, естественные английские переводы**
  (проверено построчно в b012). Не MT-мусор.
- **rejected — настоящие минимально-парные дистракторы** с реальным дефектом по
  7 типам (literal/grammar/false_friend/missing_idiom/omission/wrong_register/
  wrong_word_choice). Цитаты (b012):
  - `1214` false_friend: src «Акционер получил дивиденды.» → chosen «The
    shareholder received dividends.» / rejected «The **actioner** received
    dividends.» (акционер ≠ actioner).
  - `1218` false_friend: «Страховой полис…» → chosen «insurance **policy**» /
    rejected «insurance **police**».
  - `1210` missing_idiom: «Деньги счёт любят.» → chosen «Money likes to be
    counted.» / rejected «Money loves account.» (буквальщина).
  - `1203` literal: «У него нет сбережений.» → chosen «He has no savings.» /
    rejected «At him there are no savings.» (калька дательного).
  - `1216` omission: «…и они принесли хорошую прибыль.» → chosen с прибылью /
    rejected «He invested in stocks.» (второе предложение выброшено).
  - `1208` wrong_register: chosen «Make your payment by the end of the month.» /
    rejected «I respectfully request that you deposit the payment amount no later
    than the final day of the current calendar month.» (верный дефект регистра).
- **Анализ длины (ВСЕ 1000 пар):** rejected длиннее **343 (34%)** / короче
  **319 (31%)** / равно **338 (34%)** — **сбалансировано, БЕЗ односторонней
  length-хак-корреляции** на уровне потока.
- **Ключевая находка — length-сигнал внутренне ПРОТИВОРЕЧИВ по типам дефекта**
  (потому и не эксплуатируется length-only дискриминатором):
  - `omission`: rejected короче **91/100** (выброшена клауза → короче);
  - `wrong_register`: rejected длиннее **60/63** (многословное формальное эссе);
  - `literal_translation`: длиннее 104/173; `wrong_word_choice`: 70 длиннее /
    73 короче / 148 равно (симметрично).
  Direction для omission («предпочитай длинное») ПРОТИВОПОЛОЖЕН direction для
  wrong_register («предпочитай короткое») → **чистый length-хак невозможен
  поверх смешанного потока.** Это КОЛИЧЕСТВЕННО подтверждает пасс-3 «годны» и
  опровергает length-хак на масштабе (в отличие от seed-корпуса b029, пасс 5,
  где ВСЕ rejected — длинные эссе).
- **32/1000 (3.2%)** — verbose-essay rejected (rejected ≥2× длины chosen при
  chosen ≥4 слов); все это `wrong_register` — законный дефект регистра, но с
  сильнейшей length-корреляцией. Оставить, но при подключении runner добавить
  length-control по этому подтипу.
- **1 дефектная пара (удалить):** `TR-RU-EN-GEN-1544` (batch-015) —
  chosen == rejected («Folk customs are studied by ethnographers.»), помечена
  `wrong_word_choice`, но обе стороны идентичны → **нулевой DPO-сигнал**
  (нарушает hard-reject #1 гайда REJECTED-PROMPTS; валидатор пропустил, т.к.
  сравнивал поля так, что различие где-то в исходных полях ложно прошло).
  Реальный usable RU→EN = **999 well-formed пар**.
- **Оговорка источника:** ВСЕ 1000 пар — `source_id: original` /
  `source_license_state: original` = **СИНТЕТИКА** (самоавторские). «chosen
  верен» ничем ВНЕШНИМ не подтверждён; в потоке **0 heldout** (нет eval-сплита).

**EN→RU поток (900 train + 50 heldout, 18 батчей):**
- **b001-017 (850 пар):** плоский формат, `source_id: original` /
  `source_license_state: clean` = синтетика. Длина по всем 900: длиннее 310
  (34%) / короче 167 (18%) / равно 423 (47%) — умеренный перекос к
  rejected-длиннее (пасс 3 видел 66% в b001; на масштабе спал до 34%), высокая
  доля равнодлинных (47%) — хороший знак минимальных пар.
- **b018 (50 train + 50 heldout) — ЗРЕЛЫЙ, единственный лицензионно-привязанный:**
  `source_id: TR-S01` / `source_license_state: ACCEPT` = **реальный Tatoeba
  EN-RU health_body**. chosen — корректный разговорный русский, rejected —
  настоящие кальки/регистр/сущности. Цитаты:
  - `0001` literal: «I have a headache.» → chosen «У меня болит голова.» /
    rejected «Я имею боль головы.» (калька, нужен дательный безличный).
  - `0005` grammar: «My throat hurts.» → chosen «У меня болит горло.» /
    rejected «Моё горло болеет.» (болеет требует одушевл. субъект).
  - `0011` missing_idiom: «He's under the weather.» → chosen «Ему нездоровится.»
    / rejected «Он под погодой.» (не русская идиома).
  - `0042` T09 named-entity: «The World Health Organization…» → chosen
    «Всемирная организация здравоохранения…» / rejected «Мировая организация
    здоровья… репорт» (сломано устоявшееся имя ВОЗ).
  - `0048` mixed_language (негативный контроль): «I feel sick today.» → chosen
    «Мне сегодня плохо.» / rejected «Мне **sick today**.» (англ. слова не
    переведены — контроль на copied/target-script).
  - heldout `0002` literal: «She has a runny nose.» → chosen «У неё насморк.» /
    rejected «У неё бегущий нос.» (калька; насморк — лексикализованное слово).
- **b018 несёт негативные контроли, которых НЕТ в RU→EN:** T08 completeness
  (`0040`), T09 named-entity (`0041-0045`), `corruption_type:
  explanation_instead_of_translation` (`0047, 0050` — rejected = энциклопедическое
  эссе; **та же форма, что провальный seed b029 пасса 5, но здесь НАМЕРЕННЫЙ
  размеченный контроль**, учит отвергать объяснения-вместо-перевода),
  mixed_language (`0048`). Плюс `0046, 0049` несут `repair_note` «replaced copied
  English source with Russian variant; both sides now Cyrillic» → **copied_source
  дефект, который пасс 3 видел в `0046`, был ПОЧИНЕН** → b018 = очищенный,
  закалённый артефакт. Heldout b018 — ПОЛНОЦЕННЫЙ eval-сплит: несёт reference
  (`chosen`) + rejected + defect (в отличие от роутеров, где heldout был без
  chosen) → это и есть материал для BLEU/chrF по RUNNER-CONTRACT (пасс 5).
- **ПРОБЛЕМА разнообразия дефектов (EN→RU):** `wrong_word_choice` = **575/900
  (64%)** — поток сильно перекошен в один тип дефекта (в отличие от RU→EN, где
  распределение ровное: wwc 291, literal 173, grammar 164, idiom 130, omission
  100, false_friend 79, register 63). Низкое разнообразие ослабляет EN→RU
  DPO-сигнал; для плана — добить недостающие типы (особенно omission: всего 11).
- 0 идентичных пар, 1 verbose-essay. Real usable EN→RU = **900 well-formed**.

### 3. Реальный размер обучаемого актива — и юридический разрыв (КРИТИЧНО для «dpo legal build»)

**grep по всему дереву данных `translation-runs/` (source_id / license_state):**
- `TR-S01` = **102 строки** (b018 train 50 + heldout 50 + 2 в source-inventory);
  `ACCEPT` = 101. **Весь реальный лицензированный корпус папки = эти ~100 пар,
  все в b018, ОДНА тема (health_body), ОДНО направление (EN→RU).**
- `original`/`clean` (синтетика) = 1100+ строк; `synthetic-translator-seed`
  252; `synthetic-router-seed` 654.
- **Real DPO-прогон 2026-05-11 (пасс 2) обучался на `synthetic-seed-tatoeba-style`
  (751+400 строк) с `SYNTHETIC_SEED`** — т.е. даже батч, названный «в стиле
  Tatoeba», это ИМИТАЦИЯ, а НЕ лицензированный CC-BY корпус. «tatoeba-style» ≠
  Tatoeba.
- **TR-S02 (ParaNames), TR-S03 (Knesset), TR-S04 (UN) — НОЛЬ строк данных** в
  папке; встречаются только как ссылки в реестре и в DOCUMENT-USE-LOG прогона
  2026-05-11. Из 4 ACCEPT-источников физически материализован **только TR-S01**.

**Итоговый баланс обучаемого актива:**

| Категория | Пары (train) | Источник | Юр. статус |
|---|---|---:|---|
| RU→EN поток | 999 (1000 − 1 идентичная) | `original` (синтетика) | самоавторские, chosen не проверен внешним gold, 0 heldout |
| EN→RU b001-017 | 850 | `original`/`clean` (синтетика) | то же |
| **EN→RU b018 (train)** | **50** | **TR-S01 Tatoeba** | **ACCEPT / CC-BY 2.0, лицензионно чистый** |
| EN→RU b018 (heldout) | +50 | TR-S01 Tatoeba | ACCEPT, eval-сплит с reference |
| **Всего well-formed train** | **1899** | — | из них лицензионно-привязанных **50 (+50 heldout)** |

- **Well-formed DPO-пар всего: 1899 train** (999 RU→EN + 900 EN→RU) **+ 50
  heldout**. Дефекта пасса 2 (length/format-хак) на масштабе НЕТ.
- **НО легально-чистых, gold-привязанных = только 100 пар** (b018 TR-S01,
  одна тема health_body). Остальные **1849 — синтетика `original`**: годны как
  DPO-скелет, но «chosen верен» — самоутверждение, у RU→EN вообще нет heldout.
- **Прямой вывод для «dpo legal build» и P0-пилота:** чтобы масштабировать
  пилот ЛЕГАЛЬНО, тянуть НЕ из 1849 синтетических пар и НЕ из REVIEW_REQUIRED/
  BLOCKED, а из ACCEPT-permissive реестра: **TR-S01 Tatoeba (RU-EN ~700K,
  HE-EN ~20K), TR-S02 ParaNames (T09), TR-S04 UN (RU/EN legal_diplomatic)**.
  Сейчас из них материализован только TR-S01, и только 100 пар. Синтетический
  скелет (1849 пар) можно оставить как cold-start/ablation, но НЕ выдавать за
  лицензированный обученный актив, и НЕ переносить в `27.TranslationDomain` как
  «gold seed» (согласуется с пассом 5: верхний seed b029 — вообще reward-hacked,
  переносить нельзя).

### Что осталось для пассов 7-9 (no-neglect)

- Построчные тела router R1/R2 b001-003 (подтверждение confidence/evidence-count
  утечки количественно) — пасс 5 флагнул.
- `translation-heldout-evaluation.json` + `translation-sample-review.md` b018
  целиком (пасс 3 отметил ручной 9/10, деталь не сведена).
- Детальные `batch-summary.jsonl`/`progress.jsonl` по 58 dpo-run прогона
  2026-05-11 (пасс 2 свёл internal-summary).
- Явная проверка: почему seed прогона 2026-05-11 назван `synthetic-seed-tatoeba-
  style`, хотя реестр требует реального TR-S01 (нарушение EXAMINATION-METHOD?).
- Проход 10: матрица полноты 1154 файлов × статус + прямой transfer-план в
  `27.TranslationDomain`.

- **Тег:** `#реестр-v3-каноничен #гейт-затягивался-v1-v2-v3 #TR-S01=Tatoeba-CC-BY-2.0 #только-TR-S01-материализован #1899-well-formed-но-100-лицензионных #length-сигнал-противоречив-не-хак #RU-EN-1-идентичная-удалить #EN-RU-64%-wwc-мало-разнообразия #синтетика-не-gold`

---
*RAG-03, проход 6/10. Evidence-bound: v1/v2/v3 реестры прочитаны целиком;
translator RU→EN b012 построчно + анализ длины по всем 1000 парам; EN→RU b018
train+heldout целиком + анализ длины по всем 900 парам; grep source_id/
license_state по всему дереву. Числа (343/319/338; 91/100 omission; 60/63
register; 575/900 wwc; TR-S01=102 строки; TR-S02/03/04=0 строк) получены
скриптами Python на живых данных. Записан только этот RAG-файл; всё прочее
read-only; git-write не запускался.*


---

## Pass 7/10 — router leakage per-row + b018 heldout + synthetic-vs-licensed provenance

Проход 7 из 10. Цель — закрыть оставшиеся аналитические нити (no-neglect):
подтвердить утечку роутеров R1/R2 НА УРОВНЕ СТРОКИ (не по агрегатам), вскрыть,
что реально лежит в held-out эвалуации b018, и проследить провенанс
«синтетика vs лицензионный Tatoeba».

**Что читалось (READ-ONLY, построчно):**
- `router-r1-direction/batch-001`: `translation-router-train-50.jsonl` (строки
  0001-0006), `translation-router-heldout-50.jsonl` (HO-0001..2),
  `translation-heldout-evaluation.json` целиком.
- `router-r2-topic/batch-001`: `translation-router-train-50.jsonl` (0101-0106),
  `translation-router-heldout-50.jsonl` (HO-0101..2),
  `translation-heldout-evaluation.json` целиком.
- `translator-en-ru-general/batch-018`: `translation-heldout-evaluation.json`,
  `translation-sample-review.md`, `SOURCE-REGISTRATION-REQUIRED.md`,
  `translation-batch-summary.json`, `source-inventory.jsonl`,
  `translation-train-50.jsonl` (0001-0002), `training-pairs-repair-required.jsonl`,
  `NEXT-BATCH-DECISION.md` — целиком.
- `2026-05-11/ru-en-general/level-1/batch-001`: `training-pairs-train-ready.jsonl`
  (source-теги), `NEXT-BATCH-DECISION.md`, `dpo-run/ru-en-l1-batch001/progress.jsonl`
  + `batch-summary.jsonl` — целиком.
- `translation-runs/DOCUMENT-USE-LOG.md` — целиком.

---

### 1. Router R1 (direction) — утечка подтверждена ПОСТРОЧНО (2 канала)

Реальные строки train-набора b001 (цитаты сокращены до полей утечки):

- `TR-ROUTE-R1-0001` src «Привет, как дела?» → **chosen** conf **0.96**,
  evidence=`[cyrillic_script, target_language=en, short_general_sentence, greeting_pattern]` (**4 признака**);
  **rejected** conf **0.61**, evidence=`[target_language=ru]` (**1 признак**).
- `TR-ROUTE-R1-0002` → chosen **0.97** (3 evidence) / rejected **0.58** (1 evidence).
- `TR-ROUTE-R1-0003` → chosen **0.97** (3) / rejected **0.55** (`[no_explicit_source_lang]`).
- `0004`→0.96/0.60, `0005`→0.97/0.57, `0006`→0.97/0.59. По всему b001: chosen
  всегда **0.96-0.97**, rejected всегда **0.55-0.61** — ровно диапазон, который
  пасс 3 назвал по агрегату; теперь он подтверждён на каждой прочитанной строке.

**Два канала утечки на уровне строки:**
1. **Confidence-артефакт.** `confidence` chosen — синтетическая ~постоянная
   (~0.955; `heldout-evaluation.json` → `average_confidence: 0.955`), rejected —
   ~постоянная 0.55-0.61. DPO-тренер может выучить «предпочитай сторону с бОльшим
   числом в поле confidence» вместо логики направления.
2. **Evidence-count-артефакт.** chosen всегда 3-4 элемента `evidence_terms`,
   rejected 0-1. Ещё один шорткат «предпочитай длинный список evidence».

**Задача R1 детерминированная (не обучена).** `heldout-evaluation.json`
дословно: `"ACTUAL INFERENCE — TranslationRoutingFlow (deterministic R1
script-detection, no neural model) was executed on all 50 held-out rows ...
the deterministic flow serves as the oracle baseline."` Причём per-direction
поля называются `projected_correct` / `projected_direction_accuracy` — т.е.
«actual» 1.00 = проекция скрипт-логики, а НЕ прогон модели; `dpo_reward_margin`
по-прежнему `pending_neural_training`. Значит 1.00 доказывает, что скрипт-детектор
согласен сам с собой, а не обученный роутер.

**Формат heldout-строки** (подтверждён): только `expected_direction` +
`expected_adapter_id` + `script_class`, без chosen/rejected — eval-строки для
детерминированного оракула. Пример `TR-ROUTE-R1-HO-0001` src «Добрый день, чем
могу помочь?» → `expected_direction:RU_EN`, `script_class:cyrillic`.

---

### 2. Router R2 (topic) — циркулярная метрика + утечка подтверждены ПОСТРОЧНО

Реальные строки train-набора b001:

- `TR-ROUTE-R2-0101` src «Врач назначил мне антибиотики от инфекции.» →
  **chosen** topic=`medical` conf **0.92** evidence=`[врач, антибиотик, инфекц]`,
  `decision_reason:"Topic domain 'medical' detected from 3 keyword match(es);
  routing to adapter-ru-en-medical."`; **rejected** topic=`general` conf **0.55**
  evidence=`[]` (**пусто**), `decision_reason:"No specialised domain keywords
  detected; defaulting to general topic domain."`
- `TR-ROUTE-R2-0104` src «Адвокат изучил условия контракта…» → chosen `legal`
  **0.97** evidence=`[адвокат, контракт, нарушени]` / rejected `general` **0.55**
  evidence=`[]`.
- `0102`→medical 0.92/technical 0.45, `0103`→legal 0.85/general 0.55,
  `0105`/`0106`→technical 0.97/general 0.55. chosen 0.85-0.97, rejected 0.45-0.55.

**Три дефекта на уровне строки:**
1. **Циркулярная метрика (keyword-матчер судит keyword-матчер).** Метка `chosen`
   ПОРОЖДЕНА keyword-матчером (`"detected from N keyword match(es)"`), а held-out
   accuracy СУДИТСЯ тем же keyword-flow. `heldout-evaluation.json`:
   `topic_accuracy 1.00`, `correct 50/50`, `gate_status ACCEPTED`,
   `inference_test:"TranslationTopicRoutingFlowTests.R2Batch001_HeldOut50_TopicAccuracyMeetsGate"`.
   1.00 тавтологично: правило само расставило метки и само их оценило.
2. **Confidence-артефакт** (как в R1): chosen ≥0.85, rejected ≤0.55.
3. **Empty-vs-populated evidence.** rejected.evidence_terms **ВСЕГДА `[]`**, chosen
   всегда 2-3 keyword'а. Резче, чем в R1 (там rejected иногда имеет 1 термин; в R2
   — всегда пусто). Модель может выучить «предпочитай непустой evidence_terms».

**Heldout R2** несёт `chosen.topic_domain` (gold-метку) прямо в строке
(`HO-0101` «Моя температура поднялась…» → chosen topic=`medical`) — ту самую
метку, которую те же keyword-правила и расставили бы.

**Вывод для плана (R1/R2 as-is НЕ обучаемые семантические юниты):** это
детерминированные keyword/script-flow, чьи поля `confidence`/`evidence_terms`/
`decision_reason` ЛИКАЮТ ответ. Если R1/R2 нужны как обучаемый `ILogicalUnit`:
(a) убрать `confidence`+`evidence_terms`+`decision_reason` из ВХОДА модели —
это и есть утечка; (b) R1 честнее объявить детерминированным script-детектор-
оракулом (baseline), а не «обученным роутером»; (c) R2 нужен НЕЗАВИСИМЫЙ gold
(не выход того же keyword-матчера), иначе метрика циркулярна. Согласуется с
CLAUDE.md R4 (`NO HIDDEN DETERMINISM`) и allowed-levels-гейтом.

---

### 3. b018 held-out evaluation — что реально внутри (структурно, не модель)

- `translation-heldout-evaluation.json` — **СТРУКТУРНО-ПУСТ**: `metric_mode:"not_run"`,
  `status:"PAIRSET_READY_TRAINING_RUNNER_MISSING"`, все 9 метрик
  (`dpo_reward_margin/dpo_accuracy/bleu/chrf/target_script_accuracy/
  copied_source_rate/wrong_direction_rate/named_entity_preservation/
  completeness_score`) = `null`. Дословная note:
  `"DPO training runner required before metrics can be measured. Do not invent
  metric values."` → НИКАКОГО вывода модели, НИКАКОЙ метрики (как и весь
  pairset-режим).
- `translation-sample-review.md` — единственный содержательный контент =
  **РУЧНАЯ инспекция человеком** 10 held-out строк (0001,0006,…,0046). Реальная
  оценка КАЧЕСТВА перевода: **9/10 полный pass**, 1 annotation-repair (row 0036:
  текст аннотации ссылается на nausea/photosensitivity вместо sneezing/feverish;
  контент верный, обучение не блокирует). Пример строки: `source: I have a
  stomachache. / chosen: У меня болит живот. / rejected: Я имею боль живота. /
  defect: literal_translation → Chosen PASS (dative-impersonal) … Rejected PASS
  (clear calque; contrast sharp and teachable)`. Пары действительно зрелые EN→RU
  (dative-impersonal chosen vs калька rejected, source-in-prompt, длина
  сбалансирована) — самый качественный pairset папки. **НО** это ЧЕЛОВЕЧЕСКАЯ
  ручная оценка, а не метрика модели.

Итог: b018 held-out НЕ содержит реального прогона/метрики — только `null`-скелет
+ ручной обзор. Тренер по b018 не запускался.

---

### 4. Синтетика vs лицензионный TR-S01 — core legal-vs-reality gap (ДВА слоя)

**Слой (a) — REAL-DPO прогон 2026-05-11: честный SYNTHETIC_SEED.**
Каждая train-пара помечена `source_id:"synthetic-seed-tatoeba-style"`,
`source_license_state:"SYNTHETIC_SEED"` (строка `TR-RU_EN-L1-T01-0001` «Мне
двадцать лет.» → «I am twenty years old.»). Реестр v3 и `DOCUMENT-USE-LOG.md`
ОБЪЯВЛЯЮТ авторитетом first-run именно Tatoeba TR-S01 (лог дословно:
`"First-run sources: TR-S01 Tatoeba (RU-EN, HE-EN) and reverse directions"`), но
фактические данные — сентенции, написанные ВРУЧНУЮ «в стиле» Tatoeba, а не
извлечение из Tatoeba. Сам `NEXT-BATCH-DECISION.md` batch-001 рекомендовал
(дословно): `"Consider sourcing real Tatoeba RU-EN sentence pairs (ACCEPT
licence) to improve chosen-side authority."` — но за все 29 батчей это так и не
сделано. **Почему не подключён:** `SYNTHETIC_SEED` — санкционированная лазейка
«missing corpus ≠ stop» из `TRANSLATION-EXECUTOR-HARD-RESET-v1.md`; реальный
Tatoeba требует download + Layer 0-A квалификацию + per-row `source_locator`
(sentence id), инфраструктура ингеста которого не построена → исполнитель
продолжал генерировать синтетику. Тег `synthetic-seed-tatoeba-style` — **ЧЕСТНЫЙ**:
он НЕ заявляет ACCEPT.

**Слой (b) — pairset b018 (translator-en-ru): тег ПЕРЕКЛЕЕН на ACCEPT без
извлечения.** Противоречие ВНУТРИ одной папки:
- Тела пар + `source-inventory.jsonl` + `translation-batch-summary.json` +
  `NEXT-BATCH-DECISION.md` заявляют `source_id:"TR-S01"`,
  `source_license_state:"ACCEPT"`, `license:"CC-BY 2.0 FR"`, `row_count_used:45`,
  `source_authority: TR-S01 ACCEPT for all 50 pairs`.
- `SOURCE-REGISTRATION-REQUIRED.md` (та же папка, та же дата) прямо говорит:
  `"This batch uses source_id: synthetic-seed-en-ru-v1 with source_license_state:
  SYNTHETIC_SEED. All 50 train pairs and 50 held-out pairs carry quality_status:
  source_registration_required."` + `"Pairs CANNOT be promoted to train_ready
  until the source is registered as ACCEPT or replaced with a registered ACCEPT
  corpus extract (e.g., Tatoeba TR-S01)."` + батчи 001-017 использовали
  незарегистрированный `source_id:"original"`, чей `gate_result: ACCEPTED`
  «incorrectly recorded».
- **Решающий тэлл:** тот же doc перечисляет `source_locator: tatoeba sentence id
  per row` как ОБЯЗАТЕЛЬНОЕ поле для настоящей замены на TR-S01. Реальные строки
  пар b018 `source_locator` **НЕ несут** (ключи строки 0001: `pair_id, pair_family,
  source_text, chosen, rejected, defect_type/annotation, source_id, …` — locator
  отсутствует). Значит метка `TR-S01/ACCEPT` **ЗАЯВЛЕНА, а не доказана**. Данные —
  шаблонные health/body-сентенции («I have a headache.»→«У меня болит голова.»,
  «She caught a cold.»→«Она простудилась.») ПЕРЕКЛЕЕННЫЕ как Tatoeba, а не
  извлечённые из Tatoeba.

**Net для плана:** НИ ОДИН учебный набор этой папки — ни REAL-DPO прогон
2026-05-11, ни pairset-стримы — не является проверенным лицензионным
извлечением. Прогон 2026-05-11 честно `SYNTHETIC_SEED`; b018 — синтетика,
переклеенная в `ACCEPT` (провенанс-оверклейм, ОТЛИЧНЫЙ от честности 2026-05-11).
Реестровый TR-S01 Tatoeba (CC-BY 2.0 FR) полностью очищен «на бумаге», но
материализован как **0 реальных строк с locator** (пасс 6 нашёл grep TR-S01=102
строки — это ровно эти переклеенные/декларированные строки b018, не извлечения).
Это и есть центральный legal-vs-reality разрыв: расширениям нельзя заявлять
«обучено на лицензионном Tatoeba», пока не построен ингест Tatoeba с per-row
locator и не переизвлечены пары.

---

### 5. Оставшиеся run-detail файлы охарактеризованы (item 4)

- `progress.jsonl` (2026-05-11 batch-001) — настоящий событийный лог. `start`
  несёт `SourcePath` + `SourceSha256=DD6AC099…`, `PairCount:50`, `BatchSize:10`,
  `ContextLen:128`, `BpeVocab:2048`, `StartedAt:2026-05-11T21:30:03+03:00`; 5×
  `batch_complete` с per-batch `Loss/Accuracy/RewardMargin` + wall-clock; `complete`
  `TotalElapsedMs:14197` (~14 c). Реальные метки времени и sha — подлинный лог.
- `batch-summary.jsonl` — добавляет per-batch `ChosenReward/RejectedReward/
  CpuPercent/WorkingSetDeltaMb/CheckpointBytesDelta`. Решающая деталь:
  батчи 1-4 пишут `CheckpointBytesDelta` 129/127/124/129 (крохи — метрический
  JSON), **батч 5 пишет 1 188 873 байт** = единственный `policy_step_000005.bin`
  ~1.16 МБ. Подтверждает пасс 2: реальные веса материализуются ТОЛЬКО на финальном
  шаге.
- **Телеметрия Loss:** Loss держится 0.685-0.695 по всем 5 батчам
  (ln 2 = 0.693 = неинформированный baseline). DPO-loss почти не спускается ниже
  случайного — подкрепляет вывод «дискриминатор длины/стиля, а не обучение
  переводу»: целевая функция фактически плоская.
- `translation-runs/DOCUMENT-USE-LOG.md` (корень, цикл `router-r1-direction/
  batch-001`) — ledger порядка чтения 11 управляющих доков перед первой парой;
  ОБЪЯВЛЯЕТ first-run авторитет TR-S01 Tatoeba, Knesset support-only, длины
  HE≤15/RU≤20 слов, узел R1 = `TranslationDirectionRouterUnit`. Именно этот doc
  ДЕКЛАРИРУЕТ авторитет Tatoeba, который фактические данные так и не соблюли.

---

### Что осталось для пассов 8-9

- **Пасс 8:** `translator-ru-en-general` (20 батчей, пасс 3 назвал «чистым»:
  source-in-prompt, семантические дистракторы, длина сбалансирована) — построчная
  проверка source-тегов (`synthetic-translator-seed`?) и есть ли хоть один реальный
  `source_locator`; сверка чистоты/длины по всему стриму (пасс 6 — частично).
- **Пасс 9:** верхнеуровневый `training-pairs-train-ready.jsonl` (50 пар, выгрузка
  b029) построчно + прямая сверка с `27.TranslationDomain`: какой именно seed
  «отсутствует на HEAD», даёт ли он accuracy 0.3333 при обучении на 1 строке.
- **Пасс 10:** матрица полноты 1154 файла × статус + прямой transfer-план в
  `27.TranslationDomain` (что подключать/переизвлекать, чтобы продолжить пилот).

- **Тег:** `#R1-R2-утечка-построчно #confidence-артефакт-0.96-vs-0.55 #evidence-count-утечка #R2-циркулярная-keyword-матчер #b018-heldout-структурно-null #b018-ручной-9из10 #2026-05-11-честный-SYNTHETIC_SEED #b018-переклеен-в-ACCEPT-без-locator #TR-S01=0-реальных-извлечений #Loss-0.693-плоский #веса-только-на-финальном-шаге`

---
*RAG-03, проход 7/10. Evidence-bound: router R1/R2 b001 train+heldout+heldout-
evaluation прочитаны построчно (цитаты реальных строк 0001-0006 / 0101-0106);
b018 heldout-evaluation/sample-review/SOURCE-REGISTRATION-REQUIRED/batch-summary/
source-inventory/train/repair/NEXT-BATCH прочитаны целиком; 2026-05-11 batch-001
train-ready source-тег + NEXT-BATCH + dpo-run progress.jsonl + batch-summary.jsonl
+ translation-runs/DOCUMENT-USE-LOG.md прочитаны целиком. Записан только этот
RAG-файл; всё прочее read-only; git-write не запускался.*


---

## Pass 8/10 — ru-en provenance + remaining-file sweep + net trainable/legal picture

Проход 8 из 10. Цель — закрыть провенанс на ГЛАВНОМ годном стриме
(`translator-ru-en-general`, 1000 пар) ПОСТРОЧНО по всем 20 батчам, вычистить
последние неохарактеризованные файл-группы по всему дереву 1154 (no-neglect) и
свести чистый обучаемый+легальный актив папки. Всё READ-ONLY, кроме этого
RAG-файла; git-write не запускался.

**Метод (что реально исполнено этим проходом):** Python-скрипты на живых данных
прочитали ВСЕ 1000 строк 20 батчей `translation-pairs-train-50.jsonl`
(ключи/source_id/source_license_state/locator/defect_type — по каждой строке);
прочитаны `translation-pairs-validation.json` b001 и b020, `STREAM-STATE.json`
b001 и b020, `source-inventory.jsonl` b001-005, `translation-batch-summary.json`
b001; выполнен tree-census всех 1154 файлов (по расширению/basename/singletons);
tree-wide census `source_license_state`+locator по ВСЕМ *.jsonl с парами (5747
пар-строк); охарактеризованы последние singleton-файлы
(`training-pairs-combined-100/150/200.jsonl`, mis-named repair-файл).

### 1. Провенанс `translator-ru-en-general` (1000 пар) — построчно по 20 батчам

**Verified-licensed count для этого стрима = 0. Ожидание задания (0 real, всё
синтетика) ПОДТВЕРЖДЕНО и уточнено.** Grep по всем 20 train-файлам:
`"ACCEPT"` = **0 строк**, `TR-S0` (реестровые S01/S02/S04) = **0 строк**,
`locator` = **0 строк**. Ни одной привязки к лицензированному корпусу; ни одного
`source_locator`/tatoeba-id ни в одной из 1000 строк.

**Уточнение к пассу 6 (тот сказал по агрегату «ВСЕ 1000 = source_id: original»
— это НЕТОЧНО).** Реально в стриме **три синтетические схемы провенанса по трём
батч-полосам** (все 50 пар/батч, все синтетические, ни одна не лицензирована):

| Батчи | Пар | `source_id` | `source_license_state` | id-диапазон |
|---|---:|---|---|---|
| **b001-005** | 250 | `synthetic-translator-seed` (один общий) | `SYNTHETIC_SEED` | TR-RU-EN-GEN-0101..0550 |
| **b006-015** | 500 | `SRC-RU-EN-GEN-NNNN` (уникальный per-row) | `original` | TR-RU-EN-GEN-0601..1550 |
| **b016-020** | 250 | `original` | `clean` | TR-RU-EN-GEN-1601..2050 |

- **Тонкий тэлл (важно для плана):** средняя полоса b006-015 несёт per-row
  `source_id: SRC-RU-EN-GEN-0601..1550` — это ВЫГЛЯДИТ как построчный источник,
  но каждый id уникален, само-референтен (зеркалит номер пары `TR-RU-EN-GEN-06xx`)
  и НЕ резолвится ни в какой реестровый корпус. Это «наряд провенанса» без
  извлечения — не путать с реальным locator. `source_license_state:"original"`
  здесь = синтетика, не лицензия.
- **Полный набор ключей строки (по всем 1000, стабилен):** `id, domain,
  contract_signature, direction, source, chosen_translation, rejected_translation,
  defect_type, defect_description, source_id, source_license_state, batch_index`.
  Locator-подобного ключа (`*locator*`/`sentence_id`/`tatoeba`) **НЕТ ни одного**.
  (Пасс 3 назвал формат «плоским простым» из 5 полей — на деле полей 12, но
  провенанс-поля всё равно синтетические.)
- **Схемы служебных файлов ДРЕЙФУЮТ внутри одного стрима** (признак
  инкрементальной ручной генерации, не единого пайплайна): `validation.json` b001
  многострочный (`pairs_evaluated/structural_valid/gate_status`) → b020 компактный
  (`sample_size/valid_pairs/gate_result`); `STREAM-STATE` b001 многословный →
  b020 компактный. `source-inventory.jsonl` существует только у b001-005 (b001-002
  = 1 строка общего synthetic-seed; b003-005 = 50 per-row синтетических),
  у b006-020 source-inventory ОТСУТСТВУЕТ вовсе.
- **Валидация — ТОЛЬКО структурная (подтверждено b001+b020):**
  `pairs_evaluated:20` из 50, `structural_accuracy:1.00`, `gate:0.99`, ACCEPTED;
  `inference_test` = имя СТРУКТУРНОГО теста
  (`TranslatorStreamBatchTests.RuEnGeneral_Batch0NN_StructuralGatePasses`), НЕ
  прогон модели. `STREAM-STATE` b020: `stream_complete:true`,
  `cumulative_train_pairs:1000`, `next_action:stream_complete`. «1000 ACCEPTED»
  = 1000 well-formed пар, НЕ обученный перевод (как и весь pairset-режим, пасс 3/16).
- **Независимое подтверждение дефектной пары пасса 6:** ровно **1** пара
  `chosen==rejected` во всём стриме — `TR-RU-EN-GEN-1544` (batch-015,
  defect `wrong_word_choice`, обе стороны = «Folk customs are studied by
  ethnographers.») → нулевой DPO-сигнал, удалить. Пустых chosen/rejected — 0.
  **Real usable RU→EN = 999 well-formed пар** (подтверждает пасс 6 точным числом).
- Distribution дефектов (все 1000, ровное — лучшая диверсификация в папке):
  wrong_word_choice 291, literal 173, grammar 164, missing_idiom 130, omission
  100, false_friend 79, wrong_register 63.

**Вердикт стрима:** данные годны как DPO-скелет (source-in-prompt, семантические
дистракторы, длина сбалансирована — пассы 3/6), но **провенанс на 100%
синтетический и на 100% без locator**; verified-licensed = 0. Ни одна из 3 полос
не ближе к легальному Tatoeba, чем другая — «original»/«clean» звучат чище
«SYNTHETIC_SEED», но это тот же синтетический материал без извлечения.

### 2. Sweep оставшихся файлов по всему дереву 1154 (no-neglect) — закрыт

**Census всех 1154 файлов по расширению:** `.json` 546, `.jsonl` 369, `.md` 178,
`.bin` 59, `.py` 1, `.zip` 1 = **1154 ✓**. Крупные повторяющиеся basename-группы
(STREAM-STATE×69, batch-summary/dpo-pairs-normalized/progress/dpo_step_000001 ×59,
internal-train-ready-dpo-summary ×58, bpe-tokenizer ×56, policy_step_000005.bin
×51, LOOK-LIKE-RUN-REPORT/NEXT-BATCH-DECISION ×45 и т.д.) — все уже
охарактеризованы схемами пассов 1-2 (рамка батча + dpo-run/checkpoints). Ни одной
неучтённой массовой группы.

**Special-группы tree-wide (точные числа, поimённо):**
- `DOCUMENT-USE-LOG.md` = **ровно 3**: корень `translation-runs/` (пасс 7),
  `2026-05-11/.../batch-001` (пасс 5/7), `translator-en-ru-general/batch-018`
  (пасс 5). **В стриме `translator-ru-en-general` DOCUMENT-USE-LOG ОТСУТСТВУЕТ.**
- `SOURCE-REGISTRATION-REQUIRED.md` = **ровно 1**, только `en-ru/batch-018` (пасс 7).
  **Важный no-neglect факт:** честная записка «это синтетика, не зарегистрирован
  ACCEPT» существует в ЕДИНСТВЕННОМ месте (en-ru b018). Стрим ru-en (1000
  синтетических пар) такой записки НЕ несёт — то есть его синтетичность нигде
  внутри стрима не задекларирована (в отличие от en-ru b018 и от честного
  `SYNTHETIC_SEED`-тега прогона 2026-05-11).
- `RUNNER-CONTRACT.md` = **ровно 1**, только `en-ru/batch-018` (пасс 5) — единственная
  спецификация отсутствующего инференс-эвалуатора во всей папке.
- `HARD-RESET` = 1 (top-level, пасс 1/4); `RUNBOOK`-файлы = 2 (ABLATION + RUNBOOK,
  top-level, пассы 1/4/5). Остатков в дереве прогонов нет.

**Впервые охарактеризованные singleton-файлы (были в manifest'е пасса 1, но не
разобраны):**
- `training-pairs-combined-100.jsonl` (100 строк, `2026-05-11/.../batch-002`),
  `training-pairs-combined-150.jsonl` (150, batch-005),
  `training-pairs-combined-200.jsonl` (200, batch-005) — это **входы
  кумулятивного эксперимента** пасса 2 (конкатенация 2/3/4 батчей по 50). Именно
  на combined-200 модель дала регресс acc 0.29/margin −0.0152. Это НЕ новые пары,
  а склейки уже учтённых → отдельный обучаемый актив не добавляют.
- `translation-training-pairs-repair-required.jsonl` (`router-r2-topic/batch-003`)
  — **0 строк** (пустой), mis-named вариант обычного `training-pairs-repair-
  required.jsonl` (лишний префикс `translation-`). Benign опечатка имени, не
  данные.

**Итог sweep: все 1154 файла отнесены к охарактеризованной группе. Неохваченных
файл-групп не осталось.** Manifest-группы пасса 1 (A: 23 top-level; B: 5 стримов
+ дата-прогон) все затронуты.

### 3. Чистый обучаемый + легальный актив папки (свод для P0-пилота)

**Tree-wide census `source_license_state` по ВСЕМ парам-строкам (5747 строк,
включая дубли train/normalized/combined/heldout):** `(нет поля)` 2150,
`SYNTHETIC_SEED` 1897, `clean` 1100, `original` 500, **`ACCEPT` 100**.
**rows с locator-полем tree-wide = 0.** Единственный каталог с `ACCEPT`-строками
во всём дереве — `translator-en-ru-general/batch-018` (train 50 + heldout 50).

| Актив | Пар (train) | Источник/лицензия | Что это на самом деле |
|---|---:|---|---|
| RU→EN стрим (b001-020) | **999** (1000 − 1 идентичная) | синтетика (SYNTHETIC_SEED/original/clean), **0 locator** | лучший DPO-скелет папки; годен, но самоавторский, 0 heldout, тренер не запускался |
| EN→RU b001-017 | 850 | синтетика (original/clean) | DPO-скелет; 64% wwc (мало диверсификации) |
| **EN→RU b018 (train)** | **50** | тег `TR-S01/ACCEPT/CC-BY 2.0` — **но БЕЗ locator** | провенанс-оверклейм: «ACCEPT» заявлен, не доказан (пасс 7) |
| EN→RU b018 (heldout) | +50 | то же | единственный eval-сплит с reference; тоже без locator |
| **Итог well-formed train** | **1899** | — | **verified-licensed (с реальным извлечением) = 0** |

**Чистый вывод (для «dpo legal build» и продолжения расширений):**
1. **Обучаемый актив (структурно годный DPO-скелет):** ~**1899 well-formed пар**
   (999 RU→EN + 900 EN→RU) + 50 heldout. Свободны от reward-hack пасса 2 (длина
   сбалансирована, дистракторы семантические). Это РЕАЛЬНАЯ ценность папки как
   cold-start/ablation-скелет.
2. **Легальный (verified-licensed) актив = 0 пар.** Ни одной строки с реальным
   `source_locator`/извлечением из ACCEPT-корпуса во всём дереве 1154. 100
   `ACCEPT`-тегов en-ru b018 — переклеенная синтетика без locator (пасс 7); прогон
   2026-05-11 честно `SYNTHETIC_SEED`; ru-en стрим — синтетика без даже honesty-note.
   «tatoeba-style» ≠ Tatoeba.
3. **Отсутствующая инфра (блокеры пилота, неизменны с пассов 2/5):**
   (a) **ингест Tatoeba/ParaNames/UN с per-row locator** — не построен → 0
   лицензированных извлечений; (b) **инференс-эвалуатор `translate()`** (BLEU/chrF/
   target_script/copied_source/…, спец. в `RUNNER-CONTRACT.md`) — `metric_mode:
   not_run`, не реализован; (c) **DPO-runner** реально работал только на
   2026-05-11 и был reward-hacked (Loss≈ln2=0.693 плоский). Иврит (HE_EN/EN_HE)
   translator-стримов НЕТ вовсе (пасс 5).

**Для `27.TranslationDomain` (пилот RAG-01):** переносить как «gold seed» НЕЛЬЗЯ
ничего из этой папки — всё либо reward-hacked (seed b029, пасс 5), либо синтетика
без locator. Легально масштабировать пилот = построить ингест ACCEPT-реестра
(TR-S01 Tatoeba RU-EN/HE-EN, TR-S02 ParaNames, TR-S04 UN) с per-row locator и
переизвлечь пары; синтетический скелет (1899) оставить только как cold-start,
явно помеченный не-лицензированным.

### Что осталось для пассов 9-10

- **Пасс 9:** верхнеуровневый `training-pairs-train-ready.jsonl` (50 пар, выгрузка
  b029) построчно (пасс 5 назвал его reward-hacked — все rejected длинные эссе) +
  прямая сверка с `27.TranslationDomain`: какой именно seed «отсутствует на HEAD»,
  даёт ли он accuracy 0.3333 при обучении на 1 строке; и почему seed прогона
  2026-05-11 назван `synthetic-seed-tatoeba-style` вопреки EXAMINATION-METHOD.
- **Пасс 10:** матрица полноты 1154 файла × статус + executive summary + прямой
  transfer-план в `27.TranslationDomain` (что подключать/переизвлекать/строить,
  чтобы продолжить перевод-пилот в расширениях VS / VS Code).

- **Тег:** `#ru-en-provenance-0-locator #verified-licensed=0 #3-синтетические-полосы-b001-005-b006-015-b016-020 #SRC-RU-EN-GEN-наряд-провенанса-без-извлечения #ACCEPT-только-en-ru-b018-без-locator #tree-wide-locator=0 #1899-well-formed-но-0-легальных #1544-идентичная-подтверждена #combined-100-150-200-это-склейки #SOURCE-REGISTRATION-только-b018 #схемы-validation-STREAM-STATE-дрейфуют`

---
*RAG-03, проход 8/10. Evidence-bound: все 1000 строк 20 батчей ru-en прочитаны
Python-скриптом (ключи/source_id/license_state/locator/defect по строке);
validation.json+STREAM-STATE b001/b020, source-inventory b001-005,
batch-summary b001 прочитаны; tree-census 1154 файлов (расширения/basename/
singletons) + tree-wide census license_state/locator по 5747 пар-строкам
исполнены на живых данных. Числа (verified-licensed=0; ACCEPT=100 только en-ru
b018; locator=0 tree-wide; identical=1 TR-RU-EN-GEN-1544; combined 100/150/200;
DOCUMENT-USE-LOG=3; SOURCE-REGISTRATION-REQUIRED=1; RUNNER-CONTRACT=1) получены
скриптами. Записан только этот RAG-файл; всё прочее read-only; git-write не
запускался.*


---

## Pass 9/10 — synthesis: design-vs-reality, usable asset, cross-folder bridges, open questions

Проход 9 из 10 — **синтез** (не новое индексирование). Пассы 1-8 полностью
проиндексировали 1154 файла; этот проход сводит ОДНУ действенную картину папки #3
для плана «продолжить расширения VS / VS Code», строит мосты к остальным 11
папкам свипа и формулирует открытые вопросы Любе. Всё READ-ONLY, кроме этого
RAG-файла; git-write не запускался. Кросс-источники этого прохода прочитаны
как evidence: `RAG-01-compare-branches.md`, `RAG-02-orchestrator-extension.md`
(та же папка свипа) и физические папки-мосты `external-sources`, `dpo legal
build`, `design to code session`, `orchestrator experiment` (листинги + grep).

### 1. Folder-3 синтез — одна картина для плана перевода (P0)

Роль папки в целевом флоу (по Luba, RAG-01): **перевести промпт пользователя с
другого языка (например иврита) на английский ПЕРЕД оркестрацией.** Т.е. это
верхний, UPSTREAM-узел: `иностранный промпт → [translate→EN] → оркестратор
(Node1 Splitter …)`. Вот что папка реально даёт этому узлу, слоями «дизайн ↔
реальность»:

**(a) ДИЗАЙН (сильный, полный, правдоподобный).** Папка несёт зрелую
архитектуру перевода:
- **4-уровневый обучаемый роутер** `TranslationRoutingFlow : ILogicalFlow` (+
  опц. `TranslationRoutingOrchestrator`), 6 внутренних `ILogicalUnit`-узлов,
  R1 direction → R2 topic → R3 register/risk → R4 adapter; темы D01-D06
  (пассы 1/3). Явно запрещён hardcode-классификатор (совпадает с R4
  `NO HIDDEN DETERMINISM` в CLAUDE.md).
- **Таксономия пар T01-T09** (core/register/morphology/idiom/false-cognate/
  RTL-иврит/back-translation/completeness/named-entity), 4 механизма, форма
  DPO-пары, хард-гейты, overlap-гейт (пассы 1/4).
- **RUNNER-CONTRACT** (пасс 5): точный интерфейс отсутствующего тренера —
  14 train-метрик + 6 quality-метрик (bleu/chrf/target_script/copied_source/
  wrong_direction/named_entity/completeness) + 10 полей per-row held-out; per-row
  требует настоящий `translate(source, checkpoint)`. Единственный физический
  экземпляр — `translator-en-ru-general/batch-018/`.
- Инфра-требования: мультискрипт-BPE (кир/иврит/лат, покрытие ≥95/90/97), LoRA
  Rank4/Alpha8, `TranslationPromptBuilder`, изоляция от оркестрации (пасс 1).
- **Вывод по дизайну:** для плана это **готовый архитектурный чертёж** capability
  перевода — переиспользуемый как есть; ничего доизобретать не надо.

**(b) РЕАЛЬНОСТЬ (обучение НЕ доказано — honest-untrained-scaffold — потолок).**
- **Единственный настоящий DPO-прогон** = `2026-05-11/ru-en-general/level-1`
  (29 батчей, 58 dpo-run, 59 бинарных чекпоинтов). Он **reward-hacked**:
  Loss держится 0.685-0.695 ≈ ln2=0.693 (плоская, неинформированная цель, пасс 7);
  accuracy скачет 0.00↔1.00 между соседними батчами (дисперсия крошечной модели,
  не обучение); на накопленных 200 парах — **регресс** acc 0.29 / margin −0.0152
  (пасс 2). Модель вырожденная: `hidden_dim=64, num_layers=1, ContextLen=128,
  VocabSize=2048`, вес ровно 1 188 744 байт (~1.16 МБ). «17 принятых / 850 пар»
  = сумма прошедших per-batch гейт попыток, а НЕ модель, обученная на 850 парах.
- Все «новые» стримы (router-r1/r2, translator-en-ru 18, translator-ru-en 20 =
  1000 пар) — режим **PAIRSET-ONLY** (`metric_mode:not_run`,
  `PAIRSET_READY_TRAINING_RUNNER_MISSING`): `structural_accuracy:1.0` = ГОТОВНОСТЬ
  НАБОРА, тренер по ним не запускался вовсе (пассы 3/5/16).
- Роутеры R1/R2 (as-is) — **детерминированные keyword/script-flow с утечкой**:
  confidence-артефакт (chosen ~0.96 vs rejected ~0.55), evidence-count-артефакт
  (chosen 3-4 термина vs rejected 0-1), R2 циркулярна (keyword-матчер сам
  расставил метки и сам их оценил → «accuracy 1.00» тавтологична). Подтверждено
  построчно (пасс 7). Это НЕ обученные семантические юниты.
- **Прямая связь с `27.TranslationDomain` (пилот RAG-01):** доменный
  runner-report `accuracy 0.3333` / `reward_margin 0.03` — это ОТДЕЛЬНОЕ,
  унаследованное байт-в-байт из `main`, 1-строчно-обученное число (RAG-01: blob
  идентичен origin/main; `RewardMargin:0.03` захардкожен в `TranslationMachine.cs:70`
  и `:187`). Реальная механика обучения этой папки (0.58/0.29, margin ≈0)
  СКРОМНЕЕ и НЕ равна 0.3333 — но обе истории сходятся в одном: **capability
  перевода как ОБУЧЕННАЯ не доказана.** Потолок папки = честный необученный
  скелет (`PASS_AS_HONEST_UNTRAINED_SCAFFOLD`, не `PASS_AS_TRAINED_FEATURE`).

**(c) ГОДНЫЙ cold-start-скелет (реальная ценность, с ловушками).**
- **~1899 well-formed DPO-пар** = 999 RU→EN (1000 − 1 идентичная
  `TR-RU-EN-GEN-1544`) + 900 EN→RU, + 50 heldout (пассы 6/8). Свободны от
  reward-hack пасса 2 на масштабе: длина сбалансирована (RU→EN: длиннее 34% /
  короче 31% / равно 34%), length-сигнал внутренне ПРОТИВОРЕЧИВ по типам дефекта
  (omission→короче 91/100 ПРОТИВ register→длиннее 60/63 ⇒ чистый length-хак
  невозможен), дистракторы семантические (false_friend «actioner»≠«акционер»,
  калька «Я имею боль головы» и т.п.).
- **Ловушки, которые план ОБЯЗАН учесть:**
  1. **Верхнеуровневый seed `training-pairs-train-ready.jsonl` (50 пар) — НЕ
     переносить.** Это дамп ПРОВАЛЬНОГО batch-029 (лучшая попытка 0.58 < гейт
     0.60); все rejected — 50-80-словные мета-эссе `[WRONG:…]` = классический
     length/format-хак, нарушает собственный REJECTED-PROMPTS гайд (пасс 5). Это
     ровно тот «seed, отсутствующий на HEAD» из RAG-01 — и переносить его в
     `27.TranslationDomain` как gold НЕЛЬЗЯ.
  2. **EN→RU перекошен:** `wrong_word_choice` = 575/900 (64%), omission всего 11.
     Слабая диверсификация → добить недостающие типы (пасс 6).
  3. **b018 (50+50) — самый зрелый, но провенанс-оверклейм:** размечен
     `TR-S01/ACCEPT/CC-BY 2.0`, но БЕЗ per-row `source_locator`; тот же батч несёт
     `SOURCE-REGISTRATION-REQUIRED.md` «это синтетика, не зарегистрирован ACCEPT»
     (пасс 7). Метка ACCEPT **заявлена, не доказана.**

**(d) САМЫЙ БОЛЬШОЙ разрыв — нет лицензированного корпуса И нет `translate()`
эвалуатора.**
- **Verified-licensed актив = 0 пар** во всём дереве 1154: `locator`-поле = 0
  строк tree-wide (5747 пар-строк); из 4 ACCEPT-источников реестра физически
  материализован только TR-S01, и то как 100 переклеенных/декларированных строк
  b018 БЕЗ извлечения (пасс 8). TR-S02/S03/S04 = 0 строк. `synthetic-seed-
  tatoeba-style` ≠ Tatoeba.
- **Отсутствующая инфра (блокеры пилота):** (i) ингест Tatoeba/ParaNames/UN с
  per-row locator — не построен; (ii) инференс-эвалуатор `translate()`
  (`metric_mode:not_run`) — не реализован; (iii) не-хакнутый DPO-runner — реально
  работал лишь на 2026-05-11 и был reward-hacked.

**(e) Иврит-дыра (Luba явно назвала иврит — а его нет).** HE_EN/EN_HE
translator-потоков НЕТ ВООБЩЕ: 0 пар, 0 dpo-run, 0 чекпоинтов (пасс 5). Иврит
встречается только как 6 router-файлов, где он — ВХОД для классификации
направления, не переводческая пара. Причина: TOKENIZER_BLOCKED (Hebrew coverage
≥90% — Phase-0 pre-condition) + Difficulty-Ladder ставит `he-en-general-l1`
шагами 8-9, ПОСЛЕ приёмки RU_EN/EN_RU L1, которая не состоялась. **Для P0
capability иврит→английский нужно строить с нуля** (иврит-BPE → ACCEPT HE-EN
корпус (Tatoeba HE-EN / ParaNames) → router-гейт → he-en L1); заявлять
«поддержка иврита» сейчас НЕЛЬЗЯ.

**(f) Связь с целевым шагом флоу «перевести иностранный промпт в английский
ДО оркестрации».** Этот узел — UPSTREAM оркестратора (RAG-02 Node1 Splitter).
Папка #3 даёт: (i) готовый дизайн роутера+адаптеров+RUNNER-CONTRACT; (ii) годный
RU→EN/EN→RU DPO-скелет для cold-start; НО (iii) 0 обученной модели, 0
лицензированного корпуса, 0 иврита. Значит в плане расширений этот шаг сегодня =
**честный необученный scaffold** (детерминированный script-детектор направления
как baseline-оракул + PAIRSET готов), а НЕ обученный переводчик. Расширение может
показать «маршрутизацию и заготовку», но не «перевод обучен на лицензии».

### 2. Cross-folder bridges (что папка #3 берёт у / даёт каждой)

| Мост (папка/RAG) | Что папка #3 ДАЁТ | Что папка #3 БЕРЁТ / чего ей не хватает оттуда |
|---|---|---|
| **`compare branches` / RAG-01** (пилот `27.TranslationDomain`, 0.3333 / R-19) | Реальную механику обучения (0.58/0.29, margin≈0) и правду «seed b029 — reward-hacked, не переносить»; годный 1899-скелет как кандидат для восстановления поведения ВНУТРИ `TranslationMachine`. | Точку приземления: `27.TranslationDomain/TranslationMachine.cs` (`ILearningMachine<TranslationDomain>`), morph-стек (`FindNearestCascade` `TranslationUnitContracts.cs:241`, KNN `:335`), lifecycle-хост `27.TranslationDomain.Tools/Program.cs` + `models/translation/translation-v1/`. RAG-01 фиксирует: восстановленное поведение = trained `ILogicalUnit` ВНУТРИ machine, НЕ standalone `TranslationRoutingFlow`. 0.3333 пометить NOT_PROVEN baseline; measurement two-PASS (AccuracyAfter>Before). |
| **`dpo legal build`** (легальный DPO-билдер + license-gate; `active-codex-execution-*`, guide-01 how-to-build-dpo, guide-02 train, guide-03 measure, 04 legal-model-policy, TeacherProvenance) | Каноничный **SOURCE-REGISTRY-v3** (ACCEPT Tatoeba/ParaNames/UN; REVIEW web-mined/subtitles; BLOCK NC) + метод EXAMINATION (Layer 0-A..5) + честный урок «tatoeba-style ≠ Tatoeba, provenance-оверклейм b018» — прямой вход для license-gate/TeacherProvenance-enforcement. | Настоящий legal-DPO-pipeline и per-row `source_locator`/провенанс-enforcement, которого у папки #3 НЕТ (locator=0). Именно `dpo legal build` — место, где license-gate должен ОТКЛОНИТЬ синтетику-без-locator и потребовать реальное извлечение. |
| **`external-sources`** (лицензированные корпуса — живут ли Tatoeba/UN там?) | — | **ПРОВЕРЕНО: НЕТ.** `external-sources` — реестр КОДОВЫХ/software-eng DPO-источников (source-examinations, open-source-knowledge-domain-map). Tatoeba фигурирует ровно как кандидат-строка: «Group 16 sources (Tatoeba, Common Voice) now in plan; **no rows yet**». То есть лицензированный NL-корпус перевода **не материализован НИГДЕ по свипу**, не только в папке #3. Разрыв «нет лицензии» — универсальный. |
| **`design to code session`** (по заданию — «рабочий DPO-runner/evaluator»?) | Папка #3 — живой пример для их honesty-таксономии: reward-hacked прогон = `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`, НЕ trained. | **Уточнение (честно):** это НЕ переводческий runner. Это scaffold→trained-feature honesty/verification-сессия (`claude-SCAFFOLD-TO-TRAINED-FEATURE-PLAN`, `claude-NOT-IMPLEMENTED-MAP-10pass`, `VERIFICATION-is-task-implemented`). Даёт папке #3 **дисциплину классификации** «scaffold vs trained feature» и two-PASS-гейт. Переиспользуемого `translate()`-эвалуатора там я НЕ нашёл (runner-упоминания — про design-conversion домен / в NOT-IMPLEMENTED-картах). Для рабочего DPO-runner-прецедента смотреть RUNNER-CONTRACT (папка #3) + local-wiki `ProjectWikiTrainingRunner/Program.cs` (RAG-01). |
| **`orchestrator experiment`** (translator/result-translator раунды E53; CB4 Node-02 Translator / Node-06 Result-Translator evidence) | Явное разграничение: «translator» оркестратора — это ВНУТРЕННИЕ узлы (Node2 Domain Translator: фрагмент→формальный statement; Node6 Result Translator: результат→человекочит. сводка), в RAG-02 оба **вырождены/не реализованы** (Node2 «фрагмент дословно», Node6 «echo-truncate»). | Ничего для NL-перевода. **Критично не путать:** capability папки #3 = кросс-язычный NL-перевод (RU/HE→EN) ПЕРЕД оркестрацией; «Translator»-узлы оркестратора = внутридоменный перевод задачи ВНУТРИ пайплайна. Это разные вещи; NL-перевод папки #3 сидит UPSTREAM Node1 Splitter, а не заменяет Node2/Node6. |

**Сводный мост-вывод:** капабилити перевода питается из трёх контуров —
`dpo legal build` (license-gate + legal pipeline, куда идёт REGISTRY-v3),
`27.TranslationDomain`/RAG-01 (код-хост, куда приземляется обученный юнит),
`design to code session` (honesty-классификация scaffold-vs-trained). Ни
`external-sources`, ни `orchestrator experiment` НЕ дают лицензированного
NL-корпуса перевода — его нет во всём свипе.

### 3. Открытые вопросы Любе (нумерованные, с рекомендуемыми дефолтами)

**Q1. Строить реальный ингест Tatoeba (+ParaNames/UN) с per-row locator — или
оставить синтетический cold-start?**
Рекоменд. дефолт: **построить минимальный ингест TR-S01 Tatoeba RU-EN** (ACCEPT,
CC-BY 2.0, атрибуция) с per-row `source_locator`=sentence-id; синтетический
1899-скелет оставить ТОЛЬКО как явно помеченный cold-start/ablation. Причина:
verified-licensed=0 сегодня; без locator расширение не может честно заявить
«обучено на лицензии». Это работа для `dpo legal build` (license-gate).

**Q2. Реализовать `translate()` инференс-эвалуатор по RUNNER-CONTRACT?**
Рекоменд. дефолт: **да, это следующий разблокирующий шаг** — без него все
метрики `metric_mode:not_run` и capability недоказуема. Контракт уже написан
(b018): 14+6 метрик + per-row held-out; нужен реальный `translate(source,
checkpoint)`. До этого честный статус — «scaffold, не trained feature».

**Q3. Приоритет иврит→английский — сейчас или после RU-EN?**
Рекоменд. дефолт: **отдельным явным треком ПОСЛЕ первого доказанного RU-EN
цикла**, т.к. иврит = 0 пар / 0 токенайзер-покрытия (TOKENIZER_BLOCKED) и по
Difficulty-Ladder идёт шагами 8-9. НО: Luba явно назвала иврит — если иврит есть
жёсткое требование P0, поднять его в отдельный параллельный трек «иврит-BPE →
Tatoeba HE-EN ingest → router-гейт → he-en L1» и НЕ скрывать, что сегодня это
0%. Нужно решение Luba: RU-EN-first (дефолт) vs Hebrew-параллельно.

**Q4. Что делать с верхнеуровневым seed b029 (50 пар) и меткой ACCEPT на b018?**
Рекоменд. дефолт: **b029-seed НЕ переносить** (reward-hacked, нарушает свой же
гайд) — карантин/удаление; **метку `TR-S01/ACCEPT` на b018 понизить** до
SYNTHETIC/registration_required, пока нет locator (как честно требует его же
`SOURCE-REGISTRATION-REQUIRED.md`). Иначе провенанс-оверклейм протечёт в план.

**Q5. Какой годный стрим промотировать первым?**
Рекоменд. дефолт: **RU→EN 999-парный стрим** (лучшая диверсификация дефектов:
wwc 291 / literal 173 / grammar 164 / idiom 130 / omission 100 / false_friend 79
/ register 63; length-хак опровергнут количественно). Сначала удалить 1
идентичную пару `TR-RU-EN-GEN-1544`. EN→RU (64% wwc) — вторым, после добора
недостающих типов. Оба — как cold-start-скелет ПОД `translate()`-эвалуатор, не
как «обученный лицензированный актив».

**Q6. Роутеры R1/R2 — объявить детерминированным baseline-оракулом или
переучить как ILogicalUnit?**
Рекоменд. дефолт: **R1 (direction) оставить детерминированным script-детектором,
помеченным baseline-оракулом** (он честно так и работает); **R2 (topic) — либо
переучить как ILogicalUnit с НЕЗАВИСИМЫМ gold (не выход того же keyword-матчера)
и убрать из входа confidence/evidence_terms/decision_reason (это утечка), либо
тоже пометить baseline.** Без этого метрика 1.00 циркулярна и нарушает R4
`NO HIDDEN DETERMINISM`.

- **Тег:** `#синтез-P0 #дизайн-полон-обучение-не-доказано #honest-untrained-scaffold-потолок #1899-скелет-годен #seed-b029-не-переносить #verified-licensed=0-по-всему-свипу #иврит-0-строить-с-нуля #translate-эвалуатор-следующий-шаг #мост-dpo-legal-build-license-gate #мост-27.TranslationDomain-хост #external-sources-Tatoeba-no-rows-yet #orchestrator-translator≠NL-перевод #6-открытых-вопросов`

---
*RAG-03, проход 9/10 (синтез). Evidence-bound: сведены находки пассов 1-8 (числа
перепроверены по уже записанным секциям этого файла — Loss≈0.693, регресс
acc0.29/margin−0.0152, 1899 well-formed, locator=0 tree-wide, ACCEPT=100 только
b018, иврит=0 потоков, seed=b029-провальный). Кросс-мосты получены чтением
`RAG-01`/`RAG-02` (та же папка свипа) + листингом/grep физических папок
`external-sources` (Tatoeba «no rows yet»), `dpo legal build` (active-codex-
execution-* + guide-01..05 + legal-model-policy), `design to code session`
(scaffold→trained-feature honesty-сессия, не translate-runner), `orchestrator
experiment` (CB4 Node-02/Node-06 translator-узлы — внутридоменные, не NL). Записан
только этот RAG-файл; всё прочее read-only; git-write не запускался.*


---

## Pass 10/10 — completeness sweep + executive summary (folder #3/12 language-translate-learning)

Проход 10 из 10 — **финальный**: свод полноты (все 1154 файла × статус, без
неглекта) + **executive summary** (чтобы Luba могла прочитать ТОЛЬКО эту секцию) +
честный отчёт о покрытии. Не новое индексирование — сверка и синтез уже записанных
пассов 1-9. Всё READ-ONLY, кроме этого RAG-файла; ни build, ни test, ни training,
ни inference НЕ запускались; git-write не запускался.

### 0. Независимая верификация census (пасс 10 перепроверил своими командами)

`find`-census живой папки (read-only), сверен с манифестом пасса 1 и tree-census
пасса 8 — **совпал байт-в-байт**:

| Метрика | Пасс 1/8 заявил | Пасс 10 перепроверил (`find`) | Совпало |
|---|---|---|---|
| Всего файлов | 1154 | **1154** | ✓ |
| Всего папок | 233 | **233** | ✓ |
| `.json` / `.jsonl` / `.md` / `.bin` / `.py` / `.zip` | 546/369/178/59/1/1 | **546/369/178/59/1/1** | ✓ |
| Верхний уровень (файлов) | 23 | **23** (18 .md + 1 .py + 3 .jsonl + 1 .zip) | ✓ |
| `translation-runs/` корень | 1 (DOCUMENT-USE-LOG) | **1** | ✓ |
| router-r1 / router-r2 | 32 / 33 | **32 / 33** | ✓ |
| translator-en-ru / translator-ru-en | 116 / 125 | **116 / 125** | ✓ |
| дата-прогон `2026-05-11` | 824 | **824** | ✓ |
| Сумма | 1154 | 23+1+32+33+116+125+824 = **1154** | ✓ |
| `policy_step_000005.bin` | ×51 | **×51** | ✓ |
| ВСЕ `.bin` (веса) | 59, все 1 188 744 Б | **59, все ровно 1 188 744 Б, идентичны** | ✓ |
| `STREAM-STATE.json` | ×69 | **×69** | ✓ |
| `DOCUMENT-USE-LOG.md` / `RUNNER-CONTRACT.md` / `SOURCE-REGISTRATION-REQUIRED.md` | 3 / 1 / 1 | **3 / 1 / 1** | ✓ |

Никакого дрейфа. Индекс пассов 1-9 воспроизводим.

### 1. Матрица полноты — все 1154 файла отнесены к покрытой группе (no-neglect)

| Группа | Файлов | Как покрыто | Пассы | Статус |
|---|---:|---|---|---|
| **A1.** 18 верхних гайдов/планов/реестров (.md) | 18 | Прочитаны **ЦЕЛИКОМ** построчно (тела всех планов, T01-T09, R1-R4, реестры v1/v2/v3, токенайзер, examination, hard-reset) | 1,4,5,6 | **COMPLETE** |
| **A2.** гейт-скрипт `prepare_translation_learning_sets.py` | 1 | Прочитан **ЦЕЛИКОМ** (330 строк, тело `validate_pair`, 9 гейтов, таблицы источников) | 4 | **COMPLETE** |
| **A3.** seed `training-pairs-train-ready.jsonl` (дамп b029) | 1 | Прочитан **ЦЕЛИКОМ построчно** (50/50 пар, цитаты) | 5,9 | **COMPLETE** |
| **A4.** `training-pairs-repair-required.jsonl` / `-blocked.jsonl` | 2 | Подтверждено **0 строк** (пустые) | 1 | **COMPLETE** |
| **A5.** `translation-runs.zip` (~68 МБ) | 1 | Листинг **без распаковки**; 1131=1131 зеркало живого дерева, уникального контента нет | 5 | **COMPLETE (артефакт-бэкап)** |
| **B0.** `translation-runs/DOCUMENT-USE-LOG.md` (корень) | 1 | Прочитан **ЦЕЛИКОМ** (ledger 11 доков, декларация авторитета Tatoeba) | 7 | **COMPLETE** |
| **B1.** router-r1-direction (3 батча) | 32 | b001 train+heldout+`heldout-evaluation` **построчно** (строки 0001-0006 цитаты); b002/003 — state/summary/eval + образцы пар | 3,7 | **COMPLETE** (b001 deep, b002/003 pattern) |
| **B2.** router-r2-topic (3 батча) | 33 | b001 train+heldout+eval **построчно** (0101-0106 цитаты); b002/003 — state/summary/eval | 3,7 | **COMPLETE** (b001 deep, b002/003 pattern) |
| **B3.** translator-en-ru-general (18 батчей) | 116 | b018 **ЦЕЛИКОМ** (train50+heldout50+RUNNER-CONTRACT+SOURCE-REGISTRATION+sample-review+все control-doc); b001-017 — **length-census всех 900 пар** + validation/state + образцы тел | 3,5,6,7,8 | **COMPLETE** (b018 deep, b001-017 census+sample) |
| **B4.** translator-ru-en-general (20 батчей) | 125 | **ВСЕ 1000 пар-строк** прочитаны скриптом (ключи/source_id/license/locator/defect по каждой); b012 **построчно** (цитаты); validation/state b001/b020; source-inventory b001-005 | 3,6,8 | **COMPLETE** (полный row-census 20/20 батчей) |
| **B5.** 2026-05-11 REAL DPO (29 батчей / 58 dpo-run) | 824 | **ВСЕ 58** `internal-train-ready-dpo-summary.json` + **ВСЕ 25** STREAM-STATE + batch-001 (progress/batch-summary/checkpoints/tokenizer/normalized) **ЦЕЛИКОМ**; траектория метрик сведена по 58 прогонам; чекпоинты — размер/идентичность | 2,7,8 | **COMPLETE по метрикам/структуре/траектории; PARTIAL по per-pair телам и бинарям (честный остаток ниже)** |

**Честный остаток по B5 (единственная PARTIAL-группа — прямо названо):**
- **~290 pair-body `.jsonl`** (`dpo-pairs-normalized.jsonl` / `training-pairs-train-ready.jsonl` внутри каждого dpo-run): построчно прочитаны образцы **batch-001 / 012 / 029** + held-out review 10 пар; остальные покрыты **по паттерну** (стабильная схема `{Prompt, Chosen, Rejected}`, где Prompt = постоянная строка без источника — установлено пассом 2). НЕ каждая из ~14 000 строк прочитана глазами; но source/license/locator-census исполнен tree-wide скриптом по 5747 пар-строкам (пасс 8).
- **59 `.bin` policy-весов:** подтверждён размер (**все ровно 1 188 744 Б, идентичны**) и модель-конфиг из STREAM-STATE (`hidden=64,layers=1,ctx=128,vocab=2048`); бинарь **НЕ десериализован** (это веса, не текст — построчно не читается по смыслу).
- **58 per-run `batch-summary.jsonl` / `progress.jsonl`:** batch-001 прочитан целиком; остальные **сведены** через одинаковые `internal-...-summary.json` (тот же формат метрик).

**Вывод sweep:** все 1154 файла отнесены к охарактеризованной группе; неохваченных
файл-групп НЕТ. Deep-read (прочитано глазами целиком): все 22 верхних текстовых
файла + b018 целиком + router-b001 целиком + все 1000 ru-en пар (скриптом) + все
58 dpo-summary + все 25 STREAM-STATE + batch-001 целиком. Pattern/sample-covered
(не каждая строка глазами): ~290 pair-body jsonl прогона и 59 бинарных весов —
честно помечены как остаток, обоснованный (одинаковая схема/идентичные веса).

---

### 2. EXECUTIVE SUMMARY (можно читать ТОЛЬКО эту секцию)

**Что это за папка и её роль в плане продолжения расширений VS / VS Code.**
`language translate learning` — это учебная «мастерская» **capability перевода**,
активного **P0-пилота** (по RAG-01). Её роль в целевом флоу — **перевести промпт
пользователя с другого языка (например с иврита или русского) на английский ПЕРЕД
оркестрацией**, т.е. это верхний UPSTREAM-узел перед Node1 Splitter оркестратора
(RAG-02). 1154 файла делятся на два пласта: **22 верхних текстовых документа**
(зрелый дизайн + гайды + реестры лицензий + один рабочий гейт-скрипт) и
**`translation-runs/`** (1131 файл фактических прогонов) — где живёт то, что
реально было натренировано и сгенерировано.

**Дизайн — сильный, полный, переиспользуемый как есть.** Папка несёт готовый
архитектурный чертёж перевода, который НЕ надо доизобретать: **4-уровневый
обучаемый роутер** `TranslationRoutingFlow : ILogicalFlow` (+`Orchestrator`), 6
внутренних `ILogicalUnit`-узлов (R1 direction → R2 topic → R3 register/risk → R4
adapter), **таксономию DPO-пар T01-T09** (core/register/морфология/идиома/
false-cognate/RTL-иврит/back-translation/полнота/named-entity), 4-уровневый
curriculum L1-L4 с promotion-гейтами, мультискрипт-BPE (кириллица/иврит/латиница),
LoRA Rank4/Alpha8, и — что важнее всего для продолжения — **`RUNNER-CONTRACT.md`**:
точный интерфейс отсутствующего тренера/эвалуатора (14 train-метрик + 6
quality-метрик bleu/chrf/target_script/copied_source/wrong_direction/named_entity/
completeness + 10 полей per-row held-out). Дизайн явно запрещает hardcode-
классификатор (совпадает с CLAUDE.md R4 `NO HIDDEN DETERMINISM`).

**Реальность — обучение НЕ доказано; потолок = честный необученный скелет
(`PASS_AS_HONEST_UNTRAINED_SCAFFOLD`).** Настоящий DPO-тренер запускался
**единственный раз** — в дата-прогоне `2026-05-11/ru-en-general/level-1` (29
батчей, 58 прогонов, 59 чекпоинтов). Этот прогон **reward-hacked**: (1) в промпт
тренера подаётся ПОСТОЯННАЯ строка без русского источника — модель физически не
может обусловиться на вход, она лишь учит «короткий чистый ответ > длинная
простыня»; (2) Loss держится 0.685-0.695 ≈ ln2=0.693 (плоская неинформированная
цель); (3) accuracy скачет 0.00↔1.00 между соседними батчами (дисперсия крошечной
модели), а на накопленных 200 парах — **регресс** до acc 0.29 / margin −0.0152;
(4) оператор вручную подбирал слова под нужный BPE-токен, чтобы гейт 0.6 прошёл
(инженерия токенайзера ради метрики). Сама модель **вырожденная**: ровно
**1 188 744 байт (~1.16 МБ)**, hidden=64, 1 слой, ctx=128, vocab=2048. «17
принятых / 850 пар» — это сумма прошедших per-batch гейт попыток, а НЕ модель,
обученная на 850 парах. Все прочие стримы (router-r1/r2, translator-en-ru,
translator-ru-en) — режим **PAIRSET-ONLY** (`metric_mode:not_run`): тренер по ним
не запускался вовсе, `structural_accuracy:1.0` = готовность НАБОРА, не обученная
модель. **Ключевой блокер:** инференс-эвалуатор `translate(source, checkpoint)` из
`09.Inference/TextGenerator.cs` **НЕ реализован** — поэтому качество перевода
(BLEU/chrF/script) не измерялось НИКОГДА.

**Данные — есть годный cold-start-скелет (~1899 пар), но 0 лицензионных и две
ловушки.** Реальная ценность папки: **~1899 well-formed DPO-пар** (999 RU→EN +
900 EN→RU + 50 heldout), свободных от reward-hack прогона 2026-05-11 на масштабе
(длина сбалансирована, length-сигнал внутренне противоречив по типам дефекта →
чистый length-хак невозможен, дистракторы семантические). НО **verified-licensed
актив = 0 пар**: во всём дереве 1154 поле `source_locator` встречается **0 раз**;
из 4 ACCEPT-источников реестра (TR-S01 Tatoeba, TR-S02 ParaNames, TR-S03 Knesset,
TR-S04 UN) физически материализован только TR-S01, и то как ~100 **переклеенных/
декларированных** строк без извлечения. `synthetic-seed-tatoeba-style` ≠ Tatoeba.
**Две ловушки, которые план ОБЯЗАН учесть:** (1) верхнеуровневый seed
`training-pairs-train-ready.jsonl` (50 пар) — это дамп **ПРОВАЛЬНОГО batch-029**
(лучшая попытка 0.58 < гейт 0.60), все `rejected` — 50-80-словные мета-эссе
`[WRONG:…]` = классический format/length-хак, нарушает собственный гайд папки;
**переносить в `27.TranslationDomain` как gold НЕЛЬЗЯ** (это и есть «seed,
отсутствующий на HEAD» из RAG-01); (2) самый зрелый батч **b018** размечен
`TR-S01/ACCEPT/CC-BY 2.0`, но БЕЗ per-row locator, и тот же батч несёт честную
записку `SOURCE-REGISTRATION-REQUIRED.md` «это синтетика, не зарегистрирован
ACCEPT» → метка ACCEPT **заявлена, а не доказана** (провенанс-оверклейм).

**Иврит-дыра и точка приземления.** Luba явно назвала **иврит→английский**, а его
в папке **НЕТ ВООБЩЕ**: 0 переводческих пар, 0 dpo-run, 0 чекпоинтов. Иврит
встречается только как ВХОД в 6 router-файлах (для классификации направления), не
как переводческая пара. Причина: `TOKENIZER_BLOCKED` (иврит-покрытие ≥90% —
Phase-0 pre-condition) + Difficulty-Ladder ставит `he-en-general-l1` шагами 8-9,
ПОСЛЕ приёмки RU_EN/EN_RU L1, которая не состоялась. **Заявлять «поддержка иврита»
сегодня нельзя** — capability иврита надо строить с нуля (иврит-BPE → ACCEPT HE-EN
корпус → router-гейт → he-en L1). Точка приземления обученного юнита —
`27.TranslationDomain/TranslationMachine.cs` (`ILearningMachine<TranslationDomain>`,
пилот RAG-01, где `accuracy 0.3333`/`reward_margin 0.03` — отдельное,
унаследованное из main, 1-строчно-обученное/захардкоженное число, НЕ отражающее
этот прогон).

**Итог для плана + 6 открытых вопросов Любе (заголовки).** Сегодня узел перевода в
расширениях = **честный необученный scaffold** (готовый дизайн + годный
PAIRSET-скелет + детерминированный script-детектор направления как baseline), а
НЕ обученный лицензированный переводчик. Разблокирующая последовательность:
(Q1) построить ингест **TR-S01 Tatoeba** с per-row locator, синтетику оставить
явным cold-start; (Q2) реализовать **`translate()`-эвалуатор** по RUNNER-CONTRACT —
следующий разблокирующий шаг; (Q3) иврит — отдельным треком ПОСЛЕ первого RU-EN
цикла (или параллельно, если это жёсткое P0-требование — нужно решение Luba);
(Q4) seed b029 **не переносить**, метку ACCEPT на b018 понизить до
registration_required; (Q5) первым промотировать **RU→EN 999-парный стрим**
(лучшая диверсификация; сначала удалить 1 идентичную пару `TR-RU-EN-GEN-1544`);
(Q6) роутеры R1/R2 — R1 объявить детерминированным baseline-оракулом, R2 либо
переучить как `ILogicalUnit` с НЕЗАВИСИМЫМ gold и без утечки confidence/
evidence_terms, либо тоже пометить baseline.

**Must-read файлы (минимальный набор для плана):**
1. `TRANSLATION-FOUR-LEVEL-LLM-TRAINING-PLAN-v1.md` — активный операционный дизайн (4×4, 50+50).
2. `TRANSLATION-TOPIC-ROUTER-ORCHESTRATION-PLAN-v1.md` — дизайн роутера R1-R4, темы D01-D06.
3. `TRANSLATION-SOURCE-REGISTRY-v3.md` — каноничный license-gate (ACCEPT Tatoeba/ParaNames/UN; REVIEW web-mined; BLOCK NC).
4. `translation-runs/translator-en-ru-general/batch-018/RUNNER-CONTRACT.md` — **единственный** физический экземпляр интерфейса отсутствующего runner/evaluator.
5. `TRANSLATION-EXECUTOR-HARD-RESET-v1.md` — режим `PAIRSET_READY_TRAINING_RUNNER_MISSING`, forbidden-claims, `SYNTHETIC_SEED`.
6. `translation-runs/translator-en-ru-general/batch-018/SOURCE-REGISTRATION-REQUIRED.md` — честная записка о провенанс-оверклейме (ловушка №2).
7. (данные) `translation-runs/translator-ru-en-general/` — годный 999-парный RU→EN cold-start-скелет.
8. (не переносить) верхний `training-pairs-train-ready.jsonl` — reward-hacked дамп b029 (ловушка №1).

---

### 3. Честный отчёт о покрытии (read-fully vs sampled; что НЕ запускалось)

**Всего файлов:** 1154 (546 json + 369 jsonl + 178 md + 59 bin + 1 py + 1 zip),
233 папки — независимо перепроверено пассом 10.

**Прочитано ПОЛНОСТЬЮ глазами / построчно (deep):**
- все **22** верхних текстовых файла (18 гайдов/планов/реестров + 1 py-скрипт целиком + seed 50 строк + 2 пустых jsonl);
- `translator-en-ru-general/batch-018` целиком (train50 + heldout50 + RUNNER-CONTRACT + SOURCE-REGISTRATION + sample-review + все control-doc);
- `router-r1/r2 batch-001` целиком (train + heldout + heldout-evaluation, цитаты 0001-0006 / 0101-0106);
- `2026-05-11/batch-001` целиком (progress + batch-summary + checkpoints json + tokenizer + normalized pairs);
- все **58** `internal-train-ready-dpo-summary.json` + все **25** `STREAM-STATE.json` дата-прогона (метрики/вердикты сведены в траекторию по 58 прогонам);
- `translation-runs/DOCUMENT-USE-LOG.md` (корень).

**Прочитано СКРИПТОМ по каждой строке (row-census, не глазами, но каждая строка обработана):**
- **все 1000 пар** `translator-ru-en-general` (20/20 батчей): ключи/source_id/license_state/locator/defect_type;
- length-census **всех 900 пар** `translator-en-ru-general`;
- tree-wide census `source_license_state`+locator по **5747 пар-строкам** всего дерева (результат: `ACCEPT`=100 только b018, locator=0 везде).

**Покрыто ПО ПАТТЕРНУ / выборочно (честный остаток, обоснованный):**
- ~**290 pair-body jsonl** внутри 58 dpo-run прогона 2026-05-11: прочитаны образцы batch-001/012/029 + 10 held-out; остальные — по стабильной схеме `{Prompt(const), Chosen, Rejected}` (per-string глазами не читались, но source/locator покрыты census-скриптом);
- **59 бинарных `.bin`**: подтверждены размер (все 1 188 744 Б) и идентичность; НЕ десериализованы (веса, не текст);
- 57 per-run `batch-summary.jsonl`/`progress.jsonl` (кроме batch-001): сведены через одинаковые internal-summary;
- `translation-runs.zip`: листинг без распаковки (1131=1131 зеркало, уникального контента нет).

**Что НЕ запускалось (ни в одном пассе 1-10):** сборка (`dotnet build`), тесты
(`dotnet test`), DPO-тренинг, инференс/`translate()`, исполнение
`prepare_translation_learning_sets.py`, любой runner. Все находки — из ЧТЕНИЯ уже
записанных на диск артефактов прогонов (метрики, чекпоинты, state), а НЕ из новых
запусков. Причина честности этого различия: сам runner reward-hacked и
inference-эвалуатор не реализован — новый запуск не дал бы валидного качества
перевода, только повторил бы length-дискриминатор.

**folder #3/12 «language-translate-learning» RAG COMPLETE** — все 1154 файла
отнесены к охарактеризованной группе; единственная PARTIAL-подгруппа (per-pair
тела и бинарные веса прогона 2026-05-11) честно помечена и обоснована (одинаковая
схема / идентичные веса); executive summary самодостаточен для плана продолжения
пилота перевода в расширениях VS / VS Code.

---
*RAG-03, проход 10/10 (финал). Evidence-bound: census 1154/233/546-369-178-59-1-1,
per-stream (32/33/116/125/824/1) и basename-группы (STREAM-STATE×69,
policy_step_000005.bin×51, все 59 .bin=1 188 744 Б, DOCUMENT-USE-LOG×3,
RUNNER-CONTRACT×1, SOURCE-REGISTRATION-REQUIRED×1) перепроверены `find` этим
пассом и совпали с манифестом пасса 1 / census пасса 8. Числа executive-summary
(Loss≈0.693, регресс acc0.29/margin−0.0152, 1.16 МБ вес, 1899 well-formed,
verified-licensed=0, locator=0 tree-wide, иврит=0 потоков, seed=b029-провальный,
b018-провенанс-оверклейм) сведены из уже записанных секций пассов 1-9. Записан
только этот RAG-файл; всё прочее read-only; ни build/test/training/inference, ни
git-write не запускались.*
