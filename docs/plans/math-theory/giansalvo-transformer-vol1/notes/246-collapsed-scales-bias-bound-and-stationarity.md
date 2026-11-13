---
title: "246. Совпавшие временные масштабы: ошибки bias floor и условия tracking"
source_pdf_pages: [630, 632]
book_pages: [594, 596]
batch: "013"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 246. Совпавшие временные масштабы: ошибки bias floor и условия tracking

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Две динамики и выбранная цель

§16.11 обозначает $\rho_{\rm sep}=\eta^+/\alpha$ и рассматривает конечное отношение task/gate steps. Theorem 16.39 утверждает geometric contraction squared distance к $z^*$ до floor $\sigma_{\rm bias}^2/\rho_{\rm sep}$. При этом вводный текст говорит о moving residual operator, а формула измеряет расстояние до одной неиндексированной точки. Нужно различить фиксированный equilibrium и меняющуюся цель $z_t^*$. [Постановка](../source/0628-0630-ch16-nonorthogonal-random-and-adversarial-growth.md#section-16-11), [Теорема 16.39](../source/0631-0634-ch16-collapsed-scales-and-polyak-ruppert-recovery.md#theorem-16-39).

Для geometric множителя также требуется режим шага, в котором он задаёт неотрицательную убывающую envelope, например $0\le1-4c_0\eta^+\Delta^+<1$. Грубое удаление A3 без замены соответствующим tracking condition не гарантирует это и не контролирует быстро движущиеся eigenvectors при малом spectral gap. Слова о comparable magnitudes сами не задают таких условий.

## Три разные формулы вместо одной

Обозначим $G^2=\|\nabla_{M_a}\ell\|_F^2$, $\eta=\eta^+$, $\Delta=\Delta^+$, $\rho=\rho_{\rm sep}$ и используем $\alpha=\eta/\rho$. В theorem definition floor равен

$$
A=\frac{G^2}{\Delta\rho}.
$$

В proof его отождествляют последовательно с

$$
B=\frac{\alpha^2G^2}{(\eta\Delta)^2}=\frac{G^2}{\rho^2\Delta^2},
\qquad C=\frac{\eta G^2}{\rho^2\Delta}.
$$

В общем случае $A\ne B\ne C$. Например, $\eta=0.1,\Delta=2,\rho=4,G^2=1$ дают 0.125, 0.015625 и 0.003125. Это обычная подстановка в напечатанные равенства, без моделирования gate dynamics. [Theorem floor](../source/0631-0634-ch16-collapsed-scales-and-polyak-ruppert-recovery.md#equation-16-61), [Proof sketch с несовпадающими выражениями](../source/0631-0634-ch16-collapsed-scales-and-polyak-ruppert-recovery.md#pdf-0632).

Вводный абзац PDF 631 говорит о радиусе пропорционально $1/\rho$, а последующий текст использует $1/\sqrt\rho$. Первая зависимость могла бы относиться к одному из иных вариантов squared floor, но не следует из фактически заявленного (16.61). Нужно сначала заново вывести recurrence, а затем согласовать квадрат нормы, radius и единицы.

## MSE не равна statistical bias

Для оценки $Z$ верно

$$
\mathbb E\|Z-z^*\|^2
=\|\mathbb EZ-z^*\|^2+\mathbb E\|Z-\mathbb EZ\|^2.
$$

Положительная верхняя граница MSE не доказывает ни ненулевого смещения, ни отсутствия сходимости: она может быть неточной, а ошибка может состоять только из variance. Поэтому фразы «unbiased only in the two-time-scale limit» и «persistent bias» сильнее указанного theorem. [The implication for diagnostics](../source/0631-0634-ch16-collapsed-scales-and-polyak-ruppert-recovery.md#pdf-0632).

Числовой аргумент безопасности тоже смешивает величины. При $\rho=1$ и $\sigma_{\rm bias}^2\sim10^{-2}$ соответствующая RMS scale равна $10^{-1}$, что **больше**, а не меньше заявленного threshold gap $5\times10^{-2}$. К тому же norm error eigenvector pair и scalar diagnostic gap имеют разные единицы; нужен Lipschitz или perturbation bound, переводящий одно в другое, и нужный confidence level.

Для реализации полезны явно заданные target, regime stationarity, error bars конкретного scalar test и запасы относительно порога. Фиксация одинакового optimizer step для разных parameter groups не доказывает одинаковой эффективной скорости всех наблюдаемых. Авторские `[citation]` и empirical magnitude claims сохранены как сообщения источника; эксплуатационная безопасность controller ими не установлена.
