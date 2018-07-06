///
/// Copyright (c) 2018, shimoda as kuri65536 _dot_ hot mail _dot_ com
///                     ( email address: convert _dot_ to . and joint string )
///
/// This Source Code Form is subject to the terms of the Mozilla Public License,
/// v.2.0. If a copy of the MPL was not distributed with this file,
/// You can obtain one at https://mozilla.org/MPL/2.0/.
///
using System;
using System.Text;

namespace PrePandoc {
public class Config {
    public static System.Text.Encoding enc =
            System.Text.Encoding.GetEncoding("utf-8");

    public static bool f_output_empty_block = false;

    public static string format_block_name(string name) {
        return "### " + name + "\n";
    }


    public static string format_file_name(string name) {
        return "<!-- " + name + " -->\n";
    }
}
}

// vi: ft=cs
