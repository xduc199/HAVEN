# HAVEN Visual Defect Investigation

**Status: PARTIALLY COMPLETED — confirmed defects were corrected in source; post-change browser rendering remains unverified.**

## Repository safety

- Branch: `develop`; HEAD unchanged at `5fdad8cc5c3c53942ba18afd83b22260d669cf54`.
- Before this investigation, the tree already contained 10 modified tracked files, plus untracked QA reports and `haven-tokens.css`. Those existing changes were preserved. There were no staged changes.
- This investigation changed `src/Haven.Web/wwwroot/css/home.css` and this report only. No reset, clean, checkout, commit, or push was performed.

## Screenshot findings and source causes

The seven supplied captures show the running Home page at `localhost:5104`. The capture with two property cards is the Home featured-listing section, not a capture of the dedicated `/Properties` listing page.

| Defect visible in supplied captures | Source cause | Change made |
|---|---|---|
| Two Home cards had visibly different widths; the first image was nearly square and much taller; most of the right side remained empty. | `home.css` explicitly used three unequal tracks (`1.2fr`, `.8fr`, `.8fr`), made the first card span two rows, and gave it a separate `.95/1` image ratio. With only two listings, the third track stayed empty. The controller can return up to six, but the screenshot data contains two. | Replaced the asymmetric mosaic with equal auto-fit tracks (20rem minimum on desktop and tablet), removed the first-card row span and special ratio, and set one shared 16:10 image ratio. Cards stretch uniformly; their action row is pushed to the bottom. The existing one-column mobile rule remains. |
| Several Home serif headings looked cramped, with Vietnamese accents appearing visually detached or awkwardly spaced. | Home headings bypassed the shared display-font token and directly selected Georgia. Several used strong negative tracking (`-.035em` to `-.05em`). The Razor strings inspected use precomposed Vietnamese characters; no combining-mark sequences were present in Home text. The screenshot and source support a font/tracking inconsistency, but do not establish which web font the browser actually loaded. | Standardized Home display text to `--haven-font-display` and removed the aggressive negative tracking. No new font was added. Actual loaded-font/network status still requires browser inspection. |
| Large gaps weakened the relationship between some successive Home sections. | Desktop section margins were 6–7rem; the search area alone used 6.5rem below it. | Reduced the corresponding desktop gaps to 5–5.5rem while retaining the existing compact mobile spacing and editorial structure. No section or content was removed. |
| FAQ used the native black disclosure triangle seen in the capture. | `details/summary` markup had no marker reset or custom indicator. | Removed the native marker and added a gold plus/minus indicator that reflects the open state. The native `<details>` interaction and all FAQ content remain. |
| A small floating robot labeled `NULL` appeared at the right edge. | No such widget, text, or component exists in the app Razor views, CSS, or application JavaScript searched (excluding vendor libraries). It is absent from app source. | Nothing was removed. The capture is consistent with a browser-injected overlay, but its exact origin cannot be identified from the screenshot or repository alone. |

The hero and search placement in the captures were not changed. The footer's source already uses light text on the charcoal surface; the supplied screenshot shows the same contrast treatment. No footer source change was needed for the defects addressed here.

## Files changed

- `src/Haven.Web/wwwroot/css/home.css` — Home card grid/image dimensions, shared display typography and tracking, section spacing, and FAQ indicator.
- `docs/qa/HAVEN_VISUAL_DEFECT_REPORT.md` — this evidence and results report.

## Validation

- Before screenshots: supplied by the user on 2026-11-19; they show Home at multiple scroll positions and the two-card featured section.
- After screenshots and browser render: not available in this pass. No viewport is marked visually PASS after the changes.
- Dedicated Property Listing, Property Detail, Login/Register, account pages, and admin pages: not shown in the supplied captures and not visually inspected in this pass.
- Browser font/network details and exact origin of `NULL`: unverified.
- Build: PASS, `dotnet build HAVEN.slnx --no-restore`, 0 warnings and 0 errors. Because the running app PID 21288 locked output files, the successful build used a temporary output directory and left the server running. The first default-output build attempt failed only on those file-copy locks.
- Unit tests: PASS, 74/74 using the repository output directory. An initial attempt to run those tests from the temporary build directory produced one environment-dependent architecture-test failure because that test locates the repository relative to its assembly; rerunning from the normal repository output passed all 74.
- Integration tests: PASS, 75/75 from the isolated temporary output.
- `git diff --check`: PASS.

## Remaining work

The installed Windows browser-control runtime could not initialize (`failed to write kernel assets`), so it was not used to reload the site. Next, reload Home in the permitted browser and capture the same desktop/tablet/mobile viewports, including the card grid, headings, FAQ open/closed states, and footer. Inspect the dedicated listing separately. The final completion status depends on that render check; source changes alone do not establish visual acceptance.
