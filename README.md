# The Middle

Wraps an application's stdin/stdout/stderr and dumps the console I/O into file.

It's well known that we cannot intercept an application's console I/O unless we are the creator of the process. In some cases, especially when we make the processes communicate with each other via console I/O, however, we may still wish to see the content that is passed in/out the console. This utility can act as a "wrapper" that dumps all the console activities into a file.

