---
title: "279. Stratified growth: изоляция bins, непрерывная гарантия и ограниченность empirical density"
source_pdf_pages: [677, 679]
book_pages: [641, 643]
batch: "014"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 279. Stratified growth: изоляция bins, непрерывная гарантия и ограниченность empirical density

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Вопрос

Гарантирует ли per-bin trigger локальность новой головы и подтверждена ли head-count density?

## Что действительно предписано

[§16.18.5](../source/0675-0678-ch16-diffusion-family-bounds-and-stratified-growth.md#algorithm-0677-stratified-diffusion-growth) содержит **ненумерованное algorithmic prescription**, а не новый numbered Algorithm: хранить stratified diagnostics по $M$ bins, расти при превышении порога в любом bin и дефлировать только operator соответствующего bin. Для $M=16$ ширина $\Delta t=T/M$.

Это полезная схема диагностического интерфейса. Но метка bin у новой головы не доказывает, что её параметры влияют только на этот bin. Следующий абзац одновременно допускает gradients от всех bins. При общем параметре $\phi$ изменение другого residual имеет первый порядок

$$
\Delta A_b\approx D_\phi A_b[\Delta\phi].
$$

Чтобы утверждать нулевое влияние вне bin рождения, нужны, например, disjoint parameters, точная поддержка routing gate или доказанная нулевая соответствующая производная. Без этого необходим контроль cross-bin changes; хорошая дефляция одного оператора может ухудшить другой. [Продолжение предписания](../source/0675-0678-ch16-diffusion-family-bounds-and-stratified-growth.md#pdf-0678).

## Grid не обеспечивает весь continuum

Наблюдение residual в $M$ точках не доказывает bound для всех $t$. Если дополнительно известно, что $\Gamma(t)$ имеет Lipschitz constant $L$ и ошибки оценок в узлах не превосходят $\delta$, то при покрывающей сетке и nearest-node distance $\le\Delta t/2$ можно получить редакторскую оценку

$$
\sup_t\Gamma(t)\le\max_b\widehat\Gamma(t_b)+\delta+\frac{L\Delta t}{2}.
$$

Для меняющегося threshold нужен контроль и его variation. В исходнике такая регулярность не установлена. «Any bin» trigger также делает ошибочные срабатывания зависимыми от числа bins и повторных проверок, а редко sampled bins требуют собственных counts/confidence. Для хранения полных operators размерности $q$ нужно порядка $Mq^2$ чисел; low-rank diagnostics меняют cost и погрешность.

## Численная часть здесь арифметически согласована

[Table 16.14](../source/0679-0681-ch16-diffusion-calibration-and-concluding-conjectures.md#table-16-14) сравнивает $\widehat\gamma_K$ с $2/\beta$. Относительные ошибки по **неокруглённому** теоретическому значению равны:

| $\beta$ | $2/\beta$ | $\widehat\gamma_K$ | Относительная ошибка |
|---|---|---|---|
| 1 | 2 | 1.85 | 7.5% |
| 1.5 | 4/3 | 1.30 | 2.5% |
| 2 | 1 | 0.97 | 3.0% |
| 2.5 | 0.8 | 0.78 | 2.5% |
| 3 | 2/3 | 0.61 | 8.5% |

Все пять меньше 10%; [Figure 16.15](../source/0679-0681-ch16-diffusion-calibration-and-concluding-conjectures.md#figure-16-15) соответствует этим точкам. В отличие от ICL-таблицы, здесь нет арифметического нарушения заявленного tolerance. Это подтверждает согласованность напечатанных чисел, а не качество неописанного эксперимента.

## Что именно измеряет proxy

В setup $\widehat p_K(t)$ оценивается по magnitude вклада головы в gradient и затем аппроксимируется log–log regression. Это **activity/gradient proxy**, а не число выделенных голов. Одна фиксированная архитектура может менять gradient magnitudes с $t$ без изменения count. Нормировка, parametrisation и loss scaling тоже влияют на этот proxy.

Не приведены число runs, диапазон fitting, uncertainty slope и контроль других причин такого exponent. Поэтому совпадение пяти slopes не доказывает оптимальность allocation, интегральный head budget, отсутствие cross-bin interference или объяснение deviations конечной variance.

Для проекта схема bins допускает проверяемый прототип: отдельно регистрировать operator residual, active/head count, gradient proxy, cross-bin changes и стоимость. Этот разбор не содержит запуска DiT и не объявляет авторские результаты воспроизведёнными.
