using Telerik.SvgIcons;

namespace OMMP.WebClient.Models;

public class DrawerItem
{
    public string Text { get; set; }
    public ISvgIcon Icon { get; set; }
    public string Url { get; set; }
    public string Group { get; set; }
}