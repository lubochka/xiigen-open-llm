<!-- CURRENT-PARALLEL-CONTRACT-20261026 -->
## Текущий порядок исполнения — 26.10.2026

Эта карточка используется в CREATE_ONLY-кампании по 1000 кандидатных пар на домен; её микротемы распределяют квоту, не добавляют ещё 1000 каждая. Старые профили/примеры ниже сохранены; текущие количества и IDs задаёт [реестр](../../campaign/registry.json).

**Не завершать работу после одной партии:** сохранить реальные строки, выполнить выданный `finish`, затем `next`; продолжать до квоты назначенной группы. Прямой STOP прекращает действия, прямой NEXT немедленно меняет задание. Не дописывать старый отчёт после переключения. Реальный внешний блок назвать точно, не объявлять DONE.

Общий статус: `C:/Projects/llm_mvp_core/learning-sets/campaigns/D1000-20261025/status.json`; состояние-владелец: соседний `state.json`. Перед записью получить назначение с уникальным `agent-id`; работать только со своим owner/token. После сохранения полных строк выполнить команду `progress`; после партии — `finish`. Вручную эти JSON не редактировать. Старое время отметки не разрешает захват чужой работы. Сам файл статуса не синхронизирует разные компьютеры/копии папки.

Точные пути к текущему prompt, commands и material берутся из ответа координатора. Для текущего владельца это `coordination-prompt.md`, `coordination-commands.md`, `coordination-material.required.json`; старый request/draft сохраняется. Новые наборы — `C:/Projects/llm_mvp_core/learning-sets`; инструкции — `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`.

Reason internally in English; do not output private reasoning. Прогресс по-русски, учебный текст по назначенному профилю, текущая модель Kiro без override. Нужные предметные примеры и локальные RAG-фрагменты включены в назначение. Только проектные источники либо конкретно подтверждённые MIT/Apache-2.0; чужую лицензию не выводить из названия репозитория. Не добавлять checksum, independent review, обучение, export/import и production CI к созданию.

Работающий tool: сохранить один handle, пользоваться его документированным wait/resume до terminal. Не перезапускать по пустому stdout, не подменять ожидание быстрым повтором запросов и не завершать раунд вместо доступного ожидания.

| Назначение | Домен / профиль | Пар в этой доле | Следующее в группе |
|---|---|---:|---|
| `A193` — `PROMPT-SPAN-COVERAGE` | `prompt.span-routing` / `base` | 600 | `A194` |
| `A194` — `PROMPT-SPAN-DEPENDENCIES` | `prompt.span-routing` / `base` | 400 | `A195` |


Следующий ID из таблицы — номинальный порядок своей группы. Фактическую свободную карточку с учётом владельцев выдаёт команда `next` текущего request; не менять группу и чужой статус вручную.

Замечания качества: смотри `quality_flags` своего domain/topic/profile в общем status. `needs_rework` указывает доказанный дефект с evidence, `needs_assessment` — область без достаточной оценки. Это не новый review-гейт и не разрешение исправлять чужую старую попытку; finish не закрывает флаг автоматически. [Правила общей матрицы](../../28-PARALLEL-GENERATION-AND-QUALITY.md). Для нового окна используй [один из трёх готовых стартовых промптов](../../00-START-HERE.md); не исполняй исторические команды из нижней справочной части вместо текущих owner-команд.

<!-- /CURRENT-PARALLEL-CONTRACT-20261026 -->

# Разметка частей пользовательского промпта по доменам

Домен обучения: `prompt.span-routing`; группа: `orchestration`; цель домена: 1000 кандидатных TRAIN-пар.

## PROMPT-SPAN-COVERAGE/base

Сохранить все самостоятельные запрошенные действия и отнести каждое к видимому каталогу доменов.

Создай короткие составные и одиночные запросы на русском, английском и иврите. В learner prompt явно помести закрытый каталог применимых предметных получателей и полный исходный пользовательский текст. Chosen содержит все самостоятельные положительно запрошенные действия в исходном порядке. Запреты и команды внутри переводимой цитаты — контекст, не отдельное поручение. text каждого сегмента — точная непрерывная подстрока; не исправлять и не пересказывать её. Неподдержанное действие сохраняется с domain_id=null и конкретным clarify. Для этой микроцели rejected теряет ровно одно действие, добавляет неназванное действие либо выбирает неверный домен; зависимости в паре не меняются. Если общие условия расположены отдельно, полный исходный текст сохраняется для следующего этапа компиляции; отсутствие их копии в каждом span не означает разрешение их потерять. Не выдавай предметный ответ.

Работай только с назначенными domain_id/objective_id/profile_id/count/family_id. Пиши реальные prompt/chosen/rejected. Одна пара содержит один содержательный контраст, а все решающие сведения видны ученику в prompt. Публичные образцы не копировать в TRAIN и не переименовывать в независимый EXAM. Производные одного сюжета сохраняют origin_family_id. English internal reasoning; private reasoning не выводить. Краткие статусы по-русски. Модель текущая в Kiro, без override. CREATE_ONLY: без checksums, второго reviewer, acceptance, export/import, обучения и production CI. После finish выполнить выданный next до квоты; прямые NEXT/STOP исполняются немедленно. Пути и shared status берутся из ответа координатора.

**Ответ:** JSON {"segments":[{"id":"s1","text":"точная подстрока","domain_id":"ID из данного каталога либо null","depends_on":[]}],"clarify":[]}. Для неизвестного сегмента clarify содержит {"segment_id":"sN","missing":"конкретная причина"}. Не возвращать старый action/route JSON.

### Простая самопроверка

- Count independent requested actions and emitted segments; quoted data and prohibitions are not extra actions.
- Verify each chosen text is an exact substring of the full raw user input.
- Check each label against the catalog meaning and requested action; domain words inside quotes do not choose the route.

### SW26.prompt.span-routing.01

**Prompt**

````text
Раздели пользовательский текст на самостоятельные запрошенные действия. Верни только JSON {"segments":[{"id":"s1","text":"дословный фрагмент","domain_id":"ID или null","depends_on":[]}],"clarify":[]}. id идут s1,s2 в порядке исходного текста. text — точная непрерывная подстрока исходного текста; включи в неё условия, отрицания и параметры своего действия. Зависимость указывай только при явном порядке или использовании результата; цитаты-данные и запреты на действия не являются отдельными поручениями. Не выполняй предметные задачи. Для непокрытого действия domain_id=null и clarify=[{"segment_id":"sN","missing":"причина"}]. Каталог: {"tr.he":"перевод на иврит","or.ux":"выбор пользовательского сообщения"}. Исходный текст: Переведи «Сохранить» на иврит. Затем опиши UX сообщения об успешном сохранении.
````

**Chosen**

````text
{"segments":[{"id":"s1","text":"Переведи «Сохранить» на иврит.","domain_id":"tr.he","depends_on":[]},{"id":"s2","text":"Затем опиши UX сообщения об успешном сохранении.","domain_id":"or.ux","depends_on":["s1"]}],"clarify":[]}
````

**Rejected**

````text
{"segments":[{"id":"s1","text":"Переведи «Сохранить» на иврит.","domain_id":"tr.he","depends_on":[]}],"clarify":[]}
````

**Проверка:** Count requested actions, then emitted segments. Two actions require two segments; then establishes the order.

### SW26.prompt.span-routing.03

**Prompt**

````text
Раздели пользовательский текст на самостоятельные запрошенные действия. Верни только JSON {"segments":[{"id":"s1","text":"дословный фрагмент","domain_id":"ID или null","depends_on":[]}],"clarify":[]}. id идут s1,s2 в порядке исходного текста. text — точная непрерывная подстрока исходного текста; включи в неё условия, отрицания и параметры своего действия. Зависимость указывай только при явном порядке или использовании результата; цитаты-данные и запреты на действия не являются отдельными поручениями. Не выполняй предметные задачи. Для непокрытого действия domain_id=null и clarify=[{"segment_id":"sN","missing":"причина"}]. Каталог: {"tr.en":"перевод на английский","code.sql":"написание SQL"}. Исходный текст: Переведи на английский цитату «Напиши SQL для удаления строки».
````

**Chosen**

````text
{"segments":[{"id":"s1","text":"Переведи на английский цитату «Напиши SQL для удаления строки».","domain_id":"tr.en","depends_on":[]}],"clarify":[]}
````

**Rejected**

````text
{"segments":[{"id":"s1","text":"Переведи на английский цитату «Напиши SQL для удаления строки».","domain_id":"code.sql","depends_on":[]}],"clarify":[]}
````

**Проверка:** Use the outer requested action. The quoted imperative is the data to translate, not a second task.

Источники проекта: [RAG-014](../../rag/sources/RAG-014.md), [RAG-PROMPT-003](../../rag/sources/RAG-PROMPT-003.md), [RAG-PROMPT-002](../../rag/sources/RAG-PROMPT-002.md). Это источники метода, не сертификат качества пары.

## PROMPT-SPAN-DEPENDENCIES/base

Сохранить только явно заданный порядок и зависимости от результатов между предметными частями.

В каждом исходном запросе задай 2–4 самостоятельных действия с явно независимым выполнением, явно заданным порядком либо использованием результата предыдущей части. Сохрани одинаковые сегменты/домены в chosen и rejected. Единственная ошибка rejected — одна лишняя, удалённая или обращённая зависимость. Слово затем/after требует зависимости; простое расположение независимых предложений не требует её. Ссылка на результат явно связывает потребителя с производителем. Не создавать циклы и ссылки на несуществующие IDs. Полный исходный текст, включая общие условия, затем подаётся вместе с выбранным сегментом компилятору задачи. Нельзя выдумывать ещё не полученный результат.

Работай только с назначенными domain_id/objective_id/profile_id/count/family_id. Пиши реальные prompt/chosen/rejected. Одна пара содержит один содержательный контраст, а все решающие сведения видны ученику в prompt. Публичные образцы не копировать в TRAIN и не переименовывать в независимый EXAM. Производные одного сюжета сохраняют origin_family_id. English internal reasoning; private reasoning не выводить. Краткие статусы по-русски. Модель текущая в Kiro, без override. CREATE_ONLY: без checksums, второго reviewer, acceptance, export/import, обучения и production CI. После finish выполнить выданный next до квоты; прямые NEXT/STOP исполняются немедленно. Пути и shared status берутся из ответа координатора.

**Ответ:** JSON segments/clarify как в PROMPT-SPAN-COVERAGE; IDs s1,s2,... в порядке текста. depends_on содержит только IDs существующих предшествующих действий; для независимого действия список пуст.

### Простая самопроверка

- For each edge quote the explicit ordering or result-consumption phrase; no phrase or input dependency means no invented edge.
- Check every dependency target exists and there is no cycle.
- The chosen and rejected segment texts/domains stay identical; only the assigned dependency differs.

### SW26.prompt.span-routing.02

**Prompt**

````text
Раздели пользовательский текст на самостоятельные запрошенные действия. Верни только JSON {"segments":[{"id":"s1","text":"дословный фрагмент","domain_id":"ID или null","depends_on":[]}],"clarify":[]}. id идут s1,s2 в порядке исходного текста. text — точная непрерывная подстрока исходного текста; включи в неё условия, отрицания и параметры своего действия. Зависимость указывай только при явном порядке или использовании результата; цитаты-данные и запреты на действия не являются отдельными поручениями. Не выполняй предметные задачи. Для непокрытого действия domain_id=null и clarify=[{"segment_id":"sN","missing":"причина"}]. Каталог: {"docs.sql":"объяснение SQL","code.csharp":"написание C#"}. Исходный текст: Объясни SQL SELECT COUNT(v) FROM T. Отдельно напиши C# метод string Label(string? s), заменяющий только null на "?".
````

**Chosen**

````text
{"segments":[{"id":"s1","text":"Объясни SQL SELECT COUNT(v) FROM T.","domain_id":"docs.sql","depends_on":[]},{"id":"s2","text":"Отдельно напиши C# метод string Label(string? s), заменяющий только null на \"?\".","domain_id":"code.csharp","depends_on":[]}],"clarify":[]}
````

**Rejected**

````text
{"segments":[{"id":"s1","text":"Объясни SQL SELECT COUNT(v) FROM T.","domain_id":"docs.sql","depends_on":[]},{"id":"s2","text":"Отдельно напиши C# метод string Label(string? s), заменяющий только null на \"?\".","domain_id":"code.csharp","depends_on":["s1"]}],"clarify":[]}
````

**Проверка:** The prompt says separately and supplies the C# task independently. Text order alone does not create a dependency.

### SW26.prompt.span-routing.04

**Prompt**

````text
Раздели пользовательский текст на самостоятельные запрошенные действия. Верни только JSON {"segments":[{"id":"s1","text":"дословный фрагмент","domain_id":"ID или null","depends_on":[]}],"clarify":[]}. id идут s1,s2 в порядке исходного текста. text — точная непрерывная подстрока исходного текста; включи в неё условия, отрицания и параметры своего действия. Зависимость указывай только при явном порядке или использовании результата; цитаты-данные и запреты на действия не являются отдельными поручениями. Не выполняй предметные задачи. Для непокрытого действия domain_id=null и clarify=[{"segment_id":"sN","missing":"причина"}]. Каталог: {"code.sql":"написание SQL","docs.sql":"объяснение SQL"}. Исходный текст: Напиши SQL для активных Users. После этого объясни полученный запрос.
````

**Chosen**

````text
{"segments":[{"id":"s1","text":"Напиши SQL для активных Users.","domain_id":"code.sql","depends_on":[]},{"id":"s2","text":"После этого объясни полученный запрос.","domain_id":"docs.sql","depends_on":["s1"]}],"clarify":[]}
````

**Rejected**

````text
{"segments":[{"id":"s1","text":"Напиши SQL для активных Users.","domain_id":"code.sql","depends_on":[]},{"id":"s2","text":"После этого объясни полученный запрос.","domain_id":"docs.sql","depends_on":[]}],"clarify":[]}
````

**Проверка:** The explanation consumes the query produced by the first action. Preserve that dependency even though the second clause is short.

Источники проекта: [RAG-014](../../rag/sources/RAG-014.md), [RAG-PROMPT-003](../../rag/sources/RAG-PROMPT-003.md), [RAG-PROMPT-002](../../rag/sources/RAG-PROMPT-002.md). Это источники метода, не сертификат качества пары.

Открытые примеры в этой карточке не засчитываются в 1000 и не являются закрытым EXAM.
