
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LiteWS
{
    public class GenCSS
    {
        private StringBuilder m_buf;
        private int m_indentlevel;

        public GenCSS(int indentlevel=0)
        {
            m_buf = new StringBuilder();
            m_indentlevel = indentlevel;
        }

        private void WriteIndent(int tolevel)
        {
            int i;
            for(i=0; i<tolevel; i++)
            {
                m_buf.Append("    ");
            }
        }

        private void WriteIndent()
        {
            WriteIndent(m_indentlevel);
        }

        private void BeginIndent()
        {
            m_indentlevel++;
            WriteIndent();
        }

        private void EndIndent()
        {
            m_indentlevel--;
        }

        public void Attrib(string attrname, string attrval)
        {
            BeginIndent();
            m_buf.AppendFormat("{0}: {1};\n", attrname, attrval);
            EndIndent();
        }

        public void Selector(string selector, Action cb)
        {
            BeginIndent();
            m_buf.Append(selector);
            m_buf.Append(" {\n");
            cb();
            WriteIndent();
            m_buf.Append("}\n");
            EndIndent();
        }

        public override string ToString()
        {
            return m_buf.ToString();
        }
    }

    public class GenHTML
    {
        public class AttribList: Dictionary<string, string>
        {
            public AttribList attr(string k, string v)
            {
                Add(k, v);
                return this;
            }
        }

        public delegate void TAddTag               (string tn, AttribList attr, Action cb);
        public delegate void TAddTagNoAttribs      (string tn, Action cb);
        public delegate void TAddTagNoAction       (string tn, AttribList attribs);
        public delegate void TAddTagStrAttrContent (string tn, AttribList attribs, string content);
        public delegate void TAddTagStrContent     (string tn, string content);

        private StringBuilder m_buf;
        private bool m_doindent;
        private int m_indcnt;

        public int IndentationLevel  { get{ return m_indcnt; } }
        public StringBuilder Builder { get{ return m_buf; } }

        public GenHTML(bool do_indent=false)
        {
            m_buf = new StringBuilder();
            m_doindent = do_indent;
            m_indcnt = 0;
        }

        private void WriteIndent(int howmuch)
        {
            int i;
            //if(howmuch > 0)
            {
                for(i=0; i!=howmuch; i++)
                {
                    m_buf.Append("    ");
                }
            }
        }

        private void BeginIndent()
        {
            if(m_doindent)
            {
                WriteIndent(m_indcnt);
                m_indcnt++;
            }
        }

        private void EndIndent()
        {
            if(m_doindent)
            {
                m_indcnt--;
            }
        }

        private void BeginTag(string tagname, AttribList attribs, bool isempty=false)
        {
            BeginIndent();
            m_buf.AppendFormat("<{0}", tagname);
            WriteAttribs(attribs);
            if(isempty)
            {
                m_buf.Append(" />");
                if(m_doindent)
                {
                    m_buf.Append("\n");
                }
                EndIndent();
            }
            else
            {
                m_buf.Append(">");
            }
        }

        private void EndTag(string tagname)
        {
            m_buf.Append("</");
            m_buf.Append(tagname);
            m_buf.Append(">");
            if(m_doindent)
            {
                m_buf.Append("\n");
            }
            EndIndent();
        }

        private void WriteAttribs(AttribList attribs)
        {
            int icount = attribs.Count;
            bool islast = false;
            if(attribs.Count > 0)
            {
                m_buf.Append(" ");
                foreach(var kvp in attribs)
                {
                    icount--;       
                    islast = (icount == 0);
                    m_buf.AppendFormat("{0}=\"{1}\"", kvp.Key, kvp.Value);
                    if(!islast)
                    {
                        m_buf.Append(" ");
                    }
                }
            }
        }

        public override string ToString()
        {
            return m_buf.ToString();
        }

        public void AppendString(string content)
        {
            m_buf.Append(content);
        }

        public void AppendTag(string tagname, AttribList attribs, Action cb)
        {
            int curlevel = m_indcnt;
            BeginTag(tagname, attribs, (cb == null));
            if(cb != null)
            {
                if(m_doindent)
                {
                    m_buf.Append("\n");
                }
                cb();
                if(m_doindent)
                {
                    WriteIndent(curlevel);
                }
                EndTag(tagname);
            }
        }

        public void AppendTag(string tagname, Action cb)
        {
            AppendTag(tagname, new AttribList(), cb);
        }

        public void AppendTag(string tagname, AttribList attribs)
        {
            AppendTag(tagname, attribs, (Action)null);
        }

        public void AppendTag(string tagname, AttribList attribs, string content)
        {
            int curlevel = m_indcnt;
            bool isempty = (String.IsNullOrEmpty(content));
            bool needsindent = (m_doindent && (content.IndexOf("\n") > -1));
            BeginTag(tagname, attribs, isempty);
            if(isempty == false)
            {
                if(needsindent)
                {
                    int i;
                    var lines = content.Replace("\r", "").Trim().Split('\n');
                    for(i=0; i<lines.Length; i++)
                    {
                        var line = lines[i].Trim();
                        WriteIndent(m_indcnt);
                        m_buf.Append(line);
                        if((i + 1) != lines.Length)
                        {
                            m_buf.Append("\n");
                        }
                    }
                    m_buf.Append("\n");
                }
                else
                {
                    m_buf.Append(content.Trim());
                }
                if(m_doindent)
                {
                    if(needsindent)
                    {
                        WriteIndent(curlevel);
                    }
                }
                EndTag(tagname);
            }
        }

        public void AppendTag(string tagname, string content)
        {
            AppendTag(tagname, new AttribList(), content);
        }

        /*************************************************
        ** there has got to be a better way to do this! **
        *************************************************/

        public AttribList attr()
        {
            return new AttribList();
        }

        public AttribList attr(string k, string v)
        {
            var att = attr();
            return att.attr(k, v);
        }

        public AttribList attr(object[][] obs)
        {
            int i;
            var att = attr();
            for(i=0; i<obs.Length; i++)
            {
                att.Add(obs[i][0] as string, obs[i][1] as string);
            }
            return att;
        }

        public void t(string tagname, AttribList attribs, Action cb)
        {
            AppendTag(tagname, attribs, cb);
        }

        public void t(string tagname, Action cb)
        {
            AppendTag(tagname, cb);
        }

        public void t(string tagname, AttribList attribs)
        {
            AppendTag(tagname, attribs);
        }

        public void t(string tagname, AttribList attribs, string content)
        {
            AppendTag(tagname, attribs, content);
        }

        public void t(string tagname, string content)
        {
            AppendTag(tagname, content);
        }

        public void t(string tagname)
        {
            AppendTag(tagname, "");
        }
    };
}
