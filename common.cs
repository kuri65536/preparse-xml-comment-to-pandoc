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
    public static int __level__ = 40;
    static readonly int VERB = 10;
    static readonly int DEBG = 20;
    static readonly int INFO = 30;
    static readonly int WARN = 40;
    static readonly int EROR = 50;
    static readonly int CRIT = 60;

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
}


public delegate void FuncXmlParseStart(
        string name, Dictionary<string, string> attrs);
public delegate void FuncXmlParseEnd(string name);
public delegate void FuncXmlParseData(string data);


public class XmlParser {
    public FuncXmlParseStart StartElementHandler;
    public FuncXmlParseEnd EndElementHandler;
    public FuncXmlParseData CharacterDataHandler;

    public void Parse(string src) {
        var strreader = new StringReader(src);
        this.Parse(strreader);
    }

    public void Parse(TextReader src) {
        var reader = XmlReader.Create(src);

        reader.MoveToContent();
        // Parse the file and display each of the nodes.
        while (reader.Read())  {
            switch (reader.NodeType)  {
                case XmlNodeType.Element:
                    var d = parse_attributes(reader);
                    this.StartElementHandler(reader.Name, d);
                    break;
                case XmlNodeType.EndElement:
                    this.EndElementHandler(reader.Name);
                    break;
                case XmlNodeType.Whitespace:
                    var data = reader.ReadString();
                    this.CharacterDataHandler(data);
                    break;
                case XmlNodeType.Text:
                    data = reader.ReadString();
                    this.CharacterDataHandler(data);
                    break;
            }
        }
        reader.Close();
        src.Close();
    }

    public Dictionary<string, string> parse_attributes(
            XmlReader reader
    ) {
        var ret = new Dictionary<string, string>();
        for (int i = 0; i < reader.AttributeCount; i++) {
            reader.MoveToAttribute(i);
            ret.Add(reader.Name, reader.Value);
        }
        return ret;
    }
}
}
// vi: ft=cs:sw=4:ts=4:et:nowrap:fdm=marker
