using System.ComponentModel;

namespace BookfetSystem.Services.Enum
{
    public enum PostBlockType
    {
        [Description("Heading 1")]
        Heading1 = 1,

        [Description("Heading 2")]
        Heading2 = 2,

        [Description("Content")]
        Content = 3,

        [Description("Image")]
        Image = 4,

        [Description("Quote")]
        Quote = 5
    }
}
