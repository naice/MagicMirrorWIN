using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.Factory.RSSFeed
{
    public static class GenericHtml
    {
        public const string HTML =
@"
<html>
<head>
<style> 
    body, html {{ overflow: hidden; }}
    body {{
        background-color:black; color:#afafaf;
        font-size:14pt; font-family: 'Segoe UI';
        margin-bottom: 400px;
    }}
    h1 {{
        color:white
    }}
    a {{
        color:gray
    }}
    div.center {{
        max-width: 600px;
        display: block;
        margin-left: auto;
        margin-right: auto;
    }}
</style>
</head>
<body>
<script>
function pageScroll() {{
    window.scrollBy(0,1);
    scrolldelay = setTimeout(pageScroll,200);
}}
pageScroll();
</script>
<br/>
<br/>
<br/>
<div class='center'>
{0}
</div>
</body></html>
";
    }
}
