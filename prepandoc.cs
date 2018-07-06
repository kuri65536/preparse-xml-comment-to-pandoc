///
/// Copyright (c) 2018, shimoda as kuri65536 _dot_ hot mail _dot_ com
///                     ( email address: convert _dot_ to . and joint string )
///
/// This Source Code Form is subject to the terms of the Mozilla Public License,
/// v.2.0. If a copy of the MPL was not distributed with this file,
/// You can obtain one at https://mozilla.org/MPL/2.0/.
///
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Log = PrePandoc.logging;  // import debug as debg, error as eror
using cfg = PrePandoc.Config;

/// <summary>
/// </summary>
namespace PrePandoc {

/// <summary> <!-- Parser {{{1 -->
/// </summary>
public class Parser {
    public string tag;
    public string tag_f;
    public string block_name;
    public List<string> block;
    public string dname_root;

    static Regex rex_comment = new Regex(@"^ *\/\/\/");
    static Regex rex_white = new Regex(@"\s*");

    /// <summary> <!-- make_header {{{1 -->
    /// </summary>
    public static string make_header(string ftop) {
        var txt = "";
        try {
            txt = System.IO.File.ReadAllText(ftop, cfg.enc);
        } catch (Exception) {
            return null;
        }

        var all = TextFile.print("<all>\n");
        // var fname = filename_relative(ftop);
        all += TextFile.print("<file name=\"{0}\">", ftop);
        all += TextFile.print("<summary>");
        all += TextFile.print(txt);
        all += TextFile.print("</summary>");
        all += TextFile.print("</file>\n");
        return all;
    }

    /// <summary> <!-- filename_relative {{{1 -->
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

    /// <summary> <!-- iter_files {{{1 -->
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

    /// <summary> <!-- iter_source {{{1 -->
    /// </summary>
    public static IEnumerable<string> iter_sources(string dname) {
        var seq = new List<string>();
        foreach (var fname in iter_files(dname)) {
            // fname = System.IO.Path.Combine(dbase, fbase);
            if (!fname.EndsWith(".cs")) {
                continue;
            }
            var f = System.IO.Path.GetFullPath(fname);
            seq.Add(f);
        }
        // TODO: seq.sort();
        foreach (var fname in seq) {
            yield return fname;
        }
    }

    /// <summary> <!-- iter_files {{{1 -->
    /// </summary>
    public static IEnumerable<string> extract_plain_and_xml_text(
            string fname
    ) {
        yield return "<file name=\"" + fname + "\">\n";
        Log.debg("parse {0}...", fname);
        var block = new List<String>();
        foreach (var line in System.IO.File.ReadAllLines(fname)) {
            /*
            try {
                lin = line.decode("utf-8");
            } catch (UnicodeDecodeError) {
                lin = line.decode("sjis");
            }
             */
            var lin = line;
            var src = lin.Trim();
            var f = !src.StartsWith("///");
            if (f && block.Count > 0) {
                lin = determine_function_name(lin);
                if (lin.Length > 0) {
                    yield return "<block>" + lin + "\n</block>\n";
                }
                foreach (var i in block) {
                    var j = strip_comment(i);
                    yield return j + "\n";
                }
                yield return "\n";
                block.Clear();
                continue;
            } else if (f) {
                continue;
            }
            Log.debg(lin);
            block.Add(lin);
        }
        yield return "</file>\n";
    }

    /// <summary> <!-- strip_comment {{{1 -->
    /// </summary>
    public static string strip_comment(string line) {
        var ret = rex_comment.Replace(line, "");
        ret = ret.Replace("&", "&amp;");
        // ret = htmlquote(ret)  # N.G.: <summary> -> &lt;summary
        return ret;
    }

    /// <summary> <!-- determine_function_name {{{1 -->
    /// </summary>
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

    /// <summary> <!-- strip_indent {{{1 -->
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

    /// <summary> <!-- is_empty {{{1 -->
    /// </summary>
    public static bool is_empty(string src) {
        var ret = rex_white.Replace("", src);
        return ret.Length < 1;
    }


    /// <summary> <!-- new {{{1 -->
    /// </summary>
    public Parser(XmlParser parser, string fname) {
        // type: (expat.Parser, Text) -> None
        if (fname != null) {
            TextFile.open(fname, true);
        } else {
            // this.fp = fname;
        }
        var s = String.Format("<link href=\"{0}\" rel=\"stylesheet\"></link>",
                "tools/swiss.css");
        TextFile.print(s);

        parser.StartElementHandler = this.enter_tag;
        parser.EndElementHandler = this.leave_tag;
        parser.CharacterDataHandler = this.chardata;

        this.block_name = "";
        this.tag = "";
        this.tag_f = "";
        this.block = new List<string>();
    }

    public void close() {
        // just for ignore warnings.
    }

    /// <summary> <!-- flash_output {{{1 -->
    /// </summary>
    public void flash_output() {
        var blk = this.block;
        Log.eror("flash: {0}", blk.Count);
        if (blk.Count < 1) {
            blk.Clear();
            return;
        }

        var ret = strip_indent(blk);
        if (is_empty(ret)) {
            Log.eror("block is empty: " + this.block_name);
            if (!cfg.f_output_empty_block) {
                blk.Clear();
                return;
            }
        }
        if (this.block_name.Length > 0) {
            Log.eror("block name: " + this.block_name);
            var s = cfg.format_block_name(this.block_name);
            TextFile.print(s);
            this.block_name = "";
        }
        TextFile.print(ret);
        blk.Clear();
    }

    /// <summary> <!-- enter_tag {{{1 -->
    /// </summary>
    public void enter_tag(string tagname,
                          Dictionary<String, String> attrs
    ) {
        Log.eror("enter {0}", tagname);
        // type: (Text, Dict[Text, Text]) -> None
        if (tagname == "block") {
            this.tag = tagname;
        }
        if (tagname == "summary") {
            this.flash_output();
            this.tag = tagname;
        }
        if (tagname == "file") {
            this.tag_f = tagname;
            var name = attrs["name"];
            name = filename_relative(name);
            name = cfg.format_file_name(name);
            if (name.Length > 0) {
                TextFile.print(name);
            }
        }
    }

    /// <summary> <!-- leave_tag {{{1 -->
    /// </summary>
    public void leave_tag(string tagname) {
        Log.eror("leave {0}", tagname);
        if (tagname == "file") {
            this.flash_output();
            if (tagname == this.tag_f) {
                this.tag_f = "";
            }
        }
        if (tagname == "block" && tagname == this.tag) {
            var data = this.block_name;
            data = data.Trim();
            Log.eror("found block: " + data);
            this.block_name = data;
            this.tag = "";
        }
        if (tagname == "summary" && tagname == this.tag) {
            TextFile.print("");
            this.tag = "";
            Log.debg("leave summary...");
        }
    }

    /// <summary> <!-- chardata {{{1 -->
    /// </summary>
    public void chardata(String data) {
        Log.debg("chardata: {0}", data);
        if (new[] {"file"}.Contains(this.tag)) {
            TextFile.print(data);
            this.block.Add(data);
        }
        if (new[] {"block"}.Contains(this.tag)) {
            this.block_name += data;
        }
        if (new[] {"summary", "remarks"}.Contains(this.tag)) {
            this.block.Add(data);
        }
    }

    /// <summary> <!-- Main {{{1 -->
    /// </summary>
    public static void Main(String[] args) {
        var droot = (args.Length < 1) ? ".": args[0];
        var ftop = (args.Length < 2) ? "README.md": args[1];
        var fout = (args.Length < 3) ? "temp.md": args[2];

        TextFile.open("temp.txt", true);
        make_header(ftop);

        foreach (var fname in iter_sources(droot)) {
            if (cfg.filter_file_name(fname)) {
                continue;
            }
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
