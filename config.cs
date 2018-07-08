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
using System.Linq;
using System.Collections.Generic;

using Log = PrePandoc.logging;

/// <page>2</page>
namespace PrePandoc {
/// <remarks> class Config
/// ---
/// you can customize the behavior of this tools by
/// editing this class.
///
/// </remarks>
public class Config {
    /// <summary a="1">- specify the input file encoding.
    /// </summary>
    public static System.Text.Encoding enc =
            System.Text.Encoding.GetEncoding("utf-8");

    /// <summary a="1">- do not output the empty comment block to markdown.
    /// </summary>
    public static bool f_output_empty_block = false;
    /// <summary a="1">- specify markdown CSS file name.
    /// </summary>
    public static string css_file_name = "swiss.css";
    /// <summary a="1">- specify XML-tags to output markdown file.
    /// </summary>
    public static string[] tags_output = new[] {
        "remarks"};
    /// <summary a="1">
    /// - output the tag which have attribute 'article' in `tag_article` .
    /// </summary>
    public static string[] tags_article = new[] {
        "summary"};
    /// <summary a="1">- attribute name for `tags_article` .
    /// </summary>
    public static string attr_article = "a";

    /// <summary a="1"><!-- format_block_name {{{1 -->
    /// - function to format the block name in markdown
    /// </summary>
    public static string format_block_name(string name) {
        return "";  // "### " + name + "\n";
    }

    /// <summary a="1"><!-- format_file_name {{{1 -->
    /// - function to format the file name in markdown
    /// </summary>
    public static string format_file_name(string name) {
        return "<!-- " + name + " -->\n";
    }

    /// <summary a="1"><!-- filter_file_name {{{1 -->
    /// - function to specify the filtering of source file names.
    /// </summary>
    public static bool filter_file_name(string name) {
        if (!name.EndsWith(".cs")) {
            return true;
        }
        if (name.Contains("Designer.cs")) {
            return true;
        }
        return false;
    }

    /// <summary><!-- sort_files {{{1 -->
    /// - function to specify the order of the source files.
    /// </summary>
    public static List<string> sort_files(List<string> seq) {
        var ret1 = new SortedDictionary<int, string>();
        var ret2 = new List<string>();

        foreach (var fname in seq) {
            var n = extract_order_from_file(fname);
            if (!n.HasValue) {
                ret2.Add(fname);
                continue;
            }
            var n2 = n.Value * 10000;
            while (ret1.ContainsKey(n2)) {
                Log.warn("order {1} was already specified for {0}, " +
                         "re-order it...", ret1[n2], n.Value);
                n2 += 1;
            }
            ret1.Add(n2, fname);
        }
        var ret3 = ret1.Values.ToList().Concat(ret2).ToList();
        return ret3;
    }

    /// <summary><!-- extract_order_from_file -->
    /// - function to extract the orders of the source files.
    /// </summary>
    private static int? extract_order_from_file(string fname) {
        String txt;
        try {
            txt = System.IO.File.ReadAllText(fname);
        } catch (Exception) {
            return null;
        }
        var n = txt.IndexOf("<" + "page>");
        if (n == -1) {
            return null;
        }
        Log.debg("page-order: page-tag detected: {0} in {1}", n, fname);
        txt = txt.Substring(n + 6);
        n = txt.IndexOf("<" + "/page>");
        if (n == -1) {
            return null;
        }
        txt = txt.Substring(0, n);
        Log.debg("page-order: /page detected: at {0} => {1}", n, txt);
        if (!Int32.TryParse(txt, out n)) {
            Log.eror("page-order: can't parse number: {0} in {1}, check it",
                     txt, fname);
            return null;
        }
        Log.info("page-order: extracted {0} for {1}.", n, fname);
        return n;
    }
}
}

// vi: ft=cs:tw=4:ts=4:et:nowrap:fdm=marker
