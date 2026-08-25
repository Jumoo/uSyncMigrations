# Changelog

Notable changes to uSync.Migrations. The package ships one release line per Umbraco major, so
versions track the Umbraco major they target.

## Unreleased

### Fixed

- CI never ran the backoffice client's npm build before packing `uSync.Migrations.Client`,
  so `17.0.1` shipped with no backoffice assets — and the client's `Migrations-Client`
  source folder (`package.json`, `tsconfig.json`, `package-lock.json`) was being swept
  into the package as loose content files instead. Workflows now build the client first,
  and `Migrations-Client` is excluded from the csproj's default item globs.

### Added

- Repository standards: `README.md`, `CHANGELOG.md`, `SECURITY.md`, `CODE_OF_CONDUCT.md`,
  `.editorconfig`, `.gitattributes`, `global.json`, `Directory.Build.props`, `GitVersion.yml`,
  dependabot, and a pull request template.
- CI workflows — PR build, package build, release, and CodeQL.
- Releases publish to NuGet from a `v{version}` tag pushed on a release branch, authenticated
  with trusted publishing (OIDC) rather than a stored API key.

## 17.0.0 - 2026-04-14

- Umbraco 17 release.

[Unreleased]: https://github.com/Jumoo/uSyncMigrations/compare/v17.0.0...HEAD
