#! /usr/bin/python3
from logging import debug as debg
import os
import re
import sys
from typing import Dict, IO, Iterator, List, Text, Tuple

from xml.parsers import expat  # type: ignore
from html import escape as htmlquote


Dict, IO, Iterator, Text, Tuple
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
        for line in fp:
            try:
                lin = line.decode("utf-8")
            except UnicodeDecodeError:
                lin = line.decode("sjis")
            src = lin.strip()
            if not src.startswith("///"):
                continue
            lin = strip_comment(lin)
            debg(lin, end="")
            yield lin
    yield "</file>\n"


def strip_comment(line):  # {{{1
    # type: (Text) -> Text
    ret = rex_comment.sub("", line)
    # ret = ret.replace("&", "&amp;")
    ret = htmlquote(ret)
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

        self.summary_name = ""
        self.tag = ""
        self.tag_f = ""

    def enter_tag(self, tagname, attrs):  # {{{1
        # type: (Text, Dict[Text, Text]) -> None
        if tagname == "summary":
            self.tag = tagname
            self.fp.write("### " + self.summary_name)
        if tagname == "file":
            self.tag_f = tagname

    def leave_tag(self, tagname):  # {{{1
        # type: (Text) -> None
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
        if self.tag in ("summary", "remarks"):
            self.fp.write(data)


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
