---
title: "309. Завершение главы 17: локальные гарантии, открытые проверки и цена dispatch"
source_pdf_pages: [723, 725]
book_pages: [687, 689]
batch: "015"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 309. Завершение главы 17: локальные гарантии, открытые проверки и цена dispatch

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Вопрос

Какие результаты главы 17 уже условно обоснованы, а какие остаются проверяемыми гипотезами для реального обучения?

## Полезная граница, которую устанавливает сам автор

Глава 17 заканчивается на PDF 725. В заключении прямо признаны локальность cubic RQI convergence, зависимость stochastic argument от moments и эмпирический iteration floor. В частности, не доказано, что Ritz vector из Lanczos с $m=2$ при любом ненулевом исходном overlap обязательно попадает в basin нужного eigenvector. Это самостоятельное условие композиции двух solver stages. [§17.8.1](../source/0722-0725-ch17-adaptive-dispatch-and-optimiser-aware-gates.md#section-17-8-1).

Пять открытых направлений перечислены конкретно: online floor estimation без ground truth; выбор $\gamma_{\rm near}$; real-scale validation; tracking drifting eigenspaces; optimiser-induced perturbations. Частичные operational bounds §17.7 не закрывают эти задачи автоматически. В частности, формула с неизвестными constants и ненормированным adaptive threshold ещё не является откалиброванным controller. [§17.8.2](../source/0722-0725-ch17-adaptive-dispatch-and-optimiser-aware-gates.md#section-17-8-2), [карточки 306–308: начало](306-numerical-floor-upper-bound-and-adaptive-threshold-units.md).

## Moments входа и moments стохастического обновления

Условие $\mathbb E\|X\|_F^2<\infty$ само по себе не доказывает bounded second moment всех функций от $X$. Если update содержит квадратичный множитель $X^2$, его second moment требует $\mathbb E X^4$. Например, хвост с $P(|X|>t)\asymp t^{-3}$ при больших $t$ имеет конечный второй и бесконечный четвёртый moment. Это редакторский пример логического различия, а не утверждение о распределении реальных данных автора.

Поэтому для применения stochastic convergence theorem надо проверять именно noise/update assumptions выбранной динамики вместе с boundedness итератов, шагами и условным centering. Тезис о «всех стандартных Transformer distributions» не заменяет эту проверку. Заключение источника само ограничивает проведённые проверки bounded synthetic data.

## Синтетическое наблюдение не завершает prospective experiment

Автор сообщает, что опыты главы выполнены при $d\le200$, одном growth event и контролируемом спектре. Повтор SARS-CoV-2 под Algorithm 7 с ожидаемым $K_{\rm struct}^*\approx3$ вместо $K^*=6$ назван **ещё не выполненным** и требующим реализации и интеграции. Это prediction/falsification target, а не результат главы 18, который можно приписать текущему фрагменту заранее. Число $8\cdot10^{-7}$ также относится к определённому synthetic protocol. [PDF 724](../source/0722-0725-ch17-adaptive-dispatch-and-optimiser-aware-gates.md#pdf-0724).

Согласие двух инициализаций не оценивает общий bias относительно истины. Например, обе могут попасть в один неверный eigenspace. А сравнение elapsed iterations требует определить стоимость каждого шага: Lanczos products, linear solves RQI, block iteration, diagnostics и orthogonalisation различны.

## Почему regular-branch recovery не означает worst-case parity

В §17.8.3 автор утверждает, что худший случай — parity, поскольку в regular regime unified gate сводится к прежнему. Логически это не следует для всего алгоритма: regular branch может иметь те же математические обновления и при этом дополнительную стоимость classification; misclassification, finite-budget solver, regularisation bias и переключения могут менять траекторию. Даже одинаковое число голов не устанавливает равенство task accuracy или времени. [Авторская оценка последствий](../source/0722-0725-ch17-adaptive-dispatch-and-optimiser-aware-gates.md#section-17-8-3).

Для Xiigen этот фрагмент полезен как точный перечень оставшихся экспериментальных вопросов. Сопоставимая проверка должна фиксировать data split, budget, seed, optimiser, режим solver, критерий события и стоимость diagnostics, а также сравнивать task quality при равных ресурсах. Библиотека сохраняет original claims вместе с ограничениями; новые запуски обучения и повтор prospective experiment не выполнялись.
