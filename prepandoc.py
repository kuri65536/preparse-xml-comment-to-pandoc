#! /usr/bin/python3
from logging import debug as debg
import os
import re
import sys
from typing import Dict, IO, Iterator, List, Text, Tuple

from xml.parsers import expat  # type: ignore
# from html import escape as htmlquote


Dict, IO, Iterator, List, Text, Tuple
rex_comment = re.compile("^ *\/\/\/")


def make_header(fp, ftop):
    # type: (IO[Text], Text) -> Text
    all = "<all>\n"
    with open(ftop, "rt", encoding="utf-8") as fin:
        def out(src):
            # type: (Text) -> Text
            fp.write(src)
            return src

        all += out("<file>\n")
        all += out("<summary>\n")
        all += out(fin.read() + "\n")
        all += out("</summary>\n")
        all += out("</file>\n")
    return all


def iter_sources(dname):  # {{{1
    # type: (Text) -> Iterator[Text]
    seq = []
    for dbase, dnames, fnames in os.walk(dname):
        for fbase in fnames:
            fname = os.path.join(dbase, fbase)
            if not fname.endswith(".cs"):
                continue
            fname = os.path.realpath(fname)
            seq.append(fname)
    seq.sort()
    for fname in seq:
        yield fname


def extract_plain_and_xml_text(fname):  # {{{1
    # type: (Text) -> Iterator[Text]
    yield "<file> <!-- {} -->\n".format(fname)
    with open(fname, "rb") as fp:
        debg("parse {}...".format(fname))
        block = []  # type: List[Text]
        for line in fp:
            try:
                lin = line.decode("utf-8")
            except UnicodeDecodeError:
                lin = line.decode("sjis")
            src = lin.strip()
            f = not src.startswith("///")
            if f and len(block) > 0:
                lin = determine_function_name(lin)
                if len(lin) > 0:
                    yield "<block>{}</block>\n".format(lin)
                for i in block:
                    i = strip_comment(i)
                    yield i
                block = []
                continue
            elif f:
                continue
            debg(lin, end="")
            block.append(lin)
    yield "</file>\n"


def strip_comment(line):  # {{{1
    # type: (Text) -> Text
    ret = rex_comment.sub("", line)
    ret = ret.replace("&", "&amp;")
    # ret = htmlquote(ret)  # N.G.: <summary> -> &lt;summary
    return ret


def determine_function_name(src):  # {{{1
    # type: (Text) -> Text
    if "using" in src:
        return ""

    # case of: static int var = new int();
    if "=" in src:
        src = src.split("=")[0]
        src = src.strip()
        src = src.split(" ")[-1]
        return src.strip()

    # remove comment...
    src = src.split("//")[0]
    src = src.strip()

    # case of: static int func(int a1, int a2) {
    # case of: class cls {
    # case of: int prop {get {return some[0];}}
    if "{" in src:
        src = src.split("{")[0]
        src = src.strip()

    # case of: static int var;
    elif ";" in src:
        src = src.split(" ")[-1]
        src = src.replace(";", "")
        return src

    # case of: class cls: int {  // inherit
    if "class" in src and ":" in src:
        src = src.split(":")[0]

    # case of: static int func(int a,
    # case of: static int func(
    if "(" in src:
        src = src.split("(")[0]
        src = src.split(" ")[-1]
        src = src.strip()
        return src

    # case of: static int var
    src = src.split(" ")[-1]
    src = src.strip()
    return src


def strip_indent(block):  # {{{1
    # type: (List[Text]) -> Text
    src = ''.join(block)
    lines = src.splitlines()
    for line in lines:
        lin1 = strip_comment(line)
        if len(lin1.strip()) > 0:
            break
    else:
        lin1 = ""

    ind = 0  # can't determine, reserve all text.
    if len(lin1) > 0:
        for n, ch in enumerate(lin1):
            if ch != " ":
                ind = n
                break

    ret = ""
    for line in lines:
        if len(line) < ind:
            ret += line
        else:
            ret += line[ind:]
        ret += "\n"
    return ret


class Parser(object):  # {{{1
    def __init__(self, parser, fname):  # {{{1
        # type: (expat.Parser, Text) -> None
        if isinstance(fname, str):
            self.fp = open(fname, "w", encoding="utf-8")
        else:
            self.fp = fname

        parser.StartElementHandler = self.enter_tag
        parser.EndElementHandler = self.leave_tag
        parser.CharacterDataHandler = self.chardata

        self.block_name = ""
        self.tag = ""
        self.tag_f = ""
        self.block = []  # type: List[Text]

    def flash_output(self):  # {{{1
        # type: () -> None
        if len(self.block) > 0:
            ret = strip_indent(self.block)
            if len(self.block_name) > 0:
                self.fp.write("### " + self.block_name + "\n")
            self.fp.write(ret)
        self.block = []

    def enter_tag(self, tagname, attrs):  # {{{1
        # type: (Text, Dict[Text, Text]) -> None
        if tagname == "block":
            self.tag = tagname
        if tagname == "summary":
            self.flash_output()
            self.tag = tagname
        if tagname == "file":
            self.tag_f = tagname

    def leave_tag(self, tagname):  # {{{1
        # type: (Text) -> None
        if tagname == "file":
            self.flash_output()
        if tagname == "block" and tagname == self.tag:
            self.tag = ""
        if tagname == "summary" and tagname == self.tag:
            self.fp.write("\n")
            self.tag = ""
            debg("leave summary...")
        if tagname == "file" and tagname == self.tag:
            self.tag_f = ""

    def chardata(self, data):  # {{{1
        # type: (Text) -> None
        debg(data)
        if self.tag in ("file", ):
            self.fp.write(data)
            self.block.append(data)
        if self.tag in ("block", ):
            self.block_name = data
        if self.tag in ("summary", "remarks"):
            self.block.append(data)


def main(args):  # {{{1
    # type: (List[Text]) -> None
    if len(args) < 1:
        droot = "."
    else:
        droot = args[0]
    if len(args) < 2:
        ftop = "README.md"
    else:
        ftop = args[1]
    if len(args) < 3:
        fout = "temp.md"
    else:
        fout = args[2]

    p = expat.ParserCreate()
    parser = Parser(p, fout)
    # parser = Parser(p, sys.stdout)
    fp = open("temp.txt", "wt", encoding="utf-8")
    all = make_header(fp, ftop)

    for fname in iter_sources(droot):
        txt = ""
        for line in extract_plain_and_xml_text(fname):
            if line.endswith("\x10\x13"):
                line = line[:-2]
            txt += line

        all += txt
        fp.write(txt)
    fp.close()
    all += "</all>"

    try:
        p.Parse(all, True)
    except Exception as ex:
        print("parse.exception: " + str(ex))
    parser.fp.close()


if __name__ == "__main__":
    main(sys.argv[1:])
# vi: ft=python:et:ts=4:nowrap:fdm=marker
