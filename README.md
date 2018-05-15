# The Middle

Wraps an application's stdin/stdout/stderr and dumps the console I/O into file.

It's well known that we cannot intercept an application's console I/O unless we are the creator of the process. In some cases, especially when we make the processes communicate with each other via console I/O, however, we may still wish to see the content that is passed in/out the console. This utility can act as a "wrapper" that dumps all the console activities into a file.

## Usage

Since this utility is built on .NET Core, you need to install [.NET Core Runtime 2.1](https://www.microsoft.com/net/download/) before running the utility. 

For now, you have to use `dotnet` command to execute the app; later some simple wrapper scripts will be introduced so that you can execute the utility with simple `middle` command.

