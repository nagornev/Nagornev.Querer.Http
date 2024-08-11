# Nagornev.Querer.Http
__Nagornev.Querer.Http__ is library was created to simplify working with the HTTP protocol by creating and handling requests in isolation.

## Installation
Install the current version with __[dotnet](https://dotnet.microsoft.com/ru-ru/)__:
```C#
dotnet add package Nagornev.Querer.Http -s "https://nuget.pkg.github.com/nagornev/index.json"
```
## Usage

### Quick start
Use this code to send an HTTP GET-request to the server __[httpbin](https://httpbin.org)__. You will receive data about your request in JSON format.
```C#
using Nagornev.Querer.Http;
using System.Net;

var httpQuerer = new QuererHttpClient();

var compiler = QuererHttpRequestMessageCompilerBuilder.Create()
                                                        .UseMethod(compiler => compiler.Set(HttpMethod.Get))
                                                        .UseUrl(compiler => compiler.Set(new Uri("https://httpbin.org/get")))
                                                      .Build();

var handler  = QuererHttpResponseMessageHandlerBuilder<string>.Create()
                                                                .UsePreview(handler => handler.Set(response => response.StatusCode == HttpStatusCode.OK))
                                                                .UseContent(handler => handler.SetContent(response => response.GetText())
                                                                                              .SetConfirmation(content => !string.IsNullOrEmpty(content)))
                                                              .Build();

await httpQuerer.SendAsync(compiler, handler);

Console.WriteLine(handler.Content);
```
