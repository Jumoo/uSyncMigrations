# uSync.Migrations - The v17 Experience

> [!NOTE]
> This is very much a work in progress. We are trying out some ideas to make migrations simpler in the v17 world.
> Feel free to nose about, but at the moment, there is ZERO support for any of this - you should use this only on local development sites, where you have backups of your data and don't mind starting again.

## Brave New Migrations

For v17 we thought we would have a go at making migrating a simpler experience - integrating it with the sync process
as much as we can, and making it happen without you really noticing.

## Config and Content Migration via Serializers

Firstly, uSync has some 'migration' points already built in. Whenever a datatype or a bit of content is imported,
uSync looks for ConfigSerializers and Value mappers to get the values that actually should be imported.

So for migrations we can use these points to update config and update content if we think we need to.
(uSync already does this for blockgrid/list without you even knowing, because the format changes slightly between v13, v15 & v17).

uSync Migrations adds some extras here. You can see them in the uSync.Migrations.Migrators project.

### Limitations

These migration points are cool because they are already there, but they are not as powerful as the migration points
in old school uSync.Migrations.

v13 migrations has a concept of a migration context, which contains all sorts of information about the migration, so any
bit of code anywhere along the migration can say 'what is this content type going to be called when we finish' or add
new datatypes, or split up property values or all sorts.

With the uSync 'migration' points you are separated from all that. You know about the datatype or content value you are
looking at, so this means we can't split or merge properties during install, or rename or add new datatypes or content types.

For 90% of migrations this is fine! For the other 10%, well we might have to do something else (for 5% of them anyway).

## Upgraders

This version of uSync.Migrations has the ability to add 'upgraders' to the 'update' process inside the migrate tab.
This effectively is something that sits in between the file copy of files from a legacy uSync folder to the new one.

At this point the upgrader gets the whole file (so the XML) for a datatype, doctype, content item, or anything - so it can
do a bit more.

### The Grid

The main point of putting this feature in is so we can manipulate grid elements before we do the upgrade.
The grid upgrader is quite basic just now, but with it - we can add new datatype files (for the grid.editors.config.js file)
and new content types (again for the config).

Doing this during the copy means when you then click import later on, hopefully the right datatypes and content types
are there waiting (the serializers and mappers will then be able to convert the grid content properties into blockgrids 🤞).

This gets us another 5% of migration cases.

## Other?

So what can't we do? Well, we still can't split/merge properties across content items, and we can't reach across
different items to do fancy things like rename/rationalize all the content types.

If this is you, then we would say use uSync.Migrations v13 edition, get your site all nice on v13 and then move it to v17.

It's still true that a 'modern' v13 site (e.g. no grid, or nested content, or other 'fancy' things) - will 'just' work if you
bring the uSync files to v17. The formats are all very close, and in fact v17 uSync is tested against these upgrades already.

uSync.Migrations for v17 is aiming to do the slightly more complicated things (like the grid) - while still being something
quite simple if you haven't done anything too clever...

## Repository layout

| Path | What it is |
| --- | --- |
| `src/uSync.Migrations` | The meta package - references the three below, this is what most sites install |
| `src/uSync.Migrations.Core` | Core migration types, context, and services |
| `src/uSync.Migrations.Client` | The backoffice client (the migrate tab, and its API) |
| `src/uSync.Migrations.Client/Migrations-Client` | The backoffice client's TypeScript/Lit/Vite source |
| `src/uSync.Migrations.Migrators` | Built-in migrators, including the grid upgrader |
| `src/uSync.Migrations.Migrators.Seven` | Migrators for content coming from a v7 site |
| `src/uSync.Migrations.Site` | A local Umbraco site for manual testing - not shipped |
| `dist/build-package.ps1` | Local packaging script, for a package you don't want to release |

## Building

Requires the .NET SDK pinned in [`global.json`](global.json) and Node 24.

The backoffice client has to be built first — it produces `wwwroot/App_Plugins`, which is
gitignored, so a fresh clone has no client assets (and `uSync.Migrations.Client` packs with
none) until you do:

```bash
npm ci --prefix src/uSync.Migrations.Client/Migrations-Client
npm run build --prefix src/uSync.Migrations.Client/Migrations-Client
```

Then the solution itself:

```bash
dotnet build src/uSync.Migrations.slnx -c Release
```

Shared build and package metadata lives in [`src/Directory.Build.props`](src/Directory.Build.props).
Restores are locked, so if you change a dependency you have to commit the regenerated lock file
alongside it:

```bash
dotnet restore src/uSync.Migrations.slnx --force-evaluate
```

## Releasing

Pushing a `v{version}` tag on `v17/main` publishes to NuGet:

```bash
git tag v17.3.1 && git push origin v17.3.1
```

The tag is the version — `v17.3.1` publishes `17.3.1`. The workflow refuses to run if the
tag isn't a valid version, or if the tagged commit isn't on a release branch.

Authentication is [trusted publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing) —
the job exchanges a GitHub OIDC token for a short-lived NuGet key, so there is no API key
stored in the repository. It depends on a policy on nuget.org naming this repository, the
`release.yml` workflow and the `nuget` environment; if any of those are renamed, the policy
has to be updated to match or publishing stops working.

Every push to `v17/main` also builds packages and uploads them as a build artifact, so a
release candidate can be tested without publishing anything.

## Contributing

Please read [SECURITY.md](SECURITY.md) before reporting anything security related, and
[CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) before taking part.

## Licence

[MPL-2.0](LICENSE).
