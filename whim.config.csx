#nullable enable
#r "C:\Users\Desto\AppData\Local\Programs\Whim\whim.dll"
#r "C:\Users\Desto\AppData\Local\Programs\Whim\plugins\Whim.Bar\Whim.Bar.dll"
#r "C:\Users\Desto\AppData\Local\Programs\Whim\plugins\Whim.CommandPalette\Whim.CommandPalette.dll"
#r "C:\Users\Desto\AppData\Local\Programs\Whim\plugins\Whim.FloatingLayout\Whim.FloatingLayout.dll"
#r "C:\Users\Desto\AppData\Local\Programs\Whim\plugins\Whim.FocusIndicator\Whim.FocusIndicator.dll"
#r "C:\Users\Desto\AppData\Local\Programs\Whim\plugins\Whim.Gaps\Whim.Gaps.dll"
#r "C:\Users\Desto\AppData\Local\Programs\Whim\plugins\Whim.LayoutPreview\Whim.LayoutPreview.dll"
#r "C:\Users\Desto\AppData\Local\Programs\Whim\plugins\Whim.SliceLayout\Whim.SliceLayout.dll"
#r "C:\Users\Desto\AppData\Local\Programs\Whim\plugins\Whim.TreeLayout\Whim.TreeLayout.dll"
#r "C:\Users\Desto\AppData\Local\Programs\Whim\plugins\Whim.TreeLayout.Bar\Whim.TreeLayout.Bar.dll"
#r "C:\Users\Desto\AppData\Local\Programs\Whim\plugins\Whim.TreeLayout.CommandPalette\Whim.TreeLayout.CommandPalette.dll"
#r "C:\Users\Desto\AppData\Local\Programs\Whim\plugins\Whim.Updater\Whim.Updater.dll"

using System;
using System.Collections.Generic;
using Whim;
using Whim.Bar;
using Whim.CommandPalette;
using Whim.FloatingLayout;
using Whim.FocusIndicator;
using Whim.Gaps;
using Whim.LayoutPreview;
using Whim.SliceLayout;
using Whim.TreeLayout;
using Whim.TreeLayout.Bar;
using Whim.TreeLayout.CommandPalette;
using Whim.Updater;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;

/// <summary>
/// This is what's called when Whim is loaded.
/// </summary>
/// <param name="context"></param>
void DoConfig(IContext context)
{
	context.Logger.Config = new LoggerConfig();

	// Bar plugin.
	List<BarComponent> leftComponents = new() { WorkspaceWidget.CreateComponent() };
	List<BarComponent> centerComponents = new() { FocusedWindowWidget.CreateComponent() };
	List<BarComponent> rightComponents = new()
	{
		ActiveLayoutWidget.CreateComponent(),
		DateTimeWidget.CreateComponent(5000, "hh:mm:ss ddd dd-MMM-yyyy")
	};

	BarConfig barConfig = new(leftComponents, centerComponents, rightComponents);
	BarPlugin barPlugin = new(context, barConfig);
	context.PluginManager.AddPlugin(barPlugin);

	// Gap plugin.
	GapsConfig gapsConfig = new() { OuterGap = 5, InnerGap = 4 };
	GapsPlugin gapsPlugin = new(context, gapsConfig);
	context.PluginManager.AddPlugin(gapsPlugin);

	// Floating window plugin.
	FloatingLayoutPlugin floatingLayoutPlugin = new(context);
	context.PluginManager.AddPlugin(floatingLayoutPlugin);

	// Focus indicator.
	// FocusIndicatorConfig focusIndicatorConfig = new() {
	// 	Color = new SolidColorBrush(Colors.SkyBlue),
	// 	FadeEnabled = true,
	// 	FadeTimeout = TimeSpan.FromMilliseconds(300),
	// 	BorderSize = 3
	// };
	// FocusIndicatorPlugin focusIndicatorPlugin = new(context, focusIndicatorConfig);
	// context.PluginManager.AddPlugin(focusIndicatorPlugin);

	// Command palette.
	CommandPaletteConfig commandPaletteConfig = new(context);
	CommandPalettePlugin commandPalettePlugin = new(context, commandPaletteConfig);
	context.PluginManager.AddPlugin(commandPalettePlugin);

	// Slice layout.
	SliceLayoutPlugin sliceLayoutPlugin = new(context);
	context.PluginManager.AddPlugin(sliceLayoutPlugin);

	// Tree layout.
	TreeLayoutPlugin treeLayoutPlugin = new(context);
	context.PluginManager.AddPlugin(treeLayoutPlugin);

	// Tree layout bar.
	TreeLayoutBarPlugin treeLayoutBarPlugin = new(treeLayoutPlugin);
	context.PluginManager.AddPlugin(treeLayoutBarPlugin);
	rightComponents.Add(treeLayoutBarPlugin.CreateComponent());

	// Tree layout command palette.
	TreeLayoutCommandPalettePlugin treeLayoutCommandPalettePlugin = new(context, treeLayoutPlugin, commandPalettePlugin);
	context.PluginManager.AddPlugin(treeLayoutCommandPalettePlugin);

	// Layout preview.
	LayoutPreviewPlugin layoutPreviewPlugin = new(context);
	context.PluginManager.AddPlugin(layoutPreviewPlugin);

	// Updater.
	// UpdaterConfig updaterConfig = new() { ReleaseChannel = ReleaseChannel.Alpha };
	// UpdaterPlugin updaterPlugin = new(context, updaterConfig);
	// context.PluginManager.AddPlugin(updaterPlugin);

	// Set up workspaces.
	const string w1 = "dev";
	const string w2 = "web";
	const string w3 = "docs";
	const string w4 = "chat";
	const string w5 = "media";
	const string w6 = "sys";
	const string w7 = "game";
	const string w8 = "misc";
	const string w9 = "other";

	context.WorkspaceManager.Add(w1);
	context.WorkspaceManager.Add(w2);
	context.WorkspaceManager.Add(w3);
	context.WorkspaceManager.Add(w4);
	context.WorkspaceManager.Add(w5);
	context.WorkspaceManager.Add(w6);
	context.WorkspaceManager.Add(w7);
	context.WorkspaceManager.Add(w8);
	context.WorkspaceManager.Add(w9);

	// Set up layout engines.
	context.WorkspaceManager.CreateLayoutEngines = () => new CreateLeafLayoutEngine[]
	{
		(id) => SliceLayouts.CreatePrimaryStackLayout(context, sliceLayoutPlugin, id),
		(id) => SliceLayouts.CreateSecondaryPrimaryLayout(context, sliceLayoutPlugin, id),
		(id) => SliceLayouts.CreateMultiColumnLayout(context, sliceLayoutPlugin, id, 1, 2, 0),
		(id) => new FocusLayoutEngine(id),
		(id) => new TreeLayoutEngine(context, treeLayoutPlugin, id)
	};

	// ----- Filters -----
	var fm = context.FilterManager;
	fm.AddProcessNameFilter("ShareX");
	fm.AddProcessNameFilter("PowerToys");

	// ----- Routing -----
	// By process file name
	context.RouterManager.Add(window => context.WorkspaceManager.TryGet(window.ProcessFileName switch {
		"WindowsTerminal.exe" or "Code.exe" or "DB Browser for SQLite.exe" => w1,
		"firefox.exe" or "chrome.exe" => w2,
		"Discord.exe" or "WhatsApp.exe" => w4,
		"Spotify.exe" => w5,
		"SystemSettings.exe" => w6,
		"steam.exe" or "steamwebhelper.exe" => w7,
		_ => null,
	}));

	// ----- Keybinds -----
	var km = context.KeybindManager;
	km.Clear();
	km.SetKeybind("whim.core.activate_previous_workspace", new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_LEFT));
	km.SetKeybind("whim.core.activate_next_workspace", new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_RIGHT));

	km.SetKeybind("whim.core.focus_window_in_direction.left", new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_H));
	km.SetKeybind("whim.core.focus_window_in_direction.down", new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_J));
	km.SetKeybind("whim.core.focus_window_in_direction.up", new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_K));
	km.SetKeybind("whim.core.focus_window_in_direction.right", new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_L));

	km.SetKeybind("whim.core.swap_window_in_direction.left", new Keybind(IKeybind.WinShift, VIRTUAL_KEY.VK_H));
	km.SetKeybind("whim.core.swap_window_in_direction.down", new Keybind(IKeybind.WinShift, VIRTUAL_KEY.VK_J));
	km.SetKeybind("whim.core.swap_window_in_direction.up", new Keybind(IKeybind.WinShift, VIRTUAL_KEY.VK_K));
	km.SetKeybind("whim.core.swap_window_in_direction.right", new Keybind(IKeybind.WinShift, VIRTUAL_KEY.VK_L));

	km.SetKeybind("whim.core.move_window_left_edge_left", new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_H));
	km.SetKeybind("whim.core.move_window_left_edge_right", new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_J));
	km.SetKeybind("whim.core.move_window_right_edge_left", new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_K));
	km.SetKeybind("whim.core.move_window_right_edge_right", new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_L));

	km.SetKeybind("whim.core.move_window_to_next_monitor", new Keybind(IKeybind.WinShift, VIRTUAL_KEY.VK_N));
	km.SetKeybind("whim.core.focus_next_monitor", new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_N));

	km.SetKeybind("whim.core.cycle_layout_engine.next", new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_X));
	km.SetKeybind("whim.core.cycle_layout_engine.previous", new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_Z));

	km.SetKeybind("whim.core.activate_workspace_1", new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_1));
	km.SetKeybind("whim.core.activate_workspace_2", new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_2));
	km.SetKeybind("whim.core.activate_workspace_3", new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_3));
	km.SetKeybind("whim.core.activate_workspace_4", new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_4));
	km.SetKeybind("whim.core.activate_workspace_5", new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_5));
	km.SetKeybind("whim.core.activate_workspace_6", new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_6));
	km.SetKeybind("whim.core.activate_workspace_7", new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_7));
	km.SetKeybind("whim.core.activate_workspace_8", new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_8));
	km.SetKeybind("whim.core.activate_workspace_9", new Keybind(IKeybind.WinAlt, VIRTUAL_KEY.VK_9));

	km.SetKeybind("whim.command_palette.toggle", new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_P));

	km.SetKeybind("whim.command_palette.move_window_to_workspace", new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_M));

	km.SetKeybind("whim.floating_layout.toggle_window_floating", new Keybind(IKeybind.WinCtrl, VIRTUAL_KEY.VK_F));

	// Styles
	string file = context.FileManager.GetWhimFileDir("Resources.xaml");
    context.ResourceManager.AddUserDictionary(file);
}


// We return doConfig here so that Whim can call it when it loads.
return DoConfig;
