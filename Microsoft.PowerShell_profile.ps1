
#34de4b3d-13a8-4540-b76d-b9e8d3851756 PowerToys CommandNotFound module

Import-Module "C:\Users\Desto\AppData\Local\PowerToys\WinUI3Apps\..\WinGetCommandNotFound.psd1"
#34de4b3d-13a8-4540-b76d-b9e8d3851756

# Make Windows Terminal duplicate pane/tab open on the smame cwd
function Invoke-Starship-PreCommand {
  $loc = $executionContext.SessionState.Path.CurrentLocation;
  $prompt = "$([char]27)]9;12$([char]7)"
  if ($loc.Provider.Name -eq "FileSystem")
  {
    $prompt += "$([char]27)]9;9;`"$($loc.ProviderPath)`"$([char]27)\"
  }
  $host.ui.Write($prompt)
}

# oh-my-posh init pwsh --config "$env:POSH_THEMES_PATH\the-unnamed-better.omp.json" | Invoke-Expression
Invoke-Expression (&starship init powershell)

# Aliases
function ll_alias { eza -alh }
function tree_alias { eza --tree $args[0] }
Invoke-Expression (& { (zoxide init --cmd cd powershell | Out-String) })
Set-Alias -Name ls -Value eza
Set-Alias -Name ll -Value ll_alias
Set-Alias -Name tree -Value tree_alias
Set-Alias -Name cat -Value bat

$env:TERM = "xterm-256color"
