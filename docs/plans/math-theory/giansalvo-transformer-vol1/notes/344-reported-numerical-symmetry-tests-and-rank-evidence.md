---
title: "344. Численные проверки: что подтверждают пять runs и таблица rank 32"
source_pdf_pages: [775, 776]
book_pages: [739, 740]
batch: "016"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 344. Численные проверки: что подтверждают пять runs и таблица rank 32

## Вопрос

Можно ли считать Table 19.3–19.4 доказательством устранения rank collapse?

**Таблицы сообщают локальные авторские проверки малых матриц. Они полезны для проверки алгебраических тождеств, но не заменяют доказательство generic результата или исследование обученной глубокой модели.**

Источник: [Table 19.3](../source/0774-0776-ch19-cost-tables-numerical-audit-and-ddcl.md#table-19-3), [Table 19.4](../source/0774-0776-ch19-cost-tables-numerical-audit-and-ddcl.md#table-19-4).

## Какие проверки перечислены

В книге сообщены 20 случайных diagonal gauge trials с относительной ошибкой $3.3\cdot10^{-16}$, 20 non-diagonal trials с ошибкой около 60 и 20 head permutations с ошибкой $1.6\cdot10^{-16}$. Малые ошибки согласуются с точными алгебраическими равенствами; большие ошибки показывают нарушение проверенных преобразований в данной выборке. Они не доказывают отсутствие всех других симметрий.

Rank experiment содержит пять trials и 12 слоёв: для pure attention указан rank 1 с collapse на слое 3; для attention+residual и MHB-G без residual — rank 32 на слое 12. Шесть configurations full bilinear span дали dimension $d_h$. Эти результаты здесь перенесены как **сообщения автора**; исходный NumPy notebook, seeds и runs в процессе преобразования не воспроизводились.

## Что требуется для точной интерпретации

Нужны определения numerical rank и порога, точная activation, norm, residual placement, distributions weights/inputs и правила routing. Формулировка «rank preserved at 32» не задаёт singular spectrum: несколько values могут быть очень близки к порогу. В зависимости от $n,d$ maximum rank также меняется; 32 не является максимальным рангом для всех перечисленных configurations.

Название «four lemmas» относится к 19.2, 19.4, 19.6, 19.8; проверка anchored-head Lemma 19.5 в таблице не представлена. Нельзя распространить unanchored head-permutation test на успешное устранение anchored non-identifiability.

## Связь с более поздними результатами

Remark 19.7 прямо допускает ухудшение rank в глубокой обученной модели и отсылает к §19.9. Поэтому Table 19.4 и последующая потеря rank после pretraining описывают разные settings и не обязаны совпадать. Точная гипотеза для переноса — «при данных условиях bilinear layer может лучше сохранять численный span»; утверждение «collapse устранён для любых depths и обучения» этими trials не установлено.
