# HAVEN README Audit Report

## Summary

- Branch before documentation edits: `develop`
- HEAD before documentation edits: `5fdad8cc5c3c53942ba18afd83b22260d669cf54`
- README-like files found: 2
- README files updated: 1 (`README.md` at repository root)
- Nested planning README retained: `docs/HAVEN_phan_cong_5_thanh_vien/README_HUONG_DAN_CHUNG.md`; it describes the proposed five-member assignment and is not a current product/setup guide.
- No application source, project configuration, database, or migration was changed or run.

## Audit and changes

The root README was rewritten as a concise Vietnamese onboarding and project overview. It now describes the current MVC/Razor application, the solution layers, user and administrator capabilities, environment configuration, local SQL Server options, migrations, launch profiles, tests, media storage, security notes, and links to existing project documentation.

Outdated or unnecessarily detailed material was removed or corrected, including the obsolete latest-migration reference, repeated endpoint-by-endpoint API detail already maintained by the API contract, and implementation-status statements that were too dated for the current source. Current avatar and public-member-profile behavior was added where confirmed in code.

The planning README was left unchanged because its scope is team assignment/planning rather than general installation or the current feature inventory.

## Source facts checked

- Solution: `HAVEN.slnx`; projects include `Haven.Web`, `Haven.Application`, `Haven.Domain`, `Haven.Infrastructure`, `Haven.UnitTests`, and `Haven.IntegrationTests`.
- Target framework: `net10.0`; `global.json` selects SDK `10.0.401` with `latestFeature` roll-forward.
- Web stack: ASP.NET Core MVC/Razor, ASP.NET Core Identity, EF Core, and SQL Server. Bootstrap assets identify version 5.3.3. No `package.json` or Node-based frontend build is present.
- `ApplicationDbContext` and EF migrations are in `Haven.Infrastructure`; `Haven.Web` is the startup project.
- Latest source migration is `20261119170000_AddUserAvatar`, adding nullable `AvatarPath` to `AspNetUsers`.
- Development launch profiles use `http://localhost:5104` and `https://localhost:7081` (the HTTPS profile also exposes HTTP on 5104). OpenAPI and Scalar are Development-only.
- Avatar upload code accepts JPEG, PNG, and WebP, validates content, generates server-side filenames, limits files to 5 MiB, and stores them under `wwwroot/uploads/avatars`.
- Public member route is `/members/{id}` and returns public profile data with approved listings paginated; it does not expose the email or raw Identity entity.
- Integration tests use `WebApplicationFactory` and EF Core InMemory; they do not prove SQL Server migration behavior.
- The README's Docker option is based on the repository Compose and `.env.example` configuration. The Development app settings use local SQL Express with Windows authentication by default. The Web application reads `ConnectionStrings:HavenDb`; Compose `.env` values are not automatically consumed by the Web process.

## Command and link validation

Commands and paths in the README were checked against the solution, project files, `global.json`, launch settings, Compose configuration, and test projects. The locally available tool versions were read as:

- `dotnet --version`: `10.0.401`
- `dotnet ef --version`: `10.0.13`

The documented restore/build/test and EF commands were validated against existing project paths and context names, but a build, test run, database connection, or migration was not performed during this documentation-only task. The migration command is therefore documented, not claimed as executed.

Internal Markdown links across both README-like files: 10 checked, 0 broken. External links were not fetched or independently verified.

## Remaining limitations and out-of-scope findings

- SQL Server availability and credentials depend on each developer's local environment; no database was contacted.
- Tests and application startup were not run because this task was limited to documentation and made no source/configuration changes.
- The repository uses local media storage by default; deployments with multiple instances need persistent shared storage or an equivalent operational arrangement, noted in the README.
- No source/configuration change was made to address any issue that might require code changes to make a documented command work.

## Git preservation

The working tree already contained numerous modified and untracked application, test, asset, and QA files before this README task. Those changes were left intact. The README edit is the only tracked-file change made for this task; this report is a new documentation file under the already-untracked `docs/qa/` area. No commit, push, merge, reset, clean, checkout, or pull request was performed.
