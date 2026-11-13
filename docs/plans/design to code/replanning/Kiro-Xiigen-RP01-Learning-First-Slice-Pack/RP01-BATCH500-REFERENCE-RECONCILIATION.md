# RP01 BATCH500 — свежие артефакты и осторожное использование legacy

15.10.2026. Это source reconciliation для нового поручения, не новый цикл R01–R25. [Основной контракт](RP01-OFFLINE-TEACHER-BATCH500.md), [task index](tasks/batch500/README.md). Новые training/data/model claims этой публикацией не создаются.

## Что принёс пользователь во время правки

Core commit [ac9ce15ea39337a3f652266215cd396651f6671d — pr1 artifacts](https://github.com/lubochka/llm_mvp_core/commit/ac9ce15ea39337a3f652266215cd396651f6671d) реально открыт через GitHub. Прочитаны metadata и отдельные returned artifact fragments, в том числе `RP01.01A-exact-material-20261013/EXECUTOR-RETURN.json` и `PUBLICATION-RETURN.json` из diff. Наличие этих артефактов не является новым доказательством BATCH500 runtime. Весь commit и все его файлы здесь НЕ объявляются полностью проверенными. Local private held bundle, tensors и examiner outputs здесь заново не запускались/не проверялись.

В quoted handoff исполнитель уже принял: first C#500; затем другие домены через реальные consumers; честное Astra synthetic authorship; новый опыт без +2/12; убрать Qwen и в author, и в evaluator; сначала измерить короткий local step. Это `EXECUTOR_REPORTED_INTENT`, не подтверждение реализации или обучения. B500.02/B500.03 сначала переиспользуют его latest handoff/diff, а не запускают второго writer.

Пользовательское уточнение `PLAN-DOTNET-LEGACY-DPO-v1.md` означает **historical reference only**, не действующий algorithm/gate authority. Файл найден в Library, версия1, заголовок даты2026-05-11, 513 строк прочитаны. Canonical GitHub path этого конкретного Library snapshot здесь не установлен; не выдумывать ссылку и не объявлять его текущим core code. Исходный файл не изменялся и не копировался в новый TRAIN.

## Что взять, что не переносить

| Старый материал | Решение для нового режима | Почему / точный риск |
|---|---|---|
| §§1–2: различать framework-era и source/pattern families; dedup до дорогостоящей генерации | Переиспользовать принцип после domain-specific validation | Счётчик500 переименований одного шаблона не означает широкого curriculum; версия влияет на закон домена |
| §2: regex strips comments, replaces numeric/string literals, hash truncated SHA1 | Только подсказка для candidate buckets; не authoritative semantic identity | `return 0` и `return 1` не должны слиться, если число — смысл задачи; regex может повредить строку с `//`; semantic equivalence требует syntax/context. Exact judged TRAIN bytes никогда не нормализовать |
| §2/5: максимум3 паттерна на cluster, token overlap .50/.60 как discard gate | Не импортировать как универсальные пороги | Близкая correct/near-miss пара намеренно может отличаться одним `not`; высокая лексическая общность не делает preference плохим. Dedup и полезная минимальная пара — разные вопросы |
| §4: `[WRONG:]` annotation прямо в rejected | Причину ошибки хранить в sidecar `targetedViolation`/J rationale; не в response tokens | Иначе Student может научиться обнаруживать метку, а не доменный дефект. Actual chosen/rejected должны быть естественными ответами в одинаковом формате |
| §4: intent/why/edge-case structure | Переиспользовать как критерии содержательной полезности, где source это доказывает | Нельзя изобретать “почему автор так решил” или исключение конкретного framework без source evidence |
| §7: 16→32, margin .010, accuracy .60/.70, V246 .016892 | Historical only; НЕ критерии BATCH500 | Другие objective/tokenization/model/splits/denominators. Ни числа, ни старый прирост не переносим в новую архитектуру |
| §0/9: таблица источников, лицензий, legacy tags | Сохранять как список для будущей отдельной проверки, не current permission | Для нового original-synthetic curriculum не нужно клонировать все эти repos. Для реальных внешних samples нужны точные source/version/rights; старый список не заменяет проверку |
| §1: substring marker автоматически выбирает domain | Retrieval heuristic only, не learned domain mapping | `ISession`, `IContainer`, `SqlCommand` или слово в комментарии не доказывают единственный домен; нужен type/context/consumer |
| §8: исключить interfaces/config/exception-only методы | Не переносить как границы всей XIIGEN программы | Эти поверхности могут быть отдельными нужными доменами; текущий C# pilot не удаляет их из общего product scope |

## Обязательная compatibility row при использовании legacy

Для каждого реально перенесённого положения записать: exact source/version/span; current input/output semantics; current producer→learned state→consumer; objective/masks/tokenizer/reference differences; domain-law applicability; применяемый design principle; отклонённые thresholds/assumptions; planned discriminating observation. Пустая строка совместимости означает reference-only, не permission ждать ещё один полный audit.

GOOD: взять family-level dedup, сохранив значения и отрицания; judge различает правдоподобную предметную ошибку. BAD: перенести regex signature как векторизацию или `.60` как обязательный success gate. GOOD: historical source сообщает старый результат с другим baseline. BAD: назвать старый V246 gain прогнозом эффекта одного нового LoRA step.

## Финальное уточнение BATCH500 после handoff

1. Первое массовое обучение — **C#500**. EN→RU500 — следующий domain, подготовка его данных/consumer может быть параллельной; первый шаг не ждёт второго домена.
2. Для скорости разрешён отдельный TRAIN-only probe1–4 ДО shard50. Это не требование уже показать quality gain. После него первый useful shard50 и следующие portions доходят до500 через одну research lineage.
3. В новом inner loop нет local Qwen **author и evaluator**, и нет обязательного прежнего calibration60. Astra author/J/E работают отдельными assignments над сохранёнными материалами; actual receipts обязательны.
4. Малый early gain определяется заранее выбранной содержательной Q_law по новым сохранённым ответам. Частичное исправление закона допустимо. +1/20 fully correct — достаточный пример, не новая обязательная квота. Train loss/margin без изменения качества — отдельная диагностика.
5. Ни новый план, ни небольшой observed gain не переписывают старый verdict, не доказывают significance и не разрешают автоматически production promotion.

При противоречии ранней фразы в основном BATCH500 документе этим уточнениям применяются конкретные новые task contracts: [TRAIN](tasks/batch500/RP01.B500.03-TRAIN.md), [EXAM](tasks/batch500/RP01.B500.04-EXAM.md). Общий dispatcher будет ссылаться на них; нового альтернативного launcher здесь нет.

## Граница текущей работы

Реально сделано этой сессией: прочитаны repo documents/selected skills, опубликованы planning changes через connected GitHub. Не сделано этой сессией: product code edits, независимое отдельное Astra review, генерация1000 допущенных пар, local Student training, numerical tensor/PC measurements. Артефакты executor сохраняют его provenance и не считаются нашей независимой повторной проверкой.
