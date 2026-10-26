namespace CrossOptimizerAPI.Models;

public class OptimizationStats
{
    public int FilesDeleted { get; set; }
    public int FoldersDeleted { get; set; }
    public int AppsRemoved { get; set; }
    public int ServicesDisabled { get; set; }
    public int RegistryTweaksApplied { get; set; }
}