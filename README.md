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


### Example output

- generated the document in [this repository - sample](sample-this-proj.md)
- see my script in [Makefile](Makefile) `doc` section.


Change-Log
---

### 1.2.0
- filter output of blocks by user specified tag or attribute.
- update XML document comment and update doxygen output.

### 1.1.0
- make XML document comment in this project to sample-this-proj.md.
- command line explanation.
- move filtering '.cs' in filter_file_name function.
- implement test environment.
- specify toc title in pandoc framework.

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
- use command line library.
- setting files for customize behavior.
- want: rename block-tag to member? it similar to msbuild output.
- insert block name by macro.
- parse indent of `&lt;remarks&gt; &lt;!-- some --&gt; start` to ` start`
- block for class or method


Please donate
---
If you are feel to nice for this software,
please donation to my

- Bitcoin **| 1FTBAUaVdeGG9EPsGMD5j2SW8QHNc5HzjT |**
- or Ether **| 0xd7Dc5cd13BD7636664D6bf0Ee8424CFaF6b2FA8f |** .

<!--
 vi: ft=markdown
 -->

