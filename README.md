# The Middle

Wraps an application's stdin/stdout/stderr and dumps the console I/O into file.

Installation (with [.NET SDK 2.1.300-rc1](https://www.microsoft.com/net/download/all) or later)

```powershell
dotnet tool install -g CXuesong.TheMiddle
```

And it's all done. Try it out on Windows with

```powershell
middle -d:D:\stdio.txt -a:cmd -- /K "ECHO Hello, World!"
```

Open another shell, and check the dump file (with PowerShell/pwsh)

```powershell
cat D:\stdio.txt -Wait
```

See [Usage](#usage) section for more information.

It's well known that we cannot intercept an application's console I/O unless we are the creator of the process. In some cases, especially when we make the processes communicate with each other via console I/O, however, we may still wish to see the content that is passed in/out the console. This utility can act as a "wrapper" that dumps all the console activities into a file.

## Usage

With the advent of .NET CLI 2.1 Preview, it is possible for you to install The Middle as a global command (just like `npm install -g`), no matter which platform you are working on.

First, you need to install [.NET SDK 2.1.300-rc1](https://www.microsoft.com/net/download/all) or later version. Then, just proceed with the steps shown in the leading section. If you meet any difficulty during the procedure, feel free to open an issue to let me know.



