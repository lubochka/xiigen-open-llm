# MICRO-26 — один короткий причинный опыт: язык в ключе обученного Unit и RU RuleTag parity

**Статус:** EXECUTOR-READY PROTOCOL / FIXTURE DESIGN / NOT RUN native C# / NOT RUN full model / no claimed improvement.  
**Цель:** отличить гарантированную по коду коллизию состояния от реально ухудшенного ответа пользователя. **Не выполнять новые 500 DPO, Windows GitHub Actions, многодневное исправление тестовой инфраструктуры.**

Связанные новые исходные различия: [N26-LANG-01 и N26-TAGS-02](26-NEW-SEMANTIC-LANGUAGE-KEY-AND-RULETAG-PARITY-2026-10-16.md). Это проверка **переводческих обучаемых Units**, а не испытание случайной B0 нейросети RP01.

## Гипотезы и заранее заданные фальсификаторы

| Контраст | Гипотеза | Что её опровергает на указанном уровне |
| --- | --- | --- |
| M26-A, state key | Для EN и ES с одинаковым raw при serial train используется общая last-write запись | При фиксированной версии кода разные сохранённые записи реально сосуществуют и Tokenize выдаёт обе соответствующие signature независимо от train order |
| M26-B, RuleTag parity | Путь learned exact correction срезает RU ruleTags из prefix/structure | На реальной фразе с base RuleTags непустым после active correction обе сигнатуры сохраняют те же RuleTags |
| M26-C, consumer quality | Эти отличия приводят к ошибочному output / verifier-fallback в обычном переводческом Flow | Stateful bug есть, но direct Unit/Flow on/off, train order и patched вариант выдают одинаковые слепые правильные ответы, а требуемые candidate paths не используют эти адреса |

M26-A и M26-B — быстрые механические проверки. **Качество** устанавливается только M26-C, отдельно от первых двух. Если первые две верны, но путь выдачи их не использует, verdict: CONFIRMED_INTERFACE_GAP / NO_MEASURED_CONSUMER_IMPACT, а не «исправление качества доказано».

## Подготовка (один короткий запуск на локальном .NET, без CI)

Работать в disposable worktree, **не** в core/docs/plans/replanning-possible-diffs. Проверить snapshot и хеши первичных файлов; сохранить sourceSha и git HEAD в новый owned output, не писать в frozen 500-pair data, weights или прежний checkpoint. Разрешён лишь test-only отдельный console harness (например, tools/Micro26Probe с ProjectReference на 27.TranslationDomain), не product refactor.

Сначала прочитать точные владельцы:

- [TranslationLearningExample contract:126–162](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationUnitContracts.cs#L126-L162).
- [Shared semantic Unit training/lookup:89–163](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L89-L163).
- [KNN cascade:210–310](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationUnitContracts.cs#L210-L310).
- [Ordinary adapter and verifier:116–139](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationDirectionAdapters.cs#L116-L139).

**Входной fixture:** [26-MICRO-LANGUAGE-KEY-AND-RULETAG-FIXTURE.json](26-MICRO-LANGUAGE-KEY-AND-RULETAG-FIXTURE.json). Он содержит **предложенные разработчиком** контрастные строки, НЕ утверждённые независимые метки, НЕ разрешённые финальные DPO chosen/rejected и НЕ output модели. До реального learning A/B независимый examiner подтверждает семантические ожидания и лицензии train corpus.

## M26-A: доказать или отвергнуть коллизию вообще без KNN и модели

Инициализировать чистый SemanticTokenizerUnit. Сформировать две test-only TranslationLearningExample (UnitId = semantic-tokenizer, HeldOut=false) через JsonSerializer и реальные типы TokenizerInput / TokenizerOutput. Для обеих использовать RawText="pan", но SourceLang="en"/"es", и разрешённые диагностические выходы с различающейся Signature: например SVO|PAIR:EN-TEST и SVO|PAIR:ES-TEST. Pattern=SentencePattern.SVO. Это **диагностические метки**, не подготовленные teacher DPO.

Минимальная схема C# для isolated harness:

~~~csharp
static TranslationLearningExample Semantic(
    string id, string raw, string lang, string signature) =>
    new(
      ExampleId: id,
      UnitId: "semantic-tokenizer",
      FlowId: TranslationRoutingFlow.FlowId,
      InputJson: JsonSerializer.Serialize(new TokenizerInput(raw, lang)),
      ChosenOutputJson: JsonSerializer.Serialize(
          new TokenizerOutput(SentencePattern.SVO, signature)),
      RejectedOutputJson: JsonSerializer.Serialize(
          new TokenizerOutput(SentencePattern.UNKNOWN, "WRONG|" + signature)),
      RejectedType: "DEV_PROBE_ONLY",
      SemanticSignature: signature,
      SourceId: "MICRO26_SYNTHETIC_NOT_DPO",
      HeldOut: false);
      
var u = new SemanticTokenizerUnit();
u.TrainOnExample(Semantic("en1", "pan", "en", "SVO|PAIR:EN-TEST"));
u.TrainOnExample(Semantic("es1", "pan", "es", "SVO|PAIR:ES-TEST"));
var en = u.Tokenize("pan", "en");
var es = u.Tokenize("pan", "es");
// Писать en.Signature/es.Signature и ожидаемое состояние,
// но НЕ помечать всё качество модели PASS по этому assert.
~~~

Повторить в обратном порядке. Контроль 1: raw разные (pan/pie), записи не конфликтуют. Контроль 2: unit без TrainOnExample. Контроль 3: языки написаны канонически в обоих случаях (не RU vs ru). Контроль 4: сохранение/загрузка через поддерживаемый checkpoint path только **после** подтверждённого живого состояния.

Сравнение B с точечным prospective патчем C: private key=(canonicalLanguage, normalizedRaw); другая логика tokenizer неизменна. Для legacy checkpoint неизвестный язык не угадывать: отдельная ветка миграции или explicit invalidation, не молчаливый reuse.

**Ожидаемое source-based:** у текущего shared key обе Tokenize("pan", "en"/"es") берут signature последней учебной записи. Из этого НЕ следует, что перевод "pan" будет неправильным: script guard/дельта verifier могут маскировать. Не писать actual numbers без запуска.

## M26-B: три подписи base ↔ trained на том же входе RU

На свежем Unit взять source="она запустила поток" c sourceLang="ru". Получить base = new TranslationSemanticTokenizer().Tokenize(source,"ru"); **валидность fixture** требует base.RuleTags.Count > 0. Если пусто, подобрать короткий source из существующего corpus со стабильным русским RU:SUBJECT/RU:... tag, записать выбранный текст и его первичную метку до просмотра результата.

Обучить только semantic example с TokenizerInput(source,"ru"), Pattern=base.Pattern, Signature=base.Signature+"|PAIR:RU-PROBE"; получить learned=unit.Tokenize(source,"ru"). Зафиксировать:

~~~text
base.Signature, base.PrefixSignature, base.StructureSignature, base.RuleTags
learned.Signature, learned.PrefixSignature, learned.StructureSignature, learned.RuleTags
prefix_equal?, structure_equal?, base_rule_tags_present_in_learned_prefix?, in_learned_structure?
~~~

Снять отдельно direct KNN lookup key parity: во время ExecutorUnit.TrainOnExample prefix/structure вычисляются base tokenizer, а при Execute используются learned. Не утверждать, что KNN win изменился, если exact-source lookup раньше него.

Патч D отличается от baseline **только** тем, что re-builder сохраняет ruleTags. Проверить те же keys на неизменном corpus; одно правило, одна правка, единый comparator. Если разница есть только в signature, а final ответ не зависит от неё, quality gain не подтверждён.

## M26-C: пользовательский matched A/B на 12–18 независимых случаях

После независимой проверки gold и source-disjoint правил дать пять изолированных arms, каждый на **отдельном новом TranslationMachine**, чтобы избежать carryover:

| Arm | Единственная изменённая переменная | Назначение |
| --- | --- | --- |
| A | NO LEARNING | Выявить baseline deterministic bootstrap |
| B | EXISTING code, EN→ES order | Исходная обученная память и возможный overwrite |
| C | EXISTING code, ES→EN order | Проверка причинности last-write |
| D | TRAINED with language-scoped key, остальное как B | Кандидат адресной коррекции N26-LANG-01 |
| E | TRAINED with RuleTags preserved, остальное как B | Отдельный эффект N26-TAGS-02; не смешивать с D |

Дополнительно отрицательный контроль F: shuffled legal labels на той же численности; и direct trained Unit vs ordinary TranslationMachine.GetOrchestratorAsync. Отдельно отслеживать active branch exact-source/phrase/word/KNN/verifier/bootstrap, причину отказа, ключи и количество фактических Unit вызовов. **Если tracing покажет 0 KNN calls в B–E, гипотезу о KNN-quality-loss именно на данной панели не подтверждать.**

Фиксировать train rows, seed, source SHA, legal provenance, training order, heldout IDs, report of independent gold judgement. В panel должны быть:
- минимум два **межъязыковых омографа** с одинаковыми raw spellings, но разными meanings (например, EN/ES "pan", "pie", "once"; проверять контекст вручную, неоднозначное исключать или метить);
- минимум четыре RU-фразы с различающимися субъектом/полом/отрицанием, включая незнакомые exact forms; KNN должен иметь реальный шанс участвовать после exact/phrase/word;
- 4–6 source-family-disjoint варианта, в которых не копируется готовый oracle ответ и нельзя угадать translated target только по лекcическому совпадению;
- контроль на пустую память и ложное/запрещённое направление.

**Основные пользовательские метрики:** independent semantic-law correct / false accepted / false rejected / abstain с одинаковыми denominators и исходными text/target language для всех arms. Отдельно retrieval key hit level, candidate provenance, first consumer call, checkpoint live/reopen parity, время CPU/RAM локального процесса. Не выдавать внутризадачное confidence или UnitCoverage за правильность перевода.

**Критерий влияния:** A/B фиксирует другое пользовательское решение хотя бы на одной *заранее утверждённой* законной паре и улучшение по независимому judge без регрессии на отрицательном контроле. Это **case-level causal evidence**, не оценка генерализации. Для заявления стабильного качества нужны повторные семьи с sealed split и доверительный интервал; не ставить надуманную обязательную планку +2/12.

## Публикуемые артефакты маленького запуска

В НОВОЙ директории owned results записать только:

1. source pins (repo/branch/commit/blob hashes), fixture SHA, compiler/runtime version;
2. predeclared gold/acceptance rubric и independent judge identity;
3. один JSONL на каждый case × arm: inputSha, lang, unit+KNN key fingerprints, consumer_route, candidate/refusal/verified verdict, traceId;
4. один human-readable summary: механика A/B, какая правка улучшила какие случаи и какие ухудшила, negative controls, honest STOP;
5. independent architecture/code/plan review, verdict REVIEW_PENDING пока реально не выполнено.

**Необходимая последовательность:** сначала M26-A/B (cheap), затем M26-C только если есть реальный consumer путь к изменённой функции. Без KNN/Verifier path считать условие ошибкой контракта и перенести улучшение в отдельный bounded slice. В code changes пока ничего не коммитить в protected docs.

## Execution prompt для одного исполнителя

~~~text
Read project trainable-architecture-decision, learning-path-audit,
experiment-design, generated-code-review, plan-review,
prompt-patch-authoring and human-readable-architecture-audit skills.
Target only MICRO-26, not global RP01 rerun.
Pin the exact HEAD and read the source URLs in the MICRO-26 protocol.
Run M26-A and M26-B as native C# disposable probes (one new small harness).
Produce raw traces, source SHA and honest results.
Only if the relevant production call path was exercised, run M26-C
with approved fixtures and an independent blind semantic examiner.
Change one hypothesis per arm; never edit frozen exams/teacher pairs.
Do not edit llm_mvp_core/docs/plans/replanning-possible-diffs.
Publish NEW owned evidence and a human-language verdict.
Never claim a quality win from changed counters or a JS/string-only proxy.
~~~

**Человеческая формулировка результата:** Мы проверяем, теряются ли *выученные языковые различия* при передаче данных между Unit и KNN. Сначала дешёвая реальная проверка ключей, затем — только при подтверждённом рабочем пути — короткая проверка правильности переводов. Улучшения качества пока не измерены.
