
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;
using LiteWS;


public partial class FileServer
{
    public LiteWS.GenHTML HTMLPageDirIndex(LiteWS.Client client, string dir)
    {
        string dpath;
        var files = Utils.GetSortedFiles(dir, m_rootdir);
        var gh = new LiteWS.GenHTML(true);
        dpath = MakeRelativePath(dir);
        gh.Builder.Append("<!DOCTYPE html>\n");
        gh.t("html", () =>
        {
            gh.t("head", () =>
            {
                gh.t("title", String.Format("Index of {0}", dpath));
                var css = new LiteWS.GenCSS(gh.IndentationLevel);
                css.Selector("table>thead>tr#flisthead", ()=>
                {
                    css.Attrib("color", "red");
                });
                css.Selector("tr#flistparentdir", () =>
                {
                    css.Attrib("background-color", "#eee");
                });
                gh.t("style", gh.attr("type", "text/css"), css.ToString());
                /*gh.t("style", gh.attr("type", "text/css"), () =>
                {
                    gh.AppendString(css.ToString());
                });
                */
            });
            gh.t("body", () =>
            {
                gh.t("h2", string.Format("Index of {0}", dpath));
                gh.t("div", gh.attr("id", "flistbox"), () =>
                {
                    gh.t("table", () =>
                    {
                        gh.t("thead", () =>
                        {
                            gh.t("tr", gh.attr("id", "flisthead"), ()=>
                            {
                                gh.t("th", "Type");
                                gh.t("th", "Name");
                                gh.t("th", "Size");
                                gh.t("th", "Last Changed");
                            });
                        });
                        gh.t("tbody", () =>
                        {
                            if(dpath != "/")
                            {
                                gh.t("tr", gh.attr("id", "flistparentdir"), () =>
                                {
                                    gh.t("td", () =>
                                    {
                                        gh.t("span", "-");
                                    });
                                    gh.t("td", () =>
                                    {
                                        gh.t("a", gh.attr("href", ".."), "[parent directory]");
                                    });
                                    gh.t("td", () =>
                                    {
                                        gh.t("span", "-");
                                    });
                                });
                            }
                            foreach(var fi in files)
                            {
                                gh.t("tr", gh.attr("class", "flistitem"), () =>
                                {
                                    // what kind of item it is:
                                    // directories: "[dir]"
                                    // files: "[file]"
                                    // todo: replace with an icon, perhaps?
                                    gh.t("td", gh.attr("class", "indextype"), () =>
                                    {
                                        gh.t("div", fi.ItemShortDescription());
                                    });
                                    // the filename, which is also a clickable url
                                    gh.t("td", () =>
                                    {
                                        gh.t("div", gh.attr("class", "indexfilename"), () =>
                                        {
                                            gh.t("a", gh.attr("href", fi.ItemURL()), fi.FormattedName());
                                        });
                                    });
                                    // the filesize
                                    gh.t("td", gh.attr("class", "indexsize"), () =>
                                    {
                                        gh.t("div", fi.FormattedSize());
                                    });
                                    // the last time this item was changed
                                    gh.t("td", gh.attr("class", "indexlastchanged"), () =>
                                    {
                                        gh.t("div", fi.FormattedLastChanged());
                                    });
                                });
                            }
                        });
                    });
                });
                gh.t("hr");
                gh.t("address", ()=>
                {
                    gh.t("div", "powered by litews v1.0");
                });
            });
        });
        return gh;
    }
}