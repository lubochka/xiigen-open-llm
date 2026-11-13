# MICRO-N27 — один маленький причинный эксперимент: правильный новый урок портит прежний перевод?

**Дата:** 2026-10-16. **Статус:** READY_FOR_NATIVE_IMPLEMENTATION / JS_FORMULA_CHECKED / NATIVE_DOTNET_NOT_RUN / FROZEN_GOLD_NOT_APPROVED. Это научная проверка гипотезы из [N27](27-NEW-ROLE-BLIND-MORPHOLOGY-AND-QUOTE-NEGATIVE-COLLAPSE-2026-10-16.md), не команда массового RP01-retrain. Не выполнять Windows GitHub actions/full CI.

## Почему это лучше ещё 500 DPO-строк

Мы меняем ровно **одно правило генерации обучаемого состояния**: можно ли порождать русскую глагольную «гендерную альтернативу» без проверенной глагольной роли. Остальные данные, LoRA, tokenizer, KNN, параметры генерации и Flow одинаковы. Требование результата — **улучшение ответа настоящего переводческого consumer на незнакомой фразе**, а не только успешный unit test или изменение hash.

### ExperimentId / гипотеза

- **ID:** MICRO-N27-ROLE-GENDER-RETENTION.
- **H1:** В действительном RU_EN ExecutorUnit обучение правильному noun-уроку с поверхностью «пила» добавляет ложную map «пил»→«saw» и перезаписывает ранее правильную verb-map «пил»→«drank». Если отключить **только** морфологическую деривацию из неверифицированного существительного, прежний перевод глагола сохраняется, а доля ошибочно принятых переводов новых глагольных запросов падает.
- **H0:** Исходный токенизатор, priority exact/phrase/KNN, состав реальных word mappings или verifier не позволят ложной записи изменить обычный ответ; semantic quality A и B совпадут. Тогда не назначать N27 причиной RP01.
- **Область:** структурный TranslationMachine/TrainedTranslationAdapter. Никакой причинный вывод о neural RP01 Student без отдельного трассированного моста.

## Исходный код и обязательная проверка достижимости (L0)

1. Пинить current core HEAD и прочитать [FreqQuote producer](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationFrequencyQuoteMap.cs#L14-L96), [ExecutorUnit.TrainOnExample](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L404-L455), [DeriveGenderAlternateForm](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L760-L795), [Execute + KNN](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L264-L403), [ordinary adapter](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationDirectionAdapters.cs#L116-L146).
2. Выяснить фактический caller FrequencyQuoteMapBatchBuilder; если отсутствует, оставить именно producer-specific риск NON_REACHABLE для RP01. **Всё равно** можно проверить DeriveGenderAlternateForm в основном TrainStructuredBatchAsync, где WordMappings есть. В журнале различать «путь вызван» и «класс существует».
3. В одном native .NET локальном экспериментальном хосте/TestBench (или одном точечно вызванном test, НЕ полный CI) подать **одни и те же законные** примеры, скопировать native snapshots ExportCheckpoint before/after, и получить детальную трассу: actual WordMap/SurfaceWordMap для «пил», candidate.AdapterId, KNN top5 identities, verifier result, selected final adapter/fallback, финальный текст.
4. Нативный эксперимент L0 считается подтверждённым только когда две последовательно сохранённые памяти показывают реальную разницу. Не приравнивать JS proxy к C#.

### Synthetic DEV_ONLY train sequence (не подменяет утверждённые DPO)

| Шаг | Source fragment (роль) | WordMapping chosen | Действие |
| --- | --- | --- | --- |
| T0 | «он пил чай» (PAST_VERB) | «пил» → «drank» | Записать корректный глагол и его текущую surface-memory |
| T1 | «у неё есть пила» (NOUN) | «пила» → «saw» | Записать корректное существительное **после** T0; наблюдать, переписался ли «пил» |
| T2 | Независимо одобренные сопутствующие слова | «он» → «he», «воду» → «water», при необходимости остальные безопасные mappings | Обеспечить достижимость wordmap ветки без изменения целевого правила |

Эти строки — **локальные синтетические контрпримеры, не лицензированный новый production DPO**. Перед измерением реального качества предоставить независимый законный gold для неучебных запросов, не использовать ответы хоста как gold.

### Минимальные армы, один фактор

| Арм | Обучающий набор / порядок | Единственное отличие |
| --- | --- | --- |
| A — CURRENT | T0→T1→T2 | Текущий DeriveGenderAlternateForm без POS gate |
| B — POS-GATED | T0→T1→T2 | Гендерные формы порождать только для действительно проверенной русской past-tense **глагольной** WordMapping; noun mappings не порождают alt. Все остальные Unit/Flow остаются идентичны |
| C — ORDER CONTROL | T1→T0→T2 | Проверить, что симптом зависит от очередности и last-write-wins; не считать C основным causal gain |
| D — NO-LEARN / SHUFFLED-LABEL | Freeze без новых WordMappings и отдельно переставленная неверная разметка в безопасной fixture | Проверить, что результат B нельзя получить просто за счёт иного fallback/несвязанных эвристик |

*Техническое уточнение:* если текущие поля не дают достоверного POS, B не должен угадывать его из окончания; для probe использовать точно размеченное input.GrammaticalRole или строго утверждённый POS oracle как shadow metadata. Не менять production schema в этом опыте.

### Frozen blind evaluation — 12 source-disjoint пользовательских запросов

Состав: четыре male-past verbal («Он пил воду», «Вчера он пил ...» и иные **не повторяющие** train-family), четыре noun («В магазине была пила», альтернативы без exact overlap), четыре contrast/negation (negative polarity, actor/recipient/order) с независимой предметной корректностью. До запуска заморозить full source text, направление RU_EN, ожидаемые существенные семантические признаки, разрешённые варианты английского, forbidden meaning, SHA панели. Произвольная свободная корректность оценивается слепо, **не** точным строковым совпадением. Не брать шаблоны напрямую из T0/T1.

**Primary:** semantic correct / 12 для ordinary TranslationRoutingFlow (A vs B, paired by exact case ID), и separately **wrong accepted / 12**. **Secondary:** abstain/fallback, coverage, retention for old verbal law (4), new noun learning (4), memory equality after SaveCheckpointAsync→LoadCheckpointAsync, per-case latency. Gold/judge и источники не меняются между A/B; no prompt tuning on heldout.

**Критерий сигналов:**
- Механика: после T0 «пил»→«drank», после T1 current «пил»→«saw»; B сохраняет «drank». Если native C# этого не показывает — **CODE_HYPOTHESIS_REJECTED**.
- Causal quality: на той же закрытой панели B делает больше **семантически верных** ответов, уменьшает wrong accepted и не ухудшает законный noun skill; в каждом исправленном кейсе trace показывает изменившийся memory source и достигший ordinary consumer. Если изменён только map, но конечный ответ тот же — только MEMORY_EFFECT, **не QUALITY_GAIN**.
- Если обе модели выдают fallback и не обладают verb-skill, нельзя говорить «восстановили обучение»; следующий шаг — минимальный recipient reachability/warm-up, не 500 DPO.
- Размер панели диагностический, **не** доказывает многодоменную generalization; успешный локальный результат требует отдельного source-family-disjoint повтора в 2+ доменах.

## Две ортогональные дешёвые проверки producer (без переобучения)

**P1 — GrammaticalRole projection.** Две FrequencyQuoteEntry с SourceWord=«пила», SourceLang=ru, GrammaticalRole=NOUN/PAST_VERB, разными утверждёнными переводами. Прямой BuildBatch: сверить 6 созданных Unit examples, наличие role identity, фактический chosen WordMapping и recipient. Shadow исправление — передавать роль в typed mapping и ключ обучения. **Не смешивать P1 с B morphology guard в основном A/B.** Отдельно сверить уже опубликованный риск hard-coded ExecutorRuEn [N21](21-NEW-GAPS-SYMBOL-BLIND-LESSON-ADMISSION-AND-DIRECTION-MISROUTING-2026-10-16.md).

**P2 — TargetPrefix/negative validity.** Три synthetic quote rows: (i) marker существует/удаление меняет смысл, (ii) TargetPrefix кончается на «a», тогда как SourceWord переводится «saw» — неправильный alignment, (iii) marker отсутствует в FullTargetSentence — current chosen/rejected text-tie. Проверить именно actual BuildBatch text outputs и семантическую paired-quality rubric. Shadow исправление: **refuse** unaligned marker или одинаковую пару; не выдумывать rejected text для прохождения проверки.

## Results packet, review и запреты

Для одного исполнения сохранить: exact pinned code HEAD + diff B, input SHA и frozen panel SHA; per-case A/B/C/D results/ordinary trace, WordMap before/after, KNN hits, verifier, save→load parity; aggregated correct, wrong accepted, abstain, retention; full negative controls; runtime cost; явное verdict. Отдельный кодовый reviewer проверяет изменение **одного фактора**, плановый reviewer — отсутствующее сравнение при несовпавших baseline/heldout. CODEX/Astra independent judgment назначать только как реальную отдельную операцию, **не симулировать вызов**.

**Порядок приоритетов:** прежде нужен L0 caller и 1 native trial с реальным Unit; если FrequencyQuoteMap вне RP01, исключить его из текущего критического пути и вернуться к Student B0/SFT→DPO диагностике из [RP01 pack](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/README.md). Ни одного нового Windows GitHub run, 500 DPO, полного regression CI или автоматического promotion ради этой гипотезы.
