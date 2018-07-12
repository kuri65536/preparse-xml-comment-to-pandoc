///
/// Copyright (c) 2018, shimoda as kuri65536 _dot_ hot mail _dot_ com
///                     ( email address: convert _dot_ to . and joint string )
///
/// This Source Code Form is subject to the terms of the Mozilla Public License,
/// v.2.0. If a copy of the MPL was not distributed with this file,
/// You can obtain one at https://mozilla.org/MPL/2.0/.
///
/// <page>1</page>
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Security;

using Log = PrePandoc.logging;  // import debug as debg, error as eror
using cfg = PrePandoc.Config;

namespace PrePandoc {

/// <summary> <!-- Parser {{{1 --> The main class of this project.
/// </summary>
/// <remarks>
/// How it works
/// ----
/// 1. parse XML from C\# sources
/// 2. parse single markdown file from XML.
/// </remarks>
public class Parser {
    public string tag;
    public string tag_f;
    public string block_name;
    public Dictionary<string, string> block_info;
    public List<string> block;
    public string dname_root;

    static Regex rex_comment = new Regex(@"^ *\/\/\/");
    static Regex rex_white = new Regex(@"\s*");

    /// <summary> <!-- make_header {{{1 --> parse source:
    /// make the begining of XML and insert header file.
    /// </summary>
    /// <remarks>
    /// ### parse XML from C\# sources
    /// </remarks>
    public static string make_header(string ftop) {
        var txt = "";
        try {
            txt = System.IO.File.ReadAllText(ftop, cfg.enc);
        } catch (Exception) {
            txt = "<!-- file is not found! -->";
        }
        var tag = cfg.tags_output.Length > 0 ? cfg.tags_output[0]: "summary";

        var all = TextFile.print("<all>\n");
        // var fname = filename_relative(ftop);
        all += TextFile.print("<file name=\"{0}\">", ftop);
        all += TextFile.print("<block name=\"top\">");
        all += TextFile.print("<{0}>", tag);
        all += TextFile.print(txt);
        all += TextFile.print("</{0}>", tag);
        all += TextFile.print("</block>\n");
        all += TextFile.print("</file>\n");
        return all;
    }

    /// <summary> <!-- filename_relative {{{1 --> utility,
    /// strip the directory of a root from path string.
    /// </summary>
    public string filename_relative(string path) {
        var droot = System.IO.Path.GetFullPath(this.dname_root);
        // var droot = System.IO.Path.GetDirectoryName(
        //     System.Reflection.Assembly.GetEntryAssembly().Location);
        // droot = System.IO.Path.GetDirectoryName(droot);  // ..
        // droot = System.IO.Path.GetDirectory(droot);  // ..

        Log.debg("relative: directory root: {0}", droot);
        var _path = System.IO.Path.GetFullPath(path);
        Log.debg("relative: file-name     : {0}", _path);
        if (_path.StartsWith(droot)) {
            _path = _path.Substring(droot.Length);
        }
        if (_path.StartsWith("/")) {
            _path = _path.Substring(1);
        }
        return _path;
    }

    /// <summary> <!-- iter_files {{{1 --> tool for enumerate the
    /// files of specified directory (recursive) .
    /// </summary>
    public static IEnumerable<string> iter_files(string dname) {
        foreach (var d in System.IO.Directory.GetDirectories(dname)) {
            foreach (var f in iter_files(d)) {
                yield return f;
            }
        }

        foreach (var f in System.IO.Directory.GetFiles(dname)) {
            yield return f;
        }
    }

    /// <summary> <!-- iter_source {{{1 --> tool for enumerate the
    /// source files of specified directory (recursive) .
    ///
    /// ignore the files by `Config.filter_file_name()` .
    /// </summary>
    /// <remarks>
    /// - enumerate the C\# sources by `iter_source()` .
    /// </remarks>
    public static IEnumerable<string> iter_sources(string dname) {
        var seq = new List<string>();
        foreach (var fname in iter_files(dname)) {
            // fname = System.IO.Path.Combine(dbase, fbase);
            if (cfg.filter_file_name(fname)) {
                continue;
            }
            var f = System.IO.Path.GetFullPath(fname);
            seq.Add(f);
        }
        seq = cfg.sort_files(seq);
        foreach (var fname in seq) {
            yield return fname;
        }
    }

    /// <summary> <!-- extract_plain_and_xml_output {{{1 --> parse_source
    /// - python version: can handle multi-encodings of documents.
    /// - C# version: did not handle multi-encodings,
    ///     you must choose single encoding.
    /// </summary>
    /// <remarks>
    /// - strip triple slash lines from source `///`
    ///     by `extract_plain_and_xml_text()` .
    /// </remarks>
    public static IEnumerable<string> extract_plain_and_xml_text(
            string fname
    ) {
        yield return "<file name=\"" + fname + "\">\n";
        Log.debg("parse {0}...", fname);
        var block = new List<String>();
        foreach (var line in System.IO.File.ReadAllLines(fname)) {
            /* try {  // multi-encodings
                lin = line.decode(cfg.enc);
            } catch (UnicodeDecodeError) {
                lin = line.decode(cfg.encs[1]); } */
            var src = line.Trim();
            var f = !src.StartsWith("///");
            if (f && block.Count > 0) {
                var name = determine_function_name(line);
                name = SecurityElement.Escape(name);
                name = name.Length < 1 ? "<block>":
                       String.Format("<block name=\"{0}\">", name);
                yield return name + "\n";
                foreach (var i in block) {
                    var j = strip_comment(i);
                    yield return j + "\n";
                }
                yield return "</block>\n";
                block.Clear();
                continue;
            } else if (f) {
                continue;
            }
            Log.debg("src:extracted: " + line);
            block.Add(line);
        }
        yield return "</file>\n";
    }

    /// <summary> <!-- strip_comment {{{1 --> parse source,
    /// strip the head `///` and quote some strings for XML.
    /// </summary>
    public static string strip_comment(string line) {
        var ret = rex_comment.Replace(line, "");
        ret = ret.Replace("&", "&amp;");
        // ret = htmlquote(ret)  # N.G.: <summary> -> &lt;summary
        return ret;
    }

    /// <summary> <!-- determine_function_name {{{1 --> parse source,
    /// determine the name of identifier at the bottom of comment block.
    /// </summary>
    /// <remarks>
    /// - name the blocks from C\# statement on the next line,
    ///     but the algo is simple and lazy...
    ///     this process is specified in `determine_function_name()` .
    /// </remarks>
    public static string determine_function_name(string src) {
        if (src.Contains("using")) {
            return "";
        }

        // case of: static int var = new int();
        if (src.Contains("=")) {
            src = src.Split('=')[0];
            src = src.Trim();
            src = src.Split(' ').Last();
            return src.Trim();
        }

        // remove comment...
        src = src.Split(new [] {"//"}, StringSplitOptions.None)[0];
        src = src.Trim();

        // case of: static int func(int a1, int a2) {
        // case of: class cls {
        // case of: int prop {get {return some[0];}}
        if (src.Contains("{")) {
            src = src.Split('{')[0];
            src = src.Trim();

        // case of: static int var;
        } else if (src.Contains(";")) {
            src = src.Split(' ').Last();
            src = src.Replace(";", "");
            return src;
        }

        // case of: class cls: int {  // inherit
        if (src.Contains("class") && src.Contains(":")) {
            src = src.Split(':')[0];
        }

        // case of: static int func(int a,
        // case of: static int func(
        if (src.Contains("(")) {
            src = src.Split('(')[0];
            src = src.Split(' ').Last();
            src = src.Trim();
            return src;
        }

        // case of: static int var
        src = src.Split(' ').Last();
        src = src.Trim();
        return src;
    }

    /// <summary> <!-- strip_indent {{{1 --> part of parse XML,
    /// strip indent from content block.
    /// </summary>
    public string strip_indent(List<string> block) {
        var src = String.Join("", block);
        var lines = src.Split('\n');
        var lin1 = "";
        foreach (var line in lines) {
            lin1 = strip_comment(line);
            if (lin1.Trim().Length > 0) {
                break;
            }
            lin1 = "";
        }

        var ind = 0;  // can"t determine, reserve all text.
        if (lin1.Length > 0) {
            int n = -1;
            foreach (var ch in lin1) {
                n += 1;
                if (ch != ' ') {
                    ind = n;
                    break;
                }
            }
        }
        Log.eror("indent: {0}", ind);

        var ret = "";
        foreach (var line in lines) {
            if (line.Length < ind) {
                ret += line;
            } else {
                ret += line.Substring(ind);
            }
            ret += "\n";
        }
        return ret;
    }

    /// <summary> <!-- is_empty {{{1 --> part of parse XML,
    /// check the content string is empty.
    /// </summary>
    public static bool is_empty(string src) {
        var ret = rex_white.Replace(src, "");
        return ret.Length < 1;
    }


    /// <summary> <!-- new {{{1 --> part of parse XML,
    /// regist callbacks to XmlParser, and clear the callback status variables.
    /// </summary>
    /// <remarks>
    /// ### parse single markdown file from XML.
    /// - you can specify markdown style sheet by
    ///     `Config.css_file_name` .
    /// - you can choose the **tag name** of the XML
    ///     by `Config.tags_output` .
    ///     my choise is `remarks` to generate markdown.
    ///     (it is not shown in intelli-sense)
    ///
    /// </remarks>
    public Parser(XmlParser parser, string fname) {
        // type: (expat.Parser, Text) -> None
        if (fname != null) {
            TextFile.open(fname, true);
        } else {
            // this.fp = fname;
        }
        var s = String.Format("<link href=\"{0}\" rel=\"stylesheet\"></link>",
                cfg.css_file_name);
        TextFile.print(s);

        parser.StartElementHandler = this.enter_tag;
        parser.EndElementHandler = this.leave_tag;
        parser.CharacterDataHandler = this.chardata;

        this.block_info = new Dictionary<string, string>();
        this.tag = "";
        this.tag_f = "";
        this.block = new List<string>();
    }

    public void close() {
        // just for ignore warnings.
    }

    /// <summary> <!-- flash_output {{{1 --> part of parse XML.
    /// append the cached block text into the markdown file.
    /// </summary>
    public void flash_output() {
        var blk = this.block;
        Log.debg("flash: {0}", blk.Count);
        if (blk.Count < 1) {
            Log.eror("block is empty(1): " + this.block_name);
            blk.Clear();
            return;
        }

        var ret = strip_indent(blk);
        if (is_empty(ret)) {
            Log.eror("block is empty(2): " + this.block_name);
            if (!cfg.f_output_empty_block) {
                blk.Clear();
                return;
            }
        }
        this.block_info.Add("name", this.block_name);
        var s = cfg.format_block_head(this.block_info);
        if (s.Length > 0) {
            TextFile.print(s);
        }
        TextFile.print(ret);
        blk.Clear();
    }

    /// <summary> <!-- enter_tag {{{1 --> part of parse XML.
    /// the callback from XmlParser when parser found begining of tags.
    /// </summary>
    /// <param name="tagname">begining tag name</param>
    /// <param name="attrs">attributes of the tag</param>
    public void enter_tag(string tagname,
                          Dictionary<String, String> attrs
    ) {
        Log.debg("enter {0}", tagname);
        if (tagname == "block") {
            this.block_name = !attrs.ContainsKey("name") ? "": attrs["name"];
        }
        if (cfg.tags_article.Contains(tagname)
                && attrs.ContainsKey(cfg.attr_article)) {
            Log.debg("detect tag-a {0}", tagname);
            this.block_info.Add(tagname, "1");
            this.tag = tagname;
        }
        if (cfg.tags_output.Contains(tagname)) {
            Log.debg("detect tag-n {0}", tagname);
            this.block_info.Add(tagname, "1");
            this.tag = tagname;
        }
        if (tagname == "file") {
            this.tag_f = tagname;
            var name = attrs.ContainsKey("name") ? attrs["name"]: "";
            name = filename_relative(name);
            name = cfg.format_file_name(name);
            if (name.Length > 0) {
                TextFile.print(name);
            }
        }
    }

    /// <summary> <!-- leave_tag {{{1 --> part of parse XML.
    /// the callback from XmlParser when parser found end of tags.
    /// </summary>
    public void leave_tag(string tagname) {
        if (tagname == "file") {
            if (tagname == this.tag_f) {
                this.tag_f = "";
            }
            return;
        }
        if (tagname == "block") {
            Log.warn("XML:leave block: " + this.block_name);
            this.tag = "";
            this.flash_output();
            this.block_info.Clear();
            return;
        }
        if (tagname != this.tag) {
            return;
        }
        if (cfg.tags_output.Contains(tagname)) {
            this.tag = "";
            return;
        }
        if (cfg.tags_article.Contains(tagname)) {
            this.tag = "";
            return;
        }
    }

    /// <summary> <!-- chardata {{{1 --> parse XML
    /// the callback from XmlParser when parser found data block.
    /// </summary>
    /// <param name="data">data block contents as string</param>
    public void chardata(String data) {
        Log.debg("XML:chardata: {0}", data);
        if (this.tag.Length > 0) {
            this.block.Add(data);
        }
    }

    /// <summary> <!-- Main {{{1 -->
    /// </summary>
    /// <remarks>
    /// command line
    /// ----
    /// main program: parse command line and run parse sources and xml.
    ///
    /// ### command line arguments
    ///
    /// 1. directory name to search C\# sources. (default: `.` )
    /// 2. header markdown. (default: `README.md` )
    /// 3. output markdown. (default: `temp.md` )
    ///
    /// </remarks>
    public static void Main(String[] args) {
        string droot, ftop, fout;
        if (Options.run(args, out droot, out ftop, out fout)) {
            return;
        }
        TextFile.open("temp.txt", true);
        make_header(ftop);

        foreach (var fname in iter_sources(droot)) {
            var txt = "";
            foreach (var line in extract_plain_and_xml_text(fname)) {
                var _line = line;
                if (line.EndsWith("\x10\x13")) {
                    _line = _line.Substring(0, line.Length - 2);
                }
                txt += _line;
            }
            TextFile.write(txt + "\n");
        }
        TextFile.print("</all>");

        var p = new XmlParser();  // expat.ParserCreate();
        var parser = new Parser(p, fout);
        parser.dname_root = droot;
        try {
            var stm = new System.IO.StreamReader("temp.txt");
            p.Parse(stm);  // , True);
        } catch (Exception ex) {
            Log.crit("parse.exception: {0}", ex.Message);
            // can't do in Mono
            // Log.dump_stack(ex.StackInfo.ToString());
        }
        parser.close();
    }
}
}
// vi: ft=cs:et:ts=4:sw=4:nowrap:fdm=marker
