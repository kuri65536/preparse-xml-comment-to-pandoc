<link href="swiss.css" rel="stylesheet"></link> <!-- README.md -->

XML comment pre-parser for C\#
==============================

I want the C\# XML comments pass to pandoc or doxygen with more whole or
full-architecture specification of my softwares.

but XML comments status is...

-   can write only for classess or members, not for the files,
    namespaces.
-   output all documents include unusable or small one.
-   can't order the document output.

my use case of this tool is...

-   make a archtecture document with this tool and pandoc by papers and
    computer.
-   make a reference documents with Doxygen on computer.
-   show comments on intelli-sense.

How to use
----------

### requirements

-   Linux: Mono (4.6.2) and Mono C\#
-   Windows: Visual Studio 2013 or later ( should be OK, I did not test
    yet. )
-   pandoc

### build and run

-   compile this source (or binary is in [release](release) )

    -   see [Makefile](Makefile) `build` section.
    -   [Mono binary compatible with
        Windows](https://www.mono-project.com/docs/faq/technical/#is-mono-binary-compatible-with-windows%5D)

-   run the tool in your C\# project

    see [Makefile](Makefile) `doc` section.

``` {.bash}
$ cd path/to/your/project
$ prepandoc . README.md temp.md
```

-   run pandoc to convert the md to output.

``` {.bash}
$ pandoc -o doc.html temp.md
```

### Example output

-   generated the document in [this repository -
    sample](sample-this-proj.md)

Change-Log
----------

### 1.1.0

-   make XML document comment in this project to sample-this-proj.md.
-   command line explanation.
-   move filtering '.cs' in filter\_file\_name function.
-   implement test environment.

### 1.0.0

-   C\# version
-   change the output order of source files.

### 0.1.0

-   python version
-   extract /// comments from C\# source.
-   parse XML for pandoc-Markdown
-   output relative file name in XML.

TODO
----

-   use command line library.
-   setting files for customize behavior.
-   filter output of blocks by user specified tag or attribute.
-   want: rename block-tag to member? it similar to msbuild output.
-   insert block name by macro.
-   parse indent of `<remarks> <!-- some --> start` to `start`
-   specify toc title to pandoc.

Please donate
-------------

If you are feel to nice for this software, please donation to my

-   Bitcoin **| 1FTBAUaVdeGG9EPsGMD5j2SW8QHNc5HzjT |**
-   or Ether **| 0xd7Dc5cd13BD7636664D6bf0Ee8424CFaF6b2FA8f |** .

<!-- prepandoc.cs -->
How it works
------------

1.  parse XML from C\# sources
2.  parse single markdown file from XML.

### parse XML from C\# sources

-   enumerate the C\# sources by `iter_source()` .

-   strip triple slash lines from source `///` by
    `extract_plain_and_xml_text()` .

-   name the blocks from C\# statement on the next line, but the algo is
    simple and lazy... this process is specified in
    `determine_function_name()` .

### parse single markdown file from XML.

-   you can specify markdown style sheet by `Config.css_file_name` .
-   you can choose the **tag name** of the XML by `Config.tags_output` .
    my choise is `remarks` to generate markdown. (it is not shown in
    intelli-sense)

command line
------------

main program: parse command line and run parse sources and xml.

### command line arguments

1.  directory name to search C\# sources. (default: `.` )
2.  header markdown. (default: `README.md` )
3.  output markdown. (default: `temp.md` )

<!-- config.cs -->
class Config
------------

you can customize the behavior of this tools by editing this class.

-   specify encoding.
-   do not output the empty comment block to markdown.
-   specify markdown CSS file name.
-   specify XML-tags to output markdown file.

-   format the block name in markdown

-   format the file name in markdown

-   specify the filtering of source file names.

-   specify the order of the source files.

<!-- common.cs -->
<!-- tests.cs -->
Tests
-----

test XmlParser
:   check simple data and it's counting.

## table of contents

-   [XML comment pre-parser for C\#](#xml-comment-pre-parser-for-c)
    -   [How to use](#how-to-use)
        -   [requirements](#requirements)
        -   [build and run](#build-and-run)
        -   [Example output](#example-output)
    -   [Change-Log](#change-log)
        -   [1.1.0](#section)
        -   [1.0.0](#section-1)
        -   [0.1.0](#section-2)
    -   [TODO](#todo)
    -   [Please donate](#please-donate)
    -   [How it works](#how-it-works)
        -   [parse XML from C\# sources](#parse-xml-from-c-sources)
        -   [parse single markdown file from
            XML.](#parse-single-markdown-file-from-xml.)
    -   [command line](#command-line)
        -   [command line arguments](#command-line-arguments)
    -   [class Config](#class-config)
    -   [Tests](#tests)


