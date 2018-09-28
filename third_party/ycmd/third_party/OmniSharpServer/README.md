#Notice
This project is being replaced with [OmniSharp Roslyn](https://github.com/OmniSharp/omnisharp-roslyn). I'll still accept PRs for this project, but please note that all development has stopped here.



OmniSharpServer
===============
[![Mono Build Status](https://travis-ci.org/OmniSharp/omnisharp-server.svg?branch=master)](https://travis-ci.org/OmniSharp/omnisharp-server) [![Windows Build Status](http://teamcity.codebetter.com/app/rest/builds/buildType:(id:bt1232)/statusIcon)](http://teamcity.codebetter.com/viewType.html?buildTypeId=bt1232&guest=1)

HTTP wrapper around [NRefactory] (https://github.com/icsharpcode/NRefactory) allowing C# editor plugins to be written for any editor in any language.


This is the server component for the [Vim OmniSharp plugin](https://github.com/nosami/OmniSharp), [YouCompleteMe](https://github.com/Valloric/YouCompleteMe), [Sublime Text 2](https://github.com/PaulCampbell/OmniSharpSublimePlugin), [Sublime Text 3](https://github.com/moonrabbit/OmniSharpSublime), [Emacs OmniSharp plugin](https://github.com/sp3ctum/omnisharp-emacs) and [Atom plugin](https://github.com/Mpdreamz/atom-sharper)


#Build

####OSX / Linux 
Requires a minimum of Mono 3.0.12 - If you absolutely must use mono 2.10 then checkout the mono-2.10.8 tag. [Updating mono on ubuntu](https://github.com/nosami/OmniSharpServer/wiki). If you are using OSX, please use the official version of Mono as the homebrew version has [known](https://github.com/nosami/OmniSharpServer/issues/83) [issues](https://github.com/nosami/OmniSharpServer/issues/80).
```
    git clone https://github.com/OmniSharp/omnisharp-server.git
    cd omnisharp-server
    git submodule update --init --recursive
    xbuild
```

####Windows
```
    git clone https://github.com/OmniSharp/omnisharp-server.git
    cd omnisharp-server
    git submodule update --init --recursive

    # (if using Cygwin, overwrite the default config file config.json with config-cygwin.json)
    copy OmniSharp\config-cygwin.json OmniSharp\config.json
    msbuild
```

To start the Omnisharp server manually (The Vim OmniSharp client and YouCompleteMe may start this for you automatically):

- Use the current working directory
    - If one solution file is found, use it. Otherwise start in folder mode.
```
[mono] OmniSharp.exe
```

- To specify a solution file / project files
```
[mono] OmniSharp.exe -s (path\to\sln)
```
- Without a solution file (OmniSharp will parse all C# files it finds and add references to every dll it finds)
```
[mono] OmniSharp.exe -s path
```

###Problems (or just want to see what's going on)?

Try adding an extra ```-v Verbose``` to the end of the command line. You should see something like the following.

```

************ Request ************
POST - /autocomplete
************ Headers ************
Accept-Encoding - identity
Content-Length - 378
Host - localhost:2000
Content-Type - application/x-www-form-urlencoded
Connection - close
User-Agent - Python-urllib/2.7
************  Body ************
column=22&filename=/Users/jason/.vim/bundle/Omnisharp/server/OmniSharp/Logger.cs&buffer=using System;

namespace OmniSharp
{
    class Test
    {
        public Test()
        {
            Console.B
        }
    }
}&WantDocumentationForEveryCompletionResult=False&line=9&wordToComplete=B
Looking for project containing file /Users/jason/.vim/bundle/Omnisharp/server/OmniSharp/Logger.cs
/Users/jason/.vim/bundle/Omnisharp/server/OmniSharp/Logger.cs belongs to /Users/jason/.vim/bundle/Omnisharp/server/OmniSharp/OmniSharp.csproj
Looking for project containing file /Users/jason/.vim/bundle/Omnisharp/server/OmniSharp/Logger.cs
/Users/jason/.vim/bundle/Omnisharp/server/OmniSharp/Logger.cs belongs to /Users/jason/.vim/bundle/Omnisharp/server/OmniSharp/OmniSharp.csproj
Getting Completion Data
Got Completion Data
************  Response ************
[{"CompletionText":"BackgroundColor","Description":"ConsoleColor BackgroundColor { get; set; }","DisplayText":"ConsoleColor BackgroundColor"},{"CompletionText":"Beep(","Description":"void Beep(int frequency, int duration);","DisplayText":"void Beep(int frequency, int duration)"},{"CompletionText":"Beep()","Description":"void Beep();","DisplayText":"void Beep()"},{"CompletionText":"BufferHeight","Description":"int BufferHeight { get; set; }","DisplayText":"int BufferHeight"},{"CompletionText":"BufferWidth","Description":"int BufferWidth { get; set; }","DisplayText":"int BufferWidth"},{"CompletionText":"CursorVisible","Description":"bool CursorVisible { get; set; }","DisplayText":"bool CursorVisible"},{"CompletionText":"KeyAvailable","Description":"bool KeyAvailable { get; }","DisplayText":"bool KeyAvailable"},{"CompletionText":"MoveBufferArea(","Description":"void MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop);","DisplayText":"void MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop)"},{"CompletionText":"MoveBufferArea(","Description":"void MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop, char sourceChar, ConsoleColor sourceForeColor, ConsoleColor sourceBackColor);","DisplayText":"void MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop, char sourceChar, ConsoleColor sourceForeColor, ConsoleColor sourceBackColor)"},{"CompletionText":"NumberLock","Description":"bool NumberLock { get; }","DisplayText":"bool NumberLock"},{"CompletionText":"SetBufferSize(","Description":"void SetBufferSize(int width, int height);","DisplayText":"void SetBufferSize(int width, int height)"}]
/autocomplete 7ms
```
