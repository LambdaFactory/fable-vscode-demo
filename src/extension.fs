module VSCodeFable

open Fable.Core
open Fable.Import
open Fable.Import.vscode

let activate (context : vscode.ExtensionContext) =
    printfn "Hello world"

    vscode.commands.registerCommand("extension.sayHello", fun _ ->
        vscode.window.showInformationMessage "Hello world!" |> unbox )
    |> context.subscriptions.Add
