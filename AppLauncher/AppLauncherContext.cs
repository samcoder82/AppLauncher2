using System.Diagnostics;
using System.Reflection;
using AppLauncher.Configuration;
using Microsoft.Extensions.Configuration;

namespace AppLauncher;

/**
 * <summary>The AppLauncher Context</summary>
 */
public class AppLauncherContext : ApplicationContext
{
    
    /**
     * <summary>The Notify Icon</summary>
     */
    private readonly NotifyIcon _notifyIcon = null!;

    /**
     * <summary>The Context Menu</summary>
     */
    private readonly ContextMenuStrip _contextMenu = null!;

    /**
     * <summary>The Exit MenuItem</summary>
     */
    private readonly ToolStripMenuItem _exitMenuItem = null!;

    /**
     * <summary>The Configuration</summary>
     */
    private readonly AppsConfig? _configuration;
    
    /**
     * <summary>Initialize AppLauncher Context</summary>
     * <remarks>
     * This is a very simple application.  All this really has to do is read the configuration for
     * a list of all applications which we might want to launch.  With this list we are constructing
     * a context menu which will be posted on the Notify Icon.
     *
     * Since this will simply read from the configuration file, there really isn't much need for a user interface
     * to configure the available applications.
     * </remarks>
     */
    public AppLauncherContext()
    {
        try
        {
            // Read the configuration
            _configuration = Program.Configuration!.Get<AppsConfig>();
            if (_configuration is null)
            {
                throw new InvalidOperationException("Application must be configured");
            }
            
            // Use the icon for this application as the icon in the notify section
            var appIcon = GetAppIcon();
            _notifyIcon = new NotifyIcon
            {
                Text = "AppLauncher",
                Icon = appIcon
            };

            Application.ApplicationExit += ApplicationOnApplicationExit;
            
            _contextMenu = new ContextMenuStrip();
            _contextMenu.SuspendLayout();

            CreateToolStripMenuItem(_contextMenu, $@"Run as [{Environment.UserName}]", null!);
            
            AddAppLaunchMenus(_contextMenu);

            _exitMenuItem = CreateToolStripMenuItem(_contextMenu, "Exit", null!);
            _exitMenuItem.Click += OnExitClick;
            
            _contextMenu.ResumeLayout(false);
            _notifyIcon.ContextMenuStrip = _contextMenu;
            _notifyIcon.Visible = true;
        }
        catch (Exception caught)
        {
            Console.Error.WriteLine($"Unexpected error initializing AppLauncher Context." +
                                    $"{Environment.NewLine}{caught.Message}" +
                                    $"{Environment.NewLine}{caught.StackTrace}", caught);
            MessageBox.Show("An unexpected error has occurred and execution cannot continue.  Diagnostic information" +
                            " is sent to the standard output, if it is not obvious why this error is occurring, it may helpful" +
                            " to run this in the command prompt to see the diagnostic output.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
        }
    }

    /**
     * <summary>Fires on Exit Click</summary>
     */
    private void OnExitClick(object? sender, EventArgs e)
    {
        var choice = MessageBox.Show("Are you sure you wish to exit?", "Confirm Exit",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
        if (choice == DialogResult.Yes)
        {
            Application.Exit();
        }
    }

    /**
     * <summary>Handles Application Exit</summary>
     */
    private void ApplicationOnApplicationExit(object? sender, EventArgs e)
    {
        _notifyIcon.Visible = false;
    }

    /**
     * <summary>Get Application Icon</summary>
     */
    private static Icon GetAppIcon()
    {
        var asm = Assembly.GetExecutingAssembly();
        var asmLocation = asm.Location;
        var icon = GetAppIcon(asmLocation);
        return icon!;
    }

    /**
     * <summary>Get App Icon</summary>
     * <param name="path">The path of the app</param>
     */
    private static Icon? GetAppIcon(string path)
    {
        try
        {
            var icon = Icon.ExtractAssociatedIcon(path);
            return icon!;
        }
        catch (Exception caught)
        {
            Console.Error.WriteLine($"Unexpected error getting application icon" +
                                    $"{Environment.NewLine}{caught.Message}" +
                                    $"{Environment.NewLine}{caught.StackTrace}", caught);
            return null;
        }
    }

    /**
     * <summary>Create ToolStrip menuItem</summary>
     * <param name="parent">The Parent ContextMenu to attach new MenuItem to</param>
     * <param name="text">The MenuItem Text</param>
     * <param name="icon">The MenuItem Icon</param>
     */
    private ToolStripMenuItem CreateToolStripMenuItem(ContextMenuStrip parent, string text, Image? icon)
    {
        var menuItem = parent.Items.Add(text, icon);
        return (ToolStripMenuItem)menuItem;
    }
    
    /**
     * <summary>Create ToolStrip MenuItem</summary>
     * <param name="parent">The Parent MenuItem to attach new MenuItem to</param>
     * <param name="text">The MenuItem Text</param>
     * <param name="icon">The MenuItem Icon</param>
     */
    private ToolStripMenuItem CreateToolStripMenuItem(ToolStripMenuItem parent, string text, Image? icon)
    {
        var menuItem = parent.DropDownItems.Add(text, icon);
        return (ToolStripMenuItem)menuItem;
    }

    /**
     * <summary>Assign App MenuItem Click Event</summary>
     * <param name="menuItem">The Menu Item</param>
     * <param name="path">The App Path</param>
     * <param name="arguments">The Command Line Arguments</param>
     * <remarks>
     * Sets up a method to handle the click event, which should launch
     * the application with any configured arguments
     * </remarks>
     */
    private void AssignAppMenuItemClickEvent(ToolStripMenuItem menuItem, string path, string arguments = "")
    {
        menuItem.Click += (_, _) =>
        {
            var psi = new ProcessStartInfo
            {
                FileName = path,
                Arguments = arguments,
                WindowStyle = ProcessWindowStyle.Normal
            };
            Process.Start(psi);
        };
    }

    /**
     * <summary>Add App Launch Menus to context menu</summary>
     * <param name="parent">The Context Menu</param>
     * <remarks>
     * First we want to add uncategorized apps to the menu
     *
     * Then we want to add categorized apps to the menu.
     * Such that, each category becomes it's own sub-menu
     *
     * Everything should be sorted alphabetically
     * </remarks>
     */
    private void AddAppLaunchMenus(ContextMenuStrip parent)
    {
        try
        {
            // Add Uncategorized apps
            AddUncategorizedAppLaunchers(parent);
            
            //Get categories
            var categories = _configuration!.Apps
                .Where(a => !string.IsNullOrWhiteSpace(a.Category))
                .DistinctBy(a => a.Category)
                .OrderBy(a => a.Category)
                .Select(a => a.Category);
            foreach (var category in categories)
            {
                var categoryMenuItem = CreateToolStripMenuItem(_contextMenu, category!, null);
                AddCategorizedAppLaunchers(categoryMenuItem, category!);
            }

        }
        catch (Exception caught)
        {
            Console.Error.WriteLine($"Unexpected error adding app launch menus" +
                                    $"{Environment.NewLine}{caught.Message}" +
                                    $"{Environment.NewLine}{caught.StackTrace}", caught);
            throw;
        }
    }

    /**
     * <summary>Add Uncategorized App Launchers to Manu</summary>
     * <param name="parent">The Context Menu</param>
     */
    private void AddUncategorizedAppLaunchers(ContextMenuStrip parent)
    {
        try
        {
            var uncategorizedApps = _configuration!.Apps
                .Where(a => a.Category!.Trim() == "")
                .OrderBy(a => a.Name);
            foreach (var app in uncategorizedApps)
            {
                var icon = GetAppIcon(app.Path!);
                var appLauncher = CreateToolStripMenuItem(parent, app.Name!,icon?.ToBitmap());
                AssignAppMenuItemClickEvent(appLauncher, app.Path!, app.Args!);
            }
        }
        catch (Exception caught)
        {
            Console.Error.WriteLine($"Unexpected error adding uncategorized app launchers" +
                                    $"{Environment.NewLine}{caught.Message}" +
                                    $"{Environment.NewLine}{caught.StackTrace}", caught);
            throw;
        }
    }
    
    /**
     * <summary>Add Categorized App Launchers</summary>
     * <param name="parent">The Category MenuItem</param>
     * <param name="category">The Category Name</param>
     */
    private void AddCategorizedAppLaunchers(ToolStripMenuItem parent, string category)
    {
        try
        {
            var uncategorizedApps = _configuration!.Apps
                .Where(a => a.Category == category)
                .OrderBy(a => a.Name);
            foreach (var app in uncategorizedApps)
            {
                var icon = GetAppIcon(app.Path!);
                var appLauncher = CreateToolStripMenuItem(parent, app.Name!,icon?.ToBitmap());
                AssignAppMenuItemClickEvent(appLauncher, app.Path!, app.Args!);
            }
        }
        catch (Exception caught)
        {
            Console.Error.WriteLine($"Unexpected error adding categorized app launchers" +
                                    $"{Environment.NewLine}{caught.Message}" +
                                    $"{Environment.NewLine}{caught.StackTrace}", caught);
            throw;
        }
    }
    
}