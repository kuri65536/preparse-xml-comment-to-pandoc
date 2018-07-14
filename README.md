<link href="swiss.css" rel="stylesheet"></link> <!-- source.md -->

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

Example output
--------------

### 1. this project output

-   generated the document in [this repository - readme](README.md)
-   generated the reference in [this repository - html](html/index.html)
-   see my script in [Makefile](Makefile) `doc` section.

### 2. sample output

-   here is C\# code.

``` {.c#}
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

-   use prepandoc.exe

``` {.bash}
$ unzip prepandoc-cs1.3.0.zip
$ git clone https://github.com/kuri65536/preparse-xml-comment-to-pandoc test
$ cd test
$ ../prepandoc . source.md README2.md
$ pandoc -o result.html README2.md
$ browse result.html
```

-   then, you got a markdown file from C\# source

``` {.markdown}
sample
===
output order from up to low in file.

### main
program start point

some explanation here.
```

Check latest release
--------------------

this tool hosted in github, please check
https://github.com/kuri65536/preparse-xml-comment-to-pandoc

and please pull request new feature or debug results...

Please donate
-------------

If you are feel to nice for this software, please donation to my

-   Bitcoin **| 1FTBAUaVdeGG9EPsGMD5j2SW8QHNc5HzjT |**
-   or Ether **| 0xd7Dc5cd13BD7636664D6bf0Ee8424CFaF6b2FA8f |** .

<!--
 vi: ft=markdown
 -->
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

<!-- config.cs -->
Configuration
-------------

you can customize the behavior of this tools by editing this config.cs -
`Config` class.

enc

:   -   specify the input file encoding.
    -   this can be override with `--encoding` option.

f\_output\_empty\_block

:   -   do not output the empty comment block to markdown.
    -   this can be override with `--empty-block` option.

css\_file\_name

:   -   specify markdown CSS file name.
    -   this can be override with `--css` option.

tags\_output

:   -   specify XML-tags to output markdown file.
    -   this can be override with `--output-tags` option.

tags\_article

:   -   output the tag which have attribute specified in `attr_article`
        .
    -   this can be override with `--article-tags` option.

attr\_article

:   -   attribute name for `tags_article` .
    -   this can be override with `--attribute` option.

format\_block\_head

:   -   function to format the block name in markdown.
    -   this is hard code behavior, you can edit with rebuild this tool.

format\_file\_name

:   -   function to format the file name in markdown.
    -   this is hard code behavior, you can edit with rebuild this tool.

filter\_file\_name

:   -   function to specify the filtering of source file names.
    -   this is hard code behavior, you can edit with rebuild this tool.

<!-- common.cs -->
### Command-line arguments

this program parse command line and then run. main arguments are the
below 3.

1.  document root dirctory. : default `.`
2.  header markdown file. : default `source.md`
3.  output markdown file. : default `temp.md`

### Command-line options

option `-h` or `--help`

:   -   output help message

option `-v` or `--verbose`

:   -   set the output log-level.
    -   it is equivalent to change logging.**level**.

option `-b` or `--header`

:   -   specify header markdown file before parse sources.

option `-o` or `--output`

:   -   the file name of markdown output.

option `-e` or `--encoding`

:   -   specify the input files encoding, all of source files open with
        this encoding.
    -   this is equivalent to change `Config.encoding`

option `-a` or `--attribute`

:   -   specify the XML attribute name
    -   this option use with `--article-tag` option

option `-c` or `--css`

:   -   the embedded CSS file name in markdown.

option `-E` or `--empty-block`

:   -   enable/disable to output the empty blocks into markdown.

option `-O` or `--output-tags`

:   -   tag-name to output into markdown.

option `-A` or `--article-tags`

:   -   tag-name to output into markdown (with attributes).
    -   this tag is not output without attribute, attribute name can be
        specified with `--attribute` option.

option `--version`

:   -   output the version message and then exit.

<!-- tests.cs -->
Tests
-----

test XmlParser
:   check simple data and it's counting.

<!-- versions.cs -->

TODO
---
- want: setting files for customize behavior.
- want: rename block-tag to member? it similar to msbuild output.
- want: invoke pandoc with recommend options. (did not use Makefile for newbie)
- want: indicates the block for class or method
- want: improve format_file_name function to see option.
- want: improve filter_file_name function to see optinos.


Change-Log
---

### 1.4.1
- fix error with block_name contains '{' (String.Format expression)
- fix: parse indent of `<remarks> <!-- some --> start` to ` start`
- fix: remove double \n\n in attribute tag.
- fix: can't parse header markdown which contain bad XML format (<, & or etc).
- update XML document comment on command line options.

### 1.4.0
- versioning into message
- update reference documents by doxygen output.
- fix: output <all> tag if source.md is missing.

### 1.3.0
- new packaging.
- split CHANGE.log from README.md
- append footer markdown (by pandoc).
- use command line library.

### 1.2.0
- filter output of blocks by user specified tag or attribute.
- insert the block name by block conditions.
- update XML document comment and confirm doxygen output.

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

## table of contents

-   [XML comment pre-parser for C\#](#xml-comment-pre-parser-for-c)
    -   [How to use](#how-to-use)
        -   [requirements](#requirements)
        -   [build and run](#build-and-run)
    -   [Example output](#example-output)
        -   [1. this project output](#this-project-output)
        -   [2. sample output](#sample-output)
    -   [Check latest release](#check-latest-release)
    -   [Please donate](#please-donate)
    -   [How it works](#how-it-works)
        -   [parse XML from C\# sources](#parse-xml-from-c-sources)
        -   [parse single markdown file from
            XML.](#parse-single-markdown-file-from-xml.)
    -   [Configuration](#configuration)
        -   [Command-line arguments](#command-line-arguments)
        -   [Command-line options](#command-line-options)
    -   [Tests](#tests)


