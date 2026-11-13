# N58-03 — два малых PRIOR-ART воспроизведения K1/K2; отдельный новый K3 в 06. Без разрешения запусков нет

**Статус обоих: DESIGN_ONLY; PENDING_LUBA_REVIEW; APPROVAL_ID=NONE; MODEL_EXECUTION=NOT_RUN; .NET_RUN=NOT_RUN; AGENT_RUN=NOT_RUN.** Создание карточки НЕ разрешает исправлять код, запускать модель или обучать. Никаких постфактум подогнанных gold/threshold.

## 0. Общий полный паспорт — часть тела каждого опыта

**Изначальный запрос Любы — дословная целевая формулировка:** «Твоя задача собрать возможные пробелы в реализации, которые привели к ухудшению качества обучения». «Твоя задача так же находить новые гипотезы, которые еще не нашли другие агенты!!!». «Нужно уведомиться что все найденные детали описаны в экспериментах ... и эксперименты действительно правильно описаны и правильно измеряют показатели до и после». «ты должен внести изначальные промпт, скилы, начальные данные, название модели, ожидаемый результат в тело эксперимента и мне придется тебя проверить перед началом работ». Это **дословные выдержки**, а не претензия воспроизвести всё исходное сообщение, ссылки и знаки препинания.

**Тема обучения:** перевод. **Профиль:** English→Russian, императивные инструкции. **Получатель обучения:** native memory `TranslationMachine` → `ExecutorUnit(UnitId="executor-en_ru")`; он должен получить **два typed structured урока**. **Механизм:** kNN source filter и consumer, **не LoRA, не DPO-optimizer, не нейронный Student**. Исполнитель эксперимента: **запрошенная модель** `gpt-6-astra / ultra` как исторический CODEX review profile; **ФАКТИЧЕСКОЕ model ID, вариант thinking, runtime provenance и независимый рецензент ещё не подтверждены; нельзя утверждать их наличие/исполнение**. Обучаемая модель: **`TranslationMachine` (memory, no neural model weights)**, модель языковых весов: N/A. Для возможного иного neural RP01 опыта требуется отдельно **`generation-v1` B0 random, vocab629/4 layers/context2048, LoRA q/v rank8** и иной review; этот опыт его не проверяет.

**Набор обязательных скилов для чтения, без модификации:** `docs/skills/intent-to-plan-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `experiment-design-SKILL.md`, `code-examination-SKILL.md`, `root-cause-tracing-SKILL.md`, `learning-path-audit-SKILL.md`, `claim-verification-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `human-communication-GLOSSARY.md` — все из [source snapshot](https://github.com/lubochka/llm_mvp_core/tree/eb05219c27663835ce927c7bf97cf1274c07cddb/docs/skills). Перед будущим RUN записать file SHA каждого реально загруженного навыка, caller file SHA, `AGENTS.md`, `CLAUDE.md` и согласованный exec model ID. Если не удалось загрузить стек навыков — **не заявлять skill-compliant PASS**.

**Начальное состояние:** отдельные новые `TranslationMachine` A/B из одного исходного `new TranslationMachine()`, ещё без `executor-en_ru` уроков; reset memory и cache должны быть подтверждены snapshot hashes. Точные initial checkpoint IDs/hash до выполнения **неизвестны; нельзя их изобретать**. A0/B0 могут повторно использовать один baseline лишь при доказанно равной функции. Exact/prefix/structure KNN index, policy and validator в A/B идентичны. Вся модель, tokenizer, scorer, lessons, case order, seed, version, clock/caches фиксируются. Нельзя подавать EXAM gold внутрь Unit/KNN/semantic tokenizer.

**Единственная разница A/B:** конкретный компонент фильтра поиска ниже (K1 или K2, НИКОГДА одновременно). `k=5`, векторы, tokenization, number of candidates, same signature, LoRA/no LoRA, memory maps, initial state, training calls и scorer не меняются. **Сначала read-only/source reachability preflight** с журналом exact/phrase/wordmap/kNN, без фиктивного self-injected "лучшего" соседа. Если реальный `FindNearestCascade` не возвращает intended neighbor на тестовых данных: NO_CONTRAST / NOT_REACHED; затем можно проектировать отдельный component-contrast, но **нельзя сообщать ordinary end-to-end gain**.

**Измерения A0/A1/B0/B1:** четыре actual ответа EXAM от обычного `GetOrchestratorAsync().RunAsync` и несколько дополнительных компонентных traces при необходимости (считать отдельно). На каждом плечe 2 урока в 1 structured batch, optimizer updates0, нейронных LoRA steps0. По4 EXAM×4 состояния =16 логических ответов,12 physical допустимы только при подтверждённом идентичном initial consumer A0/B0. Дополнительные TRAIN-fit вызовы необязательны и считаются отдельно. Нельзя считать 12 логических как 16 фактических. Cold save/load отдельно после основных ответов (zero updates) с exact output, route и state hash parity.

**Primary quality метрика:** `correct/all=correct_request_count/4` на each arm/phase, one case=one request, frozen semantic obligations (действие, объект, отрицание, запрет лишних действий). Дополнительно `wrong_accepted/all`, `coverage=answered/all`, `correct/answered` (N/A если 0), `abstain/all`, acquisition=after−before для обоих arms, interaction `(B1−B0)−(A1−A0)` только на совместимой панели; family independence по возможности, но два названия одного шаблона **не** два независимых закона. Memory bytes/triples, wall time by stage, process CPU time **не Environment.TickCount64**, RAM sampled peak **не GC.GetTotalMemory single point**; other unmeasured ресурсы отмечать N/A. Число neural params/layers N/A; Unit/Flow instantiated/executed levels показать. Два коротких урока не гарантия обнаружения статистически устойчивого эффекта.

### K1 — N58-EXP-K1-SOURCE-CONTAINMENT

**Baseline A:** native `SourceTokenSimilarity = |Q∩C| / |Q|` на обеих `HashSet`, threshold0.60 и shared minimum2.
**Enhanced B, ровно один фактор:** только в месте score для этого ordinary ENRU вызова `Jaccard=|Q∩C| / |Q∪C|`, threshold0.60; stopword list, index и eligibility неизменны. Это **предложенная политика**, не заявленный текущий исправленный метод. Случаи, где score floor другой/зависит от order/cascade, показывать честно.

**TRAIN** (искусственные, человек до запуска проверяет семантику):
- K1-T1 source `open the door and delete the file`, chosen `открой дверь и удали файл` (два действия).
- K1-T2 source `save the file and close the door`, chosen `сохрани файл и закрой дверь` (два действия).
- В оба arms записать именно это как `ExecutorOutput.Text` для `executor-en_ru`; `Rejected` не используется при memory training, не фабриковать DPO pair. Генерация `SemanticSignature` обязана быть настоящим current builder либо явно версионированной fixture, не gold.

**EXAM** (не входит в TRAIN; замороженный контракт — выполнить РОВНО одно действие, без другого):
- K1-E1 `open the door` → «открой дверь».
- K1-E2 `delete the file` → «удали файл».
- K1-E3 `save the file` → «сохрани файл».
- K1-E4 `close the door` → «закрой дверь».
Допустимы синонимичные русские формы с той же командой, без дополнительного действия. Предметные source families не являются независимыми, так как повторяются базовые T1/T2 words; это **сначала local memory collision test**, не generalization benchmark.

**Доказанное формулой ожидание до RUN:** для E1 vs T1 content Q={open,door}, C={open,door,delete,file}: A=2/2=1.00, B=2/4=0.50. Для E2 vs T1 content Q={delete,file}, C={open,door,delete,file}: A1.00/B0.50. Аналогично E3/E4 vs T2. **Не обещать** 0→4 correct: B может abstain и снизить false accepts, не приобрести способность генерировать короткий верный перевод. Actual index rank, selected neighbor и final output неизвестны.

**Edge / negative:** полностью совпадающее законное одно-действие как независимый *контроль* даёт A=B; длинная инструкция с лишним, но законно запрошенным действием не должна считаться ошибкой. Пары с <2 shared content tokens на A/B исключаются из causal effect этого threshold.

**Refutation:** при matched actual route и found candidate обе arms одинаково принимают неправильный длинный output либо B отказывает и `wrongaccepted` не меняется из-за другого consumer; NO_CONTRAST когда kNN не достигается. Если Jaccard снижает recall, записать цена coverage, не скрывать.

### K2 — N58-EXP-K2-IMPERATIVE-POLARITY

**Baseline A:** current SourceContentTokens удаляет `not/no/do`, узкий existing SourceLaw для copular phrases; никакого дополнительного general imperative policy.
**Enhanced B — ровно один фактор:** при распознанной в **заранее ограниченной грамматике** императивной форме `(do not)? VERB [the/a] NOUN` отвергнуть KNN candidate с иной polarity; при parser UNKNOWN сохранить прежнее поведение и записать UNKNOWN, не угадывать из EXAM gold. Tokenizer/score/rank, K1 query-containment остаются прежними. Parser это **предлагаемый deterministic guard, не доказательство обучения**, меняется только candidate admission.

**TRAIN:** 
- K2-T1 `do not open the door` → «не открывай дверь».
- K2-T2 `do not save the file` → «не сохраняй файл».
Законные negative memories. Один batch для `executor-en_ru` в каждом arm; нет neural updates.

**EXAM** (same four for A/B, no TRAIN exact copy):
- K2-E1 `open the door` → «открой дверь».
- K2-E2 `save the file` → «сохрани файл».
- K2-E3 `do not open a door` → «не открывай дверь».
- K2-E4 `do not save a file` → «не сохраняй файл».
Рубрика фиксирует отрицание и действие, допускает варианты перевода, не награждает пустой отказ за correct translation. Для E1 с T1 `Q={open,door}`, `C={open,door}` A similarity1.00, B **полярностный отказ**; E3/T1 одинаковый знак, B не должен ложно veto. **Проверить reachability:** текущий SemanticTokenizer/Signature/semanticConcept может не дать искать T1 при E1, а exact/phrase/wordmap может обойти KNN. Это НЕ готовый claim о production regression.

**Negative controls:** совпадающая polarity при изменённом артикле; `the door is not open` отдельно как COPULAR уже covered existing SourceLaw, **не выдавать тот же эффект за новый**; quotation `"do not"` и двойное отрицание за пределами parser ⇒ UNKNOWN. Для незнакомого языка/домена B не включать.

**Ожидаемое измеряемое отличие:** если opposite-polarity neighbor selected, число неверно принятых команд B может снизиться ценой отказа, но количество самостоятельно правильных переводов **не обязательно** возрастёт. 2 урока / 4 EXAM — короткая дискриминация, никакого обещанного прироста или автоматического adoption.

## Форма результатов ОБЯЗАТЕЛЬНА для каждого отдельного K1/K2 — сейчас все значения пусты

| ID / arm phase | Baseline A0 | A1 | Enhanced B0 | B1 | Evidence |
|---|---|---|---|---|---|
| correct/all | NOT_RUN | NOT_RUN | NOT_RUN | NOT_RUN | actual outputs + semantic scores |
| wrongaccepted/all | NOT_RUN | NOT_RUN | NOT_RUN | NOT_RUN | route, obligations |
| coverage/answered | NOT_RUN | NOT_RUN | NOT_RUN | NOT_RUN | count accepted vs fallback |
| learning-state digest/triples | NOT_RUN | NOT_RUN | NOT_RUN | NOT_RUN | actual checkpoint hashes |
| process CPU / peak RAM / wall | NOT_RUN | NOT_RUN | NOT_RUN | NOT_RUN | sampled/monotonic instruments |

Считать `learned-output improvement` и `retrieval guard benefit` отдельно. **Никакой таблицы «после» до исполнения заполнять предполагаемыми числами.** Классификация: REACHED+CAUSAL, REACHED+NO_DIFFERENCE, NOT_REACHED, INVALID_GOLD, CONFOUNDED, NEEDS_REPLICATION — не «PASS» из ожидания.

**DUPLICATE-SCREEN:** K1/K2 повторяют ранее найденную формулу SourceSimilarity и ignored negation из [N27-04](../27-NEW-SEQUENCE-SPLIT-CONFOUNDING-AND-NEGATION-KNN-GAPS-2026-10-16.md), [N37-C](../37-NEW-KNN-UNKNOWN-TAIL-AND-POSTFILTER-CASCADE-2026-10-16.md). Их смысл — точный минимальный reproducibility/consumer stage, НЕ новая гипотеза. Для реально новой narrow candidate и собственного experimental body см. [N58-EXP-K3](06-NEW-PREDICATE-COMPLEMENT-LAW-AND-SMALL-EXPERIMENT.md).
