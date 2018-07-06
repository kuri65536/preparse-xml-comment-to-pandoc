pre-parser for C#
=====
I want the C# XML comments pass to pandoc or doxygen
with more whole or full-architecture specification.

but XML comments status is...

- can write only for classess or members, not for the files, namespaces.
- output all unusable (or small) documents.
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
- run the tool in your C# project

```bash
$ cd path/to/your/project
$ prepandoc . README.md temp.md
```

- run pandoc to convert the md to output.

```bash
$ pandoc -o doc.html temp.md
```

### Example output

- generated the document in [this repository - sample](sample-this-proj.md)


Change-Log
---

### 1.0.0
- C# version
- change the output order of source files.

### 0.1.0
- python version
- extract /// comments from C# source.
- parse XML for pandoc-Markdown
- output relative file name in XML.


TODO
---
- make XML document comment in this project to sample-this-proj.md.
- command line explanation.
- use command line library.
- setting files for customize behavior.
- implement test.
- want: rename block to member?


Please donate
---
If you are feel to nice for this software,
please donation to my
Bitcoin **< 1FTBAUaVdeGG9EPsGMD5j2SW8QHNc5HzjT >**
or Ether **< 0xd7Dc5cd13BD7636664D6bf0Ee8424CFaF6b2FA8f >** .

<!--
 vi: ft=markdown
 -->

