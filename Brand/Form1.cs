using HtmlAgilityPack;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace Brand
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public static List<string> ExtractLinksWithHtmlAgilityPack(string html)
        {
            var links = new List<string>();

            try
            {
                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(html);

                // 获取所有<a>标签
                var anchorNodes = htmlDoc.DocumentNode.SelectNodes("//a[@href]");

                if (anchorNodes != null)
                {
                    foreach (HtmlNode link in anchorNodes)
                    {
                        string href = link.GetAttributeValue("href", "");
                        if (!string.IsNullOrEmpty(href))
                        {
                            links.Add(href);
                        }
                    }
                }

                // 也可以获取其他可能包含链接的元素，如<area>, <link>, <base>等
                var areaNodes = htmlDoc.DocumentNode.SelectNodes("//area[@href]");
                if (areaNodes != null)
                {
                    foreach (HtmlNode link in areaNodes)
                    {
                        string href = link.GetAttributeValue("href", "");
                        if (!string.IsNullOrEmpty(href))
                        {
                            links.Add(href);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"解析HTML时出错: {ex.Message}");
            }

            return links;
        }

        public static string HtmlGetTitleCmpContent(string htmlContent)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlContent);

            // 提取描述
            var descriptionNode = doc.DocumentNode.SelectSingleNode("//meta[@name='description']");

            if (descriptionNode != null)
            {
                return descriptionNode.GetAttributeValue("content", "");
            }

            return "";
        }

        private static string HtmlGetTitleCmpName(string htmlContent)
        {
            // 加载 HTML 文档
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlContent);

            // 提取 <title> 标签内容
            var titleNode = doc.DocumentNode.SelectSingleNode("//title");
            if (titleNode != null)
            {
                string titleText = titleNode.InnerText;
                // 提取括号内的品牌名称（如 UMW(友台半导体)）
                int startBracket = titleText.IndexOf('【');
                int endBracket = titleText.IndexOf('】');

                if (startBracket >= 0 && endBracket > startBracket)
                {
                    string brandText = titleText.Substring(
                        startBracket + 1,
                        endBracket - startBracket - 1
                    );
                    return brandText;
                }
            }
            return "";
        }

        private StringBuilder gOutTxt = new();
        private string[] gBrandUrl;
        private static UInt32 gBrandPos;

        private string BrandGetNextUrl()
        {
            progressBar1.Value = (int)((gBrandPos * 100) / gBrandUrl.Length);
            try
            {
                while (gBrandPos < gBrandUrl.Length)
                {
                    var url = gBrandUrl[gBrandPos];
                    gBrandPos++;
                    if (url.StartsWith("https://list.szlcsc.com/brand/", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (url.IndexOf("?") != -1)
                            url = url.Substring(0, url.IndexOf("?"));
                        Debug.WriteLine($"网页:{url}");
                        return url;
                    }
                }
            }
            catch { }
            timer1.Enabled = true;
            timer1.Stop();
            timer1.Interval = 40000;
            timer1.Start();
            progressBar1.Value = 100;
            return "";
        }

        private async void webView2_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            Microsoft.Web.WebView2.WinForms.WebView2 wv = (Microsoft.Web.WebView2.WinForms.WebView2)sender;
            if (e.IsSuccess)
            {
                string htmlContent = await wv.CoreWebView2.ExecuteScriptAsync("document.body.innerText");
                string html = JsonConvert.DeserializeObject<string>(htmlContent) ?? "";
                html = html.Substring(html.IndexOf("<html"));
                //File.WriteAllText("brand.html", html);
                var txtCmpName = HtmlGetTitleCmpName(html);
                var txtCmpCnt = HtmlGetTitleCmpContent(html);
                var url = wv.Source.ToString();
                if (url.IndexOf("?") != -1)
                    url = url.Substring(0, url.IndexOf("?"));
                gOutTxt.AppendLine($"\"{url}\",\"{txtCmpName}\",\"{txtCmpCnt}\"");

                await wv.CoreWebView2.Profile.ClearBrowsingDataAsync();
                url = BrandGetNextUrl();
                await Task.Delay(50);
                if (string.IsNullOrWhiteSpace(url) == false)
                    wv.Source = new Uri(@"view-source:" + url);
            }
            else
            {
                await Task.Delay(100);
                wv.Reload();
            }
            GC.Collect();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            gOutTxt.AppendLine("url,name,Content");
        }

        private async void webView2First_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                string htmlContent = await webView2First.CoreWebView2.ExecuteScriptAsync("document.documentElement.outerHTML");
                string html = JsonConvert.DeserializeObject<string>(htmlContent) ?? "";
                //File.WriteAllText("brand.html", html);
                gBrandUrl = ExtractLinksWithHtmlAgilityPack(html).ToArray();
                webView2First.Visible = false;

                await webView21.EnsureCoreWebView2Async();
                webView21.CoreWebView2.Settings.IsScriptEnabled = false;
                webView21.CoreWebView2.Settings.IsStatusBarEnabled = false;
                webView21.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                webView21.CoreWebView2.Settings.AreDevToolsEnabled = false;
                webView21.ZoomFactor = 0.3;

                await webView22.EnsureCoreWebView2Async();
                webView22.CoreWebView2.Settings.IsScriptEnabled = false;
                webView22.CoreWebView2.Settings.IsStatusBarEnabled = false;
                webView22.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                webView22.CoreWebView2.Settings.AreDevToolsEnabled = false;
                webView22.ZoomFactor = 0.3;

                await webView23.EnsureCoreWebView2Async();
                webView23.CoreWebView2.Settings.IsScriptEnabled = false;
                webView23.CoreWebView2.Settings.IsStatusBarEnabled = false;
                webView23.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                webView23.CoreWebView2.Settings.AreDevToolsEnabled = false;
                webView23.ZoomFactor = 0.3;

                await webView24.EnsureCoreWebView2Async();
                webView24.CoreWebView2.Settings.IsScriptEnabled = false;
                webView24.CoreWebView2.Settings.IsStatusBarEnabled = false;
                webView24.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                webView24.CoreWebView2.Settings.AreDevToolsEnabled = false;
                webView24.ZoomFactor = 0.3;

                await webView25.EnsureCoreWebView2Async();
                webView25.CoreWebView2.Settings.IsScriptEnabled = false;
                webView25.CoreWebView2.Settings.IsStatusBarEnabled = false;
                webView25.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                webView25.CoreWebView2.Settings.AreDevToolsEnabled = false;
                webView25.ZoomFactor = 0.3;

                await webView26.EnsureCoreWebView2Async();
                webView26.CoreWebView2.Settings.IsScriptEnabled = false;
                webView26.CoreWebView2.Settings.IsStatusBarEnabled = false;
                webView26.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                webView26.CoreWebView2.Settings.AreDevToolsEnabled = false;
                webView26.ZoomFactor = 0.3;

                await webView27.EnsureCoreWebView2Async();
                webView27.CoreWebView2.Settings.IsScriptEnabled = false;
                webView27.CoreWebView2.Settings.IsStatusBarEnabled = false;
                webView27.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                webView27.CoreWebView2.Settings.AreDevToolsEnabled = false;
                webView27.ZoomFactor = 0.3;

                await webView28.EnsureCoreWebView2Async();
                webView28.CoreWebView2.Settings.IsScriptEnabled = false;
                webView28.CoreWebView2.Settings.IsStatusBarEnabled = false;
                webView28.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                webView28.CoreWebView2.Settings.AreDevToolsEnabled = false;
                webView28.ZoomFactor = 0.3;

                await webView29.EnsureCoreWebView2Async();
                webView29.CoreWebView2.Settings.IsScriptEnabled = false;
                webView29.CoreWebView2.Settings.IsStatusBarEnabled = false;
                webView29.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                webView29.CoreWebView2.Settings.AreDevToolsEnabled = false;
                webView29.ZoomFactor = 0.3;

                await webView210.EnsureCoreWebView2Async();
                webView210.CoreWebView2.Settings.IsScriptEnabled = false;
                webView210.CoreWebView2.Settings.IsStatusBarEnabled = false;
                webView210.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                webView210.CoreWebView2.Settings.AreDevToolsEnabled = false;
                webView210.ZoomFactor = 0.3;

                await webView211.EnsureCoreWebView2Async();
                webView211.CoreWebView2.Settings.IsScriptEnabled = false;
                webView211.CoreWebView2.Settings.IsStatusBarEnabled = false;
                webView211.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                webView211.CoreWebView2.Settings.AreDevToolsEnabled = false;
                webView211.ZoomFactor = 0.3;

                await webView212.EnsureCoreWebView2Async();
                webView212.CoreWebView2.Settings.IsScriptEnabled = false;
                webView212.CoreWebView2.Settings.IsStatusBarEnabled = false;
                webView212.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                webView212.CoreWebView2.Settings.AreDevToolsEnabled = false;
                webView212.ZoomFactor = 0.3;

                await webView213.EnsureCoreWebView2Async();
                webView213.CoreWebView2.Settings.IsScriptEnabled = false;
                webView213.CoreWebView2.Settings.IsStatusBarEnabled = false;
                webView213.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                webView213.CoreWebView2.Settings.AreDevToolsEnabled = false;
                webView213.ZoomFactor = 0.3;

                await webView214.EnsureCoreWebView2Async();
                webView214.CoreWebView2.Settings.IsScriptEnabled = false;
                webView214.CoreWebView2.Settings.IsStatusBarEnabled = false;
                webView214.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                webView214.CoreWebView2.Settings.AreDevToolsEnabled = false;
                webView214.ZoomFactor = 0.3;

                await webView215.EnsureCoreWebView2Async();
                webView215.CoreWebView2.Settings.IsScriptEnabled = false;
                webView215.CoreWebView2.Settings.IsStatusBarEnabled = false;
                webView215.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                webView215.CoreWebView2.Settings.AreDevToolsEnabled = false;
                webView215.ZoomFactor = 0.3;

                await webView216.EnsureCoreWebView2Async();
                webView216.CoreWebView2.Settings.IsScriptEnabled = false;
                webView216.CoreWebView2.Settings.IsStatusBarEnabled = false;
                webView216.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                webView216.CoreWebView2.Settings.AreDevToolsEnabled = false;
                webView216.ZoomFactor = 0.3;

                await webView217.EnsureCoreWebView2Async();
                webView217.CoreWebView2.Settings.IsScriptEnabled = false;
                webView217.CoreWebView2.Settings.IsStatusBarEnabled = false;
                webView217.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                webView217.CoreWebView2.Settings.AreDevToolsEnabled = false;
                webView217.ZoomFactor = 0.3;

                await webView218.EnsureCoreWebView2Async();
                webView218.CoreWebView2.Settings.IsScriptEnabled = false;
                webView218.CoreWebView2.Settings.IsStatusBarEnabled = false;
                webView218.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                webView218.CoreWebView2.Settings.AreDevToolsEnabled = false;
                webView218.ZoomFactor = 0.3;

                await webView219.EnsureCoreWebView2Async();
                webView219.CoreWebView2.Settings.IsScriptEnabled = false;
                webView219.CoreWebView2.Settings.IsStatusBarEnabled = false;
                webView219.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                webView219.CoreWebView2.Settings.AreDevToolsEnabled = false;
                webView219.ZoomFactor = 0.3;

                await webView220.EnsureCoreWebView2Async();
                webView220.CoreWebView2.Settings.IsScriptEnabled = false;
                webView220.CoreWebView2.Settings.IsStatusBarEnabled = false;
                webView220.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                webView220.CoreWebView2.Settings.AreDevToolsEnabled = false;
                webView220.ZoomFactor = 0.3;

                webView21.Source = new Uri(@"view-source:" + BrandGetNextUrl());
                webView22.Source = new Uri(@"view-source:" + BrandGetNextUrl());
                webView23.Source = new Uri(@"view-source:" + BrandGetNextUrl());
                webView24.Source = new Uri(@"view-source:" + BrandGetNextUrl());
                webView25.Source = new Uri(@"view-source:" + BrandGetNextUrl());
                webView26.Source = new Uri(@"view-source:" + BrandGetNextUrl());
                webView27.Source = new Uri(@"view-source:" + BrandGetNextUrl());
                webView28.Source = new Uri(@"view-source:" + BrandGetNextUrl());
                webView29.Source = new Uri(@"view-source:" + BrandGetNextUrl());
                webView210.Source = new Uri(@"view-source:" + BrandGetNextUrl());
                webView211.Source = new Uri(@"view-source:" + BrandGetNextUrl());
                webView212.Source = new Uri(@"view-source:" + BrandGetNextUrl());
                webView213.Source = new Uri(@"view-source:" + BrandGetNextUrl());
                webView214.Source = new Uri(@"view-source:" + BrandGetNextUrl());
                webView215.Source = new Uri(@"view-source:" + BrandGetNextUrl());
                webView216.Source = new Uri(@"view-source:" + BrandGetNextUrl());
                webView217.Source = new Uri(@"view-source:" + BrandGetNextUrl());
                webView218.Source = new Uri(@"view-source:" + BrandGetNextUrl());
                webView219.Source = new Uri(@"view-source:" + BrandGetNextUrl());
                webView220.Source = new Uri(@"view-source:" + BrandGetNextUrl());
                webView2First.Dispose();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            File.WriteAllText("BrandInfo.csv", gOutTxt.ToString());
            MessageBox.Show("数据爬取完成", "停止", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}
