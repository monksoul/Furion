[中](https://gitee.com/dotnetchina/Furion) | **En**

# Furion

An application framework that you can integrate into any .NET/C# application.

> AI can generate ten thousand lines of code in a second, but it cannot think through architectural evolution and system boundaries for you.  
> AI brings code generation out of control, Furion keeps system architecture in control.  
> Furion does not follow blindly or make noise. It never introduces AI-generated low-level code, guarding only the framework's purity, transparency, and rigor.  
> Furion gives you engineering certainty, and irreplaceable long-term stability.

## Installation

```powershell
dotnet add package Furion
```

## Examples

We have several examples [on the website](https://furion.net). Here is the first one to get you started:

```cs
Serve.Run();

[DynamicApiController]
public class HelloService
{
    public string Say() => "Hello, Furion";
}
```

Open browser access `http://localhost:5000`.

## Documentation

You can find the [Furion](https://gitee.com/dotnetchina/Furion) documentation [on the website](https://furion.net).

## Contributing

The main purpose of this repository is to continue evolving [Furion](https://gitee.com/dotnetchina/Furion) core, making it faster and easier to use. Development of [Furion](https://gitee.com/dotnetchina/Furion) happens in the open on [Gitee](https://gitee.com/dotnetchina/Furion), and we are grateful to the community for contributing bugfixes and improvements.

Read [contribution documents](https://gitee.com/dotnetchina/Furion/blob/v4/CONTRIBUTING.md) to learn how you can take part in improving [Furion](https://gitee.com/dotnetchina/Furion).

## License

[Furion](https://gitee.com/dotnetchina/Furion) is primarily distributed under the terms of both the MIT license and the Apache License (Version 2.0).

See [LICENSE-APACHE](https://gitee.com/dotnetchina/Furion/blob/v4/LICENSE-APACHE), [LICENSE-MIT](https://gitee.com/dotnetchina/Furion/blob/v4/LICENSE-MIT), [COPYRIGHT](https://gitee.com/dotnetchina/Furion/blob/v4/COPYRIGHT.md) and [DISCLAIMER](https://gitee.com/dotnetchina/Furion/blob/v4/DISCLAIMER.md) for details.

[![](./assets/baiqian.svg)](https://baiqian.com)