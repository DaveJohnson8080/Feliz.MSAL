namespace App

open Fable
open Feliz

type Components =

    static member rootPage = React.functionComponent(fun () ->
        let doAuth, setAuth = React.useState(false)
        let doLogout, setLogout = React.useState(false)
        let isAuthenticated = UseMsal.isAuthenticated()
        
        let msal = UseMsal.useMsal()
        if doAuth then
            if msal.inProgress = "none" then
                promise {
                    let req = {
                        scopes = [||]
                        redirectStartPage = None
                        onRedirectNavigate = None
                    }
                    do! msal.instance.loginRedirect(req)
                } |> ignore
        
        if doLogout then
            if msal.inProgress = "none" then
                promise {
                    let req = {
                        authority = None
                        onRedirectNavigate = None
                    }
                    do! msal.instance.logoutRedirect(req)
                } |> ignore
        
        let msg =
            "Hello " + (if isAuthenticated then "(Authenticated) " else "") + "World"
        
        Html.div [
            Html.h1 msg
            if isAuthenticated then
                Html.button [
                    prop.onClick (fun _ -> true |> setLogout)
                    prop.text "Logout"
                ]
            else
                Html.button [
                    prop.onClick (fun _ -> true |> setAuth)
                    prop.text "Login"
                ]
        ]
    )

    [<ReactComponent>]
    static member appRoot() = 
        let msalConfig = {
            auth = {
                clientId = "<Your application's clientId goes here>"
                authority = "<Your authority url goes here>"
                redirectUri = Browser.Dom.window.location.href
            }
            cache = None
            system = None
        }

        let clientApp = PublicClientApplication(msalConfig)

        MsalProvider.msalProvider [
            msalProvider.instance clientApp
            msalProvider.children (
                Components.rootPage()
            )
        ]
