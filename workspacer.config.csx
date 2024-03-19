// Development
// #r "C:\Users\dalyisaac\Repos\workspacer\src\workspacer.Shared\bin\Debug\net5.0-windows\win10-x64\workspacer.Shared.dll"
// #r "C:\Users\dalyisaac\Repos\workspacer\src\workspacer.Bar\bin\Debug\net5.0-windows\win10-x64\workspacer.Bar.dll"
// #r "C:\Users\dalyisaac\Repos\workspacer\src\workspacer.Gap\bin\Debug\net5.0-windows\win10-x64\workspacer.Gap.dll"
// #r "C:\Users\dalyisaac\Repos\workspacer\src\workspacer.ActionMenu\bin\Debug\net5.0-windows\win10-x64\workspacer.ActionMenu.dll"
// #r "C:\Users\dalyisaac\Repos\workspacer\src\workspacer.FocusIndicator\bin\Debug\net5.0-windows\win10-x64\workspacer.FocusIndicator.dll"


// Production
#r "C:\Program Files\workspacer\workspacer.Shared.dll"
#r "C:\Program Files\workspacer\plugins\workspacer.Bar\workspacer.Bar.dll"
#r "C:\Program Files\workspacer\plugins\workspacer.Gap\workspacer.Gap.dll"
#r "C:\Program Files\workspacer\plugins\workspacer.ActionMenu\workspacer.ActionMenu.dll"
#r "C:\Program Files\workspacer\plugins\workspacer.FocusIndicator\workspacer.FocusIndicator.dll"

using System;
using System.Collections.Generic;
using System.Linq;
using workspacer;
using workspacer.Bar;
using workspacer.Bar.Widgets;
using workspacer.Gap;
using workspacer.ActionMenu;
using workspacer.FocusIndicator;

return new Action<IConfigContext>((IConfigContext context) =>
{
    /* Variables */
    var fontSize = 11;
    var barHeight = 22;
    var fontName = "Consolas";
    var background = new Color(0x21, 0x1f, 0x22); // 211f22

    /* Config */
    context.CanMinimizeWindows = false;

    /* Gap */
    var gap = 6;
    var gapPlugin = context.AddGap(new GapPluginConfig() { InnerGap = gap, OuterGap = gap / 2, Delta = gap / 2 });

    /* Bar */
    context.AddBar(new BarPluginConfig()
    {
        FontSize = fontSize,
        BarHeight = barHeight,
        FontName = fontName,
        DefaultWidgetBackground = background,
        LeftWidgets = () => new IBarWidget[]
        {
            new WorkspaceWidget()
            {
                WorkspaceHasFocusColor = new Color(0x60, 0xdc, 0xe8), // 60dce8
                WorkspaceIndicatingBackColor = background
            },
            new TitleWidget() 
            {
                MonitorHasFocusColor = new Color(0xf2, 0xd8, 0x54), // f2d854
                IsShortTitle = true
            }
        },
        RightWidgets = () => new IBarWidget[]
        {
            // new NetworkPerformanceWidget(),
            // new MemoryPerformanceWidget(),
            // new CpuPerformanceWidget(),
            new TimeWidget(5000, "hh:mm:ss tt | ddd dd-MMM-yyyy"),
        }
    });

    /* Bar focus indicator */
    // context.AddFocusIndicator();

    /* Default layouts */
    Func<ILayoutEngine[]> defaultLayouts = () => new ILayoutEngine[]
    {
        new TallLayoutEngine(),
        new FullLayoutEngine(),
    };

    Func<ILayoutEngine[]> onlyFullLayout = () => new ILayoutEngine[]
    {
        new FullLayoutEngine(),
    };

    context.DefaultLayouts = defaultLayouts;

    string w1 = "dev";
    string w2 = "web";
    string w3 = "docs";
    string w4 = "chat";
    string w5 = "media";
    string w6 = "sys";
    string w7 = "game";
    string w8 = "tool";
    string w9 = "misc";

    /* Workspaces */
    // Array of workspace names and their layouts
    (string, ILayoutEngine[])[] workspaces =
    {
        (w1, defaultLayouts()),
        (w2, defaultLayouts()),
        (w3, defaultLayouts()),
        (w4, defaultLayouts()),
        (w5, defaultLayouts()),
        (w6, defaultLayouts()),
        (w7, onlyFullLayout()),
        (w8, defaultLayouts()),
        (w9, defaultLayouts()),
    };

    foreach ((string name, ILayoutEngine[] layouts) in workspaces)
    {
        context.WorkspaceContainer.CreateWorkspace(name, layouts);
    }

    /* Filters */
    context.WindowRouter.AddFilter((window) => !window.ProcessName.Equals("GeometryDash"));
    context.WindowRouter.AddFilter((window) => !window.ProcessName.Equals("PowerToys.PowerLauncher"));
    context.WindowRouter.AddFilter((window) => !window.ProcessName.Equals("PowerToys.MeasureToolUI"));
    context.WindowRouter.AddFilter((window) => !window.ProcessName.Equals("RtkNGUI64"));
    context.WindowRouter.AddFilter((window) => !window.Title.Equals("Calculator"));
    context.WindowRouter.AddFilter((window) => context.Workspaces.FocusedWorkspace.Name != w7);

    // The following filter means that Edge will now open on the correct display
    context.WindowRouter.AddFilter((window) => !window.Class.Equals("ShellTrayWnd"));

    /* Routes */
    // context.WindowRouter.RouteProcessName("OBS", "obs");
    context.WindowRouter.AddRoute((window) => window.Title.Contains("Google Chrome") ? context.WorkspaceContainer[w2] : null);
    context.WindowRouter.RouteProcessName("firefox", w2);
    context.WindowRouter.AddRoute((window) => window.Title.Contains("Firefox") ? context.WorkspaceContainer[w2] : null);
    context.WindowRouter.AddRoute((window) => window.Title.Contains("Visual Studio") ? context.WorkspaceContainer[w1] : null);
    context.WindowRouter.RouteProcessName("WindowsTerminal", w1);
    context.WindowRouter.RouteProcessName("DB Browser for SQLite", w1);
    context.WindowRouter.AddRoute((window) => window.Title.Contains("WhatsApp") ? context.WorkspaceContainer[w4] : null);
    context.WindowRouter.AddRoute((window) => window.Title.Contains("Discord") ? context.WorkspaceContainer[w4] : null);
    context.WindowRouter.RouteProcessName("Spotify", w5);
    context.WindowRouter.RouteProcessName("SystemSettings", w6);

    /* Action menu */
    var actionMenu = context.AddActionMenu(new ActionMenuPluginConfig()
    {
        RegisterKeybind = false,
        MenuHeight = barHeight,
        FontSize = fontSize,
        FontName = fontName,
        Background = background,
    });

    /* Action menu builder */
    Func<ActionMenuItemBuilder> createActionMenuBuilder = () =>
    {
        var menuBuilder = actionMenu.Create();

        // Switch to workspace
        menuBuilder.AddMenu("switch", () =>
        {
            var workspaceMenu = actionMenu.Create();
            var monitor = context.MonitorContainer.FocusedMonitor;
            var workspaces = context.WorkspaceContainer.GetWorkspaces(monitor);

            Func<int, Action> createChildMenu = (workspaceIndex) => () =>
            {
                context.Workspaces.SwitchMonitorToWorkspace(monitor.Index, workspaceIndex);
            };

            int workspaceIndex = 0;
            foreach (var workspace in workspaces)
            {
                workspaceMenu.Add(workspace.Name, createChildMenu(workspaceIndex));
                workspaceIndex++;
            }

            return workspaceMenu;
        });

        // Move window to workspace
        menuBuilder.AddMenu("move", () =>
        {
            var moveMenu = actionMenu.Create();
            var focusedWorkspace = context.Workspaces.FocusedWorkspace;

            var workspaces = context.WorkspaceContainer.GetWorkspaces(focusedWorkspace).ToArray();
            Func<int, Action> createChildMenu = (index) => () => { context.Workspaces.MoveFocusedWindowToWorkspace(index); };

            for (int i = 0; i < workspaces.Length; i++)
            {
                moveMenu.Add(workspaces[i].Name, createChildMenu(i));
            }

            return moveMenu;
        });

        // Rename workspace
        menuBuilder.AddFreeForm("rename", (name) =>
        {
            context.Workspaces.FocusedWorkspace.Name = name;
        });

        // Create workspace
        menuBuilder.AddFreeForm("create workspace", (name) =>
        {
            context.WorkspaceContainer.CreateWorkspace(name);
        });

        // Delete focused workspace
        menuBuilder.Add("close", () =>
        {
            context.WorkspaceContainer.RemoveWorkspace(context.Workspaces.FocusedWorkspace);
        });

        // Workspacer
        menuBuilder.Add("toggle keybind helper", () => context.Keybinds.ShowKeybindDialog());
        menuBuilder.Add("toggle enabled", () => context.Enabled = !context.Enabled);
        menuBuilder.Add("restart", () => context.Restart());
        menuBuilder.Add("quit", () => context.Quit());

        return menuBuilder;
    };
    var actionMenuBuilder = createActionMenuBuilder();

    /* Keybindings */
    Action setKeybindings = () =>
    {
        KeyModifiers win = KeyModifiers.Win;
        KeyModifiers winShift = win | KeyModifiers.Shift;
        KeyModifiers winCtrl = win | KeyModifiers.Control;
        KeyModifiers winAlt = win | KeyModifiers.Alt;

        IKeybindManager manager = context.Keybinds;

        var workspaces = context.Workspaces;

        manager.UnsubscribeAll();
        manager.Subscribe(MouseEvent.MouseMove, () => workspaces.SwitchFocusedMonitorToMouseLocation());

        // Switch monitors
        manager.Subscribe(winAlt, Keys.N, () => workspaces.SwitchFocusToNextMonitor(), "switch focus to next monitor");

        // Close window
        manager.Subscribe(winCtrl, Keys.W, () => workspaces.FocusedWorkspace.CloseFocusedWindow(), "close the currently focused window");

        // Switch layout engine
        manager.Subscribe(winAlt, Keys.S, () => workspaces.FocusedWorkspace.NextLayoutEngine(), "rotate to the next layout engine");

        // Left, Right keys
        manager.Subscribe(winCtrl, Keys.Left, () => workspaces.SwitchToPreviousWorkspace(), "switch to previous workspace");
        manager.Subscribe(winCtrl, Keys.Right, () => workspaces.SwitchToNextWorkspace(), "switch to next workspace");

        manager.Subscribe(winShift, Keys.Left, () => workspaces.MoveFocusedWindowToPreviousMonitor(), "move focused window to previous monitor");
        manager.Subscribe(winShift, Keys.Right, () => workspaces.MoveFocusedWindowToNextMonitor(), "move focused window to next monitor");

        // HJKL keys
        manager.Subscribe(winAlt, Keys.H, () => workspaces.FocusedWorkspace.FocusPrimaryWindow(), "focus the primary window");
        manager.Subscribe(winAlt, Keys.L, () => workspaces.FocusedWorkspace.FocusLastFocusedWindow(), "focus the last focused window");

        manager.Subscribe(winCtrl, Keys.H, () => workspaces.FocusedWorkspace.ShrinkPrimaryArea(), "shrink primary area");
        manager.Subscribe(winCtrl, Keys.L, () => workspaces.FocusedWorkspace.ExpandPrimaryArea(), "expand primary area");

        manager.Subscribe(winAlt, Keys.K, () => workspaces.FocusedWorkspace.FocusPreviousWindow(), "rotate focus to the previous window");
        manager.Subscribe(winAlt, Keys.J, () => workspaces.FocusedWorkspace.FocusNextWindow(), "rotate focus to the previous window");

        manager.Subscribe(winCtrl, Keys.K, () => workspaces.FocusedWorkspace.SwapFocusAndPreviousWindow(), "swap focus and previous window");
        manager.Subscribe(winCtrl, Keys.J, () => workspaces.FocusedWorkspace.SwapFocusAndNextWindow(), "swap focus and next window");

        // number keys
        manager.Subscribe(winAlt, Keys.D1, () => workspaces.SwitchToWorkspace(0), "switch focused window to workspace 1");
        manager.Subscribe(winAlt, Keys.D2, () => workspaces.SwitchToWorkspace(1), "switch focused window to workspace 2");
        manager.Subscribe(winAlt, Keys.D3, () => workspaces.SwitchToWorkspace(2), "switch focused window to workspace 3");
        manager.Subscribe(winAlt, Keys.D4, () => workspaces.SwitchToWorkspace(3), "switch focused window to workspace 4");
        manager.Subscribe(winAlt, Keys.D5, () => workspaces.SwitchToWorkspace(4), "switch focused window to workspace 5");
        manager.Subscribe(winAlt, Keys.D6, () => workspaces.SwitchToWorkspace(5), "switch focused window to workspace 6");
        manager.Subscribe(winAlt, Keys.D7, () => workspaces.SwitchToWorkspace(6), "switch focused window to workspace 7");
        manager.Subscribe(winAlt, Keys.D8, () => workspaces.SwitchToWorkspace(7), "switch focused window to workspace 8");
        manager.Subscribe(winAlt, Keys.D9, () => workspaces.SwitchToWorkspace(8), "switch focused window to workspace 9");

        manager.Subscribe(winShift, Keys.D1, () => workspaces.MoveFocusedWindowToWorkspace(0), "switch focused window to workspace 1");
        manager.Subscribe(winShift, Keys.D2, () => workspaces.MoveFocusedWindowToWorkspace(1), "switch focused window to workspace 2");
        manager.Subscribe(winShift, Keys.D3, () => workspaces.MoveFocusedWindowToWorkspace(2), "switch focused window to workspace 3");
        manager.Subscribe(winShift, Keys.D4, () => workspaces.MoveFocusedWindowToWorkspace(3), "switch focused window to workspace 4");
        manager.Subscribe(winShift, Keys.D5, () => workspaces.MoveFocusedWindowToWorkspace(4), "switch focused window to workspace 5");
        manager.Subscribe(winShift, Keys.D6, () => workspaces.MoveFocusedWindowToWorkspace(5), "switch focused window to workspace 6");
        manager.Subscribe(winShift, Keys.D7, () => workspaces.MoveFocusedWindowToWorkspace(6), "switch focused window to workspace 7");
        manager.Subscribe(winShift, Keys.D8, () => workspaces.MoveFocusedWindowToWorkspace(7), "switch focused window to workspace 8");
        manager.Subscribe(winShift, Keys.D9, () => workspaces.MoveFocusedWindowToWorkspace(8), "switch focused window to workspace 9");

        // Other shortcuts
        manager.Subscribe(winAlt, Keys.P, () => actionMenu.ShowMenu(actionMenuBuilder), "show menu");
        manager.Subscribe(winShift, Keys.Escape, () => context.Enabled = !context.Enabled, "toggle enabled/disabled");
        manager.Subscribe(winShift, Keys.I, () => context.ToggleConsoleWindow(), "toggle console window");

    };
    setKeybindings();
});
