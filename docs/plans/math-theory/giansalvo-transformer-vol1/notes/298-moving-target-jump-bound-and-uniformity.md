---
title: "298. Moving-target refinement: локальная ошибка projector и независимость от K"
source_pdf_pages: [709, 710]
book_pages: [673, 674]
batch: "015"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 298. Moving-target refinement: локальная ошибка projector и независимость от K

## Что утверждает Corollary 17.12

Источник заменяет jump bound на $J_{\max}^{\mathrm{unified}}=O(1/n+\varepsilon_{\mathrm{reg}})$ и использует внешний cluster gap в dwell-time condition. Практический замысел понятен: надёжная orthogonalisation ограничивает ошибки дефляции, а block method опирается на отделённость всего cluster. Но исчезновение зависимости от $K$ требует дополнительных uniform estimates. [Corollary 17.12 и (17.43)–(17.44)](../source/0706-0709-ch17-subspace-bias-and-conditional-convergence.md#corollary-17-12).

В частности, собственная (17.35) оценивает orthogonality defect как $O(K\varepsilon_{\mathrm{mach}})$, тогда как proof corollary подставляет $O(\varepsilon_{\mathrm{mach}})$ без $K$. Исходная ссылка “Equation **??**” также не определяет, какой полный network estimate используется. Ровная empirical curve не заменяет доказательства равномерности. [Исходная оценка (17.35)](../source/0701-0702-ch17-incremental-qr-and-spectral-method-comparison.md#equation-17-35).

## Проверяемое локальное ядро

Пусть $Q$ имеет полный column rank и

$$
\delta=\|Q^\top Q-I\|_2<1,\qquad
U=Q(Q^\top Q)^{-1/2}.
$$

Тогда $U^\top U=I$, $P=I-UU^\top$ — точный projector на complement, а $\widetilde P=I-QQ^\top$ удовлетворяет $\|\widetilde P-P\|_2=\delta$. При любом $A$:

$$
\|\widetilde P A\widetilde P-PAP\|_2
\le (2\delta+\delta^2)\|A\|_2.
$$

Действительно, при $D=\widetilde P-P$ разность равна $DAP+PAD+DAD$, и $\|P\|_2\le1$. Эта оценка непосредственно показывает, зачем контролировать orthogonality defect. Но она также сохраняет всю зависимость $\delta$ от числа столбцов, conditioning и истории обновлений.

Чтобы получить глобальный jump bound, надо ещё перенести perturbation residual operator через конкретную сеть или Lyapunov functional. Число слоёв, нормы промежуточных операторов и степень накопления ошибок не исчезают из доказательства только потому, что один локальный projector стал точнее.

## Regularisation bias имеет нормировку

Lemma 17.10 даёт локальный angular scale $\varepsilon_{\mathrm{reg}}\|R\|_2/\delta_{\mathrm{out}}$. Запись $O(\varepsilon_{\mathrm{reg}})$ допустима, если спрятанные коэффициенты — включая $\|R\|_2/\delta_{\mathrm{out}}$ и downstream sensitivity — ограничены на рассматриваемом семействе задач. Если gap уменьшается или Gaussian perturbation меняет норму с размерностью, такой uniform claim надо доказывать заново.

Это относится и к утверждению, что dwell time одинаково конечен для всех событий: U2 говорит о положительности каждого gap, а не сама по себе об их общей нижней границе. [Условия theorem](../source/0706-0709-ch17-subspace-bias-and-conditional-convergence.md#assumption-17-5-u2), [разбор в карточке 297](297-unified-convergence-hypotheses-rates-and-rank-preservation.md).

## Что означают приведённые масштабы

Для $n=200$, $K=100$ и условного $\varepsilon_{\mathrm{mach}}=10^{-16}$:

$$
\frac1n=5\cdot10^{-3},\qquad
K\varepsilon_{\mathrm{mach}}=10^{-14}.
$$

Первое слагаемое больше второго в $5\cdot10^{11}$ раз. Поэтому source правильно отмечает отсутствие ведущего улучшения в этом data-dominated режиме. При условном default $\varepsilon_{\mathrm{reg}}\approx10^{-8}$ и норме оператора порядка единицы regularisation term тоже гораздо меньше $1/n$, хотя больше старого roundoff term. Это сравнение масштабов; оно не превращает Big-O expressions с неизвестными константами в измеренные jumps. [Продолжение corollary и practical consequence](../source/0710-0713-ch17-tracking-complexity-and-kernel-example.md#pdf-0710).

Полезный engineering вывод — измерять orthogonality defect, projector error и accepted-event jump отдельно. Корректность local QR и численное улучшение конкретного запуска не устанавливают заявленную независимость всей moving-target динамики от $K$.
