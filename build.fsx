#r "paket:
    nuget System.Text.Json
    nuget FSharp.Core
    nuget Fake.Core.ReleaseNotes
    nuget Fake.Core.Target
    nuget Fake.DotNet.Cli
    nuget Fake.IO.FileSystem
    nuget Fake.JavaScript.Npm"
#load ".fake/build.fsx/intellisense.fsx"
#r "netstandard"

#if !FAKE
#r "netstandard"
#r "Facades/netstandard" // https://github.com/ionide/ionide-vscode-fsharp/issues/839#issuecomment-396296095
#endif

open System
open System.Text.Json
open Fake.Core
open Fake.Core.TargetOperators
open Fake.Core.Xml
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.IO.FileSystemOperators
open Fake.JavaScript

let versionFromTag =
    match (Environment.environVarOrNone "APPVEYOR_REPO_TAG_NAME") with
    | Some s -> s
    | None -> "1.0.0"

let getExitCode (res: ProcessResult) =
  res.ExitCode

let failIfNonZero (exitCode: int) =
  if exitCode <> 0 then failwithf "Failed with exit code %i" exitCode

Target.create "Clean" (fun _ ->
  !! "**/bin"
  ++ "**/obj"
  ++ "**/dist"
  ++ "**/deploy"
  ++ "**/*.fable"
  -- "**/node_modules/**"
  |> Shell.cleanDirs
)

Target.create "DotNetRestore" (fun _ ->
  DotNet.exec id "paket" "restore" |> getExitCode |> failIfNonZero
)

Target.create "Build" (fun _ ->
  DotNet.build
    (fun c -> { c with Configuration = DotNet.BuildConfiguration.Release })
    "Feliz.MSAL.sln"
)

Target.create "Pack" (fun _ ->
  DotNet.exec id "paket" (versionFromTag |> sprintf "pack --version %s dist") |> getExitCode |> failIfNonZero
)

Target.create "UpdatePackages" (fun _ ->
  DotNet.exec id "paket" "update" |> getExitCode |> failIfNonZero
  Npm.exec "up" (fun c -> { c with WorkingDirectory = "demo" } )
  DotNet.exec id "femto" "--resolve demo/src" |> getExitCode |> failIfNonZero
)

Target.create "UpdateFemtoVersionMetadata" (fun _ ->
  let npm = ProcessUtils.findFilesOnPath "npm" |> Seq.head
  let latestStableVersion =
    CreateProcess.fromRawCommand npm ["show"; "@azure/msal-react"; "versions"; "--json"]
    |> CreateProcess.withWorkingDirectory "demo"
    |> CreateProcess.redirectOutput
    |> Proc.run
    |> fun r -> r.Result.Output
    |> fun s -> printfn "%s" s; s
    |> JsonSerializer.Deserialize<seq<string>>
    |> Seq.map SemVer.parse
    |> Seq.filter (fun v -> v.PreRelease.IsSome)        // NOTE!!! : Allowing Pre-release since it's still beta.
    |> Seq.last

  let latestBrowserVersion =
    CreateProcess.fromRawCommand npm ["show"; "@azure/msal-browser"; "version"]
    |> CreateProcess.withWorkingDirectory "demo"
    |> CreateProcess.redirectOutput
    |> Proc.run
    |> fun r -> r.Result.Output
    |> SemVer.parse

  poke
    "src/Feliz.MSAL/Feliz.MSAL.fsproj"
    "//NpmPackage[@Name='@azure/msal-react']/@Version"
    latestStableVersion.AsString

  poke
    "src/Feliz.MSAL/Feliz.MSAL.fsproj"
    "//NpmPackage[@Name='@azure/msal-browser']/@Version"
    latestBrowserVersion.AsString
)

Target.create "CiBuild" ignore

"Clean"
  ==> "DotNetRestore"
  ==> "Build"
  ==> "Pack"

"Build"
  ==> "Pack"

"Pack"
==> "CiBuild"

Target.runOrDefault "CiBuild"
