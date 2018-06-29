#! /usr/bin/python3
from logging import debug as debg, error as eror
import os
import re
import sys
from typing import Dict, IO, Iterator, List, Text, Tuple

from xml.parsers import expat  # type: ignore
# from html import escape as htmlquote

import config as cfg


Dict, IO, Iterator, List, Text, Tuple
rex_comment = re.compile("^ *\/\/\/")
rex_white = re.compile(r"\s*")


def make_header(fp, ftop):  # {{{1
    # type: (IO[Text], Text) -> Text
    all = "<all>\n"
    with open(ftop, "rt", encoding="utf-8") as fin:
        def out(src):
            # type: (Text) -> Text
            fp.write(src)
            return src

        fname = filename_relative(ftop)
        all += out('<file name="{}">\n'.format(fname))
        all += out("<summary>\n")
        all += out(fin.read() + "\n")
        all += out("</summary>\n")
        all += out("</file>\n")
    return all


def filename_relative(path):  # {{{1
    # type: (Text) -> Text
    droot = os.path.abspath(__file__)  # tools/prepandoc.py
    droot = os.path.dirname(droot)
    droot = os.path.dirname(droot)     # .

    path = os.path.abspath(path)
    if path.startswith(droot):
        path = path[len(droot):]
    else:
        path = os.path.realpath(path)
        if path.startswith(droot):
            path = path[len(droot):]
    if path.startswith("/"):
        path = path[1:]
    return path


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
    yield '<file name="{}"> '.format(fname)
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


def is_empty(src):  # {{{1
    # type: (Text) -> bool
    src = rex_white.sub("", src)
    return len(src) < 1


class Parser(object):  # {{{1
    def __init__(self, parser, fname):  # {{{1
        # type: (expat.Parser, Text) -> None
        if isinstance(fname, str):
            self.fp = open(fname, "w", encoding="utf-8")
        else:
            self.fp = fname
        s = '<link href="{}" rel="stylesheet"></link>'.format(
                "tools/swiss.css")
        self.fp.write(s + "\n")

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
            if is_empty(ret):
                eror("block is empty: " + self.block_name)
                ret = ""
            elif len(self.block_name) > 0:
                s = cfg.format_block_name(self.block_name)
                self.fp.write(s)
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
            name = attrs.get("name", "")
            name = filename_relative(name)
            name = cfg.format_file_name(name)
            if len(name) > 0:
                self.fp.write(name)

    def leave_tag(self, tagname):  # {{{1
        # type: (Text) -> None
        if tagname == "file":
            self.flash_output()
            if tagname == self.tag_f:
                self.tag_f = ""
        if tagname == "block" and tagname == self.tag:
            self.tag = ""
        if tagname == "summary" and tagname == self.tag:
            self.fp.write("\n")
            self.tag = ""
            debg("leave summary...")

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
