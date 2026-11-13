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
| `A197` — `TYPO-DELETE` | `text.typo-normalization` / `en` | 48 | `A198` |
| `A198` — `TYPO-DELETE` | `text.typo-normalization` / `ru` | 48 | `A199` |
| `A199` — `TYPO-DELETE` | `text.typo-normalization` / `he` | 48 | `A200` |


Следующий ID из таблицы — номинальный порядок своей группы. Фактическую свободную карточку с учётом владельцев выдаёт команда `next` текущего request; не менять группу и чужой статус вручную.

Замечания качества: смотри `quality_flags` своего domain/topic/profile в общем status. `needs_rework` указывает доказанный дефект с evidence, `needs_assessment` — область без достаточной оценки. Это не новый review-гейт и не разрешение исправлять чужую старую попытку; finish не закрывает флаг автоматически. [Правила общей матрицы](../../28-PARALLEL-GENERATION-AND-QUALITY.md). Для нового окна используй [один из трёх готовых стартовых промптов](../../00-START-HERE.md); не исполняй исторические команды из нижней справочной части вместо текущих owner-команд.

<!-- /CURRENT-PARALLEL-CONTRACT-20261026 -->

# Удаление одной буквы

Домен обучения: `text.typo-normalization`; группа: `translations`; цель домена: 1000 кандидатных TRAIN-пар.

## TYPO-DELETE/en

Удаление одной буквы

Нормализуй исходный текст внутри его языка, без перевода. В prompt укажи исходный язык, точный текст, разрешённую операцию и достаточный контекст. В metadata сохраняй raw текст, операцию, индексы Unicode code points, точную словоформу, lemma/sense и lexical_family_ids. После одной локальной операции остальная строка должна совпасть посимвольно. Семья лексемы сохраняется у опечатки и ошибки раскладки; независимые реальные слова не сливаются. Для restore исходная форма должна быть однозначной по видимому контексту; нельзя печатать её как подсказку в самом задании. Только один удалённый Unicode code point. Повреждённое слово на один code point короче; удаление по указанному индексу canonical_token даёт raw_token. Письмо→писмо подходит; письмо→пильмо не подходит. Если одинаковые соседние буквы дают несколько допустимых индексов, проверяй указанный индекс, не требуй единственности индекса.

Работай только с назначенными domain_id/objective_id/profile_id/count/family_id. Пиши реальные prompt/chosen/rejected. Одна пара содержит один содержательный контраст, а все решающие сведения видны ученику в prompt. Публичные образцы не копировать в TRAIN и не переименовывать в независимый EXAM. Производные одного сюжета сохраняют origin_family_id. English internal reasoning; private reasoning не выводить. Краткие статусы по-русски. Модель текущая в Kiro, без override. CREATE_ONLY: без checksums, второго reviewer, acceptance, export/import, обучения и production CI. После finish выполнить выданный next до квоты; прямые NEXT/STOP исполняются немедленно. Пути и shared status берутся из ответа координатора.

**Ответ:** Только JSON {"action":"restore|preserve|clarify","text":"полный текст на исходном языке","question":""}. Для preserve/clarify text дословно равен исходному; question непустой только для clarify, на исходном языке. Для restore меняется только разрешённое повреждение.

### Простая самопроверка

- Separate permission to edit, exact character operation, and semantic uniqueness. All three must agree.
- Compare the whole sentence before/after, not only the isolated word. Preserve punctuation, case, inflection and other tokens.
- For ambiguity keep canonical_source null; two candidate repairs prove why guessing is not authorized.
- A precomposed letter and combining marks are not interchangeable counting units. Use the assigned unpointed simple Unicode cases; nikud requires a separate explicit task, never silently normalize it.

### TQ26.TYPO-DELETE.en.01

**Prompt**

````text
Нормализуй исходный текст на языке «английский», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: В выделенном слове пропущена ровно одна буква; речь о плетёной корзине.
Исходный текст: The baskt is on the chair.
````

**Chosen**

````text
{"action":"restore","text":"The basket is on the chair.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"The baskt is on the chair.","question":""}
````

**Проверка:** Canonical token 'basket' has 6 code points; raw 'baskt' has 5. Deleting position 4 gives the exact raw token. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The answer text must be the entire original sentence after this single justified restoration; rejected leaves the damaged token unchanged.

### TQ26.TYPO-DELETE.en.02

**Prompt**

````text
Нормализуй исходный текст на языке «английский», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: В названии бумажного пакетика пропущена ровно одна буква.
Исходный текст: She opened the paket.
````

**Chosen**

````text
{"action":"restore","text":"She opened the packet.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"She opened the paket.","question":""}
````

**Проверка:** Canonical token 'packet' has 6 code points; raw 'paket' has 5. Deleting position 2 gives the exact raw token. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The answer text must be the entire original sentence after this single justified restoration; rejected leaves the damaged token unchanged.

Источники проекта: [RAG-023](../../rag/sources/RAG-023.md), [RAG-024](../../rag/sources/RAG-024.md). Это источники метода, не сертификат качества пары.

## TYPO-DELETE/ru

Удаление одной буквы

Нормализуй исходный текст внутри его языка, без перевода. В prompt укажи исходный язык, точный текст, разрешённую операцию и достаточный контекст. В metadata сохраняй raw текст, операцию, индексы Unicode code points, точную словоформу, lemma/sense и lexical_family_ids. После одной локальной операции остальная строка должна совпасть посимвольно. Семья лексемы сохраняется у опечатки и ошибки раскладки; независимые реальные слова не сливаются. Для restore исходная форма должна быть однозначной по видимому контексту; нельзя печатать её как подсказку в самом задании. Только один удалённый Unicode code point. Повреждённое слово на один code point короче; удаление по указанному индексу canonical_token даёт raw_token. Письмо→писмо подходит; письмо→пильмо не подходит. Если одинаковые соседние буквы дают несколько допустимых индексов, проверяй указанный индекс, не требуй единственности индекса.

Работай только с назначенными domain_id/objective_id/profile_id/count/family_id. Пиши реальные prompt/chosen/rejected. Одна пара содержит один содержательный контраст, а все решающие сведения видны ученику в prompt. Публичные образцы не копировать в TRAIN и не переименовывать в независимый EXAM. Производные одного сюжета сохраняют origin_family_id. English internal reasoning; private reasoning не выводить. Краткие статусы по-русски. Модель текущая в Kiro, без override. CREATE_ONLY: без checksums, второго reviewer, acceptance, export/import, обучения и production CI. После finish выполнить выданный next до квоты; прямые NEXT/STOP исполняются немедленно. Пути и shared status берутся из ответа координатора.

**Ответ:** Только JSON {"action":"restore|preserve|clarify","text":"полный текст на исходном языке","question":""}. Для preserve/clarify text дословно равен исходному; question непустой только для clarify, на исходном языке. Для restore меняется только разрешённое повреждение.

### Простая самопроверка

- Separate permission to edit, exact character operation, and semantic uniqueness. All three must agree.
- Compare the whole sentence before/after, not only the isolated word. Preserve punctuation, case, inflection and other tokens.
- For ambiguity keep canonical_source null; two candidate repairs prove why guessing is not authorized.
- A precomposed letter and combining marks are not interchangeable counting units. Use the assigned unpointed simple Unicode cases; nikud requires a separate explicit task, never silently normalize it.

### TQ26.TYPO-DELETE.ru.01

**Prompt**

````text
Нормализуй исходный текст на языке «русский», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: В названии почтового конверта пропущена ровно одна буква.
Исходный текст: На столе лежит конерт.
````

**Chosen**

````text
{"action":"restore","text":"На столе лежит конверт.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"На столе лежит конерт.","question":""}
````

**Проверка:** Canonical token 'конверт' has 7 code points; raw 'конерт' has 6. Deleting position 3 gives the exact raw token. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The answer text must be the entire original sentence after this single justified restoration; rejected leaves the damaged token unchanged.

### TQ26.TYPO-DELETE.ru.02

**Prompt**

````text
Нормализуй исходный текст на языке «русский», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: В названии упаковочного пакета пропущена ровно одна буква; речь не о соглашении.
Исходный текст: Она открыла пакт.
````

**Chosen**

````text
{"action":"restore","text":"Она открыла пакет.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"Она открыла пакт.","question":""}
````

**Проверка:** Canonical token 'пакет' has 5 code points; raw 'пакт' has 4. Deleting position 3 gives the exact raw token. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The answer text must be the entire original sentence after this single justified restoration; rejected leaves the damaged token unchanged.

Источники проекта: [RAG-023](../../rag/sources/RAG-023.md), [RAG-024](../../rag/sources/RAG-024.md). Это источники метода, не сертификат качества пары.

## TYPO-DELETE/he

Удаление одной буквы

Нормализуй исходный текст внутри его языка, без перевода. В prompt укажи исходный язык, точный текст, разрешённую операцию и достаточный контекст. В metadata сохраняй raw текст, операцию, индексы Unicode code points, точную словоформу, lemma/sense и lexical_family_ids. После одной локальной операции остальная строка должна совпасть посимвольно. Семья лексемы сохраняется у опечатки и ошибки раскладки; независимые реальные слова не сливаются. Для restore исходная форма должна быть однозначной по видимому контексту; нельзя печатать её как подсказку в самом задании. Только один удалённый Unicode code point. Повреждённое слово на один code point короче; удаление по указанному индексу canonical_token даёт raw_token. Письмо→писмо подходит; письмо→пильмо не подходит. Если одинаковые соседние буквы дают несколько допустимых индексов, проверяй указанный индекс, не требуй единственности индекса.

Работай только с назначенными domain_id/objective_id/profile_id/count/family_id. Пиши реальные prompt/chosen/rejected. Одна пара содержит один содержательный контраст, а все решающие сведения видны ученику в prompt. Публичные образцы не копировать в TRAIN и не переименовывать в независимый EXAM. Производные одного сюжета сохраняют origin_family_id. English internal reasoning; private reasoning не выводить. Краткие статусы по-русски. Модель текущая в Kiro, без override. CREATE_ONLY: без checksums, второго reviewer, acceptance, export/import, обучения и production CI. После finish выполнить выданный next до квоты; прямые NEXT/STOP исполняются немедленно. Пути и shared status берутся из ответа координатора.

**Ответ:** Только JSON {"action":"restore|preserve|clarify","text":"полный текст на исходном языке","question":""}. Для preserve/clarify text дословно равен исходному; question непустой только для clarify, на исходном языке. Для restore меняется только разрешённое повреждение.

### Простая самопроверка

- Separate permission to edit, exact character operation, and semantic uniqueness. All three must agree.
- Compare the whole sentence before/after, not only the isolated word. Preserve punctuation, case, inflection and other tokens.
- For ambiguity keep canonical_source null; two candidate repairs prove why guessing is not authorized.
- A precomposed letter and combining marks are not interchangeable counting units. Use the assigned unpointed simple Unicode cases; nikud requires a separate explicit task, never silently normalize it.

### TQ26.TYPO-DELETE.he.01

**Prompt**

````text
Нормализуй исходный текст на языке «иврит», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: В слове, обозначающем бумажное письмо с артиклем, пропущена ровно одна буква.
Исходный текст: המכת בתיק.
````

**Chosen**

````text
{"action":"restore","text":"המכתב בתיק.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"המכת בתיק.","question":""}
````

**Проверка:** Canonical token 'המכתב' has 5 code points; raw 'המכת' has 4. Deleting position 4 gives the exact raw token. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The answer text must be the entire original sentence after this single justified restoration; rejected leaves the damaged token unchanged.

### TQ26.TYPO-DELETE.he.02

**Prompt**

````text
Нормализуй исходный текст на языке «иврит», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: В названии окна с артиклем пропущена ровно одна буква.
Исходный текст: היא פתחה את החלן.
````

**Chosen**

````text
{"action":"restore","text":"היא פתחה את החלון.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"היא פתחה את החלן.","question":""}
````

**Проверка:** Canonical token 'החלון' has 5 code points; raw 'החלן' has 4. Deleting position 3 gives the exact raw token. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The answer text must be the entire original sentence after this single justified restoration; rejected leaves the damaged token unchanged.

Источники проекта: [RAG-023](../../rag/sources/RAG-023.md), [RAG-024](../../rag/sources/RAG-024.md). Это источники метода, не сертификат качества пары.

Открытые примеры в этой карточке не засчитываются в 1000 и не являются закрытым EXAM.
