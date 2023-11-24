<#
SYNOPSIS
    Buils the optionally pushes the uSync.Migrations package.
#>
param (

    [Parameter(Mandatory)]
    [string]
    [Alias("v")]  $version, #version to build

    [Parameter()]
    [string]
    $suffix, # optional suffix to append to version (for pre-releases)

    [Parameter()]
    [string]
    $env = 'release', #build environment to use when packing

    [Parameter()]
    [switch]
    $push=$false #push to devops nightly feed (You have to be authorized for this to work)
)

if ($version.IndexOf('-') -ne -1) {
    Write-Host "Version shouldn't contain a - (remember version and suffix are seperate)"
    exit
}

$fullVersion = $version;

if (![string]::IsNullOrWhiteSpace($suffix)) {
   $fullVersion = -join($version, '-', $suffix)
}

$majorFolder = $version.Substring(0, $version.LastIndexOf('.'))

$outFolder = ".\$majorFolder\$version\$fullVersion"
if (![string]::IsNullOrWhiteSpace($suffix)) {
    $suffixFolder = $suffix;
    if ($suffix.IndexOf('.') -ne -1) {
        $suffixFolder = $suffix.substring(0, $suffix.indexOf('.'))
    }
    $outFolder = ".\$majorFolder\$version\$version-$suffixFolder\$fullVersion"
}

$buildParams = "ContinuousIntegrationBuild=true,version=$fullVersion"

"----------------------------------"
Write-Host "Version  :" $fullVersion
Write-Host "Config   :" $env
Write-Host "Folder   :" $outFolder
"----------------------------------"; ""

""; "##### Restoring project"; "--------------------------------"; ""
dotnet restore ..

""; "##### Building project"; "--------------------------------"; ""
dotnet build ..\uSyncMigrations.sln -c $env /p:$buildParams

""; "##### Packaging"; "----------------------------------" ; ""

dotnet pack ..\uSync.Migrations.Core\uSync.Migrations.Core.csproj --no-restore --no-build -c $env -o $outFolder -p:$buildParams
dotnet pack ..\uSync.Migrations.Client\uSync.Migrations.Client.csproj --no-restore --no-build -c $env -o $outFolder -p:$buildParams
dotnet pack ..\uSync.Migrations.Migrators\uSync.Migrations.Migrators.csproj --no-restore --no-build -c $env -o $outFolder -p:$buildParams
dotnet pack ..\uSync.Migrations\uSync.Migrations.csproj --no-restore --no-build -c $env -o $outFolder -p:$buildParams

## will copy to c:\source\localgit (assuming it exists)

""; "##### Copying to LocalGit folder"; "----------------------------------" ; ""
XCOPY "$outFolder\*.nupkg" "C:\Source\localgit" /Q /Y 

## only works if you have authorization to actuall push to the nightly repo.
if ($push) {
    ""; "##### Pushing to our nighly package feed"; "----------------------------------" ; ""
    .\nuget.exe push "$outFolder\*.nupkg" -ApiKey AzureDevOps -src https://pkgs.dev.azure.com/jumoo/Public/_packaging/nightly/nuget/v3/index.json
    
    Remove-Item ".\last-push-*" 
    Out-File -FilePath ".\last-push-$fullVersion.txt" -InputObject $fullVersion
}

Write-Host "uSync.Migrations Packaged : $fullVersion"

Remove-Item ".\last-build-*" 
Out-File -FilePath ".\last-build-$fullVersion.txt" -InputObject $fullVersion