namespace DotNetStarterProject.Helpers;

public static class DangerLevelHelper
{
    public static string GetDangerLevel(int reportCount)
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

    public static string GetColor(int reportCount)
    {
        return reportCount switch
        {
            <= 1 => "blue",
            2 => "green",
            3 or 4 => "yellow",
            5 or 6 => "orange",
            _ => "red"
        };
    }

    public static string GetHexColor(int reportCount)
    {
        return reportCount switch
        {
            <= 1 => "#007bff", // Blue
            2 => "#28a745", // Green
            3 or 4 => "#ffc107", // Yellow
            5 or 6 => "#fd7e14", // Orange
            _ => "#dc3545"  // Red
        };
    }

    public static string GetBootstrapClass(int reportCount)
    {
        return reportCount switch
        {
            <= 1 => "info",
            2 => "success",
            3 or 4 => "warning",
            5 or 6 => "warning", // Bootstrap warning is yellow/orange
            _ => "danger"
        };
    }
}
