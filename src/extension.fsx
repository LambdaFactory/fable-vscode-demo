#r "../node_modules/fable-import/Fable.Import.dll"
#load "../node_modules/fable-import-vscode/Fable.Import.VSCode.fs"
 
open Fable.Core
open Fable.Import
open Fable.Import.vscode

let activate (context : vscode.ExtensionContext) = 
    printfn "Hello world"
    
    vscode.commands.Globals.registerCommand("extension.sayHello", fun _ ->
        vscode.window.Globals.showInformationMessage "Hello world!" |> unbox )
    |> context.subscriptions.Add
    