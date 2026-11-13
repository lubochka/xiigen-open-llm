# T-TRAIN-CONTEXT-01 — сохранить контекст в двух учебных записях

**Уточнение исполнения по a822 (20.10):** ссылки на оригинальные файлы ниже обозначают происхождение и адрес предлагаемого последующего переноса. Сначала проверяется точная экспериментальная копия A/B в существующем проекте `34.Experiments` или `35.ExperimentsV2`, с малым host в `experiments-v2/RP01-MICRO/<ID>/`. Оригинальный продукт и его тесты остаются read-only. Копия сохраняет настоящий алгоритм, зависимости, admission и mutation capabilities; их обход, поддельный lease или суррогатный learner недопустимы. До исполнения фиксируются copied-file hashes, A/B diff и actual binding вызовов. Перенос в основной код — отдельное решение после результата. Текущий запрет пользователя менять protected распространяется и на results: для возможного запуска выбирается разрешённый каталог вне protected. Подробнее: [финальная дельта](12-CONCURRENT-UPDATES-AND-NOVELTY.md).

**Готовая научная постановка; native выполнение: NOT_RUN.** Это дополнительный опыт с фактором в записи урока, а не обязательная замена малого T и не новый запуск E26. В актуальном защищённом `03-translation.md` на commit `f5dd532045716495a2c7668573250e5d7cc46810` обе памяти получают одинаковые уроки, различаются только query POS. По действующему правилу 20.10 такой опыт допустим как проверка использования выученного состояния, если показана точная зависимость. Здесь другой вопрос: какая информация сохраняется настоящим обучаемым получателем при записи урока. Защищённая папка не меняется.

## 1. Что проверяем и где происходит обучение

Исполнитель и независимый reviewer в паспорте этой программы — gpt-6-astra / ultra, последовательно. Это роли подготовки и проверки; сам ученик — указанный ниже native компонент. В этом подаудите применены source-first правила `architect-behavior-classifier` и `code-examination`: полностью прочитан original owner, показаны writer/reader, фактический и предполагаемый результат разделены. Назначение модели исполнителя не является заявлением о запущенной нейронной модели.

Предметная тема — перевод, направление EN→RU, выбор между существительным «книга» и глаголом «бронировать» для одного слова `book` с внешней POS-аннотацией. Ученик — настоящий `SemanticCompiler.ContextualDictionary`, не языковая модель. Он усваивает две явно размеченные записи. Нейронная модель, LoRA, optimizer, генерация полного предложения в этот опыт не входят.

Единственный фактор: **сохранить или отбросить POS-контекст учебной записи перед native AddEntry**.

| Плечо | Что поступает в настоящий writer | Что поступает в reader |
|---|---|---|
| A | Те же SourceWord, Translation и Priority; ContextBefore=[] и ContextAfter=[] | Настоящие POS каждого EXAM, одинаковые с B |
| B | Те же SourceWord, Translation и Priority; ContextBefore/After из TRAIN | Те же EXAM, без преобразований |

В обоих плечах — две реальные записи через `AddEntry`; reader и его формула не меняются. Это проверка ценности признака в усвоенной памяти. Она **не доказывает**, что нынешний production writer XIIGEN теряет эти POS: автоматическое извлечение таких записей из произвольного текста и соединение этого класса с рабочим TranslationExecutor не установлены. Условный риск переноса — потерять контекст при создании записей. Сам эксперимент проверяет механизм на существующем публичном получателе.

**Точный original owner:** core commit `bf9b543e149ef197da85a767821d98c9b67916d2`, [12.SemanticCompiler/Translation/ContextualDictionary.cs](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/12.SemanticCompiler/Translation/ContextualDictionary.cs). SHA256 полного файла: `9926db1ed06111c7f64fd80f69cf6b81e28af110df77a4c4a452cf46d8313525`.

- [L13–18](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/12.SemanticCompiler/Translation/ContextualDictionary.cs#L13-L18): реальные поля DictionaryEntry.
- [L22–27](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/12.SemanticCompiler/Translation/ContextualDictionary.cs#L22-L27): пустой список и `_entries.Add(entry)`, настоящий writer состояния.
- [L68–85](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/12.SemanticCompiler/Translation/ContextualDictionary.cs#L68-L85): native reader ищет SourceWord, сравнивает сохранённый контекст с query, возвращает Translation.
- [L99–112](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/12.SemanticCompiler/Translation/ContextualDictionary.cs#L99-L112): действительная ContextDistance. `_k` здесь не участвует; голосование по трём соседям не измеряется.

## 2. Полный материал

Данные — небольшая simulation fixture, составленная для проверки этого механизма. Авторство внешнему датасету не приписывается. Порядок TRAIN фиксирован T1, T2. Priority=1 у обеих записей. POS-схема оригинала: DET=0, PRON=3, AUX=4, NOUN=7, VERB=8, ADJ=9. Человек задаёт входные POS; опыт не измеряет обучение POS-аннотатора.

| TRAIN ID | Фраза для человека | SourceWord | ContextBefore | ContextAfter | Translation |
|---|---|---|---|---|---|
| T1 | the book arrived | book | [0] | [8] | книга |
| T2 | we book rooms | book | [3] | [7] | бронировать |

Exact typed TRAIN сохранён в [fixtures/T-TRAIN-CONTEXT.train.json](fixtures/T-TRAIN-CONTEXT.train.json). A получает те же две записи после удаления только двух context-массивов; B получает их целиком. Содержимое EXAM никогда не добавляется в память. `BuildDefault` не вызывается.

| EXAM ID | Фраза для человека | Native input: SourceWord; before; after | Gold |
|---|---|---|---|
| Q1 | a red book fell | book; [0,9]; [8] | книга |
| Q2 | they will book flights | book; [3,4]; [7] | бронировать |
| Q3 | this old book vanished | book; [0,9]; [8] | книга |
| Q4 | you can book cheap tickets | book; [3,4]; [9,7] | бронировать |

Входы и gold находятся раздельно: [fixtures/T-TRAIN-CONTEXT.exam.json](fixtures/T-TRAIN-CONTEXT.exam.json), [fixtures/T-TRAIN-CONTEXT.gold.json](fixtures/T-TRAIN-CONTEXT.gold.json). Gold используется только после native ответа. Сам reader получает одно слово и два массива, а не полную фразу. Q1/Q3 — один и тот же вход на уровне learner: всего три различных EXAM tuple, два значения слова. Это узкая проверка выбора усвоенной метки по расширенному контексту, не четыре независимые задачи и не свободный перевод предложения.

## 3. Точные native вызовы

Ниже контракт будущего минимального caller, использующего **оригинальный класс**, а не собственную реализацию distance или перевода. Пустое начало — два независимых `new ContextualDictionary()`. Конструктор с default k=3 одинаков у обоих, параметр не меняется.

```csharp
using SemanticCompiler;

var a = new ContextualDictionary();
var b = new ContextualDictionary();

// Read the same four EXAM tuples through TranslateWithContext:
// first a -> A0, then b -> B0. Do not call AddEntry for EXAM.
a.AddEntry(new ContextualDictionary.DictionaryEntry(
    "book", Array.Empty<int>(), Array.Empty<int>(), "книга", 1));
a.AddEntry(new ContextualDictionary.DictionaryEntry(
    "book", Array.Empty<int>(), Array.Empty<int>(), "бронировать", 1));
b.AddEntry(new ContextualDictionary.DictionaryEntry(
    "book", new[] { 0 }, new[] { 8 }, "книга", 1));
b.AddEntry(new ContextualDictionary.DictionaryEntry(
    "book", new[] { 3 }, new[] { 7 }, "бронировать", 1));

// Exact EXAM calls per state, Q1-Q4 in that order:
// reader.TranslateWithContext("book", new[] {0, 9}, new[] {8});
// reader.TranslateWithContext("book", new[] {3, 4}, new[] {7});
// reader.TranslateWithContext("book", new[] {0, 9}, new[] {8});
// reader.TranslateWithContext("book", new[] {3, 4}, new[] {9, 7});
// Repeat unchanged after training: a -> A1, b -> B1.
```

Этот фрагмент фиксирует actual API и данные, но не объявляется запущенным runner. В текущем окружении `dotnet` не найден, native бинарник не собирался. Результаты будущего запуска сохраняются в новом run-каталоге рядом с этой карточкой; исходные snapshots и protected каталог остаются read-only. До запуска runner должен содержать только загрузку указанных fixture, приведённые оригинальные вызовы, чтение результатов/состояния и измерение времени/ресурсов. Перенос алгоритма в Python/JS не считается выполнением.

Снимок actual private `_entries` допустимо прочитать без изменения через один диагностический reflection accessor, закреплённый на этом имени и типе; иначе отчёт честно называется журналом входов AddEntry, а не экспортом actual memory. У класса нет публичного ExportCheckpoint. Чтение до/после является частью этого обучения, отдельным диагностическим экспериментом оно не становится.

## 4. Последовательность и бюджет

1. Проверить pinned source и идентичность A/B reader; загрузить фиксированные TRAIN и EXAM inputs. Gold отдельно.
2. Создать обе пустые памяти. Получить A0 и B0 на Q1–Q4 через native TranslateWithContext. Сохранить raw строки.
3. Записать T1, T2 один раз в каждое плечо: A без context, B с context. Зафиксировать четыре AddEntry и фактические записи.
4. На неизменных Q1–Q4 получить A1 и B1.
5. Отдельно сопоставить raw outputs с gold и сохранить результаты по строкам, включая потери.

Предел — **4 AddEntry, 16 TranslateWithContext, 0 optimizer updates, 0 Generate/Flow**. Повторное использование baseline и дополнительные label-shuffle/cold/replay проверки не требуются. JIT/build/startup не скрываются в «стоимости обучения»; отдельно фиксируются подготовка и собственные интервалы AddEntry/TranslateWithContext. Для настолько малого опыта низкое разрешение таймера или единственный allocation sample надо показать явно; нельзя выдавать округлённый 0 ms за бесплатное обучение.

## 5. Что измеряем до и после

| Полезный показатель | A0/A1/B0/B1 | Смысл и ограничение |
|---|---|---|
| Правильные /4, неправильные /4 | Raw output каждой Q; aggregate отдельно | Один ответ — перевод выделенного слова |
| Правильные на трёх уникальных tuples | Отдельная таблица Q1=Q3 | Не считать повтор Q3 независимым свидетельством |
| Выбор каждого из двух значений | Counts noun/verb отдельно | Общий средний результат не скрывает потерю одного значения |
| Время native обучения и ответа | Actual timestamps, частота таймера, число вызовов | Сравнимые интервалы и одинаковое число вызовов |
| CPU, allocation/peak RAM, если надёжно доступны | Единицы и конкретный измеритель | Неизмеренное = NOT_MEASURED; VRAM неприменима |
| Число сохранённых записей, POS ints, bytes сериализованного снимка | Пусто до, actual после | POS-preserving состояние может стоить больше памяти; это регрессия по стоимости |
| Фактические learning calls | Expected4 против actual | Вызов и содержимое writer, не декоративное имя Train |

Для каждого M вывести `A1−A0`, `B1−B0`, `(B1−B0)−(A1−A0)`, а также A1/B1. Направление пользы объявлено заранее: больше correct, меньше wrong; меньше времени/CPU/памяти при сопоставимой работе. Сохранение большего числа POS ints само по себе — не успех качества, а механическое объяснение различия состояния.

**Правило пользователя для этой карточки:** любое фактически измеренное улучшение полезного показателя — успех по этому показателю и данному домену; рост accuracy не обязателен. Остальные показатели, включая ухудшения, показываются рядом. При zero effect настоящий выполненный опыт остаётся содержательным. Если нет native AddEntry, фактор не достигает writer или reader заменён вычислением gold, результат недействителен.

Ожидание, выведенное из кода, — сохранённые контексты могут различить две метки, потерянные контексты дают tie и зависимость от одинаковых priority/порядка. Это **гипотеза до запуска**, не измеренные 2/4 или 4/4. Никаких actual чисел сейчас нет.

## 6. Полный промпт исполнителю

> Работай сначала на точной экспериментальной копии исходного механизма по правилу a822: A — неизменённый алгоритм, B — одна правка в копии, общие зависимости и состояния одинаковы. Основной код и продуктовые тесты не меняй; оригинальные пути ниже — provenance и будущий адрес переноса. Сохрани исходные admission и mutation authority, не имитируй их. Покажи результаты и точный diff переноса; даже успех не разрешает автоматически менять основной код или запускать продуктовый цикл. Защищённый каталог остаётся read-only; runtime evidence размещай в разрешённом каталоге вне него. 
>
> Проверь T-TRAIN-CONTEXT-01 на core bf9b543e149ef197da85a767821d98c9b67916d2. Прочитай полный original 12.SemanticCompiler/Translation/ContextualDictionary.cs и эту карточку; source SHA256 9926db1ed06111c7f64fd80f69cf6b81e28af110df77a4c4a452cf46d8313525. Ученик — этот native ContextualDictionary, без языковой модели. Используй два typed TRAIN и четыре EXAM tuple, приведённые в карточке и fixtures. Создай два пустых экземпляра. Меняй только сохранение ContextBefore/ContextAfter при AddEntry: A хранит пустые массивы, B исходные POS; все query POS одинаковые и настоящие в обоих плечах. SourceWord, Translation, Priority=1, порядок T1/T2 и reader неизменны.
>
> Сохрани четыре состояния A0/A1/B0/B1, сделай ровно 4 native AddEntry и 16 native TranslateWithContext. Не вызывай BuildDefault, не добавляй EXAM в память, не подменяй алгоритм Python/JS и не подавай gold learner. Сохрани raw outputs, реальный writer-call журнал, actual state если доступно только на чтение, counts, интервалы времени и доступные ресурсы; неизвестные оси пометь NOT_MEASURED. Раздельно покажи четыре строки и три уникальных query tuples. Полные предложения здесь служат человеку; learner переводит только book.
>
> Любое измеренное полезное улучшение считается успехом по своему показателю и этому домену; рост accuracy не требуется. Покажи ухудшения рядом. Нулевой эффект не скрывай, дополнительных запусков ради выигрыша не делай. Если конкретная попытка подготовить native runtime в пределах поручения не удалась либо оригинальный writer не достигнут, сохрани попытку и причину, отметь NOT_RUN для невыполненной части и продолжи независимую готовую работу. Одно отсутствие команды в PATH не объявляй внешним блокером; не записывай придуманные метрики. Отчёт не должен объявлять восстановление LoRA, production TranslationExecutor, обучение извлечению POS или свободному переводу: эти пути не измеряются. Защищённую docs/plans/replanning-possible-diffs не меняй.

## 7. Связь с существующими документами

Актуальный [protected T L62–66](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/03-translation.md#L62-L66) меняет query-контекст и прямо ограничивает вывод выбором усвоенного значения. Его [действующее правило L3–9](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/03-translation.md#L3-L9) допускает зависимые участки до/после update и отменяет повторное согласование каждой карточки. Этот T нельзя исключать только по отсутствию различий в writer. SHA256 целиком прочитанного latest protected файла `8e15169b0a029ebb977851a8467522a0ea153c7ab58d44c966e912713b9cc8f6`.

Контекстная dictionary/Any-overlap/missing-context/`_k` тема уже покрыта P43/K44 и [active09](https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/09-additions-44-48.md). Мы не заявляем новую реализационную причину RP01. Добавление — **самостоятельный учебный A/B в точке записи**, которого исходный T не формулирует. Он имеет более прямой приоритет для проверки самого обучения; исходный T сохраняет свою отдельную ценность на стороне использования памяти.

