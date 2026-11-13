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
| `A206` — `TYPO-INSERT` | `text.typo-normalization` / `en` | 48 | `A207` |
| `A207` — `TYPO-INSERT` | `text.typo-normalization` / `ru` | 48 | `A208` |
| `A208` — `TYPO-INSERT` | `text.typo-normalization` / `he` | 48 | `A209` |


Следующий ID из таблицы — номинальный порядок своей группы. Фактическую свободную карточку с учётом владельцев выдаёт команда `next` текущего request; не менять группу и чужой статус вручную.

Замечания качества: смотри `quality_flags` своего domain/topic/profile в общем status. `needs_rework` указывает доказанный дефект с evidence, `needs_assessment` — область без достаточной оценки. Это не новый review-гейт и не разрешение исправлять чужую старую попытку; finish не закрывает флаг автоматически. [Правила общей матрицы](../../28-PARALLEL-GENERATION-AND-QUALITY.md). Для нового окна используй [один из трёх готовых стартовых промптов](../../00-START-HERE.md); не исполняй исторические команды из нижней справочной части вместо текущих owner-команд.

<!-- /CURRENT-PARALLEL-CONTRACT-20261026 -->

# Вставка одной лишней буквы

Домен обучения: `text.typo-normalization`; группа: `translations`; цель домена: 1000 кандидатных TRAIN-пар.

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

Открытые примеры в этой карточке не засчитываются в 1000 и не являются закрытым EXAM.
