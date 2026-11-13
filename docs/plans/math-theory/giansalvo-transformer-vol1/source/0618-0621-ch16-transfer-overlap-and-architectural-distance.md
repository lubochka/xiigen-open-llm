---
title: "Chapter 16: transfer overlap, architectural distance, and empirical regimes"
type: source-transcription
language: en
batch: "013"
source_pdf_pages: [618, 621]
book_pages: [582, 585]
---

# Chapter 16: transfer overlap, architectural distance, and empirical regimes

[Library index](../README.md) · [Source and transcription policy](../SOURCE.md)

[Previous source: PDF 617](0615-0617-ch16-expert-count-routing-and-coherence.md#pdf-0617).

<a id="pdf-0618"></a>
## PDF 618 · Book page 582

<a id="section-16-6-6"></a>
## 16.6.6 Algorithmic shape

The INCRT algorithm of INCRT I extends to the input-conditional regime with one structural modification: each growth event creates not a head within a fixed architecture but an *expert*, that is, a separate sub-architecture associated with a specific input subset. The routing function $r^*$ assigns each input to its $k$ most energetic experts, and the per-expert deflation proceeds independently within each expert's input slice.

The algorithm reduces to standard INCRT when $k=E$ (every input activates every expert, which is the dense limit) and to standard MoE training when $E$ is fixed by the practitioner and the deflation is not enforced. The single new free parameter is the activation budget $k$, which the practitioner chooses based on inference-cost constraints. The pool size $E$ is then determined by the theorem stated in this part from the empirical $\widehat{\bar\kappa}_A$.

**Summary.** Input-conditional task complexity, captured by the expected directional task complexity index $\bar\kappa_A$, determines the optimal MoE expert count through the same compressed-sensing formula that governs the static head count. The bound is tight up to a constant of two and is calibrated to within seven percent on the deployed MoE architectures of the table. The routing function is the bilateral gate applied input-conditionally; the orthogonality regime under which the bound is tight is the same condition that the deflation step of the algorithm enforces by construction.

<a id="section-16-7"></a>
## 16.7 Architectural transfer between related tasks

The two preceding sections allow the task operator to vary in time (INCRT I) or with the input (INCRT I). A third regime, of practical importance, lies between them: the operator changes once, abruptly, when the architecture is transferred from one task to another. A model pretrained on a broad distribution and then fine-tuned on a narrow specialisation faces this regime; so does an architecture trained on one modality and adapted to a related but distinct one.

The current practice is to keep the pretrained architecture unchanged during fine-tuning and adjust only the weights. This is sensible when the two tasks are sufficiently similar that the architecture sized for the first remains adequate for the second. It is wasteful when the two tasks differ enough that the second needs less capacity (a narrow fine-tuning task on a broadly pretrained model), or insufficient when it needs more (a specialised task that probes directions absent from the pretraining).

The framework gives a quantitative bound on the architectural distance between two tasks in terms of a single similarity measure: the spectral overlap between their task operators.

<a id="section-16-7-1"></a>
### 16.7.1 Spectral overlap as task similarity

<a id="definition-16-26"></a>
**Definition 16.26 (Spectral overlap).** For two task operators $\mathcal B_{\rm pre},\mathcal B_{\rm fine}$ with non-zero Frobenius norm, the *spectral overlap* is

<a id="equation-16-36"></a>
$$
\Omega(\mathcal B_{\rm pre},\mathcal B_{\rm fine})
=\frac{\langle\mathcal B_{\rm pre},\mathcal B_{\rm fine}\rangle_F}
{\|\mathcal B_{\rm pre}\|_F\cdot\|\mathcal B_{\rm fine}\|_F}\in[-1,1].
\tag{16.36}
$$

The overlap is unity when the two operators are aligned in the same direction, zero when they are orthogonal in the operator inner product, and minus one when they are anti-aligned. The intermediate values quantify partial alignment.

<a id="pdf-0619"></a>
## PDF 619 · Book page 583

The framework treats the transition between tasks as a continuous deformation along the linear path

<a id="equation-16-37"></a>
$$
\mathcal B^{(\alpha)}=(1-\alpha)\,\mathcal B_{\rm pre}+\alpha\,\mathcal B_{\rm fine},\qquad \alpha\in[0,1].
\tag{16.37}
$$

Along this path, the spectral structure changes monotonically from that of $\mathcal B_{\rm pre}$ at $\alpha=0$ to that of $\mathcal B_{\rm fine}$ at $\alpha=1$. The total Frobenius distance traversed is $\|\mathcal B_{\rm fine}-\mathcal B_{\rm pre}\|_F$, which is finite and therefore satisfies the summability condition of the theorem. The continuous fine-tuning regime is, in the language of the preceding sections, a single summable drift event.

<a id="section-16-7-2"></a>
### 16.7.2 The architectural distance bound

<a id="theorem-16-27"></a>
**Theorem 16.27 (Architectural distance).** Let $\mathcal B_{\rm pre},\mathcal B_{\rm fine}$ be two task operators with spectral overlap $\Omega$. Let $\kappa_A^{\max}$ denote the largest directional task complexity index along the interpolation path defined in this subsection, and let $K_{\rm pre}^*,K_{\rm fine}^*$ denote the optimal head counts of the theorem stated in this part for the two endpoints. Then

<a id="equation-16-38"></a>
$$
\big|K_{\rm fine}^*-K_{\rm pre}^*\big|
\le C\,(\kappa_A^{\max})^2\,(1-|\Omega|),
\tag{16.38}
$$

where $C$ depends only on the threshold pair $(\theta_w,\phi_g)$.

*Proof sketch.* The total Frobenius distance traversed along the path is

<a id="display-0619-transfer-distance-identity"></a>
$$
\begin{aligned}
\|\mathcal B_{\rm fine}-\mathcal B_{\rm pre}\|_F^2
&=\|\mathcal B_{\rm pre}\|_F^2+\|\mathcal B_{\rm fine}\|_F^2-2\langle\mathcal B_{\rm pre},\mathcal B_{\rm fine}\rangle_F\\
&=\|\mathcal B_{\rm pre}\|_F^2+\|\mathcal B_{\rm fine}\|_F^2-2\Omega\,\|\mathcal B_{\rm pre}\|_F\,\|\mathcal B_{\rm fine}\|_F.
\end{aligned}
$$

By the perturbation bound of the lemma stated above applied to the cumulative drift, the number of growth and pruning events along the path is bounded by $\|\mathcal B_{\rm fine}-\mathcal B_{\rm pre}\|_F^2/(\delta_0^2-\phi_g^2)$. The change in head count is bounded by this number of events. Substituting the expression above and using $\|\mathcal B\|_F\le\kappa_A^{\max}\theta_w$ at each endpoint yields the bound. $\square$

The bound has three regimes, each with a distinct architectural behaviour.

**High overlap.** When $|\Omega|\ge1-\theta_w/((\kappa_A^{\max})^2\Gamma_0)$, the bound implies $|K_{\rm fine}^*-K_{\rm pre}^*|<1$. The architecture is essentially unchanged between the two tasks; fine-tuning is purely a weight adjustment within the existing structure. This is the regime in which conventional fine-tuning practice — keeping the architecture fixed — is correct.

**Intermediate overlap.** For $|\Omega|$ in a band below the high-overlap threshold, the architecture changes by a small number of heads, with the change proportional to $1-|\Omega|$. A higher-complexity pretraining task admits a lower-complexity fine-tuned architecture (specialisation), and vice versa (recommissioning).

**Low overlap.** When $|\Omega|$ is small, the bound permits architectural changes of the order of $K_{\rm pre}^*$ itself. Effectively the architecture must be regrown. The cost of fine-tuning approaches that of training from scratch, and the practical recommendation is to train a fresh architecture rather than attempt transfer.

<a id="pdf-0620"></a>
## PDF 620 · Book page 584

<a id="section-16-7-3"></a>
### 16.7.3 Three regimes of architectural transfer

<a id="corollary-16-28"></a>
**Corollary 16.28 (Three regimes).** Define the critical overlap $\Omega_{\rm crit}=1-\theta_w/((\kappa_A^{\max})^2\Gamma_0)$. Three regimes follow from the theorem stated in this part:

1. (i) Adaptation $(|\Omega|>\Omega_{\rm crit})$: no architectural change is required; standard fine-tuning is correct.
2. (ii) Specialisation or extension $(\frac12\Omega_{\rm crit}<|\Omega|\le\Omega_{\rm crit})$: the architecture changes by at most $C(\kappa_A^{\max})^2(1-|\Omega|)$ heads, with growth if the fine-tuning task is broader and pruning if it is narrower.
3. (iii) Recommissioning $(|\Omega|\le\frac12\Omega_{\rm crit})$: the architecture is partially or wholly regrown.

*Proof.* The three regimes follow directly from the theorem stated in this part by partitioning the range of $|\Omega|$ at the points where the bound permits, respectively, zero events, fewer than $K_{\rm pre}^*/2$ events, and more. $\square$

The corollary connects to an empirical observation in the fine-tuning literature: the most effective fine-tuning regimes are those in which the source and target models lie in a “linearly connected region of parameter space” [citation]. The framework identifies this region with the high-overlap regime: when $\Omega$ is sufficiently close to unity, the path $\mathcal B^{(\alpha)}$ is short, the moving-target tracking applies without architectural events, and the parameter trajectory is linear because the architecture above it is fixed.

<a id="section-16-7-4"></a>
### 16.7.4 Continuous fine-tuning along the path

The linear interpolation path satisfies the summability condition of the theorem stated in this part: the cumulative drift is $\|\mathcal B_{\rm fine}-\mathcal B_{\rm pre}\|_F<\infty$. The moving-target convergence theorem therefore applies directly, and the architecture tracks the path within a ball of radius $R_\infty$ proportional to the drift.

This suggests a practical algorithm. Rather than switching abruptly from $\mathcal B_{\rm pre}$ to $\mathcal B_{\rm fine}$ at the start of fine-tuning, the practitioner interpolates the task operator along the linear path, increasing $\alpha$ slowly during fine-tuning while INCRT manages the architectural events along the way. The approach turns a single jump into a controlled deformation, spreading the architectural change over the fine-tuning horizon rather than concentrating it at the start.

<a id="theorem-16-29"></a>
**Theorem 16.29 (Bounded events under continuous transfer).** Under continuous interpolation along the interpolation path with rate $\dot\alpha$ small enough that $\dot\alpha\,\|\mathcal B_{\rm fine}-\mathcal B_{\rm pre}\|_F<\delta_0$, the total number of architectural events satisfies

<a id="equation-16-39"></a>
$$
N_{\rm events}\le\frac{\|\mathcal B_{\rm fine}-\mathcal B_{\rm pre}\|_F^2}{\delta_0^2-\phi_g^2},
\tag{16.39}
$$

where $\delta_0$ is the minimum-step energy and $\phi_g$ the pruning threshold.

*Proof.* The total drift along the path is $\|\mathcal B_{\rm fine}-\mathcal B_{\rm pre}\|_F$, summable by construction. By the Lyapunov decrease of the theorem stated in this part, each architectural event reduces the Lyapunov functional by at least $\delta_0^2-\phi_g^2$. The total available decrease is bounded by the squared drift, and the number of events is the ratio. $\square$

<a id="section-16-7-5"></a>
### 16.7.5 An empirical illustration

The three regimes can be illustrated on standard transfer tasks. For a $\mathrm{BERT}_{\rm Base}$ model pretrained on Wikipedia and fine-tuned on each of the GLUE benchmarks, the spectral

<a id="pdf-0621"></a>
## PDF 621 · Book page 585

overlap $\Omega$ between the pretraining task operator and each fine-tuning task operator can be estimated from the gradient covariance during the early phase of fine-tuning. the table reports the estimated overlaps and the predicted regime.

<a id="table-16-4"></a>
| GLUE task | $\widehat\Omega$ | Regime | $\bigl\lvert K_{\rm fine}^*-K_{\rm pre}^*\bigr\rvert_{\rm pred}$ | ratio to deployed |
|---|---:|---|---:|---:|
| SST-2 | 0.94 | adaptation | $\approx0$ | — |
| MRPC | 0.91 | adaptation | $\approx0$ | — |
| QQP | 0.85 | specialisation | $\sim8$ | 0.94 |
| QNLI | 0.83 | specialisation | $\sim11$ | 1.05 |
| MNLI | 0.78 | specialisation | $\sim18$ | 0.97 |
| CoLA | 0.62 | recommissioning | $\sim47$ | — |

**Table 16.4:** Spectral overlap of GLUE fine-tuning tasks with the $\mathrm{BERT}_{\rm Base}$ pretraining task operator (estimated from early-phase gradient covariance), with the predicted regime and the predicted architectural distance.

*The ratio column is reported only for the specialisation regime, where deployed fine-tuned models retain a measurable but small fraction of pruned heads. The recommissioning regime (CoLA) corresponds to the linguistically idiosyncratic acceptability judgement task, on which fine-tuning of $\mathrm{BERT}_{\rm Base}$ is known to under-perform; the framework predicts that a freshly grown architecture would do better.*

The agreement is again strong: the predicted architectural distances match the deployed fine-tuned models within seven percent in the specialisation regime, and the recommissioning regime correctly identifies the GLUE task on which standard fine-tuning is known to be problematic.

**Summary.** A single quantity, the spectral overlap $\Omega$ between source and target task operators, classifies fine-tuning into three regimes with distinct architectural behaviour: adaptation ($\Omega\approx1$, no architectural change), specialisation or extension ($\Omega$ in an intermediate band, modest change), and recommissioning ($\Omega$ small, substantial regrowth). The bound on the architectural distance is proportional to $1-|\Omega|$, with the constant given by the largest directional task complexity index along the interpolation path between the two operators. The continuous-transfer algorithm, in which $\alpha$ is increased slowly during fine-tuning while INCRT manages the architectural events, bounds the total number of events by a finite quantity determined by the squared Frobenius distance.

<a id="section-16-8"></a>
## 16.8 Adaptive optimisers and the preconditioned gate

The three preceding sections have extended the framework along three independent axes: time (INCRT I), input (INCRT I), and task (INCRT I). Each extension preserves an assumption that the static analysis makes implicitly and the dynamic analysis inherits without comment: the underlying optimiser is a stochastic-gradient method of the form $\theta_{t+1}=\theta_t-\eta\,g_t$, with constant or slowly-decaying step size and unmodified gradient.

This assumption is at variance with the practice of modern deep learning. Foundation models are trained almost exclusively with adaptive preconditioned methods: AdamW [citation], Adafactor [citation], Lion [citation], and the more recent Shampoo [citation] or Muon [citation] families. These methods replace the bare gradient $g_t$ by a preconditioned version $P_t^{-1}g_t$, where $P_t$ is a running estimate of second-moment information. The empirical effect is a different effective learning rate per parameter, sensitive to the local curvature.
