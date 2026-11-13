---
title: "314. Proteome pretraining: сравнимость Figure 18.4 и границы convexity MLM"
source_pdf_pages: [733, 734]
book_pages: [697, 698]
batch: "015"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 314. Proteome pretraining: сравнимость Figure 18.4 и границы convexity MLM

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Вопрос

Что означает преимущество V9 на preview figure и следует ли quasi-convexity обучения из convexity cross-entropy?

## Что уже входит в этот фрагмент

§18.2 вводит INCRT **во время MLM pretraining**, где controller меняет head count и глубину encoder, а downstream protein-family benchmark оценивает полученные representations. Это отличается от выбора архитектуры по frozen pretrained encoder. Однако подробный механизм, полный ablation и протоколы продолжаются дальше; Figure 18.4 прямо названа preview. [Начало §18.2](../source/0732-0734-ch18-cross-task-validation-and-proteome-setup.md#section-18-2), [Figure 18.4](../source/0732-0734-ch18-cross-task-validation-and-proteome-setup.md#figure-18-4).

Числа ниже — утверждения книги о работах Cirrincione et al. (2026) и Cirrincione and Lovino (2026), а не независимо воспроизведённые нами benchmarks.

| Вариант из рисунка | Reported corpus | Parameters | Pfam-50 linear probe |
|---|---|---:|---:|
| ESM-2 small | UniRef50, около 60 M sequences | 7.5 M | 71.13% |
| ProtBERT | BFD, около 200 M sequences | 420 M | 72.18% |
| V9 | human proteome, около 20 k sequences | 22.7 M | 76.03% |

Разница V9 составляет **4.90 процентного пункта** против ESM-2 small и **3.85 пункта** против ProtBERT. V9 при этом примерно в 3.03 раза крупнее ESM-2 small по параметрам и в 18.50 раза меньше ProtBERT. Отношения номинальных чисел sequences составляют около 3000 и 10000. Эти различия делают Figure 18.4 содержательным benchmark comparison, но не изолируют эффект controller при равном parameter budget или равных вычислениях.

## Какие сравнения пока не установлены

Число sequences не равно числу tokens, числу update steps или FLOPs: длины белков, число epochs и sampling меняют объём вычислений. Потребуются одинаковые splits, правила deduplication и контроля homology/overlap, downstream training budget, pooling, tuning и uncertainty. Эти условия нельзя восстановить из preview figure.

На PDF 734 отдельно перечислены **linear probe на frozen mean-pooled representations** и **light fine-tuning всего encoder**. Проценты Figure 18.4 относятся именно к linear probe; их нельзя смешивать с ещё последующими fine-tuning результатами. Заявление о mechanistic role $\mathcal L_\alpha$ анонсирует ablation, а не само по себе доказывает causal necessity на всех задачах. [Протокол и предварительное описание](../source/0732-0734-ch18-cross-task-validation-and-proteome-setup.md#pdf-0734).

## Masking и цель обучения

При номинальной схеме «выбрать 15% позиций, затем 80/10/10» доли от всех позиций равны 12% для `[MASK]`, 1.5% для случайной замены и 1.5% для selected-but-unchanged; оставшиеся 85% не выбираются. Слово masked positions в таком описании требует различать loss positions и фактическую замену токена. Источник даёт общий BERT-style protocol, но в пределах этих страниц не задаёт seed маскирования, словарь replacement и особенности filtering. [§18.2.1](../source/0732-0734-ch18-cross-task-validation-and-proteome-setup.md#section-18-2-1).

## Convex loss по logits не делает encoder quasi-convex

Пункт (i) говорит о convexity per sample при фиксированных representations и затем помещает весь benchmark в «convex half-plane». Корректный перенос ограничен logits или linear classification head при фиксированных features. Nonlinear parameterisation меняет геометрию objective.

Уже простой logit $z=ab$ для одного положительного binary label даёт

$$
\ell(a,b)=\log(1+e^{-ab}).
$$

В точках $(1,1)$ и $(-1,-1)$ loss равен $\log(1+e^{-1})\approx.313262$, а в середине $(0,0)$ — $\log2\approx.693147$. Значение в середине больше обоих endpoint values, что нарушает определение quasi-convexity. Это не эксперимент на protein model, а точный counterexample к выводу из convexity cross-entropy по logit в quasi-convexity по обучаемым параметрам.

Утверждение об empirically near-convex basin сопровождается placeholder `?`; без метрики, области и evidence оно не обосновывает глобальную гарантию. Source сохраняет формулировку. Пункт (i) обрывается на PDF 734 после «the same region in which the classification campaigns of» и продолжается на [PDF 735](../source/0735-0738-ch18-proteome-mlm-mechanism-and-ablation.md#pdf-0735). Эта карточка фиксирует уже прочитанный setup и не подменяет им ещё последующий ablation.
