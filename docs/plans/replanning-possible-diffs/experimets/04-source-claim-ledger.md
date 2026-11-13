# Ledger причинных утверждений источников

18 октября 2026. **Содержательное сопоставление всего снимка H завершено в 06; ниже сохранены 107 объединённых C-тезисов и обратный индекс всех подслучаев.** Этот ledger не является экспериментом или независимым review. Полный файловый снимок находится в [01](01-full-source-registry.md). Историческое утверждение источника и наш проект будущего сравнения различаются; source-only proxy не становится native результатом.

Полное по пунктам закрытие каждого источника ведётся в [06 — содержательный обход файлов](06-source-content-walk.md); C-записи ниже объединяют одинаковые причины между файлами. Наличие одной C-ссылки у файла не закрывает другие его утверждения.

## Содержательно разобранная группа: конкретные тезисы

Ниже авторские сопоставления по прочитанным фрагментам. Идентификатор C обозначает причинное утверждение либо существенное уточнение доказательства, а не отдельный эксперимент. Несколько файлов могут давать текст причины, контрпример, проект и receipt одной C-записи. Все 125 карточек записаны. C — не исчерпывающая новая нумерация: последующие подробные тезисы сохранены по source ID в 06, без дублирования их текста.

<a id="c001"></a>
### C001. NormalizeMapKey без NFC превращает si+combining acute в si и смешивает испанские «да» и «если»; второй урок перезаписывает первый.

**Сравнение:** [P51.4](cards/P51-translation-context.md#s-4).

**Допущения и предел:** Триггер — канонически эквивалентная Unicode-запись в этом Translation owner. ё/е не объявляются NFC-парой; native качество не измерено.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S193 28-NEW-UNICODE-CANONICAL-KEY-AND-SPANISH-N-FALSE-MOJIBAKE-2026-10-16.md:L7–17](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/28-NEW-UNICODE-CANONICAL-KEY-AND-SPANISH-N-FALSE-MOJIBAKE-2026-10-16.md:7>) | Исходное причинное утверждение и адрес normalizer |
| [S184 28-MICRO-UNICODE-AND-SCRIPT-SOURCE-PROXY.mjs:L5–27](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/28-MICRO-UNICODE-AND-SCRIPT-SOURCE-PROXY.mjs:5>) | Модель normalization и присваивания Map; script не исполнен нами |
| [S183 28-MICRO-UNICODE-AND-SCRIPT-SOURCE-PROXY-RESULT.json:L4–9](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/28-MICRO-UNICODE-AND-SCRIPT-SOURCE-PROXY-RESULT.json:4>) | Предоставленный receipt: memorySize1, if заменён yes; nativeQualityMeasured=false L32 |
| [S182 28-MICRO-UNICODE-AND-MIXED-SCRIPT-LEARNING-EXPERIMENTS-2026-10-16.md:L9–15](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/28-MICRO-UNICODE-AND-MIXED-SCRIPT-LEARNING-EXPERIMENTS-2026-10-16.md:9>) | Предложенный single-factor NFC-контраст и cold restore; старые квоты не приняты как универсальные |
| [S175 28-DELTA-CROSSWALK-NOVEL-UNICODE-AND-SCRIPT-2026-10-16.md:L20–24](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/28-DELTA-CROSSWALK-NOVEL-UNICODE-AND-SCRIPT-2026-10-16.md:20>) | Дедупликация: отдельная от operator loss причина |

<a id="c002"></a>
### C002. Валидная заглавная Ñ включает mojibake-ветку, поэтому case-equivalent SEÑAL/señal получают разную обработку внутренних операторов.

**Сравнение:** [P51.6](cards/P51-translation-context.md#s-6).

**Допущения и предел:** Исправление ветвления само по себе может оставить потерю ==/!=; это отдельный P51.5. Branch parity не доказывает RP01 регрессию.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S193 28-NEW-UNICODE-CANONICAL-KEY-AND-SPANISH-N-FALSE-MOJIBAKE-2026-10-16.md:L9–27](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/28-NEW-UNICODE-CANONICAL-KEY-AND-SPANISH-N-FALSE-MOJIBAKE-2026-10-16.md:9>) | Причина, унаследованность и предел по направлениям |
| [S184 28-MICRO-UNICODE-AND-SCRIPT-SOURCE-PROXY.mjs:L5–23](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/28-MICRO-UNICODE-AND-SCRIPT-SOURCE-PROXY.mjs:5>) | Ветвление по Ð/Ñ/Ã до lowercase и два результата ключа |
| [S183 28-MICRO-UNICODE-AND-SCRIPT-SOURCE-PROXY-RESULT.json:L25–30](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/28-MICRO-UNICODE-AND-SCRIPT-SOURCE-PROXY-RESULT.json:25>) | upper сохраняет !=, lower даёт xy; это receipt модели |
| [S182 28-MICRO-UNICODE-AND-MIXED-SCRIPT-LEARNING-EXPERIMENTS-2026-10-16.md:L17–21](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/28-MICRO-UNICODE-AND-MIXED-SCRIPT-LEARNING-EXPERIMENTS-2026-10-16.md:17>) | Отдельный A/B; явный предел CASE_PARITY при Law FAIL |
| [S175 28-DELTA-CROSSWALK-NOVEL-UNICODE-AND-SCRIPT-2026-10-16.md:L24–24](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/28-DELTA-CROSSWALK-NOVEL-UNICODE-AND-SCRIPT-2026-10-16.md:24>) | Уточняет новизну относительно прежнего operator loss |

<a id="c003"></a>
### C003. Один lawful ENRU target с protected API отвергается strict KNN/WordMap script gate, но проходит PhraseMap gate.

**Сравнение:** [P51.7](cards/P51-translation-context.md#s-7).

**Допущения и предел:** Proxy не моделирует source-copy, admission, verifier или обычный route. B допускает только подтверждённые typed identifier spans, не всю латиницу.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S186 28-NEW-ENRU-MIXED-SCRIPT-PROTECTED-IDENTIFIER-REJECTION-2026-10-16.md:L7–13](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/28-NEW-ENRU-MIXED-SCRIPT-PROTECTED-IDENTIFIER-REJECTION-2026-10-16.md:7>) | Два конкретных несовместимых gate |
| [S186 28-NEW-ENRU-MIXED-SCRIPT-PROTECTED-IDENTIFIER-REJECTION-2026-10-16.md:L27–35](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/28-NEW-ENRU-MIXED-SCRIPT-PROTECTED-IDENTIFIER-REJECTION-2026-10-16.md:27>) | Ограничения proxy и bounded proposed B |
| [S184 28-MICRO-UNICODE-AND-SCRIPT-SOURCE-PROXY.mjs:L14–35](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/28-MICRO-UNICODE-AND-SCRIPT-SOURCE-PROXY.mjs:14>) | Формула strict/phrase и три контрольных targets |
| [S183 28-MICRO-UNICODE-AND-SCRIPT-SOURCE-PROXY-RESULT.json:L11–23](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/28-MICRO-UNICODE-AND-SCRIPT-SOURCE-PROXY-RESULT.json:11>) | mixed false/true, Cyrillic true/true, Latin false/false |
| [S182 28-MICRO-UNICODE-AND-MIXED-SCRIPT-LEARNING-EXPERIMENTS-2026-10-16.md:L23–29](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/28-MICRO-UNICODE-AND-MIXED-SCRIPT-LEARNING-EXPERIMENTS-2026-10-16.md:23>) | Один уже извлечённый кандидат, затем настоящий consumer |
| [S175 28-DELTA-CROSSWALK-NOVEL-UNICODE-AND-SCRIPT-2026-10-16.md:L21–24](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/28-DELTA-CROSSWALK-NOVEL-UNICODE-AND-SCRIPT-2026-10-16.md:21>) | Повтор той же причины, не отдельный опыт |

<a id="c004"></a>
### C004. Rejected annotation RULE становится частью positive training lookup key, но ordinary unseen source не содержит этой учительской аннотации.

**Сравнение:** [P52.1](cards/P52-semantic-annotation.md#s-1), [P52.6](cards/P52-semantic-annotation.md#s-6).

**Допущения и предел:** Exact correction и prefix/structure могут скрыть quality effect. Нельзя утверждать NO_HIT без route trace; чистый neural DPO не использует этот builder.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S202 29-NEW-REJECTED-ANNOTATION-IN-TRAIN-ONLY-KNN-KEY-2026-10-16.md:L13–18](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/29-NEW-REJECTED-ANNOTATION-IN-TRAIN-ONLY-KNN-KEY-2026-10-16.md:13>) | Producer→три Unit examples→runtime key mismatch |
| [S202 29-NEW-REJECTED-ANNOTATION-IN-TRAIN-ONLY-KNN-KEY-2026-10-16.md:L20–27](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/29-NEW-REJECTED-ANNOTATION-IN-TRAIN-ONLY-KNN-KEY-2026-10-16.md:20>) | Контрпример и masking альтернативными buckets |

<a id="c005"></a>
### C005. First eligible source token→first target word не является bilingual alignment; два правильных перевода с инверсией порядка могут записать ложные mappings и перезаписать друг друга.

**Сравнение:** [P52.2](cards/P52-semantic-annotation.md#s-2).

**Допущения и предел:** Изолировать positional origin: earlier marker может уже занять key; word-map coverage/приоритет и import могут скрыть/изменить результат. Exact recall не является unseen transfer.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S201 29-NEW-FIRST-TOKEN-PSEUDOALIGNMENT-ORDER-CONTAMINATION-2026-10-16.md:L11–15](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/29-NEW-FIRST-TOKEN-PSEUDOALIGNMENT-ORDER-CONTAMINATION-2026-10-16.md:11>) | Фактический producer, AddWordMapping overwrite и consumer |
| [S201 29-NEW-FIRST-TOKEN-PSEUDOALIGNMENT-ORDER-CONTAMINATION-2026-10-16.md:L21–33](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/29-NEW-FIRST-TOKEN-PSEUDOALIGNMENT-ORDER-CONTAMINATION-2026-10-16.md:21>) | Два урока Книгу долго... и независимый unseen запрос |

<a id="c006"></a>
### C006. Более ранний ExtractSourceMarker→first target heuristic может создать ту же ложную запись до positional fallback и маскировать его ablation.

**Сравнение:** [P52.7](cards/P52-semantic-annotation.md#s-7).

**Допущения и предел:** Это отдельный origin загрязнения. В P52.2 он фиксирован; в P52.7 меняется только marker policy. Реальная POS/RuleTags трасса обязательна.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S201 29-NEW-FIRST-TOKEN-PSEUDOALIGNMENT-ORDER-CONTAMINATION-2026-10-16.md:L28–30](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/29-NEW-FIRST-TOKEN-PSEUDOALIGNMENT-ORDER-CONTAMINATION-2026-10-16.md:28>) | Почему простой Книгу я... не изолирует positional; долго→I/He — отдельный confound |

<a id="c007"></a>
### C007. После удаления punctuation-only токенов phrase composer использует compacted i как индекс исходного tokens.Tokens и выбирает чужую word-map запись.

**Сравнение:** [P54.2](cards/P54-phrase-composition.md#s-2).

**Допущения и предел:** Предусловия: отдельный whitespace-delimited символ, частичный phrase match и доступный word-map. Исправление originalIndex не исправляет ложное соседство через удалённый оператор (P54.4).

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S253 33-NEW-PHRASE-FILTERED-ORDINAL-INDEX-DRIFT-2026-10-16.md:L7–18](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/33-NEW-PHRASE-FILTERED-ORDINAL-INDEX-DRIFT-2026-10-16.md:7>) | Точная причина и приоритет phrase-path |
| [S253 33-NEW-PHRASE-FILTERED-ORDINAL-INDEX-DRIFT-2026-10-16.md:L24–40](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/33-NEW-PHRASE-FILTERED-ORDINAL-INDEX-DRIFT-2026-10-16.md:24>) | Четыре варианта положения тире и предел AST/neural |
| [S244 33-MICRO-PHRASE-INDEX-SOURCE-PROXY.mjs:L16–29](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/33-MICRO-PHRASE-INDEX-SOURCE-PROXY.mjs:16>) | Модель i против originalIndex; acceptance≥.85 |
| [S243 33-MICRO-PHRASE-INDEX-SOURCE-PROXY-RESULT.json:L30–74](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/33-MICRO-PHRASE-INDEX-SOURCE-PROXY-RESULT.json:30>) | Leading: check check→check file; interposed: reject→check file |
| [S243 33-MICRO-PHRASE-INDEX-SOURCE-PROXY-RESULT.json:L100–112](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/33-MICRO-PHRASE-INDEX-SOURCE-PROXY-RESULT.json:100>) | Предоставленные 2→4 correctAccepted; gold предполагает несмысловое тире, native отсутствует |

<a id="c008"></a>
### C008. Один TrainOnExample может дважды вставить тот же lesson под равными full/prefix signature, заняв лишние места до top-k.

**Сравнение:** [P60.1](cards/P60-signature-duplicate.md#s-1).

**Допущения и предел:** full==prefix проверяется после admitted RuleTags. Proxy предполагает допустимые ранние wrong-target entries, не реализует admission/source law/preemption; меньшие bytes допустимы как отдельный выигрыш без quality gain.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S266 34-NEW-ONE-LESSON-DOUBLE-INSERT-UNDER-ALIAS-SIGNATURE-2026-10-16.md:L7–21](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/34-NEW-ONE-LESSON-DOUBLE-INSERT-UNDER-ALIAS-SIGNATURE-2026-10-16.md:7>) | Условие alias, три AddPair и сохранение дублей при restore |
| [S266 34-NEW-ONE-LESSON-DOUBLE-INSERT-UNDER-ALIAS-SIGNATURE-2026-10-16.md:L25–37](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/34-NEW-ONE-LESSON-DOUBLE-INSERT-UNDER-ALIAS-SIGNATURE-2026-10-16.md:25>) | Proxy assumptions, Distinct keys within lesson, falsifiers |
| [S264 34-MICRO-KNN-ALIAS-TOPK-SOURCE-PROXY.mjs:L5–36](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/34-MICRO-KNN-ALIAS-TOPK-SOURCE-PROXY.mjs:5>) | Модель full==prefix, top5 и примитив target-script gate |
| [S263 34-MICRO-KNN-ALIAS-TOPK-SOURCE-PROXY-RESULT.json:L4–26](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/34-MICRO-KNN-ALIAS-TOPK-SOURCE-PROXY-RESULT.json:4>) | bucket8→4, недоступный D→выбранный D в модели; не native output |

<a id="c009"></a>
### C009. FIFO admissible.Take(k) может скрыть позднюю правильную correction при сохранении всех уроков в TranslationKnnStore.

**Сравнение:** [P59.2](cards/P59-correction-visibility.md#s-2).

**Допущения и предел:** Нужны actual KNN route, source-law admissibility и действительно неверные старые targets. Это temporal order, не token order P16 и не generic ExampleId conflict P18.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S052 16-KNN-CORRECTION-VISIBILITY-AND-TOPIC-LOSS-2026-10-16.md:L9–19](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/16-KNN-CORRECTION-VISIBILITY-AND-TOPIC-LOSS-2026-10-16.md:9>) | 5 старых+1 новый при k5; reverse order falsifier и четыре masking условия |

<a id="c010"></a>
### C010. Executor принимает TopicDomain, но maps и lookup не используют контекст; одинаковый source разных смыслов перезаписывается, а restore не восстанавливает незаписанное различие.

**Сравнение:** [P51.1](cards/P51-translation-context.md#s-1).

**Допущения и предел:** Finance/geography здесь значения поля контекста перевода, не автоматический список доменов обучения. Сохранить общий перевод и unknown fallback при scoped key.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S052 16-KNN-CORRECTION-VISIBILITY-AND-TOPIC-LOSS-2026-10-16.md:L22–30](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/16-KNN-CORRECTION-VISIBILITY-AND-TOPIC-LOSS-2026-10-16.md:22>) | Ignored field, last writer, checkpoint без контекста и bank-контраст |

<a id="c011"></a>
### C011. Шесть direction-specific Executor делят TranslationKnnStore без direction в ключе; одинаковые подписи могут смешать кандидатов.

**Сравнение:** [P51.2](cards/P51-translation-context.md#s-2), [P117.2](cards/P117-translation-direction-memory.md#s-2).

**Допущения и предел:** Смешение условно: последующие guards могут отсеять target. Одинаковая Latin target script у нескольких →EN направлений — отдельный различающий случай.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S052 16-KNN-CORRECTION-VISIBILITY-AND-TOPIC-LOSS-2026-10-16.md:L32–33](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/16-KNN-CORRECTION-VISIBILITY-AND-TOPIC-LOSS-2026-10-16.md:32>) | Shared owner и отсутствие direction namespace |

<a id="c012"></a>
### C012. Exact-source return обходит target-direction check на уровне Executor, хотя поздний adapter/verifier ещё может отвергнуть ответ.

**Сравнение:** [P117.3](cards/P117-translation-direction-memory.md#s-3).

**Допущения и предел:** Не утверждает конечное нарушение. Сравнить exact и не exact путь с одним кандидатом/памятью; скрипт не равен полноценной language correctness.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S052 16-KNN-CORRECTION-VISIBILITY-AND-TOPIC-LOSS-2026-10-16.md:L35–36](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/16-KNN-CORRECTION-VISIBILITY-AND-TOPIC-LOSS-2026-10-16.md:35>) | Exact bypass и поздняя граница |
| [S186 28-NEW-ENRU-MIXED-SCRIPT-PROTECTED-IDENTIFIER-REJECTION-2026-10-16.md:L11–24](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/28-NEW-ENRU-MIXED-SCRIPT-PROTECTED-IDENTIFIER-REJECTION-2026-10-16.md:11>) | Тот же exact-path контраст как уточнение route asymmetry |

<a id="c013"></a>
### C013. Phrase candidate confidence .86 может пройти adapter при verification.Pass=false, поэтому обнаруженная ошибка индекса не обязательно блокируется verifier.

**Сравнение:** [P56.4](cards/P56-verifier-context.md#s-4).

**Допущения и предел:** Это ранее известный bypass, не новая причина N33. Product/Flow может добавить ограничения; нужен actual caller.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S253 33-NEW-PHRASE-FILTERED-ORDINAL-INDEX-DRIFT-2026-10-16.md:L18–18](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/33-NEW-PHRASE-FILTERED-ORDINAL-INDEX-DRIFT-2026-10-16.md:18>) | Конкретный .85 guard против .86 phrase confidence |
| [S253 33-NEW-PHRASE-FILTERED-ORDINAL-INDEX-DRIFT-2026-10-16.md:L44–44](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/33-NEW-PHRASE-FILTERED-ORDINAL-INDEX-DRIFT-2026-10-16.md:44>) | Masking/falsifier, не безусловное wrong output |

<a id="c014"></a>
### C014. N33 называет TrainBatchAsync train-consumer со ссылкой L51–119, но этот span пересекает legacy scaffold и structured method; прежний источник отдельно указывает отсутствие Unit update в legacy.

**Сравнение:** [P58.3](cards/P58-feedback-learning.md#s-3), [P53.1](cards/P53-batch-atomicity.md#s-1).

**Допущения и предел:** Противоречие атрибуции источников, не новая product mutation. В будущем trace обязан назвать фактическую перегрузку; N33 citation сама не доказывает обучение legacy.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S253 33-NEW-PHRASE-FILTERED-ORDINAL-INDEX-DRIFT-2026-10-16.md:L17–17](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/33-NEW-PHRASE-FILTERED-ORDINAL-INDEX-DRIFT-2026-10-16.md:17>) | Спорное имя caller в сводке |
| [S022 10-TRANSLATION-UNIT-AND-RESTORE-EDGE-GAPS.md:L4–8](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/10-TRANSLATION-UNIT-AND-RESTORE-EDGE-GAPS.md:4>) | Legacy +.01 и отличие TrainStructuredBatchAsync |

<a id="c015"></a>
### C015. Исторический E09 baseline оценён на whole validation, kNN на val.Take(500); E34 baseline whole val против fusion valSecond после tuning на valFirst.

**Сравнение:** [P27.5](cards/P27-structural-codec.md#s-5), [P30.1](cards/P30-learned-fusion-alpha.md#s-1).

**Допущения и предел:** Сохранённый положительный сигнал не отменяется, но две цифры не дают чистого causal gain без matched cases. Это отдельно от codec/split leakage.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S001 00-MASTER-SYNTHESIS-AND-CONFLICTS.md:L17–17](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/00-MASTER-SYNTHESIS-AND-CONFLICTS.md:17>) | Обе точные несовпадающие панели |
| [S175 28-DELTA-CROSSWALK-NOVEL-UNICODE-AND-SCRIPT-2026-10-16.md:L12–12](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/28-DELTA-CROSSWALK-NOVEL-UNICODE-AND-SCRIPT-2026-10-16.md:12>) | Повтор E09 denominator caveat, не новое наблюдение |

<a id="c016"></a>
### C016. E09 BuildTokenVectors заново строит kindMap на каждый файл; один SyntaxKind может получить разные координаты/labels.

**Сравнение:** [P27.4](cards/P27-structural-codec.md#s-4).

**Допущения и предел:** Это отдельное несогласование от production RawKind string/typed codec в P27.1. Новый global map не устраняет unmatched eval.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S001 00-MASTER-SYNTHESIS-AND-CONFLICTS.md:L44–44](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/00-MASTER-SYNTHESIS-AND-CONFLICTS.md:44>) | Перефайловый kindMap и граница старого PASS |

<a id="c017"></a>
### C017. UNKNOWN-токены не входят в full semantic signature, поэтому новое условие может остаться невидимым при одинаковых Pattern/known concepts/rule tags.

**Сравнение:** [P61.1](cards/P61-unknown-tail.md#s-1).

**Допущения и предел:** Actual training PAIR/rejected tags и ordinary exact/phrase shortcuts могут изменить trigger; C# AST имеет другой owner.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S320 37-NEW-KNN-UNKNOWN-TAIL-AND-POSTFILTER-CASCADE-2026-10-16.md:L19–34](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-KNN-UNKNOWN-TAIL-AND-POSTFILTER-CASCADE-2026-10-16.md:19>) | Причина key collision, actual-key falsifier и границы переноса |

<a id="c018"></a>
### C018. true/false имеют TECH_TERM POS, но non-Russian ResolveConcept оставляет их UNKNOWN; пример null customerId42 true/false сохраняет известные concepts.

**Сравнение:** [P61.2](cards/P61-unknown-tail.md#s-2).

**Допущения и предел:** Это более узкая source-backed гипотеза, чем любой UNKNOWN-tail; выдуманных enum TRUE_LITERAL/FALSE_LITERAL в проекте нет.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S320 37-NEW-KNN-UNKNOWN-TAIL-AND-POSTFILTER-CASCADE-2026-10-16.md:L36–42](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-KNN-UNKNOWN-TAIL-AND-POSTFILTER-CASCADE-2026-10-16.md:36>) | Boolean contrast и признанная поправка раннего proxy |

<a id="c019"></a>
### C019. Добавленный RuleTag может переключить full-tier source threshold с .35 на 0 и тем самым ослабить guard при тех же source текстах.

**Сравнение:** [P62.1](cards/P62-signature-monotonicity.md#s-1).

**Допущения и предел:** Искусственный TYPE|NOUN|VERB key проверяет store, но не доказывает производство такого key tokenizer; real RU-tag path ещё требует actual equality.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S318 37-NEW-KNN-SIGNATURE-GUARD-MONOTONICITY-CONTAINMENT-AND-CASCADE-2026-10-16.md:L7–19](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-KNN-SIGNATURE-GUARD-MONOTONICITY-CONTAINMENT-AND-CASCADE-2026-10-16.md:7>) | Немонотонный threshold и контролируемый blue widget/green vehicle |
| [S320 37-NEW-KNN-UNKNOWN-TAIL-AND-POSTFILTER-CASCADE-2026-10-16.md:L21–22](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-KNN-UNKNOWN-TAIL-AND-POSTFILTER-CASCADE-2026-10-16.md:21>) | Связь guard с узким SourceLaw, который может вернуть Unknown |

<a id="c020"></a>
### C020. SourceSimilarity измеряет overlap/queryCount, поэтому источник-супермножество с чужим условием может пройти там, где symmetric Jaccard ниже порога.

**Сравнение:** [P63.1](cards/P63-source-containment.md#s-1).

**Допущения и предел:** Asymmetry не означает, что конкретный gate изменился: proxy1/.5 оба выше.35; Jaccard не объявлен универсально правильным.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S318 37-NEW-KNN-SIGNATURE-GUARD-MONOTONICITY-CONTAINMENT-AND-CASCADE-2026-10-16.md:L21–27](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-KNN-SIGNATURE-GUARD-MONOTONICITY-CONTAINMENT-AND-CASCADE-2026-10-16.md:21>) | delete records:2/2 против 2/6 и boundary.35 |
| [S320 37-NEW-KNN-UNKNOWN-TAIL-AND-POSTFILTER-CASCADE-2026-10-16.md:L52–60](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-KNN-UNKNOWN-TAIL-AND-POSTFILTER-CASCADE-2026-10-16.md:52>) | Уточнение случая без threshold crossing |
| [S296 37-MICRO-CASCADE-UNKNOWN-PROXY.mjs:L5–24](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-MICRO-CASCADE-UNKNOWN-PROXY.mjs:5>) | Предоставленная модель множеств и directional formula, не исполнена нами |
| [S295 37-MICRO-CASCADE-UNKNOWN-PROXY-RESULT.json:L5–11](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-MICRO-CASCADE-UNKNOWN-PROXY-RESULT.json:5>) | Receipt1/.5/.5 без native translation |

<a id="c021"></a>
### C021. Английское not удаляется из SourceTokens, а SourceLaw распознаёт только узкую copular-конструкцию; вне неё полярность может не защитить retrieval.

**Сравнение:** [P63.2](cards/P63-source-containment.md#s-2), [P55.1](cards/P55-translation-laws.md#s-1).

**Допущения и предел:** Сохранение not само по себе не гарантирует threshold crossing; native query и закон проверяются отдельно, non-English token не приравнивается к not.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S320 37-NEW-KNN-UNKNOWN-TAIL-AND-POSTFILTER-CASCADE-2026-10-16.md:L25–25](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-KNN-UNKNOWN-TAIL-AND-POSTFILTER-CASCADE-2026-10-16.md:25>) | Stop-list и формула |
| [S320 37-NEW-KNN-UNKNOWN-TAIL-AND-POSTFILTER-CASCADE-2026-10-16.md:L60–60](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-KNN-UNKNOWN-TAIL-AND-POSTFILTER-CASCADE-2026-10-16.md:60>) | Ограничение SourceLaw |
| [S296 37-MICRO-CASCADE-UNKNOWN-PROXY.mjs:L5–7](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-MICRO-CASCADE-UNKNOWN-PROXY.mjs:5>) | Модель stop-list явно содержит not/no; это не обычный consumer |

<a id="c022"></a>
### C022. Непустой full-tier останавливает cascade до проверки target; если все его ответы отвергнуты, пригодный prefix не посещается.

**Сравнение:** [P64.1](cards/P64-tier-postfilter.md#s-1).

**Допущения и предел:** Отдельно от FIFO, direction collision и alias duplicate; до ordinary выигрыша требуется достигнутый KNN route.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S318 37-NEW-KNN-SIGNATURE-GUARD-MONOTONICITY-CONTAINMENT-AND-CASCADE-2026-10-16.md:L29–37](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-KNN-SIGNATURE-GUARD-MONOTONICITY-CONTAINMENT-AND-CASCADE-2026-10-16.md:29>) | Две стадии eligibility и контрпример full wrong-language/prefix valid |
| [S320 37-NEW-KNN-UNKNOWN-TAIL-AND-POSTFILTER-CASCADE-2026-10-16.md:L44–50](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-KNN-UNKNOWN-TAIL-AND-POSTFILTER-CASCADE-2026-10-16.md:44>) | Backoff только после всех rejected и сохранение SourceLaw/Verifier |
| [S296 37-MICRO-CASCADE-UNKNOWN-PROXY.mjs:L26–61](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-MICRO-CASCADE-UNKNOWN-PROXY.mjs:26>) | Controlled abstract keys/buckets; toy target regex, не full native filters |
| [S295 37-MICRO-CASCADE-UNKNOWN-PROXY-RESULT.json:L12–25](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-MICRO-CASCADE-UNKNOWN-PROXY-RESULT.json:12>) | Предоставленная трасса baselineFinal null/prefix result, не quality |

<a id="c023"></a>
### C023. RU_EN history Jaccard>=.70 удаляет polarity/order/quantity-changing lessons вместе с настоящими повторами до обучения.

**Сравнение:** [P65.1](cards/P65-history-admission.md#s-1), [P65.2](cards/P65-history-admission.md#s-2).

**Допущения и предел:** B с verified TRAIN contrast manifest — контролируемый input; скрытый TEST gold запрещён. Другие языковые gates не доказаны.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S322 37-NEW-RUEN-HISTORY-ADMISSION-AND-CONTRAST-BLINDSPOT-2026-10-16.md:L15–16](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-RUEN-HISTORY-ADMISSION-AND-CONTRAST-BLINDSPOT-2026-10-16.md:15>) | Реальный owner/caller по источнику |
| [S322 37-NEW-RUEN-HISTORY-ADMISSION-AND-CONTRAST-BLINDSPOT-2026-10-16.md:L26–38](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-RUEN-HISTORY-ADMISSION-AND-CONTRAST-BLINDSPOT-2026-10-16.md:26>) | Три meaning-changing contrasts и два controls |
| [S328 37-ruen-history-gate-source-proxy.py:L5–34](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-ruen-history-gate-source-proxy.py:5>) | Модель token set и threshold, не исполнялась нами |
| [S327 37-ruen-history-gate-source-proxy-result.json:L9–45](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-ruen-history-gate-source-proxy-result.json:9>) | Предоставленные DROP/KEEP результаты и native NOT_RUN |

<a id="c024"></a>
### C024. RU_EN heldout guard использует lexical overlap>.70 без порядка/полярности и может исключить независимый минимальный смысловой контраст.

**Сравнение:** [P66.1](cards/P66-heldout-contrast.md#s-1).

**Допущения и предел:** Нужна отдельная source-family-independent diagnostic panel; изменение denominator не качество модели. Training>=.70 и exam>.70 различаются на boundary.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S322 37-NEW-RUEN-HISTORY-ADMISSION-AND-CONTRAST-BLINDSPOT-2026-10-16.md:L17–17](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-RUEN-HISTORY-ADMISSION-AND-CONTRAST-BLINDSPOT-2026-10-16.md:17>) | Отдельный evaluator predicate |
| [S322 37-NEW-RUEN-HISTORY-ADMISSION-AND-CONTRAST-BLINDSPOT-2026-10-16.md:L38–40](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-RUEN-HISTORY-ADMISSION-AND-CONTRAST-BLINDSPOT-2026-10-16.md:38>) | Admission и examiner-selection разделены |
| [S328 37-ruen-history-gate-source-proxy.py:L23–34](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-ruen-history-gate-source-proxy.py:23>) | Два различных неравенства |
| [S327 37-ruen-history-gate-source-proxy-result.json:L7–8](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-ruen-history-gate-source-proxy-result.json:7>) | Сохранённые threshold значения; boundary ровно.70 receipt не содержит |

<a id="c025"></a>
### C025. History gate видит corpus, но не все supplemental/in-batch конфликты; ExactSourceMap last writer может как исправить, так и вернуть устаревший перевод.

**Сравнение:** [P67.1](cards/P67-correction-identity.md#s-1).

**Допущения и предел:** Порядок сравнивается при том же наборе уроков и реальном gold; last-write сам по себе не дефект.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S322 37-NEW-RUEN-HISTORY-ADMISSION-AND-CONTRAST-BLINDSPOT-2026-10-16.md:L18–20](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-RUEN-HISTORY-ADMISSION-AND-CONTRAST-BLINDSPOT-2026-10-16.md:18>) | Producer gaps и exact-source приоритет |
| [S322 37-NEW-RUEN-HISTORY-ADMISSION-AND-CONTRAST-BLINDSPOT-2026-10-16.md:L42–42](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-RUEN-HISTORY-ADMISSION-AND-CONTRAST-BLINDSPOT-2026-10-16.md:42>) | Самостоятельная overwrite hypothesis |
| [S328 37-ruen-history-gate-source-proxy.py:L35–45](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-ruen-history-gate-source-proxy.py:35>) | Toy dictionary assignment по двум provenance, без native machine |
| [S327 37-ruen-history-gate-source-proxy-result.json:L46–49](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-ruen-history-gate-source-proxy-result.json:46>) | OUT_SUPPLEMENTAL receipt, не доказательство правильного перевода |

<a id="c026"></a>
### C026. ProjectWiki повторный PairId блокируется на trainer/store/pilot gates и сохраняется через checkpoint; content revision может не дойти.

**Сравнение:** [P67.2](cards/P67-correction-identity.md#s-2).

**Допущения и предел:** При immutable PairId и новой identity для revision это корректная idempotency. B не отменяет byte-identical retry protection.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S192 28-NEW-PROJECTWIKI-MEMORY-CONSUMPTION-AND-CORRECTION-ID-2026-10-16.md:L18–22](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/28-NEW-PROJECTWIKI-MEMORY-CONSUMPTION-AND-CORRECTION-ID-2026-10-16.md:18>) | Три admission consumers, cold restore и conditional revision contract |

<a id="c027"></a>
### C027. ProjectWiki Mode2/3 принимает learned pair, но renderer использует PairId/FlowRef, не Chosen.PlanText/Title; Mode1 читает Chosen.

**Сравнение:** [P118.1](cards/P118-projectwiki-content.md#s-1), [P118.2](cards/P118-projectwiki-content.md#s-2), [P118.3](cards/P118-projectwiki-content.md#s-3).

**Допущения и предел:** Payload consumption и routing отдельны; exact recall candidate не обещает генерацию новых plans.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S192 28-NEW-PROJECTWIKI-MEMORY-CONSUMPTION-AND-CORRECTION-ID-2026-10-16.md:L9–16](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/28-NEW-PROJECTWIKI-MEMORY-CONSUMPTION-AND-CORRECTION-ID-2026-10-16.md:9>) | Source→memory→consumer и два диагностических interventions |

<a id="c028"></a>
### C028. ProjectWiki scorer может засчитать flags/FlowRef либо Contains(ExpectedResponse), не проверяя fidelity newly learned Chosen.

**Сравнение:** [P118.4](cards/P118-projectwiki-content.md#s-4).

**Допущения и предел:** Не объявляем конкретный historical PASS ложным без его raw outputs; сравниваем один frozen output набор двумя scorers.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S192 28-NEW-PROJECTWIKI-MEMORY-CONSUMPTION-AND-CORRECTION-ID-2026-10-16.md:L14–14](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/28-NEW-PROJECTWIKI-MEMORY-CONSUMPTION-AND-CORRECTION-ID-2026-10-16.md:14>) | Два evaluator контракта и оговорка о старых PASS |

<a id="c029"></a>
### C029. OrchestrationKnnStore выбирает nearest даже при всех нулевых Jaccard scores.

**Сравнение:** [P118.5](cards/P118-projectwiki-content.md#s-5), [P93.1](cards/P93-confidence-abstention.md#s-1).

**Допущения и предел:** No-match полезен лишь вместе с coverage/false abstentions; конкретный zero boundary, а не новая универсальная confidence шкала.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S192 28-NEW-PROJECTWIKI-MEMORY-CONSUMPTION-AND-CORRECTION-ID-2026-10-16.md:L11–11](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/28-NEW-PROJECTWIKI-MEMORY-CONSUMPTION-AND-CORRECTION-ID-2026-10-16.md:11>) | Нулевое сходство допускает nearest pair |

<a id="c030"></a>
### C030. DatasetBuilder вставляет обычный byte ID1 как EOF; EosToken=vocab−1 тоже не гарантирует reserved EOD.

**Сравнение:** [P68.2](cards/P68-pretrain-pipeline.md#s-2).

**Допущения и предел:** В 629vocab ID628 — последний merge; proposed A/B фиксирует одинаковую model shape, не меняет вместе attention/loss policy.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S267 34-NEW-PRETRAIN-SOURCE-AND-BATCHING-GAPS-2026-10-16.md:L16–30](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/34-NEW-PRETRAIN-SOURCE-AND-BATCHING-GAPS-2026-10-16.md:16>) | Фактический marker и пределы EOD |
| [S271 34-SOURCE-ONLY-DATA-PIPELINE-PROXY.mjs:L24–35](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/34-SOURCE-ONLY-DATA-PIPELINE-PROXY.mjs:24>) | Арифметика 1/628 и SOH, не NPY/model |
| [S270 34-SOURCE-ONLY-DATA-PIPELINE-PROXY-RESULT.json:L108–114](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/34-SOURCE-ONLY-DATA-PIPELINE-PROXY-RESULT.json:108>) | Предоставленные marker fields |
| [S265 34-MICRO-PRETRAIN-DOMAIN-AB-EXPERIMENTS-2026-10-16.md:L20–27](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/34-MICRO-PRETRAIN-DOMAIN-AB-EXPERIMENTS-2026-10-16.md:20>) | Предложенный code-document boundary опыт; составные policy здесь разделены |

<a id="c031"></a>
### C031. DataLoader с _pos++ предъявляет сильно перекрывающиеся окна, но stride1 может быть полезным режимом; цена зависит от бюджета.

**Сравнение:** [P68.1](cards/P68-pretrain-pipeline.md#s-1).

**Допущения и предел:** distinct ID values из toy receipt не равны unique source positions; equal updates и equal coverage — разные сравнения.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S267 34-NEW-PRETRAIN-SOURCE-AND-BATCHING-GAPS-2026-10-16.md:L32–39](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/34-NEW-PRETRAIN-SOURCE-AND-BATCHING-GAPS-2026-10-16.md:32>) | Причина overlap и отказ от blanket bug verdict |
| [S271 34-SOURCE-ONLY-DATA-PIPELINE-PROXY.mjs:L9–23](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/34-SOURCE-ONLY-DATA-PIPELINE-PROXY.mjs:9>) | Модель неповторяющихся IDs и windows |
| [S270 34-SOURCE-ONLY-DATA-PIPELINE-PROXY-RESULT.json:L7–107](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/34-SOURCE-ONLY-DATA-PIPELINE-PROXY-RESULT.json:7>) | Три окна и 6/12 различных ID values |
| [S265 34-MICRO-PRETRAIN-DOMAIN-AB-EXPERIMENTS-2026-10-16.md:L29–37](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/34-MICRO-PRETRAIN-DOMAIN-AB-EXPERIMENTS-2026-10-16.md:29>) | Translation budget/design proposal |

<a id="c032"></a>
### C032. EnsureAvailable пропускает слишком короткие shards и теряет окна через межшардовую границу, а не обязательно все конечные токены длинного shard.

**Сравнение:** [P68.3](cards/P68-pretrain-pipeline.md#s-3), [P68.6](cards/P68-pretrain-pipeline.md#s-6).

**Допущения и предел:** H-source не моделируется supplied JS; уточнение O69 предотвращает ложное 'весь хвост потерян'. Cross-document/split склейка не разрешена.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S267 34-NEW-PRETRAIN-SOURCE-AND-BATCHING-GAPS-2026-10-16.md:L49–55](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/34-NEW-PRETRAIN-SOURCE-AND-BATCHING-GAPS-2026-10-16.md:49>) | seqLen+1, reset и различие document/shard boundary |
| [S265 34-MICRO-PRETRAIN-DOMAIN-AB-EXPERIMENTS-2026-10-16.md:L47–52](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/34-MICRO-PRETRAIN-DOMAIN-AB-EXPERIMENTS-2026-10-16.md:47>) | План boundary/cold restore, не receipt выполненного сравнения |

<a id="c033"></a>
### C033. Directory.GetFiles без собственной сортировки меняет возможное соседство corpus и раннее покрытие; _seqLen builder не выравнивает shards.

**Сравнение:** [P68.7](cards/P68-pretrain-pipeline.md#s-7).

**Допущения и предел:** Не утверждается, что порядок обязательно различается при каждом запуске; deterministic manifest сравнивается отдельно от window policy.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S267 34-NEW-PRETRAIN-SOURCE-AND-BATCHING-GAPS-2026-10-16.md:L51–55](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/34-NEW-PRETRAIN-SOURCE-AND-BATCHING-GAPS-2026-10-16.md:51>) | Enumeration и shardSize |
| [S265 34-MICRO-PRETRAIN-DOMAIN-AB-EXPERIMENTS-2026-10-16.md:L47–52](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/34-MICRO-PRETRAIN-DOMAIN-AB-EXPERIMENTS-2026-10-16.md:47>) | Source manifest proposal, у нас разделён на факторы |

<a id="c034"></a>
### C034. ushort clamp сливает 65536 и 70000 в 65535, но корректный текущий 629vocab не достигает этой ветви.

**Сравнение:** [P68.5](cards/P68-pretrain-pipeline.md#s-5).

**Допущения и предел:** Большой vocab/повреждённые IDs — отдельный trigger; runtime range check после clamp не восстанавливает потерянный ID.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S267 34-NEW-PRETRAIN-SOURCE-AND-BATCHING-GAPS-2026-10-16.md:L41–47](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/34-NEW-PRETRAIN-SOURCE-AND-BATCHING-GAPS-2026-10-16.md:41>) | Writer/reader и предел текущего vocab |
| [S271 34-SOURCE-ONLY-DATA-PIPELINE-PROXY.mjs:L25–37](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/34-SOURCE-ONLY-DATA-PIPELINE-PROXY.mjs:25>) | Числовая saturation модель |
| [S270 34-SOURCE-ONLY-DATA-PIPELINE-PROXY-RESULT.json:L115–140](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/34-SOURCE-ONLY-DATA-PIPELINE-PROXY-RESULT.json:115>) | Предоставленные collisions; script сам не пишет NPY |

<a id="c035"></a>
### C035. GraphRAG embedding при ordinary ненулевой ветке видит первые actual maxLen BPE IDs;512 лишь constructor default, а не 512characters.

**Сравнение:** [P69.1](cards/P69-graphrag-context.md#s-1).

**Допущения и предел:** Прежняя родительская атрибуция 512characters отвергнута после readback. Hash fallback .6 читает полный input; Transformer-KNN64 — иной owner.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S281 35-NEW-GRAPHRAG-EMBEDDING-PREFIX-METADATA-AND-HIDDEN-FALLBACK-2026-10-16.md:L12–24](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/35-NEW-GRAPHRAG-EMBEDDING-PREFIX-METADATA-AND-HIDDEN-FALLBACK-2026-10-16.md:12>) | BPE input, defaultmaxLen и предел равенства vectors |
| [S275 35-MICRO-GRAPHRAG-PREFIX-METADATA-AND-FALLBACK-DOMAIN-EXPERIMENTS-2026-10-16.md:L12–20](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/35-MICRO-GRAPHRAG-PREFIX-METADATA-AND-FALLBACK-DOMAIN-EXPERIMENTS-2026-10-16.md:12>) | Head-tail same-budget и пересчёт query/document index обеих arms |

<a id="c036"></a>
### C036. Непустой RawText вытесняет Name/Signature из embedding input, хотя metadata может хранить name/type/path.

**Сравнение:** [P69.2](cards/P69-graphrag-context.md#s-2).

**Допущения и предел:** Важность Signature должна быть предметно обоснована; короткий материал исключает truncation confound.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S281 35-NEW-GRAPHRAG-EMBEDDING-PREFIX-METADATA-AND-HIDDEN-FALLBACK-2026-10-16.md:L26–32](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/35-NEW-GRAPHRAG-EMBEDDING-PREFIX-METADATA-AND-HIDDEN-FALLBACK-2026-10-16.md:26>) | Field priority и overloaded identity contrast |
| [S275 35-MICRO-GRAPHRAG-PREFIX-METADATA-AND-FALLBACK-DOMAIN-EXPERIMENTS-2026-10-16.md:L22–30](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/35-MICRO-GRAPHRAG-PREFIX-METADATA-AND-FALLBACK-DOMAIN-EXPERIMENTS-2026-10-16.md:22>) | Отдельный short-text metadata A/B |

<a id="c037"></a>
### C037. Pooled norm<=1e−6 скрыто включает bag-of-token-ID modulo embedding полного текста вместо learned hidden vector.

**Сравнение:** [P69.6](cards/P69-graphrag-context.md#s-6).

**Допущения и предел:** Частота в ordinary runtime неизвестна, empty Encode может не достичь ветви. Hash-полезность допустима, но не приписывается LoRA.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S281 35-NEW-GRAPHRAG-EMBEDDING-PREFIX-METADATA-AND-HIDDEN-FALLBACK-2026-10-16.md:L34–38](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/35-NEW-GRAPHRAG-EMBEDDING-PREFIX-METADATA-AND-HIDDEN-FALLBACK-2026-10-16.md:34>) | Fallback branch, отсутствие flag и full-input exception к prefix |
| [S275 35-MICRO-GRAPHRAG-PREFIX-METADATA-AND-FALLBACK-DOMAIN-EXPERIMENTS-2026-10-16.md:L32–38](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/35-MICRO-GRAPHRAG-PREFIX-METADATA-AND-FALLBACK-DOMAIN-EXPERIMENTS-2026-10-16.md:32>) | Controlled zero activations и сравнение с abstention на достигнутой ветви |

<a id="c038"></a>
### C038. При empty seeds непустой graph может заполнить GlobalSearch и ContextText узлами, не подтверждёнными vector search.

**Сравнение:** [P69.3](cards/P69-graphrag-context.md#s-3).

**Допущения и предел:** Local branch и final consumer могут изменить итог; no answer после B не автоматически quality win.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S291 36-NEW-GRAPHRAG-SEEDLESS-PAGERANK-FALSE-EVIDENCE-2026-10-16.md:L16–25](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/36-NEW-GRAPHRAG-SEEDLESS-PAGERANK-FALSE-EVIDENCE-2026-10-16.md:16>) | Control flow до ContextText и empty-index construction |
| [S289 36-MICRO-SEEDLESS-PAGERANK-SOURCE-PROXY.mjs:L7–40](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/36-MICRO-SEEDLESS-PAGERANK-SOURCE-PROXY.mjs:7>) | Математическая модель isolated graph, списки wouldInclude не native |
| [S288 36-MICRO-SEEDLESS-PAGERANK-SOURCE-PROXY-RESULT.json:L5–20](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/36-MICRO-SEEDLESS-PAGERANK-SOURCE-PROXY-RESULT.json:5>) | Предоставленное residual mass,3IDs, native не запускался |

<a id="c039"></a>
### C039. Stale-only ANN IDs дают нулевую персонализацию, но GlobalSearch всё равно может вернуть существующие чужие graph nodes.

**Сравнение:** [P69.4](cards/P69-graphrag-context.md#s-4).

**Допущения и предел:** Случай отличается от empty seed и обычного recall loss; mixed valid/stale контроль сохраняет valid evidence.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S291 36-NEW-GRAPHRAG-SEEDLESS-PAGERANK-FALSE-EVIDENCE-2026-10-16.md:L16–27](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/36-NEW-GRAPHRAG-SEEDLESS-PAGERANK-FALSE-EVIDENCE-2026-10-16.md:16>) | Unfiltered IDs и stale-only cause |
| [S289 36-MICRO-SEEDLESS-PAGERANK-SOURCE-PROXY.mjs:L14–31](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/36-MICRO-SEEDLESS-PAGERANK-SOURCE-PROXY.mjs:14>) | Формула mass по отсутствующим seeds |
| [S288 36-MICRO-SEEDLESS-PAGERANK-SOURCE-PROXY-RESULT.json:L22–39](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/36-MICRO-SEEDLESS-PAGERANK-SOURCE-PROXY-RESULT.json:22>) | Предоставленные mass0 и 3returned IDs |

<a id="c040"></a>
### C040. Даже при valid seed Take(topK) может добавить isolated zero-rank nodes; valid-seed control не доказывает релевантность всех returned IDs.

**Сравнение:** [P69.9](cards/P69-graphrag-context.md#s-9).

**Допущения и предел:** Strict positive rank candidate не доказывает семантическую пригодность оставшихся nodes.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S291 36-NEW-GRAPHRAG-SEEDLESS-PAGERANK-FALSE-EVIDENCE-2026-10-16.md:L33–41](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/36-NEW-GRAPHRAG-SEEDLESS-PAGERANK-FALSE-EVIDENCE-2026-10-16.md:33>) | Valid B control возвращает также A/C |
| [S289 36-MICRO-SEEDLESS-PAGERANK-SOURCE-PROXY.mjs:L29–30](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/36-MICRO-SEEDLESS-PAGERANK-SOURCE-PROXY.mjs:29>) | Take3 без positive-score filter |
| [S288 36-MICRO-SEEDLESS-PAGERANK-SOURCE-PROXY-RESULT.json:L41–58](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/36-MICRO-SEEDLESS-PAGERANK-SOURCE-PROXY-RESULT.json:41>) | ValidSeedControl3IDs и mass1; nativeNOT_RUN |

<a id="c041"></a>
### C041. BuildContext break на длинном втором snippet пропускает следующий короткий закон, хотя он есть в Results и помещается.

**Сравнение:** [P69.5](cards/P69-graphrag-context.md#s-5).

**Допущения и предел:** Длины относятся к formatted snippet, а не rawText; реальный consumer ContextText ещё требуется.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S187 28-NEW-GRAPHRAG-CONTEXT-BUDGET-EARLY-BREAK-AND-EVIDENCE-OMISSION-2026-10-16.md:L14–27](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/28-NEW-GRAPHRAG-CONTEXT-BUDGET-EARLY-BREAK-AND-EVIDENCE-OMISSION-2026-10-16.md:14>) | 11990/30/5 при 12000 и отдельные Results/ContextText |
| [S187 28-NEW-GRAPHRAG-CONTEXT-BUDGET-EARLY-BREAK-AND-EVIDENCE-OMISSION-2026-10-16.md:L35–43](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/28-NEW-GRAPHRAG-CONTEXT-BUDGET-EARLY-BREAK-AND-EVIDENCE-OMISSION-2026-10-16.md:35>) | Предложенный break→continue, frozen ranking и eventual law output |

<a id="c042"></a>
### C042. GraphRAG token budget вычислен как 3000×4characters без tokenizer; языковой состав может менять actual token count.

**Сравнение:** [P69.7](cards/P69-graphrag-context.md#s-7).

**Допущения и предел:** Не предполагаем направление языкового эффекта;3000×4chars не ограничение 512BPE embedding.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S187 28-NEW-GRAPHRAG-CONTEXT-BUDGET-EARLY-BREAK-AND-EVIDENCE-OMISSION-2026-10-16.md:L14–16](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/28-NEW-GRAPHRAG-CONTEXT-BUDGET-EARLY-BREAK-AND-EVIDENCE-OMISSION-2026-10-16.md:14>) | TokenCount=Length/4 и char budget |
| [S187 28-NEW-GRAPHRAG-CONTEXT-BUDGET-EARLY-BREAK-AND-EVIDENCE-OMISSION-2026-10-16.md:L29–33](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/28-NEW-GRAPHRAG-CONTEXT-BUDGET-EARLY-BREAK-AND-EVIDENCE-OMISSION-2026-10-16.md:29>) | Отдельный риск actual BPE capacity |

<a id="c043"></a>
### C043. AppendLine separators не входят в used, поэтому фактическая длина ContextText может превысить character budget.

**Сравнение:** [P69.8](cards/P69-graphrag-context.md#s-8).

**Допущения и предел:** Это отдельный фактор от token/character approximation и break/continue; CRLF/LF фиксируется.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S187 28-NEW-GRAPHRAG-CONTEXT-BUDGET-EARLY-BREAK-AND-EVIDENCE-OMISSION-2026-10-16.md:L16–16](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/28-NEW-GRAPHRAG-CONTEXT-BUDGET-EARLY-BREAK-AND-EVIDENCE-OMISSION-2026-10-16.md:16>) | used не учитывает separators |
| [S187 28-NEW-GRAPHRAG-CONTEXT-BUDGET-EARLY-BREAK-AND-EVIDENCE-OMISSION-2026-10-16.md:L33–33](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/28-NEW-GRAPHRAG-CONTEXT-BUDGET-EARLY-BREAK-AND-EVIDENCE-OMISSION-2026-10-16.md:33>) | Уточнение budget discrepancy |

<a id="c044"></a>
### C044. E17 сравнивает PPL разных event alphabets и отдельных token sequences;190.1389→5.5964 не является общей шкалой semantic next-law quality.

**Сравнение:** [P71.1](cards/P71-perplexity-alphabet.md#s-1).

**Допущения и предел:** Исторический положительный числовой signal сохранён; CONST контрпример не утверждение, что E17 реально заменял всё CONST.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S312 37-NEW-E17-INCOMMENSURATE-PERPLEXITY-AND-STRUCTURAL-PREDICATES-2026-10-16.md:L15–24](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-E17-INCOMMENSURATE-PERPLEXITY-AND-STRUCTURAL-PREDICATES-2026-10-16.md:15>) | Разные vocab/lengths и математическая граница PPL |
| [S312 37-NEW-E17-INCOMMENSURATE-PERPLEXITY-AND-STRUCTURAL-PREDICATES-2026-10-16.md:L28–40](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-E17-INCOMMENSURATE-PERPLEXITY-AND-STRUCTURAL-PREDICATES-2026-10-16.md:28>) | Предложенный общий next-law экзамен |

<a id="c045"></a>
### C045. E17 Level3 может молча перейти в Level2, а независимый token-count split каждого уровня создаёт разные source panels.

**Сравнение:** [P71.2](cards/P71-perplexity-alphabet.md#s-2), [P71.3](cards/P71-perplexity-alphabet.md#s-3).

**Допущения и предел:** Две независимые причины: атрибуция fallback и split-role. Новая representation не меняется одновременно с evaluator policy.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S312 37-NEW-E17-INCOMMENSURATE-PERPLEXITY-AND-STRUCTURAL-PREDICATES-2026-10-16.md:L16–18](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-E17-INCOMMENSURATE-PERPLEXITY-AND-STRUCTURAL-PREDICATES-2026-10-16.md:16>) | Конкатенация files, fallback и разные 80/20 |
| [S312 37-NEW-E17-INCOMMENSURATE-PERPLEXITY-AND-STRUCTURAL-PREDICATES-2026-10-16.md:L30–38](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-E17-INCOMMENSURATE-PERPLEXITY-AND-STRUCTURAL-PREDICATES-2026-10-16.md:30>) | Нужны representation_version и общий source-family split |

<a id="c046"></a>
### C046. E28 genericPairs построены по allFiles, включая test; enhanced OOV fallback использует эту же загрязнённую generic memory.

**Сравнение:** [P72.2](cards/P72-project-conditioning.md#s-2).

**Допущения и предел:** Историческое FAIL−5.74% не опровержение project specialization. Удаление leak не обязано повысить числовую accuracy.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S314 37-NEW-E28-PROJECT-ADAPTOR-TEST-LEAKAGE-AND-CROSSFILE-2026-10-16.md:L12–16](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-E28-PROJECT-ADAPTOR-TEST-LEAKAGE-AND-CROSSFILE-2026-10-16.md:12>) | Два пути утечки и archived values |
| [S314 37-NEW-E28-PROJECT-ADAPTOR-TEST-LEAKAGE-AND-CROSSFILE-2026-10-16.md:L19–21](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-E28-PROJECT-ADAPTOR-TEST-LEAKAGE-AND-CROSSFILE-2026-10-16.md:19>) | Предоставленный toy TRAIN1→2/TEST1→3, не native |

<a id="c047"></a>
### C047. E28 после concatenation создаёт last(file_i)→first(file_i+1), не существующий внутри исходных файлов.

**Сравнение:** [P72.3](cards/P72-project-conditioning.md#s-3).

**Допущения и предел:** Реальные cross-file dependencies требуют отдельного typed contract; смежность списка его не заменяет. E43 имеет отдельного ownerP116.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S314 37-NEW-E28-PROJECT-ADAPTOR-TEST-LEAKAGE-AND-CROSSFILE-2026-10-16.md:L14–14](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-E28-PROJECT-ADAPTOR-TEST-LEAKAGE-AND-CROSSFILE-2026-10-16.md:14>) | ExtractPredPairs Zip |
| [S314 37-NEW-E28-PROJECT-ADAPTOR-TEST-LEAKAGE-AND-CROSSFILE-2026-10-16.md:L21–21](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-E28-PROJECT-ADAPTOR-TEST-LEAKAGE-AND-CROSSFILE-2026-10-16.md:21>) | Artificial12→21 в модельном примере |

<a id="c048"></a>
### C048. E28 projectA означает первые 60% списка, а не проверенную принадлежность проекту; project gating и train-only global fallback нужно проверять раздельно.

**Сравнение:** [P72.1](cards/P72-project-conditioning.md#s-1), [P72.4](cards/P72-project-conditioning.md#s-4), [P72.5](cards/P72-project-conditioning.md#s-5).

**Допущения и предел:** Нужен actual project manifest и общий train-only corpus; не выдавать три разных interventions за один factor.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S314 37-NEW-E28-PROJECT-ADAPTOR-TEST-LEAKAGE-AND-CROSSFILE-2026-10-16.md:L15–15](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-E28-PROJECT-ADAPTOR-TEST-LEAKAGE-AND-CROSSFILE-2026-10-16.md:15>) | Prefix label не project identity |
| [S314 37-NEW-E28-PROJECT-ADAPTOR-TEST-LEAKAGE-AND-CROSSFILE-2026-10-16.md:L25–39](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-E28-PROJECT-ADAPTOR-TEST-LEAKAGE-AND-CROSSFILE-2026-10-16.md:25>) | Отдельные proposedglobal/project/fallback arms |

<a id="c049"></a>
### C049. E31v2 выбрасывает Domain при TrainStep: сравнивает order-only global learners, не f(x, context).

**Сравнение:** [P73.1](cards/P73-domain-conditioning.md#s-1), [P73.2](cards/P73-domain-conditioning.md#s-2).

**Допущения и предел:** 0.0025/0.0025 не опровергает conditional predictor. Direction/project context не автоматически новая тема обучения.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S315 37-NEW-E31-DOMAIN-ROUTING-CLAIM-VS-UNCONDITIONED-LEARNER-2026-10-16.md:L11–20](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-E31-DOMAIN-ROUTING-CLAIM-VS-UNCONDITIONED-LEARNER-2026-10-16.md:11>) | TrainStepdiscard и историческое order-only сравнение |
| [S315 37-NEW-E31-DOMAIN-ROUTING-CLAIM-VS-UNCONDITIONED-LEARNER-2026-10-16.md:L28–36](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-E31-DOMAIN-ROUTING-CLAIM-VS-UNCONDITIONED-LEARNER-2026-10-16.md:28>) | Typed-codec условие и distinct proposed arms |

<a id="c050"></a>
### C050. E31v2 AutoLabel по basename может ошибочно определять контекст, влияющий на curriculum order.

**Сравнение:** [P73.3](cards/P73-domain-conditioning.md#s-3).

**Допущения и предел:** Передача context во вход predictor — другой фактор; B context metadata не скрытый labely/evaluator verdict.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S315 37-NEW-E31-DOMAIN-ROUTING-CLAIM-VS-UNCONDITIONED-LEARNER-2026-10-16.md:L14–14](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-E31-DOMAIN-ROUTING-CLAIM-VS-UNCONDITIONED-LEARNER-2026-10-16.md:14>) | Actual basename label producer |
| [S315 37-NEW-E31-DOMAIN-ROUTING-CLAIM-VS-UNCONDITIONED-LEARNER-2026-10-16.md:L24–26](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-E31-DOMAIN-ROUTING-CLAIM-VS-UNCONDITIONED-LEARNER-2026-10-16.md:24>) | Отдельная label hypothesis |

<a id="c051"></a>
### C051. E42 knowledge_retention считает покрытие 12predicate types, а DTO не содержит source→correct-target; правильные bindings могут исчезнуть при неизменных markers.

**Сравнение:** [P74.1](cards/P74-memory-eviction.md#s-1).

**Допущения и предел:** Source не доказывает actual lost learned law; настоящий owner связи с consumer отсутствует в DTO.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S316 37-NEW-E42-KNOWLEDGE-RETENTION-METRIC-WITHOUT-RETRIEVABLE-LAWS-2026-10-16.md:L13–22](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-E42-KNOWLEDGE-RETENTION-METRIC-WITHOUT-RETRIEVABLE-LAWS-2026-10-16.md:13>) | ComputeCoverage, DTO и wrong-target analytical control |

<a id="c052"></a>
### C052. E42 value eviction при marker retention100% сохранил 3/5context labels против 5/5FIFO; trust синтетически зависит от label.

**Сравнение:** [P74.2](cards/P74-memory-eviction.md#s-2), [P74.3](cards/P74-memory-eviction.md#s-3).

**Допущения и предел:** Это одновременно положительный coverage ceiling и потеря разнообразия исторического toy; utility/quality по темам не измерена. Trust permutation отдельна от eviction.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S316 37-NEW-E42-KNOWLEDGE-RETENTION-METRIC-WITHOUT-RETRIEVABLE-LAWS-2026-10-16.md:L15–18](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-E42-KNOWLEDGE-RETENTION-METRIC-WITHOUT-RETRIEVABLE-LAWS-2026-10-16.md:15>) | Value policy, synthetic trust и сохранённый результат |
| [S316 37-NEW-E42-KNOWLEDGE-RETENTION-METRIC-WITHOUT-RETRIEVABLE-LAWS-2026-10-16.md:L24–39](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-E42-KNOWLEDGE-RETENTION-METRIC-WITHOUT-RETRIEVABLE-LAWS-2026-10-16.md:24>) | Конкурирующие lawful-retention hypotheses и actual-owner prerequisite |

<a id="c053"></a>
### C053. Float probabilities, суммируемые в double, могут иметь массу<draw; residual fallback возвращает last vocab ID даже при probability0.

**Сравнение:** [P75.1](cards/P75-sampler-residual.md#s-1).

**Допущения и предел:** Условный native predicate; controlled r не production frequency, greedy неприменим.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S317 37-NEW-FLOAT32-SAMPLER-MASK-ESCAPE-2026-10-16.md:L13–19](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-FLOAT32-SAMPLER-MASK-ESCAPE-2026-10-16.md:13>) | Точная ветка<= и unconditional lastId |
| [S317 37-NEW-FLOAT32-SAMPLER-MASK-ESCAPE-2026-10-16.md:L23–31](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-FLOAT32-SAMPLER-MASK-ESCAPE-2026-10-16.md:23>) | Float32 proxy и историческая/greedy граница |
| [S306 37-MICRO-SAMPLER-FLOAT32-TAIL-PROXY.mjs:L5–30](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-MICRO-SAMPLER-FLOAT32-TAIL-PROXY.mjs:5>) | Предоставленная arithmetic replica, не native Softmax |
| [S305 37-MICRO-SAMPLER-FLOAT32-TAIL-PROXY-RESULT.json:L2–9](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-MICRO-SAMPLER-FLOAT32-TAIL-PROXY-RESULT.json:2>) | Масса и tailChance, только supplied receipt |

<a id="c054"></a>
### C054. Residual interval width из proxy не равен эмпирической частоте: draw должен быть представим конкретным runtime RNG.

**Сравнение:** [P75.2](cards/P75-sampler-residual.md#s-2).

**Допущения и предел:** Дополнительная инженерная оговорка проекта, совместимая с исходником; supplied proxy подставляет decimal r напрямую.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S317 37-NEW-FLOAT32-SAMPLER-MASK-ESCAPE-2026-10-16.md:L19–25](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-FLOAT32-SAMPLER-MASK-ESCAPE-2026-10-16.md:19>) | Source явно не заявляет частоту |
| [S306 37-MICRO-SAMPLER-FLOAT32-TAIL-PROXY.mjs:L11–11](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-MICRO-SAMPLER-FLOAT32-TAIL-PROXY.mjs:11>) | Controlled r=.99999999 |

<a id="c055"></a>
### C055. При опубликованном условии r<=acc ведущий probability0 и r0 создают отдельную возможную support violation.

**Сравнение:** [P75.3](cards/P75-sampler-residual.md#s-3).

**Допущения и предел:** Выведенный нами boundary из исходного predicate, не наблюдение авторского proxy и не объявленная production частота.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S317 37-NEW-FLOAT32-SAMPLER-MASK-ESCAPE-2026-10-16.md:L16–16](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-NEW-FLOAT32-SAMPLER-MASK-ESCAPE-2026-10-16.md:16>) | Опубликованное сравнение<= позволяет сформулировать boundary |
| [S306 37-MICRO-SAMPLER-FLOAT32-TAIL-PROXY.mjs:L13–17](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/37-MICRO-SAMPLER-FLOAT32-TAIL-PROXY.mjs:13>) | Та же формула цикла; конкретный r0 этим script не проверен |

<a id="c056"></a>
### C056. Byte BPE generation может остановиться внутри UTF8 scalar, replacement Decode выдаёт� без изменения neural knowledge.

**Сравнение:** [P76.1](cards/P76-utf8-boundary.md#s-1), [P76.2](cards/P76-utf8-boundary.md#s-2), [P76.3](cards/P76-utf8-boundary.md#s-3).

**Допущения и предел:** Partial tail, interior invalid и literalU+FFFD разные случаи. Completion требует настоящих continuation tokens; trimming теряет символ и не обязательно улучшает смысл.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S254 33-NEW-UTF8-DECODER-CUTOFF-2026-10-16.md:L15–18](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/33-NEW-UTF8-DECODER-CUTOFF-2026-10-16.md:15>) | Byte vocabulary→stop→Decode |
| [S254 33-NEW-UTF8-DECODER-CUTOFF-2026-10-16.md:L22–39](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/33-NEW-UTF8-DECODER-CUTOFF-2026-10-16.md:22>) | Supplied byte examples и conditional continuation design |
| [S246 33-MICRO-UTF8-PROXY-RESULT-2026-10-16.json:L1–70](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/33-MICRO-UTF8-PROXY-RESULT-2026-10-16.json:1>) | Четыре provided byte-level примера, native/inferencefalse |

<a id="c057"></a>
### C057. StratifiedSplit max(1, floor(n*.8)) отдаёт singleton целиком TRAIN, поэтому overallval>=3 не доказывает support каждого класса.

**Сравнение:** [P77.1](cards/P77-rare-class-support.md#s-1), [P77.2](cards/P77-rare-class-support.md#s-2).

**Допущения и предел:** Дублировать singleton в TEST нельзя; новый independent rare exam меняет панель, а не model. Исторический E40 LOW действительно отсутствовал — не утверждается.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S283 35-NEW-RARE-CLASS-VALIDATION-BLINDNESS-IN-STRATIFIED-SPLIT-2026-10-16.md:L7–15](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/35-NEW-RARE-CLASS-VALIDATION-BLINDNESS-IN-STRATIFIED-SPLIT-2026-10-16.md:7>) | Split и E40 общий val gate |
| [S283 35-NEW-RARE-CLASS-VALIDATION-BLINDNESS-IN-STRATIFIED-SPLIT-2026-10-16.md:L22–36](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/35-NEW-RARE-CLASS-VALIDATION-BLINDNESS-IN-STRATIFIED-SPLIT-2026-10-16.md:22>) | Counts1/2/3/5 и дополнительная independent panel |

<a id="c058"></a>
### C058. E44 secVal<=1 подставляет general accuracy в security metric, превращая отсутствие измерения в чужой score.

**Сравнение:** [P77.3](cards/P77-rare-class-support.md#s-3).

**Допущения и предел:** Это report substitution, отдельно от E44/E45target-shuffle; BUNMEASURED не neural improvement.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S283 35-NEW-RARE-CLASS-VALIDATION-BLINDNESS-IN-STRATIFIED-SPLIT-2026-10-16.md:L16–18](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/35-NEW-RARE-CLASS-VALIDATION-BLINDNESS-IN-STRATIFIED-SPLIT-2026-10-16.md:16>) | Прямой source predicate и отличие от target confound |
| [S283 35-NEW-RARE-CLASS-VALIDATION-BLINDNESS-IN-STRATIFIED-SPLIT-2026-10-16.md:L38–40](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/35-NEW-RARE-CLASS-VALIDATION-BLINDNESS-IN-STRATIFIED-SPLIT-2026-10-16.md:38>) | Security diagnostic без смены scoreroutputs |

<a id="c059"></a>
### C059. Редкий закон и дополнительный TRAIN lesson можно выбрать по TRAIN/DEV и metadata покрытия до вскрытия SEALED и затем отдельно проверить пользу нового материала.

**Сравнение:** [P77.4](cards/P77-rare-class-support.md#s-4).

**Допущения и предел:** Если новый lesson выбран после просмотра exam outputs, этот exam становится адаптивно использованным: результат exploratory, для нового вывода нужна новая закрытая source-family панель. Источник предлагает training arm, но не предъявляет его положительный результат.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S283 35-NEW-RARE-CLASS-VALIDATION-BLINDNESS-IN-STRATIFIED-SPLIT-2026-10-16.md:L34–40](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/35-NEW-RARE-CLASS-VALIDATION-BLINDNESS-IN-STRATIFIED-SPLIT-2026-10-16.md:34>) | Separate training arm после measurement-only stages |

<a id="c060"></a>
### C060. Generated-capability defaultwindow1 при loss plateau может остановить StepVector на 3 из max5; monitor не проверяет ordinary answer quality.

**Сравнение:** [P78.1](cards/P78-loss-plateau.md#s-1).

**Допущения и предел:** Это другой caller, не WikiTrace8epochs/RP01. Window5 при max5 изолирует schedulecheck; последующий gain стоит дополнительных updates.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S280 35-NEW-GENERATED-CAPABILITY-DPO-LOSS-PLATEAU-STOP-2026-10-16.md:L13–19](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/35-NEW-GENERATED-CAPABILITY-DPO-LOSS-PLATEAU-STOP-2026-10-16.md:13>) | Параметры, monitor и формульная trajectory |
| [S274 35-MICRO-DPO-CONVERGENCE-VS-DOMAIN-QUALITY-2026-10-16.md:L11–24](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/35-MICRO-DPO-CONVERGENCE-VS-DOMAIN-QUALITY-2026-10-16.md:11>) | Существующая metadata A1/B5 при same max5 |
| [S274 35-MICRO-DPO-CONVERGENCE-VS-DOMAIN-QUALITY-2026-10-16.md:L30–30](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/35-MICRO-DPO-CONVERGENCE-VS-DOMAIN-QUALITY-2026-10-16.md:30>) | Prefix checkpoint counterfactual отдельно от scheduler A/B |

<a id="c061"></a>
### C061. E05 grammarSources включает holdout, поэтому mask знает наблюдаемые проверочные тройки при train-onlytrigram.

**Сравнение:** [P79.1](cards/P79-grammar-leakage.md#s-1), [P79.2](cards/P79-grammar-leakage.md#s-2).

**Допущения и предел:** Исторический+.29relative не causal train-onlymaskgain; tokenizer vocabulary видимость фиксируется отдельно.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S214 30-NEW-E05-E07-STRUCTURAL-EXPERIMENT-VALIDITY-2026-10-16.md:L14–24](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/30-NEW-E05-E07-STRUCTURAL-EXPERIMENT-VALIDITY-2026-10-16.md:14>) | Mask table leakage и одинаковая panel |
| [S203 30-historical-seams-microproxy.py:L11–28](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/30-historical-seams-microproxy.py:11>) | Toy lookup heldout-only6:7→8, не Roslyn |

<a id="c062"></a>
### C062. E07 использует BPE token index как char-position map index; multi-character merges смещают AstMeta.

**Сравнение:** [P26.1](cards/P26-structural-features.md#s-1).

**Допущения и предел:** P26.1 сопоставляет tokenizer span и Roslyn offset. Источник не подтверждает полезность исправления native качеством.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S214 30-NEW-E05-E07-STRUCTURAL-EXPERIMENT-VALIDITY-2026-10-16.md:L28–33](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/30-NEW-E05-E07-STRUCTURAL-EXPERIMENT-VALIDITY-2026-10-16.md:28>) | Два пространства индексов и предложенный byte→UTF16Span |

<a id="c063"></a>
### C063. E07 tier2–5 меняет note, но train/predict key остаётся(bpe, ast, depth), без parent/file/scope.

**Сравнение:** [P26.2](cards/P26-structural-features.md#s-2), [P26.3](cards/P26-structural-features.md#s-3), [P26.5](cards/P26-structural-features.md#s-5).

**Допущения и предел:** Старые FAIL не являются отрицательными результатами неиспользованных features. FileId=0 дополнительно ограничивает смысл file-channel; один source file не позволяет проверить межфайловое различие.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S214 30-NEW-E05-E07-STRUCTURAL-EXPERIMENT-VALIDITY-2026-10-16.md:L35–39](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/30-NEW-E05-E07-STRUCTURAL-EXPERIMENT-VALIDITY-2026-10-16.md:35>) | Actual key и no-op tier |

<a id="c064"></a>
### C064. E20 делит survived markers на 6slots каждого примера;5/18 возможно при сохранении всех 5 присутствовавших markers.

**Сравнение:** [P80.1](cards/P80-marker-denominator.md#s-1), [P80.2](cards/P80-marker-denominator.md#s-2).

**Допущения и предел:** Это density, не 72%потеря; native normalizer в supplied proxy не исполняется.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S215 30-NEW-E20-E21-ROUTER-AND-THRESHOLD-VALIDITY-2026-10-16.md:L13–24](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/30-NEW-E20-E21-ROUTER-AND-THRESHOLD-VALIDITY-2026-10-16.md:13>) | Формула, archive и true-retentionproposal |
| [S203 30-historical-seams-microproxy.py:L46–59](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/30-historical-seams-microproxy.py:46>) | Raw/norm sets нарочноодинаковыдляизоляциизнаменателя |

<a id="c065"></a>
### C065. E21 TuneThreshold не передаёт t в ProductionSplitCount; invariant score выбирает первый t.20, а archive пишет.30.

**Сравнение:** [P41.1](cards/P41-stop-and-split.md#s-1).

**Допущения и предел:** Archive/current source parityopen; хороший Decomposeroutput неотрицается, но не доказывает threshold tuning или continual learning.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S215 30-NEW-E20-E21-ROUTER-AND-THRESHOLD-VALIDITY-2026-10-16.md:L28–37](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/30-NEW-E20-E21-ROUTER-AND-THRESHOLD-VALIDITY-2026-10-16.md:28>) | Unusedparameter, versioncontradiction и separateparameterizeddesign |
| [S203 30-historical-seams-microproxy.py:L62–69](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/30-historical-seams-microproxy.py:62>) | Toy invariant scores, notrealDecomposer |

<a id="c066"></a>
### C066. Generic KnnLogicalUnit добавляет два ANN-узла для одного ExampleId, но словарь хранит последний Output; старый вектор может вернуть новый ответ.

**Сравнение:** [P18.2](cards/P18-state-identity.md#s-2).

**Допущения и предел:** Не TranslationKnnStore. Реальная частота повторных IDs и продуктовый consumer не установлены; B отвергает конфликт до мутации обеих структур.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S332 38-CODEX-NEW-CAUSAL-GAPS-GENERIC-KNN-DPO-LOSS-REPLAY-FUSION-2026-10-16.md:L9–25](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-CODEX-NEW-CAUSAL-GAPS-GENERIC-KNN-DPO-LOSS-REPLAY-FUSION-2026-10-16.md:9>) | Идентичность ANN node → dictionary output, два конфликтующих урока и контроль уникальных IDs |
| [S333 38-CODEX-SOURCE-LOGIC-PROXY-2026-10-16.json:L15–40](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-CODEX-SOURCE-LOGIC-PROXY-2026-10-16.json:15>) | Предоставленный receipt: два узла, один ключ; фактический поиск HNSW не измерен |

<a id="c067"></a>
### C067. StableHash для нормализованного UNSLAWTU достигает Int32.MinValue; Math.Abs(int) не даёт допустимый индекс.

**Сравнение:** [P18.5](cards/P18-state-identity.md#s-5).

**Допущения и предел:** Сконструированная граница, не наблюдённая частота в корпусе и не доказанная причина RP01.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S332 38-CODEX-NEW-CAUSAL-GAPS-GENERIC-KNN-DPO-LOSS-REPLAY-FUSION-2026-10-16.md:L55–59](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-CODEX-NEW-CAUSAL-GAPS-GENERIC-KNN-DPO-LOSS-REPLAY-FUSION-2026-10-16.md:55>) | Отдельное условие переполнения хеша |
| [S333 38-CODEX-SOURCE-LOGIC-PROXY-2026-10-16.json:L79–89](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-CODEX-SOURCE-LOGIC-PROXY-2026-10-16.json:79>) | Точный предикат, seed23, multiplier31 и ожидаемый OverflowException |

<a id="c068"></a>
### C068. Generated-capability DPO публикует loss с VectorSignal, тогда как margin/accuracy нормированы иначе; этот loss управляет абсолютным early-stop.

**Сравнение:** [P89.1](cards/P89-density-stop.md#s-1), [P89.2](cards/P89-density-stop.md#s-2).

**Допущения и предел:** В RP01 с фиксированными updates этот monitor не останавливает обучение. Коррекция измерения отделена от изменения градиента.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S332 38-CODEX-NEW-CAUSAL-GAPS-GENERIC-KNN-DPO-LOSS-REPLAY-FUSION-2026-10-16.md:L27–41](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-CODEX-NEW-CAUSAL-GAPS-GENERIC-KNN-DPO-LOSS-REPLAY-FUSION-2026-10-16.md:27>) | Связь density → reported loss → monitor, frozen loss ln2 против 2ln2 |
| [S333 38-CODEX-SOURCE-LOGIC-PROXY-2026-10-16.json:L42–65](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-CODEX-SOURCE-LOGIC-PROXY-2026-10-16.json:42>) | Численная реконструкция и явное отсутствие native stopping measurements |

<a id="c069"></a>
### C069. ReplayFusionRanker присваивает pattern=vector: фактически 0.65 token-set overlap +0.35 BM25; при count≤topK возвращается исходный порядок.

**Сравнение:** [P24.1](cards/P24-replay-selection.md#s-1), [P24.2](cards/P24-replay-selection.md#s-2), [P24.3](cards/P24-replay-selection.md#s-3), [P24.4](cards/P24-replay-selection.md#s-4).

**Допущения и предел:** AST/ANN-компоненты не доказаны. Реальный caller после attestation использует topK3; private pre-cut10 без caller не считается продуктовым эффектом.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S332 38-CODEX-NEW-CAUSAL-GAPS-GENERIC-KNN-DPO-LOSS-REPLAY-FUSION-2026-10-16.md:L43–53](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-CODEX-NEW-CAUSAL-GAPS-GENERIC-KNN-DPO-LOSS-REPLAY-FUSION-2026-10-16.md:43>) | Формула, точка вызова, отрицание/порядок и граница shortlist |
| [S333 38-CODEX-SOURCE-LOGIC-PROXY-2026-10-16.json:L67–78](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-CODEX-SOURCE-LOGIC-PROXY-2026-10-16.json:67>) | Receipt подтверждает только реконструированную формулу и bypass |

<a id="c070"></a>
### C070. Высокий chosen-vs-rejected margin при одном prompt не доказывает чувствительность к входу; требуются противоположные ответы для минимально различающихся prompts.

**Сравнение:** [P81.1](cards/P81-input-conditioning.md#s-1), [P81.2](cards/P81-input-conditioning.md#s-2), [P81.3](cards/P81-input-conditioning.md#s-3).

**Допущения и предел:** Контрфактические d и C отделяют input conditioning от response-only framing prior. Ни supplied simulation, ни B0 receipt не доказывают native обучение.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S355 38-NEW-RP01-INPUT-INSENSITIVITY-COUNTERFACTUAL-DPO-2026-10-16.md:L9–36](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-NEW-RP01-INPUT-INSENSITIVITY-COUNTERFACTUAL-DPO-2026-10-16.md:9>) | Двойной знак margin, контрфактические пары, frozen B0 и обычный generation |
| [S340 38-MICRO-COUNTERFACTUAL-CONDITIONING-EXPERIMENT-2026-10-16.md:L15–56](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-MICRO-COUNTERFACTUAL-CONDITIONING-EXPERIMENT-2026-10-16.md:15>) | Будущая схема пар и измерений, без выполненной причинной проверки |

<a id="c071"></a>
### C071. E09 делит общий список AST-узлов по префиксу; один пограничный файл может оказаться одновременно в TRAIN и TEST.

**Сравнение:** [P82.1](cards/P82-file-split.md#s-1).

**Допущения и предел:** Это не означает перекрытия всех файлов. Требуется тот же исторический список и фиксированная внешняя панель для сравнения политик.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S349 38-NEW-E09-WITHIN-FILE-HOLDOUT-LEAKAGE-2026-10-16.md:L13–31](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-NEW-E09-WITHIN-FILE-HOLDOUT-LEAKAGE-2026-10-16.md:13>) | Точное source-условие, граница одного файла и различие file split/node split |

<a id="c072"></a>
### C072. E13 читает method.Body, поэтому expression-bodied методы могут схлопнуться в пустой KindBag и label Simple.

**Сравнение:** [P83.1](cards/P83-expression-body.md#s-1).

**Допущения и предел:** Добавление ExpressionBody не гарантирует улучшения nearest; block-bodied и abstract/empty методы служат разными контролями.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S350 38-NEW-E13-EXPRESSION-BODY-COLLAPSE-2026-10-16.md:L13–34](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-NEW-E13-EXPRESSION-BODY-COLLAPSE-2026-10-16.md:13>) | Пропущенный синтаксический путь и конкретный B DescendantNodesAndSelf |
| [S335 38-E13-SOURCE-LOGIC-PROXY.mjs:L1–20](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-E13-SOURCE-LOGIC-PROXY.mjs:1>) | Текст модели извлечения, не запуск Roslyn |
| [S334 38-E13-SOURCE-LOGIC-PROXY-RESULT-2026-10-16.json:L1–13](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-E13-SOURCE-LOGIC-PROXY-RESULT-2026-10-16.json:1>) | Предоставленные expression-body результаты, без native подтверждения |

<a id="c073"></a>
### C073. E13 сокращает реальные SimpleLambdaExpression и ParenthesizedLambdaExpression в разные категории; объединение Lambda может убрать искусственную дистанцию, но потерять arity.

**Сравнение:** [P84.1](cards/P84-lambda-family.md#s-1), [P84.2](cards/P84-lambda-family.md#s-2).

**Допущения и предел:** Canonicalization и добавление arity — отдельные вмешательства. Roslyn kinds нельзя автоматически переносить на JS/SQL.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S351 38-NEW-E13-LAMBDA-FAMILY-AND-COMPOSITE-LABEL-2026-10-16.md:L8–16](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-NEW-E13-LAMBDA-FAMILY-AND-COMPOSITE-LABEL-2026-10-16.md:8>) | Точное несовпадение имён и контроль arity |
| [S335 38-E13-SOURCE-LOGIC-PROXY.mjs:L21–35](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-E13-SOURCE-LOGIC-PROXY.mjs:21>) | Алгебра Jaccard над заданными категориями |
| [S334 38-E13-SOURCE-LOGIC-PROXY-RESULT-2026-10-16.json:L14–21](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-E13-SOURCE-LOGIC-PROXY-RESULT-2026-10-16.json:14>) | Предоставленный proxy lambda-distance, не обученное качество |

<a id="c074"></a>
### C074. Приоритетный E13 single-label скрывает Await за Guard или ErrorHandling; это ограничение только для consumer, требующего составные обязанности.

**Сравнение:** [P85.1](cards/P85-composite-obligations.md#s-1).

**Допущения и предел:** Старый single-label API может быть полностью корректен. B сохраняет legacy label и отдельно передаёт набор obligations, без объявления старой метрики ошибочной.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S351 38-NEW-E13-LAMBDA-FAMILY-AND-COMPOSITE-LABEL-2026-10-16.md:L18–33](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-NEW-E13-LAMBDA-FAMILY-AND-COMPOSITE-LABEL-2026-10-16.md:18>) | Три KindBag и потеря вторичной категории |
| [S335 38-E13-SOURCE-LOGIC-PROXY.mjs:L36–50](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-E13-SOURCE-LOGIC-PROXY.mjs:36>) | Программный текст приоритетного switch |
| [S334 38-E13-SOURCE-LOGIC-PROXY-RESULT-2026-10-16.json:L22–28](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-E13-SOURCE-LOGIC-PROXY-RESULT-2026-10-16.json:22>) | Предоставленные composite-label значения |

<a id="c075"></a>
### C075. Legacy fractional mask использует weighted mean в forward, но одинаковый коэффициент для всех положительных mask в backward.

**Сравнение:** [P86.1](cards/P86-fractional-response-mask.md#s-1), [P86.2](cards/P86-fractional-response-mask.md#s-2).

**Допущения и предел:** Binary masks — отрицательный контроль; текущий exact-v2 RP01 нельзя объявлять затронутым. Численная согласованность формулы отдельно от пользы native обучения.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S352 38-NEW-LEGACY-FRACTIONAL-RESPONSE-MASK-GRADIENT-MISMATCH-2026-10-16.md:L9–38](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-NEW-LEGACY-FRACTIONAL-RESPONSE-MASK-GRADIENT-MISMATCH-2026-10-16.md:9>) | Формулы и fractional/binary различие |
| [S341 38-MICRO-DPO-MASK-FINITE-DIFFERENCE-PROXY.mjs:L8–41](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-MICRO-DPO-MASK-FINITE-DIFFERENCE-PROXY.mjs:8>) | Исходный текст finite-difference proxy, не исполнен здесь |
| [S342 38-MICRO-DPO-MASK-PROXY-RESULT.json:L2–60](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-MICRO-DPO-MASK-PROXY-RESULT.json:2>) | Receipt различает несовпадающий fractional gradient и совпадающий binary control |

<a id="c076"></a>
### C076. StepMany generic path может усреднить exact и legacy objectives; RP01 guard, если он реально вызывается, обязан отвергнуть такой batch до обновления.

**Сравнение:** [P87.1](cards/P87-mixed-objectives.md#s-1), [P87.2](cards/P87-mixed-objectives.md#s-2).

**Допущения и предел:** Guarded RP01 и допускающий смешение общий путь — разные адресаты. Homogeneous controls нужны отдельно; смешение не объявляется текущим RP01 результатом.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S353 38-NEW-MIXED-EXACT-LEGACY-DPO-OBJECTIVE-IN-SAME-STEP-2026-10-16.md:L11–30](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-NEW-MIXED-EXACT-LEGACY-DPO-OBJECTIVE-IN-SAME-STEP-2026-10-16.md:11>) | Два формата в одном шаге и роль version guard |

<a id="c077"></a>
### C077. T35 Unit может игнорировать вход и возвращать заранее собранные packets с confidence0.92; Flow adapter сам создаёт ответ из ожидаемого source.

**Сравнение:** [P88.1](cards/P88-unit-context-consumption.md#s-1), [P88.2](cards/P88-unit-context-consumption.md#s-2), [P88.3](cards/P88-unit-context-consumption.md#s-3), [P88.4](cards/P88-unit-context-consumption.md#s-4).

**Допущения и предел:** Canonical parity не доказывает conditioning. Источник доверенного source binding должен быть разрешённым runtime contract, не evaluator gold.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S356 38-NEW-T35-UNIT-INPUT-ERASURE-AND-CONFIDENCE-2026-10-16.md:L9–42](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-NEW-T35-UNIT-INPUT-ERASURE-AND-CONFIDENCE-2026-10-16.md:9>) | Unit context erasure, константная confidence и adapter oracle |
| [S348 38-MICRO-T35-COUNTERFACTUAL-SOURCE-BOUNDARY-2026-10-16.md:L9–45](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-MICRO-T35-COUNTERFACTUAL-SOURCE-BOUNDARY-2026-10-16.md:9>) | Раздельные контрфактические границы Unit, adapter и benchmark |

<a id="c078"></a>
### C078. Все 12 E47 запросов содержат буквальные tags ожидаемой модели; supplied baseline12/12 и исторический BM25 11/12 не доказывают выигрыш semantic search.

**Сравнение:** [P90.1](cards/P90-tagged-search.md#s-1), [P90.2](cards/P90-tagged-search.md#s-2).

**Допущения и предел:** Hybrid — будущий кандидат. Нужны no-tag/ambiguous-tag панели, не только исходные 12; receipt не исполнял BM25.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S358 39-NEW-E47-LEXICAL-ORACLE-CONFOUND-AND-DOMAIN-ROUTING-2026-10-16.md:L7–45](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/39-NEW-E47-LEXICAL-ORACLE-CONFOUND-AND-DOMAIN-ROUTING-2026-10-16.md:7>) | Лексический oracle, реальные normalizer/registry и предложенное hybrid сравнение |
| [S357 39-E47-SOURCE-LOGIC-BASELINE-RECEIPT-2026-10-16.json:L2–131](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/39-E47-SOURCE-LOGIC-BASELINE-RECEIPT-2026-10-16.json:2>) | 12 конкретных query/tag случаев; BM2511/12 явно скопирован из истории |

<a id="c079"></a>
### C079. E13 SplitLearnableTypes использует долю методов, а Learn получает отдельно долю method-pairs; это разные TRAIN множества.

**Сравнение:** [P91.1](cards/P91-method-pair-split.md#s-1), [P91.2](cards/P91-method-pair-split.md#s-2).

**Допущения и предел:** Target — настоящий Next.KindBag, не ошибочная метка текущего метода. Согласование TRAIN не устраняет source-family leakage автоматически.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S065 17-HISTORICAL-E13-TRAIN-VALIDATION-BOUNDARY.md:L10–35](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/17-HISTORICAL-E13-TRAIN-VALIDATION-BOUNDARY.md:10>) | Две границы, корректный next target и отдельная независимость семейства |

<a id="c080"></a>
### C080. Capability обучение строит KnnTrainingCorpus с chosen outputs, затем TransformerEmbeddingKnnLogicalUnit и GeneratedCapabilityFlowFactory; прямой RP01 Student этот маршрут не проверяет.

**Сравнение:** [P92.1](cards/P92-dpo-memory-flow-bridge.md#s-1), [P92.2](cards/P92-dpo-memory-flow-bridge.md#s-2), [P92.3](cards/P92-dpo-memory-flow-bridge.md#s-3).

**Допущения и предел:** Реальная память полезна самостоятельно; bridge on/off — системное вмешательство. Не объявлять selected chosen генеративным LoRA ответом.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [O ../03-hybrid-units-and-domain-representation.md:L58–70](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/03-hybrid-units-and-domain-representation.md:58>) | Прямая ранее прочитанная цепь owners и frozen Student contrast |
| [O ../05-small-discriminating-experiments.md:L93–96](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/05-small-discriminating-experiments.md:93>) | Связанные Э 06–09; старые численные квоты не переносятся |
| [S003 01-PRIORITIZED-DIFFERENCES.md:L13–13](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/01-PRIORITIZED-DIFFERENCES.md:13>) | DIF08:точная граница RP01 |
| [S011 05-UNITS-FLOW-GRAPH-DETERMINISM.md:L16–20](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/05-UNITS-FLOW-GRAPH-DETERMINISM.md:16>) | Typed mapping не следует из текстовой тройки |

<a id="c081"></a>
### C081. Generic confidence — clipped similarity, а low score создаёт warning вместо отказа; это не калиброванная вероятность правильности.

**Сравнение:** [P93.1](cards/P93-confidence-abstention.md#s-1), [P93.2](cards/P93-confidence-abstention.md#s-2).

**Допущения и предел:** Calibration не меняет rank/accuracy сама по себе; abstention оценивается вместе с coverage. All-abstain не означает знание.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S009 04-KNN-HYBRID-RETRIEVAL.md:L11–15](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/04-KNN-HYBRID-RETRIEVAL.md:11>) | Confidence и контекстная область закона |
| [S010 05-SEMANTIC-VECTOR-KNN.md:L10–12](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/05-SEMANTIC-VECTOR-KNN.md:10>) | Конкретный first-hit owner |

<a id="c082"></a>
### C082. Исходный Unit контракт допускает typed many→many, а generic nearest copy возвращает один TOutput; недостаёт доказанного correspondence/fan-out/fan-in consumer.

**Сравнение:** [P94.1](cards/P94-typed-many-results.md#s-1), [P94.2](cards/P94-typed-many-results.md#s-2), [P94.3](cards/P94-typed-many-results.md#s-3).

**Допущения и предел:** Не утверждается отсутствие всех других реализаций. Proposed adapter требует реальной typed schema и ordinary consumer.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S011 05-UNITS-FLOW-GRAPH-DETERMINISM.md:L4–5](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/05-UNITS-FLOW-GRAPH-DETERMINISM.md:4>) | DIF501 и граница одного bridge |
| [S013 06-UNITS-FLOWS-GRAPH.md:L3–8](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/06-UNITS-FLOWS-GRAPH.md:3>) | Исходные четыре роли |
| [S013 06-UNITS-FLOWS-GRAPH.md:L18–19](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/06-UNITS-FLOWS-GRAPH.md:18>) | OutputRefs и input/output correspondence |

<a id="c083"></a>
### C083. Статическая проводка Flow не подтверждает обучаемую коррекцию отношений при frozen Units.

**Сравнение:** [P95.1](cards/P95-learned-flow-edge.md#s-1), [P95.2](cards/P95-learned-flow-edge.md#s-2), [P95.3](cards/P95-learned-flow-edge.md#s-3).

**Допущения и предел:** Edge preference table — наш ограниченный кандидат; actual owner/update/export/import пока не назван источником.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S011 05-UNITS-FLOW-GRAPH-DETERMINISM.md:L7–8](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/05-UNITS-FLOW-GRAPH-DETERMINISM.md:7>) | DIF502:state owner, signal, restore, consumer |
| [S013 06-UNITS-FLOWS-GRAPH.md:L20–20](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/06-UNITS-FLOWS-GRAPH.md:20>) | Unit→Flow:коррекция следующего входа |

<a id="c084"></a>
### C084. Async альтернативы требуют реальных ветвей, общей стоимости и cancellation; один default route не доказывает такую оркестрацию.

**Сравнение:** [P96.1](cards/P96-async-orchestration.md#s-1), [P96.2](cards/P96-async-orchestration.md#s-2), [P96.3](cards/P96-async-orchestration.md#s-3).

**Допущения и предел:** Await-both и arbiter фиксированы для concurrency contrast. Quota и late-result — отдельные proposed policies.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S011 05-UNITS-FLOW-GRAPH-DETERMINISM.md:L10–11](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/05-UNITS-FLOW-GRAPH-DETERMINISM.md:10>) | DIF503:async/quota/cancellation |
| [S013 06-UNITS-FLOWS-GRAPH.md:L21–21](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/06-UNITS-FLOWS-GRAPH.md:21>) | Flow→orchestrator trace и cost attribution |

<a id="c085"></a>
### C085. Число рёбер или replay route не доказывают learned higher-order composition на новых motifs.

**Сравнение:** [P97.1](cards/P97-learned-graph-order.md#s-1), [P97.2](cards/P97-learned-graph-order.md#s-2), [P97.3](cards/P97-learned-graph-order.md#s-3).

**Допущения и предел:** Предлагаемый relation-state candidate условен до настоящего typed motif mapping; GraphRAG retrieval не подменяет его.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S011 05-UNITS-FLOW-GRAPH-DETERMINISM.md:L10–11](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/05-UNITS-FLOW-GRAPH-DETERMINISM.md:10>) | DynamicGraph contract и edge-count limit |
| [S013 06-UNITS-FLOWS-GRAPH.md:L22–26](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/06-UNITS-FLOWS-GRAPH.md:22>) | Новое composition против replayed IDs; существующие wrappers не отрицаются |

<a id="c086"></a>
### C086. Byte BPE629 может иметь дорогую multilingual fertility при сохранённом round-trip; увеличение vocab меняет согласованную форму модели.

**Сравнение:** [P98.1](cards/P98-tokenizer-capacity.md#s-1), [P98.2](cards/P98-tokenizer-capacity.md#s-2).

**Допущения и предел:** 64k–100k в истории — вариант, не обязательный размер. Token fertility не равна потере кириллицы или качеству перевода.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S007 03-LORA-TOKENIZATION-VECTOR-SPACE.md:L23–27](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/03-LORA-TOKENIZATION-VECTOR-SPACE.md:23>) | DIF304 и multilingual metrics |
| [S003 01-PRIORITIZED-DIFFERENCES.md:L15–15](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/01-PRIORITIZED-DIFFERENCES.md:15>) | DIF10 |

<a id="c087"></a>
### C087. Ordinary BpeTokenizer.Decode заменяет unknown ID пустыми bytes, detailed Generate проверяет допустимость ID.

**Сравнение:** [P99.1](cards/P99-unknown-token-id.md#s-1), [P99.2](cards/P99-unknown-token-id.md#s-2).

**Допущения и предел:** Реальный incompatible manifest caller не показан; controlled invalid stream не доказывает частоту drift.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S099 23-INDEPENDENT-AUDIT-SHAPE-DECODER-AND-LEARNING-2026-10-16.md:L29–31](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/23-INDEPENDENT-AUDIT-SHAPE-DECODER-AND-LEARNING-2026-10-16.md:29>) | N23-C и отрицательный контроль detailed RP01 |

<a id="c088"></a>
### C088. Threshold TopK сохраняет все ties≥K-го logit, поэтому support может превышать K.

**Сравнение:** [P100.1](cards/P100-topk-ties.md#s-1), [P100.2](cards/P100-topk-ties.md#s-2).

**Допущения и предел:** Fair-ties может быть намеренным контрактом. Strict-K добавляет token-ID bias; greedy RP01 не затронут.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S126 26-INDEPENDENT-STOP-TARGET-AND-SAMPLER-2026-10-16.md:L39–45](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/26-INDEPENDENT-STOP-TARGET-AND-SAMPLER-2026-10-16.md:39>) | DS26-03 exact support и граница качества |

<a id="c089"></a>
### C089. 500 строк при batch10 дают 50 updates, а достаточность бюджета и tiny TRAIN fit не установлены одним фактом изменённых весов.

**Сравнение:** [P101.1](cards/P101-learning-budget.md#s-1), [P101.2](cards/P101-learning-budget.md#s-2), [P101.3](cards/P101-learning-budget.md#s-3).

**Допущения и предел:** Будущая 0→100 программа отдельная, historical applied-once не продолжается. Больше compute может улучшить качество, но не доказывает algorithm efficiency.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S003 01-PRIORITIZED-DIFFERENCES.md:L9–11](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/01-PRIORITIZED-DIFFERENCES.md:9>) | Families, updates, unchanged argmax |
| [S016 08-CAUSAL-DIAGNOSTICS.md:L7–10](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/08-CAUSAL-DIAGNOSTICS.md:7>) | Tiny fit и cold-start next step как проекты |
| [S006 03-DPO-PAIR-AND-GRADIENT.md:L3–3](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/03-DPO-PAIR-AND-GRADIENT.md:3>) | Реальные historical counts и unchanged utility |

<a id="c090"></a>
### C090. Небольшое изменение logits/margin может не менять greedy output; teacher-forced prefix и собственный prefix проверяют разные способности.

**Сравнение:** [P102.1](cards/P102-teacher-forced-decoding.md#s-1), [P102.2](cards/P102-teacher-forced-decoding.md#s-2), [P81.1](cards/P81-input-conditioning.md#s-1).

**Допущения и предел:** Decode gain на frozen checkpoint не является новым обучением; token mismatch при lawful paraphrase не равно semantic failure.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S008 04-BASE-LORA-CHECKPOINT.md:L5–7](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/04-BASE-LORA-CHECKPOINT.md:5>) | Изменённые 16adapters и argmax388→388 |
| [S003 01-PRIORITIZED-DIFFERENCES.md:L11–16](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/01-PRIORITIZED-DIFFERENCES.md:11>) | DIF06/11:margin versus free utility |

<a id="c091"></a>
### C091. Stack detection после нормализации может лишиться нужных markers; R04 уже мог использовать правильный порядок.

**Сравнение:** [P103.1](cards/P103-stack-before-normalization.md#s-1), [P103.2](cards/P103-stack-before-normalization.md#s-2).

**Допущения и предел:** Сначала actual call order. Density5/18 не доказывает потерю markers; P80 измеряет другой знаменатель.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S003 01-PRIORITIZED-DIFFERENCES.md:L22–22](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/01-PRIORITIZED-DIFFERENCES.md:22>) | DIF17 |
| [S215 30-NEW-E20-E21-ROUTER-AND-THRESHOLD-VALIDITY-2026-10-16.md:L18–24](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/30-NEW-E20-E21-ROUTER-AND-THRESHOLD-VALIDITY-2026-10-16.md:18>) | Не отменять D07; будущий before/after E15 contrast |

<a id="c092"></a>
### C092. Generic DomainTag/UseDomainScopedRetrieval не фильтруют mixed pool локально; HARD/SOFT/negative/warning требуют разных правил допуска.

**Сравнение:** [P104.1](cards/P104-law-eligibility.md#s-1), [P104.2](cards/P104-law-eligibility.md#s-2), [P104.3](cards/P104-law-eligibility.md#s-3), [P104.4](cards/P104-law-eligibility.md#s-4).

**Допущения и предел:** Upstream может уже обеспечивать scope. Исходные code labels не становятся автоматически пользовательскими темами обучения; запретный доступ не отключается ради A.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S046 14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS.md:L28–32](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS.md:28>) | Local absent filter и upstream boundary |
| [S009 04-KNN-HYBRID-RETRIEVAL.md:L14–18](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/04-KNN-HYBRID-RETRIEVAL.md:14>) | Закон может быть conditional/soft/unknown |
| [S010 05-SEMANTIC-VECTOR-KNN.md:L14–16](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/05-SEMANTIC-VECTOR-KNN.md:14>) | Arbiter/negative/warning контракт |
| [S010 05-SEMANTIC-VECTOR-KNN.md:L29–29](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/05-SEMANTIC-VECTOR-KNN.md:29>) | No gold leakage into query metadata |

<a id="c093"></a>
### C093. Два импорта одного checkpoint не доказывают mutable alias; reference должен оставаться отдельным frozen состоянием при policy update.

**Сравнение:** [P105.1](cards/P105-frozen-reference.md#s-1), [P105.2](cards/P105-frozen-reference.md#s-2).

**Допущения и предел:** Исторический separate reference C50 — положительное свидетельство. Alias только диагностический кандидат, не найденный дефект.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S012 06-CHECKPOINT-RESTORE-ADOPTION.md:L2–6](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/06-CHECKPOINT-RESTORE-ADOPTION.md:2>) | Frozen C50 и reference boundary |
| [S003 01-PRIORITIZED-DIFFERENCES.md:L37–37](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/01-PRIORITIZED-DIFFERENCES.md:37>) | DIF32 import identity |

<a id="c094"></a>
### C094. BuildGeneralBatch берёт semantic target из собственного Tokenize, хотя SemanticTokenizerUnit умеет принять внешний исправленный chosen JSON.

**Сравнение:** [P106.1](cards/P106-corrected-semantic-target.md#s-1), [P106.2](cards/P106-corrected-semantic-target.md#s-2).

**Допущения и предел:** Producer self-target, wrong gold и annotation ingress изолируются. Word/phrase annotation ошибки относятся к P52/P53, если не читаются semantic producer.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [O ../13-domain-context-and-dpo-subunits.md:L37–53](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/13-domain-context-and-dpo-subunits.md:37>) | Три recipients и self-confirmation boundary |
| [O ../13-domain-context-and-dpo-subunits.md:L102–107](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/13-domain-context-and-dpo-subunits.md:102>) | Отдельный proposed corrected target contrast |

<a id="c095"></a>
### C095. Generic Unit получает k hits, но answer берёт только hits[0]; увеличение k само по себе не добавляет aggregation.

**Сравнение:** [P107.1](cards/P107-lawful-neighbor-aggregation.md#s-1), [P107.2](cards/P107-lawful-neighbor-aggregation.md#s-2), [P107.3](cards/P107-lawful-neighbor-aggregation.md#s-3), [P107.4](cards/P107-lawful-neighbor-aggregation.md#s-4).

**Допущения и предел:** Majority candidate требует lawful OutputKey; free text нельзя голосовать по evaluator verdict. Learned memory и law-only contribution раздельны.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S009 04-KNN-HYBRID-RETRIEVAL.md:L2–3](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/04-KNN-HYBRID-RETRIEVAL.md:2>) | DIF401 |
| [S009 04-KNN-HYBRID-RETRIEVAL.md:L17–18](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/04-KNN-HYBRID-RETRIEVAL.md:17>) | Факторизация retrieval/law |
| [S010 05-SEMANTIC-VECTOR-KNN.md:L12–12](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/05-SEMANTIC-VECTOR-KNN.md:12>) | Actual generic consumer |

<a id="c096"></a>
### C096. Predicate IDs в runtime List/custom IReadOnlyList/JsonElement превращаются в пустой tier, когда ingress принимает только string[].

**Сравнение:** [P108.1](cards/P108-metadata-carrier.md#s-1), [P108.2](cards/P108-metadata-carrier.md#s-2).

**Допущения и предел:** Declared interface не меняет runtime type массива. Actual nonarray caller неизвестен, shape/schema parser предложен отдельно.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S046 14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS.md:L16–26](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS.md:16>) | Точное условие carrier boundary и пять представлений |

<a id="c097"></a>
### C097. Валидный JSON/admission и происхождение не доказывают смысловую истинность chosen или meaningful rejected.

**Сравнение:** [P109.1](cards/P109-gold-validity.md#s-1), [P109.2](cards/P109-gold-validity.md#s-2), [P04.1](cards/P04-preference-contrast.md#s-1).

**Допущения и предел:** Simulation остаётся simulation без новых имён моделей/авторов. Исправленный scorer отдельно от изменённого learner.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S004 02-DPO-DATA-OBJECTIVE-GRADIENT.md:L16–20](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/02-DPO-DATA-OBJECTIVE-GRADIENT.md:16>) | DIF203 semantic VALID/TIE/BOTH_BAD/UNJUDGEABLE |
| [S003 01-PRIORITIZED-DIFFERENCES.md:L23–25](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/01-PRIORITIZED-DIFFERENCES.md:23>) | Compiler-verification/provenance/semantic-quality разные вопросы |

<a id="c098"></a>
### C098. E12 ускорил aggregate MatVec benchmark, но sparse и SIMD64 режимы замедлились; это numerical performance, не semantic KNN transfer.

**Сравнение:** [P110.1](cards/P110-numeric-matrix-cost.md#s-1), [P110.2](cards/P110-numeric-matrix-cost.md#s-2), [P110.3](cards/P110-numeric-matrix-cost.md#s-3).

**Допущения и предел:** Actual product MatVec caller и полный cost обязательны. E07 n-gram, simplified Unit matrix и E12 численная матрица различны.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S037 12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md:L13–13](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md:13>) | 480.6→275.6 aggregate и два ухудшения |
| [S037 12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md:L30–34](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md:30>) | Разные механизмы matrix/knn |
| [S038 12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md:L5–5](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md:5>) | Три значения матрицы |
| [S038 12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md:L28–28](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md:28>) | Aggregate PASS rule |

<a id="c099"></a>
### C099. GraphDpoPair.Level/UnitId не устанавливают mapping feedback к выбранным параметрам Unit/Flow; default route может скрыть неправильный credit.

**Сравнение:** [P111.1](cards/P111-reward-credit.md#s-1), [P111.2](cards/P111-reward-credit.md#s-2), [P111.3](cards/P111-reward-credit.md#s-3).

**Допущения и предел:** Registered recipient mapping и idempotence — proposed explicit contracts; не найденный native dispatcher defect.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S013 06-UNITS-FLOWS-GRAPH.md:L12–23](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/06-UNITS-FLOWS-GRAPH.md:12>) | Точный RP01 recipient и открытые интеграционные границы |
| [S003 01-PRIORITIZED-DIFFERENCES.md:L43–43](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/01-PRIORITIZED-DIFFERENCES.md:43>) | DIF38 |

<a id="c100"></a>
### C100. 500 template-derived строк не 500 независимых законов, а раскрытые исторические экзамены не являются новым sealed TEST.

**Сравнение:** [P112.1](cards/P112-family-disjoint-evaluation.md#s-1), [P112.2](cards/P112-family-disjoint-evaluation.md#s-2), [P112.3](cards/P112-family-disjoint-evaluation.md#s-3).

**Допущения и предел:** Source/template grouping не заменяет общий конечный TEST. Если урок выбран после exam, прежний файл не untouched evidence.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S016 08-CAUSAL-DIAGNOSTICS.md:L19–22](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/08-CAUSAL-DIAGNOSTICS.md:19>) | Family independence и виды gold |
| [S003 01-PRIORITIZED-DIFFERENCES.md:L9–9](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/01-PRIORITIZED-DIFFERENCES.md:9>) | 25/20 templates |
| [S003 01-PRIORITIZED-DIFFERENCES.md:L46–46](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/01-PRIORITIZED-DIFFERENCES.md:46>) | DIF41 disclosed panels |

<a id="c101"></a>
### C101. В generic Vectorize общий L2 подавляет predicate×2 при длинном text-count tier, даже при корректных metadata и без hash collisions.

**Сравнение:** [P113.1](cards/P113-feature-energy.md#s-1), [P113.2](cards/P113-feature-energy.md#s-2).

**Допущения и предел:** Отдельная tier normalization — кандидат. Не переносить на TransformerEmbedding или TranslationKnnStore.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S062 16-NEW-CAUSAL-GAPS-KNN-TIER-DPO-ZERO-WEIGHT-KV-2026-10-16.md:L16–33](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/16-NEW-CAUSAL-GAPS-KNN-TIER-DPO-ZERO-WEIGHT-KV-2026-10-16.md:16>) | Формула energy и численный контрпример |
| [S053 16-KNN-TIER-BALANCE-CAUSAL-MICROEXPERIMENT.md:L1–70](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/16-KNN-TIER-BALANCE-CAUSAL-MICROEXPERIMENT.md:1>) | Полный план того же механизма, не новый опыт |

<a id="c102"></a>
### C102. Math.Max(.01, Weight) делает zero/negative weight положительным вкладом; RP01 pairs Weight1 не затронуты.

**Сравнение:** [P114.1](cards/P114-zero-weight-pair.md#s-1), [P114.2](cards/P114-zero-weight-pair.md#s-2), [P114.3](cards/P114-zero-weight-pair.md#s-3).

**Допущения и предел:** Weight0 может быть min-floor контрактом; quarantine не должна полагаться на нулевой вес. Real gradient/update ещё не измерен.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S062 16-NEW-CAUSAL-GAPS-KNN-TIER-DPO-ZERO-WEIGHT-KV-2026-10-16.md:L35–43](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/16-NEW-CAUSAL-GAPS-KNN-TIER-DPO-ZERO-WEIGHT-KV-2026-10-16.md:35>) | Нормировка, actual1f, трёхплечая диагностика |

<a id="c103"></a>
### C103. TransformerEmbeddingKnnLogicalUnit пересчитывает avg length и document frequency полным corpus scan для каждого candidate/query term.

**Сравнение:** [P115.1](cards/P115-bm25-statistics-cache.md#s-1), [P115.2](cards/P115-bm25-statistics-cache.md#s-2).

**Допущения и предел:** Операционный счётчик не wall time; cached stats обязаны быть согласованы с immutable corpus version и тем же ranking.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S080 20-NEW-FINDINGS-FUSION-MEMORY-GROWTH-2026-10-16.md:L21–33](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/20-NEW-FINDINGS-FUSION-MEMORY-GROWTH-2026-10-16.md:21>) | O(N²(1+Q)) и native/caller границы |

<a id="c104"></a>
### C104. E43 Zip после общей конкатенации файлов создаёт искусственные boundary bigrams; это исторический механизм, не новая RP01 регрессия.

**Сравнение:** [P116.1](cards/P116-e43-file-boundary.md#s-1), [P116.3](cards/P116-e43-file-boundary.md#s-3).

**Допущения и предел:** Positive replay.3226→.3978 не отрицается; B должен сравниваться на одном within-file family panel.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S261 34-E43-POSITIVE-REPLAY-FILE-BOUNDARY-CAUSAL-GAP-2026-10-16.md:L7–19](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/34-E43-POSITIVE-REPLAY-FILE-BOUNDARY-CAUSAL-GAP-2026-10-16.md:7>) | Source chain, old/new/replay и positive receipt |
| [S261 34-E43-POSITIVE-REPLAY-FILE-BOUNDARY-CAUSAL-GAP-2026-10-16.md:L33–46](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/34-E43-POSITIVE-REPLAY-FILE-BOUNDARY-CAUSAL-GAP-2026-10-16.md:33>) | Matched boundary/replay contrasts |

<a id="c105"></a>
### C105. E43 oldTrain/oldVal по разным позициям может содержать одинаковые pair keys; exact recall не доказывает unseen-key перенос.

**Сравнение:** [P116.2](cards/P116-e43-file-boundary.md#s-2).

**Допущения и предел:** Actual Tier2 duplicates неизвестны. Не смешивать key grouping с boundary repair; обе причины изолируются.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S261 34-E43-POSITIVE-REPLAY-FILE-BOUNDARY-CAUSAL-GAP-2026-10-16.md:L27–31](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/34-E43-POSITIVE-REPLAY-FILE-BOUNDARY-CAUSAL-GAP-2026-10-16.md:27>) | Вторая независимая гипотеза и ratio limit |

<a id="c106"></a>
### C106. Пять Translation directions могут не достигать ordinary KNN без semantic concept/PAIR, хотя память уже обучена; EN_RU имеет отдельную fallback ветку.

**Сравнение:** [P117.1](cards/P117-translation-direction-memory.md#s-1).

**Допущения и предел:** Early exact/phrase/word maps и technical concepts могут скрыть gate. Native route trace обязателен.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S218 30-NOVEL-DIRECTION-GATED-KNN-AND-SHARED-CROSS-LANGUAGE-CANDIDATES-2026-10-16.md:L23–24](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/30-NOVEL-DIRECTION-GATED-KNN-AND-SHARED-CROSS-LANGUAGE-CANDIDATES-2026-10-16.md:23>) | Gate и UNKNOWN concepts |
| [S218 30-NOVEL-DIRECTION-GATED-KNN-AND-SHARED-CROSS-LANGUAGE-CANDIDATES-2026-10-16.md:L31–39](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/30-NOVEL-DIRECTION-GATED-KNN-AND-SHARED-CROSS-LANGUAGE-CANDIDATES-2026-10-16.md:31>) | Пять направлений и ранние карты |

<a id="c107"></a>
### C107. Шесть Executor используют общий signature store без direction namespace; target filter для HE/ES default проверяет лишь отсутствие кириллицы.

**Сравнение:** [P117.2](cards/P117-translation-direction-memory.md#s-2), [P117.3](cards/P117-translation-direction-memory.md#s-3).

**Допущения и предел:** Общая память не гарантирует ошибку:signature, source law, ранний lookup и verifier могут предотвратить её. Namespace и language predicate — отдельные факторы.

| Источник / строки | Вклад в этот тезис |
|---|---|
| [S218 30-NOVEL-DIRECTION-GATED-KNN-AND-SHARED-CROSS-LANGUAGE-CANDIDATES-2026-10-16.md:L19–29](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/30-NOVEL-DIRECTION-GATED-KNN-AND-SHARED-CROSS-LANGUAGE-CANDIDATES-2026-10-16.md:19>) | Shared store, candidate acceptance и EN_ES→EN_HE контрпример |
| [S218 30-NOVEL-DIRECTION-GATED-KNN-AND-SHARED-CROSS-LANGUAGE-CANDIDATES-2026-10-16.md:L41–61](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/30-NOVEL-DIRECTION-GATED-KNN-AND-SHARED-CROSS-LANGUAGE-CANDIDATES-2026-10-16.md:41>) | Полная цепь и разрушающие условия |

Содержательно адресованы 107 тезиса с 242 вкладами 92 файлов. Это не заявление, что все утверждения каждого файла исчерпаны: master/crosswalk и ряд источников содержат другие находки, продолжающие сопоставление. Совпадение receipt/proxy с текстом не считается независимым запуском. Исторические распоряжения об авторах, защищённых каталогах или квотах опытов сохранены как содержание источника; текущую пользовательскую авторизацию они не меняют.


## Обратный индекс: подслучай → содержательные источники

Индекс построен из явных причинных связей C и завершённого обхода 06, без поиска по сходству названий. Ссылка S ведёт к точному тезису, строкам и пределу. У дополнительных контролей прежнего каталога отдельно указан источник O либо вывод из механизма; отсутствие H-связи не маскируется случайной цитатой. Прямые источники карточки дают дополнительный контекст, но не автоматически обосновывают каждый её подслучай.

| Подслучай | Источники и характер основания |
|---|---|
| <a id="p01-1"></a>[P01.1](cards/P01-trainable-recipient.md#s-1) — Область trainable parameters | [S003](06-source-content-walk.md#s003), [S007](06-source-content-walk.md#s007), [S008](06-source-content-walk.md#s008), [S016](06-source-content-walk.md#s016), [S017](06-source-content-walk.md#s017), [S020](06-source-content-walk.md#s020), [S021](06-source-content-walk.md#s021), [S029](06-source-content-walk.md#s029), [S040](06-source-content-walk.md#s040), [S103](06-source-content-walk.md#s103), [S125](06-source-content-walk.md#s125), [S173](06-source-content-walk.md#s173), [S176](06-source-content-walk.md#s176), [S272](06-source-content-walk.md#s272) |
| <a id="p01-2"></a>[P01.2](cards/P01-trainable-recipient.md#s-2) — LoRA rank | [S003](06-source-content-walk.md#s003), [S007](06-source-content-walk.md#s007), [S031](06-source-content-walk.md#s031), [S036](06-source-content-walk.md#s036), [S039](06-source-content-walk.md#s039) |
| <a id="p01-3"></a>[P01.3](cards/P01-trainable-recipient.md#s-3) — LoRA alpha | [S007](06-source-content-walk.md#s007) |
| <a id="p01-4"></a>[P01.4](cards/P01-trainable-recipient.md#s-4) — LoRA dropout | [S003](06-source-content-walk.md#s003), [S007](06-source-content-walk.md#s007) |
| <a id="p02-1"></a>[P02.1](cards/P02-warm-base.md#s-1) — Supervised warm-up при равном DPO бюджете | [S001](06-source-content-walk.md#s001), [S002](06-source-content-walk.md#s002), [S003](06-source-content-walk.md#s003), [S005](06-source-content-walk.md#s005), [S007](06-source-content-walk.md#s007), [S008](06-source-content-walk.md#s008), [S014](06-source-content-walk.md#s014), [S016](06-source-content-walk.md#s016), [S020](06-source-content-walk.md#s020), [S021](06-source-content-walk.md#s021), [S029](06-source-content-walk.md#s029), [S030](06-source-content-walk.md#s030), [S031](06-source-content-walk.md#s031), [S039](06-source-content-walk.md#s039), [S040](06-source-content-walk.md#s040), [S041](06-source-content-walk.md#s041), [S043](06-source-content-walk.md#s043), [S103](06-source-content-walk.md#s103), [S123](06-source-content-walk.md#s123), [S125](06-source-content-walk.md#s125), [S127](06-source-content-walk.md#s127), [S147](06-source-content-walk.md#s147), [S173](06-source-content-walk.md#s173), [S238](06-source-content-walk.md#s238), [S268](06-source-content-walk.md#s268), [S272](06-source-content-walk.md#s272), [S285](06-source-content-walk.md#s285), [S297](06-source-content-walk.md#s297), [S303](06-source-content-walk.md#s303), [S331](06-source-content-walk.md#s331), [S337](06-source-content-walk.md#s337), [S360](06-source-content-walk.md#s360) |
| <a id="p02-2"></a>[P02.2](cards/P02-warm-base.md#s-2) — Warm-up при равной полной вычислительной цене | [S008](06-source-content-walk.md#s008), [S012](06-source-content-walk.md#s012), [S030](06-source-content-walk.md#s030), [S041](06-source-content-walk.md#s041), [S331](06-source-content-walk.md#s331) |
| <a id="p02-3"></a>[P02.3](cards/P02-warm-base.md#s-3) — Замена первой учебной стадии и собственная frozen reference | [S030](06-source-content-walk.md#s030), [S036](06-source-content-walk.md#s036), [S040](06-source-content-walk.md#s040), [S041](06-source-content-walk.md#s041) |
| <a id="p03-1"></a>[P03.1](cards/P03-historical-score-lineage.md#s-1) — Lineage старых before/after и точная метрика | [S001](06-source-content-walk.md#s001), [S002](06-source-content-walk.md#s002), [S006](06-source-content-walk.md#s006), [S018](06-source-content-walk.md#s018), [S019](06-source-content-walk.md#s019), [S023](06-source-content-walk.md#s023), [S024](06-source-content-walk.md#s024), [S037](06-source-content-walk.md#s037), [S038](06-source-content-walk.md#s038), [S043](06-source-content-walk.md#s043), [S099](06-source-content-walk.md#s099), [S173](06-source-content-walk.md#s173), [S185](06-source-content-walk.md#s185), [S199](06-source-content-walk.md#s199), [S205](06-source-content-walk.md#s205), [S207](06-source-content-walk.md#s207), [S208](06-source-content-walk.md#s208), [S221](06-source-content-walk.md#s221), [S227](06-source-content-walk.md#s227), [S233](06-source-content-walk.md#s233), [S234](06-source-content-walk.md#s234), [S235](06-source-content-walk.md#s235), [S236](06-source-content-walk.md#s236), [S237](06-source-content-walk.md#s237), [S258](06-source-content-walk.md#s258), [S269](06-source-content-walk.md#s269), [S272](06-source-content-walk.md#s272), [S273](06-source-content-walk.md#s273), [S285](06-source-content-walk.md#s285), [S286](06-source-content-walk.md#s286), [S290](06-source-content-walk.md#s290), [S293](06-source-content-walk.md#s293), [S323](06-source-content-walk.md#s323), [S324](06-source-content-walk.md#s324), [S325](06-source-content-walk.md#s325), [S330](06-source-content-walk.md#s330), [S331](06-source-content-walk.md#s331), [S336](06-source-content-walk.md#s336), [S338](06-source-content-walk.md#s338), [S360](06-source-content-walk.md#s360) |
| <a id="p03-2"></a>[P03.2](cards/P03-historical-score-lineage.md#s-2) — Backward off/on в том же tiny trainer | [S024](06-source-content-walk.md#s024) |
| <a id="p03-3"></a>[P03.3](cards/P03-historical-score-lineage.md#s-3) — Исторический matrix→kNN port на общей панели — условная lineage-проверка | [S037](06-source-content-walk.md#s037), [S038](06-source-content-walk.md#s038), [S043](06-source-content-walk.md#s043), [S044](06-source-content-walk.md#s044), [S103](06-source-content-walk.md#s103) |
| <a id="p03-4"></a>[P03.4](cards/P03-historical-score-lineage.md#s-4) — E52: contract gates и фиксированный timing отделены от измерений | [S042](06-source-content-walk.md#s042) |
| <a id="p03-5"></a>[P03.5](cards/P03-historical-score-lineage.md#s-5) — Manifest нулевых LoRA/DPO при переданном обученном runtime | [S074](06-source-content-walk.md#s074) |
| <a id="p04-1"></a>[P04.1](cards/P04-preference-contrast.md#s-1) — Слабый или близкий смысловой rejected | [S003](06-source-content-walk.md#s003), [S004](06-source-content-walk.md#s004), [S006](06-source-content-walk.md#s006), [S014](06-source-content-walk.md#s014), [S020](06-source-content-walk.md#s020), [S199](06-source-content-walk.md#s199), [S268](06-source-content-walk.md#s268), [S286](06-source-content-walk.md#s286) |
| <a id="p04-2"></a>[P04.2](cards/P04-preference-contrast.md#s-2) — Несколько контрастов одного prompt при одинаковом бюджете | [S227](06-source-content-walk.md#s227), [S230](06-source-content-walk.md#s230), [S232](06-source-content-walk.md#s232) |
| <a id="p04-3"></a>[P04.3](cards/P04-preference-contrast.md#s-3) — Токенный общий префикс и admission повторного prompt | [S058](06-source-content-walk.md#s058), [S063](06-source-content-walk.md#s063), [S064](06-source-content-walk.md#s064), [S067](06-source-content-walk.md#s067), [S068](06-source-content-walk.md#s068), [S069](06-source-content-walk.md#s069), [S103](06-source-content-walk.md#s103), [S172](06-source-content-walk.md#s172), [S227](06-source-content-walk.md#s227), [S228](06-source-content-walk.md#s228), [S229](06-source-content-walk.md#s229), [S230](06-source-content-walk.md#s230), [S232](06-source-content-walk.md#s232) |
| <a id="p05-1"></a>[P05.1](cards/P05-response-length-objective.md#s-1) — Sum против mean response log probabilities | [S001](06-source-content-walk.md#s001), [S004](06-source-content-walk.md#s004), [S006](06-source-content-walk.md#s006), [S020](06-source-content-walk.md#s020), [S064](06-source-content-walk.md#s064) |
| <a id="p05-2"></a>[P05.2](cards/P05-response-length-objective.md#s-2) — Prefix targets при одинаковой нормировке mean | [S058](06-source-content-walk.md#s058), [S059](06-source-content-walk.md#s059), [S063](06-source-content-walk.md#s063), [S064](06-source-content-walk.md#s064) |
| <a id="p06-1"></a>[P06.1](cards/P06-teaching-vector-adamw.md#s-1) — Знаки и значения teaching vector при той же плотности | [S001](06-source-content-walk.md#s001), [S003](06-source-content-walk.md#s003), [S004](06-source-content-walk.md#s004), [S006](06-source-content-walk.md#s006), [S010](06-source-content-walk.md#s010), [S020](06-source-content-walk.md#s020), [S029](06-source-content-walk.md#s029), [S031](06-source-content-walk.md#s031), [S036](06-source-content-walk.md#s036), [S039](06-source-content-walk.md#s039), [S040](06-source-content-walk.md#s040), [S272](06-source-content-walk.md#s272), [S360](06-source-content-walk.md#s360) |
| <a id="p06-2"></a>[P06.2](cards/P06-teaching-vector-adamw.md#s-2) — Uniform gradient scale при одном AdamW state | [S123](06-source-content-walk.md#s123), [S132](06-source-content-walk.md#s132), [S138](06-source-content-walk.md#s138) |
| <a id="p06-3"></a>[P06.3](cards/P06-teaching-vector-adamw.md#s-3) — Относительный вес одной пары | [S138](06-source-content-walk.md#s138) |
| <a id="p07-1"></a>[P07.1](cards/P07-materialized-token-contrast.md#s-1) — 480-symbol Compact и хвост условия | [S001](06-source-content-walk.md#s001), [S003](06-source-content-walk.md#s003), [S006](06-source-content-walk.md#s006), [S020](06-source-content-walk.md#s020), [S039](06-source-content-walk.md#s039), [S123](06-source-content-walk.md#s123), [S125](06-source-content-walk.md#s125), [S147](06-source-content-walk.md#s147), [S172](06-source-content-walk.md#s172), [S173](06-source-content-walk.md#s173), [S176](06-source-content-walk.md#s176), [S199](06-source-content-walk.md#s199), [S272](06-source-content-walk.md#s272), [S285](06-source-content-walk.md#s285), [S294](06-source-content-walk.md#s294) |
| <a id="p07-2"></a>[P07.2](cards/P07-materialized-token-contrast.md#s-2) — Раздельное encoding и response boundary | [S003](06-source-content-walk.md#s003), [S004](06-source-content-walk.md#s004), [S006](06-source-content-walk.md#s006), [S020](06-source-content-walk.md#s020), [S039](06-source-content-walk.md#s039), [S286](06-source-content-walk.md#s286) |
| <a id="p07-3"></a>[P07.3](cards/P07-materialized-token-contrast.md#s-3) — Удаление diagnostic marker | [S058](06-source-content-walk.md#s058), [S059](06-source-content-walk.md#s059), [S063](06-source-content-walk.md#s063) |
| <a id="p07-4"></a>[P07.4](cards/P07-materialized-token-contrast.md#s-4) — Legacy hash IDs против настоящих token IDs | [S024](06-source-content-walk.md#s024), [S058](06-source-content-walk.md#s058), [S059](06-source-content-walk.md#s059), [S063](06-source-content-walk.md#s063), [S064](06-source-content-walk.md#s064), [S067](06-source-content-walk.md#s067), [S069](06-source-content-walk.md#s069), [S360](06-source-content-walk.md#s360) |
| <a id="p07-5"></a>[P07.5](cards/P07-materialized-token-contrast.md#s-5) — Serving prompt framing совпадает с обучающим prefix | [S020](06-source-content-walk.md#s020), [S021](06-source-content-walk.md#s021), [S029](06-source-content-walk.md#s029), [S039](06-source-content-walk.md#s039), [S040](06-source-content-walk.md#s040) |
| <a id="p07-6"></a>[P07.6](cards/P07-materialized-token-contrast.md#s-6) — Side-label shortcut в историческом encoder | [S024](06-source-content-walk.md#s024), [S043](06-source-content-walk.md#s043) |
| <a id="p07-7"></a>[P07.7](cards/P07-materialized-token-contrast.md#s-7) — Translation handoff: source в хвосте урезанного prefix | [S058](06-source-content-walk.md#s058), [S059](06-source-content-walk.md#s059), [S063](06-source-content-walk.md#s063) |
| <a id="p07-8"></a>[P07.8](cards/P07-materialized-token-contrast.md#s-8) — Translation response tail: различие после бюджета ответа | [S058](06-source-content-walk.md#s058), [S063](06-source-content-walk.md#s063), [S064](06-source-content-walk.md#s064), [S067](06-source-content-walk.md#s067), [S068](06-source-content-walk.md#s068), [S069](06-source-content-walk.md#s069) |
| <a id="p07-9"></a>[P07.9](cards/P07-materialized-token-contrast.md#s-9) — Один hash-collision без смены всего codec | [S067](06-source-content-walk.md#s067), [S068](06-source-content-walk.md#s068) |
| <a id="p08-1"></a>[P08.1](cards/P08-answer-boundary.md#s-1) — EOS target при неизменных остальных данных | [S001](06-source-content-walk.md#s001), [S003](06-source-content-walk.md#s003), [S004](06-source-content-walk.md#s004), [S007](06-source-content-walk.md#s007), [S008](06-source-content-walk.md#s008), [S016](06-source-content-walk.md#s016), [S021](06-source-content-walk.md#s021), [S063](06-source-content-walk.md#s063), [S126](06-source-content-walk.md#s126) |
| <a id="p08-2"></a>[P08.2](cards/P08-answer-boundary.md#s-2) — MaxAnswerChars raw/service parity | [S021](06-source-content-walk.md#s021) |
| <a id="p08-3"></a>[P08.3](cards/P08-answer-boundary.md#s-3) — TopK приоритет против TopK∩TopP | [S099](06-source-content-walk.md#s099), [S126](06-source-content-walk.md#s126), [S317](06-source-content-walk.md#s317) |
| <a id="p08-4"></a>[P08.4](cards/P08-answer-boundary.md#s-4) — Serving: обычный последний BPE merge ошибочно объявлен EOS | [S020](06-source-content-walk.md#s020), [S021](06-source-content-walk.md#s021), [S029](06-source-content-walk.md#s029), [S040](06-source-content-walk.md#s040) |
| <a id="p08-5"></a>[P08.5](cards/P08-answer-boundary.md#s-5) — Заранее заданная строковая граница при frozen generation | [S021](06-source-content-walk.md#s021), [S031](06-source-content-walk.md#s031), [S245](06-source-content-walk.md#s245), [S257](06-source-content-walk.md#s257), [S258](06-source-content-walk.md#s258) |
| <a id="p09-1"></a>[P09.1](cards/P09-cached-forward.md#s-1) — Full forward против cached prefill | [S012](06-source-content-walk.md#s012), [S062](06-source-content-walk.md#s062) |
| <a id="p09-2"></a>[P09.2](cards/P09-cached-forward.md#s-2) — Chunked cache append и absolute position | [S062](06-source-content-walk.md#s062) |
| <a id="p09-3"></a>[P09.3](cards/P09-cached-forward.md#s-3) — Ошибка append и целостность слоёв cache | Выведенный контроль/краевой подслучай механизма карточки; отдельный дефект H этим не утверждается; A/B и предел даны в самой карточке |
| <a id="p10-1"></a>[P10.1](cards/P10-composed-gradient.md#s-1) — Exact DPO chosen/rejected gradient | [S001](06-source-content-walk.md#s001), [S003](06-source-content-walk.md#s003), [S004](06-source-content-walk.md#s004), [S006](06-source-content-walk.md#s006), [S014](06-source-content-walk.md#s014), [S016](06-source-content-walk.md#s016), [S017](06-source-content-walk.md#s017), [S020](06-source-content-walk.md#s020), [S029](06-source-content-walk.md#s029), [S031](06-source-content-walk.md#s031), [S036](06-source-content-walk.md#s036), [S039](06-source-content-walk.md#s039), [S043](06-source-content-walk.md#s043), [S123](06-source-content-walk.md#s123), [S268](06-source-content-walk.md#s268), [S272](06-source-content-walk.md#s272), [S297](06-source-content-walk.md#s297), [S331](06-source-content-walk.md#s331) |
| <a id="p10-2"></a>[P10.2](cards/P10-composed-gradient.md#s-2) — RMSNorm и attention composition | Выведенный контроль/краевой подслучай механизма карточки; отдельный дефект H этим не утверждается; A/B и предел даны в самой карточке |
| <a id="p10-3"></a>[P10.3](cards/P10-composed-gradient.md#s-3) — LoRA query/KV head sharing | Выведенный контроль/краевой подслучай механизма карточки; отдельный дефект H этим не утверждается; A/B и предел даны в самой карточке |
| <a id="p10-4"></a>[P10.4](cards/P10-composed-gradient.md#s-4) — Accumulation двух пар | Выведенный контроль/краевой подслучай механизма карточки; отдельный дефект H этим не утверждается; A/B и предел даны в самой карточке |
| <a id="p11-1"></a>[P11.1](cards/P11-autograd-reachability.md#s-1) — Embedding backward reachability | [S234](06-source-content-walk.md#s234), [S236](06-source-content-walk.md#s236), [S238](06-source-content-walk.md#s238), [S239](06-source-content-walk.md#s239), [S249](06-source-content-walk.md#s249), [S255](06-source-content-walk.md#s255), [S360](06-source-content-walk.md#s360) |
| <a id="p11-2"></a>[P11.2](cards/P11-autograd-reachability.md#s-2) — Linear bias gradient | [S236](06-source-content-walk.md#s236), [S238](06-source-content-walk.md#s238), [S239](06-source-content-walk.md#s239), [S249](06-source-content-walk.md#s249), [S255](06-source-content-walk.md#s255) |
| <a id="p11-3"></a>[P11.3](cards/P11-autograd-reachability.md#s-3) — MeanPooling: отдельный разрыв градиента | [S268](06-source-content-walk.md#s268), [S273](06-source-content-walk.md#s273), [S277](06-source-content-walk.md#s277), [S279](06-source-content-walk.md#s279) |
| <a id="p11-4"></a>[P11.4](cards/P11-autograd-reachability.md#s-4) — Supervised next-token chain | [S030](06-source-content-walk.md#s030), [S040](06-source-content-walk.md#s040), [S041](06-source-content-walk.md#s041), [S238](06-source-content-walk.md#s238), [S239](06-source-content-walk.md#s239), [S265](06-source-content-walk.md#s265) |
| <a id="p11-5"></a>[P11.5](cards/P11-autograd-reachability.md#s-5) — Slice backward — условный operator caller | [S255](06-source-content-walk.md#s255) |
| <a id="p11-6"></a>[P11.6](cards/P11-autograd-reachability.md#s-6) — Concat backward — условный operator caller | [S255](06-source-content-walk.md#s255) |
| <a id="p11-7"></a>[P11.7](cards/P11-autograd-reachability.md#s-7) — Unsqueeze backward — условный operator caller | [S255](06-source-content-walk.md#s255) |
| <a id="p11-8"></a>[P11.8](cards/P11-autograd-reachability.md#s-8) — Gather backward — условный operator caller | [S255](06-source-content-walk.md#s255) |
| <a id="p11-9"></a>[P11.9](cards/P11-autograd-reachability.md#s-9) — L2Normalize: отдельный разрыв градиента | [S273](06-source-content-walk.md#s273), [S277](06-source-content-walk.md#s277), [S279](06-source-content-walk.md#s279) |
| <a id="p11-10"></a>[P11.10](cards/P11-autograd-reachability.md#s-10) — Tensor training API против конечного float[] | [S273](06-source-content-walk.md#s273), [S279](06-source-content-walk.md#s279) |
| <a id="p11-11"></a>[P11.11](cards/P11-autograd-reachability.md#s-11) — Обучаемый sentence encoder и согласованный индекс | [S273](06-source-content-walk.md#s273), [S279](06-source-content-walk.md#s279) |
| <a id="p12-1"></a>[P12.1](cards/P12-optimizer-boundaries.md#s-1) — Learning rate warm-up zero | [S041](06-source-content-walk.md#s041), [S236](06-source-content-walk.md#s236), [S239](06-source-content-walk.md#s239), [S249](06-source-content-walk.md#s249), [S265](06-source-content-walk.md#s265), [S360](06-source-content-walk.md#s360) |
| <a id="p12-2"></a>[P12.2](cards/P12-optimizer-boundaries.md#s-2) — Clipping overflow float norm | Выведенный контроль/краевой подслучай механизма карточки; отдельный дефект H этим не утверждается; A/B и предел даны в самой карточке |
| <a id="p12-3"></a>[P12.3](cards/P12-optimizer-boundaries.md#s-3) — SFT partial state и отказ до update | Выведенный контроль/краевой подслучай механизма карточки; отдельный дефект H этим не утверждается; A/B и предел даны в самой карточке |
| <a id="p12-4"></a>[P12.4](cards/P12-optimizer-boundaries.md#s-4) — Один nonfinite gradient заражает общий clip norm | [S252](06-source-content-walk.md#s252) |
| <a id="p12-5"></a>[P12.5](cards/P12-optimizer-boundaries.md#s-5) — SFT schedule position после нового Trainer | [S236](06-source-content-walk.md#s236), [S239](06-source-content-walk.md#s239), [S249](06-source-content-walk.md#s249) |
| <a id="p12-6"></a>[P12.6](cards/P12-optimizer-boundaries.md#s-6) — Pretraining первый нулевой LR и счётчик updates | [S236](06-source-content-walk.md#s236), [S239](06-source-content-walk.md#s239), [S249](06-source-content-walk.md#s249) |
| <a id="p13-1"></a>[P13.1](cards/P13-finite-ingress.md#s-1) — NaN/Inf vector ingress | [S004](06-source-content-walk.md#s004), [S256](06-source-content-walk.md#s256) |
| <a id="p13-2"></a>[P13.2](cards/P13-finite-ingress.md#s-2) — Недопустимое число до optimizer mutation | [S252](06-source-content-walk.md#s252), [S256](06-source-content-walk.md#s256) |
| <a id="p13-3"></a>[P13.3](cards/P13-finite-ingress.md#s-3) — Защитный upstream caller как отрицательный контроль | [S256](06-source-content-walk.md#s256) |
| <a id="p14-1"></a>[P14.1](cards/P14-teacher-provenance.md#s-1) — Per-side author kind | [S001](06-source-content-walk.md#s001), [S003](06-source-content-walk.md#s003), [S004](06-source-content-walk.md#s004), [S006](06-source-content-walk.md#s006), [S039](06-source-content-walk.md#s039), [S042](06-source-content-walk.md#s042), [S321](06-source-content-walk.md#s321) |
| <a id="p14-2"></a>[P14.2](cards/P14-teacher-provenance.md#s-2) — Per-side model ID через import | [S321](06-source-content-walk.md#s321), [S360](06-source-content-walk.md#s360) |
| <a id="p14-3"></a>[P14.3](cards/P14-teacher-provenance.md#s-3) — Сохранение provenance после shuffle/restore | [S015](06-source-content-walk.md#s015), [S321](06-source-content-walk.md#s321) |
| <a id="p14-4"></a>[P14.4](cards/P14-teacher-provenance.md#s-4) — Регистр JSON полей и точная сохранность принятой пары | [S042](06-source-content-walk.md#s042) |
| <a id="p14-5"></a>[P14.5](cards/P14-teacher-provenance.md#s-5) — Shuffle сохраняет admission вместе с pair | [S191](06-source-content-walk.md#s191) |
| <a id="p15-1"></a>[P15.1](cards/P15-retention-replay.md#s-1) — Replay on/off при том же бюджете | [S003](06-source-content-walk.md#s003), [S005](06-source-content-walk.md#s005), [S008](06-source-content-walk.md#s008), [S015](06-source-content-walk.md#s015), [S016](06-source-content-walk.md#s016), [S029](06-source-content-walk.md#s029), [S031](06-source-content-walk.md#s031), [S036](06-source-content-walk.md#s036), [S039](06-source-content-walk.md#s039), [S043](06-source-content-walk.md#s043), [S284](06-source-content-walk.md#s284) |
| <a id="p15-2"></a>[P15.2](cards/P15-retention-replay.md#s-2) — Curriculum order на фиксированных pairs | [S003](06-source-content-walk.md#s003), [S015](06-source-content-walk.md#s015), [S036](06-source-content-walk.md#s036), [S039](06-source-content-walk.md#s039) |
| <a id="p15-3"></a>[P15.3](cards/P15-retention-replay.md#s-3) — Сохранение старого ненулевого навыка | [S003](06-source-content-walk.md#s003), [S005](06-source-content-walk.md#s005), [S012](06-source-content-walk.md#s012), [S015](06-source-content-walk.md#s015), [S024](06-source-content-walk.md#s024), [S030](06-source-content-walk.md#s030) |
| <a id="p16-1"></a>[P16.1](cards/P16-semantic-order-roles.md#s-1) — Порядок действий при том же bag | [S003](06-source-content-walk.md#s003), [S007](06-source-content-walk.md#s007), [S009](06-source-content-walk.md#s009), [S010](06-source-content-walk.md#s010), [S020](06-source-content-walk.md#s020), [S029](06-source-content-walk.md#s029), [S036](06-source-content-walk.md#s036), [S037](06-source-content-walk.md#s037), [S038](06-source-content-walk.md#s038), [S039](06-source-content-walk.md#s039), [S142](06-source-content-walk.md#s142), [S147](06-source-content-walk.md#s147), [S176](06-source-content-walk.md#s176), [S286](06-source-content-walk.md#s286), [S303](06-source-content-walk.md#s303), [S359](06-source-content-walk.md#s359), [S360](06-source-content-walk.md#s360) |
| <a id="p16-2"></a>[P16.2](cards/P16-semantic-order-roles.md#s-2) — Actor/recipient roles | [S003](06-source-content-walk.md#s003), [S007](06-source-content-walk.md#s007), [S009](06-source-content-walk.md#s009), [S010](06-source-content-walk.md#s010) |
| <a id="p16-3"></a>[P16.3](cards/P16-semantic-order-roles.md#s-3) — Scope и отрицание в представлении | [S003](06-source-content-walk.md#s003), [S007](06-source-content-walk.md#s007), [S009](06-source-content-walk.md#s009), [S010](06-source-content-walk.md#s010) |
| <a id="p16-4"></a>[P16.4](cards/P16-semantic-order-roles.md#s-4) — Числа и hash collision без потери строкового источника | [S007](06-source-content-walk.md#s007), [S009](06-source-content-walk.md#s009), [S010](06-source-content-walk.md#s010) |
| <a id="p16-5"></a>[P16.5](cards/P16-semantic-order-roles.md#s-5) — Универсальный positional-BPE block при frozen tokenizer | [S034](06-source-content-walk.md#s034), [S035](06-source-content-walk.md#s035) |
| <a id="p17-1"></a>[P17.1](cards/P17-tokenizer-refit.md#s-1) — BPE refit после нового урока | [S034](06-source-content-walk.md#s034), [S045](06-source-content-walk.md#s045) |
| <a id="p17-2"></a>[P17.2](cards/P17-tokenizer-refit.md#s-2) — Порядок одинаковых frequency merges | [S084](06-source-content-walk.md#s084), [S087](06-source-content-walk.md#s087) |
| <a id="p17-3"></a>[P17.3](cards/P17-tokenizer-refit.md#s-3) — CRLF/LF и platform newline | [S084](06-source-content-walk.md#s084) |
| <a id="p17-4"></a>[P17.4](cards/P17-tokenizer-refit.md#s-4) — Согласованный refit всего корпуса против frozen BPE | [S084](06-source-content-walk.md#s084), [S103](06-source-content-walk.md#s103) |
| <a id="p18-1"></a>[P18.1](cards/P18-state-identity.md#s-1) — Shape mismatch и неполная норма | [S099](06-source-content-walk.md#s099), [S108](06-source-content-walk.md#s108), [S109](06-source-content-walk.md#s109), [S110](06-source-content-walk.md#s110) |
| <a id="p18-2"></a>[P18.2](cards/P18-state-identity.md#s-2) — Повтор ExampleId и dictionary/index identity | [S046](06-source-content-walk.md#s046), [S048](06-source-content-walk.md#s048), [S103](06-source-content-walk.md#s103), [S330](06-source-content-walk.md#s330), [S331](06-source-content-walk.md#s331), [S332](06-source-content-walk.md#s332), [S333](06-source-content-walk.md#s333), [S360](06-source-content-walk.md#s360) |
| <a id="p18-3"></a>[P18.3](cards/P18-state-identity.md#s-3) — Case sensitivity идентификатора | [S001](06-source-content-walk.md#s001), [S112](06-source-content-walk.md#s112), [S116](06-source-content-walk.md#s116), [S360](06-source-content-walk.md#s360) |
| <a id="p18-4"></a>[P18.4](cards/P18-state-identity.md#s-4) — Live encoder W1 против W0 index | [S007](06-source-content-walk.md#s007), [S012](06-source-content-walk.md#s012), [S056](06-source-content-walk.md#s056), [S117](06-source-content-walk.md#s117), [S360](06-source-content-walk.md#s360) |
| <a id="p18-5"></a>[P18.5](cards/P18-state-identity.md#s-5) — StableHash Int32.MinValue boundary | [S054](06-source-content-walk.md#s054), [S330](06-source-content-walk.md#s330), [S332](06-source-content-walk.md#s332), [S333](06-source-content-walk.md#s333), [S360](06-source-content-walk.md#s360) |
| <a id="p19-1"></a>[P19.1](cards/P19-mutable-output.md#s-1) — Мутация исходного TOutput после Train | [S308](06-source-content-walk.md#s308), [S309](06-source-content-walk.md#s309), [S319](06-source-content-walk.md#s319) |
| <a id="p19-2"></a>[P19.2](cards/P19-mutable-output.md#s-2) — Мутация возвращённого output между Execute | [S308](06-source-content-walk.md#s308), [S309](06-source-content-walk.md#s309), [S319](06-source-content-walk.md#s319) |
| <a id="p19-3"></a>[P19.3](cards/P19-mutable-output.md#s-3) — Мутация входных PredicateIds/DomainTags/EvidenceIds при immutable Output | [S308](06-source-content-walk.md#s308), [S309](06-source-content-walk.md#s309), [S319](06-source-content-walk.md#s319), [S360](06-source-content-walk.md#s360) |
| <a id="p19-4"></a>[P19.4](cards/P19-mutable-output.md#s-4) — Мутация возвращённой metadata при immutable Output | [S319](06-source-content-walk.md#s319) |
| <a id="p20-1"></a>[P20.1](cards/P20-hnsw-reachability.md#s-1) — HNSW обратное pruning и достижимость урока | [S050](06-source-content-walk.md#s050), [S176](06-source-content-walk.md#s176), [S178](06-source-content-walk.md#s178), [S188](06-source-content-walk.md#s188) |
| <a id="p20-2"></a>[P20.2](cards/P20-hnsw-reachability.md#s-2) — Exact-index контроль при тех же vectors | [S034](06-source-content-walk.md#s034), [S037](06-source-content-walk.md#s037), [S048](06-source-content-walk.md#s048), [S178](06-source-content-walk.md#s178), [S188](06-source-content-walk.md#s188) |
| <a id="p21-1"></a>[P21.1](cards/P21-hnsw-ingress-atomicity.md#s-1) — Failed Add после несовместимой размерности | [S213](06-source-content-walk.md#s213), [S216](06-source-content-walk.md#s216), [S233](06-source-content-walk.md#s233) |
| <a id="p21-2"></a>[P21.2](cards/P21-hnsw-ingress-atomicity.md#s-2) — Мутация caller vector после Add | [S213](06-source-content-walk.md#s213), [S217](06-source-content-walk.md#s217), [S233](06-source-content-walk.md#s233) |
| <a id="p21-3"></a>[P21.3](cards/P21-hnsw-ingress-atomicity.md#s-3) — NaN/Inf на ingress индекса | [S213](06-source-content-walk.md#s213), [S217](06-source-content-walk.md#s217), [S233](06-source-content-walk.md#s233) |
| <a id="p22-1"></a>[P22.1](cards/P22-hnsw-rng-continuation.md#s-1) — Save/load и продолжение Add без RNG state | [S048](06-source-content-walk.md#s048), [S237](06-source-content-walk.md#s237), [S240](06-source-content-walk.md#s240), [S251](06-source-content-walk.md#s251) |
| <a id="p22-2"></a>[P22.2](cards/P22-hnsw-rng-continuation.md#s-2) — Вставки в другой индекс меняют shared RNG | [S046](06-source-content-walk.md#s046), [S048](06-source-content-walk.md#s048), [S237](06-source-content-walk.md#s237), [S240](06-source-content-walk.md#s240), [S251](06-source-content-walk.md#s251) |
| <a id="p22-3"></a>[P22.3](cards/P22-hnsw-rng-continuation.md#s-3) — Rebuild против search-only restore | [S046](06-source-content-walk.md#s046) |
| <a id="p23-1"></a>[P23.1](cards/P23-retrieval-fusion.md#s-1) — Transformer embedding prefix 64 TOKENS | [S018](06-source-content-walk.md#s018), [S037](06-source-content-walk.md#s037), [S056](06-source-content-walk.md#s056), [S117](06-source-content-walk.md#s117), [S176](06-source-content-walk.md#s176), [S360](06-source-content-walk.md#s360) |
| <a id="p23-2"></a>[P23.2](cards/P23-retrieval-fusion.md#s-2) — Candidate-pool normalization | [S003](06-source-content-walk.md#s003), [S005](06-source-content-walk.md#s005), [S036](06-source-content-walk.md#s036), [S072](06-source-content-walk.md#s072), [S073](06-source-content-walk.md#s073), [S074](06-source-content-walk.md#s074), [S075](06-source-content-walk.md#s075), [S076](06-source-content-walk.md#s076), [S077](06-source-content-walk.md#s077), [S078](06-source-content-walk.md#s078), [S079](06-source-content-walk.md#s079), [S080](06-source-content-walk.md#s080), [S081](06-source-content-walk.md#s081), [S082](06-source-content-walk.md#s082), [S083](06-source-content-walk.md#s083), [S103](06-source-content-walk.md#s103) |
| <a id="p23-3"></a>[P23.3](cards/P23-retrieval-fusion.md#s-3) — Unknown pattern bonus | [S074](06-source-content-walk.md#s074), [S079](06-source-content-walk.md#s079), [S082](06-source-content-walk.md#s082), [S083](06-source-content-walk.md#s083) |
| <a id="p23-4"></a>[P23.4](cards/P23-retrieval-fusion.md#s-4) — Global IDF после unrelated corpus addition | [S074](06-source-content-walk.md#s074), [S077](06-source-content-walk.md#s077), [S078](06-source-content-walk.md#s078), [S079](06-source-content-walk.md#s079), [S080](06-source-content-walk.md#s080), [S081](06-source-content-walk.md#s081) |
| <a id="p23-5"></a>[P23.5](cards/P23-retrieval-fusion.md#s-5) — ANN shortlist не содержит lexical-relevant кандидата | [S074](06-source-content-walk.md#s074), [S117](06-source-content-walk.md#s117) |
| <a id="p24-1"></a>[P24.1](cards/P24-replay-selection.md#s-1) — Count<=topK bypass ranking | [S123](06-source-content-walk.md#s123), [S125](06-source-content-walk.md#s125), [S221](06-source-content-walk.md#s221), [S222](06-source-content-walk.md#s222), [S223](06-source-content-walk.md#s223), [S224](06-source-content-walk.md#s224), [S225](06-source-content-walk.md#s225), [S226](06-source-content-walk.md#s226), [S330](06-source-content-walk.md#s330), [S331](06-source-content-walk.md#s331), [S332](06-source-content-walk.md#s332), [S333](06-source-content-walk.md#s333), [S336](06-source-content-walk.md#s336), [S344](06-source-content-walk.md#s344), [S345](06-source-content-walk.md#s345), [S346](06-source-content-walk.md#s346), [S354](06-source-content-walk.md#s354), [S360](06-source-content-walk.md#s360) |
| <a id="p24-2"></a>[P24.2](cards/P24-replay-selection.md#s-2) — Lexical vector против настоящего frozen embedding | [S226](06-source-content-walk.md#s226), [S330](06-source-content-walk.md#s330), [S331](06-source-content-walk.md#s331), [S332](06-source-content-walk.md#s332), [S333](06-source-content-walk.md#s333), [S336](06-source-content-walk.md#s336), [S344](06-source-content-walk.md#s344), [S345](06-source-content-walk.md#s345), [S346](06-source-content-walk.md#s346), [S354](06-source-content-walk.md#s354) |
| <a id="p24-3"></a>[P24.3](cards/P24-replay-selection.md#s-3) — Pattern-копия lexical vector против независимого order/scope признака | [S221](06-source-content-walk.md#s221), [S222](06-source-content-walk.md#s222), [S223](06-source-content-walk.md#s223), [S224](06-source-content-walk.md#s224), [S225](06-source-content-walk.md#s225), [S226](06-source-content-walk.md#s226), [S330](06-source-content-walk.md#s330), [S331](06-source-content-walk.md#s331), [S332](06-source-content-walk.md#s332), [S333](06-source-content-walk.md#s333), [S336](06-source-content-walk.md#s336), [S344](06-source-content-walk.md#s344), [S345](06-source-content-walk.md#s345), [S346](06-source-content-walk.md#s346), [S354](06-source-content-walk.md#s354) |
| <a id="p24-4"></a>[P24.4](cards/P24-replay-selection.md#s-4) — Score shortlist gate | [S330](06-source-content-walk.md#s330), [S331](06-source-content-walk.md#s331), [S332](06-source-content-walk.md#s332), [S333](06-source-content-walk.md#s333), [S336](06-source-content-walk.md#s336), [S344](06-source-content-walk.md#s344) |
| <a id="p24-5"></a>[P24.5](cards/P24-replay-selection.md#s-5) — ReplayMixer повторно сортирует по времени | [S152](06-source-content-walk.md#s152), [S156](06-source-content-walk.md#s156), [S157](06-source-content-walk.md#s157), [S162](06-source-content-walk.md#s162), [S344](06-source-content-walk.md#s344), [S354](06-source-content-walk.md#s354) |
| <a id="p24-6"></a>[P24.6](cards/P24-replay-selection.md#s-6) — Top-3 aperture к DPO или KNN recipient | [S152](06-source-content-walk.md#s152), [S156](06-source-content-walk.md#s156), [S157](06-source-content-walk.md#s157), [S162](06-source-content-walk.md#s162), [S223](06-source-content-walk.md#s223), [S344](06-source-content-walk.md#s344), [S354](06-source-content-walk.md#s354) |
| <a id="p24-7"></a>[P24.7](cards/P24-replay-selection.md#s-7) — Вложенный KnnExamples переносится отдельно от manifest shortlist | [S152](06-source-content-walk.md#s152), [S162](06-source-content-walk.md#s162) |
| <a id="p24-8"></a>[P24.8](cards/P24-replay-selection.md#s-8) — Предел 256: полный отказ против проверяемого bounded snapshot | [S152](06-source-content-walk.md#s152), [S162](06-source-content-walk.md#s162) |
| <a id="p24-9"></a>[P24.9](cards/P24-replay-selection.md#s-9) — Coverage законов внутри одного DomainTag | [S152](06-source-content-walk.md#s152), [S162](06-source-content-walk.md#s162) |
| <a id="p24-10"></a>[P24.10](cards/P24-replay-selection.md#s-10) — Старый Unit доступен обычному router | [S152](06-source-content-walk.md#s152), [S156](06-source-content-walk.md#s156), [S162](06-source-content-walk.md#s162), [S233](06-source-content-walk.md#s233) |
| <a id="p25-1"></a>[P25.1](cards/P25-cold-restore.md#s-1) — Metadata-only export против owner full state | [S001](06-source-content-walk.md#s001), [S020](06-source-content-walk.md#s020), [S022](06-source-content-walk.md#s022), [S024](06-source-content-walk.md#s024), [S026](06-source-content-walk.md#s026), [S027](06-source-content-walk.md#s027), [S028](06-source-content-walk.md#s028), [S029](06-source-content-walk.md#s029), [S030](06-source-content-walk.md#s030), [S031](06-source-content-walk.md#s031), [S032](06-source-content-walk.md#s032), [S033](06-source-content-walk.md#s033), [S036](06-source-content-walk.md#s036), [S097](06-source-content-walk.md#s097), [S103](06-source-content-walk.md#s103), [S178](06-source-content-walk.md#s178), [S268](06-source-content-walk.md#s268), [S272](06-source-content-walk.md#s272), [S284](06-source-content-walk.md#s284) |
| <a id="p25-2"></a>[P25.2](cards/P25-cold-restore.md#s-2) — Cold import того же обученного состояния | [S001](06-source-content-walk.md#s001), [S002](06-source-content-walk.md#s002), [S003](06-source-content-walk.md#s003), [S004](06-source-content-walk.md#s004), [S007](06-source-content-walk.md#s007), [S008](06-source-content-walk.md#s008), [S012](06-source-content-walk.md#s012), [S013](06-source-content-walk.md#s013), [S014](06-source-content-walk.md#s014), [S016](06-source-content-walk.md#s016), [S017](06-source-content-walk.md#s017), [S019](06-source-content-walk.md#s019), [S022](06-source-content-walk.md#s022), [S039](06-source-content-walk.md#s039), [S040](06-source-content-walk.md#s040), [S041](06-source-content-walk.md#s041), [S043](06-source-content-walk.md#s043), [S056](06-source-content-walk.md#s056), [S057](06-source-content-walk.md#s057), [S058](06-source-content-walk.md#s058), [S060](06-source-content-walk.md#s060), [S061](06-source-content-walk.md#s061), [S084](06-source-content-walk.md#s084), [S093](06-source-content-walk.md#s093), [S095](06-source-content-walk.md#s095), [S111](06-source-content-walk.md#s111), [S113](06-source-content-walk.md#s113), [S123](06-source-content-walk.md#s123), [S124](06-source-content-walk.md#s124), [S125](06-source-content-walk.md#s125), [S147](06-source-content-walk.md#s147), [S148](06-source-content-walk.md#s148), [S155](06-source-content-walk.md#s155), [S161](06-source-content-walk.md#s161), [S165](06-source-content-walk.md#s165), [S212](06-source-content-walk.md#s212), [S276](06-source-content-walk.md#s276), [S286](06-source-content-walk.md#s286), [S298](06-source-content-walk.md#s298), [S330](06-source-content-walk.md#s330), [S340](06-source-content-walk.md#s340), [S360](06-source-content-walk.md#s360) |
| <a id="p25-3"></a>[P25.3](cards/P25-cold-restore.md#s-3) — Cache alias внешней мутации | [S001](06-source-content-walk.md#s001), [S101](06-source-content-walk.md#s101), [S151](06-source-content-walk.md#s151), [S161](06-source-content-walk.md#s161) |
| <a id="p25-4"></a>[P25.4](cards/P25-cold-restore.md#s-4) — Cache mtime/length без content hash | [S001](06-source-content-walk.md#s001), [S022](06-source-content-walk.md#s022), [S101](06-source-content-walk.md#s101), [S151](06-source-content-walk.md#s151), [S161](06-source-content-walk.md#s161) |
| <a id="p25-5"></a>[P25.5](cards/P25-cold-restore.md#s-5) — Миграция correction key без утраченного SourceLang | [S122](06-source-content-walk.md#s122), [S133](06-source-content-walk.md#s133), [S134](06-source-content-walk.md#s134), [S143](06-source-content-walk.md#s143) |
| <a id="p26-1"></a>[P26.1](cards/P26-structural-features.md#s-1) — BPE token index против char offset | [S038](06-source-content-walk.md#s038), [S043](06-source-content-walk.md#s043), [S176](06-source-content-walk.md#s176), [S203](06-source-content-walk.md#s203), [S204](06-source-content-walk.md#s204), [S207](06-source-content-walk.md#s207), [S209](06-source-content-walk.md#s209), [S214](06-source-content-walk.md#s214), [S268](06-source-content-walk.md#s268), [S269](06-source-content-walk.md#s269), [S272](06-source-content-walk.md#s272), [S293](06-source-content-walk.md#s293), [S297](06-source-content-walk.md#s297), [S323](06-source-content-walk.md#s323), [S359](06-source-content-walk.md#s359), [S360](06-source-content-walk.md#s360) |
| <a id="p26-2"></a>[P26.2](cards/P26-structural-features.md#s-2) — Parent feature реально используется | [S038](06-source-content-walk.md#s038), [S214](06-source-content-walk.md#s214) |
| <a id="p26-3"></a>[P26.3](cards/P26-structural-features.md#s-3) — Scope feature реально используется | [S038](06-source-content-walk.md#s038), [S214](06-source-content-walk.md#s214) |
| <a id="p26-4"></a>[P26.4](cards/P26-structural-features.md#s-4) — Sibling feature реально используется | [S273](06-source-content-walk.md#s273), [S277](06-source-content-walk.md#s277), [S278](06-source-content-walk.md#s278) |
| <a id="p26-5"></a>[P26.5](cards/P26-structural-features.md#s-5) — E07 tier flag меняет фактический train key | [S038](06-source-content-walk.md#s038), [S043](06-source-content-walk.md#s043), [S207](06-source-content-walk.md#s207), [S209](06-source-content-walk.md#s209), [S214](06-source-content-walk.md#s214), [S285](06-source-content-walk.md#s285) |
| <a id="p27-1"></a>[P27.1](cards/P27-structural-codec.md#s-1) — RawKind и SyntaxKind label codec | [S360](06-source-content-walk.md#s360) |
| <a id="p27-2"></a>[P27.2](cards/P27-structural-codec.md#s-2) — Tie rule после перестановки train rows | [S140](06-source-content-walk.md#s140) |
| <a id="p27-3"></a>[P27.3](cards/P27-structural-codec.md#s-3) — Cross-file transitions | [S227](06-source-content-walk.md#s227), [S228](06-source-content-walk.md#s228), [S229](06-source-content-walk.md#s229), [S230](06-source-content-walk.md#s230), [S231](06-source-content-walk.md#s231) |
| <a id="p27-4"></a>[P27.4](cards/P27-structural-codec.md#s-4) — Файловый kindMap против общего отображения SyntaxKind | [S001](06-source-content-walk.md#s001), [S018](06-source-content-walk.md#s018), [S022](06-source-content-walk.md#s022), [S037](06-source-content-walk.md#s037), [S038](06-source-content-walk.md#s038), [S140](06-source-content-walk.md#s140) |
| <a id="p27-5"></a>[P27.5](cards/P27-structural-codec.md#s-5) — Одна validation-панель для baseline и E09 kNN | [S001](06-source-content-walk.md#s001), [S002](06-source-content-walk.md#s002), [S005](06-source-content-walk.md#s005), [S009](06-source-content-walk.md#s009), [S010](06-source-content-walk.md#s010), [S017](06-source-content-walk.md#s017), [S018](06-source-content-walk.md#s018), [S029](06-source-content-walk.md#s029), [S031](06-source-content-walk.md#s031), [S037](06-source-content-walk.md#s037), [S038](06-source-content-walk.md#s038), [S040](06-source-content-walk.md#s040), [S140](06-source-content-walk.md#s140), [S149](06-source-content-walk.md#s149), [S175](06-source-content-walk.md#s175), [S269](06-source-content-walk.md#s269), [S273](06-source-content-walk.md#s273), [S285](06-source-content-walk.md#s285), [S293](06-source-content-walk.md#s293), [S323](06-source-content-walk.md#s323), [S324](06-source-content-walk.md#s324), [S330](06-source-content-walk.md#s330), [S336](06-source-content-walk.md#s336), [S337](06-source-content-walk.md#s337), [S338](06-source-content-walk.md#s338) |
| <a id="p27-6"></a>[P27.6](cards/P27-structural-codec.md#s-6) — Production SyntaxKind%200 против collision-free codec | [S140](06-source-content-walk.md#s140) |
| <a id="p27-7"></a>[P27.7](cards/P27-structural-codec.md#s-7) — E09 csharpSource и фактический владелец files | [S227](06-source-content-walk.md#s227), [S230](06-source-content-walk.md#s230), [S231](06-source-content-walk.md#s231) |
| <a id="p28-1"></a>[P28.1](cards/P28-morphology-family.md#s-1) — Идентичность morphology family против одного размера | [S023](06-source-content-walk.md#s023), [S042](06-source-content-walk.md#s042), [S044](06-source-content-walk.md#s044), [S048](06-source-content-walk.md#s048), [S173](06-source-content-walk.md#s173), [S233](06-source-content-walk.md#s233), [S303](06-source-content-walk.md#s303), [S325](06-source-content-walk.md#s325) |
| <a id="p28-2"></a>[P28.2](cards/P28-morphology-family.md#s-2) — False family и homonym | [S042](06-source-content-walk.md#s042), [S044](06-source-content-walk.md#s044) |
| <a id="p29-1"></a>[P29.1](cards/P29-formula-memory.md#s-1) — E13B formula retrieval против фиксированного prefix | [S024](06-source-content-walk.md#s024), [S125](06-source-content-walk.md#s125), [S173](06-source-content-walk.md#s173), [S205](06-source-content-walk.md#s205), [S235](06-source-content-walk.md#s235), [S325](06-source-content-walk.md#s325) |
| <a id="p30-1"></a>[P30.1](cards/P30-learned-fusion-alpha.md#s-1) — E34 learned alpha при одной панели | [S001](06-source-content-walk.md#s001), [S003](06-source-content-walk.md#s003), [S005](06-source-content-walk.md#s005), [S010](06-source-content-walk.md#s010), [S018](06-source-content-walk.md#s018), [S029](06-source-content-walk.md#s029), [S031](06-source-content-walk.md#s031), [S037](06-source-content-walk.md#s037), [S038](06-source-content-walk.md#s038), [S039](06-source-content-walk.md#s039), [S040](06-source-content-walk.md#s040), [S175](06-source-content-walk.md#s175), [S354](06-source-content-walk.md#s354) |
| <a id="p31-1"></a>[P31.1](cards/P31-source-trust.md#s-1) — E40V2 learned trust против fixed sum | [S099](06-source-content-walk.md#s099), [S124](06-source-content-walk.md#s124), [S173](06-source-content-walk.md#s173), [S191](06-source-content-walk.md#s191), [S285](06-source-content-walk.md#s285), [S294](06-source-content-walk.md#s294), [S325](06-source-content-walk.md#s325), [S330](06-source-content-walk.md#s330), [S337](06-source-content-walk.md#s337) |
| <a id="p31-2"></a>[P31.2](cards/P31-source-trust.md#s-2) — Source features против capability constant features | Дополнительный подслучай прежнего каталога O; H не назван его прямым основанием: [10-historical-function-transfer-and-learning.md:5](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/10-historical-function-transfer-and-learning.md:5>) |
| <a id="p32-1"></a>[P32.1](cards/P32-skill-selection.md#s-1) — ScoreForKnn влияет на выбор | [S173](06-source-content-walk.md#s173), [S325](06-source-content-walk.md#s325), [S360](06-source-content-walk.md#s360) |
| <a id="p32-2"></a>[P32.2](cards/P32-skill-selection.md#s-2) — Category как настоящий input | Дополнительный подслучай прежнего каталога O; H не назван его прямым основанием: [10-historical-function-transfer-and-learning.md:25](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/10-historical-function-transfer-and-learning.md:25>) |
| <a id="p32-3"></a>[P32.3](cards/P32-skill-selection.md#s-3) — AlwaysRequired и полный load budget | Дополнительный подслучай прежнего каталога O; H не назван его прямым основанием: [10-historical-function-transfer-and-learning.md:25](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/10-historical-function-transfer-and-learning.md:25>) |
| <a id="p32-4"></a>[P32.4](cards/P32-skill-selection.md#s-4) — Category hit rate против required coverage | Дополнительный подслучай прежнего каталога O; H не назван его прямым основанием: [10-historical-function-transfer-and-learning.md:25](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/10-historical-function-transfer-and-learning.md:25>) |
| <a id="p33-1"></a>[P33.1](cards/P33-domain-classifier.md#s-1) — Точный overload | [S001](06-source-content-walk.md#s001), [S002](06-source-content-walk.md#s002), [S005](06-source-content-walk.md#s005), [S010](06-source-content-walk.md#s010), [S023](06-source-content-walk.md#s023), [S036](06-source-content-walk.md#s036), [S037](06-source-content-walk.md#s037), [S038](06-source-content-walk.md#s038), [S039](06-source-content-walk.md#s039), [S175](06-source-content-walk.md#s175), [S336](06-source-content-walk.md#s336), [S338](06-source-content-walk.md#s338) |
| <a id="p33-2"></a>[P33.2](cards/P33-domain-classifier.md#s-2) — Seed против trained memory | Дополнительный подслучай прежнего каталога O; H не назван его прямым основанием: [20-domain-routing-positive-mechanism.md:95](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/20-domain-routing-positive-mechanism.md:95>) |
| <a id="p33-3"></a>[P33.3](cards/P33-domain-classifier.md#s-3) — Независимые labels против self-label | [S037](06-source-content-walk.md#s037) |
| <a id="p33-4"></a>[P33.4](cards/P33-domain-classifier.md#s-4) — OOD abstain при одном обученном состоянии | Дополнительный подслучай прежнего каталога O; H не назван его прямым основанием: [20-domain-routing-positive-mechanism.md:95](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/20-domain-routing-positive-mechanism.md:95>) |
| <a id="p33-5"></a>[P33.5](cards/P33-domain-classifier.md#s-5) — Дедупликация profiles и vote multiplicity | Дополнительный подслучай прежнего каталога O; H не назван его прямым основанием: [20-domain-routing-positive-mechanism.md:95](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/20-domain-routing-positive-mechanism.md:95>) |
| <a id="p33-6"></a>[P33.6](cards/P33-domain-classifier.md#s-6) — Segment ExistingDomain priority | [S003](06-source-content-walk.md#s003) |
| <a id="p33-7"></a>[P33.7](cards/P33-domain-classifier.md#s-7) — Язык и версия predicate slots | [S003](06-source-content-walk.md#s003) |
| <a id="p33-8"></a>[P33.8](cards/P33-domain-classifier.md#s-8) — UNKNOWN fallback | Дополнительный подслучай прежнего каталога O; H не назван его прямым основанием: [20-domain-routing-positive-mechanism.md:95](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/20-domain-routing-positive-mechanism.md:95>) |
| <a id="p33-9"></a>[P33.9](cards/P33-domain-classifier.md#s-9) — Cold-load того же обученного классификатора | Дополнительный подслучай прежнего каталога O; H не назван его прямым основанием: [20-domain-routing-positive-mechanism.md:95](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/20-domain-routing-positive-mechanism.md:95>) |
| <a id="p34-1"></a>[P34.1](cards/P34-router-labels.md#s-1) — E22 AST labels против substring labels | [S299](06-source-content-walk.md#s299), [S313](06-source-content-walk.md#s313), [S323](06-source-content-walk.md#s323), [S324](06-source-content-walk.md#s324), [S325](06-source-content-walk.md#s325), [S330](06-source-content-walk.md#s330), [S336](06-source-content-walk.md#s336), [S338](06-source-content-walk.md#s338) |
| <a id="p35-1"></a>[P35.1](cards/P35-actual-neighbor-votes.md#s-1) — Configured k против actual n votes при n=0..3 | [S299](06-source-content-walk.md#s299), [S300](06-source-content-walk.md#s300), [S301](06-source-content-walk.md#s301), [S302](06-source-content-walk.md#s302), [S313](06-source-content-walk.md#s313), [S323](06-source-content-walk.md#s323) |
| <a id="p36-1"></a>[P36.1](cards/P36-incremental-router.md#s-1) — Train одной новой записью replace против full corpus | [S299](06-source-content-walk.md#s299), [S300](06-source-content-walk.md#s300), [S301](06-source-content-walk.md#s301), [S302](06-source-content-walk.md#s302), [S313](06-source-content-walk.md#s313), [S323](06-source-content-walk.md#s323) |
| <a id="p37-1"></a>[P37.1](cards/P37-empty-router-query.md#s-1) — Empty features и first bucket против явного отказа | [S299](06-source-content-walk.md#s299), [S300](06-source-content-walk.md#s300), [S301](06-source-content-walk.md#s301), [S302](06-source-content-walk.md#s302), [S313](06-source-content-walk.md#s313), [S323](06-source-content-walk.md#s323) |
| <a id="p38-1"></a>[P38.1](cards/P38-learned-segmentation.md#s-1) — E24 learned relevant spans против length proxy | Дополнительный подслучай прежнего каталога O; H не назван его прямым основанием: [20-domain-routing-positive-mechanism.md:107](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/20-domain-routing-positive-mechanism.md:107>) |
| <a id="p38-2"></a>[P38.2](cards/P38-learned-segmentation.md#s-2) — Обычный consumer использует выделенный span | [S299](06-source-content-walk.md#s299), [S313](06-source-content-walk.md#s313) |
| <a id="p39-1"></a>[P39.1](cards/P39-oracle-output-separation.md#s-1) — E51 oracle output против настоящего supplier | [S023](06-source-content-walk.md#s023), [S025](06-source-content-walk.md#s025), [S042](06-source-content-walk.md#s042), [S071](06-source-content-walk.md#s071), [S099](06-source-content-walk.md#s099) |
| <a id="p39-2"></a>[P39.2](cards/P39-oracle-output-separation.md#s-2) — Shared train root vocabulary против per-prompt vocabulary | [S001](06-source-content-walk.md#s001), [S003](06-source-content-walk.md#s003), [S004](06-source-content-walk.md#s004), [S010](06-source-content-walk.md#s010), [S025](06-source-content-walk.md#s025), [S036](06-source-content-walk.md#s036), [S039](06-source-content-walk.md#s039) |
| <a id="p39-3"></a>[P39.3](cards/P39-oracle-output-separation.md#s-3) — Train/source overlap при том же экзамене | [S023](06-source-content-walk.md#s023), [S025](06-source-content-walk.md#s025), [S042](06-source-content-walk.md#s042), [S047](06-source-content-walk.md#s047), [S049](06-source-content-walk.md#s049), [S099](06-source-content-walk.md#s099), [S103](06-source-content-walk.md#s103), [S360](06-source-content-walk.md#s360) |
| <a id="p39-4"></a>[P39.4](cards/P39-oracle-output-separation.md#s-4) — CH ID permutation | [S025](06-source-content-walk.md#s025) |
| <a id="p39-5"></a>[P39.5](cards/P39-oracle-output-separation.md#s-5) — First32 общего root vocabulary и порядок корпуса | [S050](06-source-content-walk.md#s050) |
| <a id="p39-6"></a>[P39.6](cards/P39-oracle-output-separation.md#s-6) — Новые E51A примеры и проверка лишних predicates | [S047](06-source-content-walk.md#s047), [S049](06-source-content-walk.md#s049) |
| <a id="p40-1"></a>[P40.1](cards/P40-router-feedback.md#s-1) — E35 coupled seed и split labels | [S098](06-source-content-walk.md#s098), [S115](06-source-content-walk.md#s115), [S120](06-source-content-walk.md#s120), [S124](06-source-content-walk.md#s124) |
| <a id="p40-2"></a>[P40.2](cards/P40-router-feedback.md#s-2) — E36 learned feedback memory против majority baseline | [S115](06-source-content-walk.md#s115), [S120](06-source-content-walk.md#s120), [S360](06-source-content-walk.md#s360) |
| <a id="p40-3"></a>[P40.3](cards/P40-router-feedback.md#s-3) — Enhanced deterministic branch против learned state | [S115](06-source-content-walk.md#s115), [S120](06-source-content-walk.md#s120), [S124](06-source-content-walk.md#s124) |
| <a id="p40-4"></a>[P40.4](cards/P40-router-feedback.md#s-4) — E36 split по семейству шаблона feedback | [S120](06-source-content-walk.md#s120) |
| <a id="p40-5"></a>[P40.5](cards/P40-router-feedback.md#s-5) — E36 контекст адресата при одинаковом feedback | [S120](06-source-content-walk.md#s120) |
| <a id="p41-1"></a>[P41.1](cards/P41-stop-and-split.md#s-1) — E21: параметр порога действительно меняет решение | [S203](06-source-content-walk.md#s203), [S204](06-source-content-walk.md#s204), [S207](06-source-content-walk.md#s207), [S209](06-source-content-walk.md#s209), [S215](06-source-content-walk.md#s215) |
| <a id="p41-2"></a>[P41.2](cards/P41-stop-and-split.md#s-2) — E32: kNN против медианы на одинаковых count labels | [S127](06-source-content-walk.md#s127), [S131](06-source-content-walk.md#s131), [S145](06-source-content-walk.md#s145) |
| <a id="p41-3"></a>[P41.3](cards/P41-stop-and-split.md#s-3) — E33: правила STOP/CONTINUE против kNN | Дополнительный подслучай прежнего каталога O; H не назван его прямым основанием: [29-e33-failed-status-versus-useful-rule.md:7](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/29-e33-failed-status-versus-useful-rule.md:7>) |
| <a id="p41-4"></a>[P41.4](cards/P41-stop-and-split.md#s-4) — E32: передача count в реальное разбиение | [S127](06-source-content-walk.md#s127), [S131](06-source-content-walk.md#s131), [S145](06-source-content-walk.md#s145) |
| <a id="p41-5"></a>[P41.5](cards/P41-stop-and-split.md#s-5) — E32 row split против canonical-family holdout | [S127](06-source-content-walk.md#s127), [S131](06-source-content-walk.md#s131), [S145](06-source-content-walk.md#s145) |
| <a id="p42-1"></a>[P42.1](cards/P42-checker-arbiter.md#s-1) — Checker получает ответ и задачу против request-only | Дополнительный подслучай прежнего каталога O; H не назван его прямым основанием: [40-e52-contract-success-and-answer-independent-veto.md:30](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/40-e52-contract-success-and-answer-independent-veto.md:30>), [40-e52-contract-success-and-answer-independent-veto.md:38](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/40-e52-contract-success-and-answer-independent-veto.md:38>) |
| <a id="p42-2"></a>[P42.2](cards/P42-checker-arbiter.md#s-2) — Queued feedback против actual update | [S036](06-source-content-walk.md#s036), [S039](06-source-content-walk.md#s039) |
| <a id="p42-3"></a>[P42.3](cards/P42-checker-arbiter.md#s-3) — Arbiter override и law authority | [S003](06-source-content-walk.md#s003), [S023](06-source-content-walk.md#s023) |
| <a id="p42-4"></a>[P42.4](cards/P42-checker-arbiter.md#s-4) — E39 promotion после фактической проверки | [S005](06-source-content-walk.md#s005), [S127](06-source-content-walk.md#s127), [S146](06-source-content-walk.md#s146) |
| <a id="p42-5"></a>[P42.5](cards/P42-checker-arbiter.md#s-5) — Объявленный список проверок против реально выполненных | [S146](06-source-content-walk.md#s146) |
| <a id="p43-1"></a>[P43.1](cards/P43-contextual-pos.md#s-1) — Исторический E26: статический контекстный выбор перевода | [S018](06-source-content-walk.md#s018), [S114](06-source-content-walk.md#s114), [S124](06-source-content-walk.md#s124), [S125](06-source-content-walk.md#s125), [S175](06-source-content-walk.md#s175), [S176](06-source-content-walk.md#s176), [S269](06-source-content-walk.md#s269), [S273](06-source-content-walk.md#s273), [S285](06-source-content-walk.md#s285), [S297](06-source-content-walk.md#s297), [S323](06-source-content-walk.md#s323), [S324](06-source-content-walk.md#s324), [S325](06-source-content-walk.md#s325), [S330](06-source-content-walk.md#s330), [S336](06-source-content-walk.md#s336), [S360](06-source-content-walk.md#s360) |
| <a id="p43-2"></a>[P43.2](cards/P43-contextual-pos.md#s-2) — Lexicon cooccurrence против прямого gold | [S001](06-source-content-walk.md#s001), [S101](06-source-content-walk.md#s101), [S104](06-source-content-walk.md#s104), [S105](06-source-content-walk.md#s105) |
| <a id="p43-3"></a>[P43.3](cards/P43-contextual-pos.md#s-3) — Контекст предыдущего предложения reset/carry | [S100](06-source-content-walk.md#s100), [S103](06-source-content-walk.md#s103), [S111](06-source-content-walk.md#s111), [S114](06-source-content-walk.md#s114) |
| <a id="p43-4"></a>[P43.4](cards/P43-contextual-pos.md#s-4) — Индуцированный word lexicon включён/выключен | [S101](06-source-content-walk.md#s101), [S104](06-source-content-walk.md#s104), [S105](06-source-content-walk.md#s105), [S119](06-source-content-walk.md#s119) |
| <a id="p43-5"></a>[P43.5](cards/P43-contextual-pos.md#s-5) — Индуцированные phrase mappings отдельно от word lexicon | [S101](06-source-content-walk.md#s101) |
| <a id="p43-6"></a>[P43.6](cards/P43-contextual-pos.md#s-6) — Non-RU NOUN/UNKNOWN: управляемое различающее представление | [S100](06-source-content-walk.md#s100), [S103](06-source-content-walk.md#s103), [S111](06-source-content-walk.md#s111) |
| <a id="p43-7"></a>[P43.7](cards/P43-contextual-pos.md#s-7) — Перенос POS-before/after в обучаемый word-map key | [S114](06-source-content-walk.md#s114), [S121](06-source-content-walk.md#s121) |
| <a id="p43-8"></a>[P43.8](cards/P43-contextual-pos.md#s-8) — Числовые POS enum разных схем нельзя переносить напрямую | [S114](06-source-content-walk.md#s114), [S121](06-source-content-walk.md#s121) |
| <a id="p43-9"></a>[P43.9](cards/P43-contextual-pos.md#s-9) — E25 learned POS-bigram classifier против его rule baseline | [S124](06-source-content-walk.md#s124), [S149](06-source-content-walk.md#s149), [S191](06-source-content-walk.md#s191) |
| <a id="p43-10"></a>[P43.10](cards/P43-contextual-pos.md#s-10) — E25: text→POS отдельно от classifier | Дополнительный подслучай прежнего каталога O; H не назван его прямым основанием: [17-positive-pos-knn-versus-production-rules.md:1](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/17-positive-pos-knn-versus-production-rules.md:1>), [17-positive-pos-knn-versus-production-rules.md:19](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/17-positive-pos-knn-versus-production-rules.md:19>) |
| <a id="p43-11"></a>[P43.11](cards/P43-contextual-pos.md#s-11) — E25 label достигает неизменного fusion consumer | Дополнительный подслучай прежнего каталога O; H не назван его прямым основанием: [17-positive-pos-knn-versus-production-rules.md:1](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/17-positive-pos-knn-versus-production-rules.md:1>), [17-positive-pos-knn-versus-production-rules.md:19](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/17-positive-pos-knn-versus-production-rules.md:19>) |
| <a id="p44-1"></a>[P44.1](cards/P44-template-projection.md#s-1) — TemplateVocab projection при одной библиотеке | Дополнительный подслучай прежнего каталога O; H не назван его прямым основанием: [27-e23-template-projection-and-production-difference.md:7](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/27-e23-template-projection-and-production-difference.md:7>), [27-e23-template-projection-and-production-difference.md:30](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/27-e23-template-projection-and-production-difference.md:30>) |
| <a id="p44-2"></a>[P44.2](cards/P44-template-projection.md#s-2) — Production matcher против того же Jaccard selector | Дополнительный подслучай прежнего каталога O; H не назван его прямым основанием: [27-e23-template-projection-and-production-difference.md:7](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/27-e23-template-projection-and-production-difference.md:7>), [27-e23-template-projection-and-production-difference.md:30](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/27-e23-template-projection-and-production-difference.md:30>) |
| <a id="p44-3"></a>[P44.3](cards/P44-template-projection.md#s-3) — Число шаблонов при фиксированном matcher | Дополнительный подслучай прежнего каталога O; H не назван его прямым основанием: [27-e23-template-projection-and-production-difference.md:7](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/27-e23-template-projection-and-production-difference.md:7>), [27-e23-template-projection-and-production-difference.md:30](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/27-e23-template-projection-and-production-difference.md:30>) |
| <a id="p45-1"></a>[P45.1](cards/P45-learned-reordering.md#s-1) — Таблица структурных перестановок против одного swap | [S018](06-source-content-walk.md#s018), [S123](06-source-content-walk.md#s123), [S132](06-source-content-walk.md#s132), [S141](06-source-content-walk.md#s141), [S294](06-source-content-walk.md#s294) |
| <a id="p45-2"></a>[P45.2](cards/P45-learned-reordering.md#s-2) — Неполное покрытие и неизвестный обязательный token | [S132](06-source-content-walk.md#s132), [S141](06-source-content-walk.md#s141) |
| <a id="p46-1"></a>[P46.1](cards/P46-pair-value.md#s-1) — Повторное взвешивание выбранных переходов | [S002](06-source-content-walk.md#s002), [S005](06-source-content-walk.md#s005), [S018](06-source-content-walk.md#s018), [S149](06-source-content-walk.md#s149), [S155](06-source-content-walk.md#s155), [S164](06-source-content-walk.md#s164), [S167](06-source-content-walk.md#s167), [S168](06-source-content-walk.md#s168) |
| <a id="p46-2"></a>[P46.2](cards/P46-pair-value.md#s-2) — Создание переходов до или после shuffle | [S164](06-source-content-walk.md#s164) |
| <a id="p47-1"></a>[P47.1](cards/P47-file-vector-context.md#s-1) — E30 FileVector без Current | [S127](06-source-content-walk.md#s127), [S128](06-source-content-walk.md#s128), [S129](06-source-content-walk.md#s129), [S130](06-source-content-walk.md#s130), [S142](06-source-content-walk.md#s142), [S147](06-source-content-walk.md#s147), [S360](06-source-content-walk.md#s360) |
| <a id="p47-2"></a>[P47.2](cards/P47-file-vector-context.md#s-2) — Fallback меняет источник результата | [S127](06-source-content-walk.md#s127), [S128](06-source-content-walk.md#s128), [S142](06-source-content-walk.md#s142), [S144](06-source-content-walk.md#s144) |
| <a id="p47-3"></a>[P47.3](cards/P47-file-vector-context.md#s-3) — Predicate cache mutable alias | Выведенный контроль/краевой подслучай механизма карточки; отдельный дефект H этим не утверждается; A/B и предел даны в самой карточке |
| <a id="p47-4"></a>[P47.4](cards/P47-file-vector-context.md#s-4) — Condition state входит в lookup | [S142](06-source-content-walk.md#s142) |
| <a id="p47-5"></a>[P47.5](cards/P47-file-vector-context.md#s-5) — E30 first-wins при равном cosine | [S142](06-source-content-walk.md#s142), [S147](06-source-content-walk.md#s147) |
| <a id="p47-6"></a>[P47.6](cards/P47-file-vector-context.md#s-6) — E30 Current equality filter как отдельная политика | [S142](06-source-content-walk.md#s142) |
| <a id="p47-7"></a>[P47.7](cards/P47-file-vector-context.md#s-7) — E30v2 cosine против word Jaccard на одной панели | [S142](06-source-content-walk.md#s142), [S144](06-source-content-walk.md#s144), [S147](06-source-content-walk.md#s147) |
| <a id="p47-8"></a>[P47.8](cards/P47-file-vector-context.md#s-8) — E30v2 no-hit: label0 против abstain | [S144](06-source-content-walk.md#s144) |
| <a id="p47-9"></a>[P47.9](cards/P47-file-vector-context.md#s-9) — E30v2 operator veto отдельно от fallback | [S144](06-source-content-walk.md#s144) |
| <a id="p48-1"></a>[P48.1](cards/P48-weighting-curriculum.md#s-1) — E44 relative weight ×10 при fixed targets | [S003](06-source-content-walk.md#s003), [S005](06-source-content-walk.md#s005), [S018](06-source-content-walk.md#s018), [S036](06-source-content-walk.md#s036), [S039](06-source-content-walk.md#s039), [S149](06-source-content-walk.md#s149), [S155](06-source-content-walk.md#s155), [S164](06-source-content-walk.md#s164), [S234](06-source-content-walk.md#s234), [S235](06-source-content-walk.md#s235), [S241](06-source-content-walk.md#s241), [S248](06-source-content-walk.md#s248), [S250](06-source-content-walk.md#s250), [S273](06-source-content-walk.md#s273), [S360](06-source-content-walk.md#s360) |
| <a id="p48-2"></a>[P48.2](cards/P48-weighting-curriculum.md#s-2) — E45 curriculum order при fixed targets | [S005](06-source-content-walk.md#s005), [S155](06-source-content-walk.md#s155), [S164](06-source-content-walk.md#s164), [S234](06-source-content-walk.md#s234), [S235](06-source-content-walk.md#s235), [S241](06-source-content-walk.md#s241), [S250](06-source-content-walk.md#s250), [S273](06-source-content-walk.md#s273) |
| <a id="p48-3"></a>[P48.3](cards/P48-weighting-curriculum.md#s-3) — Batch stratification вместо storage order | [S164](06-source-content-walk.md#s164), [S171](06-source-content-walk.md#s171), [S173](06-source-content-walk.md#s173), [S174](06-source-content-walk.md#s174), [S294](06-source-content-walk.md#s294) |
| <a id="p48-4"></a>[P48.4](cards/P48-weighting-curriculum.md#s-4) — E44 shuffled validation targets при frozen predictor | [S235](06-source-content-walk.md#s235), [S241](06-source-content-walk.md#s241), [S247](06-source-content-walk.md#s247), [S248](06-source-content-walk.md#s248), [S250](06-source-content-walk.md#s250) |
| <a id="p48-5"></a>[P48.5](cards/P48-weighting-curriculum.md#s-5) — Формирование targets до или после token shuffle | [S235](06-source-content-walk.md#s235), [S241](06-source-content-walk.md#s241), [S247](06-source-content-walk.md#s247), [S248](06-source-content-walk.md#s248), [S250](06-source-content-walk.md#s250) |
| <a id="p49-1"></a>[P49.1](cards/P49-probe-warm-start.md#s-1) — E48 warm start при малом бюджете | [S005](06-source-content-walk.md#s005), [S150](06-source-content-walk.md#s150), [S153](06-source-content-walk.md#s153), [S159](06-source-content-walk.md#s159), [S233](06-source-content-walk.md#s233) |
| <a id="p49-2"></a>[P49.2](cards/P49-probe-warm-start.md#s-2) — Probe backward использует old weights против inplace | Дополнительный подслучай прежнего каталога O; H не назван его прямым основанием: [54-e48-early-learning-and-probe-gradient-boundary.md:1](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/54-e48-early-learning-and-probe-gradient-boundary.md:1>), [24-e29-pair-value-and-probe-objective.md:25](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/24-e29-pair-value-and-probe-objective.md:25>) |
| <a id="p49-3"></a>[P49.3](cards/P49-probe-warm-start.md#s-3) — Донор first60% против релевантного TRAIN-only отбора | [S150](06-source-content-walk.md#s150), [S159](06-source-content-walk.md#s159) |
| <a id="p49-4"></a>[P49.4](cards/P49-probe-warm-start.md#s-4) — Persistent warm state против повторного pretraining | [S150](06-source-content-walk.md#s150), [S153](06-source-content-walk.md#s153), [S159](06-source-content-walk.md#s159) |
| <a id="p50-1"></a>[P50.1](cards/P50-graph-dependencies.md#s-1) — E49/E50 dependency heuristic | [S160](06-source-content-walk.md#s160) |
| <a id="p50-2"></a>[P50.2](cards/P50-graph-dependencies.md#s-2) — Noise/rename robustness | [S160](06-source-content-walk.md#s160) |
| <a id="p50-3"></a>[P50.3](cards/P50-graph-dependencies.md#s-3) — Early graph closure | [S160](06-source-content-walk.md#s160) |
| <a id="p50-4"></a>[P50.4](cards/P50-graph-dependencies.md#s-4) — E50 ранний низкий delta против устойчивого окна | [S150](06-source-content-walk.md#s150), [S153](06-source-content-walk.md#s153), [S160](06-source-content-walk.md#s160) |
| <a id="p50-5"></a>[P50.5](cards/P50-graph-dependencies.md#s-5) — E50 average/last не treatment effect | [S150](06-source-content-walk.md#s150), [S153](06-source-content-walk.md#s153), [S160](06-source-content-walk.md#s160) |
| <a id="p51-1"></a>[P51.1](cards/P51-translation-context.md#s-1) — TopicDomain включён в ключ | [S051](06-source-content-walk.md#s051), [S052](06-source-content-walk.md#s052), [S058](06-source-content-walk.md#s058), [S059](06-source-content-walk.md#s059) |
| <a id="p51-2"></a>[P51.2](cards/P51-translation-context.md#s-2) — Направление включено в namespace ключа | [S052](06-source-content-walk.md#s052) |
| <a id="p51-3"></a>[P51.3](cards/P51-translation-context.md#s-3) — Сохранение значимого регистра | Выведенный контроль/краевой подслучай механизма карточки; отдельный дефект H этим не утверждается; A/B и предел даны в самой карточке |
| <a id="p51-4"></a>[P51.4](cards/P51-translation-context.md#s-4) — NFC до нормализации | [S175](06-source-content-walk.md#s175), [S182](06-source-content-walk.md#s182), [S183](06-source-content-walk.md#s183), [S184](06-source-content-walk.md#s184), [S193](06-source-content-walk.md#s193), [S360](06-source-content-walk.md#s360) |
| <a id="p51-5"></a>[P51.5](cards/P51-translation-context.md#s-5) — Сохранение операторов | [S051](06-source-content-walk.md#s051), [S060](06-source-content-walk.md#s060), [S061](06-source-content-walk.md#s061) |
| <a id="p51-6"></a>[P51.6](cards/P51-translation-context.md#s-6) — Ложное распознавание mojibake по Ñ | [S175](06-source-content-walk.md#s175), [S182](06-source-content-walk.md#s182), [S183](06-source-content-walk.md#s183), [S184](06-source-content-walk.md#s184), [S193](06-source-content-walk.md#s193) |
| <a id="p51-7"></a>[P51.7](cards/P51-translation-context.md#s-7) — Разные script gates у phrase и обычного кандидата | [S001](06-source-content-walk.md#s001), [S051](06-source-content-walk.md#s051), [S175](06-source-content-walk.md#s175), [S182](06-source-content-walk.md#s182), [S183](06-source-content-walk.md#s183), [S184](06-source-content-walk.md#s184), [S186](06-source-content-walk.md#s186) |
| <a id="p51-8"></a>[P51.8](cards/P51-translation-context.md#s-8) — Язык источника входит в ключ tokenizer correction | [S051](06-source-content-walk.md#s051), [S112](06-source-content-walk.md#s112), [S122](06-source-content-walk.md#s122), [S125](06-source-content-walk.md#s125), [S133](06-source-content-walk.md#s133), [S134](06-source-content-walk.md#s134), [S143](06-source-content-walk.md#s143) |
| <a id="p51-9"></a>[P51.9](cards/P51-translation-context.md#s-9) — Пробел против TAB/LF/NBSP в semantic tokenizer | [S085](06-source-content-walk.md#s085), [S094](06-source-content-walk.md#s094), [S095](06-source-content-walk.md#s095) |
| <a id="p51-10"></a>[P51.10](cards/P51-translation-context.md#s-10) — Код языка нормализуется роутером, но не tokenizer | [S100](06-source-content-walk.md#s100), [S102](06-source-content-walk.md#s102), [S103](06-source-content-walk.md#s103), [S111](06-source-content-walk.md#s111) |
| <a id="p51-11"></a>[P51.11](cards/P51-translation-context.md#s-11) — RuleTags сохраняются после signature correction | [S122](06-source-content-walk.md#s122), [S125](06-source-content-walk.md#s125), [S133](06-source-content-walk.md#s133), [S134](06-source-content-walk.md#s134), [S143](06-source-content-walk.md#s143) |
| <a id="p52-1"></a>[P52.1](cards/P52-semantic-annotation.md#s-1) — RuleTags из rejected против выбранного корректного side | [S022](06-source-content-walk.md#s022), [S199](06-source-content-walk.md#s199), [S200](06-source-content-walk.md#s200), [S202](06-source-content-walk.md#s202), [S272](06-source-content-walk.md#s272), [S360](06-source-content-walk.md#s360) |
| <a id="p52-2"></a>[P52.2](cards/P52-semantic-annotation.md#s-2) — First-word pseudoalignment | [S199](06-source-content-walk.md#s199), [S200](06-source-content-walk.md#s200), [S201](06-source-content-walk.md#s201) |
| <a id="p52-3"></a>[P52.3](cards/P52-semantic-annotation.md#s-3) — Парсер ролей concept/context в verifier issue | [S177](06-source-content-walk.md#s177), [S194](06-source-content-walk.md#s194), [S197](06-source-content-walk.md#s197), [S198](06-source-content-walk.md#s198) |
| <a id="p52-4"></a>[P52.4](cards/P52-semantic-annotation.md#s-4) — Base tokenizer против обученного tokenizer producer | Дополнительный подслучай прежнего каталога O; H не назван его прямым основанием: [13-domain-context-and-dpo-subunits.md:39](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/13-domain-context-and-dpo-subunits.md:39>) |
| <a id="p52-5"></a>[P52.5](cards/P52-semantic-annotation.md#s-5) — Диагностический marker в model-answer bytes | Дополнительный подслучай прежнего каталога O; H не назван его прямым основанием: [13-domain-context-and-dpo-subunits.md:39](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/13-domain-context-and-dpo-subunits.md:39>) |
| <a id="p52-6"></a>[P52.6](cards/P52-semantic-annotation.md#s-6) — Train-only RuleTags против доступного runtime ключа | [S086](06-source-content-walk.md#s086), [S089](06-source-content-walk.md#s089), [S202](06-source-content-walk.md#s202) |
| <a id="p52-7"></a>[P52.7](cards/P52-semantic-annotation.md#s-7) — Более ранняя marker-эвристика маскирует positional ablation | [S201](06-source-content-walk.md#s201) |
| <a id="p52-8"></a>[P52.8](cards/P52-semantic-annotation.md#s-8) — Builder создаёт одинаковый rejected text и неверный verifier label | [S086](06-source-content-walk.md#s086), [S089](06-source-content-walk.md#s089) |
| <a id="p52-9"></a>[P52.9](cards/P52-semantic-annotation.md#s-9) — FrequencyQuote TargetPrefix как неподтверждённый перевод | [S148](06-source-content-walk.md#s148), [S158](06-source-content-walk.md#s158), [S163](06-source-content-walk.md#s163), [S169](06-source-content-walk.md#s169), [S170](06-source-content-walk.md#s170) |
| <a id="p52-10"></a>[P52.10](cards/P52-semantic-annotation.md#s-10) — FrequencyQuote rejected совпадает с chosen | [S148](06-source-content-walk.md#s148), [S158](06-source-content-walk.md#s158), [S163](06-source-content-walk.md#s163), [S169](06-source-content-walk.md#s169), [S170](06-source-content-walk.md#s170) |
| <a id="p53-1"></a>[P53.1](cards/P53-batch-atomicity.md#s-1) — Частичный batch и атомарность | [S001](06-source-content-walk.md#s001), [S022](06-source-content-walk.md#s022), [S112](06-source-content-walk.md#s112), [S116](06-source-content-walk.md#s116), [S253](06-source-content-walk.md#s253), [S360](06-source-content-walk.md#s360) |
| <a id="p53-2"></a>[P53.2](cards/P53-batch-atomicity.md#s-2) — Unknown Unit dispatch | [S050](06-source-content-walk.md#s050), [S057](06-source-content-walk.md#s057), [S066](06-source-content-walk.md#s066), [S068](06-source-content-walk.md#s068) |
| <a id="p53-3"></a>[P53.3](cards/P53-batch-atomicity.md#s-3) — Readiness promotion без реального update | [S050](06-source-content-walk.md#s050), [S051](06-source-content-walk.md#s051), [S057](06-source-content-walk.md#s057), [S066](06-source-content-walk.md#s066) |
| <a id="p53-4"></a>[P53.4](cards/P53-batch-atomicity.md#s-4) — Повтор ExampleId проверяется до mutation | [S051](06-source-content-walk.md#s051), [S151](06-source-content-walk.md#s151), [S165](06-source-content-walk.md#s165) |
| <a id="p53-5"></a>[P53.5](cards/P53-batch-atomicity.md#s-5) — FrequencyQuote builder выбирает существующий Unit чужого направления | [S093](06-source-content-walk.md#s093), [S096](06-source-content-walk.md#s096), [S158](06-source-content-walk.md#s158), [S163](06-source-content-walk.md#s163) |
| <a id="p54-1"></a>[P54.1](cards/P54-phrase-composition.md#s-1) — Greedy phrase segmentation | [S032](06-source-content-walk.md#s032), [S033](06-source-content-walk.md#s033), [S206](06-source-content-walk.md#s206), [S208](06-source-content-walk.md#s208), [S211](06-source-content-walk.md#s211), [S219](06-source-content-walk.md#s219), [S220](06-source-content-walk.md#s220), [S360](06-source-content-walk.md#s360) |
| <a id="p54-2"></a>[P54.2](cards/P54-phrase-composition.md#s-2) — Filtered ordinal против original index | [S234](06-source-content-walk.md#s234), [S242](06-source-content-walk.md#s242), [S243](06-source-content-walk.md#s243), [S244](06-source-content-walk.md#s244), [S253](06-source-content-walk.md#s253) |
| <a id="p54-3"></a>[P54.3](cards/P54-phrase-composition.md#s-3) — Unknown обязательное слово не исчезает | [S032](06-source-content-walk.md#s032), [S033](06-source-content-walk.md#s033), [S085](06-source-content-walk.md#s085), [S094](06-source-content-walk.md#s094), [S095](06-source-content-walk.md#s095) |
| <a id="p54-4"></a>[P54.4](cards/P54-phrase-composition.md#s-4) — Удалённый разделитель создаёт ложное соседство | [S206](06-source-content-walk.md#s206), [S208](06-source-content-walk.md#s208), [S211](06-source-content-walk.md#s211), [S219](06-source-content-walk.md#s219), [S220](06-source-content-walk.md#s220), [S242](06-source-content-walk.md#s242) |
| <a id="p54-5"></a>[P54.5](cards/P54-phrase-composition.md#s-5) — Word-map omission отдельно от phrase composition | [S085](06-source-content-walk.md#s085), [S094](06-source-content-walk.md#s094), [S268](06-source-content-walk.md#s268), [S276](06-source-content-walk.md#s276), [S282](06-source-content-walk.md#s282), [S286](06-source-content-walk.md#s286) |
| <a id="p54-6"></a>[P54.6](cards/P54-phrase-composition.md#s-6) — Полная композиция и независимый law rerank | [S206](06-source-content-walk.md#s206), [S208](06-source-content-walk.md#s208), [S211](06-source-content-walk.md#s211), [S219](06-source-content-walk.md#s219), [S220](06-source-content-walk.md#s220) |
| <a id="p54-7"></a>[P54.7](cards/P54-phrase-composition.md#s-7) — Word-map прерывает поиск до проверки известного противоречия | [S303](06-source-content-walk.md#s303) |
| <a id="p55-1"></a>[P55.1](cards/P55-translation-laws.md#s-1) — Negation can't | [S032](06-source-content-walk.md#s032), [S033](06-source-content-walk.md#s033), [S149](06-source-content-walk.md#s149), [S155](06-source-content-walk.md#s155), [S164](06-source-content-walk.md#s164), [S167](06-source-content-walk.md#s167), [S168](06-source-content-walk.md#s168), [S296](06-source-content-walk.md#s296), [S320](06-source-content-walk.md#s320), [S360](06-source-content-walk.md#s360) |
| <a id="p55-2"></a>[P55.2](cards/P55-translation-laws.md#s-2) — I/you и actor roles | Дополнительный подслучай прежнего каталога O; H не назван его прямым основанием: [72-negation-confidence-and-e46-feedback-consumers.md:1](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/72-negation-confidence-and-e46-feedback-consumers.md:1>) |
| <a id="p55-3"></a>[P55.3](cards/P55-translation-laws.md#s-3) — Gender при заданном контексте | [S148](06-source-content-walk.md#s148), [S158](06-source-content-walk.md#s158), [S163](06-source-content-walk.md#s163), [S169](06-source-content-walk.md#s169), [S170](06-source-content-walk.md#s170) |
| <a id="p55-4"></a>[P55.4](cards/P55-translation-laws.md#s-4) — SourceLaw narrow coverage | [S164](06-source-content-walk.md#s164) |
| <a id="p55-5"></a>[P55.5](cards/P55-translation-laws.md#s-5) — Лицо и actor теряются в eligibility guard | [S139](06-source-content-walk.md#s139) |
| <a id="p55-6"></a>[P55.6](cards/P55-translation-laws.md#s-6) — FrequencyQuote теряет GrammaticalRole при projection | [S148](06-source-content-walk.md#s148), [S158](06-source-content-walk.md#s158), [S163](06-source-content-walk.md#s163), [S169](06-source-content-walk.md#s169), [S170](06-source-content-walk.md#s170) |
| <a id="p56-1"></a>[P56.1](cards/P56-verifier-context.md#s-1) — Применение rule только в своём context | [S086](06-source-content-walk.md#s086), [S089](06-source-content-walk.md#s089), [S177](06-source-content-walk.md#s177), [S194](06-source-content-walk.md#s194), [S197](06-source-content-walk.md#s197), [S198](06-source-content-walk.md#s198), [S360](06-source-content-walk.md#s360) |
| <a id="p56-2"></a>[P56.2](cards/P56-verifier-context.md#s-2) — Противоречащие версии required terms | Выведенный контроль/краевой подслучай механизма карточки; отдельный дефект H этим не утверждается; A/B и предел даны в самой карточке |
| <a id="p56-3"></a>[P56.3](cards/P56-verifier-context.md#s-3) — No-rule confidence .99 против upstream confidence | [S276](06-source-content-walk.md#s276), [S282](06-source-content-walk.md#s282), [S286](06-source-content-walk.md#s286) |
| <a id="p56-4"></a>[P56.4](cards/P56-verifier-context.md#s-4) — Pass и Threshold в actual caller | [S051](06-source-content-walk.md#s051), [S085](06-source-content-walk.md#s085), [S113](06-source-content-walk.md#s113), [S118](06-source-content-walk.md#s118), [S154](06-source-content-walk.md#s154), [S194](06-source-content-walk.md#s194), [S242](06-source-content-walk.md#s242), [S253](06-source-content-walk.md#s253) |
| <a id="p56-5"></a>[P56.5](cards/P56-verifier-context.md#s-5) — Сохранение сигнала novelty до verifier | [S286](06-source-content-walk.md#s286), [S290](06-source-content-walk.md#s290), [S294](06-source-content-walk.md#s294), [S323](06-source-content-walk.md#s323), [S325](06-source-content-walk.md#s325) |
| <a id="p56-6"></a>[P56.6](cards/P56-verifier-context.md#s-6) — Exact-source answer проходит target-language validation | [S051](06-source-content-walk.md#s051) |
| <a id="p56-7"></a>[P56.7](cards/P56-verifier-context.md#s-7) — Required whole token против Contains substring | [S086](06-source-content-walk.md#s086), [S089](06-source-content-walk.md#s089) |
| <a id="p56-8"></a>[P56.8](cards/P56-verifier-context.md#s-8) — Независимые законные requirements ошибочно объединены AND | [S113](06-source-content-walk.md#s113), [S118](06-source-content-walk.md#s118) |
| <a id="p56-9"></a>[P56.9](cards/P56-verifier-context.md#s-9) — Общий verifier по направлениям против direction namespace | [S113](06-source-content-walk.md#s113), [S118](06-source-content-walk.md#s118) |
| <a id="p56-10"></a>[P56.10](cards/P56-verifier-context.md#s-10) — Выученное required word не выражает полярность закона | [S135](06-source-content-walk.md#s135), [S136](06-source-content-walk.md#s136), [S137](06-source-content-walk.md#s137), [S139](06-source-content-walk.md#s139) |
| <a id="p56-11"></a>[P56.11](cards/P56-verifier-context.md#s-11) — Язык source marker при обучении Verifier | [S190](06-source-content-walk.md#s190) |
| <a id="p57-1"></a>[P57.1](cards/P57-semantic-measurement.md#s-1) — Surface scorer против предметной рубрики | [S001](06-source-content-walk.md#s001), [S040](06-source-content-walk.md#s040), [S101](06-source-content-walk.md#s101), [S106](06-source-content-walk.md#s106), [S107](06-source-content-walk.md#s107), [S119](06-source-content-walk.md#s119), [S124](06-source-content-walk.md#s124), [S181](06-source-content-walk.md#s181), [S191](06-source-content-walk.md#s191), [S195](06-source-content-walk.md#s195), [S196](06-source-content-walk.md#s196), [S286](06-source-content-walk.md#s286), [S360](06-source-content-walk.md#s360) |
| <a id="p57-2"></a>[P57.2](cards/P57-semantic-measurement.md#s-2) — UTF16 и Unicode codepoint единицы | Выведенный контроль/краевой подслучай механизма карточки; отдельный дефект H этим не утверждается; A/B и предел даны в самой карточке |
| <a id="p57-3"></a>[P57.3](cards/P57-semantic-measurement.md#s-3) — Macro Unit и micro request aggregation | [S050](06-source-content-walk.md#s050), [S190](06-source-content-walk.md#s190), [S245](06-source-content-walk.md#s245), [S257](06-source-content-walk.md#s257), [S258](06-source-content-walk.md#s258) |
| <a id="p57-4"></a>[P57.4](cards/P57-semantic-measurement.md#s-4) — Старый AccuracyBefore и актуальная панель | Выведенный контроль/краевой подслучай механизма карточки; отдельный дефект H этим не утверждается; A/B и предел даны в самой карточке |
| <a id="p57-5"></a>[P57.5](cards/P57-semantic-measurement.md#s-5) — Coverage без heldout | [S050](06-source-content-walk.md#s050), [S051](06-source-content-walk.md#s051), [S057](06-source-content-walk.md#s057) |
| <a id="p57-6"></a>[P57.6](cards/P57-semantic-measurement.md#s-6) — Verifier gold только отрицательного класса | [S151](06-source-content-walk.md#s151), [S154](06-source-content-walk.md#s154), [S166](06-source-content-walk.md#s166), [S179](06-source-content-walk.md#s179), [S180](06-source-content-walk.md#s180), [S185](06-source-content-walk.md#s185), [S190](06-source-content-walk.md#s190) |
| <a id="p57-7"></a>[P57.7](cards/P57-semantic-measurement.md#s-7) — Tokenizer self-label как gold | [S151](06-source-content-walk.md#s151), [S166](06-source-content-walk.md#s166) |
| <a id="p57-8"></a>[P57.8](cards/P57-semantic-measurement.md#s-8) — Evaluation route использует фактическое направление | [S050](06-source-content-walk.md#s050), [S057](06-source-content-walk.md#s057), [S095](06-source-content-walk.md#s095), [S165](06-source-content-walk.md#s165), [S212](06-source-content-walk.md#s212) |
| <a id="p57-9"></a>[P57.9](cards/P57-semantic-measurement.md#s-9) — TargetScriptOk: Hebrew и mixed identifiers | [S101](06-source-content-walk.md#s101), [S106](06-source-content-walk.md#s106), [S107](06-source-content-walk.md#s107) |
| <a id="p57-10"></a>[P57.10](cards/P57-semantic-measurement.md#s-10) — Direction-specific UnitCoverage вместо RU-only списка | [S139](06-source-content-walk.md#s139) |
| <a id="p57-11"></a>[P57.11](cards/P57-semantic-measurement.md#s-11) — Verifier MeasureAccuracy не должен принудительно выбирать RU | [S151](06-source-content-walk.md#s151), [S154](06-source-content-walk.md#s154), [S166](06-source-content-walk.md#s166), [S179](06-source-content-walk.md#s179), [S180](06-source-content-walk.md#s180), [S185](06-source-content-walk.md#s185), [S190](06-source-content-walk.md#s190) |
| <a id="p57-12"></a>[P57.12](cards/P57-semantic-measurement.md#s-12) — Tokenizer accuracy по Pattern скрывает неверную Signature | [S151](06-source-content-walk.md#s151), [S166](06-source-content-walk.md#s166) |
| <a id="p57-13"></a>[P57.13](cards/P57-semantic-measurement.md#s-13) — Executor held-out использует тот же semantic extractor | [S179](06-source-content-walk.md#s179), [S180](06-source-content-walk.md#s180), [S185](06-source-content-walk.md#s185), [S190](06-source-content-walk.md#s190) |
| <a id="p57-14"></a>[P57.14](cards/P57-semantic-measurement.md#s-14) — Пустые marker lists и знаменатель сохранности | [S181](06-source-content-walk.md#s181), [S191](06-source-content-walk.md#s191), [S195](06-source-content-walk.md#s195), [S196](06-source-content-walk.md#s196) |
| <a id="p57-15"></a>[P57.15](cards/P57-semantic-measurement.md#s-15) — Диагностические bag scores не означают сохранение смысла | [S181](06-source-content-walk.md#s181), [S191](06-source-content-walk.md#s191), [S195](06-source-content-walk.md#s195), [S196](06-source-content-walk.md#s196) |
| <a id="p57-16"></a>[P57.16](cards/P57-semantic-measurement.md#s-16) — Потолок качества и обязательный абсолютный прирост | [S181](06-source-content-walk.md#s181), [S191](06-source-content-walk.md#s191), [S195](06-source-content-walk.md#s195), [S196](06-source-content-walk.md#s196) |
| <a id="p58-1"></a>[P58.1](cards/P58-feedback-learning.md#s-1) — Lesson строится из actual wrong output | [S002](06-source-content-walk.md#s002), [S003](06-source-content-walk.md#s003), [S088](06-source-content-walk.md#s088), [S092](06-source-content-walk.md#s092), [S284](06-source-content-walk.md#s284) |
| <a id="p58-2"></a>[P58.2](cards/P58-feedback-learning.md#s-2) — Premature promotion | [S003](06-source-content-walk.md#s003), [S119](06-source-content-walk.md#s119), [S284](06-source-content-walk.md#s284) |
| <a id="p58-3"></a>[P58.3](cards/P58-feedback-learning.md#s-3) — Legacy +0.01 без unit update | [S001](06-source-content-walk.md#s001), [S011](06-source-content-walk.md#s011), [S020](06-source-content-walk.md#s020), [S022](06-source-content-walk.md#s022), [S026](06-source-content-walk.md#s026), [S027](06-source-content-walk.md#s027), [S028](06-source-content-walk.md#s028), [S040](06-source-content-walk.md#s040), [S085](06-source-content-walk.md#s085), [S253](06-source-content-walk.md#s253), [S284](06-source-content-walk.md#s284) |
| <a id="p58-4"></a>[P58.4](cards/P58-feedback-learning.md#s-4) — Оценщик меняет выбор следующих уроков | [S119](06-source-content-walk.md#s119), [S147](06-source-content-walk.md#s147) |
| <a id="p59-1"></a>[P59.1](cards/P59-correction-visibility.md#s-1) — Новый корректирующий урок после cold restore | [S022](06-source-content-walk.md#s022), [S360](06-source-content-walk.md#s360) |
| <a id="p59-2"></a>[P59.2](cards/P59-correction-visibility.md#s-2) — Видимость поздней correction при прежнем top-k | [S022](06-source-content-walk.md#s022), [S050](06-source-content-walk.md#s050), [S052](06-source-content-walk.md#s052), [S057](06-source-content-walk.md#s057) |
| <a id="p59-3"></a>[P59.3](cards/P59-correction-visibility.md#s-3) — Слепой recency-first: видимость коррекции и вред от newest error | [S058](06-source-content-walk.md#s058), [S059](06-source-content-walk.md#s059) |
| <a id="p60-1"></a>[P60.1](cards/P60-signature-duplicate.md#s-1) — Одинаковые full/prefix signature создают два slot | [S234](06-source-content-walk.md#s234), [S263](06-source-content-walk.md#s263), [S264](06-source-content-walk.md#s264), [S266](06-source-content-walk.md#s266) |
| <a id="p61-1"></a>[P61.1](cards/P61-unknown-tail.md#s-1) — UNKNOWN смысловой хвост | [S307](06-source-content-walk.md#s307), [S320](06-source-content-walk.md#s320) |
| <a id="p61-2"></a>[P61.2](cards/P61-unknown-tail.md#s-2) — True/false исчезают из signature | [S307](06-source-content-walk.md#s307), [S320](06-source-content-walk.md#s320), [S324](06-source-content-walk.md#s324) |
| <a id="p62-1"></a>[P62.1](cards/P62-signature-monotonicity.md#s-1) — Добавленный RuleTag ослабляет guard threshold | [S122](06-source-content-walk.md#s122), [S292](06-source-content-walk.md#s292), [S294](06-source-content-walk.md#s294), [S298](06-source-content-walk.md#s298), [S318](06-source-content-walk.md#s318), [S320](06-source-content-walk.md#s320), [S329](06-source-content-walk.md#s329) |
| <a id="p63-1"></a>[P63.1](cards/P63-source-containment.md#s-1) — Одностороннее containment теряет ограничение source | [S164](06-source-content-walk.md#s164), [S292](06-source-content-walk.md#s292), [S294](06-source-content-walk.md#s294), [S295](06-source-content-walk.md#s295), [S296](06-source-content-walk.md#s296), [S298](06-source-content-walk.md#s298), [S307](06-source-content-walk.md#s307), [S318](06-source-content-walk.md#s318), [S320](06-source-content-walk.md#s320), [S329](06-source-content-walk.md#s329) |
| <a id="p63-2"></a>[P63.2](cards/P63-source-containment.md#s-2) — Stopword not скрывает полярность вне узкого SourceLaw | [S051](06-source-content-walk.md#s051), [S296](06-source-content-walk.md#s296), [S320](06-source-content-walk.md#s320) |
| <a id="p63-3"></a>[P63.3](cards/P63-source-containment.md#s-3) — Отрицательное contraction до удаления апострофа | [S206](06-source-content-walk.md#s206), [S208](06-source-content-walk.md#s208), [S211](06-source-content-walk.md#s211), [S219](06-source-content-walk.md#s219), [S220](06-source-content-walk.md#s220) |
| <a id="p63-4"></a>[P63.4](cards/P63-source-containment.md#s-4) — Дополнительное обязательство source отдельно от lexical score | [S298](06-source-content-walk.md#s298) |
| <a id="p64-1"></a>[P64.1](cards/P64-tier-postfilter.md#s-1) — Early nonempty tier закрывает путь до target filter | [S022](06-source-content-walk.md#s022), [S050](06-source-content-walk.md#s050), [S282](06-source-content-walk.md#s282), [S292](06-source-content-walk.md#s292), [S294](06-source-content-walk.md#s294), [S295](06-source-content-walk.md#s295), [S296](06-source-content-walk.md#s296), [S298](06-source-content-walk.md#s298), [S307](06-source-content-walk.md#s307), [S318](06-source-content-walk.md#s318), [S320](06-source-content-walk.md#s320), [S329](06-source-content-walk.md#s329), [S360](06-source-content-walk.md#s360) |
| <a id="p65-1"></a>[P65.1](cards/P65-history-admission.md#s-1) — History Jaccard0.70 отбрасывает новый закон | [S086](06-source-content-walk.md#s086), [S088](06-source-content-walk.md#s088), [S089](06-source-content-walk.md#s089), [S092](06-source-content-walk.md#s092), [S303](06-source-content-walk.md#s303), [S304](06-source-content-walk.md#s304), [S322](06-source-content-walk.md#s322), [S325](06-source-content-walk.md#s325), [S326](06-source-content-walk.md#s326), [S327](06-source-content-walk.md#s327), [S328](06-source-content-walk.md#s328), [S360](06-source-content-walk.md#s360) |
| <a id="p65-2"></a>[P65.2](cards/P65-history-admission.md#s-2) — RU→EN negation/operators/numbers и lexical normalization | [S304](06-source-content-walk.md#s304), [S322](06-source-content-walk.md#s322), [S327](06-source-content-walk.md#s327), [S328](06-source-content-walk.md#s328) |
| <a id="p65-3"></a>[P65.3](cards/P65-history-admission.md#s-3) — Operator-aware set при том же Jaccard threshold | [S090](06-source-content-walk.md#s090), [S091](06-source-content-walk.md#s091), [S093](06-source-content-walk.md#s093), [S096](06-source-content-walk.md#s096), [S103](06-source-content-walk.md#s103) |
| <a id="p66-1"></a>[P66.1](cards/P66-heldout-contrast.md#s-1) — True leakage против законного semantic contrast heldout | [S093](06-source-content-walk.md#s093), [S096](06-source-content-walk.md#s096), [S304](06-source-content-walk.md#s304), [S322](06-source-content-walk.md#s322), [S325](06-source-content-walk.md#s325), [S326](06-source-content-walk.md#s326), [S327](06-source-content-walk.md#s327), [S328](06-source-content-walk.md#s328) |
| <a id="p67-1"></a>[P67.1](cards/P67-correction-identity.md#s-1) — Supplemental correction last writer | [S051](06-source-content-walk.md#s051), [S060](06-source-content-walk.md#s060), [S061](06-source-content-walk.md#s061), [S304](06-source-content-walk.md#s304), [S322](06-source-content-walk.md#s322), [S325](06-source-content-walk.md#s325), [S327](06-source-content-walk.md#s327), [S328](06-source-content-walk.md#s328), [S360](06-source-content-walk.md#s360) |
| <a id="p67-2"></a>[P67.2](cards/P67-correction-identity.md#s-2) — PairId revision против applied-once identity | [S177](06-source-content-walk.md#s177), [S192](06-source-content-walk.md#s192) |
| <a id="p68-1"></a>[P68.1](cards/P68-pretrain-pipeline.md#s-1) — Stride и повторное предъявление позиций | [S265](06-source-content-walk.md#s265), [S267](06-source-content-walk.md#s267), [S269](06-source-content-walk.md#s269), [S270](06-source-content-walk.md#s270), [S271](06-source-content-walk.md#s271), [S360](06-source-content-walk.md#s360) |
| <a id="p68-2"></a>[P68.2](cards/P68-pretrain-pipeline.md#s-2) — EOD: byte1 против выделенного маркера | [S265](06-source-content-walk.md#s265), [S267](06-source-content-walk.md#s267), [S269](06-source-content-walk.md#s269), [S270](06-source-content-walk.md#s270), [S271](06-source-content-walk.md#s271) |
| <a id="p68-3"></a>[P68.3](cards/P68-pretrain-pipeline.md#s-3) — Короткий shard не даёт полноценного окна | [S265](06-source-content-walk.md#s265), [S267](06-source-content-walk.md#s267), [S269](06-source-content-walk.md#s269) |
| <a id="p68-4"></a>[P68.4](cards/P68-pretrain-pipeline.md#s-4) — Старые shards при повторной сборке каталога | [S269](06-source-content-walk.md#s269) |
| <a id="p68-5"></a>[P68.5](cards/P68-pretrain-pipeline.md#s-5) — uint16 saturation за пределами текущего vocab629 | [S265](06-source-content-walk.md#s265), [S267](06-source-content-walk.md#s267), [S270](06-source-content-walk.md#s270), [S271](06-source-content-walk.md#s271) |
| <a id="p68-6"></a>[P68.6](cards/P68-pretrain-pipeline.md#s-6) — Межшардовое окно одного документа | [S265](06-source-content-walk.md#s265), [S267](06-source-content-walk.md#s267) |
| <a id="p68-7"></a>[P68.7](cards/P68-pretrain-pipeline.md#s-7) — Детерминированный порядок исходных файлов | [S265](06-source-content-walk.md#s265), [S267](06-source-content-walk.md#s267) |
| <a id="p69-1"></a>[P69.1](cards/P69-graphrag-context.md#s-1) — BPE-prefix cutoff при ordinary ненулевом embedding | [S268](06-source-content-walk.md#s268), [S272](06-source-content-walk.md#s272), [S275](06-source-content-walk.md#s275), [S281](06-source-content-walk.md#s281) |
| <a id="p69-2"></a>[P69.2](cards/P69-graphrag-context.md#s-2) — RawText исключает смысловую Signature/Name | [S272](06-source-content-walk.md#s272), [S275](06-source-content-walk.md#s275), [S281](06-source-content-walk.md#s281) |
| <a id="p69-3"></a>[P69.3](cards/P69-graphrag-context.md#s-3) — Пустые seeds создают глобальные результаты | [S275](06-source-content-walk.md#s275), [S287](06-source-content-walk.md#s287), [S288](06-source-content-walk.md#s288), [S289](06-source-content-walk.md#s289), [S291](06-source-content-walk.md#s291) |
| <a id="p69-4"></a>[P69.4](cards/P69-graphrag-context.md#s-4) — Stale seed IDs без существующего узла | [S275](06-source-content-walk.md#s275), [S287](06-source-content-walk.md#s287), [S288](06-source-content-walk.md#s288), [S289](06-source-content-walk.md#s289), [S291](06-source-content-walk.md#s291) |
| <a id="p69-5"></a>[P69.5](cards/P69-graphrag-context.md#s-5) — Context packing break пропускает later-fit item | [S187](06-source-content-walk.md#s187), [S275](06-source-content-walk.md#s275) |
| <a id="p69-6"></a>[P69.6](cards/P69-graphrag-context.md#s-6) — Скрытый hash fallback вместо learned embedding | [S272](06-source-content-walk.md#s272), [S275](06-source-content-walk.md#s275), [S281](06-source-content-walk.md#s281) |
| <a id="p69-7"></a>[P69.7](cards/P69-graphrag-context.md#s-7) — Символьная оценка token budget | [S187](06-source-content-walk.md#s187) |
| <a id="p69-8"></a>[P69.8](cards/P69-graphrag-context.md#s-8) — Разделители не входят в used budget | [S187](06-source-content-walk.md#s187) |
| <a id="p69-9"></a>[P69.9](cards/P69-graphrag-context.md#s-9) — Zero-rank fillers при наличии одного valid seed | [S288](06-source-content-walk.md#s288), [S289](06-source-content-walk.md#s289), [S291](06-source-content-walk.md#s291) |
| <a id="p70-1"></a>[P70.1](cards/P70-wikitrace-context.md#s-1) — Исходные trace/ответы против шаблона RejectedType | Дополнительный подслучай прежнего каталога O; H не назван его прямым основанием: [76-wiki-trace-donor-input-loss-and-checkpoint-identity.md:29](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/76-wiki-trace-donor-input-loss-and-checkpoint-identity.md:29>), [76-wiki-trace-donor-input-loss-and-checkpoint-identity.md:43](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/76-wiki-trace-donor-input-loss-and-checkpoint-identity.md:43>), [76-wiki-trace-donor-input-loss-and-checkpoint-identity.md:13](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/76-wiki-trace-donor-input-loss-and-checkpoint-identity.md:13>) |
| <a id="p70-2"></a>[P70.2](cards/P70-wikitrace-context.md#s-2) — Neural checkpoint действительно достигает consumer | Дополнительный подслучай прежнего каталога O; H не назван его прямым основанием: [76-wiki-trace-donor-input-loss-and-checkpoint-identity.md:29](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/76-wiki-trace-donor-input-loss-and-checkpoint-identity.md:29>), [76-wiki-trace-donor-input-loss-and-checkpoint-identity.md:43](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/76-wiki-trace-donor-input-loss-and-checkpoint-identity.md:43>), [76-wiki-trace-donor-input-loss-and-checkpoint-identity.md:13](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/76-wiki-trace-donor-input-loss-and-checkpoint-identity.md:13>) |
| <a id="p70-3"></a>[P70.3](cards/P70-wikitrace-context.md#s-3) — Exact train/heldout overlap отдельно от навыка | Дополнительный подслучай прежнего каталога O; H не назван его прямым основанием: [76-wiki-trace-donor-input-loss-and-checkpoint-identity.md:29](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/76-wiki-trace-donor-input-loss-and-checkpoint-identity.md:29>), [76-wiki-trace-donor-input-loss-and-checkpoint-identity.md:43](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/76-wiki-trace-donor-input-loss-and-checkpoint-identity.md:43>), [76-wiki-trace-donor-input-loss-and-checkpoint-identity.md:13](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/76-wiki-trace-donor-input-loss-and-checkpoint-identity.md:13>) |
| <a id="p70-4"></a>[P70.4](cards/P70-wikitrace-context.md#s-4) — Manifest описывает artifact и действительные units | Дополнительный подслучай прежнего каталога O; H не назван его прямым основанием: [76-wiki-trace-donor-input-loss-and-checkpoint-identity.md:29](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/76-wiki-trace-donor-input-loss-and-checkpoint-identity.md:29>), [76-wiki-trace-donor-input-loss-and-checkpoint-identity.md:43](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/76-wiki-trace-donor-input-loss-and-checkpoint-identity.md:43>), [76-wiki-trace-donor-input-loss-and-checkpoint-identity.md:13](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/76-wiki-trace-donor-input-loss-and-checkpoint-identity.md:13>) |
| <a id="p71-1"></a>[P71.1](cards/P71-perplexity-alphabet.md#s-1) — E17 PPL разных event alphabets | [S002](06-source-content-walk.md#s002), [S005](06-source-content-walk.md#s005), [S293](06-source-content-walk.md#s293), [S297](06-source-content-walk.md#s297), [S310](06-source-content-walk.md#s310), [S311](06-source-content-walk.md#s311), [S312](06-source-content-walk.md#s312) |
| <a id="p71-2"></a>[P71.2](cards/P71-perplexity-alphabet.md#s-2) — Level3 fallback должен быть видим | [S312](06-source-content-walk.md#s312) |
| <a id="p71-3"></a>[P71.3](cards/P71-perplexity-alphabet.md#s-3) — Одинаковые source-family панели для Level2/3 | [S312](06-source-content-walk.md#s312) |
| <a id="p72-1"></a>[P72.1](cards/P72-project-conditioning.md#s-1) — Train-only project conditioning | [S293](06-source-content-walk.md#s293), [S297](06-source-content-walk.md#s297), [S314](06-source-content-walk.md#s314) |
| <a id="p72-2"></a>[P72.2](cards/P72-project-conditioning.md#s-2) — AllFiles leakage и contaminated fallback | [S293](06-source-content-walk.md#s293), [S297](06-source-content-walk.md#s297), [S310](06-source-content-walk.md#s310), [S311](06-source-content-walk.md#s311), [S314](06-source-content-walk.md#s314) |
| <a id="p72-3"></a>[P72.3](cards/P72-project-conditioning.md#s-3) — Cross-file artificial edges | [S293](06-source-content-walk.md#s293), [S310](06-source-content-walk.md#s310), [S311](06-source-content-walk.md#s311), [S314](06-source-content-walk.md#s314) |
| <a id="p72-4"></a>[P72.4](cards/P72-project-conditioning.md#s-4) — Global train-only fallback | [S314](06-source-content-walk.md#s314) |
| <a id="p72-5"></a>[P72.5](cards/P72-project-conditioning.md#s-5) — Project identity не определяется первыми 60% списка | [S314](06-source-content-walk.md#s314) |
| <a id="p73-1"></a>[P73.1](cards/P73-domain-conditioning.md#s-1) — Context входит в предиктор | [S002](06-source-content-walk.md#s002), [S005](06-source-content-walk.md#s005), [S293](06-source-content-walk.md#s293), [S297](06-source-content-walk.md#s297), [S310](06-source-content-walk.md#s310), [S311](06-source-content-walk.md#s311), [S315](06-source-content-walk.md#s315) |
| <a id="p73-2"></a>[P73.2](cards/P73-domain-conditioning.md#s-2) — Порядок grouped/shuffled без context feature | [S002](06-source-content-walk.md#s002), [S005](06-source-content-walk.md#s005), [S315](06-source-content-walk.md#s315) |
| <a id="p73-3"></a>[P73.3](cards/P73-domain-conditioning.md#s-3) — Basename label против фактического контекста | [S315](06-source-content-walk.md#s315) |
| <a id="p74-1"></a>[P74.1](cards/P74-memory-eviction.md#s-1) — Marker coverage против доступного правильного закона | [S293](06-source-content-walk.md#s293), [S297](06-source-content-walk.md#s297), [S310](06-source-content-walk.md#s310), [S311](06-source-content-walk.md#s311), [S316](06-source-content-walk.md#s316) |
| <a id="p74-2"></a>[P74.2](cards/P74-memory-eviction.md#s-2) — FIFO против value eviction | [S316](06-source-content-walk.md#s316) |
| <a id="p74-3"></a>[P74.3](cards/P74-memory-eviction.md#s-3) — Synthetic trust не смешивается с context identity | [S316](06-source-content-walk.md#s316) |
| <a id="p75-1"></a>[P75.1](cards/P75-sampler-residual.md#s-1) — Float32 residual выбирает masked last token | [S305](06-source-content-walk.md#s305), [S306](06-source-content-walk.md#s306), [S308](06-source-content-walk.md#s308), [S317](06-source-content-walk.md#s317), [S360](06-source-content-walk.md#s360) |
| <a id="p75-2"></a>[P75.2](cards/P75-sampler-residual.md#s-2) — RNG representability и реальная частота | [S306](06-source-content-walk.md#s306), [S308](06-source-content-walk.md#s308), [S317](06-source-content-walk.md#s317) |
| <a id="p75-3"></a>[P75.3](cards/P75-sampler-residual.md#s-3) — r=0 и ведущая нулевая probability | [S306](06-source-content-walk.md#s306), [S317](06-source-content-walk.md#s317) |
| <a id="p76-1"></a>[P76.1](cards/P76-utf8-boundary.md#s-1) — Обрезать только incomplete UTF8 tail | [S237](06-source-content-walk.md#s237), [S240](06-source-content-walk.md#s240), [S246](06-source-content-walk.md#s246), [S254](06-source-content-walk.md#s254) |
| <a id="p76-2"></a>[P76.2](cards/P76-utf8-boundary.md#s-2) — Не скрывать interior invalid bytes и literal U+FFFD | [S240](06-source-content-walk.md#s240), [S246](06-source-content-walk.md#s246), [S254](06-source-content-walk.md#s254) |
| <a id="p76-3"></a>[P76.3](cards/P76-utf8-boundary.md#s-3) — Завершить scalar реальным продолжением | [S246](06-source-content-walk.md#s246), [S254](06-source-content-walk.md#s254) |
| <a id="p77-1"></a>[P77.1](cards/P77-rare-class-support.md#s-1) — Rare law отсутствует в validation | [S125](06-source-content-walk.md#s125), [S272](06-source-content-walk.md#s272), [S273](06-source-content-walk.md#s273), [S277](06-source-content-walk.md#s277), [S283](06-source-content-walk.md#s283) |
| <a id="p77-2"></a>[P77.2](cards/P77-rare-class-support.md#s-2) — Singleton не копируется между split roles | [S283](06-source-content-walk.md#s283) |
| <a id="p77-3"></a>[P77.3](cards/P77-rare-class-support.md#s-3) — E44 security metric подставляет general score | [S273](06-source-content-walk.md#s273), [S283](06-source-content-walk.md#s283) |
| <a id="p77-4"></a>[P77.4](cards/P77-rare-class-support.md#s-4) — Дополнительный TRAIN lesson при неизменном rare exam | [S283](06-source-content-walk.md#s283) |
| <a id="p78-1"></a>[P78.1](cards/P78-loss-plateau.md#s-1) — Convergence window1 против 5 при fixed max5 | [S274](06-source-content-walk.md#s274), [S280](06-source-content-walk.md#s280), [S285](06-source-content-walk.md#s285), [S360](06-source-content-walk.md#s360) |
| <a id="p79-1"></a>[P79.1](cards/P79-grammar-leakage.md#s-1) — E05 grammar строится только по TRAIN | [S203](06-source-content-walk.md#s203), [S204](06-source-content-walk.md#s204), [S207](06-source-content-walk.md#s207), [S209](06-source-content-walk.md#s209), [S214](06-source-content-walk.md#s214) |
| <a id="p79-2"></a>[P79.2](cards/P79-grammar-leakage.md#s-2) — Grammar mask полезен без утечки | [S203](06-source-content-walk.md#s203), [S214](06-source-content-walk.md#s214) |
| <a id="p80-1"></a>[P80.1](cards/P80-marker-denominator.md#s-1) — Present-marker denominator против шести slots | [S203](06-source-content-walk.md#s203), [S204](06-source-content-walk.md#s204), [S207](06-source-content-walk.md#s207), [S209](06-source-content-walk.md#s209), [S215](06-source-content-walk.md#s215) |
| <a id="p80-2"></a>[P80.2](cards/P80-marker-denominator.md#s-2) — Density отдельно от actual normalized routing | [S203](06-source-content-walk.md#s203), [S215](06-source-content-walk.md#s215) |
| <a id="p81-1"></a>[P81.1](cards/P81-input-conditioning.md#s-1) — Counterfactual prompt меняет sign preference | [S003](06-source-content-walk.md#s003), [S004](06-source-content-walk.md#s004), [S008](06-source-content-walk.md#s008), [S339](06-source-content-walk.md#s339), [S340](06-source-content-walk.md#s340), [S355](06-source-content-walk.md#s355) |
| <a id="p81-2"></a>[P81.2](cards/P81-input-conditioning.md#s-2) — Prompt-free prior control | [S339](06-source-content-walk.md#s339), [S340](06-source-content-walk.md#s340), [S355](06-source-content-walk.md#s355) |
| <a id="p81-3"></a>[P81.3](cards/P81-input-conditioning.md#s-3) — B0/C50 sensitivity отдельно от positive margin | [S339](06-source-content-walk.md#s339), [S340](06-source-content-walk.md#s340), [S355](06-source-content-walk.md#s355) |
| <a id="p82-1"></a>[P82.1](cards/P82-file-split.md#s-1) — E09 within-file split straddles one file | [S125](06-source-content-walk.md#s125), [S349](06-source-content-walk.md#s349) |
| <a id="p83-1"></a>[P83.1](cards/P83-expression-body.md#s-1) — E13 expression body попадает в KindBag | [S334](06-source-content-walk.md#s334), [S335](06-source-content-walk.md#s335), [S337](06-source-content-walk.md#s337), [S343](06-source-content-walk.md#s343), [S350](06-source-content-walk.md#s350) |
| <a id="p84-1"></a>[P84.1](cards/P84-lambda-family.md#s-1) — Simple/parenthesized lambda family | [S334](06-source-content-walk.md#s334), [S335](06-source-content-walk.md#s335), [S337](06-source-content-walk.md#s337), [S343](06-source-content-walk.md#s343), [S351](06-source-content-walk.md#s351) |
| <a id="p84-2"></a>[P84.2](cards/P84-lambda-family.md#s-2) — Lambda arity отдельный признак | [S334](06-source-content-walk.md#s334), [S335](06-source-content-walk.md#s335), [S337](06-source-content-walk.md#s337), [S343](06-source-content-walk.md#s343), [S351](06-source-content-walk.md#s351) |
| <a id="p85-1"></a>[P85.1](cards/P85-composite-obligations.md#s-1) — Composite obligations против single-label credit | [S334](06-source-content-walk.md#s334), [S335](06-source-content-walk.md#s335), [S337](06-source-content-walk.md#s337), [S343](06-source-content-walk.md#s343), [S351](06-source-content-walk.md#s351) |
| <a id="p86-1"></a>[P86.1](cards/P86-fractional-response-mask.md#s-1) — Fractional mask derivative | [S004](06-source-content-walk.md#s004), [S338](06-source-content-walk.md#s338), [S341](06-source-content-walk.md#s341), [S342](06-source-content-walk.md#s342), [S352](06-source-content-walk.md#s352) |
| <a id="p86-2"></a>[P86.2](cards/P86-fractional-response-mask.md#s-2) — Полезность согласованного backward | [S338](06-source-content-walk.md#s338), [S341](06-source-content-walk.md#s341), [S342](06-source-content-walk.md#s342), [S352](06-source-content-walk.md#s352) |
| <a id="p87-1"></a>[P87.1](cards/P87-mixed-objectives.md#s-1) — RP01 profile отвергает mixed objectives до мутации | [S338](06-source-content-walk.md#s338), [S353](06-source-content-walk.md#s353) |
| <a id="p87-2"></a>[P87.2](cards/P87-mixed-objectives.md#s-2) — Generic разрешённый StepMany exact+legacy | [S338](06-source-content-walk.md#s338), [S353](06-source-content-walk.md#s353) |
| <a id="p87-3"></a>[P87.3](cards/P87-mixed-objectives.md#s-3) — Homogeneous objective контроль | [S338](06-source-content-walk.md#s338) |
| <a id="p88-1"></a>[P88.1](cards/P88-unit-context-consumption.md#s-1) — Unit packet content зависит от context | [S002](06-source-content-walk.md#s002), [S347](06-source-content-walk.md#s347), [S348](06-source-content-walk.md#s348), [S356](06-source-content-walk.md#s356), [S359](06-source-content-walk.md#s359) |
| <a id="p88-2"></a>[P88.2](cards/P88-unit-context-consumption.md#s-2) — Confidence0.92 не measured accuracy | [S002](06-source-content-walk.md#s002), [S011](06-source-content-walk.md#s011), [S347](06-source-content-walk.md#s347), [S348](06-source-content-walk.md#s348), [S356](06-source-content-walk.md#s356), [S359](06-source-content-walk.md#s359) |
| <a id="p88-3"></a>[P88.3](cards/P88-unit-context-consumption.md#s-3) — Отсутствующий source запрещает covered packet | [S347](06-source-content-walk.md#s347), [S348](06-source-content-walk.md#s348), [S356](06-source-content-walk.md#s356), [S359](06-source-content-walk.md#s359) |
| <a id="p88-4"></a>[P88.4](cards/P88-unit-context-consumption.md#s-4) — Flow-adapter читает actual request | [S347](06-source-content-walk.md#s347), [S348](06-source-content-walk.md#s348), [S356](06-source-content-walk.md#s356), [S359](06-source-content-walk.md#s359) |
| <a id="p89-1"></a>[P89.1](cards/P89-density-stop.md#s-1) — Loss scale меняет stop на frozen trajectory | [S138](06-source-content-walk.md#s138), [S330](06-source-content-walk.md#s330), [S331](06-source-content-walk.md#s331), [S332](06-source-content-walk.md#s332), [S333](06-source-content-walk.md#s333), [S360](06-source-content-walk.md#s360) |
| <a id="p89-2"></a>[P89.2](cards/P89-density-stop.md#s-2) — Stop читает нормированную loss при прежних gradients | [S330](06-source-content-walk.md#s330), [S331](06-source-content-walk.md#s331), [S332](06-source-content-walk.md#s332), [S333](06-source-content-walk.md#s333) |
| <a id="p90-1"></a>[P90.1](cards/P90-tagged-search.md#s-1) — E47 exact tags | [S357](06-source-content-walk.md#s357), [S358](06-source-content-walk.md#s358), [S359](06-source-content-walk.md#s359) |
| <a id="p90-2"></a>[P90.2](cards/P90-tagged-search.md#s-2) — E47 absent tags и BM25 fallback | [S357](06-source-content-walk.md#s357), [S358](06-source-content-walk.md#s358), [S359](06-source-content-walk.md#s359) |
| <a id="p91-1"></a>[P91.1](cards/P91-method-pair-split.md#s-1) — Одинаковые TRAIN pairs у baseline и kNN | [S002](06-source-content-walk.md#s002), [S065](06-source-content-walk.md#s065), [S124](06-source-content-walk.md#s124), [S337](06-source-content-walk.md#s337), [S360](06-source-content-walk.md#s360) |
| <a id="p91-2"></a>[P91.2](cards/P91-method-pair-split.md#s-2) — E13 source-family independence отдельно | [S065](06-source-content-walk.md#s065) |
| <a id="p92-1"></a>[P92.1](cards/P92-dpo-memory-flow-bridge.md#s-1) — Мост DPO→KnnCorpus→GeneratedCapabilityFlowFactory on/off при frozen Student | [S001](06-source-content-walk.md#s001), [S002](06-source-content-walk.md#s002), [S003](06-source-content-walk.md#s003), [S004](06-source-content-walk.md#s004), [S005](06-source-content-walk.md#s005), [S007](06-source-content-walk.md#s007), [S011](06-source-content-walk.md#s011), [S017](06-source-content-walk.md#s017), [S019](06-source-content-walk.md#s019), [S020](06-source-content-walk.md#s020), [S021](06-source-content-walk.md#s021), [S027](06-source-content-walk.md#s027), [S028](06-source-content-walk.md#s028), [S033](06-source-content-walk.md#s033), [S036](06-source-content-walk.md#s036), [S039](06-source-content-walk.md#s039), [S040](06-source-content-walk.md#s040), [S043](06-source-content-walk.md#s043), [S045](06-source-content-walk.md#s045), [S049](06-source-content-walk.md#s049), [S123](06-source-content-walk.md#s123), [S124](06-source-content-walk.md#s124), [S125](06-source-content-walk.md#s125), [S127](06-source-content-walk.md#s127), [S147](06-source-content-walk.md#s147), [S148](06-source-content-walk.md#s148), [S149](06-source-content-walk.md#s149), [S176](06-source-content-walk.md#s176), [S185](06-source-content-walk.md#s185), [S199](06-source-content-walk.md#s199), [S205](06-source-content-walk.md#s205), [S208](06-source-content-walk.md#s208), [S209](06-source-content-walk.md#s209), [S211](06-source-content-walk.md#s211), [S237](06-source-content-walk.md#s237), [S240](06-source-content-walk.md#s240), [S258](06-source-content-walk.md#s258), [S268](06-source-content-walk.md#s268), [S269](06-source-content-walk.md#s269), [S272](06-source-content-walk.md#s272), [S274](06-source-content-walk.md#s274), [S284](06-source-content-walk.md#s284), [S285](06-source-content-walk.md#s285), [S286](06-source-content-walk.md#s286), [S293](06-source-content-walk.md#s293), [S320](06-source-content-walk.md#s320), [S323](06-source-content-walk.md#s323), [S324](06-source-content-walk.md#s324), [S359](06-source-content-walk.md#s359), [S360](06-source-content-walk.md#s360) |
| <a id="p92-2"></a>[P92.2](cards/P92-dpo-memory-flow-bridge.md#s-2) — Факторный 2×2 Student update × memory bridge | [S001](06-source-content-walk.md#s001), [S003](06-source-content-walk.md#s003), [S011](06-source-content-walk.md#s011), [S021](06-source-content-walk.md#s021), [S043](06-source-content-walk.md#s043), [S354](06-source-content-walk.md#s354), [S359](06-source-content-walk.md#s359) |
| <a id="p92-3"></a>[P92.3](cards/P92-dpo-memory-flow-bridge.md#s-3) — Cold consumer читает изменённую память | [S003](06-source-content-walk.md#s003), [S011](06-source-content-walk.md#s011), [S022](06-source-content-walk.md#s022), [S359](06-source-content-walk.md#s359) |
| <a id="p92-4"></a>[P92.4](cards/P92-dpo-memory-flow-bridge.md#s-4) — Раздельные ProductHost generation и learned Unit: явная композиция | [S066](06-source-content-walk.md#s066), [S070](06-source-content-walk.md#s070), [S071](06-source-content-walk.md#s071), [S077](06-source-content-walk.md#s077), [S078](06-source-content-walk.md#s078), [S079](06-source-content-walk.md#s079), [S092](06-source-content-walk.md#s092), [S103](06-source-content-walk.md#s103) |
| <a id="p93-1"></a>[P93.1](cards/P93-confidence-abstention.md#s-1) — Калибровка score по DEV при неизменном rank | [S009](06-source-content-walk.md#s009), [S010](06-source-content-walk.md#s010), [S021](06-source-content-walk.md#s021), [S024](06-source-content-walk.md#s024), [S026](06-source-content-walk.md#s026), [S028](06-source-content-walk.md#s028), [S036](06-source-content-walk.md#s036), [S039](06-source-content-walk.md#s039), [S051](06-source-content-walk.md#s051), [S097](06-source-content-walk.md#s097), [S192](06-source-content-walk.md#s192), [S290](06-source-content-walk.md#s290) |
| <a id="p93-2"></a>[P93.2](cards/P93-confidence-abstention.md#s-2) — Warning против calibrated abstention | [S003](06-source-content-walk.md#s003), [S009](06-source-content-walk.md#s009), [S010](06-source-content-walk.md#s010), [S013](06-source-content-walk.md#s013), [S097](06-source-content-walk.md#s097), [S221](06-source-content-walk.md#s221) |
| <a id="p94-1"></a>[P94.1](cards/P94-typed-many-results.md#s-1) — Many→many cardinality | [S003](06-source-content-walk.md#s003), [S010](06-source-content-walk.md#s010), [S011](06-source-content-walk.md#s011), [S013](06-source-content-walk.md#s013), [S014](06-source-content-walk.md#s014), [S016](06-source-content-walk.md#s016), [S020](06-source-content-walk.md#s020), [S022](06-source-content-walk.md#s022), [S023](06-source-content-walk.md#s023), [S029](06-source-content-walk.md#s029), [S031](06-source-content-walk.md#s031), [S036](06-source-content-walk.md#s036), [S039](06-source-content-walk.md#s039), [S043](06-source-content-walk.md#s043), [S103](06-source-content-walk.md#s103) |
| <a id="p94-2"></a>[P94.2](cards/P94-typed-many-results.md#s-2) — Typed fan-out | [S011](06-source-content-walk.md#s011), [S013](06-source-content-walk.md#s013) |
| <a id="p94-3"></a>[P94.3](cards/P94-typed-many-results.md#s-3) — Typed fan-in и неоднозначность | [S011](06-source-content-walk.md#s011), [S013](06-source-content-walk.md#s013) |
| <a id="p95-1"></a>[P95.1](cards/P95-learned-flow-edge.md#s-1) — Обучение отношения при frozen Units | [S003](06-source-content-walk.md#s003), [S011](06-source-content-walk.md#s011), [S013](06-source-content-walk.md#s013), [S020](06-source-content-walk.md#s020), [S023](06-source-content-walk.md#s023), [S029](06-source-content-walk.md#s029), [S031](06-source-content-walk.md#s031), [S036](06-source-content-walk.md#s036), [S039](06-source-content-walk.md#s039), [S040](06-source-content-walk.md#s040), [S043](06-source-content-walk.md#s043), [S103](06-source-content-walk.md#s103), [S123](06-source-content-walk.md#s123) |
| <a id="p95-2"></a>[P95.2](cards/P95-learned-flow-edge.md#s-2) — Cold restore состояния ребра | [S011](06-source-content-walk.md#s011), [S013](06-source-content-walk.md#s013) |
| <a id="p95-3"></a>[P95.3](cards/P95-learned-flow-edge.md#s-3) — Новая композиция тех же Units | [S011](06-source-content-walk.md#s011), [S013](06-source-content-walk.md#s013) |
| <a id="p96-1"></a>[P96.1](cards/P96-async-orchestration.md#s-1) — Последовательные против параллельных альтернатив | [S003](06-source-content-walk.md#s003), [S011](06-source-content-walk.md#s011), [S013](06-source-content-walk.md#s013), [S020](06-source-content-walk.md#s020), [S023](06-source-content-walk.md#s023), [S029](06-source-content-walk.md#s029), [S036](06-source-content-walk.md#s036), [S039](06-source-content-walk.md#s039), [S103](06-source-content-walk.md#s103) |
| <a id="p96-2"></a>[P96.2](cards/P96-async-orchestration.md#s-2) — Общая quota всех ветвей | [S011](06-source-content-walk.md#s011), [S013](06-source-content-walk.md#s013) |
| <a id="p96-3"></a>[P96.3](cards/P96-async-orchestration.md#s-3) — Cancellation и late result | [S011](06-source-content-walk.md#s011), [S013](06-source-content-walk.md#s013) |
| <a id="p96-4"></a>[P96.4](cards/P96-async-orchestration.md#s-4) — Следующий уровень получает все разрешённые branch outputs | [S146](06-source-content-walk.md#s146) |
| <a id="p97-1"></a>[P97.1](cards/P97-learned-graph-order.md#s-1) — Обновление higher-order relation | [S003](06-source-content-walk.md#s003), [S011](06-source-content-walk.md#s011), [S013](06-source-content-walk.md#s013), [S014](06-source-content-walk.md#s014), [S020](06-source-content-walk.md#s020), [S023](06-source-content-walk.md#s023), [S029](06-source-content-walk.md#s029), [S036](06-source-content-walk.md#s036), [S039](06-source-content-walk.md#s039), [S040](06-source-content-walk.md#s040), [S043](06-source-content-walk.md#s043), [S103](06-source-content-walk.md#s103) |
| <a id="p97-2"></a>[P97.2](cards/P97-learned-graph-order.md#s-2) — Перенос на новый motif | [S011](06-source-content-walk.md#s011), [S013](06-source-content-walk.md#s013) |
| <a id="p97-3"></a>[P97.3](cards/P97-learned-graph-order.md#s-3) — Restore learned topology | [S011](06-source-content-walk.md#s011), [S013](06-source-content-walk.md#s013) |
| <a id="p98-1"></a>[P98.1](cards/P98-tokenizer-capacity.md#s-1) — Диагностика multilingual token fertility | [S001](06-source-content-walk.md#s001), [S003](06-source-content-walk.md#s003), [S007](06-source-content-walk.md#s007), [S008](06-source-content-walk.md#s008), [S031](06-source-content-walk.md#s031), [S036](06-source-content-walk.md#s036), [S039](06-source-content-walk.md#s039) |
| <a id="p98-2"></a>[P98.2](cards/P98-tokenizer-capacity.md#s-2) — Один размер BPE при совместимом model state | [S003](06-source-content-walk.md#s003), [S007](06-source-content-walk.md#s007) |
| <a id="p99-1"></a>[P99.1](cards/P99-unknown-token-id.md#s-1) — Plain decoder молча теряет unknown ID | [S099](06-source-content-walk.md#s099), [S108](06-source-content-walk.md#s108) |
| <a id="p99-2"></a>[P99.2](cards/P99-unknown-token-id.md#s-2) — Detailed decoder отказывает на том же ID | [S099](06-source-content-walk.md#s099) |
| <a id="p100-1"></a>[P100.1](cards/P100-topk-ties.md#s-1) — TopK ties оставляют больше K | [S126](06-source-content-walk.md#s126) |
| <a id="p100-2"></a>[P100.2](cards/P100-topk-ties.md#s-2) — K=1 и K>=vocab | [S126](06-source-content-walk.md#s126) |
| <a id="p100-3"></a>[P100.3](cards/P100-topk-ties.md#s-3) — Одновременно topK и topP: else-if против последовательных фильтров | [S100](06-source-content-walk.md#s100), [S103](06-source-content-walk.md#s103), [S108](06-source-content-walk.md#s108), [S109](06-source-content-walk.md#s109), [S110](06-source-content-walk.md#s110) |
| <a id="p101-1"></a>[P101.1](cards/P101-learning-budget.md#s-1) — Learning curve по updates | [S003](06-source-content-walk.md#s003), [S004](06-source-content-walk.md#s004), [S006](06-source-content-walk.md#s006), [S016](06-source-content-walk.md#s016), [S036](06-source-content-walk.md#s036), [S039](06-source-content-walk.md#s039) |
| <a id="p101-2"></a>[P101.2](cards/P101-learning-budget.md#s-2) — Равный total compute | [S003](06-source-content-walk.md#s003), [S006](06-source-content-walk.md#s006), [S016](06-source-content-walk.md#s016) |
| <a id="p101-3"></a>[P101.3](cards/P101-learning-budget.md#s-3) — Tiny TRAIN fit отдельно от heldout | [S001](06-source-content-walk.md#s001), [S003](06-source-content-walk.md#s003), [S006](06-source-content-walk.md#s006), [S016](06-source-content-walk.md#s016), [S331](06-source-content-walk.md#s331) |
| <a id="p102-1"></a>[P102.1](cards/P102-teacher-forced-decoding.md#s-1) — Teacher-forced против self-prefix | [S001](06-source-content-walk.md#s001), [S003](06-source-content-walk.md#s003), [S004](06-source-content-walk.md#s004), [S006](06-source-content-walk.md#s006), [S008](06-source-content-walk.md#s008), [S039](06-source-content-walk.md#s039), [S126](06-source-content-walk.md#s126) |
| <a id="p102-2"></a>[P102.2](cards/P102-teacher-forced-decoding.md#s-2) — Greedy против одного sampling factor | [S003](06-source-content-walk.md#s003), [S008](06-source-content-walk.md#s008) |
| <a id="p102-3"></a>[P102.3](cards/P102-teacher-forced-decoding.md#s-3) — Универсальный response prefix при том же Student | [S172](06-source-content-walk.md#s172), [S173](06-source-content-walk.md#s173) |
| <a id="p103-1"></a>[P103.1](cards/P103-stack-before-normalization.md#s-1) — Stack detection до normalization | [S003](06-source-content-walk.md#s003), [S005](06-source-content-walk.md#s005), [S036](06-source-content-walk.md#s036), [S039](06-source-content-walk.md#s039), [S209](06-source-content-walk.md#s209), [S215](06-source-content-walk.md#s215) |
| <a id="p103-2"></a>[P103.2](cards/P103-stack-before-normalization.md#s-2) — Каждый потерянный marker отдельно | [S003](06-source-content-walk.md#s003), [S215](06-source-content-walk.md#s215) |
| <a id="p104-1"></a>[P104.1](cards/P104-law-eligibility.md#s-1) — HARD eligibility | [S003](06-source-content-walk.md#s003), [S005](06-source-content-walk.md#s005), [S009](06-source-content-walk.md#s009), [S010](06-source-content-walk.md#s010), [S013](06-source-content-walk.md#s013), [S023](06-source-content-walk.md#s023), [S029](06-source-content-walk.md#s029), [S036](06-source-content-walk.md#s036), [S037](06-source-content-walk.md#s037), [S039](06-source-content-walk.md#s039), [S046](06-source-content-walk.md#s046), [S048](06-source-content-walk.md#s048) |
| <a id="p104-2"></a>[P104.2](cards/P104-law-eligibility.md#s-2) — SOFT preference | [S009](06-source-content-walk.md#s009), [S010](06-source-content-walk.md#s010), [S013](06-source-content-walk.md#s013), [S046](06-source-content-walk.md#s046) |
| <a id="p104-3"></a>[P104.3](cards/P104-law-eligibility.md#s-3) — Negative memory | [S003](06-source-content-walk.md#s003), [S005](06-source-content-walk.md#s005), [S009](06-source-content-walk.md#s009), [S010](06-source-content-walk.md#s010), [S013](06-source-content-walk.md#s013), [S046](06-source-content-walk.md#s046) |
| <a id="p104-4"></a>[P104.4](cards/P104-law-eligibility.md#s-4) — Warning memory и fallback authority | [S003](06-source-content-walk.md#s003), [S009](06-source-content-walk.md#s009), [S010](06-source-content-walk.md#s010), [S013](06-source-content-walk.md#s013), [S046](06-source-content-walk.md#s046) |
| <a id="p105-1"></a>[P105.1](cards/P105-frozen-reference.md#s-1) — Отдельный frozen reference snapshot | [S003](06-source-content-walk.md#s003), [S004](06-source-content-walk.md#s004), [S008](06-source-content-walk.md#s008), [S012](06-source-content-walk.md#s012), [S041](06-source-content-walk.md#s041) |
| <a id="p105-2"></a>[P105.2](cards/P105-frozen-reference.md#s-2) — Import и mutable alias reference | [S003](06-source-content-walk.md#s003), [S012](06-source-content-walk.md#s012) |
| <a id="p106-1"></a>[P106.1](cards/P106-corrected-semantic-target.md#s-1) — Corrected target достигает semantic-unit producer | [S001](06-source-content-walk.md#s001), [S011](06-source-content-walk.md#s011), [S022](06-source-content-walk.md#s022), [S086](06-source-content-walk.md#s086), [S089](06-source-content-walk.md#s089), [S116](06-source-content-walk.md#s116), [S179](06-source-content-walk.md#s179), [S185](06-source-content-walk.md#s185), [S189](06-source-content-walk.md#s189), [S360](06-source-content-walk.md#s360) |
| <a id="p106-2"></a>[P106.2](cards/P106-corrected-semantic-target.md#s-2) — Ошибочная annotation при фиксированном builder | Дополнительный подслучай прежнего каталога O; H не назван его прямым основанием: [13-domain-context-and-dpo-subunits.md:37](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/13-domain-context-and-dpo-subunits.md:37>), [13-domain-context-and-dpo-subunits.md:106](<C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/13-domain-context-and-dpo-subunits.md:106>) |
| <a id="p106-3"></a>[P106.3](cards/P106-corrected-semantic-target.md#s-3) — Обученный pattern требует согласованного memory key | [S133](06-source-content-walk.md#s133), [S143](06-source-content-walk.md#s143), [S189](06-source-content-walk.md#s189) |
| <a id="p106-4"></a>[P106.4](cards/P106-corrected-semantic-target.md#s-4) — Executor обучает prefix/structure keys тем же semantic extractor | [S179](06-source-content-walk.md#s179), [S180](06-source-content-walk.md#s180), [S185](06-source-content-walk.md#s185), [S189](06-source-content-walk.md#s189) |
| <a id="p107-1"></a>[P107.1](cards/P107-lawful-neighbor-aggregation.md#s-1) — Первый hit против lawful aggregation | [S001](06-source-content-walk.md#s001), [S003](06-source-content-walk.md#s003), [S009](06-source-content-walk.md#s009), [S010](06-source-content-walk.md#s010), [S014](06-source-content-walk.md#s014), [S016](06-source-content-walk.md#s016), [S017](06-source-content-walk.md#s017), [S020](06-source-content-walk.md#s020), [S031](06-source-content-walk.md#s031), [S036](06-source-content-walk.md#s036), [S037](06-source-content-walk.md#s037), [S038](06-source-content-walk.md#s038), [S039](06-source-content-walk.md#s039), [S040](06-source-content-walk.md#s040), [S043](06-source-content-walk.md#s043), [S045](06-source-content-walk.md#s045), [S115](06-source-content-walk.md#s115), [S360](06-source-content-walk.md#s360) |
| <a id="p107-2"></a>[P107.2](cards/P107-lawful-neighbor-aggregation.md#s-2) — Shuffled outputs control | [S001](06-source-content-walk.md#s001), [S009](06-source-content-walk.md#s009), [S010](06-source-content-walk.md#s010), [S025](06-source-content-walk.md#s025), [S045](06-source-content-walk.md#s045), [S070](06-source-content-walk.md#s070), [S071](06-source-content-walk.md#s071), [S088](06-source-content-walk.md#s088), [S095](06-source-content-walk.md#s095) |
| <a id="p107-3"></a>[P107.3](cards/P107-lawful-neighbor-aggregation.md#s-3) — Law-only control | [S001](06-source-content-walk.md#s001), [S009](06-source-content-walk.md#s009), [S010](06-source-content-walk.md#s010), [S045](06-source-content-walk.md#s045), [S071](06-source-content-walk.md#s071) |
| <a id="p107-4"></a>[P107.4](cards/P107-lawful-neighbor-aggregation.md#s-4) — Adaptive k отдельно | [S009](06-source-content-walk.md#s009), [S010](06-source-content-walk.md#s010) |
| <a id="p107-5"></a>[P107.5](cards/P107-lawful-neighbor-aggregation.md#s-5) — Полный Train(T0) против Train(T0+T1) при том же selection | [S045](06-source-content-walk.md#s045) |
| <a id="p108-1"></a>[P108.1](cards/P108-metadata-carrier.md#s-1) — string[] против List/IReadOnlyList carrier | [S046](06-source-content-walk.md#s046), [S048](06-source-content-walk.md#s048) |
| <a id="p108-2"></a>[P108.2](cards/P108-metadata-carrier.md#s-2) — JSON и missing metadata contract | [S046](06-source-content-walk.md#s046), [S048](06-source-content-walk.md#s048) |
| <a id="p109-1"></a>[P109.1](cards/P109-gold-validity.md#s-1) — Смысловая истинность gold | [S003](06-source-content-walk.md#s003), [S004](06-source-content-walk.md#s004), [S006](06-source-content-walk.md#s006), [S029](06-source-content-walk.md#s029), [S031](06-source-content-walk.md#s031), [S036](06-source-content-walk.md#s036), [S039](06-source-content-walk.md#s039), [S040](06-source-content-walk.md#s040), [S043](06-source-content-walk.md#s043) |
| <a id="p109-2"></a>[P109.2](cards/P109-gold-validity.md#s-2) — Происхождение автора независимо от истинности | [S001](06-source-content-walk.md#s001), [S003](06-source-content-walk.md#s003), [S004](06-source-content-walk.md#s004), [S005](06-source-content-walk.md#s005), [S006](06-source-content-walk.md#s006), [S321](06-source-content-walk.md#s321) |
| <a id="p110-1"></a>[P110.1](cards/P110-numeric-matrix-cost.md#s-1) — Одна numeric implementation MatVec | [S037](06-source-content-walk.md#s037), [S038](06-source-content-walk.md#s038), [S360](06-source-content-walk.md#s360) |
| <a id="p110-2"></a>[P110.2](cards/P110-numeric-matrix-cost.md#s-2) — Sparsity threshold отдельно | [S037](06-source-content-walk.md#s037), [S038](06-source-content-walk.md#s038) |
| <a id="p110-3"></a>[P110.3](cards/P110-numeric-matrix-cost.md#s-3) — Полная стоимость реального consumer | [S001](06-source-content-walk.md#s001), [S003](06-source-content-walk.md#s003), [S037](06-source-content-walk.md#s037), [S038](06-source-content-walk.md#s038) |
| <a id="p111-1"></a>[P111.1](cards/P111-reward-credit.md#s-1) — Credit selected Unit | [S001](06-source-content-walk.md#s001), [S003](06-source-content-walk.md#s003), [S004](06-source-content-walk.md#s004), [S011](06-source-content-walk.md#s011), [S013](06-source-content-walk.md#s013), [S022](06-source-content-walk.md#s022), [S036](06-source-content-walk.md#s036), [S039](06-source-content-walk.md#s039) |
| <a id="p111-2"></a>[P111.2](cards/P111-reward-credit.md#s-2) — Credit selected Flow edge | [S003](06-source-content-walk.md#s003), [S004](06-source-content-walk.md#s004), [S011](06-source-content-walk.md#s011), [S013](06-source-content-walk.md#s013) |
| <a id="p111-3"></a>[P111.3](cards/P111-reward-credit.md#s-3) — Unselected/shared branch control | [S003](06-source-content-walk.md#s003), [S011](06-source-content-walk.md#s011), [S013](06-source-content-walk.md#s013) |
| <a id="p112-1"></a>[P112.1](cards/P112-family-disjoint-evaluation.md#s-1) — Source-family split | [S002](06-source-content-walk.md#s002), [S003](06-source-content-walk.md#s003), [S005](06-source-content-walk.md#s005), [S016](06-source-content-walk.md#s016), [S020](06-source-content-walk.md#s020), [S024](06-source-content-walk.md#s024), [S025](06-source-content-walk.md#s025), [S029](06-source-content-walk.md#s029), [S036](06-source-content-walk.md#s036), [S039](06-source-content-walk.md#s039), [S041](06-source-content-walk.md#s041), [S128](06-source-content-walk.md#s128), [S159](06-source-content-walk.md#s159), [S272](06-source-content-walk.md#s272) |
| <a id="p112-2"></a>[P112.2](cards/P112-family-disjoint-evaluation.md#s-2) — Template-family split | [S002](06-source-content-walk.md#s002), [S003](06-source-content-walk.md#s003), [S004](06-source-content-walk.md#s004), [S006](06-source-content-walk.md#s006), [S016](06-source-content-walk.md#s016) |
| <a id="p112-3"></a>[P112.3](cards/P112-family-disjoint-evaluation.md#s-3) — Одинаковый независимый конечный TEST | [S001](06-source-content-walk.md#s001), [S003](06-source-content-walk.md#s003), [S016](06-source-content-walk.md#s016), [S019](06-source-content-walk.md#s019) |
| <a id="p113-1"></a>[P113.1](cards/P113-feature-energy.md#s-1) — Единый L2 против раздельных групп | [S053](06-source-content-walk.md#s053), [S054](06-source-content-walk.md#s054), [S055](06-source-content-walk.md#s055), [S062](06-source-content-walk.md#s062), [S360](06-source-content-walk.md#s360) |
| <a id="p113-2"></a>[P113.2](cards/P113-feature-energy.md#s-2) — Missing predicate tier и zero norm | [S053](06-source-content-walk.md#s053), [S054](06-source-content-walk.md#s054), [S062](06-source-content-walk.md#s062) |
| <a id="p114-1"></a>[P114.1](cards/P114-zero-weight-pair.md#s-1) — Weight0 против отсутствующей пары | [S062](06-source-content-walk.md#s062), [S256](06-source-content-walk.md#s256), [S360](06-source-content-walk.md#s360) |
| <a id="p114-2"></a>[P114.2](cards/P114-zero-weight-pair.md#s-2) — Clamp0.01 против genuine zero | [S062](06-source-content-walk.md#s062) |
| <a id="p114-3"></a>[P114.3](cards/P114-zero-weight-pair.md#s-3) — Negative weight admission | [S062](06-source-content-walk.md#s062) |
| <a id="p115-1"></a>[P115.1](cards/P115-bm25-statistics-cache.md#s-1) — Cached corpus statistics против повторного scan | [S072](06-source-content-walk.md#s072), [S073](06-source-content-walk.md#s073), [S077](06-source-content-walk.md#s077), [S078](06-source-content-walk.md#s078), [S080](06-source-content-walk.md#s080), [S081](06-source-content-walk.md#s081), [S360](06-source-content-walk.md#s360) |
| <a id="p115-2"></a>[P115.2](cards/P115-bm25-statistics-cache.md#s-2) — Corpus update invalidates cache | [S080](06-source-content-walk.md#s080) |
| <a id="p116-1"></a>[P116.1](cards/P116-e43-file-boundary.md#s-1) — Reset predicate sequence на file boundary | [S125](06-source-content-walk.md#s125), [S162](06-source-content-walk.md#s162), [S176](06-source-content-walk.md#s176), [S208](06-source-content-walk.md#s208), [S259](06-source-content-walk.md#s259), [S260](06-source-content-walk.md#s260), [S261](06-source-content-walk.md#s261), [S262](06-source-content-walk.md#s262), [S268](06-source-content-walk.md#s268), [S360](06-source-content-walk.md#s360) |
| <a id="p116-2"></a>[P116.2](cards/P116-e43-file-boundary.md#s-2) — Повторные pair keys в train/test | [S261](06-source-content-walk.md#s261), [S262](06-source-content-walk.md#s262) |
| <a id="p116-3"></a>[P116.3](cards/P116-e43-file-boundary.md#s-3) — Replay on/off после фиксирования материала | [S005](06-source-content-walk.md#s005), [S015](06-source-content-walk.md#s015), [S018](06-source-content-walk.md#s018), [S175](06-source-content-walk.md#s175), [S261](06-source-content-walk.md#s261), [S262](06-source-content-walk.md#s262), [S269](06-source-content-walk.md#s269), [S336](06-source-content-walk.md#s336) |
| <a id="p117-1"></a>[P117.1](cards/P117-translation-direction-memory.md#s-1) — Semantic-concept gate поиска | [S205](06-source-content-walk.md#s205), [S210](06-source-content-walk.md#s210), [S212](06-source-content-walk.md#s212), [S218](06-source-content-walk.md#s218), [S273](06-source-content-walk.md#s273), [S294](06-source-content-walk.md#s294), [S320](06-source-content-walk.md#s320), [S324](06-source-content-walk.md#s324) |
| <a id="p117-2"></a>[P117.2](cards/P117-translation-direction-memory.md#s-2) — Direction namespace общего KNN | [S050](06-source-content-walk.md#s050), [S052](06-source-content-walk.md#s052), [S057](06-source-content-walk.md#s057), [S205](06-source-content-walk.md#s205), [S210](06-source-content-walk.md#s210), [S212](06-source-content-walk.md#s212), [S218](06-source-content-walk.md#s218) |
| <a id="p117-3"></a>[P117.3](cards/P117-translation-direction-memory.md#s-3) — Target-language acceptance predicate | [S052](06-source-content-walk.md#s052), [S186](06-source-content-walk.md#s186), [S205](06-source-content-walk.md#s205), [S210](06-source-content-walk.md#s210), [S212](06-source-content-walk.md#s212), [S218](06-source-content-walk.md#s218) |
| <a id="p118-1"></a>[P118.1](cards/P118-projectwiki-content.md#s-1) — Chosen swap при постоянных PairId/Prompt/Mode | [S177](06-source-content-walk.md#s177), [S192](06-source-content-walk.md#s192), [S360](06-source-content-walk.md#s360) |
| <a id="p118-2"></a>[P118.2](cards/P118-projectwiki-content.md#s-2) — Memory on/off при неизменном запросе | [S192](06-source-content-walk.md#s192) |
| <a id="p118-3"></a>[P118.3](cards/P118-projectwiki-content.md#s-3) — Mode2/3 потребляет Chosen.PlanText | [S192](06-source-content-walk.md#s192) |
| <a id="p118-4"></a>[P118.4](cards/P118-projectwiki-content.md#s-4) — Scorer проверяет содержание урока | [S192](06-source-content-walk.md#s192) |
| <a id="p118-5"></a>[P118.5](cards/P118-projectwiki-content.md#s-5) — Zero-similarity nearest требует отказа | [S192](06-source-content-walk.md#s192) |
| <a id="p119-1"></a>[P119.1](cards/P119-learning-dynamics.md#s-1) — Learning rate при одном бюджете | [S004](06-source-content-walk.md#s004), [S006](06-source-content-walk.md#s006), [S008](06-source-content-walk.md#s008) |
| <a id="p119-2"></a>[P119.2](cards/P119-learning-dynamics.md#s-2) — DPO beta при одном LR | [S004](06-source-content-walk.md#s004), [S006](06-source-content-walk.md#s006), [S008](06-source-content-walk.md#s008) |
| <a id="p119-3"></a>[P119.3](cards/P119-learning-dynamics.md#s-3) — Размер минибатча при одинаковом материале | [S003](06-source-content-walk.md#s003), [S004](06-source-content-walk.md#s004), [S171](06-source-content-walk.md#s171) |
| <a id="p120-1"></a>[P120.1](cards/P120-semantic-compression-laws.md#s-1) — Отрицание в identifier не удаляется stopwords | [S002](06-source-content-walk.md#s002), [S018](06-source-content-walk.md#s018) |
| <a id="p120-2"></a>[P120.2](cards/P120-semantic-compression-laws.md#s-2) — Синонимический op сохраняет нужную идентичность операции | [S002](06-source-content-walk.md#s002), [S018](06-source-content-walk.md#s018) |
| <a id="p120-3"></a>[P120.3](cards/P120-semantic-compression-laws.md#s-3) — Общий внешний semantic target вместо несопоставимой perplexity | [S002](06-source-content-walk.md#s002), [S005](06-source-content-walk.md#s005), [S017](06-source-content-walk.md#s017), [S018](06-source-content-walk.md#s018) |
| <a id="p121-1"></a>[P121.1](cards/P121-project-adapter-selection.md#s-1) — Project→adapter selection при frozen adapters | [S005](06-source-content-walk.md#s005) |
| <a id="p121-2"></a>[P121.2](cards/P121-project-adapter-selection.md#s-2) — Изолированный update одного project recipient | [S005](06-source-content-walk.md#s005) |
| <a id="p122-1"></a>[P122.1](cards/P122-root-channel-unicode-digits-operators.md#s-1) — Сохранение различий root extraction при одинаковых координатах | [S003](06-source-content-walk.md#s003), [S010](06-source-content-walk.md#s010), [S014](06-source-content-walk.md#s014), [S016](06-source-content-walk.md#s016), [S020](06-source-content-walk.md#s020), [S036](06-source-content-walk.md#s036), [S039](06-source-content-walk.md#s039), [S043](06-source-content-walk.md#s043), [S074](06-source-content-walk.md#s074), [S082](06-source-content-walk.md#s082), [S083](06-source-content-walk.md#s083) |
| <a id="p122-2"></a>[P122.2](cards/P122-root-channel-unicode-digits-operators.md#s-2) — Граница active-count consumer и настоящего semantic consumer | [S003](06-source-content-walk.md#s003), [S010](06-source-content-walk.md#s010) |
| <a id="p123-1"></a>[P123.1](cards/P123-design-conversion-critic-evidence.md#s-1) — Зависимость verdict от QA evidence при неизменном артефакте | [S022](06-source-content-walk.md#s022) |
| <a id="p123-2"></a>[P123.2](cards/P123-design-conversion-critic-evidence.md#s-2) — Различающий визуальный вход при одинаковых QA flags | [S022](06-source-content-walk.md#s022), [S286](06-source-content-walk.md#s286) |
| <a id="p123-3"></a>[P123.3](cards/P123-design-conversion-critic-evidence.md#s-3) — Обученный critic state при том же визуальном входе | [S022](06-source-content-walk.md#s022) |
| <a id="p124-1"></a>[P124.1](cards/P124-p3-fixed-discriminator-contract.md#s-1) — Зависимость правила от runtime contract вместо буквального BaseService | [S022](06-source-content-walk.md#s022), [S023](06-source-content-walk.md#s023), [S297](06-source-content-walk.md#s297), [S324](06-source-content-walk.md#s324) |
| <a id="p124-2"></a>[P124.2](cards/P124-p3-fixed-discriminator-contract.md#s-2) — Изменение learned state после feedback отдельно от добавления правила | [S022](06-source-content-walk.md#s022) |
| <a id="p125-1"></a>[P125.1](cards/P125-executor-memory-learning.md#s-1) — Один structured Executor lesson-batch против неизменённой памяти | [S020](06-source-content-walk.md#s020), [S021](06-source-content-walk.md#s021), [S026](06-source-content-walk.md#s026), [S027](06-source-content-walk.md#s027), [S028](06-source-content-walk.md#s028), [S031](06-source-content-walk.md#s031), [S032](06-source-content-walk.md#s032), [S033](06-source-content-walk.md#s033), [S040](06-source-content-walk.md#s040), [S043](06-source-content-walk.md#s043), [S057](06-source-content-walk.md#s057), [S066](06-source-content-walk.md#s066), [S070](06-source-content-walk.md#s070), [S085](06-source-content-walk.md#s085), [S094](06-source-content-walk.md#s094), [S095](06-source-content-walk.md#s095), [S102](06-source-content-walk.md#s102), [S103](06-source-content-walk.md#s103), [S111](06-source-content-walk.md#s111), [S155](06-source-content-walk.md#s155), [S245](06-source-content-walk.md#s245), [S285](06-source-content-walk.md#s285), [S293](06-source-content-walk.md#s293) |

## Граница полноты

360/360 файлов H имеют содержательный разбор; 476 записей/блоков обхода — не число независимых находок. 125 карточек объединяют 470 подслучаев, включая проверки измерителя, негативные контроли и условные продолжения. Обнаруженных, но оставленных без связи тезисов в завершённом обходе — 0. Внешние donor SHA, текущие native callers, независимые языковые gold и действующая совместимость API там, где источник их не устанавливает, остаются явными будущими зависимостями. Полный новый аудит core или внешних архивов не проводился.
