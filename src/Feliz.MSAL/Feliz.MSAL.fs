namespace Feliz

open System.ComponentModel
open Fable.Core
open Fable.Core.JsInterop
open Fable.React
open Feliz
open System
open System.Collections.Generic

[<EditorBrowsable(EditorBrowsableState.Never)>]
module Object =
    [<Emit("$0 === undefined")>]
    let private isUndefined x = jsNative

    let fromFlatEntries (kvs: seq<string * obj>) : obj =
        let rec setProperty (target : obj) (key : string) (value : obj) =
            match key.IndexOf '.' with
            | -1 -> target?(key) <- value
            | sepIdx ->
                let topKey = key.Substring (0, sepIdx)
                let nestedKey = key.Substring (sepIdx + 1)
                if isUndefined target?(topKey) then
                    target?(topKey) <- obj ()
                setProperty target?(topKey) nestedKey value

        let target = obj ()
        for (key, value) in kvs do
            setProperty target key value
        target

[<AutoOpen; EditorBrowsable(EditorBrowsableState.Never)>]
module MsalHelpers =

    let reactElement (el: ReactElementType) (props: 'a) : ReactElement =
        import "createElement" "react"

    let reactElementTag (tag: string) (props: 'a) : ReactElement =
        import "createElement" "react"

    let createElement (el: ReactElementType) (properties: IReactProperty seq) : ReactElement =
        reactElement el (!!properties |> Object.fromFlatEntries)

    let createElementTag (tag: string) (properties: IReactProperty seq) : ReactElement =
        reactElementTag tag (!!properties |> Object.fromFlatEntries)

type User = {
    picture : string
    name : string
}

type Error = {
    name : string
    message : string
    stack : string
}

type Auth = {
    clientId : string
    authority : string
    redirectUri : string
}

type Cache = {
    cacheLocation : string
    storeAuthStateInCookie : bool
}

type LoggerOptions = {
    loggerCallback : string * string * bool -> unit
}

type System = {
    loggerOptions : LoggerOptions
}

type MSALConfig = {
    auth : Auth
    cache : Cache option
    system : System option
}

type RedirectRequest = {
    scopes : string array
    redirectStartPage : string option
    onRedirectNavigate : (string -> unit) option;
}

type PopupRequest = {
    scopes : string array
}

type AccountInfo = {
    homeAccountId: string
    environment: string
    tenantId: string
    username: string
    localAccountId: string
    name: string option
    idTokenClaims: obj option
};

type AuthenticationResult = {
    authority: string;
    uniqueId: string;
    tenantId: string;
    scopes: string array
    account: AccountInfo option
    idToken: string
    idTokenClaims: obj
    accessToken: string
    fromCache: bool
    expiresOn: DateTime
    tokenType: string
    extExpiresOn: DateTime option
    state: string option
    familyId: string option
    cloudGraphHostName: string option
    msGraphHost: string option
};

type SilentRequest = {
    redirectUri: string option
    extraQueryParameters: Dictionary<string, string> option
    authority: string option
    account: AccountInfo option
    correlationId: string option
    forceRefresh: bool option
}

type EndSessionRequest = {
    authority : string option
    onRedirectNavigate : (string -> unit) option
}

[<Import("PublicClientApplication", from="@azure/msal-browser")>]
type PublicClientApplication (conf : MSALConfig) =
    class
        abstract member loginRedirect : RedirectRequest -> JS.Promise<unit>
        default this.loginRedirect(_: RedirectRequest) : JS.Promise<unit> = jsNative
        abstract member logoutRedirect : EndSessionRequest -> JS.Promise<unit>
        default this.logoutRedirect(_: EndSessionRequest) : JS.Promise<unit> = jsNative
        abstract member loginPopup : PopupRequest -> JS.Promise<AuthenticationResult>
        default this.loginPopup(_: PopupRequest) : JS.Promise<AuthenticationResult> = jsNative
        abstract member acquireTokenSilent : SilentRequest -> JS.Promise<AuthenticationResult>
        default this.acquireTokenSilent(_: SilentRequest) : JS.Promise<AuthenticationResult> = jsNative
    end


type IMsalContext = {
    instance: PublicClientApplication
    inProgress: string
    accounts: AccountInfo
    // logger: Logger
}

type UseMsal =
    static member inline useMsal : unit -> IMsalContext = import "useMsal" "@azure/msal-react"
    static member inline isAuthenticated : unit -> bool = import "useIsAuthenticated" "@azure/msal-react"

type msalProvider =
    static member inline children (element: ReactElement) = prop.children element
    static member inline instance (value : PublicClientApplication) = Interop.mkAttr "instance" value

type MsalProvider =
    static member inline msalProvider props = createElement (import "MsalProvider" "@azure/msal-react") props
