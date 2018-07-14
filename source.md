XML comment pre-parser for C#
=====
I want the C# XML comments pass to pandoc or doxygen
with more whole or full-architecture specification of my softwares.

but XML comments status is...

- can write only for classess or members, not for the files, namespaces.
- output all documents include unusable or small one.
- can't order the document output.

my use case of this tool is...

- make a archtecture document with this tool and pandoc by papers and computer.
- make a reference documents with Doxygen on computer.
- show comments on intelli-sense.


How to use
---

### requirements

- Linux: Mono (4.6.2) and Mono C#
- Windows: Visual Studio 2013 or later ( should be OK, I did not test yet. )
- pandoc


### build and run

- compile this source (or binary is in [release](release) )

    - see [Makefile](Makefile) `build` section.
    - [Mono binary compatible with Windows](https://www.mono-project.com/docs/faq/technical/#is-mono-binary-compatible-with-windows])

- run the tool in your C# project

    see [Makefile](Makefile) `doc` section.

```bash
$ cd path/to/your/project
$ prepandoc . README.md temp.md
```

- run pandoc to convert the md to output.

```bash
$ pandoc -o doc.html temp.md
```


Example output
------

### 1. this project output

- generated the document in [this repository - readme](README.md)
- generated the reference in [this repository - html](html/index.html)
- see my script in [Makefile](Makefile) `doc` section.

### 2. sample output

- here is C\# code.

```C#
namespace {
/// <remarks>
/// sample
/// ===
/// output order from up to low in file.
/// </remarks>
public class abc {
    /// <summary a="1"> program start point
    /// </summary>
    /// <remarks>
    /// some explanation here.
    /// </remarks>
    public void main() {
    }
}
}
```

- use prepandoc.exe

```bash
$ unzip prepandoc-cs1.4.1.zip
$ ./prepandoc.exe . a README2.md
$ pandoc -o result.html README2.md
$ browse result.html
```

- then, you got a markdown file from C\# source and its html.

```markdown
sample
===
output order from up to low in file.

### main
program start point

some explanation here.
```


Check latest release
---
this tool hosted in github, please check
https://github.com/kuri65536/preparse-xml-comment-to-pandoc

and please pull request new feature or debug results...


Please donate
---
If you are feel to nice for this software,
please donation to my

- Bitcoin **| 1FTBAUaVdeGG9EPsGMD5j2SW8QHNc5HzjT |**
- or Ether **| 0xd7Dc5cd13BD7636664D6bf0Ee8424CFaF6b2FA8f |** .

<!--
 vi: ft=markdown
 -->

