# HAVEN Cinematic Editorial Homepage Report

**Status: PARTIALLY COMPLETED**

## Repository and baseline

- Branch: `develop`
- HEAD before and after this work: `5fdad8cc5c3c53942ba18afd83b22260d669cf54` (unchanged)
- The repository already had 15 tracked modified files and four untracked entries when this task began. Those changes were retained; nothing was staged, committed, pushed, reset, cleaned, or checked out.
- Backups of the Home Razor view and stylesheet were made outside the repository before editing.
- Bootstrap is **5.3.3**, loaded once by the shared layout. Its bundled JavaScript was reused.

## Starting point

Home had one static hero image. Search was a GET form to `Properties/Index`, and the featured listings and property types came from the existing Home view model. Those routes, query names, data sources, listing cards, FAQ copy, and user journey content remain in place.

## Design and implementation

### Cinematic hero

- Replaced the single static hero with three distinct Bootstrap Carousel slides, each with a Vietnamese heading, short copy, and working existing search/listing links.
- Added Previous/Next controls, three line indicators, and a pause/resume control. Bootstrap provides fade, touch, keyboard, and hover-pause behavior; Home JavaScript starts the six-second cycle only when reduced motion is not requested.
- Added a gentle image zoom and staggered text entrance. Fixed hero geometry and `object-fit: cover` keep the image area stable between slides.
- The first image remains high priority. Other hero images use lazy loading. All new photography is stored locally as WebP; sizes are 449,712 bytes (villa), 121,044 bytes (interior), and 232,620 bytes (Da Nang architecture).

### Editorial sections and real data

- Added an architectural image and caption to the editorial story block.
- Turned the property type links into image-led category tiles. Category names and destination IDs still come from `Model.PropertyTypes`; imagery is illustrative and does not claim to be an actual listing.
- Featured property cards still render `Model.LatestListings`, including the existing empty state. No property listings, prices, counts, testimonials, or market statistics were invented.
- Preserved the current search form action, `GET` method, and query fields (`purpose`, `keyword`, `propertyTypeId`, `maximumPrice`).

### Images and usage terms

- First slide: existing project asset `wwwroot/image/haven-hero.jpg`; original source/license metadata was not found in the repository.
- Villa slide/category: Alef Morais, [Unsplash photo](https://unsplash.com/photos/modern-villa-with-a-swimming-pool-and-waterfall-w1BuW8zMNpw), published under the Unsplash License.
- Interior slide/category: Bailey Alexander, [Unsplash photo](https://unsplash.com/photos/a-living-room-filled-with-furniture-and-a-fire-place-pkIJXMezi_E), published under the Unsplash License.
- Editorial/category architecture: qui nguyen, [Unsplash photo](https://unsplash.com/photos/modern-building-with-unique-white-lattice-facade-and-balconies-JUE5rixe8nM), published under the Unsplash License. A photographer caption is shown with the image.
- Unsplash identifies the selected photos as free to use under its license. Asset files are local, so Home does not request these photos from a third-party host at runtime.

## Motion, responsiveness, accessibility, performance

- CSS uses a fixed hero region with responsive height, mobile-specific focal positioning, smaller headings, reachable CTA/control sizes, and a single-column editorial image/story arrangement below 640px.
- Bootstrap Carousel controls expose labels, indicators expose slide labels, inactive slides become inert/hidden from assistive technology, and visible keyboard focus is styled.
- The pause control can stop/resume autoplay. Focus within the carousel pauses it and leaving focus resumes unless the visitor paused it. `prefers-reduced-motion: reduce` disables autoplay and entrance/zoom motion; manual slide controls remain available.
- The image failure handler removes a broken image while preserving the slide copy and its dark fallback surface.
- Viewport CSS was reviewed for desktop/tablet/mobile breakpoints, but it was not accepted as visual proof. No horizontal-overflow, layout-shift, contrast, touch/swipe, or rendered image checks are marked passed.

## Files changed for this task

- `src/Haven.Web/Views/Home/Index.cshtml`
- `src/Haven.Web/wwwroot/css/home.css`
- `src/Haven.Web/wwwroot/js/home.js`
- `src/Haven.Web/wwwroot/image/home/home-villa-pool.webp`
- `src/Haven.Web/wwwroot/image/home/home-interior.webp`
- `src/Haven.Web/wwwroot/image/home/home-vietnam-architecture.webp`
- `docs/qa/HAVEN_CINEMATIC_EDITORIAL_HOMEPAGE_REPORT.md`

No controllers, view models, APIs, database objects, migrations, authentication, or Admin/User pages were edited for this task.

## Verification

- `node --check src/Haven.Web/wwwroot/js/home.js`: **PASS**
- `dotnet build HAVEN.slnx --no-restore` (outputs directed to a temporary directory): **PASS**, 0 warnings, 0 errors
- Unit tests: **PASS**, 74/74. A first run from temporary output had one repository-path lookup failure; rerunning from the normal project output passed all 74.
- Integration tests: **PASS**, 76/76
- `git diff --check`: **PASS**; Git emitted only existing working-tree LF/CRLF normalization notices.

## Browser QA and remaining blocker

Chrome was opened on the existing local site at `http://localhost:5104` and the page was refreshed. It continued to render the old Home copy and static hero rather than the current source, so the live process did not reflect this change. The cause was not established. The existing website process was left running; it was not restarted. No screenshot of the new implementation could be captured, and no viewport or carousel interaction is marked visually verified. In particular, three-slide transition, autoplay/pause, mobile swipe, search submission, and rendered desktop/mobile composition remain unverified in a browser.

To complete acceptance, run the updated application version in the existing authorized local workflow, then visually check the Home at the requested desktop/tablet/mobile sizes and exercise the carousel and search controls. Until then, this work remains **PARTIALLY COMPLETED**.
