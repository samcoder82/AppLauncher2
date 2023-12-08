namespace AppLauncher.Configuration;

/**
 * <summary>The Configured Application Model</summary>
 */
public class App
{
    
    /**
     * <summary>The Configured Application Name</summary>
     */
    public string? Name { get; set; }
    
    /**
     * <summary>The Configured Application Category</summary>
     */
    public string? Category { get; set; }
    
    /**
     * <summary>The Configured Application Path</summary>
     */
    public string? Path { get; set; }
    
    /**
     * <summary>The Configured Application Command Line Arguments</summary>
     */
    public string? Args { get; set; }
}