# NEW-DOMAIN-DEVELOPMENT-GUIDE-v1.md
# Как разрабатывается новый обучаемый домен в ILearningMachine
# Версия 1.3 — 2026-05-29
# На основе: «Рабочая сессия нового домейна.txt» + TranslationDomain + documentation learning + local wiki learning
# 22 passes of improvement: CODE-DOC-ACTIVE-GOAL, CODE-DOC-LOCAL-GAP, EXECUTOR-PROMPT-CODE-DOC-SESSION,
#   CODE-DOC-50-PAIR-LEARNING, ORCHESTRATION-PROTOCOL-SESSION-1/2, PROTOCOL-ORCHESTRATION-EXTRACTION,
#   CODE-DOC-DPO-REJECTED-PROMPTS, CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE,
#   LINGUISTIC-RULES-PROTOCOL-RU-EN (Pass 21), LANGUAGE-RULE-SOURCE-REGISTRY (Pass 22)

---

## Назначение документа

Этот документ описывает полный процесс создания нового обучаемого домена
в архитектуре `ILearningMachine<TDomain>`. TranslationDomain (27.TranslationDomain)
был первым доменом, прошедшим этот процесс целиком. Здесь он задокументирован
как повторяемый шаблон для каждого следующего домена.

---

## Принципиальная схема: что такое домен в этой системе

```
ILearningMachine<TDomain>          ← верхний уровень: Train/Evaluate/Export/Import
    ↓
ILogicalOrchestrator               ← координирует flow-ы при inference
    ↓
ILogicalFlow<TInput>               ← топология unit-ов для одного сценария
    ↓
ILogicalUnit<TInput, TOutput>      ← минимальная обучаемая ячейка
```

Каждый домен — это `ILearningMachine<TDomain>` с набором специфичных для него
`ILogicalUnit` реализаций, DPO-парами и training runner.

**Правило A-LM-02 (locked):** методы `Train*` существуют ТОЛЬКО на `ILearningMachine`.
`ILogicalUnit` и `ILogicalFlow` — это pure inference контракты. Никогда не добавлять
методы обучения в исполняющий интерфейс.

**Важное различие (Pass 14): Orchestration vs Inventory**

Orchestration говорит КАК работа движется: input → transformation → verification → persisted state.
Inventory говорит ЧТО существует: unit-ы, store-ы, artефакты, числа.

При описании домена и при построении training set-ов всегда начинать с orchestration
(как пользовательский запрос становится контролируемым путём к результату),
а не с инвентаря (какие классы есть в коде).

---

## Шаг 0 — Предварительные условия

Перед созданием нового домена должны быть закрыты три технических долга:

| Долг | Что требуется | Статус |
|------|--------------|--------|
| TD3 | `ILearningMachine<TDomain>` интерфейс в `00.Core` | ✅ Закрыт 2026-05-28 |
| TD1 | Bootstrap ILogicalUnit-ы как обёртки существующей логики | ✅ Закрыт 2026-05-28 |
| TD2 | Подключение к `ILogicalOrchestrator` через registered flow | ✅ Закрыт 2026-05-28 |

Если любой из них не закрыт — новый домен нельзя создавать, потому что
у него не будет правильного контракта обучения.

**Самопроверка перед стартом (Pass 3 self-check rule):**
- Существует ли `<Domain>RoutingFlow : ILogicalFlow<TInput>` в `15.OrchestratorModel`?
- Может ли runner читать training pairs и held-out rows для этого домена?
- Запускалось ли DPO обучение с реальными метриками, а не только с unit-тестами?
- Есть ли CPU, RAM, timing записанные как реальные числа, а не null/UNKNOWN?

Если хотя бы одно «нет» — домен в режиме `PAIRSET_READY_TRAINING_RUNNER_MISSING`.
Работу по построению пар можно начинать, но принять batch нельзя.

---

## Шаг 1 — Проектирование архитектуры домена

### 1A. Определить специфичные для домена ILogicalUnit-ы

Каждый домен требует набора unit-ов, которые разбивают его задачу на
отдельно обучаемые шаги.

**Пример: TranslationDomain**

| Unit | Что обучается | Ключевое хранилище |
|------|--------------|-------------------|
| `SemanticTokenizerUnit` | Правильная структурная сигнатура (POS-биграммный паттерн) для входа | `_signatureCorrections`, `_patternCorrections` |
| `ExecutorUnit` | Выбор слов в контексте структуры — KNN retrieval | `TranslationKnnStore` (общий с inference) |
| `QualityVerifierUnit` | Какие термины обязательны для данного структурного паттерна | `_requiredTermsBySignature`, `_conceptRules` |

**Правило проектирования unit-ов:**
- Каждый unit учит одно решение, а не всё сразу.
- Unit-ы должны быть обучаемы независимо.
- Обучение всех unit-ов вместе — не то же самое, что обучение каждого по отдельности.
  Если обучен только один unit, это неполный gate.

**Pass 13 — Array unit rule (из ORCHESTRATION-PROTOCOL-SESSION-2):**
Некоторые unit-ы — **array unit-ы**: они выполняются один раз на каждый элемент,
а не один раз на весь pipeline run. Примеры: domain translator, domain processor,
applier, result translator.

Для array unit-ов правило построения пар:
- `A` = один элемент (одна область, один под-запрос, один фрагмент результата) — НЕ полный список
- `B` = один вывод для этого элемента — НЕ все выводы объединённые

Пары где `A` = полный список, `B` = все выводы объединённые — невалидны, переписать.

### 1B. Определить Bootstrap-реализацию

До начала обучения каждый unit должен иметь детерминистическую Bootstrap-реализацию,
которая делает то же самое что текущая логика.

Bootstrap — это **стартовая точка**, не финальный код. Пример:
- `BootstrapSemanticTokenizerUnit` — возвращает сигнатуру на основе правил
- `BootstrapExecutorUnit` — возвращает `[DIRECTION:BOOTSTRAP]` когда KNN нет

Bootstrap не сломан. `[DIRECTION:BOOTSTRAP]` — честный ответ «нет обученных пар
для этого паттерна». Но показывать его пользователю как будто это перевод — нельзя.

### 1C. Определить routing flow

Для домена определяется `ILogicalFlow<TInput>` который:
1. Выбирает direction (RU_EN / EN_RU / HE_EN / EN_HE для перевода)
2. Выбирает topic / register / risk adapter
3. Запускает unit-ы в правильном порядке

Flow регистрируется в `15.OrchestratorModel/Logical/LogicalOrchestratorAssistant.cs`.

**Pass 14 — Routing flow должен маршрутизировать, не вычислять результат:**
Routing flow — это orchestration (как запрос движется через unit-ы),
а не inventory (список unit-ов которые существуют). Конкретные поля маршрутизации
должны быть явными в `TInput`:
- AppSide (server/client/mixed)
- RuntimeFamily + TargetFramework
- DocumentationLevel или TranslationDirection
- ArtifactKind
- Version routing fields

### 1D. Anti-determinism — жёсткое правило

**Запрещено в коде домена:**
```csharp
// ❌ if (word == "run") return "запустить";
// ❌ dictionary["run"] = "запустить";
// ❌ regex pattern → hardcoded translation
```

Код вычисляет только **структурные адреса** (POS-биграммы, сигнатуры).
Все конкретные решения — из того, что KNN выучил через DPO-пары.

Если в коде есть if/else или словарь, который производит domain output напрямую —
это детерминистический код. Его нужно удалить и заменить DPO-парами.

---

## Шаг 2 — Планирование корпуса и источников данных

### 2A. Определить требуемый объём

Типичный базовый блок для языкового домена:

| Компонент | Объём |
|-----------|-------|
| Слов на язык | 3 000 |
| Предложений на слово | 10 |
| Пар предложений на направление | 30 000 |
| DPO-примеров (× 3 unit-а) | 90 000 на направление |
| Held-out (20%) | 6 000 пар |

Для других доменов объём другой, но принцип тот же:
- базовый блок (general patterns)
- pilot сначала (100 примеров)
- scale только после прохождения pilot gate

**Pass 8 — Gap analysis перед стартом:**
До начала построения batch-ей составить таблицу покрытия по каждому источнику/подпроекту:

| Источник/подпроект | Примеров | Структурных edge-ов | Текущее покрытие | Пробелы |
|---|---:|---:|---|---|
| (название) | N | M | Good/Weak | Что именно не хватает |

Если coverage по любому критическому источнику пустое — добавить framework lifecycle support rows
(Pass 9) или external source proof stream (Pass 10) для этого источника до начала training.

### 2B. Определить правовой whitelist источников

**Обязательные вопросы перед использованием любого источника:**

1. Разрешено ли коммерческое использование?
2. Требуется ли атрибуция? Где она будет храниться?
3. Есть ли NC (Non-Commercial) ограничение? → Исключить.
4. Является ли источник переводом, созданным после 1928 года? → Проверить авторские права.

**Примеры для TranslationDomain (EN↔RU):**

| Источник | Лицензия | Использование |
|----------|----------|--------------|
| Tatoeba | CC BY 2.0 FR | ✅ с атрибуцией |
| UN Parallel Corpus | Свободная | ✅ официальный язык |
| Project Gutenberg | Public domain (US) | ✅ оригиналы до 1928 |
| Standard Ebooks | CC0 | ✅ без ограничений |
| Wikisource / Викитека | PD only | ✅ проверять переводчика |
| PMC Open Access | CC0 / CC BY | ✅ только эти лицензии |
| OpenSubtitles | Mixed | ❌ исключён |
| ParaCrawl / CCMatrix | Web-crawl | ❌ исключён |
| CC BY-NC всё | NC лицензия | ❌ исключён |

**Полный whitelist:** `docs/plans/TRANSLATION-CORPUS-SOURCING-PLAN-v1.md §4`

**Source registry: обязательные поля для каждой строки источника (из CODE-DOC-SOURCE-REGISTRY-v1.md):**

```json
{
  "source_id": "<traceable — не придуманный>",
  "repository_url": "https://...",
  "commit_sha": "<exact sha или immutable tag — НЕ branch name>",
  "file_path": "src/...",
  "symbol_id": "Namespace.Type.Method(...)",
  "line_span": "L10-L42",
  "license_spdx": "<точная SPDX лицензия>",
  "license_file_path": "LICENSE",
  "license_proof_excerpt": "<первые строки LICENSE файла>",
  "extracted_at": "2026-05-29T...",
  "source_role": "chosen_authority | support_only | held_out_only | review_required | blocked",
  "allowed_training_use": true
}
```

**Критически важно:** не доверять названию репозитория, записи в памяти или blog-посту о лицензии.
Захватывать `license_proof_excerpt` из реального файла лицензии на конкретном commit/tag.
Синтетические source_id (например, `tatoeba-style-example-001` без реального sentence ID) — запрещены как chosen authority.

### 2C. Pilot-first: 100 перед 3000

**Обязательный порядок:**

```
P0: Pilot — 100 words × 10 sentences
    Gate: unseen batch shows zero contamination
          runtimeProductAccuracy записана (даже 0.00 = честный baseline)
    ↓ только после P0 gate
P1: Scale — 3000 words × 10 sentences
```

Почему pilot обязателен:
- Доказывает, что contamination guard работает на практике, не только в документации.
- Если contamination не проверена на 100 словах, заявлять 85% на 3000 бессмысленно.
- Паттерн TR-HO-17..32: без pilot прошли прямо к 3000 → accuracy = 1.000 = меморизация.

### 2D. Что хранится в git, что нет

| Артефакт | Git | Приватно |
|----------|-----|---------|
| `word-list-*.jsonl` (только слова, лицензии, rank) | ✅ | |
| `sentence-index-*.jsonl` (только IDs, лицензии) | ✅ | |
| `source-ledger.jsonl` (attribution manifest) | ✅ | |
| Runner reports (числовые метрики) | ✅ | |
| Manifests + checkpoint references | ✅ | |
| Raw corpus dumps (Tatoeba, UN) | | ✅ локально |
| Raw sentence pairs (текст) | | ✅ приватно |
| DPO JSONL с chosen/rejected парами | | ✅ приватно |
| Held-out source IDs | ✅ | |
| Held-out expected_translation | | ✅ приватно (обязательно!) |

**Важно:** held-out файлы в git должны содержать ТОЛЬКО `input_text` / source IDs.
Но приватный eval-файл **ОБЯЗАН** иметь `expected_translation`, иначе проверяется
только "не Bootstrap", а не качество перевода.

### 2E. External source proof stream (Pass 10)

Для доменов, где local sources дают слабое покрытие по некоторым категориям,
создаётся параллельный **external source proof stream**.

**Требования к external stream:**
1. Каждый источник имеет exact commit SHA и license файл захваченный как proof
2. Каждый batch: 50 training pairs + 50 held-out rows
3. Каждая строка источника имеет `EXTERNAL-SOURCE-PROOF.jsonl` запись
4. Framework lifecycle docs не выдумывают бизнес-домен поведение которого нет в источнике

**Примеры из documentation domain (аналог для других доменов):**

| Репозиторий | Commit SHA | Лицензия | Роль |
|---|---|---|---|
| `dotnet/AspNetCore.Docs` | `acba8a58d2c...` | MIT code samples | framework lifecycle support |
| `dotnet-architecture/eShopModernizing` | `63bc9ec4414d...` | MIT | clean code authority |
| `aspnet/AspNetWebStack` | `af67fe18a182...` | Apache-2.0 | clean code authority |

**Gate для external source batch:**
- 50 train + 50 held-out rows присутствуют
- source/license proof приложен
- generated output включает stack/version/direction факты из источника
- source evidence цитирован в output
- framework/domain lifecycle docs не выдумывают business domain behavior

---

## Шаг 3 — Построение DPO-пар для каждого unit-а

### 3A. Структура DPO-пары и обязательные ограничения

Каждая пара содержит:
- `a_side`: входной текст (источник)
- `b_side_chosen`: правильный вывод unit-а
- `b_side_rejected`: неправильный вывод с аннотацией `[WRONG: слабость - пробел - последствие]`
- `unit`: имя unit-а (`semantic-tokenizer` / `executor-ru_en` / `quality-verifier`)
- `source_id`: ID пары в источнике (Tatoeba sentence ID и т.д.)
- `license`: лицензия источника

**Обязательный расширенный формат пары (из CODE-DOC-DPO-REGISTRY-GUIDE-v1.md):**

```
Pair[ID]
A:
  input: <входной текст для unit-а>
  context: <дополнительный контекст — сигнатуры, call edges, callee summaries>
  domain_routing: <направление / тема / регистр / стек — зависит от домена>
B:
  chosen: <правильный вывод unit-а с полным содержанием, не описанием>
  rejected: <близкий, но неправильный вывод>
Reason:
  why_chosen_is_correct: <объяснение>
  why_rejected_is_wrong: <объяснение>
Phrasing-B:
  alternate_phrasing: <другая формулировка правильного вывода>
Coverage:
  source_id: <traceable ID, включая commit SHA или immutable tag>
  license_state: <точная SPDX лицензия>
  rejected_type: <тип ошибки в rejected>
  overlap_ratio: <0.25–0.55>
```

**Формат `[WRONG:]` аннотации:**
```
[WRONG: weakness (что именно слабо) - gap (чего не хватает) - consequence (что идёт не так)]
```

Пример для ExecutorUnit:
```
[WRONG: word map empty - executor never received context words - translation falls back to Bootstrap]
```

**Правило overlap ratio (из CODE-DOC-DPO-PAIR-AUTHORING-GUIDANCE-v1.md):**
- Target: 0.25–0.55 для нормальных пар
- Жёсткий лимит: всегда < 0.60
- Запрещено: rejected = chosen с вставленным "не/not" — это нулевое обучение

**Правило A-side evidence:**
Каждый A-side обязан содержать достаточно контекста для chosen-вывода.
Если контекст не присутствует — пара не может обучать этот вывод.
Синтетические source ID (придуманные без traceable repo + commit) — запрещены как chosen authority.

**Правило runtime pair format (из local wiki learning):**
Пары должны выглядеть как то, что unit получает во время runtime, а не как ledger-записи.
Ledger (опись "какой строке документа соответствует какой node") — это audit trail, а не обучающие данные.
Модель, обученная на ledger-строках, учится обрабатывать ledger, а не реальные запросы.

**Pass 12 — INLINE CONSTRAINTS для обучающих пар (из ORCHESTRATION-PROTOCOL-SESSION-2):**

Следующие ограничения активны для всех пар во всех batch-ах:

```
1. Один источник — никакой контент из других документов/источников в один batch
2. Не сокращать текст — источниковый текст не урезается
3. f(x) = rephrase(x) — каждая концепция появляется в переформулированном виде хотя бы в одной паре
4. Один документ, самопроверка — валидировать до пометки complete
5. Запрещённые фразы в B (chosen side): "a clear work statement", "produce a result that preserves",
   "translate it", "plan it", "a translated version", "a processed requirement"
   Если любая из этих фраз найдена — пара не является обучающей парой, переписать с реальным выводом
```

### 3B. Построение пар по unit-ам и типология пар

**Universal pair types (Pass 1 — CD-A through CD-F из CODE-DOC-DPO-SESSION-v1.md):**

Каждой паре назначается тип, применимый к любому домену:

| Тип | Что обучает |
|-----|------------|
| CD-A | Локальное поведение unit-а из данных только этого контекста |
| CD-B | Inputs, outputs, error paths, null handling |
| CD-C | Side effects и data access (записи, DB вызовы, events, кэш) |
| CD-D | Domain lifecycle routing (framework, direction, register — должен быть точным) |
| CD-E | Callee/upstream propagation (local vs delegated behavior) |
| CD-F | Component или workflow summary из накопленных фактов снизу вверх |

**Типичный mix для 50-парного batch на уровне L1 (атомарные факты):**
```
20 × CD-A (local purpose)
10 × CD-B (inputs/outputs/errors)
8 × CD-C (side effects/data access)
6 × CD-D (domain lifecycle routing)
6 × CD-F (rejection pairs for invented behavior)
```

---

**SemanticTokenizerUnit:**
- `a_side`: входное предложение
- `b_side_chosen`: правильная POS-биграммная сигнатура (VERB_NOUN, ADJ_NOUN, ...)
- `b_side_rejected`: неправильная сигнатура + `[WRONG:]`
- Цель: учит модель правильно определять структурный паттерн входа

**ExecutorUnit:**
- `a_side`: входное предложение + структурная сигнатура
- `b_side_chosen`: правильный перевод с `WordMappings` (непустые)
- `b_side_rejected`: Bootstrap-fallback или неправильный перевод + `[WRONG:]`
- Цель: учит KNN выбирать правильные слова в контексте структуры
- **Критически важно:** `WordMappings` не должен быть `null`. Это главная причина провала

**QualityVerifierUnit:**
- `a_side`: входное предложение + ожидаемые ключевые слова
- `b_side_chosen`: верификатор принял (все обязательные термины присутствуют)
- `b_side_rejected`: верификатор отверг + `[WRONG:]` почему именно
- Цель: учит определять минимальный набор терминов для данного паттерна

**Pass 9 — Domain lifecycle support rows:**
Некоторые источники дают «тонкие» примеры без богатого внутреннего call graph
(например, minimal template projects, простые utility методы, шаблонные конечные точки).

Для таких источников требуются **domain lifecycle support rows** — отдельный класс пар:
- Они помечаются: `source_role: "framework_lifecycle_support"` или `"domain_lifecycle_support"`
- Они учат модель правильно документировать тонкий источник через lifecycle знания, а не through изобретённый business domain behavior
- Lifecycle docs НЕ выдумывают business domain behavior которого нет в коде/тексте источника
- `documentation_level` для таких строк: `D3_DOMAIN_ENTRYPOINT` или `D4_DOMAIN_PIPELINE`

### 3C. Три бага которые уничтожили первый цикл TranslationDomain

Эти баги были в коде до исправления и привели к `accuracy = 0.0` на unseen данных.
Задокументированы здесь как напоминание: правильные DPO-пары бесполезны без правильного кода.

| Баг | Проблема | Исправление |
|-----|---------|------------|
| `confidence > 0.5` вместо `>= 0.5` | Fallback путь всегда проваливался | `>= 0.5` |
| `SourceWord` при обучении vs `Lemma` при inference | Word map никогда не попадал | Нормализовать ключи |
| `WordMappings = null` в builder | Executor никогда не получал контекст слов | Всегда заполнять `WordMappings` |

**Урок:** перед тем как считать что DPO-пары не работают — проверить механизм KNN cascade.
Точные места сломанности находятся построчным чтением кода, а не через тесты.

### 3D. Shared bridge между training и inference

**Критически важно:** единственный мост между `TrainBatchAsync` и `GetOrchestratorAsync`
это `TranslationKnnStore` (для TranslationDomain).

Если training и inference не используют **один и тот же экземпляр** этого хранилища —
обучение не имеет никакого эффекта на вывод. Это первое, что нужно проверить
при любом `accuracy = 0.000` результате.

### 3E. Жёсткие ворота качества пар (hard gates) и паттерны rejected

Пара НЕ входит в training если хотя бы одно из условий:

| Нарушение | Почему |
|-----------|--------|
| `b_side_chosen == b_side_rejected` | Нет обучающего сигнала |
| `b_side_rejected` не содержит `[WRONG: ...]` | Нет диагностики ошибки |
| `b_side_rejected` = `b_side_chosen` с добавленным "не/not" | Нулевое обучение |
| `b_side_chosen` описывает поведение не видимое в контексте | Invented behavior — модель учится галлюцинировать |
| `source_id` синтетический (без traceable repo + commit) | Нет proveniance — нельзя воспроизвести |
| overlap_ratio ≥ 0.60 | Пары слишком похожи — слабый DPO сигнал |
| `metric_mode` не `actual` при отметке batch как accepted | Проецируемые метрики ≠ реальный результат обучения |
| Stack label / domain routing = `UNKNOWN_REVIEW_REQUIRED` | Нельзя использовать в train-ready |
| Synthetic source locator без traceable path | Только `support_only`, не chosen authority |

**Pass 15 — Паттерны слабых rejected sides (A/B/C/D из CODE-DOC-DPO-REJECTED-PROMPTS-v1.md):**

Слабый rejected — это не просто "неправильный ответ". Категории:

| Паттерн | Описание | Пример проблемы |
|---------|----------|-----------------|
| A — Negation-only | Rejected = chosen с добавленным "not/не" или инверсией одного предложения | Модель учится избегать "not" вместо правильного поведения |
| B — Generic placeholder | Rejected = шаблонный плохой ответ без связи с конкретным контекстом | Нет специфического обучающего сигнала |
| C — Too similar | overlap_ratio > 0.55 без конкретной структурной ошибки | DPO margin будет близко к нулю |
| D — Wrong scope | Rejected описывает правильный ответ для другого контекста, не для этого | Модель учится routing вместо качества вывода |

**Требования к runner для проверки rejected quality:**
- `same_answer_prefix` check: rejected не должен иметь тот же префикс что chosen
- Mask prompt tokens в training: prompt tokens не должны получать gradient
- Persist `dpo-pair-diagnostics.jsonl`: группировать по rejected_type
- Никогда не публиковать когда held-out DPO reward margin < 0.75 threshold
- Если accuracy = 1.000 при первом корректном run — проверить contamination (снова!)

**Исторический пример (CODE-DOC domain):**
До исправления policy/reference: accuracy 1.0, margin near 0 → promoted NegativeDpoGenerator
После исправления: best corrected 0.58 accuracy, margin 0.003840186
После полного fix: accuracy 0.98, margin 0.003840186

Вывод: accuracy 1.0 с margin ≈ 0 = либо contamination, либо trivial rejected sides.

### 3F. Уровни обучения: нижний перед верхним (bottom-up)

Домены со сложной иерархией знаний обучаются уровень за уровнем, снизу вверх.
Нельзя учить верхний уровень пока нижний не имеет accepted batch.

**Принцип (из CODE-DOC-FOUR-LEVEL-TRAINING-PLAN-v1.md):**

```
L1 (атомарные факты — метод, фраза, слово)
    ↓ accepted batch → promotion gate
L2 (компонентный контекст — класс, параграф, фрагмент)
    ↓ accepted batch → promotion gate
L3 (цепочка вызовов / причинно-следственная связь)
    ↓ accepted batch → promotion gate
L4 (workflow / модуль / документ целиком)
```

**Для TranslationDomain это:**
- L1 = SemanticTokenizerUnit (структурная сигнатура слова/фразы)
- L2 = ExecutorUnit (слово в контексте структуры)
- L3 = QualityVerifierUnit (соответствие минимального набора терминов)
- L4 = полный pipeline inference через orchestrator

**Pass 2 — Конкретные gate thresholds по уровням (из EXECUTOR-PROMPT-CODE-DOC-SESSION-v1.md):**

| Уровень | Gate threshold | Что проверяется |
|---------|---------------|----------------|
| L1/D1 | factual_accuracy ≥ 0.80; invented_behavior_count ≤ 2 per 50 | Локальное поведение, parameters, return, side effects |
| L2/D2 | member_coverage ≥ 0.85; collaborator_precision ≥ 0.80 | Class responsibility from method summaries |
| L3/D3 | call_edge_preservation ≥ 0.85; failure_propagation_recall ≥ 0.75 | Caller с accepted callee summaries |
| L4/D4 | factual_accuracy ≥ 0.78; no high-severity invented subsystem | Full workflow от накопленных фактов |

**Pass 16 — 6 условий promotion gate:**

Promotion с уровня N на уровень N+1 требует все 6 условий:
1. Хотя бы один accepted batch для уровня N по данному stream/stack label
2. Два стабильных mixed batch (positive + negative gate rows) подряд
3. `dpo-pair-diagnostics.jsonl` по rejected типам для уровня N
4. Явный confirmation batch (batch специально для проверки готовности к promotion)
5. DPO reward margin > 0 на held-out inference
6. `metric_mode: "actual"` на всех acceptance метриках (не "projected", не "not_run")

### 3G. Lifecycle batch-а: три состояния и обязательные артефакты

Batch НЕ является accepted batch пока не прошёл весь lifecycle:

```
PAIRSET_READY_TRAINING_PENDING
    → пары построены, статические ворота пройдены
    → НО обучение НЕ запущено
    ↓
TRAINED_EVALUATION_PENDING
    → DPO обучение запущено
    → НО held-out inference НЕ завершён
    ↓
ACCEPT_BATCH
    → обучение запущено
    → held-out inference запущен через реальный routing flow
    → метрики записаны (metric_mode: "actual")
    → stream state обновлён
```

**Что это значит:**
- Prepared pairset → `prepared_batches` (не в `accepted_batches`)
- Только реально обученный и оценённый batch → `accepted_batches`
- Если runtime flow ещё не существует → статус `PAIRSET_READY_TRAINING_PENDING`,
  записать `metric_mode: "not_run"`, не останавливаться — продолжать строить пары

**Pass 4 — RUNNER-CONTRACT.md когда runner отсутствует:**
Когда `PAIRSET_READY_TRAINING_RUNNER_MISSING` — НЕ останавливаться и НЕ спрашивать что делать.
Написать `RUNNER-CONTRACT.md` с точными runtime поверхностями которые нужны runner-у:
- Какой интерфейс flow должен использоваться
- Какой формат TRAIN-PAIRS.jsonl ожидается
- Какой формат HELDOUT-ROWS.jsonl ожидается
- Какие метрики должны быть записаны
- Что считается acceptance threshold

**Обязательные файлы каждого batch-а (из CODE-DOC-EXECUTOR-HARD-RESET-v1.md):**

```
<runs-dir>/YYYY-MM-DD/<stream-id>/batch-<NNN>/
  DOCUMENT-USE-LOG.md           — какие документы прочитаны и что взято из каждого
  SOURCE-LEDGER.jsonl           — attribution manifest с commit SHA
  TRAIN-PAIRS.jsonl             — 50 пар в training-ready формате
  HELDOUT-ROWS.jsonl            — 50 disjoint held-out строк
  PAIR-VALIDATION-LEDGER.jsonl  — failed/repaired пары по конкретному gate
  TRAINING-METRICS.json         — метрики обучения (включая gate_b_dotnet_test)
  HELDOUT-EVALUATION.json       — метрики held-out inference
  LOOK-LIKE-RUN-REPORT.md       — human-readable отчёт (5 sample outputs)
  STREAM-STATE.json             — текущий статус stream
  NEXT-BATCH-DECISION.md        — одно решение: continue/promote/repair/stop
```

Если training не запускалось — также:
```
  RUNNER-CONTRACT.md            — точный контракт для runner-а
```

Если license proof отсутствует — также:
```
  SOURCE-REGISTRATION-REQUIRED.md  — какие поля отсутствуют
```

**Pass 3 — Self-check перед каждым accepted batch:**
```
☐ Реальный routing flow существует и был использован для held-out inference?
☐ DPO training запускался (не только проверка файлов)?
☐ Held-out inference запускался (не только static validation)?
☐ CPU, RAM, elapsed time — реальные числа, не null/UNKNOWN?
☐ gate_b_dotnet_test выполнен и pass count ≥ session floor?
☐ All metrics have metric_mode: "actual"?
```

**Метрики, которые ОБЯЗАТЕЛЬНО записываются для каждого accepted batch:**

```json
{
  "rows_trained": ...,
  "rows_held_out": ...,
  "elapsed_ms": ...,
  "cpu_avg_pct": ...,
  "cpu_peak_pct": ...,
  "ram_avg_mb": ...,
  "ram_peak_mb": ...,
  "training_loss": ...,
  "dpo_reward_margin": ...,
  "dpo_accuracy": ...,
  "runtime_product_accuracy": ...,
  "factual_accuracy": ...,
  "member_api_coverage": ...,
  "side_effect_recall": ...,
  "error_path_recall": ...,
  "call_edge_preservation": ...,
  "invented_behavior_count": ...,
  "missing_side_effect_count": ...,
  "missing_error_path_count": ...,
  "wrong_domain_routing_count": ...,
  "metric_mode": "actual",
  "next_batch_decision": "continue_same_level | promote_to_next_level | repair_same_batch | switch_stream | stop_stream",
  "gate_b_dotnet_test": {
    "floor_before": ...,
    "floor_after": ...,
    "verdict": "PASS | FAIL"
  }
}
```

**Pass 19 — Normalized cost metrics (из CODE-DOC-ACTIVE-GOAL-2026-05-13.md):**

Для каждого accepted batch дополнительно записывать:
```json
{
  "cost_metrics": {
    "cpu_ms_per_row": ...,
    "elapsed_ms_per_row": ...,
    "avg_peak_ram_mb_per_batch": ...,
    "normalized_peak_ram_mb_per_row": ...
  }
}
```

Замечание: peak RAM не кумулятивна (в отличие от CPU/elapsed). Использовать
avg_peak_ram_mb_per_batch как нормализованный pressure metric.

Если `metric_mode` ≠ `"actual"` — batch нельзя принять, нельзя промотировать.

### 3H. Two-stream design (Pass 5)

Для доменов с достаточным объёмом используется параллельная двух-потоковая стратегия:

**Stream 1: Hand-authored pairs**
- Пары написаны вручную для конкретных edge case / antipattern coverage
- Контролируемый и аккуратный источник
- Slower to produce, higher quality per pair
- Используется для начального обучения и coverage of corner cases

**Stream 2: Actual-extracted pairs**
- Пары генерируются из реально извлечённых структур (call graph edges, actual sentence pairs из corpora)
- Для code documentation: 284 methods, 277 actual call edges из реальных проектов
- Для translation: actual sentence pairs из Tatoeba dump с verified sentence IDs
- Scales much faster; tests generalization on actually occurring patterns

**Правило:** batch из extracted stream обязателен для финального gate.
Hand-authored пары могут использоваться для обучения, но held-out evaluation
должен включать actual-extracted examples для доказательства обобщения.

**Transit stages для code documentation stream (из CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE):**
```
T1 — leaf method (no callees, local behavior only)
T2 — helper with one accepted callee summary
T3 — caller/entry-point with accepted callee chain
T4 — workflow/module with accumulated callee summaries
T-F — framework lifecycle surface (Program.Main, Startup.Configure, etc.)
```

Каждый тип требует разных пар. Mixing без явного tagging = шумный batch.

### 3I. Two-session protocol для построения training set-ов (Pass 11)

Когда training set строится из существующих документов (не из corpus dump):

**Session 1 — Evidence inventory:**
1. Прочитать полный prompt/задачу перед любым действием
2. Извлечь все nodes и edges из целевой UML/flow диаграммы
3. Выбрать ОДИН источниковый документ (не derivative, не reference plan)
4. Evidence inventory: path, size, line count; поиск version copies; creation session
5. Записать: Gate 1A (prompt read) → Gate 1B (UML extracted) → Gate 1C (one doc selected) → Gate 1D (inventory) → Gate 1E (UML frame file)

**Session 2 — Per-node training sets:**
1. Для каждого node из UML frame: что node получает как runtime input, что production как output
2. ≥ 8 runtime A→B pairs per node
3. Проверить banned phrases в B column
4. Array node check (для array nodes: одна запись в A, один вывод в B)
5. Derived example marking: `[DERIVED from: section name]`
6. Gate 2F — one-document boundary: никакой контент из других документов

**9 nodes в orchestration flow (типичная структура):**
1. Domain Splitter (не array: разбивает запрос на части)
2. Domain Translator (array: переводит каждую часть)
3. Scheme Planner (не array: планирует подход)
4. Domain Processor (array: обрабатывает каждую часть)
5. Applier (array: применяет результат обработки)
6. Result Translator (array: переводит результат для вывода)
7. Arbiter Selector (не array: выбирает арбитра — ОТДЕЛЬНЫЙ NODE от Result Translator)
8. Arbiter Units (array: каждый арбитр проверяет результат)
9. Formatter (не array: форматирует финальный вывод)

---

### 3J. Протокол лингвистических правил — обязательные DPO-сигналы (RU/EN)

Лингвистические правила — это **обучающий сигнал, не код**.
Каждое правило должно появляться в тренировочных парах (chosen + rejected) и в held-out тестах.
Нельзя вписывать правила как `if/else` или словарь в производственный код.

Детальные примеры DPO-пар по правилам: `docs/plans/TRANSLATION-UNIT-DPO-PAIR-AUTHORING-GUIDE-v1.md §3B`.
Реестр rejected-пар по правилам: `docs/plans/GRAMMAR-RULE-DPO-REGISTRY-GUIDE-v1.md`.
Реестр источников правил: `docs/plans/LANGUAGE-RULE-SOURCE-REGISTRY-v1.md`.

---

#### Универсальные категории признаков для KNN-матрицы

| Группа признаков | Что хранить |
|---|---|
| Часть речи (POS) | noun, verb, adjective, adverb, pronoun, preposition, conjunction |
| Лемма | базовая форма слова |
| Словоформа | конкретная форма в тексте |
| Морфологические признаки | число, род, падеж, лицо, время, вид, наклонение |
| Синтаксическая роль | subject, object, modifier, predicate |
| Управление | какой падеж/предлог требует слово |
| Согласование | noun-adjective, subject-verb, pronoun-antecedent |
| Порядок слов | нормальный / инверсия / вопрос |
| Исключения | irregular forms, suppletive forms, spelling exceptions |
| Отрицание | not/no/never; не/ни |
| Вопрос | вспомогательный глагол, порядок слов, вопросительная частица |
| Модальность | can, must, should; можно, должен, следует |
| Степени сравнения | good→better→best; хороший→лучше→лучший |
| Коллокации | make a decision; принять решение |
| Фразовые/устойчивые выражения | look up, give in; иметь в виду |
| Пунктуация | commas, direct speech, clauses |
| Семантический класс | animate/inanimate, human, abstract, countable |
| Регистр | formal, informal, technical, legal, literary |

Canonical schema признаков — из Universal Dependencies (UD Features).
Все источники маппируются в UD-совместимые имена признаков.

---

#### Русские правила — обязательный перечень

**2.1 Падежи (Cases)**

| Падеж | Вопрос | Пример | Что проверять |
|---|---|---|---|
| Именительный | кто? что? | дом стоит | подлежащее |
| Родительный | нет кого/чего? | нет дома | отрицание, количество, принадлежность |
| Дательный | кому/чему? | дать брату, к морю | адресат, направление |
| Винительный | вижу кого/что? | вижу брата, вижу дом | объект; одушевлённость меняет форму |
| Творительный | кем/чем? | горжусь сыном | инструмент, предикатив, согласованный предикат |
| Предложный | о ком/о чём? | говорить о доме | предлоги о/в/на |

DPO-сигнал: `[RULE: case=dative]` + `[PHRASE: declined_form => english_with_preposition]`
Rejected-тип: `wrong_case`

**2.2 Род (Gender)**

| Род | Пример | Типичная ошибка |
|---|---|---|
| Мужской | большой дом | большая дом |
| Женский | большая книга | большой книга |
| Средний | большое окно | большая окно |
| Общий (зависит от контекста) | умница, сирота | — |

DPO-сигнал: `[RULE: agreement=gender_fem]`
Rejected-тип: `gender_agreement_error`

**2.3 Согласование (Agreement)**

Обязательные пары согласования:
- прилагательное + существительное (род, число, падеж)
- числительное + существительное
- местоимение + существительное (кот → он, кошка → она)
- подлежащее + сказуемое
- причастие + существительное

DPO-сигнал: `[AGREEMENT: субъект => местоимение]`; `[AGREEMENT: глагол_м => глагол_ен]`
Rejected-тип: `agreement_error`

**2.4 Вид глагола (Aspect)**

| Вид | Пример | Значение | Перевод |
|---|---|---|---|
| Несовершенный | читал / делал | процесс, повторяемость | was reading / was doing |
| Совершенный | прочитал / сделал | результат, завершённость | read (completed) / did |

DPO-сигнал: `[RULE: aspect=perfective]` / `[RULE: aspect=imperfective]`
Rejected-тип: `wrong_aspect`
Примечание: rejected здесь — не грамматическая ошибка, а неподходящее значение.

**2.5 Время (Tense)**

| Время | Пример |
|---|---|
| Прошедшее | я писал / написал |
| Настоящее | я пишу |
| Будущее простое | я напишу |
| Будущее составное | я буду писать |

DPO-сигнал: `[RULE: tense=past]` / `[RULE: tense=future_compound]`

**2.6 Согласование по роду в прошедшем времени**

| Форма | Окончание | Пример |
|---|---|---|
| Мужской | -л | забыл, ответил, пошёл |
| Женский | -ла | забыла, ответила, пошла |
| Средний | -ло | случилось, прошло |
| Множественное | -ли | ушли, пошли |

Machine stores both forms (-л and -ла) for the same English target.
DPO: `[AGREEMENT: забыл => forgot]` + `[AGREEMENT: забыла => forgot]`
Alternate form derivation: -ла → -л (fem to masc); -л → -ла (masc to fem).

**2.7 Лицо и спряжение (Person / Conjugation)**

| Лицо | Пример |
|---|---|
| 1 лицо ед.ч. | я пишу |
| 2 лицо ед.ч. | ты пишешь |
| 3 лицо ед.ч. | он пишет |
| 1 лицо мн.ч. | мы пишем |
| 2 лицо мн.ч. | вы пишете |
| 3 лицо мн.ч. | они пишут |

Исключения (irregular): хотеть → хочу/хочешь; бежать → бегу/бежишь;
есть (to eat) → ем/ешь; дать → дам/дашь.

**2.8 Одушевлённость (Animacy)**

| Тип | Пример |
|---|---|
| Неодушевлённое | вижу стол (Acc = Nom) |
| Одушевлённое | вижу брата (Acc = Gen) |
| Множ. одушевлённое | вижу студентов |
| Множ. неодушевлённое | вижу столы |

DPO-сигнал: `[RULE: case=accusative_animate]`
Rejected-тип: `wrong_case` (использована именительная форма вместо родительной у одушевлённого)

**2.9 Управление (Government)**

| Слово | Требуемый падеж | Пример |
|---|---|---|
| ждать | родительный / винительный | ждать поезда / ждать поезд |
| помогать | дательный | помогать брату |
| гордиться | творительный | гордиться сыном |
| нуждаться | в + предложный | нуждаться в помощи |
| благодаря | дательный | благодаря другу |
| согласно | дательный | согласно договору |

DPO-сигнал: `[RULE: government=dative_object]`
Rejected-тип: `wrong_case`

**2.10 Особые слова-исключения (Irregular / Exception Words)**

| Тип | Примеры |
|---|---|
| Нерегулярное множественное | человек → люди, ребёнок → дети |
| Беглые гласные | сон → сна, день → дня |
| Чередования | рука → ручной, друг → друзья |
| Несклоняемые существительные | кофе, метро, пальто, такси |
| Только множественное | ножницы, сутки, брюки |
| Только единственное | молоко, золото, молодёжь |
| Сложные числительные | 21 дом, 22 дома, 25 домов |
| Слова общего рода | умница, плакса, сирота |

Хранить отдельным словарём исключений; не вшивать в условия кода.
DPO-сигнал: `[RULE: exception=irregular_plural]`
Rejected-тип: `irregular_form_error`

---

#### Английские правила — обязательный перечень

**3.1 Времена (Tenses)**

| Группа | Пример | Русский эквивалент |
|---|---|---|
| Present Simple | I work | я работаю (обычно) |
| Present Continuous | I am working | я работаю (сейчас) |
| Present Perfect | I have worked | я поработал (результат важен) |
| Present Perfect Continuous | I have been working | я работаю уже (процесс + результат) |
| Past Simple | I worked | я работал / поработал |
| Past Continuous | I was working | я работал (в момент события) |
| Past Perfect | I had worked | я уже поработал (до другого прошлого) |
| Future Simple | I will work | я буду работать / поработаю |
| Going to future | I am going to work | я собираюсь работать |

DPO-сигнал: `[RULE: tense=present_perfect]`
Rejected-тип: `wrong_tense_aspect`

**3.2 Неправильные глаголы (Irregular Verbs)**

| Base | Past | Participle |
|---|---|---|
| go | went | gone |
| be | was/were | been |
| have | had | had |
| do | did | done |
| see | saw | seen |
| write | wrote | written |
| bring | brought | brought |
| think | thought | thought |
| take | took | taken |
| give | gave | given |

DPO-сигнал: `[RULE: exception=irregular_verb_go]`
Rejected-тип: `irregular_form_error` (rejected: goed, writed, bringed)

**3.3 Артикли (Articles)**

| Артикль | Пример | Значение |
|---|---|---|
| a / an | a book, an apple, an hour | один из многих; an — перед звуком гласной |
| the | the book | известный / конкретный |
| zero article | water, love, go to school | абстрактное / неисчисляемое / институциональное |

Русский не имеет артиклей — модель учит их из контекста.
DPO-сигнал: `[RULE: article=definite]` / `[RULE: article=indefinite]`
Rejected-тип: `wrong_article`

**3.4 Исчисляемые / неисчисляемые (Countable / Uncountable)**

| Countable | Uncountable |
|---|---|
| a book / books | water |
| an idea / ideas | information |
| a problem / problems | advice |
| a dollar / dollars | money |

DPO: `[RULE: countability=uncountable]`
Rejected: `an advice`, `an information`
Rejected-тип: `wrong_article`

**3.5 Subject-verb agreement**

| Правило | Пример |
|---|---|
| 3rd person singular | She works |
| plural subject | They work |
| existential there | There is a problem / There are problems |

DPO-сигнал: `[RULE: agreement=3sg_verb]`
Rejected: `He work every day.`
Rejected-тип: `agreement_error`

**3.6 Порядок слов (Word Order)**

| Тип | Пример |
|---|---|
| Statement | She likes coffee |
| Direct question | Does she like coffee? |
| Negative | She does not like coffee |
| Embedded question | I know where she lives (НЕ: where does she live) |

DPO-сигнал: `[RULE: syntax=embedded_question]`
Rejected: `I know where does she live.`
Rejected-тип: `wrong_word_order`

**3.7 Предлоги (Prepositions)**

| Английский | Типичная ловушка для русских |
|---|---|
| interested in | interested about |
| depend on | depend from |
| listen to | listen music |
| wait for | wait somebody |
| responsible for | responsible of |
| good at | good in |

DPO-сигнал: `[RULE: preposition=interested_in]`
Rejected-тип: `wrong_preposition`

**3.8 Модальные глаголы (Modal Verbs)**

| Правило | Пример |
|---|---|
| modal + base verb (no -s) | She can swim |
| no to after modal | must go, NOT must to go |

DPO: `[RULE: modal=no_third_person_s]`
Rejected: `He can speaks English.`
Rejected-тип: `agreement_error`

**3.9 Ложные друзья (False Friends RU↔EN)**

| Русское слово | Неправильный перевод | Правильный перевод |
|---|---|---|
| магазин | magazine | store / shop |
| аккуратный | accurate | neat / tidy |
| симпатичный | sympathetic | nice-looking / cute |
| записка | note (верно иногда) | не notebook, не memo всегда |

DPO-сигнал: `[RULE: exception=false_friend]`
Rejected-тип: `false_cognate`

---

#### Обязательные DPO-аннотации для лингвистических правил

```text
[RULE: case=accusative_animate]       — правило падежа с аниматностью
[RULE: aspect=perfective]             — вид глагола
[RULE: tense=past]                    — время
[RULE: agreement=subject_fem_past]    — согласование подлежащего с прошедшим временем
[RULE: exception=irregular_plural]    — нерегулярная форма
[RULE: exception=motion_verb_go]      — нерегулярный глагол движения
[RULE: article=indefinite]            — неопределённый артикль
[RULE: syntax=embedded_question]      — порядок слов во вложенном вопросе
[AGREEMENT: кот => he]                — местоимение-существительное привязка
[AGREEMENT: забыл => forgot]          — глагол муж.род → английский глагол
[AGREEMENT: забыла => forgot]         — глагол жен.род → тот же английский глагол
[PHRASE: declined_form => en_phrase]  — форма существительного в падеже → EN
[WRONG: error_type - weakness - gap - consequence]  — обязательная аннотация rejected
```

Каждая пара, затрагивающая лингвистическое правило, обязана нести хотя бы один `[RULE:]` тег в rejected.
Semantic-tokenizer: signature должна включать `RU:CASE:DATIVE`, `RU:ASPECT:PERFECTIVE` и т.д.

---

#### Обязательный состав тренировочного batch при наличии лингвистических правил

```text
Минимум для 100 тренировочных строк:
  20 строк — tense/aspect (вид и время глагола)
  20 строк — agreement/coreference (согласование, местоимение-антецедент)
  20 строк — case/preposition (падеж и управление)
  15 строк — exception-word (исключения: нерегулярные глаголы, plural)
  10 строк — article/countability (артикли, исчисляемость)
  10 строк — word-order (порядок слов)
   5 строк — false-friend / idiom (ложные друзья, идиомы)
```

Перед запуском удалить кандидатов с overlap ≥ 70% к истории обучения.

---

### 3K. Источники лингвистических правил — как находить для любого языка

Полный whitelist и алгоритм поиска: `docs/plans/LANGUAGE-RULE-SOURCE-REGISTRY-v1.md`.

Краткий приоритетный список:

| Tier | Источник | Для чего |
|---|---|---|
| 1 | Universal Dependencies (UD) | canonical grammar features + syntax, CoNLL-U format |
| 1 | UniMorph | morphology paradigms, irregular forms, lemma→form→features |
| 1 | Unicode CLDR | plural rules, numbers, date/locale behavior |
| 2 | LanguageTool (grammar.xml) | real error rules → negative DPO examples |
| 2 | OpenCorpora (для русского) | Russian morphology, declensions, paradigms |
| 2 | Wiktionary + Wiktextract | exceptions, inflections, translations, usage |
| 3 | Apertium | MT transfer rules, bilingual grammar rules |
| 3 | GF Resource Grammar Library | formal morphology/syntax generation |
| 4 | blogs/tutorials | только manual reference, не training source |

**Алгоритм поиска для любого языка `<LANG>`:**
1. Universal Dependencies `<LANG>` treebank → `github.com/UniversalDependencies`
2. UniMorph `<LANG>` → `unimorph.github.io`
3. CLDR plural rules `<LANG>` → `unicode-org/cldr`
4. Wiktionary `<LANG>` inflection tables → Wiktextract
5. LanguageTool `<LANG>` grammar.xml → `github.com/languagetool-org`
6. Apertium `<LANG>` → `github.com/apertium`
7. GF RGL `<LANG>` → Grammatical Framework

Для каждого источника обязательны поля: `source_id`, `language`, `license`, `feature_types`, `commercial_allowed`, `attribution_required`, `share_alike`, `allowed_for_training`, `allowed_for_distribution`.

---

## Шаг 4 — Цикл обучения 100+100

### 4A. Схема цикла

```
Batch N (100 пар) → конвертация в DPO-пары → ОБУЧЕНИЕ всех 3 unit-ов
        ↓
Batch N+1 (100 пар, НИКОГДА не видевших обучение) → ТЕСТ
        ├─ runtimeProductAccuracy ≥ 0.95 → ГОТОВО → идём в domain fine-tune
        └─ runtimeProductAccuracy < 0.95 → N+1 → в обучение, N+2 → тест
                                           ПОВТОР без остановки
```

**Pass 18 — Status report format during cycle (из EXECUTOR-PROMPT-CODE-DOC-SESSION):**

Во время цикла каждый статус-отчёт должен быть в формате:
```
Cycle: <stream-id>/batch-<NNN>
Stage: source | build_pairs | validate | train | evaluate | gate_b | report
Pairs: <built>/50 train, <heldout>/50 held-out
Repair-required: <n>
Elapsed: hh:mm:ss
CPU: <avg%> / peak <peak%>
RAM: <peak MB>
Current metric: <name=value>
Current verdict: pending | accept | repair | reject | pairset_ready
```

**Pass 6 — 500-row checkpoint rule (из CODE-DOC-ACTIVE-GOAL-2026-05-13.md):**

Каждые 500 новых training rows обязателен качественный checkpoint:
- Запустить quality check на всех validation sources/projects
- Записать CPU, RAM, routing/domain accuracy, documentation/translation quality
- Checkpoint должен включать representative held-out примеры из ВСЕХ sources/subprojects

### 4B. Инварианты цикла

**Инвариант 1:** тестовый batch — всегда СЛЕДУЮЩИЕ 100 предложений, которых модель никогда не видела.
Это проверяет обобщение, не запоминание.

**Инвариант 2:** corpus source — выбор пользователя, не оптимизация исполнителя.
Нашёл "похожие старые данные" — это НЕ замена требуемого источника.
Зафиксированное нарушение: 2026-05-29, ветка `codex/chat-gpt-5.5`.
Полный разбор: `docs/skills/translation-domain-reality-check-SKILL.md §FAQ-12`

**Инвариант 3:** обязательная проверка контаминации перед каждым результатом.
Открыть training файл и held-out файл. Сравнить `input_text` vs `a_side` построчно.
Если любой `input_text` == любой `a_side` — это меморизация, не обобщение.
Отчёт: `CONTAMINATION_DETECTED`. Gate не засчитан.

**Инвариант 4:** `runtimeProductAccuracy = 1.000` всегда подозрительно.
KNN запоминает точные сигнатуры. 1.000 на unseen невозможен без контаминации.

### 4C. Gate B: что значит "прошёл"

Предложение засчитано как прошедшее gate если:
1. Модель вернула перевод (НЕ Bootstrap)
2. Confidence ≥ 0.5
3. Когда есть эталон: BLEU-1 ≥ 0.60

`runtimeProductAccuracy ≥ 0.95` = минимум 95% предложений из тестового batch прошли все три условия.

### 4D. Диагностика если gate не пройден к итерации 50

Если к итерации 50 (5 000 обучающих предложений) `accuracy < 0.95` —
проблема НЕ в нехватке данных.

Проблема в отсутствии структурного KNN-каскада. Нужно реализовать:
```
Full signature match → Prefix signature match → Structure signature match
→ Component overlap match → BootstrapFallback
```

Добавление ещё данных без исправления каскада не поможет.

### 4E. Запрет на остановку до 95%

Цикл 100+100 — это не план на несколько шагов. Это единый непрерывный процесс.

**Запрещено:**
- Останавливаться потому что batch committed
- Останавливаться потому что review findings зафиксированы в документах
- Переходить к domain fine-tuning до прохождения base 95% gate
- Использовать старые batch-файлы как "замену" для новых unseen данных

После любого governance repair — немедленно вернуться к первому открытому gate плана.

### 4F. 10-batch inventory и depth coverage check (Pass 7)

После каждых 10 accepted batch-ей дополнительно запускать:

**Inventory coverage:**
- Методы/пары/записи покрытые хотя бы один раз
- Edges покрытые хотя бы один раз

**Depth coverage — что отсутствует:**
- Missing positive-ready examples (примеры с complete context)
- Missing negative-gate examples (примеры намеренно нарушающие gate)
- Missing D4/workflow/full-pipeline examples
- Missing positive-ready edges/transitions
- Missing partial-coverage negative examples

**Сравнение с предыдущим 10-batch checkpoint:**
Если coverage_improved = false И depth_improved = false:
- Запустить gap analysis script
- Назвать конкретный отсутствующий resource/change перед генерацией следующих batch-ей
- НЕ генерировать более batch-ей по тем же pattern-ам пока gap не назван

---

## Шаг 5 — Domain Fine-Tuning (только после Шага 4)

### 5A. Архитектура копирования

```
Базовая модель (3000 слов × общий язык)
    ↓ gate: runtimeProductAccuracy ≥ 0.95
    ↓ КОПИРОВАТЬ веса (не учить с нуля!)
    ↓
Domain fine-tune (тот же цикл 100+100, domain corpus):
  Legal     → UN Parallel Corpus
  Literary  → Gutenberg + Wikisource PD
  Medical   → PMC OA CC0/CC BY
  Technical → GNOME/KDE/Ubuntu localization
```

**Ключевое правило:** новый профессиональный домен **никогда** не учится с нуля.
Он стартует с весов базовой модели и добавляет domain-specific DPO-шард.

### 5B. Gate после domain fine-tuning

После доменного обучения обязательно проверить:
- Domain gate: `runtimeProductAccuracy ≥ 0.95` на domain corpus
- Regression check: базовая точность не упала ниже **0.85**

Если базовая точность упала — learning rate домена был слишком высокий. Уменьшить и повторить.

---

## Шаг 6 — Проверка успешного обучения (reальные held-out промпты)

Тесты не доказывают, что обучение сработало.
`dotnet test` — это contract/static verification, не доказательство реального перевода.

**Реальный экзамен: живые пользовательские промпты**

Для TranslationDomain примеры успешных held-out промптов:
1. "Переведи этот пост для сети: 'Сегодня я закрыл сделку на 150к$. Клиент в восторге!' → English, сохрани эмоциональный тон и эмодзи."
2. "Сделай естественный перевод чата: 'Брат, как ты вообще это реализовал? Я уже третий день мучаюсь с этим роутером' → English, разговорный стиль."
3. "Переведи маркетинговый текст: 'Наша система помогает аффилиатам зарабатывать пассивно без холодных звонков' → Hebrew."

**Gate B для held-out промптов:**
- Вывод НЕ начинается с `[DIRECTION:BOOTSTRAP]`
- Тон, смысл, named entities, числа сохранены
- Back-translation accuracy ≥ 0.85 (для перевода)
- Нет wrong-direction output

---

## Антипаттерны (зафиксированные нарушения)

| Антипаттерн | Почему запрещено | Правильное действие |
|-------------|-----------------|-------------------|
| Показать `[DIRECTION:BOOTSTRAP]` как перевод | Пользователь не может это использовать | Сказать честно, что нет обученных пар |
| `accuracy = 1.000` без проверки контаминации | KNN не может дать 1.000 на unseen данных | Проверить contamination первым |
| Заменить Tatoeba на "старые batch-файлы которые уже есть" | Тест на знакомых данных = меморизация, не обобщение | Использовать именно указанный источник |
| Сообщать gate пройден потому что артефакты выглядят правильно | Тесты ≠ рабочий перевод | Запустить живой вывод |
| Остановиться после review/governance repair | Документация о том что делать ≠ делать это | Продолжить с первого открытого gate |
| Обучать только один unit из трёх | Все три unit-а должны быть обучены | Обучить все три |
| Начинать domain fine-tune до base 95% gate | Домены наследуют от базы — без базы нечего наследовать | Завершить base loop сначала |
| Пометить prepared pairset как accepted batch | PAIRSET_READY ≠ ACCEPT_BATCH; нет реального обучения и inference | Статус `PAIRSET_READY_TRAINING_PENDING`; обучить и оценить |
| `metric_mode: "projected"` в отчёте принять как доказательство | Проецируемые метрики — не результат реального обучения | Запустить реальный runner; metric_mode должен быть "actual" |
| Использовать ledger-строки как training data | Модель учится обрабатывать ledger, а не runtime запросы | Пары должны выглядеть как runtime input → runtime output |
| overlap_ratio > 0.60 между chosen и rejected | Слабый DPO сигнал; пары слишком похожи | Переписать rejected с более конкретной ошибкой |
| source_id без traceable repo + commit SHA | Нельзя воспроизвести и верифицировать данные | Указать repo URL + exact commit SHA + file_path |
| Учить L3/L4 без accepted batch для L1/L2 | Верхние уровни требуют накопленных фактов снизу | Снизу вверх: L1 → gate → L2 → gate → L3 → gate → L4 |
| Смешивать несколько документов/стеков в одном batch без tagging | Шумные batch-и сложнее диагностировать | Один stream = одна комбинация (stack × level × context) |
| Rejected side — generic negation ("this is wrong") без [WRONG:] | Нет специфического обучающего сигнала | Concrete `[WRONG: weakness - gap - consequence]` |
| Описание orchestration как inventory | "Flow X добавляет 14 factories и 8 task types" ничего не объясняет | "Flow X учит движение работы от X к Y через Z guards" |
| Документировать array unit как если он non-array | Array nodes: одна запись → один вывод; mixed output = invalid pair | Явный tagging; split на отдельные пары |
| Построить 10 batch-ей без coverage gap check | Паттерны повторяются, coverage не растёт | После каждых 10 batch-ей: inventory + depth check |
| Строить pairs из B-column с запрещёнными фразами | "translate it", "plan it", "a translated version" — не реальный вывод | Переписать B с полным actual output |
| Promotion без 6-condition gate | Один achieved accuracy ≠ готовность к следующему уровню | Все 6 conditions: stable batches × 2, diagnostics, confirmation batch, margin > 0, metric_mode actual |

---

## Порядок загрузки документов в новой доменной сессии (Pass 17)

**11-step document use protocol (из EXECUTOR-PROMPT-CODE-DOC-50-PAIR-LEARNING-v1.md):**

Написать `DOCUMENT-USE-LOG.md` в run директории по мере чтения. Не пропускать документы.

| Order | Документ | Для чего |
|-------|---------|---------|
| 1 | `<Domain>-MINDSET-FAQ-v1.md` или `TRANSLATION-MINDSET-FAQ-v1.md` | Поведенческий якорь — читать первым |
| 2 | `<Domain>-SOURCE-REGISTRY-v1.md` | Верифицировать license proof для каждого source row |
| 3 | `<Domain>-CALL-TREE-ORCHESTRATION-PLAN-v1.md` | Bottom-up propagation, runtime contract, node применимый к batch |
| 4 | `<Domain>-FOUR-LEVEL-TRAINING-PLAN-v1.md` | Текущий уровень, pair mix для 50 пар, promotion gate |
| 5 | `<Domain>-50-PAIR-ABLATION-RUNBOOK-v1.md` | Операционный порядок: build → hard gates → state update → next-batch decision |
| 6 | `<Domain>-DPO-REGISTRY-GUIDE-v1.md` | Присвоить pair type (CD-A через CD-F) каждой строке |
| 7 | `<Domain>-DPO-PAIR-AUTHORING-GUIDANCE-v1.md` | Написать chosen и rejected sides правильно |
| 8 | `<Domain>-EXAMINATION-METHOD-v1.md` | Оценить каждую held-out строку и принять решение о batch |
| 9 | `<Domain>-DPO-REJECTED-PROMPTS-v1.md` | Создать или починить rejected sides; не использовать generic "bad answer" |
| 10 | `<Domain>-LOOK-LIKE-RUN-REPORT-TEMPLATE-v1.md` | Написать human-readable отчёт |
| 11 | `<Domain>-SMALL-MODEL-RATIONALE-v1.md` | Мотивация и framing задачи — не authority для gates |

---

## Связанные документы

| Документ | Содержимое |
|----------|-----------|
| `docs/plans/TRANSLATION-CORPUS-SOURCING-PLAN-v1.md` | Источники данных, whitelist, 100+100 loop §13, pilot §16 |
| `docs/plans/TRANSLATION-DOMAIN-TRAINING-GUIDE-v1.md` | Руководство по обучению TranslationDomain |
| `docs/plans/TRANSLATION-UNIT-DPO-PAIR-AUTHORING-GUIDE-v1.md` | Формат DPO-пар для трёх unit-ов; §3B — правила языка, G01–G10 |
| `docs/plans/GRAMMAR-RULE-DPO-REGISTRY-GUIDE-v1.md` | Реестр DPO-пар по лингвистическим правилам (G01–G10) |
| `docs/plans/LANGUAGE-RULE-SOURCE-REGISTRY-v1.md` | Whitelist источников языковых правил; алгоритм поиска для любого языка |
| `docs/plans/PARALLEL-TEACHING-AND-EVALUATION-GUIDANCE-v1.md` | Параллельный запуск 5 направлений |
| `docs/plans/PARALLEL-DOMAIN-TRAINING-ROADMAP-v1.md` | Дорожная карта 5 направлений |
| `docs/TRANSLATION-MINDSET-FAQ-v1.md` | Переломный момент + новые антипаттерны (v1.1) |
| `docs/skills/translation-domain-reality-check-SKILL.md` | Bootstrap, KNN, 100+100, anti-determinism |
| `00.Core/LearningMachineContracts.cs` | ILearningMachine<T>, ILearningMachinePair<TP,TG> |
| `27.TranslationDomain/` | Первая реализация: TranslationMachine.cs, unit-ы, KnnStore |
