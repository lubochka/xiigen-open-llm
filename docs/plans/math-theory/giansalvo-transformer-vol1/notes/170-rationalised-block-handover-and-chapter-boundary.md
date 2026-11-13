---
title: "170. Переход к rationalised block: что передаётся дальше и где остановлен пакет"
source_pdf_pages: [499, 500]
book_pages: [463, 464]
batch: "010"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 170. Переход к rationalised block: что передаётся дальше и где остановлен пакет

[Индекс](../README.md)

## Три слоя содержания главы

[§13.11](../source/0496-0500-ch13-pruning-growth-and-synthesis.md#section-13-11) собирает предыдущие разделы в три категории:

1. **Словарь диагностик:** parametric/functional/dynamic redundancy, silence, crowding, marginal value и условная hierarchy.
2. **Предлагаемые архитектурные ограничения:** dictionary threshold и specialisation-aware FFN bound.
3. **Правила проектирования:** P1–P6 и процедуры pruning, compression, growth.

Эта организация полезна для поиска, но категории имеют разную силу. Определение наблюдаемой не доказывает её связь с task necessity; условная теорема не становится универсальным ограничением; rule of admissibility не гарантирует сохранение loss.

## Какой блок автор намерен строить

В [PDF 499–500](../source/0496-0500-ch13-pruning-growth-and-synthesis.md#pdf-0499) перечислен блок с отдельным бюджетом directional tails, разделением symmetric/antisymmetric channels, разбиением перегруженных голов, добавлением голов при residual gaps, неоднородной параметризацией и наблюдением orbital crowding.

Автор называет это семейством архитектур, зависящим от оставленных открытыми решений и порогов. Chapter 14 должна дать статическую конструкцию; Chapter 15 — динамический INCRT с добавлением, удалением и слиянием голов. **Содержание этих будущих конструкций в данный файл не переносится.** Здесь сохранено только описание перехода из §13.11.

Переход от дискретного добавления/удаления heads к «continuous flows» также является программой дальнейшего изложения. Нужно определить пространство состояний при переменном числе голов и соответствующий смысл непрерывности; это не следует из правил фиксированного checkpoint.

## Три сильных утверждения в финале требуют сохранения оговорок

На PDF 500 автор усиливает три тезиса: dictionary threshold назван обязательным для любого non-redundant block; gap 0.45 — жёстким требованием; защита directional tail — не эвристикой, а следствием orientation conservation.

При использовании нужно сохранять результаты редакторской проверки:

- [163](163-channel-symmetry-and-orbit-separation.md) показывает, что напечатанное определение separation сравнивает два вектора после одного обратимого отображения и само не задаёт нетривиальной задачи разрушения gauge.
- [164](164-dictionary-size-and-training-criterion.md) разделяет число атомов, матричный ранг и размерность их линейной оболочки; минимальный dictionary и convergence theorem не доказаны в заявленном виде.
- [165](165-specialisation-gap-and-coefficient-diversity.md) даёт невозможность gap 0.45 для четырёх и более heads и отмечает отсутствие контроля остатка в decorrelation estimate.
- [166](166-head-output-rank-and-feedforward-width.md) и [167](167-specialisation-aware-width-and-self-sufficiency.md) отделяют корректные upper bounds от неверных равенств ранга и от недоказанного FFN minimum.
- [168](168-six-design-principles-and-evidence-status.md) и [169](169-pruning-compression-and-growth-rules.md) различают сохранение агрегированного skew-content, незаменимость каждой головы и фактический эффект операции на output/loss.

Следовательно, полезный проектный результат этого прохода — набор адресуемых определений, диагностик и проверяемых гипотез. Готового доказанного рецепта ускорения обучения Xiigen или безопасного автоматического pruning он не даёт.

## Точная граница пакета 010

На **PDF 500 / книге 464** начинаются [§13.12 “Synthesis and Outlook”](../source/0496-0500-ch13-pruning-growth-and-synthesis.md#section-13-12) и [§13.12.1 “The orbit-crowding theorem”](../source/0496-0500-ch13-pruning-growth-and-synthesis.md#section-13-12-1). Перенесён только законченный вводный абзац последнего подраздела.

Его последняя фраза в оригинале содержит повтор:

> “the head-channel non-identifiability head-channel non-identifiability phenomenon.”

Повтор сохранён буквально. Абзац и предложение не оборваны, но **§13.12.1 и глава 13 ещё не завершены**. Сам заголовок со словом theorem не меняет статус вводимого утверждения: автор прямо называет его сильнейшим недоказанным conjectural statement.

Следующая физическая страница, **PDF 501 / книга 465**, начинается **Conjecture 13.45**. Она просмотрена только для определения продолжения и не включена в основной текст пакета. Следующий диапазон — **PDF 501–550 / книга 465–514**; новые русские карточки начинаются с **171**. [Запись точной границы](../source/0496-0500-ch13-pruning-growth-and-synthesis.md#batch-010-boundary).

## Дополнение из пакета 011

Глава 13 теперь завершена: [Conjecture 13.45 и outlook, PDF 501–504](../source/0501-0504-ch13-outlook-and-open-problems.md#pdf-0501). Итоговые гипотезы и границы переноса разобраны в [карточке 171](171-orbit-conjectures-cross-layer-counting-and-task-transfer.md). Глава 14 также полностью доступна до PDF 546; описанная выше граница пакета 010 остаётся исторической.
