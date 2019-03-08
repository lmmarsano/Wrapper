// open System.Diagnostics
open System
// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r "paket:
framework netstandard2.0
nuget Fake.DotNet
nuget Fake.DotNet.Cli
nuget Fake.IO.FileSystem
nuget Fake.Core.Target"
#load ".fake/build.fsx/intellisense.fsx"
open Fake.Core
open Fake.DotNet
open Fake.IO
// open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators

// --------------------------------------------------------------------------------------
// Build variables
// --------------------------------------------------------------------------------------

let buildDir = "./build/"
let appReferences = !! "./**/*.fsproj"
//let dotnetcliVersion = "2.0.2"
let dotnetcliVersion = DotNet.CliVersion.Latest
let mutable dotnetExePath = "dotnet"

// --------------------------------------------------------------------------------------
// Targets
// --------------------------------------------------------------------------------------

Target.create "Clean" (fun _ ->
    Shell.cleanDir buildDir
)

Target.create "InstallDotNetCLI" (fun _ ->
    dotnetExePath <- (
        DotNet.install ( fun options ->
            { options with Version = dotnetcliVersion }
        ) <| DotNet.Options.Create ()
    ).DotNetCliPath
)

Target.create "Restore" (fun _ ->
    appReferences
    |> Seq.iter (DotNet.restore id)
)

Target.create "Build" (fun _ ->
    appReferences
    |> Seq.iter (DotNet.build id)
)

Target.create "Publish" (fun _ ->
    appReferences
    |> Seq.iter (DotNet.publish (fun options ->
        {options with Configuration = DotNet.BuildConfiguration.Release })
    )
)

Target.create "Pack" (fun _ ->
    appReferences
    |> Seq.iter (DotNet.pack (fun options ->
        {options with Configuration = DotNet.BuildConfiguration.Release })
    )
)
// --------------------------------------------------------------------------------------
// Build order
// --------------------------------------------------------------------------------------

"Clean"
  ==> "InstallDotNetCLI"
  ==> "Restore"
  ==> "Build" <=> "Publish" <=> "Pack"

Target.runOrDefault "Build"