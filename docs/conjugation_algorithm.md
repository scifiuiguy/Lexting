# Word mode and finalization mode

Designer-facing workflow for the dual-volume Lexting typing UI. **Word mode** (also called **Lexting mode**) builds a letter prefix; **finalization mode** picks the dictionary root, conjugation, and punctuation, then commits the word.

---

## Word mode (Lexting mode)

### Entering word mode

Word mode starts when **both hands begin a soft hold** (grab), **and** the first two chosen letters produce dictionary candidates.

If they do not—for example `"XX"`—word mode **does not** start.

### Letters 1 and 2

When word mode begins, tentative word output appears **above the typing volume(s)**. Starting word mode means the user has already fixed the **first and second** characters of the intended word (one letter from each hand’s initial grab).

### Letters 3 and 4 (optional)

By **moving or rotating** either hand, the user specifies the **third and/or fourth** characters.

The user may enter finalization with **fewer than four** letters. In that case, finalization shows a **subset** of matches (typically the **first eight** results), not the full result set.

### Leaving word mode for finalization

At any time during word mode, release **one** soft hold while **keeping the other**. Either hand may release first.

### Popular four-letter roots (six letters required)

If the first four letters match a **popular root**, the UI **cannot** enter finalization yet. It stays in word mode: alphabetic **nodes** still populate the volume so the user knows more specificity is required.

Popular roots in the prototype dictionary include:

`comp`, `cons`, `cont`, `inte`, `port`, `tran`

In this case the user hovers to **two additional characters** and **soft-taps** with an open hand to select them. Finalization can begin after that.

---

## Finalization mode

### When finalization can start

Finalization begins once the UI has **eight or fewer** matching roots for the current letter cluster.

- In **most** cases, **four letters** are enough to reach ≤8 results.
- For **popular roots**, **six letters** may be required (see above).

Results are shown in **alphabetical order**.

### Choose the root word (left volume)

All root words that match the letter cluster appear in the **left-hand typing volume** (2×2×2 **nodes**).

Hover the desired root **node**.

### Conjugation (two-hand wrist orientation)

While hovering a root, the user may set **conjugation** by orienting both wrists. See the [Conjugation section in the README](../README.md#conjugation) and `LextingConjugationState` in code for the orientation map.

Some conjugation groups contain **multiple** words—for example *activator* and *activation* under the same noun-singular orientation. **Soft-tap** with an open hand to **cycle** within the group. **Paging dots** under the word output show how many options exist in the group.

### Punctuation (right volume)

During finalization, the **right-hand typing volume** shows punctuation **nodes**. Wrist rotation labels (**relax / pronate / supinate**) do not change the punctuation layout—all three orientations use the same mapping:

| Volume region | Characters |
|---------------|------------|
| Top back | `?` `!` |
| Top front | Space · no-space |
| Bottom front | `.` `,` |
| Bottom back | `:` `;` |
| Center | `"` |

Hover the desired punctuation **node** with the **right hand**.

### Commit

Release the **remaining** soft hold to submit the fully **conjugated and punctuated** word.
