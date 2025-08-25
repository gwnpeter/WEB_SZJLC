using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Brand
{
    internal class CouponData
    {
        public string Amount { get; set; }
        public string Condition { get; set; }
        public string Description { get; set; }
    }

    public class BrandInfo
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
    }

    public class BrandDataStore
    {
        private readonly List<BrandInfo> _brands = new List<BrandInfo>();
        private readonly Dictionary<string, List<BrandInfo>> _nameIndex = new Dictionary<string, List<BrandInfo>>(StringComparer.OrdinalIgnoreCase);

        public int Count => _brands.Count;

        public BrandDataStore(string csvFilePath)
        {
            LoadCsvData(csvFilePath);
            BuildNameIndex();
        }

        private void LoadCsvData(string csvFilePath)
        {
            if (!File.Exists(csvFilePath))
                throw new FileNotFoundException("CSV file not found", csvFilePath);

            using (TextFieldParser parser = new TextFieldParser(csvFilePath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;
                parser.TrimWhiteSpace = false;

                // 跳过标题行
                if (!parser.EndOfData)
                    parser.ReadFields();

                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    if (fields.Length < 3) continue;

                    var brand = new BrandInfo
                    {
                        Url = fields[0].Trim('"'),
                        Name = fields[1].Trim('"'),
                        Content = fields[2].Trim('"')
                    };

                    _brands.Add(brand);
                }
            }

            Console.WriteLine($"Loaded {_brands.Count} brand records");
        }

        private void BuildNameIndex()
        {
            foreach (var brand in _brands)
            {
                var normalized = brand.Name.ToLowerInvariant();
                if (!_nameIndex.ContainsKey(normalized))
                {
                    _nameIndex[normalized] = new List<BrandInfo>();
                }
                _nameIndex[normalized].Add(brand);
            }

            Console.WriteLine($"Built index for {_nameIndex.Count} unique brand names");
        }

        // 方法1：精确匹配（使用字典索引）
        public List<BrandInfo> FindByNameExact(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return new List<BrandInfo>();

            return _nameIndex.TryGetValue(name.ToLowerInvariant(), out var brands)
                ? brands
                : new List<BrandInfo>();
        }

        // 方法2：模糊匹配（包含关系）
        public List<BrandInfo> FindByNameContains(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new List<BrandInfo>();

            return _brands
                .Where(b => b.Name.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }

        // 方法3：首字母匹配（提高搜索性能）
        public List<BrandInfo> FindByPrefix(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                return new List<BrandInfo>();

            return _brands
                .Where(b => b.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            webView2First.Source = new Uri(@"view-source:https://www.szlcsc.com/huodong.html");
        }

        private static string CleanText(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // 替换HTML实体
            input = input.Replace("&lt;", "<")
                        .Replace("&gt;", ">")
                        .Replace("&amp;", "&");

            // 移除多余空格和换行
            return Regex.Replace(input, @"\s+", " ").Trim();
        }

        private async void webView2First_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                string result = await webView2First.ExecuteScriptAsync("document.body.innerText;");
                string html = JsonConvert.DeserializeObject<string>(result) ?? "";
                html = html.Substring(html.IndexOf("<html"));
                //File.WriteAllText("huodong.html", html);

                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                // 选择目标节点
                var targetNode = doc.DocumentNode.SelectSingleNode("//*[@id='样品券专区-content']");
                var coupons = new List<CouponData>();

                if (targetNode != null)
                {
                    // 在目标节点内选择所有<li>节点
                    var liNodes = targetNode.SelectNodes(".//li[contains(@class, 'sc-d5f310fd-3')]");

                    if (liNodes != null)
                    {
                        foreach (var li in liNodes)
                        {
                            var coupon = new CouponData();

                            // 提取左侧金额和条件
                            var leftDiv = li.SelectSingleNode(".//div[contains(@class, 'flex flex-col items-center')]");
                            if (leftDiv != null)
                            {
                                // 提取金额（大数字）
                                var amountNode = leftDiv.SelectSingleNode(".//div[contains(@class, 'text-[30px]')]");
                                if (amountNode != null)
                                {
                                    coupon.Amount = CleanText(amountNode.InnerText);
                                }

                                // 提取使用条件
                                var conditionNode = leftDiv.SelectSingleNode(".//div[contains(., '满') and contains(., '可')]");
                                if (conditionNode != null)
                                {
                                    coupon.Condition = CleanText(conditionNode.InnerText);
                                }
                            }

                            // 提取右侧描述
                            var rightDiv = li.SelectSingleNode(".//div[contains(@class, 'flex-1 flex text-[14px] items-center')]");
                            if (rightDiv != null)
                            {
                                // 提取描述
                                var descNode = rightDiv.SelectSingleNode(".//div[contains(@class, 'text-[#333333]') and contains(@class, 'text-[16px]')]");
                                if (descNode != null)
                                {
                                    coupon.Description = CleanText(descNode.InnerText);
                                }
                            }

                            coupons.Add(coupon);
                        }

                        StringBuilder outTxt = new();
                        outTxt.AppendLine("金额,条件,描述,网址,品类");
                        var store = new BrandDataStore(@"BrandInfo.csv");
                        // 打印提取结果
                        foreach (var coupon in coupons)
                        {
                            Debug.WriteLine($"金额:{coupon.Amount},条件:{coupon.Condition},描述:{coupon.Description}");
                            var txtName = coupon.Description.Replace("品牌券", "").Trim();
                            if (txtName.StartsWith("<"))
                                txtName = txtName.Substring(txtName.IndexOf(">") + 1);
                            var matches = store.FindByNameContains(txtName);
                            if (matches.Count >= 1)
                            {
                                var info = matches[0].Content;
                                try
                                {
                                    info = info.Substring(info.IndexOf("最新价格和库存查询") + 10);
                                    info = info.Substring(0, info.IndexOf("，购买"));
                                }
                                catch
                                { }
                                outTxt.AppendLine($"\"{coupon.Amount}\",\"{coupon.Condition}\",\"{coupon.Description}\",\"{matches[0].Url}\",\"{info}\"");
                            }
                            else
                                outTxt.AppendLine($"{coupon.Amount},{coupon.Condition},{coupon.Description}");
                        }
                        File.WriteAllText("HuoDong.csv", outTxt.ToString());
                        MessageBox.Show("获取优惠券信息完成", "关闭", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                    }
                    else
                    {
                        Console.WriteLine("目标区域内未找到<li>节点");
                    }
                }
                else
                {
                    Console.WriteLine("未找到样品券专区-content节点");
                }
            }
        }
    }
}
