# [Nagornev.Querer.Http](https://github.com/nagornev/Nagornev.Querer.Http)

## Information

This library was created to simplify working with the HTTP protocol by creating and handling requests in isolation.

## Installation

Install the current version with __[dotnet](https://dotnet.microsoft.com/ru-ru/)__:
```C#
dotnet add package Nagornev.Querer.Http"
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

### How to use it?

The main advantages of using __Nagornev.Querer.Http__ is the creation requests and handing responses in _isolation_.

#### Request:

You can create a class for the request implementing ```QuererHttpRequestMessageCompiler```:

```C#
public class Request : QuererHttpRequestMessageCompiler
{
    protected override void SetMethod(MethodCompiler compiler)
    {
        compiler.Set(HttpMethod.Get);
    }

    protected override void SetUrl(UrlCompiler compiler)
    {
        compiler.Set(new Uri("https://httpbin.org/get"));
    }

    protected override void SetContent(ContentCompiler compiler)
    {
        compiler.Set(new Dictionary<string, string>()
        {
            { "ContentKey", "ContentValue" }
        });
    }

    protected override void SetHeaders(HeadersCompiler compiler)
    {
        compiler.Set(new Dictionary<string, string>()
        {
            { "HeaderKey", "HeaderValue" }
        });
    }

    protected override void SetRequest(RequestCompiler compiler)
    {
        compiler.Set(request =>
        {
            //Do something with HttpRequestMessage
        });
    }

    protected override IEnumerable<Scheme.Set> SetScheme(Scheme scheme)
    {
        return scheme.Configure(scheme.Method, scheme.Url, scheme.Content, scheme.Headers, scheme.Request);
    }
}
```

To configure the request, you can override the following methods:
- ```SetMethod(compiler)``` - sets request [method](https://learn.microsoft.com/ru-ru/dotnet/api/system.net.http.httpmethod?view=net-8.0);
- ```SetUrl(compiler)``` - sets request [url](https://learn.microsoft.com/ru-ru/dotnet/api/system.uri?view=net-8.0);
- ```SetContent(compiler)``` - sets request [content](https://learn.microsoft.com/ru-ru/dotnet/api/system.net.http.httpcontent?view=net-8.0);
- ```SetHeaders(compiler)``` - sets request [headers](https://learn.microsoft.com/ru-ru/dotnet/api/system.net.http.headers.httpheaders?view=net-8.0);
- ```SetRequest(compiler)``` - sets any parameters [HttpRequestMessage](https://learn.microsoft.com/ru-ru/dotnet/api/system.net.http.httprequestmessage?view=net-8.0);
- ```SetScheme(schme)``` - sets compilation order;

#### Response:

You can create a class for the response implementing ```QuererHttpResponseMessageHandler<T>```:

```C#

public class Response : QuererHttpResponseMessageHandler<string>
{
    protected override void SetPreview(PreviewHandler handler)
    {
        handler.Set(response => response.StatusCode == HttpStatusCode.OK);
    }

    protected override void SetContent(ContentHandler handler)
    {
        handler.SetContent(response => response.GetText())
               .SetConfirmation(content => !string.IsNullOrEmpty(content));
    }

    protected override IEnumerable<Scheme.Set> SetScheme(Scheme scheme)
    {
        return scheme.Configure(scheme.Preview, scheme.Content);
    }
}
```

To configure the response, you can override the following methods:
- ```SetPreview(handler)``` - sets [HttpResponseMessage](https://learn.microsoft.com/ru-ru/dotnet/api/system.net.http.httpresponsemessage?view=net-8.0) confirmations. This handler is needed to make sure that the response from the server is correct;
- ```SetContent(handler)``` - sets property ```QuererHttpResponseMessageHandler<T>.Content```;
- ```SetScheme(scheme)```- sets compilation order;

#### Sending:

You can send request and handle response with method ```QuererHttpClient.SendAsync()```:

```C#
var compiler = new Request();
var handler = new Response();

var client = new QuererHttpClient();

await client.SendAsync(compiler, handler);
```
