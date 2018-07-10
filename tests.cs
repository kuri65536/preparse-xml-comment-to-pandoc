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
using System.Collections.Generic;
using NUnit.Framework;

namespace PrePandocTest {

/// <remarks>
/// Tests
/// ---
/// </remarks>
[TestFixture]
public class PrePandocTest {
    /// <remarks>
    /// test XmlParser
    /// : check simple data and it's counting.
    ///
    /// </remarks>
    [Test]
    public void test_xmlparser() {
        var src = String.Join("\n",
            "<all>",
            "<block name=\"block-A\">",
            "<summary>are you ok?</summary>",
            "<remarks>I'm fine.</remarks>",
            "</block>",
            "<block>",
            "</block>",
            "<block name=\"block-B\">",
            "</block>",
            "</all>"
        );
        int nb = 0, ne = 0, na = 0, nc = 0;
        var p = new PrePandoc.XmlParser();
        p.StartElementHandler =
                (string name, Dictionary<string, string> attrs) => {
            nb += 1;
            na += attrs.Values.Count;
        };
        p.EndElementHandler = (string name) => {
            ne += 1;
        };
        p.CharacterDataHandler = (string data) => {
            nc += 1;
        };
        p.Parse(src);
        Assert.AreEqual(nb, 6, "begin");
        Assert.AreEqual(ne, 6, "end");
        Assert.AreEqual(na, 2, "attributes");
        Assert.AreEqual(nc, 11, "data");  // actual result.
    }

    /// <remarks>
    /// </remarks>
    [Test]
    public void test_options() {
        string droot, ftop, fout;
        var test = new string[] {};
        PrePandoc.Options.run(test, out droot, out ftop, out fout);
        Assert.AreEqual(".", droot, "droot");
        Assert.AreEqual("source.md", ftop, "ftop");
        Assert.AreEqual("temp.md", fout, "fout");
    }

    /// <remarks>
    /// </remarks>
    [Test]
    public void test_options_no() {
        string droot, ftop, fout;
        var test = new string[] {};
        PrePandoc.Options.run(test, out droot, out ftop, out fout);
        Assert.AreEqual(".", droot, "droot");
        Assert.AreEqual("source.md", ftop, "ftop");
        Assert.AreEqual("temp.md", fout, "fout");
    }

    /// <remarks>
    /// </remarks>
    [Test]
    public void test_options_args() {
        string droot, ftop, fout;
        var test = new[] {"..", "source2.md", "temp2.md"};
        PrePandoc.Options.run(test, out droot, out ftop, out fout);
        Assert.AreEqual("..", droot, "droot");
        Assert.AreEqual("source2.md", ftop, "ftop");
        Assert.AreEqual("temp2.md", fout, "fout");
    }

    /// <remarks>
    /// </remarks>
    [Test]
    public void test_options_opts() {
        string droot, ftop, fout;
        var test = new[] {"...", "-b", "source3.md", "-o", "temp3.md",
                          "-v", "20", "--empty-block=yes",
                          "--article-tags", "param,summary"};
        PrePandoc.Options.run(test, out droot, out ftop, out fout);
        Assert.AreEqual("...", droot, "droot");
        Assert.AreEqual("source3.md", ftop, "ftop");
        Assert.AreEqual("temp3.md", fout, "fout");
        // int
        Assert.AreEqual(20, PrePandoc.logging.__level__, "level");
        // bool
        Assert.AreEqual(true, PrePandoc.Config.f_output_empty_block, "empty");
        // string[]
        Assert.AreEqual(new[] {"param", "summary"},
                        PrePandoc.Config.tags_article, "tags");
    }

    /// <remarks>
    /// </remarks>
    [Test]
    public void test_options_ng1() {
        string droot, ftop, fout;
        var test = new[] {"-v", "a"};
        var ret = PrePandoc.Options.run(test, out droot, out ftop, out fout);
        Assert.AreEqual(true, ret, "ng result");
    }

    /// <remarks>
    /// </remarks>
    [Test]
    public void test_options_ng2() {
        string droot, ftop, fout;
        var test = new[] {"", "-b", "", "-o", "",
                          "--empty-block=nogo", "--output-tags", ""};
        PrePandoc.Options.run(test, out droot, out ftop, out fout);
        Assert.AreEqual(".", droot, "droot");
        Assert.AreEqual("source.md", ftop, "ftop");
        Assert.AreEqual("temp.md", fout, "fout");
        // bool => default
        Assert.AreEqual(false, PrePandoc.Config.f_output_empty_block, "empty");
        // string[] => default
        Assert.AreEqual(new[] {"remarks"},
                        PrePandoc.Config.tags_output, "tags");
    }
}
}
// vi: ft=cs:sw=4:ts=4:et:nowrap:fdm=marker
