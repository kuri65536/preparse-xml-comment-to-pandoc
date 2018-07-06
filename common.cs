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
using System.Collections.Generic;
using System.Xml;


namespace PrePandoc {
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


/// <summary> <!-- parse_attributes {{{1 -->
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
}
// vi: ft=cs:sw=4:ts=4:et:nowrap:fdm=marker
