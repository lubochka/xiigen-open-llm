# MICRO-25 · E26-POS-LEARN-01: одна быстрая причинная проверка, потерян ли полезный контекстный признак при переносе в обучаемый Translation Unit

**Версия:** 16.10.2026 / DRAFT / READY AS IMPLEMENTATION WORK ORDER / **NATIVE NOT RUN, QUALITY NOT MEASURED**.  
**Source:** [NEW-23](23-NEW-E26-POS-CONTEXT-TO-LEARNED-UNIT-2026-10-16.md) и [NEW-24](24-NEW-E36-FEEDBACK-KNN-SPLIT-AND-ROUTING-2026-10-16.md). Pinned code: [0eccbdc3c4efb90adf1d699806cbfa9f5610998d](https://github.com/lubochka/llm_mvp_core/tree/0eccbdc3c4efb90adf1d699806cbfa9f5610998d).

## Человеческий вопрос

**Можно ли научить Unit корректно выбирать «thread», «flow» и «stream» по контексту слова «поток», а затем узнавать это на других фразах, если хранить признаки соседних POS, которые уже давали положительный результат E26?** Не выдавать успех на старых 15 hard-coded E26 примерах за новое обучение. Вопрос **отдельный от** RP01 нейросетевой Student DPO; он проверяет конкретную причину, по которой перенесённый Unit может давать хуже исходной функции.

## Гипотеза и нулевая модель

**H1:** при одинаковых source lemma и semantic concept, но различающемся левом/правом POS, A — текущий supervised ExecutorUnit word-map — не сохранит нужную дизамбигуацию, а B — context-aware обучаемая память с тем же обучающим материалом — сохранит; B даст выше blind correct-sense и выше правильность на ordinary TranslationRoutingFlow.

**H0:** различие признаков не влияет на качество: SemanticTokenizer уже создаёт разные concept, другие текущие maps/KNN/Flow решают задачу, либо POS не несут стабильного смысла. H1 нельзя принимать, если выигрыш существует только в диагностическом словаре, а пользовательский Flow не меняет ответ.

## Состав: один кодовый seam, без DPO/LoRA, без CUDA и Windows CI

**Переменная исследования:** только добавление сохранённого **левого и правого POS-контекста** к learned choice of translation; не заменять model/BPE, не добавлять новый scorer, не увеличивать число уроков, не переносить готовый BuildDefault словарь.

| Плечо | Назначение | Train и inference |
| --- | --- | --- |
| A / CURRENT | native baseline | текущие TranslationSemanticTokenizer + ExecutorUnit + TrainedTranslationAdapter: при обучении chosen.WordMappings → (lemma,concept)/surface и текущий KNN/phrase/exact |
| B / POS-MEMORY | один добавленный признак | все те же компоненты и train rows; в test-only shadow хранить для каждого неоднозначного source word ключ (normalized lemma, concept, left POS, right POS) → approved chosen translation; использовать только когда eligible, иначе **тот же A** |
| C / SHUFFLED | отрицательный контроль | B, но train-only labels для каждого неоднозначного слова перемешаны внутри этого слова; tokenizer, источники и heldout те же |
| D / RULE-ONLY | статический контроль | до обучения тот же A/semantic rule baseline; **не** ContextualDictionary.BuildDefault и **не** подстановка hidden gold |

Дополнительный аналитический baseline E26 словарь разрешён только для объяснения прежней метрики; исключить его из B/D, иначе исторически прошитые переводы загрязнят опыт. В B **не менять порядок внешних маршрутных ветвей ради предполагаемого выигрыша**. Если existing exact/phrase прерывает путь, показывать пропуск эффекта и отдельно учитывать случаи, где POS memory действительно допускается к выбору. B надо продемонстрировать и direct shadow, и через ordinary consumer после контролируемого тестового подключения.

## Данные и независимость (малый бюджет)

- **TRAIN 6–12 утверждённых примеров** (минимум три значения «поток» и три «запрос», по возможности 2 стилистически разные формулировки каждого). Хранить approved source+target, домен, направление RU_EN, concept, left/right coarse POS, provenance и истинный UnitId. Только учитель/исследователь выбирает обучающую метку; синтетические примеры ниже — **описание smoke fixture, не автоматически проверенный gold**.
- **BLIND HELDOUT 12 новых фраз**: по две новые source-family формулировки на каждое из 6 значений, ни одна exact/prefix/normalized source не равна TRAIN; Gold утверждается независимо **до** обучения и скрыт от scorer. Не использовать E26 BuildTestCases как heldout.
- **DEV 4 короткие пробы** можно выделить отдельно для проверки readout/маршрута. Не оценивать ими финальное улучшение. **Negative**: одна фраза без различимого POS, минимум две с ложными похожими словами, случай со сменой направления; не допускать silent false match.
- Единый frozen split/seed, два порядка TRAIN для проверки last-write-wins и три повторных shadow запуска, если порядок влияет. Для этой модели не нужно 500 пар, новая LoRA, новый checkpoint B0 или общий 32-slice gate.

### Минимальный smoke fixture именно признаков (6 training distinctions)

**Это структурные тестовые случаи, а не лингвистический oracle.** Legacy POS: 7=NOUN, 8=VERB, 9=ADJ, 11=WORD. Точный маппинг в новый TranslationPos перед тестом **обязательно объявить**; WORD=11 прямого аналога не имеет, поэтому там требуется отдельный типизированный readout, а не casting numeric enum.

| Source | POS-сосед слева | POS-сосед справа | Учебная метка |
| --- | --- | --- | --- |
| поток | VERB | — | thread |
| поток | — | NOUN | flow |
| поток | ADJ | — | stream |
| запрос | — | WORD | request |
| запрос | — | NOUN | query |
| запрос | NOUN | — | prompt |

**Генерация 12 проверок:** по две *новые формы/контекста* к каждому из этих 6 отношений (например, поток/потоки/потоком; запрос/запросу), со скрытой независимой лингвистической разметкой. Классификация concept, PosBigrams, candidate path, корректность русского/английского предложения и реально потребляемый SourceText записываются. Если независимый эксперт считает synthetic label неверным в новом контексте, **не подгонять ответ/threshold**; исправить постановку **до** blind run либо снять кейс с обоснованием.

## Конкретный алгоритм реализации: 2 быстрых уровня

**L0 — source diagnostic, до обучения (локальная проба без GPU):**

1. Прочитать [старый ContextualDictionary](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/35.ExperimentsV2/Infrastructure/ContextualDictionary.cs#L12-L117), [TranslationSemanticTokenizer](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/27.TranslationDomain/TranslationSemanticTokenizer.cs#L15-L29) и [TranslationUnitContracts](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/27.TranslationDomain/TranslationUnitContracts.cs#L42-L110).
2. На 6 smoke cases получить: old POS context vector, текущий Tokenize concept, PosBigrams и текущий ключ для [TryGetAcceptedWordMapping](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/27.TranslationDomain/TranslationTrainedUnits.cs#L860-L871).
3. Для любых двух учебных пар с **одинаковым нормализованным (lemma, concept), но разным gold target** зафиксировать коллизию current mapping/поведение при перестановке строк; если concept различается, **не считать кейс доказательством потери контекста**, отобрать другой либо понизить H.
4. Построить test-only пустую память, **только AddEntry из TRAIN**, без BuildDefault, и сравнить ability to distinguish two targets. Это подтверждает возможность восстановления различения, **не quality gain**.

**L1 — native same-input learning utility (один minimal test host):**

1. Использовать существующий approved путь [TranslationLearningExample](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/27.TranslationDomain/TranslationUnitContracts.cs#L126-L149) → [ExecutorUnit.TrainOnExample](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/27.TranslationDomain/TranslationTrainedUnits.cs#L403-L455). Не переименовывать исходные pair IDs, не обойти provenance gates продукта, не модифицировать frozen RP01 C#500/ENRU500.
2. После обучения A и B по **одним и тем же** source rows считать learned-state summaries, выбранные wordmap slots, POS entries, replay ordering. На readout удержать неизменным SemanticTokenizer, старые KNN/phrase/exact кандидаты, verifier и fallback; A и B различаются только разрешённым POS lookup.
3. На всех 12 blind показать request→token/concept/POS→Unit path/id→выбранный перевод→QualityVerifier→пользовательский результат. Проверить, что в hidden queries нет exact-source и phrase memorization, что ответ B не был заранее прошит в тестовый код.
4. Затем холодно восстановить именно фактически обученный B-state (или явно пометить RESTORE_NOT_IMPLEMENTED) и повторить **ordinary consumer**; между train и readout не создавать новый случайный Unit. Сравнить A/B/C/D с одинаковыми heldout и cost.

**Если нет test seam, позволяющего подмешать POS память в реальный TrainedTranslationAdapter без изменения production маршрута,** первый проход ограничить diagnostic L0 и написать «consumer impact NOT_MEASURED». Не выдавать direct mock Unit за полноценный продукт. Не начинать общую переделку ExecutorUnit ради 12 кейсов.

## Оценка, бюджет и остановка

Основная метрика: **blind correct-sense / 12**, дополнительно end-user valid translation / 12, per-sense confusion, wrong-answer rate, coverage/abstain, number of same-key collisions, remembered old-cases retention (3–6), total preparation+execution seconds. Отдельно direct Unit и ordinary consumer, без смешивания уровня метрики.

**Предрегистрация:** если B улучшает end-user correct ≥ **3 из 12** против A, не проигрывает C/правилам, не увеличивает ложные допустимые ответы и улучшение остаётся после fresh reload, это **сигнал к 30–50 независимым парам в следующем опыте**, а не доказательство статистической общности. Если B выигрывает лишь direct Unit и не влияет на обычный consumer — verdict **PATH_NOT_ADOPTED**, а не SUCCESS. Если A≈B и trace показывает, что правила уже решают неоднозначность — **NO_GAIN for H23**. Если A≈B≈C — **LEARNED_SIGNAL_NOT_PROVEN**. Если дефект проявляется только в старых E26 cases — **LEAKAGE**. Если нет gold, source separation, code path или прав — **INCONCLUSIVE/INVALID_EVIDENCE**.

**Timebox:** один лёгкий native diagnostic host и одна blinded mini-eval; прекратить при невалидном admission, отсутствии валидного gold, утечке gold в train, несравнимом corpus, либо невозможности измерить реальный consumer. Нет разрешения на новый многодневный Windows GitHub Action, 500-парный DPO, переподгонку тестов или изменение training weights.

## Роли, проверка плана и точное поручение инженеру

**Архитектор:** сохранить ILearningMachine/typed Units/Flow ownership, отделить learned memory от deterministic tokenizer, показать вход/выход конкретных Unit и источник PosBigrams. Проверка архитектуры должна назвать, какой интерфейс несёт новое поле и почему это не новый дублирующий trainer.

**Планировщик:** закрепить A/B/C/D, одинаковый corpus/seed/consumer, frozen hidden cases, step-boundary, сохранение source SHA; выделить единственный owner микрозапуска. Код-ревьюер проверяет, что runtime использует старую бизнес-цепочку, не hidden hard-coded gold, не synthetic improvement score и не пропущенную ветку exact/phrase.

**CODEX/Astra — human role:** утвердить полезный закон и качество source/target, отдельную метку для каждого blind кейса; когда нужны именно новые DPO, **обе** final chosen/rejected создаёт разрешённая внешняя открытая китайская модель и CODEX/Astra оценивает **точную неизменную пару**. Здесь предпочтителен уже одобренный supervised Unit material, так что новый DPO trainer не является условием диагностики.

**Исполнительный prompt (scope):**

> CURRENT TASK: реализовать только E26-POS-LEARN-01 в локальном диагностическом тестовом host и выдать evidence JSON/Markdown.  
> NOT THE TASK: RP01 DPO/LoRA training, изменения production Unit/Flow, CI Windows, новые 500 пар, восстановление всего RAG.  
> SKILLS: architectural-decision-testing, human-readable-architecture-audit, learning-path-audit, reference-inventory-and-source-map, phase-planning, plan-review, generated-code-review, claim-as-hypothesis, prompt-patch-authoring.  
> READ: NEW-23, этот протокол, конкретные E26 files, TranslationSemanticTokenizer, TranslationUnitContracts, ExecutorUnit, TrainedTranslationAdapter, current applicable AGENTS and source-owned locked decisions.  
> ONE FACTOR: сохранение контекстных POS рядом с learned target; same corpus, approved labels, frozen train/holdout, unchanged scorer/other routes. GOOD = blind correct choice and actual consumer change after accepted training; BAD = E26 BuildDefault reproduces old test / pair-margin only / mock output / hardcoded gold.  
> OUTPUT: exact branch+SHA+paths/lines, case-level A/B/C/D receipts, total and per-domain correctness, provenance, train→state→Unit→Flow trace, false-positive examples, falsifiers, code-review verdict, NOT_RUN where appropriate. Do not claim PASS if no independent blind quality.  
> No changes beyond a bounded separately authorized diagnostic test host; research/planning phase alone permits only documentation.

**Отдельная гипотеза NEW-24:** E36 feedback-routing template leakage проверяется отдельным E36-SPLIT-01 по [NEW-24](24-NEW-E36-FEEDBACK-KNN-SPLIT-AND-ROUTING-2026-10-16.md). **Не** объединять два разных дефекта в одну A/B кампанию.

## Публикуемые минимальные evidence records

Создать (в разрешённом будущем исполнении) неизменяемые **новые** scientific receipts с experimentId, code commit, actual native test command, approved source hashes, split family IDs, blocked exact overlaps, method/line implementation map, caseId, arm, gold-provider, selected candidate, adapterId, before/after Unit state identity, restore state, whether context was consulted, correct/abstained/wrong, negative controls, time/memory, verdict. Не трогать архивные tier results и их PASSED/FAILED. **Ни один из этих результатов пока не существует.**