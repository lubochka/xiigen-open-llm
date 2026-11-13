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
| `A200` — `TYPO-SUBSTITUTE` | `text.typo-normalization` / `en` | 48 | `A201` |
| `A201` — `TYPO-SUBSTITUTE` | `text.typo-normalization` / `ru` | 48 | `A202` |
| `A202` — `TYPO-SUBSTITUTE` | `text.typo-normalization` / `he` | 48 | `A203` |
| `A203` — `TYPO-TRANSPOSE` | `text.typo-normalization` / `en` | 48 | `A204` |
| `A204` — `TYPO-TRANSPOSE` | `text.typo-normalization` / `ru` | 48 | `A205` |
| `A205` — `TYPO-TRANSPOSE` | `text.typo-normalization` / `he` | 48 | `A206` |
| `A206` — `TYPO-INSERT` | `text.typo-normalization` / `en` | 48 | `A207` |
| `A207` — `TYPO-INSERT` | `text.typo-normalization` / `ru` | 48 | `A208` |
| `A208` — `TYPO-INSERT` | `text.typo-normalization` / `he` | 48 | `A209` |
| `A209` — `TYPO-LAYOUT` | `text.typo-normalization` / `en` | 48 | `A210` |
| `A210` — `TYPO-LAYOUT` | `text.typo-normalization` / `ru` | 47 | `A211` |
| `A211` — `TYPO-LAYOUT` | `text.typo-normalization` / `he` | 47 | `A212` |
| `A212` — `TYPO-PRESERVE` | `text.typo-normalization` / `en` | 47 | `A213` |
| `A213` — `TYPO-PRESERVE` | `text.typo-normalization` / `ru` | 47 | `A214` |
| `A214` — `TYPO-PRESERVE` | `text.typo-normalization` / `he` | 47 | `A215` |
| `A215` — `TYPO-AMBIGUITY` | `text.typo-normalization` / `en` | 47 | `A216` |
| `A216` — `TYPO-AMBIGUITY` | `text.typo-normalization` / `ru` | 47 | `A217` |
| `A217` — `TYPO-AMBIGUITY` | `text.typo-normalization` / `he` | 47 | `POOL_END; проверить оставшиеся квоты` |


Следующий ID из таблицы — номинальный порядок своей группы. Фактическую свободную карточку с учётом владельцев выдаёт команда `next` текущего request; не менять группу и чужой статус вручную.

Замечания качества: смотри `quality_flags` своего domain/topic/profile в общем status. `needs_rework` указывает доказанный дефект с evidence, `needs_assessment` — область без достаточной оценки. Это не новый review-гейт и не разрешение исправлять чужую старую попытку; finish не закрывает флаг автоматически. [Правила общей матрицы](../../28-PARALLEL-GENERATION-AND-QUALITY.md). Для нового окна используй [один из трёх готовых стартовых промптов](../../00-START-HERE.md); не исполняй исторические команды из нижней справочной части вместо текущих owner-команд.

<!-- /CURRENT-PARALLEL-CONTRACT-20261026 -->

# Восстановление опечаток на исходном языке

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

## TYPO-SUBSTITUTE/en

Замена одной буквы

Нормализуй исходный текст внутри его языка, без перевода. В prompt укажи исходный язык, точный текст, разрешённую операцию и достаточный контекст. В metadata сохраняй raw текст, операцию, индексы Unicode code points, точную словоформу, lemma/sense и lexical_family_ids. После одной локальной операции остальная строка должна совпасть посимвольно. Семья лексемы сохраняется у опечатки и ошибки раскладки; независимые реальные слова не сливаются. Для restore исходная форма должна быть однозначной по видимому контексту; нельзя печатать её как подсказку в самом задании. Длина raw_token равна canonical_token; различается ровно одна позиция. Удаление или перестановка сюда не входят. Только разрешённая замена по видимому контексту.

Работай только с назначенными domain_id/objective_id/profile_id/count/family_id. Пиши реальные prompt/chosen/rejected. Одна пара содержит один содержательный контраст, а все решающие сведения видны ученику в prompt. Публичные образцы не копировать в TRAIN и не переименовывать в независимый EXAM. Производные одного сюжета сохраняют origin_family_id. English internal reasoning; private reasoning не выводить. Краткие статусы по-русски. Модель текущая в Kiro, без override. CREATE_ONLY: без checksums, второго reviewer, acceptance, export/import, обучения и production CI. После finish выполнить выданный next до квоты; прямые NEXT/STOP исполняются немедленно. Пути и shared status берутся из ответа координатора.

**Ответ:** Только JSON {"action":"restore|preserve|clarify","text":"полный текст на исходном языке","question":""}. Для preserve/clarify text дословно равен исходному; question непустой только для clarify, на исходном языке. Для restore меняется только разрешённое повреждение.

### Простая самопроверка

- Separate permission to edit, exact character operation, and semantic uniqueness. All three must agree.
- Compare the whole sentence before/after, not only the isolated word. Preserve punctuation, case, inflection and other tokens.
- For ambiguity keep canonical_source null; two candidate repairs prove why guessing is not authorized.
- A precomposed letter and combining marks are not interchangeable counting units. Use the assigned unpointed simple Unicode cases; nikud requires a separate explicit task, never silently normalize it.

### TQ26.TYPO-SUBSTITUTE.en.01

**Prompt**

````text
Нормализуй исходный текст на языке «английский», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: В названии головного убора заменена только последняя буква; первые две буквы сохранены. Речь не о животном.
Исходный текст: Place the cat on the table.
````

**Chosen**

````text
{"action":"restore","text":"Place the cap on the table.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"Place the cat on the table.","question":""}
````

**Проверка:** Canonical token 'cap' has 3 code points; raw 'cat' has 3. Only position 2 differs; replacing that position restores the canonical token. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The answer text must be the entire original sentence after this single justified restoration; rejected leaves the damaged token unchanged.

### TQ26.TYPO-SUBSTITUTE.en.02

**Prompt**

````text
Нормализуй исходный текст на языке «английский», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: Ровно одна буква заменена в названии короткой письменной записки.
Исходный текст: Read the nose.
````

**Chosen**

````text
{"action":"restore","text":"Read the note.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"Read the nose.","question":""}
````

**Проверка:** Canonical token 'note' has 4 code points; raw 'nose' has 4. Only position 2 differs; replacing that position restores the canonical token. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The answer text must be the entire original sentence after this single justified restoration; rejected leaves the damaged token unchanged.

Источники проекта: [RAG-023](../../rag/sources/RAG-023.md), [RAG-024](../../rag/sources/RAG-024.md). Это источники метода, не сертификат качества пары.

## TYPO-SUBSTITUTE/ru

Замена одной буквы

Нормализуй исходный текст внутри его языка, без перевода. В prompt укажи исходный язык, точный текст, разрешённую операцию и достаточный контекст. В metadata сохраняй raw текст, операцию, индексы Unicode code points, точную словоформу, lemma/sense и lexical_family_ids. После одной локальной операции остальная строка должна совпасть посимвольно. Семья лексемы сохраняется у опечатки и ошибки раскладки; независимые реальные слова не сливаются. Для restore исходная форма должна быть однозначной по видимому контексту; нельзя печатать её как подсказку в самом задании. Длина raw_token равна canonical_token; различается ровно одна позиция. Удаление или перестановка сюда не входят. Только разрешённая замена по видимому контексту.

Работай только с назначенными domain_id/objective_id/profile_id/count/family_id. Пиши реальные prompt/chosen/rejected. Одна пара содержит один содержательный контраст, а все решающие сведения видны ученику в prompt. Публичные образцы не копировать в TRAIN и не переименовывать в независимый EXAM. Производные одного сюжета сохраняют origin_family_id. English internal reasoning; private reasoning не выводить. Краткие статусы по-русски. Модель текущая в Kiro, без override. CREATE_ONLY: без checksums, второго reviewer, acceptance, export/import, обучения и production CI. После finish выполнить выданный next до квоты; прямые NEXT/STOP исполняются немедленно. Пути и shared status берутся из ответа координатора.

**Ответ:** Только JSON {"action":"restore|preserve|clarify","text":"полный текст на исходном языке","question":""}. Для preserve/clarify text дословно равен исходному; question непустой только для clarify, на исходном языке. Для restore меняется только разрешённое повреждение.

### Простая самопроверка

- Separate permission to edit, exact character operation, and semantic uniqueness. All three must agree.
- Compare the whole sentence before/after, not only the isolated word. Preserve punctuation, case, inflection and other tokens.
- For ambiguity keep canonical_source null; two candidate repairs prove why guessing is not authorized.
- A precomposed letter and combining marks are not interchangeable counting units. Use the assigned unpointed simple Unicode cases; nikud requires a separate explicit task, never silently normalize it.

### TQ26.TYPO-SUBSTITUTE.ru.01

**Prompt**

````text
Нормализуй исходный текст на языке «русский», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: Речь о ручной клади. Ровно одна буква заменена; сохрани форму слова из предложения.
Исходный текст: Она несёт сутку.
````

**Chosen**

````text
{"action":"restore","text":"Она несёт сумку.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"Она несёт сутку.","question":""}
````

**Проверка:** Canonical token 'сумку' has 5 code points; raw 'сутку' has 5. Only position 2 differs; replacing that position restores the canonical token. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The answer text must be the entire original sentence after this single justified restoration; rejected leaves the damaged token unchanged.

### TQ26.TYPO-SUBSTITUTE.ru.02

**Prompt**

````text
Нормализуй исходный текст на языке «русский», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: Речь о географической карте. В её названии заменена ровно одна буква.
Исходный текст: На столе лежит парта.
````

**Chosen**

````text
{"action":"restore","text":"На столе лежит карта.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"На столе лежит парта.","question":""}
````

**Проверка:** Canonical token 'карта' has 5 code points; raw 'парта' has 5. Only position 0 differs; replacing that position restores the canonical token. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The answer text must be the entire original sentence after this single justified restoration; rejected leaves the damaged token unchanged.

Источники проекта: [RAG-023](../../rag/sources/RAG-023.md), [RAG-024](../../rag/sources/RAG-024.md). Это источники метода, не сертификат качества пары.

## TYPO-SUBSTITUTE/he

Замена одной буквы

Нормализуй исходный текст внутри его языка, без перевода. В prompt укажи исходный язык, точный текст, разрешённую операцию и достаточный контекст. В metadata сохраняй raw текст, операцию, индексы Unicode code points, точную словоформу, lemma/sense и lexical_family_ids. После одной локальной операции остальная строка должна совпасть посимвольно. Семья лексемы сохраняется у опечатки и ошибки раскладки; независимые реальные слова не сливаются. Для restore исходная форма должна быть однозначной по видимому контексту; нельзя печатать её как подсказку в самом задании. Длина raw_token равна canonical_token; различается ровно одна позиция. Удаление или перестановка сюда не входят. Только разрешённая замена по видимому контексту.

Работай только с назначенными domain_id/objective_id/profile_id/count/family_id. Пиши реальные prompt/chosen/rejected. Одна пара содержит один содержательный контраст, а все решающие сведения видны ученику в prompt. Публичные образцы не копировать в TRAIN и не переименовывать в независимый EXAM. Производные одного сюжета сохраняют origin_family_id. English internal reasoning; private reasoning не выводить. Краткие статусы по-русски. Модель текущая в Kiro, без override. CREATE_ONLY: без checksums, второго reviewer, acceptance, export/import, обучения и production CI. После finish выполнить выданный next до квоты; прямые NEXT/STOP исполняются немедленно. Пути и shared status берутся из ответа координатора.

**Ответ:** Только JSON {"action":"restore|preserve|clarify","text":"полный текст на исходном языке","question":""}. Для preserve/clarify text дословно равен исходному; question непустой только для clarify, на исходном языке. Для restore меняется только разрешённое повреждение.

### Простая самопроверка

- Separate permission to edit, exact character operation, and semantic uniqueness. All three must agree.
- Compare the whole sentence before/after, not only the isolated word. Preserve punctuation, case, inflection and other tokens.
- For ambiguity keep canonical_source null; two candidate repairs prove why guessing is not authorized.
- A precomposed letter and combining marks are not interchangeable counting units. Use the assigned unpointed simple Unicode cases; nikud requires a separate explicit task, never silently normalize it.

### TQ26.TYPO-SUBSTITUTE.he.01

**Prompt**

````text
Нормализуй исходный текст на языке «иврит», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: Речь о хлебе, купленном в пекарне. В одном слове заменена ровно одна буква.
Исходный текст: היא קנתה לחן.
````

**Chosen**

````text
{"action":"restore","text":"היא קנתה לחם.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"היא קנתה לחן.","question":""}
````

**Проверка:** Canonical token 'לחם' has 3 code points; raw 'לחן' has 3. Only position 2 differs; replacing that position restores the canonical token. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The answer text must be the entire original sentence after this single justified restoration; rejected leaves the damaged token unchanged.

### TQ26.TYPO-SUBSTITUTE.he.02

**Prompt**

````text
Нормализуй исходный текст на языке «иврит», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: Речь о рыбе в аквариуме. В одном слове заменена ровно одна буква.
Исходный текст: הגג במים.
````

**Chosen**

````text
{"action":"restore","text":"הדג במים.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"הגג במים.","question":""}
````

**Проверка:** Canonical token 'הדג' has 3 code points; raw 'הגג' has 3. Only position 1 differs; replacing that position restores the canonical token. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The answer text must be the entire original sentence after this single justified restoration; rejected leaves the damaged token unchanged.

Источники проекта: [RAG-023](../../rag/sources/RAG-023.md), [RAG-024](../../rag/sources/RAG-024.md). Это источники метода, не сертификат качества пары.

## TYPO-TRANSPOSE/en

Перестановка двух соседних букв

Нормализуй исходный текст внутри его языка, без перевода. В prompt укажи исходный язык, точный текст, разрешённую операцию и достаточный контекст. В metadata сохраняй raw текст, операцию, индексы Unicode code points, точную словоформу, lemma/sense и lexical_family_ids. После одной локальной операции остальная строка должна совпасть посимвольно. Семья лексемы сохраняется у опечатки и ошибки раскладки; независимые реальные слова не сливаются. Для restore исходная форма должна быть однозначной по видимому контексту; нельзя печатать её как подсказку в самом задании. Поменяй ровно два соседних различных code points; длина и частоты символов сохраняются. Обратная такая же перестановка восстанавливает исходное слово и полное предложение.

Работай только с назначенными domain_id/objective_id/profile_id/count/family_id. Пиши реальные prompt/chosen/rejected. Одна пара содержит один содержательный контраст, а все решающие сведения видны ученику в prompt. Публичные образцы не копировать в TRAIN и не переименовывать в независимый EXAM. Производные одного сюжета сохраняют origin_family_id. English internal reasoning; private reasoning не выводить. Краткие статусы по-русски. Модель текущая в Kiro, без override. CREATE_ONLY: без checksums, второго reviewer, acceptance, export/import, обучения и production CI. После finish выполнить выданный next до квоты; прямые NEXT/STOP исполняются немедленно. Пути и shared status берутся из ответа координатора.

**Ответ:** Только JSON {"action":"restore|preserve|clarify","text":"полный текст на исходном языке","question":""}. Для preserve/clarify text дословно равен исходному; question непустой только для clarify, на исходном языке. Для restore меняется только разрешённое повреждение.

### Простая самопроверка

- Separate permission to edit, exact character operation, and semantic uniqueness. All three must agree.
- Compare the whole sentence before/after, not only the isolated word. Preserve punctuation, case, inflection and other tokens.
- For ambiguity keep canonical_source null; two candidate repairs prove why guessing is not authorized.
- A precomposed letter and combining marks are not interchangeable counting units. Use the assigned unpointed simple Unicode cases; nikud requires a separate explicit task, never silently normalize it.

### TQ26.TYPO-TRANSPOSE.en.01

**Prompt**

````text
Нормализуй исходный текст на языке «английский», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: В названии тарелки переставлены ровно две соседние разные буквы.
Исходный текст: The palte is clean.
````

**Chosen**

````text
{"action":"restore","text":"The plate is clean.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"The palte is clean.","question":""}
````

**Проверка:** Canonical token 'plate' has 5 code points; raw 'palte' has 5. Swapping adjacent positions [1, 2] gives the exact raw token; swap them again to restore it. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The answer text must be the entire original sentence after this single justified restoration; rejected leaves the damaged token unchanged.

### TQ26.TYPO-TRANSPOSE.en.02

**Prompt**

````text
Нормализуй исходный текст на языке «английский», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: В названии калитки переставлены ровно две соседние разные буквы.
Исходный текст: Close the agte.
````

**Chosen**

````text
{"action":"restore","text":"Close the gate.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"Close the agte.","question":""}
````

**Проверка:** Canonical token 'gate' has 4 code points; raw 'agte' has 4. Swapping adjacent positions [0, 1] gives the exact raw token; swap them again to restore it. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The answer text must be the entire original sentence after this single justified restoration; rejected leaves the damaged token unchanged.

Источники проекта: [RAG-023](../../rag/sources/RAG-023.md), [RAG-024](../../rag/sources/RAG-024.md). Это источники метода, не сертификат качества пары.

## TYPO-TRANSPOSE/ru

Перестановка двух соседних букв

Нормализуй исходный текст внутри его языка, без перевода. В prompt укажи исходный язык, точный текст, разрешённую операцию и достаточный контекст. В metadata сохраняй raw текст, операцию, индексы Unicode code points, точную словоформу, lemma/sense и lexical_family_ids. После одной локальной операции остальная строка должна совпасть посимвольно. Семья лексемы сохраняется у опечатки и ошибки раскладки; независимые реальные слова не сливаются. Для restore исходная форма должна быть однозначной по видимому контексту; нельзя печатать её как подсказку в самом задании. Поменяй ровно два соседних различных code points; длина и частоты символов сохраняются. Обратная такая же перестановка восстанавливает исходное слово и полное предложение.

Работай только с назначенными domain_id/objective_id/profile_id/count/family_id. Пиши реальные prompt/chosen/rejected. Одна пара содержит один содержательный контраст, а все решающие сведения видны ученику в prompt. Публичные образцы не копировать в TRAIN и не переименовывать в независимый EXAM. Производные одного сюжета сохраняют origin_family_id. English internal reasoning; private reasoning не выводить. Краткие статусы по-русски. Модель текущая в Kiro, без override. CREATE_ONLY: без checksums, второго reviewer, acceptance, export/import, обучения и production CI. После finish выполнить выданный next до квоты; прямые NEXT/STOP исполняются немедленно. Пути и shared status берутся из ответа координатора.

**Ответ:** Только JSON {"action":"restore|preserve|clarify","text":"полный текст на исходном языке","question":""}. Для preserve/clarify text дословно равен исходному; question непустой только для clarify, на исходном языке. Для restore меняется только разрешённое повреждение.

### Простая самопроверка

- Separate permission to edit, exact character operation, and semantic uniqueness. All three must agree.
- Compare the whole sentence before/after, not only the isolated word. Preserve punctuation, case, inflection and other tokens.
- For ambiguity keep canonical_source null; two candidate repairs prove why guessing is not authorized.
- A precomposed letter and combining marks are not interchangeable counting units. Use the assigned unpointed simple Unicode cases; nikud requires a separate explicit task, never silently normalize it.

### TQ26.TYPO-TRANSPOSE.ru.01

**Prompt**

````text
Нормализуй исходный текст на языке «русский», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: Речь о письменном инструменте. Переставлены ровно две соседние буквы в форме слова из предложения.
Исходный текст: Она взяла рукчу.
````

**Chosen**

````text
{"action":"restore","text":"Она взяла ручку.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"Она взяла рукчу.","question":""}
````

**Проверка:** Canonical token 'ручку' has 5 code points; raw 'рукчу' has 5. Swapping adjacent positions [2, 3] gives the exact raw token; swap them again to restore it. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The answer text must be the entire original sentence after this single justified restoration; rejected leaves the damaged token unchanged.

### TQ26.TYPO-TRANSPOSE.ru.02

**Prompt**

````text
Нормализуй исходный текст на языке «русский», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: В названии книги переставлены ровно две соседние разные буквы.
Исходный текст: На столе лежит кинга.
````

**Chosen**

````text
{"action":"restore","text":"На столе лежит книга.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"На столе лежит кинга.","question":""}
````

**Проверка:** Canonical token 'книга' has 5 code points; raw 'кинга' has 5. Swapping adjacent positions [1, 2] gives the exact raw token; swap them again to restore it. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The answer text must be the entire original sentence after this single justified restoration; rejected leaves the damaged token unchanged.

Источники проекта: [RAG-023](../../rag/sources/RAG-023.md), [RAG-024](../../rag/sources/RAG-024.md). Это источники метода, не сертификат качества пары.

## TYPO-TRANSPOSE/he

Перестановка двух соседних букв

Нормализуй исходный текст внутри его языка, без перевода. В prompt укажи исходный язык, точный текст, разрешённую операцию и достаточный контекст. В metadata сохраняй raw текст, операцию, индексы Unicode code points, точную словоформу, lemma/sense и lexical_family_ids. После одной локальной операции остальная строка должна совпасть посимвольно. Семья лексемы сохраняется у опечатки и ошибки раскладки; независимые реальные слова не сливаются. Для restore исходная форма должна быть однозначной по видимому контексту; нельзя печатать её как подсказку в самом задании. Поменяй ровно два соседних различных code points; длина и частоты символов сохраняются. Обратная такая же перестановка восстанавливает исходное слово и полное предложение.

Работай только с назначенными domain_id/objective_id/profile_id/count/family_id. Пиши реальные prompt/chosen/rejected. Одна пара содержит один содержательный контраст, а все решающие сведения видны ученику в prompt. Публичные образцы не копировать в TRAIN и не переименовывать в независимый EXAM. Производные одного сюжета сохраняют origin_family_id. English internal reasoning; private reasoning не выводить. Краткие статусы по-русски. Модель текущая в Kiro, без override. CREATE_ONLY: без checksums, второго reviewer, acceptance, export/import, обучения и production CI. После finish выполнить выданный next до квоты; прямые NEXT/STOP исполняются немедленно. Пути и shared status берутся из ответа координатора.

**Ответ:** Только JSON {"action":"restore|preserve|clarify","text":"полный текст на исходном языке","question":""}. Для preserve/clarify text дословно равен исходному; question непустой только для clarify, на исходном языке. Для restore меняется только разрешённое повреждение.

### Простая самопроверка

- Separate permission to edit, exact character operation, and semantic uniqueness. All three must agree.
- Compare the whole sentence before/after, not only the isolated word. Preserve punctuation, case, inflection and other tokens.
- For ambiguity keep canonical_source null; two candidate repairs prove why guessing is not authorized.
- A precomposed letter and combining marks are not interchangeable counting units. Use the assigned unpointed simple Unicode cases; nikud requires a separate explicit task, never silently normalize it.

### TQ26.TYPO-TRANSPOSE.he.01

**Prompt**

````text
Нормализуй исходный текст на языке «иврит», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: Речь о молоке. В одном слове переставлены ровно две соседние разные буквы.
Исходный текст: היא קנתה לחב.
````

**Chosen**

````text
{"action":"restore","text":"היא קנתה חלב.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"היא קנתה לחב.","question":""}
````

**Проверка:** Canonical token 'חלב' has 3 code points; raw 'לחב' has 3. Swapping adjacent positions [0, 1] gives the exact raw token; swap them again to restore it. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The answer text must be the entire original sentence after this single justified restoration; rejected leaves the damaged token unchanged.

### TQ26.TYPO-TRANSPOSE.he.02

**Prompt**

````text
Нормализуй исходный текст на языке «иврит», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: В названии двери с артиклем переставлены ровно две соседние разные буквы.
Исходный текст: הלדת סגורה.
````

**Chosen**

````text
{"action":"restore","text":"הדלת סגורה.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"הלדת סגורה.","question":""}
````

**Проверка:** Canonical token 'הדלת' has 4 code points; raw 'הלדת' has 4. Swapping adjacent positions [1, 2] gives the exact raw token; swap them again to restore it. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The answer text must be the entire original sentence after this single justified restoration; rejected leaves the damaged token unchanged.

Источники проекта: [RAG-023](../../rag/sources/RAG-023.md), [RAG-024](../../rag/sources/RAG-024.md). Это источники метода, не сертификат качества пары.

## TYPO-INSERT/en

Вставка одной лишней буквы

Нормализуй исходный текст внутри его языка, без перевода. В prompt укажи исходный язык, точный текст, разрешённую операцию и достаточный контекст. В metadata сохраняй raw текст, операцию, индексы Unicode code points, точную словоформу, lemma/sense и lexical_family_ids. После одной локальной операции остальная строка должна совпасть посимвольно. Семья лексемы сохраняется у опечатки и ошибки раскладки; независимые реальные слова не сливаются. Для restore исходная форма должна быть однозначной по видимому контексту; нельзя печатать её как подсказку в самом задании. Raw_token длиннее ровно на один code point. Удаление ровно добавленной позиции возвращает canonical_token. Одинаковые буквы не требуют единственного допустимого индекса.

Работай только с назначенными domain_id/objective_id/profile_id/count/family_id. Пиши реальные prompt/chosen/rejected. Одна пара содержит один содержательный контраст, а все решающие сведения видны ученику в prompt. Публичные образцы не копировать в TRAIN и не переименовывать в независимый EXAM. Производные одного сюжета сохраняют origin_family_id. English internal reasoning; private reasoning не выводить. Краткие статусы по-русски. Модель текущая в Kiro, без override. CREATE_ONLY: без checksums, второго reviewer, acceptance, export/import, обучения и production CI. После finish выполнить выданный next до квоты; прямые NEXT/STOP исполняются немедленно. Пути и shared status берутся из ответа координатора.

**Ответ:** Только JSON {"action":"restore|preserve|clarify","text":"полный текст на исходном языке","question":""}. Для preserve/clarify text дословно равен исходному; question непустой только для clarify, на исходном языке. Для restore меняется только разрешённое повреждение.

### Простая самопроверка

- Separate permission to edit, exact character operation, and semantic uniqueness. All three must agree.
- Compare the whole sentence before/after, not only the isolated word. Preserve punctuation, case, inflection and other tokens.
- For ambiguity keep canonical_source null; two candidate repairs prove why guessing is not authorized.
- A precomposed letter and combining marks are not interchangeable counting units. Use the assigned unpointed simple Unicode cases; nikud requires a separate explicit task, never silently normalize it.

### TQ26.TYPO-INSERT.en.01

**Prompt**

````text
Нормализуй исходный текст на языке «английский», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: В одном слове ровно одна буква случайно повторена. Речь о коробке.
Исходный текст: The boox is open.
````

**Chosen**

````text
{"action":"restore","text":"The box is open.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"The boox is open.","question":""}
````

**Проверка:** Canonical token 'box' has 3 code points; raw 'boox' has 4. Removing the inserted code point at raw position 2 restores the exact canonical token. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The answer text must be the entire original sentence after this single justified restoration; rejected leaves the damaged token unchanged.

### TQ26.TYPO-INSERT.en.02

**Prompt**

````text
Нормализуй исходный текст на языке «английский», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: В названии ключа случайно повторена ровно одна буква.
Исходный текст: She has a kkey.
````

**Chosen**

````text
{"action":"restore","text":"She has a key.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"She has a kkey.","question":""}
````

**Проверка:** Canonical token 'key' has 3 code points; raw 'kkey' has 4. Removing the inserted code point at raw position 1 restores the exact canonical token. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The answer text must be the entire original sentence after this single justified restoration; rejected leaves the damaged token unchanged.

Источники проекта: [RAG-023](../../rag/sources/RAG-023.md), [RAG-024](../../rag/sources/RAG-024.md). Это источники метода, не сертификат качества пары.

## TYPO-INSERT/ru

Вставка одной лишней буквы

Нормализуй исходный текст внутри его языка, без перевода. В prompt укажи исходный язык, точный текст, разрешённую операцию и достаточный контекст. В metadata сохраняй raw текст, операцию, индексы Unicode code points, точную словоформу, lemma/sense и lexical_family_ids. После одной локальной операции остальная строка должна совпасть посимвольно. Семья лексемы сохраняется у опечатки и ошибки раскладки; независимые реальные слова не сливаются. Для restore исходная форма должна быть однозначной по видимому контексту; нельзя печатать её как подсказку в самом задании. Raw_token длиннее ровно на один code point. Удаление ровно добавленной позиции возвращает canonical_token. Одинаковые буквы не требуют единственного допустимого индекса.

Работай только с назначенными domain_id/objective_id/profile_id/count/family_id. Пиши реальные prompt/chosen/rejected. Одна пара содержит один содержательный контраст, а все решающие сведения видны ученику в prompt. Публичные образцы не копировать в TRAIN и не переименовывать в независимый EXAM. Производные одного сюжета сохраняют origin_family_id. English internal reasoning; private reasoning не выводить. Краткие статусы по-русски. Модель текущая в Kiro, без override. CREATE_ONLY: без checksums, второго reviewer, acceptance, export/import, обучения и production CI. После finish выполнить выданный next до квоты; прямые NEXT/STOP исполняются немедленно. Пути и shared status берутся из ответа координатора.

**Ответ:** Только JSON {"action":"restore|preserve|clarify","text":"полный текст на исходном языке","question":""}. Для preserve/clarify text дословно равен исходному; question непустой только для clarify, на исходном языке. Для restore меняется только разрешённое повреждение.

### Простая самопроверка

- Separate permission to edit, exact character operation, and semantic uniqueness. All three must agree.
- Compare the whole sentence before/after, not only the isolated word. Preserve punctuation, case, inflection and other tokens.
- For ambiguity keep canonical_source null; two candidate repairs prove why guessing is not authorized.
- A precomposed letter and combining marks are not interchangeable counting units. Use the assigned unpointed simple Unicode cases; nikud requires a separate explicit task, never silently normalize it.

### TQ26.TYPO-INSERT.ru.01

**Prompt**

````text
Нормализуй исходный текст на языке «русский», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: В одном слове случайно повторена ровно одна буква; остальную фразу не меняй.
Исходный текст: Она взяла ккнигу.
````

**Chosen**

````text
{"action":"restore","text":"Она взяла книгу.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"Она взяла ккнигу.","question":""}
````

**Проверка:** Canonical token 'книгу' has 5 code points; raw 'ккнигу' has 6. Removing the inserted code point at raw position 1 restores the exact canonical token. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The answer text must be the entire original sentence after this single justified restoration; rejected leaves the damaged token unchanged.

### TQ26.TYPO-INSERT.ru.02

**Prompt**

````text
Нормализуй исходный текст на языке «русский», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: В названии окна случайно повторена ровно одна буква. Регистр не изменяй.
Исходный текст: Оккно закрыто.
````

**Chosen**

````text
{"action":"restore","text":"Окно закрыто.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"Оккно закрыто.","question":""}
````

**Проверка:** Canonical token 'Окно' has 4 code points; raw 'Оккно' has 5. Removing the inserted code point at raw position 2 restores the exact canonical token. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The answer text must be the entire original sentence after this single justified restoration; rejected leaves the damaged token unchanged.

Источники проекта: [RAG-023](../../rag/sources/RAG-023.md), [RAG-024](../../rag/sources/RAG-024.md). Это источники метода, не сертификат качества пары.

## TYPO-INSERT/he

Вставка одной лишней буквы

Нормализуй исходный текст внутри его языка, без перевода. В prompt укажи исходный язык, точный текст, разрешённую операцию и достаточный контекст. В metadata сохраняй raw текст, операцию, индексы Unicode code points, точную словоформу, lemma/sense и lexical_family_ids. После одной локальной операции остальная строка должна совпасть посимвольно. Семья лексемы сохраняется у опечатки и ошибки раскладки; независимые реальные слова не сливаются. Для restore исходная форма должна быть однозначной по видимому контексту; нельзя печатать её как подсказку в самом задании. Raw_token длиннее ровно на один code point. Удаление ровно добавленной позиции возвращает canonical_token. Одинаковые буквы не требуют единственного допустимого индекса.

Работай только с назначенными domain_id/objective_id/profile_id/count/family_id. Пиши реальные prompt/chosen/rejected. Одна пара содержит один содержательный контраст, а все решающие сведения видны ученику в prompt. Публичные образцы не копировать в TRAIN и не переименовывать в независимый EXAM. Производные одного сюжета сохраняют origin_family_id. English internal reasoning; private reasoning не выводить. Краткие статусы по-русски. Модель текущая в Kiro, без override. CREATE_ONLY: без checksums, второго reviewer, acceptance, export/import, обучения и production CI. После finish выполнить выданный next до квоты; прямые NEXT/STOP исполняются немедленно. Пути и shared status берутся из ответа координатора.

**Ответ:** Только JSON {"action":"restore|preserve|clarify","text":"полный текст на исходном языке","question":""}. Для preserve/clarify text дословно равен исходному; question непустой только для clarify, на исходном языке. Для restore меняется только разрешённое повреждение.

### Простая самопроверка

- Separate permission to edit, exact character operation, and semantic uniqueness. All three must agree.
- Compare the whole sentence before/after, not only the isolated word. Preserve punctuation, case, inflection and other tokens.
- For ambiguity keep canonical_source null; two candidate repairs prove why guessing is not authorized.
- A precomposed letter and combining marks are not interchangeable counting units. Use the assigned unpointed simple Unicode cases; nikud requires a separate explicit task, never silently normalize it.

### TQ26.TYPO-INSERT.he.01

**Prompt**

````text
Нормализуй исходный текст на языке «иврит», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: В названии двери с артиклем случайно повторена ровно одна буква.
Исходный текст: הדללת פתוחה.
````

**Chosen**

````text
{"action":"restore","text":"הדלת פתוחה.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"הדללת פתוחה.","question":""}
````

**Проверка:** Canonical token 'הדלת' has 4 code points; raw 'הדללת' has 5. Removing the inserted code point at raw position 3 restores the exact canonical token. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The answer text must be the entire original sentence after this single justified restoration; rejected leaves the damaged token unchanged.

### TQ26.TYPO-INSERT.he.02

**Prompt**

````text
Нормализуй исходный текст на языке «иврит», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: В названии книги случайно повторена ровно одна буква.
Исходный текст: הוא קרא ספפר.
````

**Chosen**

````text
{"action":"restore","text":"הוא קרא ספר.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"הוא קרא ספפר.","question":""}
````

**Проверка:** Canonical token 'ספר' has 3 code points; raw 'ספפר' has 4. Removing the inserted code point at raw position 2 restores the exact canonical token. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The answer text must be the entire original sentence after this single justified restoration; rejected leaves the damaged token unchanged.

Источники проекта: [RAG-023](../../rag/sources/RAG-023.md), [RAG-024](../../rag/sources/RAG-024.md). Это источники метода, не сертификат качества пары.

## TYPO-LAYOUT/en

Ошибочная раскладка клавиатуры

Нормализуй исходный текст внутри его языка, без перевода. В prompt укажи исходный язык, точный текст, разрешённую операцию и достаточный контекст. В metadata сохраняй raw текст, операцию, индексы Unicode code points, точную словоформу, lemma/sense и lexical_family_ids. После одной локальной операции остальная строка должна совпасть посимвольно. Семья лексемы сохраняется у опечатки и ошибки раскладки; независимые реальные слова не сливаются. Для restore исходная форма должна быть однозначной по видимому контексту; нельзя печатать её как подсказку в самом задании. Используй только точную карту domains/translation/layout-fixture-v1.json. Нужные пары клавиш, направление и правила пунктуации видны ученику. Прямое преобразование canonical→raw и обратное дают точное совпадение. Shift/CapsLock не изобретать; неподдержанное преобразование не назначать.

Работай только с назначенными domain_id/objective_id/profile_id/count/family_id. Пиши реальные prompt/chosen/rejected. Одна пара содержит один содержательный контраст, а все решающие сведения видны ученику в prompt. Публичные образцы не копировать в TRAIN и не переименовывать в независимый EXAM. Производные одного сюжета сохраняют origin_family_id. English internal reasoning; private reasoning не выводить. Краткие статусы по-русски. Модель текущая в Kiro, без override. CREATE_ONLY: без checksums, второго reviewer, acceptance, export/import, обучения и production CI. После finish выполнить выданный next до квоты; прямые NEXT/STOP исполняются немедленно. Пути и shared status берутся из ответа координатора.

**Ответ:** Только JSON {"action":"restore|preserve|clarify","text":"полный текст на исходном языке","question":""}. Для preserve/clarify text дословно равен исходному; question непустой только для clarify, на исходном языке. Для restore меняется только разрешённое повреждение.

### Простая самопроверка

- Separate permission to edit, exact character operation, and semantic uniqueness. All three must agree.
- Compare the whole sentence before/after, not only the isolated word. Preserve punctuation, case, inflection and other tokens.
- For ambiguity keep canonical_source null; two candidate repairs prove why guessing is not authorized.
- A precomposed letter and combining marks are not interchangeable counting units. Use the assigned unpointed simple Unicode cases; nikud requires a separate explicit task, never silently normalize it.

### TQ26.TYPO-LAYOUT.en.01

**Prompt**

````text
Нормализуй исходный текст на языке «английский», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: Ровно выделенная последовательность набрана по другой раскладке. Обычное слово, не идентификатор. Применяй только напечатанную карту; символы внутри последовательности, в том числе запятая, тоже являются входом карты.
Выделенная последовательность: ашду
Карта raw→canonical: {"а": "f", "ш": "i", "д": "l", "у": "e"}
Исходный текст: Open the ашду.
````

**Chosen**

````text
{"action":"restore","text":"Open the file.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"Open the tile.","question":""}
````

**Проверка:** Canonical token 'file' has 4 code points; raw 'ашду' has 4. Apply only the visible raw-to-canonical map in order; its one-to-one inverse returns the same raw token. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The answer text must be the entire original sentence after this single justified restoration; rejected produces a different, unsupported restoration.

### TQ26.TYPO-LAYOUT.en.02

**Prompt**

````text
Нормализуй исходный текст на языке «английский», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: Ровно выделенная последовательность набрана по другой раскладке. Обычное слово, не идентификатор. Применяй только напечатанную карту; символы внутри последовательности, в том числе запятая, тоже являются входом карты.
Выделенная последовательность: вщщк
Карта raw→canonical: {"в": "d", "щ": "o", "к": "r"}
Исходный текст: The вщщк is shut.
````

**Chosen**

````text
{"action":"restore","text":"The door is shut.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"The gate is shut.","question":""}
````

**Проверка:** Canonical token 'door' has 4 code points; raw 'вщщк' has 4. Apply only the visible raw-to-canonical map in order; its one-to-one inverse returns the same raw token. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The answer text must be the entire original sentence after this single justified restoration; rejected produces a different, unsupported restoration.

Источники проекта: [RAG-023](../../rag/sources/RAG-023.md), [RAG-024](../../rag/sources/RAG-024.md). Это источники метода, не сертификат качества пары.

## TYPO-LAYOUT/ru

Ошибочная раскладка клавиатуры

Нормализуй исходный текст внутри его языка, без перевода. В prompt укажи исходный язык, точный текст, разрешённую операцию и достаточный контекст. В metadata сохраняй raw текст, операцию, индексы Unicode code points, точную словоформу, lemma/sense и lexical_family_ids. После одной локальной операции остальная строка должна совпасть посимвольно. Семья лексемы сохраняется у опечатки и ошибки раскладки; независимые реальные слова не сливаются. Для restore исходная форма должна быть однозначной по видимому контексту; нельзя печатать её как подсказку в самом задании. Используй только точную карту domains/translation/layout-fixture-v1.json. Нужные пары клавиш, направление и правила пунктуации видны ученику. Прямое преобразование canonical→raw и обратное дают точное совпадение. Shift/CapsLock не изобретать; неподдержанное преобразование не назначать.

Работай только с назначенными domain_id/objective_id/profile_id/count/family_id. Пиши реальные prompt/chosen/rejected. Одна пара содержит один содержательный контраст, а все решающие сведения видны ученику в prompt. Публичные образцы не копировать в TRAIN и не переименовывать в независимый EXAM. Производные одного сюжета сохраняют origin_family_id. English internal reasoning; private reasoning не выводить. Краткие статусы по-русски. Модель текущая в Kiro, без override. CREATE_ONLY: без checksums, второго reviewer, acceptance, export/import, обучения и production CI. После finish выполнить выданный next до квоты; прямые NEXT/STOP исполняются немедленно. Пути и shared status берутся из ответа координатора.

**Ответ:** Только JSON {"action":"restore|preserve|clarify","text":"полный текст на исходном языке","question":""}. Для preserve/clarify text дословно равен исходному; question непустой только для clarify, на исходном языке. Для restore меняется только разрешённое повреждение.

### Простая самопроверка

- Separate permission to edit, exact character operation, and semantic uniqueness. All three must agree.
- Compare the whole sentence before/after, not only the isolated word. Preserve punctuation, case, inflection and other tokens.
- For ambiguity keep canonical_source null; two candidate repairs prove why guessing is not authorized.
- A precomposed letter and combining marks are not interchangeable counting units. Use the assigned unpointed simple Unicode cases; nikud requires a separate explicit task, never silently normalize it.

### TQ26.TYPO-LAYOUT.ru.01

**Prompt**

````text
Нормализуй исходный текст на языке «русский», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: Ровно выделенная последовательность набрана по другой раскладке. Обычное слово, не идентификатор. Применяй только напечатанную карту; символы внутри последовательности, в том числе запятая, тоже являются входом карты.
Выделенная последовательность: gfhjkm
Карта raw→canonical: {"g": "п", "f": "а", "h": "р", "j": "о", "k": "л", "m": "ь"}
Исходный текст: Проверь gfhjkm.
````

**Chosen**

````text
{"action":"restore","text":"Проверь пароль.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"Проверь логин.","question":""}
````

**Проверка:** Canonical token 'пароль' has 6 code points; raw 'gfhjkm' has 6. Apply only the visible raw-to-canonical map in order; its one-to-one inverse returns the same raw token. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The answer text must be the entire original sentence after this single justified restoration; rejected produces a different, unsupported restoration.

### TQ26.TYPO-LAYOUT.ru.02

**Prompt**

````text
Нормализуй исходный текст на языке «русский», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: Ровно выделенная последовательность набрана по другой раскладке. Обычное слово, не идентификатор. Применяй только напечатанную карту; символы внутри последовательности, в том числе запятая, тоже являются входом карты.
Выделенная последовательность: jryj
Карта raw→canonical: {"j": "о", "r": "к", "y": "н"}
Исходный текст: Открой jryj.
````

**Chosen**

````text
{"action":"restore","text":"Открой окно.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"Открой дверь.","question":""}
````

**Проверка:** Canonical token 'окно' has 4 code points; raw 'jryj' has 4. Apply only the visible raw-to-canonical map in order; its one-to-one inverse returns the same raw token. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The answer text must be the entire original sentence after this single justified restoration; rejected produces a different, unsupported restoration.

Источники проекта: [RAG-023](../../rag/sources/RAG-023.md), [RAG-024](../../rag/sources/RAG-024.md). Это источники метода, не сертификат качества пары.

## TYPO-LAYOUT/he

Ошибочная раскладка клавиатуры

Нормализуй исходный текст внутри его языка, без перевода. В prompt укажи исходный язык, точный текст, разрешённую операцию и достаточный контекст. В metadata сохраняй raw текст, операцию, индексы Unicode code points, точную словоформу, lemma/sense и lexical_family_ids. После одной локальной операции остальная строка должна совпасть посимвольно. Семья лексемы сохраняется у опечатки и ошибки раскладки; независимые реальные слова не сливаются. Для restore исходная форма должна быть однозначной по видимому контексту; нельзя печатать её как подсказку в самом задании. Используй только точную карту domains/translation/layout-fixture-v1.json. Нужные пары клавиш, направление и правила пунктуации видны ученику. Прямое преобразование canonical→raw и обратное дают точное совпадение. Shift/CapsLock не изобретать; неподдержанное преобразование не назначать.

Работай только с назначенными domain_id/objective_id/profile_id/count/family_id. Пиши реальные prompt/chosen/rejected. Одна пара содержит один содержательный контраст, а все решающие сведения видны ученику в prompt. Публичные образцы не копировать в TRAIN и не переименовывать в независимый EXAM. Производные одного сюжета сохраняют origin_family_id. English internal reasoning; private reasoning не выводить. Краткие статусы по-русски. Модель текущая в Kiro, без override. CREATE_ONLY: без checksums, второго reviewer, acceptance, export/import, обучения и production CI. После finish выполнить выданный next до квоты; прямые NEXT/STOP исполняются немедленно. Пути и shared status берутся из ответа координатора.

**Ответ:** Только JSON {"action":"restore|preserve|clarify","text":"полный текст на исходном языке","question":""}. Для preserve/clarify text дословно равен исходному; question непустой только для clarify, на исходном языке. Для restore меняется только разрешённое повреждение.

### Простая самопроверка

- Separate permission to edit, exact character operation, and semantic uniqueness. All three must agree.
- Compare the whole sentence before/after, not only the isolated word. Preserve punctuation, case, inflection and other tokens.
- For ambiguity keep canonical_source null; two candidate repairs prove why guessing is not authorized.
- A precomposed letter and combining marks are not interchangeable counting units. Use the assigned unpointed simple Unicode cases; nikud requires a separate explicit task, never silently normalize it.

### TQ26.TYPO-LAYOUT.he.01

**Prompt**

````text
Нормализуй исходный текст на языке «иврит», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: Ровно выделенная последовательность набрана по другой раскладке. Обычное слово, не идентификатор. Применяй только напечатанную карту; символы внутри последовательности, в том числе запятая, тоже являются входом карты.
Выделенная последовательность: vxpr
Карта raw→canonical: {"v": "ה", "x": "ס", "p": "פ", "r": "ר"}
Исходный текст: vxpr על המדף.
````

**Chosen**

````text
{"action":"restore","text":"הספר על המדף.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"העיתון על המדף.","question":""}
````

**Проверка:** Canonical token 'הספר' has 4 code points; raw 'vxpr' has 4. Apply only the visible raw-to-canonical map in order; its one-to-one inverse returns the same raw token. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The answer text must be the entire original sentence after this single justified restoration; rejected produces a different, unsupported restoration.

### TQ26.TYPO-LAYOUT.he.02

**Prompt**

````text
Нормализуй исходный текст на языке «иврит», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: Ровно выделенная последовательность набрана по другой раскладке. Обычное слово, не идентификатор. Применяй только напечатанную карту; символы внутри последовательности, в том числе запятая, тоже являются входом карты.
Выделенная последовательность: vnf,c
Карта raw→canonical: {"v": "ה", "n": "מ", "f": "כ", ",": "ת", "c": "ב"}
Исходный текст: שלח את vnf,c.
````

**Chosen**

````text
{"action":"restore","text":"שלח את המכתב.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"שלח את המסמך.","question":""}
````

**Проверка:** Canonical token 'המכתב' has 5 code points; raw 'vnf,c' has 5. Apply only the visible raw-to-canonical map in order; its one-to-one inverse returns the same raw token. Replacing only the selected span in the whole sentence restores every original character; case, inflection, spaces and punctuation outside that span stay unchanged. The answer text must be the entire original sentence after this single justified restoration; rejected produces a different, unsupported restoration.

Источники проекта: [RAG-023](../../rag/sources/RAG-023.md), [RAG-024](../../rag/sources/RAG-024.md). Это источники метода, не сертификат качества пары.

## TYPO-PRESERVE/en

Сохранение правильного слова и точного литерала

Нормализуй исходный текст внутри его языка, без перевода. В prompt укажи исходный язык, точный текст, разрешённую операцию и достаточный контекст. В metadata сохраняй raw текст, операцию, индексы Unicode code points, точную словоформу, lemma/sense и lexical_family_ids. После одной локальной операции остальная строка должна совпасть посимвольно. Семья лексемы сохраняется у опечатки и ошибки раскладки; независимые реальные слова не сливаются. Для restore исходная форма должна быть однозначной по видимому контексту; нельзя печатать её как подсказку в самом задании. В этой микроцели ВСЕ chosen имеют action=preserve: либо слово правильно употреблено в контексте, либо точный идентификатор/цитата не разрешены к исправлению. Заданный class_schedule чередует valid_word/literal; это явно назначенные два подкласса одной цели сохранения. Не добавлять restore-половину по собственной инициативе.

Работай только с назначенными domain_id/objective_id/profile_id/count/family_id. Пиши реальные prompt/chosen/rejected. Одна пара содержит один содержательный контраст, а все решающие сведения видны ученику в prompt. Публичные образцы не копировать в TRAIN и не переименовывать в независимый EXAM. Производные одного сюжета сохраняют origin_family_id. English internal reasoning; private reasoning не выводить. Краткие статусы по-русски. Модель текущая в Kiro, без override. CREATE_ONLY: без checksums, второго reviewer, acceptance, export/import, обучения и production CI. После finish выполнить выданный next до квоты; прямые NEXT/STOP исполняются немедленно. Пути и shared status берутся из ответа координатора.

**Ответ:** Только JSON {"action":"restore|preserve|clarify","text":"полный текст на исходном языке","question":""}. Для preserve/clarify text дословно равен исходному; question непустой только для clarify, на исходном языке. Для restore меняется только разрешённое повреждение.

### Простая самопроверка

- Separate permission to edit, exact character operation, and semantic uniqueness. All three must agree.
- Compare the whole sentence before/after, not only the isolated word. Preserve punctuation, case, inflection and other tokens.
- For ambiguity keep canonical_source null; two candidate repairs prove why guessing is not authorized.
- A precomposed letter and combining marks are not interchangeable counting units. Use the assigned unpointed simple Unicode cases; nikud requires a separate explicit task, never silently normalize it.

### TQ26.TYPO-PRESERVE.en.01

**Prompt**

````text
Нормализуй исходный текст на языке «английский», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: Исходный текст правильный. Port — порт устройства; опечатка не заявлена.
Исходный текст: Use the port on the left.
````

**Chosen**

````text
{"action":"preserve","text":"Use the port on the left.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"Use the part on the left.","question":""}
````

**Проверка:** The learner is explicitly told the text is correct or the selected token is literal. Compare the whole text exactly; preserve is required. A nearby word or a reversible key map does not authorize changing it.

### TQ26.TYPO-PRESERVE.en.02

**Prompt**

````text
Нормализуй исходный текст на языке «английский», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: form_id — точный, регистрозависимый идентификатор. Его нельзя исправлять как обычное слово.
Исходный текст: Set form_id to 7.
````

**Chosen**

````text
{"action":"preserve","text":"Set form_id to 7.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"Set from_id to 7.","question":""}
````

**Проверка:** The learner is explicitly told the text is correct or the selected token is literal. Compare the whole text exactly; preserve is required. A nearby word or a reversible key map does not authorize changing it.

Источники проекта: [RAG-023](../../rag/sources/RAG-023.md), [RAG-024](../../rag/sources/RAG-024.md). Это источники метода, не сертификат качества пары.

## TYPO-PRESERVE/ru

Сохранение правильного слова и точного литерала

Нормализуй исходный текст внутри его языка, без перевода. В prompt укажи исходный язык, точный текст, разрешённую операцию и достаточный контекст. В metadata сохраняй raw текст, операцию, индексы Unicode code points, точную словоформу, lemma/sense и lexical_family_ids. После одной локальной операции остальная строка должна совпасть посимвольно. Семья лексемы сохраняется у опечатки и ошибки раскладки; независимые реальные слова не сливаются. Для restore исходная форма должна быть однозначной по видимому контексту; нельзя печатать её как подсказку в самом задании. В этой микроцели ВСЕ chosen имеют action=preserve: либо слово правильно употреблено в контексте, либо точный идентификатор/цитата не разрешены к исправлению. Заданный class_schedule чередует valid_word/literal; это явно назначенные два подкласса одной цели сохранения. Не добавлять restore-половину по собственной инициативе.

Работай только с назначенными domain_id/objective_id/profile_id/count/family_id. Пиши реальные prompt/chosen/rejected. Одна пара содержит один содержательный контраст, а все решающие сведения видны ученику в prompt. Публичные образцы не копировать в TRAIN и не переименовывать в независимый EXAM. Производные одного сюжета сохраняют origin_family_id. English internal reasoning; private reasoning не выводить. Краткие статусы по-русски. Модель текущая в Kiro, без override. CREATE_ONLY: без checksums, второго reviewer, acceptance, export/import, обучения и production CI. После finish выполнить выданный next до квоты; прямые NEXT/STOP исполняются немедленно. Пути и shared status берутся из ответа координатора.

**Ответ:** Только JSON {"action":"restore|preserve|clarify","text":"полный текст на исходном языке","question":""}. Для preserve/clarify text дословно равен исходному; question непустой только для clarify, на исходном языке. Для restore меняется только разрешённое повреждение.

### Простая самопроверка

- Separate permission to edit, exact character operation, and semantic uniqueness. All three must agree.
- Compare the whole sentence before/after, not only the isolated word. Preserve punctuation, case, inflection and other tokens.
- For ambiguity keep canonical_source null; two candidate repairs prove why guessing is not authorized.
- A precomposed letter and combining marks are not interchangeable counting units. Use the assigned unpointed simple Unicode cases; nikud requires a separate explicit task, never silently normalize it.

### TQ26.TYPO-PRESERVE.ru.01

**Prompt**

````text
Нормализуй исходный текст на языке «русский», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: Исходный текст правильный. Мяч — игровой предмет; опечатка не заявлена.
Исходный текст: Она взяла мяч.
````

**Chosen**

````text
{"action":"preserve","text":"Она взяла мяч.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"Она взяла меч.","question":""}
````

**Проверка:** The learner is explicitly told the text is correct or the selected token is literal. Compare the whole text exactly; preserve is required. A nearby word or a reversible key map does not authorize changing it.

### TQ26.TYPO-PRESERVE.ru.02

**Prompt**

````text
Нормализуй исходный текст на языке «русский», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: jryj.txt — точное буквальное имя файла. Даже если jryj можно декодировать как окно, здесь это не разрешено.
Исходный текст: Открой файл jryj.txt.
````

**Chosen**

````text
{"action":"preserve","text":"Открой файл jryj.txt.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"Открой файл окно.txt.","question":""}
````

**Проверка:** The learner is explicitly told the text is correct or the selected token is literal. Compare the whole text exactly; preserve is required. A nearby word or a reversible key map does not authorize changing it.

Источники проекта: [RAG-023](../../rag/sources/RAG-023.md), [RAG-024](../../rag/sources/RAG-024.md). Это источники метода, не сертификат качества пары.

## TYPO-PRESERVE/he

Сохранение правильного слова и точного литерала

Нормализуй исходный текст внутри его языка, без перевода. В prompt укажи исходный язык, точный текст, разрешённую операцию и достаточный контекст. В metadata сохраняй raw текст, операцию, индексы Unicode code points, точную словоформу, lemma/sense и lexical_family_ids. После одной локальной операции остальная строка должна совпасть посимвольно. Семья лексемы сохраняется у опечатки и ошибки раскладки; независимые реальные слова не сливаются. Для restore исходная форма должна быть однозначной по видимому контексту; нельзя печатать её как подсказку в самом задании. В этой микроцели ВСЕ chosen имеют action=preserve: либо слово правильно употреблено в контексте, либо точный идентификатор/цитата не разрешены к исправлению. Заданный class_schedule чередует valid_word/literal; это явно назначенные два подкласса одной цели сохранения. Не добавлять restore-половину по собственной инициативе.

Работай только с назначенными domain_id/objective_id/profile_id/count/family_id. Пиши реальные prompt/chosen/rejected. Одна пара содержит один содержательный контраст, а все решающие сведения видны ученику в prompt. Публичные образцы не копировать в TRAIN и не переименовывать в независимый EXAM. Производные одного сюжета сохраняют origin_family_id. English internal reasoning; private reasoning не выводить. Краткие статусы по-русски. Модель текущая в Kiro, без override. CREATE_ONLY: без checksums, второго reviewer, acceptance, export/import, обучения и production CI. После finish выполнить выданный next до квоты; прямые NEXT/STOP исполняются немедленно. Пути и shared status берутся из ответа координатора.

**Ответ:** Только JSON {"action":"restore|preserve|clarify","text":"полный текст на исходном языке","question":""}. Для preserve/clarify text дословно равен исходному; question непустой только для clarify, на исходном языке. Для restore меняется только разрешённое повреждение.

### Простая самопроверка

- Separate permission to edit, exact character operation, and semantic uniqueness. All three must agree.
- Compare the whole sentence before/after, not only the isolated word. Preserve punctuation, case, inflection and other tokens.
- For ambiguity keep canonical_source null; two candidate repairs prove why guessing is not authorized.
- A precomposed letter and combining marks are not interchangeable counting units. Use the assigned unpointed simple Unicode cases; nikud requires a separate explicit task, never silently normalize it.

### TQ26.TYPO-PRESERVE.he.01

**Prompt**

````text
Нормализуй исходный текст на языке «иврит», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: Исходный текст правильный. Речь о дереве; опечатка не заявлена.
Исходный текст: העץ ליד הבית.
````

**Chosen**

````text
{"action":"preserve","text":"העץ ליד הבית.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"העז ליד הבית.","question":""}
````

**Проверка:** The learner is explicitly told the text is correct or the selected token is literal. Compare the whole text exactly; preserve is required. A nearby word or a reversible key map does not authorize changing it.

### TQ26.TYPO-PRESERVE.he.02

**Prompt**

````text
Нормализуй исходный текст на языке «иврит», не переводи его. Верни только JSON с тремя ключами action, text, question. action — restore при однозначном исправлении, preserve при отсутствии разрешённого исправления, clarify если вариантов несколько. В text сохрани весь исходный текст с единственным разрешённым изменением; при preserve/clarify оставь текст без изменений. question — пустая строка, кроме краткого уточнения на исходном языке при clarify.
Условие: akuo.txt — точное буквальное имя файла; не преобразовывай латинские буквы в слово שלום.
Исходный текст: פתח את הקובץ akuo.txt.
````

**Chosen**

````text
{"action":"preserve","text":"פתח את הקובץ akuo.txt.","question":""}
````

**Rejected**

````text
{"action":"restore","text":"פתח את הקובץ שלום.txt.","question":""}
````

**Проверка:** The learner is explicitly told the text is correct or the selected token is literal. Compare the whole text exactly; preserve is required. A nearby word or a reversible key map does not authorize changing it.

Источники проекта: [RAG-023](../../rag/sources/RAG-023.md), [RAG-024](../../rag/sources/RAG-024.md). Это источники метода, не сертификат качества пары.

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
