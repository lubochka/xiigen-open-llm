# MICRO-FUSION-20 — дешёвое причинное исследование качества при росте KNN памяти

**Статус 16.10.2026: PLAN+NUMERIC-PROXY; настоящий C# Unit и Student НЕ запускались.** Цель не «ещё один хороший тест», а выяснить, может ли полезное добавление уроков к hybrid KNN менять уже правильный ответ из-за ранжировки и цены вычисления. Один основной фактор: *состав обученной памяти*, плюс диагностическое математическое разложение эффекта. Не сравнивать с RP01 DPO-optimizer как с тем же learner.

## До кода — критерий человеческим языком

Проверяем старую хорошую память на 12 новых случаях; затем добавляем несколько **корректных, но не предназначенных для этих 12 вопросов** уроков. Если прежние ответы начинают ухудшаться, выясняем, исчезло ли нужное знание или ranker сделал его невидимым. В отдельной проверке используем **одинаковые** готовые candidates и спрашиваем, переворачивает ли решение одна формула maxLexical. Не нужно запускать 500 DPO или Windows CI.

## Состав и источники

- Production seam: [TransformerEmbeddingKnnLogicalUnit.Train/ExecuteAsync/ScoreCandidates](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L91-L272). Вектор index [HnswIndex.Search](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/06.VectorSearch/HnswIndex.cs#L99-L111).
- Frozen direct proxy: [MJS](20-FUSION-CANDIDATE-NORMALIZATION-PROXY.mjs) и [JSON](20-FUSION-CANDIDATE-NORMALIZATION-PROXY-RESULT.json); воспроизводимая локальная команда из каталога possibleDifs: `node 20-FUSION-CANDIDATE-NORMALIZATION-PROXY.mjs`.
- Исторический контроль: [E34V2 comparison](09-HISTORICAL-EXPERIMENT-VALIDITY-AND-FUSION-DRIFT.md); baseline E34 имеет старые unmatched subsets, не использовать его PASS как causal proof.
- Правила исполнения: [experiment-design-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/experiment-design-SKILL.md), [human-readable-architecture-audit-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/human-readable-architecture-audit-SKILL.md), [plan-review-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/plan-review-SKILL.md), [generated-code-review-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/generated-code-review-SKILL.md), [prompt-patch-authoring-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/prompt-patch-authoring-SKILL.md).

## Frozen fixture и независимость

CODEX/Astra 6 UltraCode выбирает 12 коротких source-disjoint heldout запросов и 12 исходных train уроков на 2 домена (6 C# documentation, 6 EN→RU translation или эквивалентные задачи, поддерживаемые **одним реально типизированным TOutput**). При несовместимости типов запускать две изолированные шестёрки с разными Unit IDs; не смешивать их в один невалидный typed Unit. Ещё 4 новых законных distractor-урока должны иметь сходные root слова, но иную domain law/family. Каждый train label и heldout expected law утверждён независимо **до** запуска; нормировка, rubric и frozen source hashes никогда не меняются по результату. Нет exact повторов нормализованных source, family IDs и запрещённых exam hints. В JSON-карточке фиксировать `case_id, source_family, domain_law, prompt_hash, expected_law, forbidden, label_origin`.

## Arms — не менять несколько переменных за раз

| Arm | Данные | Код/модель | Проверяемое |
| --- | --- | --- | --- |
| B0 | Только 12 train, 12 blind heldout | original current hybrid KNN | Исходное качество |
| B1 | Те же 12 + 4 выбранных корректных distractor lessons | тот же current hybrid KNN, та же frozen embedding model/tokenizer, полностью rebuilt index | Изменения пользователя при естественном росте памяти (может включать IDF, index graph, нормировку) |
| N0 (negative) | Те же 12 + 4 unrelated LOW-lexical distractors | тот же путь | Не превращать любой memory append в предсказанную деградацию |
| S0 (shadow only) | **Точно те же frozen ranked hit IDs, vector/raw-BM25/pattern values из B1** | Копия текущей формулы (0.45/0.35/0.20, max candidate raw lexical) | Воспроизведение B1 rank при исходных компонентах |
| S1 (shadow only) | **Те же B1 raw values и кандидаты** | Фиксированная заранее lexical scale, никакой корректировки labels | Изолированная оценка одного изменения normalization; не выпускать в продукт по одному proxy |

B0↔B1 — причинный тест влияния **добавления памяти** на конечный consumer (несколько mediator effects). S0↔S1 — один изменяемый фактор norm при идентичных raw features (механистический тест). Иначе сравнение index rebuilt с новой IDF и старой ranker формулой ложно приписало бы всё maxLexical.

**Критически:** достать точные raw scores из живого scorer через opt-in trace в экспериментальном harness, без изменения normal serving и скрытой подстановки expected. Отдельно сохранять queried unit ID, selected example ID, origin of final output, hash frozen model+tokenizer+index dataset, warning/abstention. Если обычный ProductHost.GenerateAsync не вызывает Unit, **не считать** такой endpoint проверкой hybrid quality. Чтобы проверить пользовательское влияние, использовать явный diagnostic composed consumer с реальным ExecuteAsync и shadow comparator; frozen generator не должен узнавать ground truth.

## Метрики и pre-declared decision

Публиковать **каждый ответ** и `correct_lawful / 12`, `newly_wrong`, `newly_correct`, `forbidden_violation`, `coverage`, `abstain`, `rank_flip_old_pair`, `explainable_flip_by_maxLexical`; для двух доменов — отдельно по 6 и worst-domain, не только macro-average. Визуально и машинно сопоставлять source, raw candidate rank и emitted answer. Слепой CODEX/Astra 6 UltraCode judge не видит arm при оценке; не менять оценочные пороги после результата.

- Исследовательский trigger: B1 теряет ≥2 ранее правильных heldout относительно B0 **и** S0/S1 различаются минимум на одном том же source case; это основание для нового расширенного исследования, **не статистическое доказательство**.
- Если B1 хуже, но S0=S1: отложить normalizer, изучить corpus BM25 IDF / embedding / ANN candidate changes и changed flow.
- Если есть S0/S1 rank flip, но B0=B1: механизм существует, но impact на выбранных heldout **не наблюдался**.
- Если B1 лучше, не называть рост памяти плохим; измерить gain/retention и продолжить домены.
- Если нет реального обращения к Unit или есть утечка expected, outcome = INVALID_EVIDENCE / NOT_RUN, не PASS.

## Отдельная performance-проба N20.02

Для размеров memory N=8/16/32/64, одинакового frozen heldout и warmup, выполнить только query без DPO шагов. Логировать latency p50/p95, managed allocation, число Tokenize вызовов в `ComputeBm25LikeScore`, `_index.Search` visits, timeout/fallback на реальном диагностическом consumer, и число кандидатов. Эквивалентный вариант с кэшированными document token vectors, df и avgLength — только SHADOW, с одинаковой семантикой scorer. Итогом должна быть quality-vs-cost таблица; не путать рост времени с loss нейросети.

## Обязательный результат и бюджеты

Один локальный маленький C# diagnostic harness под именем MICRO-FUSION-20 (создать только при исполнении); максимум 12 train + 12 heldout + 4 distractors и до 64 memory items для performance; 0 optimizer updates; без cloud/Windows CI и без полного набора тестов. После одного compile и одного focused run сохранить `MICRO-FUSION-20-RESULT.json` с полями `repo_sha, parent_state_sha, rubric_sha, records_per_arm, input_ids, output_sha, ranked_candidates, raw_score_terms, true_positive, critical_errors, runtime_ms, allocation, status, run_command, readback_sha`. Если runner не готов — фиксировать `NOT_RUN`, нельзя переименовать Node proxy в C# result.

## CODEX executable handoff

Исполнитель = CODEX с GPT-6 Astra 6 UltraCode. Сначала применить architect, planning, planning-review, code-review, prompt-engineering, human-communication, experiment-design и learning-path-audit skills из core/docs/skills. Прочитать frozen branch sources и предыдущие possibleDifs (в особенности 09, 12, 16, 18), обнаружить доступный local csproj/harness; добавить лишь диагностический code path, не менять production scorer. Провести source-first review -> one bounded execution -> per-case output -> independent quality review -> one publish/readback. Если текущий interface не позволяет сбор trace без production patch, зафиксировать точную границу и реализовать read-only shadow, не выдавая его за ordinary consumer. Отдельно проверить no oracle leak, source-family disjointness, frozen labels, exact output hashes, no credential/paid model calls.

Если в будущем добавляются новые DPO: **человеческий выбор задачи делает CODEX с Astra 6 UltraCode**, **обе стороны chosen/rejected пишет разрешённая внешняя открытая китайская модель**, **качество обеих сторон и пары отдельно оценивает CODEX с Astra 6 UltraCode**. Ни в одной из описанных выше KNN-проб внешняя модель не нужна и не вызывалась.

**Review statuses now:** protocol DRAFT, source path independently inspected this turn, proxy executed, real C# result NOT_RUN, CODEX/Astra independent judgment NOT_PERFORMED, no product repair authorized from proxy.
