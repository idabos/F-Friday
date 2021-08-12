module Server

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.Extensions.DependencyInjection
open Giraffe
open HttpHandlers
open Thoth.Json.Net


let routes =
    choose [ GET    >=> route  "/api/recipes"    >=> getRecipes 
             POST   >=> route  "/api/recipe"     >=> postRecipe
             PUT    >=> route  "/api/recipe"     >=> putRecipe
             DELETE >=> routef "/api/recipe/%O" deleteRecipe
             RequestErrors.NOT_FOUND "Not found"
           ]

let configureCors (builder: CorsPolicyBuilder) =
    builder.AllowAnyMethod()
           .AllowAnyHeader()
           .AllowAnyOrigin() |> ignore

let configureApp (app : IApplicationBuilder) =
    app.UseCors(configureCors)
       .UseGiraffe routes

let configureServices (services : IServiceCollection) =
    services.AddCors() |> ignore
    services.AddGiraffe() |> ignore
    services.AddSingleton<Json.ISerializer> (Thoth.Json.Giraffe.ThothSerializer (caseStrategy = CamelCase)) |> ignore

[<EntryPoint>]
let main args =
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .Configure(Action<IApplicationBuilder> configureApp)
                    .ConfigureServices(configureServices)
                    |> ignore)
        .Build()
        .Run()
    0