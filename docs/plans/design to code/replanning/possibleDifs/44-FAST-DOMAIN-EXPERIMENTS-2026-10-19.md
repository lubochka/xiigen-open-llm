# 44 — Быстрые причинные проверки с доменными метриками

**19 октября 2026. DESIGN ONLY / NOT RUN.** Кроме [пяти source-equivalent L0 cases](44-E26-SOURCE-EQUIVALENT-PROXY-RESULT.json) и ранее опубликованных native reports **никакие новые learning, native .NET, teacher, independent-review calls не выполнялись**. Здесь предлагаются маленькие этапы проверки, не новая бесконечная программа CI.

## Что именно измерять

Для каждого опыта необходимы: `ID`; **домен как предмет обучения**, задача/язык/направление, *какой* Unit/Student/Flow меняет состояние, `source branch SHA`, список `A/B` с **ровно одним** отличием, состав TRAIN/DEV/heldout по исходным семействам, self/independent gold, один ordinary consumer, before/after learned state, saved+new-process parity, отрицательные и пограничные cases, предметная метрика с числителем/знаменателем, false accepted, abstentions, ms/CPU/RAM/VRAM, точный фальсификатор и ссылки на raw outputs. «kNN», «LoRA», «перевод EN→RU» и «IlearningUnit» — не четыре домена.

Каждому эксперименту 1) заранее зафиксировать смысловой критерий и critical errors, 2) не использовать example families TRAIN в sealed, 3) не менять правило после просмотра sealed, 4) не считать снижение loss или CI green измерением смыслового качества. Малый пилот 6–12 уроков и 12 heldout подходит для локальной разведки, **но не гарантирует статистически значимого вывода**. Источники open-weight / closed judge — строго по актуальным legality/admission receipt; этот документ не разрешает повторно обучать уже applied pair.

## Сначала восстановить старое без нового обучения

| ID | Предмет/получатель | A и B, точное вмешательство | Данные/проверка | Фальсификатор |
| --- | --- | --- | --- | --- |
| **R44-01** | Перевод; 4 ранее существовавших направления, translator Unit memory | A = original `PrepareToGPL` loader+старый checkpoint; B = current loader с **явной миграцией** отсутствующих двух пустых направлений; C = current unpatched; тот же SHA исходного checkpoint | 0 training examples. Exact/near/composition прежних направлений через тот же ordinary Flow после холодной загрузки, сохранить исходные model maps, отвергать corrupted asset | Даже один прежний ответ/карта изменён или B всё ещё отказывается от импорта. Не создавать испанскую competence из пустого Unit. Уже исследовано N41/N43 — не заявлять новым. |
| **R44-02** | Оркестрация; guide learned checkpoint→extension API | A = old guide schema1; B = schema2 при **допущенном** source manifest и такой же исходной законной памяти, отдельно route with missing admission | Same old tasks + 3 unseen dependency patterns; chosen asset path, legal manifest, loaded pattern ids, correct action order | B только меняет metadata и не использует patterns либо разрешает недопустимые источники. Не смешивать с translator checkpoint. |
| **R44-03** | Перевод EN→RU; ExecutorUnit→normal Flow | Использовать **существующий positive control**: 8 lessons, до/после исправленной доставки phrase mappings. НЕ вводить новый программный алгоритм для уже исправленного пути | Старые receipts 11/24→19/24, correct cold 19/24; явно оставить два ошибки и cc03 ambiguity; 0 новых уроков при replay | В новом host улучшение не наблюдается или wrong-accepted растут без контроля. Это контроль route, не нейронного Student. |

## Контекст и сохранение структуры по доменам

**X44-01 — E26 «нет контекста» (RU→EN, scorer).** A=original `ContextDistance`, B=единственный фактор «когда query side отсутствует, required entry side не получает distance0», C=все query POS заданы (неизменность правильных ответов). 4 случая `поток` (no-context, leftVERB, rightNOUN, leftADJ), 4 `запрос`, 2 unknown, 2 частично потерянных POS. Gold для no-context — `UNKNOWN`, не произвольная догадка «thread» или «flow». Метрика: unsupported specific translations/ambiguous queries ↓, контекстные complete answers / all complete не падают; coverage и abstention отдельно. **L0 source-equivalent 5/5 already computed; native and Flow NOT_RUN.** Если business policy допускает wildcard на входе, не объявлять багом без его контракта. [Оригинальный контрпример](44-E26-UNKNOWN-CONTEXT-AND-ORDER-2026-10-19.md).

**X44-02 — E26 «порядок POS» (RU→EN), C# только при реальном AST consumer.** A=legacy Any-overlap, B=ordered POS-role match; нельзя одновременно менять k/priority/splits/learning map. TRAIN6 смысловых отношений, HELDOUT12 других исходных семейств; ordered contrast и permutation negative. Перевод проверять на глагол/объект и отрицание. C# — только отдельный Roslyn parser и 4 train files/8 source-disjoint heldout; **не cast старые русские numeric POS (7,8,9) в SyntaxKind**. Метрика: fully correct meaning/relations, critical reversals, false neighbors, ms, bytes. Фальсификатор: gain только на source-equivalent scorer, не ordinary consumer, либо B ухудшает порядок-неважные данные.

**X44-03 — восстановить полный E26 context в обучаемой памяти (RU→EN).** A=нынешние `ExecutorUnit` ключи lemma+concept/surface, B=добавить к word choice оба соседних coarse POS **из реальных обучающих примеров**; C=shuffle selected labels внутри одной lemma; D=no-learning static rules. 6–12 reviewed TRAIN на `поток`/`запрос`, 12 **новых** фраз, одна и та же Tokenizer/phrase/exact/KNN cascade. End-to-end через `TrainedTranslationAdapter` и `TranslationRoutingFlow`. Метрика: sense disambiguation и wrong-accepted/coverage; assert видимость нового state при actual call. Этот опыт подробно уже спроектирован [MICRO-25](25-MICRO-E26-POS-CONTEXT-CAUSAL-EXPERIMENT-2026-10-16.md): использовать, **не открывать повторный идентичный run без причины**. Метки POS и их совместимость фиксировать.

**X44-04 — Structural E09 kNN (язык программирования C#).** A=старый bigram/query baseline, B=AST vector+kNN **на том же validation set, той же stable SyntaxKind coding**. 4 source-disjoint train files /4 heldout; не добавлять cross-file transitions в train pairs, одинаковые denominator. Сначала отдельно сопоставить original E09 top1 (0.2413 baseline all val против0.3507 first500, то есть не matched), затем memory trained k=5 и opt-in production `UseStructuralKnn` (второй последовательный, а не смешанный contrast). Primary: paired next-kind correct / all heldout transitions; 2nd: actual product correct AST advice. Negative: AST с переставленной логикой vs AST с той же семантикой при другом имени. Фальсификатор: прирост только из leakage/per-file codec или функция не достигает consumer.

**X44-05 — Replay retention (изученный код→новая security тема либо старый→новый перевод).** A=100% new rows, B=70% new +30% old replay при **одинаковом общем числе updates**, C=frozen old oracle. Source-disjoint old/new split без склейки file-boundary bigrams. Измерить **две** составляющие, old-law retention и new-law acquisition, плюс critical false accepted; не сводить к одной «accuracy». Стандартная память и LoRA разные recipients: если тест проводится на Bigram, он не доказывает LoRA; при LoRA обязательны optimizer/reference + full checkpoint. Старый E43 .3226→.3978 — только archived bigram signal. Уменьшение забывания при нулевом новом навыке — не общий успех.

## Отдельно про neural DPO/LoRA (не дублировать уже нулевой эксперимент)

**X44-06 — warm base→предпочтения vs DPO-only.** Предмет 1: корректная документация C# (null vs empty, `<` vs `<=`, порядок операций). Предмет 2: EN→RU с negation/word-role/subject agreement. Получатель — **настоящий neural Student** и его ordinary TextGenerator, НЕ память E26.

- A: `B0` случайный/frozen base + DPO-only в объёме, уже имеющемся в RP01.
- B: действительно `warm` base с измеренной на новых cases способностью, после этого DPO.
- C: тот же warm base, SFT-only, чтобы локализовать contribution DPO. Долю SFT cost считать отдельно; одинаковый seed, data provenance, tokenizer, decoding, training budgets по сравниваемым причинам.
- D: отдельно диагностически `Student+Translation memory`, но **это другой recipient и более дорогой составной алгоритм**; не выдавать его прирост за LoRA.
- Сначала проверка заучиваемости 8–32 TRAIN-only на новой semantic family; sealed12+ по независимым families, Dev4 для настройки; не включать source-variant paraphrases TRAIN в HELDOUT; не повторять идентичный `embedding-sft8`, который уже показал native updates и 0/24.
- Primary: paired source-law clauses correct and critical false answers per family, raw own generated text, chosen *absolute* log probability отдельно от chosen-vs-rejected margin, days/time to skill, CPU/RAM/VRAM, restore identity, exact input. **STOP** большой 500-pair cycle, если меняются только margin и tensors, а смысловая оценка остается0.

## Таблица приёмки причинного исследования

| Условие | PASS для продолжения | NEEDS_FIX / STOP |
| --- | --- | --- |
| Recipient | Именованы реально изменившиеся Unit/Student/Flow, confirmed consumer | Указаны только модель/механизм в диаграмме или prototype не вызывается |
| Learned behavior | Новый правильный ответ на frozen unfamiliar case + no critical regression | Тот же ответ, hardcoded fallback, или только tensor change |
| Evidence | Pinned SHA, raw outputs, identical comparison population, frozen gold | Неравный val denominator, видоизмененный after-run gold, metric without source |
| Checkpoint | Full state read in fresh process, correct previously learned behavior | Только manifest/hash, no actual recovery |
| Domains | Функция реально применима к теме/языку/парсеру | Список numeric profiles принят за разные домены |
| Review | Независимый арбитр проверил exact SHA после публикации | Саморевью автора как независимое или PASS устаревшего parent SHA |

**Управление:** source-only L0 не требует Windows GitHub runs. Даже полезный native .NET тест выполняется локально и лишь при установленном causal seam; не запускать массовую CI проверку до появления измеренной способности. Запрет на изменения core protected docs действует независимо от результата.
