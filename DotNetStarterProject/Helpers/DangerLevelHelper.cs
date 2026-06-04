namespace DotNetStarterProject.Helpers;

public static class DangerLevelHelper
{
    public static int CalculateLevel(string categoryString)
    {
        if (string.IsNullOrEmpty(categoryString)) return 1;

        var categories = categoryString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                     .Select(c => c.Trim());
        
        int score = 0;
        foreach (var category in categories)
        {
            score += category switch
            {
                "Flood" => 3,
                "Crossing" => 2,
                "HeavyTraffic" => 2,
                "NightRisk" => 2,
                "NoSignal" => 2,
                "NoSidewalk" => 1,
                "Construction" => 1,
                _ => 0
            };
        }

        return score switch
        {
            <= 1 => 1,
            2 => 2,
            3 => 3,
            4 or 5 => 4,
            _ => 5
        };
    }

    public static string GetDangerLevel(int level)
    {
        return level switch
        {
            1 => "軽微",
            2 => "注意",
            3 => "危険",
            4 => "非常に危険",
            _ => "回避推奨"
        };
    }

    public static string GetBootstrapClass(int level)
    {
        return level switch
        {
            1 => "info",
            2 => "success",
            3 => "warning",
            4 => "warning",
            _ => "danger"
        };
    }

    // Keep existing methods if needed for reportCount display or rename them
    public static string GetDangerLevelFromReportCount(int reportCount)
    {
        return reportCount switch
        {
            <= 1 => "軽微",
            2 => "注意",
            3 or 4 => "危険",
            5 or 6 => "非常に危険",
            _ => "回避推奨"
        };
    }

    public static string GetBootstrapClassFromReportCount(int reportCount)
    {
        return reportCount switch
        {
            <= 1 => "info",
            2 => "success",
            3 or 4 => "warning",
            5 or 6 => "warning",
            _ => "danger"
        };
    }
}
