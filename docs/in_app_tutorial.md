# Lexting in-app tutorial (pinch / grab-first)

Replaces pop-gun flow on `TextOutputCanvas`. **One label** per step: copy **alpha off → replace text → alpha on** (~17 transitions). Tutorial prompt, **two-zone widget**, and **output field** are **visible together** from Step 3 onward (not the old “tutorial OR widget” split).

**Floating menu** = mid-air **HighlightableZoneController** mode picker (typing tutorial / speed test / freestyle). **Not shown** during this tutorial; user goes straight to live widget + output after Step 3. Skip / Don’t-show buttons are **separate 3D nodes** under the widget (see Step 3+).

**Design intent:** Selection = **index–thumb pinch** / **grab–release** (same as live typing). Demo hands: `TutorialProceduralHandPose` (open ↔ closed references).

---

## Master step table

| Step | Title (short) | Advance when | Wrong input / revert | Demo hands & chrome |
|------|-----------------|--------------|----------------------|---------------------|
| **1** | Ready position | Both palms facing each other: `dot(n_L, n_R) ≤ -0.7` held **2 s**; **reset timer** if tracking/pose lost | — | Both **open** |
| **2** | Left hand grab | After **2 s** from step start, play **left closed** demo; advance when user **matches left grab** (live `Insert`/grab) | — | Left **closed** anim; **left checkmark** on success |
| **3** | Right hand grab | **2 s** after step start, play **right closed** demo; advance on **right grab**; **summon widget + output**; instantiate **Skip** + **Don’t show again** (side-by-side, widget node styling, vertical margin below widget) | — | Right **closed**; **right checkmark** only; then **fade hands + checkmarks to 0** |
| **4** | Type `t` | `CurrentOutputValue == "t"` | Else **red ~0.25 s**, restore **step-start output** (`""`) | Hands/checkmarks **hidden** |
| **5** | Type `h` | `CurrentOutputValue == "th"` | Revert to **`"t"`** (step 5 start) | — |
| **6** | Type `e` | User reaches **`"the"`**; then **2 s** → clear to **`""`** → **1 s** → set **`"fo"`** (scripted); **do not enter Step 7** until `"the"` validated | On wrong char: red + restore **`"th"`** | — |
| **7** | Type `x` | `CurrentOutputValue == "fox"` | Revert to **`"fo"`** (step 7 start) | — |
| **8** | Back-Word | **`"fox"`** present at step entry; after step, output **empty** via Back-Word (gesture = existing **right middle → left wrist** mapping; **output-only** validation) | Any change other than clearing **fox** → reset to **`"fox"`** | — |
| **9** | Word Mode intro | **7 s** auto-advance | — | — |
| **10** | WM: start `the` | **L** holding **t**, **R** holding **h** (word-mode hold detection) | On release before valid: reset step; wrong pairing: **red + revert on release only** | — |
| **11** | WM: autofill `the` | Both release → word completes as **`the`** | Wrong on release: red + revert to step 10 start state | — |
| **12** | WM: start `tantalize` | **L** **t**, **R** **a** held | Wrong on release: red + revert; if **`ta`** correct, **may advance while still holding** | — |
| **13** | WM: `tantalize` + N | **L** dragged to **N** while word-mode hold continues (assembly toward **`tantalize`**) | Early **release** during await → reset to **step 12**; wrong letter **while still holding**: no flash (drag-through); wrong on **release**: red + revert | — |
| **14** | WM: `tantalize` + T | **R** dragged to **T** while hold continues | Same hold/release rules as 13 | — |
| **15** | WM: finalization entry | **R** still holding, **L** opens → **finalization mode** for current word stem | Early release rules as above | — |
| **16** | Finalization | User **commits** any **accepted conjugated form of `tantalize`** (dictionary); **punctuation optional** | Wrong word on commit: red + **reset to step 12** (tantalize block) | — |
| **17** | Wrap-up | **7 s** on copy, then **freestyle**: dismiss tutorial UI; fire **`OnTutorialCompleted`**; remove Skip/Don’t-show nodes; show **floating menu** if desired for mode pick | — | — |

**Dictionary:** Target word **`tantalize`** (and verb forms **`tantalizes`**, **`tantalizing`**, **`tantalized`**) is in `Lexting_Unity_Plugin/Runtime/Scripts/EnglishDictionaries/Resources/longer_words_2x2x2.json`, loaded by `LextingDictionary` for finalization. Partial stems during assembly (e.g. **`ta`**, **`tan`**, **`tant`**) are **in-progress UI state**, not standalone dictionary entries.

**Skip / Don’t show again:** **Independent** side-by-side controls. **Skip** → end tutorial **this session only** (no PlayerPrefs). **Don’t show again** → end tutorial + **never auto-show** on future sessions. Choosing one **does not** set or disable the other.

---

## Step copy (normalized 1–17)

### Step 1 — Ready position

**Title:** Lexting tutorial  

**Body:** Hold your hands in front of you, palms facing each other, fingers **open and relaxed**.

---

### Step 2 — Left hand grab

**Body:** **Great!** Now close the left hand like this.

---

### Step 3 — Right hand grab

**Body:** **Great!** Now the right hand.

---

### Step 4 — Type the letter t

**Body:** Locate the letter **T** with either hand. Hover your cursor into it and **grab–release** to type it.

---

### Step 5 — Type the letter h

**Body:** **Great!** Now **supinate** your wrist (turn palm upward). The letters changed. Find **H** and grab it.

---

### Step 6 — Type the letter e

**Body:** **Great!** Now **pronate** your wrist (turn palm downward). The letters changed again. Find **E** and grab it.

---

### Step 7 — Type the letter x

**Body:** **Great!** You typed a word! Now finish the word **FOX**. Turn your palm **forward** toward the screen. Find **X** and grab it.

---

### Step 8 — Back-Word

**Body:** **Great!** Now delete the word **fox** by tapping your **left wrist** with your **right middle finger**.

---

### Step 9 — Word Mode intro

**Body:** **Great!** You’ve learned the whole alphabet. Typing one letter at a time is slow — let’s go faster with **Word Mode**!

---

### Step 10 — Word Mode: `the` (start)

**Body:** Word Mode is **GRAB + HOLD + DRAG** with both hands. GRAB+HOLD **T** with the left and **H** with the right.**

---

### Step 11 — Word Mode: `the` (autofill)

**Body:** Now **let go** with both hands — Lexting will autofill the word.

---

### Step 12 — Word Mode: `tantalize` (start)

**Body:** **Great!** Try a longer word: **tantalize**. Grab **T** with the left and **A** with the right. **Hold both.**

---

### Step 13 — Word Mode: `tantalize` (continue)

**Body:** **Great!** Keep holding. Now drag your left hand to **N**.

---

### Step 14 — Word Mode: `tantalize` (continue)

**Body:** **Great!** Now drag your right hand to **T**.

---

### Step 15 — Word Mode: open left (finalization)

**Body:** **Great!** Keep holding with the **right**, but **open the left** hand to enter finalization.

---

### Step 16 — Finalization

**Body:** **Great!** You’re in **Finalization Mode**. Pick the word **tantalize** on the left. Rotate your wrists to explore your suffix choices. Choose any suffix and punctuation, then release.

---

### Step 17 — Wrap-up

**Body:** **Amazing!** You’re ready to freestyle. Think of a sentence and type it with Word Mode.

---

## Engineering notes

- **UI pages:** 17 step indices; single shared label; **`TutorialManager`** master-fades **`TutorialGroup` CanvasGroup** to `tutorialOpacity` (default 0.4) between steps; **`TutorialPageHandler`** swaps copy only.
- **Output gating:** Match **`CurrentOutputValue`** exactly per step until freestyle; **no blocking** of widget mechanics — only **red flash ~0.25 s** + **restore step-start output** on error (including Back-Word that removes required prefix).
- **Rotation steps 5–7:** No extra rotation detector; user must use real supinate/pronate/forward to reach letters.
- **Word Mode 10–16:** Validate **progressive** assembly per step table; advance **while still holding** when the step’s letter pairing is correct; **wrong only on release** (except early release → reset to step 12 for the `tantalize` block); **don’t** flash on wrong letter **during** hold (drag-through).
- **Finalization:** **One** interactive step (16); validate **any conjugated form of `tantalize`**, ignore punctuation requirement.
- **Hands:** 1 open both → 2 left closed → 3 right closed → **4+ fade out** hands/checkmarks when widget live.
- **Prefab/scene:** `TutorialHand_Left` / `TutorialHand_Right` with `TutorialProceduralHandPose` + open/closed ref children; **`TutorialPageHandler` on existing `Pages`** drives **`step1`** body text + canvas **`Title`** (step2–4 hidden); place Skip / Don't-show nodes anywhere under `TextOutputCanvas` with `TutorialDismissControls` — at step 3 **`TutorialManager`** reparents them under the live two-zone widget and **`TutorialSkipControlsLayout`** keeps them **`skipControlsMarginBelowWidget`** under the widget bounds, resyncing on **`ControllerActivated`** and **`TwoZoneCubePositionController.OnWidgetRepositioned`** (after calibrate).
- **Code:** `TutorialManager` (17-step state machine), `TutorialStepContent` (copy), pop-gun disabled while tutorial is incomplete (`OpenXRHandInput`).
