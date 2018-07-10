///
/// Copyright (c) 2018, shimoda as kuri65536 _dot_ hot mail _dot_ com
///                     ( email address: convert _dot_ to . and joint string )
///
/// This Source Code Form is subject to the terms of the Mozilla Public License,
/// v.2.0. If a copy of the MPL was not distributed with this file,
/// You can obtain one at https://mozilla.org/MPL/2.0/.
///
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Runtime;
using System.Xml;

using Mono.Options;
using cfg = PrePandoc.Config;


namespace PrePandoc {
/// <summary> <!-- logging {{{1 --> log function with leveling.
/// </summary>
public static class logging {
    static readonly int VERB = 10;
    static readonly int DEBG = 20;
    static readonly int INFO = 30;
    static readonly int WARN = 40;
    static readonly int EROR = 50;
    static readonly int CRIT = 60;
    public static int __level__ = WARN;

    public static void setLevel(int lvl) {
        __level__ = lvl;
    }

    public static void crit(string fmt, params object[] args) {
        __log__(CRIT, fmt, args);
    }

    public static void debg(string fmt, params object[] args) {
        __log__(DEBG, fmt, args);
    }

    public static void eror(string fmt, params object[] args) {
        __log__(EROR, fmt, args);
    }

    public static void info(string fmt, params object[] args) {
        __log__(INFO, fmt, args);
    }

    public static void warn(string fmt, params object[] args) {
        __log__(WARN, fmt, args);
    }

    public static void verb(string fmt, params object[] args) {
        __log__(VERB, fmt, args);
    }

    public static void dump_stack(string src) {
    }

    public static void __log__(int lvl, string fmt, params object[] args) {
        if (lvl < __level__) {
            return;
        }
        var msg = String.Format(fmt, args);
        Console.WriteLine(msg);
    }
}

/// <summary> <!-- TextFile {{{1 --> the simple file write tool by C\#
/// </summary>
public static class TextFile {
    static string fname;

    public static void open(string filename, bool fClear=false) {
        fname = filename;
        if (!fClear) {
            return;
        }
        try {
            File.Delete(fname);
        } catch (Exception) {

        }
    }

    public static string print(string fmt, params object[] args) {
        var src = String.Format(fmt, args);
        try {
            File.AppendAllText(fname, src + "\n");
        } catch (Exception) {
        }
        return src;
    }

    public static string write(string src) {
        try {
            File.AppendAllText(fname, src);
        } catch (Exception) {
        }
        return src;
    }
}


public delegate void FuncXmlParseStart(
        string name, Dictionary<string, string> attrs);
public delegate void FuncXmlParseEnd(string name);
public delegate void FuncXmlParseData(string data);


/// <summary> <!-- XmlParser {{{1 -->
/// XML parser tool compat with python expat module.
/// </summary>
public class XmlParser {
    public FuncXmlParseStart StartElementHandler;
    public FuncXmlParseEnd EndElementHandler;
    public FuncXmlParseData CharacterDataHandler;

    /// <summary> <!-- Parse {{{1 -->
    /// </summary>
    public void Parse(string src) {
        var strreader = new StringReader(src);
        this.Parse(strreader);
    }

    /// <summary> <!-- Pasre {{{1 -->
    /// </summary>
    public void Parse(TextReader src) {
        // var stgs = new XmlReaderSettings();
        // stgs.IgnoreWhiteSpace = true;
        var reader = XmlReader.Create(src);

        // reader.MoveToContent();
        // Parse the file and display each of the nodes.
        while (reader.Read())  {
            var node = reader.NodeType;
            logging.verb("xml-reader... {0}", node);
            switch (node)  {
                case XmlNodeType.Element:
                    var name = reader.Name;
                    logging.verb("xml-elem-bgn. {0}-{1}", node, name);
                    var d = parse_attributes(reader);
                    this.StartElementHandler(name, d);
                    break;
                case XmlNodeType.EndElement:
                    name = reader.Name;
                    logging.verb("xml-elem-end. {0}-{1}", node, name);
                    this.EndElementHandler(name);
                    break;
                case XmlNodeType.Whitespace:
                    // var text = reader.ReadString();
                    logging.verb("xml-whspace.. ");  // {0}: {1}", node, text);
                    this.CharacterDataHandler(" ");
                    break;
                case XmlNodeType.Text:
                    var text = reader.Value;
                    logging.verb("xml-text..... {0}: {1}", node, text);
                    this.CharacterDataHandler(text);
                    break;
            }
        }
        reader.Close();
        src.Close();
    }

    /// <summary> <!-- parse_attributes {{{1 -->
    /// </summary>
    public Dictionary<string, string> parse_attributes(
            XmlReader reader
    ) {
        var ret = new Dictionary<string, string>();
        if (!reader.HasAttributes) {
            return ret;
        }
        while (reader.MoveToNextAttribute()) {
            ret.Add(reader.Name, reader.Value);
        }
        #if false
        var n = reader.AttributeCount;
        for (int i = 0; i < n; i++) {
            ret.Add(reader.Name, reader.Value);
        }
        #endif
        reader.MoveToElement();
        return ret;
    }
}

public class Options {
    public static bool run(
        String[] args, out string droot, out string ftop, out string fout
    ) {
        bool ret = false;
        string _ftop = "source.md", _fout = "temp.md", msg = "";
        droot = ".";

        var suite = new Mono.Options.OptionSet() {
            "usage: prepandoc.exe [options]+ [directory] [header] [output]",
            {"h", "help", v => {ret = true;}},
            {"v=|verbose", "verbose level (0-99)", (int? v) => {
                var l = v.HasValue ? v.Value: logging.__level__;
                if (l < 0 || l > 99) {ret = true;}
                logging.__level__ = l;
             }},
            {"b=|header=", "output markdown file before parse sources.",
                (string v) =>{
                _ftop = parse_string(v, "source.md");
                msg += "\nspecified header file: " + _ftop;}},
            {"o=|output=", "file name of the markdown output",
                (string v) =>{
                _fout = parse_string(v, "temp.md");
                msg += "\nspecified output file: " + _fout;}},
            {"e=|encoding=", "source file encoding.",
                (string v) =>{
                cfg.enc = v != null ? System.Text.Encoding.GetEncoding(v):
                                      cfg.enc;
                msg += "\nspecified encoding: " + cfg.enc.ToString();}},
            {"a=|attribute=", "attribute name to extract document.",
                (string v) =>{
                cfg.attr_article = v != null ? v: cfg.attr_article;
                msg += "\nspecified attirbute: " + cfg.attr_article;}},
            {"c=|css=", "CSS file name for markdown.",
                (string v) =>{
                cfg.css_file_name = v != null ? v: cfg.css_file_name;
                msg += "\nspecified CSS: " + cfg.css_file_name;}},
            {"E=|empty-block=", "output empty block.",
                (string v) =>{
                cfg.f_output_empty_block = parse_boolean(v, false);
                msg += "\nspecified empty block: " +
                       cfg.f_output_empty_block.ToString();}},
            {"O=|output-tags=", "output tag-name.",
                (string v) =>{
                cfg.tags_output = parse_tags(v, cfg.tags_output);
                msg += "\nspecified output tags: " +
                       String.Join(",", cfg.tags_output);}},
            {"A=|article-tags=", "output tag-name with attribute.",
                (string v) =>{
                cfg.tags_article = parse_tags(v, cfg.tags_article);
                msg += "\nspecified output tags with attribute: " +
                       String.Join(",", cfg.tags_article);}},
            {"version", "output version string.", v => {
                ret = show_version();}}
        };

        List<string> extra = null;
        try {
            extra = suite.Parse(args);
        } catch (OptionException e) {
            Console.WriteLine(e.Message);
            ret = true;
        }
        ftop = _ftop;
        fout = _fout;
        if (ret) {
            suite.WriteOptionDescriptions(Console.Out);
            return true;
        }
        if (extra != null && extra.Count >= 1 && extra[0].Length > 0) {
            droot = extra[0];
            msg += "\nupdate search directory: " + droot;
        } else {
            msg += "\nsearch directory: " + droot;
        }
        if (extra != null && extra.Count >= 2 && extra[1].Length > 0) {
            ftop = extra[1];
            msg += "\nupdate header file: " + ftop;
        }
        if (extra != null && extra.Count >= 3 && extra[2].Length > 0) {
            fout = extra[2];
            msg += "\nupdate output file: " + fout;
        }
        if (msg != "") {
            Console.WriteLine(msg.Substring(1));
        }
        return false;
    }

    public static bool show_version() {
        var msg = String.Format("prepandoc.exe version {0}-{1}",
                                "cs1.3.0",   // version
                                "0000000");  // git-hash
        Console.WriteLine(msg);
        return true;
    }

    public static bool show_help() {
        foreach (var msg in new[] {
            "  directory:   document search directory",
            "  header-file: markdown file for the document header",
            "  output-file: file name for markdown output",
            "",
        }) {
            Console.WriteLine(msg);
        }
        return true;
    }

    public static string parse_string(string src, string _default
    ) {
        if (src == null || src.Length < 1) {
            return _default;
        }
        return src;
    }

    public static bool parse_boolean(string src, bool _default) {
        if (src == null) {
            return _default;
        }
        string[] keys;
        var s = src.Trim().ToLower();
        if (_default) {
            keys = new[] {"0", "null", "false", "neg", "no"};
        } else {
            keys = new[] {"1", "true", "pos", "yes"};
        }
        if (keys.Contains(s)) {
            return !_default;
        }
        return _default;
    }

    public static string[] parse_tags(string src, string[] _default) {
        if (src == null || src.Length < 1) {
            return _default;
        }
        var ret = new List<string>();
        var seq = src.Split(',');
        foreach (var name in seq) {
            ret.Add(name.Trim());
        }
        return ret.ToArray();
    }
}
}
// vi: ft=cs:sw=4:ts=4:et:nowrap:fdm=marker
