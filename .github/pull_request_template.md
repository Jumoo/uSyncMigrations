## What does this change?

<!-- A sentence or two on the change and why it is needed. -->

## Notes for the reviewer

<!-- Anything non-obvious: behaviour changes, things you decided against, areas you want a
     second opinion on. Delete if there is nothing to say. -->

## Checklist

- [ ] `npm run build --prefix src/uSync.Migrations.Client/Migrations-Client` runs clean if the client changed
- [ ] `dotnet build src/uSync.Migrations.slnx -c Release` is clean
- [ ] `CHANGELOG.md` updated under **Unreleased**
- [ ] If a dependency changed, `dotnet restore src/uSync.Migrations.slnx --force-evaluate` was
      run and the updated `packages.lock.json` files are committed
- [ ] Migrator/backoffice changes were clicked through against `uSync.Migrations.Site`, not
      just built
