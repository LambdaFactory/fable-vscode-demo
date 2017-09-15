// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#I "packages/build/FAKE/tools"
#r "FakeLib.dll"
open System
open System.Diagnostics
open System.IO
open Fake
open Fake.YarnHelper
open Fake.ZipHelper


// --------------------------------------------------------------------------------------
// Build the Generator project and run it
// --------------------------------------------------------------------------------------

Target "Clean" (fun _ ->
    CleanDir "./temp"
    CopyFiles "release" ["README.md"; "LICENSE.md"]
    CopyFile "release/CHANGELOG.md" "RELEASE_NOTES.md"
)

Target "YarnInstall" <| fun () ->
    Yarn (fun p -> { p with Command = Install Standard })

Target "DotNetRestore" <| fun () ->
    DotNetCli.Restore (fun p -> { p with WorkingDir = "src" } )

let runFable additionalArgs =
    let cmd = "fable webpack -- --config ../webpack.config.js " + additionalArgs
    DotNetCli.RunCommand (fun p -> { p with WorkingDir = "src" } ) cmd

Target "RunScript" (fun _ ->
    // Ideally we would want a production (minized) build but UglifyJS fail on PerMessageDeflate.js as it contains non-ES6 javascript.
    runFable ""
)

Target "Watch" (fun _ ->
    runFable "--watch"
)


// --------------------------------------------------------------------------------------
// Run generator by default. Invoke 'build <Target>' to override
// --------------------------------------------------------------------------------------

Target "Default" DoNothing

"YarnInstall" ?=> "RunScript"
"DotNetRestore" ?=> "RunScript"

"Clean"
==> "RunScript"
==> "Default"

RunTargetOrDefault "Default"
