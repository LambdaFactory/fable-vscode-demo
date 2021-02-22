module VSCodeFable

open Fable.Import

let activate (context : vscode.ExtensionContext) =
    printfn "Hello world"

    let action : obj -> obj = fun _ ->
        vscode.window.showInformationMessage("Hello world!", Array.empty<string>)

    vscode.commands.registerCommand("extension.sayHello", action)
    |> context.subscriptions.Add
