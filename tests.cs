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
}
}
// vi: ft=cs:sw=4:ts=4:et:nowrap:fdm=marker
