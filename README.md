
> [!CAUTION]
> This is a pre-release early access beta migration assistant. Migrations can be complicated you can ask, but support of Migrations is much more of a community effort so results may vary.

> [!IMPORTANT]
> We advice that you DO NOT install and run migrations on your 'production' or 'live' site, migrations are often messy and take muiltiple steps, we recommend you do all this on a test site
> away from live or critical infrastructure, and when you are happy with it, either use the resulting site's uSync files or database to make your new site. we cannot help if you put this
> on a live site and things break.

# uSync Migrations

uSync Migrations is a tool to help you migrate your site settings and content from Umbraco 7.x to the latest and greatest versions of Umbraco.

![](/assets/migrations-dashboard.png)

# How to use this

Runs on Umbraco 10, 11 & 12

## Getting Started


### SEE our [GETTING STARTED](GETTING-STARTED.md) guide for more details



Install uSync
```
dotnet add package uSync 
```
uSync.Migrations.

```
dotnet add package uSync.Migrations --prerelease
```

0. **Don't do this on a live server!**
1. Install it. 
2. Put an Old uSync v7 folder somewhere in your shiny new uSync Folder
3. Go to the Migrations tab in uSync. 
4. Do it .
5. Import it.

## Complex data ? 

This release covers core things, Vorto and a few community editors. 

if you want to migrate complex data you need to write a Migrator (implementing `ISyncMigrator` class) take a look at the [uSync.Migrations.Migrators](uSync.Migrations.Migrators) project.

If you want to see how you can customize the process. take a look at the [MyMigrations](MyMigrations) project in this repo

**And remember: contribute back any editors you think others will benefit from**

