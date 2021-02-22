#r "paket: groupref build //"
#load ".fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.JavaScript
open Fake.DotNet
open Fake.IO
open Fake.Core.TargetOperators

// --------------------------------------------------------------------------------------
// Build the Generator project and run it
// --------------------------------------------------------------------------------------

Target.create "Clean" (fun _ ->
    Shell.cleanDirs ["./temp"]
    Shell.copy "release" ["README.md"; "LICENSE.md"]
    Shell.copyFile "release/CHANGELOG.md" "RELEASE_NOTES.md"
)

Target.create "YarnInstall" <| fun _ ->
    Yarn.install id

Target.create "DotNetRestore" <| fun _ ->
    DotNet.restore id "src"

module Fable =
    type Command =
        | Build
        | Watch
        | Clean
    type Webpack =
        | WithoutWebpack
        | WithWebpack of args: string option
    type Args = {
        Command: Command
        Debug: bool
        Experimental: bool
        ProjectPath: string
        OutDir: string option
        Defines: string list
        AdditionalFableArgs: string option
        Webpack: Webpack
    }

    let DefaultArgs = {
        Command = Build
        Debug = false
        Experimental = false
        ProjectPath = "./src/Extension.fsproj"
        OutDir = Some "./out"
        Defines = []
        AdditionalFableArgs = None
        Webpack = WithoutWebpack
    }

    let private mkArgs args =
        let fableCmd =
            match args.Command with
            | Build -> ""
            | Watch -> "watch"
            | Clean -> "clean"
        let fableProjPath = args.ProjectPath
        let fableDebug = if args.Debug then "--define DEBUG" else ""
        let fableExperimental = if args.Experimental then "--define IONIDE_EXPERIMENTAL" else ""
        let fableOutDir =
            match args.OutDir with
            | Some dir -> sprintf "--outDir %s" dir
            | None -> ""
        let fableDefines = args.Defines |> List.map (sprintf "--define %s") |> String.concat " "
        let fableAdditionalArgs = args.AdditionalFableArgs |> Option.defaultValue ""
        let webpackCmd =
            match args.Webpack with
            | WithoutWebpack -> ""
            | WithWebpack webpackArgs ->
                sprintf "--%s webpack %s %s %s"
                    (match args.Command with | Watch -> "runWatch" | _ -> "run")
                    (if args.Debug then "--mode=development" else "--mode=production")
                    (if args.Experimental then "--env.ionideExperimental" else "")
                    (webpackArgs |> Option.defaultValue "")

        // $"{fableCmd} {fableProjPath} {fableOutDir} {fableDebug} {fableExperimental} {fableDefines} {fableAdditionalArgs} {webpackCmd}"
        sprintf "%s %s %s %s %s %s %s %s" fableCmd fableProjPath fableOutDir fableDebug fableExperimental fableDefines fableAdditionalArgs webpackCmd

    let run args =
        let cmd = mkArgs args
        let result = DotNet.exec id "fable" cmd
        if not result.OK then
            failwithf "Error while running 'dotnet fable' with args: %s" cmd

Target.create "RunScript" (fun _ ->
    Fable.run { Fable.DefaultArgs with Command = Fable.Build; Debug = false; Webpack = Fable.WithWebpack None }
)

Target.create "Watch" (fun _ ->
    Fable.run { Fable.DefaultArgs with Command = Fable.Watch; Debug = true; Webpack = Fable.WithWebpack None }
)


// --------------------------------------------------------------------------------------
// Run generator by default. Invoke 'build <Target>' to override
// --------------------------------------------------------------------------------------

Target.create "Default" ignore

"YarnInstall" ?=> "RunScript"
"DotNetRestore" ?=> "RunScript"

"Clean"
==> "RunScript"
==> "Default"

Target.runOrDefault "Default"
