# uSync Migrations

uSync Migrations is a tool to help you migrate your site settings and content from Umbraco 7.x to the latest and greatest versions of Umbraco.

# How to use this

Runs on Umbraco 10 (and soon 11!)

## Getting Started

```
dotnet add uSync.Migrations --prerelease
```

0. **Don't do this on a live server!**
1. Install it. 
2. Put an Old uSync v7 folder somewhere in your shiny new usync Folder
3. Go to the Migrations tab in uSync. 
4. Do it .
5. Import it.

## Complex data ? 

This release covers core things, and a few edge cases. if you want to
migrate complex data you need to write a Migrator (implimenting ISyncMigrator class) take a look at the [uSync.Migrations/Migrators](uSync.Migrations/Migrators) folder.

## Support ?

This is a pre-release early access beta migration tool. you can ask, but lets hope other people are watching. 


