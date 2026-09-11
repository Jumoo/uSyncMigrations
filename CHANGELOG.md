# Changelog

Notable changes to uSync.Migrations. The package ships one release line per Umbraco major, so
versions track the Umbraco major they target.

## 17.0.3 - 2026-09-11

### Fixed

- `MacroRTEConfigSerializer.GetMigratedConfigurationAsync` threw
  `System.Text.Json.JsonException: The input does not contain any JSON tokens` when a
  datatype's `blocks` config serialized to an empty/whitespace string — as it does for
  `Umbraco.TinyMCE` datatypes and the stock `RichtextEditor.config`. This aborted every RTE
  datatype import and cascaded into dropped RTE properties on dependent content types.
  ([#330](https://github.com/Jumoo/uSyncMigrations/issues/330))

## 17.0.2 - 2026-08-25

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

[Unreleased]: https://github.com/Jumoo/uSyncMigrations/compare/v17.0.3...HEAD
