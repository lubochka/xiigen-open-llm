# RAG-TR-001 — Переводные пары, которым требуется повторная разметка

Источник: [llm mvp/обучение языку/1st game/curated_ru.py](https://github.com/lubochka/xiigen-planning/blob/66cd68b247ad9d92570b37594fcb2cb5eb33a6cc/llm%20mvp/%D0%BE%D0%B1%D1%83%D1%87%D0%B5%D0%BD%D0%B8%D0%B5%20%D1%8F%D0%B7%D1%8B%D0%BA%D1%83/1st%20game/curated_ru.py), ветка `llm_plan3`, commit `66cd68b247ad9d92570b37594fcb2cb5eb33a6cc`. Git blob `0681b0b41014adfbf586eb3f037f8cb698c89e06`.

## Что подтверждает источник

В старом curated-наборе часть ответов отмечена как ошибочная без достаточного условия: допустимое «Пошли мне файл», допустимое «Это не имеет значения», возможное «They call me Anna». Для director выбран «режиссёр», хотя кино/театр не указаны в learner input. Эти наблюдения относятся только к приведённым строкам. Весь набор не объявляется неверным.

## Как использовать

Это источник проверки качества и конструирования контрпримеров, а не готовая gold-разметка. Не импортировать ok/bad автоматически. Для director новый пример должен явно задавать кино или управление организацией; для допустимых синонимов preference отвергается, пока в запросе не задан осмысленный критерий. Нельзя помещать такой критерий только в rationale: студент его не увидит. Исходник сохраняется без изменений; новая версия пары получает собственный ID, derived_from и общий origin family.

Статус: evidence-only; предметная проверка необходима. Это не измерение обучения.

## Прочитанный фрагмент L21–L22

[Оригинал](https://github.com/lubochka/xiigen-planning/blob/66cd68b247ad9d92570b37594fcb2cb5eb33a6cc/llm%20mvp/%D0%BE%D0%B1%D1%83%D1%87%D0%B5%D0%BD%D0%B8%D0%B5%20%D1%8F%D0%B7%D1%8B%D0%BA%D1%83/1st%20game/curated_ru.py#L21-L22)

```python
{"dir":"EN_RU","src":"The director is very talented.","ok":"Режиссёр очень талантливый.","bad":"Директор очень талантливый.","type":"false_friend",
 "why":"В кино и театре — «режиссёр». «Директор» — руководитель организации. Ложный друг."},
```

## Прочитанный фрагмент L29–L30

[Оригинал](https://github.com/lubochka/xiigen-planning/blob/66cd68b247ad9d92570b37594fcb2cb5eb33a6cc/llm%20mvp/%D0%BE%D0%B1%D1%83%D1%87%D0%B5%D0%BD%D0%B8%D0%B5%20%D1%8F%D0%B7%D1%8B%D0%BA%D1%83/1st%20game/curated_ru.py#L29-L30)

```python
{"dir":"EN_RU","src":"Send me the file.","ok":"Пришли мне файл.","bad":"Пошли мне файл.","type":"grammar_error",
 "why":"«Пришли» — верно. «Пошли» в разговорной речи звучит грубо и значит другое."},
```

## Прочитанный фрагмент L35–L36

[Оригинал](https://github.com/lubochka/xiigen-planning/blob/66cd68b247ad9d92570b37594fcb2cb5eb33a6cc/llm%20mvp/%D0%BE%D0%B1%D1%83%D1%87%D0%B5%D0%BD%D0%B8%D0%B5%20%D1%8F%D0%B7%D1%8B%D0%BA%D1%83/1st%20game/curated_ru.py#L35-L36)

```python
{"dir":"EN_RU","src":"It doesn't matter.","ok":"Неважно.","bad":"Это не имеет значения.","type":"wrong_register",
 "why":"«Неважно» — живой разговорный вариант. «Не имеет значения» — формальная, письменная фраза."},
```

## Прочитанный фрагмент L54–L55

[Оригинал](https://github.com/lubochka/xiigen-planning/blob/66cd68b247ad9d92570b37594fcb2cb5eb33a6cc/llm%20mvp/%D0%BE%D0%B1%D1%83%D1%87%D0%B5%D0%BD%D0%B8%D0%B5%20%D1%8F%D0%B7%D1%8B%D0%BA%D1%83/1st%20game/curated_ru.py#L54-L55)

```python
{"dir":"RU_EN","src":"Меня зовут Анна.","ok":"My name is Anna.","bad":"They call me Anna.","type":"literal_translation",
 "why":"«They call me» звучит неестественно. Идиоматичный английский — «My name is Anna»."},
```

