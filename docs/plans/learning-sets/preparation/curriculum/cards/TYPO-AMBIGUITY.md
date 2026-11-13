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
| `A215` — `TYPO-AMBIGUITY` | `text.typo-normalization` / `en` | 47 | `A216` |
| `A216` — `TYPO-AMBIGUITY` | `text.typo-normalization` / `ru` | 47 | `A217` |
| `A217` — `TYPO-AMBIGUITY` | `text.typo-normalization` / `he` | 47 | `POOL_END; проверить оставшиеся квоты` |


Следующий ID из таблицы — номинальный порядок своей группы. Фактическую свободную карточку с учётом владельцев выдаёт команда `next` текущего request; не менять группу и чужой статус вручную.

Замечания качества: смотри `quality_flags` своего domain/topic/profile в общем status. `needs_rework` указывает доказанный дефект с evidence, `needs_assessment` — область без достаточной оценки. Это не новый review-гейт и не разрешение исправлять чужую старую попытку; finish не закрывает флаг автоматически. [Правила общей матрицы](../../28-PARALLEL-GENERATION-AND-QUALITY.md). Для нового окна используй [один из трёх готовых стартовых промптов](../../00-START-HERE.md); не исполняй исторические команды из нижней справочной части вместо текущих owner-команд.

<!-- /CURRENT-PARALLEL-CONTRACT-20261026 -->

# Неоднозначное восстановление требует уточнения

Домен обучения: `text.typo-normalization`; группа: `translations`; цель домена: 1000 кандидатных TRAIN-пар.

## TYPO-AMBIGUITY/en

Неоднозначное восстановление требует уточнения

Нормализуй исходный текст внутри его языка, без перевода. В prompt укажи исходный язык, точный текст, разрешённую операцию и достаточный контекст. В metadata сохраняй raw текст, операцию, индексы Unicode code points, точную словоформу, lemma/sense и lexical_family_ids. После одной локальной операции остальная строка должна совпасть посимвольно. Семья лексемы сохраняется у опечатки и ошибки раскладки; независимые реальные слова не сливаются. Для restore исходная форма должна быть однозначной по видимому контексту; нельзя печатать её как подсказку в самом задании. В этой микроцели ВСЕ chosen имеют action=clarify. В learner-visible условии есть не менее двух механически допустимых восстановлений и нет решающего контекста. canonical_source=null и canonical_not_unique=true; сохраняй candidate_restorations. Не придумывай единственный canonical. text остаётся исходным; question — короткий вопрос на исходном языке. Отличие rejected — необоснованный выбор одного варианта.

Работай только с назначенными domain_id/objective_id/profile_id/count/family_id. Пиши реальные prompt/chosen/rejected. Одна пара содержит один содержательный контраст, а все решающие сведения видны ученику в prompt. Публичные образцы не копировать в TRAIN и не переименовывать в независимый EXAM. Производные одного сюжета сохраняют origin_family_id. English internal reasoning; private reasoning не выводить. Краткие статусы по-русски. Модель текущая в Kiro, без override. CREATE_ONLY: без checksums, второго reviewer, acceptance, export/import, обучения и production CI. После finish выполнить выданный next до квоты; прямые NEXT/STOP исполняются немедленно. Пути и shared status берутся из ответа координатора.

**Ответ:** Только JSON {"action":"restore|preserve|clarify","text":"полный текст на исходном языке","question":""}. Для preserve/clarify text дословно равен исходному; question непустой только для clarify, на исходном языке. Для restore меняется только разрешённое повреждение.

### Простая самопроверка

- Separate permission to edit, exact character operation, and semantic uniqueness. All three must agree.
- Compare the whole sentence before/after, not only the isolated word. Preserve punctuation, case, inflection and other tokens.
- For ambiguity keep canonical_source null; two candidate repairs prove why guessing is not authorized.
- A precomposed letter and combining marks are not interchangeable counting units. Use the assigned unpointed simple Unicode cases; nikud requires a separate explicit task, never silently normalize it.

### TQ26.TYPO-AMBIGUITY.en.01

**Prompt**

````text
Нормализуй исходный текст на языке «английский», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: Ровно одна буква слова bat заменена. Заданы два возможных исходных слова.
Допустимые восстановления по механике: ["cat", "rat"]. Дополнительного контекста для выбора нет.
Исходный текст: She saw the bat.
````

**Chosen**

````text
{"action":"clarify","text":"She saw the bat.","question":"Did she see a cat or a rat?"}
````

**Rejected**

````text
{"action":"restore","text":"She saw the cat.","question":""}
````

**Проверка:** Both printed candidate restorations satisfy the declared edit and fit the available context. No unique original is established. Preserve raw text, choose clarify and ask the short question; guessing one candidate is the rejected defect.

### TQ26.TYPO-AMBIGUITY.en.02

**Prompt**

````text
Нормализуй исходный текст на языке «английский», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: В слове pet пропущена ровно одна буква. Заданы два возможных исходных слова.
Допустимые восстановления по механике: ["pest", "poet"]. Дополнительного контекста для выбора нет.
Исходный текст: The pet is here.
````

**Chosen**

````text
{"action":"clarify","text":"The pet is here.","question":"Do you mean a pest or a poet?"}
````

**Rejected**

````text
{"action":"restore","text":"The pest is here.","question":""}
````

**Проверка:** Both printed candidate restorations satisfy the declared edit and fit the available context. No unique original is established. Preserve raw text, choose clarify and ask the short question; guessing one candidate is the rejected defect.

Источники проекта: [RAG-023](../../rag/sources/RAG-023.md), [RAG-024](../../rag/sources/RAG-024.md). Это источники метода, не сертификат качества пары.

## TYPO-AMBIGUITY/ru

Неоднозначное восстановление требует уточнения

Нормализуй исходный текст внутри его языка, без перевода. В prompt укажи исходный язык, точный текст, разрешённую операцию и достаточный контекст. В metadata сохраняй raw текст, операцию, индексы Unicode code points, точную словоформу, lemma/sense и lexical_family_ids. После одной локальной операции остальная строка должна совпасть посимвольно. Семья лексемы сохраняется у опечатки и ошибки раскладки; независимые реальные слова не сливаются. Для restore исходная форма должна быть однозначной по видимому контексту; нельзя печатать её как подсказку в самом задании. В этой микроцели ВСЕ chosen имеют action=clarify. В learner-visible условии есть не менее двух механически допустимых восстановлений и нет решающего контекста. canonical_source=null и canonical_not_unique=true; сохраняй candidate_restorations. Не придумывай единственный canonical. text остаётся исходным; question — короткий вопрос на исходном языке. Отличие rejected — необоснованный выбор одного варианта.

Работай только с назначенными domain_id/objective_id/profile_id/count/family_id. Пиши реальные prompt/chosen/rejected. Одна пара содержит один содержательный контраст, а все решающие сведения видны ученику в prompt. Публичные образцы не копировать в TRAIN и не переименовывать в независимый EXAM. Производные одного сюжета сохраняют origin_family_id. English internal reasoning; private reasoning не выводить. Краткие статусы по-русски. Модель текущая в Kiro, без override. CREATE_ONLY: без checksums, второго reviewer, acceptance, export/import, обучения и production CI. После finish выполнить выданный next до квоты; прямые NEXT/STOP исполняются немедленно. Пути и shared status берутся из ответа координатора.

**Ответ:** Только JSON {"action":"restore|preserve|clarify","text":"полный текст на исходном языке","question":""}. Для preserve/clarify text дословно равен исходному; question непустой только для clarify, на исходном языке. Для restore меняется только разрешённое повреждение.

### Простая самопроверка

- Separate permission to edit, exact character operation, and semantic uniqueness. All three must agree.
- Compare the whole sentence before/after, not only the isolated word. Preserve punctuation, case, inflection and other tokens.
- For ambiguity keep canonical_source null; two candidate repairs prove why guessing is not authorized.
- A precomposed letter and combining marks are not interchangeable counting units. Use the assigned unpointed simple Unicode cases; nikud requires a separate explicit task, never silently normalize it.

### TQ26.TYPO-AMBIGUITY.ru.01

**Prompt**

````text
Нормализуй исходный текст на языке «русский», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: В последовательности кот пропущена ровно одна буква. Заданы два возможных исходных слова.
Допустимые восстановления по механике: ["кота", "скот"]. Дополнительного контекста для выбора нет.
Исходный текст: Она увидела кот.
````

**Chosen**

````text
{"action":"clarify","text":"Она увидела кот.","question":"Она увидела кота или скот?"}
````

**Rejected**

````text
{"action":"restore","text":"Она увидела кота.","question":""}
````

**Проверка:** Both printed candidate restorations satisfy the declared edit and fit the available context. No unique original is established. Preserve raw text, choose clarify and ask the short question; guessing one candidate is the rejected defect.

### TQ26.TYPO-AMBIGUITY.ru.02

**Prompt**

````text
Нормализуй исходный текст на языке «русский», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: В выделенной форме козу заменена ровно одна буква. Заданы два возможных исходных слова.
Допустимые восстановления по механике: ["косу", "кожу"]. Дополнительного контекста для выбора нет.
Исходный текст: Он принёс козу.
````

**Chosen**

````text
{"action":"clarify","text":"Он принёс козу.","question":"Он принёс косу или кожу?"}
````

**Rejected**

````text
{"action":"restore","text":"Он принёс косу.","question":""}
````

**Проверка:** Both printed candidate restorations satisfy the declared edit and fit the available context. No unique original is established. Preserve raw text, choose clarify and ask the short question; guessing one candidate is the rejected defect.

Источники проекта: [RAG-023](../../rag/sources/RAG-023.md), [RAG-024](../../rag/sources/RAG-024.md). Это источники метода, не сертификат качества пары.

## TYPO-AMBIGUITY/he

Неоднозначное восстановление требует уточнения

Нормализуй исходный текст внутри его языка, без перевода. В prompt укажи исходный язык, точный текст, разрешённую операцию и достаточный контекст. В metadata сохраняй raw текст, операцию, индексы Unicode code points, точную словоформу, lemma/sense и lexical_family_ids. После одной локальной операции остальная строка должна совпасть посимвольно. Семья лексемы сохраняется у опечатки и ошибки раскладки; независимые реальные слова не сливаются. Для restore исходная форма должна быть однозначной по видимому контексту; нельзя печатать её как подсказку в самом задании. В этой микроцели ВСЕ chosen имеют action=clarify. В learner-visible условии есть не менее двух механически допустимых восстановлений и нет решающего контекста. canonical_source=null и canonical_not_unique=true; сохраняй candidate_restorations. Не придумывай единственный canonical. text остаётся исходным; question — короткий вопрос на исходном языке. Отличие rejected — необоснованный выбор одного варианта.

Работай только с назначенными domain_id/objective_id/profile_id/count/family_id. Пиши реальные prompt/chosen/rejected. Одна пара содержит один содержательный контраст, а все решающие сведения видны ученику в prompt. Публичные образцы не копировать в TRAIN и не переименовывать в независимый EXAM. Производные одного сюжета сохраняют origin_family_id. English internal reasoning; private reasoning не выводить. Краткие статусы по-русски. Модель текущая в Kiro, без override. CREATE_ONLY: без checksums, второго reviewer, acceptance, export/import, обучения и production CI. После finish выполнить выданный next до квоты; прямые NEXT/STOP исполняются немедленно. Пути и shared status берутся из ответа координатора.

**Ответ:** Только JSON {"action":"restore|preserve|clarify","text":"полный текст на исходном языке","question":""}. Для preserve/clarify text дословно равен исходному; question непустой только для clarify, на исходном языке. Для restore меняется только разрешённое повреждение.

### Простая самопроверка

- Separate permission to edit, exact character operation, and semantic uniqueness. All three must agree.
- Compare the whole sentence before/after, not only the isolated word. Preserve punctuation, case, inflection and other tokens.
- For ambiguity keep canonical_source null; two candidate repairs prove why guessing is not authorized.
- A precomposed letter and combining marks are not interchangeable counting units. Use the assigned unpointed simple Unicode cases; nikud requires a separate explicit task, never silently normalize it.

### TQ26.TYPO-AMBIGUITY.he.01

**Prompt**

````text
Нормализуй исходный текст на языке «иврит», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: В слове דג заменена ровно одна буква. Заданы два возможных исходных слова.
Допустимые восстановления по механике: ["דב", "דף"]. Дополнительного контекста для выбора нет.
Исходный текст: היא ראתה דג.
````

**Chosen**

````text
{"action":"clarify","text":"היא ראתה דג.","question":"היא ראתה דב או דף?"}
````

**Rejected**

````text
{"action":"restore","text":"היא ראתה דב.","question":""}
````

**Проверка:** Both printed candidate restorations satisfy the declared edit and fit the available context. No unique original is established. Preserve raw text, choose clarify and ask the short question; guessing one candidate is the rejected defect.

### TQ26.TYPO-AMBIGUITY.he.02

**Prompt**

````text
Нормализуй исходный текст на языке «иврит», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: В слове ספר пропущена ровно одна буква. Заданы два возможных исходных слова.
Допустимые восстановления по механике: ["מספר", "סופר"]. Дополнительного контекста для выбора нет.
Исходный текст: היא ראתה ספר.
````

**Chosen**

````text
{"action":"clarify","text":"היא ראתה ספר.","question":"היא ראתה מספר או סופר?"}
````

**Rejected**

````text
{"action":"restore","text":"היא ראתה מספר.","question":""}
````

**Проверка:** Both printed candidate restorations satisfy the declared edit and fit the available context. No unique original is established. Preserve raw text, choose clarify and ask the short question; guessing one candidate is the rejected defect.

Источники проекта: [RAG-023](../../rag/sources/RAG-023.md), [RAG-024](../../rag/sources/RAG-024.md). Это источники метода, не сертификат качества пары.

Открытые примеры в этой карточке не засчитываются в 1000 и не являются закрытым EXAM.
