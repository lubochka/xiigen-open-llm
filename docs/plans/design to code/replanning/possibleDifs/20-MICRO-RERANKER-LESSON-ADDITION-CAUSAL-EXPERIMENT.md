# MICRO-20.01 — малый причинный опыт: деградирует ли обученная память от нерелевантного нового урока?

Дата: 2026-10-16. Статус: **PLAN + runnable analytical proxy; native C# runner NOT CREATED; native/Student blind run NOT RUN**.
Сопроводительный источник: [NEW-20.01 / NEW-20.02](20-LESSON-ADDITION-RERANKER-NONLOCAL-QUALITY-GAPS-2026-10-16.md).

## Проблема, минимальная формулировка

Если KNN Unit выучил правильный пример A, новое нерелевантное знание C **не должно превращать ответ в ошибочный B** при неизменном запросе и законе. В `TransformerEmbeddingKnnLogicalUnit.ScoreCandidates` lexical score нормируется на максимум **текущего пула** (а не замороженной шкалы). Поэтому при дополнительном кандидате шкала старых A/B способна поменять порядок: [source lines 217–244](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L217-L244).

**Гипотеза H20.1:** локальная замена pool-relative `maxLexical` на **DEV-frozen** положительный lexical-reference уменьшает old-lesson semantic regressions после добавления новых примеров. Это проверка **обученной памяти**, НЕ теоретическое доказательство улучшения LoRA/DPO Student.

## Первый этап: дешёвый proxy — уже выполнен

```bash
node 20-RERANKER-NONLOCAL-PROXY.mjs > 20-RERANKER-NONLOCAL-PROXY-RESULT.json
```

`PROXY_ONLY`. Скрипт воспроизводит формулу коэффициентов текущего C# scorer; не запускает C# Unit и не выдумывает реальные embeddings/BM25.

- До C: A(correct)=0.8425, B(wrong)=0.76056 → A.
- После C: B(wrong)=0.633, A(correct)=0.5555, C(irrel)=0.35 → B; новый пример C никогда не выбран.
- При фиксированном reference=0.9, до и после A остаётся победителем.
- Контроль NEW-20.02: `UNKNOWN==UNKNOWN` добавляет ровно 0.20, хотя структура неизвестна; гипотетический wrong победитель при этом bonus меняется на correct после его подавления.

Файл [proxy-result.json](20-RERANKER-NONLOCAL-PROXY-RESULT.json) — **только аналитика**, не настоящий empirical semantic learning. Следующий шаг нельзя «закрыть» этим proxy.

## Второй этап: native matched Unit-quality pilot (основной, быстро реализуемый)

**ExperimentId:** `MICRO-20.01`; **Tier:** 2 (всего 32 случая = 12 TRAIN + 4 DEV + 16 HELDOUT на первичный pilot; при устойчивом положительном сигнале расширить до Tier3 отдельно).

**Место кода:** не менять RP01 500-triple / frozen checkpoint / production control. Сделать небольшой диагностический runner либо узкий internal scorer seam и тестовый host только для `18.CapabilityTraining/LlmIngredientLogicalUnits.cs`. Стабилизировать модели/ids.

**Данные и происхождение:**
- 12 TRAIN (8 основных positive/contrast examples, 4 независимо нерелевантных incremental lessons C) из 3–4 групп, например C# authorization vs no-auth, SQL NULL / NOT NULL, ENRU actor swap. Разные source IDs, явный domain-law, `correctOutput` и `wrongOutput` только у независимого judge; Unit index хранит **только допустимые TRAIN**.
- 4 DEV — настройка единственного **positive lexical reference** `L_dev`; зафиксировать до чтения heldout и не выбирать L_dev по HELDOUT. Если >1 DEV reference намерение — отдельный опыт, не этот.
- 16 blind HELDOUT — source-disjoint и template-disjoint; для retention — новые формулировки тех же доменных законов, для transfer — отдельные не встречавшиеся ранее semantic families. Минимум 4 с отрицанием/порядком ролей и 4 с критическим доменным законом; opaque `caseId`, forbidden test-to-train dedupe. Две группы: старый навык для проверки сохранения и новая область для проверки пользы урока. Указать `dataset_sha256` и проверить отсутствие overlap до запуска.
- Строки обрабатываются теми же source inputs и frozen verdict oracle во **всех** arm; оценки независимы от имени arm.

**Строго одно изменение:** сравнение ниже касается **только нормализации lexicalScore**. В обоих arm фиксированы tokenizer SHA, embedding model/checkpoint SHA, raw vectors, lexical raw scores кандидатов, предсказанные structural pattern, кандидатные IDs, `candidateRecall`, `k`, weights 0.45/0.35/0.20, seed и grader. Для первого доказательства вызвать **чистый scorer** на *одних и тех же frozen candidate arrays*, не строить HNSW заново. Это исключает ANN graph, encoder drift и BM25-IDF изменения. После этого допускается отдельная shadow-проверка настоящего HNSW Unit (она не считается тем же one-factor опытом).

| Группа | Статус памяти | Формула lexical | Цель |
|---|---|---|---|
| A0 | TRAIN базовая, **без C** | Current `lex/maxLexical(pool)` | Initial baseline old skill |
| A1 | TRAIN базовая + C | Current `lex/maxLexical(pool)` | Найти old-skill regression при добавлении урока |
| B0 | **те же** базовые candidates, что A0 | `clamp(lex/L_dev,0,1)` | Проверить отсутствие изначального скрытого gain/penalty |
| B1 | **те же** A1 candidates с C | `clamp(lex/L_dev,0,1)` | Изолировать влияние одного denominator |

`A0/A1` проверяет реальный эффект добавления памяти в current policy; `A1/B1` — разницу политики на идентичных примерах и input traces. `A0/B0` нужен, чтобы новая формула не маскировала фундаментальный сдвиг до урока. В tiny C# scorer test дополнительно использовать ровно синтетическую запись A/B/C из companion document для точного parity (не засчитывать как heldout).

**Первичный показатель:** `paired_retention_accuracy/16` (**реальный выбранный stored output vs независимый доменный oracle**), отдельно `wrong_after_lesson_count` — сколько прежних правильных ответов стали неправильными после C. Вторичные: `correct_after_lesson`, new-lesson gain, false-safe, coverage, latency, `winnerId`, `top3Ids`, `lexicalRaw`, `maxLexical`, `patternClass`, `model_sha`, `vector_sha`. Не заменять семантический oracle cosine/reward margin.

**Оговорка об ожидаемом результате:** при первом exploratory 16-case pilot разница хотя бы на **2–3 source-disjoint cases** при нуле law violations полезна как сигнал `REPRODUCE`, но это не статистическая общая победа; положительное утверждение `causal_unit_gain` допускается **только на этом Unit**, после независимого blind review + повторение на расширенной family-disjoint панели.

**Негативные контроли:** (1) на frozen raw lexical scores добавление C с lexicalScore ниже прежнего max не должно само менять A/B ранги через denominator (отдельный native rebuild может менять BM25 IDF); (2) permuted gold labels judge — преимущество по независимой точности должно пропасть, хотя score invariance может сохраниться; (3) wrong C не должен становиться выбранным, если он нерелевантен; (4) correct A должен остаться в **одном и том же fixed candidate set**. Все случаи разбивать по source/semantic-family, не random lines.

**Фальсификаторы:** native scoring не совпадает с proxy → проблема в исходном предположении; при настоящих corpus C не входит в ANN candidates или у C lexicalScore не может превысить старый pool max → `NOT_REPRODUCED_NATIVE` для этого корпуса; B1 не уменьшает wrong-after или ухудшает закон → `NO_QUALITY_GAIN`; реальный ProductHost не вызывает этот Unit → `RP01_CONSUMER_NOT_ATTRIBUTABLE`.

## Отдельный мини-опыт NEW-20.02 (не смешивать с H20.1)

**ExperimentId `MICRO-20.02-UNKNOWN`**, 8 русских/ивритских + 4 английских law-specific blind queries; 6 TRAIN law cases; один frozen candidate set. Формулы одинаковы, кроме **0.20 pattern score при `UNKNOWN==UNKNOWN`**: A=current, B=score 0 если хотя бы один pattern UNKNOWN. Проверить decoded unicode input, фактические roots, pattern labels, winner, law accuracy, confidence, false-safe; известные English pattern matches — отрицательный контроль. Не позволять evaluator создавать labels из query. Сначала static score parity, затем Unit ExecuteAsync. **Не собирать два вмешательства в один arm.**

## Где хранить реальные результаты и что является доказательством

```text
results/micro20/<run-id>/
  manifest.json       # branch/core+planning SHA, source IDs/digests, heldout barrier, settings
  candidates.jsonl     # identical candidate IDs, vectors sha, raw scores, patternClass by arm
  per-case.jsonl       # blind IDs, prediction, oracle grade, old/new change
  summary.json         # paired accuracy, wrong_after_count, negative controls, coverage/cost
  consumer-trace.json  # native Unit ExecuteAsync or ProductHost invoked / NOT_INVOKED
  reviewer-verdict.md  # independent readback and law review, or NOT_PERFORMED
```

No Windows CI. Не запускать 500 пар; не менять принятую production model, не дописывать старые frozen оценки. На native уровне достаточно узкого `dotnet test --filter ...` или маленького project runner **если среда поддерживает dotnet**; при невозможности честно `BLOCKED_NO_DOTNET`, а не `PASSED`.

## Исполнительский work order для CODEX (один ограниченный запуск)

**CURRENT TASK:** создать `MICRO-20.01` diagnostic в отдельной ветке/isolated workspace; source map `ScoreCandidates→ExecuteAsync→ProductHost caller`; использовать CORE версии, присутствующие в manifest. **NOT TASK:** исправлять всю матрицу, разрабатывать новую LoRA, менять RP01 research artifacts, переписывать HNSW или подгонять grader.

**Skills:** `docs/skills/architect-behavior-classifier-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `experiment-design-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `conversation-intent-and-self-reflection-SKILL.md`, `claim-verification-SKILL.md`, `training-data-gap-audit-SKILL.md` (в `llm_mvp_core/codex/dpo-implementation`). Архитектор/ревьюер проверяют план/дифф и не редактируют коды за исполнителя.

**Allowed:** read-only core/reference/old experiments; создать отдельный narrow diagnostic scorer + fixture + result files; native concise deterministic compare; одна новая candidate ветка по обычным правам. **Forbidden:** modifying acceptance/hidden grader to win, historical `scientific-result.json`, 500 input files, source licensing, production promotion, broad Windows CI, misleading AI judge provenance. Если нет полномочий commit, работать только в разрешённом isolated path; не обходить write policy.

**Independent verification:** иной ревьюер подтверждает frozen inputs и единственный denominator diff, правильность `source→code→behavior`, negative controls и реальный `unit_returned_output`, затем (только если в задаче есть authorized external model) роль выбора lesson = CODEX/Astra 6 UltraCode, обе финальные DPO стороны = лицензированная внешняя open Chinese model, оценка каждой пары = CODEX/Astra; **ни одна из этих ролей не запускалась в этом аудите**. Этот эксперимент KNN не обязан создавать DPO.

**Return packet:** `candidate_sha | source_sha | dataset_sha | fixed_control_ok | native_executed | analytic_proxy_executed | actual_accuracy_A0/A1/B0/B1 | wrong_after | negative_controls | independent_review | ordinary_consumer_invoked | limits | verdict`. При `NOT_RUN` не ставить PASS.

## Критерий решения и STOP

- Если native scorer не воспроизводит алгебру → STOP source-contract mismatch, исправить документацию, не алгоритм.
- Если native scorer воспроизводит flip, но blind quality нет → `MECHANISM_ONLY`, без product claim.
- Если blind Unit quality положительна, а actual ProductHost bypass → исправлять отдельно consumer routing по [18](18-PRODUCTHOST-GENERATION-RETRIEVAL-BYPASS.md), не приписывать это DPO Student.
- Только если одинаковый ordinary request использовал именно updated Unit и показал independently graded gain — `CONSUMER_GAIN`. Ни один исход Unit не доказывает, почему RP01 Transformer free-response был 0/3 и 0/4, без matched Student control.

**Статус этого документа на 16.10.2026: DESIGN_COMPLETE; NODE_ANALYTIC_PROXY_PASSED; NATIVE_CSHARP_NOT_RUN; NO_REAL_BLIND_GAIN_CLAIM.**
