---
title: "Chapter 14: static design recipe, baseline comparison, and programming interface"
source_pdf_pages: [523, 526]
book_pages: [487, 490]
batch: "011"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: true
continues_after_file: true
---

# Chapter 14: static design recipe, baseline comparison, and programming interface

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous: PDF 518–522](0518-0522-ch14-static-geography-closure.md#pdf-0522) · [Next: PDF 527–529](0527-0529-ch14-decision-tables-and-procedures.md#pdf-0527)

> Editorial note: Full transcription of PDF 523–526 (printed pages 487–490), including all six recipe steps, unnumbered formulas, Tables 14.3–14.4, the complete baseline comparison, and the opening of §14.10. Allocation formulas and the description of a fixed static interface are retained literally; separate qualifications appear in notes 183–184.

<a id="pdf-0523"></a>
## PDF 523 · Book 487

<a id="section-14-8-1"></a>
#### 14.8.1 Step 1: estimate the layer geography

For each layer $\ell$, a preliminary model or an early checkpoint is used to estimate the empirical signature geography $\mu_\ell$. This estimate may come from attention maps, effective motors, activation responses, or a reduced diagnostic representation. The important point is consistency: the same diagnostic representation must be used when measuring coverage, crowding, and marginal value.

The estimate need not be perfect. A static rationalised block is not an oracle design. It is a disciplined response to an observed geometry. If the geometry later moves substantially, the static design may become insufficient; this is precisely the situation that motivates INCRT.

<a id="section-14-8-2"></a>
#### 14.8.2 Step 2: choose a dictionary resolution

A tolerance $\rho^\star$ is chosen for coverage. This tolerance fixes the granularity at which two signatures are considered distinct for architectural purposes. A very small tolerance produces too many atoms and may overfit the diagnostic. A very large tolerance hides rare directions. In practice, $\rho^\star$ should be selected by comparing diagnostic stability across checkpoints.

Given $\rho^\star$, a target dictionary size $K^\star$ is estimated. This value should not be interpreted as a sacred head count. It is a lower-bound guide for the number of effective representatives required by the layer.

<a id="section-14-8-3"></a>
#### 14.8.3 Step 3: allocate head slots

The layer receives a head budget $H_\ell$. A minimal rule is

<a id="display-0523-head-budget"></a>
$$
H_\ell=K^\star+H_{\mathrm{tail}}+H_{\mathrm{mix}},
$$

where $H_{\mathrm{tail}}$ reserves capacity for rare directional tails and $H_{\mathrm{mix}}$ reserves capacity for coupled operations. If the total parameter budget must remain fixed, these extra slots may be obtained by reducing redundant symmetric slots or by using smaller head dimensions.

The allocation is then divided into three groups:

<a id="display-0523-channel-split"></a>
$$
H_{S,\ell}+H_{A,\ell}+H_{M,\ell}=H_\ell.
$$

A simple initialisation is

<a id="display-0523-channel-initialisation"></a>
$$
H_{A,\ell}\approx\lceil\eta_\ell H_\ell\rceil,\qquad H_{S,\ell}\approx\lceil(1-\eta_\ell)H_\ell\rceil-H_{M,\ell},
$$

where $\eta_\ell$ is estimated from directional residual energy or from the fraction of dictionary mass assigned to oriented atoms.

<a id="section-14-8-4"></a>
#### 14.8.4 Step 4: initialise gates

Gates should initially be close to identity. If they are too selective at the beginning of training, the block may suppress heads before their role is known. A safe initialisation is

<a id="display-0523-initial-gates"></a>
$$
g_h(0)=1-\epsilon_g,
$$

with small $\epsilon_g$, followed by slow diagnostic modulation. After a warm-up period, the gate may incorporate marginal value and crowding:

<a id="display-0523-moving-estimate-gates"></a>
$$
g_h(t)=\sigma\left(b(t)+\beta_V\widehat V_{\mathrm{marg},h}(t)-\beta_C\widehat C_{\mathrm{orb},h}(t)\right).
$$

The hats indicate moving estimates. The moving estimates are essential because raw checkpoint values may be noisy.

<a id="pdf-0524"></a>
## PDF 524 · Book 488

<a id="section-14-8-5"></a>
#### 14.8.5 Step 5: allocate feed-forward width

The feed-forward sub-layer should receive width according to unresolved residual structure. A practical rule is to keep the global width close to the standard baseline while redistributing internal capacity between generic, symmetric, directional, and residual-completion components:

<a id="display-0524-feed-forward-width"></a>
$$
d_{\mathrm{ff}}^{\mathrm{rat}}=d_{\mathrm{gen}}+d_S+d_A+d_R.
$$

Here $d_{\mathrm{gen}}$ preserves a generic non-linear reservoir, $d_S$ supports reciprocal consolidation, $d_A$ supports directional transformations, and $d_R$ supports residual completion. If $\Gamma_{\mathrm{res}}^{(\ell)}$ is high and directional, $d_A$ and $d_R$ should be increased. If crowding is high and residual energy is low, compression is justified.

<a id="section-14-8-6"></a>
#### 14.8.6 Step 6: audit after training

After training, the rationalised block should be audited with the same diagnostics used in its design. The audit should answer five questions.

1. Did the layer cover the intended dictionary atoms?
2. Did the directional-tail heads remain active?
3. Did symmetric crowding decrease or become compressible?
4. Did marginality-aware pruning preserve performance better than random pruning?
5. Did residual directional energy decrease relative to the baseline?

A negative answer does not necessarily invalidate the theory. It may indicate that the diagnostic, distance, tolerance, or gate schedule was poorly chosen. The audit is therefore part of the architecture, not an optional interpretability exercise.

<a id="table-14-3"></a>

| Design step | Input | Output |
|---|---|---|
| Estimate geography | checkpoint signatures | empirical measure $\mu_\ell$ |
| Choose resolution | stability and tolerance | dictionary size proxy $K^\star$ |
| Allocate heads | $K^\star$, tails, mixed demand | $H_S,H_A,H_M$ |
| Initialise gates | marginality and crowding proxies | stable gate schedule |
| Allocate FF width | residual energy and effective ranks | $d_{\mathrm{gen}},d_S,d_A,d_R$ |
| Audit | trained model and interventions | coverage, crowding, marginality report |

**Table 14.3:** Static design recipe for a rationalised layer. The recipe is deliberately compatible with small experimental models.

**Takeaway.** *A rationalised block can be designed by a finite static recipe: estimate the geography, choose a resolution, allocate heads, initialise gates, allocate feed-forward width, and audit the trained layer. INCRT will replace this static recipe by a dynamic process.*

<a id="section-14-9"></a>
### 14.9 Comparison with the standard block

The rationalised block differs from the standard block in allocation, not in the basic residual philosophy. This distinction matters because it prevents two common misunderstandings. The first misunderstanding is to view rationalisation as a rejection of multi-head attention. The second is to view it as a minor regularisation trick. It is neither. It is a reorganisation of the same broad template around explicit geometric quantities.

<a id="pdf-0525"></a>
## PDF 525 · Book 489

<a id="section-14-9-1"></a>
#### 14.9.1 Same skeleton, different invariants

Both blocks have residual transport, attention mixing, non-linear completion, and normalisation. The difference is in the invariants that the architecture respects. The standard block respects implementation invariants: fixed head size, fixed number of heads, fixed expansion factor, and permutation-symmetric treatment of heads. The rationalised block respects diagnostic invariants: S–A balance, signature coverage, orbit diversity, marginal value, and residual energy.

This difference changes the interpretation of a layer. In the standard block, a layer is mostly described by its dimensional hyperparameters $(H,d_k,d_{\mathrm{ff}})$. In the rationalised block, the same layer is described by

<a id="display-0525-layer-coordinates"></a>
$$
\left(H_S,H_A,H_M,d_{\mathrm{gen}},d_S,d_A,d_R,\rho_\ell,C_{\mathrm{orb}}^{(\ell)},\Gamma_{\mathrm{res}}^{(\ell)}\right).
$$

The second description is longer, but it says why the capacity is there.

<a id="section-14-9-2"></a>
#### 14.9.2 Recovering the standard block

The standard block is recovered by imposing the following degeneracies:

<a id="display-0525-standard-degeneracies"></a>
$$
\mathcal R_S+\mathcal R_A+\mathcal R_M=\mathrm{MHA},\qquad G_{\mathrm{att}}=I,\qquad G_{\mathrm{ff}}=I,\qquad d_{\mathrm{ff}}^{\mathrm{rat}}=4d,
$$

and by ignoring head-level diagnostics during training. This observation is useful because it gives a clean ablation. If all rationalising mechanisms are disabled, the model should behave like the baseline. If performance changes before any diagnostic gate is enabled, the implementation has introduced an unintended difference.

<a id="section-14-9-3"></a>
#### 14.9.3 What should improve

The first expected improvement is not necessarily accuracy. Accuracy may improve, remain equal, or decrease depending on task and tuning. The primary expected improvements are structural:

1. lower unnecessary orbit crowding;
2. better preservation of rare directional heads;
3. more stable pruning decisions;
4. better relation between parameter count and marginal value;
5. clearer diagnosis of under-allocation.

These improvements matter because they make the architecture controllable. A standard block can be large and successful while still hiding redundancy. A rationalised block attempts to expose the redundancy and decide what to do with it.

<a id="section-14-9-4"></a>
#### 14.9.4 What should not be claimed

Several claims should be avoided.

- It should not be claimed that every task requires an antisymmetric channel. Some tasks are mostly reciprocal.
- It should not be claimed that every crowded region is bad. Some repeated operations are useful and robust.

<a id="pdf-0526"></a>
## PDF 526 · Book 490

- It should not be claimed that smaller is always better. Directional under-allocation may require expansion.
- It should not be claimed that a toy coverage experiment proves language-model superiority.
- It should not be claimed that the rationalised block is the unique possible architecture satisfying the design principles.

The conservative claim is stronger in the long run: when the previous diagnostics are accepted, the rationalised block is a natural static architecture that makes those diagnostics actionable.

<a id="section-14-9-5"></a>
#### 14.9.5 Baseline comparison table

<a id="table-14-4"></a>

| Aspect | Standard block | Rationalised block |
|---|---|---|
| Head treatment | uniform and permutation-symmetric | typed, gated, or diagnostically allocated |
| S–A structure | implicit inside heads | explicit channel accounting |
| Feed-forward role | fixed expansion reservoir | residual and geometry-aware completion |
| Crowding | detected only post hoc | monitored and penalised or compressed |
| Marginal value | not architectural | used for pruning, retention, and allocation |
| Growth preparation | absent | insufficiency diagnostics prepared for INCRT |
| Universality claim | standard expressivity | standard block recoverable as degenerate case |

**Table 14.4:** Conceptual comparison between the standard Transformer block and the rationalised Transformer block.

**Takeaway.** *The rationalised block keeps the Transformer skeleton but changes the invariants. Uniformity is replaced by diagnostic allocation; post-hoc interpretation is replaced by architectural monitoring.*

<a id="section-14-10"></a>
### 14.10 A static programming interface for Transformer design

The rationalised block is not only a theoretical bridge toward an incremental architecture. It also gives a practical interface for inspecting, repairing, pruning, and stabilising an already trained Transformer block. This interface is static because the number of layers, heads, channels, and feed-forward units is fixed during the procedure. It is nevertheless actionable: every quantity introduced in the previous chapters becomes either a diagnostic or an intervention variable.

The guiding loop is

<a id="display-0526-programming-loop"></a>
$$
\text{measure}\quad\longrightarrow\quad\text{diagnose}\quad\longrightarrow\quad\text{intervene}\quad\longrightarrow\quad\text{validate}.
$$

The loop is deliberately modest. It does not claim to find a globally optimal architecture. It gives a reproducible way to decide whether a head should be preserved, compressed, reallocated, or monitored. This is the sense in which the static part of the construction closes: signature geography, S–A decomposition, residual amplification, crowding, and marginal value are no longer only descriptive coordinates. They become programming operations.
