///
/// Copyright (c) 2018, shimoda as kuri65536 _dot_ hot mail _dot_ com
///                     ( email address: convert _dot_ to . and joint string )
///
/// This Source Code Form is subject to the terms of the Mozilla Public License,
/// v.2.0. If a copy of the MPL was not distributed with this file,
/// You can obtain one at https://mozilla.org/MPL/2.0/.
///
using System;


namespace PrePandoc {
public static class logging {
    public static void crit(string fmt, params object[] args) {
    }

    public static void debg(string fmt, params object[] args) {
    }

    public static void eror(string fmt, params object[] args) {
    }

    public static void dump_stack(string src) {
    }
}

public static class TextFile {
    static string fname;

    public static void open(string filename) {
        fname = filename;
    }

    public static string print(string fmt, params object[] args) {
        var src = String.Format(fmt, args);
        return src;
    }
}
}
// vi: ft=cs:sw=4:ts=4:et:nowrap:fdm=marker
