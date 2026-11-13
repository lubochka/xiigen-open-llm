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
| `A209` — `TYPO-LAYOUT` | `text.typo-normalization` / `en` | 48 | `A210` |
| `A210` — `TYPO-LAYOUT` | `text.typo-normalization` / `ru` | 47 | `A211` |
| `A211` — `TYPO-LAYOUT` | `text.typo-normalization` / `he` | 47 | `A212` |


Следующий ID из таблицы — номинальный порядок своей группы. Фактическую свободную карточку с учётом владельцев выдаёт команда `next` текущего request; не менять группу и чужой статус вручную.

Замечания качества: смотри `quality_flags` своего domain/topic/profile в общем status. `needs_rework` указывает доказанный дефект с evidence, `needs_assessment` — область без достаточной оценки. Это не новый review-гейт и не разрешение исправлять чужую старую попытку; finish не закрывает флаг автоматически. [Правила общей матрицы](../../28-PARALLEL-GENERATION-AND-QUALITY.md). Для нового окна используй [один из трёх готовых стартовых промптов](../../00-START-HERE.md); не исполняй исторические команды из нижней справочной части вместо текущих owner-команд.

<!-- /CURRENT-PARALLEL-CONTRACT-20261026 -->

# Ошибочная раскладка клавиатуры

Домен обучения: `text.typo-normalization`; группа: `translations`; цель домена: 1000 кандидатных TRAIN-пар.

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

Открытые примеры в этой карточке не засчитываются в 1000 и не являются закрытым EXAM.
